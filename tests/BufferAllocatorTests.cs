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

using NUnit.Framework;
using Open.P2P.BufferManager;

namespace Open.P2P.Tests
{
    [TestFixture]
    public class BufferAllocatorTests
    {
        private BufferAllocator CreateBufferManager()
        {
            return new BufferAllocator(new byte[256]);
        }

        [Test]
        public void Test1 ()
        {
            // Step 1  [--------]
            // Step 2  [xx------] alloc 64 -> 0..63 
            var bufferManager = CreateBufferManager();
            var buffer = bufferManager.Allocate(64);
            Assert.AreEqual(0, buffer.Offset);
            Assert.AreEqual(64, buffer.Count);
        }

        [Test]
        public void Test2()
        {
            // Step 1  [--------]
            // Step 2  [xx------] alloc  64 ->   0.. 63 
            // Step 3  [xx--xxxx] alloc 128 -> 128..255 
            var bufferManager = CreateBufferManager();
            var buffer1 = bufferManager.Allocate(64);
            var buffer2 = bufferManager.Allocate(128);
            Assert.AreEqual(128, buffer2.Offset);
            Assert.AreEqual(128, buffer2.Count);
        }

        [Test]
        public void Test3()
        {
            // Step 1  [--------]
            // Step 2  [xx------] alloc  64 ->   0.. 63 * 
            // Step 3  [--------] free buffer (step 2)
            // Step 4  [xxxx----] alloc 128 ->   0..127
            var bufferManager = CreateBufferManager();
            var buffer1 = bufferManager.Allocate(64);
            bufferManager.Free(buffer1);

            var buffer2 = bufferManager.Allocate(128);
            Assert.AreEqual(0, buffer2.Offset);
            Assert.AreEqual(128, buffer2.Count);
        }

        [Test]
        public void Test4()
        {
            // Step 1  [--------]
            // Step 2  [xx------] alloc  64 ->   0.. 63
            // Step 3  [xxx-----] alloc  32 ->  64.. 95 *
            // Step 4  [xxxx----] alloc  32 ->  96..127
            // Step 5  [xx-x----] free buffer (step 3)
            // Step 6  [xxxx----] alloc  32 ->  64.. 95
            var bufferManager = CreateBufferManager();

            bufferManager.Allocate(64);
            var buffer = bufferManager.Allocate(32);
            bufferManager.Allocate(32); 

            bufferManager.Free(buffer);

            buffer = bufferManager.Allocate(32); // 64 - 95
            Assert.AreEqual(64, buffer.Offset);
            Assert.AreEqual(32, buffer.Count);
        }

        [Test]
        public void Test5()
        {
            // Step 1  [--------]
            // Step 2  [xxxxxxxx]
            var bufferManager = CreateBufferManager();
            bufferManager.Allocate(256);
            Assert.IsNull(bufferManager.Allocate(1));
        }
    }
}
