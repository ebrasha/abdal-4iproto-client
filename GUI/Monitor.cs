using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Abdal_Security_Group_App
{
    public partial class Monitor : Telerik.WinControls.UI.RadForm
    {


        public Monitor()
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

        private void Monitor_Load(object sender, EventArgs e)
        {

        }

        private void Monitor_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void listBoxLog_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listBoxLog.SelectedItem != null)
            {
                Clipboard.SetText(listBoxLog.SelectedItem.ToString());
                MessageBox.Show("Copied to clipboard ✔", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
