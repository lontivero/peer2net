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

namespace P2PNet.Protocols
{
    public class RawPacketHandler : IPacketHandler
    {
        private const byte FirstHeaderByte = 0x12;
        private const byte SecondHeaderByte = 0x34;
        private const byte ThirdHeaderByte = 0x89;
        private readonly Dictionary<PacketStatus, Action<byte>> _handlers;
        private readonly List<byte> _packet;
        private bool _completed;
        private int _currentByteIndex;
        private int _packetLength;
        private int _packetLengthOffset;
        private PacketStatus _status;


        public RawPacketHandler()
        {
            _packet = new List<byte>();
            _handlers = new Dictionary<PacketStatus, Action<byte>>
                {
                    {PacketStatus.Empty, b => Expect(FirstHeaderByte, b, PacketStatus.FirstHeaderReceived)},
                    {
                        PacketStatus.FirstHeaderReceived,
                        b => Expect(SecondHeaderByte, b, PacketStatus.SecondHeaderReceived)
                    },
                    {
                        PacketStatus.SecondHeaderReceived,
                        b => Expect(ThirdHeaderByte, b, PacketStatus.ThirdHeaderReceived)
                    },
                    {PacketStatus.ThirdHeaderReceived, AcceptPacketLength},
                    {PacketStatus.ReceivingPacketLength, AcceptPacketLength},
                    {PacketStatus.ReceivingData, AcceptData},
                    {PacketStatus.Unsynchronized, b => Expect(FirstHeaderByte, b, PacketStatus.FirstHeaderReceived)}
                };
        }

        #region IPacketHandler Members

        public event EventHandler<PacketReceivedEventArgs> PacketReceived;

        public bool IsWaiting
        {
            get { return !_completed; }
        }

        public void ProcessIncomingData(byte[] data)
        {
            _completed = false;
            int length = data.Length;
            for (_currentByteIndex = 0; _currentByteIndex < length && !_completed; _currentByteIndex++)
            {
                byte currentByte = data[_currentByteIndex];
                var handler = _handlers[_status];
                handler(currentByte);
            }
        }

        #endregion

        private void AcceptPacketLength(byte b)
        {
            const int sizeOfInt32 = sizeof (int);
            const int sizeOfByte = sizeof (byte);

            _packetLength |= b << (sizeOfInt32 - ++_packetLengthOffset)*sizeOfByte;
            _status = _packetLengthOffset < sizeOfInt32
                          ? PacketStatus.ReceivingPacketLength
                          : PacketStatus.ReceivingData;

            if (_status == PacketStatus.ReceivingData && _packetLength == 0)
            {
                EndProcessingData(new byte[0]);
            }
        }

        private void AcceptData(byte b)
        {
            _packet.Add(b);
            int receivedByteCount = _packet.Count;
            if (receivedByteCount == _packetLength)
            {
                var packetData = new byte[_packet.Count];
                _packet.CopyTo(packetData);

                EndProcessingData(packetData);
            }
        }

        private void Expect(byte expected, byte currentByte, PacketStatus nextStatus)
        {
            PacketStatus previousStatus = _status;
            _status = currentByte == expected ? nextStatus : PacketStatus.Unsynchronized;
            if (previousStatus != PacketStatus.Unsynchronized && _status == PacketStatus.Unsynchronized)
            {
                _currentByteIndex--;
            }
        }

        private void EndProcessingData(byte[] packetData)
        {
            ResetControlVariables();
            EventHandler<PacketReceivedEventArgs> receivedPacketHandler = PacketReceived;
            if (receivedPacketHandler != null)
            {
                receivedPacketHandler(this, new PacketReceivedEventArgs(packetData));
            }
        }

        private void ResetControlVariables()
        {
            _packet.Clear();
            _packetLength = 0;
            _packetLengthOffset = 0;
            _status = PacketStatus.Empty;
            _completed = true;
        }

        #region Nested type: PacketStatus

        private enum PacketStatus : byte
        {
            Empty,
            FirstHeaderReceived,
            SecondHeaderReceived,
            ThirdHeaderReceived,
            ReceivingData,
            ReceivingPacketLength,
            Unsynchronized
        }

        #endregion
    }
}