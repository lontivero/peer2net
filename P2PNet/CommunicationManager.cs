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
using System.Collections.Generic;
using System.Net;
using P2PNet.BufferManager;
using P2PNet.EventArgs;
using P2PNet.Progress;
using P2PNet.Utils;
using P2PNet.Workers;

namespace P2PNet
{
    /// <summary>
    /// 
    /// </summary>
    public class CommunicationManager
    {
        private readonly Listener _listener;
        private readonly ClientManager _clientManager;
        private readonly ClientWorker _worker;
        private readonly ConnectionIoActor _ioActor;
        private readonly ConcurrentDictionary<IPEndPoint, Peer> _peers;
        private readonly SpeedWatcher _globalReceiveSpeedWatcher;
        private readonly SpeedWatcher _globalSendSpeedWatcher;

        public CommunicationManager(Listener listener, ClientManager clientManager)
        {
            _listener = listener;
            _clientManager = clientManager;
            _worker = new ClientWorker();
            _ioActor = new ConnectionIoActor(_worker);
            _peers = new ConcurrentDictionary<IPEndPoint, Peer>();

            _globalReceiveSpeedWatcher = new SpeedWatcher();
            _globalSendSpeedWatcher = new SpeedWatcher();

            _worker.QueueForever(CalculateSpeed, TimeSpan.FromSeconds(0.5));
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

        public void Connect(IPEndPoint endpoint)
        {
            Guard.NotNull(endpoint, "endpoint");
            var connection = new Connection(endpoint);

            _ioActor.EnqueueConnect(connection, OnConnected, OnConnectError);
        }

        public void Receive(int bytes, IPEndPoint endpoint)
        {
            Guard.IsGreaterOrEqualTo(bytes, 0, "bytes");
            Guard.NotNull(endpoint, "endpoint");
            Guard.ContainsKey(_peers, endpoint, "Peer is not registered.");
 
            var peer = _peers[endpoint];
            _ioActor.EnqueueReceive(bytes, peer.Connection, peer.ReceiveBandwidthController, OnDataArrive, OnDataArriveError);
        }

        public void Send(byte[] message, IPEndPoint endpoint)
        {
            Guard.NotNull(endpoint, "endpoint");
            Guard.NotNull(message, "message");

            var peer = _peers[endpoint];
            _ioActor.EnqueueSend(message, peer.Connection, peer.SendBandwidthController, OnDataSent, OnDataSentError);
        }

        public void Send(byte[] message, IEnumerable<IPEndPoint> endpoints)
        {
            Guard.NotNull(message, "message");
            Guard.NotNull(endpoints, "endpoints");

            foreach (var endpoint in endpoints)
            {
                Send(message, endpoint);
            }
        }

        private void NewPeerConnected(object sender, ConnectionEventArgs args)
        {
            var connection = new Connection(args.Socket);

            PerformanceCounters.IncommingConnections.Increment();

            RegisterPeer(connection);
        }

        private void RegisterPeer(Connection connection)
        {
            var peer = new Peer(connection);
            _peers.TryAdd(peer.Connection.Endpoint, peer);

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

        private void OnConnected(Connection connection)
        {
            RegisterPeer(connection);
        }

        private void OnConnectError(Connection connection)
        {
            _clientManager.OnPeerConnectFailure(connection.Endpoint);
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

                    _clientManager.OnPeerDataReceived(peer, data);
                });
        }

        private void OnDataArriveError(Connection connection)
        {
            CloseConnection(connection);
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

        private void OnDataSentError(Connection connection)
        {
            CloseConnection(connection);
        }

        private void CloseConnection(Connection connection)
        {
            Peer peer;
            _peers.TryRemove(connection.Endpoint, out peer);
            connection.Close();
            _clientManager.OnClosed(peer);
        }
    }
}
