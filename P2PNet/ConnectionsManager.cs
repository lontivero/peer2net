//
// - ConnectionsManager.cs
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
using System.Linq;
using P2PNet.BufferManager;
using P2PNet.EventArgs;
using P2PNet.Utils;

namespace P2PNet
{
    public class ConnectionsManager
    {
        private readonly IBufferAllocator _allocator;
        private readonly ConcurrentDictionary<Guid, Connection> _connections;
        private readonly Listener _listener;

        internal event EventHandler<PeerConnectdEventArgs> PeerConnected;

        public ConnectionsManager(Listener listener)
        {
            _listener = listener;

            _connections = new ConcurrentDictionary<Guid, Connection>();
            _allocator = new BufferAllocator(new byte[4*1024*1024]);
            _listener.ConnectionRequested += NewConnectionRequested;
        }


        private void NewConnectionRequested(object sender, ConnectionEventArgs e)
        {
            var uid = Guid.NewGuid();
            var connection = new Connection(uid, e.Socket, _allocator);
            _connections.TryAdd(uid, connection);

            PerformanceCounters.IncommingConnections.Increment();

            RaisePeerConnectedEvent(new PeerConnectdEventArgs{
                Connection = connection
            });
        }


        internal void Shutdown()
        {
            var connectionsArray = new Connection[_connections.Count];
            var connections = _connections.Select(x => x.Value).ToArray();
            connections.CopyTo(connectionsArray, 0);

            foreach (var connection in connectionsArray)
            {
                connection.Close();
            }
        }

        private void RaisePeerConnectedEvent(PeerConnectdEventArgs args)
        {
            Events.RaiseAsync(PeerConnected, this, args);
        }

    }

    internal class PeerConnectdEventArgs : System.EventArgs
    {
        public Connection Connection { get; set; }
    }
}