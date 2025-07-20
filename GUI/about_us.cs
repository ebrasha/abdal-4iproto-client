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
    public partial class about_us : Telerik.WinControls.UI.RadForm
    {
        private AbdalSoundPlayer ab_player = new AbdalSoundPlayer();

        private string abdal_app_name_for_url = Assembly.GetExecutingAssembly().GetName().ToString().Split(',')[0]
          .ToLower().Replace(' ', '-');
        public about_us()
        {
            InitializeComponent();
            Version version = Assembly.GetExecutingAssembly().GetName().Version!;
            label_version.Text = "Version:" + " " + version.Major + "." + version.Minor;
            ab_player.sPlayer("ab-us");
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

        private void about_us_Load(object sender, EventArgs e)
        {
            richTextBox_about_us.Text = AboutUsWriter.about_us_content();
        }

        private void about_us_FormClosing(object sender, FormClosingEventArgs e)
        {
            ab_player.sPlayer("checkbox");
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            ab_player.sPlayer("checkbox");
            this.Close();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;

        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            ab_player.sPlayer("checkbox");
            Process.Start(new ProcessStartInfo("https://github.com/ebrasha/" + abdal_app_name_for_url) { UseShellExecute = true });
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            Monitor monitorForm = new Monitor();
            monitorForm.Show();
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            Donation donationForm = new Donation(); 
            donationForm.Show();
        }
    }
}