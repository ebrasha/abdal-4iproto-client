using Abdal_Security_Group_App.Core;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using Telerik.WinControls.UI;

namespace Abdal_Security_Group_App
{
    public partial class Main : Telerik.WinControls.UI.RadForm
    {
        private bool stop_op_status = false;
        private string abdal_app_name = Assembly.GetExecutingAssembly().GetName().ToString().Split(',')[0];
        private AbdalSoundPlayer ab_player = new AbdalSoundPlayer();

        private string abdal_app_name_for_url = Assembly.GetExecutingAssembly().GetName().ToString().Split(',')[0]
            .ToLower().Replace(' ', '-');

        private ProtoMonitor monitor;
        private Monitor monitorForm;
        private DomainExclude domainExcludeForm;
        private Donation donationForm;
        private ConfigMng ConfigSetForm;
        public string ConnectionStatus = "";

        public DateTime startTimeForConnectingTime;
        private Stopwatch stopwatchConnectingTime = new Stopwatch();
        private bool isTimerRunning = false;

        public Main()
        {
            InitializeComponent();
            //change form title
            Version version = Assembly.GetExecutingAssembly().GetName().Version!;
            Text = abdal_app_name + " " + version.Major + "." + version.Minor;

            // Call Global Chilkat Unlock
            ChilkatMng.UnlockChilkat();
        }

        #region Dragable Form Start

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_NCHITTEST)
                m.Result = (IntPtr)(HT_CAPTION);
        }

        private const int WM_NCHITTEST = 0x84;
        private const int HT_CLIENT = 0x1;
        private const int HT_CAPTION = 0x2;

        #endregion

        private async void Main_Load(object sender, EventArgs e)
        {


            desk_alert.ThemeName = "VisualStudio2022Dark";
            ConnectTunnelBtn.BackgroundImage = imageListBtns.Images["prim-btn"];
            ConnectionStatus = "disconnect";

            Config.CreateIfNotExists();
            Config.EnsureDomainFileExists();

            var configMap = Config.LoadAsDictionary();
            if (configMap != null && configMap.ContainsKey("ssh_host"))
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() =>
                    {
                        lbServerIP.Text = configMap["ssh_host"];
                    }));
                }
                else
                {
                    lbServerIP.Text = configMap["ssh_host"];
                }
            }
            else
            {
                lbServerIP.Text = "N/A";
            }



            monitorForm = new Monitor();
            domainExcludeForm = new DomainExclude();
            donationForm = new Donation();
            ConfigSetForm = new ConfigMng();
            ConfigSetForm.SetMainForm(this);


            monitor = new ProtoMonitor(this, monitorForm);
            ProtoMonitor.ProcessKiller("A4iProtoC");


            try { await UpdateChecker.CheckForUpdateAsync(); } catch { }


        }

        private void menuItem_github_Click(object sender, EventArgs e)
        {

        }

        private void menuItem_gitlab_Click(object sender, EventArgs e)
        {
            ab_player.sPlayer("checkbox");
            Process.Start(new ProcessStartInfo("https://gitlab.com/Prof.Shafiei/" + abdal_app_name_for_url) { UseShellExecute = true });
        }

        private void menuItem_donate_Click(object sender, EventArgs e)
        {

        }

        private void menuItem_about_us_Click(object sender, EventArgs e)
        {

        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            ab_player.sPlayer("checkbox");
            Process.GetCurrentProcess().Kill();
            Environment.Exit(0);
        }

        private void bg_worker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == true)
            {
                this.desk_alert.CaptionText = abdal_app_name;
                this.desk_alert.ContentText = "Canceled Process By User!";
                this.desk_alert.Show();
                ab_player.sPlayer("cancel");
            }
            else if (e.Error != null)
            {
                this.desk_alert.CaptionText = abdal_app_name;
                this.desk_alert.ContentText = e.Error.Message;
                this.desk_alert.Show();


                ab_player.sPlayer("error");
            }
            else
            {
                this.desk_alert.CaptionText = abdal_app_name;
                this.desk_alert.ContentText = "Done!";
                this.desk_alert.Show();

                if (stop_op_status)
                {
                    ab_player.sPlayerSync("cancel");
                }
                else
                {
                    ab_player.sPlayerSync("op-com");
                }

                ab_player.sPlayer("done");
            }
        }

        private void btn_start_Click(object sender, EventArgs e)
        {

        }

        private void btn_exit_Click(object sender, EventArgs e)
        {

        }

        private void btn_minimize_Click(object sender, EventArgs e)
        {

        }

        private void irDonationBtn_Click(object sender, EventArgs e)
        {

        }

        private void EnDonationBtn_Click(object sender, EventArgs e)
        {

        }



        /// <summary>
        /// Starts a real-time async timer and updates lbConnectingTime every second.
        /// </summary>
        public async void StartConnectingTimerAsync()
        {
            if (isTimerRunning) return; // جلوگیری از اجرای همزمان چندتایی

            isTimerRunning = true;
            stopwatchConnectingTime.Reset();
            stopwatchConnectingTime.Start();

            while (isTimerRunning)
            {
                TimeSpan elapsed = stopwatchConnectingTime.Elapsed;

                string timeText = elapsed.ToString(@"hh\:mm\:ss");

                // چون در async هستیم، مطمئن می‌شیم که در UI thread اجرا بشه
                if (lbConnectingTime.InvokeRequired)
                {
                    lbConnectingTime.Invoke(() => lbConnectingTime.Text = timeText);
                }
                else
                {
                    lbConnectingTime.Text = timeText;
                }

                await Task.Delay(1000); // صبر کن ۱ ثانیه بدون بلاک کردن ترد UI
            }
        }


        public void StopConnectingTimer()
        {
            isTimerRunning = false;
            stopwatchConnectingTime.Stop();
            lbConnectingTime.Text = "00:00:00";
        }

        private void ConnectTunnelBtn_Click(object sender, EventArgs e)
        {
            if (monitor == null)
            {
                MessageBox.Show("Error: Monitor object not initialized.");
                return;
            }

            if (ConnectionStatus == "disconnect")
            {

                ConnectTunnelBtn.BackgroundImage = imageListBtns.Images["warn-btn"]; // warn
                ConnectionStatus = "connecting";
                ab_player.sPlayer("start");
                monitor.Start();

            }
            else if (ConnectionStatus == "connected")
            {
                monitor?.Stop();
                ConnectTunnelBtn.BackgroundImage = imageListBtns.Images["prim-btn"]; // blue
                ConnectionStatus = "disconnect";
                StopConnectingTimer();
                ab_player.sPlayer("cancel");


            }
            else if (ConnectionStatus == "connection_error")
            {
                ab_player.sPlayer("error");
                monitor?.Stop();
                ConnectTunnelBtn.BackgroundImage = imageListBtns.Images["danger-btn"];
                ConnectionStatus = "connection_error";
                StopConnectingTimer();
                monitor.Start();
            }
            else if (ConnectionStatus == "connecting")
            {
                ProtoMonitor.ProcessKiller("A4iProtoC");
                monitor?.Stop();
                ConnectTunnelBtn.BackgroundImage = imageListBtns.Images["prim-btn"]; // blue
                ConnectionStatus = "disconnect";
                StopConnectingTimer();
                ab_player.sPlayer("cancel");
            }
            else
            {
                ConnectTunnelBtn.BackgroundImage = imageListBtns.Images["warn-btn"];// warn
                ConnectionStatus = "connecting";
            }

        }

        private void bg_worker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            /*
             *  if (bg_worker.IsBusy != true)
            {
                bg_worker.RunWorkerAsync();
            }
             */
        }

        private void radButton2_Click(object sender, EventArgs e)
        {


        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            ProtoMonitor.ProcessKiller("A4iProtoC");
            Application.Exit();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            //this.WindowState = FormWindowState.Minimized;
            HideToTray();
        }

        private void bntMonitoringLog_Click(object sender, EventArgs e)
        {
            monitorForm.Show();
        }

        private void bntAboutUs_Click(object sender, EventArgs e)
        {
            ab_player.sPlayer("checkbox");
            about_us about_form = new Abdal_Security_Group_App.about_us();
            about_form.ShowDialog();
            about_form.TopMost = true;
        }

        private void btnGitHub_Click(object sender, EventArgs e)
        {
            ab_player.sPlayer("checkbox");
            Process.Start(new ProcessStartInfo("https://github.com/ebrasha/" + abdal_app_name_for_url) { UseShellExecute = true });
        }

        private void pictureBox1_Click_1(object sender, EventArgs e)
        {

            donationForm.Show();
        }


        private void HideToTray()
        {
            this.Hide();

            notifyIcon.Visible = true;

            notifyIcon.ShowBalloonTip(2000, "Abdal 4iProto Client", "The application is running in the System Tray.", ToolTipIcon.Info);
        }



        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {

                contextMenuStrip.Show(Cursor.Position);
            }
            else if (e.Button == MouseButtons.Left)
            {

                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.BringToFront();
                notifyIcon.Visible = false;
            }
        }

        private void connectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (monitor == null)
            {
                MessageBox.Show("Error: Monitor object not initialized.");
                return;
            }

            if (ConnectionStatus == "disconnect")
            {

                ConnectTunnelBtn.BackgroundImage = imageListBtns.Images["warn-btn"]; // warn
                ConnectionStatus = "connecting";
                ab_player.sPlayer("start");
                monitor.Start();

            }
            else if (ConnectionStatus == "connected")
            {
                monitor?.Stop();
                ConnectTunnelBtn.BackgroundImage = imageListBtns.Images["prim-btn"]; // blue
                ConnectionStatus = "disconnect";
                StopConnectingTimer();
                ab_player.sPlayer("cancel");


            }
            else if (ConnectionStatus == "connection_error")
            {
                ab_player.sPlayer("error");
                monitor?.Stop();
                ConnectTunnelBtn.BackgroundImage = imageListBtns.Images["danger-btn"];
                ConnectionStatus = "connection_error";
                StopConnectingTimer();
                monitor.Start();
            }
            else
            {
                ConnectTunnelBtn.BackgroundImage = imageListBtns.Images["warn-btn"];// warn
                ConnectionStatus = "connecting";
            }
        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ProtoMonitor.ProcessKiller("A4iProtoC");
            Application.Exit();
        }

        private void disconnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (monitor == null)
            {
                MessageBox.Show("Error: Monitor object not initialized.");
                return;
            }

            if (ConnectionStatus == "disconnect")
            {

                ConnectTunnelBtn.BackgroundImage = imageListBtns.Images["warn-btn"]; // warn
                ConnectionStatus = "connecting";
                ab_player.sPlayer("start");
                monitor.Start();

            }
            else if (ConnectionStatus == "connected")
            {
                monitor?.Stop();
                ConnectTunnelBtn.BackgroundImage = imageListBtns.Images["prim-btn"]; // blue
                ConnectionStatus = "disconnect";
                StopConnectingTimer();
                ab_player.sPlayer("cancel");


            }
            else if (ConnectionStatus == "connection_error")
            {
                ab_player.sPlayer("error");
                monitor?.Stop();
                ConnectTunnelBtn.BackgroundImage = imageListBtns.Images["danger-btn"];
                ConnectionStatus = "connection_error";
                StopConnectingTimer();
                monitor.Start();
            }
            else
            {
                ConnectTunnelBtn.BackgroundImage = imageListBtns.Images["warn-btn"];// warn
                ConnectionStatus = "connecting";
            }
        }

        private void btnConfigSettings_Click(object sender, EventArgs e)
        {
            ab_player.sPlayer("checkbox");
            ConfigSetForm.Show();
        }

        private void aboutUSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ab_player.sPlayer("checkbox");
            about_us about_form = new Abdal_Security_Group_App.about_us();
            about_form.ShowDialog();
            about_form.TopMost = true;
        }

        private void btnDomainList_Click(object sender, EventArgs e)
        {
            domainExcludeForm.Show();
        }

        // Public method in Main form to update label safely
        public void UpdateServerIPLabel(string newIP)
        {
            if (lbServerIP.InvokeRequired)
            {
                lbServerIP.Invoke(new System.Windows.Forms.MethodInvoker(delegate
                {
                    lbServerIP.Text = newIP;
                }));
            }
            else
            {
                lbServerIP.Text = newIP;
            }
        }

        private void lbServerIP_Click(object sender, EventArgs e)
        {

        }
    }
}