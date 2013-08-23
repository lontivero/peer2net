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
using Peer2Net.BufferManager;
using Peer2Net.Progress;
using Peer2Net.Workers;
using Buffer = Peer2Net.BufferManager.Buffer;

namespace Peer2Net.Tests
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
        public Func<int, Buffer> AllocateFunc; 
        public Buffer Allocate(int size)
        {
            return AllocateFunc!=null ? AllocateFunc(size) : null;
        }

        public void Free(Buffer segments)
        {
        }
    }
    class TestConnection: IConnection
    {
        public Action<Buffer, ConnectionIoCallback> ReceiveAction;

        public IPEndPoint Endpoint { get; private set; }
        public bool IsConnected { get; private set; }
        public void Receive(Buffer buffer, ConnectionIoCallback callback)
        {
            if(ReceiveAction!=null) ReceiveAction(buffer, callback);
        }
        public void Send(Buffer buffer, ConnectionIoCallback callback)
        {
        }
        public void Connect(ConnectionConnectCallback callback)
        {
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
            var worker = new TestWorkScheduler();
            var io = new ConnectionIoActor(worker, _dummyBufferAllocator);
            var endpoint = new IPEndPoint(IPAddress.Loopback, 9999);
            Action<IConnection, bool> callback = (connection, success) =>{
                connected = success;
                connectWaiter.Set();
            };
            io.Connect(new Connection(endpoint), c => callback(c, true), c => callback(c, false));
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

            var io = new ConnectionIoActor(worker, _dummyBufferAllocator);
            var endpoint = new IPEndPoint(IPAddress.Parse("217.87.23.53"), 9999);
            Action<IConnection, bool> callback = (connection, success) =>
            {
                connected = success;
                connectWaiter.Set();
            };

            io.Connect(new Connection(endpoint), c => callback(c, true), c => callback(c, false));
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
            var io = new ConnectionIoActor(worker, bufferAllocator);

            bufferAllocator.AllocateFunc= i => null;
            connection.ReceiveAction = (buffer, callback) => {
                receiveCalled = true;
                receiveWaiter.Set();
            };
            io.Receive(128, connection, new BandwidthController(), null, null );
            bufferAllocator.AllocateFunc = i => new Buffer(new byte[128]);
            receiveWaiter.WaitOne(100);

            Assert.IsTrue(receiveCalled, "Receive was not called.");
        }
    }
}
