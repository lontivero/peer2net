//
// - ClientWorker.cs
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

namespace Open.P2P.Workers
{
    class ClientWorker : IWorker, IWorkScheduler
    {
        private readonly BackgroundWorker _backgroundWorker;
        private readonly TimedWorker _timedWorker;

        public ClientWorker()
        {
            _backgroundWorker = new BackgroundWorker();
            _timedWorker = new TimedWorker();
        }

        public void Queue(Action action)
        {
            _backgroundWorker.Queue(action);
        }

        public void QueueForever(Action action, TimeSpan interval)
        {
            _timedWorker.QueueForever(() => _backgroundWorker.Queue(action), interval);
        }

        public void QueueOneTime(Action action, TimeSpan interval)
        {
            _timedWorker.QueueOneTime(() => _backgroundWorker.Queue(action), interval);
        }

        public void Start()
        {
            _backgroundWorker.Start();
            _timedWorker.Start();
        }

        public void Stop()
        {
            _timedWorker.Stop();
            _backgroundWorker.Stop();
        }
    }
}
