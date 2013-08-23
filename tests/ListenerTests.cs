//
// - Listener.cs
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

namespace Peer2Net.Tests
{
    [TestFixture]
    public class ListenerTests
    {
        [Test]
        public void ShouldBeStoppedBeforeStart()
        {
            var listener = new Listener(9991);
            Assert.AreEqual(ListenerStatus.Stopped, listener.Status);
        }

        [Test]
        public void ShouldBeListeningAfterStart()
        {
            var listener = new Listener(9992);
            listener.Start();
            Assert.AreEqual(ListenerStatus.Listening, listener.Status);
        }

        [Test]
        public void ShouldBeStoppedAfterStop()
        {
            var listener = new Listener(9993);
            listener.Start();
            listener.Stop();
            Assert.AreEqual(ListenerStatus.Stopped, listener.Status);
        }

        [Test]
        public void ShouldBeSilentAfterStopWhenStopped()
        {
            var listener = new Listener(9994);
            listener.Stop();
            Assert.AreEqual(ListenerStatus.Stopped, listener.Status);
        }

        [Test]
        public void ShouldAllowRestart()
        {
            var listener = new Listener(9995);
            listener.Start();
            listener.Stop();
            listener.Start();
            Assert.AreEqual(ListenerStatus.Listening, listener.Status);
        }

        [Test]
        public void ShouldNotifyNewConnection()
        {
            var completion = new ManualResetEvent(false);
            var passed = false;
            var listener = new Listener(9996);
            listener.ConnectionRequested += (sender, args) => { passed = true; completion.Set(); };
            listener.Start();

            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(new IPEndPoint(IPAddress.Loopback, listener.Port));
            completion.WaitOne(500);
            
            Assert.IsTrue(passed);
        }

        [Test]
        public void ShouldBeSilentAfterStopWhenStopped2()
        {
            var listener = new Listener(80);
            Assert.Throws<SocketException>(listener.Start);
        }
    }
}
