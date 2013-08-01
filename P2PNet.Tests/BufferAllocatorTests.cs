//
// - BufferAllocator.cs
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

using System.Linq;
using NUnit.Framework;
using P2PNet.BufferManager;
using Buffer = P2PNet.BufferManager.Buffer;

namespace P2PNet.Tests
{
    [TestFixture]
    public class BufferAllocatorTests
    {

        [Test]
        public void Test1 ()
        {
            var memeory = new byte[256];
            var bufferManager = new P2PNet.BufferManager.BufferAllocator(memeory);
            var buffer = bufferManager.Allocate(64);
            Assert.AreEqual(0, buffer.Segment.Offset);
            Assert.AreEqual(64, buffer.Segment.Count);
        }

        [Test]
        public void Test2()
        {
            var memeory = new byte[256];
            var bufferManager = new P2PNet.BufferManager.BufferAllocator(memeory);
            var buffer1 = bufferManager.Allocate(64);
            var buffer2 = bufferManager.Allocate(128);
            Assert.AreEqual(128, buffer2.Segment.Offset);
            Assert.AreEqual(128, buffer2.Segment.Count);
        }

        [Test]
        public void Test3()
        {
            var memeory = new byte[256];
            var bufferManager = new BufferAllocator(memeory);
            var buffer1 = bufferManager.Allocate(64);
            bufferManager.Free(buffer1);

            var buffer2 = bufferManager.Allocate(128);
            Assert.AreEqual(0, buffer2.Segment.Offset);
            Assert.AreEqual(128, buffer2.Segment.Count);
        }

        [Test]
        public void Test4()
        {
            var memeory = new byte[256];
            var bufferManager = new P2PNet.BufferManager.BufferAllocator(memeory);

            bufferManager.Allocate(32); //  0 - 31
            bufferManager.Allocate(32); // 32 - 63
            Buffer buffer = bufferManager.Allocate(32);
            bufferManager.Allocate(32); // 96 - 128

            bufferManager.Free(buffer);

            buffer = bufferManager.Allocate(32); // 64 - 95
            Assert.AreEqual(64, buffer.Segment.Offset);
            Assert.AreEqual(32, buffer.Segment.Count);
        }
    }
}
