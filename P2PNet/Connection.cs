//
// - Connection.cs
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
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Buffer = P2PNet.BufferManager.Buffer;

namespace P2PNet
{
    public class Connection
    {
        private readonly Socket _socket;
        private readonly IPEndPoint _endpoint;

        public Guid Id { get; private set; }
        public Uri Uri { get; private set; }

        public IPEndPoint Endpoint
        {
            get { return _endpoint; }
        }

        public Connection(IPEndPoint endpoint)
            : this(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp), endpoint)
        {}

        public Connection(Socket socket)
            : this(socket, (IPEndPoint)socket.RemoteEndPoint)
        {}

        public Connection(Socket socket, IPEndPoint endpoint)
        {
            Id = Guid.NewGuid();
            _socket = socket;
            _endpoint = endpoint;
            Uri = new Uri("tcp://" + Endpoint.Address + ':' + Endpoint.Port);
        }

        internal void Receive(Buffer buffer, Action<int> onFinishCallback)
        {
            Func<AsyncCallback, object, IAsyncResult> beginReceive =
                (callback, s) => _socket.BeginReceive(buffer.ToArraySegmentList(), SocketFlags.None, callback, s);

            Task.Factory.FromAsync<int>(beginReceive, _socket.EndReceive, this)
                .ContinueWith(task =>
                    {
                        if(task.IsFaulted) return;
                        int bytesRead = task.Result;
                        onFinishCallback(bytesRead);
                    });
        }

        internal void Send(Buffer buffer, Action<int> onFinishCallback)
        {
            Func<AsyncCallback, object, IAsyncResult> beginSend =
                (callback, s) => _socket.BeginSend(buffer.ToArraySegmentList(), SocketFlags.None, callback, s);

            Task.Factory.FromAsync<int>(beginSend, _socket.EndSend, this)
                .ContinueWith(task =>
                {
                    int sentByteCount = task.Result;
                    onFinishCallback(sentByteCount);
                });
        }

        internal void Connect(Action onConnected)
        {
            Func<AsyncCallback, object, IAsyncResult> beginConnect =
                (callback, s) => _socket.BeginConnect( Endpoint, callback, s);

            Task.Factory.FromAsync(beginConnect, _socket.EndConnect, this)
                .ContinueWith(task => onConnected());
        }

        internal void Close()
        {
            _socket.Close();
        }
    }
}