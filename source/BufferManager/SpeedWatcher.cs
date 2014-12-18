//
// - SpeedWatcher.cs
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

namespace Open.P2P.BufferManager
{
    public class SpeedWatcher
    {
        private int _transmitedBytes;
        private DateTime _sampledTime;
        private double _speed;
        private TimeSpan _deltaTime;
        private readonly object _syncObject = new object();

        internal SpeedWatcher()
        {
            _sampledTime = DateTime.UtcNow;
            _transmitedBytes = 0;
        }

        public double BytesPerSecond
        {
            get { return _speed;  }
        }

        public TimeSpan MeasuredDeltaTime
        {
            get { return _deltaTime; }
        }

        internal void AddBytes(int byteCount)
        {
            lock (_syncObject)
            {
                _transmitedBytes += byteCount;
            }
        }

        internal void CalculateAndReset()
        {
            lock (_syncObject)
            {
                var now = DateTime.UtcNow;
                _deltaTime = now - _sampledTime;
                _speed = _transmitedBytes / (_deltaTime.TotalMilliseconds / 1000.0);

                _transmitedBytes = 0;
                _sampledTime = now;
            }
        }
    }
}