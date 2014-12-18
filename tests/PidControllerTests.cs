//
// - PidControllerTests.cs
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
using Open.P2P.Progress;

namespace Open.P2P.Tests
{
    [TestFixture]
    public class PidControllerTests
    {
        [Test]
        public void ItMustConvege()
        {
            var rand = new Random();
            var desired = 100 * rand.NextDouble();
            var measured = 100 * rand.NextDouble();

            var controller = new PidController(0.6, 0.4);
            for(int i = 0; i < 35 ; i++)
            {
                var error = desired - measured;
                Debug.WriteLine("{0} e: {1}",i, error);
                var output = controller.Control(error, 1);

                desired += 10;
                measured = measured + output;
            }

            Assert.IsTrue(Math.Abs(desired - measured) <= 1e-5);
        }
    }
}
