namespace P2PNet
{
    public class ConnectionStat
    {
        private int _receivedByteCount;

        public void AddReceivedBytes(int byteCount)
        {
            _receivedByteCount += byteCount;
        }
    }
}