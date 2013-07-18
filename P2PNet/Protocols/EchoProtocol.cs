using System;

namespace P2PNet.Protocols
{
    public class EchoProtocol : IProtocol
    {
        private readonly IProtocol _baseProtocol;

        public EchoProtocol(IProtocol baseProtocol)
        {
            _baseProtocol = baseProtocol;
            _baseProtocol.MessageReceived += BaseProtocolOnMessageReceived;
        }

        #region IProtocol Members

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        #endregion

        private void BaseProtocolOnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Send(e.Client, e.Message);
        }

        internal void Send(Connection connection, string message)
        {
            // _baseProtocol.Send(connection, message);
        }
    }
}