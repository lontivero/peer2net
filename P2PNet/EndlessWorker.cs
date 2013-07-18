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