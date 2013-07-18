using System;
using System.Windows.Forms;
using System.Net.Sockets;
using P2PNet.Protocols;

namespace P2PNet.Console
{
    public partial class TcpServerConsole : Form
    {
        private ConnectionsManager _man;
        private Listener _listener;

        public TcpServerConsole()
        {
            InitializeComponent();
            txtIP.Text = "127.0.0.1";
        }

        private void ButtonStartListenClick(object sender, System.EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtPortNumber.Text))
                {
                    MessageBox.Show("Please enter a Port Number");
                    return;
                }

                var portStr = txtPortNumber.Text;
                var port = Convert.ToInt32(portStr);

                _listener = new Listener(port);
                _listener.Start();
                _man = new ConnectionsManager(_listener);
                _man.MessageReceived += ManOnPacketReceived;
                UpdateControls(true);

            }
            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
            }
        }

        private void ManOnPacketReceived(object sender, PacketReceivedEventArgs e)
        {
            AppendText(rtbReceivedMsg, GetString(e.Packet));
        }

        private void UpdateControls(bool listening)
        {
            btnStartListen.Enabled = !listening;
            btnStopListen.Enabled = listening;
        }

        private void ButtonSendMsgClick(object sender, System.EventArgs e)
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

        private void ButtonStopListenClick(object sender, System.EventArgs e)
        {
            _listener.Stop();
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

        private void SocketServerFormFormClosing(object sender, FormClosingEventArgs e)
        {
            var r = MessageBox.Show("Are you sure?", "Confirmation", MessageBoxButtons.YesNo);
            e.Cancel = r == DialogResult.No;
        }

        private void SocketServerFormFormClosed(object sender, FormClosedEventArgs e)
        {
            _listener.Stop();
        }

        static string GetString(byte[] bytes)
        {
            var chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }
    }
}