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
using System.Threading;
using System.Threading.Tasks;
using Open.P2P.Utils;

namespace Open.P2P.Progress
{
    internal class BandwidthController : IBandwidthController
    {
        private readonly PidController _pidController;
        private int _setpoint;
        private DateTime _lastTime;
        private double _lastSpeed;

        internal BandwidthController()
        {
            _pidController = new PidController();
            _setpoint = Int32.MaxValue;
            _lastTime = DateTime.UtcNow;
            _lastSpeed = 0;
        }

        public int TargetSpeed
        {
            get { return _setpoint; }
            set
            {
                Guard.IsBeetwen(value, 0, int.MaxValue, "value");
                _setpoint = value;
            }
        }

        public Task WaitToTransmit(int bytesCount)
        {
            var t1 = DateTime.UtcNow;
            var t0 = _lastTime;
            var dt = (t1 - t0).TotalSeconds;
            dt = Math.Abs(dt - 0) < 0.0001 ? 0.0001 : dt; 

            var v1 = TargetSpeed;
            var v0 = _lastSpeed;
            var dv = (v1 - v0)/dt;

            var vn = _pidController.Control(dv, dt);
            var v2 = v1 + vn;

            var wait = (bytesCount / v2) * 1000;
            return Task.Delay((int)wait);
        }

        public void UpdateSpeed(int bytes, TimeSpan deltaTime)
        {

            _lastSpeed = bytes / deltaTime.TotalSeconds;
            _lastTime = DateTime.UtcNow;
        }
    }
}
