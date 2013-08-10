//
// - Program.cs
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
using Mono.Options;

namespace Peer2Net.ConsoleChat
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            var settings = new Settings();
            var p = new OptionSet()
                .Add("t", v => settings.Tracing = true)
                .Add("p=|port=", v => settings.Port = Int32.Parse(v))
                .Add("s=|seed=", v => settings.Seed = v);

            p.Parse (args);
            var console = new P2PConsole(settings);
            console.Start();

        }
    }
}
