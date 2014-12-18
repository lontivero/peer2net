using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Open.P2P.IO
{
    public class SocketAwaitable : INotifyCompletion
    {
        private static readonly Action SENTINEL = () => { };

        internal bool WasCompleted;
        internal Action Continuation;
        internal SocketAsyncEventArgs EventArgs;

        public SocketAwaitable(SocketAsyncEventArgs eventArgs)
        {
            if (eventArgs == null) throw new ArgumentNullException("eventArgs");
            EventArgs = eventArgs;
            eventArgs.Completed += (sender, args) => {
                                       var prev = Continuation ?? Interlocked.CompareExchange(
                                           ref Continuation, SENTINEL, null);
                                       if (prev != null) prev();
                                   };
        }

        internal void Reset()
        {
            WasCompleted = false;
            Continuation = null;
        }

        public SocketAwaitable GetAwaiter()
        {
            return this;
        }

        public bool IsCompleted
        {
            get { return WasCompleted; }
        }

        public void OnCompleted(Action continuation)
        {
            if (Continuation == SENTINEL ||
                Interlocked.CompareExchange(
                    ref Continuation, continuation, null) == SENTINEL)
            {
                Task.Run(continuation);
            }
        }

        public void GetResult()
        {
            if (EventArgs.SocketError != SocketError.Success)
                throw new SocketException((int) EventArgs.SocketError);
        }
    }

    public static class SocketExtensions
    {
        public static SocketAwaitable ReceiveAsync(this Socket socket,
            SocketAwaitable awaitable)
        {
            awaitable.Reset();
            if (!socket.ReceiveAsync(awaitable.EventArgs))
                awaitable.WasCompleted = true;
            return awaitable;
        }

        public static SocketAwaitable SendAsync(this Socket socket,
            SocketAwaitable awaitable)
        {
            awaitable.Reset();
            if (!socket.SendAsync(awaitable.EventArgs))
                awaitable.WasCompleted = true;
            return awaitable;
        }

        public static SocketAwaitable ConnectAsync(this Socket socket,
            SocketAwaitable awaitable)
        {
            awaitable.Reset();
            if (!socket.ConnectAsync(awaitable.EventArgs))
                awaitable.WasCompleted = true;
            return awaitable;
        }
    }
}
