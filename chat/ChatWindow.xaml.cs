//
// - ChatWindow.xaml.cs
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
using System.Windows;
using System.Windows.Input;
using Peer2Net.EventArgs;
using Peer2Net.MessageHandlers;

namespace Peer2Net.Chat
{
    public partial class MainWindow : Window
    {
        private readonly CommunicationManager _comunicationManager;
        private readonly Listener _listener;
        private readonly Dictionary<IPEndPoint, Tuple<Peer, PascalMessageHandler>> _sessions;
        private readonly UdpListener _discovery;

        public MainWindow()
        {
            InitializeComponent();
            var r = new Random();
            var port = r.Next(1500, 2000);

            _sessions = new Dictionary<IPEndPoint, Tuple<Peer, PascalMessageHandler>>();
            _listener = new Listener(port);
            _comunicationManager = new CommunicationManager(_listener);
            _comunicationManager.ConnectionClosed += ChatOnMemberDisconnected;
            _comunicationManager.PeerConnected += ChatOnMemberConnected;
            _comunicationManager.ConnectionFailed += ChatOnMemberConnectionFailure;
            _comunicationManager.PeerDataReceived += OnPeerDataReceived;

            _listener.Start();

            _discovery = new UdpListener(3000);
            _discovery.DiscoveredNode += (sender, args) => _comunicationManager.Connect(args.DiscoveredEndPoint);
            _discovery.Start();
            _discovery.SayHello(port);
        }

        private void OnPeerDataReceived(object sender, PeerDataEventArgs e)
        {
            var session = _sessions[e.Peer.EndPoint];
            var messageHandler = session.Item2;
            messageHandler.ProcessIncomingData(e.Data);
            _comunicationManager.Receive(messageHandler.PendingBytes, e.Peer.EndPoint);
        }

        private void ChatOnMemberConnectionFailure(object sender, ConnectionEventArgs e)
        {
            Display(e.EndPoint + " not found");
        }

        private void ChatOnMemberDisconnected(object sender, ConnectionEventArgs e)
        {
            Display(e.EndPoint + " left the room");
        }

        private void ChatOnMemberConnected(object sender, PeerEventArgs e)
        {
            var messageHandler = new PascalMessageHandler();
            messageHandler.MessageReceived += (o, ea) => NotifyMessageReceived(e.Peer, ea);

            _sessions.Add(e.Peer.EndPoint, new Tuple<Peer, PascalMessageHandler>(e.Peer, messageHandler));
            _comunicationManager.Receive(4, e.Peer.EndPoint);

            Display(e.Peer.EndPoint + " has joined");
        }

        private void NotifyMessageReceived(Peer peer, MessageReceivedEventArgs e)
        {
            Display(peer.EndPoint + " say:");
            Display("  " + GetString(e.Data));
        }

        private void Display(string str)
        {
            Dispatcher.Invoke((Action)(() => textBoxChatPane.Text +=str + "\n"));
        }

        private void textBoxEntryField_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Enter)
            {
                var text = textBoxEntryField.Text;
                if(text == ":q") Application.Current.Shutdown();;
                Display("Me:");
                Display("  " + text);
                SendMessage(text);
                textBoxEntryField.Clear();
            }
        }

        public void SendMessage(string message)
        {
            var msg = GetBytes(message);

            _comunicationManager.Send(PascalMessageHandler.FormatMessage(msg), _sessions.Keys);
        }

        static byte[] GetBytes(string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

        static string GetString(byte[] buffer)
        {
            return Encoding.UTF8.GetString(buffer);
        }
    }
}
