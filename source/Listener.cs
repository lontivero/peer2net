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
using Peer2Net.EventArgs;
using Peer2Net.Utils;

namespace Peer2Net
{
    public enum ListenerStatus
    {
        Listening,
        Stopped
    }

    public class Listener
    {
        private static readonly BlockingPool<SocketAsyncEventArgs> ConnectSaeaPool =
            new BlockingPool<SocketAsyncEventArgs>(() =>
            {
                var e = new SocketAsyncEventArgs();
                return e;
            });

        private readonly IPEndPoint _endpoint;
        private Socket _listener;
        private int _port;
        private ListenerStatus _status;

        internal event EventHandler<NewConnectionEventArgs> ConnectionRequested;

        public Listener(int port)
        {
            _port = port;
            _endpoint = new IPEndPoint(IPAddress.Any, port);
            _status = ListenerStatus.Stopped;
        }

        public ListenerStatus Status
        {
            get { return _status; }
        }

        public EndPoint Endpoint
        {
            get { return _endpoint; }
        }

        public int Port
        {
            get { return _port; }
        }

        public void Start()
        {
            try
            {
                _listener = new Socket(_endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _listener.SetIPProtectionLevel(IPProtectionLevel.Unrestricted);
                _listener.Bind(_endpoint);
                _listener.Listen(4);
                _status = ListenerStatus.Listening;

                ListenForConnections();
            }
            catch (SocketException)
            {
                if (_listener == null) return;
                Stop();
                throw;
            }
        }

        private void ListenForConnections()
        {
            var saea = ConnectSaeaPool.Take();
            saea.AcceptSocket = null;
            saea.Completed += ConnectCompleted;
            if(_status == ListenerStatus.Stopped) return;

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
                    RaiseConnectionRequestedEvent(new NewConnectionEventArgs(saea.AcceptSocket));
                }
            }
            finally
            {
                saea.Completed -= ConnectCompleted;
                ConnectSaeaPool.Add(saea);
                if(_listener!=null) ListenForConnections();
            }
        }

        public void Stop()
        {
            _status = ListenerStatus.Stopped;
            if (_listener != null)
            {
                _listener.Close();
                _listener = null;
            }
        }

        private void RaiseConnectionRequestedEvent(NewConnectionEventArgs args)
        {
            Events.RaiseAsync(ConnectionRequested, this, args);
        }
    }
}