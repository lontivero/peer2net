namespace P2PNet
{
    internal class ConnectionIoActor
    {
        private readonly EndlessWorker<ReceiveJobState> _worker;

        public ConnectionIoActor()
        {
            _worker = new EndlessWorker<ReceiveJobState>(Receive);
        }

        private void Receive(ReceiveJobState jobState)
        {
            const bool canReceive = true;

            if (canReceive)
            {
                jobState.Connection.Receive();
            }
            else
            {
                _worker.Enqueue(jobState);
            }
        }

        public void EnqueueReceive(Connection connection)
        {
            Receive(new ReceiveJobState(connection));
        }
    }
}