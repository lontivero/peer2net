using System;
using System.Net.Sockets;

namespace AsyncTcpServer
{
    class SocketPacket
    {
        public Socket CurrentSocket;
        public byte[] DataBuffer = new byte[4 * 1024];
    }
}
