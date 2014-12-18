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
using System.Threading;
using System.Threading.Tasks;

namespace Open.P2P.Workers
{
    internal class TimedWorker: IWorkScheduler
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly List<ScheduledAction> _actions = new List<ScheduledAction>();
        private readonly AutoResetEvent _resetEvent = new AutoResetEvent(false);

        public TimedWorker()
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Start()
        {
            Task.Factory.StartNew(() =>
                {
                    ScheduledAction scheduledAction = null;

                    while (!_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        bool any;
                        lock (_actions)
                        {
                            any = _actions.Count > 0;
                            if (any) scheduledAction = _actions[0];
                        }

                        TimeSpan timeToWait;
                        if (any)
                        {
                            DateTime runTime = scheduledAction.NextExecutionDate;
                            var dT = runTime - DateTime.UtcNow;
                            timeToWait = dT > TimeSpan.Zero ? dT : TimeSpan.Zero;
                        }
                        else
                        {
                            timeToWait = TimeSpan.FromMilliseconds(-1);
                        }

                        if (_resetEvent.WaitOne(timeToWait, false)) continue;

                        Debug.Assert(scheduledAction != null, "scheduledAction != null");
                        scheduledAction.Execute();
                        lock (_actions)
                        {
                            Remove(scheduledAction);
                            if (scheduledAction.Repeat)
                            {
                                QueueForever(scheduledAction.Action, scheduledAction.Interval);
                            }
                        }
                    }
                }, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private void Remove(ScheduledAction scheduledAction)
        {
            lock (_actions)
            {
                var pos = _actions.BinarySearch(scheduledAction);
                _actions.RemoveAt(pos);
                scheduledAction.Release();
                if(pos==0)
                {
                    _resetEvent.Set();
                }
            }
        }

        public void QueueForever(Action action, TimeSpan interval)
        {
            QueueInternal(ScheduledAction.Create(action, interval, true));
        }

        public void QueueOneTime(Action action, TimeSpan interval)
        {
            QueueInternal(ScheduledAction.Create(action, interval, false));
        }

        private void QueueInternal(ScheduledAction scheduledAction)
        {
            lock (_actions)
            {
                var pos = _actions.BinarySearch(scheduledAction);
                pos = pos >= 0 ? pos : ~pos;
                _actions.Insert(pos, scheduledAction);

                if (pos == 0)
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