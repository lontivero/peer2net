using System;

namespace P2PNet.Progress
{
    public interface IBandwidthController
    {
        bool CanTransmit(int bytesCount);
        void Update(double measuredSpeed, TimeSpan deltaTime);
    }
}