using System.Net;

namespace Peer2Net
{
    public class NewDiscoveredNodeEventArgs : System.EventArgs
    {
        private readonly IPEndPoint _remoteEndPoint;

        public NewDiscoveredNodeEventArgs(IPEndPoint remoteEndPoint)
        {
            _remoteEndPoint = remoteEndPoint;
        }

        public IPEndPoint DiscoveredEndPoint
        {
            get { return _remoteEndPoint; }
        }
    }
}
