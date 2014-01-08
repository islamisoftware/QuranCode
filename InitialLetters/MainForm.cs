using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace InitialLetters
{
    public partial class MainForm : Form
    {
        List<bag_and_anagrams> m_dictionary;
        DateTime m_start_time;
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            Bag.Test();
        }
        private void MainForm_Shown(object sender, EventArgs e)
        {
            string filename = "Data" + "/" + "quran-uthmani-unique-words.txt";
            if (File.Exists(filename))
            {
                using (StreamReader reader = File.OpenText(filename))
                {
                    ToolStripStatusLabel.Text = "Compiling dictionary ...";
                    ProgressBar.Value = 0;
                    ProgressBar.Maximum = (int)reader.BaseStream.Length;
                    ListView_Resize(sender, e);
                    try
                    {
                        String line;
                        // Read and display lines from the file until the end of the file is reached.
                        int linesRead = 0;
                        Hashtable stringlists_by_bag = new Hashtable();
                        while ((line = reader.ReadLine()) != null)
                        {
                            line = line.ToLower();

                            Bag aBag = new Bag(line);
                            if (!stringlists_by_bag.ContainsKey(aBag))
                            {
                                strings l = new strings();
                                l.Add(line);
                                stringlists_by_bag.Add(aBag, l);
                            }
                            else
                            {
                                strings l = (strings)stringlists_by_bag[aBag];
                                if (!l.Contains(line))
                                    l.Add(line);
                            }
                            linesRead++;
                            ProgressBar.Increment((line.Length + 2) * 2);   // the +1 is for the line ending character, I'd guess.
                            // the *2 is to deal with unicode characters
                            Application.DoEvents();
                        }

                        // Now convert the hash table, which isn't useful for
                        // actually generating anagrams, into a list, which is.

                        m_dictionary = new List<bag_and_anagrams>();
                        foreach (DictionaryEntry de in stringlists_by_bag)
                        {
                            m_dictionary.Add(new bag_and_anagrams((Bag)de.Key, (strings)de.Value));
                        }

                        // Now just for amusement, sort the list so that the biggest bags 
                        // come first.  This might make more interesting anagrams appear first.
                        bag_and_anagrams[] sort_me = new bag_and_anagrams[m_dictionary.Count];
                        m_dictionary.CopyTo(sort_me);
                        Array.Sort(sort_me);
                        m_dictionary.Clear();
                        m_dictionary.InsertRange(0, sort_me);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Dictionary: " + ex.Message);
                    }

                    ToolStripStatusLabel.Text = "Dictionary is compiled.";
                    ListView.Enabled = true;
                    LettersTextBox.Enabled = true;
                    LettersTextBox.Focus();
                    UniqueLettersToolStripMenuItem_Click(sender, e);
                }
            }
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            ElapsedTimeLabel.Text = DateTime.Now.Subtract(m_start_time).ToString();
        }

        private void ListView_Resize(object sender, EventArgs e)
        {
            // trial and error shows that we must make the column
            // header four pixels narrower than the containing
            // listview in order to avoid a scrollbar.
            ListView.Columns[0].Width = ListView.Width - 4;

            // if the listview is big enough to show all the items, then make sure
            // the first item is at the top.  This works around behavior (which I assume is 
            // a bug in C# or .NET or something) whereby 
            // some blank lines appear before the first item

            if (ListView.Items.Count > 0
                &&
                ListView.TopItem != null
                &&
                ListView.TopItem.Index == 0)
                ListView.EnsureVisible(0);
        }
        private void ListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            Clipboard.Clear();

            string selected_text = "";
            ListView me = (ListView)sender;
            foreach (ListViewItem it in me.SelectedItems)
            {
                if (selected_text.Length > 0)
                    selected_text += Environment.NewLine;
                selected_text += it.Text;
            }
            // Under some circumstances -- probably a bug in my code somewhere --
            // we can get blank lines in the listview.  And if you click on one, since it
            // has no text, selected_text will be blank; _and_, apparantly, calling
            // Clipboard.set_text with an empty string provokes an access violation ...
            // so avoid that AV.
            if (selected_text.Length > 0)
                Clipboard.SetText(selected_text);
        }
        private void ListView_SortColumnClick(object sender, ColumnClickEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            ListView.Sorting = SortOrder.Ascending;
            ListView.Sorting = SortOrder.None;
            ListView.Columns[0].Text = "";
            this.Cursor = Cursors.Default;
        }
        private void LettersTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // stop annoying beep
            if ((e.KeyChar == (char)Keys.Enter) || (e.KeyChar == (char)Keys.Escape))
            {
                e.Handled = true;
            }
            // allow Ctrl+A to SelectAll
            if ((ModifierKeys == Keys.Control) && (e.KeyChar == (char)Keys.A))
            {
                (sender as TextBoxBase).SelectAll();
            }

            if ((e.KeyChar == (char)Keys.Enter) || (e.KeyChar == (char)Keys.Escape))
            {
                GenerateAnagrams(sender, e);
            }
        }

        private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog.FileName = LettersTextBox.Text + ".txt";
            SaveFileDialog.InitialDirectory = Application.ExecutablePath;
            if (SaveFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter sw = new StreamWriter(SaveFileDialog.OpenFile()))
                {
                    sw.WriteLine("{0} sentences of '{1}'",
                        ListView.Items.Count, LettersTextBox.Text);
                    sw.WriteLine("-----------------------");
                    foreach (ListViewItem it in ListView.Items)
                    {
                        sw.WriteLine(it.Text);
                    }
                }
            }
        }
        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void UniqueLettersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LettersTextBox.Text = "ق ص ر ع س ن م ل ك ي ط ح ه ا";
            UniqueLettersToolStripMenuItem.Checked = true;
            UniqueWordsToolStripMenuItem.Checked = false;
            AllWordsToolStripMenuItem.Checked = false;
        }
        private void UniqueWordsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LettersTextBox.Text = "الم المص الر المر كهيعص طه طسم طس يس ص حم عسق ق ن";
            UniqueLettersToolStripMenuItem.Checked = false;
            UniqueWordsToolStripMenuItem.Checked = true;
            AllWordsToolStripMenuItem.Checked = false;
        }
        private void AllWordsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LettersTextBox.Text = "الم الم المص الر الر الر المر الر الر كهيعص طه طسم طس طسم الم الم الم الم يس ص حم حم عسق حم حم حم حم حم ق ن";
            UniqueLettersToolStripMenuItem.Checked = false;
            UniqueWordsToolStripMenuItem.Checked = false;
            AllWordsToolStripMenuItem.Checked = true;
        }
        private void RunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GenerateAnagrams(sender, e);
        }
        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(String.Format(
                "Quran Initial Letters - v{0}", Application.ProductVersion + "\r\n" + "\r\n"
                + "Based on https://github.com/offby1/anagrams" + "\r\n"
                + "©2007 OffBy1 - ©2012 Ali Adams" + "\r\n" + "\r\n"
                + "www.qurancode.com" + "\r\n"
                ),
                Application.ProductName);
        }

        private void GenerateAnagrams(object sender, EventArgs e)
        {
            LettersTextBox.Enabled = false;
            Bag LettersTextBox_bag = new Bag(LettersTextBox.Text);
            ListView.Items.Clear();
            FileToolStripMenuItem.Enabled = false;
            TypeToolStripMenuItem.Enabled = false;
            m_start_time = DateTime.Now;
            ElapsedTimeLabel.Text = "00:00:00";
            Timer.Enabled = true;
            ProgressBar.Value = 0;
            Anagrams.anagrams(LettersTextBox_bag, m_dictionary, 0,

                // bottom of main loop
                delegate()
                {
                    ProgressBar.PerformStep();
                    Application.DoEvents();
                },

                // done pruning
                delegate(uint recursion_level, List<bag_and_anagrams> pruned_dict)
                {
                    if (recursion_level == 0)
                    {
                        ProgressBar.Maximum = pruned_dict.Count;
                        Application.DoEvents();
                    }
                },

                // found a top-level anagram
                delegate(strings words)
                {
                    string display_me = "";
                    foreach (string s in words)
                    {
                        if (display_me.Length > 0)
                            display_me += " ";
                        display_me += s;
                    }

                    ListView.Items.Add(display_me);
                    ListView.EnsureVisible(ListView.Items.Count - 1);
                    ToolStripStatusLabel.Text = ListView.Items.Count.ToString() + " sentences so far";
                    if (ListView.Items.Count % 1000 == 0)
                    {
                        Application.DoEvents();
                    }

                });
            Timer.Enabled = false;
            ToolStripStatusLabel.Text = String.Format("Done. {0} sentences",
                ListView.Items.Count);
            if (ListView.Items.Count > 0)
                ListView.EnsureVisible(0);
            LettersTextBox.Enabled = true;
            LettersTextBox.Focus();
            ListView.Columns[0].Text = "Click to sort";
            FileToolStripMenuItem.Enabled = true;
            TypeToolStripMenuItem.Enabled = true;

            string filename = "Data" + "/" + "Sentences.txt";
            SaveFileDialog.InitialDirectory = Application.ExecutablePath;
            try
            {
                using (StreamWriter writer = new StreamWriter(filename))
                {
                    writer.WriteLine("-----------------------------------------------------------------");
                    writer.WriteLine("{0} sentences of '{1}'", ListView.Items.Count, LettersTextBox.Text);
                    writer.WriteLine("-----------------------------------------------------------------");
                    foreach (ListViewItem item in ListView.Items)
                    {
                        writer.WriteLine(item.Text);
                    }
                    writer.WriteLine("-----------------------------------------------------------------");
                }

                System.Diagnostics.Process.Start("Notepad.exe", filename);
            }
            catch
            {
                // silence IO error in case running from read-only media (CD/DVD)
            }
        }
    }
}
