using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telerik.WinControls.UI;
using Telerik.WinControls;
using System.Diagnostics;
using Abdal_Security_Group_App.Core;
using System.Reflection;

namespace Abdal_Security_Group_App
{
    public partial class DomainExclude : Telerik.WinControls.UI.RadForm
    {
        private AbdalSoundPlayer ab_player = new AbdalSoundPlayer();
        private Monitor monitorForm;
        private Donation donationForm;
        private string abdal_app_name_for_url = Assembly.GetExecutingAssembly().GetName().ToString().Split(',')[0]
        .ToLower().Replace(' ', '-');

        public DomainExclude()
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

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void DomainExclude_Load(object sender, EventArgs e)
        {
            LoadDomains(richTextBoxDomain);
            monitorForm = new Monitor();
            donationForm = new Donation();
        }

        // Save current RichTextBox content into domains.txt (overwrite mode)
        public static void SaveDomains(RichTextBox richTextBox)
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "domains.txt");
                File.WriteAllText(path, richTextBox.Text);
            }
            catch
            {
                // Silent error handling
            }
        }


        // Load contents of domains.txt into the RichTextBox
        public static void LoadDomains(RichTextBox richTextBox)
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "domains.txt");

                if (File.Exists(path))
                {
                    string content = File.ReadAllText(path);
                    richTextBox.Text = content;
                }
            }
            catch
            {
                // Silent error handling
            }
        }

        private void richTextBoxDomain_TextChanged(object sender, EventArgs e)
        {
            SaveDomains(richTextBoxDomain);
        }

        private void btnGitHub_Click(object sender, EventArgs e)
        {
            ab_player.sPlayer("checkbox");
            Process.Start(new ProcessStartInfo("https://github.com/ebrasha/" + abdal_app_name_for_url) { UseShellExecute = true });
        }

        private void bntAboutUs_Click(object sender, EventArgs e)
        {
            ab_player.sPlayer("checkbox");
            about_us about_form = new Abdal_Security_Group_App.about_us();
            about_form.ShowDialog();
            about_form.TopMost = true;
        }

        private void bntMonitoringLog_Click(object sender, EventArgs e)
        {
            monitorForm.Show();
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            donationForm.Show();
        }
    }
}
