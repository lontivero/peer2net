//
// - CommunicationIoActorTests.cs
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
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Open.P2P.BufferManager;
using Open.P2P.IO;
using Open.P2P.Workers;

namespace Open.P2P.Tests
{
    class TestWorkScheduler : IWorkScheduler
    {
        public Action<Action, TimeSpan> QueueOneTimeAction;
        public Action<Action, TimeSpan> QueueForeverAction;

        public void QueueForever(Action action, TimeSpan interval)
        {
            if (QueueForeverAction != null)
                QueueForeverAction(action, interval);
        }

        public void QueueOneTime(Action action, TimeSpan interval)
        {
            if (QueueOneTimeAction!=null)   
                QueueOneTimeAction(action, interval);
        }
    }

    class TestBufferAllocator : IBufferAllocator
    {
        public Func<int, ArraySegment<byte>> AllocateFunc; 
        public ArraySegment<byte> Allocate(int size)
        {
            return AllocateFunc!=null ? AllocateFunc(size) : new ArraySegment<byte>();
        }

        public void Free(ArraySegment<byte> segments)
        {
        }
    }

    class TestConnection: IConnection
    {
        public Func<ArraySegment<byte>, Task<int>> ReceiveAction;

        public IPEndPoint Endpoint { get; private set; }
        public bool IsConnected { get; private set; }

        public async Task<int> ReceiveAsync(ArraySegment<byte> buffer)
        {
            if (ReceiveAction != null) return await ReceiveAction(buffer);
            return 0;
        }

        public Task<int> SendAsync(ArraySegment<byte> buffer)
        {
            return new Task<int>(() => 0);
        }

        public Task ConnectAsync()
        {
            return new Task<int>(() => 0);
        }

        public void Close()
        {
        }
    }

    [TestFixture]
    public class CommunicationIoActorTests
    {
        private readonly IBufferAllocator _dummyBufferAllocator = new BufferAllocator(new byte[0]);

        [Test]
        public void ShouldInvokeFailureCallback()
        {
            var connected = false;
            var connectWaiter = new ManualResetEvent(false);
            var endpoint = new IPEndPoint(IPAddress.Loopback, 9999);

            //io.Connect(new Connection(endpoint), c => callback(c, true), c => callback(c, false));
            connectWaiter.WaitOne();
            Assert.IsFalse(connected);
        }

        [Test]
        public void ShouldFailBecauseTimeout()
        {
            var connected = false;
            var timeout = false;

            var connectWaiter = new ManualResetEvent(false);
            var worker = new TestWorkScheduler{
                QueueOneTimeAction = (action, interval) =>{
                    Thread.Sleep(50);
                    action();
                    timeout = true;
                }
            };

            //var io = new ConnectionIoActor(worker);
            var endpoint = new IPEndPoint(IPAddress.Parse("217.87.23.53"), 9999);
            Action<IConnection, bool> callback = (connection, success) =>
            {
                connected = success;
                connectWaiter.Set();
            };

            //io.Connect(new Connection(endpoint), c => callback(c, true), c => callback(c, false));
            connectWaiter.WaitOne();
            Assert.IsFalse(connected);
            Assert.IsTrue(timeout);
        }


        [Test]
        public void ShouldWaitForBuffer()
        {
            var receiveCalled = false;
            var receiveWaiter = new ManualResetEvent(false);
            var worker = new TestWorkScheduler();
            worker.QueueForeverAction = (action, span) =>
            {
                Task.Factory.StartNew(() =>
                {
                    while (true) action();
                });
            };
            var bufferAllocator = new TestBufferAllocator();
            var connection = new TestConnection();
            //var io = new ConnectionIoActor(worker);

            bufferAllocator.AllocateFunc= i => new ArraySegment<byte>();
            connection.ReceiveAction = (buffer) => {
                receiveCalled = true;
                receiveWaiter.Set();
                return new Task<int>(()=>19);
            };
            //io.Receive(new byte[128], connection, new BandwidthController(), null, null );
            bufferAllocator.AllocateFunc = i => new ArraySegment<byte>(new byte[128]);
            receiveWaiter.WaitOne(100);

            Assert.IsTrue(receiveCalled, "Receive was not called.");
        }
    }
}
