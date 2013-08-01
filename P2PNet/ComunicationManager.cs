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
using System.Net;
using P2PNet.BufferManager;
using P2PNet.EventArgs;
using P2PNet.Workers;

namespace P2PNet
{
    public class ComunicationManager
    {
        private readonly Listener _listener;
        private readonly IClientManager _clientManager;
        private readonly ClientWorker _worker;
        private readonly ConnectionIoActor _ioActor;
        private readonly ConcurrentDictionary<IPEndPoint, Peer> _peers;
        private readonly SpeedWatcher _globalReceiveSpeedWatcher;
        private readonly SpeedWatcher _globalSendSpeedWatcher;

        public ComunicationManager(Listener listener, IClientManager clientManager)
        {
            _listener = listener;
            _clientManager = clientManager;
            _worker = new ClientWorker();
            _ioActor = new ConnectionIoActor(_worker);
            _peers = new ConcurrentDictionary<IPEndPoint, Peer>();

            _globalReceiveSpeedWatcher = new SpeedWatcher();
            _globalSendSpeedWatcher = new SpeedWatcher();

            _worker.Queue(CalculateSpeed, TimeSpan.FromSeconds(0.5));
            _worker.Start();

            _listener.ConnectionRequested += NewPeerConnected;
        }

        public SpeedWatcher GlobalReceiveSpeedWatcher
        {
            get { return _globalReceiveSpeedWatcher; }
        }

        public SpeedWatcher GlobalSendSpeedWatcher
        {
            get { return _globalSendSpeedWatcher; }
        }

        private void NewPeerConnected(object sender, ConnectionEventArgs args)
        {
            var connection = new Connection(args.Socket);

            PerformanceCounters.IncommingConnections.Increment();

            var peer = new Peer(connection);
            _peers.TryAdd(peer.Connection.Endpoint, peer);

//            _ioActor.EnqueueReceive(1, connection, peer.ReceiveBandwidthController, OnDataArrive);
            _clientManager.OnPeerConnected(peer);
        }


        private void CalculateSpeed()
        {
            foreach (var peer in _peers.Values)
            {
                var sendWatcher = peer.SendSpeedWatcher;
                var receiveWatcher = peer.ReceiveSpeedWatcher;
                sendWatcher.CalculateAndReset();
                receiveWatcher.CalculateAndReset();

                peer.SendBandwidthController.Update(sendWatcher.BytesPerSecond, sendWatcher.MeasuredDeltaTime);
                peer.ReceiveBandwidthController.Update(receiveWatcher.BytesPerSecond, receiveWatcher.MeasuredDeltaTime);
            }
        }

        private void OnDataArrive(Connection connection, byte[] data)
        {
            _worker.Queue(() =>
                {
                    var peer = _peers[connection.Endpoint];
                    var butesReceived = data.Length;

                    GlobalReceiveSpeedWatcher.AddBytes(butesReceived);

                    peer.Statistics.AddReceivedBytes(butesReceived);
                    peer.ReceiveSpeedWatcher.AddBytes(butesReceived);

                    //if (peer.PacketHandler.IsWaiting)
                    //{
                    //    _ioActor.EnqueueReceive(peer.PacketHandler.PendingBytes, peer.Connection,
                    //                            peer.ReceiveBandwidthController, OnDataArrive);
                    //}
                    _clientManager.OnPeerDataReceived(peer, data);
                });
        }

        public void Connect(IPEndPoint endpoint)
        {
            _ioActor.EnqueueConnect(endpoint, OnConnected);
        }

        private void OnConnected(Connection connection)
        {
            var peer = new Peer(connection);
            _peers.TryAdd(connection.Endpoint, peer);

//            _ioActor.EnqueueReceive(1, connection, peer.ReceiveBandwidthController, OnDataArrive);
            _clientManager.OnPeerConnected(peer);
        }

        public void Receive(int bytes, IPEndPoint endpoint)
        {
            var peer = _peers[endpoint];
            _ioActor.EnqueueReceive(bytes, peer.Connection, peer.ReceiveBandwidthController, OnDataArrive);
        }

        public void SendTo(byte[] message, IPEndPoint endpoint)
        {
            var peer = _peers[endpoint];
            _ioActor.EnqueueSend(message, peer.Connection, peer.SendBandwidthController, OnDataSent);
        }

        private void OnDataSent(Connection connection, byte[] data)
        {
            _worker.Queue(() =>
            {
                var peer = _peers[connection.Endpoint];
                var bytesSent = data.Length;

                GlobalSendSpeedWatcher.AddBytes(bytesSent);

                peer.Statistics.AddSentBytes(bytesSent);
                peer.SendSpeedWatcher.AddBytes(bytesSent);

                _clientManager.OnPeerDataSent(peer, data);
            });
        }
    }
}
