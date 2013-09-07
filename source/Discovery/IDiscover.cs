using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml.Linq;
using System.Linq;

namespace Peer2Net
{
	internal interface IDiscover
	{
		void StartDiscovering(); 
	}

	class UpdDiscovery : IDiscover
	{
        private readonly Guid _id = Guid.NewGuid();
		private readonly UdpListener _listener;

		public UpdDiscovery (UdpListener listener)
		{
			_listener = listener;
			_listener.UdpPacketReceived += HandleUdpPacketReceived;
		}

		private void HandleUdpPacketReceived (object sender, UdpPacketReceivedEventArgs e)
		{
			var data = Encoding.ASCII.GetString (e.Data);
			var message = XElement.Parse (data);
			var root = message.Element (XName.Get ("Peer2Net"));
			var messageType = root.Element (XName.Get ("MessageType")).Value;

			if (messageType == "Hello") {
				ResponseHello (e.EndPoint);
			} else {
				RaisePeerDiscovered(e.EndPoint);
			}
		}

		public void StartDiscovering ()
		{
			_listener.Start();

			SayHello();
		}

        public void SayHello ()
		{
			var group = new IPEndPoint (IPAddress.Broadcast, _listener.Port);
			var hi = new XElement ("Peer2Net",
						new XElement ("MessageType", "Hello"), 
						new XElement ("UUID", _id),
						new XElement ("Application", "peer2net"),
						new XElement ("Version", 0));

			using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)) {
				socket.EnableBroadcast = true; 
				socket.SendTo (Encoding.ASCII.GetBytes (hi.ToString()), group);
			}
        }

		void ResponseHello (IPEndPoint remoteEndPoint)
		{
			var hi = new XElement ("Peer2Net",
						new XElement ("MessageType", "Hi"), 
						new XElement ("UUID", _id),
						new XElement ("IPAddress", GetLocalIPAddress()),
						new XElement ("Port", 23));

			using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)) {
				socket.SendTo (Encoding.ASCII.GetBytes (hi.ToString ()), remoteEndPoint);
			}			
		}

		private void RaisePeerDiscovered (IPEndPoint endPoint)
		{
			throw new NotImplementedException ();
		}

		private static IPAddress GetLocalIPAddress ()
		{
			var host = Dns.GetHostEntry (Dns.GetHostName ());
			return host.AddressList.FirstOrDefault(ip=> ip.AddressFamily == AddressFamily.InterNetwork);
		}
	}
}

