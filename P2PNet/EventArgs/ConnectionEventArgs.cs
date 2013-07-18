using System.Net.Sockets;

namespace P2PNet.EventArgs
{
    internal class ConnectionEventArgs : System.EventArgs
    {
        public ConnectionEventArgs(Socket socket)
        {
            Socket = socket;
        }

        internal Socket Socket { get; set; }
    }
}