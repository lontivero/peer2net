//
// - ConnectionIOManager.cs
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
using P2PNet.BufferManager;
using P2PNet.Progress;
using P2PNet.Workers;

namespace P2PNet
{
    internal class ConnectionIoActor
    {
        private readonly IWorkScheduler _worker;
        private readonly BlockingQueue<IOState> _sendQueue;
        private readonly BlockingQueue<IOState> _receiveQueue;
        private readonly BufferAllocator _bufferAllocator;

        public ConnectionIoActor(IWorkScheduler worker)
        {
            _sendQueue = new BlockingQueue<IOState>();
            _receiveQueue = new BlockingQueue<IOState>();

            _bufferAllocator = new BufferAllocator(new byte[1 << 16]);
            _worker = worker;
          //  _worker.Queue(SendEnqueued, TimeSpan.FromMilliseconds(100));
            _worker.Queue(ReceiveEnqueued, TimeSpan.FromMilliseconds(100));
        }

        private void ReceiveEnqueued()
        {
            var c = _receiveQueue.Count;
            for(var i = 0; i < c; i++)
            {
                Receive(_receiveQueue.Take());
            }

            var d = _sendQueue.Count;
            for (var i = 0; i < d; i++)
            {
                Send(_sendQueue.Take());
            }
        }

        //private void SendEnqueued()
        //{
        //    foreach (var ioState in _sendQueue)
        //    {
        //        Send(ioState);
        //    }
        //}

        private void Send(IOState state)
        {
            if (!state.BandwidthController.CanTransmit(state.PendingBytes))
            {
                _sendQueue.Add(state);
                return;
            }

            state.Connection.Send(state.Buffer, sentCount =>
                {
                    try
                    {
                        if (sentCount == 0)
                        {
                            state.Connection.Close();
                            return;
                        }
                        if (sentCount < state.PendingBytes)
                        {
                            state.PendingBytes -= sentCount;
                            _sendQueue.Add(state);
                        }
                        else
                        {
                            var data = state.GetData();
                            state.Callback(data);
                        }
                    }
                    finally
                    {
                        state.Release();
                        _bufferAllocator.Free(state.InternalBuffer);
                    }
                });
        }

        private void Receive(IOState state)
        {
            if (!state.BandwidthController.CanTransmit(state.PendingBytes))
            {
                _receiveQueue.Add(state);
                return;
            }

            state.Connection.Receive(state.Buffer, readCount =>
                {
                    try
                    {
                        if (readCount == 0)
                        {
                            state.Connection.Close();
                            return;
                        }
                        if (readCount < state.PendingBytes)
                        {
                            state.PendingBytes -= readCount;
                            _receiveQueue.Add(state);
                        }
                        else
                        {
                            var data = state.GetData();
                            state.Callback(data);
                        }
                    }
                    finally
                    {
                        state.Release();
                        _bufferAllocator.Free(state.InternalBuffer);
                    }
                });
        }

        public void EnqueueSend(byte[] data, Connection connection, BandwidthController bandwidthController,
                                Action<Connection, byte[]> callback)
        {
            var buffer = _bufferAllocator.AllocateAndCopy(data);
            Send(IOState.Create(buffer, connection, bandwidthController, callback));
        }

        public void EnqueueReceive(int bytes, Connection connection, BandwidthController bandwidthController,
                                   Action<Connection, byte[]> callback)
        {
            var buffer = _bufferAllocator.Allocate(bytes);
            Receive(IOState.Create(buffer, connection, bandwidthController, callback));
        }

        public void EnqueueConnect(IPEndPoint endpoint, Action<Connection> callback)
        {
            var connection = new Connection(endpoint);
            Connect(ConnectState.Create(connection, callback));
        }

        private void Connect(ConnectState state)
        {
            state.Connection.Connect(() => state.Callback());
        }
    }
}