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
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Open.P2P.BufferManager;
using Open.P2P.EventArgs;
using Open.P2P.Progress;
using Open.P2P.Streams;
using Open.P2P.Utils;
using Open.P2P.Workers;
using TcpListener = Open.P2P.Listeners.TcpListener;

namespace Open.P2P.IO
{
    /// <summary>
    /// CommunicationManager is one of the most important classes in the Peer2Net class library given it provides 
    /// the main API functionalities, these are the four methods: Connect, Disconnect, Send and Receive.
    /// </summary>
    public class CommunicationManager
    {
        private readonly TcpListener _listener;
        private readonly ClientWorker _worker;
        private readonly ConcurrentDictionary<IPEndPoint, Peer> _peers;

        public event EventHandler<PeerEventArgs> PeerConnected;
        public event EventHandler<ConnectionEventArgs> ConnectionClosed;


        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationManager"/> class.
        /// </summary>
        public CommunicationManager(TcpListener listener)
            : this()
        {
            _listener = listener;
            _listener.ConnectionRequested += NewPeerConnected;
        }

        public CommunicationManager()
        {
            _worker = new ClientWorker();
            _peers = new ConcurrentDictionary<IPEndPoint, Peer>();

            GlobalReceiveSpeedWatcher = new SpeedWatcher();
            GlobalSendSpeedWatcher = new SpeedWatcher();

            _worker.QueueForever(CalculateSpeed, 500.Milliseconds());
            _worker.Start();
        }

        /// <summary>
        /// Gets the global receive speed watcher.
        /// </summary>
        /// <value>
        /// The global receive speed watcher.
        /// </value>
        public SpeedWatcher GlobalReceiveSpeedWatcher { get; private set; }

        /// <summary>
        /// Gets the global send speed watcher.
        /// </summary>
        /// <value>
        /// The global send speed watcher.
        /// </value>
        public SpeedWatcher GlobalSendSpeedWatcher { get; private set; }

        /// <summary>
        /// Connects to the specified endpoint.
        /// </summary>
        /// <param name="endpoint">The ip endpoint.</param>
        public async Task<Peer> ConnectAsync(IPEndPoint endpoint)
        {
            Guard.NotNull(endpoint, "endpoint");
            var connection = new Connection(endpoint);

            await connection.ConnectAsync();
            return RegisterPeer(connection);
        }

        internal async Task<int> ReceiveAsync(byte[] buffer, int offset, int count, IPEndPoint endpoint)
        {
            Guard.NotNull(buffer, "buffer");
            Guard.NotNull(endpoint, "endpoint");
            Guard.ContainsKey(_peers, endpoint, "Peer is not registered.");
 
            var peer = _peers[endpoint];

            try
            {
                var received = await peer.Connection.ReceiveAsync(new ArraySegment<byte>(buffer, offset, count));

                GlobalReceiveSpeedWatcher.AddBytes(received);
                peer.Statistics.AddReceivedBytes(received);
                peer.ReceiveSpeedWatcher.AddBytes(received);
                return received;
            }
            catch (SocketException)
            {
                DisconnectPeer(peer);
                return 0;
            }
        }

        internal async Task<int> SendAsync(byte[] buffer, int offset, int count, IPEndPoint endpoint)
        {
            Guard.NotNull(endpoint, "endpoint");
            Guard.NotNull(buffer, "buffer.Array");

            var peer = _peers[endpoint];
            try
            {
                var sent = await peer.Connection.SendAsync(new ArraySegment<byte>(buffer, offset, count));

                GlobalSendSpeedWatcher.AddBytes(sent);
                peer.Statistics.AddSentBytes(sent);
                peer.SendSpeedWatcher.AddBytes(sent);

                return sent;
            }
            catch (SocketException e)
            {
                DisconnectPeer(peer);
                return 0;
            }
        }

        public async Task SendAsync(byte[] buffer, int offset, int count, IEnumerable<IPEndPoint> endpoints)
        {
            Guard.NotNull(buffer, "buffer.Array");
            Guard.NotNull(endpoints, "endpoints");
            var t = endpoints.Select(endpoint => SendAsync(buffer, offset, count, endpoint));

            await Task.WhenAll(t);
        }

        private void NewPeerConnected(object sender, NewConnectionEventArgs args)
        {
            PerformanceCounters.IncommingConnections.Increment();
            var connection = new Connection(args.Socket);
            RegisterPeer(connection);
        }

        private Peer RegisterPeer(IConnection connection)
        {
            var endpoint = connection.Endpoint;
            var stream = new PeerStream(this, endpoint);

            var peer = new Peer(stream, connection);
            _peers.TryAdd(endpoint, peer);

            Events.RaiseAsync(PeerConnected, this, new PeerEventArgs(peer));
            return peer;
        }

        private void CalculateSpeed()
        {
            foreach (var peer in _peers.Values)
            {
                var sendWatcher = peer.SendSpeedWatcher;
                var receiveWatcher = peer.ReceiveSpeedWatcher;
                sendWatcher.CalculateAndReset();
                receiveWatcher.CalculateAndReset();

                peer.SendBandwidthController.UpdateSpeed((int)sendWatcher.BytesPerSecond, sendWatcher.MeasuredDeltaTime);
                peer.ReceiveBandwidthController.UpdateSpeed((int)receiveWatcher.BytesPerSecond, receiveWatcher.MeasuredDeltaTime);
            }
        }


        private void DisconnectPeer(Peer peer)
        {
            peer.Disconnect();
            Events.RaiseAsync(ConnectionClosed, this, new ConnectionEventArgs(peer.EndPoint));
        }
    }
}
