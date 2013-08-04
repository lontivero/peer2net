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
    internal class Connection
    {
        private readonly Socket _socket;
        private readonly IPEndPoint _endpoint;

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
            _socket = socket;
            _endpoint = endpoint;
            Uri = new Uri("tcp://" + Endpoint.Address + ':' + Endpoint.Port);
        }

        internal void Receive(Buffer buffer, Action<int, bool> callback)
        {
            Func<AsyncCallback, object, IAsyncResult> beginReceive =
                (cb, s) => CallbackOnError(() => _socket.BeginReceive(buffer.ToArraySegmentList(), SocketFlags.None, cb, s), e => callback(0, e));

            var task = Task.Factory.FromAsync<int>(beginReceive, _socket.EndReceive, this);
            task.ContinueWith(t => callback(t.Result, true), TaskContinuationOptions.NotOnFaulted)
                .ContinueWith(t => callback(0, false), TaskContinuationOptions.OnlyOnFaulted);
            task.ContinueWith(t => callback(0, false), TaskContinuationOptions.OnlyOnFaulted);
        }

        internal void Send(Buffer buffer, Action<int, bool> callback)
        {
            Func<AsyncCallback, object, IAsyncResult> beginSend =
                (cb, s) => CallbackOnError(()=> _socket.BeginSend(buffer.ToArraySegmentList(), SocketFlags.None, cb, s), e=>callback(0, e));

            var task = Task.Factory.FromAsync<int>(beginSend, _socket.EndSend, this);
            task.ContinueWith(t => callback(t.Result, true), TaskContinuationOptions.NotOnFaulted)
                .ContinueWith(t => callback(0, false), TaskContinuationOptions.OnlyOnFaulted);
            task.ContinueWith(t => callback(0, false), TaskContinuationOptions.OnlyOnFaulted);
        }

        internal void Connect(Action<bool> callback)
        {
            Func<AsyncCallback, object, IAsyncResult> beginConnect =
                (cb, s) => CallbackOnError(()=>
                    {
                        var asyncResult = _socket.BeginConnect(Endpoint, cb, s);
                        var success = asyncResult.AsyncWaitHandle.WaitOne(20, true);
                        if(!success) throw new TimeoutException("Connect timeout");
                        return asyncResult;
                    }, callback);

            var task = Task.Factory.FromAsync(beginConnect, _socket.EndConnect, this);
            task.ContinueWith(t => callback(true), TaskContinuationOptions.NotOnFaulted)
                .ContinueWith(t => callback(false), TaskContinuationOptions.OnlyOnFaulted);
            task.ContinueWith(t => callback(false), TaskContinuationOptions.OnlyOnFaulted);
        }

        internal void Close()
        {
            _socket.Close();
        }

        private T CallbackOnError<T>(Func<T> func, Action<bool> callback)
        {
            try
            {
                return func();
            }
            catch (Exception)
            {
                callback(false);
                return default(T);
            }
        }
    }
}