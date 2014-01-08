using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

/// <summary>
/// PictureBoxEx implements Zoom and Pan for PictureBox
/// </summary>
public class PictureBoxEx : Control
{
    private bool m_is_panning = false;
    private PointF m_start_point;
    private PointF m_current_point;
    private float m_previous_client_width = 0F;
    private float m_previous_client_height = 0F;
    private PointF m_zoom_point;
    private int m_max_zoom_steps = 20;
    private int m_zoom_step = 1;

    Image m_image = null;
    [Category("Appearance"), Description("The image to be displayed")]
    public Image Image
    {
        get { return m_image; }
        set
        {
            m_image = value;
            this.Invalidate();
            OnResize(EventArgs.Empty);
        }
    }

    PictureBoxSizeMode m_size_mode = PictureBoxSizeMode.Zoom;
    [Category("Appearance"), Description("The size mode of the image to be displayed")]
    public PictureBoxSizeMode SizeMode
    {
        get { return m_size_mode; }
        set
        {
            m_size_mode = value;
            this.Invalidate();
            OnResize(EventArgs.Empty);
        }
    }

    RectangleF m_clip_rectangle = new RectangleF();
    [Category("Appearance"), Description("The visible image part to be displayed")]
    public Rectangle ClipRectangle
    {
        get
        {
            return new Rectangle
                (
                    (int)(-m_clip_rectangle.X * m_zoom_factor),
                    (int)(-m_clip_rectangle.Y * m_zoom_factor),
                    (int)(m_clip_rectangle.Width * m_zoom_factor),
                    (int)(m_clip_rectangle.Height * m_zoom_factor)
                );
        }
    }

    float m_zoom_factor = 1.0F;
    [Category("Appearance"), Description("The zoom factor. Less than 1 to reduce. More than 1 to magnify.")]
    public float ZoomFactor
    {
        get { return m_zoom_factor; }
        set
        {
            if (value <= 0.00001F)
                value = 0.1F;
            m_zoom_factor = value;
            this.Invalidate();
        }
    }

    InterpolationMode m_interpolation_mode = InterpolationMode.Low;
    [Category("Appearance"), Description("The interpolation mode used to smooth the drawing")]
    public InterpolationMode InterpolationMode
    {
        get { return m_interpolation_mode; }
        set { m_interpolation_mode = value; }
    }

    SmoothingMode m_smoothing_mode = SmoothingMode.None;
    [Category("Appearance"), Description("The smoothing mode used to smooth the drawing")]
    public SmoothingMode SmoothingMode
    {
        get { return m_smoothing_mode; }
        set { m_smoothing_mode = value; }
    }

    public PictureBoxEx()
    {
        //Set up double buffering and a little extra.
        this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                      ControlStyles.UserPaint |
                      ControlStyles.ResizeRedraw |
                      ControlStyles.OptimizedDoubleBuffer |
                      ControlStyles.SupportsTransparentBackColor |
                      ControlStyles.DoubleBuffer, true);

        //set the part of the source image to be drawn.
        m_clip_rectangle = PadRectangle(ClientRectangle, Padding);

        base.OnCreateControl();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        //if no image, don't bother
        if (m_image == null)
        {
            base.OnPaintBackground(e);
            return;
        }
        
        ////Set up a zoom matrix
        //Matrix matrix = new Matrix(zoom_factor, 0, 0, zoom_factor, 0, 0);

        ////now translate the matrix into position for the scrollbars
        ////mx.Translate(this.AutoScrollPosition.X / zoom_factor, this.AutoScrollPosition.Y / zoom_factor);
        //matrix.Translate(clip.X / zoom_factor, clip.Y / zoom_factor);

        ////use the transform
        //e.Graphics.Transform = matrix;

        if (m_image != null)
        {
            try
            {
                e.Graphics.InterpolationMode = m_interpolation_mode;
                e.Graphics.SmoothingMode = m_smoothing_mode;
                e.Graphics.DrawImage(m_image, ClientRectangle, m_clip_rectangle, GraphicsUnit.Pixel);
            }
            catch
            {
                // silence exception
            }
        }
        base.OnPaint(e);
    }
    protected override void OnPaintBackground(PaintEventArgs e)
    {
        // block event
    }
    protected override void OnResize(EventArgs e)
    {
        m_clip_rectangle.Width += (ClientRectangle.Width - m_previous_client_width) / m_zoom_factor;
        m_clip_rectangle.Height += (ClientRectangle.Height - m_previous_client_height) / m_zoom_factor;
        m_previous_client_width = ClientRectangle.Width;
        m_previous_client_height = ClientRectangle.Height;

        if (m_image != null)
        {
            if (m_clip_rectangle.X < 0 - Padding.Left)
                m_clip_rectangle.Location = new PointF(0 - Padding.Left, m_clip_rectangle.Y);

            if (m_clip_rectangle.Y < 0 - Padding.Top)
                m_clip_rectangle.Location = new PointF(m_clip_rectangle.X, 0 - Padding.Top);

            if (m_clip_rectangle.X > m_image.Width - m_clip_rectangle.Width + Padding.Right)
                m_clip_rectangle.Location = new PointF(m_image.Width - m_clip_rectangle.Width + Padding.Right, m_clip_rectangle.Y);

            if (m_clip_rectangle.Y > m_image.Height - m_clip_rectangle.Height + Padding.Bottom)
                m_clip_rectangle.Location = new PointF(m_clip_rectangle.X, m_image.Height - m_clip_rectangle.Height + Padding.Bottom);
        }

        this.Invalidate(); // Invalidate self only after all other message processed using PostMessage

        base.OnResize(e);
    }
    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            m_is_panning = true;
            m_start_point = new PointF(e.X, e.Y);
            m_current_point = m_start_point;

            base.OnMouseDown(e);
        }
    }
    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (m_is_panning)
        {
            m_current_point = new PointF(e.X, e.Y);
            if (m_image != null)
            {
                float dx = (m_current_point.X - m_start_point.X) / m_zoom_factor;
                float dy = (m_current_point.Y - m_start_point.Y) / m_zoom_factor;
                m_clip_rectangle.Location = new PointF(m_clip_rectangle.X - dx, m_clip_rectangle.Y - dy);
                m_start_point = m_current_point;

                if (m_clip_rectangle.Location.X < 0 - Padding.Left)
                    m_clip_rectangle.Location = new PointF(0 - Padding.Left, m_clip_rectangle.Location.Y);

                if (m_clip_rectangle.Location.Y < 0 - Padding.Top)
                    m_clip_rectangle.Location = new PointF(m_clip_rectangle.Location.X, 0 - Padding.Top);

                if (m_clip_rectangle.Location.X > m_image.Width - m_clip_rectangle.Width + Padding.Right)
                    m_clip_rectangle.Location = new PointF(m_image.Width - m_clip_rectangle.Width + Padding.Right, m_clip_rectangle.Location.Y);

                if (m_clip_rectangle.Location.Y > m_image.Height - m_clip_rectangle.Height + Padding.Bottom)
                    m_clip_rectangle.Location = new PointF(m_clip_rectangle.Location.X, m_image.Height - m_clip_rectangle.Height + Padding.Bottom);

                this.Invalidate(); // Invalidate self only after all other message processed using PostMessage

                base.OnMouseMove(e);
            }
        }
    }
    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (m_is_panning)
        {
            this.Invalidate(); // Invalidate self only after all other message processed using PostMessage

            m_is_panning = false;
        }

        base.OnMouseUp(e);
    }
    protected override void OnMouseWheel(MouseEventArgs e)
    {
        if (!m_is_panning)
        {
            if (m_image != null)
            {
                if (e.Delta >= 0) // ZoomIn
                {
                    m_zoom_step++;
                    if (m_zoom_step <= m_max_zoom_steps)
                    {
                        m_zoom_factor *= 1.1F;
                    }
                    else
                    {
                        m_zoom_step = m_max_zoom_steps;
                    }
                }
                else // ZoomOut
                {
                    m_zoom_step--;
                    if (m_zoom_step >= 1)
                    {
                        m_zoom_factor /= 1.1F;
                    }
                    else
                    {
                        m_zoom_step = 1;
                    }
                }

                // find zoom_point to home on
                m_zoom_point = new PointF(e.X, e.Y);
                ZoomOnPoint(m_zoom_point, m_zoom_factor);

                base.OnMouseWheel(e);
            }
        }
    }
    public void ZoomIn()
    {
        MouseEventArgs e = new MouseEventArgs(MouseButtons.Left, 1, (int)m_zoom_point.X, (int)m_zoom_point.Y, 1);
        OnMouseWheel(e);
    }
    public void ZoomOut()
    {
        MouseEventArgs e = new MouseEventArgs(MouseButtons.Left, 1, (int)m_zoom_point.X, (int)m_zoom_point.Y, -1);
        OnMouseWheel(e);
    }

    private void ZoomOnPoint(PointF zoom_point, float zoom_factor)
    {
        m_clip_rectangle.Width = ClientRectangle.Width / zoom_factor;
        m_clip_rectangle.Height = ClientRectangle.Height / zoom_factor;
        //??? m_clip_rectangle.X = ?;
        //??? m_clip_rectangle.Y = ?;

        if (m_image != null)
        {
            if (m_clip_rectangle.X < 0 - Padding.Left)
                m_clip_rectangle.Location = new PointF(0 - Padding.Left, m_clip_rectangle.Y);

            if (m_clip_rectangle.Y < 0 - Padding.Top)
                m_clip_rectangle.Location = new PointF(m_clip_rectangle.X, 0 - Padding.Top);

            if (m_clip_rectangle.X > m_image.Width - m_clip_rectangle.Width + Padding.Right)
                m_clip_rectangle.Location = new PointF(m_image.Width - m_clip_rectangle.Width + Padding.Right, m_clip_rectangle.Y);

            if (m_clip_rectangle.Y > m_image.Height - m_clip_rectangle.Height + Padding.Bottom)
                m_clip_rectangle.Location = new PointF(m_clip_rectangle.X, m_image.Height - m_clip_rectangle.Height + Padding.Bottom);
        }

        this.Invalidate(); // Invalidate self only after all other message processed using PostMessage
    }
    private RectangleF PadRectangle(RectangleF rectangle, Padding padding)
    {
        rectangle.X += padding.Left;
        rectangle.Y += padding.Top;
        rectangle.Width -= padding.Horizontal;
        rectangle.Height -= padding.Vertical;
        return rectangle;
    }
}
