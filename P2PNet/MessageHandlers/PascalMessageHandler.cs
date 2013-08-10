//
// - RawPacketHandler.cs
// 
// Author:
//     Lucas Ontivero <lucasontivero@gmail.com>
// 
// Copyright 2013 Lucas E. Ontivero
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//  http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 

// <summary></summary>

using System;
using System.Collections.Generic;
using Peer2Net.Utils;

namespace Peer2Net.MessageHandlers
{
    public class PascalMessageHandler : IStreamHandler
    {
        private readonly Dictionary<PacketStatus, Action<byte>> _handlers;
        private readonly List<byte> _packet;
        private bool _completed;
        private int _currentByteIndex;
        private int _packetLength;
        private int _packetLengthOffset;
        private PacketStatus _status;
        private int _pendingBytes;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public PascalMessageHandler()
        {
            _packet = new List<byte>();
            _handlers = new Dictionary<PacketStatus, Action<byte>>
                {
                    {PacketStatus.Empty, AcceptPacketLength},
                    {PacketStatus.ReceivingPacketLength, AcceptPacketLength},
                    {PacketStatus.ReceivingData, AcceptData},
                };

            _pendingBytes = 4;
        }

        public bool IsWaiting
        {
            get { return !_completed; }
        }

        public int PendingBytes
        {
            get { return _pendingBytes; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void ProcessIncomingData(byte[] data)
        {
            Guard.NotNull(data, "data");

            _completed = false;
            int length = data.Length;
            for (_currentByteIndex = 0; _currentByteIndex < length && !_completed; _currentByteIndex++)
            {
                byte currentByte = data[_currentByteIndex];
                var handler = _handlers[_status];
                handler(currentByte);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public static byte[] FormatMessage(byte[] message)
        {
            Guard.NotNull(message, "message");
            var messageLength = message.Length;
            var intBytes = BitConverter.GetBytes(messageLength);
            if (BitConverter.IsLittleEndian) Array.Reverse(intBytes);

            var data = new byte[messageLength + 4];
            Buffer.BlockCopy(message, 0, data, 4, messageLength);
            Buffer.BlockCopy(intBytes, 0, data, 0, 4);
            return data;
        }

        private void AcceptPacketLength(byte b)
        {
            _packetLength |= b << (4 - ++_packetLengthOffset);
            _status = _packetLengthOffset < 4
                          ? PacketStatus.ReceivingPacketLength
                          : PacketStatus.ReceivingData;

            _pendingBytes--;
            if (_status == PacketStatus.ReceivingData)
            {
                if (_packetLength == 0)
                {
                    EndProcessingData(new byte[0]);
                }
                else
                {
                    _pendingBytes = _packetLength;
                }
            }
        }

        private void AcceptData(byte b)
        {
            _packet.Add(b);
            int receivedByteCount = _packet.Count;
            _pendingBytes--;
            if (receivedByteCount == _packetLength)
            {
                var packetData = new byte[_packet.Count];
                _packet.CopyTo(packetData);

                EndProcessingData(packetData);
            }
        }

        private void EndProcessingData(byte[] packetData)
        {
            ResetControlVariables();
            EventHandler<MessageReceivedEventArgs> receivedPacketHandler = MessageReceived;
            if (receivedPacketHandler != null)
            {
                receivedPacketHandler(this, new MessageReceivedEventArgs(packetData));
            }
        }

        private void ResetControlVariables()
        {
            _packet.Clear();
            _packetLength = 0;
            _packetLengthOffset = 0;
            _status = PacketStatus.Empty;
            _pendingBytes = 4;
            _completed = true;
        }

        #region Nested type: PacketStatus

        private enum PacketStatus : byte
        {
            Empty,
            ReceivingData,
            ReceivingPacketLength
        }

        #endregion
    }
}