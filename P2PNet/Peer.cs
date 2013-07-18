using System;

namespace P2PNet
{
    public class Peer
    {
        internal Connection Connection { get; private set; }

        internal Peer(Connection connection)
        {
            Guid = Guid.NewGuid();
            Connection = connection;
        }

        public Guid Guid { get; private set; }

        internal void Close()
        {
            Connection.Close();
        }
    }
}