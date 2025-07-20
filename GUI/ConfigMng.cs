using Abdal_Security_Group_App.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Abdal_Security_Group_App
{
    public partial class ConfigMng : Form
    {
        private AbdalSoundPlayer ab_player = new AbdalSoundPlayer();
        private Main _mainForm ;
        private string abdal_app_name_for_url = Assembly.GetExecutingAssembly().GetName().ToString().Split(',')[0]
          .ToLower().Replace(' ', '-');

        public ConfigMng()
        {
            InitializeComponent();
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

        private void ConfigMng_Load(object sender, EventArgs e)
        {
            var configMap = Config.LoadAsDictionary();

            if (configMap == null)
            {
                textBoxIP.Text = "";
                textBoxPort.Text = "";
                textBoxUserName.Text = "";
                textBoxPassword.Text = "";
                textBoxSocks.Text = "";
                return;
            }

            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    SetTextBoxes(configMap);
                }));
            }
            else
            {
                SetTextBoxes(configMap);
            }

        }

        private void SetTextBoxes(Dictionary<string, string> config)
        {
            config.TryGetValue("ssh_host", out string sshHost);
            config.TryGetValue("ssh_port", out string sshPort);
            config.TryGetValue("ssh_user", out string sshUser);
            config.TryGetValue("ssh_password", out string sshPassword);
            config.TryGetValue("socks5_port", out string socks5Port);

            textBoxIP.Text = sshHost ?? "";
            textBoxPort.Text = sshPort ?? "";
            textBoxUserName.Text = sshUser ?? "";
            textBoxPassword.Text = sshPassword ?? "";
            textBoxSocks.Text = socks5Port ?? "";
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void pictureBox7_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btnGithub_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/ebrasha/" + abdal_app_name_for_url) { UseShellExecute = true });
        }

        private void btnMonitor_Click(object sender, EventArgs e)
        {
            Monitor monitorForm = new Monitor();
            monitorForm.Show();
        }

        private void btnAboutus_Click(object sender, EventArgs e)
        {
            about_us aboutForm = new about_us();
            aboutForm.Show();
        }

        private void btnDonation_Click(object sender, EventArgs e)
        {
            Donation donationForm = new Donation();
            donationForm.Show();
        }

        // Setter for passing main form reference
        public void SetMainForm(Main mainForm)
        {
            _mainForm = mainForm;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            string sshHost = textBoxIP.Text.Trim();
            string sshUser = textBoxUserName.Text.Trim();
            string sshPassword = textBoxPassword.Text.Trim();

            string sshPortStr = textBoxPort.Text.Trim();
            string socks5PortStr = textBoxSocks.Text.Trim();

            // Convert string ports to int (fallback to defaults if invalid)
            int sshPort = int.TryParse(sshPortStr, out var parsedSshPort) ? parsedSshPort : 22;
            int socks5Port = int.TryParse(socks5PortStr, out var parsedSocks5Port) ? parsedSocks5Port : 52905;

            string autoReconnect = "yes";
            int autoReconnectTimeout = 2000;

            Config.Save(
                sshHost,
                sshPort.ToString(),        // because method signature expects string
                sshUser,
                sshPassword,
                socks5Port.ToString(),     // because method signature expects string
                autoReconnect,
                autoReconnectTimeout
            );

            ab_player.sPlayer("done");
            _mainForm.UpdateServerIPLabel(sshHost);
            MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
