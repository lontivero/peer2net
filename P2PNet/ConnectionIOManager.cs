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
using System.Collections.Concurrent;
using P2PNet.Progress;
using P2PNet.Workers;

namespace P2PNet
{
    internal class ConnectionIoActor
    {
        private readonly TimedWorker _worker;
        private readonly BlockingCollection<IOState> _sendQueue;
        private readonly BlockingCollection<IOState> _receiveQueue;

        public ConnectionIoActor(TimedWorker timedWorker)
        {
            _sendQueue = new BlockingCollection<IOState>();
            _receiveQueue = new BlockingCollection<IOState>();
            _worker = timedWorker;
            _worker.Queue(TimeSpan.FromMilliseconds(100), SendEnqueued);
            _worker.Queue(TimeSpan.FromMilliseconds(100), ReceiveEnqueued);
        }

        private void ReceiveEnqueued()
        {
            foreach (var ioState in _receiveQueue.GetConsumingEnumerable())
            {
                Receive(ioState);
            }
        }

        private void SendEnqueued()
        {
            foreach (var ioState in _sendQueue.GetConsumingEnumerable())
            {
                Send(ioState);
            }
        }

        private void Send(IOState state)
        {
            if (!state.Connection.TryReceive(state.Bytes, state.BandwidthController))
            {
                _sendQueue.Add(state);
            }
        }

        private void Receive(IOState state)
        {
            if (!state.Connection.TryReceive(state.Bytes, state.BandwidthController))
            {
                _receiveQueue.Add(state);
            }
        }

        public void EnqueueSend(int bytes, Connection connection, BandwidthController bandwidthController)
        {
            Send(IOState.Create(bytes, connection, bandwidthController));
        }

        public void EnqueueReceive(int bytes, Connection connection, BandwidthController bandwidthController)
        {
            Receive(IOState.Create(bytes, connection, bandwidthController));
        }
    }
}