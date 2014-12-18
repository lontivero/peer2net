//
// - TcpListener.cs
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
using System.Net.Sockets;
using Open.P2P.EventArgs;
using Open.P2P.Utils;

namespace Open.P2P.Listeners
{

    public class TcpListener : ListenerBase
    {
        internal event EventHandler<NewConnectionEventArgs> ConnectionRequested;

        public TcpListener(int port) : base(port)
        {
        }

        protected override Socket CreateSocket()
        {
            var socket = new Socket(EndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
//          socket.SetIPProtectionLevel(IPProtectionLevel.Unrestricted);
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
            Events.Raise(ConnectionRequested, this, new NewConnectionEventArgs(saea.AcceptSocket));
        }
    }
}