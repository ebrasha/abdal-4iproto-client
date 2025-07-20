namespace Abdal_Security_Group_App
{
    partial class Monitor
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Monitor));
            visualStudio2022DarkTheme1 = new Telerik.WinControls.Themes.VisualStudio2022DarkTheme();
            listBoxLog = new ListBox();
            pictureBox1 = new PictureBox();
            pictureBox2 = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this).BeginInit();
            SuspendLayout();
            // 
            // listBoxLog
            // 
            listBoxLog.BackColor = Color.FromArgb(27, 33, 67);
            listBoxLog.BorderStyle = BorderStyle.None;
            listBoxLog.ForeColor = SystemColors.MenuBar;
            listBoxLog.FormattingEnabled = true;
            listBoxLog.ItemHeight = 15;
            listBoxLog.Location = new Point(26, 89);
            listBoxLog.Name = "listBoxLog";
            listBoxLog.Size = new Size(283, 540);
            listBoxLog.TabIndex = 0;
            listBoxLog.MouseDoubleClick += listBoxLog_MouseDoubleClick;
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.Transparent;
            pictureBox1.Cursor = Cursors.Hand;
            pictureBox1.Location = new Point(19, 27);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(25, 25);
            pictureBox1.TabIndex = 1;
            pictureBox1.TabStop = false;
            pictureBox1.Click += pictureBox1_Click;
            // 
            // pictureBox2
            // 
            pictureBox2.BackColor = Color.Transparent;
            pictureBox2.Cursor = Cursors.Hand;
            pictureBox2.Location = new Point(51, 27);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(25, 25);
            pictureBox2.TabIndex = 2;
            pictureBox2.TabStop = false;
            pictureBox2.Click += pictureBox2_Click;
            // 
            // Monitor
            // 
            AutoScaleBaseSize = new Size(7, 15);
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(27, 33, 67);
            BackgroundImage = Properties.Resources.ui_mON_copy2;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(330, 652);
            Controls.Add(pictureBox2);
            Controls.Add(pictureBox1);
            Controls.Add(listBoxLog);
            FormBorderStyle = FormBorderStyle.None;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Monitor";
            Opacity = 0.8D;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Abdal 4iProto Client Monitor";
            ThemeName = "VisualStudio2022Dark";
            FormClosing += Monitor_FormClosing;
            Load += Monitor_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ((System.ComponentModel.ISupportInitialize)this).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private Telerik.WinControls.Themes.VisualStudio2022DarkTheme visualStudio2022DarkTheme1;
        public ListBox listBoxLog;
        private PictureBox pictureBox1;
        private PictureBox pictureBox2;
    }
}
