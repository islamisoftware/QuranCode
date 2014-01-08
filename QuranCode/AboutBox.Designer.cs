partial class AboutBox
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutBox));
        this.TableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
        this.LogoPictureBox = new System.Windows.Forms.PictureBox();
        this.ProductNameLabel = new System.Windows.Forms.Label();
        this.CopyrightLabel = new System.Windows.Forms.Label();
        this.DescriptionTextBox = new System.Windows.Forms.TextBox();
        this.TableLayoutPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.LogoPictureBox)).BeginInit();
        this.SuspendLayout();
        // 
        // TableLayoutPanel
        // 
        this.TableLayoutPanel.ColumnCount = 2;
        this.TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 29.68783F));
        this.TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70.31217F));
        this.TableLayoutPanel.Controls.Add(this.LogoPictureBox, 0, 0);
        this.TableLayoutPanel.Controls.Add(this.ProductNameLabel, 1, 0);
        this.TableLayoutPanel.Controls.Add(this.CopyrightLabel, 1, 1);
        this.TableLayoutPanel.Controls.Add(this.DescriptionTextBox, 1, 2);
        this.TableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        this.TableLayoutPanel.Location = new System.Drawing.Point(3, 3);
        this.TableLayoutPanel.Name = "TableLayoutPanel";
        this.TableLayoutPanel.RowCount = 3;
        this.TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 19F));
        this.TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
        this.TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableLayoutPanel.Size = new System.Drawing.Size(476, 212);
        this.TableLayoutPanel.TabIndex = 0;
        // 
        // LogoPictureBox
        // 
        this.LogoPictureBox.Cursor = System.Windows.Forms.Cursors.Hand;
        this.LogoPictureBox.Dock = System.Windows.Forms.DockStyle.Top;
        this.LogoPictureBox.Image = ((System.Drawing.Image)(resources.GetObject("LogoPictureBox.Image")));
        this.LogoPictureBox.Location = new System.Drawing.Point(3, 3);
        this.LogoPictureBox.Name = "LogoPictureBox";
        this.TableLayoutPanel.SetRowSpan(this.LogoPictureBox, 3);
        this.LogoPictureBox.Size = new System.Drawing.Size(135, 206);
        this.LogoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
        this.LogoPictureBox.TabIndex = 12;
        this.LogoPictureBox.TabStop = false;
        this.LogoPictureBox.Tag = "http://heliwave.com/Soul.and.Spirit.pdf";
        this.LogoPictureBox.Click += new System.EventHandler(this.LinkLabel_Click);
        // 
        // ProductNameLabel
        // 
        this.ProductNameLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        this.ProductNameLabel.Location = new System.Drawing.Point(147, 0);
        this.ProductNameLabel.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
        this.ProductNameLabel.MaximumSize = new System.Drawing.Size(0, 17);
        this.ProductNameLabel.Name = "ProductNameLabel";
        this.ProductNameLabel.Size = new System.Drawing.Size(326, 17);
        this.ProductNameLabel.TabIndex = 19;
        this.ProductNameLabel.Text = "Product Name";
        this.ProductNameLabel.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
        // 
        // CopyrightLabel
        // 
        this.CopyrightLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        this.CopyrightLabel.Location = new System.Drawing.Point(147, 19);
        this.CopyrightLabel.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
        this.CopyrightLabel.MaximumSize = new System.Drawing.Size(0, 17);
        this.CopyrightLabel.Name = "CopyrightLabel";
        this.CopyrightLabel.Size = new System.Drawing.Size(326, 17);
        this.CopyrightLabel.TabIndex = 21;
        this.CopyrightLabel.Text = "Copyright";
        this.CopyrightLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // DescriptionTextBox
        // 
        this.DescriptionTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
        this.DescriptionTextBox.Location = new System.Drawing.Point(147, 45);
        this.DescriptionTextBox.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
        this.DescriptionTextBox.Multiline = true;
        this.DescriptionTextBox.Name = "DescriptionTextBox";
        this.DescriptionTextBox.ReadOnly = true;
        this.DescriptionTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
        this.DescriptionTextBox.Size = new System.Drawing.Size(326, 164);
        this.DescriptionTextBox.TabIndex = 23;
        this.DescriptionTextBox.TabStop = false;
        this.DescriptionTextBox.Text = "Description";
        // 
        // AboutBox
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(482, 218);
        this.Controls.Add(this.TableLayoutPanel);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "AboutBox";
        this.Padding = new System.Windows.Forms.Padding(3);
        this.ShowIcon = false;
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "About";
        this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.AboutBox_KeyUp);
        this.TableLayoutPanel.ResumeLayout(false);
        this.TableLayoutPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.LogoPictureBox)).EndInit();
        this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel TableLayoutPanel;
    private System.Windows.Forms.PictureBox LogoPictureBox;
    private System.Windows.Forms.Label ProductNameLabel;
    private System.Windows.Forms.Label CopyrightLabel;
    private System.Windows.Forms.TextBox DescriptionTextBox;
}
