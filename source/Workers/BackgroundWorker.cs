//
// - EndlessWorker.cs
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

namespace Open.P2P.Workers
{
    internal class BackgroundWorker : IWorker
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly BlockingQueue<Action> _queue;


        public BackgroundWorker()
        {
            _queue = new BlockingQueue<Action>();
            _cancellationTokenSource = new CancellationTokenSource();

            Start();
        }

        public void Start()
        {
            Task.Factory.StartNew(() =>
                {
                    while (!_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        var action = _queue.Take();
                        action();
                    }
                }, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
        }

        public void Queue(Action action)
        {
            _queue.Add(action);
        }
    }
}