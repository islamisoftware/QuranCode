using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Media;
using System.Threading;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using System.CodeDom.Compiler;
using Model;

public partial class MainForm : Form
{
    // Method Names Convention
    // Update writes to m_client
    // Refresh reads from m_client or reclculates within MainForm

    private void FixMicrosoft(object sender, KeyPressEventArgs e)
    {
        // stop annoying beep due to parent not having an AcceptButton
        if ((e.KeyChar == (char)Keys.Enter) || (e.KeyChar == (char)Keys.Escape))
        {
            e.Handled = true;
        }
        // enable Ctrl+A to SelectAll
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

    private const long DEFAULT_RADIX = 10; // base for current numbering system. decimal by default.
    private const float DEFAULT_TEXT_ZOOM_FACTOR = 1.0F;
    private const float DEFAULT_GRAPHICS_ZOOM_FACTOR = 1.0F;
    private const float DEFAULT_FONT_SIZE = 14.0F;
    private const int DEFAULT_WINDOW_WIDTH = 993;
    private const int DEFAULT_WINDOW_HEIGHT = 691;
    private const bool DEFAULT_TRANSLATION_BOX_VISIBLE = true;
    private const bool DEFAULT_TRANSLATOR_COMBOBOX_VISIBLE = true;
    private const int DEFAULT_TRANSLATION_BOX_TOP = 420;
    private const float DEFAULT_TRANSLATION_FONT_SIZE = 10.0F;
    private const float DEFAULT_DPI_X = 96.0F; // 100% = 96.0F,   125% = 120.0F,   150% = 144.0F
    //private const float DEFAULT_DPI_Y = 96.0F;

    private const string DEFAULT_EMLAAEI = "ar.emlaaei";
    private const string DEFAULT_TAFSEER = "ar.jalalayn";
    private const string DEFAULT_TRANSLATION = "en.pickthall";
    private const string DEFAULT_TRANSLATION_2 = "en.qarai";
    private const string DEFAULT_TRANSLITERATION = "en.transliteration";

    private const string DEFAULT_RECITATION = "Alafasy_64kbps";
    private const int DEFAULT_AUDIO_VOLUME = 1000;
    private const float DEFAULT_SILENCE_BETWEEN_VERSES = 0.0F; // in multiples of verses
    private const int MAX_SELECTON_SCOPE_LENGTH = 16;
    private const string CAPTION_SEPARATOR = " ► ";

    private Font m_font = null;
    private float m_font_size = DEFAULT_FONT_SIZE;
    private void SetFontSize(float size)
    {
        if (m_font == null)
        {
            m_font = FontBuilder.Build("QuranCode.Fonts.me_quran.ttf", size);
            //m_font = FontBuilder.Build("QuranCode.Fonts.uthmani.otf", size * 2);
            //m_font = FontBuilder.Build("QuranCode.Fonts.akshar.ttf", size);
        }
        else
        {
            m_font = new Font("me_quran.ttf", size);
            //m_font = new Font("uthmani.otf", size * 2);
            //m_font = new Font("akshar.ttf", size);
        }
        MainTextBox.Font = m_font;
        MainTextBox.Refresh();
    }
    private float m_translation_font_size = DEFAULT_TRANSLATION_FONT_SIZE;
    private void SetTranslationFontSize(float size)
    {
        //m_translation_font_size = size; // don't overwrite but let zooming scale it up/down
        TranslationTextBox.Font = new Font("Microsoft San Serif", size, FontStyle.Bold);
        TranslationTextBox.Refresh();
    }

    private string m_ini_filename = "";

    private Client m_client = null;
    private string m_current_text = "";

    // any visibility must be set after form is shown
    private bool m_show_translation_box = DEFAULT_TRANSLATION_BOX_VISIBLE;
    private bool m_show_translator_combobox = DEFAULT_TRANSLATOR_COMBOBOX_VISIBLE;
    private int m_translation_box_top = DEFAULT_TRANSLATION_BOX_TOP;

    private AboutBox m_about_box = null;
    private PermissionSet m_permission_set = null;
    private float m_dpi_x = DEFAULT_DPI_X;
    //private float m_dpi_y = DEFAULT_DPI_Y;

    public MainForm()
    {
        InitializeComponent();
        this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);

        m_about_box = new AboutBox();

        m_permission_set = new PermissionSet(PermissionState.None);
        m_permission_set.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
        m_permission_set.AddPermission(new ReflectionPermission(ReflectionPermissionFlag.MemberAccess));
        m_permission_set.AddPermission(new UIPermission(UIPermissionWindow.SafeSubWindows));
        m_permission_set.AddPermission(new IsolatedStorageFilePermission(PermissionState.Unrestricted));

        using (Graphics graphics = this.CreateGraphics())
        {
            m_dpi_x = graphics.DpiX;
            //m_dpi_y = graphics.DpiY;
            if (m_dpi_x != DEFAULT_DPI_X)
            {
                if (m_dpi_x == 120.0F)
                {
                    // adjust GUI to fit into 125%
                    MainSplitContainer.Height = (int)(MainSplitContainer.Height / (m_dpi_x / DEFAULT_DPI_X)) + 96;
                    //MainSplitContainer.Width = (int)(MainSplitContainer.Width / (m_dpi_x / DEFAULT_DPI_X)) - 46;
                    MainSplitContainer.SplitterDistance = 215;
                }
            }
        }

        LetterValuesPanel.Dock = DockStyle.Fill;
        LetterValuesPanel.Visible = false;
        LetterValuesRestoreButton.Enabled = false;
        LetterValuesCloseButton.Enabled = true;
        LetterValuesSaveButton.Enabled = false;

        //ini_filename = AppDomain.CurrentDomain.FriendlyName.Replace(".exe", ".ini"); // BAD: Same ini_filename for all projects
        string[] parts = Assembly.GetExecutingAssembly().FullName.Split(',');
        m_ini_filename = parts[0] + ".ini";

        this.MainTextBox.HideSelection = false; //??? this won't shift the text to the left
        //this.MainTextBox.HideSelection = true; //??? this will shift the text to the left
        this.MainTextBox.MouseWheel += new MouseEventHandler(MainTextBox_MouseWheel);
        this.DrawingPictureBoxEx.MouseWheel += new MouseEventHandler(PictureBoxEx_MouseWheel);
    }
    private void MainForm_Load(object sender, EventArgs e)
    {
        bool splash_screen_done = false;
        try
        {
            SplashForm splash_form = new SplashForm();
            ThreadPool.QueueUserWorkItem(delegate
            {
                using (splash_form)
                {
                    splash_form.Show();
                    while (!splash_screen_done)
                    {
                        Application.DoEvents();
                    }
                    splash_form.Close();
                }
            }, null);


            try
            {
                splash_form.Version += "  " + Globals.SHORT_VERSION;

                InitializeControls();
                splash_form.Progress = 20;
                Thread.Sleep(100);

                string machine = "local";
                string username = "username";
                string password = "password";
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                if (identity != null)
                {
                    string[] parts = identity.Name.Split('\\');
                    if (parts.Length == 1)
                    {
                        username = parts[0];
                    }
                    if (parts.Length == 2)
                    {
                        machine = parts[0];
                        username = parts[1];
                    }
                    else
                    {
                        username = parts[parts.Length - 1];
                    }
                }
                m_client = new Client(machine, username, password); // password not used
                if (m_client != null)
                {
                    splash_form.Information = "Initializing server ...";
                    splash_form.Information = "Loading translations ...";
                    if (Globals.EDITION == Edition.Grammar)
                    {
                        splash_form.Information = "Building grammar infomation ...";
                    }

                    // must initialize the download urls before LoadApplicationOptions
                    Client.Initialize();

                    LoadApplicationOptions();

                    splash_form.Information = "Loading simplification rules ...";
                    Client.LoadSimplificationRules();
                    splash_form.Progress = 30;
                    Thread.Sleep(100);

                    splash_form.Information = "Building numerology systems ...";
                    UpdateNumerologySystems();
                    splash_form.Progress = 40;
                    Thread.Sleep(100);

                    splash_form.Information = "Loading user numerology systems ...";
                    Client.LoadNumerologySystems();
                    splash_form.Progress = 50;
                    Thread.Sleep(100);

                    splash_form.Information = "Building simplified texts ...";
                    if (Client.TextModes.Count > 0)
                    {
                        int progress_per_book = (80 - 50) / Client.TextModes.Count;
                        foreach (string text_mode in Client.TextModes)
                        {
                            if (text_mode == "Original")
                            {
                                splash_form.Information = "Generating primes numbers ...";
                            }
                            else
                            {
                                splash_form.Information = "Building " + text_mode + " text ...";
                            }
                            Client.BuildSimplifiedBook(text_mode);
                            splash_form.Progress += progress_per_book;
                        }
                    }
                    else
                    {
                        splash_form.Progress += (80 - 50) / 1;
                    }
                    PopulateTextModeComboBox();
                    Client.UpdateBookNumbersAndWordLetters(m_client.TextMode);
                    Thread.Sleep(100);

                    splash_form.Information = "Building translations ...";
                    PopulateTranslatorsCheckedListBox();
                    PopulateTranslatorComboBox();
                    splash_form.Progress = 90;
                    Thread.Sleep(100);

                    splash_form.Information = "Building reciter names ...";
                    PopulateRecitationsCheckedListBox();
                    PopulateRecitationsComboBox();
                    //MoveAudioFilesToFolders(); // upgrade old QuranCode structure
                    splash_form.Progress = 95;
                    Thread.Sleep(100);

                    splash_form.Information = "Loading application settings ...";
                    splash_form.Information = "Generating primes numbers ...";
                    LoadApplicationSettings();

                    if (!m_show_translation_box)
                    {
                        HideTranslationLabel_Click(null, null);
                    }
                    TranslatorComboBox.Visible = m_show_translator_combobox && m_show_translation_box;
                    splash_form.Progress = 97;
                    Thread.Sleep(100);

                    splash_form.Information = "Loading research methods ...";
                    LoadResearchMethods();
                    splash_form.Progress = 98;
                    Thread.Sleep(100);

                    try
                    {
                        splash_form.Information = "Loading user history ...";
                        m_client.LoadFindHistory();
                        UpdateFindHistoryButtons();
                        m_client.LoadBrowseHistory();
                        UpdateBrowseHistoryButtons();
                        splash_form.Progress = 100;
                        Thread.Sleep(100);

                        // load bookmarks of all texts of current client
                        m_client.LoadBookmarks();
                        RefreshBookmarkHistoryButtons();
                    }
                    catch (Exception ex)
                    {
                        while (ex != null)
                        {
                            //Console.WriteLine(ex.Message);
                            MessageBox.Show(ex.Message, Application.ProductName);
                            ex = ex.InnerException;
                        }
                    }

                    UpdateFindByTextControls();
                }

                HelpLabel.Text = Client.HelpMessages[0];
                splash_form.Information = "Starting ...";
                Thread.Sleep(200);

                this.Activate(); // annoying flashes, use the following code instead
            }
            catch (Exception ex)
            {
                while (ex != null)
                {
                    //Console.WriteLine(ex.Message);
                    MessageBox.Show(ex.Message, Application.ProductName);
                    ex = ex.InnerException;
                }
            }
            finally
            {
                splash_screen_done = true;
                Thread.Sleep(100);  // prevent race-condition to allow splashform.Close()
                GC.Collect();
            }
        }
        catch (Exception ex)
        {
            while (ex != null)
            {
                //Console.WriteLine(ex.Message);
                MessageBox.Show(ex.Message, Application.ProductName);
                ex = ex.InnerException;
            }
        }
    }
    private void MainForm_Shown(object sender, EventArgs e)
    {
        try
        {
            MainTextBox.AlignToStart();

            Verse.IncludeNumber = false;
            Verse.IncludeEndLine = true;

            if (RecitationsComboBox.SelectedItem != null)
            {
                RecitationGroupBox.Text = RecitationsComboBox.SelectedItem.ToString();
            }

            this.ActiveControl = MainTextBox;
            this.ShowInTaskbar = true;
            //NotifyIcon.Visible = true;
        }
        catch (Exception ex)
        {
            while (ex != null)
            {
                //Console.WriteLine(ex.Message);
                MessageBox.Show(ex.Message, Application.ProductName);
                ex = ex.InnerException;
            }
        }
    }
    private void MainForm_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.Handled = false;
        }
        else if (e.KeyCode == Keys.Escape)
        {
            HandleEscapeKeyPress(sender, e);
        }
        else if (e.Control && (e.KeyCode == Keys.Down)) // Redo
        {
            RedoGotoVerse();
        }
        else if (e.Control && (e.KeyCode == Keys.Up)) // Undo
        {
            UndoGotoVerse();
        }
        else
        {
            // do nothing
        }

        if (!e.Alt && !e.Control && !e.Shift)
        {
            if ((e.KeyCode == Keys.Back) || (e.KeyCode == Keys.BrowserBack))
            {
                if ((sender == MainTextBox) && (MainTextBox.ReadOnly))
                {
                    if (m_found_verses_displayed)
                    {
                        FindHistoryBackwardButton_Click(sender, e);
                    }
                    else
                    {
                        BrowseHistoryBackwardButton_Click(sender, e);
                    }
                }
            }
            else if ((e.KeyCode == Keys.BrowserForward))
            {
                if ((sender == MainTextBox) && (MainTextBox.ReadOnly))
                {
                    if (m_found_verses_displayed)
                    {
                        FindHistoryForwardButton_Click(sender, e);
                    }
                    else
                    {
                        BrowseHistoryForwardButton_Click(sender, e);
                    }
                }
            }
            else if (e.KeyCode == Keys.F1)
            {
            }
            else if (e.KeyCode == Keys.F2)
            {
                NextBookmarkButton_Click(sender, e);
            }
            else if (e.KeyCode == Keys.F3)
            {
                if (m_found_verses_displayed)
                {
                    FindNextMatch();
                }
            }
            else if (e.KeyCode == Keys.F4)
            {
                if (MainTextBox.Focused)
                {
                    DoFindExactMatch(MainTextBox);
                }
            }
            else if (e.KeyCode == Keys.F5)
            {
                if (MainTextBox.Focused)
                {
                    DoFindRelatedWords(MainTextBox);
                }
            }
            else if (e.KeyCode == Keys.F6)
            {
            }
            else if (e.KeyCode == Keys.F7)
            {
            }
            else if (e.KeyCode == Keys.F8)
            {
            }
            else if (e.KeyCode == Keys.F9)
            {
                if (MainTextBox.Focused)
                {
                    DoFindSameValue(MainTextBox);
                }
            }
            else if (e.KeyCode == Keys.F10)
            {
            }
            else if (e.KeyCode == Keys.F11)
            {
                WordWrapLabel_Click(sender, e);
            }
            else if (e.KeyCode == Keys.F12)
            {
                if (this.WindowState != FormWindowState.Maximized)
                {
                    this.WindowState = FormWindowState.Maximized;
                    this.FormBorderStyle = FormBorderStyle.None;
                }
                else
                {
                    this.WindowState = FormWindowState.Normal;
                    this.FormBorderStyle = FormBorderStyle.Sizable;
                }
            }
            else
            {
                // ignore key
            }
        }
        else if (!e.Alt && !e.Control && e.Shift)
        {
            if ((e.KeyCode == Keys.Back) || (e.KeyCode == Keys.BrowserBack))
            {
                if (MainTextBox.ReadOnly)
                {
                    if ((sender == MainTextBox) && (MainTextBox.ReadOnly))
                    {
                        if (m_found_verses_displayed)
                        {
                            FindHistoryForwardButton_Click(sender, e);
                        }
                        else
                        {
                            BrowseHistoryForwardButton_Click(sender, e);
                        }
                    }
                }
            }
            else if ((e.KeyCode == Keys.BrowserForward))
            {
                if ((sender == MainTextBox) && (MainTextBox.ReadOnly))
                {
                    if (m_found_verses_displayed)
                    {
                        FindHistoryBackwardButton_Click(sender, e);
                    }
                    else
                    {
                        BrowseHistoryBackwardButton_Click(sender, e);
                    }
                }
            }
            else if (e.KeyCode == Keys.F1)
            {
            }
            else if (e.KeyCode == Keys.F2)
            {
                PreviousBookmarkButton_Click(sender, e);
            }
            else if (e.KeyCode == Keys.F3)
            {
                if (m_found_verses_displayed)
                {
                    FindPreviousMatch();
                }
            }
            else if (e.KeyCode == Keys.F4)
            {
            }
            else if (e.KeyCode == Keys.F5)
            {
            }
            else if (e.KeyCode == Keys.F6)
            {
            }
            else if (e.KeyCode == Keys.F7)
            {
            }
            else if (e.KeyCode == Keys.F8)
            {
            }
            else if (e.KeyCode == Keys.F9)
            {
            }
            else if (e.KeyCode == Keys.F10)
            {
            }
            else if (e.KeyCode == Keys.F11)
            {
            }
            else if (e.KeyCode == Keys.F12)
            {
            }
            else
            {
                // ignore key
            }
        }
        else
        {
            // ignore key
        }
    }
    private void MainForm_Resize(object sender, EventArgs e)
    {
        if (this.WindowState == FormWindowState.Minimized)
        {
            //this.Visible = false;
        }

        if (DrawingPictureBoxEx.Visible)
        {
            RedrawCurrentGraph();
        }
        else
        {
            // this goes to top and changes selection and thus displayed value
            //MainTextBox.AlignToLineStart();
        }
    }
    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        // prevent user from closing from the X close button
        // allow user to close from the try NotifyIcon menu
        //if (e.CloseReason == CloseReason.UserClosing)
        //{
        //    e.Cancel = true;
        //    this.Visible = false;
        //}
    }
    private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
    {
        CloseApplication();
    }
    private void CloseApplication()
    {
        if (m_client != null)
        {
            try
            {
                // save current bookmark (if any)
                NoteTextBox_Leave(null, null);

                // save application options
                SaveApplicationOptions();

                // save bookmarks of all texts of current client
                m_client.SaveBookmarks();

                // save histories
                m_client.SaveFindHistory();
                m_client.SaveBrowseHistory();
            }
            catch
            {
                // silence IO error in case running from read-only media (CD/DVD)
            }
        }

        // remove icon from tray
        //NotifyIcon.Visible = false;
        //NotifyIcon.Dispose();
    }
    private void HandleEscapeKeyPress(object sender, KeyEventArgs e)
    {
        // back line indexes
        int backup_current_line_index = m_current_line_index;
        int backup_found_verses_current_line_index = m_found_verses_current_line_index;

        if (sender == NoteTextBox)
        {
            NoteTextBox.Text = "";
            NoteTextBox_Leave(null, null);
        }
        else
        {
            if (DrawingPictureBoxEx.Visible)
            {
                DrawingPictureBoxEx.Visible = false;

                DisplaySelection(sender, false);

                ZoomInLabel.Enabled = (m_text_zoom_factor <= (m_max_zoom_factor - m_zoom_factor_increment + m_error_margin));
                ZoomOutLabel.Enabled = (m_text_zoom_factor >= (m_min_zoom_factor + m_zoom_factor_increment - m_error_margin));
            }
            else
            {
                if (m_found_verses_displayed)
                {
                    DisplaySelection(sender, false);
                }
                else
                {
                    if (m_client != null)
                    {
                        if (m_client.FindHistory != null)
                        {
                            if (m_client.FindHistory.Count > 0)
                            {
                                DisplayFindHistoryItem(m_client.CurrentFindHistoryItem);
                            }
                        }
                    }
                }
            }
        }

        // restore line indexes
        m_current_line_index = backup_current_line_index;
        m_found_verses_current_line_index = backup_found_verses_current_line_index;

        int line_index = -1;
        if (m_found_verses_displayed)
        {
            line_index = m_found_verses_current_line_index;
        }
        else
        {
            line_index = m_current_line_index;
        }
        GotoLine(line_index);
    }
    private void UpdateVersesToCurrentTextMode(ref List<Verse> verses)
    {
        List<Verse> temp = new List<Verse>();
        foreach (Verse verse in verses)
        {
            temp.Add(Book.Instance.Verses[verse.Number - 1]);
        }
        verses = temp;
    }

    private void LoadApplicationSettings()
    {
        try
        {
            PopulateChapterComboBox();
            PopulateChaptersListBox();
            PopulateRecitationsComboBox();
            PopulateTranslatorComboBox();

            LoadApplicationOptions();

            // SetFontSize updates size BUT loses the font face in Right-to-left MainTextBox
            //SetFontSize(m_font_size);
            MainTextBox.ZoomFactor = m_text_zoom_factor;
            SetTranslationFontSize(m_translation_font_size * m_text_zoom_factor);
            DrawingPictureBoxEx.ZoomFactor = m_graphics_zoom_factor;

            RadixValueLabel.Text = m_radix.ToString();
            RefreshNumerologySystem();

            DisplaySelection(this, false);

            PlayerVolumeTrackBar.Value = m_audio_volume / (1000 / PlayerVolumeTrackBar.Maximum);
            PlayerSilenceTrackBar.Value = (int)(m_silence_between_verses * (PlayerSilenceTrackBar.Maximum / 2));
        }
        catch (Exception ex)
        {
            while (ex != null)
            {
                //Console.WriteLine(ex.Message);
                MessageBox.Show(ex.Message, Application.ProductName);
                ex = ex.InnerException;
            }
        }
    }
    private void LoadApplicationOptions()
    {
        if (File.Exists(m_ini_filename))
        {
            try
            {
                if (m_client != null)
                {
                    // Selection.Scope and Selection.Indexes are readonly so create a new Selection to replace Client.Selection 
                    SelectionScope selection_scope = SelectionScope.Book;
                    List<int> selection_indexes = new List<int>();

                    using (StreamReader reader = File.OpenText(m_ini_filename))
                    {
                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine();
                            string[] parts = line.Split('=');
                            if (parts.Length == 2)
                            {
                                switch (parts[0])
                                {
                                    // [Window]
                                    case "Top":
                                        {
                                            try
                                            {
                                                this.Top = int.Parse(parts[1].Trim());
                                            }
                                            catch
                                            {
                                                this.Top = 100;
                                            }
                                        }
                                        break;
                                    case "Left":
                                        {
                                            try
                                            {
                                                this.Left = int.Parse(parts[1].Trim());
                                            }
                                            catch
                                            {
                                                this.Left = 100;
                                            }
                                        }
                                        break;
                                    case "Width":
                                        {
                                            try
                                            {
                                                this.Width = int.Parse(parts[1].Trim());
                                            }
                                            catch
                                            {
                                                this.Width = DEFAULT_WINDOW_WIDTH;
                                            }
                                        }
                                        break;
                                    case "Height":
                                        {
                                            try
                                            {
                                                this.Height = int.Parse(parts[1].Trim());
                                            }
                                            catch
                                            {
                                                this.Height = DEFAULT_WINDOW_HEIGHT;
                                            }
                                        }
                                        break;
                                    case "TranslationBoxVisible":
                                        {
                                            try
                                            {
                                                m_show_translation_box = bool.Parse(parts[1].Trim());
                                            }
                                            catch
                                            {
                                                m_show_translation_box = DEFAULT_TRANSLATION_BOX_VISIBLE;
                                            }
                                        }
                                        break;
                                    case "TranslatorSelectionBoxVisible":
                                        {
                                            try
                                            {
                                                m_show_translator_combobox = bool.Parse(parts[1].Trim());
                                            }
                                            catch
                                            {
                                                m_show_translator_combobox = DEFAULT_TRANSLATOR_COMBOBOX_VISIBLE;
                                            }
                                        }
                                        break;
                                    case "TranslationBoxTop":
                                        {
                                            try
                                            {
                                                m_translation_box_top = int.Parse(parts[1].Trim());
                                            }
                                            catch
                                            {
                                                m_translation_box_top = DEFAULT_TRANSLATION_BOX_TOP;
                                            }
                                        }
                                        break;
                                    // [Numerology]
                                    case "Radix":
                                        {
                                            try
                                            {
                                                m_radix = long.Parse(parts[1].Trim());
                                            }
                                            catch
                                            {
                                                m_radix = DEFAULT_RADIX;
                                            }
                                            RadixValueLabel.Text = m_radix.ToString();
                                        }
                                        break;
                                    case "TextMode":
                                        {
                                            // read multiple related options together
                                            if (m_client != null)
                                            {
                                                m_client.TextMode = parts[1].Trim();

                                                line = reader.ReadLine();
                                                parts = line.Split('=');
                                                if (parts.Length == 2)
                                                {
                                                    m_client.LetterOrderSystem = parts[1].Trim();
                                                }

                                                line = reader.ReadLine();
                                                parts = line.Split('=');
                                                if (parts.Length == 2)
                                                {
                                                    m_client.LetterValueSystem = parts[1].Trim();
                                                }

                                                line = reader.ReadLine();
                                                parts = line.Split('=');
                                                if (parts.Length == 2)
                                                {
                                                    m_client.IsDynamic = bool.Parse(parts[1].Trim());
                                                }

                                                line = reader.ReadLine();
                                                parts = line.Split('=');
                                                if (parts.Length == 2)
                                                {
                                                    m_client.IsHighlightedText = bool.Parse(parts[1].Trim());
                                                }

                                                line = reader.ReadLine();
                                                parts = line.Split('=');
                                                if (parts.Length == 2)
                                                {
                                                    m_client.AddToLetterLNumber = bool.Parse(parts[1].Trim());
                                                }

                                                line = reader.ReadLine();
                                                parts = line.Split('=');
                                                if (parts.Length == 2)
                                                {
                                                    m_client.AddToLetterWNumber = bool.Parse(parts[1].Trim());
                                                }

                                                line = reader.ReadLine();
                                                parts = line.Split('=');
                                                if (parts.Length == 2)
                                                {
                                                    m_client.AddToLetterVNumber = bool.Parse(parts[1].Trim());
                                                }

                                                line = reader.ReadLine();
                                                parts = line.Split('=');
                                                if (parts.Length == 2)
                                                {
                                                    m_client.AddToLetterCNumber = bool.Parse(parts[1].Trim());
                                                }

                                                line = reader.ReadLine();
                                                parts = line.Split('=');
                                                if (parts.Length == 2)
                                                {
                                                    m_client.AddToLetterLDistance = bool.Parse(parts[1].Trim());
                                                }

                                                line = reader.ReadLine();
                                                parts = line.Split('=');
                                                if (parts.Length == 2)
                                                {
                                                    m_client.AddToLetterWDistance = bool.Parse(parts[1].Trim());
                                                }

                                                line = reader.ReadLine();
                                                parts = line.Split('=');
                                                if (parts.Length == 2)
                                                {
                                                    m_client.AddToLetterVDistance = bool.Parse(parts[1].Trim());
                                                }

                                                line = reader.ReadLine();
                                                parts = line.Split('=');
                                                if (parts.Length == 2)
                                                {
                                                    m_client.AddToLetterCDistance = bool.Parse(parts[1].Trim());
                                                }

                                                line = reader.ReadLine();
                                                parts = line.Split('=');
                                                if (parts.Length == 2)
                                                {
                                                    m_client.AddToWordWNumber = bool.Parse(parts[1].Trim());
                                                }

                                                line = reader.ReadLine();
                                                parts = line.Split('=');
                                                if (parts.Length == 2)
                                                {
                                                    m_client.AddToWordVNumber = bool.Parse(parts[1].Trim());
                                                }

                                                line = reader.ReadLine();
                                                parts = line.Split('=');
                                                if (parts.Length == 2)
                                                {
                                                    m_client.AddToWordCNumber = bool.Parse(parts[1].Trim());
                                                }

                                                line = reader.ReadLine();
                                                parts = line.Split('=');
                                                if (parts.Length == 2)
                                                {
                                                    m_client.AddToWordWDistance = bool.Parse(parts[1].Trim());
                                                }

                                                line = reader.ReadLine();
                                                parts = line.Split('=');
                                                if (parts.Length == 2)
                                                {
                                                    m_client.AddToWordVDistance = bool.Parse(parts[1].Trim());
                                                }

                                                line = reader.ReadLine();
                                                parts = line.Split('=');
                                                if (parts.Length == 2)
                                                {
                                                    m_client.AddToWordCDistance = bool.Parse(parts[1].Trim());
                                                }

                                                line = reader.ReadLine();
                                                parts = line.Split('=');
                                                if (parts.Length == 2)
                                                {
                                                    m_client.AddToVerseVNumber = bool.Parse(parts[1].Trim());
                                                }

                                                line = reader.ReadLine();
                                                parts = line.Split('=');
                                                if (parts.Length == 2)
                                                {
                                                    m_client.AddToVerseCNumber = bool.Parse(parts[1].Trim());
                                                }

                                                line = reader.ReadLine();
                                                parts = line.Split('=');
                                                if (parts.Length == 2)
                                                {
                                                    m_client.AddToVerseVDistance = bool.Parse(parts[1].Trim());
                                                }

                                                line = reader.ReadLine();
                                                parts = line.Split('=');
                                                if (parts.Length == 2)
                                                {
                                                    m_client.AddToVerseCDistance = bool.Parse(parts[1].Trim());
                                                }

                                                line = reader.ReadLine();
                                                parts = line.Split('=');
                                                if (parts.Length == 2)
                                                {
                                                    m_client.AddToChapterCNumber = bool.Parse(parts[1].Trim());
                                                }

                                                //???
                                                //// if NOT Research Edition, then reset all numbers/positions/distances
                                                //// in case last run was Research Edition and user enabled some of them
                                                //if (Globals.EDITION != Edition.Research)
                                                //{
                                                //    m_client.AddToLetterLNumber = false;
                                                //    m_client.AddToLetterWNumber = false;
                                                //    m_client.AddToLetterVNumber = false;
                                                //    m_client.AddToLetterCNumber = false;
                                                //    m_client.AddToLetterLDistance = false;
                                                //    m_client.AddToLetterWDistance = false;
                                                //    m_client.AddToLetterVDistance = false;
                                                //    m_client.AddToLetterCDistance = false;
                                                //    m_client.AddToWordWNumber = false;
                                                //    m_client.AddToWordVNumber = false;
                                                //    m_client.AddToWordCNumber = false;
                                                //    m_client.AddToWordWDistance = false;
                                                //    m_client.AddToWordVDistance = false;
                                                //    m_client.AddToWordCDistance = false;
                                                //    m_client.AddToVerseVNumber = false;
                                                //    m_client.AddToVerseCNumber = false;
                                                //    m_client.AddToVerseVDistance = false;
                                                //    m_client.AddToVerseCDistance = false;
                                                //    m_client.AddToChapterCNumber = false;
                                                //}
                                            }
                                        }
                                        break;
                                    // [Text]
                                    case "Translator":
                                        {
                                            try
                                            {
                                                int index = int.Parse(parts[1].Trim());
                                                if (index < this.TranslatorComboBox.Items.Count)
                                                {
                                                    this.TranslatorComboBox.SelectedIndex = index;
                                                }
                                                else
                                                {
                                                    this.TranslatorComboBox.SelectedItem = -1;
                                                }
                                            }
                                            catch
                                            {
                                                if (this.TranslatorComboBox.Items.Count >= 3)
                                                {
                                                    this.TranslatorComboBox.SelectedItem = Client.Translations[DEFAULT_TRANSLATION].Name;
                                                }
                                                else
                                                {
                                                    this.TranslatorComboBox.SelectedItem = -1;
                                                }
                                            }
                                        }
                                        break;
                                    case "SelectionScope":
                                        {
                                            try
                                            {
                                                selection_scope = (SelectionScope)Enum.Parse(typeof(SelectionScope), parts[1].Trim());
                                            }
                                            catch
                                            {
                                                selection_scope = SelectionScope.Chapter;
                                            }
                                        }
                                        break;
                                    case "SelectionIndexes":
                                        {
                                            try
                                            {
                                                string part = parts[1].Trim();
                                                string[] sub_parts = part.Split('+');
                                                selection_indexes.Clear();
                                                for (int i = 0; i < sub_parts.Length; i++)
                                                {
                                                    try
                                                    {
                                                        int index = int.Parse(sub_parts[i].Trim()) - 1;
                                                        selection_indexes.Add(index);
                                                    }
                                                    catch
                                                    {
                                                        // skip invalid index
                                                    }
                                                }
                                            }
                                            catch
                                            {
                                                selection_indexes.Add(0);
                                            }
                                        }
                                        break;
                                    case "SelectionStart":
                                        {
                                            try
                                            {
                                                this.MainTextBox.SelectionStart = int.Parse(parts[1].Trim());
                                            }
                                            catch
                                            {
                                                this.MainTextBox.SelectionStart = 0;
                                            }
                                        }
                                        break;
                                    case "SelectionLength":
                                        {
                                            try
                                            {
                                                this.MainTextBox.SelectionLength = int.Parse(parts[1].Trim());
                                            }
                                            catch
                                            {
                                                this.MainTextBox.SelectionLength = 0;
                                            }
                                        }
                                        break;
                                    case "FontSize":
                                        {
                                            try
                                            {
                                                m_font_size = float.Parse(parts[1].Trim());
                                            }
                                            catch
                                            {
                                                m_font_size = DEFAULT_FONT_SIZE;
                                            }
                                        }
                                        break;
                                    case "TranlationFontSize":
                                        {
                                            try
                                            {
                                                m_translation_font_size = float.Parse(parts[1].Trim());
                                            }
                                            catch
                                            {
                                                m_translation_font_size = DEFAULT_TRANSLATION_FONT_SIZE;
                                            }
                                        }
                                        break;
                                    case "TextZoomFactor":
                                        {
                                            try
                                            {
                                                m_text_zoom_factor = float.Parse(parts[1].Trim());
                                            }
                                            catch
                                            {
                                                m_text_zoom_factor = DEFAULT_TEXT_ZOOM_FACTOR;
                                            }
                                        }
                                        break;
                                    case "GraphicsZoomFactor":
                                        {
                                            try
                                            {
                                                m_graphics_zoom_factor = float.Parse(parts[1].Trim());
                                            }
                                            catch
                                            {
                                                m_graphics_zoom_factor = DEFAULT_GRAPHICS_ZOOM_FACTOR;
                                            }
                                        }
                                        break;
                                    // [Audio]
                                    case "Volume":
                                        {
                                            try
                                            {
                                                m_audio_volume = int.Parse(parts[1].Trim());
                                            }
                                            catch
                                            {
                                                m_audio_volume = DEFAULT_AUDIO_VOLUME;
                                            }
                                        }
                                        break;
                                    case "SilenceBetweenVerses":
                                        {
                                            try
                                            {
                                                m_silence_between_verses = float.Parse(parts[1].Trim());
                                            }
                                            catch
                                            {
                                                m_silence_between_verses = DEFAULT_SILENCE_BETWEEN_VERSES;
                                            }
                                        }
                                        break;
                                    case "Reciter":
                                        {
                                            try
                                            {
                                                int index = int.Parse(parts[1].Trim());
                                                if (index < this.RecitationsComboBox.Items.Count)
                                                {
                                                    this.RecitationsComboBox.SelectedIndex = index;
                                                }
                                                else
                                                {
                                                    this.RecitationsComboBox.SelectedItem = -1;
                                                }
                                            }
                                            catch
                                            {
                                                this.RecitationsComboBox.SelectedItem = Client.Recitations[DEFAULT_RECITATION].Reciter;
                                            }
                                        }
                                        break;
                                    // [Downloads]
                                    case "PageUrlPrefix":
                                        {
                                            try
                                            {
                                                Page.UrlPrefix = parts[1].Trim();
                                            }
                                            catch
                                            {
                                                Page.UrlPrefix = Page.DEFAULT_URL_PREFIX;
                                            }
                                        }
                                        break;
                                    case "PageFileType":
                                        {
                                            try
                                            {
                                                Page.FileType = parts[1].Trim();
                                            }
                                            catch
                                            {
                                                Page.FileType = Page.DEFAULT_FILE_TYPE;
                                            }
                                        }
                                        break;
                                    case "TranslationUrlPrefix":
                                        {
                                            try
                                            {
                                                Translation.UrlPrefix = parts[1].Trim();
                                            }
                                            catch
                                            {
                                                Translation.UrlPrefix = Translation.DEFAULT_URL_PREFIX;
                                            }
                                        }
                                        break;
                                    case "TranslationFileType":
                                        {
                                            try
                                            {
                                                Translation.FileType = parts[1].Trim();
                                            }
                                            catch
                                            {
                                                Translation.FileType = Translation.DEFAULT_FILE_TYPE;
                                            }
                                        }
                                        break;
                                    case "TranslationIconUrlPrefix":
                                        {
                                            try
                                            {
                                                Translation.IconUrlPrefix = parts[1].Trim();
                                            }
                                            catch
                                            {
                                                Translation.IconUrlPrefix = Translation.DEFAULT_ICON_URL_PREFIX;
                                            }
                                        }
                                        break;
                                    case "RecitationUrlPrefix":
                                        {
                                            try
                                            {
                                                Recitation.UrlPrefix = parts[1].Trim();
                                            }
                                            catch
                                            {
                                                Recitation.UrlPrefix = Recitation.DEFAULT_URL_PREFIX;
                                            }
                                        }
                                        break;
                                    case "RecitationFileType":
                                        {
                                            try
                                            {
                                                Recitation.FileType = parts[1].Trim();
                                            }
                                            catch
                                            {
                                                Recitation.FileType = Recitation.DEFAULT_FILE_TYPE;
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                    }

                    m_client.Selection = new Selection(Book.Instance, selection_scope, selection_indexes);
                }
            }
            catch
            {
                // silence Parse exceptions
                // continue with next INI entry
            }
        }
        else // first Application launch
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Width = DEFAULT_WINDOW_WIDTH;
            this.Height = DEFAULT_WINDOW_HEIGHT;

            ResetNumerologySystem();

            if (this.ChapterComboBox.Items.Count > 1)
            {
                this.ChapterComboBox.SelectedIndex = 0;
            }

            if (this.TranslatorComboBox.Items.Count >= 3)
            {
                this.TranslatorComboBox.SelectedItem = Client.Translations[DEFAULT_TRANSLATION].Name;
            }

            if (m_client != null)
            {
                try
                {
                    m_client.Selection = new Selection(Book.Instance, SelectionScope.Chapter, new List<int>() { 0 });
                }
                catch
                {
                    // log exception
                }
            }

            this.MainTextBox.SelectionStart = 0;
            this.MainTextBox.SelectionLength = 0;
        }
    }
    private void SaveApplicationOptions()
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(m_ini_filename, false, Encoding.Unicode))
            {
                if (this.WindowState == FormWindowState.Maximized)
                {
                    this.WindowState = FormWindowState.Normal;
                }

                writer.WriteLine("[Window]");
                if (this.WindowState == FormWindowState.Minimized)
                {
                    // restore or width/height will be saved as 0
                    writer.WriteLine("Top=" + (Screen.PrimaryScreen.WorkingArea.Height - DEFAULT_WINDOW_HEIGHT) / 2);
                    writer.WriteLine("Left=" + (Screen.PrimaryScreen.WorkingArea.Width - DEFAULT_WINDOW_WIDTH) / 2);
                    writer.WriteLine("Width=" + DEFAULT_WINDOW_WIDTH);
                    writer.WriteLine("Height=" + DEFAULT_WINDOW_HEIGHT);
                    writer.WriteLine("TranslationBoxVisible=" + DEFAULT_TRANSLATION_BOX_VISIBLE);
                    writer.WriteLine("TranslatorComboBoxVisible=" + DEFAULT_TRANSLATOR_COMBOBOX_VISIBLE);
                    writer.WriteLine("TranslationBoxTop=" + DEFAULT_TRANSLATION_BOX_TOP);
                }
                else
                {
                    writer.WriteLine("Top=" + this.Top);
                    writer.WriteLine("Left=" + this.Left);
                    writer.WriteLine("Width=" + this.Width);
                    writer.WriteLine("Height=" + this.Height);
                    writer.WriteLine("TranslationBoxVisible=" + m_show_translation_box);
                    writer.WriteLine("TranslatorSelectionBoxVisible=" + m_show_translator_combobox);
                    writer.WriteLine("TranslationBoxTop=" + m_translation_box_top);
                }
                writer.WriteLine();

                writer.WriteLine("[Numerology]");
                writer.WriteLine("Radix=" + m_radix);
                if (m_client != null)
                {
                    writer.WriteLine("TextMode=" + m_client.TextMode);
                    writer.WriteLine("LetterOrderSystem=" + m_client.LetterOrderSystem);
                    writer.WriteLine("LetterValueSystem=" + m_client.LetterValueSystem);
                    writer.WriteLine("IsDynamic=" + m_client.IsDynamic.ToString());
                    writer.WriteLine("IsHighlightedText=" + m_client.IsHighlightedText.ToString());
                    writer.WriteLine("AddToLetterLNumber=" + m_client.AddToLetterLNumber.ToString());
                    writer.WriteLine("AddToLetterWNumber=" + m_client.AddToLetterWNumber.ToString());
                    writer.WriteLine("AddToLetterVNumber=" + m_client.AddToLetterVNumber.ToString());
                    writer.WriteLine("AddToLetterCNumber=" + m_client.AddToLetterCNumber.ToString());
                    writer.WriteLine("AddToLetterLDistance=" + m_client.AddToLetterLDistance.ToString());
                    writer.WriteLine("AddToLetterWDistance=" + m_client.AddToLetterWDistance.ToString());
                    writer.WriteLine("AddToLetterVDistance=" + m_client.AddToLetterVDistance.ToString());
                    writer.WriteLine("AddToLetterCDistance=" + m_client.AddToLetterCDistance.ToString());
                    writer.WriteLine("AddToWordWNumber=" + m_client.AddToWordWNumber.ToString());
                    writer.WriteLine("AddToWordVNumber=" + m_client.AddToWordVNumber.ToString());
                    writer.WriteLine("AddToWordCNumber=" + m_client.AddToWordCNumber.ToString());
                    writer.WriteLine("AddToWordWDistance=" + m_client.AddToWordWDistance.ToString());
                    writer.WriteLine("AddToWordVDistance=" + m_client.AddToWordVDistance.ToString());
                    writer.WriteLine("AddToWordCDistance=" + m_client.AddToWordCDistance.ToString());
                    writer.WriteLine("AddToVerseVNumber=" + m_client.AddToVerseVNumber.ToString());
                    writer.WriteLine("AddToVerseCNumber=" + m_client.AddToVerseCNumber.ToString());
                    writer.WriteLine("AddToVerseVDistance=" + m_client.AddToVerseVDistance.ToString());
                    writer.WriteLine("AddToVerseCDistance=" + m_client.AddToVerseCDistance.ToString());
                    writer.WriteLine("AddToChapterCNumber=" + m_client.AddToChapterCNumber.ToString());
                }
                writer.WriteLine();

                writer.WriteLine("[Text]");
                writer.WriteLine("Translator=" + this.TranslatorComboBox.SelectedIndex);
                if (m_client != null)
                {
                    if (m_client.Selection != null)
                    {
                        writer.WriteLine("SelectionScope=" + (int)m_client.Selection.Scope);
                        StringBuilder str = new StringBuilder("SelectionIndexes=");
                        foreach (int index in m_client.Selection.Indexes)
                        {
                            str.Append((index + 1).ToString() + "+");
                        }
                        if (str.Length > 0) // remove last "+"
                        {
                            str.Remove(str.Length - "+".Length, "+".Length);
                        }
                        writer.WriteLine(str);
                        writer.WriteLine("SelectionStart=" + this.MainTextBox.SelectionStart);
                        writer.WriteLine("SelectionLength=" + this.MainTextBox.SelectionLength);
                        writer.WriteLine("FontSize=" + m_font_size);
                        writer.WriteLine("TranslationFontSize=" + m_translation_font_size);
                        writer.WriteLine("TextZoomFactor=" + m_text_zoom_factor);
                        writer.WriteLine("GraphicsZoomFactor=" + m_graphics_zoom_factor);
                    }
                }
                writer.WriteLine();

                writer.WriteLine("[Audio]");
                writer.WriteLine("Volume=" + m_audio_volume);
                writer.WriteLine("SilenceBetweenVerses=" + m_silence_between_verses);
                writer.WriteLine("Reciter=" + this.RecitationsComboBox.SelectedIndex);
                writer.WriteLine();

                writer.WriteLine("[Downloads]");
                writer.WriteLine("PageUrlPrefix=" + Page.UrlPrefix);
                writer.WriteLine("PageFileType=" + Page.FileType);
                writer.WriteLine("TranslationUrlPrefix=" + Translation.UrlPrefix);
                writer.WriteLine("TranslationFileType=" + Translation.FileType);
                writer.WriteLine("TranslationIconUrlPrefix=" + Translation.IconUrlPrefix);
                writer.WriteLine("RecitationUrlPrefix=" + Recitation.UrlPrefix);
                writer.WriteLine("RecitationFileType=" + Recitation.FileType);
            }
        }
        catch
        {
            // silence IO errors in case running from read-only media (CD/DVD)
        }
    }
    private void RefreshNumerologySystem()
    {
        if (m_client != null)
        {
            TextModeComboBox.SelectedItem = m_client.TextMode;
            LetterOrderSystemComboBox.SelectedItem = m_client.LetterOrderSystem;
            LetterValueSystemComboBox.SelectedItem = m_client.LetterValueSystem;

            DynamicLetterScopeCheckBox.Enabled = true;
            DynamicLetterScopeSelectionRadioButton.Enabled = m_client.IsDynamic;
            DynamicLetterScopeHighlightedTextRadioButton.Enabled = m_client.IsDynamic;

            try
            {
                DynamicLetterScopeCheckBox.CheckedChanged -= new EventHandler(DynamicLetterScopeCheckBox_CheckedChanged);
                DynamicLetterScopeSelectionRadioButton.CheckedChanged -= new EventHandler(DynamicLetterScopeSelectionRadioButton_CheckedChanged);
                DynamicLetterScopeHighlightedTextRadioButton.CheckedChanged -= new EventHandler(DynamicLetterScopeHighlightedTextRadioButton_CheckedChanged);

                DynamicLetterScopeCheckBox.Checked = m_client.IsDynamic;
                DynamicLetterScopeSelectionRadioButton.Checked = !m_client.IsHighlightedText;
                DynamicLetterScopeHighlightedTextRadioButton.Checked = m_client.IsHighlightedText;
            }
            finally
            {
                DynamicLetterScopeCheckBox.CheckedChanged += new EventHandler(DynamicLetterScopeCheckBox_CheckedChanged);
                DynamicLetterScopeSelectionRadioButton.CheckedChanged += new EventHandler(DynamicLetterScopeSelectionRadioButton_CheckedChanged);
                DynamicLetterScopeHighlightedTextRadioButton.CheckedChanged += new EventHandler(DynamicLetterScopeHighlightedTextRadioButton_CheckedChanged);
            }

            RefreshAddNumberCheckBoxes();
        }
    }
    private void RefreshAddNumberCheckBoxes()
    {
        if (m_client != null)
        {
            AddToLetterLNumberCheckBox.Checked = m_client.AddToLetterLNumber;
            AddToLetterWNumberCheckBox.Checked = m_client.AddToLetterWNumber;
            AddToLetterVNumberCheckBox.Checked = m_client.AddToLetterVNumber;
            AddToLetterCNumberCheckBox.Checked = m_client.AddToLetterCNumber;
            AddToLetterLDistanceCheckBox.Checked = m_client.AddToLetterLDistance;
            AddToLetterWDistanceCheckBox.Checked = m_client.AddToLetterWDistance;
            AddToLetterVDistanceCheckBox.Checked = m_client.AddToLetterVDistance;
            AddToLetterCDistanceCheckBox.Checked = m_client.AddToLetterCDistance;
            AddToWordWNumberCheckBox.Checked = m_client.AddToWordWNumber;
            AddToWordVNumberCheckBox.Checked = m_client.AddToWordVNumber;
            AddToWordCNumberCheckBox.Checked = m_client.AddToWordCNumber;
            AddToWordWDistanceCheckBox.Checked = m_client.AddToWordWDistance;
            AddToWordVDistanceCheckBox.Checked = m_client.AddToWordVDistance;
            AddToWordCDistanceCheckBox.Checked = m_client.AddToWordCDistance;
            AddToVerseVNumberCheckBox.Checked = m_client.AddToVerseVNumber;
            AddToVerseCNumberCheckBox.Checked = m_client.AddToVerseCNumber;
            AddToVerseVDistanceCheckBox.Checked = m_client.AddToVerseVDistance;
            AddToVerseCDistanceCheckBox.Checked = m_client.AddToVerseCDistance;
            AddToChapterCNumberCheckBox.Checked = m_client.AddToChapterCNumber;
        }
    }
    private void UpdateAddNumberCheckBoxes()
    {
        if (m_client != null)
        {
            m_client.AddToLetterLNumber = AddToLetterLNumberCheckBox.Checked;
            m_client.AddToLetterWNumber = AddToLetterWNumberCheckBox.Checked;
            m_client.AddToLetterVNumber = AddToLetterVNumberCheckBox.Checked;
            m_client.AddToLetterCNumber = AddToLetterCNumberCheckBox.Checked;
            m_client.AddToLetterLDistance = AddToLetterLDistanceCheckBox.Checked;
            m_client.AddToLetterWDistance = AddToLetterWDistanceCheckBox.Checked;
            m_client.AddToLetterVDistance = AddToLetterVDistanceCheckBox.Checked;
            m_client.AddToLetterCDistance = AddToLetterCDistanceCheckBox.Checked;
            m_client.AddToWordWNumber = AddToWordWNumberCheckBox.Checked;
            m_client.AddToWordVNumber = AddToWordVNumberCheckBox.Checked;
            m_client.AddToWordCNumber = AddToWordCNumberCheckBox.Checked;
            m_client.AddToWordWDistance = AddToWordWDistanceCheckBox.Checked;
            m_client.AddToWordVDistance = AddToWordVDistanceCheckBox.Checked;
            m_client.AddToWordCDistance = AddToWordCDistanceCheckBox.Checked;
            m_client.AddToVerseVNumber = AddToVerseVNumberCheckBox.Checked;
            m_client.AddToVerseCNumber = AddToVerseCNumberCheckBox.Checked;
            m_client.AddToVerseVDistance = AddToVerseVDistanceCheckBox.Checked;
            m_client.AddToVerseCDistance = AddToVerseCDistanceCheckBox.Checked;
            m_client.AddToChapterCNumber = AddToChapterCNumberCheckBox.Checked;
        }
    }
    private void ResetNumerologySystem()
    {
        if (m_client != null)
        {
            m_client.ResetNumerologySystem();
            RefreshNumerologySystem();
        }
    }

    private Assembly m_assembly = null;
    private void LoadResearchMethods()
    {
        this.Cursor = Cursors.WaitCursor;
        ResearchMethodsComboBox.SelectedIndexChanged -= new EventHandler(ResearchMethodsComboBox_SelectedIndexChanged);

        try
        {
            AppDomain domain = AppDomain.CurrentDomain;
            if (domain != null)
            {
                m_assembly = domain.Load("Research");
                if (m_assembly != null)
                {
                    MethodInfo[] method_infos = null;
                    if (Globals.EDITION == Edition.Research)
                    {
                        method_infos = typeof(Research).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    }
                    else
                    {
                        method_infos = typeof(Research).GetMethods(BindingFlags.Static | BindingFlags.Public);
                    }

                    ResearchMethodsComboBox.Items.Clear();
                    foreach (MethodInfo method_info in method_infos)
                    {
                        ParameterInfo[] parameters = method_info.GetParameters();
                        if ((parameters.Length == 2) && (parameters[0].ParameterType == typeof(Client)) && (parameters[1].ParameterType == typeof(string)))
                        {
                            // skip grammar methods if not Grammar or Research edition
                            if (Globals.EDITION != Edition.Grammar)
                            {
                                if (method_info.Name.Contains("WordPart"))
                                {
                                    continue;
                                }
                            }
                            ResearchMethodsComboBox.Items.Add(method_info.Name);
                        }
                    }
                    if (Globals.EDITION == Edition.Research)
                    {
                        if (ResearchMethodsComboBox.Items.Count > 48)
                        {
                            ResearchMethodsComboBox.SelectedIndex = 48; // AllahWords
                        }
                    }
                    else
                    {
                        if (ResearchMethodsComboBox.Items.Count > 30)
                        {
                            ResearchMethodsComboBox.SelectedIndex = 30; // AllahWords
                        }
                    }
                }
            }
        }
        finally
        {
            ResearchMethodsComboBox.SelectedIndexChanged += new EventHandler(ResearchMethodsComboBox_SelectedIndexChanged);
            this.Cursor = Cursors.Default;
        }
    }
    private void RunResearchMethod()
    {
        if (m_client != null)
        {
            if (m_client.Selection != null)
            {
                if (ResearchMethodsComboBox.SelectedIndex > -1)
                {
                    if (ResearchMethodsComboBox.SelectedItem != null)
                    {
                        string method_name = ResearchMethodsComboBox.SelectedItem.ToString();
                        string extra = ResearchMethodParameterTextBox.Text;
                        if (!string.IsNullOrEmpty(method_name))
                        {
                            List<Verse> verses = m_client.Selection.Verses;
                            if (verses != null)
                            {
                                if (verses.Count > 0)
                                {
                                    InvokeResearchMethod(method_name, m_client, extra);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    private void InvokeResearchMethod(string method_name, Client client, string extra)
    {
        this.Cursor = Cursors.WaitCursor;
        ResearchMethodsComboBox.Enabled = false;
        ResearchMethodParameterTextBox.Enabled = false;
        ResearchMethodsRunButton.Enabled = false;

        try
        {
            if (m_assembly != null)
            {
                Type type = typeof(Research);
                if (type != null)
                {
                    MethodInfo method_info = type.GetMethod(method_name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    if ((method_info != null) && (method_info.IsStatic))
                    {
                        object[] parameters = { client, extra };
                        object result = method_info.Invoke(null, parameters);
                    }
                }
            }
        }
        catch (TargetInvocationException /*ex*/)
        {
            MessageBox.Show(method_name + " is not available in " + Globals.EDITION.ToString() + " Edition.", Application.ProductName);
        }
        catch (Exception ex)
        {
            while (ex != null)
            {
                //Console.WriteLine(ex.Message);
                MessageBox.Show(ex.Message, Application.ProductName);
                ex = ex.InnerException;
            }
        }
        finally
        {
            ResearchMethodsComboBox.Enabled = true;
            ResearchMethodParameterTextBox.Enabled = true;
            ResearchMethodsRunButton.Enabled = true;
            this.Cursor = Cursors.Default;
        }
    }
    private void ResearchMethodsComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (ResearchMethodsComboBox.SelectedItem.ToString().Contains("_____"))
        {
            ResearchMethodsRunButton.Enabled = false;
            ResearchMethodParameterTextBox.Text = "";
            ResearchMethodParameterTextBox.BackColor = SystemColors.ControlLight;
            ResearchMethodParameterTextBox.Enabled = false;
            ToolTip.SetToolTip(ResearchMethodParameterTextBox, "");
        }
        else
        {
            ResearchMethodsRunButton.Enabled = true;
            ResearchMethodParameterTextBox.BackColor = SystemColors.Window;
            ResearchMethodParameterTextBox.Enabled = true;
            ToolTip.SetToolTip(ResearchMethodParameterTextBox, "Parameter");

            if (ResearchMethodsComboBox.SelectedItem.ToString().Contains("ByX"))
            {
                ResearchMethodParameterTextBox.Text = "19";
                ToolTip.SetToolTip(ResearchMethodParameterTextBox, "X");
            }
            else if (ResearchMethodsComboBox.SelectedItem.ToString().Contains("WithX"))
            {
                ResearchMethodParameterTextBox.Text = "19";
                ToolTip.SetToolTip(ResearchMethodParameterTextBox, "X");
            }
            else if (ResearchMethodsComboBox.SelectedItem.ToString().Contains("XY"))
            {
                ResearchMethodParameterTextBox.Text = "14,33";
                ToolTip.SetToolTip(ResearchMethodParameterTextBox, "X,Y");
            }
            else if (ResearchMethodsComboBox.SelectedItem.ToString().Contains("XYZ"))
            {
                ResearchMethodParameterTextBox.Text = "7,29,139";
                ToolTip.SetToolTip(ResearchMethodParameterTextBox, "X,Y,Z");
            }
            else if (ResearchMethodsComboBox.SelectedItem.ToString().Contains("Sound"))
            {
                ResearchMethodParameterTextBox.Text = "1433";
                ToolTip.SetToolTip(ResearchMethodParameterTextBox, "Hz");
            }
            else
            {
                ResearchMethodParameterTextBox.Text = "";
                ResearchMethodParameterTextBox.BackColor = SystemColors.ControlLight;
                ResearchMethodParameterTextBox.Enabled = false;
            }
        }
    }
    private void ResearchMethodsComboBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            RunResearchMethod();
        }
    }
    private void ResearchMethodParameterTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            RunResearchMethod();
        }
    }
    private void ResearchMethodsRunButton_Click(object sender, EventArgs e)
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            if (ResearchMethodsComboBox.SelectedItem != null)
            {
                RunResearchMethod();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }

    private enum DoubleClickTarget { Page, Chapter };
    private bool m_found_verses_displayed = false;
    private int m_found_verse_double_click_position = 0;
    private int m_found_verse_double_click_target_position = 0;
    private DoubleClickTarget m_found_verse_double_click_target = DoubleClickTarget.Chapter;

    private string m_find_result_header;
    private string m_note_writing_instruction = "write a note";
    private Color m_note_writing_instruction_color = Color.LightGray;
    private Color m_note_edit_color = Color.Salmon;
    private Color m_note_view_color = Color.Red;

    private void NoteTextBox_Enter(object sender, EventArgs e)
    {
        SearchGroupBox_Leave(sender, e);

        NoteTextBox.ForeColor = m_note_edit_color;
        if (NoteTextBox.Text.StartsWith(m_note_writing_instruction))
        {
            NoteTextBox.Text = "";
        }
        this.AcceptButton = AddBookmarkButton;
    }
    private void NoteTextBox_Leave(object sender, EventArgs e)
    {
        if (NoteTextBox.Text.Length == 0)
        {
            DisplayNoteWritingInstruction();
        }
        this.AcceptButton = null;
    }
    private void NoteTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            if (NoteTextBox.Text.Length > 0)
            {
                AddBookmarkButton_Click(sender, e);
            }
            else
            {
                DeleteBookmarkButton_Click(sender, e);
            }
        }
        else
        {
            NoteTextBox.ForeColor = m_note_edit_color;
        }
        RefreshBookmarkHistoryButtons();
    }
    private void NoteTextBox_TextChanged(object sender, EventArgs e)
    {
    }
    private void DisplayNoteWritingInstruction()
    {
        NoteTextBox.Enabled = true;
        NoteTextBox.ForeColor = m_note_writing_instruction_color;
        if (NoteTextBox.Focused)
        {
            NoteTextBox.Text = "";
        }
        else
        {
            if (m_client != null)
            {
                if (m_client.Selection != null)
                {
                    if (Book.Instance != null)
                    {
                        if (m_client.Selection.Scope == SelectionScope.Book)
                        {
                            NoteTextBox.Text = m_note_writing_instruction + " for "
                                         + m_client.TextMode + " "
                                         + m_client.Selection.Scope.ToString();
                        }
                        else if ((m_client.Selection.Scope == SelectionScope.Verse) || (m_client.Selection.Scope == SelectionScope.Word) || (m_client.Selection.Scope == SelectionScope.Letter))
                        {
                            NoteTextBox.Text = m_note_writing_instruction + " for Chapter "
                                         + (ChapterComboBox.SelectedIndex + 1).ToString() + " Verse "
                                         + (ChapterVerseNumericUpDown.Value).ToString();
                        }
                        else
                        {
                            StringBuilder str = new StringBuilder();
                            foreach (int index in m_client.Selection.Indexes)
                            {
                                str.Append((index + 1).ToString() + "+");
                            }
                            if (str.Length > 0)
                            {
                                str.Remove(str.Length - "+".Length, "+".Length);
                            }

                            NoteTextBox.Text = m_note_writing_instruction + " for "
                                         + m_client.Selection.Scope.ToString() + " "
                                         + str.ToString();
                        }
                    }
                }
            }
        }
        RefreshBookmarkHistoryButtons();
    }
    private void DisplayBookmark(Bookmark bookmark)
    {
        if (bookmark != null)
        {
            if (bookmark.Selection != null)
            {
                if (m_client != null)
                {
                    m_client.Selection = new Selection(Book.Instance, bookmark.Selection.Scope, bookmark.Selection.Indexes);

                    DisplaySelection(NoteTextBox, false);

                    NoteTextBox.Text = bookmark.Note;
                    NoteTextBox.ForeColor = m_note_view_color;
                    string hint = "Creation Time" + "\t" + bookmark.CreatedTime + "\r\n"
                         + "Last Modified" + "\t" + bookmark.LastModifiedTime;
                    ToolTip.SetToolTip(NoteTextBox, hint);
                    MainTextBox.Focus();

                    RefreshBookmarkHistoryButtons();
                }
            }
        }
    }
    private void AddBookmarkButton_Click(object sender, EventArgs e)
    {
        if (m_client != null)
        {
            if (m_client.Selection != null)
            {
                if (Book.Instance != null)
                {
                    if (NoteTextBox.Text.StartsWith(m_note_writing_instruction))
                    {
                        // ignore it
                    }
                    else if (NoteTextBox.Text.Length == 0)
                    {
                        DeleteBookmarkButton_Click(sender, e);
                    }
                    else //if (!NoteTextBox.Text.StartsWith(m_note_writing_instruction))
                    {
                        Selection selection = new Selection(Book.Instance, m_client.Selection.Scope, m_client.Selection.Indexes);
                        Bookmark bookmark = m_client.AddOverwriteBookmark(selection, NoteTextBox.Text);

                        NoteTextBox.ForeColor = m_note_view_color;
                        RefreshBookmarkHistoryButtons();
                    }
                }
            }
        }
    }
    private void PreviousBookmarkButton_Click(object sender, EventArgs e)
    {
        if (m_client != null)
        {
            if (Book.Instance != null)
            {
                Bookmark bookmark = m_client.GotoPreviousBookmark();
                if (bookmark != null)
                {
                    DisplayBookmark(bookmark);
                }
            }
        }
    }
    private void NextBookmarkButton_Click(object sender, EventArgs e)
    {
        if (m_client != null)
        {
            if (Book.Instance != null)
            {
                Bookmark bookmark = m_client.GotoNextBookmark();
                if (bookmark != null)
                {
                    DisplayBookmark(bookmark);
                }
            }
        }
    }
    private void DeleteBookmarkButton_Click(object sender, EventArgs e)
    {
        if (m_client != null)
        {
            if (Book.Instance != null)
            {
                // remove existing bookmark (if any)
                m_client.DeleteCurrentBookmark();

                Bookmark bookmark = m_client.CurrentBookmark;
                if (bookmark != null)
                {
                    DisplayBookmark(bookmark);
                }
                else
                {
                    DisplaySelection(sender, false);
                }
            }
        }
    }
    private void RefreshBookmarkHistoryButtons()
    {
        if (m_client != null)
        {
            if (m_client.Bookmarks != null)
            {
                {
                    PreviousBookmarkButton.Enabled = (m_client.Bookmarks.Count > 0) && (m_client.CurrentBookmarkIndex > 0);
                    NextBookmarkButton.Enabled = (m_client.Bookmarks.Count > 0) && (m_client.CurrentBookmarkIndex < m_client.Bookmarks.Count - 1);
                    NoteCounterLabel.Text = (m_client.CurrentBookmarkIndex + 1).ToString() + " / " + m_client.Bookmarks.Count.ToString();
                }
            }
        }
    }

    private string m_recitation_folder = DEFAULT_RECITATION;
    private MP3Player m_player = new MP3Player();
    private int m_audio_volume = DEFAULT_AUDIO_VOLUME;
    private string m_downloaded_audio_filename = "";
    private bool m_playerLooping = false;
    private bool m_playerLoopingAll = false;
    private bool m_first_play = true;

    private bool m_in_silent_mode = false;
    private float m_silence_between_verses = DEFAULT_SILENCE_BETWEEN_VERSES; // in multiples of verses
    private int m_silence_time_between_verses = 0;
    private void PlayNextVerse()
    {
        // if looping then replay
        if (m_playerLooping)
        {
            PlayerPlayLabel_Click(null, null);
        }
        else // move to next verse
        {
            if (GotoNextLine())
            {
                PlayerPlayLabel_Click(null, null);
            }
            else // if final verse in selection
            {
                PlayerStopLabel_Click(null, null);

                // reset to the top or the first page image
                m_current_line_index = 0;
                m_found_verses_current_line_index = 0;
                if (m_found_verses_displayed)
                {
                    DisplayFoundVerses(false, false);
                    if (m_client != null)
                    {
                        DisplayTranslation(m_client.FoundVerses[0]);
                    }
                }
                else
                {
                    DisplaySelection(null, false);
                    if (m_client != null)
                    {
                        if (m_client.Selection != null)
                        {
                            DisplayTranslation(m_client.Selection.Verses[0]);
                        }
                    }
                }
                m_is_selection_mode = false;

                if (m_playerLoopingAll)
                {
                    PlayerPlayLabel_Click(null, null);
                }
            }
        }
    }
    private void PlayerTimer_Tick(object sender, EventArgs e)
    {
        if (m_player != null)
        {
            // if silent_mode or verse play is finished, then stop it
            if (
                    m_in_silent_mode
                    ||
                    ((m_player.Length - m_player.Position) < ((ulong)PlayerTimer.Interval / 2))
               )
            {
                // get verse time length before stop
                int verse_time_length = (int)m_player.Length;

                // stop verse play
                if (m_player.Opened)
                {
                    m_player.Stop();
                    m_player.Close();
                }

                // re-calculate required silence time
                m_silence_time_between_verses = (int)(verse_time_length * m_silence_between_verses);

                // if silence still required
                if (m_silence_time_between_verses > 0)
                {
                    m_in_silent_mode = true;
                    PlayerTimer.Interval = m_silence_time_between_verses;
                    return; // and call us back after Interval
                }

                m_in_silent_mode = false;
                PlayerTimer.Interval = 100;
                PlayNextVerse();
            }
        }
    }
    private void PlayerPlayAudhuBillah()
    {
        Verse audhuthuBillah_verse = null;
        if (DownloadVerseAudioFile(audhuthuBillah_verse))
        {
            m_player.Open(m_downloaded_audio_filename);
        }
        else
        {
            m_player.Open("Audio/audhubillah.mp3");
        }

        if (m_player.Opened)
        {
            if (File.Exists("Images/player_pause.png"))
            {
                PlayerPlayLabel.Image = new Bitmap("Images/player_pause.png");
            }
            PlayerPlayLabel.Refresh();

            m_player.VolumeAll = m_audio_volume;
            m_player.Play();
        }
        else
        {
            PlayerStopLabel_Click(null, null);
            AskUserToDownloadAudioFilesManually();
        }
    }
    private void PlayerPlayBismAllah()
    {
        if (m_client != null)
        {
            if (Book.Instance != null)
            {
                Chapter al_fatiha = null;
                foreach (Chapter chapter in Book.Instance.Chapters)
                {
                    if (chapter.Number == 1)
                    {
                        al_fatiha = chapter;
                        break;
                    }
                }

                if (al_fatiha != null)
                {
                    if (al_fatiha.Verses.Count > 0)
                    {
                        // download file if not on disk
                        if (DownloadVerseAudioFile(al_fatiha.Verses[0]))
                        {
                            m_player.Open(m_downloaded_audio_filename);

                            if (m_player.Opened)
                            {
                                if (File.Exists("Images/player_pause.png"))
                                {
                                    PlayerPlayLabel.Image = new Bitmap("Images/player_pause.png");
                                }
                                PlayerPlayLabel.Refresh();

                                m_player.VolumeAll = m_audio_volume;
                                m_player.Play();
                            }
                        }
                        else
                        {
                            PlayerStopLabel_Click(null, null);
                            AskUserToDownloadAudioFilesManually();
                        }
                    }
                }
            }
        }
    }
    private void PlayerOpenAudioFile(Verse verse)
    {
        if (verse != null)
        {
            if (m_player != null)
            {
                try
                {
                    // on first play
                    if (m_first_play)
                    {
                        PlayerPlayAudhuBillah();
                        WaitForPlayToFinish();

                        // don't play bismAllah for not first verses NOR first verse of chapter 9 
                        if (verse.Chapter != null)
                        {
                            if ((verse.Chapter.Number != 9) && (verse.NumberInChapter != 1))
                            {
                                PlayerPlayBismAllah();
                                WaitForPlayToFinish();
                            }
                        }

                        m_first_play = false;
                    }

                    // on all plays
                    // play BismAllah for every verse 1, except chapter 9
                    if (verse.NumberInChapter == 1)
                    {
                        if (verse.Chapter != null)
                        {
                            if ((verse.Chapter.Number != 1) && (verse.Chapter.Number != 9))
                            {
                                PlayerPlayBismAllah();
                                WaitForPlayToFinish();
                            }
                        }
                    }

                    // download file if not on disk
                    if (DownloadVerseAudioFile(verse))
                    {
                        // open only, don't play
                        m_player.Open(m_downloaded_audio_filename);
                    }
                    else
                    {
                        PlayerStopLabel_Click(null, null);
                        AskUserToDownloadAudioFilesManually();
                    }
                }
                catch
                {
                    PlayerStopLabel_Click(null, null);
                }
            }
        }
        else // invalid verse
        {
            MessageBox.Show("No verse available.", Application.ProductName);

            // reset player buttons
            PlayerStopLabel_Click(null, null);
        }
    }
    private void PlayerPreviousLabel_Click(object sender, EventArgs e)
    {
        int line_index = GetCurrentLineIndex() - 1;
        if (line_index >= 0)
        {
            bool player_is_opened = false;
            if (m_player != null)
            {
                player_is_opened = m_player.Opened;
                PlayerStopLabel_Click(sender, e);
            }

            if (m_found_verses_displayed)
            {
                m_found_verses_current_line_index = line_index;
            }
            else
            {
                m_current_line_index = line_index;
            }
            GotoLine(line_index);

            if (m_player != null)
            {
                if (player_is_opened)
                {
                    PlayerPlayLabel_Click(sender, e);
                }
            }
        }
    }
    private void PlayerNextLabel_Click(object sender, EventArgs e)
    {
        int line_index = GetCurrentLineIndex() + 1;
        if (line_index < MainTextBox.Lines.Length)
        {
            bool player_is_opened = false;
            if (m_player != null)
            {
                player_is_opened = m_player.Opened;
                PlayerStopLabel_Click(sender, e);
            }

            if (m_found_verses_displayed)
            {
                m_found_verses_current_line_index = line_index;
            }
            else
            {
                m_current_line_index = line_index;
            }
            GotoLine(line_index);

            if (m_player != null)
            {
                if (player_is_opened)
                {
                    PlayerPlayLabel_Click(sender, e);
                }
            }
        }
    }
    private void PlayerPlayLabel_Click(object sender, EventArgs e)
    {
        if (m_player != null)
        {
            if ((m_player.Closed) || (m_player.Stopped) || (m_player.Paused))
            {
                if (File.Exists("Images/player_pause.png"))
                {
                    PlayerPlayLabel.Image = new Bitmap("Images/player_pause.png");
                }
                ToolTip.SetToolTip(PlayerPlayLabel, "Pause");
                PlayerPlayLabel.Refresh();

                Verse verse = GetCurrentVerse();
                if (verse != null)
                {
                    try
                    {
                        //int line_index = -1;
                        //if (m_found_verses_displayed)
                        //{
                        //    line_index = m_found_verses_current_line_index;
                        //}
                        //else
                        //{
                        //    line_index = m_current_line_index;
                        //}
                        //int first_char_index = MainTextBox.GetFirstCharIndexFromLine(line_index);
                        //int hightlight_length = verse.Texts[m_client.TextMode].Length;
                        //if (m_found_verses_displayed)
                        //{
                        //    if (m_find_by_text_language_type == FindByTextLanguageType.Arabic)
                        //    {
                        //        hightlight_length += 1 + verse.ArabicAddress.Length;
                        //    }
                        //    else
                        //    {
                        //        hightlight_length += 1 + verse.PaddedAddress.Length;
                        //    }
                        //}
                        //Highlight(first_char_index, hightlight_length);

                        if (m_player != null)
                        {
                            if (m_player.Closed)
                            {
                                PlayerOpenAudioFile(verse);
                            }

                            if (m_player.Opened)
                            {
                                if (m_player.MuteAll)
                                {
                                    m_player.VolumeAll = 0;
                                }
                                else
                                {
                                    m_player.VolumeAll = m_audio_volume;
                                }

                                m_player.Play();
                                PlayerTimer.Enabled = true;
                                PlayerStopLabel.Enabled = true;
                                PlayerStopLabel.Refresh();
                            }
                        }
                    }
                    catch
                    {
                        PlayerStopLabel_Click(null, null);
                    }

                    // simulate mouse click inside MainTextBox to continue playing next verse and not restart from 1
                    m_is_selection_mode = false;
                    MainTextBox.Focus();
                }
            }
            else if (m_player.Playing)
            {
                if (File.Exists("Images/player_play.png"))
                {
                    PlayerPlayLabel.Image = new Bitmap("Images/player_play.png");
                }
                ToolTip.SetToolTip(PlayerPlayLabel, "Play");
                PlayerPlayLabel.Refresh();

                m_player.Pause();
                PlayerStopLabel.Enabled = true;
                PlayerStopLabel.Refresh();
            }
        }
    }
    private void PlayerStopLabel_Click(object sender, EventArgs e)
    {
        if (m_player != null)
        {
            if (m_player.Opened)
            {
                m_player.Stop();
                m_player.Close();
            }
            PlayerTimer.Enabled = false;
            PlayerStopLabel.Enabled = false;
            PlayerStopLabel.Refresh();
            if (File.Exists("Images/player_play.png"))
            {
                PlayerPlayLabel.Image = new Bitmap("Images/player_play.png");
            }
            PlayerPlayLabel.Refresh();
        }
    }
    private void PlayerRepeatLabel_Click(object sender, EventArgs e)
    {
        if (m_player != null)
        {
            //m_player.Looping = !m_player.Looping;
            //if (m_player.Looping)
            m_playerLooping = !m_playerLooping; // manual looping to allow different reciters to read the same verse
            if (m_playerLooping)
            {
                if (File.Exists("Images/player_repeat_on.png"))
                {
                    PlayerRepeatLabel.Image = new Bitmap("Images/player_repeat_on.png");
                }
            }
            else
            {
                if (File.Exists("Images/player_repeat.png"))
                {
                    PlayerRepeatLabel.Image = new Bitmap("Images/player_repeat.png");
                }
            }
            PlayerRepeatLabel.Refresh();
        }
    }
    private void PlayerRepeatSelectionLabel_Click(object sender, EventArgs e)
    {
        if (m_player != null)
        {
            m_playerLoopingAll = !m_playerLoopingAll;
            if (m_playerLoopingAll)
            {
                if (File.Exists("Images/player_repeat_all_on.png"))
                {
                    PlayerRepeatSelectionLabel.Image = new Bitmap("Images/player_repeat_all_on.png");
                }
            }
            else
            {
                if (File.Exists("Images/player_repeat_all.png"))
                {
                    PlayerRepeatSelectionLabel.Image = new Bitmap("Images/player_repeat_all.png");
                }
            }
            PlayerRepeatSelectionLabel.Refresh();
        }
    }
    private void PlayerMuteLabel_Click(object sender, EventArgs e)
    {
        if (m_player != null)
        {
            m_player.MuteAll = !m_player.MuteAll;
            if (m_player.MuteAll)
            {
                m_player.VolumeAll = 0;
                if (File.Exists("Images/player_muted.png"))
                {
                    PlayerMuteLabel.Image = new Bitmap("Images/player_muted.png");
                }
            }
            else
            {
                m_player.VolumeAll = m_audio_volume;
                if (File.Exists("Images/player_vol_hi.png"))
                {
                    PlayerMuteLabel.Image = new Bitmap("Images/player_vol_hi.png");
                }
            }
            PlayerMuteLabel.Refresh();
        }
    }
    private void PlayerVolumeTrackBar_Scroll(object sender, EventArgs e)
    {
        if (m_player != null)
        {
            m_audio_volume = PlayerVolumeTrackBar.Value * (1000 / PlayerVolumeTrackBar.Maximum);
            m_player.VolumeAll = m_audio_volume;
            ToolTip.SetToolTip(PlayerVolumeTrackBar, "Volume " + (m_audio_volume / (1000 / PlayerVolumeTrackBar.Maximum)).ToString() + "%");
        }
    }
    private void PlayerSilenceTrackBar_Scroll(object sender, EventArgs e)
    {
        if (m_player != null)
        {
            m_silence_between_verses = (float)PlayerSilenceTrackBar.Value / (PlayerSilenceTrackBar.Maximum / 2);
            ToolTip.SetToolTip(PlayerSilenceTrackBar, "Silence " + m_silence_between_verses.ToString("0.00") + " verses");
        }
    }

    private bool DownloadVerseAudioFile(Verse verse)
    {
        // mirror remote_folder locally
        string audio_folder = "Audio" + "/" + m_recitation_folder;
        if (!Directory.Exists(audio_folder))
        {
            Directory.CreateDirectory(audio_folder);
        }

        // generate audio_filename from verse address
        string audio_filename = "";
        string full_audio_folder = "";
        if (verse == null)
        {
            audio_filename = "001000" + "." + Recitation.FileType; // audhubillah
            full_audio_folder = audio_folder + "/" + "001";
        }
        else
        {
            if (verse.Chapter != null)
            {
                audio_filename = verse.Chapter.Number.ToString("000") + verse.NumberInChapter.ToString("000") + "." + Recitation.FileType;
                full_audio_folder = audio_folder + "/" + verse.Chapter.Number.ToString("000");
            }
        }

        // fill up local_audio_filename to return to caller
        m_downloaded_audio_filename = full_audio_folder + "/" + audio_filename;
        string outer_downloaded_audio_filename = audio_folder + "/" + audio_filename;
        if (File.Exists(m_downloaded_audio_filename))
        {
            return true; // no need to download
        }
        else if (File.Exists(outer_downloaded_audio_filename))
        {
            if (!Directory.Exists(full_audio_folder))
            {
                Directory.CreateDirectory(full_audio_folder);
            }

            if (Directory.Exists(full_audio_folder))
            {
                File.Move(outer_downloaded_audio_filename, m_downloaded_audio_filename);
            }

            return true; // no need to download
        }
        else
        {
            // try to download audio file
            this.Cursor = Cursors.WaitCursor;
            try
            {
                string recitation_url = "";
                foreach (string key in Client.Recitations.Keys)
                {
                    if (Client.Recitations[key].Folder == m_recitation_folder)
                    {
                        recitation_url = Recitation.UrlPrefix + Client.Recitations[key].Url + "/" + audio_filename;
                        break;
                    }
                }

                DownloadFile(recitation_url, m_downloaded_audio_filename);
                return true;
            }
            catch
            {
                PlayerStopLabel_Click(null, null);
                return false;
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
    }
    private void DownloadFile(string url, string path)
    {
        string download_folder = Path.GetDirectoryName(path);
        if (!Directory.Exists(download_folder))
        {
            Directory.CreateDirectory(download_folder);
        }

        Downloader.Download(url, Application.StartupPath + "/" + path, 10000);
        //using (WebClient web_client = new WebClient())
        //{
        //    web_client.DownloadDataCompleted += new DownloadDataCompletedEventHandler(DownloadDataCompleted);
        //    web_client.DownloadDataAsync(new Uri(url));
        //}
    }
    //private void DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
    //{
    //    // WARNING: runs on different thread to UI thread
    //    byte[] raw = e.Result;
    //}
    private void RecitationsComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (RecitationsComboBox.SelectedItem != null)
        {
            string reciter = RecitationsComboBox.SelectedItem.ToString();
            foreach (string key in Client.Recitations.Keys)
            {
                if (Client.Recitations[key].Reciter == reciter)
                {
                    m_recitation_folder = Client.Recitations[key].Folder;
                    break;
                }
            }
            RecitationGroupBox.Text = reciter;
        }
    }
    private void RecitationsApplySettingsLabel_Click(object sender, EventArgs e)
    {
        if (!RecitationsDownloadGroupBox.Visible)
        {
            UpdateRecitationsCheckedListBox();

            RecitationsDownloadGroupBox.Visible = true;
            RecitationsCancelSettingsLabel.Visible = true;
            RecitationsDownloadGroupBox.BringToFront();

            if (File.Exists("Images/arrow_down.png"))
            {
                RecitationsApplySettingsLabel.Image = new Bitmap("Images/arrow_down.png");
            }
            ToolTip.SetToolTip(RecitationsApplySettingsLabel, "Download complete Quran recitations");
        }
        else
        {
            RecitationsDownloadGroupBox.Visible = false;
            RecitationsCancelSettingsLabel.Visible = false;
            RecitationsDownloadGroupBox.SendToBack();

            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (File.Exists("Images/settings.png"))
                {
                    RecitationsApplySettingsLabel.Image = new Bitmap("Images/settings.png");
                }
                ToolTip.SetToolTip(RecitationsApplySettingsLabel, "Download recitation audios");

                List<string> keys_to_download = new List<string>();
                foreach (int cheched_index in RecitationsCheckedListBox.CheckedIndices)
                {
                    if (RecitationsCheckedListBox.GetItemCheckState(cheched_index) != CheckState.Indeterminate)
                    {
                        foreach (string key in Client.Recitations.Keys)
                        {
                            string reciter = RecitationsCheckedListBox.Items[cheched_index].ToString();
                            if (Client.Recitations[key].Reciter == reciter)
                            {
                                keys_to_download.Add(key);
                                break;
                            }
                        }
                    }
                }

                string recitations_folder = "Audio";
                foreach (string reciter_folder in Client.Recitations.Keys)
                {
                    if (keys_to_download.Contains(reciter_folder))
                    {
                        ProgressBar.Minimum = Verse.MIN_NUMBER;
                        ProgressBar.Maximum = Verse.MAX_NUMBER;
                        ProgressBar.Value = 1;
                        ProgressBar.Refresh();

                        for (int i = 0; i < Verse.MAX_NUMBER; i++)
                        {
                            string download_folder = recitations_folder + "/" + reciter_folder;
                            string filename = GetVerseAudioFilename(i); // e.g. i=8 ==> 002001.mp3
                            string full_filename = GetVerseAudioFullFilename(i); // e.g. i=8 ==> 002/002001.mp3
                            string full_path = download_folder + "/" + full_filename;
                            if (File.Exists(full_path)) // file exist
                            {
                                long filesize = (new FileInfo(full_path)).Length;
                                if (filesize < 1024) // if < 1kb invalid file then re-download
                                {
                                    DownloadFile(Recitation.UrlPrefix + Client.Recitations[reciter_folder].Url + "/" + filename, full_path);
                                }
                            }
                            else // file not found so download it
                            {
                                DownloadFile(Recitation.UrlPrefix + Client.Recitations[reciter_folder].Url + "/" + filename, full_path);
                            }

                            ProgressBar.Value = i + 1;
                            ProgressBar.Refresh();

                            Application.DoEvents();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName);
            }
            finally
            {
                if (m_client != null)
                {
                    if (m_client.Selection != null)
                    {
                        List<Verse> verses = m_client.Selection.Verses;
                        if (verses.Count > 0)
                        {
                            ProgressBar.Minimum = Verse.MIN_NUMBER;
                            ProgressBar.Maximum = Verse.MAX_NUMBER;
                            ProgressBar.Value = verses[0].Number;
                            ProgressBar.Refresh();
                        }
                        this.Cursor = Cursors.Default;
                    }
                }
            }
        }
        RecitationsApplySettingsLabel.Refresh();
    }
    private void RecitationsCancelSettingsLabel_Click(object sender, EventArgs e)
    {
        RecitationsDownloadGroupBox.Visible = false;
        RecitationsDownloadGroupBox.Refresh();
        RecitationsCancelSettingsLabel.Visible = RecitationsDownloadGroupBox.Visible;
        RecitationsCancelSettingsLabel.Refresh();
        if (File.Exists("Images/settings.png"))
        {
            RecitationsApplySettingsLabel.Image = new Bitmap("Images/settings.png");
        }
        ToolTip.SetToolTip(RecitationsApplySettingsLabel, "Setup recitations");
    }
    private void AskUserToDownloadAudioFilesManually()
    {
        if (MessageBox.Show("Cannot auto-download audio files.\r\n\r\n"
                          + "Would you like to manually download audio files\r\n"
                          + "and unzip them to: "
                          + "QuranCode\\Audio\\" + m_recitation_folder + "\\" + "\r\n"
                          + "?",
                          Application.ProductName, MessageBoxButtons.YesNo) == DialogResult.Yes)
        {
            Control control = new Control();
            foreach (string key in Client.Recitations.Keys)
            {
                if (Client.Recitations[key].Folder == m_recitation_folder)
                {
                    control.Tag = Recitation.DEFAULT_URL_PREFIX + Client.Recitations[key].Url;
                    LinkLabel_Click(control, null);
                    break;
                }
            }
        }
    }
    private string GetVerseAudioFilename(int verse_index)
    {
        if (m_client != null)
        {
            if (Book.Instance != null)
            {
                if ((verse_index >= 0) && (verse_index < Book.Instance.Verses.Count))
                {
                    Verse verse = Book.Instance.Verses[verse_index];
                    if (verse != null)
                    {
                        if (verse.Chapter != null)
                        {
                            return (verse.Chapter.Number.ToString("000") + verse.NumberInChapter.ToString("000") + "." + Recitation.FileType);
                        }
                    }
                }
            }
        }
        return "000000.mp3";
    }
    private string GetVerseAudioFullFilename(int verse_index)
    {
        if (m_client != null)
        {
            if (Book.Instance != null)
            {
                if ((verse_index >= 0) && (verse_index < Book.Instance.Verses.Count))
                {
                    Verse verse = Book.Instance.Verses[verse_index];
                    if (verse != null)
                    {
                        if (verse.Chapter != null)
                        {
                            return (verse.Chapter.Number.ToString("000") + "/" + verse.Chapter.Number.ToString("000") + verse.NumberInChapter.ToString("000") + "." + Recitation.FileType);
                        }
                    }
                }
            }
        }
        return "000/000000.mp3";
    }
    private void WaitForPlayToFinish()
    {
        while ((m_player.Length - m_player.Position) > (ulong)PlayerTimer.Interval)
        {
            Thread.Sleep(100);
        }
    }

    private void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Visible = true;

                this.WindowState = FormWindowState.Normal;

                this.Activate();    // bring to foreground
            }
            else if (this.WindowState == FormWindowState.Normal)
            {
                this.WindowState = FormWindowState.Minimized;

                this.Visible = false;
            }
        }
    }
    private void NotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            // make visible (in case it is hidden)
            this.Visible = true;

            // toggle maximized
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
            }
            else
            {
                this.WindowState = FormWindowState.Maximized;
            }

            // and bring to foreground
            this.Activate();
        }
    }
    private void VersionLabel_Click(object sender, EventArgs e)
    {
        AboutToolStripMenuItem_Click(null, null);
    }
    private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
    {
        m_about_box.ShowDialog(this);
    }
    private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
    {
        CloseApplication();
        Application.Exit();
        System.Environment.Exit(0);
    }

    private ContextMenu EditContextMenu = null;
    private MenuItem ExactMatchMenuItem = null;
    private MenuItem SameValueMenuItem = null;
    private MenuItem RelatedWordsMenuItem = null;
    private void RegisterContextMenu(TextBoxBase control)
    {
        EditContextMenu = new ContextMenu();
        MenuItem MenuItem = null;

        MenuItem = new MenuItem("Undo\t\tCtrl+Z");
        MenuItem.Click += new EventHandler(MenuItem_Undo);
        EditContextMenu.MenuItems.Add(MenuItem);

        //MenuItem = new MenuItem("Redo\t\tCtrl+Y");
        //MenuItem.Click += new EventHandler(MenuItem_Redo);
        //TextBoxBaseContextMenu.MenuItems.Add(MenuItem);

        MenuItem = new MenuItem("-");
        EditContextMenu.MenuItems.Add(MenuItem);

        MenuItem = new MenuItem("Cut\t\tCtrl+X");
        MenuItem.Click += new EventHandler(MenuItem_Cut);
        EditContextMenu.MenuItems.Add(MenuItem);

        MenuItem = new MenuItem("Copy\t\tCtrl+C");
        MenuItem.Click += new EventHandler(MenuItem_Copy);
        EditContextMenu.MenuItems.Add(MenuItem);

        MenuItem = new MenuItem("Paste\t\tCtrl+V");
        MenuItem.Click += new EventHandler(MenuItem_Paste);
        EditContextMenu.MenuItems.Add(MenuItem);

        MenuItem = new MenuItem("-");
        EditContextMenu.MenuItems.Add(MenuItem);

        MenuItem = new MenuItem("Select All\tCtrl+A");
        MenuItem.Click += new EventHandler(MenuItem_SelectAll);
        EditContextMenu.MenuItems.Add(MenuItem);

        if (control == MainTextBox)
        {
            MenuItem = new MenuItem("-");
            EditContextMenu.MenuItems.Add(MenuItem);

            MenuItem = new MenuItem("Exact Match\tF4");
            MenuItem.Click += new EventHandler(MenuItem_ExactMatch);
            EditContextMenu.MenuItems.Add(MenuItem);
            ExactMatchMenuItem = MenuItem;

            MenuItem = new MenuItem("Related Words\tF5");
            MenuItem.Click += new EventHandler(MenuItem_RelatedWords);
            EditContextMenu.MenuItems.Add(MenuItem);
            RelatedWordsMenuItem = MenuItem;

            MenuItem = new MenuItem("-");
            EditContextMenu.MenuItems.Add(MenuItem);

            MenuItem = new MenuItem("Same Value\tF9");
            MenuItem.Click += new EventHandler(MenuItem_SameValue);
            EditContextMenu.MenuItems.Add(MenuItem);
            SameValueMenuItem = MenuItem;
        }

        EditContextMenu.Popup += new EventHandler(ContextMenu_Popup);
        EditContextMenu.Collapse += new EventHandler(ContextMenu_Collapse);

        control.ContextMenu = EditContextMenu;
    }
    private void ContextMenu_Popup(object sender, EventArgs e)
    {
        //if (sender is ContextMenu)
        //{
        //    if ((sender as ContextMenu).SourceControl is TextBoxBase)
        //    {
        //        ((sender as ContextMenu).SourceControl as TextBoxBase).Cursor = Cursors.Arrow;
        //    }
        //}

        // Enable/Disable Cut menu
        if (sender is ContextMenu)
        {
            if (
                 ((sender as ContextMenu).SourceControl == MainTextBox)
                 ||
                 ((sender as ContextMenu).SourceControl == TranslationTextBox)
               )
            {
                (sender as ContextMenu).MenuItems[2].Enabled = !((sender as ContextMenu).SourceControl as TextBoxBase).ReadOnly;
            }
        }
    }
    private void ContextMenu_Collapse(object sender, EventArgs e)
    {
        //if (sender is ContextMenu)
        //{
        //    if ((sender as ContextMenu).SourceControl is TextBoxBase)
        //    {
        //        ((sender as ContextMenu).SourceControl as TextBoxBase).Cursor = Cursors.Default;
        //    }
        //}
    }
    private void MenuItem_Undo(object sender, EventArgs e)
    {
        if (sender is MenuItem)
        {
            if ((sender as MenuItem).Parent is ContextMenu)
            {
                if (((sender as MenuItem).Parent as ContextMenu).SourceControl is TextBoxBase)
                {
                    (((sender as MenuItem).Parent as ContextMenu).SourceControl as TextBoxBase).Undo();
                }
            }
        }
    }
    //private void MenuItem_Redo(object sender, EventArgs e)
    //{
    //    if (sender is MenuItem)
    //    {
    //        if ((sender as MenuItem).Parent is ContextMenu)
    //        {
    //            if (((sender as MenuItem).Parent as ContextMenu).SourceControl is TextBoxBase)
    //            {
    //                (((sender as MenuItem).Parent as ContextMenu).SourceControl as TextBoxBase).Redo();
    //            }
    //        }
    //    }
    //}
    private void MenuItem_Cut(object sender, EventArgs e)
    {
        if (sender is MenuItem)
        {
            if ((sender as MenuItem).Parent is ContextMenu)
            {
                if (((sender as MenuItem).Parent as ContextMenu).SourceControl is TextBoxBase)
                {
                    (((sender as MenuItem).Parent as ContextMenu).SourceControl as TextBoxBase).Cut();
                }
            }
        }
    }
    private void MenuItem_Copy(object sender, EventArgs e)
    {
        if (sender is MenuItem)
        {
            if ((sender as MenuItem).Parent is ContextMenu)
            {
                if (((sender as MenuItem).Parent as ContextMenu).SourceControl is TextBoxBase)
                {
                    (((sender as MenuItem).Parent as ContextMenu).SourceControl as TextBoxBase).Copy();
                }
            }
        }
    }
    private void MenuItem_Paste(object sender, EventArgs e)
    {
        if (sender is MenuItem)
        {
            if ((sender as MenuItem).Parent is ContextMenu)
            {
                if (((sender as MenuItem).Parent as ContextMenu).SourceControl is TextBoxBase)
                {
                    // intercept Paste and Simplify Text before pasting
                    SimplifyClipboardTextBeforePaste();
                    Thread.Sleep(100); // must give chance for Clipboard to refresh its content before Paste
                    (((sender as MenuItem).Parent as ContextMenu).SourceControl as TextBoxBase).Paste();
                    RestoreClipboardTextAfterPaste();
                }
            }
        }
    }
    private void MenuItem_SelectAll(object sender, EventArgs e)
    {
        if (sender is MenuItem)
        {
            if ((sender as MenuItem).Parent is ContextMenu)
            {
                if (((sender as MenuItem).Parent as ContextMenu).SourceControl is TextBoxBase)
                {
                    (((sender as MenuItem).Parent as ContextMenu).SourceControl as TextBoxBase).SelectAll();
                    (((sender as MenuItem).Parent as ContextMenu).SourceControl as TextBoxBase).KeyDown += new KeyEventHandler(TextBox_KeyDown);
                }
            }
        }
    }
    private void MenuItem_ExactMatch(object sender, EventArgs e)
    {
        if (sender is MenuItem)
        {
            if ((sender as MenuItem).Parent is ContextMenu)
            {
                if (((sender as MenuItem).Parent as ContextMenu).SourceControl == MainTextBox)
                {
                    DoFindExactMatch(((sender as MenuItem).Parent as ContextMenu).SourceControl);
                }
            }
        }
    }
    private void MenuItem_SameValue(object sender, EventArgs e)
    {
        if (sender is MenuItem)
        {
            if ((sender as MenuItem).Parent is ContextMenu)
            {
                if (((sender as MenuItem).Parent as ContextMenu).SourceControl == MainTextBox)
                {
                    DoFindSameValue(((sender as MenuItem).Parent as ContextMenu).SourceControl);
                }
            }
        }
    }
    private void MenuItem_RelatedWords(object sender, EventArgs e)
    {
        if (sender is MenuItem)
        {
            if ((sender as MenuItem).Parent is ContextMenu)
            {
                if (((sender as MenuItem).Parent as ContextMenu).SourceControl == MainTextBox)
                {
                    DoFindRelatedWords(((sender as MenuItem).Parent as ContextMenu).SourceControl);
                }
            }
        }
    }
    private string RemovePunctuationMarks(string text)
    {
        if (!String.IsNullOrEmpty(text))
        {
            if (m_find_by_text_language_type == FindByTextLanguageType.Translation)
            {
                text = text.Replace(".", "");
                text = text.Replace(",", "");
                text = text.Replace(";", "");
                text = text.Replace(":", "");
                text = text.Replace("?", "");
                text = text.Replace("/", "");
                text = text.Replace(")", "");
                text = text.Replace("(", "");
                text = text.Replace(">", "");
                text = text.Replace("<", "");
                text = text.Replace("[", "");
                text = text.Replace("]", "");
                text = text.Replace("{", "");
                text = text.Replace("}", "");
                text = text.Replace("-", "");
                text = text.Replace("\"", "");
                text = text.Replace("\'", "");
                text = text.Replace("!", "");
                text = text.Replace("`", "");
                text = text.Replace("@", "");
                text = text.Replace("#", "");
                text = text.Replace("$", "");
                text = text.Replace("%", "");
                text = text.Replace("^", "");
                text = text.Replace("&", "");
                text = text.Replace("|", "");
                text = text.Replace("*", "");
                text = text.Replace("=", "");
            }
        }
        return text;
    }
    private void DoFindExactMatch(object sender)
    {
        if (sender is TextBoxBase)
        {
            string text = (sender as TextBoxBase).SelectedText.Trim();
            if (text.Length == 0)
            {
                Word current_word = GetWordAtCursor(sender);
                if (current_word == null)
                {
                    return;
                }
                text = current_word.Texts[m_client.TextMode];
            }
            text = RemovePunctuationMarks(text);

            if (!String.IsNullOrEmpty(text))
            {
                FindByTextTextBox.Text = text;
                FindByTextLanguageType language_type = m_find_by_text_language_type;

                string translation = DEFAULT_TRANSLATION;
                if (TranslatorComboBox.SelectedItem != null)
                {
                    translation = Client.GetTranslationKey(TranslatorComboBox.SelectedItem.ToString());
                }

                FindByTextLocation text_location = FindByTextLocation.Anywhere;
                bool case_sensitive = false;
                FindByTextWordness wordness = FindByTextWordness.Any;
                int multiplicity = -1;
                FindByText(text, language_type, translation, text_location, case_sensitive, wordness, multiplicity, true, false);
            }
        }
    }
    private void DoFindRelatedWords(object sender)
    {
        if (sender is TextBoxBase)
        {
            string text = (sender as TextBoxBase).SelectedText.Trim();
            if (text.Length == 0)
            {
                Word current_word = GetWordAtCursor(sender);
                if (current_word == null)
                {
                    return;
                }
                text = current_word.Texts[m_client.TextMode];
            }
            text = RemovePunctuationMarks(text);

            // if a whole word, search by root
            if ((text.Length > 0) && (!text.Contains(" ")) && (!text.Contains("\t")))
            {
                string simplified_text = Client.SimplifyText(text, "Simplified29");
                FindByTextTextBox.Text = simplified_text;
                int multiplicity = -1;
                FindByRoot(simplified_text, multiplicity);
            }
        }
    }
    private void DoFindSameValue(object sender)
    {
        if (sender is TextBoxBase)
        {
            string text = (sender as TextBoxBase).SelectedText.Trim();
            if (text.Length == 0) // no selection, get word under mouse pointer
            {
                Word current_word = GetWordAtCursor(sender);
                if (current_word == null)
                {
                    return;
                }
                text = current_word.Texts[m_client.TextMode];
            }

            long value = CalculateValue(text);
            DoFindSameValue(value);
        }
    }
    private void DoFindSameValue(long value)
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            if (m_client != null)
            {
                int match_count = 0;
                List<Verse> found_verses = null;
                List<Phrase> found_phrases = null;
                PrepareNewSearch();

                string text = "value" + "=" + value.ToString();

                m_client.FindWords(0, 0, 0, value,
                                   NumberType.None, NumberType.None, NumberType.None, NumberType.None,
                                   ComparisonOperator.Equal, ComparisonOperator.Equal, ComparisonOperator.Equal, ComparisonOperator.Equal);
                if (m_client.FoundWords != null)
                {
                    match_count += m_client.FoundWords.Count;
                    // save result before running another search
                    found_verses = m_client.FoundVerses;
                    found_phrases = m_client.FoundPhrases;
                }

                m_client.FindWordRanges(0, 0, 0, value,
                   NumberType.None, NumberType.None, NumberType.None, NumberType.None,
                   ComparisonOperator.Equal, ComparisonOperator.Equal, ComparisonOperator.Equal, ComparisonOperator.Equal);
                if (m_client.FoundWordRanges != null)
                {
                    match_count += m_client.FoundWordRanges.Count;
                    // insert previous result before this result
                    m_client.FoundVerses.InsertRange(0, found_verses);
                    m_client.FoundPhrases.InsertRange(0, found_phrases);
                    // save result before running another search
                    found_verses = m_client.FoundVerses;
                    found_phrases = m_client.FoundPhrases;
                }

                m_client.FindVerses(0, 0, 0, 0, value,
                                   NumberType.None, NumberType.None, NumberType.None, NumberType.None, NumberType.None,
                                   ComparisonOperator.Equal, ComparisonOperator.Equal, ComparisonOperator.Equal, ComparisonOperator.Equal, ComparisonOperator.Equal);
                if (m_client.FoundVerses != null)
                {
                    match_count += m_client.FoundVerses.Count;
                    // insert previous result before this result
                    m_client.FoundVerses.InsertRange(0, found_verses);
                    m_client.FoundPhrases.InsertRange(0, found_phrases);

                    m_find_result_header = match_count + ((match_count == 1) ? " match" : " matches") + " in " + m_client.FoundVerses.Count + ((m_client.FoundVerses.Count == 1) ? " verse" : " verses") + " with " + text + " in " + FindScopeLabel.Text;
                    DisplayFoundVerses(true, false);
                }
            }
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }

    // TextBox has no Ctrl+A by default
    private void TextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (ModifierKeys == Keys.Control)
        {
            if (e.KeyCode == Keys.A)
            {
                if (sender is TextBoxBase)
                {
                    (sender as TextBoxBase).SelectAll();
                }
            }
        }
        else
        {
            if (e.KeyCode == Keys.Tab)
            {
                e.Handled = true;
            }
        }
    }

    private string m_clipboard_text = "";
    private void SimplifyClipboardTextBeforePaste()
    {
        m_clipboard_text = Clipboard.GetText(TextDataFormat.UnicodeText);
        if ((m_clipboard_text != null) && (m_clipboard_text.Length > 0))
        {
            if (m_client != null)
            {
                string simplified_text = Client.SimplifyText(m_clipboard_text, m_client.TextMode);
                if ((simplified_text != null) && (simplified_text.Length > 0))
                {
                    Clipboard.SetText(simplified_text, TextDataFormat.UnicodeText);
                }
            }
        }
    }
    private void RestoreClipboardTextAfterPaste()
    {
        if ((m_clipboard_text != null) && (m_clipboard_text.Length > 0))
        {
            Clipboard.SetText(m_clipboard_text, TextDataFormat.UnicodeText);
        }
    }

    private void InitializeControls()
    {
        VersionLabel.Text = Globals.SHORT_VERSION;

        if (NoteTextBox.Text.Length > 0)
        {
            m_note_writing_instruction = NoteTextBox.Text;
        }

        // use Right-Click for going to Related Words instead of shwoing context menu
        RegisterContextMenu(MainTextBox);
        RegisterContextMenu(TranslationTextBox);
        MainTextBox.ReadOnly = true;
        TranslationTextBox.ReadOnly = true;
        RegisterContextMenu(FindByTextTextBox);
        RegisterContextMenu(FindByFrequencyPhraseTextBox);
        RegisterContextMenu(ValueTextBox);
        RegisterContextMenu(NthPrimeTextBox);
        RegisterContextMenu(NthAdditivePrimeTextBox);
        RegisterContextMenu(NthPurePrimeTextBox);

        DrawingPictureBoxEx.BringToFront();
        DrawingPictureBoxEx.Visible = false;
        DrawingPictureBoxEx.Refresh();

        DrawPrimesLabel.ForeColor = GetNumberTypeColor(19);
        DrawAdditivePrimesLabel.ForeColor = GetNumberTypeColor(47);
        DrawPurePrimesLabel.ForeColor = GetNumberTypeColor(313);

        PageNumericUpDown.Minimum = Page.MIN_NUMBER;
        PageNumericUpDown.Maximum = Page.MAX_NUMBER;
        StationNumericUpDown.Minimum = Station.MIN_NUMBER;
        StationNumericUpDown.Maximum = Station.MAX_NUMBER;
        PartNumericUpDown.Minimum = Part.MIN_NUMBER;
        PartNumericUpDown.Maximum = Part.MAX_NUMBER;
        GroupNumericUpDown.Minimum = Model.Group.MIN_NUMBER;
        GroupNumericUpDown.Maximum = Model.Group.MAX_NUMBER;
        QuarterNumericUpDown.Minimum = Quarter.MIN_NUMBER;
        QuarterNumericUpDown.Maximum = Quarter.MAX_NUMBER;
        BowingNumericUpDown.Minimum = Bowing.MIN_NUMBER;
        BowingNumericUpDown.Maximum = Bowing.MAX_NUMBER;
        PageNumericUpDown.Minimum = Page.MIN_NUMBER;
        PageNumericUpDown.Maximum = Page.MAX_NUMBER;
        VerseNumericUpDown.Minimum = Verse.MIN_NUMBER;
        VerseNumericUpDown.Maximum = Verse.MAX_NUMBER;

        // install default size font
        SetFontSize(DEFAULT_FONT_SIZE);
        SetTranslationFontSize(DEFAULT_TRANSLATION_FONT_SIZE);

        SetupToolTips();
    }
    private void SetupToolTips()
    {
        this.ToolTip.SetToolTip(this.WebsiteLabel, "Author website");
        this.ToolTip.SetToolTip(this.DrawWordValuesButton, "Draw word values of current selection");
        this.ToolTip.SetToolTip(this.DrawLetterValuesButton, "Draw letter values of current selection");
        this.ToolTip.SetToolTip(this.DrawWordAllahButton, "Draw Allah words");
        this.ToolTip.SetToolTip(this.DrawWordsWithAllahButton, "Draw words with Allah");
        this.ToolTip.SetToolTip(this.DrawWordsWithAllahHelpButton, "Show words with Allah");
        this.ToolTip.SetToolTip(this.DrawPrimesLabel, "Draw Primes");
        this.ToolTip.SetToolTip(this.DrawAdditivePrimesLabel, "Draw Additive Primes");
        this.ToolTip.SetToolTip(this.DrawPurePrimesLabel, "Draw Pure Primes");
        this.ToolTip.SetToolTip(this.DrawAllPrimesLabel, "Draw All Primes");
        this.ToolTip.SetToolTip(this.GenerateAllPrimeDrawingsLabel, "Generate and Save All Prime Drawings");
        this.ToolTip.SetToolTip(this.VersionLabel, "About");
        this.ToolTip.SetToolTip(this.PrimalogyLabel, "The Encrypted Quran");
        this.ToolTip.SetToolTip(this.PlayerPreviousLabel, "Previous verse");
        this.ToolTip.SetToolTip(this.PlayerPlayLabel, "Play");
        this.ToolTip.SetToolTip(this.PlayerNextLabel, "Next verse");
        this.ToolTip.SetToolTip(this.PlayerStopLabel, "Stop");
        this.ToolTip.SetToolTip(this.PlayerRepeatLabel, "Repeat verse");
        this.ToolTip.SetToolTip(this.PlayerRepeatSelectionLabel, "Repeat selection");
        this.ToolTip.SetToolTip(this.PlayerMuteLabel, "Mute");
        this.ToolTip.SetToolTip(this.VerseByVerseNumberLabel, "Go to verse number = current value");
        this.ToolTip.SetToolTip(this.VerseByLetterNumberLabel, "Go to verse containing letter number = current value");
        this.ToolTip.SetToolTip(this.VerseByWordNumberLabel, "Go to verse containing word number = current value");
        this.ToolTip.SetToolTip(this.UndoValueNavigationLabel, "Back");
        this.ToolTip.SetToolTip(this.RedoValueNavigationLabel, "Forward");
        this.ToolTip.SetToolTip(this.TextModeComboBox, "Text Mode نظام تبسيط النص");
        this.ToolTip.SetToolTip(this.ValueTextBox, "Enter an expression to factor");
        this.ToolTip.SetToolTip(this.FindScopeLabel, "Search in Entire Book");
        this.ToolTip.SetToolTip(this.FindByTextLanguageTypeLabel, "Search in Uthmani or emlaaei Arabic");
        this.ToolTip.SetToolTip(this.FindByTextSearchTypeLabel, "Search for exact word or phrase");
        this.ToolTip.SetToolTip(this.FindBySimilaritySourceLabel, "as current verse");
        this.ToolTip.SetToolTip(this.FindBySimilarityPercentageTrackBar, "similarity percentage");
        this.ToolTip.SetToolTip(this.FindBySimilaritySimilarTextRadioButton, "verses with similar text");
        this.ToolTip.SetToolTip(this.FindBySimilaritySimilarWordsRadioButton, "verses with similar words");
        this.ToolTip.SetToolTip(this.FindBySimilaritySimilarStartRadioButton, "verses with similar first word");
        this.ToolTip.SetToolTip(this.FindBySimilaritySimilarEndRadioButton, "verses with similar last word");
        this.ToolTip.SetToolTip(this.FindByNumbersTextRangeLabel, "for any verse matching conditions");
        this.ToolTip.SetToolTip(this.FindByTextAtEndRadioButton, "find at the end of the verse");
        this.ToolTip.SetToolTip(this.FindByTextAtStartRadioButton, "find at the beginning of the verse");
        this.ToolTip.SetToolTip(this.FindByTextAnywhereRadioButton, "find anywhere in the verse");
        this.ToolTip.SetToolTip(this.FindByTextAtMiddleRadioButton, "find anywhere in the middle of the verse");
        this.ToolTip.SetToolTip(this.DigitalRootLabel, "Digit sum");
        this.ToolTip.SetToolTip(this.DigitalRootTextBox, "Digital root");
        this.ToolTip.SetToolTip(this.NthPrimeTextBox, "Find prime by index");
        this.ToolTip.SetToolTip(this.NthAdditivePrimeTextBox, "Find additive prime by index");
        this.ToolTip.SetToolTip(this.NthPurePrimeTextBox, "Find pure prime by index");
        this.ToolTip.SetToolTip(this.ChapterComboBox, "CC, CC:VV, CC-CC, CC:VV-CC, CC-CC:VV, CC:VV-CC:VV");        // 11, 13-14, 15:55, 16:19-23, 13-14:19, 24:35-27:62
        this.ToolTip.SetToolTip(this.ChapterVerseNumericUpDown, "ءاية");
        this.ToolTip.SetToolTip(this.ChapterWordNumericUpDown, "كلمة");
        this.ToolTip.SetToolTip(this.ChapterLetterNumericUpDown, "حرف");
        this.ToolTip.SetToolTip(this.PartNumericUpDown, "جزء");
        this.ToolTip.SetToolTip(this.PageNumericUpDown, "صفحة");
        this.ToolTip.SetToolTip(this.StationNumericUpDown, "منزل");
        this.ToolTip.SetToolTip(this.GroupNumericUpDown, "حزب");
        this.ToolTip.SetToolTip(this.QuarterNumericUpDown, "ربع حزب");
        this.ToolTip.SetToolTip(this.BowingNumericUpDown, "ركوع");
        this.ToolTip.SetToolTip(this.VerseNumericUpDown, "ءاية");
        this.ToolTip.SetToolTip(this.WordNumericUpDown, "كلمة");
        this.ToolTip.SetToolTip(this.LetterNumericUpDown, "حرف");
        this.ToolTip.SetToolTip(this.VerseDiffTextBox, "فرق ءايات");
        this.ToolTip.SetToolTip(this.WordDiffTextBox, "فرق كلمات");
        this.ToolTip.SetToolTip(this.LetterDiffTextBox, "فرق حروف");
        this.ToolTip.SetToolTip(this.FindByTextCaseSensitiveCheckBox, "case sensitive for non-Arabic");
        this.ToolTip.SetToolTip(this.FindByTextWordnessCheckBox, "find verses with whole word only");
        this.ToolTip.SetToolTip(this.FindScopeLabel, "Search scope");
        this.ToolTip.SetToolTip(this.FindByTextTextBox, "text to search for");
        this.ToolTip.SetToolTip(this.FindByNumbersUniqueLettersLabel, "unique letters");
        this.ToolTip.SetToolTip(this.FindByNumbersValueLabel, "numerology value");
        this.ToolTip.SetToolTip(this.FindByFrequencySumTypeLabel, "include duplicate phrase letters");
        this.ToolTip.SetToolTip(this.LetterOrderSystemComboBox, "Letter Order نظام ترتيب الحروف");
        this.ToolTip.SetToolTip(this.LetterValueSystemComboBox, "Letter Values نظام تقيم الحروف");
        this.ToolTip.SetToolTip(this.SaveLetterValuationButton, "Save Letter Valuation");
        this.ToolTip.SetToolTip(this.ResetNumerologySystemButton, "Primalogy System");
        this.ToolTip.SetToolTip(this.SaveLetterStatisticsButton, "Save Letter Statistics");
        this.ToolTip.SetToolTip(this.SavePhraseLetterStatisticsButton, "Save Phrase Letter Statistics");
        this.ToolTip.SetToolTip(this.DynamicLetterScopeCheckBox, "Build letter valuation system dynamically");
        this.ToolTip.SetToolTip(this.DynamicLetterScopeSelectionRadioButton, "Use letters of current selection to build  a dynamic valuation system");
        this.ToolTip.SetToolTip(this.DynamicLetterScopeHighlightedTextRadioButton, "Use letters of current line or highlighted text to build a dynamic valuation system");
        this.ToolTip.SetToolTip(this.AddToLetterLNumberCheckBox, "Add L (letter number in word)");
        this.ToolTip.SetToolTip(this.AddToLetterWNumberCheckBox, "Add W (word number in verse)");
        this.ToolTip.SetToolTip(this.AddToLetterVNumberCheckBox, "Add V (verse number in chapter)");
        this.ToolTip.SetToolTip(this.AddToLetterCNumberCheckBox, "Add C (chapter number in the Quran)");
        this.ToolTip.SetToolTip(this.AddToLetterLDistanceCheckBox, "Add ∆L distance to previous same letter");
        this.ToolTip.SetToolTip(this.AddToLetterWDistanceCheckBox, "Add ∆W distance to previous same letter");
        this.ToolTip.SetToolTip(this.AddToLetterVDistanceCheckBox, "Add ∆V distance to previous same letter");
        this.ToolTip.SetToolTip(this.AddToLetterCDistanceCheckBox, "Add ∆C distance to previous same letter");
        this.ToolTip.SetToolTip(this.AddToWordWNumberCheckBox, "Add W (word number in verse)");
        this.ToolTip.SetToolTip(this.AddToWordVNumberCheckBox, "Add V (verse number in chapter)");
        this.ToolTip.SetToolTip(this.AddToWordCNumberCheckBox, "Add C (chapter number in the Quran)");
        this.ToolTip.SetToolTip(this.AddToWordWDistanceCheckBox, "Add ∆W distance to previous same word");
        this.ToolTip.SetToolTip(this.AddToWordVDistanceCheckBox, "Add ∆V distance to previous same word");
        this.ToolTip.SetToolTip(this.AddToWordCDistanceCheckBox, "Add ∆C distance to previous same word");
        this.ToolTip.SetToolTip(this.AddToVerseVNumberCheckBox, "Add V (verse number in chapter)");
        this.ToolTip.SetToolTip(this.AddToVerseCNumberCheckBox, "Add C (chapter number in the Quran)");
        this.ToolTip.SetToolTip(this.AddToVerseVDistanceCheckBox, "Add ∆V distance to previous same verse");
        this.ToolTip.SetToolTip(this.AddToVerseCDistanceCheckBox, "Add ∆C distance to previous same verse");
        this.ToolTip.SetToolTip(this.AddToChapterCNumberCheckBox, "Add C (chapter number in the Quran)");
        this.ToolTip.SetToolTip(this.FindHistoryDeleteButton, "Delete");
        this.ToolTip.SetToolTip(this.LetterCountLabel, "Unique letters");
        this.ToolTip.SetToolTip(this.FindByTextMultiplicityCheckBox, "find verses with given number of word repetitions");
        this.ToolTip.SetToolTip(this.LockUnlockLabel, "Readonly mode");
        this.ToolTip.SetToolTip(this.WordWrapLabel, "Word wrap");
        this.ToolTip.SetToolTip(this.BrowseHistoryDeleteButton, "Delete");
        this.ToolTip.SetToolTip(this.FindByTextAllWordsRadioButton, "find verses with all words in any order");
        this.ToolTip.SetToolTip(this.FindByTextAnyWordRadioButton, "find verses with at least one word");
        this.ToolTip.SetToolTip(this.FindByTextAllTextModesCheckBox, "All texts");
        this.ToolTip.SetToolTip(this.HadiLabel, "©2005 Hadi Al-Thahaby");
    }
    private void PopulateTextModeComboBox()
    {
        try
        {
            TextModeComboBox.SelectedIndexChanged -= new EventHandler(TextModeComboBox_SelectedIndexChanged);
            if (m_client != null)
            {
                // add text_modes to ComboBoxes
                TextModeComboBox.BeginUpdate();
                TextModeComboBox.Items.Clear();
                foreach (string text_mode in Client.TextModes)
                {
                    TextModeComboBox.Items.Add(text_mode);
                }
            }
        }
        finally
        {
            TextModeComboBox.EndUpdate();
            TextModeComboBox.SelectedIndexChanged += new EventHandler(TextModeComboBox_SelectedIndexChanged);
        }
    }
    private void PopulateChapterComboBox()
    {
        try
        {
            ChapterComboBox.SelectedIndexChanged -= new EventHandler(ChapterComboBox_SelectedIndexChanged);
            if (m_client != null)
            {
                if (Book.Instance != null)
                {
                    ChapterComboBox.BeginUpdate();
                    ChapterComboBox.Items.Clear();
                    foreach (Chapter chapter in Book.Instance.Chapters)
                    {
                        ChapterComboBox.Items.Add(chapter.Number + " - " + chapter.Name);
                    }
                }
            }
        }
        finally
        {
            ChapterComboBox.EndUpdate();
            ChapterComboBox.SelectedIndexChanged += new EventHandler(ChapterComboBox_SelectedIndexChanged);
        }
    }
    private void PopulateChaptersListBox()
    {
        ChapterGroupBox.Text = " Chapters ";
        try
        {
            ChaptersListBox.SelectedIndexChanged -= new EventHandler(ChaptersListBox_SelectedIndexChanged);
            if (m_client != null)
            {
                if (Book.Instance != null)
                {
                    ChaptersListBox.BeginUpdate();
                    ChaptersListBox.Items.Clear();
                    foreach (Chapter chapter in Book.Instance.Chapters)
                    {
                        ChaptersListBox.Items.Add(chapter.Number + " - " + chapter.Name);
                    }
                }
            }
        }
        finally
        {
            ChaptersListBox.EndUpdate();
            ChaptersListBox.SelectedIndexChanged += new EventHandler(ChaptersListBox_SelectedIndexChanged);
        }
    }
    private List<string> m_downloaded_reciter_folders = null;
    //private void MoveAudioFilesToFolders()
    //{
    //    this.Cursor = Cursors.WaitCursor;
    //    try
    //    {
    //        string recitations_folder = "Audio";
    //        foreach (string reciter_folder in Client.Recitations.Keys)
    //        {
    //            for (int i = 0; i < Verse.MAX_NUMBER; i++)
    //            {
    //                string download_folder = recitations_folder + "/" + reciter_folder;
    //                string filename = GetVerseAudioFilename(i); // e.g. i=8 ==> 002001.mp3
    //                string full_filename = GetVerseAudioFullFilename(i); // e.g. i=8 ==> 002/002001.mp3
    //                string path = download_folder + "/" + filename;
    //                string full_path = download_folder + "/" + full_filename;
    //                if (File.Exists(path)) // file exist
    //                {
    //                    if (File.Exists(full_path)) // file exist
    //                    {
    //                        long filesize = (new FileInfo(full_path)).Length;
    //                        if (filesize > 1024) // valid file
    //                        {
    //                            File.Delete(path);
    //                        }
    //                        else
    //                        {
    //                            filesize = (new FileInfo(path)).Length;
    //                            if (filesize > 1024) // valid file
    //                            {
    //                                File.Move(path, full_path);
    //                            }
    //                            else
    //                            {
    //                                File.Delete(path);
    //                            }
    //                        }
    //                    }
    //                    else
    //                    {
    //                        long filesize = (new FileInfo(path)).Length;
    //                        if (filesize > 1024) // valid file
    //                        {
    //                            string full_foldername = full_path.Substring(0, full_path.Length - 10);
    //                            if (!Directory.Exists(full_foldername))
    //                            {
    //                                Directory.CreateDirectory(full_foldername);
    //                            }
    //                            if (Directory.Exists(full_foldername))
    //                            {
    //                                File.Move(path, full_path);
    //                            }
    //                        }
    //                        else
    //                        {
    //                            File.Delete(path);
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        MessageBox.Show(ex.Message, Application.ProductName);
    //    }
    //    finally
    //    {
    //        this.Cursor = Cursors.Default;
    //    }
    //}
    private void UpdateRecitationsCheckedListBox()
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            /////////////////////////////////////////////////////////////////////////////
            // foreach reciter -> foreach verse, if audio file exist and valid then check
            /////////////////////////////////////////////////////////////////////////////

            if (m_downloaded_reciter_folders == null)
            {
                m_downloaded_reciter_folders = new List<string>();
            }
            m_downloaded_reciter_folders.Clear();

            string recitations_folder = "Audio";
            foreach (string reciter_folder in Client.Recitations.Keys)
            {
                bool fully_downloaded = true;
                for (int i = 0; i < Verse.MAX_NUMBER; i++)
                {
                    string download_folder = recitations_folder + "/" + reciter_folder;
                    string filename = GetVerseAudioFilename(i); // e.g. i=8 ==> 002001.mp3
                    string full_filename = GetVerseAudioFullFilename(i); // e.g. i=8 ==> 002/002001.mp3
                    string full_path = download_folder + "/" + full_filename;
                    if (File.Exists(full_path)) // file exist
                    {
                        long filesize = (new FileInfo(full_path)).Length;
                        if (filesize < 1024) // invalid file
                        {
                            fully_downloaded = false;
                            break;
                        }
                    }
                    else // file not found
                    {
                        fully_downloaded = false;
                        break;
                    }
                }

                int index = 0;
                string reciter = Client.Recitations[reciter_folder].Reciter;
                for (int i = 0; i < RecitationsCheckedListBox.Items.Count; i++)
                {
                    if (RecitationsCheckedListBox.Items[i].ToString() == reciter)
                    {
                        index = i;
                    }
                }

                if (fully_downloaded)
                {
                    RecitationsCheckedListBox.SetItemCheckState(index, CheckState.Indeterminate);
                    m_downloaded_reciter_folders.Add(reciter_folder);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }
    private void PopulateRecitationsCheckedListBox()
    {
        try
        {
            RecitationsCheckedListBox.SelectedIndexChanged -= new EventHandler(RecitationsCheckedListBox_SelectedIndexChanged);
            RecitationsCheckedListBox.BeginUpdate();
            RecitationsCheckedListBox.Items.Clear();
            foreach (string key in Client.Recitations.Keys)
            {
                string reciter = Client.Recitations[key].Reciter;
                RecitationsCheckedListBox.Items.Add(reciter);
            }
        }
        finally
        {
            RecitationsCheckedListBox.EndUpdate();
            RecitationsCheckedListBox.SelectedIndexChanged += new EventHandler(RecitationsCheckedListBox_SelectedIndexChanged);
        }
    }
    private void PopulateRecitationsComboBox()
    {
        try
        {
            RecitationsComboBox.BeginUpdate();
            RecitationsComboBox.SelectedIndexChanged -= new EventHandler(RecitationsComboBox_SelectedIndexChanged);
            RecitationsComboBox.Items.Clear();
            foreach (string key in Client.Recitations.Keys)
            {
                string reciter = Client.Recitations[key].Reciter;
                RecitationsComboBox.Items.Add(reciter);
            }
            if (RecitationsComboBox.Items.Count > 3)
            {
                RecitationsComboBox.SelectedIndex = 3;
            }
        }
        finally
        {
            RecitationsComboBox.EndUpdate();
            RecitationsComboBox.SelectedIndexChanged += new EventHandler(RecitationsComboBox_SelectedIndexChanged);
        }
    }
    private void PopulateTranslatorsCheckedListBox()
    {
        try
        {
            TranslatorsCheckedListBox.SelectedIndexChanged -= new EventHandler(TranslatorsCheckedListBox_SelectedIndexChanged);
            TranslatorsCheckedListBox.BeginUpdate();
            TranslatorsCheckedListBox.Items.Clear();

            if (Book.Instance != null)
            {
                if (Book.Instance.Verses != null)
                {
                    if (Book.Instance.Verses.Count > 0)
                    {
                        List<string> checked_keys = new List<string>();
                        foreach (string key in Book.Instance.Verses[0].Translations.Keys)
                        {
                            checked_keys.Add(key);
                        }
                        AddTranslation(checked_keys, DEFAULT_EMLAAEI);
                        AddTranslation(checked_keys, DEFAULT_TAFSEER);
                        AddTranslation(checked_keys, DEFAULT_TRANSLATION);
                        AddTranslation(checked_keys, DEFAULT_TRANSLATION_2);
                        AddTranslation(checked_keys, DEFAULT_TRANSLITERATION);

                        foreach (string key in Client.Translations.Keys)
                        {
                            string name = Client.Translations[key].Name;
                            bool is_checked = checked_keys.Contains(key);
                            TranslatorsCheckedListBox.Items.Add(name, is_checked);
                            if (
                                (key == DEFAULT_EMLAAEI) ||
                                (key == DEFAULT_TAFSEER) ||
                                (key == DEFAULT_TRANSLATION) ||
                                (key == DEFAULT_TRANSLATION_2) ||
                                (key == DEFAULT_TRANSLITERATION)
                               )
                            {
                                TranslatorsCheckedListBox.SetItemCheckState(TranslatorsCheckedListBox.Items.Count - 1, CheckState.Indeterminate);
                            }
                        }
                    }
                }
            }
        }
        finally
        {
            TranslatorsCheckedListBox.Sorted = true;
            TranslatorsCheckedListBox.EndUpdate();
            TranslatorsCheckedListBox.SelectedIndexChanged += new EventHandler(TranslatorsCheckedListBox_SelectedIndexChanged);
        }
    }
    private void PopulateTranslatorComboBox()
    {
        try
        {
            if (Book.Instance != null)
            {
                if (Book.Instance.Verses.Count > 0)
                {
                    TranslatorComboBox.SelectedIndexChanged -= new EventHandler(TranslatorComboBox_SelectedIndexChanged);
                    string backup_translation_name = "";
                    if (TranslatorComboBox.SelectedItem != null)
                    {
                        backup_translation_name = TranslatorComboBox.SelectedItem.ToString();
                    }

                    TranslatorComboBox.BeginUpdate();
                    TranslatorComboBox.Items.Clear();
                    foreach (string key in Book.Instance.Verses[0].Translations.Keys)
                    {
                        string name = Client.Translations[key].Name;
                        TranslatorComboBox.Items.Add(name);
                    }

                    if (backup_translation_name.Length > 0)
                    {
                        bool found = false;
                        for (int i = 0; i < TranslatorComboBox.Items.Count; i++)
                        {
                            if (TranslatorComboBox.Items[i].ToString() == backup_translation_name)
                            {
                                found = true;
                                break;
                            }
                        }
                        if (found)
                        {
                            this.TranslatorComboBox.SelectedItem = backup_translation_name;
                        }
                        else
                        {
                            this.TranslatorComboBox.SelectedItem = Client.Translations[DEFAULT_TRANSLATION].Name;
                        }
                    }
                    else // if all translations were cleared, we still have the 3 mandatory ones at minimum
                    {
                        if (this.TranslatorComboBox.Items.Count >= 3)
                        {
                            this.TranslatorComboBox.SelectedItem = Client.Translations[DEFAULT_TRANSLATION].Name;
                        }
                        else // if user deleted one or more of the 3 mandatory translations manually
                        {
                            if (this.TranslatorComboBox.Items.Count > 0)
                            {
                                this.TranslatorComboBox.SelectedItem = 0;
                            }
                            else // if no transaltion at all was left
                            {
                                TranslatorComboBox.SelectedIndex = -1;
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            TranslatorComboBox.SelectedIndex = -1;
        }
        finally
        {
            TranslatorComboBox.Sorted = true;
            TranslatorComboBox.EndUpdate();
            TranslatorComboBox.SelectedIndexChanged += new EventHandler(TranslatorComboBox_SelectedIndexChanged);
        }
    }
    private void RecitationsCheckedListBox_SelectedIndexChanged(object sender, EventArgs e)
    {
    }
    private void RecitationsCheckedListBox_MouseUp(object sender, MouseEventArgs e)
    {
        if (RecitationsCheckedListBox.SelectedItem != null)
        {
            string reciter = RecitationsCheckedListBox.SelectedItem.ToString();

            string reciter_folder = "";
            foreach (string key in Client.Recitations.Keys)
            {
                if (reciter == Client.Recitations[key].Reciter)
                {
                    reciter_folder = key;
                    break;
                }
            }

            if (m_downloaded_reciter_folders.Contains(reciter_folder))
            {
                RecitationsCheckedListBox.SetItemCheckState(RecitationsCheckedListBox.SelectedIndex, CheckState.Indeterminate);
            }
        }
    }
    private void TranslatorsCheckedListBox_SelectedIndexChanged(object sender, EventArgs e)
    {
    }
    private void TranslatorsCheckedListBox_MouseUp(object sender, MouseEventArgs e)
    {
        if (TranslatorsCheckedListBox.SelectedItem != null)
        {
            string name = TranslatorsCheckedListBox.SelectedItem.ToString();
            if (
                (name == Client.Translations[DEFAULT_EMLAAEI].Name) ||
                (name == Client.Translations[DEFAULT_TAFSEER].Name) ||
                (name == Client.Translations[DEFAULT_TRANSLATION].Name) ||
                (name == Client.Translations[DEFAULT_TRANSLATION_2].Name) ||
                (name == Client.Translations[DEFAULT_TRANSLITERATION].Name)
               )
            {
                TranslatorsCheckedListBox.SetItemCheckState(TranslatorsCheckedListBox.SelectedIndex, CheckState.Indeterminate);
            }
        }
    }
    private void AddTranslation(List<string> checked_keys, string title)
    {
        if (!checked_keys.Contains(title))
        {
            checked_keys.Add(title);
            Client.LoadTranslationBook(title);
        }
    }
    private void ClientSplitContainer_SplitterMoved(object sender, SplitterEventArgs e)
    {
        if ((ClientSplitContainer.Height - ClientSplitContainer.SplitterDistance) > 40)
        {
            m_translation_box_top = this.ClientSplitContainer.SplitterDistance;
        }
    }
    private void HideTranslationLabel_Click(object sender, EventArgs e)
    {
        ShowTranslationLabel.Visible = true;
        m_show_translation_box = false;
        this.ClientSplitContainer.SplitterDistance = ClientSplitContainer.Height;

        TranslatorsCheckedListBox.Visible = false;
        TranslationsCancelSettingsLabel.Visible = false;
        TranslatorComboBox.Visible = false;
        //m_show_translator_combobox = false; // don't change status to remember status for ShowTranslationLabel)

        if (File.Exists("Images/settings.png"))
        {
            TranslationsApplySettingsLabel.Image = new Bitmap("Images/settings.png");
        }
        ToolTip.SetToolTip(TranslationsApplySettingsLabel, "Add/Remove translations");
        PopulateTranslatorsCheckedListBox();
    }
    private void ShowTranslationLabel_Click(object sender, EventArgs e)
    {
        ShowTranslationLabel.Visible = false;
        m_show_translation_box = true;
        this.ClientSplitContainer.SplitterDistance = m_translation_box_top;

        TranslatorsCheckedListBox.Visible = false;
        TranslationsCancelSettingsLabel.Visible = false;
        TranslatorComboBox.Visible = m_show_translator_combobox;
    }
    private void TranslationsApplySettingsLabel_Click(object sender, EventArgs e)
    {
        if (!TranslatorComboBox.Visible)
        {
            TranslatorComboBox.Visible = true;
            m_show_translator_combobox = true;
        }
        else
        {
            TranslatorsCheckedListBox.Visible = !TranslatorsCheckedListBox.Visible;
            TranslationsCancelSettingsLabel.Visible = TranslatorsCheckedListBox.Visible;

            if (TranslatorsCheckedListBox.Visible)
            {
                if (File.Exists("Images/apply.png"))
                {
                    TranslationsApplySettingsLabel.Image = new Bitmap("Images/apply.png");
                }
                ToolTip.SetToolTip(TranslationsApplySettingsLabel, "Download translations");
            }
            else
            {
                if (File.Exists("Images/settings.png"))
                {
                    TranslationsApplySettingsLabel.Image = new Bitmap("Images/settings.png");
                }
                ToolTip.SetToolTip(TranslationsApplySettingsLabel, "Add/Remove translations");

                int index_of_first_new_translation = DownloadTranslations();
                if ((index_of_first_new_translation >= 0) && (index_of_first_new_translation < TranslatorComboBox.Items.Count))
                {
                    TranslatorComboBox.SelectedIndex = index_of_first_new_translation;
                }
            }
        }
    }
    private void TranslationsCancelSettingsLabel_Click(object sender, EventArgs e)
    {
        TranslatorsCheckedListBox.Visible = false;
        TranslationsCancelSettingsLabel.Visible = TranslatorsCheckedListBox.Visible;
        TranslatorComboBox.Visible = false;
        m_show_translator_combobox = false;

        if (File.Exists("Images/settings.png"))
        {
            TranslationsApplySettingsLabel.Image = new Bitmap("Images/settings.png");
        }
        ToolTip.SetToolTip(TranslationsApplySettingsLabel, "Add/Remove translations");
        PopulateTranslatorsCheckedListBox();
    }
    /// <summary>
    /// Download all new requested translations
    /// </summary>
    /// <returns>index of first new transaltion</returns>
    private int DownloadTranslations()
    {
        int index_of_first_new_transaltion = -1;

        this.Cursor = Cursors.WaitCursor;
        try
        {
            List<string> checked_keys = new List<string>();
            foreach (string key in Client.Translations.Keys)
            {
                if (
                    (key == DEFAULT_EMLAAEI) ||
                    (key == DEFAULT_TAFSEER) ||
                    (key == DEFAULT_TRANSLATION) ||
                    (key == DEFAULT_TRANSLATION_2) ||
                    (key == DEFAULT_TRANSLITERATION)
                   )
                {
                    checked_keys.Add(key);
                }
                else
                {
                    foreach (int index in TranslatorsCheckedListBox.CheckedIndices)
                    {
                        if (Client.Translations[key].Name == TranslatorsCheckedListBox.Items[index].ToString())
                        {
                            checked_keys.Add(key);
                            break;
                        }
                    }
                }
            }

            string translations_folder = "Translations";
            string offline_folder = translations_folder + "/" + "Offline";
            ProgressBar.Minimum = 0;
            ProgressBar.Maximum = checked_keys.Count;
            ProgressBar.Value = 0;
            ProgressBar.Refresh();
            foreach (string key in Client.Translations.Keys)
            {
                if (checked_keys.Contains(key))
                {
                    ProgressBar.Value++;
                    ProgressBar.Refresh();

                    string translations_path = translations_folder + "/" + key + ".txt";
                    string offline_path = offline_folder + "/" + key + ".txt";

                    // delete file in translations_path if invalid
                    if (File.Exists(translations_path))
                    {
                        long filesize = (new FileInfo(translations_path)).Length;
                        if (filesize < 1024) // < 1kb invalid file
                        {
                            File.Delete(translations_path);
                        }
                    }

                    // delete file in offline_path if invalid
                    if (File.Exists(offline_path))
                    {
                        long filesize = (new FileInfo(offline_path)).Length;
                        if (filesize < 1024) // < 1kb invalid file
                        {
                            File.Delete(offline_path);
                        }
                    }

                    if (!File.Exists(translations_path))
                    {
                        // download file to offline_path
                        if (!File.Exists(offline_path))
                        {
                            DownloadFile(Translation.UrlPrefix + Client.Translations[key].Url, offline_path);

                            if (index_of_first_new_transaltion == -1)
                            {
                                int index_of_new_transaltion = -1;
                                foreach (int index in TranslatorsCheckedListBox.CheckedIndices)
                                {
                                    index_of_new_transaltion++;
                                    if (Client.Translations[key].Name == TranslatorsCheckedListBox.Items[index].ToString())
                                    {
                                        index_of_first_new_transaltion = index_of_new_transaltion;
                                        break;
                                    }
                                }
                            }
                        }

                        // copy to translations_path
                        if (File.Exists(offline_path))
                        {
                            long filesize = (new FileInfo(offline_path)).Length;
                            if (filesize < 1024) // < 1kb invalid file
                            {
                                File.Delete(offline_path);
                                Client.UnloadTranslationBook(key);
                            }
                            else // copy valid file
                            {
                                File.Copy(offline_path, translations_path);
                                Client.LoadTranslationBook(key);
                            }
                        }
                    }
                }
                else // unload translation
                {
                    if (File.Exists(translations_folder + "/" + key + ".txt"))
                    {
                        Client.UnloadTranslationBook(key);
                        File.Delete(translations_folder + "/" + key + ".txt");
                    }
                }

                Application.DoEvents();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
        }
        finally
        {
            PopulateTranslatorsCheckedListBox();
            PopulateTranslatorComboBox();

            if (m_client != null)
            {
                if (m_client.Selection != null)
                {
                    List<Verse> verses = m_client.Selection.Verses;
                    if (verses.Count > 0)
                    {
                        ProgressBar.Minimum = Verse.MIN_NUMBER;
                        ProgressBar.Maximum = Verse.MAX_NUMBER;
                        ProgressBar.Value = verses[0].Number;
                        ProgressBar.Refresh();
                    }
                }
            }

            this.Cursor = Cursors.Default;
        }

        return index_of_first_new_transaltion;
    }
    private void TranslatorComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (m_client != null)
        {
            if (m_client.Selection != null)
            {
                if (m_is_selection_mode)
                {
                    if (m_found_verses_displayed)
                    {
                        DisplayTranslation(m_client.FoundVerses);
                    }
                    else
                    {
                        List<Verse> verses = m_client.Selection.Verses;
                        DisplayTranslation(verses);
                    }
                }
                else // highlighted text or current line
                {
                    int line_index = -1;
                    if (m_found_verses_displayed)
                    {
                        line_index = m_found_verses_current_line_index;
                    }
                    else
                    {
                        line_index = m_current_line_index;
                    }

                    if (m_found_verses_displayed)
                    {
                        if (m_client.FoundVerses != null)
                        {
                            if ((line_index >= 0) && (line_index < m_client.FoundVerses.Count))
                            {
                                DisplayTranslation(m_client.FoundVerses[line_index]);
                            }
                        }
                    }
                    else
                    {
                        List<Verse> verses = m_client.Selection.Verses;
                        if ((line_index >= 0) && (line_index < verses.Count))
                        {
                            DisplayTranslation(verses[line_index]);
                        }
                    }
                }
            }
        }
    }
    private void DisplayTranslation(List<Verse> verses)
    {
        if (verses != null)
        {
            if (TranslatorComboBox.SelectedItem != null)
            {
                string name = TranslatorComboBox.SelectedItem.ToString();
                string translation = Client.GetTranslationKey(name);
                DisplayTranslation(verses, translation);
            }
        }
    }
    private void DisplayTranslation(Verse verse)
    {
        if (verse != null)
        {
            if (TranslatorComboBox.SelectedItem != null)
            {
                string name = TranslatorComboBox.SelectedItem.ToString();
                string translation = Client.GetTranslationKey(name);
                DisplayTranslation(verse, translation);
            }
        }
    }
    private void DisplayTranslation(Verse verse, string translation)
    {
        if (verse != null)
        {
            if (verse.Translations.ContainsKey(translation))
            {
                TranslationTextBox.WordWrap = true;
                TranslationTextBox.Text = (verse.PaddedAddress + " " + verse.Translations[translation]);
                TranslationTextBox.Refresh();
            }
        }
    }
    private void DisplayTranslation(List<Verse> verses, string translation)
    {
        if (verses != null)
        {
            if (verses.Count > 0)
            {
                if (verses[0].Translations.ContainsKey(translation))
                {
                    StringBuilder str = new StringBuilder();
                    foreach (Verse verse in verses)
                    {
                        str.AppendLine(verse.PaddedAddress + " " + verse.Translations[translation]);
                    }

                    TranslationTextBox.WordWrap = (verses.Count == 1);
                    TranslationTextBox.Text = str.ToString();
                    TranslationTextBox.Refresh();
                }
            }
        }
    }

    private void TextModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        try
        {
            if (m_client != null)
            {
                if (TextModeComboBox.SelectedItem != null)
                {
                    int line_index = -1;
                    if (m_found_verses_displayed)
                    {
                        line_index = m_found_verses_current_line_index;
                    }
                    else
                    {
                        line_index = m_current_line_index;
                    }

                    if (TextModeComboBox.SelectedItem != null)
                    {
                        m_client.TextMode = TextModeComboBox.SelectedItem.ToString();
                    }

                    if (Book.Instance != null)
                    {
                        FindByNumbersUniqueLettersNumericUpDown.Minimum = 0;
                        if (m_client.TextMode.StartsWith("Simplified"))
                        {
                            string digits = "";
                            foreach (char character in m_client.TextMode)
                            {
                                if (Char.IsDigit(character))
                                {
                                    digits += character.ToString();
                                }
                            }

                            try
                            {
                                FindByNumbersUniqueLettersNumericUpDown.Maximum = int.Parse(digits);
                            }
                            catch
                            {
                                FindByNumbersUniqueLettersNumericUpDown.Maximum = 0; // Unsupported
                            }
                        }
                        else if (m_client.TextMode.StartsWith("Image"))
                        {
                            FindByNumbersUniqueLettersNumericUpDown.Maximum = 29; // Simplified29
                        }
                        else if (m_client.TextMode.StartsWith("Original"))
                        {
                            FindByNumbersUniqueLettersNumericUpDown.Maximum = 69; // Original
                        }
                        else
                        {
                            FindByNumbersUniqueLettersNumericUpDown.Maximum = 0; // Unsupported
                        }

                        WordNumericUpDown.Minimum = Book.Instance.MinWords;
                        WordNumericUpDown.Maximum = Book.Instance.MaxWords;
                        LetterNumericUpDown.Minimum = Book.Instance.MinLetters;
                        LetterNumericUpDown.Maximum = Book.Instance.MaxLetters;

                        DrawLetterValuesButton.Enabled = true;
                        DrawWordAllahButton.Enabled = ((m_client.TextMode != null) && (m_client.TextMode.StartsWith("Simplified")));
                        DrawWordsWithAllahButton.Enabled = DrawWordAllahButton.Enabled;
                        DrawWordsWithAllahHelpButton.Enabled = DrawWordAllahButton.Enabled;

                        // update ComboBoxes and CheckBoxes with numerology system settings
                        RefreshLetterOrderSystemComboBoxItems();
                        // will fire LetterOrderSystemComboBox_SelectedIndexChanged event
                        // and call RefreshLetterValueSystemComboBoxItems()

                        // display text and recalcuate its value and statistics
                        if (DrawingPictureBoxEx.Visible)
                        {
                            DisplaySelection(sender, false);
                            RedrawCurrentGraph();
                        }
                        else
                        {
                            if (m_found_verses_displayed)
                            {
                                List<Verse> current_text_mode_verses = new List<Verse>();
                                foreach (Verse verse in m_client.FoundVerses)
                                {
                                    current_text_mode_verses.Add(Book.Instance.Verses[verse.Number - 1]);
                                }
                                m_client.FoundVerses = current_text_mode_verses;

                                DisplayFoundVerses(true, true);
                            }
                            else
                            {
                                DisplaySelection(sender, false);
                            }
                        }

                        if (m_found_verses_displayed)
                        {
                            m_found_verses_current_line_index = line_index;
                        }
                        else
                        {
                            m_current_line_index = line_index;
                        }
                        GotoLine(line_index);
                    }

                    UpdateKeyboard(m_client.TextMode);
                }
            }

            // simulate mouse click inside MainTextBox to continue playing next verse and not restart from 1
            m_is_selection_mode = false;
            MainTextBox.Focus();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
        }
    }
    private void RefreshLetterOrderSystemComboBoxItems()
    {
        if (m_client != null)
        {
            LetterOrderSystemComboBox.Items.Clear();
            if (Client.NumerologySystems != null)
            {
                foreach (string key in Client.NumerologySystems.Keys)
                {
                    string[] parts = key.Split('_');
                    if (parts.Length == 3)
                    {
                        if (parts[0] == m_client.TextMode)
                        {
                            // don't allow duplicates
                            if (!LetterOrderSystemComboBox.Items.Contains(parts[1]))
                            {
                                LetterOrderSystemComboBox.Items.Add(parts[1]);
                            }
                        }
                    }
                }
            }

            if (LetterOrderSystemComboBox.Items.Count > 0)
            {
                if (LetterOrderSystemComboBox.Items.Contains(m_client.LetterOrderSystem))
                {
                    LetterOrderSystemComboBox.SelectedItem = m_client.LetterOrderSystem;
                }
                else
                {
                    LetterOrderSystemComboBox.SelectedIndex = 0;
                }
            }
        }
    }
    private void LetterOrderSystemComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (m_client != null)
        {
            if (LetterOrderSystemComboBox.SelectedItem != null)
            {
                m_client.LetterOrderSystem = LetterOrderSystemComboBox.SelectedItem.ToString();
                RefreshLetterValueSystemComboBoxItems();
            }
        }
    }
    private void RefreshLetterValueSystemComboBoxItems()
    {
        if (m_client != null)
        {
            LetterValueSystemComboBox.Items.Clear();
            foreach (string key in Client.NumerologySystems.Keys)
            {
                string[] parts = key.Split('_');
                if (parts.Length == 3)
                {
                    if (parts[0] == m_client.TextMode)
                    {
                        if (LetterOrderSystemComboBox.SelectedItem != null)
                        {
                            if (LetterOrderSystemComboBox.SelectedItem.ToString() == parts[1])
                            {
                                // don't allow duplicates
                                if (!LetterValueSystemComboBox.Items.Contains(parts[2]))
                                {
                                    LetterValueSystemComboBox.Items.Add(parts[2]);
                                }
                            }
                        }
                    }
                }
            }

            if (LetterOrderSystemComboBox.Items.Count > 0)
            {
                if (LetterValueSystemComboBox.Items.Contains(m_client.LetterValueSystem))
                {
                    LetterValueSystemComboBox.SelectedItem = m_client.LetterValueSystem;
                }
                else
                {
                    LetterValueSystemComboBox.SelectedIndex = 0;
                }
            }
        }
    }
    private void LetterValueSystemComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (m_client != null)
        {
            if (LetterValueSystemComboBox.SelectedItem != null)
            {
                m_client.LetterValueSystem = LetterValueSystemComboBox.SelectedItem.ToString();

                // re-calculate numerology value
                CalculateCurrentText();
                UpdateNumerologySystems();
                CalculateCurrentValue();
            }
        }
    }
    private void LetterOrderSystemComboBox_DropDown(object sender, EventArgs e)
    {
        LetterOrderSystemComboBox.DropDownHeight = StatisticsGroupBox.Height - LetterOrderSystemComboBox.Top - LetterOrderSystemComboBox.Height;
    }
    private void LetterValueSystemComboBox_DropDown(object sender, EventArgs e)
    {
        LetterValueSystemComboBox.DropDownHeight = StatisticsGroupBox.Height - LetterValueSystemComboBox.Top - LetterValueSystemComboBox.Height;
    }

    private void UpdateNumerologySystemName()
    {
        if (
              (TextModeComboBox.SelectedItem != null) &&
              (LetterOrderSystemComboBox.SelectedItem != null) &&
              (LetterValueSystemComboBox.SelectedItem != null)
           )
        {
            if (m_client != null)
            {
                m_client.NumerologySystemName =
                     TextModeComboBox.SelectedItem.ToString() + "_" +
                     LetterOrderSystemComboBox.SelectedItem.ToString() + "_" +
                     LetterValueSystemComboBox.SelectedItem.ToString();
            }
        }
    }
    private void UpdateNumerologySystems()
    {
        if (m_client != null)
        {
            if (m_client.IsDynamic)
            {
                if (m_client.IsHighlightedText)
                {
                    // CurrentText-wide numerology system
                    Client.UpdateNumerologySystems(m_client.TextMode, m_current_text, true);
                }
                else
                {
                    if (m_client.Selection != null)
                    {
                        // Selection-wide numerology system
                        Client.UpdateNumerologySystems(m_client.TextMode, m_client.Selection.GetText(m_client.TextMode), true);
                    }
                }
            }
            else
            {
                // Book-wide numerology system
                if (Book.Instance != null)
                {
                    Client.UpdateNumerologySystems(m_client.TextMode, Book.Instance.GetText(m_client.TextMode), false);
                }
            }

            RefreshDynamicLetterScopeCheckBoxes();

            // refresh ObjectListView
            if (LetterValuesObjectListView != null)
            {
                LetterValuesObjectListView.ClearObjects();
                LetterValuesObjectListView.SetObjects(m_client.NumerologySystem);
            }
        }
    }
    private void RefreshDynamicLetterScopeCheckBoxes()
    {
        if (m_client != null)
        {
            try
            {
                DynamicLetterScopeCheckBox.CheckedChanged -= new EventHandler(DynamicLetterScopeCheckBox_CheckedChanged);
                DynamicLetterScopeSelectionRadioButton.CheckedChanged -= new EventHandler(DynamicLetterScopeSelectionRadioButton_CheckedChanged);
                DynamicLetterScopeHighlightedTextRadioButton.CheckedChanged -= new EventHandler(DynamicLetterScopeHighlightedTextRadioButton_CheckedChanged);

                DynamicLetterScopeCheckBox.Checked = m_client.IsDynamic;
                DynamicLetterScopeSelectionRadioButton.Checked = !m_client.IsHighlightedText;
                DynamicLetterScopeHighlightedTextRadioButton.Checked = m_client.IsHighlightedText;
            }
            finally
            {
                DynamicLetterScopeCheckBox.CheckedChanged += new EventHandler(DynamicLetterScopeCheckBox_CheckedChanged);
                DynamicLetterScopeSelectionRadioButton.CheckedChanged += new EventHandler(DynamicLetterScopeSelectionRadioButton_CheckedChanged);
                DynamicLetterScopeHighlightedTextRadioButton.CheckedChanged += new EventHandler(DynamicLetterScopeHighlightedTextRadioButton_CheckedChanged);
            }
        }
    }

    private void DynamicLetterScopeCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        if (m_client != null)
        {
            DynamicLetterScopeSelectionRadioButton.Enabled = DynamicLetterScopeCheckBox.Checked;
            DynamicLetterScopeHighlightedTextRadioButton.Enabled = DynamicLetterScopeCheckBox.Checked;

            // update client's current numerology system
            m_client.IsDynamic = DynamicLetterScopeCheckBox.Checked;
            m_client.IsHighlightedText = DynamicLetterScopeHighlightedTextRadioButton.Checked;

            // re-calculate numerology value
            CalculateCurrentText();
            UpdateNumerologySystems();
            CalculateCurrentValue();
        }
    }
    private void DynamicLetterScopeSelectionRadioButton_CheckedChanged(object sender, EventArgs e)
    {
        if (m_client != null)
        {
            // update client's current numerology system
            m_client.IsHighlightedText = !DynamicLetterScopeSelectionRadioButton.Checked;

            // re-calculate numerology value
            CalculateCurrentText();
            UpdateNumerologySystems();
            CalculateCurrentValue();
        }
    }
    private void DynamicLetterScopeHighlightedTextRadioButton_CheckedChanged(object sender, EventArgs e)
    {
        if (m_client != null)
        {
            // update client's current numerology system
            m_client.IsHighlightedText = DynamicLetterScopeHighlightedTextRadioButton.Checked;

            // re-calculate numerology value
            CalculateCurrentText();
            UpdateNumerologySystems();
            CalculateCurrentValue();
        }
    }

    private void AddToLetterLNumberCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        AddToLetterLNumberCheckBox_EnabledChanged(sender, e);
    }
    private void AddToLetterWNumberCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        AddToLetterWNumberCheckBox_EnabledChanged(sender, e);
    }
    private void AddToLetterVNumberCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        AddToLetterVNumberCheckBox_EnabledChanged(sender, e);
    }
    private void AddToLetterCNumberCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        AddToLetterCNumberCheckBox_EnabledChanged(sender, e);
    }
    private void AddToLetterLDistanceCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        AddToLetterLDistanceCheckBox_EnabledChanged(sender, e);
    }
    private void AddToLetterWDistanceCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        AddToLetterWDistanceCheckBox_EnabledChanged(sender, e);
    }
    private void AddToLetterVDistanceCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        AddToLetterVDistanceCheckBox_EnabledChanged(sender, e);
    }
    private void AddToLetterCDistanceCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        AddToLetterCDistanceCheckBox_EnabledChanged(sender, e);
    }
    private void AddToWordWNumberCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        AddToWordWNumberCheckBox_EnabledChanged(sender, e);
    }
    private void AddToWordVNumberCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        AddToWordVNumberCheckBox_EnabledChanged(sender, e);
    }
    private void AddToWordCNumberCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        AddToWordCNumberCheckBox_EnabledChanged(sender, e);
    }
    private void AddToWordWDistanceCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        AddToWordWDistanceCheckBox_EnabledChanged(sender, e);
    }
    private void AddToWordVDistanceCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        AddToWordVDistanceCheckBox_EnabledChanged(sender, e);
    }
    private void AddToWordCDistanceCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        AddToWordCDistanceCheckBox_EnabledChanged(sender, e);
    }
    private void AddToVerseVNumberCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        AddToVerseVNumberCheckBox_EnabledChanged(sender, e);
    }
    private void AddToVerseCNumberCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        AddToVerseCNumberCheckBox_EnabledChanged(sender, e);
    }
    private void AddToVerseVDistanceCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        AddToVerseVDistanceCheckBox_EnabledChanged(sender, e);
    }
    private void AddToVerseCDistanceCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        AddToVerseCDistanceCheckBox_EnabledChanged(sender, e);
    }
    private void AddToChapterCNumberCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        AddToChapterCNumberCheckBox_EnabledChanged(sender, e);
    }
    private void AddToLetterLNumberCheckBox_EnabledChanged(object sender, EventArgs e)
    {
        if (m_client != null)
        {
            // update client's current numerology system
            m_client.AddToLetterLNumber = AddToLetterLNumberCheckBox.Enabled && AddToLetterLNumberCheckBox.Checked;
            RecalculateCurrentValue();
        }
    }
    private void AddToLetterWNumberCheckBox_EnabledChanged(object sender, EventArgs e)
    {
        if (m_client != null)
        {
            // update client's current numerology system
            m_client.AddToLetterWNumber = AddToLetterWNumberCheckBox.Enabled && AddToLetterWNumberCheckBox.Checked;
            RecalculateCurrentValue();
        }
    }
    private void AddToLetterVNumberCheckBox_EnabledChanged(object sender, EventArgs e)
    {
        if (m_client != null)
        {
            // update client's current numerology system
            m_client.AddToLetterVNumber = AddToLetterVNumberCheckBox.Enabled && AddToLetterVNumberCheckBox.Checked;
            RecalculateCurrentValue();
        }
    }
    private void AddToLetterCNumberCheckBox_EnabledChanged(object sender, EventArgs e)
    {
        if (m_client != null)
        {
            // update client's current numerology system
            m_client.AddToLetterCNumber = AddToLetterCNumberCheckBox.Enabled && AddToLetterCNumberCheckBox.Checked;
            RecalculateCurrentValue();
        }
    }
    private void AddToLetterLDistanceCheckBox_EnabledChanged(object sender, EventArgs e)
    {
        if (m_client != null)
        {
            // update client's current numerology system
            m_client.AddToLetterLDistance = AddToLetterLDistanceCheckBox.Enabled && AddToLetterLDistanceCheckBox.Checked;
            RecalculateCurrentValue();
        }
    }
    private void AddToLetterWDistanceCheckBox_EnabledChanged(object sender, EventArgs e)
    {
        if (m_client != null)
        {
            // update client's current numerology system
            m_client.AddToLetterWDistance = AddToLetterWDistanceCheckBox.Enabled && AddToLetterWDistanceCheckBox.Checked;
            RecalculateCurrentValue();
        }
    }
    private void AddToLetterVDistanceCheckBox_EnabledChanged(object sender, EventArgs e)
    {
        if (m_client != null)
        {
            // update client's current numerology system
            m_client.AddToLetterVDistance = AddToLetterVDistanceCheckBox.Enabled && AddToLetterVDistanceCheckBox.Checked;
            RecalculateCurrentValue();
        }
    }
    private void AddToLetterCDistanceCheckBox_EnabledChanged(object sender, EventArgs e)
    {
        if (m_client != null)
        {
            // update client's current numerology system
            m_client.AddToLetterCDistance = AddToLetterCDistanceCheckBox.Enabled && AddToLetterCDistanceCheckBox.Checked;
            RecalculateCurrentValue();
        }
    }
    private void AddToWordWNumberCheckBox_EnabledChanged(object sender, EventArgs e)
    {
        if (m_client != null)
        {
            // update client's current numerology system
            m_client.AddToWordWNumber = AddToWordWNumberCheckBox.Enabled && AddToWordWNumberCheckBox.Checked;
            RecalculateCurrentValue();
        }
    }
    private void AddToWordVNumberCheckBox_EnabledChanged(object sender, EventArgs e)
    {
        if (m_client != null)
        {
            // update client's current numerology system
            m_client.AddToWordVNumber = AddToWordVNumberCheckBox.Enabled && AddToWordVNumberCheckBox.Checked;
            RecalculateCurrentValue();
        }
    }
    private void AddToWordCNumberCheckBox_EnabledChanged(object sender, EventArgs e)
    {
        if (m_client != null)
        {
            // update client's current numerology system
            m_client.AddToWordCNumber = AddToWordCNumberCheckBox.Enabled && AddToWordCNumberCheckBox.Checked;
            RecalculateCurrentValue();
        }
    }
    private void AddToWordWDistanceCheckBox_EnabledChanged(object sender, EventArgs e)
    {
        if (m_client != null)
        {
            // update client's current numerology system
            m_client.AddToWordWDistance = AddToWordWDistanceCheckBox.Enabled && AddToWordWDistanceCheckBox.Checked;
            RecalculateCurrentValue();
        }
    }
    private void AddToWordVDistanceCheckBox_EnabledChanged(object sender, EventArgs e)
    {
        if (m_client != null)
        {
            // update client's current numerology system
            m_client.AddToWordVDistance = AddToWordVDistanceCheckBox.Enabled && AddToWordVDistanceCheckBox.Checked;
            RecalculateCurrentValue();
        }
    }
    private void AddToWordCDistanceCheckBox_EnabledChanged(object sender, EventArgs e)
    {
        if (m_client != null)
        {
            // update client's current numerology system
            m_client.AddToWordCDistance = AddToWordCDistanceCheckBox.Enabled && AddToWordCDistanceCheckBox.Checked;
            RecalculateCurrentValue();
        }
    }
    private void AddToVerseVNumberCheckBox_EnabledChanged(object sender, EventArgs e)
    {
        if (m_client != null)
        {
            // update client's current numerology system
            m_client.AddToVerseVNumber = AddToVerseVNumberCheckBox.Enabled && AddToVerseVNumberCheckBox.Checked;
            RecalculateCurrentValue();
        }
    }
    private void AddToVerseCNumberCheckBox_EnabledChanged(object sender, EventArgs e)
    {
        if (m_client != null)
        {
            // update client's current numerology system
            m_client.AddToVerseCNumber = AddToVerseCNumberCheckBox.Enabled && AddToVerseCNumberCheckBox.Checked;
            RecalculateCurrentValue();
        }
    }
    private void AddToVerseVDistanceCheckBox_EnabledChanged(object sender, EventArgs e)
    {
        if (m_client != null)
        {
            // update client's current numerology system
            m_client.AddToVerseVDistance = AddToVerseVDistanceCheckBox.Enabled && AddToVerseVDistanceCheckBox.Checked;
            RecalculateCurrentValue();
        }
    }
    private void AddToVerseCDistanceCheckBox_EnabledChanged(object sender, EventArgs e)
    {
        if (m_client != null)
        {
            // update client's current numerology system
            m_client.AddToVerseCDistance = AddToVerseCDistanceCheckBox.Enabled && AddToVerseCDistanceCheckBox.Checked;
            RecalculateCurrentValue();
        }
    }
    private void AddToChapterCNumberCheckBox_EnabledChanged(object sender, EventArgs e)
    {
        if (m_client != null)
        {
            // update client's current numerology system
            m_client.AddToChapterCNumber = AddToChapterCNumberCheckBox.Enabled && AddToChapterCNumberCheckBox.Checked;
            RecalculateCurrentValue();
        }
    }
    private void RecalculateCurrentValue()
    {
        // re-calculate numerology value without CalculateCurrentText as neither text nor selection has changed
        UpdateNumerologySystems();
        CalculateCurrentValue();
    }

    private void ChapterComboBox_KeyDown(object sender, KeyEventArgs e)
    {
        bool SeparatorKeys = (
            ((e.KeyCode == Keys.Subtract) && (e.Modifiers != Keys.Shift))           // HYPHEN
            || ((e.KeyCode == Keys.OemMinus) && (e.Modifiers != Keys.Shift))        // HYPHEN
            || ((e.KeyCode == Keys.Oemcomma) && (e.Modifiers != Keys.Shift))        // COMMA
            || ((e.KeyCode == Keys.OemSemicolon) && (e.Modifiers == Keys.Shift))    // COLON
            );

        bool NumericKeys = (
            ((e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9) || (e.KeyCode >= Keys.NumPad0 && e.KeyCode <= Keys.NumPad9))
            && e.Modifiers != Keys.Shift);

        bool EditKeys = (
            (e.KeyCode == Keys.A && e.Modifiers == Keys.Control) ||
            (e.KeyCode == Keys.Z && e.Modifiers == Keys.Control) ||
            (e.KeyCode == Keys.X && e.Modifiers == Keys.Control) ||
            (e.KeyCode == Keys.C && e.Modifiers == Keys.Control) ||
            (e.KeyCode == Keys.V && e.Modifiers == Keys.Control) ||
            e.KeyCode == Keys.Delete ||
            e.KeyCode == Keys.Back);

        bool NavigationKeys = (
            e.KeyCode == Keys.Up ||
            e.KeyCode == Keys.Right ||
            e.KeyCode == Keys.Down ||
            e.KeyCode == Keys.Left ||
            e.KeyCode == Keys.Home ||
            e.KeyCode == Keys.End);

        bool ExecuteKeys = (e.KeyCode == Keys.Enter);

        if (ExecuteKeys)
        {
            if (m_client != null)
            {
                try
                {
                    string text = ChapterComboBox.Text;
                    if (text.Length > 0)
                    {
                        // 1, 3-4, 5:55, 3-4:19, 6:19-23, 24:35-27:62
                        SelectionScope scope = SelectionScope.Verse;
                        List<int> indexes = new List<int>();

                        foreach (string part in text.Split(','))
                        {
                            string[] range_parts = part.Split('-');
                            if (range_parts.Length == 1) // 1 | 5:55
                            {
                                string[] sub_range_parts = part.Split(':');
                                if (sub_range_parts.Length == 1) // 1
                                {
                                    int chapter_number;
                                    if (int.TryParse(sub_range_parts[0], out chapter_number))
                                    {
                                        Chapter chapter = null;
                                        foreach (Chapter book_chapter in Book.Instance.Chapters)
                                        {
                                            if (book_chapter.Number == chapter_number)
                                            {
                                                chapter = book_chapter;
                                            }
                                        }
                                        foreach (Verse verse in chapter.Verses)
                                        {
                                            indexes.Add(verse.Number - 1);
                                        }
                                    }
                                }
                                else if (sub_range_parts.Length == 2) // 5:55
                                {
                                    int chapter_number;
                                    if (int.TryParse(sub_range_parts[0], out chapter_number)) // 5:55
                                    {
                                        int verse_number_in_chapter;
                                        if (int.TryParse(sub_range_parts[1], out verse_number_in_chapter))
                                        {
                                            Chapter chapter = null;
                                            foreach (Chapter book_chapter in Book.Instance.Chapters)
                                            {
                                                if (book_chapter.Number == chapter_number)
                                                {
                                                    chapter = book_chapter;
                                                }
                                            }
                                            int from_verse_index = chapter.Verses[verse_number_in_chapter - 1].Number - 1;
                                            indexes.Add(from_verse_index);
                                        }
                                    }
                                }
                            }
                            else if (range_parts.Length == 2) // 3-4, 3-4:19, 6:19-23, 24:35-27:62
                            {
                                int from_chapter_number;
                                int to_chapter_number;
                                if (int.TryParse(range_parts[0], out from_chapter_number)) // 3-4
                                {
                                    if (int.TryParse(range_parts[1], out to_chapter_number))
                                    {
                                        if (from_chapter_number <= to_chapter_number)
                                        {
                                            for (int number = from_chapter_number; number <= to_chapter_number; number++)
                                            {
                                                Chapter chapter = null;
                                                foreach (Chapter book_chapter in Book.Instance.Chapters)
                                                {
                                                    if (book_chapter.Number == number)
                                                    {
                                                        chapter = book_chapter;
                                                    }
                                                }
                                                foreach (Verse verse in chapter.Verses)
                                                {
                                                    indexes.Add(verse.Number - 1);
                                                }
                                            }
                                        }
                                    }
                                    else // 3-4:19
                                    {
                                        // range_parts[0] == 3
                                        // range_parts[1] == 4:19
                                        string[] to_range_parts = range_parts[1].Split(':'); // 4:19
                                        if (to_range_parts.Length == 2)
                                        {
                                            int from_verse_number_in_chapter = 1; // not specified so start from beginning of chapter

                                            if (int.TryParse(to_range_parts[0], out to_chapter_number))  // 4
                                            {
                                                int to_verse_number_in_chapter;
                                                if (int.TryParse(to_range_parts[1], out to_verse_number_in_chapter)) // 19
                                                {
                                                    Chapter from_chapter = null;
                                                    foreach (Chapter book_chapter in Book.Instance.Chapters)
                                                    {
                                                        if (book_chapter.Number == from_chapter_number)
                                                        {
                                                            from_chapter = book_chapter;
                                                        }
                                                    }
                                                    int from_verse_index = from_chapter.Verses[from_verse_number_in_chapter - 1].Number - 1;

                                                    Chapter to_chapter = null;
                                                    foreach (Chapter book_chapter in Book.Instance.Chapters)
                                                    {
                                                        if (book_chapter.Number == to_chapter_number)
                                                        {
                                                            to_chapter = book_chapter;
                                                        }
                                                    }
                                                    int to_verse_index = to_chapter.Verses[to_verse_number_in_chapter - 1].Number - 1;
                                                    for (int i = from_verse_index; i <= to_verse_index; i++)
                                                    {
                                                        indexes.Add(i);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else // "range_parts[0]" contains a colon ':'  // "6:19"-23, "24:35"-27:62
                                {
                                    //int from_chapter_number;
                                    //int to_chapter_number;
                                    string[] from_parts = range_parts[0].Split(':');
                                    if (from_parts.Length == 2)
                                    {
                                        int from_verse_number_in_chapter;
                                        if (int.TryParse(from_parts[0], out from_chapter_number))
                                        {
                                            if (int.TryParse(from_parts[1], out from_verse_number_in_chapter))
                                            {
                                                string[] to_parts = range_parts[1].Split(':'); // "range_parts[1]" may or may not contain a colon ':'  // 6:19-"23", 24:35-"27:62"
                                                if (to_parts.Length == 1) // 6:19-"23"
                                                {
                                                    int to_verse_number_in_chapter;
                                                    if (int.TryParse(to_parts[0], out to_verse_number_in_chapter))
                                                    {
                                                        if (from_verse_number_in_chapter <= to_verse_number_in_chapter)  // XX:19-23
                                                        {
                                                            Chapter from_chapter = null;
                                                            foreach (Chapter book_chapter in Book.Instance.Chapters)
                                                            {
                                                                if (book_chapter.Number == from_chapter_number)
                                                                {
                                                                    from_chapter = book_chapter;
                                                                }
                                                            }
                                                            int from_verse_index = from_chapter.Verses[from_verse_number_in_chapter - 1].Number - 1;
                                                            int to_verse_index = from_chapter.Verses[to_verse_number_in_chapter - 1].Number - 1;
                                                            for (int i = from_verse_index; i <= to_verse_index; i++)
                                                            {
                                                                indexes.Add(i);
                                                            }
                                                        }
                                                    }
                                                }
                                                else if (to_parts.Length == 2) // 24:35-"27:62"
                                                {
                                                    int to_verse_number_in_chapter;
                                                    if (int.TryParse(to_parts[0], out to_chapter_number))
                                                    {
                                                        if (int.TryParse(to_parts[1], out to_verse_number_in_chapter))
                                                        {
                                                            if (from_chapter_number <= to_chapter_number)  // 24:XX-27:XX // only worry about chapters
                                                            {
                                                                Chapter from_chapter = null;
                                                                foreach (Chapter book_chapter in Book.Instance.Chapters)
                                                                {
                                                                    if (book_chapter.Number == from_chapter_number)
                                                                    {
                                                                        from_chapter = book_chapter;
                                                                    }
                                                                }
                                                                int from_verse_index = from_chapter.Verses[from_verse_number_in_chapter - 1].Number - 1;
                                                                Chapter to_chapter = null;
                                                                foreach (Chapter book_chapter in Book.Instance.Chapters)
                                                                {
                                                                    if (book_chapter.Number == to_chapter_number)
                                                                    {
                                                                        to_chapter = book_chapter;
                                                                    }
                                                                }
                                                                int to_verse_index = to_chapter.Verses[to_verse_number_in_chapter - 1].Number - 1;
                                                                for (int i = from_verse_index; i <= to_verse_index; i++)
                                                                {
                                                                    indexes.Add(i);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        m_client.Selection = new Selection(Book.Instance, scope, indexes);
                        DisplaySelection(sender, true);
                    }
                }
                catch
                {
                    // log exception
                }
            }
        }

        // reject all other keys
        if (!(SeparatorKeys || NumericKeys || EditKeys || NavigationKeys))
        {
            e.SuppressKeyPress = true;
            e.Handled = true;
        }
    }
    private void ChapterComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (m_client != null)
        {
            int chapter_index = Book.Instance.Chapters[ChapterComboBox.SelectedIndex].Number - 1;

            if (
                 ChapterComboBox.Focused ||
                 ChapterVerseNumericUpDown.Focused ||
                 ChapterWordNumericUpDown.Focused ||
                 ChapterLetterNumericUpDown.Focused ||
                 PageNumericUpDown.Focused ||
                 StationNumericUpDown.Focused ||
                 PartNumericUpDown.Focused ||
                 GroupNumericUpDown.Focused ||
                 QuarterNumericUpDown.Focused ||
                 BowingNumericUpDown.Focused ||
                 VerseNumericUpDown.Focused ||
                 WordNumericUpDown.Focused ||
                 LetterNumericUpDown.Focused
             )
            {
                m_client.Selection = new Selection(Book.Instance, SelectionScope.Chapter, new List<int>() { chapter_index });
            }
            else if ((sender == PreviousBookmarkButton) || (sender == NextBookmarkButton))
            {
            }
            else if ((sender == BrowseHistoryBackwardButton) || (sender == BrowseHistoryForwardButton))
            {
            }
            else
            {
            }

            DisplaySelection(sender, false);
            UpdateMinMaxChapterVerseWordLetter(m_client.TextMode, chapter_index);
        }
    }
    private void UpdateMinMaxChapterVerseWordLetter(string text_mode, int chapter_index)
    {
        if (m_client != null)
        {
            if (Book.Instance != null)
            {
                if (Book.Instance.Chapters != null)
                {
                    if ((chapter_index >= 0) && (chapter_index < Book.Instance.Chapters.Count))
                    {
                        Chapter chapter = Book.Instance.Chapters[chapter_index];
                        if (chapter != null)
                        {
                            try
                            {
                                ChapterVerseNumericUpDown.ValueChanged -= new EventHandler(NumericUpDown_ValueChanged);
                                ChapterWordNumericUpDown.ValueChanged -= new EventHandler(NumericUpDown_ValueChanged);
                                ChapterLetterNumericUpDown.ValueChanged -= new EventHandler(NumericUpDown_ValueChanged);

                                ChapterVerseNumericUpDown.Minimum = 1;
                                ChapterVerseNumericUpDown.Maximum = chapter.Verses.Count;

                                ChapterWordNumericUpDown.Minimum = 1;
                                ChapterWordNumericUpDown.Maximum = chapter.WordCount;

                                ChapterLetterNumericUpDown.Minimum = 1;
                                ChapterLetterNumericUpDown.Maximum = chapter.LetterCount;
                            }
                            finally
                            {
                                ChapterVerseNumericUpDown.ValueChanged += new EventHandler(NumericUpDown_ValueChanged);
                                ChapterWordNumericUpDown.ValueChanged += new EventHandler(NumericUpDown_ValueChanged);
                                ChapterLetterNumericUpDown.ValueChanged += new EventHandler(NumericUpDown_ValueChanged);
                            }
                        }
                    }
                }
            }
        }
    }

    private void BookNavigationControls_Enter(object sender, EventArgs e)
    {
        SearchGroupBox_Leave(sender, e);

        this.AcceptButton = null;
    }
    private void VerseNavigatorControls_Enter(object sender, EventArgs e)
    {
        SearchGroupBox_Leave(sender, e);

        this.AcceptButton = null;
    }
    private void StatisticsControls_Enter(object sender, EventArgs e)
    {
        SearchGroupBox_Leave(sender, e);
        this.AcceptButton = null;
    }
    private void StatisticsControls_Click(object sender, EventArgs e)
    {
        if (ModifierKeys == Keys.Control)
        {
            TextBox control = (sender as TextBox);
            if (control != null)
            {
                if (
                    (control != ValueTextBox)
                    &&
                    (control != PrimeFactorsTextBox)
                    //&&
                    //(control != DigitSumTextBox)
                    //&&
                    //(control != NthPrimeTextBox)
                    //&&
                    //(control != NthAdditivePrimeTextBox)
                    //&&
                    //(control != NthPurePrimeTextBox)
                   )
                {
                    long value = 0;
                    try
                    {
                        string text = control.Text;
                        if (text.Length > 0)
                        {
                            if (control.Name.StartsWith("Decimal"))
                            {
                                value = Radix.Decode(text, 10);
                            }
                            else
                            {
                                value = Radix.Decode(text, m_radix);
                            }
                        }
                    }
                    catch
                    {
                        value = -1; // error
                    }

                    FactorizeValue(value, "Number");
                }
            }
        }
    }
    private void StatusControls_Enter(object sender, EventArgs e)
    {
        SearchGroupBox_Leave(sender, e);
        this.AcceptButton = null;
    }

    private void NumericUpDown_Enter(object sender, EventArgs e)
    {
        SearchGroupBox_Leave(sender, e);

        this.AcceptButton = null;

        if (ModifierKeys == Keys.Control)
        {
            long value = 0;
            if (sender == ChapterComboBox)
            {
                if (ChapterComboBox.SelectedIndex != -1)
                {
                    string[] parts = ChapterComboBox.Text.Split('-');
                    if (parts.Length > 0)
                    {
                        value = long.Parse(parts[0]);
                    }
                }
            }
            else if (sender is NumericUpDown)
            {
                try
                {
                    value = (long)(sender as NumericUpDown).Value;
                }
                catch
                {
                    value = -1; // error
                }
            }
            else if (sender is TextBox)
            {
                try
                {
                    value = long.Parse((sender as TextBox).Text);
                }
                catch
                {
                    value = -1; // error
                }
            }
            else
            {
                value = -1; // error
            }
            FactorizeValue(value, "Position");
        }
    }
    private void NumericUpDown_Leave(object sender, EventArgs e)
    {
        this.AcceptButton = null;
    }
    private void NumericUpDown_ValueChanged(object sender, EventArgs e)
    {
        Control control = sender as NumericUpDown;
        if (control != null)
        {
            if (control.Focused)
            {
                DisplayNumericSelection(control, true);
            }
        }
    }
    private void ColorizePositionNumbers()
    {
        if (m_client != null)
        {
            if (Book.Instance != null)
            {
                if ((ChapterComboBox.SelectedIndex >= 0) && (ChapterComboBox.SelectedIndex < Book.Instance.Chapters.Count))
                {
                    int chapter_number = Book.Instance.Chapters[ChapterComboBox.SelectedIndex].Number;
                    ChapterComboBox.ForeColor = GetNumberTypeColor(chapter_number);
                }
            }
        }

        ChapterVerseNumericUpDown.ForeColor = GetNumberTypeColor((int)ChapterVerseNumericUpDown.Value);
        ChapterWordNumericUpDown.ForeColor = GetNumberTypeColor((int)ChapterWordNumericUpDown.Value);
        ChapterLetterNumericUpDown.ForeColor = GetNumberTypeColor((int)ChapterLetterNumericUpDown.Value);
        PageNumericUpDown.ForeColor = GetNumberTypeColor((int)PageNumericUpDown.Value);
        StationNumericUpDown.ForeColor = GetNumberTypeColor((int)StationNumericUpDown.Value);
        PartNumericUpDown.ForeColor = GetNumberTypeColor((int)PartNumericUpDown.Value);
        GroupNumericUpDown.ForeColor = GetNumberTypeColor((int)GroupNumericUpDown.Value);
        QuarterNumericUpDown.ForeColor = GetNumberTypeColor((int)QuarterNumericUpDown.Value);
        BowingNumericUpDown.ForeColor = GetNumberTypeColor((int)BowingNumericUpDown.Value);
        VerseNumericUpDown.ForeColor = GetNumberTypeColor((int)VerseNumericUpDown.Value);
        WordNumericUpDown.ForeColor = GetNumberTypeColor((int)WordNumericUpDown.Value);
        LetterNumericUpDown.ForeColor = GetNumberTypeColor((int)LetterNumericUpDown.Value);

        ChapterComboBox.Refresh();
        ChapterVerseNumericUpDown.Refresh();
        ChapterWordNumericUpDown.Refresh();
        ChapterLetterNumericUpDown.Refresh();
        PageNumericUpDown.Refresh();
        StationNumericUpDown.Refresh();
        PartNumericUpDown.Refresh();
        GroupNumericUpDown.Refresh();
        QuarterNumericUpDown.Refresh();
        BowingNumericUpDown.Refresh();
        VerseNumericUpDown.Refresh();
        WordNumericUpDown.Refresh();
        LetterNumericUpDown.Refresh();
    }
    private void ColorizePositionControls()
    {
        if (m_client != null)
        {
            if (m_client.Selection != null)
            {
                // Clear BackColors
                ChapterComboBox.BackColor = SystemColors.Window;
                ChapterVerseNumericUpDown.BackColor = SystemColors.Window;
                ChapterWordNumericUpDown.BackColor = SystemColors.Window;
                ChapterLetterNumericUpDown.BackColor = SystemColors.Window;
                PageNumericUpDown.BackColor = SystemColors.Window;
                StationNumericUpDown.BackColor = SystemColors.Window;
                PartNumericUpDown.BackColor = SystemColors.Window;
                GroupNumericUpDown.BackColor = SystemColors.Window;
                QuarterNumericUpDown.BackColor = SystemColors.Window;
                BowingNumericUpDown.BackColor = SystemColors.Window;
                VerseNumericUpDown.BackColor = SystemColors.Window;
                WordNumericUpDown.BackColor = SystemColors.Window;
                LetterNumericUpDown.BackColor = SystemColors.Window;

                switch (m_client.Selection.Scope)
                {
                    case SelectionScope.Book:
                        {
                        }
                        break;
                    case SelectionScope.Chapter:
                        {
                            ChapterComboBox.BackColor = Color.FromArgb(255, 255, 192);
                        }
                        break;
                    case SelectionScope.Page:
                        {
                            PageNumericUpDown.BackColor = Color.FromArgb(255, 255, 192);
                        }
                        break;
                    case SelectionScope.Station:
                        {
                            StationNumericUpDown.BackColor = Color.FromArgb(255, 255, 192);
                        }
                        break;
                    case SelectionScope.Part:
                        {
                            PartNumericUpDown.BackColor = Color.FromArgb(255, 255, 192);
                        }
                        break;
                    case SelectionScope.Group:
                        {
                            GroupNumericUpDown.BackColor = Color.FromArgb(255, 255, 192);
                        }
                        break;
                    case SelectionScope.Quarter:
                        {
                            QuarterNumericUpDown.BackColor = Color.FromArgb(255, 255, 192);
                        }
                        break;
                    case SelectionScope.Bowing:
                        {
                            BowingNumericUpDown.BackColor = Color.FromArgb(255, 255, 192);
                        }
                        break;
                    case SelectionScope.Verse:
                        {
                            ChapterVerseNumericUpDown.BackColor = Color.FromArgb(255, 255, 192);
                            VerseNumericUpDown.BackColor = Color.FromArgb(255, 255, 192);
                        }
                        break;
                    case SelectionScope.Word:
                        {
                            WordNumericUpDown.BackColor = Color.FromArgb(255, 255, 192);
                            ChapterWordNumericUpDown.BackColor = Color.FromArgb(255, 255, 192);
                        }
                        break;
                    case SelectionScope.Letter:
                        {
                            LetterNumericUpDown.BackColor = Color.FromArgb(255, 255, 192);
                            ChapterLetterNumericUpDown.BackColor = Color.FromArgb(255, 255, 192);
                        }
                        break;
                    default: // Unknown
                        {
                            MessageBox.Show("Unknown bookmark scope.", Application.ProductName);
                        }
                        break;
                }
            }
        }
    }

    private bool m_is_selection_mode = false;
    private void DisplayNumericSelection(Control control, bool add_to_history)
    {
        if (control is NumericUpDown)
        {
            if (control.Focused)
            {
                try
                {
                    ChapterVerseNumericUpDown.ValueChanged -= new EventHandler(NumericUpDown_ValueChanged);
                    ChapterWordNumericUpDown.ValueChanged -= new EventHandler(NumericUpDown_ValueChanged);
                    ChapterLetterNumericUpDown.ValueChanged -= new EventHandler(NumericUpDown_ValueChanged);
                    PageNumericUpDown.ValueChanged -= new EventHandler(NumericUpDown_ValueChanged);
                    StationNumericUpDown.ValueChanged -= new EventHandler(NumericUpDown_ValueChanged);
                    PartNumericUpDown.ValueChanged -= new EventHandler(NumericUpDown_ValueChanged);
                    GroupNumericUpDown.ValueChanged -= new EventHandler(NumericUpDown_ValueChanged);
                    QuarterNumericUpDown.ValueChanged -= new EventHandler(NumericUpDown_ValueChanged);
                    BowingNumericUpDown.ValueChanged -= new EventHandler(NumericUpDown_ValueChanged);
                    VerseNumericUpDown.ValueChanged -= new EventHandler(NumericUpDown_ValueChanged);
                    WordNumericUpDown.ValueChanged -= new EventHandler(NumericUpDown_ValueChanged);
                    LetterNumericUpDown.ValueChanged -= new EventHandler(NumericUpDown_ValueChanged);

                    int number = (int)((control as NumericUpDown).Value);

                    // backup number before as it will be overwritten with verse.Number
                    // if control is WordNumericUpDown OR LetterNumericUpDown or
                    // if control is ChapterWordNumericUpDown OR ChapterLetterNumericUpDown 
                    int word_number = 0;
                    int letter_number = 0;
                    if ((control == WordNumericUpDown) || (control == ChapterLetterNumericUpDown))
                    {
                        word_number = number;
                    }
                    else if ((control == LetterNumericUpDown) || (control == ChapterLetterNumericUpDown))
                    {
                        letter_number = number;
                    }

                    if (m_client != null)
                    {
                        SelectionScope scope = SelectionScope.Book;

                        if (control == ChapterVerseNumericUpDown)
                        {
                            if (Book.Instance.Verses != null)
                            {
                                scope = SelectionScope.Verse;

                                int chapter_index = ChapterComboBox.SelectedIndex;
                                if (Book.Instance.Chapters != null)
                                {
                                    int verse_number_in_chapter = (int)ChapterVerseNumericUpDown.Value;
                                    if ((chapter_index >= 0) && (chapter_index < Book.Instance.Chapters.Count))
                                    {
                                        Chapter chapter = Book.Instance.Chapters[chapter_index];
                                        if (chapter != null)
                                        {
                                            if (chapter.Verses != null)
                                            {
                                                if (Book.Instance.Chapters[chapter_index].Verses.Count > verse_number_in_chapter - 1)
                                                {
                                                    Verse verse = Book.Instance.Chapters[chapter_index].Verses[verse_number_in_chapter - 1];
                                                    if (verse != null)
                                                    {
                                                        number = verse.Number;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if ((control == ChapterWordNumericUpDown) || (control == ChapterLetterNumericUpDown))
                        {
                            if (Book.Instance.Verses != null)
                            {
                                scope = SelectionScope.Verse;

                                int chapter_index = ChapterComboBox.SelectedIndex;
                                if (Book.Instance.Chapters != null)
                                {
                                    if ((chapter_index >= 0) && (chapter_index < Book.Instance.Chapters.Count))
                                    {
                                        Chapter chapter = Book.Instance.Chapters[chapter_index];
                                        if (chapter != null)
                                        {
                                            if (chapter.Verses != null)
                                            {
                                                Verse verse = null;
                                                if (control == ChapterWordNumericUpDown)
                                                {
                                                    word_number = number + chapter.Verses[0].Words[0].Number - 1;
                                                    verse = Book.Instance.GetVerseByWordNumber(word_number);
                                                }
                                                else if (control == ChapterLetterNumericUpDown)
                                                {
                                                    letter_number = number + chapter.Verses[0].Words[0].Letters[0].Number - 1;
                                                    verse = Book.Instance.GetVerseByLetterNumber(letter_number);
                                                }

                                                if (verse != null)
                                                {
                                                    number = verse.Number;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if (control == PageNumericUpDown)
                        {
                            if (Book.Instance.Pages != null)
                            {
                                scope = SelectionScope.Page;
                            }
                        }
                        else if (control == StationNumericUpDown)
                        {
                            if (Book.Instance.Stations != null)
                            {
                                scope = SelectionScope.Station;
                            }
                        }
                        else if (control == PartNumericUpDown)
                        {
                            if (Book.Instance.Parts != null)
                            {
                                scope = SelectionScope.Part;
                            }
                        }
                        else if (control == GroupNumericUpDown)
                        {
                            if (Book.Instance.Groups != null)
                            {
                                scope = SelectionScope.Group;
                            }
                        }
                        else if (control == QuarterNumericUpDown)
                        {
                            if (Book.Instance.Quarters != null)
                            {
                                scope = SelectionScope.Quarter;
                            }
                        }
                        else if (control == BowingNumericUpDown)
                        {
                            if (Book.Instance.Bowings != null)
                            {
                                scope = SelectionScope.Bowing;
                            }
                        }
                        else if (control == VerseNumericUpDown)
                        {
                            if (Book.Instance.Verses != null)
                            {
                                scope = SelectionScope.Verse;
                            }
                        }
                        else if (control == WordNumericUpDown)
                        {
                            Verse verse = Book.Instance.GetVerseByWordNumber(word_number);
                            if (verse != null)
                            {
                                scope = SelectionScope.Verse;
                                number = verse.Number;
                            }
                        }
                        else if (control == LetterNumericUpDown)
                        {
                            Verse verse = Book.Instance.GetVerseByLetterNumber(letter_number);
                            if (verse != null)
                            {
                                scope = SelectionScope.Verse;
                                number = verse.Number;
                            }
                        }
                        else
                        {
                            // do nothing
                        }

                        // if selection has changed
                        if (
                            (m_client.Selection.Scope != scope)
                            ||
                            ((m_client.Selection.Indexes.Count > 0) && (m_client.Selection.Indexes[0] != (number - 1)))
                           )
                        {
                            List<int> indexes = new List<int>() { number - 1 };
                            m_client.Selection = new Selection(Book.Instance, scope, indexes);
                            DisplaySelection(control, add_to_history);
                        }

                        if ((control == WordNumericUpDown) || (control == ChapterWordNumericUpDown))
                        {
                            HighlightWord(word_number);
                        }
                        else if ((control == LetterNumericUpDown) || (control == ChapterLetterNumericUpDown))
                        {
                            HighlightLetter(letter_number);
                        }
                        else
                        {
                            // unknown
                        }
                    }
                }
                finally
                {
                    ChapterVerseNumericUpDown.ValueChanged += new EventHandler(NumericUpDown_ValueChanged);
                    ChapterWordNumericUpDown.ValueChanged += new EventHandler(NumericUpDown_ValueChanged);
                    ChapterLetterNumericUpDown.ValueChanged += new EventHandler(NumericUpDown_ValueChanged);
                    PageNumericUpDown.ValueChanged += new EventHandler(NumericUpDown_ValueChanged);
                    StationNumericUpDown.ValueChanged += new EventHandler(NumericUpDown_ValueChanged);
                    PartNumericUpDown.ValueChanged += new EventHandler(NumericUpDown_ValueChanged);
                    GroupNumericUpDown.ValueChanged += new EventHandler(NumericUpDown_ValueChanged);
                    QuarterNumericUpDown.ValueChanged += new EventHandler(NumericUpDown_ValueChanged);
                    BowingNumericUpDown.ValueChanged += new EventHandler(NumericUpDown_ValueChanged);
                    VerseNumericUpDown.ValueChanged += new EventHandler(NumericUpDown_ValueChanged);
                    WordNumericUpDown.ValueChanged += new EventHandler(NumericUpDown_ValueChanged);
                    LetterNumericUpDown.ValueChanged += new EventHandler(NumericUpDown_ValueChanged);
                }
            }
        }
    }
    private void DisplaySelection(object sender, bool add_to_history)
    {
        this.Cursor = Cursors.WaitCursor;
        MainTextBox.BeginUpdate();

        try
        {
            if (m_client != null)
            {
                if (m_client.Selection != null)
                {
                    m_current_line_index = 0; // 0 to get a valid verse from GetVerse(..)
                    m_is_selection_mode = true;
                    m_found_verses_displayed = false;

                    AutoCompleteHeaderLabel.Visible = false;
                    AutoCompleteAnywhereInWordCheckBox.Visible = false;
                    AutoCompleteListBox.SendToBack();

                    RefreshFindScopeLabel();
                    UpdateHeaderLabel();

                    MainTextBox.BackColor = SystemColors.Info;
                    TranslationTextBox.BackColor = SystemColors.Info;
                    try
                    {
                        MainTextBox.TextChanged -= new EventHandler(MainTextBox_TextChanged);
                        DisplaySelectionText();
                    }
                    finally
                    {
                        MainTextBox.TextChanged += new EventHandler(MainTextBox_TextChanged);
                    }

                    // re-calculate numerology value
                    CalculateCurrentText();
                    UpdateNumerologySystems();
                    CalculateCurrentValue();

                    DisplaySelectionPositions();
                    DisplayVersesWordsLetters(m_current_text);
                    DisplayLetterFrequencies();
                    DisplayPhraseLetterFrequencies();

                    MainTextBox.AlignToStart();

                    List<Verse> verses = m_client.Selection.Verses;
                    if (verses != null)
                    {
                        DisplayTranslation(verses);
                        if (verses.Count > 0)
                        {
                            if (add_to_history)
                            {
                                AddBrowseHistoryItem();
                            }

                            m_find_by_similarity_current_verse = verses[0];
                        }
                    }

                    // update graphs as we move between selections
                    if (DrawingPictureBoxEx.Visible)
                    {
                        RedrawCurrentGraph();
                    }

                    // update nofity icon text
                    UpdateNotifyIconText();
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
        }
        finally
        {
            MainTextBox.EndUpdate();
            this.Cursor = Cursors.Default;
        }
    }
    private void DisplaySelectionText()
    {
        this.Cursor = Cursors.WaitCursor;
        MainTextBox.BeginUpdate();

        try
        {
            if (m_client != null)
            {
                if (m_client.Selection != null)
                {
                    List<Verse> verses = m_client.Selection.Verses;
                    StringBuilder str = new StringBuilder();
                    if (verses != null)
                    {
                        foreach (Verse verse in verses)
                        {
                            str.Append(verse.GetText(m_client.TextMode) + verse.Endmark);
                        }
                        // remove trailing newLines
                        if ((verses.Count > 0) && (verses[0].Endmark == "\r\n"))
                        {
                            str.Remove(str.Length - 2, 2);
                        }
                    }
                    m_current_text = str.ToString();
                    MainTextBox.Text = m_current_text;
                }
            }
        }
        finally
        {
            MainTextBox.EndUpdate();
            this.Cursor = Cursors.Default;
        }
    }
    private void RefreshLanguageType(string text)
    {
        if (text.IsArabic())
        {
            SetLanguageType(FindByTextLanguageType.Arabic);
        }
        else
        {
            SetLanguageType(FindByTextLanguageType.Translation);
        }
        UpdateFindByTextControls();
    }
    private void DisplaySelectionPositions()
    {
        if (m_client != null)
        {
            if (m_client.Selection != null)
            {
                List<Verse> verses = m_client.Selection.Verses;
                if (verses != null)
                {
                    if (verses.Count > 0)
                    {
                        Verse verse = verses[0];
                        if (verse != null)
                        {
                            // show postion of verse in the Quran visually
                            ProgressBar.Minimum = Verse.MIN_NUMBER;
                            ProgressBar.Maximum = Verse.MAX_NUMBER;
                            ProgressBar.Value = verse.Number;
                            ProgressBar.Refresh();

                            if (ChapterComboBox.Items.Count > 0)
                            {
                                // to stop circular event firing
                                try
                                {
                                    ChapterComboBox.SelectedIndexChanged -= new EventHandler(ChapterComboBox_SelectedIndexChanged);
                                    ChapterComboBox.SelectedIndex = GetChapterIndex(verse.Chapter);
                                }
                                finally
                                {
                                    ChapterComboBox.SelectedIndexChanged += new EventHandler(ChapterComboBox_SelectedIndexChanged);
                                }
                                RefreshChapterSelection();
                            }
                            UpdateVersePositions(verse);

                            UpdateHeaderLabel();

                            Bookmark bookmark = m_client.GotoBookmark(m_client.Selection.Scope, m_client.Selection.Indexes);
                            if (bookmark != null)
                            {
                                NoteTextBox.ForeColor = m_note_view_color;
                                NoteTextBox.Text = bookmark.Note;
                                string hint = "Creation Time" + "\t" + bookmark.CreatedTime + "\r\n"
                                            + "Last Modified" + "\t" + bookmark.LastModifiedTime;
                                ToolTip.SetToolTip(NoteTextBox, hint);
                                RefreshBookmarkHistoryButtons();
                            }
                            else
                            {
                                DisplayNoteWritingInstruction();
                            }
                        }
                    }
                }
            }
        }
    }
    private void DisplayCurrentPositions()
    {
        if (Verse.IncludeEndLine)
        {
            if (MainTextBox.Lines.Length > 0)
            {
                Verse verse = GetCurrentVerse();
                if (verse != null)
                {
                    if (verse.Chapter != null)
                    {
                        // show postion of verse in the Quran visually
                        ProgressBar.Minimum = Verse.MIN_NUMBER;
                        ProgressBar.Maximum = Verse.MAX_NUMBER;
                        ProgressBar.Value = verse.Number;
                        ProgressBar.Refresh();

                        if (ChapterComboBox.Items.Count > 0)
                        {
                            // to stop circular event firing
                            try
                            {
                                ChapterComboBox.SelectedIndexChanged -= new EventHandler(ChapterComboBox_SelectedIndexChanged);
                                ChapterComboBox.SelectedIndex = GetChapterIndex(verse.Chapter);
                            }
                            finally
                            {
                                ChapterComboBox.SelectedIndexChanged += new EventHandler(ChapterComboBox_SelectedIndexChanged);
                            }
                            RefreshChapterSelection();
                        }
                        UpdateVersePositions(verse);

                        DisplayTranslation(verse);
                        UpdateHeaderLabel();
                    }
                }
            }
        }
    }
    int m_word_number_in_verse = -1;
    int m_letter_number_in_verse = -1;
    int m_word_number_in_chapter = -1;
    int m_letter_number_in_chapter = -1;
    private void UpdateVersePositions(Verse verse)
    {
        if (verse != null)
        {
            try
            {
                ChapterVerseNumericUpDown.ValueChanged -= new EventHandler(NumericUpDown_ValueChanged);
                ChapterWordNumericUpDown.ValueChanged -= new EventHandler(NumericUpDown_ValueChanged);
                ChapterLetterNumericUpDown.ValueChanged -= new EventHandler(NumericUpDown_ValueChanged);
                PageNumericUpDown.ValueChanged -= new EventHandler(NumericUpDown_ValueChanged);
                StationNumericUpDown.ValueChanged -= new EventHandler(NumericUpDown_ValueChanged);
                PartNumericUpDown.ValueChanged -= new EventHandler(NumericUpDown_ValueChanged);
                GroupNumericUpDown.ValueChanged -= new EventHandler(NumericUpDown_ValueChanged);
                QuarterNumericUpDown.ValueChanged -= new EventHandler(NumericUpDown_ValueChanged);
                BowingNumericUpDown.ValueChanged -= new EventHandler(NumericUpDown_ValueChanged);
                VerseNumericUpDown.ValueChanged -= new EventHandler(NumericUpDown_ValueChanged);
                WordNumericUpDown.ValueChanged -= new EventHandler(NumericUpDown_ValueChanged);
                LetterNumericUpDown.ValueChanged -= new EventHandler(NumericUpDown_ValueChanged);

                if ((verse.NumberInChapter >= Verse.MIN_NUMBER) && (verse.NumberInChapter <= Verse.MAX_NUMBER))
                {
                    if (verse.Chapter != null)
                    {
                        ChapterVerseNumericUpDown.Minimum = 1;
                        ChapterVerseNumericUpDown.Maximum = verse.Chapter.Verses.Count;
                        ChapterVerseNumericUpDown.Value = (verse.NumberInChapter > ChapterVerseNumericUpDown.Maximum) ? ChapterVerseNumericUpDown.Maximum : verse.NumberInChapter;
                    }
                }

                if (verse.Page != null)
                {
                    PageNumericUpDown.Value = verse.Page.Number;
                }
                if (verse.Station != null)
                {
                    StationNumericUpDown.Value = verse.Station.Number;
                }
                if (verse.Part != null)
                {
                    PartNumericUpDown.Value = verse.Part.Number;
                }
                if (verse.Group != null)
                {
                    GroupNumericUpDown.Value = verse.Group.Number;
                }
                if (verse.Quarter != null)
                {
                    QuarterNumericUpDown.Value = verse.Quarter.Number;
                }
                if (verse.Bowing != null)
                {
                    BowingNumericUpDown.Value = verse.Bowing.Number;
                }
                VerseNumericUpDown.Value = verse.Number;

                int char_index = MainTextBox.SelectionStart;
                int line_index = MainTextBox.GetLineFromCharIndex(char_index);
                int column_index = CalculateColumnIndex(char_index, line_index);
                int word_index = CalculateWordIndex(verse, column_index);
                int letter_index = CalculateLetterIndex(verse, column_index);
                if (verse.Words.Count > 0)
                {
                    WordNumericUpDown.Value = (verse.Words[0].Number + word_index > WordNumericUpDown.Maximum) ? WordNumericUpDown.Maximum : ((verse.Words[0].Number + word_index) > 0) ? verse.Words[0].Number + word_index : WordNumericUpDown.Minimum;
                }
                else // handle Simplified29Shadda loaded in Lite Edition due to last run was using it
                {
                    ResetNumerologySystem();
                }
                if (verse.LetterCount > 0)
                {
                    LetterNumericUpDown.Value = (verse.Words[0].Letters[0].Number + letter_index > LetterNumericUpDown.Maximum) ? LetterNumericUpDown.Maximum : ((verse.Words[0].Letters[0].Number + letter_index) > 0) ? verse.Words[0].Letters[0].Number + letter_index : LetterNumericUpDown.Minimum;
                }
                else // handle Simplified29Shadda loaded in Lite Edition due to last run was using it
                {
                    ResetNumerologySystem();
                }
                m_word_number_in_verse = word_index + 1;
                m_letter_number_in_verse = letter_index + 1;
                int word_count = 0;
                int letter_count = 0;
                if (verse.Chapter != null)
                {
                    foreach (Verse chapter_verse in verse.Chapter.Verses)
                    {
                        if (chapter_verse.NumberInChapter < verse.NumberInChapter)
                        {
                            word_count += chapter_verse.Words.Count;
                            letter_count += chapter_verse.LetterCount;
                        }
                    }
                }
                m_word_number_in_chapter = word_count + m_word_number_in_verse;
                m_letter_number_in_chapter = letter_count + m_letter_number_in_verse;

                ChapterWordNumericUpDown.Value = (m_word_number_in_chapter > ChapterWordNumericUpDown.Maximum) ? ChapterWordNumericUpDown.Maximum : m_word_number_in_chapter;
                ChapterLetterNumericUpDown.Value = (m_letter_number_in_chapter > ChapterLetterNumericUpDown.Maximum) ? ChapterLetterNumericUpDown.Maximum : m_letter_number_in_chapter;

                ColorizePositionNumbers();
                ColorizePositionControls();

                // update player buttons
                PlayerPreviousLabel.Enabled = (line_index > 0);
                PlayerNextLabel.Enabled = (line_index < (MainTextBox.Lines.Length - 1));
            }
            catch
            {
                // ignore poosible error due to non-Arabic search result
                // showing verses with more words than the words in the Arabic verse
                // and throwing exception when assigned to WordNumericUpDown.Value or LetterNumericUpDown.Value
            }
            finally
            {
                ChapterVerseNumericUpDown.ValueChanged += new EventHandler(NumericUpDown_ValueChanged);
                ChapterWordNumericUpDown.ValueChanged += new EventHandler(NumericUpDown_ValueChanged);
                ChapterLetterNumericUpDown.ValueChanged += new EventHandler(NumericUpDown_ValueChanged);
                PageNumericUpDown.ValueChanged += new EventHandler(NumericUpDown_ValueChanged);
                StationNumericUpDown.ValueChanged += new EventHandler(NumericUpDown_ValueChanged);
                PartNumericUpDown.ValueChanged += new EventHandler(NumericUpDown_ValueChanged);
                GroupNumericUpDown.ValueChanged += new EventHandler(NumericUpDown_ValueChanged);
                QuarterNumericUpDown.ValueChanged += new EventHandler(NumericUpDown_ValueChanged);
                BowingNumericUpDown.ValueChanged += new EventHandler(NumericUpDown_ValueChanged);
                VerseNumericUpDown.ValueChanged += new EventHandler(NumericUpDown_ValueChanged);
                WordNumericUpDown.ValueChanged += new EventHandler(NumericUpDown_ValueChanged);
                LetterNumericUpDown.ValueChanged += new EventHandler(NumericUpDown_ValueChanged);
            }
        }
    }

    private void UpdateNotifyIconText()
    {
        if (m_client != null)
        {
            if (m_client.Selection != null)
            {
                string notify_icon_text = Application.ProductName + " " + Globals.VERSION + " | " + m_client.TextMode + " | ";

                if ((m_client.Selection.Scope == SelectionScope.Word) || (m_client.Selection.Scope == SelectionScope.Letter))
                {
                    notify_icon_text += "Verse" + " " + VerseNumericUpDown.Value.ToString();
                }
                else // if m_bookmark_scope is Chapter, Page, Station, Part, Group, Quarter, Bowing, Verse
                {
                    StringBuilder str = new StringBuilder();
                    foreach (int index in m_client.Selection.Indexes)
                    {
                        str.Append((index + 1).ToString() + "+");
                    }
                    if (str.Length > 0) // remove last "+"
                    {
                        str.Remove(str.Length - "+".Length, "+".Length);
                    }
                    //if str too long, trim it
                    if (str.Length > MAX_SELECTON_SCOPE_LENGTH)
                    {
                        str.Remove(MAX_SELECTON_SCOPE_LENGTH, str.Length - MAX_SELECTON_SCOPE_LENGTH);
                        str.Append(" ...");
                    }
                    notify_icon_text += m_client.Selection.Scope.ToString() + ((m_client.Selection.Indexes.Count > 1) ? "s" : "") + " " + str.ToString();
                }

                // 63 is the max length allowed in notify icon text
                if (notify_icon_text.Length > 63)
                {
                    notify_icon_text = notify_icon_text.Substring(0, 60) + "...";
                }
                NotifyIcon.Text = notify_icon_text;
            }
        }
    }

    private void UpdateHeaderLabel()
    {
        if (m_client != null)
        {
            if (m_found_verses_displayed)
            {
                if (m_client.FoundVerses != null)
                {
                    int number = m_client.FoundVerses.Count;
                    if (m_find_by_text_search_type != FindByTextSearchType.Proximity)
                    {
                        if (m_client.FoundPhrases != null)
                        {
                            number = m_client.FoundPhrases.Count;
                        }
                    }
                    HeaderLabel.ForeColor = GetNumberTypeColor(number);
                    HeaderLabel.Text = m_find_result_header;
                    HeaderLabel.Refresh();
                }
            }
            else
            {
                HeaderLabel.ForeColor = Color.Black; ;
                if (m_client.Selection != null)
                {
                    if (m_client.Selection.Verses.Count > 0)
                    {
                        Verse verse = GetCurrentVerse();
                        if (verse != null)
                        {
                            if (verse.Chapter != null)
                            {
                                HeaderLabel.Text = verse.Chapter.Name
                                          + "  " + verse.NumberInChapter
                                          + "    "
                                    //+ "   منزل " + verse.Station.Number
                                          + "    جزء " + verse.Part.Number
                                          + "    حزب " + verse.Group.Number
                                          + "    ربع " + verse.Quarter.Number
                                    //+ "   ركوع " + verse.Bowing.Number
                                          + "    "
                                          + "   صفحة " + verse.Page.Number
                                          ;
                                HeaderLabel.Refresh();
                            }
                        }
                    }
                }
            }
        }
    }

    private void Highlight(int start, int length)
    {
        if ((start >= 0) && (start < MainTextBox.Text.Length))
        {
            MainTextBox.Select(start, length);
        }
    }
    private void Unhighlight(int start, int length)
    {
        if ((start >= 0) && (start < MainTextBox.Text.Length))
        {
            MainTextBox.Select(start, length + 1);
        }
    }
    private void UnhighlightCurrentLine()
    {
        Verse verse = GetCurrentVerse();
        if (verse != null)
        {
            int line_index = -1;
            if (m_found_verses_displayed)
            {
                line_index = m_found_verses_current_line_index;
            }
            else
            {
                line_index = m_current_line_index;
            }

            if (line_index >= 0)
            {
                int first_char_index = MainTextBox.GetFirstCharIndexFromLine(line_index);
                int hightlight_length = verse.GetText(m_client.TextMode).Length;
                if (m_found_verses_displayed)
                {
                    if (m_find_by_text_language_type == FindByTextLanguageType.Arabic)
                    {
                        hightlight_length += 1 + verse.ArabicAddress.Length;
                    }
                    else
                    {
                        hightlight_length += 1 + verse.PaddedAddress.Length;
                    }
                }

                Unhighlight(first_char_index, hightlight_length);
            }
        }
    }
    private void UnhighlightAll()
    {
        Unhighlight(0, MainTextBox.Text.Length);
    }

    private void UpdateChapterSelection()
    {
        SelectionScope scope = SelectionScope.Chapter;
        List<int> indexes = new List<int>();
        for (int i = 0; i < ChaptersListBox.SelectedIndices.Count; i++)
        {
            int index = ChaptersListBox.SelectedIndices[i];
            Chapter chapter = Book.Instance.Chapters[index];
            if (chapter != null)
            {
                indexes.Add(chapter.Number - 1);
            }
        }
        m_client.Selection = new Selection(Book.Instance, scope, indexes);
    }
    private void RefreshChapterSelection()
    {
        // many methods call this method so we need a guard
        // without this guard, we cannot select more than 1 chapter in ChaptersListBox
        if (ChaptersListBox.Focused) return;
        // and we cannot move backward/forward when inside the ChaptersListBox using Backspace key

        if (m_client != null)
        {
            if (m_client.Selection != null)
            {
                List<Chapter> selected_chapters = m_client.Selection.Chapters;

                // add the chapters to the ChaptersListBox
                try
                {
                    ChaptersListBox.SelectedIndexChanged -= new EventHandler(ChaptersListBox_SelectedIndexChanged);
                    ChaptersListBox.SelectedIndices.Clear();
                    foreach (Chapter chapter in selected_chapters)
                    {
                        int index = GetChapterIndex(chapter);
                        if ((index >= 0) && (index < ChaptersListBox.Items.Count))
                        {
                            ChaptersListBox.SelectedIndices.Add(index);
                        }
                    }
                }
                finally
                {
                    ChaptersListBox.SelectedIndexChanged += new EventHandler(ChaptersListBox_SelectedIndexChanged);
                }
            }
        }
    }
    private int GetChapterIndex(Chapter chapter)
    {
        if (chapter != null)
        {
            return chapter.Number - 1;
        }
        return -1;
    }
    private int m_last_selected_index = -1;
    private void ChaptersListBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (sender == ChaptersListBox)
        {
            if (m_client != null)
            {
                UpdateChapterSelection();
                DisplaySelection(sender, false);
                m_last_selected_index = ChaptersListBox.SelectedIndex;
            }
        }
    }
    private void AutoCompleteHeaderLabel_VisibleChanged(object sender, EventArgs e)
    {
        AutoCompleteAnywhereInWordCheckBox.Visible = AutoCompleteHeaderLabel.Visible;
    }

    private void LockUnlockLabel_Click(object sender, EventArgs e)
    {
        if (MainTextBox.ReadOnly)
        {
            MainTextBox.ReadOnly = false;
            TranslationTextBox.ReadOnly = false;
            if (File.Exists("Images/text_unlocked.png"))
            {
                LockUnlockLabel.Image = new Bitmap("Images/text_unlocked.png");
            }
            ToolTip.SetToolTip(LockUnlockLabel, "Edit mode");
        }
        else
        {
            MainTextBox.ReadOnly = true;
            TranslationTextBox.ReadOnly = true;
            if (File.Exists("Images/text_locked.png"))
            {
                LockUnlockLabel.Image = new Bitmap("Images/text_locked.png");
            }
            ToolTip.SetToolTip(LockUnlockLabel, "Readonly mode");
        }
    }
    private void WordWrapLabel_Click(object sender, EventArgs e) // F11
    {
        MainTextBox.WordWrap = !MainTextBox.WordWrap;
        if (MainTextBox.WordWrap)
        {
            Verse.IncludeNumber = true;
            Verse.IncludeEndLine = false;
            TranslationTextBox.WordWrap = true;

            if (File.Exists("Images/arrow_left.png"))
            {
                WordWrapLabel.Image = new Bitmap("Images/arrow_left.png");
            }
        }
        else
        {
            Verse.IncludeNumber = false;
            Verse.IncludeEndLine = true;
            TranslationTextBox.WordWrap = false;

            if (File.Exists("Images/arrow_rotate_anticlockwise.png"))
            {
                WordWrapLabel.Image = new Bitmap("Images/arrow_rotate_anticlockwise.png");
            }
        }

        if (m_found_verses_displayed)
        {
            DisplayFoundVerses(false, false);
        }
        else
        {
            DisplaySelection(sender, false);
        }
    }

    private Point m_previous_location_under_mouse = new Point(0, 0);
    private int m_previous_verse_under_mouse_number = 1;
    private int m_previous_word_under_mouse_number = 1;
    private int m_previous_letter_under_mouse_number = 1;
    private float m_text_zoom_factor = DEFAULT_TEXT_ZOOM_FACTOR;
    private float m_graphics_zoom_factor = DEFAULT_GRAPHICS_ZOOM_FACTOR;
    private float m_min_zoom_factor = 0.1F;
    private float m_max_zoom_factor = 2.0F;
    private float m_zoom_factor_increment = 0.1F;
    private float m_error_margin = 0.001F;
    private void ZoomInLabel_Click(object sender, EventArgs e)
    {
        if (DrawingPictureBoxEx.Visible)
        {
            if (m_graphics_zoom_factor <= (m_max_zoom_factor - m_zoom_factor_increment + m_error_margin))
            {
                DrawingPictureBoxEx.ZoomIn();
                m_graphics_zoom_factor = DrawingPictureBoxEx.ZoomFactor;
            }
            // re-check same condition after zoom_factor update
            ZoomInLabel.Enabled = (m_graphics_zoom_factor <= (m_max_zoom_factor - m_zoom_factor_increment + m_error_margin));
            ZoomOutLabel.Enabled = true;
            RedrawCurrentGraph();
        }
        else
        {
            if (m_text_zoom_factor <= (m_max_zoom_factor - m_zoom_factor_increment + m_error_margin))
            {
                m_text_zoom_factor += m_zoom_factor_increment;
                MainTextBox.ZoomFactor = m_text_zoom_factor;
                SetTranslationFontSize(m_translation_font_size * m_text_zoom_factor);
            }
            // re-check same condition after zoom_factor update
            ZoomInLabel.Enabled = (m_text_zoom_factor <= (m_max_zoom_factor - m_zoom_factor_increment + m_error_margin));
            ZoomOutLabel.Enabled = true;
        }
    }
    private void ZoomOutLabel_Click(object sender, EventArgs e)
    {
        if (DrawingPictureBoxEx.Visible)
        {
            if (m_graphics_zoom_factor >= (m_min_zoom_factor + m_zoom_factor_increment - m_error_margin))
            {
                DrawingPictureBoxEx.ZoomOut();
                m_graphics_zoom_factor = DrawingPictureBoxEx.ZoomFactor;
            }
            // re-check same condition after zoom_factor update
            ZoomOutLabel.Enabled = (m_graphics_zoom_factor >= (m_min_zoom_factor + m_zoom_factor_increment - m_error_margin));
            ZoomInLabel.Enabled = true;
            RedrawCurrentGraph();
        }
        else
        {
            if (m_text_zoom_factor >= (m_min_zoom_factor + m_zoom_factor_increment - m_error_margin))
            {
                m_text_zoom_factor -= m_zoom_factor_increment;
                MainTextBox.ZoomFactor = m_text_zoom_factor;
                SetTranslationFontSize(m_translation_font_size * m_text_zoom_factor);
            }
            // re-check same condition after zoom_factor update
            ZoomOutLabel.Enabled = (m_text_zoom_factor >= (m_min_zoom_factor + m_zoom_factor_increment - m_error_margin));
            ZoomInLabel.Enabled = true;
        }
    }
    private void MainTextBox_MouseWheel(object sender, MouseEventArgs e)
    {
        if (ModifierKeys == Keys.Control)
        {
            if (!DrawingPictureBoxEx.Visible)
            {
                if (MainTextBox.ZoomFactor <= (m_min_zoom_factor + m_error_margin))
                {
                    MainTextBox.ZoomFactor = m_min_zoom_factor;
                    ZoomOutLabel.Enabled = false;
                    ZoomInLabel.Enabled = true;
                }
                else if (MainTextBox.ZoomFactor >= (m_max_zoom_factor - m_error_margin))
                {
                    MainTextBox.ZoomFactor = m_max_zoom_factor;
                    ZoomOutLabel.Enabled = true;
                    ZoomInLabel.Enabled = false;
                }
                m_text_zoom_factor = MainTextBox.ZoomFactor;
            }
        }
    }
    private void MainTextBox_FontChanged(object sender, EventArgs e)
    {
        m_font_size = MainTextBox.Font.Size;
    }
    private void TranslationTextBox_FontChanged(object sender, EventArgs e)
    {
        // don't overwrite but let zooming scale it up/down
        //m_translation_font_size = TranslationTextBox.Font.Size;
    }
    private void MainTextBox_TextChanged(object sender, EventArgs e)
    {
        //ApplyFont(); // don't do as it disables Undo/Redo of MainTextBox
        if (MainTextBox.Text.Length == 0)   // if MainTextBox empty
        {
            m_current_text = "";

            // update the letter frequency table
            if (m_client != null)
            {
                m_client.CalculateLetterStatistics("");
                DisplayLetterStatistics();
            }
        }
        else
        {
            // there must be some code here but DisplaySelection has taken this responsibility (wrongly ???)
        }
    }
    private void MainTextBox_SelectionChanged(object sender, EventArgs e)
    {
        if (
             (sender == MainTextBox) &&
             (
               (MainTextBox.Focused) ||
               (ChapterWordNumericUpDown.Focused) ||
               (ChapterLetterNumericUpDown.Focused) ||
               (WordNumericUpDown.Focused) ||
               (LetterNumericUpDown.Focused)
             )
           )
        {
            if (m_client != null)
            {
                m_is_selection_mode = false;

                // re-calculate numerology value
                CalculateCurrentText();
                UpdateNumerologySystems();
                CalculateCurrentValue();

                DisplayVersesWordsLetters(m_current_text);
                DisplayCurrentPositions();
                DisplayLetterFrequencies();
                DisplayPhraseLetterFrequencies();
            }
        }
    }
    private void MainTextBox_Enter(object sender, EventArgs e)
    {
        SearchGroupBox_Leave(sender, e);

        this.AcceptButton = null;
        MainTextBox.Focus();
    }
    private void MainTextBox_Click(object sender, EventArgs e)
    {
        try
        {
            // reset find_by_similarity current verse
            m_find_by_similarity_current_verse = null;

            // calculate the Verse/Word/Letter distances
            if ((ModifierKeys == Keys.None) || (ModifierKeys == Keys.Shift))
            {
                int current_verse_number = (int)VerseNumericUpDown.Value;
                int current_word_number = (int)WordNumericUpDown.Value;
                int current_letter_number = (int)LetterNumericUpDown.Value;
                int verse_diff = Math.Abs(current_verse_number - m_previous_verse_under_mouse_number);
                int word_diff = Math.Abs(current_word_number - m_previous_word_under_mouse_number);
                int letter_diff = Math.Abs(current_letter_number - m_previous_letter_under_mouse_number);

                VerseDiffTextBox.Text = verse_diff.ToString();
                WordDiffTextBox.Text = word_diff.ToString();
                LetterDiffTextBox.Text = letter_diff.ToString();
                VerseDiffTextBox.ForeColor = GetNumberTypeColor(verse_diff);
                WordDiffTextBox.ForeColor = GetNumberTypeColor(word_diff);
                LetterDiffTextBox.ForeColor = GetNumberTypeColor(letter_diff);

                m_previous_verse_under_mouse_number = current_verse_number;
                m_previous_word_under_mouse_number = current_word_number;
                m_previous_letter_under_mouse_number = current_letter_number;

                PrepareVerseToPlay();
            }
            else // go to related words to word under mouse pointer
            {
                try
                {
                    MainTextBox.SelectionChanged -= new EventHandler(MainTextBox_SelectionChanged);
                    MainTextBox.SelectionChanged -= new EventHandler(MainTextBox_SelectionChanged);
                    MainTextBox.SelectionChanged -= new EventHandler(MainTextBox_SelectionChanged);

                    ToolTip.SetToolTip(MainTextBox, "");

                    if (ModifierKeys == Keys.Control)   // get related words
                    {
                        if (m_find_by_text_language_type == FindByTextLanguageType.Arabic)
                        {
                            DoFindRelatedWords(sender);
                        }
                        else if (m_find_by_text_language_type == FindByTextLanguageType.Translation)
                        {
                            DoFindExactMatch(sender);
                        }
                    }
                    else if (ModifierKeys == Keys.Alt)  // get exact words
                    {
                        if (m_find_by_text_language_type == FindByTextLanguageType.Arabic)
                        {
                            DoFindExactMatch(sender);
                        }
                        else if (m_find_by_text_language_type == FindByTextLanguageType.Translation)
                        {
                            DoFindExactMatch(sender);
                        }
                    }
                }
                finally
                {
                    MainTextBox.SelectionChanged += new EventHandler(MainTextBox_SelectionChanged);
                }
            }
        }
        finally
        {
            UpdateMainTextBoxCursor();
        }
    }
    private void MainTextBox_DoubleClick(object sender, EventArgs e)
    {
        // go to double_click_target (Chapter or Page) containing double clicked verse
        if (m_found_verses_displayed)
        {
            Verse verse = GetCurrentVerse();
            if (verse != null)
            {
                if (verse.Chapter != null)
                {
                    // backup position before going to target so you can come back here on Escape
                    m_found_verse_double_click_position = MainTextBox.SelectionStart;

                    if (m_client != null)
                    {
                        if (m_client.Selection != null)
                        {
                            if (m_found_verse_double_click_target == DoubleClickTarget.Page)
                            {
                                m_client.Selection = new Selection(Book.Instance, SelectionScope.Page, new List<int>() { verse.Page.Number - 1 });
                            }
                            else if (m_found_verse_double_click_target == DoubleClickTarget.Chapter)
                            {
                                m_client.Selection = new Selection(Book.Instance, SelectionScope.Chapter, new List<int>() { verse.Chapter.Number - 1 });
                            }

                            DisplaySelection(sender, false);

                            // move cursor to double_clicked verse
                            List<Verse> verses = m_client.Selection.Verses;
                            if (verses != null)
                            {
                                if (verses.Count > 0)
                                {
                                    int verses_to_skip = verse.Number - verses[0].Number;
                                    if (verses.Count > verses_to_skip)
                                    {
                                        int char_index = 0;
                                        for (int i = 0; i < verses_to_skip; i++)
                                        {
                                            char_index += verses[i].GetText(m_client.TextMode).Length + 1; // 1 for "\n"
                                        }
                                        MainTextBox.SelectionStart = char_index;

                                        // backup position before coming back to found verses in case you want to go back to target with Escape
                                        m_found_verse_double_click_target_position = MainTextBox.SelectionStart;

                                        // backup verse in case user does FindBySimilarity after DoubleClick
                                        m_find_by_similarity_current_verse = verse;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    private void MainTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        try
        {
            if ((e.Control) && (e.KeyCode == Keys.V))
            {
                SimplifyClipboardTextBeforePaste();
                Thread.Sleep(100); // must give chance for Clipboard to refresh its content before Paste
                MainTextBox.Paste();
                RestoreClipboardTextAfterPaste();
                e.Handled = true;
            }
        }
        finally
        {
            UpdateMainTextBoxCursor();
        }
    }
    private void MainTextBox_KeyUp(object sender, KeyEventArgs e)
    {
        try
        {
            bool NavigationKeys = (
            e.KeyCode == Keys.Up ||
            e.KeyCode == Keys.Right ||
            e.KeyCode == Keys.Down ||
            e.KeyCode == Keys.Left ||
            e.KeyCode == Keys.Home ||
            e.KeyCode == Keys.End);

            if (NavigationKeys)
            {
                PrepareVerseToPlay();
            }
        }
        finally
        {
            UpdateMainTextBoxCursor();
        }
    }
    private void PrepareVerseToPlay()
    {
        if (m_player != null)
        {
            if (m_player.Opened)
            {
                // prepare to play this verse, not next
                if (m_found_verses_displayed)
                {
                    if (m_found_verses_current_line_index >= 0)
                    {
                        m_found_verses_current_line_index--;
                    }
                }
                else
                {
                    if (m_current_line_index >= 0)
                    {
                        m_current_line_index--;
                    }
                }
            }
        }
    }
    private void MainTextBox_MouseDown(object sender, MouseEventArgs e)
    {
        // let cursor at mouse pointer know which word to get related words for
        RichTextBox richtextbox = (sender as RichTextBox);
        int start = richtextbox.GetCharIndexFromPosition(e.Location);
        if (
             (start <= richtextbox.SelectionStart)
             ||
             (start > (richtextbox.SelectionStart + richtextbox.SelectionLength))
           )
        {
            richtextbox.SelectionStart = start;
            richtextbox.SelectionLength = 0;
        }

        //// Go to related words
        //if (MouseButtons == MouseButtons.Right)
        //{
        //    try
        //    {
        //        MainTextBox.SelectionChanged -= new EventHandler(MainTextBox_SelectionChanged);
        //        MainTextBox.SelectionChanged -= new EventHandler(MainTextBox_SelectionChanged);
        //        MainTextBox.SelectionChanged -= new EventHandler(MainTextBox_SelectionChanged);

        //        ToolTip.SetToolTip(MainTextBox, "");

        //        if (m_find_by_text_language_type == FindByTextLanguageType.Arabic)
        //        {
        //            DoFindRelatedWords(sender);
        //        }
        //        else if (m_find_by_text_language_type == FindByTextLanguageType.Translation)
        //        {
        //            DoFindExactMatch(sender);
        //        }
        //    }
        //    finally
        //    {
        //        MainTextBox.SelectionChanged += new EventHandler(MainTextBox_SelectionChanged);
        //    }
        //}
    }
    private void MainTextBox_MouseMove(object sender, MouseEventArgs e)
    {
        UpdateMainTextBoxCursor();

        // stop flickering
        if (
            (Math.Abs(m_previous_location_under_mouse.X - e.X) < 8)
            &&
            (Math.Abs(m_previous_location_under_mouse.Y - e.Y) < 8)
           )
        {
            return;
        }
        m_previous_location_under_mouse = e.Location;

        try
        {
            if (m_client != null)
            {
                Word word = GetWordAtPointer(sender, e);
                if (word != null)
                {
                    if (word.Texts.ContainsKey(m_client.TextMode))
                    {
                        if (
                            (m_find_by_text_language_type == FindByTextLanguageType.Translation)
                            &&
                            (m_found_verses_displayed)
                           )
                        {
                            ToolTip.AutomaticDelay = 500;
                            ToolTip.SetToolTip(MainTextBox, word.Texts[m_client.TextMode]);
                        }
                        else
                        {
                            ToolTip.AutomaticDelay = 5000;

                            int occurrence = word.GetOccurrence(m_client.TextMode);
                            int occurrences = word.GetOccurrences(m_client.TextMode);

                            string root = word.Texts[m_client.TextMode];

                            if ((ModifierKeys == Keys.None) || (ModifierKeys == Keys.Shift))
                            {
                                ToolTip.AutomaticDelay = 500;
                                ToolTip.SetToolTip(MainTextBox, "");

                                if (Globals.EDITION == Edition.Grammar)
                                {
                                    string word_arabic_grammar = word.ArabicGrammar;

                                    this.Text = Application.ProductName + " | " + m_client.TextMode + " | " +
                                    (
                                        word.Address + "   " +
                                        word.Transliteration + "   " +
                                        word.Texts[m_client.TextMode] + "   " +
                                        word.Meaning + "   " +
                                        "[" + occurrence.ToString() + "/" + occurrences.ToString() + "]" +
                                        "      " + word_arabic_grammar.Replace("\t", " ").Replace("\r\n", "  ")
                                    );
                                }
                                else
                                {
                                    this.Text = Application.ProductName + " | " + m_client.TextMode + " | " +
                                    (
                                        word.Address + "   " +
                                        word.Transliteration + "   " +
                                        word.Texts[m_client.TextMode] + "   " +
                                        word.Meaning + "   " +
                                        "[" + occurrence.ToString() + "/" + occurrences.ToString() + "]"
                                    );
                                }
                            }
                            else
                            {
                                if (ModifierKeys == Keys.Control)
                                {
                                    string related_words_lines = "";
                                    int words_per_line = 0;
                                    int max_words_per_line = 10;
                                    List<string> related_words = Client.GetRelatedWords(m_client.TextMode, root);
                                    if (related_words != null)
                                    {
                                        StringBuilder str = new StringBuilder();
                                        foreach (string related_word in related_words)
                                        {
                                            words_per_line++;
                                            str.Append(related_word + (((words_per_line % max_words_per_line) == 0) ? "\r\n" : "\t"));
                                        }
                                        related_words_lines = str.ToString();
                                    }

                                    if (Globals.EDITION == Edition.Grammar)
                                    {
                                        ToolTip.SetToolTip
                                        (
                                             MainTextBox,
                                             word.Address + "        " +
                                             word.Transliteration + "        " +
                                             word.Texts[m_client.TextMode] + "        " +
                                             word.Meaning + "        " +
                                             occurrence.ToString() + "/" + occurrences.ToString() + "\t" +
                                             ((word.CorpusLemma.Length > 0) ? word.CorpusLemma + "\t" : "") +
                                             ((word.CorpusSpecialGroup.Length > 0) ? word.CorpusSpecialGroup + ((word.CorpusRoot.Length > 0) ? "\t" : "") : "") +
                                             ((word.CorpusRoot.Length > 0) ? word.CorpusRoot : "") + "\r\n" + "\r\n" +
                                             ((word.ArabicGrammar.Length > 0) ? word.ArabicGrammar : "") + "\r\n" +
                                             ((word.EnglishGrammar.Length > 0) ? word.EnglishGrammar : "") + "\r\n" + "\r\n" +
                                             related_words_lines
                                        );

                                        this.Text = Application.ProductName + " | " + m_client.TextMode + " | " +
                                        (
                                             word.Address + "   " +
                                             word.Transliteration + "   " +
                                             word.Texts[m_client.TextMode] + "        " +
                                             word.Meaning + "   " +
                                             "[" + occurrence.ToString() + "/" + occurrences.ToString() + "]" +
                                             "      " + word.ArabicGrammar.Replace("\t", " ").Replace("\r\n", "  ")
                                        );
                                    }
                                    else // no Grammar
                                    {
                                        ToolTip.SetToolTip
                                        (
                                             MainTextBox,
                                             word.Address + "        " +
                                             word.Transliteration + "        " +
                                             word.Texts[m_client.TextMode] + "        " +
                                             word.Meaning + "        " +
                                             occurrence.ToString() + "/" + occurrences.ToString() + "\r\n" +
                                             related_words_lines
                                        );

                                        this.Text = Application.ProductName + " | " + m_client.TextMode + " | " +
                                        (
                                             word.Address + "   " +
                                             word.Transliteration + "   " +
                                             word.Texts[m_client.TextMode] + "   " +
                                             word.Meaning + "   " +
                                             "[" + occurrence.ToString() + "/" + occurrences.ToString() + "]"
                                        );
                                    }
                                }
                                else if (ModifierKeys == Keys.Alt)
                                {
                                    List<string> related_words = Client.GetRelatedWords(m_client.TextMode, root);
                                    if (related_words != null)
                                    {
                                        StringBuilder str = new StringBuilder();
                                        foreach (string related_word in related_words)
                                        {
                                            str.Append(related_word + "\r\n");
                                        }
                                        ToolTip.SetToolTip(MainTextBox, str.ToString());
                                    }

                                    //// Related Verses
                                    //List<Verse> related_verses = Client.GetRelatedVerses(m_client.TextMode, root);
                                    //if (related_verses != null)
                                    //{
                                    //    StringBuilder str = new StringBuilder();
                                    //    foreach (Verse related_verse in related_verses)
                                    //    {
                                    //        str.Append(related_verse.GetText(m_client.TextMode) + "\r\n");
                                    //    }
                                    //    ToolTip.SetToolTip(MainTextBox, str.ToString());
                                    //}
                                }
                            }
                        }
                    }
                }
            }
        }
        catch  /*(Exception ex)*/
        {
            // log error
        }
    }
    private void MainTextBox_MouseEnter(object sender, EventArgs e)
    {
        UpdateMainTextBoxCursor();
    }
    private void UpdateMainTextBoxCursor()
    {
        if ((ModifierKeys == Keys.Control) || (ModifierKeys == Keys.Alt))
        {
            // stop cursor flicker
            if (MainTextBox.Cursor != Cursors.Hand)
            {
                MainTextBox.Cursor = Cursors.Hand;
            }
        }
        else
        {
            // stop cursor flicker
            if (MainTextBox.Cursor != Cursors.IBeam)
            {
                MainTextBox.Cursor = Cursors.IBeam;
            }
        }
    }
    private void MainTextBox_Resize(object sender, EventArgs e)
    {
        MainTextBox_VScroll(sender, e);
    }
    private void MainTextBox_VScroll(object sender, EventArgs e)
    {
        if (MouseButtons == MouseButtons.Left)
        {
            //ColorizeMatches(); // only use if DisplyFoundVerses uses ColorizeMatches(count);
        }
    }

    // F3 and Shift+F3 Goto next/previous matches
    private class TextSelection
    {
        public int Start = 0;
        public int Length = 0;
    }
    private List<TextSelection> m_text_selections = new List<TextSelection>();
    private void BuildTextSelection(int start, int length)
    {
        // build text_selections list for F3 and Shift+F3
        TextSelection text_selection = new TextSelection();
        text_selection.Start = start;
        text_selection.Length = length;
        m_text_selections.Add(text_selection);
    }
    private int m_text_selection_index = -1;
    private void GototSelectionIndexBeforeCursor()
    {
        m_text_selection_index = -1;
        for (int i = 0; i < m_text_selections.Count; i++)
        {
            if (m_text_selections[i].Start > MainTextBox.SelectionStart)
            {
                m_text_selection_index = i - 1;
                break;
            }
        }
    }
    private void GotoSelectionIndexAfterCursor()
    {
        m_text_selection_index = m_text_selections.Count;
        for (int i = m_text_selections.Count - 1; i >= 0; i--)
        {
            if (m_text_selections[i].Start < MainTextBox.SelectionStart)
            {
                m_text_selection_index = i + 1;
                break;
            }
        }
    }
    private void FindNextMatch()
    {
        if (m_found_verses_displayed)
        {
            if (m_text_selections != null)
            {
                if (m_text_selections.Count > 0)
                {
                    // find the index prior to the current cursor postion
                    GototSelectionIndexBeforeCursor();
                    m_text_selection_index++;

                    // round robin
                    if (m_text_selection_index == m_text_selections.Count)
                    {
                        m_text_selection_index = 0;
                    }

                    // find next match
                    if ((m_text_selection_index >= 0) && (m_text_selection_index < m_text_selections.Count))
                    {
                        int start = m_text_selections[m_text_selection_index].Start;
                        int length = m_text_selections[m_text_selection_index].Length;
                        if ((start >= 0) && (start < MainTextBox.Text.Length))
                        {
                            MainTextBox.Select(start, length);
                            MainTextBox.SelectionColor = Color.Red;
                        }
                    }
                }
            }
        }
        RefreshApplicationCaption();
    }
    private void FindPreviousMatch()
    {
        if (m_found_verses_displayed)
        {
            if (m_text_selections != null)
            {
                if (m_text_selections.Count > 0)
                {
                    // find the index after the current cursor postion
                    GotoSelectionIndexAfterCursor();
                    m_text_selection_index--;

                    // round robin
                    if (m_text_selection_index < 0)
                    {
                        m_text_selection_index = m_text_selections.Count - 1;
                    }

                    // find previous match
                    if ((m_text_selection_index >= 0) && (m_text_selection_index < m_text_selections.Count))
                    {
                        int start = m_text_selections[m_text_selection_index].Start;
                        int length = m_text_selections[m_text_selection_index].Length;
                        if ((start >= 0) && (start < MainTextBox.Text.Length))
                        {
                            MainTextBox.Select(start, length);
                            MainTextBox.SelectionColor = Color.Red;
                        }
                    }
                }
            }
        }
        RefreshApplicationCaption();
    }
    private void RefreshApplicationCaption()
    {
        string caption = this.Text;
        int pos = caption.IndexOf(CAPTION_SEPARATOR);
        if (pos > -1)
        {
            caption = caption.Substring(0, pos);
        }

        if (m_found_verses_displayed)
        {
            caption += CAPTION_SEPARATOR + " Match " + ((m_text_selection_index + 1) + "/" + m_text_selections.Count);
        }
        else
        {
            //caption += CAPTION_SEPARATOR;
        }

        this.Text = caption;
    }

    private int m_current_line_index = -1;
    private int m_found_verses_current_line_index = -1;
    private int GetCurrentLineIndex()
    {
        int char_index = MainTextBox.SelectionStart;
        int line_index = MainTextBox.GetLineFromCharIndex(char_index);
        return line_index;
    }
    private bool GotoLine(int line_index)
    {
        if (m_client != null)
        {
            if (line_index >= 0)
            {
                int index = MainTextBox.GetFirstCharIndexFromLine(line_index);
                if ((index >= 0) && (index < MainTextBox.Text.Length))
                {
                    //MainTextBox.Focus();
                    MainTextBox.Select(index, 0);
                    MainTextBox.Refresh();

                    Verse verse = GetCurrentVerse();
                    UpdateVersePositions(verse);

                    return true;
                }
            }
        }
        return false;
    }
    private bool GotoNextLine()
    {
        if (m_client != null)
        {
            if (m_client.Selection != null)
            {
                // reset first verse to -1 on new selection
                if (m_is_selection_mode)
                {
                    if (m_found_verses_displayed)
                    {
                        if (m_found_verses_current_line_index == 0)
                        {
                            m_found_verses_current_line_index--;
                        }
                    }
                    else
                    {
                        if (m_current_line_index == 0)
                        {
                            m_current_line_index--;
                        }
                    }
                    m_is_selection_mode = false;
                }

                // go to next line
                if (m_found_verses_displayed)
                {
                    m_found_verses_current_line_index++;
                }
                else
                {
                    m_current_line_index++;
                }

                int line_index = -1;
                if (m_found_verses_displayed)
                {
                    line_index = m_found_verses_current_line_index;
                }
                else
                {
                    line_index = m_current_line_index;
                }
                return GotoLine(line_index);
            }
        }
        return false;
    }
    private Verse GetCurrentVerse()
    {
        int line_index = GetCurrentLineIndex();

        if (m_found_verses_displayed)
        {
            m_found_verses_current_line_index = line_index;
        }
        else
        {
            m_current_line_index = line_index;
        }

        return GetVerseAt(line_index);
    }
    private Verse GetVerseAtCursor(object sender)
    {
        Verse verse = null;
        if (sender is RichTextBox)
        {
            int char_index = (sender as RichTextBox).SelectionStart;
            int line_index = (sender as RichTextBox).GetLineFromCharIndex(char_index);
            verse = GetVerseAt(line_index);
        }
        return verse;
    }
    private Word GetWordAtCursor(object sender)
    {
        Word word = null;
        if (sender is RichTextBox)
        {
            int char_index = (sender as RichTextBox).SelectionStart;
            int line_index = (sender as RichTextBox).GetLineFromCharIndex(char_index);
            word = GetWordAt(line_index, char_index);
        }
        return word;
    }
    //private Letter GetLetterAtCursor(object sender)
    //{
    //    Letter letter = null;
    //    if (sender is RichTextBox)
    //    {
    //        int char_index = (sender as RichTextBox).SelectionStart;
    //        int line_index = (sender as RichTextBox).GetLineFromCharIndex(char_index);
    //        letter = GetLetterAt(line_index, char_index);
    //    }
    //    return letter;
    //}
    private Verse GetVerseAtPointer(object sender, MouseEventArgs e)
    {
        Verse verse = null;
        if (sender is RichTextBox)
        {
            Point mouse_location = e.Location;
            int char_index = (sender as RichTextBox).GetCharIndexFromPosition(mouse_location);
            int line_index = (sender as RichTextBox).GetLineFromCharIndex(char_index);
            verse = GetVerseAt(line_index);
        }
        return verse;
    }
    private Word GetWordAtPointer(object sender, MouseEventArgs e)
    {
        Word word = null;
        if (sender is RichTextBox)
        {
            Point mouse_location = e.Location;
            int char_index = (sender as RichTextBox).GetCharIndexFromPosition(mouse_location);
            int line_index = (sender as RichTextBox).GetLineFromCharIndex(char_index);
            word = GetWordAt(line_index, char_index);
        }
        return word;
    }
    //private Letter GetLetterAtPointer(object sender, MouseEventArgs e)
    //{
    //    Letter letter = null;
    //    if (sender is RichTextBox)
    //    {
    //        Point mouse_location = e.Location;
    //        int char_index = (sender as RichTextBox).GetCharIndexFromPosition(mouse_location);
    //        int line_index = (sender as RichTextBox).GetLineFromCharIndex(char_index);
    //        letter = GetLetterAt(line_index, char_index);
    //    }
    //    return letter;
    //}
    private Verse GetVerseAt(int line_index)
    {
        Verse verse = null;
        if (m_client != null)
        {
            if (m_client.Selection != null)
            {
                if (m_found_verses_displayed)
                {
                    if (m_client.FoundVerses != null)
                    {
                        if ((line_index >= 0) && (line_index < m_client.FoundVerses.Count))
                        {
                            verse = m_client.FoundVerses[line_index];
                        }
                    }
                }
                else // m_curent_verses displayed
                {
                    List<Verse> verses = m_client.Selection.Verses;
                    if (verses != null)
                    {
                        if ((line_index >= 0) && (line_index < verses.Count))
                        {
                            verse = verses[line_index];
                        }
                    }
                }
            }
        }
        return verse;
    }
    private Word GetWordAt(int line_index, int char_index)
    {
        Word word = null;
        if (m_client != null)
        {
            if (char_index >= 0)
            {
                Verse verse = GetVerseAt(line_index);
                if (verse != null)
                {
                    int column_index = CalculateColumnIndex(char_index, line_index);
                    column_index--; // to show word info when inside word not after the last letter of the previous word
                    int word_index = CalculateWordIndex(verse, column_index);
                    if ((word_index >= 0) && (word_index < verse.Words.Count))
                    {
                        word = verse.Words[word_index];
                    }
                }
            }
        }
        return word;
    }
    //private Letter GetLetterAt(int line_index, int char_index)
    //{
    //    Letter letter = null;
    //    if (m_client != null)
    //    {
    //        if (char_index >= 0)
    //        {
    //            Verse verse = GetVerseAt(line_index);
    //            if (verse != null)
    //            {
    //                int column_index = CalculateColumnIndex(char_index, line_index);
    //                int word_index = CalculateWordIndex(verse, column_index);
    //                int letter_index = CalculateLetterIndex(verse, column_index);
    //                if ((word_index >= 0) && (word_index < verse.Words.Count))
    //                {
    //                    Word word = verse.Words[word_index];
    //                    int word_letter_index = letter_index;
    //                    for (int i = 0; i < word_index; i++)
    //                    {
    //                        word_letter_index -= verse.Words[i].Letters.Count;// +1; // 1 for space // CalculateLetterIndex already discounts " "
    //                    }
    //                    if ((word_letter_index >= 0) && (word_letter_index < word.Letters.Count))
    //                    {
    //                        letter = word.Letters[word_letter_index];
    //                    }
    //                }
    //            }
    //        }
    //    }
    //    return letter;
    //}

    private void HighlightWord(int word_number)
    {
        if (m_client != null)
        {
            if (Book.Instance != null)
            {
                Verse verse = Book.Instance.GetVerseByWordNumber(word_number);
                if (verse != null)
                {
                    string[] words = verse.GetText(m_client.TextMode).Split(' ');
                    int remaining_word_count = word_number - verse.Words[0].Number;
                    int word_offset = 0;
                    for (int i = 0; i < remaining_word_count; i++)
                    {
                        word_offset += words[i].Length;
                        word_offset++;  // increment for space after word
                    }
                    int word_length = 0;
                    if ((remaining_word_count >= 0) && (remaining_word_count < words.Length))
                    {
                        word_length = words[remaining_word_count].Length;
                    }

                    Highlight(word_offset, word_length);
                }
            }
        }
    }
    private void HighlightLetter(int letter_number)
    {
        if (m_client != null)
        {
            if (Book.Instance != null)
            {
                Verse verse = Book.Instance.GetVerseByLetterNumber(letter_number);
                if (verse != null)
                {
                    string[] word_texts = verse.GetText(m_client.TextMode).Split(' ');
                    int remaining_letter_count = letter_number - verse.Words[0].Letters[0].Number;
                    int letter_offset = remaining_letter_count;
                    foreach (string word_text in word_texts)
                    {
                        if (remaining_letter_count < word_text.Length)
                        {
                            break;
                        }
                        remaining_letter_count -= word_text.Length;
                        letter_offset++;    // increment for space after word
                    }

                    int letter_length = 1;
                    Highlight(letter_offset, letter_length);
                }
            }
        }
    }
    private int CalculateColumnIndex(int char_index, int line_index)
    {
        int column_index = char_index;
        if (m_client != null)
        {
            if (m_client.Selection != null)
            {
                if (m_found_verses_displayed)
                {
                    if (m_client.FoundVerses != null)
                    {
                        if (m_client.FoundVerses.Count > 0)
                        {
                            if (m_find_by_text_language_type == FindByTextLanguageType.Arabic)
                            {
                                for (int i = 0; i < line_index; i++)
                                {
                                    if (i < m_client.FoundVerses.Count)
                                    {
                                        column_index -= m_client.FoundVerses[i].ArabicAddress.Length + 1 + m_client.FoundVerses[i].GetText(m_client.TextMode).Length + 1;
                                    }
                                }
                                // skip the address at the beginning of the current verse
                                if ((line_index >= 0) && (line_index < m_client.FoundVerses.Count))
                                {
                                    column_index -= m_client.FoundVerses[line_index].ArabicAddress.Length + 1; // no verse.Texts[m_client.TextMode] nor + 1 for \n in MainTextBox
                                }
                            }
                            else if (m_find_by_text_language_type == FindByTextLanguageType.Translation)
                            {
                                for (int i = 0; i < line_index; i++)
                                {
                                    if (i < m_client.FoundVerses.Count)
                                    {
                                        column_index -= m_client.FoundVerses[i].PaddedAddress.Length + 1 + m_client.FoundVerses[i].GetText(m_client.TextMode).Length + 1;
                                    }
                                }
                                // skip the address at the beginning of the current verse
                                if ((line_index >= 0) && (line_index < m_client.FoundVerses.Count))
                                {
                                    column_index -= m_client.FoundVerses[line_index].PaddedAddress.Length + 1; // no verse.Texts[m_client.TextMode] nor \n in MainTextBox
                                }
                            }
                        }
                    }
                }
                else
                {
                    List<Verse> verses = m_client.Selection.Verses;
                    if (verses != null)
                    {
                        for (int i = 0; i < line_index; i++)
                        {
                            if (i < verses.Count)
                            {
                                column_index -= verses[i].GetText(m_client.TextMode).Length + 1;
                            }
                        }
                    }
                }
            }
        }

        return column_index;
    }
    private int CalculateWordIndex(Verse verse, int column_number)
    {
        int word_index = 0;
        if (verse != null)
        {
            string[] word_texts = verse.GetText(m_client.TextMode).Split(' ');
            foreach (string word_text in word_texts)
            {
                // skip stop marks (1-letter words), except real Quranic 1-letter words
                if ((word_text.Length == 1) && (word_text != "ص") && (word_text != "ق") && (word_text != "ن"))
                {
                    // skip stop_mark words
                    column_number -= word_text.Length + 1; // 1 for space
                }
                else
                {
                    if (column_number < word_text.Length)
                    {
                        break;
                    }
                    word_index++;
                    column_number -= word_text.Length + 1; // 1 for space
                }
            }
        }
        return word_index;
    }
    private int CalculateLetterIndex(Verse verse, int column_number)
    {
        int letter_number = 0;
        if (verse.GetText(m_client.TextMode).Length >= column_number)
        {
            for (int i = 0; i < column_number; i++)
            {
                char character = verse.GetText(m_client.TextMode)[i];
                foreach (char c in Constants.ARABIC_LETTERS)
                {
                    if (character == c)
                    {
                        letter_number++;
                        break;
                    }
                }
            }
            return letter_number;
        }
        else  // text edited by hand
        {
            return verse.GetText(m_client.TextMode).Length;
        }
    }
    private string RemoveVerseAddresses(string text)
    {
        if (string.IsNullOrEmpty(text)) return "";

        string[] lines = text.Split('\n');
        StringBuilder str = new StringBuilder();
        foreach (string line in lines)
        {
            if (!string.IsNullOrEmpty(line))
            {
                string[] line_parts = line.Split('\t'); // (TAB delimited)
                if (line_parts.Length > 1) // has address
                {
                    str.Append(line_parts[1] + "\n");  // remove verse address
                }
                else if (line_parts.Length > 0)
                {
                    str.Append(line_parts[0] + "\n");  // leave it as it is
                }
            }
        }
        // remove last newline
        if (str.Length > 1)
        {
            str.Remove(str.Length - 1, 1);
        }
        return str.ToString();
    }
    private string RemoveVerseEndMarks(string text)
    {
        // RTL script misplaces brackets
        return text; // do nothing for now

        if (string.IsNullOrEmpty(text)) return "";
        while (text.Contains(Verse.OPEN_BRACKET) || text.Contains(Verse.CLOSE_BRACKET)) // F11 WordWrap
        {
            int start = text.IndexOf(Verse.OPEN_BRACKET);
            int end = text.IndexOf(Verse.CLOSE_BRACKET);
            if ((start >= 0) && (end >= 0))
            {
                if (start < end)
                {
                    text = text.Remove(start, (end - start) + 1); // remove space after it
                }
                else // Arabic script misplaces brackets
                {
                    text = text.Remove(end, (start - end) + 1); // remove space after it
                }
            }
        }
        return text;
    }

    private void DisplayLetterFrequencies()
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            if (m_client != null)
            {
                // update the letter frequency table
                if (m_current_text.Length >= 0) // if some text is selected
                {
                    m_client.CalculateLetterStatistics(m_current_text);

                    DisplayLetterStatistics();
                }
            }
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }
    private void DisplayPhraseLetterFrequencies()
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            if (m_client != null)
            {
                FindByFrequencySumNumericUpDown.Value = 0;
                FindByFrequencySumNumericUpDown.ForeColor = GetNumberTypeColor(0);
                FindByFrequencySumNumericUpDown.Refresh();

                if (m_current_text.Length >= 0) // if some text is selected
                {
                    string phrase = FindByFrequencyPhraseTextBox.Text;

                    int phrase_letter_frequency_sum = m_client.CalculatePhraseLetterStatistics(m_current_text, phrase, m_find_by_frequency_sum_type);
                    if (phrase_letter_frequency_sum >= 0)
                    {
                        FindByFrequencySumNumericUpDown.Value = phrase_letter_frequency_sum;
                    }

                    DisplayPhraseLetterStatistics();
                }
            }
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }
    private void DisplayVersesWordsLetters(string text)
    {
        text = text.Trim();

        // get rid of the extra "\r\n" if any to count correct verse number
        string[] separators = { "\r\n", "\r", "\n" };
        string[] parts = text.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        int verse_count = parts.Length;

        // dispaly word and letter count
        string[] patterns = { " ", "\r\n", "\r", "\n" };
        string[] words = text.Split(patterns, StringSplitOptions.RemoveEmptyEntries);
        int word_count = 0;
        int letter_count = 0;
        foreach (string word in words)
        {
            // skip empty words
            if (word.Length == 0)
            {
                continue;
            }

            // skip verse address
            if ((word.Contains(Verse.OPEN_BRACKET)) || (word.Contains(Verse.CLOSE_BRACKET)))
            {
                continue;
            }

            // skip stop marks
            if (Constants.STOP_MARKS.Contains(word[0]))
            {
                continue;
            }

            word_count++;
            letter_count += word.Length;
        }
        DisplayVersesWordsLetters(verse_count, word_count, letter_count);
    }
    private void DisplayVersesWordsLetters(Verse verse)
    {
        if (verse != null)
        {
            int verse_count = 1;
            int word_count = verse.Words.Count;
            int letter_count = verse.LetterCount;
            DisplayVersesWordsLetters(verse_count, word_count, letter_count);
        }
    }
    private void DisplayVersesWordsLetters(List<Verse> verses)
    {
        if (verses != null)
        {
            int verse_count = verses.Count;
            int word_count = 0;
            int letter_count = 0;
            foreach (Verse verse in verses)
            {
                word_count += verse.Words.Count;
                letter_count += verse.LetterCount;
            }
            DisplayVersesWordsLetters(verse_count, word_count, letter_count);
        }
    }
    //private void DisplayVersesWordsLetters(int verse_count, int word_count, int letter_count)
    //{
    //    VersesTextBox.Text = verse_count.ToString();
    //    VersesTextBox.ForeColor = GetNumberTypeColor(verse_count);
    //    ToolTip.SetToolTip(VersesTextBox, verse_count.ToString());
    //    VersesTextBox.Refresh();
    //    WordsTextBox.Text = word_count.ToString();
    //    WordsTextBox.ForeColor = GetNumberTypeColor(word_count);
    //    ToolTip.SetToolTip(WordsTextBox, word_count.ToString());
    //    WordsTextBox.Refresh();
    //    LettersTextBox.Text = letter_count.ToString();
    //    LettersTextBox.ForeColor = GetNumberTypeColor(letter_count);
    //    ToolTip.SetToolTip(LettersTextBox, letter_count.ToString());
    //    LettersTextBox.Refresh();
    //}

    private long m_radix = DEFAULT_RADIX;
    private void RadixValueLabel_Click(object sender, EventArgs e)
    {
        try
        {
            // get values in current radix
            int verse_count = (int)Radix.Decode(VersesTextBox.Text, m_radix);
            int word_count = (int)Radix.Decode(WordsTextBox.Text, m_radix);
            int letter_count = (int)Radix.Decode(LettersTextBox.Text, m_radix);
            long value = Radix.Decode(ValueTextBox.Text, m_radix);

            // toggle radix
            if (m_radix == 10)
            {
                m_radix = 19;
            }
            else
            {
                m_radix = 10;
            }
            RadixValueLabel.Text = m_radix.ToString();

            // display values in nex radix
            DisplayVersesWordsLetters(verse_count, word_count, letter_count);
            DisplayValue(value);
        }
        catch
        {
            // log exception
        }
    }
    private void RadixValueUpLabel_Click(object sender, EventArgs e)
    {
        try
        {
            // get values in current radix
            int verse_count = (int)Radix.Decode(VersesTextBox.Text, m_radix);
            int word_count = (int)Radix.Decode(WordsTextBox.Text, m_radix);
            int letter_count = (int)Radix.Decode(LettersTextBox.Text, m_radix);
            long value = Radix.Decode(ValueTextBox.Text, m_radix);

            // increment radix
            m_radix++;
            if (m_radix > 36) m_radix = 2;
            RadixValueLabel.Text = m_radix.ToString();

            // display values in nex radix
            DisplayVersesWordsLetters(verse_count, word_count, letter_count);
            DisplayValue(value);
        }
        catch
        {
            // log exception
        }
    }
    private void RadixValueDownLabel_Click(object sender, EventArgs e)
    {
        try
        {
            // get values in current radix
            int verse_count = (int)Radix.Decode(VersesTextBox.Text, m_radix);
            int word_count = (int)Radix.Decode(WordsTextBox.Text, m_radix);
            int letter_count = (int)Radix.Decode(LettersTextBox.Text, m_radix);
            long value = Radix.Decode(ValueTextBox.Text, m_radix);

            // increment radix
            m_radix--;
            if (m_radix < 2) m_radix = 36;
            RadixValueLabel.Text = m_radix.ToString();

            // display values in nex radix
            DisplayVersesWordsLetters(verse_count, word_count, letter_count);
            DisplayValue(value);
        }
        catch
        {
            // log exception
        }
    }
    private void DisplayValue(long value)
    {
        ValueTextBox.Text = Radix.Encode(value, m_radix);
        DecimalValueTextBox.Text = value.ToString();
        ToolTip.SetToolTip(ValueTextBox, value.ToString());
        ValueTextBox.ForeColor = GetNumberTypeColor(ValueTextBox.Text, m_radix);
        ValueTextBox.SelectionStart = ValueTextBox.Text.Length;
        ValueTextBox.SelectionLength = 0;
        ValueTextBox.Refresh();
        DecimalValueTextBox.Visible = (m_radix != DEFAULT_RADIX);
        DecimalValueTextBox.ForeColor = GetNumberTypeColor(value);
        DecimalValueTextBox.Refresh();

        DigitSumTextBox.Text = Numbers.DigitSum(ValueTextBox.Text).ToString();
        DigitSumTextBox.ForeColor = GetNumberTypeColor(DigitSumTextBox.Text, m_radix);
        DigitSumTextBox.Refresh();

        DigitalRootTextBox.Text = Numbers.DigitalRoot(ValueTextBox.Text).ToString();
        DigitalRootTextBox.ForeColor = GetNumberTypeColor(DigitalRootTextBox.Text, m_radix);
        DigitalRootTextBox.Refresh();
    }
    private void DisplayVersesWordsLetters(int verse_count, int word_count, int letter_count)
    {
        VersesTextBox.Text = Radix.Encode(verse_count, m_radix);
        VersesTextBox.ForeColor = GetNumberTypeColor(VersesTextBox.Text, m_radix);
        ToolTip.SetToolTip(VersesTextBox, verse_count.ToString());
        VersesTextBox.Refresh();
        DecimalVersesTextBox.Text = verse_count.ToString();
        DecimalVersesTextBox.ForeColor = GetNumberTypeColor(verse_count);
        DecimalVersesTextBox.Visible = (m_radix != DEFAULT_RADIX);
        VersesTextBox.Refresh();

        WordsTextBox.Text = Radix.Encode(word_count, m_radix);
        WordsTextBox.ForeColor = GetNumberTypeColor(WordsTextBox.Text, m_radix);
        ToolTip.SetToolTip(WordsTextBox, word_count.ToString());
        WordsTextBox.Refresh();
        DecimalWordsTextBox.Text = word_count.ToString();
        DecimalWordsTextBox.ForeColor = GetNumberTypeColor(word_count);
        DecimalWordsTextBox.Visible = (m_radix != DEFAULT_RADIX);
        DecimalWordsTextBox.Refresh();

        LettersTextBox.Text = Radix.Encode(letter_count, m_radix);
        LettersTextBox.ForeColor = GetNumberTypeColor(LettersTextBox.Text, m_radix);
        ToolTip.SetToolTip(LettersTextBox, letter_count.ToString());
        LettersTextBox.Refresh();
        DecimalLettersTextBox.Text = letter_count.ToString();
        DecimalLettersTextBox.ForeColor = GetNumberTypeColor(letter_count);
        DecimalLettersTextBox.Visible = (m_radix != DEFAULT_RADIX);
        DecimalLettersTextBox.Refresh();
    }

    private void CalculateCurrentText()
    {
        if (m_is_selection_mode)
        {
            m_current_text = MainTextBox.Text;
        }
        else
        {
            if (MainTextBox.SelectedText.Length == 0) // get text at current line
            {
                if ((Verse.IncludeEndLine) && (!Verse.IncludeNumber)) // no F11 WordWrap
                {
                    int line_index = GetCurrentLineIndex();
                    if (MainTextBox.Lines.Length > line_index)
                    {
                        m_current_text = MainTextBox.Lines[line_index];
                    }
                }
                else // F11 WordWrap
                {
                    m_current_text = MainTextBox.Text;
                }
            }
            else // get current text
            {
                m_current_text = MainTextBox.SelectedText;
            }
        }
        m_current_text = RemoveVerseAddresses(m_current_text);
        m_current_text = RemoveVerseEndMarks(m_current_text);
        m_current_text = m_current_text.Trim();
    }
    private void CalculateCurrentValue()
    {
        if (m_client != null)
        {
            if (m_client.Selection != null)
            {
                if (m_is_selection_mode)
                {
                    if (m_found_verses_displayed)
                    {
                        CalculateValueAndDisplayFactors(m_client.FoundVerses);
                    }
                    else
                    {
                        List<Verse> verses = m_client.Selection.Verses;
                        if (verses != null)
                        {
                            CalculateValueAndDisplayFactors(verses);
                        }
                    }
                }
                else // no text is highlighted so take complete line, or some text is selected
                {
                    if (m_client.IsTranslation)
                    {
                        string translation_text = "";
                        if (MainTextBox.SelectedText.Length == 0) // no text is selected
                        {
                            int line_index = GetCurrentLineIndex();
                            if (MainTextBox.Lines.Length > line_index)
                            {
                                translation_text = MainTextBox.Lines[line_index];
                            }
                        }
                        else // some text is highlighted
                        {
                            translation_text = MainTextBox.SelectedText;
                        }
                        CalculateValueAndDisplayFactors(translation_text);
                        m_current_text = translation_text;
                    }
                    else // if Arabic
                    {
                        if (MainTextBox.SelectedText.Length == 0) // no text is selected
                        {
                            if (MainTextBox.ReadOnly)
                            {
                                if ((Verse.IncludeEndLine) && (!Verse.IncludeNumber)) // no F11 WordWrap
                                {
                                    int line_index = GetCurrentLineIndex();
                                    if (MainTextBox.Lines.Length > line_index)
                                    {
                                        Verse current_verse = GetVerseAt(line_index);
                                        if (current_verse != null)
                                        {
                                            CalculateValueAndDisplayFactors(current_verse);
                                        }
                                    }
                                }
                                else // F11 WordWrap
                                {
                                    string user_text = m_current_text;
                                    CalculateValueAndDisplayFactors(user_text);
                                }
                            }
                            else // edit mode so user can paste any text they like to calculate its value
                            {
                                string user_text = m_current_text;
                                CalculateValueAndDisplayFactors(user_text);
                            }
                        }
                        else // some text is selected
                        {
                            if (MainTextBox.ReadOnly)
                            {
                                if ((Verse.IncludeEndLine) && (!Verse.IncludeNumber)) // no F11 WordWrap
                                {
                                    int char_index = MainTextBox.SelectionStart;
                                    int line_index = MainTextBox.GetLineFromCharIndex(char_index);
                                    int column_index = CalculateColumnIndex(char_index, line_index);
                                    if (MainTextBox.Lines.Length > line_index)
                                    {
                                        List<Verse> highlighted_verses = new List<Verse>();

                                        Verse first_verse = GetVerseAt(line_index);
                                        if (first_verse != null)
                                        {
                                            highlighted_verses.Add(first_verse);
                                            int first_verse_letter_number = CalculateLetterIndex(first_verse, column_index) + 1;

                                            Verse last_verse = first_verse;
                                            int last_verse_letter_number = first_verse_letter_number - 1; // -1 points to last letter in highlighted_text, not cursor letter
                                            for (int i = 0; i < m_current_text.Length; i++)
                                            {
                                                // if ' ', word_index++, last_verse_letter_number=0
                                                // if '\n', highlighted_verses.Add(verse), verse_index++, word_index=0, last_verse_letter_number=0
                                                if (m_current_text[i] == ' ')
                                                {
                                                    // skip ' '
                                                }
                                                else if ((m_current_text[i] == '\n') && (i < m_current_text.Length - 1)) // \n is not last char, there is another verse
                                                {
                                                    line_index++;
                                                    last_verse = GetVerseAt(line_index);
                                                    if (last_verse != null)
                                                    {
                                                        highlighted_verses.Add(last_verse);
                                                    }
                                                    last_verse_letter_number = 0;
                                                }
                                                else if ((m_current_text[i] == '\n') && (i == m_current_text.Length - 1)) // \n is last char, there is not other verse
                                                {
                                                    // skip \n
                                                }
                                                else
                                                {
                                                    last_verse_letter_number++;
                                                }
                                            }

                                            CalculateValueAndDisplayFactors(highlighted_verses, first_verse_letter_number, last_verse_letter_number);
                                        }
                                    }
                                }
                                else // F11 WordWrap
                                {
                                    string user_text = m_current_text;
                                    CalculateValueAndDisplayFactors(user_text);
                                }
                            }
                            else // edit mode so user can paste any text they like to calculate its value
                            {
                                string user_text = m_current_text;
                                CalculateValueAndDisplayFactors(user_text);
                            }
                        }
                    }
                }
            }
        }
    }
    private long CalculateValue(string user_text)
    {
        long result = 0;
        if (m_client != null)
        {
            if (
                  (LetterOrderSystemComboBox.SelectedIndex != -1) &&
                  (LetterValueSystemComboBox.SelectedIndex != -1)
               )
            {
                result = m_client.CalculateValue(user_text);
            }
        }
        return result;
    }
    private void CalculateValueAndDisplayFactors(string user_text)
    {
        if (m_client != null)
        {
            long value = CalculateValue(user_text);
            FactorizeValue(value, "Value");
            UpdateVerseNavigator(value);
        }
    }

    private void CalculateValueAndDisplayFactors(Verse verse)
    {
        if (m_client != null)
        {
            long value = 0;
            if (
                  (LetterOrderSystemComboBox.SelectedIndex != -1) &&
                  (LetterValueSystemComboBox.SelectedIndex != -1)
               )
            {
                value = m_client.CalculateValue(verse);
            }
            FactorizeValue(value, "Value");
            UpdateVerseNavigator(value);
        }
    }
    private void CalculateValueAndDisplayFactors(List<Verse> verses)
    {
        if (m_client != null)
        {
            long value = 0;
            if (
                  (LetterOrderSystemComboBox.SelectedIndex >= 0) &&
                  (LetterValueSystemComboBox.SelectedIndex >= 0)
               )
            {
                value = m_client.CalculateValue(verses);
            }
            FactorizeValue(value, "Value");
            UpdateVerseNavigator(value);
        }
    }
    private void CalculateValueAndDisplayFactors(Chapter chapter)
    {
        if (m_client != null)
        {
            long value = 0;
            if (
                  (LetterOrderSystemComboBox.SelectedIndex >= 0) &&
                  (LetterValueSystemComboBox.SelectedIndex >= 0)
               )
            {
                value = m_client.CalculateValue(chapter);
            }
            FactorizeValue(value, "Value");
            UpdateVerseNavigator(value);
        }
    }
    private void CalculateValueAndDisplayFactors(List<Verse> verses, int first_verse_letter_number, int last_verse_letter_number)
    {
        if (m_client != null)
        {
            long value = 0;
            if (
                  (LetterOrderSystemComboBox.SelectedIndex != -1) &&
                  (LetterValueSystemComboBox.SelectedIndex != -1)
               )
            {
                value = m_client.CalculateValue(verses, first_verse_letter_number, last_verse_letter_number);
            }
            FactorizeValue(value, "Value");
            UpdateVerseNavigator(value);
        }
    }
    private void UpdateVerseNavigator(long value)
    {
        if (m_client != null)
        {
            if (Book.Instance != null)
            {
                Verse verse = Book.Instance.GetVerseByVerseNumber(value);
                if (verse != null)
                {
                    VerseByVerseNumberLabel.Text = verse.Address;
                }
                else
                {
                    VerseByVerseNumberLabel.Text = "---:---";
                }

                verse = Book.Instance.GetVerseByWordNumber(value);
                if (verse != null)
                {
                    VerseByWordNumberLabel.Text = verse.Address;
                }
                else
                {
                    VerseByWordNumberLabel.Text = "---:---";
                }

                verse = Book.Instance.GetVerseByLetterNumber(value);
                if (verse != null)
                {
                    VerseByLetterNumberLabel.Text = verse.Address;
                }
                else
                {
                    VerseByLetterNumberLabel.Text = "---:---";
                }
            }
        }
    }
    private void ValueTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (ModifierKeys == Keys.Control)
        {
            if (e.KeyCode == Keys.A)
            {
                if (sender is TextBoxBase)
                {
                    (sender as TextBoxBase).SelectAll();
                }
            }
        }
        else if (e.KeyCode == Keys.Enter)
        {
            try
            {
                long value = 0L;
                if (long.TryParse(ValueTextBox.Text, out value))
                {
                    // do nothing
                }
                else // if expression
                {
                    try
                    {
                        // evaluate expression and display result and continue
                        string result = CalculateExpression(ValueTextBox.Text);
                        this.Text = result;

                        value = (long)Math.Round((double.Parse(result)));
                        DisplayValue(value);
                    }
                    catch
                    {
                        return;
                    }
                }

                FactorizeValue(value, "Expression");
                UpdateVerseNavigator(value);
            }
            catch
            {
                // log exception
            }
        }
        else
        {
            ValueTextBox.ForeColor = Color.DarkGray;
        }
    }
    private void FactorizeValue(long value, string caption)
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            ValueLabel.Text = caption;
            ValueLabel.Refresh();

            DisplayValue(value);

            PrimeFactorsTextBox.Text = Numbers.FactorizeToString(value);
            PrimeFactorsTextBox.Refresh();

            int nth_prime_index = -1;
            int nth_additive_prime_index = -1;
            int nth_pure_prime_index = -1;
            NthPrimeTextBox.ForeColor = GetNumberTypeColor(nth_prime_index);
            NthPrimeTextBox.Text = nth_prime_index.ToString();
            NthPrimeTextBox.Refresh();
            NthAdditivePrimeTextBox.ForeColor = GetNumberTypeColor(nth_additive_prime_index);
            NthAdditivePrimeTextBox.Text = nth_additive_prime_index.ToString();
            NthAdditivePrimeTextBox.Refresh();
            NthPurePrimeTextBox.ForeColor = GetNumberTypeColor(nth_pure_prime_index);
            NthPurePrimeTextBox.Text = nth_pure_prime_index.ToString();
            NthPurePrimeTextBox.Refresh();

            if (value < 0)
            {
                nth_prime_index = -1;
                nth_additive_prime_index = -1;
                nth_pure_prime_index = -1;
                NthPrimeLabel.Text = "P Index";
                NthPrimeLabel.Refresh();
                NthAdditivePrimeLabel.Text = "AP Index";
                NthAdditivePrimeLabel.Refresh();
                NthPurePrimeLabel.Text = "PP Index";
                NthPurePrimeLabel.Refresh();
            }
            else if (value == 0)
            {
                nth_prime_index = 0;
                nth_additive_prime_index = 0;
                nth_pure_prime_index = 0;
                NthPrimeLabel.Text = "C Index";
                NthPrimeLabel.Refresh();
                NthAdditivePrimeLabel.Text = "AC Index";
                NthAdditivePrimeLabel.Refresh();
                NthPurePrimeLabel.Text = "PC Index";
                NthPurePrimeLabel.Refresh();
            }
            else if (value == 1)
            {
                nth_prime_index = 0;
                nth_additive_prime_index = 0;
                nth_pure_prime_index = 0;
                NthPrimeLabel.Text = "P Index";
                NthPrimeLabel.Refresh();
                NthAdditivePrimeLabel.Text = "AP Index";
                NthAdditivePrimeLabel.Refresh();
                NthPurePrimeLabel.Text = "PP Index";
                NthPurePrimeLabel.Refresh();
            }
            else if (Numbers.IsPrime(value))
            {
                nth_prime_index = Numbers.IndexOfPrime(value) + (Numbers.IsOnePrime ? 0 : 1);
                nth_additive_prime_index = Numbers.IndexOfAdditivePrime(value) + (Numbers.IsOnePrime ? 0 : 1);
                nth_pure_prime_index = Numbers.IndexOfPurePrime(value) + (Numbers.IsOnePrime ? 0 : 1);
                NthPrimeLabel.Text = "P Index";
                NthPrimeLabel.Refresh();
                NthAdditivePrimeLabel.Text = "AP Index";
                NthAdditivePrimeLabel.Refresh();
                NthPurePrimeLabel.Text = "PP Index";
                NthPurePrimeLabel.Refresh();
            }
            else
            {
                nth_prime_index = Numbers.IndexOfComposite(value) + 1;
                if (nth_prime_index == 0)
                {
                    nth_prime_index = -1;
                }
                nth_additive_prime_index = Numbers.IndexOfAdditiveComposite(value) + 1;
                if (nth_additive_prime_index == 0)
                {
                    nth_additive_prime_index = -1;
                }
                nth_pure_prime_index = Numbers.IndexOfPureComposite(value) + 1;
                if (nth_pure_prime_index == 0)
                {
                    nth_pure_prime_index = -1;
                }
                NthPrimeLabel.Text = "C Index";
                NthPrimeLabel.Refresh();
                NthAdditivePrimeLabel.Text = "AC Index";
                NthAdditivePrimeLabel.Refresh();
                NthPurePrimeLabel.Text = "PC Index";
                NthPurePrimeLabel.Refresh();
            }

            NthPrimeTextBox.ForeColor = GetNumberTypeColor(nth_prime_index);
            NthPrimeTextBox.Text = nth_prime_index.ToString();
            NthPrimeTextBox.Refresh();

            NthAdditivePrimeTextBox.ForeColor = GetNumberTypeColor(nth_additive_prime_index);
            NthAdditivePrimeTextBox.Text = nth_additive_prime_index.ToString();
            NthAdditivePrimeTextBox.Refresh();

            NthPurePrimeTextBox.ForeColor = GetNumberTypeColor(nth_pure_prime_index);
            NthPurePrimeTextBox.Text = nth_pure_prime_index.ToString();
            NthPurePrimeTextBox.Refresh();


            // update graphs as we move between selections
            if (DrawingPictureBoxEx.Visible)
            {
                RedrawCurrentGraph();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }
    private string CalculateExpression(string expression)
    {
        try
        {
            string result = Calculator.Evaluate(expression);
            return String.Format(CultureInfo.InvariantCulture, "{0}", result);
        }
        catch
        {
            return expression;
        }
    }

    private void NthPrimeTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            this.Cursor = Cursors.WaitCursor;

            try
            {
                long number = -1;
                int nth_index = int.Parse(NthPrimeTextBox.Text);
                if (nth_index < 0)
                {
                    number = -1;
                }
                else
                {
                    NthPrimeTextBox.ForeColor = GetNumberTypeColor(nth_index);
                    if (NthPrimeLabel.Text == "P Index")
                    {
                        number = Numbers.Primes[nth_index];
                    }
                    else //if (NthPrimeLabel.Text == "C Index")
                    {
                        if (nth_index == 0)
                        {
                            number = 0;
                        }
                        else
                        {
                            number = Numbers.Composites[nth_index - 1];
                        }
                    }
                }
                DisplayValue(number);
                FactorizeValue(number, NthPrimeLabel.Text);
                UpdateVerseNavigator(number);
            }
            catch
            {
                //MessageBox.Show(ex.Message, Application.ProductName);
                DisplayValue(0);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
        else
        {
            //NthPrimeTextBox.ForeColor = Color.DarkGray;
        }
    }
    private void NthAdditivePrimeTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            this.Cursor = Cursors.WaitCursor;

            try
            {
                long number = -1;
                int nth_index = int.Parse(NthAdditivePrimeTextBox.Text);
                if (nth_index < 0)
                {
                    number = -1;
                }
                else
                {
                    NthAdditivePrimeTextBox.ForeColor = GetNumberTypeColor(nth_index);
                    if (NthAdditivePrimeLabel.Text == "AP Index")
                    {
                        number = Numbers.AdditivePrimes[nth_index];
                    }
                    else //if (NthAdditivePrimeLabel.Text == "AC Index")
                    {
                        if (nth_index == 0)
                        {
                            number = 0;
                        }
                        else
                        {
                            number = Numbers.AdditiveComposites[nth_index - 1];
                        }
                    }
                }
                DisplayValue(number);
                FactorizeValue(number, NthAdditivePrimeLabel.Text);
                UpdateVerseNavigator(number);
            }
            catch
            {
                //MessageBox.Show(ex.Message, Application.ProductName);
                DisplayValue(0);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
        else
        {
            //NthAdditivePrimeTextBox.ForeColor = Color.DarkGray;
        }
    }
    private void NthPurePrimeTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            this.Cursor = Cursors.WaitCursor;

            try
            {
                long number = -1;
                int nth_index = int.Parse(NthPurePrimeTextBox.Text);
                if (nth_index < 0)
                {
                    number = -1;
                }
                else
                {
                    NthPurePrimeTextBox.ForeColor = GetNumberTypeColor(nth_index);
                    if (NthPurePrimeLabel.Text == "PP Index")
                    {
                        number = Numbers.PurePrimes[nth_index];
                    }
                    else //if (NthPurePrimeLabel.Text == "PC Index")
                    {
                        if (nth_index == 0)
                        {
                            number = 0;
                        }
                        else
                        {
                            number = Numbers.PureComposites[nth_index - 1];
                        }
                    }
                }
                DisplayValue(number);
                FactorizeValue(number, NthPurePrimeLabel.Text);
                UpdateVerseNavigator(number);
            }
            catch
            {
                //MessageBox.Show(ex.Message, Application.ProductName);
                DisplayValue(0);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
        else
        {
            //NthPurePrimeTextBox.ForeColor = Color.DarkGray;
        }
    }
    private Color GetNumberTypeColor(long number)
    {
        return GetNumberTypeColor(number.ToString(), 10);
    }
    private Color GetNumberTypeColor(string value, long radix)
    {
        if (Numbers.IsPurePrime(value, radix))
        {
            return Color.DarkViolet;
        }
        else if (Numbers.IsAdditivePrime(value, radix))
        {
            return Color.Blue;
        }
        else if (Numbers.IsPrime(value, radix))
        {
            return Color.Green;
        }
        else if (Numbers.IsPureComposite(value, radix))
        {
            return Color.Red;
        }
        else if (Numbers.IsAdditiveComposite(value, radix))
        {
            return Color.Brown;
        }
        else if (Numbers.IsComposite(value, radix))
        {
            return Color.Black;
        }
        else
        {
            return Color.Black;
        }
    }

    private void DisplayLetterStatistics()
    {
        if (m_client != null)
        {
            LetterCountValueLabel.Text = 0.ToString();
            LetterCountValueLabel.ForeColor = GetNumberTypeColor(0);
            LetterCountValueLabel.Refresh();
            LetterFrequencySumValueLabel.Text = 0.ToString();
            LetterFrequencySumValueLabel.ForeColor = GetNumberTypeColor(0);
            LetterFrequencySumValueLabel.Refresh();

            int count = m_client.LetterStatistics.Count;
            int frequency_sum = 0;
            LetterStatisticsListView.Items.Clear();
            for (int i = 0; i < count; i++)
            {
                string[] item_parts = new string[3];
                item_parts[0] = m_client.LetterStatistics[i].Order.ToString();
                item_parts[1] = m_client.LetterStatistics[i].Letter.ToString();
                item_parts[2] = m_client.LetterStatistics[i].Frequency.ToString();
                LetterStatisticsListView.Items.Add(new ListViewItem(item_parts, i));
                frequency_sum += m_client.LetterStatistics[i].Frequency;
            }
            LetterCountValueLabel.Text = count.ToString();
            LetterCountValueLabel.ForeColor = GetNumberTypeColor(count);
            LetterFrequencySumValueLabel.Text = frequency_sum.ToString();
            LetterFrequencySumValueLabel.ForeColor = GetNumberTypeColor(frequency_sum);

            // reset sort-markers
            foreach (ColumnHeader column in LetterStatisticsListView.Columns)
            {
                if (column.Text.EndsWith("▲"))
                {
                    column.Text = column.Text.Replace("▲", " ");
                }
                else if (column.Text.EndsWith("▼"))
                {
                    column.Text = column.Text.Replace("▼", " ");
                }
            }
            LetterStatisticsListView.Columns[0].Text = LetterStatisticsListView.Columns[0].Text.Replace("  ", " ▲");

            ListView_SizeChanged(LetterStatisticsListView, null);
        }
    }
    private void DisplayPhraseLetterStatistics()
    {
        if (m_client != null)
        {
            FindByFrequencyListView.Items.Clear();
            for (int i = 0; i < m_client.PhraseLetterStatistics.Count; i++)
            {
                string[] item_parts = new string[3];
                item_parts[0] = m_client.PhraseLetterStatistics[i].Order.ToString();
                item_parts[1] = m_client.PhraseLetterStatistics[i].Letter.ToString();
                item_parts[2] = m_client.PhraseLetterStatistics[i].Frequency.ToString();
                FindByFrequencyListView.Items.Add(new ListViewItem(item_parts, i));
            }

            // reset sort-markers
            foreach (ColumnHeader column in FindByFrequencyListView.Columns)
            {
                if (column.Text.EndsWith("▲"))
                {
                    column.Text = column.Text.Replace("▲", " ");
                }
                else if (column.Text.EndsWith("▼"))
                {
                    column.Text = column.Text.Replace("▼", " ");
                }
            }
            FindByFrequencyListView.Columns[0].Text = FindByFrequencyListView.Columns[0].Text.Replace("  ", " ▲");

            ListView_SizeChanged(FindByFrequencyListView, null);
        }
    }
    private void LetterFrequencySumLabel_TextChanged(object sender, EventArgs e)
    {
        Control control = (sender as Control);
        if (control != null)
        {
            control.Width = (control.Text.Length <= 4) ? 48 : 64;
        }
    }
    private void LetterFrequencySumLabel_Click(object sender, EventArgs e)
    {
        if (ModifierKeys == Keys.Control)
        {
            Control control = (sender as Control);
            if (control != null)
            {
                if (!string.IsNullOrEmpty(control.Text))
                {
                    try
                    {
                        long value = long.Parse(control.Text);
                        FactorizeValue(value, "Frequency");
                    }
                    catch
                    {
                        // log exception
                    }
                }
            }
        }
    }
    private void ListView_SizeChanged(object sender, EventArgs e)
    {
    }
    private void ListView_ColumnClick(object sender, ColumnClickEventArgs e)
    {
        if (sender is ListView)
        {
            ListView listview = sender as ListView;
            try
            {
                if (m_client != null)
                {
                    if (listview == LetterStatisticsListView)
                    {
                        m_client.SortLetterStatistics((StatisticSortMethod)e.Column);
                        DisplayLetterStatistics();
                    }
                    else if (listview == FindByFrequencyListView)
                    {
                        m_client.SortPhraseLetterStatistics((StatisticSortMethod)e.Column);
                        DisplayPhraseLetterStatistics();
                    }
                    else
                    {
                        // do nothing
                    }

                    // display sort marker
                    string sort_marker = (LetterStatistic.SortOrder == StatisticSortOrder.Ascending) ? "▼" : "▲";
                    // empty all sort markers
                    foreach (ColumnHeader column in listview.Columns)
                    {
                        if (column.Text.EndsWith("▲"))
                        {
                            column.Text = column.Text.Replace("▲", " ");
                        }
                        else if (column.Text.EndsWith("▼"))
                        {
                            column.Text = column.Text.Replace("▼", " ");
                        }
                    }
                    // mark clicked column
                    listview.Columns[e.Column].Text = listview.Columns[e.Column].Text.Replace("  ", " " + sort_marker);
                }
            }
            catch
            {
                // log exception
            }
        }
    }
    private void ListView_MouseDown(object sender, MouseEventArgs e)
    {
        if (ModifierKeys == Keys.Control)
        {
            ListView listview = (sender as ListView);
            if (listview != null)
            {
                if (listview.Items.Count > 1)
                {
                    //col_height is the height of the listview column header
                    int col_height = listview.Items[1].Bounds.Height + 6;

                    //items_height and items_width are used for checking if the mouse click was outside all items.
                    int items_height = 0;
                    for (int item_count = 0; item_count < listview.Items.Count; item_count++)
                    {
                        items_height += listview.Items[item_count].Bounds.Height;
                    }
                    items_height += col_height;

                    int items_width = 0;
                    for (int col_count = 0; col_count < listview.Columns.Count; col_count++)
                    {
                        items_width += listview.Columns[col_count].Width + 1;
                    }

                    //if the mouse click was outside all items and columns, just return.
                    if (e.X >= items_width || e.Y >= items_height)
                    {
                        return;
                    }

                    //i and j are the index of the subitem.
                    int i = 0, j = 0;
                    int width_len = listview.Columns[0].Width;
                    int height_len = col_height + listview.Items[0].Bounds.Height;
                    while (e.X > width_len)
                    {
                        i++;
                        width_len += listview.Columns[i].Width + 1;
                    }

                    while (e.Y > height_len)
                    {
                        j++;
                        height_len += listview.Items[j].Bounds.Height;
                    }

                    if (i == 2)
                    {
                        try
                        {
                            long value = long.Parse(listview.Items[j + listview.TopItem.Index].SubItems[i].Text);
                            FactorizeValue(value, "Frequency");
                        }
                        catch
                        {
                            // log exception
                        }
                    }
                }
            }
        }
    }

    private void ResetNumerologySystemButton_Click(object sender, EventArgs e)
    {
        ResetNumerologySystem();
    }

    private void SaveLetterStatisticsButton_Click(object sender, EventArgs e)
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            SaveLetterStatistics();
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }
    private void SaveLetterValuationButton_Click(object sender, EventArgs e)
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            SaveLetterValuation();
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }
    private void SaveLetterValuation()
    {
        string filename = "";
        string text = "";
        PrepareFilenameAndTextToSave(ref filename, ref text);
        if (m_client != null)
        {
            m_client.SaveLetterValuation(filename, text);
        }
    }
    private void SaveLetterStatistics()
    {
        string filename = "";
        string text = "";
        PrepareFilenameAndTextToSave(ref filename, ref text);
        if (m_client != null)
        {
            m_client.SaveLetterStatistics(filename, text);
        }
    }
    private void SavePhraseLetterStatistics()
    {
        string filename = "";
        string text = "";
        string phrase = FindByFrequencyPhraseTextBox.Text;
        PrepareFilenameAndTextToSave(ref filename, ref text);
        if ((text.Length > 0) && (phrase.Length > 0))
        {
            phrase = phrase.Replace("\n", "\r\n");
            phrase += "\r\n";
            if (m_client != null)
            {
                m_client.SavePhraseLetterStatistics(filename, text, phrase);
            }
        }
    }
    private void PrepareFilenameAndTextToSave(ref string filename, ref string text)
    {
        if (
              (TextModeComboBox.SelectedItem != null) &&
              (LetterOrderSystemComboBox.SelectedItem != null) &&
              (LetterValueSystemComboBox.SelectedItem != null)
           )
        {
            filename =
               DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss") + "_"
             + TextModeComboBox.SelectedItem.ToString() + "_"
             + LetterOrderSystemComboBox.SelectedItem.ToString() + "_"
             + LetterValueSystemComboBox.SelectedItem.ToString() + ".txt";

            text += m_current_text;
            text = text.Replace("\n", "\r\n");
            text += "\r\n";
            text += "----------------------------------------";
            text += "\r\n";
            text += "Verses\t= " + VersesTextBox.Text + "\r\n";
            text += "Words\t= " + WordsTextBox.Text + "\r\n";
            text += "Letters\t= " + LettersTextBox.Text + "\r\n";
            text += "Value\t= " + ValueTextBox.Text;
        }
    }

    private void FindScopeLabel_Click(object sender, EventArgs e)
    {
        UpdateFindScope();
        FindByTextTextBox_TextChanged(sender, e);
    }
    private void UpdateFindScope()
    {
        if (m_client != null)
        {
            if (m_client.FindScope == FindScope.Book)
            {
                m_client.FindScope = FindScope.SearchResult;
            }
            else if (m_client.FindScope == FindScope.SearchResult)
            {
                m_client.FindScope = FindScope.Selection;
            }
            else if (m_client.FindScope == FindScope.Selection)
            {
                m_client.FindScope = FindScope.Book;
            }
            else
            {
                //
            }
            RefreshFindScopeLabel();
        }
    }
    private void RefreshFindScopeLabel()
    {
        if (m_client != null)
        {
            if (m_client.Selection != null)
            {
                if (m_client.FindScope == FindScope.Book)
                {
                    FindScopeLabel.Text = "Search in Entire Book";
                    FindScopeLabel.BackColor = Color.SteelBlue;
                }
                else if (m_client.FindScope == FindScope.SearchResult)
                {
                    FindScopeLabel.Text = "Search in Result";
                    FindScopeLabel.BackColor = Color.SteelBlue;
                }
                else if (m_client.FindScope == FindScope.Selection)
                {
                    if (m_client.Selection.Scope == SelectionScope.Book)
                    {
                        m_client.FindScope = FindScope.Book;
                        FindScopeLabel.Text = "Search in Entire Book";
                        FindScopeLabel.BackColor = Color.SteelBlue;
                    }
                    else
                    {
                        m_client.FindScope = FindScope.Selection;
                        //FindScopeLabel.Text = "Search in Selection";
                        FindScopeLabel.Text = "Search in " + GetFindScope();
                        FindScopeLabel.BackColor = Color.SteelBlue;
                    }
                }
                else
                {
                    FindScopeLabel.Text = "Search in Entire Book";
                    FindScopeLabel.BackColor = Color.SteelBlue;
                }

                FindScopeLabel.Refresh();
                ToolTip.SetToolTip(FindScopeLabel, FindScopeLabel.Text);
                this.Text = Application.ProductName + " | " + m_client.TextMode + " | " + GetFindScope();
            }
        }
    }
    private string GetFindScope()
    {
        string result = "";
        if (m_client != null)
        {
            if (m_client.Selection != null)
            {
                if ((m_client.Selection.Scope == SelectionScope.Word) || (m_client.Selection.Scope == SelectionScope.Letter))
                {
                    result = "Verse" + " " + VerseNumericUpDown.Value.ToString();
                }
                else // if m_bookmark_scope is Chapter, Page, Station, Part, Group, Quarter, Bowing, Verse
                {
                    StringBuilder str = new StringBuilder();
                    foreach (int index in m_client.Selection.Indexes)
                    {
                        str.Append((index + 1).ToString() + " ");
                    }
                    if (str.Length > 0) // remove last " "
                    {
                        str.Remove(str.Length - 1, 1);
                    }

                    //if str too long, trim it
                    if (str.Length > MAX_SELECTON_SCOPE_LENGTH)
                    {
                        str.Remove(MAX_SELECTON_SCOPE_LENGTH, str.Length - MAX_SELECTON_SCOPE_LENGTH);
                        str.Append(" ...");
                    }

                    if (m_client.Selection.Indexes.Count == 1)
                    {
                        result = m_client.Selection.Scope.ToString() + " " + str.ToString();
                    }
                    else if (m_client.Selection.Indexes.Count > 1)
                    {
                        //result = m_client.Selection.Scope.ToString() + "s" + "\r\n" + str.ToString();
                        result = m_client.Selection.Scope.ToString() + "s" + " " + str.ToString();
                    }
                }
            }
        }
        return result;
    }

    private void AddFindHistoryItem()
    {
        if (m_client != null)
        {
            if (m_client.FoundVerses != null)
            {
                if (m_client.FoundVerses.Count > 0)
                {
                    Client.FindHistoryItem item = new Client.FindHistoryItem();
                    item.FindText = FindByTextTextBox.Text;
                    item.LanguageType = m_find_by_text_language_type;

                    if (TranslatorComboBox.SelectedItem != null)
                    {
                        item.Translator = (m_find_by_text_language_type != FindByTextLanguageType.Arabic) ? TranslatorComboBox.SelectedItem.ToString() : "N/A";
                    }

                    item.Verses = new List<Verse>(m_client.FoundVerses);
                    if (m_client.FoundPhrases == null)
                    {
                        item.Phrases = null;
                    }
                    else
                    {
                        item.Phrases = new List<Phrase>(m_client.FoundPhrases);
                    }
                    item.Header = m_find_result_header;// HeaderLabel.Text;
                    m_client.AddFindHistoryItem(item);
                    UpdateFindHistoryButtons();
                }
            }
        }
    }
    private void DisplayFindHistoryItem(Client.FindHistoryItem item)
    {
        if (m_client != null)
        {
            if (item != null)
            {
                FindByTextTextBox.Text = item.FindText;
                FindByTextTextBox.SelectionStart = item.FindText.Length;

                m_find_result_header = item.Header;

                m_find_by_text_language_type = item.LanguageType;

                TranslatorComboBox.SelectedItem = item.Translator;

                if (item.Phrases == null)
                {
                    m_client.FoundPhrases = null;
                }
                else
                {
                    m_client.FoundPhrases = new List<Phrase>(item.Phrases);
                }

                m_client.FoundVerses = new List<Verse>(item.Verses);
                DisplayFoundVerses(false, false);
            }
        }
    }
    private void FindHistoryBackwardButton_Click(object sender, EventArgs e)
    {
        if (m_client != null)
        {
            m_text_selections.Clear(); // to reset Matched count
            Client.FindHistoryItem item = m_client.GotoPreviousFindHistory();
            DisplayFindHistoryItem(item);
            UpdateFindHistoryButtons();
        }
    }
    private void FindHistoryForwardButton_Click(object sender, EventArgs e)
    {
        if (m_client != null)
        {
            m_text_selections.Clear(); // to reset Matched count
            Client.FindHistoryItem item = m_client.GotoNextFindHistory();
            DisplayFindHistoryItem(item);
            UpdateFindHistoryButtons();
        }
    }
    private void FindHistoryDeleteButton_Click(object sender, EventArgs e)
    {
        if (m_client != null)
        {
            m_client.DeleteCurrentFindHistoryItem();
            if (m_client.FindHistory.Count == 0) // no item to display
            {
                DisplaySelection(sender, false);
                FindByTextTextBox.Text = "";
            }
            else // there is an item to display
            {
                Client.FindHistoryItem item = m_client.CurrentFindHistoryItem;
                if (item != null)
                {
                    DisplayFindHistoryItem(item);
                }
            }
            UpdateFindHistoryButtons();
        }
    }
    private void FindHistoryClearButton_Click(object sender, EventArgs e)
    {
        if (m_client != null)
        {
            m_client.ClearFindHistory();
            DisplaySelection(sender, false);
            FindByTextTextBox.Text = "";
            UpdateFindHistoryButtons();
            AutoCompleteHeaderLabel.Visible = false;
            AutoCompleteListBox.SendToBack();
        }
    }
    private void UpdateFindHistoryButtons()
    {
        if (m_client.FindHistory != null)
        {
            FindHistoryBackwardButton.Enabled = (m_client.FindHistory.Count > 0) && (m_client.FindHistoryIndex > 0);
            FindHistoryForwardButton.Enabled = (m_client.FindHistory.Count >= 0) && (m_client.FindHistoryIndex < m_client.FindHistory.Count - 1);
            FindHistoryDeleteButton.Enabled = (m_client.FindHistory.Count > 0);
            FindHistoryClearButton.Enabled = (m_client.FindHistory.Count > 0);
            FindCounterLabel.Text = (m_client.FindHistoryIndex + 1).ToString() + " / " + m_client.FindHistory.Count.ToString();
        }
    }

    private void AddBrowseHistoryItem()
    {
        if (m_client != null)
        {
            if ((Book.Instance != null) && (m_client.Selection != null))
            {
                Client.BrowseHistoryItem item = new Client.BrowseHistoryItem(m_client.Selection);
                if (item != null)
                {
                    m_client.AddBrowseHistoryItem(item);
                    UpdateBrowseHistoryButtons();
                }
            }
        }
    }
    private void DisplayBrowseHistoryItem(Client.BrowseHistoryItem item)
    {
        if (m_client != null)
        {
            if (item != null)
            {
                m_client.Selection = new Selection(Book.Instance, item.Scope, item.Indexes);
                DisplaySelection(this, false);
            }
        }
    }
    private void BrowseHistoryBackwardButton_Click(object sender, EventArgs e)
    {
        if (m_client != null)
        {
            Client.BrowseHistoryItem item = m_client.GotoPreviousBrowseHistory();
            DisplayBrowseHistoryItem(item);
            RefreshChapterSelection();
            UpdateBrowseHistoryButtons();
        }
    }
    private void BrowseHistoryForwardButton_Click(object sender, EventArgs e)
    {
        if (m_client != null)
        {
            Client.BrowseHistoryItem item = m_client.GotoNextBrowseHistory();
            DisplayBrowseHistoryItem(item);
            RefreshChapterSelection();
            UpdateBrowseHistoryButtons();
        }
    }
    private void BrowseHistoryDeleteButton_Click(object sender, EventArgs e)
    {
        if (m_client != null)
        {
            m_client.DeleteCurrentBrowseHistoryItem();
            if (m_client.BrowseHistory != null)
            {
                if (m_client.BrowseHistory.Count == 0) // no item to display
                {
                    DisplaySelection(sender, false);
                }
                else // there is an item to display
                {
                    Client.BrowseHistoryItem item = m_client.CurrentBrowseHistoryItem;
                    if (item != null)
                    {
                        DisplayBrowseHistoryItem(item);
                    }
                }
            }
            RefreshChapterSelection();
            UpdateBrowseHistoryButtons();
        }
    }
    private void BrowseHistoryClearButton_Click(object sender, EventArgs e)
    {
        if (m_client != null)
        {
            m_client.ClearBrowseHistory();
            DisplaySelection(sender, false);
            RefreshChapterSelection();
            UpdateBrowseHistoryButtons();
        }
    }
    private void UpdateBrowseHistoryButtons()
    {
        if (m_client.BrowseHistory != null)
        {
            BrowseHistoryBackwardButton.Enabled = (m_client.BrowseHistory.Count > 0) && (m_client.BrowseHistoryIndex > 0);
            BrowseHistoryForwardButton.Enabled = (m_client.BrowseHistory.Count >= 0) && (m_client.BrowseHistoryIndex < m_client.BrowseHistory.Count - 1);
            BrowseHistoryDeleteButton.Enabled = (m_client.BrowseHistory.Count > 0);
            BrowseHistoryClearButton.Enabled = (m_client.BrowseHistory.Count > 0);
            SelectionCounterLabel.Text = (m_client.BrowseHistoryIndex + 1).ToString() + " / " + m_client.BrowseHistory.Count.ToString();
            BrowseGroupBox.Text = " Browse [" + SelectionCounterLabel.Text + "]" + "                          ";
        }
    }

    private void DisplayFoundVerses(bool add_to_history, bool update_phrase_positions)
    {
        this.Cursor = Cursors.WaitCursor;
        MainTextBox.BeginUpdate();
        MainTextBox.SelectionChanged -= new EventHandler(MainTextBox_SelectionChanged);
        MainTextBox.SelectionChanged -= new EventHandler(MainTextBox_SelectionChanged);
        MainTextBox.SelectionChanged -= new EventHandler(MainTextBox_SelectionChanged);

        try
        {
            NoteTextBox.Enabled = false;
            if (m_client != null)
            {
                if (m_client.FoundVerses != null)
                {
                    TranslationTextBox.Text = "";
                    DrawingPictureBoxEx.Visible = false;
                    ZoomInLabel.Enabled = (m_text_zoom_factor <= (m_max_zoom_factor - m_zoom_factor_increment + m_error_margin));
                    ZoomOutLabel.Enabled = (m_text_zoom_factor >= (m_min_zoom_factor + m_zoom_factor_increment - m_error_margin));

                    m_current_text = "";
                    if (m_client.FoundVerses.Count > 0)
                    {
                        StringBuilder str = new StringBuilder();
                        foreach (Verse verse in m_client.FoundVerses)
                        {
                            //string verse_address = (m_find_by_text_language_type == FindByTextLanguageType.Arabic) ? verse.ArabicAddress : verse.PaddedAddress;
                            string verse_address = verse.ArabicAddress;
                            if (verse.Chapter != null)
                            {
                                str.Append(verse_address + "\t" + verse.GetText(m_client.TextMode) + "\n");
                            }
                        }
                        if (str.Length > 0)
                        {
                            str.Remove(str.Length - 1, 1);
                        }
                        m_current_text = str.ToString();
                    }

                    MainTextBox.BackColor = Color.AliceBlue;
                    TranslationTextBox.BackColor = Color.AliceBlue;
                    try
                    {
                        MainTextBox.TextChanged -= new EventHandler(MainTextBox_TextChanged);
                        MainTextBox.Text = m_current_text;
                    }
                    finally
                    {
                        MainTextBox.TextChanged += new EventHandler(MainTextBox_TextChanged);
                    }

                    m_found_verses_current_line_index = 0; // 0 to get a valid verse from GetVerse(..)
                    m_is_selection_mode = true;
                    m_found_verses_displayed = true;

                    // re-calculate numerology value
                    CalculateCurrentText();
                    UpdateNumerologySystems();
                    CalculateCurrentValue();

                    DisplaySelectionPositions();
                    DisplayVersesWordsLetters(m_current_text);
                    DisplayLetterFrequencies();
                    DisplayPhraseLetterFrequencies();

                    // if FindByText/Root OR HandleEscape OR BrowseFindHistory
                    if ((m_find_by_text_language_type == FindByTextLanguageType.Arabic) && (m_client.FoundPhrases != null))
                    {
                        ColorizeMatches();
                        BuildTextSelections();
                    }

                    if (!m_client.IsTranslation)
                    {
                        // don't use this because it overwrites the ChapterListBox.SelectedIndex
                        //DisplayCurrentPositions();

                        // use this instead
                        if (m_client.FoundVerses != null)
                        {
                            if (m_client.FoundVerses.Count > 0)
                            {
                                try
                                {
                                    ChapterComboBox.SelectedIndexChanged -= new EventHandler(ChapterComboBox_SelectedIndexChanged);
                                    ChapterComboBox.SelectedIndex = GetChapterIndex(m_client.FoundVerses[0].Chapter);
                                }
                                finally
                                {
                                    ChapterComboBox.SelectedIndexChanged += new EventHandler(ChapterComboBox_SelectedIndexChanged);
                                }
                                UpdateVersePositions(m_client.FoundVerses[0]);
                            }
                        }
                    }
                    DisplayTranslation(m_client.FoundVerses);

                    if ((add_to_history) && (m_client.FoundVerses.Count > 0))
                    {
                        AddFindHistoryItem();
                    }

                    RealignFoundMatchedToStart();

                    UpdateHeaderLabel();
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
        }
        finally
        {
            MainTextBox.SelectionChanged += new EventHandler(MainTextBox_SelectionChanged);
            MainTextBox.EndUpdate();
            this.Cursor = Cursors.Default;
        }
    }
    private void DisplayFoundChapters(bool add_to_history)
    {
        this.Cursor = Cursors.WaitCursor;
        MainTextBox.BeginUpdate();
        MainTextBox.SelectionChanged -= new EventHandler(MainTextBox_SelectionChanged);
        MainTextBox.SelectionChanged -= new EventHandler(MainTextBox_SelectionChanged);
        MainTextBox.SelectionChanged -= new EventHandler(MainTextBox_SelectionChanged);

        try
        {
            NoteTextBox.Enabled = false;
            if (m_client != null)
            {
                if (m_client.FoundChapters != null)
                {
                    TranslationTextBox.Text = "";
                    DrawingPictureBoxEx.Visible = false;
                    ZoomInLabel.Enabled = (m_text_zoom_factor <= (m_max_zoom_factor - m_zoom_factor_increment + m_error_margin));
                    ZoomOutLabel.Enabled = (m_text_zoom_factor >= (m_min_zoom_factor + m_zoom_factor_increment - m_error_margin));

                    ChaptersListBox.SelectedIndexChanged -= new EventHandler(ChaptersListBox_SelectedIndexChanged);
                    if (m_client.FoundChapters.Count > 0)
                    {
                        ChaptersListBox.SelectedIndices.Clear();
                        foreach (Chapter chapter in m_client.FoundChapters)
                        {
                            if (((chapter.Number - 1) >= 0) && ((chapter.Number - 1) < ChaptersListBox.Items.Count))
                            {
                                ChaptersListBox.SelectedIndices.Add(chapter.Number - 1);
                            }
                        }
                    }
                    else
                    {
                        ChaptersListBox.SelectedIndices.Clear();
                    }
                    ChaptersListBox.SelectedIndexChanged += new EventHandler(ChaptersListBox_SelectedIndexChanged);

                    m_current_text = "";
                    if (m_client.FoundChapters.Count > 0)
                    {
                        StringBuilder str = new StringBuilder();
                        foreach (Chapter chapter in m_client.FoundChapters)
                        {
                            foreach (Verse verse in chapter.Verses)
                            {
                                //string verse_address = (m_find_by_text_language_type == FindByTextLanguageType.Arabic) ? verse.ArabicAddress : verse.PaddedAddress;
                                string verse_address = verse.ArabicAddress;
                                if (verse.Chapter != null)
                                {
                                    str.Append(verse_address + "\t" + verse.GetText(m_client.TextMode) + "\n");
                                }
                            }
                        }
                        if (str.Length > 0)
                        {
                            str.Remove(str.Length - 1, 1);
                        }
                        m_current_text = str.ToString();
                    }

                    MainTextBox.BackColor = Color.AliceBlue;
                    TranslationTextBox.BackColor = Color.AliceBlue;
                    MainTextBox.TextChanged -= new EventHandler(MainTextBox_TextChanged);
                    MainTextBox.Text = m_current_text;
                    MainTextBox.TextChanged += new EventHandler(MainTextBox_TextChanged);

                    m_found_verses_current_line_index = 0; // 0 to get a valid verse from GetVerse(..)
                    m_is_selection_mode = true;
                    m_found_verses_displayed = true;

                    // re-calculate numerology value
                    CalculateCurrentText();
                    UpdateNumerologySystems();
                    CalculateCurrentValue();

                    DisplayVersesWordsLetters(m_current_text);
                    DisplayLetterFrequencies();
                    DisplayPhraseLetterFrequencies();

                    if (!m_client.IsTranslation)
                    {
                        // don't use this because it overwrites the ChapterListBox.SelectedIndex
                        //DisplayCurrentPositions();

                        // use this instead
                        if (m_client.FoundVerses != null)
                        {
                            if (m_client.FoundVerses.Count > 0)
                            {
                                ChapterComboBox.SelectedIndexChanged -= new EventHandler(ChapterComboBox_SelectedIndexChanged);
                                ChapterComboBox.SelectedIndex = GetChapterIndex(m_client.FoundVerses[0].Chapter);
                                ChapterComboBox.SelectedIndexChanged += new EventHandler(ChapterComboBox_SelectedIndexChanged);

                                UpdateVersePositions(m_client.FoundVerses[0]);
                            }
                        }
                    }
                    DisplayTranslation(m_client.FoundVerses);

                    if ((add_to_history) && (m_client.FoundChapters.Count > 0))
                    {
                        AddFindHistoryItem();
                    }

                    RealignFoundMatchedToStart();

                    UpdateHeaderLabel();
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
        }
        finally
        {
            MainTextBox.SelectionChanged += new EventHandler(MainTextBox_SelectionChanged);
            MainTextBox.EndUpdate();
            this.Cursor = Cursors.Default;
        }
    }
    private void RealignFoundMatchedToStart()
    {
        if (m_found_verses_displayed)
        {
            int start = 0;

            // scroll to beginning to show complete verse address as in Arabic, pos=0 is after the first number :(
            if (m_client.FoundVerses.Count > 0)
            {
                Verse verse = m_client.FoundVerses[0];
                if (verse != null)
                {
                    if (verse.Chapter != null)
                    {
                        if (m_find_by_text_language_type == FindByTextLanguageType.Arabic)
                        {
                            if (verse.Chapter != null)
                            {
                                start = verse.Chapter.Number.ToString().Length;
                            }
                        }
                        else // in non-Arabic (actually Left-to-Right languages) text
                        {
                            //start = verse.EnglishAddress.Length;
                            start = verse.Chapter.Number.ToString().Length;
                        }
                    }
                }
            }

            // re-align to text start
            try
            {
                MainTextBox.SelectionChanged -= new EventHandler(MainTextBox_SelectionChanged);
                MainTextBox.SelectionChanged -= new EventHandler(MainTextBox_SelectionChanged);
                MainTextBox.SelectionChanged -= new EventHandler(MainTextBox_SelectionChanged);

                if ((start >= 0) && (start < MainTextBox.Text.Length))
                {
                    MainTextBox.ScrollToCaret();    // must be first
                    MainTextBox.Select(start, 0);   // must be second
                }
            }
            finally
            {
                MainTextBox.SelectionChanged += new EventHandler(MainTextBox_SelectionChanged);
            }
        }
    }

    private void ColorizeMatches()
    {
        this.Cursor = Cursors.WaitCursor;
        MainTextBox.BeginUpdate();
        MainTextBox.SelectionChanged -= new EventHandler(MainTextBox_SelectionChanged);
        MainTextBox.SelectionChanged -= new EventHandler(MainTextBox_SelectionChanged);
        MainTextBox.SelectionChanged -= new EventHandler(MainTextBox_SelectionChanged);
        MainTextBox.VScroll -= new EventHandler(MainTextBox_VScroll);

        try
        {
            if (m_client != null)
            {
                m_client.UpdatePhrasePositions();
                if (m_client.FoundPhrases != null)
                {
                    if (m_client.FoundPhrases.Count > 0)
                    {
                        foreach (Phrase phrase in m_client.FoundPhrases)
                        {
                            int start = GetPhrasePositionInRichTextBox(phrase);
                            int length = phrase.Text.Length;
                            if ((start >= 0) && (start < MainTextBox.Text.Length))
                            {
                                MainTextBox.Select(start, length);
                                MainTextBox.SelectionColor = Color.Red;
                            }
                        }
                        RealignFoundMatchedToStart();
                        RefreshApplicationCaption();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
        }
        finally
        {
            MainTextBox.VScroll += new EventHandler(MainTextBox_VScroll);
            MainTextBox.SelectionChanged += new EventHandler(MainTextBox_SelectionChanged);
            MainTextBox.EndUpdate();
            this.Cursor = Cursors.Default;
        }
    }
    private int GetPhrasePositionInRichTextBox(Phrase phrase)
    {
        if (m_client != null)
        {
            int start = 0;
            foreach (Verse verse in m_client.FoundVerses)
            {
                if (phrase.Verse.Number == verse.Number)
                {
                    if (m_find_by_text_language_type == FindByTextLanguageType.Arabic)
                    {
                        start += verse.ArabicAddress.Length + 1 + phrase.Position;
                    }
                    else if (m_find_by_text_language_type == FindByTextLanguageType.Translation)
                    {
                        //start += verse.EnglishAddress.Length + 1 + phrase.Position;
                        start += verse.ArabicAddress.Length + 1 + phrase.Position;
                    }
                    return start;
                }

                // take into account prior verses
                if (m_find_by_text_language_type == FindByTextLanguageType.Arabic)
                {
                    start += verse.ArabicAddress.Length + 1 + verse.GetText(m_client.TextMode).Length + 1; // 1 for \n
                }
                else if (m_find_by_text_language_type == FindByTextLanguageType.Translation)
                {
                    //start += verse.EnglishAddress.Length + 1 + verse.GetText(m_client.TextMode).Length + 1; // 1 for \n
                    start += verse.ArabicAddress.Length + 1 + verse.GetText(m_client.TextMode).Length + 1; // 1 for \n
                }
            }
        }
        return -1;
    }
    private void BuildTextSelections()
    {
        try
        {
            if (m_client != null)
            {
                if (m_client.FoundPhrases != null)
                {
                    if (m_client.FoundPhrases.Count > 0)
                    {
                        foreach (Phrase phrase in m_client.FoundPhrases)
                        {
                            int start = GetPhrasePositionInRichTextBox(phrase);
                            int length = phrase.Text.Length;
                            BuildTextSelection(start, length);
                        }
                    }
                }
            }
            RefreshApplicationCaption();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
        }
    }

    private FindType m_private_find_type = FindType.Text; // named with private to indicate must set via Property, not directly by field
    private FindType FindType
    {
        get { return m_private_find_type; }
        set
        {
            m_private_find_type = value;
            if (m_private_find_type == FindType.Text)
            {
                AutoCompleteHeaderLabel.Visible = true;
                AutoCompleteListBox.BringToFront();
                //SearchGroupBox.Text = " Search by " + m_find_by_text_search_type.ToString() + ((m_find_by_text_search_type.ToString() == "Exact") ? " words " : "") + "          ";
                //SearchGroupBox.Refresh();
            }
            else
            {
                AutoCompleteHeaderLabel.Visible = false;
                AutoCompleteListBox.SendToBack();
                //SearchGroupBox.Text = " Search by " + m_private_find_type.ToString() + "              ";
                //SearchGroupBox.Refresh();
            }
        }
    }
    private FindByTextLanguageType m_find_by_text_language_type = FindByTextLanguageType.Arabic;
    private FindByTextSearchType m_find_by_text_search_type = FindByTextSearchType.Exact;
    private void SearchGroupBox_Enter(object sender, EventArgs e)
    {
        if (FindType == FindType.Text)
        {
            AutoCompleteHeaderLabel.Visible = true;
            AutoCompleteListBox.BringToFront();
        }
        else
        {
            AutoCompleteHeaderLabel.Visible = false;
            AutoCompleteListBox.SendToBack();
        }
    }
    private void SearchGroupBox_Leave(object sender, EventArgs e)
    {
        if ((!AutoCompleteListBox.Focused) && (!AutoCompleteAnywhereInWordCheckBox.Focused))
        {
            AutoCompleteHeaderLabel.Visible = false;
            AutoCompleteListBox.SendToBack();
        }
    }
    private void FindByTextSearchTypeLabel_MouseEnter(object sender, EventArgs e)
    {
        Control control = (sender as Control);
        control.BackColor = Color.SlateGray;
        control.Refresh();

        if (control == FindByTextSearchTypeLabel)
        {
            WordRootsSourceLabel.BackColor = Color.SlateGray;
            WordRootsSourceLabel.Refresh();
        }
    }
    private void FindByTextSearchTypeLabel_MouseLeave(object sender, EventArgs e)
    {
        Control control = (sender as Control);
        control.BackColor = Color.SteelBlue;
        control.Refresh();

        if (control == FindByTextSearchTypeLabel)
        {
            WordRootsSourceLabel.BackColor = Color.SteelBlue;
            WordRootsSourceLabel.Refresh();
        }
    }
    private void PrepareNewSearch()
    {
        if (m_client != null)
        {
            m_client.FoundPhrases = null;
            m_text_selections.Clear();
            m_text_selection_index = -1;
        }
    }

    private bool m_find_in_all_text_modes = true;
    private void FindByTextAllTextModesCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        m_find_in_all_text_modes = FindByTextAllTextModesCheckBox.Checked;
        if (TextModeComboBox.SelectedItem != null)
        {
            UpdateKeyboard(TextModeComboBox.SelectedItem.ToString());
        }
    }
    private void SetLanguageType(FindByTextLanguageType language_type)
    {
        if (language_type == FindByTextLanguageType.Translation)
        {
            if (m_find_by_text_search_type != FindByTextSearchType.Root)
            {
                m_find_by_text_language_type = FindByTextLanguageType.Translation;
                FindByTextLanguageTypeLabel.Text = "TRANSLATION";
                ToolTip.SetToolTip(FindByTextLanguageTypeLabel, "search in current translation");
                FindByTextCaseSensitiveCheckBox.BringToFront();
                if (RelatedWordsMenuItem != null) RelatedWordsMenuItem.Enabled = false;
            }
        }
        else if (language_type == FindByTextLanguageType.Arabic)
        {
            m_find_by_text_language_type = FindByTextLanguageType.Arabic;
            FindByTextLanguageTypeLabel.Text = "ARABIC";
            ToolTip.SetToolTip(FindByTextLanguageTypeLabel, "search in Uthmani or emlaaei Arabic");
            FindByTextCaseSensitiveCheckBox.SendToBack();
            if (RelatedWordsMenuItem != null) RelatedWordsMenuItem.Enabled = true;
        }
    }
    private void FindByTextLabel_Click(object sender, EventArgs e)
    {
        FindByTextControls_Enter(sender, e);
    }
    private void FindByTextLanguageTypeLabel_Click(object sender, EventArgs e)
    {
        if (m_find_by_text_language_type == FindByTextLanguageType.Arabic)
        {
            SetLanguageType(FindByTextLanguageType.Translation);
        }
        else if (m_find_by_text_language_type == FindByTextLanguageType.Translation)
        {
            SetLanguageType(FindByTextLanguageType.Arabic);
        }
        else
        {
            if (RelatedWordsMenuItem != null) RelatedWordsMenuItem.Enabled = true;
        }
        FindByTextLanguageTypeLabel.Refresh();

        UpdateFindByTextControls();

        FindByTextControls_Enter(sender, e);
    }
    private void FindByTextSearchTypeLabel_Click(object sender, EventArgs e)
    {
        if (m_find_by_text_search_type == FindByTextSearchType.Exact)
        {
            m_find_by_text_search_type = FindByTextSearchType.Proximity;
            FindByTextSearchTypeLabel.Text = "WORDS";// "PROXIMITY";
            ToolTip.SetToolTip(FindByTextSearchTypeLabel, "search for any or all words in any order");
            PopulateAutoCompleteListBoxWithCurrentWords();
            FindByTextAllWordsRadioButton.Checked = true;
        }
        else if (m_find_by_text_search_type == FindByTextSearchType.Proximity)
        {
            m_find_by_text_search_type = FindByTextSearchType.Root;
            FindByTextSearchTypeLabel.Text = "ROOT";
            ToolTip.SetToolTip(FindByTextSearchTypeLabel, "search for all words with same root");
            PopulateAutoCompleteListBoxWithRoots();

            // toggle back to EXACT if ROOT
            if (m_find_by_text_language_type == FindByTextLanguageType.Translation)
            {
                // keep changing search type until we reach Exact search
                while (m_find_by_text_search_type != FindByTextSearchType.Exact)
                {
                    FindByTextSearchTypeLabel_Click(sender, e);
                }
            }
            FindByTextAnywhereRadioButton.Checked = true;
        }
        else if (m_find_by_text_search_type == FindByTextSearchType.Root)
        {
            m_find_by_text_search_type = FindByTextSearchType.Exact;
            FindByTextSearchTypeLabel.Text = "EXACT";
            ToolTip.SetToolTip(FindByTextSearchTypeLabel, "search for exact word or phrase");
            PopulateAutoCompleteListBoxWithNextWords();
            FindByTextAnywhereRadioButton.Checked = true;
        }
        else
        {
            //
        }

        WordRootsSourceLabel.Visible = (m_find_by_text_search_type == FindByTextSearchType.Root);
        FindByTextLanguageTypeLabel.Enabled = !WordRootsSourceLabel.Visible;
        FindByTextSearchTypeLabel.Refresh();
        UpdateFindByTextControls();
        FindByTextControls_Enter(sender, e);
    }
    private void FindByTextRadioButton_CheckedChanged(object sender, EventArgs e)
    {
        UpdateFindByTextControls();
    }
    private void FindByTextWordnessCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        UpdateFindByTextControls();
    }
    private void FindByTextCaseSensitiveCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        UpdateFindByTextControls();
    }
    private void FindByTextMultiplicityCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        FindByTextMultiplicityNumericUpDown.Enabled = FindByTextMultiplicityCheckBox.Checked;
    }
    private void FindByTextMultiplicityNumericUpDown_ValueChanged(object sender, EventArgs e)
    {
        // do nothing
    }
    private void FindByTextMultiplicityNumericUpDown_KeyDown(object sender, KeyEventArgs e)
    {
        // do nothing
    }
    private void FindByTextControls_Enter(object sender, EventArgs e)
    {
        this.AcceptButton = FindByTextButton;
        FindType = FindType.Text;
    }
    private void FindByTextPanel_Leave(object sender, EventArgs e)
    {
        SearchGroupBox_Leave(sender, e);
    }
    private void FindByTextTextBox_TextChanged(object sender, EventArgs e)
    {
        UpdateFindByTextControls();

        if (m_find_by_text_search_type == FindByTextSearchType.Exact)
        {
            PopulateAutoCompleteListBoxWithNextWords();
        }
        else if (m_find_by_text_search_type == FindByTextSearchType.Root)
        {
            PopulateAutoCompleteListBoxWithRoots();
        }
        else if (m_find_by_text_search_type == FindByTextSearchType.Proximity)
        {
            PopulateAutoCompleteListBoxWithCurrentWords();
        }
        else
        {
            //
        }

        RefreshLanguageType(FindByTextTextBox.Text);
    }
    private void FindByTextTextBox_KeyPress(object sender, KeyPressEventArgs e)
    {
        FixMicrosoft(sender, e);

        if (e.KeyChar == ' ')
        {
            // if no multi-root search, then prevent space entry
            //if (m_find_by_text_search_type == FindByTextSearchType.Root)
            //{
            //    e.Handled = true; // prevent space in Root search
            //}

            // prevent double spaces
            if (FindByTextTextBox.SelectionStart > 0)
            {
                if (FindByTextTextBox.Text[FindByTextTextBox.SelectionStart - 1] == ' ')
                {
                    e.Handled = true;
                }
            }
        }
        else if ((e.KeyChar == '-') || (e.KeyChar == '+'))
        {
            // - and + are only applicable in Proximity search
            // force change search type to Proximity search
            while (m_find_by_text_search_type != FindByTextSearchType.Proximity)
            {
                FindByTextSearchTypeLabel_Click(sender, e);
            }
            FindByTextAllWordsRadioButton.Checked = true;
        }
    }
    private void FindByTextTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (ModifierKeys == Keys.Control)
        {
            if (e.KeyCode == Keys.A)
            {
                if (sender is TextBoxBase)
                {
                    (sender as TextBoxBase).SelectAll();
                }
            }
        }
        else if ((e.KeyCode == Keys.Up) || (e.KeyCode == Keys.Down))
        {
            AutoCompleteListBox.Focus();
        }
    }
    private void FindByTextButton_Click(object sender, EventArgs e)
    {
        PrepareNewSearch();
        if (m_find_by_text_search_type == FindByTextSearchType.Exact)
        {
            FindByText();
        }
        else if (m_find_by_text_search_type == FindByTextSearchType.Root)
        {
            FindByRoot();
        }
        else if (m_find_by_text_search_type == FindByTextSearchType.Proximity)
        {
            FindByProximity();
        }
        else
        {
            // do nothing
        }
    }
    private void FindByText()
    {
        string text = FindByTextTextBox.Text;
        if (text.Length > 0)
        {
            FindByTextLanguageType language_type = m_find_by_text_language_type;

            string translation = DEFAULT_TRANSLATION;
            if (TranslatorComboBox.SelectedItem != null)
            {
                translation = Client.GetTranslationKey(TranslatorComboBox.SelectedItem.ToString());
            }

            FindByTextLocation text_location = FindByTextLocation.Anywhere;
            if (FindByTextAnywhereRadioButton.Checked)
            {
                text_location = FindByTextLocation.Anywhere;
            }
            else if (FindByTextAtStartRadioButton.Checked)
            {
                text_location = FindByTextLocation.AtStart;
            }
            else if (FindByTextAtMiddleRadioButton.Checked)
            {
                text_location = FindByTextLocation.AtMiddle;
            }
            else if (FindByTextAtEndRadioButton.Checked)
            {
                text_location = FindByTextLocation.AtEnd;
            }
            else if (FindByTextAllWordsRadioButton.Checked)
            {
                text_location = FindByTextLocation.AllWords;
            }
            else if (FindByTextAnyWordRadioButton.Checked)
            {
                text_location = FindByTextLocation.AnyWord;
            }
            else // default
            {
                text_location = FindByTextLocation.Anywhere;
            }

            bool case_sensitive = FindByTextCaseSensitiveCheckBox.Checked;

            FindByTextWordness wordness = FindByTextWordness.Any;
            switch (FindByTextWordnessCheckBox.CheckState)
            {
                case CheckState.Checked:
                    wordness = FindByTextWordness.WholeWord;
                    break;
                case CheckState.Indeterminate:
                    wordness = FindByTextWordness.PartOfWord;
                    break;
                case CheckState.Unchecked:
                    wordness = FindByTextWordness.Any;
                    break;
                default:
                    wordness = FindByTextWordness.Any;
                    break;
            }
            int multiplicity = FindByTextMultiplicityNumericUpDown.Enabled ? (int)FindByTextMultiplicityNumericUpDown.Value : -1;
            FindByText(text, language_type, translation, text_location, case_sensitive, wordness, multiplicity, m_anywhere_in_word, m_find_in_all_text_modes);
        }
    }
    private void FindByText(string text, FindByTextLanguageType language_type, string translation, FindByTextLocation text_location, bool case_sensitive, FindByTextWordness wordness, int multiplicity, bool anywhere_in_word, bool all_text_modes)
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            if (m_client != null)
            {
                PrepareNewSearch();

                if (text.Length > 0)
                {
                    m_client.FindPhrases(text, language_type, translation, text_location, case_sensitive, wordness, multiplicity, anywhere_in_word, all_text_modes);
                    if (m_client.FoundPhrases != null)
                    {
                        bool display_phrases_count = true;
                        int phrase_count = m_client.FoundPhrases.Count;

                        if ((text_location == FindByTextLocation.AtStart) || (text_location == FindByTextLocation.AtEnd) || (text_location == FindByTextLocation.AllWords) || (text_location == FindByTextLocation.AnyWord))
                        {
                            display_phrases_count = false;  // not to be dispalyed
                        }

                        if (m_client.FoundVerses != null)
                        {
                            int verse_count = m_client.FoundVerses.Count;
                            if (multiplicity == 0)
                            {
                                m_find_result_header = verse_count + ((verse_count == 1) ? " verse" : " verses") + " without " + text + " " + text_location.ToString() + " in " + FindScopeLabel.Text;
                            }
                            else
                            {
                                m_find_result_header = (display_phrases_count ? phrase_count + " matches in " : "") + verse_count + ((verse_count == 1) ? " verse" : " verses") + " with " + text + " " + text_location.ToString() + " in " + FindScopeLabel.Text;
                            }

                            DisplayFoundVerses(true, false);
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
    private void FindByRoot()
    {
        if (FindByTextTextBox.Text.Length > 0)
        {
            string root = FindByTextTextBox.Text.Trim();
            int multiplicity = FindByTextMultiplicityNumericUpDown.Enabled ? (int)FindByTextMultiplicityNumericUpDown.Value : -1;
            FindByRoot(root, multiplicity);
        }
    }
    private void FindByRoot(string root, int multiplicity)
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            if (m_client != null)
            {
                PrepareNewSearch();

                if (root.Length > 0)
                {
                    m_client.FindPhrases(root, multiplicity);
                    if (m_client.FoundPhrases != null)
                    {
                        if (m_client.FoundVerses != null)
                        {
                            int verse_count = m_client.FoundVerses.Count;
                            int phrase_count = m_client.FoundPhrases.Count;
                            m_find_result_header = ((m_client.FoundPhrases != null) ? phrase_count + " matches in " : "") + verse_count + ((verse_count == 1) ? " verse" : " verses") + " with root " + root + " in " + FindScopeLabel.Text;

                            DisplayFoundVerses(true, false);
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
    private void FindByProximity()
    {
        FindByText();
    }
    private void FindByTextKeyboardLabel_Click(object sender, EventArgs e)
    {
        Control control = (sender as Control);
        control.BackColor = Color.LightSteelBlue;
        control.Refresh();

        // prevent double spaces
        if (control == FindByTextSpaceLabel)
        {
            if (FindByTextTextBox.SelectionStart > 0)
            {
                if (FindByTextTextBox.Text[FindByTextTextBox.SelectionStart - 1] == ' ')
                {
                    return;
                }
            }
        }

        string letter = control.Text[0].ToString();
        int pos = FindByTextTextBox.SelectionStart;
        int len = FindByTextTextBox.SelectionLength;
        if (pos >= 0)
        {
            if (len > 0)
            {
                FindByTextTextBox.Text = FindByTextTextBox.Text.Remove(pos, len);
            }
            else
            {
                // do nothing
            }
            FindByTextTextBox.Text = FindByTextTextBox.Text.Insert(pos, letter);
            FindByTextTextBox.SelectionStart = pos + 1;
            FindByTextTextBox.Refresh();
        }

        Thread.Sleep(100);
        control.BackColor = Color.LightGray;
        control.Refresh();

        FindByTextKeyboardLabel_MouseEnter(sender, e);
        FindByTextControls_Enter(sender, e);
    }
    private void FindByTextBackspaceLabel_Click(object sender, EventArgs e)
    {
        if (!FindByTextTextBox.Focused)
        {
            FindByTextTextBox.Focus();
        }

        Control control = (sender as Control);
        control.BackColor = Color.LightSteelBlue;
        control.Refresh();

        int pos = FindByTextTextBox.SelectionStart;
        int len = FindByTextTextBox.SelectionLength;
        if ((len == 0) && (pos > 0))        // delete character prior to cursor
        {
            FindByTextTextBox.Text = FindByTextTextBox.Text.Remove(pos - 1, 1);
            FindByTextTextBox.SelectionStart = pos - 1;
        }
        else if ((len > 0) && (pos >= 0))   // delete current highlighted characters
        {
            FindByTextTextBox.Text = FindByTextTextBox.Text.Remove(pos, len);
            FindByTextTextBox.SelectionStart = pos;
        }
        else                  // nothing to delete
        {
        }
        FindByTextTextBox.Refresh();

        Thread.Sleep(100);
        control.BackColor = Color.LightGray;
        control.Refresh();

        FindByTextKeyboardLabel_MouseEnter(sender, e);
        FindByTextControls_Enter(sender, e);
    }
    private void FindByTextKeyboardLabel_MouseEnter(object sender, EventArgs e)
    {
        Control control = (sender as Control);
        control.BackColor = Color.White;
        control.Refresh();
    }
    private void FindByTextKeyboardLabel_MouseLeave(object sender, EventArgs e)
    {
        Control control = (sender as Control);
        control.BackColor = Color.LightGray;
        control.Refresh();
    }
    private void UpdateKeyboard(string text_mode)
    {
        if (FindByTextAllTextModesCheckBox.Checked)
        {
            FindByTextHamzaLabel.Enabled = true;
            FindByTextTaaMarbootaLabel.Enabled = true;
            FindByTextElfMaqsuraLabel.Enabled = true;
            FindByTextSkoonAboveElfLabel.Enabled = true;
            FindByTextHamzaAboveElfLabel.Enabled = true;
            FindByTextHamzaBelowElfLabel.Enabled = true;
            FindByTextElfMamdudaLabel.Enabled = true;
            FindByTextHamzaAboveWaawLabel.Enabled = true;
            FindByTextHamzaAboveYaaLabel.Enabled = true;
        }
        else
        {
            FindByTextHamzaLabel.Enabled = false;
            FindByTextTaaMarbootaLabel.Enabled = false;
            FindByTextElfMaqsuraLabel.Enabled = false;
            FindByTextSkoonAboveElfLabel.Enabled = false;
            FindByTextHamzaAboveElfLabel.Enabled = false;
            FindByTextHamzaBelowElfLabel.Enabled = false;
            FindByTextElfMamdudaLabel.Enabled = false;
            FindByTextHamzaAboveWaawLabel.Enabled = false;
            FindByTextHamzaAboveYaaLabel.Enabled = false;

            if (text_mode == "Simplified28")
            {
                // do nothing
            }
            else if (text_mode.Contains("Simplified29"))
            {
                FindByTextHamzaLabel.Enabled = true;
            }
            else if (text_mode == "Simplified30")
            {
                FindByTextTaaMarbootaLabel.Enabled = true;
                FindByTextElfMaqsuraLabel.Enabled = true;
            }
            else if (text_mode == "Simplified31")
            {
                FindByTextHamzaLabel.Enabled = true;
                FindByTextTaaMarbootaLabel.Enabled = true;
                FindByTextElfMaqsuraLabel.Enabled = true;
            }
            else if (text_mode == "Simplified37")
            {
                FindByTextHamzaLabel.Enabled = true;

                FindByTextTaaMarbootaLabel.Enabled = true;
                FindByTextElfMaqsuraLabel.Enabled = true;

                FindByTextSkoonAboveElfLabel.Enabled = true;
                FindByTextHamzaAboveElfLabel.Enabled = true;
                FindByTextHamzaBelowElfLabel.Enabled = true;
                FindByTextElfMamdudaLabel.Enabled = true;
                FindByTextHamzaAboveWaawLabel.Enabled = true;
                FindByTextHamzaAboveYaaLabel.Enabled = true;
            }
            else if (text_mode == "Original")
            {
                FindByTextHamzaLabel.Enabled = true;

                FindByTextTaaMarbootaLabel.Enabled = true;
                FindByTextElfMaqsuraLabel.Enabled = true;

                FindByTextSkoonAboveElfLabel.Enabled = true;
                FindByTextHamzaAboveElfLabel.Enabled = true;
                FindByTextHamzaBelowElfLabel.Enabled = true;
                FindByTextElfMamdudaLabel.Enabled = true;
                FindByTextHamzaAboveWaawLabel.Enabled = true;
                FindByTextHamzaAboveYaaLabel.Enabled = true;

                // enable harakaat keys here (if we add harakaat keys)
            }
            else
            {
                // do nothing
            }
        }
    }

    private bool m_auto_complete_list_double_click = false;
    private void AutoCompleteListBox_Enter(object sender, EventArgs e)
    {
        this.AcceptButton = null;
    }
    private void AutoCompleteListBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            AutoCompleteListBox_DoubleClick(sender, e);
        }
        else if (e.KeyCode == Keys.Space)
        {
            FindByTextTextBox.Text += " ";
            FindByTextTextBox.Focus();
        }
        else if ((e.KeyCode == Keys.Left) || (e.KeyCode == Keys.Right))
        {
            FindByTextTextBox.Focus();
        }
        FindByTextTextBox.SelectionStart = FindByTextTextBox.Text.Length;
    }
    private void AutoCompleteListBox_Click(object sender, EventArgs e)
    {
        // do nothing
    }
    private void AutoCompleteListBox_DoubleClick(object sender, EventArgs e)
    {
        m_auto_complete_list_double_click = true;
        if (AutoCompleteListBox.Items.Count > 0)
        {
            AddNextWordToFindText();
        }
        else
        {
            FindByTextButton_Click(sender, e);
        }
        m_auto_complete_list_double_click = false;
    }
    private void AutoCompleteListBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (m_auto_complete_words != null)
        {
            int count = 0;
            int total = 0;
            if (AutoCompleteListBox.SelectedIndices.Count > 1)
            {
                // update total(unique) counts
                foreach (object item in AutoCompleteListBox.SelectedItems)
                {
                    string[] parts = item.ToString().Split(' ');
                    foreach (string key in m_auto_complete_words.Keys)
                    {
                        if (key == parts[parts.Length - 1])
                        {
                            string entry = String.Format("{0,-3} {1,10}", m_auto_complete_words[key], key);
                            total += m_auto_complete_words[key];
                            count++;

                            break;
                        }
                    }
                }
            }
            else
            {
                // restore total(unique) counts
                foreach (string key in m_auto_complete_words.Keys)
                {
                    string entry = String.Format("{0,-3} {1,10}", m_auto_complete_words[key], key);
                    total += m_auto_complete_words[key];
                    count++;
                }
            }
            AutoCompleteHeaderLabel.Text = total.ToString() + " (" + count.ToString() + ")";
            AutoCompleteHeaderLabel.Refresh();
        }
    }
    private void AddNextWordToFindText()
    {
        if (AutoCompleteListBox.SelectedItem != null)
        {
            string word_to_add = AutoCompleteListBox.SelectedItem.ToString();
            int pos = word_to_add.LastIndexOf(' ');
            if (pos > -1)
            {
                word_to_add = word_to_add.Substring(pos + 1);
            }

            string text = FindByTextTextBox.Text;
            int last_space_index = text.LastIndexOf(' ');
            if (last_space_index != -1)
            {
                // if no multi-root search, then prevent losing previous word in TextBox
                //if (m_find_by_text_search_type == FindByTextSearchType.Root)
                //{
                //    FindByTextTextBox.Text = word_to_add;
                //}
                //else
                {
                    text = text.Substring(0, last_space_index + 1);
                    text += word_to_add;
                    FindByTextTextBox.Text = text + " ";
                }
            }
            else
            {
                FindByTextTextBox.Text = word_to_add + " ";
            }
            FindByTextTextBox.Refresh();
            FindByTextTextBox.SelectionStart = FindByTextTextBox.Text.Length;
        }
    }
    private void ClearAutoCompleteListBox()
    {
        AutoCompleteListBox.Items.Clear();
    }

    private bool m_anywhere_in_word = true;
    private Dictionary<string, int> m_auto_complete_words = null;
    private void PopulateAutoCompleteListBoxWithNextWords()
    {
        AutoCompleteListBox.SelectedIndexChanged -= new EventHandler(AutoCompleteListBox_SelectedIndexChanged);

        try
        {
            if (m_client != null)
            {
                //SearchGroupBox.Text = " Search by Exact words      ";
                //SearchGroupBox.Refresh();
                AutoCompleteHeaderLabel.Text = "000 (00)";
                ToolTip.SetToolTip(AutoCompleteHeaderLabel, "total (unique)");
                AutoCompleteHeaderLabel.Refresh();

                AutoCompleteListBox.BeginUpdate();
                AutoCompleteListBox.Items.Clear();

                string text = FindByTextTextBox.Text;
                if (text.Length > 0)
                {
                    if (text.EndsWith(" "))
                    {
                        m_auto_complete_words = m_client.GetNextWords(text, m_anywhere_in_word);
                    }
                    else
                    {
                        m_auto_complete_words = m_client.GetCurrentWords(text, m_anywhere_in_word);
                    }

                    if (m_auto_complete_words != null)
                    {
                        int count = 0;
                        int total = 0;
                        foreach (string key in m_auto_complete_words.Keys)
                        {
                            //string value_str = found_words[key].ToString().PadRight(3, ' ');
                            //string key_str = key.PadLeft(10, ' ');
                            //string entry = String.Format("{0} {1}", value_str, key_str);
                            string entry = String.Format("{0,-3} {1,10}", m_auto_complete_words[key], key);
                            AutoCompleteListBox.Items.Add(entry);
                            total += m_auto_complete_words[key];
                            count++;
                        }

                        if (AutoCompleteListBox.Items.Count > 0)
                        {
                            AutoCompleteListBox.SelectedIndex = 0;
                        }
                        else // no match [either current text_mode doesn't have a match or it was last word in verse]
                        {
                            if (m_find_by_text_language_type == FindByTextLanguageType.Arabic)
                            {
                                // m_auto_complete_list_double_click == false if input was via keyboard
                                // m_auto_complete_list_double_click == true  if input was via double click
                                // if no more word when double click, then it means it was the last word in the verse
                                // else the user has entered non-matching text

                                // if last word in verse, remove the extra space after it
                                if ((m_auto_complete_list_double_click) && (AutoCompleteListBox.Items.Count == 0) && (FindByTextTextBox.Text.EndsWith(" ")))
                                {
                                    FindByTextTextBox.TextChanged -= new EventHandler(FindByTextTextBox_TextChanged);
                                    FindByTextTextBox.Text = FindByTextTextBox.Text.Remove(FindByTextTextBox.Text.Length - 1);
                                    FindByTextTextBox.TextChanged += new EventHandler(FindByTextTextBox_TextChanged);
                                }
                            }
                            else
                            {
                                // allow no-matching text entry e.g. in English
                            }
                        }

                        AutoCompleteHeaderLabel.Text = total.ToString() + " (" + count.ToString() + ")";
                        AutoCompleteHeaderLabel.Refresh();
                    }
                }
            }
        }
        finally
        {
            AutoCompleteListBox.EndUpdate();
            AutoCompleteListBox.SelectedIndexChanged += new EventHandler(AutoCompleteListBox_SelectedIndexChanged);
        }
    }
    private void PopulateAutoCompleteListBoxWithCurrentWords()
    {
        AutoCompleteListBox.SelectedIndexChanged -= new EventHandler(AutoCompleteListBox_SelectedIndexChanged);

        try
        {
            //SearchGroupBox.Text = " Search by Proximity        ";
            //SearchGroupBox.Refresh();
            AutoCompleteHeaderLabel.Text = "000 (00)";
            ToolTip.SetToolTip(AutoCompleteHeaderLabel, "total (unique)");
            AutoCompleteHeaderLabel.Refresh();

            AutoCompleteListBox.BeginUpdate();
            AutoCompleteListBox.Items.Clear();

            string text = FindByTextTextBox.Text;
            if (text.Length > 0)
            {
                string[] text_parts = text.Split(' ');
                text = text_parts[text_parts.Length - 1];
                if (text.Length > 0)
                {
                    m_auto_complete_words = m_client.GetCurrentWords(text, m_anywhere_in_word);
                    if (m_auto_complete_words != null)
                    {
                        int count = 0;
                        int total = 0;
                        foreach (string key in m_auto_complete_words.Keys)
                        {
                            //string value_str = found_words[key].ToString().PadRight(3, ' ');
                            //string key_str = key.PadLeft(10, ' ');
                            //string entry = String.Format("{0} {1}", value_str, key_str);
                            string entry = String.Format("{0,-3} {1,10}", m_auto_complete_words[key], key);
                            AutoCompleteListBox.Items.Add(entry);
                            total += m_auto_complete_words[key];
                            count++;
                        }


                        if (AutoCompleteListBox.Items.Count > 0)
                        {
                            AutoCompleteListBox.SelectedIndex = 0;
                        }
                        else
                        {
                            // if not a valid word, keep word as is
                        }

                        AutoCompleteHeaderLabel.Text = total.ToString() + " (" + count.ToString() + ")";
                        AutoCompleteHeaderLabel.Refresh();
                    }
                }
            }
        }
        finally
        {
            AutoCompleteListBox.EndUpdate();
            AutoCompleteListBox.SelectedIndexChanged += new EventHandler(AutoCompleteListBox_SelectedIndexChanged);
        }
    }
    private void PopulateAutoCompleteListBoxWithRoots()
    {
        AutoCompleteListBox.SelectedIndexChanged -= new EventHandler(AutoCompleteListBox_SelectedIndexChanged);

        try
        {
            //SearchGroupBox.Text = " Search by Roots            ";
            //SearchGroupBox.Refresh();
            AutoCompleteHeaderLabel.Text = "0 Roots";
            ToolTip.SetToolTip(AutoCompleteHeaderLabel, "total roots");
            AutoCompleteHeaderLabel.Refresh();

            AutoCompleteListBox.BeginUpdate();
            AutoCompleteListBox.Items.Clear();

            string text = FindByTextTextBox.Text;
            if (text.Length > 0)
            {
                string[] text_parts = text.Split(' ');
                text = text_parts[text_parts.Length - 1];
                if (text.Length > 0)
                {
                    List<string> found_roots = Client.GetRootsWith(m_client.TextMode, text, m_anywhere_in_word);
                    if (found_roots != null)
                    {
                        int count = 0;
                        foreach (string root in found_roots)
                        {
                            string entry = root.PadLeft(14, ' ');
                            //string entry = String.Format("{0,14}", root);
                            AutoCompleteListBox.Items.Add(entry);
                            count++;
                        }

                        if (AutoCompleteListBox.Items.Count > 0)
                        {
                            AutoCompleteListBox.SelectedIndex = 0;
                        }
                        else
                        {
                            // if not a valid root, put word as is so we can find same rooted words
                            AutoCompleteListBox.Items.Add(text);
                        }
                        AutoCompleteHeaderLabel.Text = count.ToString() + " Roots";
                        AutoCompleteHeaderLabel.Refresh();
                    }
                }
            }
        }
        finally
        {
            AutoCompleteListBox.EndUpdate();
            AutoCompleteListBox.SelectedIndexChanged += new EventHandler(AutoCompleteListBox_SelectedIndexChanged);
        }
    }
    private void UpdateFindByTextControls()
    {
        FindByTextAtStartRadioButton.Enabled = (m_find_by_text_search_type == FindByTextSearchType.Exact);
        FindByTextAtMiddleRadioButton.Enabled = (m_find_by_text_search_type == FindByTextSearchType.Exact);
        FindByTextAtEndRadioButton.Enabled = (m_find_by_text_search_type == FindByTextSearchType.Exact);
        FindByTextAnywhereRadioButton.Enabled = (m_find_by_text_search_type == FindByTextSearchType.Exact);

        FindByTextAllWordsRadioButton.Enabled = (m_find_by_text_search_type == FindByTextSearchType.Proximity);
        FindByTextAnyWordRadioButton.Enabled = (m_find_by_text_search_type == FindByTextSearchType.Proximity) && (!FindByTextTextBox.Text.Contains("-")) && (!FindByTextTextBox.Text.Contains("+"));
        FindByTextPlusLabel.Enabled = (m_find_by_text_search_type == FindByTextSearchType.Proximity);
        FindByTextMinusLabel.Enabled = (m_find_by_text_search_type == FindByTextSearchType.Proximity);
        FindByTextDoubleQuotesLabel.Enabled = (m_find_by_text_search_type == FindByTextSearchType.Proximity);
        FindByTextOrLabel.Enabled = (m_find_by_text_search_type == FindByTextSearchType.Proximity);
        FindByTextAndLabel.Enabled = (m_find_by_text_search_type == FindByTextSearchType.Proximity);

        FindByTextWordnessCheckBox.Enabled = (m_find_by_text_search_type == FindByTextSearchType.Exact);

        FindByTextMultiplicityCheckBox.Enabled =
            (!FindByTextAtStartRadioButton.Checked) &&
            (!FindByTextAtEndRadioButton.Checked) &&
            (!FindByTextAllWordsRadioButton.Checked) &&
            (!FindByTextAnyWordRadioButton.Checked);

        FindByTextMultiplicityNumericUpDown.Enabled =
            (FindByTextMultiplicityCheckBox.Enabled) &&
            (FindByTextMultiplicityCheckBox.Checked);

        if (m_find_by_text_language_type == FindByTextLanguageType.Arabic)
        {
            FindByTextCaseSensitiveCheckBox.Enabled = false;
        }
        else if (m_find_by_text_language_type == FindByTextLanguageType.Translation)
        {
            FindByTextCaseSensitiveCheckBox.Enabled = true;
        }
    }
    private void AutoCompleteAnywhereInWordCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        m_anywhere_in_word = AutoCompleteAnywhereInWordCheckBox.Checked;
        FindByTextTextBox_TextChanged(sender, e);
    }
    private void AutoCompleteSaveToolStripMenuItem_Click(object sender, EventArgs e)
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            string directory = "Frequencies";
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string text = FindByTextTextBox.Text;

            if (AutoCompleteHeaderLabel.Text.Length >= 7) // minimum is "[ 0-0 ]"
            {
                string auto_complete_text = AutoCompleteHeaderLabel.Text.Substring(2, AutoCompleteHeaderLabel.Text.Length - 4);
                string[] auto_complete_text_parts = auto_complete_text.Split('-');
                string count_str = auto_complete_text_parts[0];
                string total_str = auto_complete_text_parts[1];

                string filename = directory + "/" + text + ".txt";
                try
                {
                    using (StreamWriter writer = new StreamWriter(filename, false, Encoding.Unicode))
                    {
                        StringBuilder str = new StringBuilder();
                        str.AppendLine("------------");
                        str.AppendLine("Word" + "\t" + "Freq");
                        str.AppendLine("------------");

                        foreach (object item in AutoCompleteListBox.Items)
                        {
                            string[] parts = item.ToString().Split();
                            for (int i = parts.Length - 1; i >= 0; i--)
                            {
                                if (parts[i].Length > 0)
                                {
                                    str.Append(parts[i] + "\t");
                                }
                            }
                            if (str.Length > 0)
                            {
                                str.Remove(str.Length - 1, 1);
                            }
                            str.Append("\r\n");
                        }
                        str.AppendLine("------------");
                        str.AppendLine("Count = " + count_str);
                        str.AppendLine("Total = " + total_str);

                        writer.Write(str.ToString());
                    }

                    // show file content after save
                    System.Diagnostics.Process.Start("Notepad.exe", filename);
                }
                catch
                {
                    // silence IO error in case running from read-only media (CD/DVD)
                }
            }
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }

    private FindBySimilaritySource m_find_by_similarity_source = FindBySimilaritySource.CurrentVerse;
    private void FindBySimilarityLabel_Click(object sender, EventArgs e)
    {
        FindBySimilarityControls_Enter(sender, e);
    }
    private void FindBySimilaritySourceLabel_Click(object sender, EventArgs e)
    {
        if (m_find_by_similarity_source == FindBySimilaritySource.CurrentVerse)
        {
            m_find_by_similarity_source = FindBySimilaritySource.AllVerses;
            FindBySimilaritySourceLabel.Text = "ALL  VERSES";
            ToolTip.SetToolTip(FindBySimilaritySourceLabel, "within all verses");
        }
        else if (m_find_by_similarity_source == FindBySimilaritySource.AllVerses)
        {
            m_find_by_similarity_source = FindBySimilaritySource.CurrentVerse;
            FindBySimilaritySourceLabel.Text = "CURRENT  VERSE";
            ToolTip.SetToolTip(FindBySimilaritySourceLabel, "as current verse");
        }
        else
        {
            //
        }
        FindBySimilaritySourceLabel.Refresh();
        FindBySimilarityControls_Enter(sender, e);
    }
    private void FindBySimilarityRadioButton_CheckedChanged(object sender, EventArgs e)
    {
        //if (m_find_by_similarity_source == FindBySimilaritySource.CurrentVerse)
        //{
        //    FindBySimilarityButton_Click(sender, e);
        //}
    }
    private void FindBySimilarityPercentageTrackBar_Scroll(object sender, EventArgs e)
    {
        FindBySimilarityPercentageLabel.Text = FindBySimilarityPercentageTrackBar.Value.ToString() + " %";
        //if (m_find_by_similarity_source == FindBySimilaritySource.CurrentVerse)
        //{
        //    FindBySimilarityButton_Click(sender, e);
        //}
    }
    private void FindBySimilarityControls_Enter(object sender, EventArgs e)
    {
        this.AcceptButton = FindBySimilarityButton;
        FindType = FindType.Similarity;
    }
    private void FindBySimilarityButton_Click(object sender, EventArgs e)
    {
        FindBySimilarity();
    }
    private Verse m_find_by_similarity_current_verse = null;
    private void FindBySimilarity()
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            if (m_client != null)
            {
                PrepareNewSearch();

                FindBySimilarityMethod find_by_similarity_method = FindBySimilarityMethod.SimilarText;
                if (FindBySimilaritySimilarTextRadioButton.Checked)
                {
                    find_by_similarity_method = FindBySimilarityMethod.SimilarText;
                }
                else if (FindBySimilaritySimilarWordsRadioButton.Checked)
                {
                    find_by_similarity_method = FindBySimilarityMethod.SimilarWords;
                }
                else if (FindBySimilaritySimilarStartRadioButton.Checked)
                {
                    find_by_similarity_method = FindBySimilarityMethod.SimilarStart;
                }
                else if (FindBySimilaritySimilarEndRadioButton.Checked)
                {
                    find_by_similarity_method = FindBySimilarityMethod.SimilarEnd;
                }
                else
                {
                    //
                }

                double similarity_percentage = (double)FindBySimilarityPercentageTrackBar.Value / 100.0;

                string similarity_source = "";
                if (m_find_by_similarity_source == FindBySimilaritySource.CurrentVerse)
                {
                    if (m_find_by_similarity_current_verse == null)
                    {
                        m_find_by_similarity_current_verse = GetCurrentVerse();
                    }
                    if (m_find_by_similarity_current_verse != null)
                    {
                        if (m_find_by_similarity_current_verse.Chapter != null)
                        {
                            m_client.FindVerses(m_find_by_similarity_current_verse, find_by_similarity_method, similarity_percentage);
                            similarity_source = " to verse " + m_find_by_similarity_current_verse.Chapter.Name + " " + m_find_by_similarity_current_verse.NumberInChapter + " ";
                        }
                    }
                }
                else if (m_find_by_similarity_source == FindBySimilaritySource.AllVerses)
                {
                    m_client.FindVerseRanges(find_by_similarity_method, similarity_percentage);
                    similarity_source = "";
                }
                else
                {
                    //
                }

                if (m_client.FoundVerses != null)
                {
                    int verse_count = m_client.FoundVerses.Count;
                    m_find_result_header = verse_count + ((verse_count == 1) ? " verse" : " verses") + " with " + find_by_similarity_method.ToString() + similarity_source + " in " + FindScopeLabel.Text;

                    DisplayFoundVerses(true, false);
                }
            }
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }

    private FindByNumbersTextRange m_find_by_numbers_text_range = FindByNumbersTextRange.Verse;
    private void FindByNumbersLabel_Click(object sender, EventArgs e)
    {
        FindByNumbersControls_Enter(sender, e);
    }
    private void FindByNumbersNumberLabel_Click(object sender, EventArgs e)
    {
        FindByNumbersControls_Enter(sender, e);
    }
    private void FindByNumbersVersesLabel_Click(object sender, EventArgs e)
    {
        FindByNumbersControls_Enter(sender, e);
    }
    private void FindByNumbersWordsLabel_Click(object sender, EventArgs e)
    {
        FindByNumbersControls_Enter(sender, e);
    }
    private void FindByNumbersLettersLabel_Click(object sender, EventArgs e)
    {
        FindByNumbersControls_Enter(sender, e);
    }
    private void FindByNumbersUniqueLettersLabel_Click(object sender, EventArgs e)
    {
        FindByNumbersControls_Enter(sender, e);
    }
    private void FindByNumbersValueLabel_Click(object sender, EventArgs e)
    {
        FindByNumbersControls_Enter(sender, e);
    }
    private void FindByNumbersTextRangeLabel_Click(object sender, EventArgs e)
    {
        if (ModifierKeys == Keys.Shift)
        {
            if (m_find_by_numbers_text_range == FindByNumbersTextRange.Word)
            {
                FindByNumbersTextRangeAllVersesInChapter();
            }
            else if (m_find_by_numbers_text_range == FindByNumbersTextRange.WordRange)
            {
                FindByNumbersTextRangeWord();
            }
            else if (m_find_by_numbers_text_range == FindByNumbersTextRange.Verse)
            {
                FindByNumbersTextRangeWordRange();
            }
            else if (m_find_by_numbers_text_range == FindByNumbersTextRange.Chapter)
            {
                FindByNumbersTextRangeVerseRange();
            }
            else if (m_find_by_numbers_text_range == FindByNumbersTextRange.VerseRange)
            {
                FindByNumbersTextRangeVerse();
            }
            else if (m_find_by_numbers_text_range == FindByNumbersTextRange.ChapterVerses)
            {
                FindByNumbersTextRangeChapter();
            }
        }
        else
        {
            if (m_find_by_numbers_text_range == FindByNumbersTextRange.Word)
            {
                FindByNumbersTextRangeWordRange();
            }
            else if (m_find_by_numbers_text_range == FindByNumbersTextRange.WordRange)
            {
                FindByNumbersTextRangeVerse();
            }
            else if (m_find_by_numbers_text_range == FindByNumbersTextRange.Verse)
            {
                FindByNumbersTextRangeVerseRange();
            }
            else if (m_find_by_numbers_text_range == FindByNumbersTextRange.VerseRange)
            {
                FindByNumbersTextRangeChapter();
            }
            else if (m_find_by_numbers_text_range == FindByNumbersTextRange.Chapter)
            {
                FindByNumbersTextRangeAllVersesInChapter();
            }
            else if (m_find_by_numbers_text_range == FindByNumbersTextRange.ChapterVerses)
            {
                FindByNumbersTextRangeWord();
            }
        }
        FindByNumbersControls_Enter(sender, e);
    }
    private void FindByNumbersTextRangeWord()
    {
        m_find_by_numbers_text_range = FindByNumbersTextRange.Word;
        FindByNumbersTextRangeLabel.Text = "WORD";
        FindByNumbersTextRangeLabel.Refresh();
        ToolTip.SetToolTip(FindByNumbersTextRangeLabel, "for any word matching condition(s)");
        RefreshFindByNumbersControls(true, false, false, true, true, true);
    }
    private void FindByNumbersTextRangeWordRange()
    {
        m_find_by_numbers_text_range = FindByNumbersTextRange.WordRange;
        FindByNumbersTextRangeLabel.Text = "WORD  RANGE";
        FindByNumbersTextRangeLabel.Refresh();
        ToolTip.SetToolTip(FindByNumbersTextRangeLabel, "for any consecutive words matching condition(s)");
        RefreshFindByNumbersControls(false, false, true, true, true, true);
    }
    private void FindByNumbersTextRangeVerse()
    {
        m_find_by_numbers_text_range = FindByNumbersTextRange.Verse;
        FindByNumbersTextRangeLabel.Text = "VERSE";
        FindByNumbersTextRangeLabel.Refresh();
        ToolTip.SetToolTip(FindByNumbersTextRangeLabel, "for any verse matching condition(s)");
        RefreshFindByNumbersControls(true, false, true, true, true, true);
    }
    private void FindByNumbersTextRangeVerseRange()
    {
        m_find_by_numbers_text_range = FindByNumbersTextRange.VerseRange;
        FindByNumbersTextRangeLabel.Text = "VERSE  RANGE";
        FindByNumbersTextRangeLabel.Refresh();
        ToolTip.SetToolTip(FindByNumbersTextRangeLabel, "for any consecutive verses matching condition(s)");
        RefreshFindByNumbersControls(false, true, true, true, true, true);
    }
    private void FindByNumbersTextRangeChapter()
    {
        m_find_by_numbers_text_range = FindByNumbersTextRange.Chapter;
        FindByNumbersTextRangeLabel.Text = "CHAPTER";
        FindByNumbersTextRangeLabel.Refresh();
        ToolTip.SetToolTip(FindByNumbersTextRangeLabel, "for any chapter matching condition(s)");
        RefreshFindByNumbersControls(true, true, true, true, true, true);
    }
    private void FindByNumbersTextRangeAllVersesInChapter()
    {
        m_find_by_numbers_text_range = FindByNumbersTextRange.ChapterVerses;
        FindByNumbersTextRangeLabel.Text = "CHAPTER  VERSES";  // not wide enough to display TER
        FindByNumbersTextRangeLabel.Refresh();
        ToolTip.SetToolTip(FindByNumbersTextRangeLabel, "for any chapter with all verses matching condition(s)");
        RefreshFindByNumbersControls(false, false, true, true, true, true);
    }
    private void RefreshFindByNumbersControls(bool enable_number,
                               bool enable_verses,
                               bool enable_words,
                               bool enable_letters,
                               bool enable_unique_letters,
                               bool enable_value
                              )
    {
        FindByNumbersNumberLabel.Enabled = enable_number;
        FindByNumbersNumberOperatorLabel.Enabled = enable_number && (FindByNumbersNumberNumberTypeLabel.Text.Length == 0);
        FindByNumbersNumberNumericUpDown.Enabled = enable_number && (FindByNumbersNumberNumberTypeLabel.Text.Length == 0);
        FindByNumbersNumberNumberTypeLabel.Enabled = enable_number;

        FindByNumbersVersesLabel.Enabled = enable_verses;
        FindByNumbersVersesOperatorLabel.Enabled = enable_verses && (FindByNumbersVersesNumberTypeLabel.Text.Length == 0);
        FindByNumbersVersesNumericUpDown.Enabled = enable_verses && (FindByNumbersVersesNumberTypeLabel.Text.Length == 0);
        FindByNumbersVersesNumberTypeLabel.Enabled = enable_verses;
        if (m_find_by_numbers_text_range == FindByNumbersTextRange.VerseRange)
        {
            FindByNumbersVersesNumberTypeLabel.Text = "";
            FindByNumbersVersesNumberTypeLabel.Enabled = false;
        }

        FindByNumbersWordsLabel.Enabled = enable_words;
        FindByNumbersWordsOperatorLabel.Enabled = enable_words && (FindByNumbersWordsNumberTypeLabel.Text.Length == 0);
        FindByNumbersWordsNumericUpDown.Enabled = enable_words && (FindByNumbersWordsNumberTypeLabel.Text.Length == 0);
        FindByNumbersWordsNumberTypeLabel.Enabled = enable_words;
        if (m_find_by_numbers_text_range == FindByNumbersTextRange.WordRange)
        {
            FindByNumbersWordsNumberTypeLabel.Text = "";
            FindByNumbersWordsNumberTypeLabel.Enabled = false;
        }

        FindByNumbersLettersLabel.Enabled = enable_letters;
        FindByNumbersLettersOperatorLabel.Enabled = enable_letters && (FindByNumbersLettersNumberTypeLabel.Text.Length == 0);
        FindByNumbersLettersNumericUpDown.Enabled = enable_letters && (FindByNumbersLettersNumberTypeLabel.Text.Length == 0);
        FindByNumbersLettersNumberTypeLabel.Enabled = enable_letters;

        FindByNumbersUniqueLettersLabel.Enabled = enable_unique_letters;
        FindByNumbersUniqueLettersOperatorLabel.Enabled = enable_unique_letters && (FindByNumbersUniqueLettersNumberTypeLabel.Text.Length == 0);
        FindByNumbersUniqueLettersNumericUpDown.Enabled = enable_unique_letters && (FindByNumbersUniqueLettersNumberTypeLabel.Text.Length == 0);
        FindByNumbersUniqueLettersNumberTypeLabel.Enabled = enable_unique_letters;

        FindByNumbersValueLabel.Enabled = enable_value;
        FindByNumbersValueOperatorLabel.Enabled = enable_value && (FindByNumbersValueNumberTypeLabel.Text.Length == 0);
        FindByNumbersValueNumericUpDown.Enabled = enable_value && (FindByNumbersValueNumberTypeLabel.Text.Length == 0);
        FindByNumbersValueNumberTypeLabel.Enabled = enable_value;

        // Only EQ operator is supported in WordRange and VerseRange 
        if (
            (m_find_by_numbers_text_range == FindByNumbersTextRange.WordRange)
            ||
            (m_find_by_numbers_text_range == FindByNumbersTextRange.VerseRange)
           )
        {
            FindByNumbersNumberOperatorLabel.Text = "=";
            FindByNumbersNumberOperatorLabel.Enabled = false;

            FindByNumbersVersesOperatorLabel.Text = "=";
            FindByNumbersVersesOperatorLabel.Enabled = false;

            FindByNumbersWordsOperatorLabel.Text = "=";
            FindByNumbersWordsOperatorLabel.Enabled = false;

            FindByNumbersLettersOperatorLabel.Text = "=";
            FindByNumbersLettersOperatorLabel.Enabled = false;

            FindByNumbersUniqueLettersOperatorLabel.Text = "=";
            FindByNumbersUniqueLettersOperatorLabel.Enabled = false;

            FindByNumbersValueOperatorLabel.Text = "=";
            FindByNumbersValueOperatorLabel.Enabled = false;
        }

        // Only * operator is supported in Verse and Chapter
        if (
            (m_find_by_numbers_text_range != FindByNumbersTextRange.Verse)
            &&
            (m_find_by_numbers_text_range != FindByNumbersTextRange.Chapter)
           )
        {
            if (FindByNumbersNumberNumberTypeLabel.Text == "*") ResetControlNumberType(FindByNumbersNumberNumberTypeLabel);
            if (FindByNumbersVersesNumberTypeLabel.Text == "*") ResetControlNumberType(FindByNumbersVersesNumberTypeLabel);
            if (FindByNumbersWordsNumberTypeLabel.Text == "*") ResetControlNumberType(FindByNumbersWordsNumberTypeLabel);
            if (FindByNumbersLettersNumberTypeLabel.Text == "*") ResetControlNumberType(FindByNumbersLettersNumberTypeLabel);
            if (FindByNumbersUniqueLettersNumberTypeLabel.Text == "*") ResetControlNumberType(FindByNumbersUniqueLettersNumberTypeLabel);
            if (FindByNumbersValueNumberTypeLabel.Text == "*") ResetControlNumberType(FindByNumbersValueNumberTypeLabel);
        }
    }

    private void ResetControlNumberType(Control control)
    {
        (control as Control).Text = "";
        control.ForeColor = Color.Black;
        ToolTip.SetToolTip(control, "no number");
    }
    private void FindByNumbersControl_EnabledChanged(object sender, EventArgs e)
    {
        Control control = sender as Control;
        if (control != null)
        {
            control.BackColor = (control.Enabled) ? SystemColors.Window : SystemColors.Control;
        }
    }
    private void FindByNumbersOperatorLabel_Click(object sender, EventArgs e)
    {
        Control control = sender as Control;
        if (control != null)
        {
            if (ModifierKeys == Keys.Shift)
            {
                if (control.Text == "=")
                {
                    control.Text = "≥";
                    ToolTip.SetToolTip(control, "greater than or equal");
                }
                else if (control.Text == "≠")
                {
                    control.Text = "=";
                    ToolTip.SetToolTip(control, "equal");
                }
                else if (control.Text == "<")
                {
                    control.Text = "≠";
                    ToolTip.SetToolTip(control, "not equal");
                }
                else if (control.Text == "≤")
                {
                    control.Text = "<";
                    ToolTip.SetToolTip(control, "less than");
                }
                else if (control.Text == ">")
                {
                    control.Text = "≤";
                    ToolTip.SetToolTip(control, "less than or equal");
                }
                else if (control.Text == "≥")
                {
                    control.Text = ">";
                    ToolTip.SetToolTip(control, "greater than");
                }
                else
                {
                    // do nothing
                }
            }
            else
            {
                if (control.Text == "=")
                {
                    control.Text = "≠";
                    ToolTip.SetToolTip(control, "not equal");
                }
                else if (control.Text == "≠")
                {
                    control.Text = "<";
                    ToolTip.SetToolTip(control, "less than");
                }
                else if (control.Text == "<")
                {
                    control.Text = "≤";
                    ToolTip.SetToolTip(control, "less than or equal");
                }
                else if (control.Text == "≤")
                {
                    control.Text = ">";
                    ToolTip.SetToolTip(control, "greater than");
                }
                else if (control.Text == ">")
                {
                    control.Text = "≥";
                    ToolTip.SetToolTip(control, "greater than or equal");
                }
                else if (control.Text == "≥")
                {
                    control.Text = "=";
                    ToolTip.SetToolTip(control, "equal");
                }
                else
                {
                    // do nothing
                }
            }
        }

        FindByNumbersControls_Enter(sender, e);
    }
    private void FindByNumbersNumberTypeLabel_Click(object sender, EventArgs e)
    {
        Control control = sender as Control;
        if (control != null)
        {
            if (ModifierKeys == Keys.Shift)
            {
                if (control.Text == "")
                {
                    control.Text = "PC";
                    control.ForeColor = GetNumberTypeColor(4);
                    ToolTip.SetToolTip(control, "pure composite = additive composite with composite digits");
                }
                else if (control.Text == "PC")
                {
                    control.Text = "AC";
                    control.ForeColor = GetNumberTypeColor(114);
                    ToolTip.SetToolTip(control, "additive composite = composite with composite digit sum");
                }
                else if (control.Text == "AC")
                {
                    control.Text = "C";
                    control.ForeColor = GetNumberTypeColor(14);
                    ToolTip.SetToolTip(control, "composite = not prime");
                }
                else if (control.Text == "C")
                {
                    control.Text = "PP";
                    control.ForeColor = GetNumberTypeColor(313);
                    ToolTip.SetToolTip(control, "pure prime = additive prime with prime digits");
                }
                else if (control.Text == "PP")
                {
                    control.Text = "AP";
                    control.ForeColor = GetNumberTypeColor(47);
                    ToolTip.SetToolTip(control, "additive prime = prime with prime digit sum");
                }
                else if (control.Text == "AP")
                {
                    control.Text = "P";
                    control.ForeColor = GetNumberTypeColor(19);
                    ToolTip.SetToolTip(control, "prime = only divisible by 1 and itself");
                }
                else if (control.Text == "P")
                {
                    if (
                        (m_find_by_numbers_text_range == FindByNumbersTextRange.Verse)
                        ||
                        (m_find_by_numbers_text_range == FindByNumbersTextRange.Chapter)
                       )
                    {
                        control.Text = "*";
                        control.ForeColor = Color.Black;
                        ToolTip.SetToolTip(control, "same number");
                    }
                    else
                    {
                        control.Text = "";
                        control.ForeColor = Color.Black;
                        ToolTip.SetToolTip(control, "no number");

                    }
                }
                else if (control.Text == "*")
                {
                    control.Text = "";
                    control.ForeColor = Color.Black;
                    ToolTip.SetToolTip(control, "no number");
                }
                else
                {
                    // do nothing
                }
            }
            else
            {
                if (control.Text == "")
                {
                    if (
                        (m_find_by_numbers_text_range == FindByNumbersTextRange.Verse)
                        ||
                        (m_find_by_numbers_text_range == FindByNumbersTextRange.Chapter)
                       )
                    {
                        control.Text = "*";
                        control.ForeColor = Color.Black;
                        ToolTip.SetToolTip(control, "same number");
                    }
                    else
                    {
                        control.Text = "P";
                        control.ForeColor = GetNumberTypeColor(19);
                        ToolTip.SetToolTip(control, "prime = only divisible by 1 and itself");
                    }
                }
                else if (control.Text == "*")
                {
                    control.Text = "P";
                    control.ForeColor = GetNumberTypeColor(19);
                    ToolTip.SetToolTip(control, "prime = only divisible by 1 and itself");
                }
                else if (control.Text == "P")
                {
                    control.Text = "AP";
                    control.ForeColor = GetNumberTypeColor(47);
                    ToolTip.SetToolTip(control, "additive prime = prime with prime digit sum");
                }
                else if (control.Text == "AP")
                {
                    control.Text = "PP";
                    control.ForeColor = GetNumberTypeColor(313);
                    ToolTip.SetToolTip(control, "pure prime = additive prime with prime digits");
                }
                else if (control.Text == "PP")
                {
                    control.Text = "C";
                    control.ForeColor = GetNumberTypeColor(14);
                    ToolTip.SetToolTip(control, "composite = not prime");
                }
                else if (control.Text == "C")
                {
                    control.Text = "AC";
                    control.ForeColor = GetNumberTypeColor(114);
                    ToolTip.SetToolTip(control, "additive composite = composite with composite digit sum");
                }
                else if (control.Text == "AC")
                {
                    control.Text = "PC";
                    control.ForeColor = GetNumberTypeColor(4);
                    ToolTip.SetToolTip(control, "pure composite = additive composite with composite digits");
                }
                else if (control.Text == "PC")
                {
                    control.Text = "";
                    control.ForeColor = Color.Black;
                    ToolTip.SetToolTip(control, "no number");
                }
                else
                {
                    // do nothing
                }
            }

            if (control.Name.Contains("NumbersNumber"))
            {
                FindByNumbersNumberOperatorLabel.Enabled = (control.Text.Length == 0) && (m_find_by_numbers_text_range != FindByNumbersTextRange.WordRange);
                FindByNumbersNumberNumericUpDown.Enabled = (control.Text.Length == 0);
                if (control.Text.Length > 0) FindByNumbersNumberOperatorLabel.Text = "=";
            }
            else if (control.Name.Contains("Verses"))
            {
                FindByNumbersVersesOperatorLabel.Enabled = (control.Text.Length == 0) && (m_find_by_numbers_text_range != FindByNumbersTextRange.WordRange);
                FindByNumbersVersesNumericUpDown.Enabled = (control.Text.Length == 0);
                if (control.Text.Length > 0) FindByNumbersVersesOperatorLabel.Text = "=";
            }
            else if (control.Name.Contains("Words"))
            {
                FindByNumbersWordsOperatorLabel.Enabled = (control.Text.Length == 0) && (m_find_by_numbers_text_range != FindByNumbersTextRange.WordRange);
                FindByNumbersWordsNumericUpDown.Enabled = (control.Text.Length == 0);
                if (control.Text.Length > 0) FindByNumbersWordsOperatorLabel.Text = "=";
            }
            else if (control.Name.Contains("UniqueLetters"))
            {
                FindByNumbersUniqueLettersOperatorLabel.Enabled = (control.Text.Length == 0) && (m_find_by_numbers_text_range != FindByNumbersTextRange.WordRange);
                FindByNumbersUniqueLettersNumericUpDown.Enabled = (control.Text.Length == 0);
                if (control.Text.Length > 0) FindByNumbersUniqueLettersOperatorLabel.Text = "=";
            }
            else if (control.Name.Contains("Letters"))
            {
                FindByNumbersLettersOperatorLabel.Enabled = (control.Text.Length == 0) && (m_find_by_numbers_text_range != FindByNumbersTextRange.WordRange);
                FindByNumbersLettersNumericUpDown.Enabled = (control.Text.Length == 0);
                if (control.Text.Length > 0) FindByNumbersLettersOperatorLabel.Text = "=";
            }
            else if (control.Name.Contains("Value"))
            {
                FindByNumbersValueOperatorLabel.Enabled = (control.Text.Length == 0) && (m_find_by_numbers_text_range != FindByNumbersTextRange.WordRange);
                FindByNumbersValueNumericUpDown.Enabled = (control.Text.Length == 0);
                if (control.Text.Length > 0) FindByNumbersValueOperatorLabel.Text = "=";
            }
            else
            {
                // do nothing
            }

            FindByNumbersControls_Enter(sender, e);
        }
    }
    private void FindByNumbersControls_Enter(object sender, EventArgs e)
    {
        this.AcceptButton = FindByNumbersButton;
        FindType = FindType.Numbers;
    }
    private void FindByNumbersNumericUpDown_Leave(object sender, EventArgs e)
    {
        NumericUpDown control = sender as NumericUpDown;
        if (control != null)
        {
            if (String.IsNullOrEmpty(control.Text))
            {
                control.Value = 0;
                control.Refresh();
            }
        }
    }
    private void FindByNumbersNumericUpDown_ValueChanged(object sender, EventArgs e)
    {
        NumericUpDown control = sender as NumericUpDown;
        if (control != null)
        {
            if (control.Value > 0)
            {
                FindByNumbers();
            }
        }
    }
    private void FindByNumbersButton_Click(object sender, EventArgs e)
    {
        // fire FindByNumbersNumericUpDown_Enter with correct sender
        FindByNumbersButton.Focus();

        FindByNumbers();
    }
    private void FindByNumbers()
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            if (m_client != null)
            {
                PrepareNewSearch();

                // 1. prime types
                string number_symbol = FindByNumbersNumberNumberTypeLabel.Enabled ? FindByNumbersNumberNumberTypeLabel.Text : "";
                NumberType number_number_type = (number_symbol == "PP") ? NumberType.PurePrime :
                                  (number_symbol == "AP") ? NumberType.AdditivePrime :
                                  (number_symbol == "P") ? NumberType.Prime :
                                  (number_symbol == "PC") ? NumberType.PureComposite :
                                  (number_symbol == "AC") ? NumberType.AdditiveComposite :
                                  (number_symbol == "C") ? NumberType.Composite :
                                  (number_symbol == "*") ? NumberType.Any :
                                                           NumberType.None;

                string verse_count_symbol = FindByNumbersVersesNumberTypeLabel.Enabled ? FindByNumbersVersesNumberTypeLabel.Text : "";
                NumberType verse_count_number_type = (verse_count_symbol == "PP") ? NumberType.PurePrime :
                                       (verse_count_symbol == "AP") ? NumberType.AdditivePrime :
                                       (verse_count_symbol == "P") ? NumberType.Prime :
                                       (verse_count_symbol == "PC") ? NumberType.PureComposite :
                                       (verse_count_symbol == "AC") ? NumberType.AdditiveComposite :
                                       (verse_count_symbol == "C") ? NumberType.Composite :
                                       (verse_count_symbol == "*") ? NumberType.Any :
                                                                     NumberType.None;

                string word_count_symbol = FindByNumbersWordsNumberTypeLabel.Enabled ? FindByNumbersWordsNumberTypeLabel.Text : "";
                NumberType word_count_number_type = (word_count_symbol == "PP") ? NumberType.PurePrime :
                                      (word_count_symbol == "AP") ? NumberType.AdditivePrime :
                                      (word_count_symbol == "P") ? NumberType.Prime :
                                      (word_count_symbol == "PC") ? NumberType.PureComposite :
                                      (word_count_symbol == "AC") ? NumberType.AdditiveComposite :
                                      (word_count_symbol == "C") ? NumberType.Composite :
                                      (word_count_symbol == "*") ? NumberType.Any :
                                                                   NumberType.None;

                string letter_count_symbol = FindByNumbersLettersNumberTypeLabel.Enabled ? FindByNumbersLettersNumberTypeLabel.Text : "";
                NumberType letter_count_number_type = (letter_count_symbol == "PP") ? NumberType.PurePrime :
                                        (letter_count_symbol == "AP") ? NumberType.AdditivePrime :
                                        (letter_count_symbol == "P") ? NumberType.Prime :
                                        (letter_count_symbol == "PC") ? NumberType.PureComposite :
                                        (letter_count_symbol == "AC") ? NumberType.AdditiveComposite :
                                        (letter_count_symbol == "C") ? NumberType.Composite :
                                        (letter_count_symbol == "*") ? NumberType.Any :
                                                                       NumberType.None;

                string unique_letter_count_symbol = FindByNumbersUniqueLettersNumberTypeLabel.Enabled ? FindByNumbersUniqueLettersNumberTypeLabel.Text : "";
                NumberType unique_letter_count_number_type = (unique_letter_count_symbol == "PP") ? NumberType.PurePrime :
                                               (unique_letter_count_symbol == "AP") ? NumberType.AdditivePrime :
                                               (unique_letter_count_symbol == "P") ? NumberType.Prime :
                                               (unique_letter_count_symbol == "PC") ? NumberType.PureComposite :
                                               (unique_letter_count_symbol == "AC") ? NumberType.AdditiveComposite :
                                               (unique_letter_count_symbol == "C") ? NumberType.Composite :
                                               (unique_letter_count_symbol == "*") ? NumberType.Any :
                                                                                     NumberType.None;

                string value_symbol = FindByNumbersValueNumberTypeLabel.Enabled ? FindByNumbersValueNumberTypeLabel.Text : "";
                NumberType value_number_type = (value_symbol == "PP") ? NumberType.PurePrime :
                                        (value_symbol == "AP") ? NumberType.AdditivePrime :
                                        (value_symbol == "P") ? NumberType.Prime :
                                        (value_symbol == "PC") ? NumberType.PureComposite :
                                        (value_symbol == "AC") ? NumberType.AdditiveComposite :
                                        (value_symbol == "C") ? NumberType.Composite :
                                        (value_symbol == "*") ? NumberType.Any :
                                                                NumberType.None;

                // 2. numbers
                int number = FindByNumbersNumberNumericUpDown.Enabled ? ((number_number_type == NumberType.None) ? (int)FindByNumbersNumberNumericUpDown.Value : 0) : 0;
                int verse_count = FindByNumbersVersesNumericUpDown.Enabled ? ((verse_count_number_type == NumberType.None) ? (int)FindByNumbersVersesNumericUpDown.Value : 0) : 0;
                int word_count = FindByNumbersWordsNumericUpDown.Enabled ? ((word_count_number_type == NumberType.None) ? (int)FindByNumbersWordsNumericUpDown.Value : 0) : 0;
                int letter_count = FindByNumbersLettersNumericUpDown.Enabled ? ((letter_count_number_type == NumberType.None) ? (int)FindByNumbersLettersNumericUpDown.Value : 0) : 0;
                int unique_letter_count = FindByNumbersUniqueLettersNumericUpDown.Enabled ? ((unique_letter_count_number_type == NumberType.None) ? (int)FindByNumbersUniqueLettersNumericUpDown.Value : 0) : 0;
                long value = FindByNumbersValueNumericUpDown.Enabled ? ((value_number_type == NumberType.None) ? (long)FindByNumbersValueNumericUpDown.Value : 0) : 0;

                // 3. comparison operators = ≠ < ≤ > ≥
                string number_operator_symbol = FindByNumbersNumberOperatorLabel.Text;
                ComparisonOperator number_operator =
                    (number_operator_symbol == "=") ? ComparisonOperator.Equal :
                    (number_operator_symbol == "≠") ? ComparisonOperator.NotEqual :
                    (number_operator_symbol == "<") ? ComparisonOperator.LessThan :
                    (number_operator_symbol == "≤") ? ComparisonOperator.LessThanOrEqual :
                    (number_operator_symbol == ">") ? ComparisonOperator.GreaterThan :
                    (number_operator_symbol == "≥") ? ComparisonOperator.GreaterThanOrEqual :
                                                      ComparisonOperator.Unknown;
                string verse_count_operator_symbol = FindByNumbersVersesOperatorLabel.Text;
                ComparisonOperator verse_count_operator =
                    (verse_count_operator_symbol == "=") ? ComparisonOperator.Equal :
                    (verse_count_operator_symbol == "≠") ? ComparisonOperator.NotEqual :
                    (verse_count_operator_symbol == "<") ? ComparisonOperator.LessThan :
                    (verse_count_operator_symbol == "≤") ? ComparisonOperator.LessThanOrEqual :
                    (verse_count_operator_symbol == ">") ? ComparisonOperator.GreaterThan :
                    (verse_count_operator_symbol == "≥") ? ComparisonOperator.GreaterThanOrEqual :
                                                           ComparisonOperator.Unknown;
                string word_count_operator_symbol = FindByNumbersWordsOperatorLabel.Text;
                ComparisonOperator word_count_operator =
                    (word_count_operator_symbol == "=") ? ComparisonOperator.Equal :
                    (word_count_operator_symbol == "≠") ? ComparisonOperator.NotEqual :
                    (word_count_operator_symbol == "<") ? ComparisonOperator.LessThan :
                    (word_count_operator_symbol == "≤") ? ComparisonOperator.LessThanOrEqual :
                    (word_count_operator_symbol == ">") ? ComparisonOperator.GreaterThan :
                    (word_count_operator_symbol == "≥") ? ComparisonOperator.GreaterThanOrEqual :
                                                          ComparisonOperator.Unknown;
                string letter_count_operator_symbol = FindByNumbersLettersOperatorLabel.Text;
                ComparisonOperator letter_count_operator =
                    (letter_count_operator_symbol == "=") ? ComparisonOperator.Equal :
                    (letter_count_operator_symbol == "≠") ? ComparisonOperator.NotEqual :
                    (letter_count_operator_symbol == "<") ? ComparisonOperator.LessThan :
                    (letter_count_operator_symbol == "≤") ? ComparisonOperator.LessThanOrEqual :
                    (letter_count_operator_symbol == ">") ? ComparisonOperator.GreaterThan :
                    (letter_count_operator_symbol == "≥") ? ComparisonOperator.GreaterThanOrEqual :
                                                            ComparisonOperator.Unknown;
                string unique_letter_count_operator_symbol = FindByNumbersUniqueLettersOperatorLabel.Text;
                ComparisonOperator unique_letter_count_operator =
                    (unique_letter_count_operator_symbol == "=") ? ComparisonOperator.Equal :
                    (unique_letter_count_operator_symbol == "≠") ? ComparisonOperator.NotEqual :
                    (unique_letter_count_operator_symbol == "<") ? ComparisonOperator.LessThan :
                    (unique_letter_count_operator_symbol == "≤") ? ComparisonOperator.LessThanOrEqual :
                    (unique_letter_count_operator_symbol == ">") ? ComparisonOperator.GreaterThan :
                    (unique_letter_count_operator_symbol == "≥") ? ComparisonOperator.GreaterThanOrEqual :
                                                                   ComparisonOperator.Unknown;
                string value_operator_symbol = FindByNumbersValueOperatorLabel.Text;
                ComparisonOperator value_operator =
                    (value_operator_symbol == "=") ? ComparisonOperator.Equal :
                    (value_operator_symbol == "≠") ? ComparisonOperator.NotEqual :
                    (value_operator_symbol == "<") ? ComparisonOperator.LessThan :
                    (value_operator_symbol == "≤") ? ComparisonOperator.LessThanOrEqual :
                    (value_operator_symbol == ">") ? ComparisonOperator.GreaterThan :
                    (value_operator_symbol == "≥") ? ComparisonOperator.GreaterThanOrEqual :
                                                     ComparisonOperator.Unknown;

                string text = "";
                if ((m_find_by_numbers_text_range == FindByNumbersTextRange.Word) || (m_find_by_numbers_text_range == FindByNumbersTextRange.Verse) || (m_find_by_numbers_text_range == FindByNumbersTextRange.Chapter))
                {
                    text += "number" + number_operator_symbol + ((number > 0) ? number.ToString() : ((number_number_type != NumberType.None) ? FindByNumbersNumberNumberTypeLabel.Text : "*")) + ", ";
                }
                if ((m_find_by_numbers_text_range == FindByNumbersTextRange.VerseRange) || (m_find_by_numbers_text_range == FindByNumbersTextRange.Chapter))
                {
                    text += "verses" + verse_count_operator_symbol + ((verse_count > 0) ? verse_count.ToString() : ((verse_count_number_type != NumberType.None) ? FindByNumbersVersesNumberTypeLabel.Text : "*")) + ", ";
                }
                text += "words" + word_count_operator_symbol + ((word_count > 0) ? word_count.ToString() : ((word_count_number_type != NumberType.None) ? FindByNumbersWordsNumberTypeLabel.Text : "*")) + ", ";
                text += "letters" + letter_count_operator_symbol + ((letter_count > 0) ? letter_count.ToString() : ((letter_count_number_type != NumberType.None) ? FindByNumbersLettersNumberTypeLabel.Text : "*")) + ", ";
                text += "unique" + unique_letter_count_operator_symbol + ((unique_letter_count > 0) ? unique_letter_count.ToString() : ((unique_letter_count_number_type != NumberType.None) ? FindByNumbersUniqueLettersNumberTypeLabel.Text : "*")) + ", ";
                text += "value" + value_operator_symbol + ((value > 0) ? value.ToString() : ((value_number_type != NumberType.None) ? FindByNumbersValueNumberTypeLabel.Text : "*")) + "";

                switch (m_find_by_numbers_text_range)
                {
                    case FindByNumbersTextRange.Word:
                        {
                            m_client.FindWords(number, letter_count, unique_letter_count, value,
                                               number_number_type, letter_count_number_type, unique_letter_count_number_type, value_number_type,
                                               number_operator, letter_count_operator, unique_letter_count_operator, value_operator);
                        }
                        break;
                    case FindByNumbersTextRange.WordRange:
                        {
                            m_client.FindWordRanges(word_count, letter_count, unique_letter_count, value,
                                                    word_count_number_type, letter_count_number_type, unique_letter_count_number_type, value_number_type,
                                                    word_count_operator, letter_count_operator, unique_letter_count_operator, value_operator);
                        }
                        break;
                    case FindByNumbersTextRange.Verse:
                        {
                            m_client.FindVerses(number, word_count, letter_count, unique_letter_count, value,
                                                number_number_type, word_count_number_type, letter_count_number_type, unique_letter_count_number_type, value_number_type,
                                                number_operator, word_count_operator, letter_count_operator, unique_letter_count_operator, value_operator);
                        }
                        break;
                    case FindByNumbersTextRange.VerseRange:
                        {
                            m_client.FindVerseRanges(verse_count, word_count, letter_count, unique_letter_count, value,
                                                     verse_count_number_type, word_count_number_type, letter_count_number_type, unique_letter_count_number_type, value_number_type,
                                                     verse_count_operator, word_count_operator, letter_count_operator, unique_letter_count_operator, value_operator);
                        }
                        break;
                    case FindByNumbersTextRange.Chapter:
                        {
                            m_client.FindChapters(number, verse_count, word_count, letter_count, unique_letter_count, value,
                                                  number_number_type, verse_count_number_type, word_count_number_type, letter_count_number_type, unique_letter_count_number_type, value_number_type,
                                                  number_operator, verse_count_operator, word_count_operator, letter_count_operator, unique_letter_count_operator, value_operator);
                        }
                        break;
                    case FindByNumbersTextRange.ChapterVerses:
                        {
                            m_client.FindChaptersWithAllVersesHaving(word_count, letter_count, unique_letter_count, value,
                                                                     word_count_number_type, letter_count_number_type, unique_letter_count_number_type, value_number_type,
                                                                     word_count_operator, letter_count_operator, unique_letter_count_operator, value_operator);
                        }
                        break;
                }

                int match_count = -1;
                switch (m_find_by_numbers_text_range)
                {
                    case FindByNumbersTextRange.Word:
                        {
                            if (m_client.FoundWords != null)
                            {
                                match_count = m_client.FoundWords.Count;
                                m_find_result_header = match_count + ((match_count == 1) ? " word" : " words") + " in " + m_client.FoundVerses.Count + ((m_client.FoundVerses.Count == 1) ? " verse" : " verses") + " with " + text + " in " + FindScopeLabel.Text;

                                DisplayFoundVerses(true, false);
                            }
                        }
                        break;
                    case FindByNumbersTextRange.WordRange:
                        {
                            if (m_client.FoundWordRanges != null)
                            {
                                match_count = m_client.FoundWordRanges.Count;
                                m_find_result_header = match_count + ((match_count == 1) ? " range" : " ranges") + " in " + m_client.FoundVerses.Count + ((m_client.FoundVerses.Count == 1) ? " verse" : " verses") + " with " + text + " in " + FindScopeLabel.Text;

                                DisplayFoundVerses(true, false);
                            }
                        }
                        break;
                    case FindByNumbersTextRange.Verse:
                        {
                            if (m_client.FoundVerses != null)
                            {
                                match_count = m_client.FoundVerses.Count;
                                m_find_result_header = match_count + ((match_count == 1) ? " verse" : " verses") + " with " + text + " in " + FindScopeLabel.Text;

                                DisplayFoundVerses(true, false);
                            }
                        }
                        break;
                    case FindByNumbersTextRange.VerseRange:
                        {
                            if (m_client.FoundVerseRanges != null)
                            {
                                match_count = m_client.FoundVerseRanges.Count;
                                m_find_result_header = match_count + ((match_count == 1) ? " range" : " ranges") + " in " + m_client.FoundVerses.Count + ((m_client.FoundVerses.Count == 1) ? " verse" : " verses") + " with " + text + " in " + FindScopeLabel.Text;

                                DisplayFoundVerses(true, false);
                            }
                        }
                        break;
                    case FindByNumbersTextRange.Chapter:
                        {
                            if (m_client.FoundChapters != null)
                            {
                                match_count = m_client.FoundChapters.Count;
                                m_find_result_header = match_count + ((match_count == 1) ? " chapter" : " chapters") + " with " + text + " in " + FindScopeLabel.Text;

                                DisplayFoundChapters(true);
                            }
                        }
                        break;
                    case FindByNumbersTextRange.ChapterVerses:
                        {
                            if (m_client.FoundChapters != null)
                            {
                                match_count = m_client.FoundChapters.Count;
                                m_find_result_header = match_count + ((match_count == 1) ? " chapter" : " chapters") + " with all verses having " + text + " in " + FindScopeLabel.Text;

                                DisplayFoundChapters(true);
                            }
                        }
                        break;
                }
            }
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }

    private FindByProstrationType m_find_by_prostration_type = FindByProstrationType.None;
    private void FindByProstrationTypeLabel_Click(object sender, EventArgs e)
    {
        FindByProstrationTypeControls_Enter(sender, e);
    }
    private void FindByProstrationTypeLevelLabel_Click(object sender, EventArgs e)
    {
        FindByProstrationTypeControls_Enter(sender, e);
    }
    private void FindByProstrationTypeRadioButton_CheckedChanged(object sender, EventArgs e)
    {
    }
    private void FindByProstrationTypeControls_Enter(object sender, EventArgs e)
    {
        this.AcceptButton = null;
        FindType = FindType.Prostration;
    }
    private void FindByProstrationTypeButton_Click(object sender, EventArgs e)
    {
        FindByProstration();
    }
    private void FindByProstration()
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            if (m_client != null)
            {
                PrepareNewSearch();

                if (!FindByProstrationTypeObligatoryRadioButton.Checked && !FindByProstrationTypeRecommendedRadioButton.Checked)
                {
                    m_find_by_prostration_type = FindByProstrationType.None;
                }
                else if (FindByProstrationTypeObligatoryRadioButton.Checked && !FindByProstrationTypeRecommendedRadioButton.Checked)
                {
                    m_find_by_prostration_type = FindByProstrationType.Obligatory;
                }
                else if (!FindByProstrationTypeObligatoryRadioButton.Checked && FindByProstrationTypeRecommendedRadioButton.Checked)
                {
                    m_find_by_prostration_type = FindByProstrationType.Recommended;
                }
                else if (FindByProstrationTypeObligatoryRadioButton.Checked && FindByProstrationTypeRecommendedRadioButton.Checked)
                {
                    m_find_by_prostration_type = FindByProstrationType.Both;
                }

                m_client.FindVerses(m_find_by_prostration_type);
                if (m_client.FoundVerses != null)
                {
                    int verse_count = m_client.FoundVerses.Count;
                    m_find_result_header = verse_count + ((verse_count == 1) ? " verse" : " verses") + " with " + m_find_by_prostration_type.ToString() + " prostrations" + " in " + FindScopeLabel.Text;

                    DisplayFoundVerses(true, false);
                }
            }
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }

    private FindByRevelationPlace m_find_by_revelation_place = FindByRevelationPlace.None;
    private void FindByRevelationPlaceLabel_Click(object sender, EventArgs e)
    {
        FindByRevelationPlaceControls_Enter(sender, e);
    }
    private void FindByRevelationPlaceRadioButton_CheckedChanged(object sender, EventArgs e)
    {
    }
    private void FindByRevelationPlaceControls_Enter(object sender, EventArgs e)
    {
        this.AcceptButton = null;
        FindType = FindType.Revelation;
    }
    private void FindByRevelationPlaceButton_Click(object sender, EventArgs e)
    {
        FindByRevelation();
    }
    private void FindByRevelation()
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            if (m_client != null)
            {
                PrepareNewSearch();

                if (!FindByRevelationPlaceMakkahRadioButton.Checked && !FindByRevelationPlaceMedinaRadioButton.Checked)
                {
                    m_find_by_revelation_place = FindByRevelationPlace.None;
                }
                else if (FindByRevelationPlaceMakkahRadioButton.Checked && !FindByRevelationPlaceMedinaRadioButton.Checked)
                {
                    m_find_by_revelation_place = FindByRevelationPlace.Makkah;
                }
                else if (!FindByRevelationPlaceMakkahRadioButton.Checked && FindByRevelationPlaceMedinaRadioButton.Checked)
                {
                    m_find_by_revelation_place = FindByRevelationPlace.Medina;
                }
                else if (FindByRevelationPlaceMakkahRadioButton.Checked && FindByRevelationPlaceMedinaRadioButton.Checked)
                {
                    m_find_by_revelation_place = FindByRevelationPlace.Both;
                }

                m_client.FindChapters(m_find_by_revelation_place);
                if (m_client.FoundChapters != null)
                {
                    m_client.FoundVerses = new List<Verse>();
                    foreach (Chapter chapter in m_client.FoundChapters)
                    {
                        m_client.FoundVerses.AddRange(chapter.Verses);
                    }

                    if (m_client.FoundVerses != null)
                    {
                        int chapter_count = m_client.FoundChapters.Count;
                        m_find_result_header = chapter_count + ((chapter_count == 1) ? " chapter" : " chapters") + " revealed in " + m_find_by_revelation_place.ToString() + " in " + FindScopeLabel.Text;

                        DisplayFoundChapters(true);
                    }
                }
            }
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }

    private FindByFrequencySumType m_find_by_frequency_sum_type = FindByFrequencySumType.DuplicateLetters;
    private void FindByFrequencySumTypeLabel_Click(object sender, EventArgs e)
    {
        if (m_find_by_frequency_sum_type == FindByFrequencySumType.NoDuplicateLetters)
        {
            m_find_by_frequency_sum_type = FindByFrequencySumType.DuplicateLetters;
            FindByFrequencySumTypeLabel.Text = "DUPLICATE LETTERS";
            FindByFrequencySumTypeLabel.BackColor = Color.PaleVioletRed;
            ToolTip.SetToolTip(FindByFrequencySumTypeLabel, "include duplicate phrase letters");
        }
        else if (m_find_by_frequency_sum_type == FindByFrequencySumType.DuplicateLetters)
        {
            m_find_by_frequency_sum_type = FindByFrequencySumType.NoDuplicateLetters;
            FindByFrequencySumTypeLabel.Text = "NO DUPLICATE LETTERS";
            FindByFrequencySumTypeLabel.BackColor = Color.PaleVioletRed;
            ToolTip.SetToolTip(FindByFrequencySumTypeLabel, "exclude duplicate phrase letters");
        }
        FindByFrequencyControls_Enter(sender, e);
        DisplayPhraseLetterFrequencies();
    }
    private void FindByFrequencyControls_Enter(object sender, EventArgs e)
    {
        SearchGroupBox_Leave(sender, e);

        this.AcceptButton = FindByFrequencySumButton;
        FindType = FindType.Frequency;
    }
    private void FindByFrequencyPhraseLabel_Click(object sender, EventArgs e)
    {
        LinkLabel_Click(sender, e);
    }
    private void FindByFrequencyPhraseTextBox_TextChanged(object sender, EventArgs e)
    {
        DrawingPictureBoxEx.Visible = false;

        if (m_found_verses_displayed)
        {
            DisplaySelection(sender, false);
            MainTextBox.SelectionStart = m_found_verse_double_click_target_position;
        }
        DisplayPhraseLetterFrequencies();
    }
    private void FindByFrequencySumNumericUpDown_ValueChanged(object sender, EventArgs e)
    {
        FindByFrequencySumNumericUpDown.ForeColor = GetNumberTypeColor((long)FindByFrequencySumNumericUpDown.Value);
        // don't do it, as search result will change the control value which would result in another search
        //if (sender == FindByFrequencySumNumericUpDown)
        //{
        //    FindByFrequencySum();
        //}
    }
    private void FindByFrequencySumNumericUpDown_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            FindByFrequencySum();
        }
    }
    private void FindByFrequencySumButton_Click(object sender, EventArgs e)
    {
        FindByFrequencySum();
    }
    private void FindByFrequencySum()
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            if (m_client != null)
            {
                PrepareNewSearch();

                m_text_selections.Clear();
                m_text_selection_index = -1;

                string phrase = FindByFrequencyPhraseTextBox.Text;
                int phrase_letter_frequency_sum = (int)FindByFrequencySumNumericUpDown.Value;
                if (phrase.Length > 0)
                {
                    if (phrase_letter_frequency_sum > 0)
                    {
                        m_client.FindVerses(phrase, phrase_letter_frequency_sum, m_find_by_frequency_sum_type);
                        if (m_client.FoundVerses != null)
                        {
                            int verse_count = m_client.FoundVerses.Count;
                            m_find_result_header = verse_count + ((verse_count == 1) ? " verse" : " verses") + " with " + "letter frequency sum in " + phrase + " = " + phrase_letter_frequency_sum + ((m_find_by_frequency_sum_type == FindByFrequencySumType.DuplicateLetters) ? "" : " without duplicates") + " in " + FindScopeLabel.Text;

                            DisplayFoundVerses(true, false);
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
    private void SavePhraseLetterStatisticsButton_Click(object sender, EventArgs e)
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            SavePhraseLetterStatistics();
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }

    private Stack<int> m_navigation_undo_stack = new Stack<int>();
    private Stack<int> m_navigation_redo_stack = new Stack<int>();
    private void VerseFromNumerologyValue_Click(object sender, EventArgs e)
    {
        string verse_address = (sender as Label).Text;
        if (verse_address.Length > 0)
        {
            Verse verse = GetCurrentVerse();
            if (verse != null)
            {
                int current_verse_number = verse.Number;
                m_navigation_undo_stack.Push(current_verse_number);
                m_navigation_redo_stack.Clear();
                GotoVerse(verse_address);
            }
        }
    }
    private void DisplayVerse(Verse verse)
    {
        if (m_client != null)
        {
            if (Book.Instance != null)
            {
                if (verse != null)
                {
                    if (verse.Chapter != null)
                    {
                        int chapter_number = verse.Chapter.Number;
                        int verse_number_in_chapter = verse.NumberInChapter;
                        int verse_number = Book.Instance.GetVerseNumber(chapter_number, verse_number_in_chapter);
                        if ((verse_number >= VerseNumericUpDown.Minimum) && (verse_number <= VerseNumericUpDown.Maximum))
                        {
                            VerseNumericUpDown.Focus();
                            VerseNumericUpDown.Value = verse_number;
                        }
                    }
                }
            }
        }
    }
    private void GotoVerse(string verse_address)
    {
        if (m_client != null)
        {
            if (Book.Instance != null)
            {
                int chapter_number = 0;
                int verse_number_in_chapter = 0;
                int verse_number = 0;
                try
                {
                    string[] parts = verse_address.Split(':');
                    if (parts.Length == 2)
                    {
                        chapter_number = int.Parse(parts[0]);
                        verse_number_in_chapter = int.Parse(parts[1]);
                        verse_number = Book.Instance.GetVerseNumber(chapter_number, verse_number_in_chapter);
                        if ((verse_number >= VerseNumericUpDown.Minimum) && (verse_number <= VerseNumericUpDown.Maximum))
                        {
                            VerseNumericUpDown.Focus();
                            VerseNumericUpDown.Value = verse_number;
                        }
                    }
                    UndoValueNavigationLabel.ForeColor = (m_navigation_undo_stack.Count > 0) ? Color.Yellow : SystemColors.Info;
                    RedoValueNavigationLabel.ForeColor = (m_navigation_redo_stack.Count > 0) ? Color.Yellow : SystemColors.Info;
                }
                catch
                {
                    MessageBox.Show("Invalid verse number format.", Application.ProductName);
                }
            }
        }
    }
    private void UndoGotoVerse()
    {
        if (m_client != null)
        {
            if (m_navigation_undo_stack.Count > 0)
            {
                Verse verse = GetCurrentVerse();
                if (verse != null)
                {
                    int current_verse_number = verse.Number;
                    m_navigation_redo_stack.Push(current_verse_number);
                    int verse_number = m_navigation_undo_stack.Pop();
                    string verse_address = Book.Instance.Verses[verse_number - 1].Address;
                    GotoVerse(verse_address);
                }
            }
        }
    }
    private void RedoGotoVerse()
    {
        if (m_client != null)
        {
            if (m_navigation_redo_stack.Count > 0)
            {
                Verse verse = GetCurrentVerse();
                if (verse != null)
                {
                    int current_verse_number = verse.Number;
                    m_navigation_undo_stack.Push(current_verse_number);
                    int verse_number = m_navigation_redo_stack.Pop();
                    string verse_address = Book.Instance.Verses[verse_number - 1].Address;
                    GotoVerse(verse_address);
                }
            }
        }
    }
    private void UndoValueNavigationLabel_Click(object sender, EventArgs e)
    {
        UndoGotoVerse();
    }
    private void RedoValueNavigationLabel_Click(object sender, EventArgs e)
    {
        RedoGotoVerse();
    }

    private string m_current_drawing_type = "";
    private Bitmap m_bitmap = null;
    //private Graphics m_offscreen_graphics = null;
    private int m_offscreen_left = 0;
    private int m_offscreen_top = 0;
    private int m_zoomed_width = 0;
    private int m_zoomed_height = 0;
    private Point m_mouse_down_point;
    private Point m_mouse_up_point;
    private void MainForm_Paint(object sender, PaintEventArgs e)
    {
        if (m_bitmap != null)
        {
            this.DrawingPictureBoxEx.Image = m_bitmap;

            ZoomInLabel.Enabled = (m_graphics_zoom_factor <= (m_max_zoom_factor - m_zoom_factor_increment + m_error_margin));
            ZoomOutLabel.Enabled = (m_graphics_zoom_factor >= (m_min_zoom_factor + m_zoom_factor_increment - m_error_margin));
        }
    }
    private void PictureBoxEx_Resize(object sender, EventArgs e)
    {
        m_zoomed_width = (int)((float)DrawingPictureBoxEx.Width * DrawingPictureBoxEx.ZoomFactor);
        m_zoomed_height = (int)((float)DrawingPictureBoxEx.Height * DrawingPictureBoxEx.ZoomFactor);
    }
    private void PictureBoxEx_MouseHover(object sender, EventArgs e)
    {
        if (ModifierKeys == Keys.Control)
        {
            ToolTip.AutomaticDelay = 5000;
        }
        else
        {
            ToolTip.AutomaticDelay = 500;
        }
        DrawingPictureBoxEx.Focus(); // to enable zooming
    }
    private void PictureBoxEx_MouseDown(object sender, MouseEventArgs e)
    {
        m_mouse_down_point = e.Location;
    }
    private void PictureBoxEx_MouseMove(object sender, MouseEventArgs e)
    {
        m_offscreen_left = DrawingPictureBoxEx.ClipRectangle.Left;
        m_offscreen_top = DrawingPictureBoxEx.ClipRectangle.Top;
    }
    private void PictureBoxEx_MouseUp(object sender, MouseEventArgs e)
    {
        m_mouse_up_point = e.Location;
        if (m_mouse_up_point == m_mouse_down_point)
        {
            int x = e.X - m_offscreen_left;
            int y = e.Y - m_offscreen_top;
        }
    }
    private void PictureBoxEx_MouseWheel(object sender, EventArgs e)
    {
        m_zoomed_width = (int)((float)DrawingPictureBoxEx.Width * DrawingPictureBoxEx.ZoomFactor);
        m_zoomed_height = (int)((float)DrawingPictureBoxEx.Height * DrawingPictureBoxEx.ZoomFactor);
        m_offscreen_left = DrawingPictureBoxEx.ClipRectangle.Left;
        m_offscreen_top = DrawingPictureBoxEx.ClipRectangle.Top;
    }

    private DrawingShape m_drawing_shape = DrawingShape.SquareSpiral;
    private void DrawNewShapeButton_Click(object sender, EventArgs e)
    {
        if (ModifierKeys == Keys.Shift)
        {
            GotoPreviousShape();
        }
        else
        {
            GotoNextShape();
        }
        ToolTip.SetToolTip(DrawNewShapeButton, m_drawing_shape.ToString());

        // update graphs as we move between selections
        if (DrawingPictureBoxEx.Visible)
        {
            RedrawCurrentGraph();
        }
    }
    private void GotoNextShape()
    {
        switch (m_drawing_shape)
        {
            case DrawingShape.Spiral:
                {
                    m_drawing_shape = DrawingShape.SquareSpiral;
                    if (File.Exists("Images/squarespiral.png"))
                    {
                        DrawNewShapeButton.Image = new Bitmap("Images/squarespiral.png");
                    }
                }
                break;
            case DrawingShape.SquareSpiral:
                {
                    m_drawing_shape = DrawingShape.Square;
                    if (File.Exists("Images/square.png"))
                    {
                        DrawNewShapeButton.Image = new Bitmap("Images/square.png");
                    }
                }
                break;
            case DrawingShape.Square:
                {
                    m_drawing_shape = DrawingShape.HGoldenRect;
                    if (File.Exists("Images/hgoldenrect.png"))
                    {
                        DrawNewShapeButton.Image = new Bitmap("Images/hgoldenrect.png");
                    }
                }
                break;
            case DrawingShape.HGoldenRect:
                {
                    m_drawing_shape = DrawingShape.VGoldenRect;
                    if (File.Exists("Images/vgoldenrect.png"))
                    {
                        DrawNewShapeButton.Image = new Bitmap("Images/vgoldenrect.png");
                    }
                }
                break;
            case DrawingShape.VGoldenRect:
                {
                    m_drawing_shape = DrawingShape.Cube;
                    if (File.Exists("Images/cube.png"))
                    {
                        DrawNewShapeButton.Image = new Bitmap("Images/cube.png");
                    }
                }
                break;
            case DrawingShape.Cube:
                {
                    // short circuit to spiral directly for now 
                    m_drawing_shape = DrawingShape.Spiral;
                    if (File.Exists("Images/spiral.png"))
                    {
                        DrawNewShapeButton.Image = new Bitmap("Images/spiral.png");
                    }
                    //m_drawing_shape = DrawingShape.HGoldenCube;
                    //if (File.Exists("Images/hgoldencube.png"))
                    //{
                    //    DrawNewShapeButton.Image = new Bitmap("Images/hgoldencube.png");
                    //}
                }
                break;
            case DrawingShape.HGoldenCube:
                {
                    m_drawing_shape = DrawingShape.VGoldenCube;
                    if (File.Exists("Images/vgoldencube.png"))
                    {
                        DrawNewShapeButton.Image = new Bitmap("Images/vgoldencube.png");
                    }
                }
                break;
            case DrawingShape.VGoldenCube:
                {
                    m_drawing_shape = DrawingShape.Spiral;
                    if (File.Exists("Images/spiral.png"))
                    {
                        DrawNewShapeButton.Image = new Bitmap("Images/spiral.png");
                    }
                }
                break;
        }
    }
    private void GotoPreviousShape()
    {
        switch (m_drawing_shape)
        {
            case DrawingShape.Spiral:
                {
                    // short circuit to cube directly for now 
                    m_drawing_shape = DrawingShape.Cube;
                    if (File.Exists("Images/cube.png"))
                    {
                        DrawNewShapeButton.Image = new Bitmap("Images/cube.png");
                    }
                    //m_drawing_shape = DrawingShape.VGoldenCube;
                    //if (File.Exists("Images/vgoldencube.png"))
                    //{
                    //    DrawNewShapeButton.Image = new Bitmap("Images/vgoldencube.png");
                    //}
                }
                break;
            case DrawingShape.SquareSpiral:
                {
                    m_drawing_shape = DrawingShape.Spiral;
                    if (File.Exists("Images/spiral.png"))
                    {
                        DrawNewShapeButton.Image = new Bitmap("Images/spiral.png");
                    }
                }
                break;
            case DrawingShape.Square:
                {
                    m_drawing_shape = DrawingShape.SquareSpiral;
                    if (File.Exists("Images/squarespiral.png"))
                    {
                        DrawNewShapeButton.Image = new Bitmap("Images/squarespiral.png");
                    }
                }
                break;
            case DrawingShape.HGoldenRect:
                {
                    m_drawing_shape = DrawingShape.Square;
                    if (File.Exists("Images/square.png"))
                    {
                        DrawNewShapeButton.Image = new Bitmap("Images/square.png");
                    }
                }
                break;
            case DrawingShape.VGoldenRect:
                {
                    m_drawing_shape = DrawingShape.HGoldenRect;
                    if (File.Exists("Images/hgoldenrect.png"))
                    {
                        DrawNewShapeButton.Image = new Bitmap("Images/hgoldenrect.png");
                    }
                }
                break;
            case DrawingShape.Cube:
                {
                    m_drawing_shape = DrawingShape.VGoldenRect;
                    if (File.Exists("Images/vgoldenrect.png"))
                    {
                        DrawNewShapeButton.Image = new Bitmap("Images/vgoldenrect.png");
                    }
                }
                break;
            case DrawingShape.HGoldenCube:
                {
                    m_drawing_shape = DrawingShape.Cube;
                    if (File.Exists("Images/cube.png"))
                    {
                        DrawNewShapeButton.Image = new Bitmap("Images/cube.png");
                    }
                }
                break;
            case DrawingShape.VGoldenCube:
                {
                    m_drawing_shape = DrawingShape.HGoldenRect;
                    if (File.Exists("Images/hgoldencube.png"))
                    {
                        DrawNewShapeButton.Image = new Bitmap("Images/hgoldencube.png");
                    }
                }
                break;
        }
    }

    private void DrawLetterValuesButton_Click(object sender, EventArgs e)
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            if (m_client != null)
            {
                if (m_client.Selection != null)
                {
                    DrawingPictureBoxEx.Visible = true;
                    m_current_drawing_type = "LetterValues";
                    HeaderLabel.Text = m_current_drawing_type;
                    HeaderLabel.Refresh();

                    m_bitmap = new Bitmap(DrawingPictureBoxEx.Width, DrawingPictureBoxEx.Height, PixelFormat.Format24bppRgb);
                    if (m_bitmap != null)
                    {
                        List<Verse> verses = m_client.Selection.Verses;
                        List<long> values = m_client.CalculateAllLetterValues(verses);
                        if (m_drawing_shape == DrawingShape.Cube)
                        {
                            // cubic-root
                            int width = (int)Math.Pow(values.Count + 1, 1.0 / 3.0);
                            int height = width;
                            int layers = width;

                            for (int n = 0; n < layers; n++)
                            {
                                int value_index = n * (width * height);
                                int value_count = (width * height);
                                Drawing.DrawValues(m_bitmap, values.GetRange(value_index, value_count), Color.Pink, m_drawing_shape);
                                this.Refresh();
                            }
                        }
                        else
                        {
                            Drawing.DrawValues(m_bitmap, values, Color.Pink, m_drawing_shape);
                            this.Refresh();
                        }
                    }
                }
            }
        }
        catch
        {
            DrawingPictureBoxEx.Visible = false;
            ZoomInLabel.Enabled = (m_text_zoom_factor <= (m_max_zoom_factor - m_zoom_factor_increment + m_error_margin));
            ZoomOutLabel.Enabled = (m_text_zoom_factor >= (m_min_zoom_factor + m_zoom_factor_increment - m_error_margin));
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }
    private void DrawWordValuesButton_Click(object sender, EventArgs e)
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            if (m_client != null)
            {
                if (m_client.Selection != null)
                {
                    DrawingPictureBoxEx.Visible = true;
                    m_current_drawing_type = "WordValues";
                    HeaderLabel.Text = m_current_drawing_type;
                    HeaderLabel.Refresh();

                    m_bitmap = new Bitmap(DrawingPictureBoxEx.Width, DrawingPictureBoxEx.Height, PixelFormat.Format24bppRgb);
                    if (m_bitmap != null)
                    {
                        List<Verse> verses = m_client.Selection.Verses;
                        List<long> values = m_client.CalculateAllWordValues(verses);
                        if (m_drawing_shape == DrawingShape.Cube)
                        {
                            // cubic-root
                            int width = (int)Math.Pow(values.Count + 1, 1.0 / 3.0);
                            int height = width;
                            int layers = width;

                            for (int n = 0; n < layers; n++)
                            {
                                int value_index = n * (width * height);
                                int value_count = (width * height);
                                Drawing.DrawValues(m_bitmap, values.GetRange(value_index, value_count), Color.Pink, m_drawing_shape);
                                this.Refresh();
                            }
                        }
                        else
                        {
                            Drawing.DrawValues(m_bitmap, values, Color.Pink, m_drawing_shape);
                            this.Refresh();
                        }
                    }
                }
            }
        }
        catch
        {
            DrawingPictureBoxEx.Visible = false;
            ZoomInLabel.Enabled = (m_text_zoom_factor <= (m_max_zoom_factor - m_zoom_factor_increment + m_error_margin));
            ZoomOutLabel.Enabled = (m_text_zoom_factor >= (m_min_zoom_factor + m_zoom_factor_increment - m_error_margin));
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }
    private void DrawWordAllahButton_Click(object sender, EventArgs e)
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            if (m_client != null)
            {
                if (m_client.Selection != null)
                {
                    DrawingPictureBoxEx.Visible = true;
                    m_current_drawing_type = "WordAllah";

                    List<long> values = new List<long>();
                    m_bitmap = new Bitmap(DrawingPictureBoxEx.Width, DrawingPictureBoxEx.Height, PixelFormat.Format24bppRgb);
                    if (m_bitmap != null)
                    {
                        int count = 0;
                        int Allah_word_count = 0;

                        foreach (Verse verse in m_client.Selection.Verses)
                        {
                            foreach (Word word in verse.Words)
                            {
                                if ((word.Texts[m_client.TextMode] == "الله") || (word.Texts[m_client.TextMode] == "ٱلله"))
                                {
                                    values.Add(1);
                                    Allah_word_count++;
                                }
                                else
                                {
                                    values.Add(0);
                                }
                                count++;
                            }
                        }

                        StringBuilder str = new StringBuilder();
                        str.Append("Allah words = " + Allah_word_count);
                        HeaderLabel.Text = str.ToString();
                        HeaderLabel.Refresh();

                        str.Length = 0;
                        str.AppendLine("Quran words\t= " + count);
                        str.AppendLine("Allah words\t= " + Allah_word_count);
                    }

                    Drawing.DrawValues(m_bitmap, values, Color.Pink, Color.Pink, Color.Pink, m_drawing_shape);
                    this.Refresh();
                }
            }
        }
        catch
        {
            DrawingPictureBoxEx.Visible = false;
            ZoomInLabel.Enabled = (m_text_zoom_factor <= (m_max_zoom_factor - m_zoom_factor_increment + m_error_margin));
            ZoomOutLabel.Enabled = (m_text_zoom_factor >= (m_min_zoom_factor + m_zoom_factor_increment - m_error_margin));
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }
    private void DrawWordsWithAllahButton_Click(object sender, EventArgs e)
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            if (m_client != null)
            {
                if (m_client.Selection != null)
                {
                    DrawingPictureBoxEx.Visible = true;
                    m_current_drawing_type = "WordsWithAllah";

                    List<long> values = new List<long>();
                    m_bitmap = new Bitmap(DrawingPictureBoxEx.Width, DrawingPictureBoxEx.Height, PixelFormat.Format24bppRgb);
                    if (m_bitmap != null)
                    {
                        int count = 0;
                        int Allah_word_count = 0;
                        int with_Allah_word_count = 0;
                        int with_lillah_word_count = 0;

                        foreach (Verse verse in m_client.Selection.Verses)
                        {
                            foreach (Word word in verse.Words)
                            {
                                if ((word.Texts[m_client.TextMode] == "الله") || (word.Texts[m_client.TextMode] == "ٱلله")) // "Allah"
                                {
                                    values.Add(3);
                                    Allah_word_count++;
                                }
                                else if ( // Prefix"Allah", Prefix"Allah"Suffix, "Allah"Suffix
                                          ((word.Texts[m_client.TextMode].Contains("الله")) || (word.Texts[m_client.TextMode].Contains("ٱلله"))) &&
                                          (!word.Texts[m_client.TextMode].Contains("اللهو")) && (!word.Texts[m_client.TextMode].Contains("ٱللهو")) &&
                                          (!word.Texts[m_client.TextMode].Contains("اللهب")) && (!word.Texts[m_client.TextMode].Contains("ٱللهب"))
                                        )
                                {
                                    values.Add(2);
                                    with_Allah_word_count++;
                                }
                                else if ( // Prefix"Lillah", Prefix"Lillah"Suffix, "Lillah"Suffix
                                          (word.Texts[m_client.TextMode].Contains("لله")) &&
                                          (!word.Texts[m_client.TextMode].Contains("اللهو")) && (!word.Texts[m_client.TextMode].Contains("ٱللهو")) &&   // 1 words
                                          (!word.Texts[m_client.TextMode].Contains("اللهب")) && (!word.Texts[m_client.TextMode].Contains("ٱللهب")) &&   // 1 words
                                          (!word.Texts[m_client.TextMode].Contains("ضلله")) && (!word.Texts[m_client.TextMode].Contains("ضللة")) &&     // 8 words
                                          (!word.Texts[m_client.TextMode].Contains("ظلله")) && (!word.Texts[m_client.TextMode].Contains("ظللة")) &&     // 3 words
                                          (!word.Texts[m_client.TextMode].Contains("كلله")) && (!word.Texts[m_client.TextMode].Contains("كللة")) &&     // 2 words
                                          (!word.Texts[m_client.TextMode].Contains("خلله")) &&                        // 5 words
                                          (!word.Texts[m_client.TextMode].Contains("سلله")) && (!word.Texts[m_client.TextMode].Contains("سللة")) &&     // 2 words
                                          (!word.Texts[m_client.TextMode].Contains("للهدي")) && (!word.Texts[m_client.TextMode].Contains("للهدى"))      // 1 word
                                        )
                                {
                                    values.Add(1);
                                    with_lillah_word_count++;
                                }
                                else
                                {
                                    values.Add(0);
                                }
                                count++;
                            }
                        }

                        StringBuilder str = new StringBuilder();
                        str.Append("Allah words = " + Allah_word_count + " | ");
                        str.Append("+Allah+ = " + with_Allah_word_count + " | ");
                        str.Append("+Lillah+ = " + with_lillah_word_count + " | ");
                        str.Append("Total = " + (Allah_word_count + with_Allah_word_count + with_lillah_word_count));
                        HeaderLabel.Text = str.ToString();
                        HeaderLabel.Refresh();

                        str.Length = 0;
                        str.AppendLine("Quran words\t= " + count);
                        str.AppendLine("Allah words\t= " + Allah_word_count);
                        str.AppendLine("Words with Allah\t= " + with_Allah_word_count + "\t  " + "No اللهو اللهب");
                        str.AppendLine("Words with Lillah\t= " + with_lillah_word_count + "\t  " + "No اللهو اللهب خلله كللة ضللة ظلله سللة للهدى");
                        str.AppendLine("All Allah words\t= " + (Allah_word_count + with_Allah_word_count + with_lillah_word_count));
                        str.AppendLine();
                        str.AppendLine("Excluding:");
                        str.AppendLine("2:16  الضللة");
                        str.AppendLine("2:175 الضللة");
                        str.AppendLine("4:44  الضللة");
                        str.AppendLine("4:12  كللة");
                        str.AppendLine("4:176 الكللة");
                        str.AppendLine("6:39  يضلله");
                        str.AppendLine("7:30  الضللة");
                        str.AppendLine("7:61  ضللة");
                        str.AppendLine("13:15 وظللهم");
                        str.AppendLine("16:36 الضللة");
                        str.AppendLine("16:48 ظلله");
                        str.AppendLine("17:91 خللها");
                        str.AppendLine("18:33 خللهما");
                        str.AppendLine("19:75 الضللة");
                        str.AppendLine("23:12 سللة");
                        str.AppendLine("24:43 خلله");
                        str.AppendLine("27:61 خللها");
                        str.AppendLine("30:48 خلله");
                        str.AppendLine("32:8  سللة");
                        str.AppendLine("62:11 اللهو");
                        str.AppendLine("76:14 ظللها");
                        str.AppendLine("77:31 اللهب");
                        str.AppendLine("92:12 للهدى");
                    }

                    Drawing.DrawValues(m_bitmap, values, Color.Pink, Color.Pink, Color.Pink, m_drawing_shape);
                    this.Refresh();
                }
            }
        }
        catch
        {
            DrawingPictureBoxEx.Visible = false;
            ZoomInLabel.Enabled = (m_text_zoom_factor <= (m_max_zoom_factor - m_zoom_factor_increment + m_error_margin));
            ZoomOutLabel.Enabled = (m_text_zoom_factor >= (m_min_zoom_factor + m_zoom_factor_increment - m_error_margin));
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }
    private void DrawWordsWithAllahHelpButton_Click(object sender, EventArgs e)
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            System.Diagnostics.Process.Start("Notepad.exe", "Help/AllahWords.txt");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }
    private void DrawPrimesLabel_Click(object sender, EventArgs e)
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            DrawingPictureBoxEx.Visible = true;
            m_current_drawing_type = "Primes";
            HeaderLabel.Text = m_current_drawing_type;
            HeaderLabel.Refresh();

            m_bitmap = new Bitmap(DrawingPictureBoxEx.Width, DrawingPictureBoxEx.Height, PixelFormat.Format24bppRgb);
            if (m_bitmap != null)
            {
                int width = (m_bitmap.Width > m_bitmap.Height) ? m_bitmap.Width : m_bitmap.Height;
                int height = width;
                int max = width * height;
                List<long> values = new List<long>(max);
                for (int i = 0; i < max; i++)
                {
                    if (Numbers.IsPrime(i + 1))
                    {
                        values.Add(1);
                    }
                    else
                    {
                        values.Add(0);
                    }
                }
                Drawing.DrawValues(m_bitmap, values, Color.LightGreen, Color.LightGreen, Color.LightGreen, m_drawing_shape);
                this.Refresh();
            }
        }
        catch
        {
            DrawingPictureBoxEx.Visible = false;
            ZoomInLabel.Enabled = (m_text_zoom_factor <= (m_max_zoom_factor - m_zoom_factor_increment + m_error_margin));
            ZoomOutLabel.Enabled = (m_text_zoom_factor >= (m_min_zoom_factor + m_zoom_factor_increment - m_error_margin));
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }
    private void DrawAdditivePrimesLabel_Click(object sender, EventArgs e)
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            DrawingPictureBoxEx.Visible = true;
            m_current_drawing_type = "AdditivePrimes";
            HeaderLabel.Text = m_current_drawing_type;
            HeaderLabel.Refresh();

            m_bitmap = new Bitmap(DrawingPictureBoxEx.Width, DrawingPictureBoxEx.Height, PixelFormat.Format24bppRgb);
            if (m_bitmap != null)
            {
                int width = (m_bitmap.Width > m_bitmap.Height) ? m_bitmap.Width : m_bitmap.Height;
                int height = width;
                int max = width * height;
                List<long> values = new List<long>(max);
                for (int i = 0; i < max; i++)
                {
                    if (Numbers.IsAdditivePrime(i + 1))
                    {
                        values.Add(2);
                    }
                    else
                    {
                        values.Add(0);
                    }
                }
                Drawing.DrawValues(m_bitmap, values, Color.CornflowerBlue, Color.CornflowerBlue, Color.CornflowerBlue, m_drawing_shape);
                this.Refresh();
            }
        }
        catch
        {
            DrawingPictureBoxEx.Visible = false;
            ZoomInLabel.Enabled = (m_text_zoom_factor <= (m_max_zoom_factor - m_zoom_factor_increment + m_error_margin));
            ZoomOutLabel.Enabled = (m_text_zoom_factor >= (m_min_zoom_factor + m_zoom_factor_increment - m_error_margin));
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }
    private void DrawPurePrimesLabel_Click(object sender, EventArgs e)
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            DrawingPictureBoxEx.Visible = true;
            m_current_drawing_type = "PurePrimes";
            HeaderLabel.Text = m_current_drawing_type;
            HeaderLabel.Refresh();

            m_bitmap = new Bitmap(DrawingPictureBoxEx.Width, DrawingPictureBoxEx.Height, PixelFormat.Format24bppRgb);
            if (m_bitmap != null)
            {
                int width = (m_bitmap.Width > m_bitmap.Height) ? m_bitmap.Width : m_bitmap.Height;
                int height = width;
                int max = width * height;
                List<long> values = new List<long>(max);
                for (int i = 0; i < max; i++)
                {
                    if (Numbers.IsPurePrime(i + 1))
                    {
                        values.Add(3);
                    }
                    else
                    {
                        values.Add(0);
                    }
                }
                Drawing.DrawValues(m_bitmap, values, Color.Violet, Color.Violet, Color.Violet, m_drawing_shape);
                this.Refresh();
            }
        }
        catch
        {
            DrawingPictureBoxEx.Visible = false;
            ZoomInLabel.Enabled = (m_text_zoom_factor <= (m_max_zoom_factor - m_zoom_factor_increment + m_error_margin));
            ZoomOutLabel.Enabled = (m_text_zoom_factor >= (m_min_zoom_factor + m_zoom_factor_increment - m_error_margin));
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }
    private void DrawAllPrimesLabel_Click(object sender, EventArgs e)
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            DrawingPictureBoxEx.Visible = true;
            m_current_drawing_type = "AllPrimes";
            HeaderLabel.Text = m_current_drawing_type;
            HeaderLabel.Refresh();

            m_bitmap = new Bitmap(DrawingPictureBoxEx.Width, DrawingPictureBoxEx.Height, PixelFormat.Format24bppRgb);
            if (m_bitmap != null)
            {
                int width = (m_bitmap.Width > m_bitmap.Height) ? m_bitmap.Width : m_bitmap.Height;
                int height = width;
                int max = width * height;
                List<long> values = new List<long>(max);
                for (int i = 0; i < max; i++)
                {
                    if (Numbers.IsPurePrime(i + 1))
                    {
                        values.Add(3);
                    }
                    else if (Numbers.IsAdditivePrime(i + 1))
                    {
                        values.Add(2);
                    }
                    else if (Numbers.IsPrime(i + 1))
                    {
                        values.Add(1);
                    }
                    else
                    {
                        values.Add(0);
                    }
                }
                Drawing.DrawValues(m_bitmap, values, Color.LightGreen, Color.CornflowerBlue, Color.Violet, m_drawing_shape);
                this.Refresh();
            }
        }
        catch
        {
            DrawingPictureBoxEx.Visible = false;
            ZoomInLabel.Enabled = (m_text_zoom_factor <= (m_max_zoom_factor - m_zoom_factor_increment + m_error_margin));
            ZoomOutLabel.Enabled = (m_text_zoom_factor >= (m_min_zoom_factor + m_zoom_factor_increment - m_error_margin));
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }
    private void GenerateAllPrimeDrawingsLabel_Click(object sender, EventArgs e)
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            m_current_drawing_type = "GenerateAllPrimeDrawings";
            HeaderLabel.Text = m_current_drawing_type + " needs few minutes to complete";
            HeaderLabel.Refresh();

            Drawing.GenerateAndSaveAllPrimeDrawings(Color.LightGreen, Color.CornflowerBlue, Color.Violet);
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }
    private void RedrawCurrentGraph()
    {
        if (DrawingPictureBoxEx.Visible)
        {
            switch (m_current_drawing_type)
            {
                case "LetterValues":
                    DrawLetterValuesButton_Click(null, null);
                    break;
                case "WordValues":
                    DrawWordValuesButton_Click(null, null);
                    break;
                case "WordAllah":
                    DrawWordAllahButton_Click(null, null);
                    break;
                case "WordsWithAllah":
                    DrawWordsWithAllahButton_Click(null, null);
                    break;
                case "Primes":
                    DrawPrimesLabel_Click(null, null);
                    break;
                case "AdditivePrimes":
                    DrawAdditivePrimesLabel_Click(null, null);
                    break;
                case "PurePrimes":
                    DrawPurePrimesLabel_Click(null, null);
                    break;
                case "AllPrimes":
                    DrawAllPrimesLabel_Click(null, null);
                    break;
                default:
                    break;
            }
        }
    }

    private int m_help_index = 0;
    private void HelpLabel_Click(object sender, EventArgs e)
    {
        int maximum = Client.HelpMessages.Count - 1;
        if (ModifierKeys == Keys.Shift)
        {
            m_help_index--;
            if (m_help_index < 0) m_help_index = maximum;
        }
        else
        {
            m_help_index++;
            if (m_help_index > maximum) m_help_index = 0;
        }
        HelpLabel.Text = Client.HelpMessages[m_help_index];
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
                        catch (Exception ex)
                        {
                            while (ex != null)
                            {
                                //Console.WriteLine(ex.Message);
                                MessageBox.Show(ex.Message, Application.ProductName);
                                ex = ex.InnerException;
                            }
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
    private void PrimalogyLabel_Click(object sender, EventArgs e)
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            if (!File.Exists("Help/EncryptedQuran.pdf"))
            {
                DownloadFile("http://www.heliwave.com/EncryptedQuran.pdf", "Help/EncryptedQuran.pdf");
            }
            System.Diagnostics.Process.Start(Application.StartupPath + "/" + "Help/EncryptedQuran.pdf");
        }
        catch (Exception ex)
        {
            while (ex != null)
            {
                //Console.WriteLine(ex.Message);
                MessageBox.Show(ex.Message, Application.ProductName);
                ex = ex.InnerException;
            }
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }
    private void PrimeCalculatorButton_Click(object sender, EventArgs e)
    {
        try
        {
            System.Diagnostics.Process.Start("PrimeCalculator.exe");
        }
        catch (Exception ex)
        {
            while (ex != null)
            {
                //Console.WriteLine(ex.Message);
                MessageBox.Show(ex.Message, Application.ProductName);
                ex = ex.InnerException;
            }
        }
    }

    private string m_find_scope_label_text = "";
    private bool m_are_delegates_installed = false;
    private object ValuesValueColumnAspectGetter(object entry)
    {
        return (int)(((KeyValuePair<char, long>)entry).Value);
    }
    private void ValuesValueColumnAspectPutter(object entry, object value)
    {
        if (m_client != null)
        {
            char key = (char)((KeyValuePair<char, long>)entry).Key;
            m_client.NumerologySystem[key] = (long)((int)value);

            // refresh ObjectListView
            if (LetterValuesObjectListView != null)
            {
                LetterValuesObjectListView.ClearObjects();
                LetterValuesObjectListView.SetObjects(m_client.NumerologySystem);
            }

            CalculateCurrentValue();
        }
    }
    private void LetterValuesButton_Click(object sender, EventArgs e)
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            if (!m_are_delegates_installed)
            {
                ValuesValueColumn.AspectGetter = ValuesValueColumnAspectGetter;
                ValuesValueColumn.AspectPutter = ValuesValueColumnAspectPutter;

                //// auto drag and drop
                //if (LetterValuesObjectListView != null)
                //{
                //    LetterValuesObjectListView.DragSource = new BrightIdeasSoftware.SimpleDragSource();
                //    LetterValuesObjectListView.DropSink = new BrightIdeasSoftware.RearrangingDropSink(false);
                //}

                m_are_delegates_installed = true;
            }


            if (m_client != null)
            {
                // toggle visibility
                LetterValuesPanel.Visible = !LetterValuesPanel.Visible;

                if (LetterValuesPanel.Visible)
                {
                    m_find_scope_label_text = FindScopeLabel.Text;
                    FindScopeLabel.Text = "Edit Valuation System";
                    FindScopeLabel.Refresh();

                    if (LetterValuesObjectListView != null)
                    {
                        LetterValuesObjectListView.ClearObjects();
                        LetterValuesObjectListView.SetObjects(m_client.NumerologySystem);
                    }
                }
                else
                {
                    FindScopeLabel.Text = m_find_scope_label_text;
                    FindScopeLabel.Refresh();
                }
            }
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }
    private void LetterValuesRestoreButton_Click(object sender, EventArgs e)
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            if (m_client != null)
            {
                UpdateNumerologySystems();

                // refresh ObjectListView
                if (LetterValuesObjectListView != null)
                {
                    LetterValuesObjectListView.ClearObjects();
                    LetterValuesObjectListView.SetObjects(m_client.NumerologySystem);
                }
                CalculateCurrentValue();

                LetterValuesRestoreButton.Enabled = false;
                LetterValuesCloseButton.Enabled = true;
                LetterValuesSaveButton.Enabled = false;
            }
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }
    private void LetterValuesSaveButton_Click(object sender, EventArgs e)
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            if (m_client != null)
            {
                m_client.SaveNumerologySystem();

                LetterValuesRestoreButton.Enabled = false;
                LetterValuesCloseButton.Enabled = true;
                LetterValuesSaveButton.Enabled = false;
            }
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }
    private void LetterValuesCloseButton_Click(object sender, EventArgs e)
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            if (m_client != null)
            {
                LetterValuesPanel.Visible = false;
                FindScopeLabel.Text = m_find_scope_label_text;
                FindScopeLabel.Refresh();

                LetterValuesRestoreButton.Enabled = false;
                LetterValuesCloseButton.Enabled = true;
                LetterValuesSaveButton.Enabled = false;
            }
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }
    private char source = '\0';
    private char target = '\0';
    private int source_index = -1;
    private int target_index = -1;
    private void LetterValuesObjectListView_ModelCanDrop(object sender, BrightIdeasSoftware.ModelDropEventArgs e)
    {
        if (e.TargetModel != null)
        {
            e.DropSink.CanDropBetween = true;
            e.DropSink.CanDropOnItem = false;

            source = ((KeyValuePair<char, long>)e.SourceModels[0]).Key;
            target = ((KeyValuePair<char, long>)e.TargetModel).Key;

            if (m_client != null)
            {
                source_index = -1;
                target_index = -1;

                int index = 0;
                foreach (char key in m_client.NumerologySystem.Keys)
                {
                    if (key == source)
                    {
                        source_index = index;
                        break;
                    }
                    index++;
                }
                index = 0;
                foreach (char key in m_client.NumerologySystem.Keys)
                {
                    if (key == target)
                    {
                        target_index = index;
                        break;
                    }
                    index++;
                }

                if (source_index != target_index)
                {
                    e.Effect = DragDropEffects.Move;
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                }
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }
        else
        {
            e.Effect = DragDropEffects.None;
        }
    }
    private void LetterValuesObjectListView_ModelDropped(object sender, BrightIdeasSoftware.ModelDropEventArgs e)
    {
        if (e.TargetModel != null)
        {
            if (m_client != null)
            {
                List<char> NewNumerologySystemKeys = new List<char>();
                List<long> NewNumerologySystemValues = new List<long>();

                // setup Keys without any change in order
                foreach (char key in m_client.NumerologySystem.Keys)
                {
                    NewNumerologySystemKeys.Add(key);
                }
                // setup Values without any change in order
                foreach (char key in m_client.NumerologySystem.Keys)
                {
                    NewNumerologySystemValues.Add(m_client.NumerologySystem[key]);
                }

                // move source to before target
                if ((target_index > source_index) && (target_index < m_client.NumerologySystem.Keys.Count - 1))
                {
                    target_index--;
                }
                // do move
                foreach (char key in m_client.NumerologySystem.Keys)
                {
                    if (key == source)
                    {
                        NewNumerologySystemKeys.Remove(source);
                        NewNumerologySystemKeys.Insert(target_index, source);
                    }
                }

                m_client.NumerologySystem.Clear();
                for (int i = 0; i < NewNumerologySystemKeys.Count; i++)
                {
                    m_client.NumerologySystem.Add(NewNumerologySystemKeys[i], NewNumerologySystemValues[i]);
                }
            }

            // force list to refresh doesn't work
            //e.RefreshObjects();
            if (LetterValuesObjectListView != null)
            {
                LetterValuesObjectListView.ClearObjects();
                LetterValuesObjectListView.SetObjects(m_client.NumerologySystem);
            }

            CalculateCurrentValue();

            LetterValuesRestoreButton.Enabled = true;
            LetterValuesCloseButton.Enabled = true;
            LetterValuesSaveButton.Enabled = true;
        }
    }
    private void LetterValuesObjectListView_CellEditFinishing(object sender, BrightIdeasSoftware.CellEditEventArgs e)
    {
        LetterValuesRestoreButton.Enabled = true;
        LetterValuesCloseButton.Enabled = true;
        LetterValuesSaveButton.Enabled = true;
    }

}
