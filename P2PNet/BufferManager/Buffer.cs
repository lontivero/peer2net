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