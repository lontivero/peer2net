using System;
using System.Collections.Generic;

namespace P2PNet.BufferManager
{
    public class Buffer : List<ArraySegment<byte>>
    {
        public void CopyTo(byte[] array)
        {
            var length = array.Length;
            var offset = 0;
            foreach (var segment in this)
            {
                System.Buffer.BlockCopy(segment.Array, segment.Offset, array, offset,
                                        Math.Min(segment.Count, length - offset));
                offset += segment.Count;
            }
        }
    }
}