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