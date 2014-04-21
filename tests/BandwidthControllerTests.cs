//
// - BandwidthControllerTests.cs
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
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Peer2Net.Progress;

namespace Peer2Net.Tests
{
    [TestFixture]
    public class BandwidthControllerTests
    {

        [Test]
        public void ControlBeforeUpdate()
        {
            var controller = new BandwidthController {TargetSpeed = 1024};
            Assert.IsFalse(controller.CanTransmit(3453), "Controller should NOT allow more than target speed");
            Assert.IsTrue(controller.CanTransmit(512), "Controller should allow transmit 512");
            controller.SetTransmittion(512);
            Assert.IsTrue(controller.CanTransmit(512), "Controller should allow transmit 512");
            controller.SetTransmittion(512);
            Assert.IsFalse(controller.CanTransmit(512), "Controller should NOT allow transmit 512 after exceed the limit");
        }

        [Test]
        public void ControlAccumulatedBytes()
        {
            var controller = new BandwidthController {TargetSpeed = 1024};
            controller.SetTransmittion(0);
            controller.Update(measuredSpeed: 0, deltaTime: TimeSpan.FromSeconds(1));
            Assert.IsTrue(controller.CanTransmit(2048), "Controller should allow transmit accumulated 2048 bytes");
        }

        [Test]
        public void ControlAccumulatedBytesAfterUpdate()
        {
            var controller = new BandwidthController { TargetSpeed = 1024 };
            controller.SetTransmittion(0);
            controller.Update(measuredSpeed: 0, deltaTime: TimeSpan.FromSeconds(1));
            Assert.IsTrue(controller.CanTransmit(2048), "Controller should allow transmit 2048");
        }

        [Test]
        public void ControlTooSlow()
        {
            var controller = new BandwidthController { TargetSpeed = 1024 };
            controller.SetTransmittion(512);
            controller.Update(measuredSpeed: 512, deltaTime: TimeSpan.FromSeconds(1));
            Assert.IsFalse(controller.CanTransmit(2048), "Controller should NOT allow transmit more than 1024 bytes");
            Assert.IsTrue(controller.CanTransmit(1024), "Controller should allow transmit 1024");
        }

        [Test]
        public void ControlTooFast()
        {
            var controller = new BandwidthController { TargetSpeed = 1024 };
            controller.SetTransmittion(4 * 1024);
            controller.Update(measuredSpeed: 4 * 1024, deltaTime: TimeSpan.FromSeconds(1));
            Assert.IsFalse(controller.CanTransmit(1), "Controller should NOT allow transmit");
            Assert.IsTrue(controller.CanTransmit(0), "Controller should allow transmit 0 bytes");
        }

        [Test]
        public void ControlSmallTimeFrame()
        {
            var controller = new BandwidthController { TargetSpeed = 1024 };
            controller.SetTransmittion(1024);
            controller.Update(measuredSpeed: 1024, deltaTime: TimeSpan.FromSeconds(0.5));
            Assert.IsFalse(controller.CanTransmit(1024), "Controller should NOT allow transmit ");
        }

        [Test]
        public void ControlLongTimeFrame()
        {
            var controller = new BandwidthController { TargetSpeed = 1024 };
            controller.Update(measuredSpeed: 1024, deltaTime: TimeSpan.FromSeconds(2));
            Assert.IsTrue(controller.CanTransmit(1025), "Controller should allow transmit ");
        }

        [Test, Timeout(5000)]
        public void ShouldTakeAbout4Seconds()
        {
            var list = new Queue(Enumerable.Range(1, 4000).ToArray());
            var controller = new BandwidthController { TargetSpeed = 1000 };
            var processed = 0;
            var timer = new System.Timers.Timer();
            timer.Elapsed += (sender, args) =>
            {
                lock (controller)
                {
                    controller.Update(processed, TimeSpan.FromMilliseconds(250));
                    processed = 0;
                }
            };
            timer.Interval = 250;
            var sw = new Stopwatch();
            timer.Start();
            sw.Start();
            while (list.Count > 0)
            {
                if (controller.CanTransmit(1))
                {
                    list.Dequeue();
                    processed++;
                    controller.SetTransmittion(1);
                }
                else
                {
                    Thread.Sleep(10);
                }
            }
            timer.Stop();
            sw.Stop();
            var measuredTime = sw.ElapsedMilliseconds;
            const int expectedTime = 4000;
            Assert.IsTrue(Math.Abs(measuredTime - expectedTime) < (expectedTime * 0.3));
        }
    }
}
