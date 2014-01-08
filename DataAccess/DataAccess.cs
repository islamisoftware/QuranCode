using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Model;

public static class DataAccess
{
    private static string s_data_directory = "Data";
    private static string s_recitations_directory = "Audio";
    private static string s_translations_directory = "Translations";
    private static string s_translations_offline_directory = "Translations/Offline";
    static DataAccess()
    {
        if (!Directory.Exists(s_data_directory))
        {
            Directory.CreateDirectory(s_data_directory);
        }
        if (!Directory.Exists(s_recitations_directory))
        {
            Directory.CreateDirectory(s_recitations_directory);
        }
        if (!Directory.Exists(s_translations_directory))
        {
            Directory.CreateDirectory(s_translations_directory);

            if (!Directory.Exists(s_translations_offline_directory))
            {
                Directory.CreateDirectory(s_translations_offline_directory);
            }
        }
    }

    private static bool s_initialized = false;
    public static bool Initialize()
    {
        try
        {
            // prevent re-initialization
            if (s_initialized) return true;

            s_initialized = false;

            List<Verse> verses = LoadBookVerses(s_data_directory + "/" + "quran-uthmani.txt");
            s_book = Book.CreateInstance("Original", verses);
            if (s_book != null)
            {
                LoadQuranRootWords();
                LoadQuranWordMeanings();
                LoadQuranWordRoots();
                if (Globals.EDITION == Edition.Grammar)
                {
                    LoadCorpusWordParts();
                }
                LoadQuranTranslations();
                LoadQuranRecitations();
                LoadQuranTranslationBooks();
            }
            s_initialized = true;
        }
        catch
        {
            s_initialized = false;
        }

        return s_initialized;
    }

    // quran text from http://tanzil.net
    private static Book s_book = null;
    public static Book Book
    {
        get
        {
            if (s_book == null)
            {
                Initialize();
            }
            return s_book;
        }
        set { s_book = value; }
    }

    // quran translations info from http://tanzil.net
    private static Dictionary<string, Translation> s_quran_translations = null;
    public static Dictionary<string, Translation> QuranTranslations
    {
        get
        {
            if (s_quran_translations == null)
            {
                LoadQuranTranslations();
            }
            return s_quran_translations;
        }
    }
    private static void LoadQuranTranslations()
    {
        s_quran_translations = new Dictionary<string, Translation>();
        string filename = s_translations_offline_directory + "/" + "metadata.txt";
        if (File.Exists(filename))
        {
            using (StreamReader reader = File.OpenText(filename))
            {
                string line = reader.ReadLine(); // skip header row
                while ((line = reader.ReadLine()) != null)
                {
                    string[] parts = line.Split('\t');
                    if (parts.Length >= 4)
                    {
                        Translation translation = new Translation();
                        translation.Url = "?transID=" + parts[0] + "&type=" + Translation.FileType;
                        translation.Flag = parts[1];
                        translation.Language = parts[2];
                        translation.Translator = parts[3];
                        translation.Name = parts[2] + " - " + parts[3];
                        s_quran_translations.Add(parts[0], translation);
                    }
                }
            }
        }
    }

    // quran translation books from http://tanzil.net
    private static void LoadQuranTranslationBooks()
    {
        try
        {
            string[] filenames = Directory.GetFiles(s_translations_directory + "/");
            foreach (string filename in filenames)
            {
                List<string> translated_lines = DataAccess.LoadLines(filename);
                if (translated_lines != null)
                {
                    string title = filename.Substring((s_translations_directory.Length + 1), filename.Length - (s_translations_directory.Length + 1) - 4);
                    if (QuranTranslations.ContainsKey(title))
                    {
                        for (int i = 0; i < Verse.MAX_NUMBER; i++)
                        {
                            s_book.Verses[i].Translations[title] = translated_lines[i];
                        }
                    }
                }
            }
        }
        catch
        {
            // ignore error
        }
    }
    public static void LoadQuranTranslationBook(string title)
    {
        try
        {
            string[] filenames = Directory.GetFiles(s_translations_directory + "/");
            bool title_is_installed = false;
            foreach (string filename in filenames)
            {
                if (filename.Contains(title))
                {
                    title_is_installed = true;
                    break;
                }
            }
            if (!title_is_installed)
            {
                File.Copy(s_translations_offline_directory + "/" + title + ".txt", s_translations_directory + "/" + title + ".txt");
            }

            filenames = Directory.GetFiles(s_translations_directory + "/");
            foreach (string filename in filenames)
            {
                if (filename.Contains(title))
                {
                    List<string> translated_lines = DataAccess.LoadLines(filename);
                    if (translated_lines != null)
                    {
                        if (QuranTranslations.ContainsKey(title))
                        {
                            for (int i = 0; i < Verse.MAX_NUMBER; i++)
                            {
                                s_book.Verses[i].Translations[title] = translated_lines[i];
                            }
                        }
                    }
                    break;
                }
            }
        }
        catch
        {
            // ignore error
        }
    }
    public static void UnloadQuranTranslationBook(string title)
    {
        try
        {
            string[] filenames = Directory.GetFiles(s_translations_directory + "/");
            foreach (string filename in filenames)
            {
                if (filename.Contains(title))
                {
                    if (QuranTranslations.ContainsKey(title))
                    {
                        for (int i = 0; i < Verse.MAX_NUMBER; i++)
                        {
                            s_book.Verses[i].Translations[title] = null;
                        }
                        QuranTranslations.Remove(title);
                    }
                    break;
                }
            }
        }
        catch
        {
            // ignore error
        }
    }

    // quran word meanings from http://qurandev.appspot.com - modified by Ali Adams
    private static void LoadQuranWordMeanings()
    {
        string filename = s_data_directory + "/" + "quran-uthmani-word-meanings.txt";
        if (File.Exists(filename))
        {
            using (StreamReader reader = File.OpenText(filename))
            {
                while (!reader.EndOfStream)
                {
                    try
                    {
                        foreach (Verse verse in s_book.Verses)
                        {
                            string line = reader.ReadLine();
                            string[] parts = line.Split('\t');
                            if (parts.Length != verse.Words.Count) throw new Exception("LoadQuranWordMeanings file format error.");

                            for (int i = 0; i < verse.Words.Count; i++)
                            {
                                verse.Words[i].Meaning = parts[i];
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("LoadQuranWordMeanings: " + ex.Message);
                    }
                }
            }
        }
    }

    // quran roots and their words from http://www.noorsoft.org version 0.9.1
    private static Dictionary<string, List<Word>> s_quran_root_words = null;
    public static Dictionary<string, List<Word>> QuranRootWords
    {
        get
        {
            if (s_quran_root_words == null)
            {
                LoadQuranRootWords();
            }
            return s_quran_root_words;
        }
    }
    private static void LoadQuranRootWords()
    {
        string filename = s_data_directory + "/" + "quran-uthmani-root-words.txt";
        if (File.Exists(filename))
        {
            s_quran_root_words = new Dictionary<string, List<Word>>();
            using (StreamReader reader = File.OpenText(filename))
            {
                while (!reader.EndOfStream)
                {
                    try
                    {
                        string line = reader.ReadLine();
                        string[] parts = line.Split('\t');
                        if (parts.Length == 3)
                        {
                            string root = parts[0];
                            int count = int.Parse(parts[1]);
                            string[] addresses = parts[2].Split(';');

                            List<Word> root_words = new List<Word>();
                            foreach (string address in addresses)
                            {
                                string[] segments = address.Split(':');
                                if (segments.Length == 2)
                                {
                                    string[] segment_parts = segments[1].Split(',');
                                    foreach (string segment_part in segment_parts)
                                    {
                                        int verse_index = int.Parse(segments[0]);
                                        Verse verse = s_book.Verses[verse_index];
                                        if (verse != null)
                                        {
                                            if (verse.Words != null)
                                            {
                                                if (verse.Words.Count > 0)
                                                {
                                                    int word_number = int.Parse(segment_part);
                                                    Word root_word = null;
                                                    // THIS software now loads an already-uthmani word root list
                                                    // generated by the SaveRootWordsDictionary() below

                                                    //// update word_number from emlaaei to uthmani
                                                    //try
                                                    //{
                                                    //    // for all words after merged words decrement word_number
                                                    //    for (int i = 0; i < word_number - 1; i++)
                                                    //    {
                                                    //        // if previous words starts with يَٰ or وَيَٰ   
                                                    //        if (
                                                    //            (verse.Words[i].Text.StartsWith("يَٰ"))
                                                    //            ||
                                                    //            (verse.Words[i].Text.StartsWith("وَيَٰ"))
                                                    //            )
                                                    //        {
                                                    //            word_number--;
                                                    //        }

                                                    //        // also, if previous words equals to these exceptions
                                                    //        if (verse.Words[i].Text == "يَبْنَؤُمَّ") // 3 words in imlaaei
                                                    //        {
                                                    //            word_number--;
                                                    //            word_number--;
                                                    //        }
                                                    //        else if (verse.Words[i].Text == "فَإِلَّمْ") // 2 words in imlaaei
                                                    //        {
                                                    //            word_number--;
                                                    //        }
                                                    //        else if (verse.Words[i].Text == "هَٰٓأَنتُمْ") // 2 words in imlaaei
                                                    //        {
                                                    //            word_number--;
                                                    //        }
                                                    //        else if (verse.Words[i].Text == "وَأَلَّوِ") // 2 words in imlaaei
                                                    //        {
                                                    //            word_number--;
                                                    //        }
                                                    //        // "بَعْدَ" and "مَا" to become "بَعْدَمَا"    // 2 words in imlaaei 
                                                    //        else if (verse.Words[i].Text == "بَعْدَ")
                                                    //        {
                                                    //            if (verse.Words.Count > (i + 1))
                                                    //            {
                                                    //                if (verse.Words[i + 1].Text == "مَا")
                                                    //                {
                                                    //                    word_number--;
                                                    //                }
                                                    //            }
                                                    //        }
                                                    //        else if ((root == "ما") && (verse.Words[word_number - 1].Text == "بَعْدَ"))
                                                    //        {
                                                    //            word_number--;
                                                    //        }
                                                    //    }
                                                    //}
                                                    //catch
                                                    //{
                                                    //    throw new Exception("Root word location cannot be corrected automatically.");
                                                    //}
                                                    if ((word_number > 0) && (word_number <= verse.Words.Count))
                                                    {
                                                        root_word = verse.Words[word_number - 1];
                                                        if (root_word != null)
                                                        {
                                                            root_words.Add(root_word);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (!s_quran_root_words.ContainsKey(root))
                            {
                                s_quran_root_words.Add(root, root_words);
                            }
                        }
                        else
                        {
                            // skip reading copyright notice;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("LoadQuranRootWords: " + ex.Message);
                    }
                }
            }
        }
    }

    // quran word and its roots reverse-extracted from http://www.noorsoft.org version 0.9.1
    private static void LoadQuranWordRoots()
    {
        // Id	Chapter	Verse	Word	Text	Roots
        string filename = s_data_directory + "/" + "quran-uthmani-word-roots.txt";
        if (File.Exists(filename))
        {
            using (StreamReader reader = File.OpenText(filename))
            {
                while (!reader.EndOfStream)
                {
                    try
                    {
                        foreach (Verse verse in s_book.Verses)
                        {
                            for (int i = 0; i < verse.Words.Count; i++)
                            {
                                string line = reader.ReadLine();
                                string[] parts = line.Split('\t');

                                if (parts.Length == 6)
                                {
                                    string text = parts[4];
                                    string[] subparts = parts[5].Split('|');
                                    verse.Words[i].Roots = new List<string>(subparts);
                                }
                                else
                                {
                                    throw new Exception("LoadQuranWordRoots: " + "Invalid file format.");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("LoadQuranWordRoots: " + ex.Message);
                    }
                }
            }
        }
    }

    // corpus word parts from http://corpus.quran.com version 0.4 - modified by Ali Adams
    // Data/quran-uthmani-word-parts.txt
    //(1:1:1:1)	bi	        P	    PREFIX|bi+
    //(1:1:1:2)	somi	    N	    STEM|POS:N|LEM:{som|ROOT:smw|M|GEN
    //(1:1:2:1)	{ll~ahi	    PN	    STEM|POS:PN|LEM:{ll~ah|ROOT:Alh|GEN
    //(1:1:3:1)	{l	        DET	    PREFIX|Al+
    //(1:1:3:2)	r~aHoma`ni	ADJ	    STEM|POS:ADJ|LEM:r~aHoma`n|ROOT:rHm|MS|GEN
    //(1:1:4:1)	{l	        DET	    PREFIX|Al+
    //(1:1:4:2)	r~aHiymi	ADJ	    STEM|POS:ADJ|LEM:r~aHiym|ROOT:rHm|MS|GEN
    private static void LoadCorpusWordParts()
    {
        if (s_book != null)
        {
            string filename = s_data_directory + "/" + "quran-uthmani-word-parts.txt";
            if (File.Exists(filename))
            {
                using (StreamReader reader = File.OpenText(filename))
                {
                    while (!reader.EndOfStream)
                    {
                        try
                        {
                            string line = reader.ReadLine();
                            if ((line.Length == 0) || line.StartsWith("#") || line.StartsWith("LOCATION") || line.StartsWith("ADDRESS"))
                            {
                                continue; // skip header info
                            }
                            else
                            {
                                string[] parts = line.Split('\t');
                                if (parts.Length >= 4)
                                {
                                    string address = parts[0];
                                    if (address.StartsWith("(") && address.EndsWith(")"))
                                    {
                                        address = parts[0].Substring(1, parts[0].Length - 2);
                                    }
                                    string[] address_parts = address.Split(':');
                                    if (address_parts.Length == 4)
                                    {
                                        int chapter_number = int.Parse(address_parts[0]);
                                        int verse_number = int.Parse(address_parts[1]);
                                        int word_number = int.Parse(address_parts[2]);
                                        int word_part_number = int.Parse(address_parts[3]);

                                        string buckwalter = parts[1];
                                        string tag = parts[2];
                                        
                                        Chapter chapter = s_book.Chapters[chapter_number - 1];
                                        if (chapter != null)
                                        {
                                            Verse verse = chapter.Verses[verse_number - 1];
                                            if (verse != null)
                                            {
                                                // add bismAllah manually to each chapter except 1 and 9
                                                if (
                                                    ((chapter_number != 1) && (chapter_number != 9))
                                                    &&
                                                    ((verse_number == 1) && (word_number == 1) && (word_part_number == 1))
                                                   )
                                                {
                                                    Verse bismAllah_verse = s_book.Verses[0];

                                                    // if there is no bismAllah, add one
                                                    if (parts[1] != bismAllah_verse.Words[0].Parts[0].Buckwalter)
                                                    {
                                                        // insert 4 new words
                                                        verse.Words.InsertRange(0, new List<Word>(4));

                                                        //(1:1:1:1)	bi	PP	PREFIX|bi+
                                                        WordPart word_part = new WordPart(verse.Words[0],
                                                              bismAllah_verse.Words[0].Parts[0].NumberInWord,
                                                              bismAllah_verse.Words[0].Parts[0].Buckwalter,
                                                              bismAllah_verse.Words[0].Parts[0].Tag,
                                                              new WordPartFeatures(bismAllah_verse.Words[0].Parts[0].Features)
                                                        );
                                                        if ((chapter_number == 95) || (chapter_number == 97))
                                                        {
                                                            // add shadda  { '~', 'ّ' } on B or bism
                                                            word_part.Buckwalter = word_part.Buckwalter.Insert(1, "~");
                                                        }

                                                        //(1:1:1:2)	somi	N	STEM|POS:N|LEM:{som|ROOT:smw|M|GEN
                                                        new WordPart(verse.Words[0],
                                                          bismAllah_verse.Words[0].Parts[1].NumberInWord,
                                                          bismAllah_verse.Words[0].Parts[1].Buckwalter,
                                                          bismAllah_verse.Words[0].Parts[1].Tag,
                                                          new WordPartFeatures(bismAllah_verse.Words[0].Parts[1].Features)
                                                        );

                                                        //(1:1:2:1)	{ll~ahi	PN	STEM|POS:PN|LEM:{ll~ah|ROOT:Alh|GEN
                                                        new WordPart(verse.Words[1],
                                                          bismAllah_verse.Words[1].Parts[0].NumberInWord,
                                                          bismAllah_verse.Words[1].Parts[0].Buckwalter,
                                                          bismAllah_verse.Words[1].Parts[0].Tag,
                                                          new WordPartFeatures(bismAllah_verse.Words[1].Parts[0].Features)
                                                        );

                                                        //(1:1:3:1)	{l	DET	PREFIX|Al+
                                                        new WordPart(verse.Words[2],
                                                          bismAllah_verse.Words[2].Parts[0].NumberInWord,
                                                          bismAllah_verse.Words[2].Parts[0].Buckwalter,
                                                          bismAllah_verse.Words[2].Parts[0].Tag,
                                                          new WordPartFeatures(bismAllah_verse.Words[2].Parts[0].Features)
                                                        );

                                                        //(1:1:3:2)	r~aHoma`ni	ADJ	STEM|POS:ADJ|LEM:r~aHoma`n|ROOT:rHm|MS|GEN
                                                        new WordPart(verse.Words[2],
                                                          bismAllah_verse.Words[2].Parts[1].NumberInWord,
                                                          bismAllah_verse.Words[2].Parts[1].Buckwalter,
                                                          bismAllah_verse.Words[2].Parts[1].Tag,
                                                          new WordPartFeatures(bismAllah_verse.Words[2].Parts[1].Features)
                                                        );

                                                        //(1:1:4:1)	{l	DET	PREFIX|Al+
                                                        new WordPart(verse.Words[3],
                                                          bismAllah_verse.Words[3].Parts[0].NumberInWord,
                                                          bismAllah_verse.Words[3].Parts[0].Buckwalter,
                                                          bismAllah_verse.Words[3].Parts[0].Tag,
                                                          new WordPartFeatures(bismAllah_verse.Words[3].Parts[0].Features)
                                                        );

                                                        //(1:1:4:2)	r~aHiymi	ADJ	STEM|POS:ADJ|LEM:r~aHiym|ROOT:rHm|MS|GEN
                                                        new WordPart(verse.Words[3],
                                                          bismAllah_verse.Words[3].Parts[1].NumberInWord,
                                                          bismAllah_verse.Words[3].Parts[1].Buckwalter,
                                                          bismAllah_verse.Words[3].Parts[1].Tag,
                                                          new WordPartFeatures(bismAllah_verse.Words[3].Parts[1].Features)
                                                        );
                                                    }
                                                }

                                                // correct word_number (if needed) for all subsequenct chapter word_parts
                                                if (
                                                    ((chapter_number != 1) && (chapter_number != 9)) && (verse_number == 1)
                                                   )
                                                {
                                                    word_number += 4;
                                                }
                                                Word word = verse.Words[word_number - 1];
                                                if (word != null)
                                                {
                                                    List<string> features = new List<string>(parts[3].Split('|'));
                                                    if (features.Count > 0)
                                                    {
                                                        new WordPart(word, word_part_number, buckwalter, tag, features);
                                                    }
                                                    else
                                                    {
                                                        throw new Exception("LoadQuranWordParts: Features field is missing.\r\n" + filename);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        throw new Exception("LoadQuranWordParts: Invalid Location Format.\r\n" + filename);
                                    }
                                }
                                else
                                {
                                    throw new Exception("LoadQuranWordParts: Invalid File Format.\r\n" + filename);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("LoadQuranWordParts: " + ex.Message);
                        }
                    }
                }
            }
        }
    }

    // quran recitations
    private static Dictionary<string, Recitation> s_quran_recitations = null;
    public static Dictionary<string, Recitation> QuranRecitations
    {
        get
        {
            if (s_quran_recitations == null)
            {
                LoadQuranRecitations();
            }
            return s_quran_recitations;
        }
    }
    private static void LoadQuranRecitations()
    {
        s_quran_recitations = new Dictionary<string, Recitation>();
        string filename = s_recitations_directory + "/" + "metadata.txt";
        if (File.Exists(filename))
        {
            using (StreamReader reader = File.OpenText(filename))
            {
                string line = reader.ReadLine(); // skip header row
                while ((line = reader.ReadLine()) != null)
                {
                    string[] parts = line.Split('\t');
                    if (parts.Length >= 4)
                    {
                        Recitation recitation = new Recitation();
                        recitation.Url = parts[0];
                        recitation.Folder = parts[0];
                        recitation.Language = parts[1];
                        recitation.Reciter = parts[2];
                        int.TryParse(parts[3], out recitation.Quality);
                        recitation.Name = recitation.Language + " - " + recitation.Reciter;
                        s_quran_recitations.Add(parts[0], recitation);
                    }
                }
            }
        }
    }

    // load Quran book with Verse.MAX_NUMBER lines
    private static List<Verse> LoadBookVerses(string filename)
    {
        List<Verse> verses = new List<Verse>();
        if (File.Exists(filename))
        {
            using (StreamReader reader = File.OpenText(filename))
            {
                string line = "";
                for (int i = 0; i < Verse.MAX_NUMBER; i++)
                {
                    line = reader.ReadLine();
                    if (line != null)
                    {
                        Verse verse = new Verse(i + 1, line);
                        verses.Add(verse);
                    }
                }
            }
        }
        return verses;
    }
    
    public static List<string> LoadLines(string filename)
    {
        List<string> result = new List<string>();
        if (File.Exists(filename))
        {
            using (StreamReader reader = File.OpenText(filename))
            {
                string line = "";
                while ((line = reader.ReadLine()) != null)
                {
                    result.Add(line);
                }
            }
        }
        return result;
    }

    public static string LoadFile(string filename)
    {
        StringBuilder str = new StringBuilder();
        if (File.Exists(filename))
        {
            using (StreamReader reader = File.OpenText(filename))
            {
                string line = "";
                while ((line = reader.ReadLine()) != null)
                {
                    str.AppendLine(line);
                }
            }
        }
        return str.ToString();
    }
    public static void SaveFile(string filename, string content)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(filename, false, Encoding.Unicode))
            {
                writer.Write(content);
            }
        }
        catch
        {
            // silence IO error in case running from read-only media (CD/DVD)
        }
    }
}
