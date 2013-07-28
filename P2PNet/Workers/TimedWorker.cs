//
// - TimedWorker.cs
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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace P2PNet.Workers
{
    internal class TimedWorker
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly SortedList<DateTime, ScheduledAction> _actions = new SortedList<DateTime, ScheduledAction>();
        private readonly AutoResetEvent _resetEvent = new AutoResetEvent(false);


        public TimedWorker()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            Start();
        }

        public void Start()
        {
            Task.Factory.StartNew(() =>
                {
                    var kvp = new KeyValuePair<DateTime, ScheduledAction>();
                    ScheduledAction scheduledAction = null;
                    var runTime = new DateTime();

                    while (!_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        bool any;
                        lock (_actions)
                        {
                            any = _actions.Any();
                            if (any) kvp = _actions.First();
                        }

                        TimeSpan timeToWait;
                        if (any)
                        {
                            scheduledAction = kvp.Value;
                            runTime = kvp.Key;
                            var dT = runTime - DateTime.UtcNow;
                            timeToWait = dT > TimeSpan.Zero ? dT : TimeSpan.Zero;
                        }
                        else
                        {
                            timeToWait = TimeSpan.FromMilliseconds(-1);
                        }

                        if (!_resetEvent.WaitOne(timeToWait, false))
                        {
                            Debug.Assert(scheduledAction != null, "scheduledAction != null");
                            scheduledAction.Execute();
                            lock (_actions)
                            {
                                _actions.Remove(runTime);
                                _actions.Add(DateTime.UtcNow.Add(kvp.Value.Interval), scheduledAction);
                            }
                        }
                    }
                }, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public void Queue(TimeSpan interval, Action action)
        {
            var scheduledAction = new ScheduledAction { Action = action, Interval = interval };
            lock (_actions)
            {
                _actions.Add(DateTime.UtcNow.Add(interval), scheduledAction);
                if (_actions.First().Value.Action == action)
                {
                    _resetEvent.Set();
                }
            }
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}