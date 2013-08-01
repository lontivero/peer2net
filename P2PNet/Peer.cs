//
// - ConnectionBundle.cs
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
using P2PNet.BufferManager;
using P2PNet.Progress;

namespace P2PNet
{
    public class Peer
    {
        private readonly Guid _peerId;
        private readonly Connection _connection;
        private readonly PeerStat _statistics;
        private readonly BandwidthController _receiveBandwidthController;
        private readonly BandwidthController _sendBandwidthController;
        private readonly SpeedWatcher _sendSpeedWatcher;
        private readonly SpeedWatcher _receiveSpeedWatcher;

        public Guid Id
        {
            get { return _peerId; }
        }

        public Connection Connection
        {
            get { return _connection; }
        }

        public PeerStat Statistics
        {
            get { return _statistics; }
        }

        public BandwidthController ReceiveBandwidthController
        {
            get { return _receiveBandwidthController; }
        }

        public BandwidthController SendBandwidthController
        {
            get { return _sendBandwidthController; }
        }

        public SpeedWatcher SendSpeedWatcher
        {
            get { return _sendSpeedWatcher; }
        }

        public SpeedWatcher ReceiveSpeedWatcher
        {
            get { return _receiveSpeedWatcher; }
        }

        public Peer(Connection connection)
        {
            _peerId = connection.Id;
            _connection = connection;
            _sendSpeedWatcher = new SpeedWatcher();
            _receiveSpeedWatcher = new SpeedWatcher();
            _receiveBandwidthController = new BandwidthController();
            _sendBandwidthController = new BandwidthController();
            _statistics = new PeerStat();
        }
    }
}