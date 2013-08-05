//
// - ClientManager.cs
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

using System.Net;
using P2PNet.Workers;

namespace P2PNet
{
    public abstract class ClientManager
    {
        private readonly BackgroundWorker _worker;

        protected ClientManager()
        {
            _worker = new BackgroundWorker();
            _worker.Start();
        }

        public abstract void Connected(Peer peer);
        public abstract void ConnectFailure(IPEndPoint endpoint);
        public abstract void Closed(Peer peer);
        public abstract void DataSent(Peer peer, byte[] data);
        public abstract void DataReceived(Peer peer, byte[] data);

        internal void OnPeerConnected(Peer peer)
        {
            _worker.Queue(()=>Connected(peer));
        }

        internal void OnPeerDataReceived(Peer peer, byte[] data)
        {
            _worker.Queue(()=>DataReceived(peer, data));
        }

        internal void OnPeerDataSent(Peer peer, byte[] data)
        {
            _worker.Queue(()=>DataSent(peer, data));
        }

        internal void OnClosed(Peer peer)
        {
            _worker.Queue(() => Closed(peer));
        }

        public void OnPeerConnectFailure(IPEndPoint endpoint)
        {
            _worker.Queue(() => ConnectFailure(endpoint));
        }
    }
}
