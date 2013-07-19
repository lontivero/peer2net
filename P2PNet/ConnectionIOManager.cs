//
// - ConnectionIOManager.cs
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

namespace P2PNet
{
    internal class ConnectionIoActor
    {
        private readonly EndlessWorker<ReceiveJobState> _worker;

        public ConnectionIoActor()
        {
            _worker = new EndlessWorker<ReceiveJobState>(Receive);
        }

        private void Receive(ReceiveJobState jobState)
        {
            const bool canReceive = true;

            if (canReceive)
            {
                jobState.Connection.Receive();
            }
            else
            {
                _worker.Enqueue(jobState);
            }
        }

        public void EnqueueReceive(Connection connection)
        {
            Receive(new ReceiveJobState(connection));
        }
    }
}