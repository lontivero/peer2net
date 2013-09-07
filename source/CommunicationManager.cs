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
using Peer2Net.BufferManager;
using Peer2Net.EventArgs;
using Peer2Net.Progress;
using Peer2Net.Utils;
using Peer2Net.Workers;

namespace Peer2Net
{
    /// <summary>
    /// CommunicationManager is one of the most important classes in the Peer2Net class library given it provides 
    /// the main API functionalities, these are the four methods: Connect, Disconnect, Send and Receive.
    /// </summary>
    public class CommunicationManager
    {
        private readonly TcpListener _listener;
        private readonly ClientWorker _worker;
        private readonly ConnectionIoActor _ioActor;
        private readonly ConcurrentDictionary<IPEndPoint, Peer> _peers;
        private readonly SpeedWatcher _globalReceiveSpeedWatcher;
        private readonly SpeedWatcher _globalSendSpeedWatcher;

        public event EventHandler<PeerEventArgs> PeerConnected;
        public event EventHandler<ConnectionEventArgs> ConnectionFailed;
        public event EventHandler<ConnectionEventArgs> ConnectionClosed;
        public event EventHandler<PeerDataEventArgs> PeerDataReceived;
        public event EventHandler<PeerDataEventArgs> PeerDataSent;


        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationManager"/> class.
        /// </summary>
        /// <example>
        ///    var portNumber = 9876;
        ///    var listener = new Listener(portNumber);
        ///    var comunicationManager = new CommunicationManager(_listener);
        ///    comunicationManager.PeerConnected += ChatOnMemberConnected;
        ///    comunicationManager.ConnectionClosed += ChatOnMemberDisconnected;
        ///    comunicationManager.ConnectionFailed += ChatOnMemberConnectionFailure;
        ///    comunicationManager.PeerDataReceived += OnPeerDataReceived;
        ///
        ///    listener.Start();
        /// </example>
        /// <param name="listener">The incomming connections <see cref="Listener"/>.</param>
        public CommunicationManager(TcpListener listener)
        {
            _listener = listener;
            _worker = new ClientWorker();
            _ioActor = new ConnectionIoActor(_worker, new BufferAllocator(new byte[1 << 16]));
            _peers = new ConcurrentDictionary<IPEndPoint, Peer>();

            _globalReceiveSpeedWatcher = new SpeedWatcher();
            _globalSendSpeedWatcher = new SpeedWatcher();

            _worker.QueueForever(CalculateSpeed, 500.Milliseconds());
            _worker.Start();

            _listener.ConnectionRequested += NewPeerConnected;
        }

        /// <summary>
        /// Gets the global receive speed watcher.
        /// </summary>
        /// <value>
        /// The global receive speed watcher.
        /// </value>
        public SpeedWatcher GlobalReceiveSpeedWatcher
        {
            get { return _globalReceiveSpeedWatcher; }
        }

        /// <summary>
        /// Gets the global send speed watcher.
        /// </summary>
        /// <value>
        /// The global send speed watcher.
        /// </value>
        public SpeedWatcher GlobalSendSpeedWatcher
        {
            get { return _globalSendSpeedWatcher; }
        }

        /// <summary>
        /// Connects to the specified endpoint.
        /// </summary>
        /// <remarks>
        /// Connect is an async operation that raises the <see cref="PeerConnected"/> event when connection is successful;
        /// <see cref="ConnectionFailed"/> is raised otherwise
        /// </remarks>
        /// <param name="endpoint">The ip endpoint.</param>
        public void Connect(IPEndPoint endpoint)
        {
            Guard.NotNull(endpoint, "endpoint");
            var connection = new Connection(endpoint);

            _ioActor.Connect(connection, OnConnected, OnConnectError);
        }

        /// <summary>
        /// Disconnects the specified endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        public void Disconnect(IPEndPoint endpoint)
        {
            Guard.NotNull(endpoint, "endpoint");

            var peer = _peers[endpoint];
            CloseConnection(peer.Connection);
        }

        /// <summary>
        /// Receive the amount of bytes from the specified ip endpoint.
        /// </summary>
        /// <remarks>
        /// Receive is an async operation that raises the <see  cref="PeerDataReceived"/> event when data arrive.
        /// </remarks>
        /// <param name="bytes">The bytes count</param>
        /// <param name="endpoint">The ip endpoint.</param>
        public void Receive(int bytes, IPEndPoint endpoint)
        {
            Guard.IsGreaterOrEqualTo(bytes, 0, "bytes");
            Guard.NotNull(endpoint, "endpoint");
            Guard.ContainsKey(_peers, endpoint, "Peer is not registered.");
 
            var peer = _peers[endpoint];
            _ioActor.Receive(bytes, peer.Connection, peer.ReceiveBandwidthController, OnDataArrive, OnDataArriveError);
        }

        /// <summary>
        /// Send the message to the specified ip endpoint.
        /// </summary>
        /// <remarks>
        /// Send is an async operation that raises the <see  cref="PeerDataSent"/> event after message is sent.
        /// </remarks>
        /// <param name="message">The message.</param>
        /// <param name="endpoint">The ip endpoint.</param>
        public void Send(byte[] message, IPEndPoint endpoint)
        {
            Guard.NotNull(endpoint, "endpoint");
            Guard.NotNull(message, "message");

            var peer = _peers[endpoint];
            _ioActor.Send(message, peer.Connection, peer.SendBandwidthController, OnDataSent, OnDataSentError);
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

        private void NewPeerConnected(object sender, NewConnectionEventArgs args)
        {
            var connection = new Connection(args.Socket);

            PerformanceCounters.IncommingConnections.Increment();

            RegisterPeer(connection);
        }

        private void RegisterPeer(IConnection connection)
        {
            var peer = new Peer(connection);
            _peers.TryAdd(peer.Connection.Endpoint, peer);

            Events.RaiseAsync(PeerConnected, this, new PeerEventArgs(peer));
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

        private void OnConnected(IConnection connection)
        {
            RegisterPeer(connection);
        }

        private void OnConnectError(IConnection connection)
        {
            Events.RaiseAsync(ConnectionFailed, this, new ConnectionEventArgs(connection.Endpoint));
        }

        private void OnDataArrive(IConnection connection, byte[] data)
        {
            _worker.Queue(() => {
                var peer = _peers[connection.Endpoint];
                var butesReceived = data.Length;

                GlobalReceiveSpeedWatcher.AddBytes(butesReceived);

                peer.Statistics.AddReceivedBytes(butesReceived);
                peer.ReceiveSpeedWatcher.AddBytes(butesReceived);

                Events.RaiseAsync(PeerDataReceived, this, new PeerDataEventArgs(peer, data));
            });
        }

        private void OnDataArriveError(IConnection connection)
        {
            CloseConnection(connection);
        }

        private void OnDataSent(IConnection connection, byte[] data)
        {
            _worker.Queue(() => {
                var peer = _peers[connection.Endpoint];
                var bytesSent = data.Length;

                GlobalSendSpeedWatcher.AddBytes(bytesSent);

                peer.Statistics.AddSentBytes(bytesSent);
                peer.SendSpeedWatcher.AddBytes(bytesSent);

                Events.RaiseAsync(PeerDataSent, this, new PeerDataEventArgs(peer, data));
            });
        }

        private void OnDataSentError(IConnection connection)
        {
            CloseConnection(connection);
        }

        private void CloseConnection(IConnection connection)
        {
            Peer peer;
            _peers.TryRemove(connection.Endpoint, out peer);
            connection.Close();
            Events.RaiseAsync(ConnectionClosed, this, new ConnectionEventArgs(connection.Endpoint));
        }
    }
}
