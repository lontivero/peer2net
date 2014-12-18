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
using Open.P2P.Utils;

namespace Open.P2P.IO
{
    internal class Connection : IConnection
    {
        private static readonly BlockingPool<SocketAwaitable> SocketAwaitablePool =
            new BlockingPool<SocketAwaitable>(() => new SocketAwaitable(new SocketAsyncEventArgs()));

        private readonly Socket _socket;
        private readonly IPEndPoint _endpoint;
        private bool _socketDisposed;

        public IPEndPoint Endpoint
        {
            get { return _endpoint; }
        }

        public bool IsConnected
        {
            get { return _socket.Connected; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", 
            Justification = "The framework manages the socket lifetime")]
        internal Connection(IPEndPoint endpoint)
            : this(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp), endpoint)
        {}

        internal Connection(Socket socket)
            : this(socket, (IPEndPoint)socket.RemoteEndPoint)
        {}

        internal Connection(Socket socket, IPEndPoint endpoint)
        {
            _socket = socket;
            _endpoint = endpoint;
            _socketDisposed = false;
        }

        public async Task<int> ReceiveAsync(ArraySegment<byte> buffer)
        {

            var awaitable = SocketAwaitablePool.Take();
            try
            {
                awaitable.EventArgs.SetBuffer(buffer.Array, buffer.Offset, buffer.Count);
                await _socket.ReceiveAsync(awaitable);
                return awaitable.EventArgs.BytesTransferred;
            }
            finally
            {
                SocketAwaitablePool.Add(awaitable);    
            }
        }

        public async Task<int> SendAsync(ArraySegment<byte> buffer)
        {
            var awaitable = SocketAwaitablePool.Take();
            try
            {
                awaitable.EventArgs.SetBuffer(buffer.Array, buffer.Offset, buffer.Count);
                await _socket.SendAsync(awaitable);
                return awaitable.EventArgs.BytesTransferred;
            }
            finally
            {
                SocketAwaitablePool.Add(awaitable);    
            }
        }

        public async Task ConnectAsync()
        {
            var awaitable = SocketAwaitablePool.Take();
            try
            {
                awaitable.EventArgs.RemoteEndPoint = Endpoint;
                await _socket.ConnectAsync(awaitable);
            }
            finally
            {
                SocketAwaitablePool.Add(awaitable);
            }
        }

        public void Close()
        {
            try
            {
                _socket.Close();
            }
            finally 
            {
                _socketDisposed = true;
            }
        }

        private bool CheckDisconnectedOrDisposed()
        {
            var disconnected = !IsConnected;
            if (disconnected || _socketDisposed)
            {
            }
            return disconnected;
        }
    }
}