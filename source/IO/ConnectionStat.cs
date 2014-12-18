//
// - ConnectionStat.cs
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

namespace Open.P2P.IO
{
    public class PeerStat
    {
        private readonly DateTime _connectionDate;

        public PeerStat()
        {
            _connectionDate = DateTime.UtcNow;
        }

        private double SecondsSinceConnected
        {
            get{ return 1000.0 / (DateTime.UtcNow - _connectionDate).TotalMilliseconds; }
        }

        public long ReceivedByteCount { get; private set; }
        public long SentByteCount { get; private set; }

        public DateTime LastTimeSent { get; private set; }
        public DateTime LastTimeReceived { get; private set; }

        public long AverageReceiveSpeed
        {
            get { return (long) (ReceivedByteCount/SecondsSinceConnected); }
        }

        public long AverageSendSpeed
        {
            get { return (long)(SentByteCount / SecondsSinceConnected); }
        }

        public DateTime ConnectionDate
        {
            get { return _connectionDate; }
        }

        public TimeSpan ConnectedTime
        {
            get { return DateTime.UtcNow - _connectionDate; }
        }

        internal void AddReceivedBytes(int byteCount)
        {
            ReceivedByteCount = ReceivedByteCount + byteCount;
            LastTimeReceived = DateTime.UtcNow;
        }

        internal void AddSentBytes(int byteCount)
        {
            SentByteCount = ReceivedByteCount + byteCount;
            LastTimeSent = DateTime.UtcNow;
        }
    }
}