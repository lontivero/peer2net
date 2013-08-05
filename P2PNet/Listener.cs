//
// - Listener.cs
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
using P2PNet.EventArgs;
using P2PNet.Utils;

namespace P2PNet
{
    public class Listener
    {
        private static readonly BlockingPool<SocketAsyncEventArgs> ConnectSaeaPool =
            new BlockingPool<SocketAsyncEventArgs>(() =>
            {
                var e = new SocketAsyncEventArgs();
                return e;
            });

        private readonly IPEndPoint _endpoint;
        private readonly Socket _listener;

        internal event EventHandler<ConnectionEventArgs> ConnectionRequested;

        public Listener(int port)
        {
            _endpoint = new IPEndPoint(IPAddress.Any, port);
            _listener = new Socket(_endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Start()
        {
            try
            {
                _listener.SetIPProtectionLevel(IPProtectionLevel.Unrestricted);
                _listener.Bind(_endpoint);
                _listener.Listen(4);

                ListenForConnections();
            }
            catch (SocketException)
            {
                if (_listener == null) return;
                Stop();
            }
        }

        private void ListenForConnections()
        {
            var saea = ConnectSaeaPool.Take();
            saea.Completed += ConnectCompleted;
            var async = _listener.AcceptAsync(saea);

            if (!async)
            {
                ConnectCompleted(null, saea);
            }
        }

        private void ConnectCompleted(object sender, SocketAsyncEventArgs saea)
        {
            try
            {
                if (saea.SocketError == SocketError.Success)
                {
                    RaiseConnectionRequestedEvent(new ConnectionEventArgs(saea.AcceptSocket));
                }
            }
            finally
            {
                saea.AcceptSocket = null;
                ConnectSaeaPool.Add(saea);
                ListenForConnections();
            }
        }

        public void Stop()
        {
            if (_listener != null)
            {
                _listener.Close();
            }
        }

        private void RaiseConnectionRequestedEvent(ConnectionEventArgs args)
        {
            Events.RaiseAsync(ConnectionRequested, this, args);
        }
    }
}