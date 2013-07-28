//
// - BasicProtocol.cs
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
using P2PNet.EventArgs;

namespace P2PNet.Protocols
{
    public class BasicProtocol : IProtocol
    {
        private readonly List<IPacketHandler> _packetHandlers;
        private readonly Listener _server;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public BasicProtocol(Listener server)
        {
            _server = server;
            _server.ConnectionRequested += ServerOnClientConnected;
            _packetHandlers = new List<IPacketHandler>();
        }

        private void ServerOnClientConnected(object sender, ConnectionEventArgs connectionEventArgs)
        {
            var clientPacketHandler = new RawPacketHandler();
            _packetHandlers.Add(clientPacketHandler);
            clientPacketHandler.PacketReceived += OnPacketReceived;
        }

        private void OnPacketReceived(object sender, PacketReceivedEventArgs e)
        {
            var messageReceivedHandler = MessageReceived;
            if (messageReceivedHandler != null)
            {
                messageReceivedHandler(this, new MessageReceivedEventArgs
                    {
//                        Client = client,
                        Message = GetString(e.Packet)
                    });
            }
        }

        public void Send(Peer client, string message)
        {
            byte[] packet = GetPacket(message);
//            client.Send(packet);
        }

        private static byte[] GetPacket(string message)
        {
            var messageBytes = message.Length*sizeof (char);
            var packet = new byte[7 + messageBytes];
            var headerMark = new byte[] {0x12, 0x34, 0x89};
            var intBytes = BitConverter.GetBytes(messageBytes);
            if (BitConverter.IsLittleEndian) Array.Reverse(intBytes);

            Buffer.BlockCopy(headerMark, 0, packet, 0, headerMark.Length);
            Buffer.BlockCopy(intBytes, 0, packet, 3, intBytes.Length);
            Buffer.BlockCopy(message.ToCharArray(), 0, packet, 7, messageBytes);

            return packet;
        }

        private static string GetString(byte[] bytes)
        {
            var chars = new char[bytes.Length/sizeof (char)];
            Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        private static byte[] GetBytes(string str)
        {
            var bytes = new byte[str.Length*sizeof (char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
    }

    public class MessageReceivedEventArgs : System.EventArgs
    {
        public string Message { get; set; }
        public Connection Client { get; set; }
    }
}