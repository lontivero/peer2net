using System;
using System.Threading.Tasks;

namespace Open.P2P.Progress
{
    internal interface IBandwidthController
    {
        Task WaitToTransmit(int bytesCount);
        void UpdateSpeed(int speed, TimeSpan deltaTime);
    }
}