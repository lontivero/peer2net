//
// - PIController.cs
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

namespace Open.P2P.Progress
{
    internal class PidController
    {
        private readonly double _kp = 0.5f;  // Proportional Gain
        private readonly double _ki = 0.5f;  // Integral Gain
        private readonly double _kd = 0.5f;  // Derivative Gain

        private double _prevError;
        private double _integral;
        private readonly object _syncObject = new object();


        public PidController()
            : this(0.3, 0.75, 0.0) // 0.3 0.75
        {
        }

        public PidController(double kp, double ki)
            : this(kp, ki, 0.0)
        {
        }

        public PidController(double kp, double ki, double kd)
        {
            _kp = kp;
            _ki = ki;
            _kd = kd;
        }

        public double Control(double error, double dt)
        {
            lock(_syncObject)
            {
                var pintegral = _integral + (error*dt);
                _integral = pintegral < -30 ? -30 : pintegral;
                _integral = pintegral >  30 ?  30 : pintegral;
                var derivative = (error - _prevError) / dt;
                var pid = _kp * error + _ki * _integral + _kd * derivative;
                _prevError = error;
                return pid;
            }
        }
    }
}
