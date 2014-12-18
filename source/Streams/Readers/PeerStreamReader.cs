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

    public abstract class PeerStreamReader
    {
        protected readonly Stream Stream;
        protected readonly IBufferAllocator BufferAllocator;
        protected ArraySegment<byte> Buffer;
        protected int Pos;
        protected int End;
        protected const int BufferSize = 64;

        protected PeerStreamReader(Stream stream, IBufferAllocator bufferAllocator)
        {
            Stream = stream;
            BufferAllocator = bufferAllocator;
        }

        public abstract Task<byte[]> ReadBytesAsync();

        protected async Task<byte> ReadByteAsync()
        {
            await ReadBufferAsync();
            var b = Buffer.Array[Buffer.Offset + Pos];
            Pos++;
            return b;
        }

        private async Task ReadBufferAsync()
        {
            if (Pos < End) return;


            BufferAllocator.Free(Buffer);
            Buffer = BufferAllocator.Allocate(BufferSize);

            Pos = 0;
            var readed = await Stream.ReadAsync(Buffer.Array, Buffer.Offset, Buffer.Count, new CancellationToken());
            if(readed == 0) throw new EndOfStreamException();
            End = readed;
        }
    }
}