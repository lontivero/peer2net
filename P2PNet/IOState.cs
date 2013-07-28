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

using P2PNet.Progress;

namespace P2PNet
{
    internal class IOState
    {
        private readonly Connection _connection;
        private readonly BandwidthController _bandwidthController;
        private readonly int _bytes;

        private IOState(int bytes, Connection connection, BandwidthController bandwidthController)
        {
            _bytes = bytes;
            _connection = connection;
            _bandwidthController = bandwidthController;
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

        public static IOState Create(int bytes, Connection connection, BandwidthController bandwidthController)
        {
            return new IOState(bytes, connection, bandwidthController);
        }
    }
}