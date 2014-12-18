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

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Open.P2P.BufferManager;
using Open.P2P.Utils;

namespace Open.P2P.Streams.Readers
{
    public class PascalStreamReader : PeerStreamReader
    {
        private static readonly IBufferAllocator _bufferAllocator = new BufferAllocator(new byte[1024*256]);

        public PascalStreamReader(Stream stream)
            : base(stream, _bufferAllocator)
        {
        }

        public PascalStreamReader(Stream stream, BufferAllocator bufferAllocator) 
            : base(stream, bufferAllocator)
        {
        }

        public override async Task<byte[]> ReadBytesAsync()
        {
            var packet = new List<byte>(4);

            for (int i = 0; i < 4; i++)
            {
                var b = await ReadByteAsync();
                packet.Add(b);
            }

            var messageLength = packet.ToArray();
            if (BitConverter.IsLittleEndian) Array.Reverse(messageLength);
            var intBytes = BitConverter.ToInt32(messageLength, 0);

            packet = new List<byte>();

            for (int i = 0; i < intBytes; i++)
            {
                var b = await ReadByteAsync();
                packet.Add(b);
            }

            return packet.ToArray();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public static byte[] FormatMessage(byte[] message)
        {
            Guard.NotNull(message, "message");
            var messageLength = message.Length;
            var intBytes = BitConverter.GetBytes(messageLength);
            if (BitConverter.IsLittleEndian) Array.Reverse(intBytes);

            var data = new byte[messageLength + 4];
            System.Buffer.BlockCopy(message, 0, data, 4, messageLength);
            System.Buffer.BlockCopy(intBytes, 0, data, 0, 4);
            return data;
        }
    }
}