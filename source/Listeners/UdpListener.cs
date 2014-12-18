//
// - UdpListener.cs
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
using Open.P2P.EventArgs;
using Open.P2P.Utils;

namespace Open.P2P.Listeners
{
    public class UdpListener : ListenerBase
    {
        /// <summary>
        /// Occurs when discovered node.
        /// </summary>
        public event EventHandler<UdpPacketReceivedEventArgs> UdpPacketReceived;

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpListener"/> class.
        /// </summary>
        /// <param name='port'>
        /// Port.
        /// </param>
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

        protected override bool ListenAsync(SocketAsyncEventArgs saea)
        {
            var bufferSize = ("Hi Peer2Net node here:" + Guid.Empty + ":127.0.0.1:0000").Length;
            saea.SetBuffer(new byte[bufferSize], 0, bufferSize);
            saea.RemoteEndPoint = new IPEndPoint(IPAddress.Any, Port);
            return Listener.ReceiveFromAsync(saea);
        }

        protected override void Notify(SocketAsyncEventArgs saea)
        {
			var endPoint = saea.RemoteEndPoint as IPEndPoint;
            Events.Raise(UdpPacketReceived, this, new UdpPacketReceivedEventArgs(endPoint, saea.Buffer));
        }
    }
}