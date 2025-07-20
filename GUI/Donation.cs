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
    public partial class Donation : Form
    {
        private string abdal_app_name_for_url = Assembly.GetExecutingAssembly().GetName().ToString().Split(',')[0]
            .ToLower().Replace(' ', '-');
        private AbdalSoundPlayer ab_player = new AbdalSoundPlayer();


        public Donation()
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

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            ab_player.sPlayer("checkbox");
            Process.Start(new ProcessStartInfo("https://github.com/ebrasha/" + abdal_app_name_for_url) { UseShellExecute = true });
        }

        private void Donation_Load(object sender, EventArgs e)
        {

        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            ab_player.sPlayer("checkbox");
            about_us about_form = new Abdal_Security_Group_App.about_us();
            about_form.ShowDialog();
            about_form.TopMost = true;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            ab_player.sPlayer("coin");
            Process.Start(new ProcessStartInfo("https://alphajet.ir/abdal-donation") { UseShellExecute = true });
        }

        private void btnEnDonation_Click(object sender, EventArgs e)
        {
            ab_player.sPlayer("coin");
            Process.Start(new ProcessStartInfo("https://ebrasha.com/abdal-donation") { UseShellExecute = true });
        }
    }
}
