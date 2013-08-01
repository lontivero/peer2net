//
// - TcpServerConsole.cs
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
using System.Net;
using System.Text;

namespace P2PNet.NodeConsole
{
    class P2PConsole : IClientManager
    {
        private readonly Settings _settings;
        private readonly ComunicationManager _comunicationManager;
        private readonly Listener _listener;
        private readonly Dictionary<string, Action<string[]>> _commands;

        public P2PConsole(Settings settings)
        {
            _commands = new Dictionary<string, Action<string[]>>()
                {
                    {"connect", p => ConnectTo(p[1])},
                    {"send", p=>SendMessageTo(p[1], p[2])}
                };
            _settings = settings;
            _listener = new Listener(settings.Port);
            _comunicationManager = new ComunicationManager(_listener, this);
        }

        public void Start()
        {
            _listener.Start();
            Wellcome();

            var line = ReadCommand();
            while (line[0] != "q")
            {
                _commands[line[0]](line);
                line = ReadCommand();
            }

            Stop();
        }

        private void Stop()
        {
            _listener.Stop();
        }
        private void SendMessageTo(string message, string node)
        {
            var nodeParts = node.Split(':');
            var ip = IPAddress.Parse(nodeParts[0]);
            var port = int.Parse(nodeParts[1]);
            _comunicationManager.SendTo(GetBytes(message), new IPEndPoint(ip, port));
        }

        private void ConnectTo(string node)
        {
            var nodeParts = node.Split(':');
            var ip = IPAddress.Parse(nodeParts[0]);
            var port = int.Parse(nodeParts[1]);
            _comunicationManager.Connect(new IPEndPoint(ip, port) );
        }

        private void Wellcome()
        {
            Console.WriteLine("P2PNet Node listening in port: " + _settings.Port);
            Console.WriteLine("Press q to exit");
            Console.WriteLine("");
        }

        private static string[] ReadCommand()
        {
            Console.Write(">");
            var line = Console.ReadLine();
            return line.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
        }

        public void OnPeerConnected(Peer peer)
        {
            Console.WriteLine("Peer: {0} Connected!", peer.Connection.Uri);
            _comunicationManager.Receive(4, peer.Connection.Endpoint);
        }

        public void OnPeerDataReceived(Peer peer, byte[] buffer)
        {
            Console.Write(peer.Connection.Uri + ": " + GetString(buffer));
            _comunicationManager.Receive(4, peer.Connection.Endpoint);
        }

        public void OnPeerDataSent(Peer peer, byte[] data)
        {
            Console.Write(" (Sent)");
        }

        static string GetString(byte[] buffer)
        {
            return Encoding.ASCII.GetString(buffer);
        }

        static byte[] GetBytes(string str)
        {
            return Encoding.ASCII.GetBytes(str);
        }
    }
}