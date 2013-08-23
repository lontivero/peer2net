//
// - ConnectionTests.cs
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

using System.Net;
using System.Net.Sockets;
using System.Threading;
using NUnit.Framework;
using Peer2Net.BufferManager;

namespace Peer2Net.Tests
{
    [TestFixture]
    public class ConnectionTests
    {
        private Listener _remote;
        private ManualResetEvent _completion;

        [SetUp]
        public void Setup()
        {
            _remote = new Listener(9999);
            _remote.Start();
            _completion = new ManualResetEvent(false);
        }

        [TearDown]
        public void TearDown()
        {
            _remote.Stop();
        }

        [Test]
        public void ShouldConnectToListener()
        {
            var connected = false;
            var connection = new Connection(new IPEndPoint(IPAddress.Loopback, 9999));
            connection.Connect((success) => { connected = success; _completion.Set(); });
            _completion.WaitOne();

            Assert.IsTrue(connected);
        }

        [Test]
        public void ShouldNotConnectToWrongPort()
        {
            var connected = true;
            var connection = new Connection(new IPEndPoint(IPAddress.Loopback, 7777));
            connection.Connect((success) => { connected = success; _completion.Set(); });
            _completion.WaitOne();

            Assert.IsFalse(connected);
        }

        [Test]
        public void ShouldSendData()
        {
            var localConnectionWaiter = new ManualResetEvent(false);
            var remoteConnectionWaiter = new ManualResetEvent(false);

            Connection remoteConnection = null;
            _remote.ConnectionRequested += (sender, args) => { 
                remoteConnection = new Connection(args.Socket);
                remoteConnectionWaiter.Set();
            };

            var localConnection = new Connection(new IPEndPoint(IPAddress.Loopback, 9999));
            localConnection.Connect(c => localConnectionWaiter.Set());
            WaitHandle.WaitAll(new WaitHandle[] { localConnectionWaiter, remoteConnectionWaiter });
            localConnectionWaiter.Reset();
            remoteConnectionWaiter.Reset();

            var remoteBuffer = new Buffer(new byte[1]);
            remoteConnection.Receive(remoteBuffer, (i, b) => remoteConnectionWaiter.Set());
            localConnection.Send(new Buffer(new byte[] { 0xf1 }), (i, b) => localConnectionWaiter.Set());
            WaitHandle.WaitAll(new WaitHandle[] { localConnectionWaiter, remoteConnectionWaiter });

            Assert.AreEqual(0xf1, remoteBuffer.Segment.Array[0]); 
        }

    }
}
