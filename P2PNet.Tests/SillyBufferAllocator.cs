using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TcpServer.Tests
{
    class SillyBufferAllocator
    {
        private string _map;
        private readonly byte[] _memory;
        private const int BlockSize = 128;

        public ArraySegment<byte> Allocate(int sizeInBytes)
        {
            var blockCount = SizeToBlocks(sizeInBytes);
            var freePattern = new string('f', blockCount);
            var usedPattern = new string('-', blockCount);
            var blockOffset = _map.IndexOf(freePattern);
            _map = _map.Substring(0, blockOffset) + usedPattern + _map.Substring(blockOffset + blockCount);

            return new ArraySegment<byte>(_memory, blockOffset * BlockSize, sizeInBytes);
        }

        public void Free(ArraySegment<byte> segment)
        {
            var blockCount = SizeToBlocks(segment.Count);
            var freePattern = new string('f', blockCount);
            var blockOffset = segment.Offset/BlockSize;
            _map = _map.Substring(0, blockOffset) + freePattern + _map.Substring(blockOffset + blockCount);
        }

        private int SizeToBlocks(int sizeInBytes)
        {
            throw new NotImplementedException();
        }
    }
}
