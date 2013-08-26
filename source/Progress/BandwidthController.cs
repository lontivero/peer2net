//
// - BandwidthController.cs
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
using Peer2Net.Utils;

namespace Peer2Net.Progress
{
    internal class BandwidthController : IBandwidthController
    {
        private readonly PidController _pidController;
        private int _setpoint;
        private int _accumulatedBytes;

        internal BandwidthController()
        {
            _pidController = new PidController();
            _setpoint = Int32.MaxValue;
            _accumulatedBytes = Int32.MaxValue;
        }

        public int TargetSpeed
        {
            get { return _setpoint; }
            set
            {
                Guard.IsBeetwen(value, 0, int.MaxValue, "value");
                _setpoint = value;
                _accumulatedBytes = _setpoint;
            }
        }

        public bool CanTransmit(int bytesCount)
        {
            if (bytesCount > 0 && _accumulatedBytes < bytesCount) return false;

            _accumulatedBytes -= bytesCount;
            return true;
        }

        public void Update(double measuredSpeed, TimeSpan deltaTime)
        {
            var seconds = deltaTime.TotalMilliseconds / 1000.0;
            var deltaSpeed = _setpoint - measuredSpeed/seconds;

            var correction = _pidController.Control(deltaSpeed, seconds);
            _accumulatedBytes += (int)(_setpoint + correction);
        }
    }
}
