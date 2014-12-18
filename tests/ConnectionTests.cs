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

using System;
using System.Net;
using System.Threading;
using NUnit.Framework;
using Open.P2P.IO;
using TcpListener = Open.P2P.Listeners.TcpListener;

namespace Open.P2P.Tests
{
    [TestFixture]
    public class ConnectionTests
    {
        private TcpListener _remote;

        [SetUp]
        public void Setup()
        {
            _remote = new TcpListener(9999);
            _remote.Start();
        }

        [TearDown]
        public void TearDown()
        {
            _remote.Stop();
        }

        [Test]
        public async void ShouldConnectToListener()
        {
            var connection = new Connection(new IPEndPoint(IPAddress.Loopback, 9999));
            await connection.ConnectAsync();

            Assert.IsTrue(connection.IsConnected);
        }

        [Test]
        public async void ShouldNotConnectToWrongPort()
        {
            var connection = new Connection(new IPEndPoint(IPAddress.Loopback, 7777));
            await connection.ConnectAsync();

            Assert.IsFalse(connection.IsConnected);
        }

        [Test]
        public async void ShouldSendData()
        {
            var remoteConnectionWaiter = new ManualResetEvent(false);

            Connection remoteConnection = null;
            _remote.ConnectionRequested += (sender, args) => { 
                remoteConnection = new Connection(args.Socket);
                remoteConnectionWaiter.Set();
            };

            var localConnection = new Connection(new IPEndPoint(IPAddress.Loopback, 9999));
            await localConnection.ConnectAsync();
            remoteConnectionWaiter.WaitOne();

            var remoteBuffer = new ArraySegment<byte>(new byte[1]);
            await localConnection.SendAsync(new ArraySegment<byte>(new byte[] { 0xf1 }));
            await remoteConnection.ReceiveAsync(remoteBuffer);

            Assert.AreEqual(0xf1, remoteBuffer.Array[0]); 
        }
    }
}
