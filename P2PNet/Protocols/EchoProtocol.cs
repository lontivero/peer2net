//
// - EchoProtocol.cs
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

namespace P2PNet.Protocols
{
    public class EchoProtocol : IProtocol
    {
        private readonly IProtocol _baseProtocol;

        public EchoProtocol(IProtocol baseProtocol)
        {
            _baseProtocol = baseProtocol;
            _baseProtocol.MessageReceived += BaseProtocolOnMessageReceived;
        }

        #region IProtocol Members

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        #endregion

        private void BaseProtocolOnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Send(e.Client, e.Message);
        }

        internal void Send(Connection connection, string message)
        {
            // _baseProtocol.Send(connection, message);
        }
    }
}