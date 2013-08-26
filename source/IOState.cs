//
// - ReceiveJobState.cs
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
using System.Collections.Generic;
using Peer2Net.Progress;

namespace Peer2Net
{
    internal class IOState
    {
        private static readonly Queue<IOState> Pool = new Queue<IOState>();

        private IConnection _connection;
        private IBandwidthController _bandwidthController;
        private int _bytes;
        private BufferManager.Buffer _buffer;
        private SuccessCallback _onSuccess;
        private FailureCallback _onFailure;
        private int _pendingBytes;


        private IOState()
        {
        }

        public IConnection Connection
        {
            get { return _connection; }
        }

        public IBandwidthController BandwidthController
        {
            get { return _bandwidthController; }
        }

        public int Bytes
        {
            get { return _bytes; }
        }

        public int PendingBytes
        {
            get { return _pendingBytes; }
            set { _pendingBytes = value;  }
        }

        public BufferManager.Buffer GetBufferForPending()
        {
            return new BufferManager.Buffer(new ArraySegment<byte>(
                _buffer.Segment.Array,
                _buffer.Segment.Offset + (_buffer.Segment.Count - _pendingBytes ),
                _pendingBytes));
        }

        public byte[] GetData()
        {
            var data = new byte[_buffer.Size];
            _buffer.CopyTo(data);
            return data;
        }

        internal static IOState Create(BufferManager.Buffer buffer, int bytes, IConnection connection, IBandwidthController bandwidthController, SuccessCallback onSuccess, FailureCallback onFailure)
        {
            var state = Pool.Count > 0 ? Pool.Dequeue() : new IOState();

            state._buffer = buffer;
            state._bytes = bytes;
            state._pendingBytes = state._bytes;
            state._connection = connection;
            state._bandwidthController = bandwidthController;
            state._onSuccess = onSuccess;
            state._onFailure = onFailure;
            return state;
        }

        public Action<byte[]> SuccessCallback
        {
            get
            {
                return data=> _onSuccess(_connection, data);
            }
        }

        public Action FailureCallback
        {
            get
            {
                return () => _onFailure(_connection);
            }
        }

        public BufferManager.Buffer Buffer
        {
            get { return _buffer; }
            set { _buffer = value; }
        }

        public bool WaitingForBuffer
        {
            get { return _buffer == null; }
        }

        public void Release()
        {
           Pool.Enqueue(this);
        }
    }
}