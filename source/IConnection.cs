using System.Net;

namespace Peer2Net
{
    internal delegate void ConnectionIoCallback(int bytes, bool success);
    internal delegate void ConnectionConnectCallback(bool success);

    interface IConnection
    {
        IPEndPoint Endpoint { get; }
        bool IsConnected { get; }
        void Receive(BufferManager.Buffer buffer, ConnectionIoCallback callback);
        void Send(BufferManager.Buffer buffer, ConnectionIoCallback callback);
        void Connect(ConnectionConnectCallback callback);
        void Close();
    }
}