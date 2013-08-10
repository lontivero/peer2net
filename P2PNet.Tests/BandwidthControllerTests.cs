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
using System.Diagnostics;
using NUnit.Framework;
using Peer2Net.BufferManager;
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
            Assert.IsTrue(controller.CanTransmit(512), "Controller should allow transmit 512");
            Assert.IsFalse(controller.CanTransmit(512), "Controller should NOT allow transmit 512 after exceed the limit");
        }

        [Test]
        public void ControlAccumulatedBytes()
        {
            var controller = new BandwidthController {TargetSpeed = 1024};
            controller.Update(measuredSpeed: 0, deltaTime: TimeSpan.FromSeconds(1));
            Assert.IsTrue(controller.CanTransmit(2048), "Controller should allow transmit accumulated 2048 bytes");
        }

        [Test]
        public void ControlAccumulatedBytesAfterUpdate()
        {
            var controller = new BandwidthController { TargetSpeed = 1024 };
            controller.Update(measuredSpeed: 1024, deltaTime: TimeSpan.FromSeconds(1));
            Assert.IsFalse(controller.CanTransmit(1025), "Controller should NOT allow transmit more than 2048 bytes");
            Assert.IsTrue(controller.CanTransmit(1024), "Controller should allow transmit 2048");
        }

        [Test]
        public void ControlTooSlow()
        {
            var controller = new BandwidthController { TargetSpeed = 1024 };
            controller.CanTransmit(512);
            controller.Update(measuredSpeed: 512, deltaTime: TimeSpan.FromSeconds(1));
            Assert.IsFalse(controller.CanTransmit(2048), "Controller should NOT allow transmit more than 1024 bytes");
            Assert.IsTrue(controller.CanTransmit(1024), "Controller should allow transmit 1024");
        }

        public void ControlSmallTimeFrame()
        {
            var controller = new BandwidthController { TargetSpeed = 1024 };
            controller.CanTransmit(512);
            controller.Update(measuredSpeed: 1024, deltaTime: TimeSpan.FromSeconds(0.5));
            Assert.IsFalse(controller.CanTransmit(513), "Controller should NOT allow transmit more than 512 bytes");
            Assert.IsTrue(controller.CanTransmit(512), "Controller should allow transmit 512");
        }

        [Test]
        public void ControlTooFast()
        {
            var controller = new BandwidthController { TargetSpeed = 1024 };
            controller.Update(measuredSpeed: 4 * 1024, deltaTime: TimeSpan.FromSeconds(1));
            Assert.IsFalse(controller.CanTransmit(1), "Controller should NOT allow transmit");
            Assert.IsTrue(controller.CanTransmit(0), "Controller should allow transmit 0 bytes");
        }

        [Test]
        public void ShouldTakeAbout10Seconds()
        {
            var controller = new BandwidthController { TargetSpeed = 1000 }; // 1 Kb/s
            var speedWatcher = new SpeedWatcher();
            var obj = new object();
            var rand = new Random();
            var timer = new System.Timers.Timer(500); // Calculate speed once per second
            int acc = 0;
            timer.Elapsed += (o, e)=>{
                lock(obj)
                {
                    speedWatcher.CalculateAndReset();
                    controller.Update(speedWatcher.BytesPerSecond, speedWatcher.MeasuredDeltaTime);
                    //Debug.WriteLine( "  -sent: " + acc);
                    //acc = 0;
                }
            };

            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();
            timer.Start();

            int bytesToTransmit = 20*1000; // we want to transmit 10 KB
            while (bytesToTransmit > 0)
            {
                int bytes = rand.Next(20);
                if (!controller.CanTransmit(bytes)) continue;
                bytesToTransmit -= bytes;
                acc += bytes;
                speedWatcher.AddBytes(bytes);
            }
            stopWatch.Stop();
            timer.Close();
            var measuredTime = stopWatch.ElapsedMilliseconds;
            const int expectedTime = 20*1000;
            Debug.WriteLine(measuredTime);
            Assert.IsTrue(Math.Abs(measuredTime - expectedTime) < (expectedTime*0.02));
        }

        //[Test]
        //public void Allow_1KBs2()
        //{
        //    var controller = new BandwidthController();
        //    var randomGenerator = new Random();
        //    var mem = new MemoryStream();
        //    var buff = new byte[1];

        //    for(int i=0;;i++)
        //    {
        //        if(controller.CanPerform(1))
        //        {
        //            randomGenerator.NextBytes(buff);
        //            mem.WriteByte(buff[0]);
        //            if(i == 30) break;
        //            controller.Update(1);
        //        }
        //        else
        //        {
        //            controller.Update(0);
        //        }
        //    }
        //}
    }
}
