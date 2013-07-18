using System;

namespace P2PNet.EventArgs
{
    public class DataArrivedEventArgs : System.EventArgs
    {
        private readonly byte[] _buffer;
        private readonly Guid _connectionUid;

        public DataArrivedEventArgs(Guid peerGuid, byte[] buffer)
        {
            _buffer = buffer;
            _connectionUid = peerGuid;
        }

        public byte[] Buffer
        {
            get { return _buffer; }
        }

        public Guid ConnectionUid
        {
            get { return _connectionUid; }
        }
    }
}