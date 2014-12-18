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
using Open.P2P.Listeners;
using TcpListener = Open.P2P.Listeners.TcpListener;

namespace Open.P2P.Tests
{
    [TestFixture]
    public class ListenerTests
    {
        [Test]
        public void ShouldBeStoppedBeforeStart()
        {
            var listener = new TcpListener(9991);
            Assert.AreEqual(ListenerStatus.Stopped, listener.Status);
        }

        [Test]
        public void ShouldBeListeningAfterStart()
        {
            var listener = new TcpListener(9992);
            listener.Start();
            Assert.AreEqual(ListenerStatus.Listening, listener.Status);
        }

        [Test]
        public void ShouldBeStoppedAfterStop()
        {
            var listener = new TcpListener(9993);
            listener.Start();
            listener.Stop();
            Assert.AreEqual(ListenerStatus.Stopped, listener.Status);
        }

        [Test]
        public void ShouldBeSilentAfterStopWhenStopped()
        {
            var listener = new TcpListener(9994);
            listener.Stop();
            Assert.AreEqual(ListenerStatus.Stopped, listener.Status);
        }

        [Test]
        public void ShouldAllowRestart()
        {
            var listener = new TcpListener(9995);
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
            var listener = new TcpListener(9996);
            listener.ConnectionRequested += (sender, args) => { passed = true; completion.Set(); };
            listener.Start();

            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(new IPEndPoint(IPAddress.Loopback, listener.Port));
            completion.WaitOne(500);
            
            Assert.IsTrue(passed);
        }
    }
}
