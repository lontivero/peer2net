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
using P2PNet.Utils;

namespace P2PNet.MessageHandlers
{
    public class EotMessageHandler : IStreamHandler
    {
        private readonly List<byte> _packet;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public EotMessageHandler()
        {
            _packet = new List<byte>();
        }

        public bool IsWaiting
        {
            get { return true; }
        }

        public void ProcessIncomingData(byte[] data)
        {
            Guard.NotNull(data, "data");
            foreach (byte d in data)
            {
                AcceptData(d);
            }
        }

        private void AcceptData(byte b)
        {
            if(b == 0)
            {
                var packetData = new byte[_packet.Count];
                _packet.CopyTo(packetData);
                EndProcessingData(packetData);
            }
            else
            {
                _packet.Add(b);
            }
        }

        private void EndProcessingData(byte[] packetData)
        {
            _packet.Clear();
            var receivedPacketHandler = MessageReceived;
            if (receivedPacketHandler != null)
            {
                receivedPacketHandler(this, new MessageReceivedEventArgs(packetData));
            }
        }
    }
}