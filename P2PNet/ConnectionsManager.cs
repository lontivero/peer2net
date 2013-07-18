using System;
using System.Collections.Generic;
using System.Linq;
using P2PNet.BufferManager;
using P2PNet.EventArgs;
using P2PNet.Protocols;
using Buffer = P2PNet.BufferManager.Buffer;

namespace P2PNet
{
    public class ConnectionsManager
    {
        private readonly IBufferAllocator _allocator;
        private readonly Dictionary<Guid, ConnectionBundle> _connectionBundles;
        private readonly ConnectionIoActor _ioActor;
        private readonly Listener _listener;

        public ConnectionsManager(Listener listener)
        {
            _listener = listener;
            _listener.ClientConnected += ClientConnected;

            _ioActor = new ConnectionIoActor();
            _connectionBundles = new Dictionary<Guid, ConnectionBundle>();
            _allocator = new BufferAllocator(new byte[4*1024*1024]);
        }

        public event EventHandler<PacketReceivedEventArgs> MessageReceived;

        private void ClientConnected(object sender, ConnectionEventArgs e)
        {
            var uid = Guid.NewGuid();
            var buffer = _allocator.Allocate(8*1024);
            var connection = new Connection(uid, e.Socket, buffer);
            var packetHandler = new RawPacketHandler();

            packetHandler.PacketReceived += (s, o) => PacketReceived(uid, o);

            connection.DataArrived += DataArrived;
            _ioActor.EnqueueReceive(connection);

            _connectionBundles.Add(uid, new ConnectionBundle
                {
                    Connection = connection,
                    PacketHandler = packetHandler,
                    Statistics = new ConnectionStat()
                });
        }

        private void PacketReceived(Guid connectionUid, PacketReceivedEventArgs e)
        {
            MessageReceived(connectionUid, e);
        }

        private void DataArrived(object sender, DataArrivedEventArgs e)
        {
            var bundle = _connectionBundles[e.ConnectionUid];
            bundle.PacketHandler.ProcessIncomingData(e.Buffer);
            bundle.Statistics.AddReceivedBytes(e.Buffer.Length);
            if (bundle.PacketHandler.IsWaiting)
            {
                bundle.Connection.Receive();
            }
        }

        internal void Shutdown()
        {
            var connectionsArray = new Connection[_connectionBundles.Count];
            var connections = _connectionBundles.Select(x => x.Value.Connection).ToArray();
            connections.CopyTo(connectionsArray, 0);

            foreach (var connection in connectionsArray)
            {
                connection.Close();
            }
        }
    }
}