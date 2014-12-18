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

using System;

namespace Open.P2P.BufferManager
{
    public class BufferAllocator : IBufferAllocator
    {
        private const int BlockSize = 1 << 5; // 32 bytes block size 
        private readonly BuddyBufferAllocator _allocator;
        private readonly byte[] _buffer;

        public BufferAllocator(byte[] buffer)
        {
            _buffer = buffer;
            _allocator = BuddyBufferAllocator.Create(SizeToBlocks(buffer.Length));
        }

        public ArraySegment<byte> Allocate(int size)
        {
            var blocks = SizeToBlocks(size);
            var offset = _allocator.Allocate(blocks);
            if (offset == -1) return new ArraySegment<byte>();

            //TODO
            //PerformanceCounters.BufferMemoryUsed.IncrementBy(blocks*BlockSize);
            return new ArraySegment<byte>(_buffer, offset * BlockSize, size);
        }

        public void Free(ArraySegment<byte> buffer)
        {
            var blocks = SizeToBlocks(buffer.Count);
            _allocator.Free(buffer.Offset / BlockSize);

            //TODO
            //PerformanceCounters.BufferMemoryUsed.IncrementBy(blocks * BlockSize);
        }

        private static int SizeToBlocks(int size)
        {
            return (int) Math.Ceiling((decimal) size/BlockSize);
        }

        public ArraySegment<byte> AllocateAndCopy(byte[] data)
        {
            var buffer = Allocate(data.Length);
            Buffer.BlockCopy(data, 0, buffer.Array, buffer.Offset, data.Length);
            return buffer;
        }
    }
}