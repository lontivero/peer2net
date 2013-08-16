using System.Net.Sockets;

namespace Peer2Net.EventArgs
{
    internal class NewConnectionEventArgs : System.EventArgs
    {
        public NewConnectionEventArgs(Socket socket)
        {
            Socket = socket;
        }

        internal Socket Socket { get; set; }
    }
}