//
// - ListenerBase.cs
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
using System.Net.Sockets;
using Open.P2P.Utils;

namespace Open.P2P.Listeners
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
    
}