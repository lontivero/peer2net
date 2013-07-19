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

namespace P2PNet.BufferManager
{
    internal class BufferAllocator : IBufferAllocator
    {
        private const int BlockSize = 1 << 5; // 32 bytes block size 
        private readonly BuddyBufferAllocator _allocator;
        private readonly byte[] _buffer;

        public BufferAllocator(byte[] buffer)
        {
            _buffer = buffer;
            _allocator = BuddyBufferAllocator.Create(SizeToBlocks(buffer.Length));
        }

        #region IBufferAllocator Members

        public Buffer Allocate(int size)
        {
            var offset = _allocator.Allocate(SizeToBlocks(size));
            return new Buffer {new ArraySegment<byte>(_buffer, offset, size)};
        }

        public void Free(Buffer segments)
        {
            foreach (var segment in segments)
            {
                _allocator.Free(segment.Offset);
            }
        }

        #endregion

        private static int SizeToBlocks(int size)
        {
            return (int) Math.Ceiling((decimal) size/BlockSize);
        }
    }
}