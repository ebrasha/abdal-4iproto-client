namespace Abdal_Security_Group_App
{
    partial class Main
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            visualStudio2022DarkTheme1 = new Telerik.WinControls.Themes.VisualStudio2022DarkTheme();
            desk_alert = new Telerik.WinControls.UI.RadDesktopAlert(components);
            bg_worker = new System.ComponentModel.BackgroundWorker();
            lbBytesSent = new Telerik.WinControls.UI.RadLabel();
            lbBytesReceived = new Telerik.WinControls.UI.RadLabel();
            listBoxLog = new ListBox();
            radButton1 = new Telerik.WinControls.UI.RadButton();
            windows11CompactDarkTheme1 = new Telerik.WinControls.Themes.Windows11CompactDarkTheme();
            lbConnectingTime = new Telerik.WinControls.UI.RadLabel();
            lbServerIP = new Telerik.WinControls.UI.RadLabel();
            btnConfigSettings = new PictureBox();
            imageListBtns = new ImageList(components);
            ConnectTunnelBtn = new PictureBox();
            btnClose = new PictureBox();
            btnMinimize = new PictureBox();
            bntMonitoringLog = new PictureBox();
            bntAboutUs = new PictureBox();
            btnGitHub = new PictureBox();
            pictureBox1 = new PictureBox();
            notifyIcon = new NotifyIcon(components);
            contextMenuStrip = new ContextMenuStrip(components);
            connectToolStripMenuItem = new ToolStripMenuItem();
            disconnectToolStripMenuItem = new ToolStripMenuItem();
            aboutUSToolStripMenuItem = new ToolStripMenuItem();
            exitToolStripMenuItem1 = new ToolStripMenuItem();
            btnDomainList = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)lbBytesSent).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lbBytesReceived).BeginInit();
            ((System.ComponentModel.ISupportInitialize)radButton1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lbConnectingTime).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lbServerIP).BeginInit();
            lbServerIP.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)btnConfigSettings).BeginInit();
            ((System.ComponentModel.ISupportInitialize)ConnectTunnelBtn).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnClose).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnMinimize).BeginInit();
            ((System.ComponentModel.ISupportInitialize)bntMonitoringLog).BeginInit();
            ((System.ComponentModel.ISupportInitialize)bntAboutUs).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnGitHub).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            contextMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)btnDomainList).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this).BeginInit();
            SuspendLayout();
            // 
            // bg_worker
            // 
            bg_worker.DoWork += bg_worker_DoWork;
            bg_worker.RunWorkerCompleted += bg_worker_RunWorkerCompleted;
            // 
            // lbBytesSent
            // 
            lbBytesSent.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lbBytesSent.Location = new Point(202, 486);
            lbBytesSent.Name = "lbBytesSent";
            lbBytesSent.Size = new Size(17, 25);
            lbBytesSent.TabIndex = 4;
            lbBytesSent.Text = "0";
            lbBytesSent.ThemeName = "VisualStudio2022Dark";
            // 
            // lbBytesReceived
            // 
            lbBytesReceived.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lbBytesReceived.Location = new Point(61, 486);
            lbBytesReceived.Name = "lbBytesReceived";
            lbBytesReceived.Size = new Size(17, 25);
            lbBytesReceived.TabIndex = 5;
            lbBytesReceived.Text = "0";
            lbBytesReceived.ThemeName = "VisualStudio2022Dark";
            // 
            // listBoxLog
            // 
            listBoxLog.BackColor = Color.FromArgb(20, 26, 60);
            listBoxLog.BorderStyle = BorderStyle.None;
            listBoxLog.ForeColor = SystemColors.MenuBar;
            listBoxLog.FormattingEnabled = true;
            listBoxLog.ItemHeight = 15;
            listBoxLog.Location = new Point(283, 558);
            listBoxLog.Name = "listBoxLog";
            listBoxLog.Size = new Size(35, 30);
            listBoxLog.TabIndex = 6;
            listBoxLog.Visible = false;
            // 
            // radButton1
            // 
            radButton1.Location = new Point(12, 431);
            radButton1.Name = "radButton1";
            radButton1.Size = new Size(0, 32);
            radButton1.TabIndex = 0;
            radButton1.Text = "radButton1";
            radButton1.ThemeName = "VisualStudio2022Dark";
            // 
            // lbConnectingTime
            // 
            lbConnectingTime.Font = new Font("Segoe UI", 41F, FontStyle.Bold);
            lbConnectingTime.Location = new Point(46, 115);
            lbConnectingTime.Name = "lbConnectingTime";
            lbConnectingTime.Size = new Size(246, 82);
            lbConnectingTime.TabIndex = 10;
            lbConnectingTime.Text = "00:00:00";
            lbConnectingTime.ThemeName = "VisualStudio2022Dark";
            // 
            // lbServerIP
            // 
            lbServerIP.Controls.Add(btnConfigSettings);
            lbServerIP.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lbServerIP.Location = new Point(94, 393);
            lbServerIP.Name = "lbServerIP";
            lbServerIP.Size = new Size(154, 33);
            lbServerIP.TabIndex = 11;
            lbServerIP.Text = "82.115.13.118";
            lbServerIP.ThemeName = "VisualStudio2022Dark";
            lbServerIP.Click += lbServerIP_Click;
            // 
            // btnConfigSettings
            // 
            btnConfigSettings.BackColor = Color.Transparent;
            btnConfigSettings.Cursor = Cursors.Hand;
            btnConfigSettings.Location = new Point(-51, -8);
            btnConfigSettings.Name = "btnConfigSettings";
            btnConfigSettings.Size = new Size(208, 61);
            btnConfigSettings.TabIndex = 20;
            btnConfigSettings.TabStop = false;
            btnConfigSettings.Click += btnConfigSettings_Click;
            // 
            // imageListBtns
            // 
            imageListBtns.ColorDepth = ColorDepth.Depth32Bit;
            imageListBtns.ImageStream = (ImageListStreamer)resources.GetObject("imageListBtns.ImageStream");
            imageListBtns.TransparentColor = Color.Transparent;
            imageListBtns.Images.SetKeyName(0, "danger-btn");
            imageListBtns.Images.SetKeyName(1, "suc-btn");
            imageListBtns.Images.SetKeyName(2, "warn-btn");
            imageListBtns.Images.SetKeyName(3, "prim-btn");
            // 
            // ConnectTunnelBtn
            // 
            ConnectTunnelBtn.BackColor = Color.Transparent;
            ConnectTunnelBtn.BackgroundImageLayout = ImageLayout.Stretch;
            ConnectTunnelBtn.Cursor = Cursors.Hand;
            ConnectTunnelBtn.Location = new Point(101, 213);
            ConnectTunnelBtn.Name = "ConnectTunnelBtn";
            ConnectTunnelBtn.Size = new Size(125, 125);
            ConnectTunnelBtn.SizeMode = PictureBoxSizeMode.StretchImage;
            ConnectTunnelBtn.TabIndex = 12;
            ConnectTunnelBtn.TabStop = false;
            ConnectTunnelBtn.Click += ConnectTunnelBtn_Click;
            // 
            // btnClose
            // 
            btnClose.BackColor = Color.Transparent;
            btnClose.Cursor = Cursors.Hand;
            btnClose.Location = new Point(18, 28);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(25, 25);
            btnClose.TabIndex = 14;
            btnClose.TabStop = false;
            btnClose.Click += pictureBox1_Click;
            // 
            // btnMinimize
            // 
            btnMinimize.BackColor = Color.Transparent;
            btnMinimize.Cursor = Cursors.Hand;
            btnMinimize.Location = new Point(52, 28);
            btnMinimize.Name = "btnMinimize";
            btnMinimize.Size = new Size(25, 25);
            btnMinimize.TabIndex = 15;
            btnMinimize.TabStop = false;
            btnMinimize.Click += pictureBox2_Click;
            // 
            // bntMonitoringLog
            // 
            bntMonitoringLog.BackColor = Color.Transparent;
            bntMonitoringLog.Cursor = Cursors.Hand;
            bntMonitoringLog.Location = new Point(183, 21);
            bntMonitoringLog.Name = "bntMonitoringLog";
            bntMonitoringLog.Size = new Size(38, 38);
            bntMonitoringLog.TabIndex = 16;
            bntMonitoringLog.TabStop = false;
            bntMonitoringLog.Click += bntMonitoringLog_Click;
            // 
            // bntAboutUs
            // 
            bntAboutUs.BackColor = Color.Transparent;
            bntAboutUs.Cursor = Cursors.Hand;
            bntAboutUs.Location = new Point(230, 21);
            bntAboutUs.Name = "bntAboutUs";
            bntAboutUs.Size = new Size(38, 38);
            bntAboutUs.TabIndex = 17;
            bntAboutUs.TabStop = false;
            bntAboutUs.Click += bntAboutUs_Click;
            // 
            // btnGitHub
            // 
            btnGitHub.BackColor = Color.Transparent;
            btnGitHub.Cursor = Cursors.Hand;
            btnGitHub.Location = new Point(276, 21);
            btnGitHub.Name = "btnGitHub";
            btnGitHub.Size = new Size(38, 38);
            btnGitHub.TabIndex = 18;
            btnGitHub.TabStop = false;
            btnGitHub.Click += btnGitHub_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.Transparent;
            pictureBox1.Cursor = Cursors.Hand;
            pictureBox1.Location = new Point(137, 21);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(38, 38);
            pictureBox1.TabIndex = 19;
            pictureBox1.TabStop = false;
            pictureBox1.Click += pictureBox1_Click_1;
            // 
            // notifyIcon
            // 
            notifyIcon.ContextMenuStrip = contextMenuStrip;
            notifyIcon.Icon = (Icon)resources.GetObject("notifyIcon.Icon");
            notifyIcon.Text = "Abdal 4iProto Client";
            notifyIcon.Visible = true;
            notifyIcon.MouseClick += notifyIcon1_MouseClick;
            // 
            // contextMenuStrip
            // 
            contextMenuStrip.Items.AddRange(new ToolStripItem[] { connectToolStripMenuItem, disconnectToolStripMenuItem, aboutUSToolStripMenuItem, exitToolStripMenuItem1 });
            contextMenuStrip.Name = "contextMenuStrip";
            contextMenuStrip.Size = new Size(134, 92);
            // 
            // connectToolStripMenuItem
            // 
            connectToolStripMenuItem.Name = "connectToolStripMenuItem";
            connectToolStripMenuItem.Size = new Size(133, 22);
            connectToolStripMenuItem.Text = "Connect";
            connectToolStripMenuItem.Click += connectToolStripMenuItem_Click;
            // 
            // disconnectToolStripMenuItem
            // 
            disconnectToolStripMenuItem.Name = "disconnectToolStripMenuItem";
            disconnectToolStripMenuItem.Size = new Size(133, 22);
            disconnectToolStripMenuItem.Text = "Disconnect";
            disconnectToolStripMenuItem.Click += disconnectToolStripMenuItem_Click;
            // 
            // aboutUSToolStripMenuItem
            // 
            aboutUSToolStripMenuItem.Name = "aboutUSToolStripMenuItem";
            aboutUSToolStripMenuItem.Size = new Size(133, 22);
            aboutUSToolStripMenuItem.Text = "About US";
            aboutUSToolStripMenuItem.Click += aboutUSToolStripMenuItem_Click;
            // 
            // exitToolStripMenuItem1
            // 
            exitToolStripMenuItem1.Name = "exitToolStripMenuItem1";
            exitToolStripMenuItem1.Size = new Size(133, 22);
            exitToolStripMenuItem1.Text = "Exit";
            exitToolStripMenuItem1.Click += exitToolStripMenuItem1_Click;
            // 
            // btnDomainList
            // 
            btnDomainList.BackColor = Color.Transparent;
            btnDomainList.Cursor = Cursors.Hand;
            btnDomainList.Location = new Point(261, 390);
            btnDomainList.Name = "btnDomainList";
            btnDomainList.Size = new Size(22, 24);
            btnDomainList.TabIndex = 20;
            btnDomainList.TabStop = false;
            btnDomainList.Click += btnDomainList_Click;
            // 
            // Main
            // 
            AutoScaleBaseSize = new Size(7, 15);
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = Properties.Resources.UI_none22;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(330, 652);
            Controls.Add(btnDomainList);
            Controls.Add(pictureBox1);
            Controls.Add(btnGitHub);
            Controls.Add(bntAboutUs);
            Controls.Add(bntMonitoringLog);
            Controls.Add(btnMinimize);
            Controls.Add(btnClose);
            Controls.Add(lbServerIP);
            Controls.Add(lbConnectingTime);
            Controls.Add(radButton1);
            Controls.Add(listBoxLog);
            Controls.Add(lbBytesReceived);
            Controls.Add(lbBytesSent);
            Controls.Add(ConnectTunnelBtn);
            FormBorderStyle = FormBorderStyle.None;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Main";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Form1";
            ThemeName = "VisualStudio2022Dark";
            FormClosing += Main_FormClosing;
            Load += Main_Load;
            ((System.ComponentModel.ISupportInitialize)lbBytesSent).EndInit();
            ((System.ComponentModel.ISupportInitialize)lbBytesReceived).EndInit();
            ((System.ComponentModel.ISupportInitialize)radButton1).EndInit();
            ((System.ComponentModel.ISupportInitialize)lbConnectingTime).EndInit();
            ((System.ComponentModel.ISupportInitialize)lbServerIP).EndInit();
            lbServerIP.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)btnConfigSettings).EndInit();
            ((System.ComponentModel.ISupportInitialize)ConnectTunnelBtn).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnClose).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnMinimize).EndInit();
            ((System.ComponentModel.ISupportInitialize)bntMonitoringLog).EndInit();
            ((System.ComponentModel.ISupportInitialize)bntAboutUs).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnGitHub).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            contextMenuStrip.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)btnDomainList).EndInit();
            ((System.ComponentModel.ISupportInitialize)this).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Telerik.WinControls.Themes.VisualStudio2022DarkTheme visualStudio2022DarkTheme1;
        private Telerik.WinControls.UI.RadDesktopAlert desk_alert;
        private System.ComponentModel.BackgroundWorker bg_worker;
        public Telerik.WinControls.UI.RadLabel lbBytesSent;
        public Telerik.WinControls.UI.RadLabel lbBytesReceived;
        public ListBox listBoxLog;
        private Telerik.WinControls.UI.RadButton radButton1;
        private Telerik.WinControls.Themes.Windows11CompactDarkTheme windows11CompactDarkTheme1;
        private Telerik.WinControls.UI.RadLabel lbConnectingTime;
        public ImageList imageListBtns;
        public PictureBox ConnectTunnelBtn;
        private PictureBox btnClose;
        private PictureBox btnMinimize;
        private PictureBox bntMonitoringLog;
        private PictureBox bntAboutUs;
        private PictureBox btnGitHub;
        private PictureBox pictureBox1;
        private NotifyIcon notifyIcon;
        private ContextMenuStrip contextMenuStrip;
        private ToolStripMenuItem connectToolStripMenuItem;
        private ToolStripMenuItem disconnectToolStripMenuItem;
        private ToolStripMenuItem aboutUSToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem1;
        private PictureBox btnConfigSettings;
        private PictureBox btnDomainList;
        public Telerik.WinControls.UI.RadLabel lbServerIP;
    }
}
