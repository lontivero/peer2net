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
using P2PNet.Workers;
using Buffer = P2PNet.BufferManager.Buffer;

namespace P2PNet
{
    internal class Connection
    {
        private static readonly BlockingQueue<SocketAsyncEventArgs> _sendRecvSaeaPool = new BlockingQueue<SocketAsyncEventArgs>();
        private static readonly BlockingQueue<SocketAsyncEventArgs> _connectSaeaPool = new BlockingQueue<SocketAsyncEventArgs>();

        private readonly Socket _socket;
        private readonly IPEndPoint _endpoint;
 
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
        }

        private SocketAsyncEventArgs GetSendRecvSocketAsyncEventArgs()
        {
            return _sendRecvSaeaPool.Count > 0
                    ? _sendRecvSaeaPool.Take()
                    : new Func<SocketAsyncEventArgs>(() =>
                    {
                        var e = new SocketAsyncEventArgs();
                        e.Completed += SendRecvCompleted;
                        return e;
                    })();
        }

        private SocketAsyncEventArgs GetConnectSocketAsyncEventArgs()
        {
            return _connectSaeaPool.Count > 0
                    ? _connectSaeaPool.Take()
                    : new Func<SocketAsyncEventArgs>(() =>
                    {
                        var e = new SocketAsyncEventArgs();
                        e.Completed += ConnectCompleted;
                        return e;
                    })();
        }


        private void SendRecvCompleted(object sender, SocketAsyncEventArgs socketAsyncEventArgs)
        {
            try
            {
                var callback = (Action<int, bool>) socketAsyncEventArgs.UserToken;

                if (socketAsyncEventArgs.SocketError != SocketError.Success ||
                    socketAsyncEventArgs.BytesTransferred == 0)
                {
                    callback(0, false);
                    return;
                }

                callback(socketAsyncEventArgs.BytesTransferred, true);
            }
            finally
            {
                _sendRecvSaeaPool.Add(socketAsyncEventArgs);
            }
        }

        private void ConnectCompleted(object sender, SocketAsyncEventArgs socketAsyncEventArgs)
        {
            try
            {
                var callback = (Action<bool>)socketAsyncEventArgs.UserToken;
                var success = socketAsyncEventArgs.SocketError == SocketError.Success;
                callback(success);
            }
            finally
            {
                _connectSaeaPool.Add(socketAsyncEventArgs);
            }
        }

        internal void Receive(Buffer buffer, Action<int, bool> callback)
        {
            var recvAsyncEventArgs = GetSendRecvSocketAsyncEventArgs();
            recvAsyncEventArgs.UserToken = callback;
            recvAsyncEventArgs.BufferList = buffer.ToArraySegmentList();
            var async = _socket.ReceiveAsync(recvAsyncEventArgs);

            if(!async)
            {
                SendRecvCompleted(null, recvAsyncEventArgs);
            }
        }

        internal void Send(Buffer buffer, Action<int, bool> callback)
        {
            var sendAsyncEventArgs = GetSendRecvSocketAsyncEventArgs();
            sendAsyncEventArgs.UserToken = callback;
            sendAsyncEventArgs.BufferList = buffer.ToArraySegmentList();
            var async = _socket.SendAsync(sendAsyncEventArgs);

            if (!async)
            {
                SendRecvCompleted(null, sendAsyncEventArgs);
            }
        }

        internal void Connect(Action<bool> callback)
        {
            var connectAsyncEventArgs = GetConnectSocketAsyncEventArgs();
            connectAsyncEventArgs.UserToken = callback;
            connectAsyncEventArgs.RemoteEndPoint = Endpoint;
            var async = _socket.ConnectAsync(connectAsyncEventArgs);
            
            if (!async)
            {
                ConnectCompleted(null, connectAsyncEventArgs);
            }
        }

        internal void Close()
        {
            _socket.Close();
        }
    }
}