using P2PNet.Protocols;

namespace P2PNet
{
    internal class ConnectionBundle
    {
        internal Connection Connection { get; set; }
        internal IPacketHandler PacketHandler { get; set; }
        internal ConnectionStat Statistics { get; set; }
    }
}