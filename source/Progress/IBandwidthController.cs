using System;

namespace Peer2Net.Progress
{
    internal interface IBandwidthController
    {
        bool CanTransmit(int bytesCount);
        void SetTransmittion(int bytesCount);
        void Update(double measuredSpeed, TimeSpan deltaTime);
    }
}