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
using P2PNet.BufferManager;
using P2PNet.EventArgs;
using P2PNet.Progress;
using P2PNet.Utils;

namespace P2PNet
{
    public class Connection
    {
        private readonly Socket _socket;
        private readonly IBufferAllocator _allocator;

        public Connection(Guid uid, Socket socket, IBufferAllocator allocator)
        {
            Uid = uid;
            _socket = socket;
            _allocator = allocator;
        }

        public Guid Uid { get; private set; }

        public event EventHandler<DataArrivedEventArgs> DataArrived;
        public event EventHandler<System.EventArgs> ClosedEvent;

        public void Close()
        {
            _socket.Close();
        }

        internal void Receive(int byteCount)
        {
            var buffer = _allocator.Allocate(byteCount);
            Func<AsyncCallback, object, IAsyncResult> beginReceive =
                (callback, s) => _socket.BeginReceive(buffer, SocketFlags.None, callback, s);

            Task.Factory.FromAsync<int>(beginReceive, _socket.EndReceive, this)
                .ContinueWith(task =>
                    {
                        int bytesRead = task.Result;

                        if (bytesRead > 0)
                        {
                            var data = new byte[bytesRead];
                            buffer.CopyTo(data);
                            RaiseDataArrivedEvent(new DataArrivedEventArgs(Uid, data));
                        }
                        else
                        {
                            Close();
                        }
                        _allocator.Free(buffer);
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

        internal bool TryReceive(int byteCount, BandwidthController bandwidthController)
        {
            if (!bandwidthController.CanTransmit(byteCount)) return false;
            Receive(byteCount);
            return true;
        }

        internal bool TrySend(int byteCount, BandwidthController bandwidthController)
        {
            if (!bandwidthController.CanTransmit(byteCount)) return false;
            Receive(byteCount);
            return true;
        }
    }
}