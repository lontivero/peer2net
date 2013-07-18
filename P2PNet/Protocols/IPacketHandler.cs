using System;

namespace P2PNet.Protocols
{
    public interface IPacketHandler
    {
        bool IsWaiting { get; }
        event EventHandler<PacketReceivedEventArgs> PacketReceived;
        void ProcessIncomingData(byte[] data);
    }
}