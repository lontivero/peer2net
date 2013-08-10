//
// - Buffer.cs
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
using System.Collections.Generic;

namespace P2PNet.BufferManager
{
    internal class Buffer
    {
        private readonly ArraySegment<byte> _segment;
        private readonly List<ArraySegment<byte>> _arrayOfSegments;

        public Buffer(byte[] array)
        {
            _segment = new ArraySegment<byte>(array);
            _arrayOfSegments = new List<ArraySegment<byte>> { _segment };
        }

        public Buffer(ArraySegment<byte> segment)
        {
            _segment = segment;
            _arrayOfSegments = new List<ArraySegment<byte>> { _segment };
        }

        public void CopyTo(byte[] array)
        {
            var length = array.Length;
            System.Buffer.BlockCopy(_segment.Array, _segment.Offset, array, 0, Math.Min(_segment.Count, length));
        }

        public int Size 
        { 
            get { return _segment.Count; }
        }

        internal ArraySegment<byte> Segment
        {
            get { return _segment; }
        }

        public List<ArraySegment<byte>> ToArraySegmentList()
        {
            return _arrayOfSegments;
        }

    }
}