using System;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace AsyncTcpServer
{
    class TcpServer
    {
        const int MaxClientNumber = 10;

        public AsyncCallback PfnWorkerCallBack;
        private Socket _serverSocket;
        private readonly Socket[] _workerSocket;
        private int _clientCount;
        private readonly IPAddress _host;
        private readonly int _port;

        public event EventHandler<DataArriveEventArg> DataArriveEvent;
        public event EventHandler<EventArgs> ClientConnectEvent;

        public TcpServer()
            : this(IPAddress.Loopback, 1080)
        {
        }

        public TcpServer(IPAddress host, int port)
        {
            _host = host;
            _port = port;
            _clientCount = 0;
            _workerSocket = new Socket[MaxClientNumber];
        }

        public void Start()
        {
            try
            {
                _serverSocket = new Socket(AddressFamily.InterNetwork,
                                          SocketType.Stream,
                                          ProtocolType.Tcp);
                var ipLocal = new IPEndPoint(_host, _port);

                _serverSocket.Bind(ipLocal);
                _serverSocket.Listen(4);
                _serverSocket.BeginAccept(OnClientConnect, null);
            }
            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
            }
        }

        public void Stop()
        {
            if (_serverSocket != null)
                _serverSocket.Close();

            foreach (var socket in _workerSocket)
            {
                if (socket != null) socket.Close();
            }
        }

        public static IPAddress GetIp()
        {
            var strHostName = Dns.GetHostName();
            var iphostentry = Dns.GetHostEntry(strHostName);

            return iphostentry.AddressList.Length == 1 
                ? iphostentry.AddressList[0] 
                : new IPAddress(new byte[]{127,0,0,1});
        }

        private void OnClientConnect(IAsyncResult asyn)
        {
            try
            {
                _workerSocket[_clientCount] = _serverSocket.EndAccept(asyn);
                WaitForData(_workerSocket[_clientCount++]);

                if (ClientConnectEvent != null)
                    ClientConnectEvent(this, new EventArgs());

                _serverSocket.BeginAccept(OnClientConnect, null);
            }
            catch (ObjectDisposedException)
            {
                Debugger.Log(0, "1", "\n OnClientConnection: Socket has been closed\n");
            }
            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
            }

        }

        private void WaitForData(Socket socket)
        {
            try
            {
                if (PfnWorkerCallBack == null)
                {
                    // Specify the call back function which is to be 
                    // invoked when there is any write activity by the 
                    // connected client
                    PfnWorkerCallBack = OnDataReceived;
                }

                var packet = new SocketPacket {CurrentSocket = socket};
                // Start receiving any data written by the connected client
                // asynchronously
                socket.BeginReceive(packet.DataBuffer, 0,
                                   packet.DataBuffer.Length,
                                   SocketFlags.None,
                                   PfnWorkerCallBack,
                                   packet);
            }
            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
            }

        }

        private void OnDataReceived(IAsyncResult asyn)
        {
            try
            {
                var asyncState = (SocketPacket)asyn.AsyncState;
                var buffSize = asyncState.CurrentSocket.EndReceive(asyn);

                var chars = new char[buffSize + 1];
                var decode = System.Text.Encoding.UTF8.GetDecoder();
                var charLen = decode.GetChars(asyncState.DataBuffer, 0, buffSize, chars, 0);
                var data = new String(chars);

                if(DataArriveEvent != null)
                    DataArriveEvent(this, new DataArriveEventArg(data)); 

                // Continue the waiting for data on the Socket
                WaitForData(asyncState.CurrentSocket);
            }
            catch (ObjectDisposedException)
            {
                Debugger.Log(0, "1", "\nOnDataReceived: Socket has been closed\n");
            }
            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
            }
        }

#if false
        private void ButtonSendMsgClick(object sender, System.EventArgs e)
        {
            try
            {
                Object objData = rtbSendMsg.Text;
                byte[] byData = System.Text.Encoding.ASCII.GetBytes(objData.ToString());
                for (int i = 0; i < clientCount; i++)
                {
                    if (workerSocket[i] != null)
                    {
                        if (workerSocket[i].Connected)
                        {
                            workerSocket[i].Send(byData);
                        }
                    }
                }

            }
            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
            }
        }
#endif 


    }
}