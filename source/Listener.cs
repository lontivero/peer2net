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
using System.Text;
using Peer2Net.EventArgs;
using Peer2Net.Utils;

namespace Peer2Net
{
    public enum ListenerStatus
    {
        Listening,
        Stopped
    }

    public abstract class ListenerBase
    {
        private static readonly BlockingPool<SocketAsyncEventArgs> ConnectSaeaPool =
            new BlockingPool<SocketAsyncEventArgs>(() =>
                {
                    var e = new SocketAsyncEventArgs();
                    return e;
                });

        protected IPEndPoint EndPoint;
        protected Socket Listener;
        private readonly int _port;
        private ListenerStatus _status;

        protected ListenerBase(int port)
        {
            _port = port;
            EndPoint = new IPEndPoint(IPAddress.Any, port);
            _status = ListenerStatus.Stopped;
        }

        public ListenerStatus Status
        {
            get { return _status; }
        }

        public EndPoint Endpoint
        {
            get { return EndPoint; }
        }

        public int Port
        {
            get { return _port; }
        }

        public void Start()
        {
            try
            {
                Listener = CreateSocket();
                _status = ListenerStatus.Listening;

                Listen();
            }
            catch (SocketException)
            {
                if (Listener == null) return;
                Stop();
                throw;
            }
        }

        protected abstract Socket CreateSocket();
        protected abstract void Notify(SocketAsyncEventArgs saea);
        protected abstract bool ListenAsync(SocketAsyncEventArgs saea);

        private void Listen()
        {
            var saea = ConnectSaeaPool.Take();
            saea.AcceptSocket = null;
            saea.Completed += IOCompleted;
            if(_status == ListenerStatus.Stopped) return;

            var async = ListenAsync(saea);

            if (!async)
            {
                IOCompleted(null, saea);
            }
        }

        private void IOCompleted(object sender, SocketAsyncEventArgs saea)
        {
            try
            {
                if (saea.SocketError == SocketError.Success)
                {
                    Notify(saea);
                }
            }
            finally
            {
                saea.Completed -= IOCompleted;
                ConnectSaeaPool.Add(saea);
                if(Listener!=null) Listen();
            }
        }

        public void Stop()
        {
            _status = ListenerStatus.Stopped;
            if (Listener != null)
            {
                Listener.Close();
                Listener = null;
            }
        }
    }

    public class Listener : ListenerBase
    {
        internal event EventHandler<NewConnectionEventArgs> ConnectionRequested;

        public Listener(int port) : base(port)
        {
        }

        protected override Socket CreateSocket()
        {
            var socket = new Socket(EndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.SetIPProtectionLevel(IPProtectionLevel.Unrestricted);
            socket.Bind(EndPoint);
            socket.Listen(4);
            return socket;
        }

        protected override bool ListenAsync(SocketAsyncEventArgs saea)
        {
            return Listener.AcceptAsync(saea);
        }

        protected override void Notify(SocketAsyncEventArgs saea)
        {
            Events.RaiseAsync(ConnectionRequested, this, new NewConnectionEventArgs(saea.AcceptSocket));
        }
    }

    public class UdpListener : ListenerBase
    {
        public event EventHandler<NewDiscoveredNodeEventArgs> DiscoveredNode;
        private readonly Guid _id = Guid.NewGuid();

        public UdpListener(int port)
            : base(port)
        {
        }

        protected override Socket CreateSocket()
        {
            var socket = new Socket(EndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(EndPoint);
            return socket;
        }

        public void SayHello(int port)
        {
            var socket = new Socket(EndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            socket.EnableBroadcast = true;
            var group = new IPEndPoint(IPAddress.Broadcast, Port);
            var hi = Encoding.ASCII.GetBytes("Hi Peer2Net node here:" + _id + ":127.0.0.1:" + port);
            socket.SendTo(hi, group);
            socket.Close();             
        }

        protected override bool ListenAsync(SocketAsyncEventArgs saea)
        {
            var bufferSize = ("Hi Peer2Net node here:" + Guid.Empty + ":127.0.0.1:0000").Length;
            saea.SetBuffer(new byte[bufferSize], 0, bufferSize);
            saea.RemoteEndPoint = new IPEndPoint(IPAddress.Any, Port);
            return Listener.ReceiveFromAsync(saea);
        }

        protected override void Notify(SocketAsyncEventArgs saea)
        {
            var message = Encoding.ASCII.GetString(saea.Buffer);
            if(message.StartsWith("Hi Peer2Net node here"))
            {
                var parts = message.Split(':');
                if(Guid.Parse(parts[1]) != _id)
                {
                    var endpoint = new IPEndPoint(IPAddress.Parse(parts[2]), int.Parse(parts[3]));
                    Events.RaiseAsync(DiscoveredNode, this, new NewDiscoveredNodeEventArgs(endpoint));
                }
            }
        }
    }
}