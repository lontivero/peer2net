using System;
using System.Net;
using System.Threading.Tasks;

namespace Open.P2P.IO
{
    interface IConnection
    {
        IPEndPoint Endpoint { get; }
        bool IsConnected { get; }
        Task<int> ReceiveAsync(ArraySegment<byte> buffer);
        Task<int> SendAsync(ArraySegment<byte> buffer);
        Task ConnectAsync();
        void Close();
    }
}