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
using P2PNet.Progress;
using Buffer = P2PNet.BufferManager.Buffer;

namespace P2PNet
{
    internal class IOState
    {
        private static readonly Queue<IOState> _pool = new Queue<IOState>();

        private Connection _connection;
        private BandwidthController _bandwidthController;
        private int _bytes;
        private Buffer _buffer;
        private Action<Connection, byte[]> _onSuccess;
        private Action<Connection> _onFailure;
        private int _pendingBytes;


        private IOState()
        {
        }

        public Connection Connection
        {
            get { return _connection; }
        }

        public BandwidthController BandwidthController
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

        public Buffer Buffer
        {
            get
            {
                return new Buffer(new ArraySegment<byte>(
                    _buffer.Segment.Array,
                    _buffer.Segment.Offset + (_buffer.Segment.Count - _pendingBytes ),
                    _pendingBytes));
            }
        }

        public byte[] GetData()
        {
            var data = new byte[_buffer.Size];
            _buffer.CopyTo(data);
            return data;
        }

        public static IOState Create(Buffer buffer, Connection connection, BandwidthController bandwidthController, Action<Connection, byte[]> onSuccess, Action<Connection> onFailure)
        {
            var state = _pool.Count > 0 ? _pool.Dequeue() : new IOState();

            state._buffer = buffer;
            state._bytes = buffer.Size;
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

        public Buffer InternalBuffer
        {
            get { return _buffer; }
        }

        public void Release()
        {
           _pool.Enqueue(this);
        }
    }
}