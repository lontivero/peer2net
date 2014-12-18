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
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Open.P2P.EventArgs;
using Open.P2P.IO;
using Open.P2P.Listeners;
using Open.P2P.Streams.Readers;
using TcpListener = Open.P2P.Listeners.TcpListener;

namespace Open.P2P.ChatExample
{
    public partial class MainWindow : Window
    {
        private readonly CommunicationManager _comunicationManager;
        private readonly TcpListener _listener;
        private readonly Dictionary<IPEndPoint, Tuple<Peer, PascalStreamReader>> _sessions;
        private readonly UdpListener _discovery;
        private readonly int _port;
        private readonly Guid _id;

        public MainWindow()
        {
            InitializeComponent();
            var r = new Random();
            _port = r.Next(1500, 2000);
            _id = Guid.NewGuid();

            _sessions = new Dictionary<IPEndPoint, Tuple<Peer, PascalStreamReader>>();
            _listener = new TcpListener(_port);
            _comunicationManager = new CommunicationManager(_listener);
            _comunicationManager.ConnectionClosed += ChatOnMemberDisconnected;
            _comunicationManager.PeerConnected += ChatOnMemberConnected;

            _listener.Start();

            _discovery = new UdpListener(3000);
            _discovery.UdpPacketReceived += DiscoveryOnUdpPacketReceived;
            _discovery.Start();

            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                socket.EnableBroadcast = true;
                var group = new IPEndPoint(IPAddress.Broadcast, 3000);
                var hi = Encoding.ASCII.GetBytes("Hi Peer2Net node here:" + _id + ":127.0.0.1:" + _port);
                socket.SendTo(hi, group);
                socket.Close();
            }
        }

        private async void DiscoveryOnUdpPacketReceived(object sender, UdpPacketReceivedEventArgs args)
        {
            var msg = Encoding.ASCII.GetString(args.Data);
            var msgArr = msg.Split(':');
            var remoteId = Guid.Parse(msgArr[1]);
            if(_id == remoteId) return;

            var remoteIP = IPAddress.Parse(msgArr[2]);
            var remoteHost = int.Parse(msgArr[3]);
            var remoteEndpoint = new IPEndPoint(remoteIP, remoteHost);
            await _comunicationManager.ConnectAsync(remoteEndpoint);
        }


        private void ChatOnMemberDisconnected(object sender, ConnectionEventArgs e)
        {
            Display(e.EndPoint + " left the room");
        }

        private async void ChatOnMemberConnected(object sender, PeerEventArgs e)
        {
            var sr = new PascalStreamReader(e.Peer.Stream);
            _sessions.Add(e.Peer.EndPoint, new Tuple<Peer, PascalStreamReader>(e.Peer, sr));
            Display(e.Peer.EndPoint + " has joined");
            byte[] bytes;
            while (true)
            {
                bytes = await sr.ReadBytesAsync();
                Display(e.Peer.EndPoint + " say:");
                Display("  " + GetString(bytes));
            }
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

        public async void SendMessage(string message)
        {
            var msg = GetBytes(message);
            var buf = PascalStreamReader.FormatMessage(msg);
            await _comunicationManager.SendAsync(buf, 0, buf.Length, _sessions.Keys);
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
