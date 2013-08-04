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
            _worker.Queue(SendEnqueued, TimeSpan.FromMilliseconds(10));
            _worker.Queue(ReceiveEnqueued, TimeSpan.FromMilliseconds(9));
        }

        private void ReceiveEnqueued()
        {
            var c = _receiveQueue.Count;
            for (var i = 0; i < c; i++)
            {
                Receive(_receiveQueue.Take());
            }
        }

        private void SendEnqueued(){
            var d = _sendQueue.Count;
            for (var i = 0; i < d; i++)
            {
                Send(_sendQueue.Take());
            }
        }

        private void Send(IOState state)
        {
            if (!state.BandwidthController.CanTransmit(state.PendingBytes))
            {
                _sendQueue.Add(state);
                return;
            }

            state.Connection.Send(state.Buffer, (sentCount, success) =>
                {
                    try
                    {
                        if(success && sentCount > 0)
                        {
                            if (sentCount < state.PendingBytes)
                            {
                                state.PendingBytes -= sentCount;
                                _sendQueue.Add(state);
                            }
                            else
                            {
                                var data = state.GetData();
                                state.SuccessCallback(data);
                            }
                        }
                        else
                        {
                            state.FailureCallback();
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

            state.Connection.Receive(state.Buffer, (readCount, success) =>
                {
                    try
                    {
                        if (success && readCount > 0 )
                        {
                            if (readCount < state.PendingBytes)
                            {
                                state.PendingBytes -= readCount;
                                _receiveQueue.Add(state);
                            }
                            else
                            {
                                var data = state.GetData();
                                state.SuccessCallback(data);
                            }
                        }
                        else
                        {
                            state.FailureCallback();
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
                                Action<Connection, byte[]> onSuccess, Action<Connection> onFailure)
        {
            var buffer = _bufferAllocator.AllocateAndCopy(data);
            Send(IOState.Create(buffer, connection, bandwidthController, onSuccess, onFailure));
        }

        public void EnqueueReceive(int bytes, Connection connection, BandwidthController bandwidthController,
                                   Action<Connection, byte[]> onSuccess, Action<Connection> onFailure)
        {
            var buffer = _bufferAllocator.Allocate(bytes);
            Receive(IOState.Create(buffer, connection, bandwidthController, onSuccess, onFailure));
        }

        public void EnqueueConnect(Connection connection, Action<Connection> onSuccess, Action<Connection> onFailure )
        {
            Connect(ConnectState.Create(connection, onSuccess, onFailure));
        }

        private void Connect(ConnectState state)
        {
            state.Connection.Connect(success =>
                {
                    if(success)
                    {
                        state.SuccessCallback();
                    }
                    else
                    {
                        state.FailureCallback();
                    }
                });
        }
    }
}