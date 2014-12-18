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
using System.IO;
using System.Net;
using Open.P2P.BufferManager;
using Open.P2P.IO;
using Open.P2P.Progress;
using Open.P2P.Streams;

namespace Open.P2P
{
    public class Peer
    {
        internal IConnection Connection { get; private set; }

        internal IBandwidthController ReceiveBandwidthController { get; private set; }
        internal IBandwidthController SendBandwidthController { get; private set; }

        public Uri Uri { get; private set; }
        public PeerStat Statistics { get; private set; }

        public SpeedWatcher SendSpeedWatcher { get; private set; }
        public SpeedWatcher ReceiveSpeedWatcher { get; private set; }

        public Stream Stream { get; private set; }

        public IPEndPoint EndPoint
        {
            get { return Connection.Endpoint; }
        }

        public long UploadSpeed
        {
            set; 
            get;
        }

        public long DownloadSpeed
        {
            set;
            get;
        }

        internal Peer(Stream stream, IConnection connection)
        {
            Connection = connection;
            SendSpeedWatcher = new SpeedWatcher();
            ReceiveSpeedWatcher = new SpeedWatcher();
            SendBandwidthController = new UnlimitedBandwidthController();
            ReceiveBandwidthController = new UnlimitedBandwidthController();
            Statistics = new PeerStat();
            Stream = new ThrottledStream(stream, ReceiveBandwidthController, SendBandwidthController);

            Uri = new Uri("tcp://" + EndPoint.Address + ':' + EndPoint.Port);
        }

        internal void Disconnect()
        {
            Stream.Close();
        }
    }
}