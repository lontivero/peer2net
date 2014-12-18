namespace Open.P2P.EventArgs
{
    public class PeerEventArgs : System.EventArgs
    {
        private readonly Peer _peer;

        public PeerEventArgs(Peer peer)
        {
            _peer = peer;
        }

        public Peer Peer
        {
            get { return _peer; }
        }
    }

    public class PeerDataEventArgs : PeerEventArgs
    {
        private readonly byte[] _data;

        public PeerDataEventArgs(Peer peer, byte[] data)
            :base(peer)
        {
            _data = data;
        }

        public byte[] Data
        {
            get { return _data; }
        }
    }
}