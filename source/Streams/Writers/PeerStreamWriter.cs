//
// - IPacketHandler.cs
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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Open.P2P.BufferManager;

namespace Open.P2P.Streams.Readers
{

    public class PeerStreamWriter
    {
        protected readonly Stream Stream;
        protected readonly IBufferAllocator BufferAllocator;
        protected ArraySegment<byte> Buffer;
        protected int Pos;
        protected int End;
        protected const int BufferSize = 64;

        public PeerStreamWriter(Stream stream, IBufferAllocator bufferAllocator)
        {
            Stream = stream;
            BufferAllocator = bufferAllocator;
        }

        public virtual async Task WriteBytesAsync(byte[] bytes)
        {
            var msgLen = bytes.Length + 1;
            var buf = BufferAllocator.Allocate(msgLen);
            System.Buffer.BlockCopy(bytes, 0, buf.Array, buf.Offset, msgLen-1);
            await Stream.WriteAsync(buf.Array, buf.Offset, msgLen);
            BufferAllocator.Free(buf);
        }
    }
}