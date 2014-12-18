//
// - RawPacketHandler.cs
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

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Open.P2P.BufferManager;

namespace Open.P2P.Streams.Readers
{
    public class EotStreamReader : PeerStreamReader
    {
        public EotStreamReader(Stream stream, BufferAllocator bufferAllocator) 
            : base(stream, bufferAllocator)
        {
        }

        public override async Task<byte[]> ReadBytesAsync()
        {
            var packet = new List<byte>();
            byte b;
            while((b = await ReadByteAsync()) > 0)
            {
                packet.Add(b);
            }
            return packet.ToArray();
        }
    }
}