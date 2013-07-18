using System;

namespace P2PNet.Protocols
{
    public interface IProtocol
    {
        event EventHandler<MessageReceivedEventArgs> MessageReceived;
        // void Send(Connection connection, string message);
    }
}