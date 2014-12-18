//
// - ProtocolPacketHandler.cs
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
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Open.P2P.IO;
using Open.P2P.Listeners;
using Open.P2P.Streams;

namespace Open.P2P.Tests
{
    [TestFixture]
    public class PeerStreamTest
    {
        private TcpListener _listener;
        private Peer _peer;
        private CommunicationManager _communicationManager;
        private ManualResetEvent _connectedEvent;

        [SetUp]
        public void Setup()
        {
            _listener = new TcpListener(9999);
            _connectedEvent = new ManualResetEvent(false);
            _communicationManager = new CommunicationManager(_listener);
            _communicationManager.PeerConnected += (sender, args) => {
                _peer = args.Peer;
                _connectedEvent.Set();
            };
            _listener.Start();
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                _peer.Disconnect();
                _listener.Stop();
            }catch
            {
            }
        }

        [Test]
        public async Task EmptyPacketData()
        {
            var connection = new Connection(new IPEndPoint(IPAddress.Loopback, 9999));
            await connection.ConnectAsync();
            _connectedEvent.WaitOne();
            var buffer = new byte[4];
            var rt = _peer.Stream.ReadAsync(buffer, 0, 4);
            await connection.SendAsync(new ArraySegment<byte>(new byte[] {0x01, 0x02, 0x03, 0x04}));
            await rt;
            Assert.AreEqual(0x01, buffer[0]);
            Assert.AreEqual(0x02, buffer[1]);
            Assert.AreEqual(0x03, buffer[2]);
            Assert.AreEqual(0x04, buffer[3]);
        }

#if false
        [Test]
        public void PerfectlyCompletedWellFormedData()
        {
            var passed = false;
            var data = new byte[] { 0x12, 0x34, 0x89, 0x0, 0x0, 0x0, 0x02, 0x48, 0x49 };
            _packetHandler.PacketReceived += (s, e) => { Assert.AreEqual(new byte[] { 0x48, 0x49 }, e.Packet); passed = true; };
            _packetHandler.ProcessIncomingData(data);

            if (!passed) Assert.Fail("PacketReceived was not fired");
        }

        [Test]
        public void PerfectlyCompletedWellFormedDataTwoParts()
        {
            var passed = false;
            var data1 = new byte[] { 0x12, 0x34, 0x89, 0x0 };
            var data2 = new byte[] { 0x0, 0x0, 0x02, 0x48, 0x49 };
            _packetHandler.PacketReceived += (s, e) => { Assert.AreEqual(new[] { 0x48, 0x49 }, e.Packet); passed = true; };
            _packetHandler.ProcessIncomingData(data1);
            _packetHandler.ProcessIncomingData(data2);

            if (!passed) Assert.Fail("PacketReceived was not fired");
        }

        [Test]
        public void CompletedWellFormedLargerData()
        {
            bool passed = false;
            var data1 = new byte[] { 0x12, 0x34, 0x89, 0x0, 0x0, 0x0, 0x02, 0x48, 0x49, 0x12, 0x36 };
            _packetHandler.PacketReceived += (s, e) => { Assert.AreEqual(new[] { 0x48, 0x49 }, e.Packet); passed = true; };
            _packetHandler.ProcessIncomingData(data1);

            if (!passed) Assert.Fail("PacketReceived was not fired");
        }

        [Test]
        public void BadFormedPacketsHeader()
        {
            var passed = false;
            var data1 = new byte[] { 0x12, 0xff, 0x34, 0xff, 0x89, 0x0, 0x0, 0x0, 0x02, 0x48, 0x49 };
            _packetHandler.PacketReceived += (s, e) => { Assert.AreEqual(new[] { 0x48, 0x49 }, e.Packet); passed = true; };
            _packetHandler.ProcessIncomingData(data1);

            if (passed) Assert.Fail("It accepted a worng header as a valid one!");
        }
#endif
    }
}
