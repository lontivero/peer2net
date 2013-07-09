using System;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace AsyncTcpServer
{
    public partial class TcpServerConsole : Form
    {
        private TcpServer _server;

        public TcpServerConsole()
        {
            InitializeComponent();
            txtIP.Text = TcpServer.GetIp().ToString();
        }

        private void ButtonStartListenClick(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtPortNumber.Text))
                {
                    MessageBox.Show("Please enter a Port Number");
                    return;
                }

                var portStr = txtPortNumber.Text;
                var ip = IPAddress.Parse(txtIP.Text);
                var port = Convert.ToInt32(portStr);

                _server = new TcpServer(ip, port);

                _server.ClientConnectEvent += OnClientConnect;
                _server.DataArriveEvent += OnDataReceived;
                _server.Start(); 

                UpdateControls(true);

            }
            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
            }
        }

        private void UpdateControls(bool listening)
        {
            btnStartListen.Enabled = !listening;
            btnStopListen.Enabled = listening;
        }

        private void OnClientConnect(object sender, EventArgs e)
        {
            SetText(statusBar, "A new client is connected");
        }

        private void OnDataReceived(object sender, DataArriveEventArg e)
        {
            AppendText(rtbReceivedMsg, e.Data);
        }

        private void ButtonSendMsgClick(object sender, EventArgs e)
        {
#if false
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
#endif
        }

        private void ButtonStopListenClick(object sender, EventArgs e)
        {
            _server.Stop();
            UpdateControls(false);
        }

        private delegate void SetTextCallback(StatusStrip st, string text);

        private void SetText(StatusStrip st, string text)
        {
            if (st.InvokeRequired)
            {
                var d = new SetTextCallback(SetText);
                Invoke(d, new object[] { st, text });
            }
            else
            {
                st.Text = text;
            }
        }

        private delegate void AppendTextCallback(TextBoxBase textBox, string text);

        private void AppendText(TextBoxBase textBox, string text)
        {
            if (textBox.InvokeRequired)
            {
                var d = new AppendTextCallback(AppendText);
                Invoke(d, new object[] { textBox, text });
            }
            else
            {
                textBox.AppendText(text);
            }
        }

        private void SocketServerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            var r = MessageBox.Show("Are you sure?", "Confirmation", MessageBoxButtons.YesNo);
            e.Cancel = r == DialogResult.No;
        }

        private void SocketServerForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _server.Stop();
        }
    }
}