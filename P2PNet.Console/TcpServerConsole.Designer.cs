using System.Windows.Forms;

namespace P2PNet.Console
{
    public partial class TcpServerConsole
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Forms Designer generated code
        /// <summary>
        /// This method is required for Windows Forms designer support.
        /// Do not change the method contents inside the source code editor. The Forms designer might
        /// not be able to load this method if it was changed manually.
        /// </summary>
        private void InitializeComponent()
        {
            this.statusBar = new System.Windows.Forms.StatusStrip();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtIP = new System.Windows.Forms.TextBox();
            this.btnStopListen = new System.Windows.Forms.Button();
            this.btnStartListen = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtPortNumber = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.rtbSendMsg = new System.Windows.Forms.RichTextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.rtbReceivedMsg = new System.Windows.Forms.RichTextBox();
            this.btnSendMsg = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusBar
            // 
            this.statusBar.Location = new System.Drawing.Point(0, 376);
            this.statusBar.Name = "statusBar";
            this.statusBar.Size = new System.Drawing.Size(434, 22);
            this.statusBar.TabIndex = 15;
            this.statusBar.Text = "statusBar";
            // 
            // toolStrip1
            // 
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(434, 25);
            this.toolStrip1.TabIndex = 16;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtIP);
            this.groupBox1.Controls.Add(this.btnStopListen);
            this.groupBox1.Controls.Add(this.btnStartListen);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.txtPortNumber);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(0, 25);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(434, 45);
            this.groupBox1.TabIndex = 17;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Server settings";
            // 
            // txtIP
            // 
            this.txtIP.Location = new System.Drawing.Point(81, 16);
            this.txtIP.Name = "txtIP";
            this.txtIP.ReadOnly = true;
            this.txtIP.Size = new System.Drawing.Size(120, 20);
            this.txtIP.TabIndex = 18;
            // 
            // btnStopListen
            // 
            this.btnStopListen.BackColor = System.Drawing.SystemColors.Control;
            this.btnStopListen.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStopListen.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnStopListen.Location = new System.Drawing.Point(356, 16);
            this.btnStopListen.Name = "btnStopListen";
            this.btnStopListen.Size = new System.Drawing.Size(66, 20);
            this.btnStopListen.TabIndex = 17;
            this.btnStopListen.Text = "Stop";
            this.btnStopListen.UseVisualStyleBackColor = false;
            this.btnStopListen.Click += new System.EventHandler(this.ButtonStopListenClick);
            // 
            // btnStartListen
            // 
            this.btnStartListen.BackColor = System.Drawing.SystemColors.Control;
            this.btnStartListen.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStartListen.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnStartListen.Location = new System.Drawing.Point(282, 16);
            this.btnStartListen.Name = "btnStartListen";
            this.btnStartListen.Size = new System.Drawing.Size(68, 20);
            this.btnStartListen.TabIndex = 16;
            this.btnStartListen.Text = "Start";
            this.btnStartListen.UseVisualStyleBackColor = false;
            this.btnStartListen.Click += new System.EventHandler(this.ButtonStartListenClick);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 13);
            this.label2.TabIndex = 15;
            this.label2.Text = "IP Address:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(207, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(10, 13);
            this.label1.TabIndex = 14;
            this.label1.Text = ":";
            // 
            // txtPortNumber
            // 
            this.txtPortNumber.Location = new System.Drawing.Point(223, 16);
            this.txtPortNumber.Name = "txtPortNumber";
            this.txtPortNumber.Size = new System.Drawing.Size(40, 20);
            this.txtPortNumber.TabIndex = 13;
            this.txtPortNumber.Text = "8000";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.splitContainer1);
            this.groupBox2.Controls.Add(this.btnSendMsg);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(0, 70);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(434, 306);
            this.groupBox2.TabIndex = 18;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Traffic Viewer";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 16);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.rtbSendMsg);
            this.splitContainer1.Panel1.Controls.Add(this.label4);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.label5);
            this.splitContainer1.Panel2.Controls.Add(this.rtbReceivedMsg);
            this.splitContainer1.Size = new System.Drawing.Size(428, 287);
            this.splitContainer1.SplitterDistance = 66;
            this.splitContainer1.TabIndex = 16;
            // 
            // rtbSendMsg
            // 
            this.rtbSendMsg.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbSendMsg.Location = new System.Drawing.Point(3, 16);
            this.rtbSendMsg.Name = "rtbSendMsg";
            this.rtbSendMsg.Size = new System.Drawing.Size(422, 47);
            this.rtbSendMsg.TabIndex = 15;
            this.rtbSendMsg.Text = "";
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(3, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(192, 16);
            this.label4.TabIndex = 14;
            this.label4.Text = "Broadcast Message To Clients";
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(3, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(192, 16);
            this.label5.TabIndex = 20;
            this.label5.Text = "Message Received From Clients";
            // 
            // rtbReceivedMsg
            // 
            this.rtbReceivedMsg.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbReceivedMsg.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.rtbReceivedMsg.Location = new System.Drawing.Point(0, 19);
            this.rtbReceivedMsg.Name = "rtbReceivedMsg";
            this.rtbReceivedMsg.ReadOnly = true;
            this.rtbReceivedMsg.Size = new System.Drawing.Size(425, 198);
            this.rtbReceivedMsg.TabIndex = 19;
            this.rtbReceivedMsg.Text = "";
            // 
            // btnSendMsg
            // 
            this.btnSendMsg.Location = new System.Drawing.Point(96, 233);
            this.btnSendMsg.Name = "btnSendMsg";
            this.btnSendMsg.Size = new System.Drawing.Size(192, 24);
            this.btnSendMsg.TabIndex = 12;
            this.btnSendMsg.Text = "Send Message";
            // 
            // TcpServerConsole
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(434, 398);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.statusBar);
            this.MinimumSize = new System.Drawing.Size(442, 425);
            this.Name = "TcpServerConsole";
            this.Text = "Tcp Server for WCF Mock Testing";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SocketServerFormFormClosed);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SocketServerFormFormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private StatusStrip statusBar;
        private ToolStrip toolStrip1;
        private GroupBox groupBox1;
        private TextBox txtIP;
        private Button btnStopListen;
        private Button btnStartListen;
        private Label label2;
        private Label label1;
        private TextBox txtPortNumber;
        private GroupBox groupBox2;
        private SplitContainer splitContainer1;
        private Button btnSendMsg;
        private RichTextBox rtbSendMsg;
        private Label label4;
        private Label label5;
        private RichTextBox rtbReceivedMsg;
    }
}

