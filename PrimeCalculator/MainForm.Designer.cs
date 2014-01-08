namespace PrimeCalculator
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.ElapsedTimeLabel = new System.Windows.Forms.Label();
            this.ProgressLabel = new System.Windows.Forms.Label();
            this.ElapsedTimeValueLabel = new System.Windows.Forms.Label();
            this.MilliSecondsLabel = new System.Windows.Forms.Label();
            this.ResultTextBox = new System.Windows.Forms.Label();
            this.ProgressValueLabel = new System.Windows.Forms.Label();
            this.NumberTextBox = new System.Windows.Forms.TextBox();
            this.FactorsLabel = new System.Windows.Forms.Label();
            this.FactorsTextBox = new System.Windows.Forms.TextBox();
            this.IsPrimeLabel = new System.Windows.Forms.Label();
            this.MainMenu = new System.Windows.Forms.MainMenu(this.components);
            this.ProgressBar = new System.Windows.Forms.ProgressBar();
            this.DigitSumLabel = new System.Windows.Forms.Label();
            this.DigitSumTextBox = new System.Windows.Forms.TextBox();
            this.WebsiteLabel = new System.Windows.Forms.Label();
            this.NotifyIconContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.AboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ExitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.NotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.NotifyIconContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // ElapsedTimeLabel
            // 
            this.ElapsedTimeLabel.BackColor = System.Drawing.SystemColors.Info;
            this.ElapsedTimeLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ElapsedTimeLabel.Font = new System.Drawing.Font("Tahoma", 8F);
            this.ElapsedTimeLabel.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.ElapsedTimeLabel.Location = new System.Drawing.Point(57, 141);
            this.ElapsedTimeLabel.Name = "ElapsedTimeLabel";
            this.ElapsedTimeLabel.Size = new System.Drawing.Size(138, 16);
            this.ElapsedTimeLabel.TabIndex = 36;
            this.ElapsedTimeLabel.Text = "Elapsed Time";
            this.ElapsedTimeLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.ElapsedTimeLabel.Click += new System.EventHandler(this.ElapsedTimeLabel_Click);
            // 
            // ProgressLabel
            // 
            this.ProgressLabel.BackColor = System.Drawing.SystemColors.Info;
            this.ProgressLabel.Font = new System.Drawing.Font("Tahoma", 8F);
            this.ProgressLabel.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.ProgressLabel.Location = new System.Drawing.Point(0, 141);
            this.ProgressLabel.Name = "ProgressLabel";
            this.ProgressLabel.Size = new System.Drawing.Size(56, 16);
            this.ProgressLabel.TabIndex = 35;
            this.ProgressLabel.Text = "Progress";
            this.ProgressLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // ElapsedTimeValueLabel
            // 
            this.ElapsedTimeValueLabel.BackColor = System.Drawing.SystemColors.ControlText;
            this.ElapsedTimeValueLabel.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.ElapsedTimeValueLabel.ForeColor = System.Drawing.SystemColors.Control;
            this.ElapsedTimeValueLabel.Location = new System.Drawing.Point(57, 155);
            this.ElapsedTimeValueLabel.Name = "ElapsedTimeValueLabel";
            this.ElapsedTimeValueLabel.Size = new System.Drawing.Size(140, 17);
            this.ElapsedTimeValueLabel.TabIndex = 34;
            this.ElapsedTimeValueLabel.Text = "00:00:00";
            this.ElapsedTimeValueLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // MilliSecondsLabel
            // 
            this.MilliSecondsLabel.BackColor = System.Drawing.SystemColors.ControlText;
            this.MilliSecondsLabel.Font = new System.Drawing.Font("Tahoma", 8F);
            this.MilliSecondsLabel.ForeColor = System.Drawing.SystemColors.Control;
            this.MilliSecondsLabel.Location = new System.Drawing.Point(159, 158);
            this.MilliSecondsLabel.Name = "MilliSecondsLabel";
            this.MilliSecondsLabel.Size = new System.Drawing.Size(37, 12);
            this.MilliSecondsLabel.TabIndex = 33;
            this.MilliSecondsLabel.Text = "000";
            // 
            // ResultTextBox
            // 
            this.ResultTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.ResultTextBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.ResultTextBox.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.ResultTextBox.Location = new System.Drawing.Point(57, 29);
            this.ResultTextBox.Name = "ResultTextBox";
            this.ResultTextBox.Size = new System.Drawing.Size(141, 23);
            this.ResultTextBox.TabIndex = 31;
            this.ResultTextBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ProgressValueLabel
            // 
            this.ProgressValueLabel.BackColor = System.Drawing.SystemColors.ControlText;
            this.ProgressValueLabel.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.ProgressValueLabel.ForeColor = System.Drawing.SystemColors.Control;
            this.ProgressValueLabel.Location = new System.Drawing.Point(0, 155);
            this.ProgressValueLabel.Name = "ProgressValueLabel";
            this.ProgressValueLabel.Size = new System.Drawing.Size(56, 17);
            this.ProgressValueLabel.TabIndex = 30;
            this.ProgressValueLabel.Text = "0%";
            this.ProgressValueLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // NumberTextBox
            // 
            this.NumberTextBox.BackColor = System.Drawing.Color.MistyRose;
            this.NumberTextBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.NumberTextBox.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.NumberTextBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.NumberTextBox.Location = new System.Drawing.Point(0, 0);
            this.NumberTextBox.MaxLength = 19;
            this.NumberTextBox.Name = "NumberTextBox";
            this.NumberTextBox.Size = new System.Drawing.Size(198, 27);
            this.NumberTextBox.TabIndex = 21;
            this.NumberTextBox.TextChanged += new System.EventHandler(this.NumberTextBox_TextChanged);
            this.NumberTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.NumberTextBox_KeyPress);
            // 
            // FactorsLabel
            // 
            this.FactorsLabel.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.FactorsLabel.Font = new System.Drawing.Font("Tahoma", 8F);
            this.FactorsLabel.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.FactorsLabel.Location = new System.Drawing.Point(0, 57);
            this.FactorsLabel.Name = "FactorsLabel";
            this.FactorsLabel.Size = new System.Drawing.Size(53, 16);
            this.FactorsLabel.TabIndex = 28;
            this.FactorsLabel.Text = "Factors";
            this.FactorsLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // FactorsTextBox
            // 
            this.FactorsTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.FactorsTextBox.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.FactorsTextBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.FactorsTextBox.Location = new System.Drawing.Point(57, 54);
            this.FactorsTextBox.Multiline = true;
            this.FactorsTextBox.Name = "FactorsTextBox";
            this.FactorsTextBox.ReadOnly = true;
            this.FactorsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.FactorsTextBox.Size = new System.Drawing.Size(141, 61);
            this.FactorsTextBox.TabIndex = 24;
            this.FactorsTextBox.TabStop = false;
            this.FactorsTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // IsPrimeLabel
            // 
            this.IsPrimeLabel.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.IsPrimeLabel.Font = new System.Drawing.Font("Tahoma", 8F);
            this.IsPrimeLabel.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.IsPrimeLabel.Location = new System.Drawing.Point(-1, 31);
            this.IsPrimeLabel.Name = "IsPrimeLabel";
            this.IsPrimeLabel.Size = new System.Drawing.Size(54, 19);
            this.IsPrimeLabel.TabIndex = 27;
            this.IsPrimeLabel.Text = "Result";
            this.IsPrimeLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // ProgressBar
            // 
            this.ProgressBar.Location = new System.Drawing.Point(0, 172);
            this.ProgressBar.Name = "ProgressBar";
            this.ProgressBar.Size = new System.Drawing.Size(197, 6);
            this.ProgressBar.TabIndex = 25;
            // 
            // DigitSumLabel
            // 
            this.DigitSumLabel.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.DigitSumLabel.Font = new System.Drawing.Font("Tahoma", 8F);
            this.DigitSumLabel.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.DigitSumLabel.Location = new System.Drawing.Point(0, 119);
            this.DigitSumLabel.Name = "DigitSumLabel";
            this.DigitSumLabel.Size = new System.Drawing.Size(53, 17);
            this.DigitSumLabel.TabIndex = 38;
            this.DigitSumLabel.Text = "Digit Sum";
            this.DigitSumLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // DigitSumTextBox
            // 
            this.DigitSumTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.DigitSumTextBox.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.DigitSumTextBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.DigitSumTextBox.Location = new System.Drawing.Point(57, 116);
            this.DigitSumTextBox.Name = "DigitSumTextBox";
            this.DigitSumTextBox.ReadOnly = true;
            this.DigitSumTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.DigitSumTextBox.Size = new System.Drawing.Size(141, 24);
            this.DigitSumTextBox.TabIndex = 39;
            this.DigitSumTextBox.TabStop = false;
            this.DigitSumTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // WebsiteLabel
            // 
            this.WebsiteLabel.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.WebsiteLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.WebsiteLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.WebsiteLabel.Font = new System.Drawing.Font("Tahoma", 8F);
            this.WebsiteLabel.ForeColor = System.Drawing.Color.Purple;
            this.WebsiteLabel.Location = new System.Drawing.Point(0, 181);
            this.WebsiteLabel.Name = "WebsiteLabel";
            this.WebsiteLabel.Size = new System.Drawing.Size(198, 18);
            this.WebsiteLabel.TabIndex = 40;
            this.WebsiteLabel.Text = "www.heliwave.com";
            this.WebsiteLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.WebsiteLabel.Click += new System.EventHandler(this.WebsiteLabel_Click);
            // 
            // NotifyIconContextMenuStrip
            // 
            this.NotifyIconContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AboutToolStripMenuItem,
            this.ExitToolStripMenuItem});
            this.NotifyIconContextMenuStrip.Name = "NotifyIconContextMenuStrip";
            this.NotifyIconContextMenuStrip.Size = new System.Drawing.Size(108, 48);
            // 
            // AboutToolStripMenuItem
            // 
            this.AboutToolStripMenuItem.Name = "AboutToolStripMenuItem";
            this.AboutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.AboutToolStripMenuItem.Text = "About";
            this.AboutToolStripMenuItem.Click += new System.EventHandler(this.AboutToolStripMenuItem_Click);
            // 
            // ExitToolStripMenuItem
            // 
            this.ExitToolStripMenuItem.Name = "ExitToolStripMenuItem";
            this.ExitToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.ExitToolStripMenuItem.Text = "Exit";
            this.ExitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItem_Click);
            // 
            // NotifyIcon
            // 
            this.NotifyIcon.ContextMenuStrip = this.NotifyIconContextMenuStrip;
            this.NotifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("NotifyIcon.Icon")));
            this.NotifyIcon.Text = "Prime Calculator";
            this.NotifyIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(this.NotifyIcon_MouseClick);
            this.NotifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.NotifyIcon_MouseDoubleClick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(198, 199);
            this.Controls.Add(this.WebsiteLabel);
            this.Controls.Add(this.DigitSumTextBox);
            this.Controls.Add(this.DigitSumLabel);
            this.Controls.Add(this.ProgressBar);
            this.Controls.Add(this.IsPrimeLabel);
            this.Controls.Add(this.FactorsTextBox);
            this.Controls.Add(this.FactorsLabel);
            this.Controls.Add(this.NumberTextBox);
            this.Controls.Add(this.ProgressValueLabel);
            this.Controls.Add(this.ResultTextBox);
            this.Controls.Add(this.MilliSecondsLabel);
            this.Controls.Add(this.ElapsedTimeValueLabel);
            this.Controls.Add(this.ProgressLabel);
            this.Controls.Add(this.ElapsedTimeLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(204, 227);
            this.Menu = this.MainMenu;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = " Prime Calculator";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyUp);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.NotifyIconContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label ElapsedTimeLabel;
        private System.Windows.Forms.Label ProgressLabel;
        private System.Windows.Forms.Label ElapsedTimeValueLabel;
        private System.Windows.Forms.Label MilliSecondsLabel;
        private System.Windows.Forms.Label ResultTextBox;
        private System.Windows.Forms.Label ProgressValueLabel;
        private System.Windows.Forms.TextBox NumberTextBox;
        private System.Windows.Forms.Label FactorsLabel;
        private System.Windows.Forms.TextBox FactorsTextBox;
        private System.Windows.Forms.Label IsPrimeLabel;
        private System.Windows.Forms.MainMenu MainMenu;
        private System.Windows.Forms.ProgressBar ProgressBar;
        private System.Windows.Forms.Label DigitSumLabel;
        private System.Windows.Forms.TextBox DigitSumTextBox;
        private System.Windows.Forms.Label WebsiteLabel;
        private System.Windows.Forms.ContextMenuStrip NotifyIconContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem AboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ExitToolStripMenuItem;
        private System.Windows.Forms.NotifyIcon NotifyIcon;

    }
}