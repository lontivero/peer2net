using System;

namespace Peer2Net.Progress
{
    public interface IBandwidthController
    {
        bool CanTransmit(int bytesCount);
        void Update(double measuredSpeed, TimeSpan deltaTime);
    }
}