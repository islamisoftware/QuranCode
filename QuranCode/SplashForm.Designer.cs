partial class SplashForm
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
        this.InformationLabel = new System.Windows.Forms.Label();
        this.ArabicLabel = new System.Windows.Forms.Label();
        this.VersionLabel = new System.Windows.Forms.Label();
        this.TitleLabel = new System.Windows.Forms.Label();
        this.ProgressBar = new System.Windows.Forms.ProgressBar();
        this.PrimalogyLabel = new System.Windows.Forms.Label();
        this.SuspendLayout();
        // 
        // InformationLabel
        // 
        this.InformationLabel.BackColor = System.Drawing.Color.Transparent;
        this.InformationLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.InformationLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.InformationLabel.ForeColor = System.Drawing.Color.AliceBlue;
        this.InformationLabel.Location = new System.Drawing.Point(0, 94);
        this.InformationLabel.Name = "InformationLabel";
        this.InformationLabel.Size = new System.Drawing.Size(178, 16);
        this.InformationLabel.TabIndex = 11;
        this.InformationLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
        // 
        // ArabicLabel
        // 
        this.ArabicLabel.BackColor = System.Drawing.Color.Transparent;
        this.ArabicLabel.Cursor = System.Windows.Forms.Cursors.Hand;
        this.ArabicLabel.Dock = System.Windows.Forms.DockStyle.Top;
        this.ArabicLabel.Font = new System.Drawing.Font("Arabic Typesetting", 21F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.ArabicLabel.ForeColor = System.Drawing.Color.AliceBlue;
        this.ArabicLabel.Location = new System.Drawing.Point(0, 0);
        this.ArabicLabel.Name = "ArabicLabel";
        this.ArabicLabel.Size = new System.Drawing.Size(178, 47);
        this.ArabicLabel.TabIndex = 16;
        this.ArabicLabel.Tag = "http://heliwave.com/Primalogy_AR.pdf";
        this.ArabicLabel.Text = "بِسْمِ ٱللَّهِ ٱلرَّحْمَٰنِ ٱلرَّحِيمِ";
        this.ArabicLabel.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
        this.ArabicLabel.Click += new System.EventHandler(this.LinkLabel_Click);
        // 
        // VersionLabel
        // 
        this.VersionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.VersionLabel.BackColor = System.Drawing.Color.Transparent;
        this.VersionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.VersionLabel.ForeColor = System.Drawing.Color.SteelBlue;
        this.VersionLabel.Location = new System.Drawing.Point(0, 74);
        this.VersionLabel.Name = "VersionLabel";
        this.VersionLabel.Size = new System.Drawing.Size(178, 14);
        this.VersionLabel.TabIndex = 18;
        this.VersionLabel.Text = "©2014 Ali Adams";
        this.VersionLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
        // 
        // TitleLabel
        // 
        this.TitleLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.TitleLabel.Cursor = System.Windows.Forms.Cursors.Hand;
        this.TitleLabel.Font = new System.Drawing.Font("Agency FB", 19F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.TitleLabel.ForeColor = System.Drawing.Color.SteelBlue;
        this.TitleLabel.Location = new System.Drawing.Point(0, 45);
        this.TitleLabel.Name = "TitleLabel";
        this.TitleLabel.Size = new System.Drawing.Size(178, 34);
        this.TitleLabel.TabIndex = 17;
        this.TitleLabel.Tag = "http://qurancode.com/";
        this.TitleLabel.Text = "QuranCode 1433";
        this.TitleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        this.TitleLabel.Click += new System.EventHandler(this.LinkLabel_Click);
        // 
        // ProgressBar
        // 
        this.ProgressBar.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.ProgressBar.Location = new System.Drawing.Point(0, 90);
        this.ProgressBar.Name = "ProgressBar";
        this.ProgressBar.Size = new System.Drawing.Size(178, 4);
        this.ProgressBar.TabIndex = 19;
        // 
        // PrimalogyLabel
        // 
        this.PrimalogyLabel.BackColor = System.Drawing.Color.LightGray;
        this.PrimalogyLabel.Cursor = System.Windows.Forms.Cursors.Hand;
        this.PrimalogyLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.PrimalogyLabel.ForeColor = System.Drawing.Color.SteelBlue;
        this.PrimalogyLabel.Location = new System.Drawing.Point(0, 0);
        this.PrimalogyLabel.Name = "PrimalogyLabel";
        this.PrimalogyLabel.Size = new System.Drawing.Size(178, 14);
        this.PrimalogyLabel.TabIndex = 20;
        this.PrimalogyLabel.Tag = "http://heliwave.com/Primalogy.pdf";
        this.PrimalogyLabel.Text = "Q u r a n   =   K e y   +   M e s s a g e";
        this.PrimalogyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        this.PrimalogyLabel.Click += new System.EventHandler(this.LinkLabel_Click);
        // 
        // SplashForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackColor = System.Drawing.Color.LightSteelBlue;
        this.ClientSize = new System.Drawing.Size(178, 110);
        this.Controls.Add(this.PrimalogyLabel);
        this.Controls.Add(this.ArabicLabel);
        this.Controls.Add(this.ProgressBar);
        this.Controls.Add(this.VersionLabel);
        this.Controls.Add(this.TitleLabel);
        this.Controls.Add(this.InformationLabel);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        this.Name = "SplashForm";
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "SplashForm";
        this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Label InformationLabel;
    private System.Windows.Forms.Label ArabicLabel;
    private System.Windows.Forms.Label VersionLabel;
    private System.Windows.Forms.Label TitleLabel;
    private System.Windows.Forms.ProgressBar ProgressBar;
    private System.Windows.Forms.Label PrimalogyLabel;
}
