/* Title:
 * ScrollablePictureBox with zoom and scroll functionallity
 * 
 * Author:
 * Alexander Kloep Apr. 2005
 * Alexander.Kloep@gmx.net
 * 
 * Reason:
 * In a past project i designed a GUI with a PictureBox control on it. Because of the low screen 
 * resolution i couldn´t make the GUI big enough to show the whole picture. So i decided to develop
 * my own scrollable picturebox with the special highlight of zooming functionallity.
 * 
 * The solution: 
 * When the mouse cursor enters the ctrl, the cursorstyle changes and you are able to zoom in or out 
 * with the mousewheel. The princip of the zooming effect is to raise or to lower the inner picturebox 
 * size by a fixed zooming factor. The scroolbars appear automatically when the inner picturebox
 * gets bigger than the ctrl.
 *  
 * Here it is...
 * 
 * Last modification: 06/04/2005
 */

#region Usings

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

#endregion

/// <summary>
/// Summary for the PictureBox Ctrl
/// </summary>
public class ScrollablePictureBox : UserControl
{
    #region Members

    private System.Windows.Forms.PictureBox PictureBox;
    private System.Windows.Forms.Panel Panel;
    private System.ComponentModel.Container components = null;
    private string m_image_path = "";

    #endregion

    #region Constants

    private double ZOOMFACTOR = 1.25;	// = 25% smaller or larger
    private int MINMAX = 5;		        // 5 times bigger or smaller than the ctrl

    #endregion

    #region Designer generated code

    private void InitializeComponent()
    {
        this.PictureBox = new System.Windows.Forms.PictureBox();
        this.Panel = new System.Windows.Forms.Panel();
        ((System.ComponentModel.ISupportInitialize)(this.PictureBox)).BeginInit();
        this.Panel.SuspendLayout();
        this.SuspendLayout();
        // 
        // PictureBox
        // 
        this.PictureBox.Location = new System.Drawing.Point(0, 0);
        this.PictureBox.Name = "PictureBox";
        this.PictureBox.Size = new System.Drawing.Size(552, 571);
        this.PictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
        this.PictureBox.TabIndex = 3;
        this.PictureBox.TabStop = false;
        this.PictureBox.MouseEnter += new System.EventHandler(this.ScrollablePictureBox_MouseEnter);
        // 
        // Panel
        // 
        this.Panel.AutoScroll = true;
        this.Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.Panel.Controls.Add(this.PictureBox);
        this.Panel.Dock = System.Windows.Forms.DockStyle.Fill;
        this.Panel.Location = new System.Drawing.Point(0, 0);
        this.Panel.Name = "Panel";
        this.Panel.Size = new System.Drawing.Size(557, 576);
        this.Panel.TabIndex = 4;
        this.Panel.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.ScrollablePictureBox_MouseWheel);
        this.Panel.MouseEnter += new System.EventHandler(this.ScrollablePictureBox_MouseEnter);
        // 
        // ScrollablePictureBox
        // 
        this.Controls.Add(this.Panel);
        this.Name = "ScrollablePictureBox";
        this.Size = new System.Drawing.Size(557, 576);
        this.Resize += new System.EventHandler(this.ScrollablePictureBox_Resize);
        ((System.ComponentModel.ISupportInitialize)(this.PictureBox)).EndInit();
        this.Panel.ResumeLayout(false);
        this.ResumeLayout(false);

    }
    #endregion

    #region Constructors

    public ScrollablePictureBox()
    {
        InitializeComponent();
    }

    #endregion

    #region Properties

    /// <summary>
    /// Property to select the picture which is displayed in the picturebox. If the 
    /// file doesn´t exist or we receive an exception, the picturebox displays 
    /// a red cross.
    /// </summary>
    /// <value>Complete filename of the picture, including path information</value>
    /// <remarks>Supported fileformat: *.gif, *.tif, *.jpg, *.bmp</remarks>
    [Browsable(false)]
    public string ImagePath
    {
        get { return m_image_path; }
        set
        {
            if (null != value)
            {
                if (System.IO.File.Exists(value))
                {
                    try
                    {
                        PictureBox.Image = System.Drawing.Image.FromFile(value);
                        m_image_path = value;
                    }
                    catch (OutOfMemoryException /*ex*/)
                    {
                        OnError();
                    }
                }
                else
                {
                    OnError();
                }

                //PictureBox.Width = Panel.Width;
                //PictureBox.Height = Panel.Height;
            }
        }
    }

    /// <summary>
    /// Set the frametype of the picturbox
    /// </summary>
    [Browsable(false)]
    public BorderStyle Border
    {
        get { return Panel.BorderStyle; }
        set { Panel.BorderStyle = value; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Create a simple red cross as a bitmap and display it in the picturebox
    /// </summary>
    private void OnError()
    {
        Bitmap bitmap = new Bitmap(Panel.Width, Panel.Height, System.Drawing.Imaging.PixelFormat.Format16bppRgb555);
        Graphics graphics = Graphics.FromImage(bitmap);
        Pen pen = new Pen(Color.Red, 5);
        graphics.DrawLine(pen, 0, 0, Panel.Width, Panel.Height);
        graphics.DrawLine(pen, 0, Panel.Height, Panel.Width, 0);
        PictureBox.Image = bitmap;
        graphics.Dispose();
    }

    /// <summary>
    /// Make the PictureBox dimensions larger to effect the Zoom.
    /// </summary>
    /// <remarks>Maximum 5 times bigger</remarks>
    public void ZoomIn()
    {
        if ((PictureBox.Width < (MINMAX * Panel.Width)) &&
            (PictureBox.Height < (MINMAX * Panel.Height)))
        {
            PictureBox.Width = Convert.ToInt32(PictureBox.Width * ZOOMFACTOR);
            PictureBox.Height = Convert.ToInt32(PictureBox.Height * ZOOMFACTOR);
        }
    }

    /// <summary>
    /// Make the PictureBox dimensions smaller to effect the Zoom.
    /// </summary>
    /// <remarks>Minimum 5 times smaller</remarks>
    public void ZoomOut()
    {
        if ((PictureBox.Width > (Panel.Width / MINMAX)) &&
            (PictureBox.Height > (Panel.Height / MINMAX)))
        {
            PictureBox.Width = Convert.ToInt32(PictureBox.Width / ZOOMFACTOR);
            PictureBox.Height = Convert.ToInt32(PictureBox.Height / ZOOMFACTOR);
        }
    }

    #endregion

    #region Events

    /// <summary>
    /// We use the mousewheel to zoom the picture in or out
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ScrollablePictureBox_MouseWheel(object sender, MouseEventArgs e)
    {
        if (e.Delta < 0)
        {
            ZoomOut();
        }
        else
        {
            ZoomIn();
        }
    }

    /// <summary>
    /// Make sure that the PictureBox have the focus, otherwise it doesn´t receive 
    /// mousewheel events !.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ScrollablePictureBox_MouseEnter(object sender, EventArgs e)
    {
        if (!PictureBox.Focused)
        {
            PictureBox.Focus();
        }
    }

    /// <summary>
    /// Resize the PictureBox as control is resized
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ScrollablePictureBox_Resize(object sender, EventArgs e)
    {
        PictureBox.Width = Panel.Width - 4;
        PictureBox.Height = Panel.Height - 4;
    }

    #endregion

    #region Disposal

    /// <summary>
    /// Die verwendeten Ressourcen bereinigen.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (components != null)
                components.Dispose();
        }
        base.Dispose(disposing);
    }

    #endregion
}
