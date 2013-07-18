namespace P2PNet
{
    internal class ReceiveJobState
    {
        private readonly Connection _connection;

        public ReceiveJobState(Connection connection)
        {
            _connection = connection;
        }

        public Connection Connection
        {
            get { return _connection; }
        }
    }
}