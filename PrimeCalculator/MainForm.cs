using System;
using System.Text;
using System.Data;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;
using System.Reflection;
using System.Runtime.InteropServices;
using System.IO;

namespace PrimeCalculator
{
    public partial class MainForm : Form
    {
        private Factorizer m_factorizer = null;
        private Thread m_worker_thread = null;

        public const int DEFAULT_SLEEP_TIME = 40; // ms
        private int m_sleep_time = DEFAULT_SLEEP_TIME;
        public int SleepTime
        {
            get { return m_sleep_time; }
            set { m_sleep_time = value; }
        }

        public long Number
        {
            get
            {
                try
                {
                    return long.Parse(NumberTextBox.Text);
                }
                catch
                {
                    return -1L;
                }
            }
            set
            {
                NumberTextBox.Text = value.ToString();
            }
        }

        public MainForm()
        {
            InitializeComponent();

            m_filename = Application.ProductName + ".ini";
            LoadSettings();
        }

        private string m_filename;
        private void LoadSettings()
        {
            if (File.Exists(m_filename))
            {
                using (StreamReader reader = File.OpenText(m_filename))
                {
                    try
                    {
                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine();
                            string[] parts = line.Split('=');
                            if (parts.Length == 2)
                            {
                                switch (parts[0])
                                {
                                    case "Top":
                                        {
                                            this.Top = int.Parse(parts[1]);
                                        }
                                        break;
                                    case "Left":
                                        {
                                            this.Left = int.Parse(parts[1]);
                                        }
                                        break;
                                    //case "Width":
                                    //    {
                                    //        this.Width = int.Parse(parts[1]);
                                    //    }
                                    //    break;
                                    //case "Height":
                                    //    {
                                    //        this.Height = int.Parse(parts[1]);
                                    //    }
                                    //    break;
                                }
                            }
                        }
                    }
                    catch
                    {
                        this.Top = 0;
                        this.Left = 0;
                    }
                }
            }
            else // first start
            {
                this.Top = (Screen.PrimaryScreen.Bounds.Height / 2) - (this.Height / 2);
                this.Left = (Screen.PrimaryScreen.Bounds.Width / 2) - (this.Width / 2);
            }
        }
        private void SaveSettings()
        {
            try
            {
                using (StreamWriter writer = File.CreateText(m_filename))
                {
                    writer.WriteLine("[Window]");
                    writer.WriteLine("Top=" + this.Top);
                    writer.WriteLine("Left=" + this.Left);
                    //writer.WriteLine("Width=" + this.Width);
                    //writer.WriteLine("Height=" + this.Height);
                }
            }
            catch
            {
                // silence IO error in case running from read-only media (CD/DVD)
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            m_factorizer = null;
            m_worker_thread = null;
        }
        private void MainForm_Shown(object sender, EventArgs e)
        {
            NotifyIcon.Visible = true;
        }
        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Run();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                Stop();
            }
        }
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            CloseApplication();
        }
        private void CloseApplication()
        {
            if (m_worker_thread != null)
            {
                m_worker_thread.Join(); // wait for workerThread to terminate
                m_worker_thread = null;
            }

            NotifyIcon.Visible = false;
            NotifyIcon.Dispose();

            SaveSettings();
        }

        private void FixMicrosoft(object sender, KeyPressEventArgs e)
        {
            // stop annoying beep due to parent not having an AcceptButton
            if ((e.KeyChar == (char)Keys.Enter) || (e.KeyChar == (char)Keys.Escape))
            {
                e.Handled = true;
            }
            // enable Ctrl+A to SelectAll in TextBox and RichTextBox
            if ((ModifierKeys == Keys.Control) && (e.KeyChar == (char)1))
            {
                TextBoxBase control = (sender as TextBoxBase);
                if (control != null)
                {
                    control.SelectAll();
                    e.Handled = true;
                }
            }
        }
        private void NumberTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            FixMicrosoft(sender, e);
        }
        private void NumberTextBox_TextChanged(object sender, EventArgs e)
        {
            ResultTextBox.Text = string.Empty;
            ResultTextBox.Refresh();

            FactorsTextBox.Text = string.Empty;
            FactorsTextBox.Refresh();

            DigitSumTextBox.Text = string.Empty;
            DigitSumTextBox.Refresh();

            ElapsedTimeValueLabel.Text = "00:00:00";
            ElapsedTimeValueLabel.Refresh();
            MilliSecondsLabel.Text = "000";
            MilliSecondsLabel.Refresh();

            ProgressBar.Value = 0;
            ProgressValueLabel.Text = ProgressBar.Value + "%";
            ProgressValueLabel.Refresh();
        }
        private void EnableEntryControls()
        {
            NumberTextBox.Enabled = true;
            NumberTextBox.ForeColor = SystemColors.ControlText;
            NumberTextBox.Refresh();

            NumberTextBox.Focus(); // will show the InputPanel too

            m_old_progress = -1;
            ProgressLabel.Text = "Progress";
            ProgressLabel.Refresh();
        }
        private void DisableEntryControls()
        {
            NumberTextBox.Enabled = false;
            NumberTextBox.ForeColor = SystemColors.ControlDark;
            NumberTextBox.Refresh();

            ProgressLabel.Text = "IsPrime ...";
            ProgressLabel.Refresh();
        }

        private void ClearResult()
        {
            ResultTextBox.Text = string.Empty;
            ResultTextBox.Refresh();
        }
        private void ClearFactors()
        {
            FactorsTextBox.Text = string.Empty;
            FactorsTextBox.Refresh();
        }
        private void ClearProgress()
        {
            ElapsedTimeValueLabel.Text = "00:00:00";
            ElapsedTimeValueLabel.Refresh();
            MilliSecondsLabel.Text = "000";
            MilliSecondsLabel.Refresh();
            ProgressBar.Value = 0;
            ProgressValueLabel.Text = ProgressBar.Value + "%";
            ProgressValueLabel.Refresh();
        }
        private void BeforeProcessing()
        {
            DisableEntryControls();
            ClearResult();
            ClearFactors();
            ClearProgress();
        }
        private void AfterCancelled()
        {
            ResultTextBox.Text = "Cancelled";
            ResultTextBox.ForeColor = Color.Gray;
            ResultTextBox.Refresh();

            ClearProgress();
            EnableEntryControls();
        }
        private void AfterProcessing()
        {
            if (m_factorizer != null)
            {
                if (m_factorizer.IsPrime == null)	// number = 0 or 1 only, no cancelling here
                {
                    EnableEntryControls();

                    if (m_factorizer.Number == 0L)
                    {
                        ResultTextBox.Text = "Not Natural";
                        ResultTextBox.ForeColor = Color.Gray;
                        ResultTextBox.Refresh();
                    }
                    else if (m_factorizer.Number == 1L)
                    {
                        ResultTextBox.Text = "The Unit";
                        ResultTextBox.ForeColor = Color.Purple;
                        ResultTextBox.Refresh();
                    }
                    else
                    {
                        ResultTextBox.ForeColor = Color.Red;
                        ResultTextBox.Text = "Invalid";
                        ResultTextBox.Refresh();
                    }
                }
                else if (m_factorizer.IsPrime == true)
                {
                    EnableEntryControls();
                    this.Cursor = Cursors.WaitCursor;

                    if (Numbers.IsPurePrime(m_factorizer.Number))
                    {
                        ResultTextBox.ForeColor = Color.Purple;
                        ResultTextBox.Text = "Pure Prime";
                        FactorsTextBox.ForeColor = Color.Purple;
                    }
                    else if (Numbers.IsAdditivePrime(m_factorizer.Number))
                    {
                        ResultTextBox.ForeColor = Color.Blue;
                        ResultTextBox.Text = "Additive Prime";
                        FactorsTextBox.ForeColor = Color.Blue;
                    }
                    else //if (Numbers.IsPrime(p_factorizer.Number))
                    {
                        ResultTextBox.ForeColor = Color.Green;
                        ResultTextBox.Text = "Prime";
                        FactorsTextBox.ForeColor = Color.Green;
                    }
                    ResultTextBox.Refresh();

                    int prime_index = Numbers.IndexOfPrime(m_factorizer.Number);
                    int additive_prime_index = Numbers.IndexOfAdditivePrime(m_factorizer.Number);
                    int pure_prime_index = Numbers.IndexOfPurePrime(m_factorizer.Number);

                    FactorsTextBox.Text =
                        ((prime_index > 0) ? ("P_" + prime_index) : Numbers.FactorizeToString(m_factorizer.Number)) + "\r\n" +
                        ((additive_prime_index > 0) ? ("  AP_" + additive_prime_index) : "") + "\r\n" +
                        ((pure_prime_index > 0) ? ("  PP_" + pure_prime_index) : "")
                        ;
                    if (FactorsTextBox.Text == "")
                    {
                        FactorsTextBox.Text = m_factorizer.Number.ToString();
                    }
                    FactorsTextBox.Refresh();

                    if (Numbers.IsPurePrime(Numbers.DigitSum(m_factorizer.Number)))
                    {
                        DigitSumTextBox.ForeColor = Color.Purple;
                    }
                    else if (Numbers.IsAdditivePrime(Numbers.DigitSum(m_factorizer.Number)))
                    {
                        DigitSumTextBox.ForeColor = Color.Blue;
                    }
                    else if (Numbers.IsPrime(Numbers.DigitSum(m_factorizer.Number)))
                    {
                        DigitSumTextBox.ForeColor = Color.Green;
                    }
                    else
                    {
                        DigitSumTextBox.ForeColor = Color.Black;
                    }
                    DigitSumTextBox.Text = Numbers.DigitSum(m_factorizer.Number).ToString();
                    DigitSumTextBox.Refresh();

                    this.Cursor = Cursors.Default;
                }
                else if (m_factorizer.IsPrime == false)
                {
                    EnableEntryControls();

                    ResultTextBox.Text = "Composite";
                    ResultTextBox.ForeColor = Color.Black;
                    ResultTextBox.Refresh();
                    FactorsTextBox.ForeColor = Color.Black;
                    FactorsTextBox.Refresh();
                }
            }
        }
        public void Run()
        {
            BeforeProcessing();

            long number = 0;
            try
            {
                number = long.Parse(NumberTextBox.Text);
            }
            catch
            {
                try
                {
                    // evaluate expression and display result and continue
                    string result = Calculator.Evaluate(NumberTextBox.Text);
                    number = (long)Math.Round((double.Parse(result)));
                    NumberTextBox.Text = number.ToString();

                    NumberTextBox.SelectionStart = NumberTextBox.Text.Length;
                    NumberTextBox.SelectionLength = 0;
                    NumberTextBox.Refresh();
                }
                catch
                {
                    // force calcellation programmatically
                    number = -1L;
                    Stop();
                }
            }

            m_factorizer = new Factorizer(this, number);
            if (m_factorizer != null)
            {
                m_worker_thread = new Thread(new ThreadStart(m_factorizer.Run));
                m_worker_thread.Priority = ThreadPriority.Lowest;
                m_worker_thread.IsBackground = true;
                m_worker_thread.Start();
            }
        }
        public void Stop()
        {
            if (m_factorizer != null)
            {
                m_factorizer.Cancel = true;
                if (m_worker_thread != null)
                {
                    m_worker_thread.Join();
                    m_worker_thread = null;
                }

                if (m_factorizer.IsPrime == null) // processing cancelled
                {
                    AfterCancelled();
                }
            }
            else // cancellation triggered by exception
            {
                AfterCancelled();
            }
        }

        private enum TimeDisplayMode { Elapsed, Remaining }
        private TimeDisplayMode m_time_display_mode = TimeDisplayMode.Elapsed;
        private void ElapsedTimeLabel_Click(object sender, EventArgs e)
        {
            if ((m_worker_thread != null) && (m_worker_thread.IsAlive))
            {
                if (m_time_display_mode == TimeDisplayMode.Elapsed)
                {
                    ElapsedTimeLabel.Text = "Remaining Time";
                    m_time_display_mode = TimeDisplayMode.Remaining;
                }
                else
                {
                    ElapsedTimeLabel.Text = "Elapsed Time";
                    m_time_display_mode = TimeDisplayMode.Elapsed;
                }
            }
        }

        private int m_old_progress = -1;
        public delegate void UpdateProgressBar(int progress);
        public void UpdateProgressBarMethod(int progress)
        {
            if (progress > m_old_progress)
            {
                m_old_progress = progress;
                if (m_factorizer != null)
                {
                    if (!m_factorizer.Cancel)
                    {
                        if ((progress >= 0) && (progress <= 100))
                        {
                            ProgressBar.Value = progress;
                            ProgressValueLabel.Text = ProgressBar.Value + "%";
                            ProgressValueLabel.Refresh();

                            UpdateTimer(progress);
                        }

                        if (progress == 100) // finished naturally 
                        {
                            AfterProcessing();
                        }
                    }
                }
            }
            else // no new progress
            {
                if (m_time_display_mode == TimeDisplayMode.Elapsed)
                {
                    if (m_factorizer != null)
                    {
                        UpdateTimer(progress);
                    }
                }
                else //if (m_time_display_mode == TimeDisplayMode.Remaining)
                {
                    // don't update remaining as it goes up wrongly with time passage and no progress
                    // if progree was double not int then ok but now
                    // it keeps going up up up till it there is progress so t goes down to the real remaining time and then
                    // it keeps going up up up again and so on, like a seesaw
                }
            }
        }

        private void UpdateTimer(int progress)
        {
            TimeSpan timespan = m_factorizer.Duration;
            if (m_time_display_mode == TimeDisplayMode.Remaining)
            {
                if (progress > 0)
                {
                    long ticks = (long)((double)timespan.Ticks * ((double)(100 - progress) / (double)progress));
                    timespan = new TimeSpan(ticks);
                }
                else
                {
                    timespan = new TimeSpan(0, 0, 0);
                }
            }
            ElapsedTimeValueLabel.Text = String.Format("{0:00}:{1:00}:{2:00}", timespan.Hours, timespan.Minutes, timespan.Seconds);
            ElapsedTimeValueLabel.Refresh();
            MilliSecondsLabel.Text = String.Format("{0:000}", timespan.Milliseconds);
            MilliSecondsLabel.Refresh();
        }

        public delegate void UpdateFactorsTextBox(string multiplied_factors);
        public void UpdateFactorsTextBoxMethod(string multiplied_factors)
        {
            if (m_factorizer != null)
            {
                if (!m_factorizer.Cancel)
                {
                    if (m_factorizer.IsPrime == false)
                    {
                        ResultTextBox.Text = "Composite";
                        ResultTextBox.ForeColor = Color.Black;
                        ResultTextBox.Refresh();

                        FactorsTextBox.ForeColor = Color.Black;
                        ResultTextBox.Refresh();

                        ProgressLabel.Text = "Factoring ...";
                        ProgressLabel.Refresh();
                    }

                    FactorsTextBox.Text = multiplied_factors;
                    FactorsTextBox.Refresh();
                    FactorsTextBox.SelectionStart = FactorsTextBox.Text.Length;
                    FactorsTextBox.ScrollToCaret();
                }
            }
        }

        private void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // toggle visible
                this.Visible = true;

                // restore if minimized
                if (this.WindowState == FormWindowState.Minimized)
                {
                    this.WindowState = FormWindowState.Normal;
                }

                // and bring to foreground
                this.Activate();
            }
        }
        private void NotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //if (e.Button == MouseButtons.Left)
            //{
            //    // make visible (in case it is hidden)
            //    this.Visible = true;

            //    // toggle maximized
            //    if (this.WindowState == FormWindowState.Maximized)
            //    {
            //        this.WindowState = FormWindowState.Normal;
            //    }
            //    else
            //    {
            //        this.WindowState = FormWindowState.Maximized;
            //    }

            //    // and bring to foreground
            //    this.Activate();
            //}
        }
        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show
            (
                Application.ProductName + "  v" + Application.ProductVersion + "\r\n" +
                "Copyright © 2008 Ali Adams" + "\r\n" + "\r\n" +
                "God > infinity",
                "About",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1
            );
        }
        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CloseApplication();

            Application.Exit();
            System.Environment.Exit(0);
        }

        private void WebsiteLabel_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://www.heliwave.com");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
