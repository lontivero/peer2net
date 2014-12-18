using System.Net.Sockets;

namespace Open.P2P.EventArgs
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