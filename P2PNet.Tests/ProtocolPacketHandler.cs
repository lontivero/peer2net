using NUnit.Framework;
using TcpServer.Protocols;

namespace TcpServer.Tests
{
    [TestFixture]
    public class ProtocolPacketHandlerTest
    {
        private RawPacketHandler _packetHandler;

        [SetUp]
        public void Setup()
        {
            _packetHandler = new RawPacketHandler(null);
        }

        [Test]
        public void Emptpy_Packet_Data()
        {
            var passed = false;
            var data = new byte[] { 0x12, 0x34, 0x89, 0x0, 0x0, 0x0, 0x00 };
            _packetHandler.PacketReceived += (s, e) => { Assert.AreEqual(new byte[0], e.Packet); passed = true; };
            _packetHandler.ProcessIncomingData(data, data.Length);

            if (!passed) Assert.Fail("PacketReceived was not fired");
        }

        [Test]
        public void Perfectly_Completed_Well_Formed_Data()
        {
            var passed = false;
            var data = new byte[] {0x12, 0x34, 0x89, 0x0, 0x0, 0x0, 0x02, 0x48, 0x49};
            _packetHandler.PacketReceived += (s, e) => { Assert.AreEqual(new byte[] { 0x48, 0x49 }, e.Packet); passed = true; };
            _packetHandler.ProcessIncomingData(data, data.Length);
            
            if(!passed) Assert.Fail("PacketReceived was not fired");
        }

        [Test]
        public void Perfectly_Completed_Well_Formed_Data_Two_Parts()
        {
            var passed = false;
            var data1 = new byte[] { 0x12, 0x34, 0x89, 0x0 };
            var data2 = new byte[] { 0x0, 0x0, 0x02, 0x48, 0x49 };
            _packetHandler.PacketReceived += (s, e) => { Assert.AreEqual(new[] { 0x48, 0x49 }, e.Packet); passed = true; };
            _packetHandler.ProcessIncomingData(data1, data1.Length);
            _packetHandler.ProcessIncomingData(data2, data2.Length);

            if (!passed) Assert.Fail("PacketReceived was not fired");
        }

        [Test]
        public void Perfectly_Completed_Well_Formed_Two_Packets()
        {
            int packets = 0;
            var data1 = new byte[] { 0x12, 0x34, 0x89, 0x0, 0x0, 0x0, 0x02, 0x48, 0x49, 0x12, 0x34, 0x89, 0x0, 0x0, 0x0, 0x02, 0x48, 0x49 };
            _packetHandler.PacketReceived += (s, e) => { Assert.AreEqual(new[] { 0x48, 0x49 }, e.Packet); packets++; };
            _packetHandler.ProcessIncomingData(data1, data1.Length);

            if (packets !=2) Assert.Fail("PacketReceived was not fired");
        }

        [Test]
        public void Completed_Well_Formed_Larger_Data()
        {
            bool passed = false;
            var data1 = new byte[] { 0x12, 0x34, 0x89, 0x0, 0x0, 0x0, 0x02, 0x48, 0x49, 0x12, 0x36 };
            _packetHandler.PacketReceived += (s, e) => { Assert.AreEqual(new[] { 0x48, 0x49 }, e.Packet); passed = true; };
            _packetHandler.ProcessIncomingData(data1, data1.Length);

            if (!passed) Assert.Fail("PacketReceived was not fired");
        }

        [Test]
        public void Completed_Well_Formed_Two_Packets_With_Garbage()
        {
            int packets = 0;
            var data1 = new byte[] { 0x12, 0x34, 0x89, 0x0, 0x0, 0x0, 0x02, 0x48, 0x49, 0x01, 0x12, 0x34, 0x89, 0x0, 0x0, 0x0, 0x02, 0x48, 0x49 };
            _packetHandler.PacketReceived += (s, e) => { Assert.AreEqual(new[] { 0x48, 0x49 }, e.Packet); packets++; };
            _packetHandler.ProcessIncomingData(data1, data1.Length);

            if (packets != 2) Assert.Fail("PacketReceived was not fired");
        }

        [Test]
        public void Completed_Well_Formed_Two_Packets_With_Garbage_Like_Header()
        {
            int packets = 0;
            var data1 = new byte[] { 0x12, 0x34, 0x89, 0x0, 0x0, 0x0, 0x02, 0x48, 0x49, 0x12, 0x34, 0x12, 0x34, 0x89, 0x0, 0x0, 0x0, 0x02, 0x48, 0x49 };
            _packetHandler.PacketReceived += (s, e) => { Assert.AreEqual(new[] { 0x48, 0x49 }, e.Packet); packets++; };
            _packetHandler.ProcessIncomingData(data1, data1.Length);

            if (packets != 2) Assert.Fail("PacketReceived was not fired");
        }

        [Test]
        public void Bad_Formed_Packets_Header()
        {
            var passed = false;
            var data1 = new byte[] { 0x12, 0xff, 0x34, 0xff, 0x89, 0x0, 0x0, 0x0, 0x02, 0x48, 0x49 };
            _packetHandler.PacketReceived += (s, e) => { Assert.AreEqual(new[] { 0x48, 0x49 }, e.Packet); passed=true; };
            _packetHandler.ProcessIncomingData(data1, data1.Length);

            if (passed) Assert.Fail("It accepted a worng header as a valid one!");
        }
    }
}
