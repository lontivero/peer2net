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
using System.Net.Sockets;
using System.Threading.Tasks;
using P2PNet.EventArgs;
using P2PNet.Utils;
using Buffer = P2PNet.BufferManager.Buffer;

namespace P2PNet
{
    public class Connection
    {
        private readonly Socket _socket;

        public Connection(Guid uid, Socket socket, Buffer buffer)
        {
            Uid = uid;
            _socket = socket;
            Buffer = buffer;
        }

        public Guid Uid { get; private set; }

        internal Buffer Buffer { get; private set; }

        public event EventHandler<DataArrivedEventArgs> DataArrived;
        public event EventHandler<System.EventArgs> ClosedEvent;

        public void Close()
        {
            _socket.Close();
        }

        internal void Receive()
        {
            Func<AsyncCallback, object, IAsyncResult> beginReceive =
                (callback, s) => _socket.BeginReceive(Buffer, SocketFlags.None, callback, s);

            //Task<int> task = Task.Factory.FromAsync<int>(begin, _stream.EndRead, null);
            //task.ContinueWith(t => callback(t.Result), TaskContinuationOptions.NotOnFaulted)
            //    .ContinueWith(t => error(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
            //return task;

            Task.Factory.FromAsync<int>(beginReceive, _socket.EndReceive, this)
                .ContinueWith(task =>
                    {
                        int bytesRead = task.Result;

                        if (bytesRead > 0)
                        {
                            var data = new byte[bytesRead];
                            Buffer.CopyTo(data);
                            RaiseDataArrivedEvent(new DataArrivedEventArgs(Uid, data));
                        }
                        else
                        {
                            Close();
                        }
                    });
        }

        private void RaiseDataArrivedEvent(DataArrivedEventArgs args)
        {
            Events.Raise(DataArrived, this, args);
        }

        private void RaiseClientClosedEvent()
        {
            Events.Raise(ClosedEvent, this, null);
        }
    }
}