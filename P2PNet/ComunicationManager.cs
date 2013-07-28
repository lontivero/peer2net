//
// - ComunicationManager.cs
// 
// Author:
//     Lucas Ontivero <lucasontivero@gmail.com>
// 
// Copyright 2013 Lucas E. Ontivero
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//  http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 

// <summary></summary>

using System;
using System.Collections.Concurrent;
using P2PNet.BufferManager;
using P2PNet.EventArgs;
using P2PNet.Progress;
using P2PNet.Protocols;
using P2PNet.Workers;

namespace P2PNet
{
    class ComunicationManager
    {
        private readonly ConnectionsManager _connectionsManager;
        private readonly TimedWorker _timedWorker;
        private readonly ConnectionIoActor _ioActor;
        private readonly ConcurrentDictionary<Guid, Peer> _peers;
        private readonly SpeedWatcher _globalReceiveSpeedWatcher;
        private readonly SpeedWatcher _globalSendSpeedWatcher;
        private BackgroundWorker _worker;

        public event EventHandler<PacketReceivedEventArgs> MessageReceived;


        public ComunicationManager(ConnectionsManager connectionsManager)
        {
            _connectionsManager = connectionsManager;
            _timedWorker = new TimedWorker();
            _ioActor = new ConnectionIoActor(_timedWorker);
            _peers = new ConcurrentDictionary<Guid, Peer>();

            _globalReceiveSpeedWatcher = new SpeedWatcher();
            _globalSendSpeedWatcher = new SpeedWatcher();

            _connectionsManager.PeerConnected += NewPeerConnected;
            _timedWorker.Queue(TimeSpan.FromSeconds(0.5), CalculateSpeed);
            _worker = new BackgroundWorker();
            _timedWorker.Start();
        }

        private void NewPeerConnected(object sender, PeerConnectdEventArgs args)
        {
            var connection = args.Connection;

            var packetHandler = new RawPacketHandler();

            var peer = new Peer(connection, packetHandler);
            _peers.TryAdd(peer.Id, peer);

            packetHandler.PacketReceived += (s, o) => PacketReceived(peer.Id, o);
            connection.DataArrived += DataArrived;
            _ioActor.EnqueueReceive(1, connection, peer.ReceiveBandwidthController);
        }


        private void CalculateSpeed()
        {
            foreach (var peer in _peers.Values)
            {
                var sendWatcher = peer.SendSpeedWatcher;
                var receiveWatcher = peer.ReceiveSpeedWatcher;
                sendWatcher.CalculateAndReset();
                receiveWatcher.CalculateAndReset();

                peer.SendBandwidthController.Update(sendWatcher.BytesPerSecond, sendWatcher.Interval);
                peer.ReceiveBandwidthController.Update(receiveWatcher.BytesPerSecond, receiveWatcher.Interval);
            }
        }

        private void PacketReceived(Guid connectionUid, PacketReceivedEventArgs e)
        {
            MessageReceived(connectionUid, e);
        }

        private void DataArrived(object sender, DataArrivedEventArgs e)
        {
            _worker.Enqueue(() =>
                {
                    var peer = _peers[e.Source];
                    var butesReceived = e.Buffer.Length;

                    _globalReceiveSpeedWatcher.AddBytes(butesReceived);

                    peer.PacketHandler.ProcessIncomingData(e.Buffer);
                    peer.Statistics.AddReceivedBytes(butesReceived);
                    peer.ReceiveSpeedWatcher.AddBytes(butesReceived);

                    if (peer.PacketHandler.IsWaiting)
                    {
                        _ioActor.EnqueueReceive(peer.PacketHandler.PendingBytes, peer.Connection,
                                                peer.ReceiveBandwidthController);
                    }
                });
        }
    }
}
