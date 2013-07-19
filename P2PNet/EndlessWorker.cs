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
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace P2PNet
{
    internal class EndlessWorker<T>
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly BlockingCollection<T> _queue;
        private readonly Action<T> _task;

        public EndlessWorker(Action<T> task)
        {
            _task = task;
            _queue = new BlockingCollection<T>();
            _cancellationTokenSource = new CancellationTokenSource();
            Start();
        }

        public void Start()
        {
            Task.Factory.StartNew(() =>
                {
                    while (!_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        var state = _queue.Take();
                        _task(state);
                    }
                }, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
        }

        public void Enqueue(T state)
        {
            _queue.Add(state);
        }
    }
}