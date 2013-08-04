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
using P2PNet.MessageHandlers;

namespace P2PNet.NodeConsole
{
    class ChatSession
    {
        private readonly PascalMessageHandler _messageHandler;
        private readonly Peer _peer;
        private readonly ComunicationManager _comunicationManager;

        public ChatSession(Peer peer, ComunicationManager comunicationManager)
        {
            _comunicationManager = comunicationManager;
            _peer = peer;
            _messageHandler = new PascalMessageHandler();
            _messageHandler.MessageReceived += OnMessageReceived;
            _comunicationManager.Receive(4, _peer.EndPoint);
        }

        private void OnMessageReceived(object sender, MessageReceivedEventArgs packetReceivedEventArgs)
        {
            Console.WriteLine("{0} wrote:", _peer.Uri);
            Console.WriteLine("\t{0}\n", GetString(packetReceivedEventArgs.Packet));
        }

        public void ProcessInput(byte[] data)
        {
            _messageHandler.ProcessIncomingData(data);
            _comunicationManager.Receive(_messageHandler.PendingBytes, _peer.EndPoint);
        }

        static string GetString(byte[] buffer)
        {
            return Encoding.UTF8.GetString(buffer);
        }
    }


    class P2PConsole : ClientManager
    {
        private readonly Settings _settings;
        private readonly ComunicationManager _comunicationManager;
        private readonly Listener _listener;
        private readonly Dictionary<IPEndPoint, ChatSession> _sessions;
 

        public P2PConsole(Settings settings)
        {
            _sessions = new Dictionary<IPEndPoint, ChatSession>();
            _settings = settings;
            _listener = new Listener(settings.Port);
            _comunicationManager = new ComunicationManager(_listener, this);
        }

        public void Start()
        {
            _listener.Start();
            Wellcome();

            var line = ReadCommand();
            while (line != ":q")
            {
                if(line.StartsWith(":c"))
                {
                    ConnectTo(line.Substring(2));
                }
                else
                {
                    SendMessage(line);                    
                }
                line = ReadCommand();
            }

            Stop();
        }

        private void Wellcome()
        {
            Console.WriteLine("P2PNet chat in port: " + _settings.Port);
            Console.WriteLine("Press :q to exit");
            Console.WriteLine("");
        }

        private static string ReadCommand()
        {
            var line = Console.ReadLine();
            return line;
        }

        private void Stop()
        {
            _listener.Stop();
        }

        private void SendMessage(string message)
        {
            var msg = GetBytes(message);

            _comunicationManager.Send(PascalMessageHandler.FormatMessage(msg), _sessions.Keys);
        }

        private void ConnectTo(string node)
        {
            var nodeParts = node.Split(':');
            var ip = IPAddress.Parse(nodeParts[0]);
            var port = int.Parse(nodeParts[1]);
            _comunicationManager.Connect(new IPEndPoint(ip, port) );
        }

        public override void Connected(Peer peer)
        {
            var session = new ChatSession(peer, _comunicationManager);
            _sessions.Add(peer.EndPoint, session);
            Console.WriteLine("{0} connected", peer.EndPoint );
        }

        public override void Closed(Peer peer)
        {
            _sessions.Remove(peer.EndPoint);
            Console.WriteLine("{0} disconnected!", peer.EndPoint);
        }

        public override void DataReceived(Peer peer, byte[] data)
        {
            var session = _sessions[peer.EndPoint];
            session.ProcessInput(data);
        }

        public override void DataSent(Peer peer, byte[] data)
        {
        }

        static byte[] GetBytes(string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }
    }
}