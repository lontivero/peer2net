//
// - LoadTest.cs
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
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using NUnit.Framework;

namespace P2PNet.Tests
{
    [TestFixture]
    public class LoadTest: IClientManager
    {
        private Listener _listener;
        private ComunicationManager _comunicationManager;
        private Socket[] _sockets;
        private List<int> _receiveMessages;

        [SetUp]
        public void Setup()
        {
            _receiveMessages = new List<int>();
            _listener = new Listener(8000);
            _comunicationManager = new ComunicationManager(_listener, this);
            _sockets = new Socket[1];
            _listener.Start();
        }

        [Test]
        public void DoIt()
        {
            var messages = new byte[_sockets.Length][];
            for (var i = 0; i < _sockets.Length; i++)
            {
                messages[i] = GetPacket(i.ToString());
                _sockets[i] = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _sockets[i].Connect("127.0.0.1", 8000);
            }

            foreach (var sm in _sockets.Select((s, i) => new { Socket = s, Message = messages[i] }))
            {
                sm.Socket.BeginSend(sm.Message, 0, sm.Message.Length, SocketFlags.None, null, sm.Socket);
            }
            Thread.Sleep(1000);

            var duplicates = _receiveMessages.GroupBy(i => i)
              .Where(g => g.Count() > 1)
              .Select(g => g.Key);

            Assert.AreEqual(_receiveMessages.Count, messages.Length, "there are missing messages");
            Assert.AreEqual(0, duplicates.Count(), "there are duplicated messages");
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var t in _sockets)
            {
                t.Shutdown(SocketShutdown.Both);
                t.Close();
            }
            _listener.Stop();
        }

        private static byte[] GetPacket(string message)
        {
            var messageBytes = Encoding.ASCII.GetBytes(message);
            var len = messageBytes.Length;
            var packet = new byte[7 + len];
            var headerMark = new byte[] { 0x12, 0x34, 0x89 };
            var intBytes = BitConverter.GetBytes(len);
            if (BitConverter.IsLittleEndian) Array.Reverse(intBytes);

            Buffer.BlockCopy(headerMark, 0, packet, 0, headerMark.Length);
            Buffer.BlockCopy(intBytes, 0, packet, 3, intBytes.Length);
            Buffer.BlockCopy(messageBytes, 0, packet, 7, len);

            return packet;
        }

        public void OnPeerConnected(Peer peer)
        {
        }

        public void OnPeerDataReceived(Peer peer, byte[] buffer)
        {
            _receiveMessages.Add(Int32.Parse(Encoding.ASCII.GetString(buffer)));
        }

        public void OnPeerDataSent(Peer peer, byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
