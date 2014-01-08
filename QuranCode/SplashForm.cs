using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

// Splash screen adapted from
// http://stackoverflow.com/questions/48916/multi-threaded-splash-screen-in-c
public partial class SplashForm : Form
{
    private const int SLEEP_TIME = 50;

    public SplashForm()
    {
        InitializeComponent();

        update_version_function = this.UpdateVersionInternal;
        update_information_function = this.UpdateInformationInternal;
        update_progress_function = this.UpdateProgressInternal;
    }

    private delegate void VersionDelegate(string value);
    private VersionDelegate update_version_function = null;
    private void UpdateVersionInternal(string value)
    {
        if (this.Handle == null)
        {
            return;
        }
        this.VersionLabel.Text = value;
        this.VersionLabel.Refresh();
    }
    public string Version
    {
        get { return VersionLabel.Text; }
        set
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(update_version_function, value);
            }
            else
            {
                try
                {
                    // recursive retry
                    Thread.Sleep(SLEEP_TIME);
                    Version = value;
                }
                catch (StackOverflowException /*ex*/)
                {
                    this.Close();
                }
            }
        }
    }

    private delegate void InformationDelegate(string value);
    private InformationDelegate update_information_function = null;
    private void UpdateInformationInternal(string value)
    {
        if (this.Handle == null)
        {
            return;
        }
        this.InformationLabel.Text = value;
        this.InformationLabel.Refresh();
    }
    public string Information
    {
        get { return InformationLabel.Text; }
        set
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(update_information_function, value);
            }
            else
            {
                try
                {
                    // recursive retry
                    Thread.Sleep(SLEEP_TIME);
                    Information = value;
                }
                catch (StackOverflowException /*ex*/)
                {
                    this.Close();
                }
            }
        }
    }

    private delegate void ProgressDelegate(int value);
    private ProgressDelegate update_progress_function = null;
    private void UpdateProgressInternal(int value)
    {
        if (this.Handle == null)
        {
            return;
        }
        this.ProgressBar.Value = value;
        this.ProgressBar.Refresh();
    }
    public int Progress
    {
        get { return ProgressBar.Value; }
        set
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(update_progress_function, value);
            }
            else
            {
                try
                {
                    // recursive retry
                    Thread.Sleep(SLEEP_TIME);
                    Progress = value;
                }
                catch (StackOverflowException /*ex*/)
                {
                    this.Close();
                }
            }
        }
    }

    private void LinkLabel_Click(object sender, EventArgs e)
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            Control control = (sender as Control);
            if (control != null)
            {
                if (control.Tag != null)
                {
                    if (!String.IsNullOrEmpty(control.Tag.ToString()))
                    {
                        try
                        {
                            System.Diagnostics.Process.Start(control.Tag.ToString());
                        }
                        catch
                        {
                            //MessageBox.Show(ex.Message, Application.ProductName);
                        }
                    }
                }
            }
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }
}
