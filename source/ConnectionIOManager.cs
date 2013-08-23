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

using Peer2Net.BufferManager;
using Peer2Net.Progress;
using Peer2Net.Utils;
using Peer2Net.Workers;

namespace Peer2Net
{
    internal delegate void ConnectCallback(IConnection connection);
    internal delegate void FailureCallback(IConnection connection);
    internal delegate void SuccessCallback(IConnection connection, byte[] data);

    internal class ConnectionIoActor
    {
        private readonly IWorkScheduler _worker;
        private readonly BlockingQueue<IOState> _sendQueue;
        private readonly BlockingQueue<IOState> _receiveQueue;
        private readonly IBufferAllocator _bufferAllocator;

        public ConnectionIoActor(IWorkScheduler worker, IBufferAllocator bufferAllocator)
        {
            _sendQueue = new BlockingQueue<IOState>();
            _receiveQueue = new BlockingQueue<IOState>();

            _bufferAllocator = bufferAllocator;
            _worker = worker;
            _worker.QueueForever(SendEnqueued, 1.Milliseconds());
            _worker.QueueForever(ReceiveEnqueued, 1.Milliseconds());
        }

        public void Connect(IConnection connection, FailureCallback onSuccess, FailureCallback onFailure)
        {
            ConnectInternal(ConnectState.Create(connection, onSuccess, onFailure));
        }

        public void Send(byte[] data, IConnection connection, BandwidthController bandwidthController,
                                SuccessCallback onSuccess, FailureCallback onFailure)
        {
            var buffer = new Buffer(data);
            SendInternal(IOState.Create(buffer, buffer.Size, connection, bandwidthController, onSuccess, onFailure));
        }

        public void Receive(int bytes, IConnection connection, BandwidthController bandwidthController,
                                   SuccessCallback onSuccess, FailureCallback onFailure)
        {
            ReceiveInternal(IOState.Create(null, bytes, connection, bandwidthController, onSuccess, onFailure));
        }

        private void ReceiveEnqueued()
        {
            var c = _receiveQueue.Count;
            for (var i = 0; i < c; i++)
            {
                ReceiveInternal(_receiveQueue.Take());
            }
        }

        private void SendEnqueued(){
            var d = _sendQueue.Count;
            for (var i = 0; i < d; i++)
            {
                SendInternal(_sendQueue.Take());
            }
        }

        private void ConnectInternal(ConnectState state)
        {
            _worker.QueueOneTime(() =>
            {
                if (!state.Connection.IsConnected)
                {
                    state.Connection.Close();
                }
            }, 2.Seconds());
            state.Connection.Connect(success =>
            {
                if (success)
                {
                    state.SuccessCallback();
                }
                else
                {
                    state.FailureCallback();
                }
            });
        }

        private void SendInternal(IOState state)
        {
            if (!state.BandwidthController.CanTransmit(state.PendingBytes))
            {
                _sendQueue.Add(state);
                return;
            }
            if (state.WaitingForBuffer)
            {
                state.Buffer = _bufferAllocator.Allocate(state.Bytes);
                if (state.WaitingForBuffer)
                {
                    _sendQueue.Add(state);
                    return;
                }
            }

            state.Connection.Send(state.GetBufferForPending(), (sentCount, success) =>
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
                        _bufferAllocator.Free(state.Buffer);
                    }
                });
        }

        private void ReceiveInternal(IOState state)
        {
            if (!state.BandwidthController.CanTransmit(state.PendingBytes))
            {
                _receiveQueue.Add(state);
                return;
            }
            if (state.WaitingForBuffer)
            {
                state.Buffer = _bufferAllocator.Allocate(state.Bytes);
                if(state.WaitingForBuffer)
                {
                    _receiveQueue.Add(state);
                    return;
                }
            }

            state.Connection.Receive(state.GetBufferForPending(), (readCount, success) =>
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
                        _bufferAllocator.Free(state.Buffer);
                    }
                });
        }
    }
}