namespace P2PNet.Protocols
{
    public class PacketReceivedEventArgs : System.EventArgs
    {
        private readonly byte[] _packet;

        public PacketReceivedEventArgs(byte[] packet)
        {
            _packet = packet;
        }

        public byte[] Packet
        {
            get { return _packet; }
        }
    }
}