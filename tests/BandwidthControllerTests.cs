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
using System.Threading.Tasks;
using NUnit.Framework;
using Open.P2P.Progress;

namespace Open.P2P.Tests
{
    [TestFixture]
    public class BandwidthControllerTests
    {
        [Test]
        public async Task ControlBeforeUpdate()
        {
            var controller = new BandwidthController {TargetSpeed = 1024};
            var w = new Stopwatch();
            w.Start();
            await controller.WaitToTransmit(1024);
            controller.UpdateSpeed(1024, TimeSpan.FromSeconds(1));
            await controller.WaitToTransmit(1024);
            controller.UpdateSpeed(1024, TimeSpan.FromSeconds(1));
            await controller.WaitToTransmit(1024);
            controller.UpdateSpeed(1024, TimeSpan.FromSeconds(1));
            await controller.WaitToTransmit(1024);
            w.Stop();
            Assert.AreEqual(3, w.Elapsed.TotalSeconds, 0.4);
        }
    }
}
