//
// - BuddyBufferAllocator.cs
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
    internal class BuddyBufferAllocator
    {
        private readonly int _levels;
        private readonly List<int> _longest;

        public BuddyBufferAllocator(int levels)
        {
            _levels = levels;
            var nodeCount = (1 << (_levels + 1)) - 1;
            _longest = new List<int>(nodeCount);

            var nodeLevel = levels;
            while (nodeLevel >= 0)
            {
                for (var i = 1 << (_levels - nodeLevel); i > 0; --i)
                {
                    _longest.Add(nodeLevel);
                }
                --nodeLevel;
            }
        }

        private static int Log2(int num)
        {
            return (int) (Math.Log(num)/Math.Log(2));
        }

        public static BuddyBufferAllocator Create(int size)
        {
            var levels = Log2(size);
            return new BuddyBufferAllocator(levels);
        }

        public int Allocate(int size)
        {
            var level = size <= 1 ? 0 : Log2(size - 1) + 1;

            var index = 0;
            if (_longest[index] < level)
                return -1;

            for (var i = _levels; i != level; --i)
            {
                var leftIndex = LeftNode(index);
                index = _longest[leftIndex] >= level ? leftIndex : RightNode(index);
            }

            _longest[index] = -1;
            var offset = (index + 1 - (1 << (_levels - level))) << level;

            while (index != 0)
            {
                index = ParentNode(index);
                var leftLongest = _longest[LeftNode(index)];
                var rightLongest = _longest[RightNode(index)];
                _longest[index] = Math.Max(leftLongest, rightLongest);
            }
            return offset;
        }

        public void Free(int offset)
        {
            var fullSize = 1 << _levels;

            var index = offset - 1 + fullSize;
            var level = 0;
            while (_longest[index] != -1)
            {
                if (index == 0)
                    return;
                ++level;
                index = ParentNode(index);
            }

            _longest[index] = level;
            while (index != 0)
            {
                index = ParentNode(index);
                var leftLongest = _longest[LeftNode(index)];
                var rightLongest = _longest[RightNode(index)];

                _longest[index] = leftLongest == level && rightLongest == level
                                      ? level + 1
                                      : Math.Max(leftLongest, rightLongest);
                ++level;
            }
        }

        private static int LeftNode(int index)
        {
            return (index << 1) + 1;
        }

        private static int RightNode(int index)
        {
            return (index << 1) + 2;
        }

        private static int ParentNode(int index)
        {
            return ((index + 1) >> 1) - 1;
        }
    }
}