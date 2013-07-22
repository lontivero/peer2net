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

using NUnit.Framework;
using P2PNet.Protocols;

namespace P2PNet.Tests
{
    [TestFixture]
    public class PacketHandlerTest
    {
        private RawPacketHandler _packetHandler;

        [SetUp]
        public void Setup()
        {
            _packetHandler = new RawPacketHandler();
        }

        [Test]
        public void Emptpy_Packet_Data()
        {
            var passed = false;
            var data = new byte[] { 0x12, 0x34, 0x89, 0x0, 0x0, 0x0, 0x00 };
            _packetHandler.PacketReceived += (s, e) => { Assert.AreEqual(new byte[0], e.Packet); passed = true; };
            _packetHandler.ProcessIncomingData(data);

            if (!passed) Assert.Fail("PacketReceived was not fired");
        }

        [Test]
        public void Perfectly_Completed_Well_Formed_Data()
        {
            var passed = false;
            var data = new byte[] {0x12, 0x34, 0x89, 0x0, 0x0, 0x0, 0x02, 0x48, 0x49};
            _packetHandler.PacketReceived += (s, e) => { Assert.AreEqual(new byte[] { 0x48, 0x49 }, e.Packet); passed = true; };
            _packetHandler.ProcessIncomingData(data);
            
            if(!passed) Assert.Fail("PacketReceived was not fired");
        }

        [Test]
        public void Perfectly_Completed_Well_Formed_Data_Two_Parts()
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
        public void Completed_Well_Formed_Larger_Data()
        {
            bool passed = false;
            var data1 = new byte[] { 0x12, 0x34, 0x89, 0x0, 0x0, 0x0, 0x02, 0x48, 0x49, 0x12, 0x36 };
            _packetHandler.PacketReceived += (s, e) => { Assert.AreEqual(new[] { 0x48, 0x49 }, e.Packet); passed = true; };
            _packetHandler.ProcessIncomingData(data1);

            if (!passed) Assert.Fail("PacketReceived was not fired");
        }

        [Test]
        public void Bad_Formed_Packets_Header()
        {
            var passed = false;
            var data1 = new byte[] { 0x12, 0xff, 0x34, 0xff, 0x89, 0x0, 0x0, 0x0, 0x02, 0x48, 0x49 };
            _packetHandler.PacketReceived += (s, e) => { Assert.AreEqual(new[] { 0x48, 0x49 }, e.Packet); passed=true; };
            _packetHandler.ProcessIncomingData(data1);

            if (passed) Assert.Fail("It accepted a worng header as a valid one!");
        }
    }
}
