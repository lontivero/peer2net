//
// - Settings.cs
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

namespace P2PNet.NodeConsole
{
    class Settings
    {
        private bool _tracing;
        private int _port;
        private string _seed;

        public Settings()
        {
            _tracing = false;
            _port = 1234 + (new Random().Next(100));
        }

        public bool Tracing
        {
            get { return _tracing; }
            set { _tracing = value; }
        }

        public int Port
        {
            get { return _port; }
            set { _port = value; }
        }

        public string Seed
        {
            get { return _seed; }
            set { _seed = value; }
        }
    }
}
