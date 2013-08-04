//
// - ConnectState.cs
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

namespace P2PNet
{
    internal class ConnectState
    {
        private readonly Action<Connection> _onSuccess;
        private readonly Action<Connection> _onFailure;
        private readonly Connection _connection;

        private ConnectState(Connection connection, Action<Connection> onSuccess, Action<Connection> onFailure)
        {
            _connection = connection;
            _onSuccess = onSuccess;
            _onFailure = onFailure;
        }

        public Action SuccessCallback
        {
            get { return ()=>_onSuccess(_connection); }
        }

        public Action FailureCallback
        {
            get { return () => _onFailure(_connection); }
        }

        public Connection Connection
        {
            get { return _connection; }
        }

        public static ConnectState Create(Connection connection, Action<Connection> onSuccess, Action<Connection> onFailure)
        {
            return new ConnectState(connection, onSuccess, onFailure);
        }
    }
}