using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Globalization;
using Model;

public static class Server
{
    static Server()
    {
    }

    // names of all text_modes loaded from the Rules folder
    private static List<string> s_text_modes = new List<string>();
    public static List<string> TextModes
    {
        get { return s_text_modes; }
    }

    // translations
    public static string GetTranslationKey(string name)
    {
        foreach (string key in Server.Translations.Keys)
        {
            if (Server.Translations[key].Name == name)
            {
                return key;
            }
        }
        return null;
    }
    public static void LoadTranslationBook(string title)
    {
        DataAccess.LoadQuranTranslationBook(title);
    }
    public static void UnloadTranslationBook(string title)
    {
        DataAccess.UnloadQuranTranslationBook(title);
    }

    // translation infos
    public static Dictionary<string, Translation> Translations
    {
        get { return DataAccess.QuranTranslations; }
    }

    // recitation infos
    public static Dictionary<string, Recitation> Recitations
    {
        get { return DataAccess.QuranRecitations; }
    }

    public static string DEFAULT_TEXT_MODE = "Simplified29";
    public static string DEFAULT_LETTER_ORDER = "Alphabet";
    public static string DEFAULT_LETTER_VALUES = "Primes";

    private static bool s_initialized = false;
    public static bool Initialize()
    {
        try
        {
            // prevent re-initialization
            if (s_initialized) return true;

            if (DataAccess.Initialize())
            {
                LoadTranslationBook("ar.emlaaei");

                if (LoadSimplificationRules())
                {
                    if (LoadNumerologySystems())
                    {
                        // simplify "ar.emlaaei" for EXACT text find
                        Book book = Book.Instance;
                        if (book != null)
                        {
                            foreach (Verse verse in book.Verses)
                            {
                                string text = verse.Translations["ar.emlaaei"];
                                verse.Translations["ar.emlaaei"] = SimplifyText(text, "Simplified37");
                            }
                        }

                        s_initialized = true;
                    }
                }
            }
        }
        catch
        {
            s_initialized = false;
        }

        return s_initialized;
    }
    public static bool Initialized
    {
        get { return s_initialized; }
    }

    private struct SimplificationRule
    {
        public string text;
        public string replacement;
    }
    private static Dictionary<string, List<SimplificationRule>> s_simplification_ruless = null;
    private static string s_simplification_rules_directory = "Rules";
    public static bool LoadSimplificationRules()
    {
        if (s_simplification_ruless == null)
        {
            s_simplification_ruless = new Dictionary<string, List<SimplificationRule>>();
            if (Directory.Exists(s_simplification_rules_directory))
            {
                string[] paths = Directory.GetFiles(s_simplification_rules_directory + "/", "*.txt", SearchOption.TopDirectoryOnly);
                foreach (string path in paths)
                {
                    using (StreamReader reader = File.OpenText(path))
                    {
                        string text_mode = Path.GetFileNameWithoutExtension(path);

                        // remove some text modes from some Editions
                        switch (Globals.EDITION)
                        {
                            case Edition.Lite:
                                {
                                    if (
                                        (text_mode == "Simplified29Shadda") ||
                                        (text_mode == "Simplified30") ||
                                        (text_mode == "Simplified31") ||
                                        (text_mode == "Simplified37")
                                       )
                                    {
                                        continue; // skip text mode
                                    }
                                }
                                break;
                            case Edition.Standard:
                            case Edition.Grammar:
                                {
                                    if (
                                        (text_mode == "Simplified30")
                                       )
                                    {
                                        continue; // skip text mode
                                    }
                                }
                                break;
                            case Edition.Research:
                            default:
                                {
                                }
                                break;
                        }

                        s_text_modes.Add(text_mode);
                    }
                }
            }

            foreach (string text_mode in s_text_modes)
            {
                string filename = s_simplification_rules_directory + "/" + text_mode + ".txt";
                if (File.Exists(filename))
                {
                    List<SimplificationRule> simplification_rules = new List<SimplificationRule>();
                    using (StreamReader reader = File.OpenText(filename))
                    {
                        string line = "";
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.StartsWith("#")) continue;

                            string[] parts = line.Split('\t');
                            if (parts.Length == 2)
                            {
                                SimplificationRule simplification_rule = new SimplificationRule();
                                simplification_rule.text = parts[0];
                                simplification_rule.replacement = parts[1];
                                simplification_rules.Add(simplification_rule);
                            }
                            else
                            {
                                throw new Exception(filename + " file format: Text TAB Replacement.");
                            }
                        }
                    }
                    s_simplification_ruless.Add(text_mode, simplification_rules);
                }
            }
            return true;
        }
        else
        {
            return false;
        }
    }
    public static void SimplifyBook(string to_text_mode)
    {
        Book book = Book.Instance;
        if (book != null)
        {
            if (to_text_mode.Length > 0)
            {
                if (s_simplification_ruless != null)
                {
                    if (s_simplification_ruless.ContainsKey(to_text_mode))
                    {
                        List<SimplificationRule> simplification_rules = s_simplification_ruless[to_text_mode];

                        if (to_text_mode.Contains("Shadda"))
                        {
                            foreach (Verse verse in book.Verses)
                            {
                                string text = verse.GetText("Original");

                                StringBuilder str = new StringBuilder(text);
                                // remove shedda in 7:196 as it is above a superscripted letter
                                if ((verse.Chapter.Number == 7) && (verse.NumberInChapter == 196))
                                {
                                    text = text.Replace("ّۧ", "ۧ");
                                }
                                for (int i = 1; i < str.Length; i++)
                                {
                                    if (str[i] == 'ّ') // replace shedda with previous letter
                                    {
                                        str[i] = str[i - 1];
                                    }
                                }
                                text = str.ToString();

                                foreach (SimplificationRule simplification_rule in simplification_rules)
                                {
                                    text = text.Replace(simplification_rule.text, simplification_rule.replacement);
                                }
                                verse.SetText(to_text_mode, text);
                            }
                        }
                        else
                        {
                            foreach (Verse verse in book.Verses)
                            {
                                string text = verse.GetText("Original");
                                foreach (SimplificationRule simplification_rule in simplification_rules)
                                {
                                    text = text.Replace(simplification_rule.text, simplification_rule.replacement);
                                }
                                verse.SetText(to_text_mode, text);
                            }
                        }
                    }
                }
            }
        }
    }

    // on-the-fly simplification of pasted text from clipboard
    public static string SimplifyText(string text, string to_text_mode)
    {
        if (to_text_mode.Length > 0)
        {
            if (s_simplification_ruless != null)
            {
                if (s_simplification_ruless.ContainsKey(to_text_mode))
                {
                    List<SimplificationRule> simplification_rules = s_simplification_ruless[to_text_mode];
                    foreach (SimplificationRule simplification_rule in simplification_rules)
                    {
                        text = text.Replace(simplification_rule.text, simplification_rule.replacement);
                    }
                }
            }
        }
        return text;
    }

    /// <summary>
    /// Update positions of all phrases from phrase text mode to client's current text mode
    /// </summary>
    /// <param name="phrases">phrases to recalculate their positions</param>
    /// <param name="text_mode">new text mode</param>
    /// <returns></returns>
    public static void UpdatePhrasePositions(ref List<Phrase> phrases, string from_text_mode, string to_text_mode)
    {
        if (from_text_mode == to_text_mode) return;

        // NOTE: Original text mode still not working correctly. It finds the positions but uses the simplified length which is too short
        // letter position stays the same except for Simplified28 and Simplified29Shadda
        Book book = Book.Instance;
        if (book != null)
        {
            if (phrases != null)
            {
                if (phrases.Count > 0)
                {
                    List<Phrase> temp = new List<Phrase>();
                    for (int i = 0; i < phrases.Count; i++)
                    {
                        Verse phrase_verse = phrases[i].Verse;
                        string phrase_text = phrases[i].Text;
                        int phrase_position = phrases[i].Position;

                        Verse verse = book.Verses[phrase_verse.Number - 1];
                        string text = SimplifyText(phrase_text, to_text_mode);

                        // FIX: double/half word length between Original and SimplifiedXX for now.
                        if (to_text_mode == "Original")
                        {
                            text += text;
                        }
                        else // to SimplifiedXX
                        {
                            if (from_text_mode == "Original")
                            {
                                int index = text.Length / 2;
                                if ((index >= 0) && (index < text.Length))
                                {
                                    text = text.Remove(index);
                                }
                            }
                        }

                        // process standalone hamza which is supported by all text_modes except Simplified28 and Simplified30
                        if (
                            ((to_text_mode == "Simplified28") && (from_text_mode != "Simplified28")) ||
                            ((to_text_mode == "Simplified28") && (from_text_mode != "Simplified30")) ||
                            ((to_text_mode == "Simplified30") && (from_text_mode != "Simplified28")) ||
                            ((to_text_mode == "Simplified30") && (from_text_mode != "Simplified30"))
                           )
                        {
                            // skip standalone hamzas in from_text_mode
                            int hamzas = 0;
                            for (int j = 0; j <= phrase_position; j++)
                            {
                                if (phrase_verse.GetText(from_text_mode)[j] == 'ء')
                                {
                                    hamzas++;
                                }
                            }
                            phrase_position -= hamzas;
                        }
                        else if
                           (
                            ((to_text_mode != "Simplified28") && (to_text_mode != "Simplified30")) &&
                            ((from_text_mode == "Simplified28") || (from_text_mode == "Simplified30"))
                           )
                        {
                            // increment positions for standalone hamzas in to_text_mode
                            int hamzas = 0;
                            for (int j = 0; j <= phrase_position; j++)
                            {
                                if (verse.GetText(to_text_mode)[j] == 'ء')
                                {
                                    hamzas++;
                                }
                            }
                            phrase_position += hamzas;
                        }

                        int position = -1;
                        int spaces = 0;
                        int letters = 0;
                        if (from_text_mode == "Original")
                        {
                            MatchCollection matches = Regex.Matches(verse.GetText(to_text_mode), text);
                            foreach (Match match in matches)
                            {
                                position = match.Index;
                                Phrase phrase = new Phrase(verse, to_text_mode, text, position);
                                temp.Add(phrase);
                            }
                        }
                        else if (to_text_mode == "Original")
                        {
                            for (int j = 0; j < verse.GetText(to_text_mode).Length; j++)
                            {
                                if (verse.GetText(to_text_mode)[j] == ' ')
                                {
                                    if (j > 2)
                                    {
                                        // don't add space for stop_mark words
                                        char previous_previous_letter = verse.GetText(to_text_mode)[j - 2];
                                        if (previous_previous_letter == ' ')
                                        {
                                            char previous_letter = verse.GetText(to_text_mode)[j - 1];
                                            if ((previous_letter == 'ص') || (previous_letter == 'ق') || (previous_letter == 'ن') || (previous_letter == 'و'))
                                            {
                                                spaces++;
                                            }
                                        }
                                        else
                                        {
                                            spaces++;
                                        }
                                    }
                                    else
                                    {
                                        spaces++;
                                    }
                                }
                                else if (Constants.ARABIC_LETTERS.Contains(verse.GetText(to_text_mode)[j]))
                                {
                                    letters++;

                                    if ((letters + spaces) > phrase_position)
                                    {
                                        position = j;
                                        break;
                                    }
                                }
                            }
                            Phrase phrase = new Phrase(verse, to_text_mode, text, position);
                            temp.Add(phrase);
                        }
                        else
                        {
                            position = phrase_position;
                            Phrase phrase = new Phrase(verse, to_text_mode, text, position);
                            temp.Add(phrase);
                        }
                    }

                    phrases = temp;
                }
            }
        }
    }

    // Book | Selection | HighlightedText-wide
    private static Dictionary<string, Dictionary<char, long>> s_numerology_systems = null;
    public static Dictionary<string, Dictionary<char, long>> NumerologySystems
    {
        get { return s_numerology_systems; }
    }

    private static string s_numerology_systems_directory = "Values";
    public static bool LoadNumerologySystems()
    {
        // update dynamic numerology systems
        UpdateDynamicNumerologySystems(DEFAULT_TEXT_MODE);

        // load user-defined systems
        return LoadNumerologySystems("all");
    }
    private static bool LoadNumerologySystems(string text_mode)
    {
        if (s_numerology_systems == null)
        {
            s_numerology_systems = new Dictionary<string, Dictionary<char, long>>();
        }

        if (s_numerology_systems != null)
        {
            if (Directory.Exists(s_numerology_systems_directory))
            {
                string[] paths = Directory.GetFiles(s_numerology_systems_directory + "/", "*.txt", SearchOption.TopDirectoryOnly);
                foreach (string path in paths)
                {
                    using (StreamReader reader = File.OpenText(path))
                    {
                        string numerology_system_name = Path.GetFileNameWithoutExtension(path);

                        if ((text_mode == "all") || (numerology_system_name.StartsWith(text_mode)))
                        {
                            // overwrite auto-generated systems with user defined ones (if any)
                            if (s_numerology_systems.ContainsKey(numerology_system_name))
                            {
                                s_numerology_systems.Remove(numerology_system_name);
                            }
                            s_numerology_systems.Add(numerology_system_name, new Dictionary<char, long>());

                            string line = "";
                            while ((line = reader.ReadLine()) != null)
                            {
                                string[] parts = line.Split('\t');
                                if (parts.Length == 2)
                                {
                                    s_numerology_systems[numerology_system_name].Add(parts[0][0], int.Parse(parts[1]));
                                    //                                      [0][0] -->
                                    //                                first [0]        for first string,
                                    //                                  second [0]     for first char
                                }
                                else
                                {
                                    throw new Exception(path + " file format: Letter TAB Value.");
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }
        else
        {
            return false;
        }
    }
    public static void UpdateNumerologySystems(string text_mode, string text, bool is_dynamic)
    {
        // fill up dynamic letter statistics dictionary
        CalculateDynamicLetterStatistics(text);

        // update dynamic numerology systems
        UpdateDynamicNumerologySystems(text_mode);

        // re-load user-defined systems
        if (!is_dynamic)
        {
            LoadNumerologySystems(text_mode);
        }
    }

    // calculate s_dynamic_letter_statistics on the fly
    // and fill up dynamic letter statistics dictionary
    private static List<LetterStatistic> s_dynamic_letter_statistics = new List<LetterStatistic>();
    private static void CalculateDynamicLetterStatistics(string text)
    {
        text = text.Replace("\r", "");
        text = text.Replace("\n", "");
        text = text.Replace(" ", "");

        s_dynamic_letter_statistics.Clear();
        for (int i = 0; i < text.Length; i++)
        {
            // calculate letter frequency
            bool is_found = false;
            for (int j = 0; j < s_dynamic_letter_statistics.Count; j++)
            {
                if (text[i] == s_dynamic_letter_statistics[j].Letter)
                {
                    is_found = true;
                    s_dynamic_letter_statistics[j].Frequency++;
                }
            }

            // add entry into dictionary
            if (!is_found)
            {
                LetterStatistic letter_statistic = new LetterStatistic();
                letter_statistic.Order = s_dynamic_letter_statistics.Count + 1;
                letter_statistic.Letter = text[i];
                letter_statistic.Frequency++;
                s_dynamic_letter_statistics.Add(letter_statistic);
            }
        }
    }

    // setup all dynamic numerology systems { ↓↑→←▼▲ }
    private static void UpdateDynamicNumerologySystems(string text_mode)
    {
        UpdateAlphabetForwardDynamicNumerologySystems(text_mode);
        if (Globals.EDITION == Edition.Research)
        {
            UpdateAlphabetBackwardDynamicNumerologySystems(text_mode);
        }

        UpdateAppearanceForwardDynamicNumerologySystems(text_mode);
        if (Globals.EDITION == Edition.Research)
        {
            UpdateAppearanceBackwardDynamicNumerologySystems(text_mode);
        }

        if (Globals.EDITION == Edition.Research)
        {
            UpdateFrequencyAscendingDynamicNumerologySystems(text_mode);
        }
        UpdateFrequencyDescendingDynamicNumerologySystems(text_mode);

        if (Globals.EDITION == Edition.Research)
        {
            Update003154DynamicNumerologySystems(text_mode);
            Update048029DynamicNumerologySystems(text_mode);
        }
    }

    // TextMode_003.154_Values
    private static void Update003154DynamicNumerologySystems(string text_mode)
    {
        if ((text_mode != null) && (text_mode.StartsWith("Simplified")))
        {
            Book book = Book.Instance;
            if (book != null)
            {
                string partial_numerology_system_name = text_mode + "_" + "Quran 3.154" + "_" + "";
                // find chapter manually not using chapter index = 2 which is incorrect in chronological order
                foreach (Chapter chapter in book.Chapters)
                {
                    if (chapter.Number == 3)
                    {
                        CalculateDynamicLetterStatistics(chapter.Verses[153].GetText(text_mode));
                        break;
                    }
                }
                UpdateNumerologySystemsStartingWith(partial_numerology_system_name);
            }
        }
    }
    // TextMode_048.29_Values
    private static void Update048029DynamicNumerologySystems(string text_mode)
    {
        if ((text_mode != null) && (text_mode.StartsWith("Simplified")))
        {
            Book book = Book.Instance;
            if (book != null)
            {
                string partial_numerology_system_name = text_mode + "_" + "Quran 48.29" + "_" + "";
                // find chapter manually not using chapter index = 47 which is incorrect in chronological order
                foreach (Chapter chapter in book.Chapters)
                {
                    if (chapter.Number == 48)
                    {
                        CalculateDynamicLetterStatistics(chapter.Verses[28].GetText(text_mode));
                        break;
                    }
                }
                UpdateNumerologySystemsStartingWith(partial_numerology_system_name);
            }
        }
    }

    // TextMode_Alphabet_Values
    private static void UpdateAlphabetForwardDynamicNumerologySystems(string text_mode)
    {
        string partial_numerology_system_name = "";
        partial_numerology_system_name = text_mode + "_" + "Alphabet" + "_" + "";
        LetterStatistic.SortOrder = StatisticSortOrder.Ascending;
        LetterStatistic.SortMethod = StatisticSortMethod.ByLetter;
        s_dynamic_letter_statistics.Sort();
        UpdateNumerologySystemsStartingWith(partial_numerology_system_name);
    }
    // TextMode_Alphabet▼_Values
    private static void UpdateAlphabetBackwardDynamicNumerologySystems(string text_mode)
    {
        string partial_numerology_system_name = text_mode + "_" + "Alphabet▼" + "_" + "";
        LetterStatistic.SortOrder = StatisticSortOrder.Descending;
        LetterStatistic.SortMethod = StatisticSortMethod.ByLetter;
        s_dynamic_letter_statistics.Sort();
        UpdateNumerologySystemsStartingWith(partial_numerology_system_name);
    }
    // TextMode_Appearance_Values
    private static void UpdateAppearanceForwardDynamicNumerologySystems(string text_mode)
    {
        string partial_numerology_system_name = "";
        partial_numerology_system_name = text_mode + "_" + "Appearance" + "_" + "";
        LetterStatistic.SortOrder = StatisticSortOrder.Ascending;
        LetterStatistic.SortMethod = StatisticSortMethod.ByOrder;
        s_dynamic_letter_statistics.Sort();
        UpdateNumerologySystemsStartingWith(partial_numerology_system_name);
    }
    // TextMode_Appearance▼_Values
    private static void UpdateAppearanceBackwardDynamicNumerologySystems(string text_mode)
    {
        string partial_numerology_system_name = text_mode + "_" + "Appearance▼" + "_" + "";
        LetterStatistic.SortOrder = StatisticSortOrder.Descending;
        LetterStatistic.SortMethod = StatisticSortMethod.ByOrder;
        s_dynamic_letter_statistics.Sort();
        UpdateNumerologySystemsStartingWith(partial_numerology_system_name);
    }
    // TextMode_Frequency▲_Values
    private static void UpdateFrequencyAscendingDynamicNumerologySystems(string text_mode)
    {
        string partial_numerology_system_name = text_mode + "_" + "Frequency▲" + "_" + "";
        LetterStatistic.SortOrder = StatisticSortOrder.Ascending;
        LetterStatistic.SortMethod = StatisticSortMethod.ByFrequency;
        s_dynamic_letter_statistics.Sort();
        UpdateNumerologySystemsStartingWith(partial_numerology_system_name);
    }
    // TextMode_Frequency_Values
    private static void UpdateFrequencyDescendingDynamicNumerologySystems(string text_mode)
    {
        string partial_numerology_system_name = "";
        partial_numerology_system_name = text_mode + "_" + "Frequency" + "_" + "";
        LetterStatistic.SortOrder = StatisticSortOrder.Descending;
        LetterStatistic.SortMethod = StatisticSortMethod.ByFrequency;
        s_dynamic_letter_statistics.Sort();
        UpdateNumerologySystemsStartingWith(partial_numerology_system_name);
    }

    // helper method for the __Values
    private static void UpdateNumerologySystemsStartingWith(string partial_numerology_system_name)
    {
        string numerology_system_name = "";
        List<char> letter_order = new List<char>();
        foreach (LetterStatistic letter_statistic in s_dynamic_letter_statistics)
        {
            letter_order.Add(letter_statistic.Letter);
        }

        List<long> letter_values = new List<long>();
        //if (letter_order.Count > 0)   // commented out to allow empty numerology_system as placeholders
        {
            numerology_system_name = partial_numerology_system_name + "Linear";
            letter_values.Clear();
            for (long i = 0; i < letter_order.Count; i++)
            {
                letter_values.Add(i + 1);
            }
            UpdateNumerologySystem(numerology_system_name, letter_order, letter_values);

            if (Globals.EDITION != Edition.Lite)
            {
                numerology_system_name = partial_numerology_system_name + "Odds";
                letter_values.Clear();
                for (long i = 0; i < letter_order.Count; i++)
                {
                    letter_values.Add(2 * i + 1);
                }
                UpdateNumerologySystem(numerology_system_name, letter_order, letter_values);

                numerology_system_name = partial_numerology_system_name + "Evens";
                letter_values.Clear();
                for (long i = 0; i < letter_order.Count; i++)
                {
                    letter_values.Add(2 * i);
                }
                UpdateNumerologySystem(numerology_system_name, letter_order, letter_values);
            }

            numerology_system_name = partial_numerology_system_name + "Primes";
            letter_values.Clear();
            for (int i = 0; i < letter_order.Count; i++)
            {
                if (i < Numbers.Primes.Count)
                {
                    letter_values.Add(Numbers.Primes[i]);     // include 1
                }
                else
                {
                    letter_values.Add(0);
                }
            }
            UpdateNumerologySystem(numerology_system_name, letter_order, letter_values);

            if (Globals.EDITION != Edition.Lite)
            {
                numerology_system_name = partial_numerology_system_name + "AdditivePrimes";
                letter_values.Clear();
                for (int i = 0; i < letter_order.Count; i++)
                {
                    if (i < Numbers.AdditivePrimes.Count)
                    {
                        letter_values.Add(Numbers.AdditivePrimes[i]);     // include 1
                    }
                    else
                    {
                        letter_values.Add(0);
                    }
                }
                UpdateNumerologySystem(numerology_system_name, letter_order, letter_values);

                numerology_system_name = partial_numerology_system_name + "PurePrimes";
                letter_values.Clear();
                for (int i = 0; i < letter_order.Count; i++)
                {
                    if (i < Numbers.PurePrimes.Count)
                    {
                        letter_values.Add(Numbers.PurePrimes[i]);     // include 1
                    }
                    else
                    {
                        letter_values.Add(0);
                    }
                }
                UpdateNumerologySystem(numerology_system_name, letter_order, letter_values);
            }

            if (Globals.EDITION == Edition.Research)
            {
                numerology_system_name = partial_numerology_system_name + "Primes2";
                letter_values.Clear();
                for (int i = 0; i < letter_order.Count; i++)
                {
                    if (i < Numbers.Primes.Count)
                    {
                        letter_values.Add(Numbers.Primes[i + 1]); // exclude 1
                    }
                    else
                    {
                        letter_values.Add(0);
                    }
                }
                UpdateNumerologySystem(numerology_system_name, letter_order, letter_values);

                numerology_system_name = partial_numerology_system_name + "AdditivePrimes2";
                letter_values.Clear();
                for (int i = 0; i < letter_order.Count; i++)
                {
                    if (i < Numbers.AdditivePrimes.Count)
                    {
                        letter_values.Add(Numbers.AdditivePrimes[i + 1]); // exclude 1
                    }
                    else
                    {
                        letter_values.Add(0);
                    }
                }
                UpdateNumerologySystem(numerology_system_name, letter_order, letter_values);

                numerology_system_name = partial_numerology_system_name + "PurePrimes2";
                letter_values.Clear();
                for (int i = 0; i < letter_order.Count; i++)
                {
                    if (i < Numbers.PurePrimes.Count)
                    {
                        letter_values.Add(Numbers.PurePrimes[i + 1]); // exclude 1
                    }
                    else
                    {
                        letter_values.Add(0);
                    }
                }
                UpdateNumerologySystem(numerology_system_name, letter_order, letter_values);
            }

            if (Globals.EDITION != Edition.Lite)
            {
                numerology_system_name = partial_numerology_system_name + "MersennePrimes";
                letter_values.Clear();
                for (int i = 0; i < letter_order.Count; i++)
                {
                    if (i < Numbers.MersennePrimes.Count)
                    {
                        letter_values.Add(Numbers.MersennePrimes[i]);
                    }
                    else
                    {
                        letter_values.Add(0);
                    }
                }
                UpdateNumerologySystem(numerology_system_name, letter_order, letter_values);
            }

            numerology_system_name = partial_numerology_system_name + "Composites";
            letter_values.Clear();
            for (int i = 0; i < letter_order.Count; i++)
            {
                if (i < Numbers.Composites.Count)
                {
                    letter_values.Add(Numbers.Composites[i]);
                }
                else
                {
                    letter_values.Add(0);
                }
            }
            UpdateNumerologySystem(numerology_system_name, letter_order, letter_values);

            if (Globals.EDITION != Edition.Lite)
            {
                numerology_system_name = partial_numerology_system_name + "AdditiveComposites";
                letter_values.Clear();
                for (int i = 0; i < letter_order.Count; i++)
                {
                    if (i < Numbers.AdditiveComposites.Count)
                    {
                        letter_values.Add(Numbers.AdditiveComposites[i]);
                    }
                    else
                    {
                        letter_values.Add(0);
                    }
                }
                UpdateNumerologySystem(numerology_system_name, letter_order, letter_values);

                numerology_system_name = partial_numerology_system_name + "PureComposites";
                letter_values.Clear();
                for (int i = 0; i < letter_order.Count; i++)
                {
                    if (i < Numbers.PureComposites.Count)
                    {
                        letter_values.Add(Numbers.PureComposites[i]);
                    }
                    else
                    {
                        letter_values.Add(0);
                    }
                }
                UpdateNumerologySystem(numerology_system_name, letter_order, letter_values);
            }

            if (Globals.EDITION != Edition.Lite)
            {
                numerology_system_name = partial_numerology_system_name + "Fibonacci";
                letter_values.Clear();
                for (int i = 0; i < letter_order.Count; i++)
                {
                    if (i < Numbers.Fibonaccis.Count)
                    {
                        letter_values.Add(Numbers.Fibonaccis[i]);
                    }
                    else
                    {
                        letter_values.Add(0);
                    }
                }
                UpdateNumerologySystem(numerology_system_name, letter_order, letter_values);
            }

            if (Globals.EDITION == Edition.Research)
            {
                // letter-frequency mismacth: different letters for different frequencies
                numerology_system_name = partial_numerology_system_name + "Frequency▲";
                LetterStatistic.SortOrder = StatisticSortOrder.Ascending;
                LetterStatistic.SortMethod = StatisticSortMethod.ByFrequency;
                s_dynamic_letter_statistics.Sort();
                letter_values.Clear();
                foreach (LetterStatistic letter_statistic in s_dynamic_letter_statistics)
                {
                    letter_values.Add(letter_statistic.Frequency);
                }
                UpdateNumerologySystem(numerology_system_name, letter_order, letter_values);
            }

            numerology_system_name = partial_numerology_system_name + "Frequency";
            // letter-frequency mismacth: different letters for different frequencies
            LetterStatistic.SortOrder = StatisticSortOrder.Descending;
            LetterStatistic.SortMethod = StatisticSortMethod.ByFrequency;
            s_dynamic_letter_statistics.Sort();
            letter_values.Clear();
            foreach (LetterStatistic letter_statistic in s_dynamic_letter_statistics)
            {
                letter_values.Add(letter_statistic.Frequency);
            }
            UpdateNumerologySystem(numerology_system_name, letter_order, letter_values);

            if (Globals.EDITION == Edition.Research)
            {
                // Log 2 -- > maximum 62 letters  !!!
                numerology_system_name = partial_numerology_system_name + "Log2";
                letter_values.Clear();
                for (int i = 0; i < letter_order.Count; i++)
                {
                    if (i < Numbers.Log2s.Count)
                    {
                        letter_values.Add(Numbers.Log2s[i]);
                    }
                    else
                    {
                        letter_values.Add(0);
                    }
                }
                UpdateNumerologySystem(numerology_system_name, letter_order, letter_values);

                // Log 10 -- > maximum 19 letters  !!!
                numerology_system_name = partial_numerology_system_name + "Log10";
                letter_values.Clear();
                for (int i = 0; i < letter_order.Count; i++)
                {
                    if (i < Numbers.Log10s.Count)
                    {
                        letter_values.Add(Numbers.Log10s[i]);
                    }
                    else
                    {
                        letter_values.Add(0);
                    }
                }
                UpdateNumerologySystem(numerology_system_name, letter_order, letter_values);
            }

            if (Globals.EDITION == Edition.Research)
            {
                // Pivot numbers for books with pivot/support chapters (eg Quran has N=114, O=N/2=57, O=P1*P2=3*19 has P3=Sum(1..57)=29 verses)
                numerology_system_name = partial_numerology_system_name + "PivotBooks"; // 114
                letter_values.Clear();
                for (int i = 0; i < letter_order.Count; i++)
                {
                    if (i < Numbers.Pivots("PivotBooks").Count)
                    {
                        letter_values.Add(Numbers.Pivots("PivotBooks")[i]);
                    }
                    else
                    {
                        letter_values.Add(0);
                    }
                }
                UpdateNumerologySystem(numerology_system_name, letter_order, letter_values);

                numerology_system_name = partial_numerology_system_name + "PivotChapters"; // 114 / 2 = 57 Al-Hadeed
                letter_values.Clear();
                for (int i = 0; i < letter_order.Count; i++)
                {
                    if (i < Numbers.Pivots("PivotChapters").Count)
                    {
                        letter_values.Add(Numbers.Pivots("PivotChapters")[i]);
                    }
                    else
                    {
                        letter_values.Add(0);
                    }
                }
                UpdateNumerologySystem(numerology_system_name, letter_order, letter_values);

                numerology_system_name = partial_numerology_system_name + "PivotChapterFirstFactors"; // 57 = 3 * 19
                letter_values.Clear();
                for (int i = 0; i < letter_order.Count; i++)
                {
                    if (i < Numbers.Pivots("PivotChapterFirstFactors").Count)
                    {
                        letter_values.Add(Numbers.Pivots("PivotChapterFirstFactors")[i]);
                    }
                    else
                    {
                        letter_values.Add(0);
                    }
                }
                UpdateNumerologySystem(numerology_system_name, letter_order, letter_values);

                numerology_system_name = partial_numerology_system_name + "PivotChapterSecondFactors"; // 57 = 3 * 19
                letter_values.Clear();
                for (int i = 0; i < letter_order.Count; i++)
                {
                    if (i < Numbers.Pivots("PivotChapterSecondFactors").Count)
                    {
                        letter_values.Add(Numbers.Pivots("PivotChapterSecondFactors")[i]);
                    }
                    else
                    {
                        letter_values.Add(0);
                    }
                }
                UpdateNumerologySystem(numerology_system_name, letter_order, letter_values);

                numerology_system_name = partial_numerology_system_name + "PivotChapterVerses"; // Mid(57) = 29
                letter_values.Clear();
                for (int i = 0; i < letter_order.Count; i++)
                {
                    if (i < Numbers.Pivots("PivotChapterVerses").Count)
                    {
                        letter_values.Add(Numbers.Pivots("PivotChapterVerses")[i]);
                    }
                    else
                    {
                        letter_values.Add(0);
                    }
                }
                UpdateNumerologySystem(numerology_system_name, letter_order, letter_values);

                numerology_system_name = partial_numerology_system_name + "PivotSupportChapters"; // 3 * 29 = 87 Al-A3laa
                letter_values.Clear();
                for (int i = 0; i < letter_order.Count; i++)
                {
                    if (i < Numbers.Pivots("PivotSupportChapters").Count)
                    {
                        letter_values.Add(Numbers.Pivots("PivotSupportChapters")[i]);
                    }
                    else
                    {
                        letter_values.Add(0);
                    }
                }
                UpdateNumerologySystem(numerology_system_name, letter_order, letter_values);
            }

            if (Globals.EDITION == Edition.Research)
            {
                // First 24 Polygons
                int polygon_types = Enum.GetValues(typeof(PolygonType)).Length;
                for (int n = 0; n < polygon_types; n++)
                {
                    numerology_system_name = partial_numerology_system_name + ((PolygonType)n).ToString() + "s";
                    letter_values.Clear();
                    for (int i = 0; i < letter_order.Count; i++)
                    {
                        if (i < Numbers.PolygonNumbers(n + 3).Count)
                        {
                            letter_values.Add(Numbers.PolygonNumbers(n + 3)[i]);
                        }
                        else
                        {
                            letter_values.Add(0);
                        }
                    }
                    UpdateNumerologySystem(numerology_system_name, letter_order, letter_values);
                }

                // First 24 CenteredPolygons
                int centered_polygon_types = Enum.GetValues(typeof(PolygonType)).Length;
                for (int n = 0; n < centered_polygon_types; n++)
                {
                    numerology_system_name = partial_numerology_system_name + "Centred" + ((PolygonType)n).ToString() + "s";
                    letter_values.Clear();
                    for (int i = 0; i < letter_order.Count; i++)
                    {
                        if (i < Numbers.CenteredPolygonNumbers(n + 3).Count)
                        {
                            letter_values.Add(Numbers.CenteredPolygonNumbers(n + 3)[i]);
                        }
                        else
                        {
                            letter_values.Add(0);
                        }
                    }
                    UpdateNumerologySystem(numerology_system_name, letter_order, letter_values);
                }

                // First 24 PolyhexNumbers
                int polymer_types = Enum.GetValues(typeof(PolyhexType)).Length;
                for (int n = 0; n < polymer_types; n++)
                {
                    numerology_system_name = partial_numerology_system_name + ((PolyhexType)n).ToString() + "s";
                    letter_values.Clear();
                    for (int i = 0; i < letter_order.Count; i++)
                    {
                        if (i < Numbers.PolyhexNumbers(n + 1).Count)
                        {
                            letter_values.Add(Numbers.PolyhexNumbers(n + 1)[i]);
                        }
                        else
                        {
                            letter_values.Add(0);
                        }
                    }
                    UpdateNumerologySystem(numerology_system_name, letter_order, letter_values);
                }

                numerology_system_name = partial_numerology_system_name + "Tetrahedrals";
                letter_values.Clear();
                for (int i = 0; i < letter_order.Count; i++)
                {
                    if (i < Numbers.Tetrahedrals.Count)
                    {
                        letter_values.Add(Numbers.Tetrahedrals[i]);
                    }
                    else
                    {
                        letter_values.Add(0);
                    }
                }
                UpdateNumerologySystem(numerology_system_name, letter_order, letter_values);

                numerology_system_name = partial_numerology_system_name + "Pyramidals";
                letter_values.Clear();
                for (int i = 0; i < letter_order.Count; i++)
                {
                    if (i < Numbers.Pyramidals.Count)
                    {
                        letter_values.Add(Numbers.Pyramidals[i]);
                    }
                    else
                    {
                        letter_values.Add(0);
                    }
                }
                UpdateNumerologySystem(numerology_system_name, letter_order, letter_values);

                numerology_system_name = partial_numerology_system_name + "Cubics";
                letter_values.Clear();
                for (int i = 0; i < letter_order.Count; i++)
                {
                    if (i < Numbers.Cubics.Count)
                    {
                        letter_values.Add(Numbers.Cubics[i]);
                    }
                    else
                    {
                        letter_values.Add(0);
                    }
                }
                UpdateNumerologySystem(numerology_system_name, letter_order, letter_values);
            }

            //...
            //... add more letter valuation systems if needed ...
            //...
        }
    }
    private static void UpdateNumerologySystem(string numerology_system_name, List<char> letter_order, List<long> letter_values)
    {
        if (s_numerology_systems == null)
        {
            s_numerology_systems = new Dictionary<string, Dictionary<char, long>>();
        }

        if (s_numerology_systems != null)
        {
            Dictionary<char, long> numerology_system = new Dictionary<char, long>();
            for (int i = 0; i < letter_order.Count; i++)
            {
                numerology_system.Add(letter_order[i], letter_values[i]);
            }

            if (s_numerology_systems.ContainsKey(numerology_system_name))
            {
                s_numerology_systems[numerology_system_name] = numerology_system;
            }
            else
            {
                s_numerology_systems.Add(numerology_system_name, numerology_system);
            }
        }
    }

    // used for non-Quran text, user text, or F11 wordwraped text only
    public static long CalculateValue(string user_text, string numerology_system_name)
    {
        if (String.IsNullOrEmpty(user_text)) return 0;

        long result = 0;
        if (numerology_system_name.Contains("English"))
        {
            user_text = user_text.ToUpper();
        }

        try
        {
            user_text = user_text.Replace(" ", "");
            user_text = user_text.Replace("\n", "");
            user_text = user_text.Replace("\n", "");
            string key = numerology_system_name;
            if (s_numerology_systems != null)
            {
                if (s_numerology_systems.ContainsKey(key))
                {
                    string[] parts = numerology_system_name.Split('_');
                    string text_mode = parts[0];
                    user_text = SimplifyText(user_text, text_mode);

                    Dictionary<char, long> numerology_system = s_numerology_systems[key];
                    if ((numerology_system != null) && (numerology_system.Count > 0))
                    {
                        for (int i = 0; i < user_text.Length; i++)
                        {
                            if (numerology_system.ContainsKey(user_text[i]))
                            {
                                result += numerology_system[user_text[i]];
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            result = 0;
        }
        return result;
    }

    // used for Quran text only
    public static long CalculateValue(Word word, string numerology_system_name,
        bool add_to_letter_l_number, bool add_to_letter_w_number, bool add_to_letter_v_number, bool add_to_letter_c_number, bool add_to_letter_l_distance, bool add_to_letter_w_distance, bool add_to_letter_v_distance, bool add_to_letter_c_distance, bool add_to_word_w_number, bool add_to_word_v_number, bool add_to_word_c_number, bool add_to_word_w_distance, bool add_to_word_v_distance, bool add_to_word_c_distance, bool add_to_verse_v_number, bool add_to_verse_c_number, bool add_to_verse_v_distance, bool add_to_verse_c_distance, bool add_to_chapter_c_number)
    {
        if (word == null) return 0;

        long result = 0;
        if (s_numerology_systems != null)
        {
            if (s_numerology_systems.ContainsKey(numerology_system_name))
            {
                string[] parts = numerology_system_name.Split('_');
                string text_mode = parts[0];
                string word_text = word.Texts[text_mode];

                Dictionary<char, long> numerology_system = s_numerology_systems[numerology_system_name];
                if ((numerology_system != null) && (numerology_system.Count > 0))
                {
                    if (word.Verse.Words.Count == 1)
                    {
                        // adjust value of verse
                        result += AdjustVerseValue(word.Verse, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance);
                    }

                    // adjust value of word
                    result += AdjustWordValue(word, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance);

                    for (int letter_index = 0; letter_index < word_text.Length; letter_index++)
                    {
                        char character = word_text[letter_index];

                        if (numerology_system.ContainsKey(character))
                        {
                            result += numerology_system[character];

                            // adjust value of letter
                            result += AdjustLetterValue(word, letter_index, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance);
                        }
                    }
                }
            }
        }
        return result;
    }
    public static long CalculateValue(Verse verse, string numerology_system_name,
        bool add_to_letter_l_number, bool add_to_letter_w_number, bool add_to_letter_v_number, bool add_to_letter_c_number, bool add_to_letter_l_distance, bool add_to_letter_w_distance, bool add_to_letter_v_distance, bool add_to_letter_c_distance, bool add_to_word_w_number, bool add_to_word_v_number, bool add_to_word_c_number, bool add_to_word_w_distance, bool add_to_word_v_distance, bool add_to_word_c_distance, bool add_to_verse_v_number, bool add_to_verse_c_number, bool add_to_verse_v_distance, bool add_to_verse_c_distance, bool add_to_chapter_c_number)
    {
        if (verse == null) return 0;

        long result = 0;
        if (s_numerology_systems != null)
        {
            if (s_numerology_systems.ContainsKey(numerology_system_name))
            {
                string[] parts = numerology_system_name.Split('_');
                string text_mode = parts[0];

                Dictionary<char, long> numerology_system = s_numerology_systems[numerology_system_name];
                if ((numerology_system != null) && (numerology_system.Count > 0))
                {
                    // adjust value of verse
                    result += AdjustVerseValue(verse, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance);

                    foreach (Word word in verse.Words)
                    {
                        string word_text = word.Texts[text_mode];

                        // adjust value of word
                        result += AdjustWordValue(word, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance);

                        for (int letter_index = 0; letter_index < word_text.Length; letter_index++)
                        {
                            char character = word_text[letter_index];

                            if (numerology_system.ContainsKey(character))
                            {
                                result += numerology_system[character];

                                // adjust value of letter
                                result += AdjustLetterValue(word, letter_index, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance);
                            }
                        }
                    }
                }
            }
        }

        verse.Value = result;
        return result;
    }
    public static long CalculateValue(List<Verse> verses, string numerology_system_name,
        bool add_to_letter_l_number, bool add_to_letter_w_number, bool add_to_letter_v_number, bool add_to_letter_c_number, bool add_to_letter_l_distance, bool add_to_letter_w_distance, bool add_to_letter_v_distance, bool add_to_letter_c_distance, bool add_to_word_w_number, bool add_to_word_v_number, bool add_to_word_c_number, bool add_to_word_w_distance, bool add_to_word_v_distance, bool add_to_word_c_distance, bool add_to_verse_v_number, bool add_to_verse_c_number, bool add_to_verse_v_distance, bool add_to_verse_c_distance, bool add_to_chapter_c_number)
    {
        if (verses == null) return 0;
        if (verses.Count == 0) return 0;

        long result = 0;

        if (verses[0].Chapter != null) // Chapter is null in translation books like emalaaei
        {
            List<Chapter> chapters = Book.GetChapters(verses);
            foreach (Chapter chapter in chapters)
            {
                // adjust value of chapter
                result += AdjustChapterValue(chapter, add_to_chapter_c_number);
            }
        }

        foreach (Verse verse in verses)
        {
            verse.Value = CalculateValue(verse, numerology_system_name, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);
            result += verse.Value;
        }

        return result;
    }
    public static long CalculateValue(Chapter chapter, string numerology_system_name,
        bool add_to_letter_l_number, bool add_to_letter_w_number, bool add_to_letter_v_number, bool add_to_letter_c_number, bool add_to_letter_l_distance, bool add_to_letter_w_distance, bool add_to_letter_v_distance, bool add_to_letter_c_distance, bool add_to_word_w_number, bool add_to_word_v_number, bool add_to_word_c_number, bool add_to_word_w_distance, bool add_to_word_v_distance, bool add_to_word_c_distance, bool add_to_verse_v_number, bool add_to_verse_c_number, bool add_to_verse_v_distance, bool add_to_verse_c_distance, bool add_to_chapter_c_number)
    {
        if (chapter == null) return 0;
        chapter.Value = CalculateValue(chapter.Verses, numerology_system_name, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);
        return chapter.Value;
    }
    public static long CalculateValue(Verse verse, int word_index, string numerology_system_name,
        bool add_to_letter_l_number, bool add_to_letter_w_number, bool add_to_letter_v_number, bool add_to_letter_c_number, bool add_to_letter_l_distance, bool add_to_letter_w_distance, bool add_to_letter_v_distance, bool add_to_letter_c_distance, bool add_to_word_w_number, bool add_to_word_v_number, bool add_to_word_c_number, bool add_to_word_w_distance, bool add_to_word_v_distance, bool add_to_word_c_distance, bool add_to_verse_v_number, bool add_to_verse_c_number, bool add_to_verse_v_distance, bool add_to_verse_c_distance, bool add_to_chapter_c_number)
    {
        if (verse == null) return 0;

        long result = 0;
        if (s_numerology_systems != null)
        {
            if (s_numerology_systems.ContainsKey(numerology_system_name))
            {
                string[] parts = numerology_system_name.Split('_');
                string text_mode = parts[0];

                if ((word_index >= 0) && (word_index < verse.Words.Count))
                {
                    Word word = verse.Words[word_index];
                    string word_text = word.Texts[text_mode];
                    for (int l = 0; l < word_text.Length; l++)
                    {
                        result += CalculateValue(verse, word_index, l, numerology_system_name, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);
                    }
                }
            }
        }
        return result;
    }
    public static long CalculateValue(Verse verse, int word_index, int letter_index, string numerology_system_name,
        bool add_to_letter_l_number, bool add_to_letter_w_number, bool add_to_letter_v_number, bool add_to_letter_c_number, bool add_to_letter_l_distance, bool add_to_letter_w_distance, bool add_to_letter_v_distance, bool add_to_letter_c_distance, bool add_to_word_w_number, bool add_to_word_v_number, bool add_to_word_c_number, bool add_to_word_w_distance, bool add_to_word_v_distance, bool add_to_word_c_distance, bool add_to_verse_v_number, bool add_to_verse_c_number, bool add_to_verse_v_distance, bool add_to_verse_c_distance, bool add_to_chapter_c_number)
    {
        if (verse == null) return 0;

        long result = 0;
        if (s_numerology_systems != null)
        {
            if (s_numerology_systems.ContainsKey(numerology_system_name))
            {
                string[] parts = numerology_system_name.Split('_');
                string text_mode = parts[0];

                Dictionary<char, long> numerology_system = s_numerology_systems[numerology_system_name];
                if ((numerology_system != null) && (numerology_system.Count > 0))
                {
                    if ((verse.Words.Count == 1) && (verse.Words[0].Letters.Count == 1))
                    {
                        // adjust value of verse
                        result += AdjustVerseValue(verse, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance);
                    }

                    if ((word_index >= 0) && (word_index < verse.Words.Count))
                    {
                        Word word = verse.Words[word_index];
                        string word_text = word.Texts[text_mode];
                        if (word.Letters.Count == 1)
                        {
                            // adjust value of word
                            result += AdjustWordValue(word, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance);
                        }

                        if ((letter_index >= 0) && (letter_index < word_text.Length))
                        {
                            char character = word_text[letter_index];
                            if (numerology_system.ContainsKey(character))
                            {
                                result += numerology_system[character];

                                // adjust value of letter
                                result += AdjustLetterValue(word, letter_index, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance);
                            }
                        }
                    }
                }
            }
        }
        return result;
    }
    public static long CalculateValue(List<Verse> verses, int first_verse_letter_number, int last_verse_letter_number, string numerology_system_name,
        bool add_to_letter_l_number, bool add_to_letter_w_number, bool add_to_letter_v_number, bool add_to_letter_c_number, bool add_to_letter_l_distance, bool add_to_letter_w_distance, bool add_to_letter_v_distance, bool add_to_letter_c_distance, bool add_to_word_w_number, bool add_to_word_v_number, bool add_to_word_c_number, bool add_to_word_w_distance, bool add_to_word_v_distance, bool add_to_word_c_distance, bool add_to_verse_v_number, bool add_to_verse_c_number, bool add_to_verse_v_distance, bool add_to_verse_c_distance, bool add_to_chapter_c_number)
    {
        if (verses == null) return 0;
        if (verses.Count == 0) return 0;

        long result = 0;

        if (verses[0].Chapter != null) // Chapter is null in translation books like emalaaei
        {
            List<Chapter> chapters = Book.GetChapters(verses);
            foreach (Chapter chapter in chapters)
            {
                // adjust value of chapter
                result += AdjustChapterValue(chapter, add_to_chapter_c_number);
            }
        }

        if (verses.Count == 1)
        {
            Verse first_verse = verses[0];
            result += CalculateMiddlePartValue(first_verse, first_verse_letter_number, last_verse_letter_number, numerology_system_name, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);
        }
        else if (verses.Count == 2)
        {
            Verse first_verse = verses[0];
            result += CalculateEndPartValue(first_verse, first_verse_letter_number, numerology_system_name, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);

            Verse last_verse = verses[1];
            result += CalculateBeginningPartValue(last_verse, last_verse_letter_number, numerology_system_name, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);
        }
        else //if (verses.Count > 2)
        {
            Verse first_verse = verses[0];
            result += CalculateEndPartValue(first_verse, first_verse_letter_number, numerology_system_name, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);

            // middle verses
            for (int i = 1; i < verses.Count - 1; i++)
            {
                result += CalculateValue(verses[i], numerology_system_name, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);
            }

            Verse last_verse = verses[verses.Count - 1];
            result += CalculateBeginningPartValue(last_verse, last_verse_letter_number, numerology_system_name, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);
        }
        return result;
    }
    private static long CalculateBeginningPartValue(Verse verse, int to_letter_number, string numerology_system_name,
        bool add_to_letter_l_number, bool add_to_letter_w_number, bool add_to_letter_v_number, bool add_to_letter_c_number, bool add_to_letter_l_distance, bool add_to_letter_w_distance, bool add_to_letter_v_distance, bool add_to_letter_c_distance, bool add_to_word_w_number, bool add_to_word_v_number, bool add_to_word_c_number, bool add_to_word_w_distance, bool add_to_word_v_distance, bool add_to_word_c_distance, bool add_to_verse_v_number, bool add_to_verse_c_number, bool add_to_verse_v_distance, bool add_to_verse_c_distance, bool add_to_chapter_c_number)
    {
        return CalculateMiddlePartValue(verse, 1, to_letter_number, numerology_system_name, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);
    }
    private static long CalculateMiddlePartValue(Verse verse, int from_letter_number, int to_letter_number, string numerology_system_name,
        bool add_to_letter_l_number, bool add_to_letter_w_number, bool add_to_letter_v_number, bool add_to_letter_c_number, bool add_to_letter_l_distance, bool add_to_letter_w_distance, bool add_to_letter_v_distance, bool add_to_letter_c_distance, bool add_to_word_w_number, bool add_to_word_v_number, bool add_to_word_c_number, bool add_to_word_w_distance, bool add_to_word_v_distance, bool add_to_word_c_distance, bool add_to_verse_v_number, bool add_to_verse_c_number, bool add_to_verse_v_distance, bool add_to_verse_c_distance, bool add_to_chapter_c_number)
    {
        if (verse == null) return 0;

        long result = 0;
        if (s_numerology_systems != null)
        {
            if (s_numerology_systems.ContainsKey(numerology_system_name))
            {
                string[] parts = numerology_system_name.Split('_');
                string text_mode = parts[0];

                Dictionary<char, long> numerology_system = s_numerology_systems[numerology_system_name];
                if ((numerology_system != null) && (numerology_system.Count > 0))
                {
                    if ((from_letter_number == 1) && (to_letter_number == verse.LetterCount))
                    {
                        // adjust value of verse
                        result += AdjustVerseValue(verse, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance);
                    }

                    int spaces = 0;
                    int word_index = 0;
                    int letter_number_in_word = 0;
                    for (int i = 0; i < verse.GetText(text_mode).Length; i++)
                    {
                        if (verse.GetText(text_mode)[i] == ' ')
                        {
                            spaces++;

                            // adjust value of previous word
                            if ((i > (from_letter_number + spaces - 1)) && (i <= (to_letter_number + spaces - 1)))
                            {
                                if ((word_index >= 0) && (word_index < verse.Words.Count))
                                {
                                    result += AdjustWordValue(verse.Words[word_index], add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance);
                                }
                            }

                            word_index++;
                            letter_number_in_word = 0;
                        }
                        else // not space
                        {
                            letter_number_in_word++;

                            if (i < (from_letter_number + spaces - 1)) continue;
                            if (i > (to_letter_number + spaces - 1)) break;

                            // adjust value of final word
                            if (
                                ((i == (to_letter_number + spaces - 1)) && ((to_letter_number + spaces - 1) == verse.GetText(text_mode).Length - 1))
                                ||
                                (((i > 0) && (i < verse.GetText(text_mode).Length - 1)) && (verse.GetText(text_mode)[i - 1] == ' ') && (verse.GetText(text_mode)[i + 1] == ' '))
                               )
                            {
                                if ((word_index >= 0) && (word_index < verse.Words.Count))
                                {
                                    result += AdjustWordValue(verse.Words[word_index], add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance);
                                }
                            }

                            if (numerology_system.ContainsKey(verse.GetText(text_mode)[i]))
                            {
                                result += numerology_system[verse.GetText(text_mode)[i]];

                                // adjust value of letter
                                int letter_index = letter_number_in_word - 1;
                                if ((word_index >= 0) && (word_index < verse.Words.Count))
                                {
                                    result += AdjustLetterValue(verse.Words[word_index], letter_index, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance);
                                }
                            }
                        }
                    }
                }
            }
        }
        return result;
    }
    private static long CalculateEndPartValue(Verse verse, int from_letter_number, string numerology_system_name,
        bool add_to_letter_l_number, bool add_to_letter_w_number, bool add_to_letter_v_number, bool add_to_letter_c_number, bool add_to_letter_l_distance, bool add_to_letter_w_distance, bool add_to_letter_v_distance, bool add_to_letter_c_distance, bool add_to_word_w_number, bool add_to_word_v_number, bool add_to_word_c_number, bool add_to_word_w_distance, bool add_to_word_v_distance, bool add_to_word_c_distance, bool add_to_verse_v_number, bool add_to_verse_c_number, bool add_to_verse_v_distance, bool add_to_verse_c_distance, bool add_to_chapter_c_number)
    {
        string[] parts = numerology_system_name.Split('_');
        string text_mode = parts[0];
        return CalculateMiddlePartValue(verse, from_letter_number, verse.LetterCount, numerology_system_name, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);
    }

    private static long AdjustLetterValue(Word word, int letter_index, bool add_to_letter_l_number, bool add_to_letter_w_number, bool add_to_letter_v_number, bool add_to_letter_c_number, bool add_to_letter_l_distance, bool add_to_letter_w_distance, bool add_to_letter_v_distance, bool add_to_letter_c_distance)
    {
        long result = 0;

        if (add_to_letter_l_number)
        {
            if ((letter_index >= 0) && (letter_index < word.Letters.Count))
            {
                result += word.Letters[letter_index].NumberInWord;
            }
        }
        if (add_to_letter_w_number)
        {
            result += word.NumberInVerse;
        }
        if (add_to_letter_v_number)
        {
            result += word.Verse.NumberInChapter;
        }
        if (add_to_letter_c_number)
        {
            result += word.Verse.Chapter.Number;
        }
        if (add_to_letter_l_distance)
        {
            if ((letter_index >= 0) && (letter_index < word.Letters.Count))
            {
                result += word.Letters[letter_index].DistanceToPrevious.dL;
            }
        }
        if (add_to_letter_w_distance)
        {
            if ((letter_index >= 0) && (letter_index < word.Letters.Count))
            {
                result += word.Letters[letter_index].DistanceToPrevious.dW;
            }
        }
        if (add_to_letter_v_distance)
        {
            if ((letter_index >= 0) && (letter_index < word.Letters.Count))
            {
                result += word.Letters[letter_index].DistanceToPrevious.dV;
            }
        }
        if (add_to_letter_c_distance)
        {
            if ((letter_index >= 0) && (letter_index < word.Letters.Count))
            {
                result += word.Letters[letter_index].DistanceToPrevious.dC;
            }
        }

        return result;
    }
    private static long AdjustWordValue(Word word, bool add_to_word_w_number, bool add_to_word_v_number, bool add_to_word_c_number, bool add_to_word_w_distance, bool add_to_word_v_distance, bool add_to_word_c_distance)
    {
        long result = 0;

        if (add_to_word_w_number)
        {
            result += word.NumberInVerse;
        }
        if (add_to_word_v_number)
        {
            result += word.Verse.NumberInChapter;
        }
        if (add_to_word_c_number)
        {
            result += word.Verse.Chapter.Number;
        }
        if (add_to_word_w_distance)
        {
            result += word.DistanceToPrevious.dW;
        }
        if (add_to_word_v_distance)
        {
            result += word.DistanceToPrevious.dV;
        }
        if (add_to_word_c_distance)
        {
            result += word.DistanceToPrevious.dC;
        }

        return result;
    }
    private static long AdjustVerseValue(Verse verse, bool add_to_verse_v_number, bool add_to_verse_c_number, bool add_to_verse_v_distance, bool add_to_verse_c_distance)
    {
        long result = 0;

        if (add_to_verse_v_number)
        {
            result += verse.NumberInChapter;
        }
        if (add_to_verse_c_number)
        {
            result += verse.Chapter.Number;
        }
        if (add_to_verse_v_distance)
        {
            result += verse.DistanceToPrevious.dV;
        }
        if (add_to_verse_c_distance)
        {
            result += verse.DistanceToPrevious.dC;
        }

        return result;
    }
    private static long AdjustChapterValue(Chapter chapter, bool add_to_chapter_c_number)
    {
        long result = 0;

        if (add_to_chapter_c_number)
        {
            result += chapter.Number;
        }

        return result;
    }

    public static List<Verse> GetVerses(int start, int end)
    {
        List<Verse> result = null;
        if (
            (start >= end)
            &&
            (start >= Verse.MIN_NUMBER && start <= Verse.MAX_NUMBER)
            &&
            (end >= Verse.MIN_NUMBER && end <= Verse.MAX_NUMBER)
            )
        {
            try
            {
                Book book = Book.Instance;
                if (book != null)
                {
                    foreach (Verse verse in book.Verses)
                    {
                        if ((verse.Number >= start) && (verse.Number <= end))
                        {
                            result.Add(verse);
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
        }
        return result;
    }

    public static Dictionary<string, int> GetWordsWith(string text_mode, List<Verse> verses, string text, bool anywhere_in_word)
    {
        Dictionary<string, int> result = new Dictionary<string, int>();
        if (verses != null)
        {
            if (!String.IsNullOrEmpty(text))
            {
                text = text.Trim();
                if (!text.Contains(" "))
                {
                    foreach (Verse verse in verses)
                    {
                        string[] word_texts = verse.GetText(text_mode).Split(' ');
                        foreach (string word_text in word_texts)
                        {
                            if (anywhere_in_word)
                            {
                                if (word_text.Contains(text))
                                {
                                    if (!result.ContainsKey(word_text))
                                    {
                                        result.Add(word_text, 1);
                                    }
                                    else
                                    {
                                        result[word_text]++;
                                    }
                                }
                            }
                            else
                            {
                                if (word_text.StartsWith(text))
                                {
                                    if (!result.ContainsKey(word_text))
                                    {
                                        result.Add(word_text, 1);
                                    }
                                    else
                                    {
                                        result[word_text]++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        return result;
    }
    public static Dictionary<string, int> GetCurrentWords(string text_mode, List<Verse> verses, string text, bool anywhere_in_word)
    {
        Dictionary<string, int> result = new Dictionary<string, int>();
        if (!String.IsNullOrEmpty(text))
        {
            text = text.Trim();
            text = text.Replace("  ", " ");
            string[] text_words = text.Split(' ');
            foreach (Verse verse in verses)
            {
                string[] words = verse.GetText(text_mode).Split(' ');
                for (int i = 0; i < words.Length; i++)
                {
                    bool is_text_matched = false;
                    if (text_words.Length == 1) // 1 text_word
                    {
                        if (anywhere_in_word)
                        {
                            if (words[i].Contains(text_words[0])) // start found
                            {
                                is_text_matched = true;
                            }
                        }
                        else
                        {
                            if (words[i].StartsWith(text_words[0])) // start found
                            {
                                is_text_matched = true;
                            }
                        }
                    }
                    else if (text_words.Length > 1)// more than 1 text_word
                    {
                        if (words[i].EndsWith(text_words[0])) // start found
                        {
                            if (words.Length >= (i + text_words.Length))
                            {
                                // match text minus last word
                                bool is_text_matched_minus_last_word = true;
                                for (int j = 1; j < text_words.Length - 1; j++)
                                {
                                    if (words[j + i] != text_words[j])
                                    {
                                        is_text_matched_minus_last_word = false;
                                        break;
                                    }
                                }

                                // is still true, check the last word
                                if (is_text_matched_minus_last_word)
                                {
                                    int last_j = text_words.Length - 1;
                                    if (words[last_j + i].StartsWith(text_words[last_j])) // last text_word
                                    {
                                        is_text_matched = true;
                                    }
                                }
                            }
                        }
                    }

                    if (is_text_matched)
                    {
                        // skip all text but not found good_word in case it followed by good_word too
                        i += text_words.Length - 1;

                        // get last word variation
                        if (i < words.Length)
                        {
                            string good_word = words[i];
                            if (!result.ContainsKey(good_word))
                            {
                                result.Add(good_word, 1);
                            }
                            else
                            {
                                result[good_word]++;
                            }
                        }
                    }
                }
            }
        }
        return result;
    }
    public static Dictionary<string, int> GetNextWords(string text_mode, List<Verse> verses, string text, bool anywhere_in_word)
    {
        Dictionary<string, int> result = new Dictionary<string, int>();
        if (verses != null)
        {
            if (!String.IsNullOrEmpty(text))
            {
                text = text.Trim();
                text = text.Replace("  ", " ");
                string[] text_words = text.Split(' ');
                foreach (Verse verse in verses)
                {
                    string[] words = verse.GetText(text_mode).Split(' ');
                    for (int i = 0; i < words.Length; i++)
                    {
                        bool start_of_text_words_found = false;
                        if (anywhere_in_word)
                        {
                            start_of_text_words_found = words[i].EndsWith(text_words[0]);
                        }
                        else
                        {
                            start_of_text_words_found = words[i].Equals(text_words[0]);
                        }

                        if (start_of_text_words_found)
                        {
                            if (words.Length >= (i + text_words.Length))
                            {
                                // check rest of text_words if matching
                                bool is_text_matched = true;
                                for (int j = 1; j < text_words.Length; j++)
                                {
                                    if (words[j + i] != text_words[j])
                                    {
                                        is_text_matched = false;
                                        break;
                                    }
                                }

                                if (is_text_matched)
                                {
                                    // skip text_words
                                    i += text_words.Length;

                                    // add next word to result (if not added already)
                                    if (i < words.Length)
                                    {
                                        string good_word = words[i];
                                        if (!result.ContainsKey(good_word))
                                        {
                                            result.Add(good_word, 1);
                                        }
                                        else
                                        {
                                            result[good_word]++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        return result;
    }

    public static string NormalizeText(string text)
    {
        string result = "";
        if (!String.IsNullOrEmpty(text))
        {
            StringBuilder str = new StringBuilder(text);
            str.Replace("ء", "");
            str.Replace("ٱ", "ا");
            str.Replace("آ", "ا");
            str.Replace("أ", "ا");
            str.Replace("إ", "ا");
            str.Replace("ا", "ا");
            str.Replace("ى", "ا");
            str.Replace("ؤ", "و");
            str.Replace("ئ", "ي");
            str.Replace("ة", "ه");
            result = str.ToString();
        }
        return result;
    }

    public static List<Verse> GetSourceVerses(string text_mode, FindScope find_scope, Selection current_selection, List<Verse> previous_result)
    {
        List<Verse> result = null;
        Book book = Book.Instance;
        if (book != null)
        {
            if (find_scope == FindScope.Book)
            {
                result = book.Verses;
            }
            else if (find_scope == FindScope.Selection)
            {
                result = current_selection.Verses;
            }
            else if (find_scope == FindScope.SearchResult)
            {
                if (previous_result != null)
                {
                    result = new List<Verse>(previous_result);
                }
            }
            else
            {
                result = null;
            }
        }

        return result;
    }
    public static List<Verse> GetVerses(List<Phrase> phrases)
    {
        List<Verse> result = new List<Verse>();
        foreach (Phrase phrase in phrases)
        {
            if (!result.Contains(phrase.Verse))
            {
                result.Add(phrase.Verse);
            }
        }
        return result;
    }
    public static List<Phrase> BuildPhrases(Verse verse, string text_mode, MatchCollection matches)
    {
        List<Phrase> result = new List<Phrase>();
        foreach (Match match in matches)
        {
            foreach (Capture capture in match.Captures)
            {
                Phrase phrase = new Phrase(verse, text_mode, capture.Value, capture.Index);
                result.Add(phrase);
            }
        }
        return result;
    }

    // find by text
    public static List<Phrase> FindPhrases(string text_mode, FindScope find_scope, Selection current_selection, List<Verse> previous_result, string text, FindByTextLanguageType language_type, string translation, FindByTextLocation text_location, bool case_sensitive, FindByTextWordness wordness, int multiplicity, bool anywhere_in_word)
    {
        List<Phrase> result = null;

        if (language_type == FindByTextLanguageType.Arabic)
        {
            if (text_mode == "all")
            {
                /////////////////////////////////////////////////////
                // ٱ أ إ آ ء ؤ ة ى ئ problematic letter variations //
                /////////////////////////////////////////////////////

                List<Phrase> new_phrases = null;

                // search all other text_modes from most complex to simplest 
                if (
                        text.Contains("ٱ") ||
                        text.Contains("أ") ||
                        text.Contains("إ") ||
                        text.Contains("آ") ||
                        text.Contains("ؤ") ||
                        text.Contains("ئ")
                   )
                {
                    text_mode = "Simplified37";
                    new_phrases = DoFindPhrases(text_mode, find_scope, current_selection, previous_result, text, language_type, translation, text_location, case_sensitive, wordness, multiplicity, anywhere_in_word, false);
                    AddNewPhrasesWithoutDuplication(ref result, new_phrases);
                }
                else if (
                            text.Contains("ة") ||
                            text.Contains("ى")
                        )
                {
                    text_mode = "Simplified31";
                    new_phrases = DoFindPhrases(text_mode, find_scope, current_selection, previous_result, text, language_type, translation, text_location, case_sensitive, wordness, multiplicity, anywhere_in_word, false);
                    AddNewPhrasesWithoutDuplication(ref result, new_phrases);
                }
                else if (text.Contains("ء"))
                {
                    text_mode = "Simplified29";
                    new_phrases = DoFindPhrases(text_mode, find_scope, current_selection, previous_result, text, language_type, translation, text_location, case_sensitive, wordness, multiplicity, anywhere_in_word, false);
                    AddNewPhrasesWithoutDuplication(ref result, new_phrases);

                    text_mode = "Simplified29Shadda";
                    new_phrases = DoFindPhrases(text_mode, find_scope, current_selection, previous_result, text, language_type, translation, text_location, case_sensitive, wordness, multiplicity, anywhere_in_word, false);
                    AddNewPhrasesWithoutDuplication(ref result, new_phrases);
                }
                else
                {
                    text_mode = "Simplified28";
                    new_phrases = DoFindPhrases(text_mode, find_scope, current_selection, previous_result, text, language_type, translation, text_location, case_sensitive, wordness, multiplicity, anywhere_in_word, false);
                    AddNewPhrasesWithoutDuplication(ref result, new_phrases);
                }

                // try emlaaei too
                text_mode = "Original"; // Original most likely will not find a match so automatically tries emalaaei text 
                text = text.Replace("ءا", "آ");
                new_phrases = DoFindPhrases(text_mode, find_scope, current_selection, previous_result, text, language_type, translation, text_location, case_sensitive, wordness, multiplicity, anywhere_in_word, true);
                AddNewPhrasesWithoutDuplication(ref result, new_phrases);
            }
            else
            {
                // don't simplify, try it exactly for mathematical codes
                result = DoFindPhrases(text_mode, find_scope, current_selection, previous_result, text, language_type, translation, text_location, case_sensitive, wordness, multiplicity, anywhere_in_word, false);
            }
        }
        else if (language_type == FindByTextLanguageType.Translation)
        {
            if (text_mode == "all")
            {
                result = new List<Phrase>();
                foreach (string key in Book.Instance.Verses[0].Translations.Keys)
                {
                    List<Phrase> new_phrases = DoFindPhrases(DEFAULT_TEXT_MODE, find_scope, current_selection, previous_result, text, language_type, key, text_location, case_sensitive, wordness, multiplicity, anywhere_in_word, false);
                    AddNewPhrasesWithoutDuplication(ref result, new_phrases);
                }
            }
            else
            {
                result = DoFindPhrases(DEFAULT_TEXT_MODE, find_scope, current_selection, previous_result, text, language_type, translation, text_location, case_sensitive, wordness, multiplicity, anywhere_in_word, false);
            }
        }
        return result;
    }
    private static List<Phrase> DoFindPhrases(string text_mode, FindScope find_scope, Selection current_selection, List<Verse> previous_result, string text, FindByTextLanguageType language_type, string translation, FindByTextLocation text_location, bool case_sensitive, FindByTextWordness wordness, int multiplicity, bool anywhere_in_word, bool try_emlaaei_if_nothing_found)
    {
        List<Phrase> result = new List<Phrase>();
        Book book = Book.Instance;
        if (book != null)
        {
            List<Verse> source = GetSourceVerses(text_mode, find_scope, current_selection, previous_result);
            if (source != null)
            {
                if (language_type == FindByTextLanguageType.Arabic)
                {
                    result = DoFindPhrases(text_mode, source, find_scope, current_selection, previous_result, text, text_location, wordness, multiplicity, anywhere_in_word, try_emlaaei_if_nothing_found);
                }
                else //if (language_type == FindByTextLanguageType.Translation)
                {
                    result = DoFindPhrases(translation, source, find_scope, current_selection, previous_result, text, text_location, case_sensitive, wordness, multiplicity, anywhere_in_word);
                }
            }
        }
        return result;
    }
    private static List<Phrase> DoFindPhrases(string text_mode, List<Verse> source, FindScope find_scope, Selection current_selection, List<Verse> previous_result, string text, FindByTextLocation text_location, FindByTextWordness wordness, int multiplicity, bool anywhere_in_word, bool try_emlaaei_if_nothing_found)
    {
        List<Phrase> result = new List<Phrase>();
        List<Verse> found_verses = new List<Verse>();
        if (source != null)
        {
            if (source.Count > 0)
            {
                if (text.Length > 0)
                {
                    /*
                    =====================================================================
                    Regular Expressions (RegEx)
                    =====================================================================
                    Best Reference: http://www.regular-expressions.info/
                    =====================================================================
                    Matches	Characters 
                    x	character x 
                    \\	backslash character 
                    \0n	character with octal value 0n (0 <= n <= 7) 
                    \0nn	character with octal value 0nn (0 <= n <= 7) 
                    \0mnn	character with octal value 0mnn (0 <= m <= 3, 0 <= n <= 7) 
                    \xhh	character with hexadecimal value 0xhh 
                    \uhhhh	character with hexadecimal value 0xhhhh 
                    \t	tab character ('\u0009') 
                    \n	newline (line feed) character ('\u000A') 
                    \r	carriage-return character ('\u000D') 
                    \f	form-feed character ('\u000C') 
                    \a	alert (bell) character ('\u0007') 
                    \e	escape character ('\u001B') 
                    \cx	control character corresponding to x 
                                  
                    Character Classes 
                    [abc]		    a, b, or c				                    (simple class) 
                    [^abc]		    any character except a, b, or c		        (negation) 
                    [a-zA-Z]	    a through z or A through Z, inclusive	    (range) 
                    [a-d[m-p]]	    a through d, or m through p: [a-dm-p]	    (union) 
                    [a-z&&[def]]	d, e, or f				                    (intersection) 
                    [a-z&&[^bc]]	a through z, except for b and c: [ad-z]	    (subtraction) 
                    [a-z&&[^m-p]]	a through z, and not m through p: [a-lq-z]  (subtraction) 
                                  
                    Predefined 
                    .	any character (inc line terminators) except newline 
                    \d	digit				            [0-9] 
                    \D	non-digit			            [^0-9] 
                    \s	whitespace character		    [ \t\n\x0B\f\r] 
                    \S	non-whitespace character	    [^\s] 
                    \w	word character (alphanumeric)	[a-zA-Z_0-9] 
                    \W	non-word character		        [^\w] 

                    Boundary Matchers 
                    ^	beginning of a line	(in Multiline)
                    $	end of a line  		(in Multiline)
                    \b	word boundary 
                    \B	non-word boundary 
                    \A	beginning of the input 
                    \G	end of the previous match 
                    \Z	end of the input but for the final terminator, if any 
                    \z	end of the input

                    Greedy quantifiers 
                    X?	X, once or not at all 
                    X*	X, zero or more times 
                    X+	X, one or more times 
                    X{n}	X, exactly n times 
                    X{n,}	X, at least n times 
                    X{n,m}	X, at least n but not more than m times 
                                  
                    Reluctant quantifiers 
                    X??	X, once or not at all 
                    X*?	X, zero or more times 
                    X+?	X, one or more times 
                    X{n}?	X, exactly n times 
                    X{n,}?	X, at least n times 
                    X{n,m}?	X, at least n but not more than m times 
                                  
                    Possessive quantifiers 
                    X?+	X, once or not at all 
                    X*+	X, zero or more times 
                    X++	X, one or more times 
                    X{n}+	X, exactly n times 
                    X{n,}+	X, at least n times 
                    X{n,m}+	X, at least n but not more than m times 

                    positive lookahead	(?=text)
                    negative lookahead	(?!text)
                    // eg: not at end of line 	    (?!$)
                    positive lookbehind	(?<=text)
                    negative lookbehind	(?<!text)
                    // eg: not at start of line 	(?<!^)
                    =====================================================================
                    */

                    RegexOptions regex_options = RegexOptions.IgnoreCase;
                    regex_options |= RegexOptions.RightToLeft;

                    try
                    {
                        string pattern_empty_line = @"^$";
                        string pattern_whole_line = "(" + @"^" + text + @"$" + ")";

                        string pattern_any_with_prefix = "(" + @"\S+?" + text + ")";
                        string pattern_any_with_prefix_and_suffix = "(" + @"\S+?" + text + @"\S+?" + ")";
                        string pattern_any_with_suffix = "(" + text + @"\S+?" + ")";

                        string pattern_word_with_prefix = "(" + pattern_any_with_prefix + @"\b" + ")";
                        string pattern_word_with_prefix_and_suffix = "(" + pattern_any_with_prefix_and_suffix + ")";
                        string pattern_word_with_suffix = "(" + @"\b" + pattern_any_with_suffix + ")";
                        string pattern_word_with_any_fixes = "(" + pattern_word_with_prefix + "|" + pattern_word_with_prefix_and_suffix + "|" + pattern_any_with_suffix + ")";

                        // Whole word
                        string pattern_whole_word_at_start = "(" + pattern_whole_line + "|" + @"^" + text + @"\b" + ")";
                        string pattern_whole_word_at_middle = "(" + pattern_whole_line + "|" + @"(?<!^)" + @"\b" + text + @"\b" + @"(?!$)" + ")";
                        string pattern_whole_word_at_end = "(" + pattern_whole_line + "|" + @"\b" + text + @"$" + ")";
                        string pattern_whole_word_anywhere = "(" + pattern_whole_line + "|" + @"\b" + text + @"\b" + ")";

                        // Part of word
                        string pattern_part_word_at_start = "(" + @"^" + pattern_word_with_any_fixes + ")";
                        string pattern_part_word_at_middle = "(" + @"(?<!^)" + pattern_word_with_any_fixes + @"(?!$)" + ")";
                        string pattern_part_word_at_end = "(" + pattern_word_with_any_fixes + @"$" + ")";
                        string pattern_part_word_anywhere = "(" + pattern_part_word_at_start + "|" + pattern_part_word_at_middle + "|" + pattern_part_word_at_end + ")";

                        // Any == Whole word | Part of word
                        string pattern_any_at_start = "(" + pattern_whole_line + "|" + @"^" + text + ")";
                        string pattern_any_at_middle = "(" + pattern_whole_line + "|" + @"(?<!^)" + text + @"(?!$)" + ")";
                        string pattern_any_at_end = "(" + pattern_whole_line + "|" + text + @"$" + ")";
                        string pattern_any_anywhere = text;

                        string pattern = "";
                        List<string> negative_words = new List<string>();
                        List<string> positive_words = new List<string>();
                        List<string> unsigned_words = new List<string>();

                        bool at_word_start = !anywhere_in_word;
                        if (at_word_start)
                        {
                            pattern = @"(?<=\b)(" + pattern + @")"; // positive lookbehind
                        }

                        switch (text_location)
                        {
                            case FindByTextLocation.Anywhere:
                                {
                                    if (wordness == FindByTextWordness.WholeWord)
                                    {
                                        pattern += pattern_whole_word_anywhere;
                                    }
                                    else if (wordness == FindByTextWordness.PartOfWord)
                                    {
                                        pattern += pattern_part_word_anywhere;
                                    }
                                    else if (wordness == FindByTextWordness.Any)
                                    {
                                        pattern += pattern_any_anywhere;
                                    }
                                    else
                                    {
                                        pattern += pattern_empty_line;
                                    }
                                }
                                break;
                            case FindByTextLocation.AtStart:
                                {
                                    if (wordness == FindByTextWordness.WholeWord)
                                    {
                                        pattern += pattern_whole_word_at_start;
                                    }
                                    else if (wordness == FindByTextWordness.PartOfWord)
                                    {
                                        pattern += pattern_part_word_at_start;
                                    }
                                    else if (wordness == FindByTextWordness.Any)
                                    {
                                        pattern += pattern_any_at_start;
                                    }
                                    else
                                    {
                                        pattern += pattern_empty_line;
                                    }
                                }
                                break;
                            case FindByTextLocation.AtMiddle:
                                {
                                    if (wordness == FindByTextWordness.WholeWord)
                                    {
                                        pattern += pattern_whole_word_at_middle;
                                    }
                                    else if (wordness == FindByTextWordness.PartOfWord)
                                    {
                                        pattern += pattern_part_word_at_middle;
                                    }
                                    else if (wordness == FindByTextWordness.Any)
                                    {
                                        pattern += pattern_any_at_middle;
                                    }
                                    else
                                    {
                                        pattern += pattern_empty_line;
                                    }
                                }
                                break;
                            case FindByTextLocation.AtEnd:
                                {
                                    if (wordness == FindByTextWordness.WholeWord)
                                    {
                                        pattern += pattern_whole_word_at_end;
                                    }
                                    else if (wordness == FindByTextWordness.PartOfWord)
                                    {
                                        pattern += pattern_part_word_at_end;
                                    }
                                    else if (wordness == FindByTextWordness.Any)
                                    {
                                        pattern += pattern_any_at_end;
                                    }
                                    else
                                    {
                                        pattern += pattern_empty_line;
                                    }
                                }
                                break;
                            case FindByTextLocation.AllWords:
                            case FindByTextLocation.AnyWord:
                                {
                                    pattern = Regex.Replace(text.Trim(), @"\s+", " "); // remove double space or higher if any

                                    string[] pattern_words = pattern.Split(' ');
                                    foreach (string pattern_word in pattern_words)
                                    {
                                        if (pattern_word.StartsWith("-"))
                                        {
                                            negative_words.Add(pattern_word.Substring(1));
                                        }
                                        else if (pattern_word.EndsWith("-"))
                                        {
                                            negative_words.Add(pattern_word.Substring(0, pattern_word.Length - 1));
                                        }
                                        else if (pattern_word.StartsWith("+"))
                                        {
                                            positive_words.Add(pattern_word.Substring(1));
                                        }
                                        else if (pattern_word.EndsWith("+"))
                                        {
                                            positive_words.Add(pattern_word.Substring(0, pattern_word.Length - 1));
                                        }
                                        else
                                        {
                                            unsigned_words.Add(pattern_word);
                                        }
                                    }
                                }
                                break;
                            default:
                                {
                                    return new List<Phrase>();
                                }
                        }

                        // do actual search
                        foreach (Verse verse in source)
                        {
                            /////////////////////////
                            // process negative_words
                            /////////////////////////
                            if (negative_words.Count > 0)
                            {
                                bool skip = false;
                                foreach (string negative_word in negative_words)
                                {
                                    foreach (Word word in verse.Words)
                                    {
                                        // if any negative_word found, skip verse
                                        if (word.Texts[text_mode].Contains(negative_word))
                                        {
                                            skip = true; // next verse
                                            break;
                                        }
                                    }
                                    if (skip)
                                    {
                                        break;
                                    }
                                }
                                if (skip) continue; // next verse
                            }

                            /////////////////////////
                            // process positive_words
                            /////////////////////////
                            if (positive_words.Count > 0)
                            {
                                int matches = 0;
                                foreach (string positive_word in positive_words)
                                {
                                    foreach (Word word in verse.Words)
                                    {
                                        if (word.Texts[text_mode].Contains(positive_word))
                                        {
                                            matches++;
                                            break; // next positive_word
                                        }
                                    }
                                }

                                // verse failed test, so skip it
                                if (matches < positive_words.Count)
                                {
                                    continue; // next verse
                                }
                            }

                            //////////////////////////////////////////////////////
                            // both negative and positive conditions have been met
                            //////////////////////////////////////////////////////

                            /////////////////////////
                            // process unsigned_words
                            /////////////////////////
                            //////////////////////////////////////////////////////////
                            // FindByText WORDS All
                            //////////////////////////////////////////////////////////
                            if (text_location == FindByTextLocation.AllWords)
                            {
                                int matches = 0;
                                foreach (string unsigned_word in unsigned_words)
                                {
                                    foreach (Word word in verse.Words)
                                    {
                                        if (word.Texts[text_mode].Contains(unsigned_word))
                                        {
                                            matches++;
                                            break; // no need to continue even if there are more matches
                                        }
                                    }
                                }

                                if (matches == unsigned_words.Count)
                                {
                                    ///////////////////////////////////////////////////////////////
                                    // all negative, positive and unsigned conditions have been met
                                    ///////////////////////////////////////////////////////////////

                                    // add positive matches
                                    foreach (string positive_word in positive_words)
                                    {
                                        foreach (Word word in verse.Words)
                                        {
                                            if (word.Texts[text_mode].Contains(positive_word))
                                            {
                                                found_verses.Add(verse);
                                                result.Add(new Phrase(verse, text_mode, word.Texts[text_mode], word.Positions[text_mode]));
                                                //break; // no break in case there are more matches
                                            }
                                        }
                                    }

                                    // add unsigned matches
                                    foreach (string unsigned_word in unsigned_words)
                                    {
                                        foreach (Word word in verse.Words)
                                        {
                                            if (word.Texts[text_mode].Contains(unsigned_word))
                                            {
                                                found_verses.Add(verse);
                                                result.Add(new Phrase(verse, text_mode, word.Texts[text_mode], word.Positions[text_mode]));
                                                //break; // no break in case there are more matches
                                            }
                                        }
                                    }
                                }
                                else // verse failed test, so skip it
                                {
                                    continue; // next verse
                                }
                            }
                            //////////////////////////////////////////////////////////
                            // FindByText WORDS Any
                            //////////////////////////////////////////////////////////
                            else if (text_location == FindByTextLocation.AnyWord)
                            {
                                bool found = false;
                                foreach (string unsigned_word in unsigned_words)
                                {
                                    foreach (Word word in verse.Words)
                                    {
                                        if (word.Texts[text_mode].Contains(unsigned_word))
                                        {
                                            found = true;
                                            break; // next unsigned_word
                                        }
                                    }
                                    if (found)
                                    {
                                        break;
                                    }
                                }

                                if (found) // found 1 unsigned word in verse, which is enough
                                {
                                    ///////////////////////////////////////////////////////////////
                                    // all negative, positive and unsigned conditions have been met
                                    ///////////////////////////////////////////////////////////////

                                    // add positive matches
                                    foreach (string positive_word in positive_words)
                                    {
                                        foreach (Word word in verse.Words)
                                        {
                                            if (word.Texts[text_mode].Contains(positive_word))
                                            {
                                                found_verses.Add(verse);
                                                result.Add(new Phrase(verse, text_mode, word.Texts[text_mode], word.Positions[text_mode]));
                                                //break; // no break in case there are more matches
                                            }
                                        }
                                    }

                                    // add unsigned matches
                                    foreach (string unsigned_word in unsigned_words)
                                    {
                                        foreach (Word word in verse.Words)
                                        {
                                            if (word.Texts[text_mode].Contains(unsigned_word))
                                            {
                                                found_verses.Add(verse);
                                                result.Add(new Phrase(verse, text_mode, word.Texts[text_mode], word.Positions[text_mode]));
                                                //break; // no break in case there are more matches
                                            }
                                        }
                                    }
                                }
                                else // verse failed test, so skip it
                                {
                                    continue; // next verse
                                }
                            }
                            //////////////////////////////////////////////////////////
                            // FindByText EXACT 
                            //////////////////////////////////////////////////////////
                            else // at start, middle, end, or anywhere
                            {
                                MatchCollection matches = Regex.Matches(verse.GetText(text_mode), pattern, regex_options);
                                if (multiplicity != -1) // with multiplicity
                                {
                                    if (matches.Count == multiplicity)
                                    {
                                        found_verses.Add(verse);
                                        if (matches.Count > 0)
                                        {
                                            result.AddRange(BuildPhrases(verse, text_mode, matches));
                                        }
                                        else
                                        {
                                            result.Add(new Phrase(verse, text_mode, "", 0));
                                        }
                                    }
                                }
                                else // without multiplicity
                                {
                                    if (matches.Count > 0)
                                    {
                                        found_verses.Add(verse);
                                        result.AddRange(BuildPhrases(verse, text_mode, matches));
                                    }
                                }
                            }
                        } // end for

                        // if nothing found in original/simplified, try emlaaei
                        if ((multiplicity != 0) && (result.Count == 0))
                        {
                            if (try_emlaaei_if_nothing_found)
                            {
                                // used simplified emlaaei and simplified search pattern
                                pattern = SimplifyText(pattern, "Simplified37");
                                if ((source != null) && (source.Count > 0))
                                {
                                    Book book = Book.Instance;
                                    if (book != null)
                                    {
                                        foreach (Verse verse in source)
                                        {
                                            if (text_location == FindByTextLocation.AllWords)
                                            {
                                                bool skip = false;
                                                foreach (string pattern_word in negative_words)
                                                {
                                                    if (verse.Translations["ar.emlaaei"].Contains(pattern_word))
                                                    {
                                                        skip = true;
                                                        break;
                                                    }
                                                }
                                                if (skip) continue;

                                                foreach (string pattern_word in positive_words)
                                                {
                                                    if (!verse.Translations["ar.emlaaei"].Contains(pattern_word))
                                                    {
                                                        skip = true;
                                                        break;
                                                    }
                                                }
                                                if (skip) continue;

                                                if (
                                                     (unsigned_words.Count == 0) ||
                                                     (verse.Translations["ar.emlaaei"].ContainsAllWordsOf(unsigned_words))
                                                   )
                                                {
                                                    result.Add(new Phrase(book.Verses[verse.Number - 1], "ar.emlaaei", "", 0));
                                                }
                                            }
                                            else if (text_location == FindByTextLocation.AnyWord)
                                            {
                                                bool skip = false;
                                                foreach (string pattern_word in negative_words)
                                                {
                                                    if (verse.Translations["ar.emlaaei"].Contains(pattern_word))
                                                    {
                                                        skip = true;
                                                        break;
                                                    }
                                                }
                                                if (skip) continue;

                                                foreach (string pattern_word in positive_words)
                                                {
                                                    if (!verse.Translations["ar.emlaaei"].Contains(pattern_word))
                                                    {
                                                        skip = true;
                                                        break;
                                                    }
                                                }
                                                if (skip) continue;

                                                if (
                                                     (negative_words.Count > 0) ||
                                                     (positive_words.Count > 0) ||
                                                     (
                                                       (unsigned_words.Count == 0) ||
                                                       (verse.Translations["ar.emlaaei"].ContainsWordOf(unsigned_words))
                                                     )
                                                   )
                                                {
                                                    result.Add(new Phrase(book.Verses[verse.Number - 1], "ar.emlaaei", "", 0));
                                                }
                                            }
                                            else // at start, middle, end, or anywhere
                                            {
                                                MatchCollection matches = Regex.Matches(verse.Translations["ar.emlaaei"], pattern, regex_options);
                                                if (multiplicity != -1) // with multiplicity
                                                {
                                                    if (matches.Count == multiplicity)
                                                    {
                                                        result.AddRange(BuildPhrases(verse, "ar.emlaaei", matches));
                                                    }
                                                }
                                                else // without multiplicity
                                                {
                                                    if (matches.Count > 0)
                                                    {
                                                        result.AddRange(BuildPhrases(verse, "ar.emlaaei", matches));
                                                    }
                                                }
                                            }
                                        } // end for
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                        // log exception
                    }
                }
            }
        }
        return result;
    }
    private static List<Phrase> DoFindPhrases(string translation, List<Verse> source, FindScope find_scope, Selection current_selection, List<Verse> previous_result, string text, FindByTextLocation text_location, bool case_sensitive, FindByTextWordness wordness, int multiplicity, bool anywhere_in_word)
    {
        List<Phrase> result = new List<Phrase>();
        List<Verse> found_verses = new List<Verse>();
        if (source != null)
        {
            if (source.Count > 0)
            {
                if (text.Length > 0)
                {
                    RegexOptions regex_options = case_sensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
                    if (text.IsArabic()) // Arabic letters in translation (Emlaaei, Urdu, Farsi, etc.) 
                    {
                        regex_options |= RegexOptions.RightToLeft;
                    }

                    try
                    {
                        string pattern_empty_line = @"^$";
                        string pattern_whole_line = "(" + @"^" + text + @"$" + ")";

                        string pattern_any_with_prefix = "(" + @"\S+?" + text + ")";
                        string pattern_any_with_prefix_and_suffix = "(" + @"\S+?" + text + @"\S+?" + ")";
                        string pattern_any_with_suffix = "(" + text + @"\S+?" + ")";

                        string pattern_word_with_prefix = "(" + pattern_any_with_prefix + @"\b" + ")";
                        string pattern_word_with_prefix_and_suffix = "(" + pattern_any_with_prefix_and_suffix + ")";
                        string pattern_word_with_suffix = "(" + @"\b" + pattern_any_with_suffix + ")";
                        string pattern_word_with_any_fixes = "(" + pattern_word_with_prefix + "|" + pattern_word_with_prefix_and_suffix + "|" + pattern_any_with_suffix + ")";

                        // Whole word
                        string pattern_whole_word_at_start = "(" + pattern_whole_line + "|" + @"^" + text + @"\b" + ")";
                        string pattern_whole_word_at_middle = "(" + pattern_whole_line + "|" + @"(?<!^)" + @"\b" + text + @"\b" + @"(?!$)" + ")";
                        string pattern_whole_word_at_end = "(" + pattern_whole_line + "|" + @"\b" + text + @"$" + ")";
                        string pattern_whole_word_anywhere = "(" + pattern_whole_line + "|" + @"\b" + text + @"\b" + ")";

                        // Part of word
                        string pattern_part_word_at_start = "(" + @"^" + pattern_word_with_any_fixes + ")";
                        string pattern_part_word_at_middle = "(" + @"(?<!^)" + pattern_word_with_any_fixes + @"(?!$)" + ")";
                        string pattern_part_word_at_end = "(" + pattern_word_with_any_fixes + @"$" + ")";
                        string pattern_part_word_anywhere = "(" + pattern_part_word_at_start + "|" + pattern_part_word_at_middle + "|" + pattern_part_word_at_end + ")";

                        // Any == Whole word | Part of word
                        string pattern_any_at_start = "(" + pattern_whole_line + "|" + @"^" + text + ")";
                        string pattern_any_at_middle = "(" + pattern_whole_line + "|" + @"(?<!^)" + text + @"(?!$)" + ")";
                        string pattern_any_at_end = "(" + pattern_whole_line + "|" + text + @"$" + ")";
                        string pattern_any_anywhere = text;

                        string pattern = "";
                        List<string> negative_words = new List<string>();
                        List<string> positive_words = new List<string>();
                        List<string> unsigned_words = new List<string>();

                        bool at_word_start = !anywhere_in_word;
                        if (at_word_start)
                        {
                            pattern = @"(?<=\b)(" + pattern + @")"; // positive lookbehind
                        }

                        switch (text_location)
                        {
                            case FindByTextLocation.Anywhere:
                                {
                                    if (wordness == FindByTextWordness.WholeWord)
                                    {
                                        pattern += pattern_whole_word_anywhere;
                                    }
                                    else if (wordness == FindByTextWordness.PartOfWord)
                                    {
                                        pattern += pattern_part_word_anywhere;
                                    }
                                    else if (wordness == FindByTextWordness.Any)
                                    {
                                        pattern += pattern_any_anywhere;
                                    }
                                    else
                                    {
                                        pattern += pattern_empty_line;
                                    }
                                }
                                break;
                            case FindByTextLocation.AtStart:
                                {
                                    if (wordness == FindByTextWordness.WholeWord)
                                    {
                                        pattern += pattern_whole_word_at_start;
                                    }
                                    else if (wordness == FindByTextWordness.PartOfWord)
                                    {
                                        pattern += pattern_part_word_at_start;
                                    }
                                    else if (wordness == FindByTextWordness.Any)
                                    {
                                        pattern += pattern_any_at_start;
                                    }
                                    else
                                    {
                                        pattern += pattern_empty_line;
                                    }
                                }
                                break;
                            case FindByTextLocation.AtMiddle:
                                {
                                    if (wordness == FindByTextWordness.WholeWord)
                                    {
                                        pattern += pattern_whole_word_at_middle;
                                    }
                                    else if (wordness == FindByTextWordness.PartOfWord)
                                    {
                                        pattern += pattern_part_word_at_middle;
                                    }
                                    else if (wordness == FindByTextWordness.Any)
                                    {
                                        pattern += pattern_any_at_middle;
                                    }
                                    else
                                    {
                                        pattern += pattern_empty_line;
                                    }
                                }
                                break;
                            case FindByTextLocation.AtEnd:
                                {
                                    if (wordness == FindByTextWordness.WholeWord)
                                    {
                                        pattern += pattern_whole_word_at_end;
                                    }
                                    else if (wordness == FindByTextWordness.PartOfWord)
                                    {
                                        pattern += pattern_part_word_at_end;
                                    }
                                    else if (wordness == FindByTextWordness.Any)
                                    {
                                        pattern += pattern_any_at_end;
                                    }
                                    else
                                    {
                                        pattern += pattern_empty_line;
                                    }
                                }
                                break;
                            case FindByTextLocation.AllWords:
                            case FindByTextLocation.AnyWord:
                                {
                                    pattern = Regex.Replace(text.Trim(), @"\s+", " "); // remove double space or higher if any

                                    string[] pattern_words = pattern.Split(' ');
                                    foreach (string pattern_word in pattern_words)
                                    {
                                        if (pattern_word.StartsWith("-"))
                                        {
                                            negative_words.Add(pattern_word.Substring(1));
                                        }
                                        else if (pattern_word.EndsWith("-"))
                                        {
                                            negative_words.Add(pattern_word.Substring(0, pattern_word.Length - 1));
                                        }
                                        else if (pattern_word.StartsWith("+"))
                                        {
                                            positive_words.Add(pattern_word.Substring(1));
                                        }
                                        else if (pattern_word.EndsWith("+"))
                                        {
                                            positive_words.Add(pattern_word.Substring(0, pattern_word.Length - 1));
                                        }
                                        else
                                        {
                                            unsigned_words.Add(pattern_word);
                                        }
                                    }
                                }
                                break;
                            default:
                                {
                                    return new List<Phrase>();
                                }
                        }

                        // do actual search
                        foreach (Verse verse in source)
                        {
                            if (text_location == FindByTextLocation.AllWords)
                            {
                                bool skip = false;
                                foreach (string negative_word in negative_words)
                                {
                                    if (verse.Translations[translation].Contains(negative_word))
                                    {
                                        skip = true;
                                        break;
                                    }
                                }
                                if (skip) continue;

                                foreach (string positive_word in positive_words)
                                {
                                    if (!verse.Translations[translation].Contains(positive_word))
                                    {
                                        skip = true;
                                        break;
                                    }
                                }
                                if (skip) continue;

                                if (
                                     (unsigned_words.Count == 0) ||
                                     (verse.Translations[translation].ContainsAllWordsOf(unsigned_words))
                                   )
                                {
                                    found_verses.Add(verse);
                                    result.Add(new Phrase(verse, translation, "", 0));
                                }
                            }
                            else if (text_location == FindByTextLocation.AnyWord)
                            {
                                bool skip = false;
                                foreach (string negative_word in negative_words)
                                {
                                    if (verse.Translations[translation].Contains(negative_word))
                                    {
                                        skip = true;
                                        break;
                                    }
                                }
                                if (skip) continue;

                                foreach (string positive_word in positive_words)
                                {
                                    if (!verse.Translations[translation].Contains(positive_word))
                                    {
                                        skip = true;
                                        break;
                                    }
                                }
                                if (skip) continue;

                                if (
                                     (negative_words.Count > 0) ||
                                     (positive_words.Count > 0) ||
                                     (
                                       (unsigned_words.Count == 0) ||
                                       (verse.Translations[translation].ContainsWordOf(unsigned_words))
                                     )
                                   )
                                {
                                    found_verses.Add(verse);
                                    result.Add(new Phrase(verse, translation, "", 0));
                                }
                            }
                            else // at start, middle, end, or anywhere
                            {
                                MatchCollection matches = Regex.Matches(verse.Translations[translation], pattern, regex_options);
                                if (multiplicity != -1) // with multiplicity
                                {
                                    if (matches.Count == multiplicity)
                                    {
                                        found_verses.Add(verse);
                                        if (matches.Count > 0)
                                        {
                                            result.AddRange(BuildPhrases(verse, translation, matches));
                                        }
                                        else
                                        {
                                            result.Add(new Phrase(verse, translation, "", 0));
                                        }
                                    }
                                }
                                else // without multiplicity
                                {
                                    if (matches.Count > 0)
                                    {
                                        found_verses.Add(verse);
                                        result.AddRange(BuildPhrases(verse, translation, matches));
                                    }
                                }
                            }
                        } // end for
                    }
                    catch
                    {
                        // log exception
                    }
                }
            }
        }
        return result;
    }
    private static void AddNewPhrasesWithoutDuplication(ref List<Phrase> old_phrases, List<Phrase> new_phrases)
    {
        if (old_phrases == null) old_phrases = new List<Phrase>();
        if (new_phrases == null) return;

        List<Phrase> temp = new List<Phrase>();
        foreach (Phrase new_phrase in new_phrases)
        {
            bool already_exist = false;
            foreach (Phrase old_phrase in old_phrases)
            {
                if (old_phrase.Verse.Number == new_phrase.Verse.Number)
                {
                    already_exist = true; // don't add
                    break;
                }
            }

            if (!already_exist)
            {
                temp.Add(new_phrase);
            }
        }

        old_phrases.AddRange(temp);
    }

    // find by root
    public static string GetRoot(string text_mode, string text)
    {
        string result = "";
        Book book = Book.Instance;
        if (book != null)
        {
            Dictionary<string, List<Word>> root_words_dictionary = DataAccess.QuranRootWords;
            if (text.Length > 0)
            {
                try
                {
                    if (root_words_dictionary != null)
                    {
                        string new_root = "";
                        bool found = false;
                        foreach (string key in root_words_dictionary.Keys)
                        {
                            List<Word> root_words = root_words_dictionary[key];
                            foreach (Word root_word in root_words)
                            {
                                Verse verse = book.Verses[root_word.Verse.Number - 1];

                                if (verse.Words.Count > root_word.NumberInVerse - 1)
                                {
                                    Word verse_word = verse.Words[root_word.NumberInVerse - 1];
                                    if (verse_word.Texts[text_mode] == text)
                                    {
                                        if ((key.Length == 3) || (key.Length == 4))  // stop at proper root even if shorter than what is currently stored in candidate_root
                                        {
                                            new_root = key;
                                            found = true;
                                        }
                                        else if (key.Length >= new_root.Length)
                                        {
                                            new_root = key;
                                            found = false;
                                        }
                                        break;
                                    }
                                }
                            }
                            if (found)
                            {
                                break;
                            }
                        }

                        if (!found)
                        {
                            foreach (string key in root_words_dictionary.Keys)
                            {
                                List<Word> root_words = root_words_dictionary[key];
                                foreach (Word root_word in root_words)
                                {
                                    Verse verse = book.Verses[root_word.Verse.Number - 1];

                                    if (verse.Words.Count > root_word.NumberInVerse - 1)
                                    {
                                        Word verse_word = verse.Words[root_word.NumberInVerse - 1];
                                        if (verse_word.Texts[text_mode].Contains(text))
                                        {
                                            if ((key.Length == 3) || (key.Length == 4))  // stop at proper root even if shorter than what is currently stored in candidate_root
                                            {
                                                new_root = key;
                                                found = true;
                                            }
                                            else if (key.Length >= new_root.Length)
                                            {
                                                new_root = key;
                                                found = false;
                                            }
                                            break;
                                        }
                                    }
                                }
                                if (found)
                                {
                                    break;
                                }
                            }
                        }

                        if (!found)
                        {
                            foreach (string key in root_words_dictionary.Keys)
                            {
                                List<Word> root_words = root_words_dictionary[key];
                                foreach (Word root_word in root_words)
                                {
                                    Verse verse = book.Verses[root_word.Verse.Number - 1];

                                    if (verse.Words.Count > root_word.NumberInVerse - 1)
                                    {
                                        Word verse_word = verse.Words[root_word.NumberInVerse - 1];
                                        if (verse_word.Texts[text_mode].IsSimilarTo(text, 0.80D))
                                        {
                                            if ((key.Length == 3) || (key.Length == 4))  // stop at proper root even if shorter than what is currently stored in candidate_root
                                            {
                                                new_root = key;
                                                found = true;
                                            }
                                            else if (key.Length >= new_root.Length)
                                            {
                                                new_root = key;
                                                found = false;
                                            }
                                            break;
                                        }
                                    }
                                }
                                if (found)
                                {
                                    break;
                                }
                            }
                        }

                        result = new_root;
                    }
                }
                catch
                {
                    // log exception
                }
            }
        }
        return result;
    }
    public static List<string> GetRoots(string text_mode, string text)
    {
        List<string> result = new List<string>();
        Book book = Book.Instance;
        if (book != null)
        {
            Dictionary<string, List<Word>> root_words_dictionary = DataAccess.QuranRootWords;
            if (text.Length > 0)
            {
                try
                {
                    if (root_words_dictionary != null)
                    {
                        foreach (string key in root_words_dictionary.Keys)
                        {
                            List<Word> root_words = root_words_dictionary[key];
                            foreach (Word root_word in root_words)
                            {
                                Verse verse = book.Verses[root_word.Verse.Number - 1];
                                if (verse.Words.Count > root_word.NumberInVerse - 1)
                                {
                                    Word verse_word = verse.Words[root_word.NumberInVerse - 1];
                                    if (verse_word.Texts[text_mode] == text)
                                    {
                                        //if (key.Length == 1) continue; // skip 1-letter roots, BIHEE
                                        //if (key.Length == 2) continue; // skip 2-letter roots , LA, MA
                                        result.Add(key);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // log exception
                }
            }
        }
        return result;
    }
    public static List<string> GetRootsWith(string text_mode, string text, bool anywhere_in_word)
    {
        List<string> result = new List<string>();
        foreach (string key in DataAccess.QuranRootWords.Keys)
        {
            if (anywhere_in_word)
            {
                if (key.Contains(text))
                {
                    result.Add(key);
                }
            }
            else
            {
                if (key.StartsWith(text))
                {
                    result.Add(key);
                }
            }
        }
        return result;
    }
    public static List<string> GetRelatedWords(string text_mode, string root)
    {
        List<string> result = new List<string>();
        Book book = Book.Instance;
        if (book != null)
        {
            Dictionary<string, List<Word>> root_words_dictionary = DataAccess.QuranRootWords;
            if (root.Length > 0)
            {
                try
                {
                    if (root_words_dictionary != null)
                    {
                        if (root_words_dictionary.ContainsKey(root))
                        {
                            // get all pre-identified root_word's verses at key
                            List<Word> words = root_words_dictionary[root];
                            foreach (Word word in words)
                            {
                                Verse verse = book.Verses[word.Verse.Number - 1];
                                Word verse_word = verse.Words[word.NumberInVerse - 1];
                                if (!result.Contains(verse_word.Texts[text_mode]))
                                {
                                    result.Add(verse_word.Texts[text_mode]);
                                }
                            }
                        }
                        else // if no such root, search for the matching root_word by its verse position and get its root and then get all root_words
                        {
                            string new_root = GetRoot(text_mode, root);
                            if (!String.IsNullOrEmpty(new_root))
                            {
                                // get all pre-identified root_word's verses at key
                                List<Word> root_words = root_words_dictionary[new_root];
                                foreach (Word root_word in root_words)
                                {
                                    Verse verse = book.Verses[root_word.Verse.Number - 1];
                                    Word verse_word = verse.Words[root_word.NumberInVerse - 1];
                                    if (!result.Contains(verse_word.Texts[text_mode]))
                                    {
                                        result.Add(verse_word.Texts[text_mode]);
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // log exception
                }
            }
        }
        return result;
    }
    public static List<string> GetRelatedWordsWithNeighbours(string text_mode, string root)
    {
        List<string> result = new List<string>();
        Book book = Book.Instance;
        if (book != null)
        {
            Dictionary<string, List<Word>> root_words_dictionary = DataAccess.QuranRootWords;
            if (root.Length > 0)
            {
                try
                {
                    if (root_words_dictionary != null)
                    {
                        if (root_words_dictionary.ContainsKey(root))
                        {
                            // get all pre-identified root_word's verses at key
                            List<Word> words = root_words_dictionary[root];
                            foreach (Word word in words)
                            {
                                Verse verse = book.Verses[word.Verse.Number - 1];
                                string text = "";
                                if (word.NumberInVerse - 2 >= 0)
                                {
                                    text += verse.Words[word.NumberInVerse - 2].Texts[text_mode];
                                }
                                text += " " + verse.Words[word.NumberInVerse - 1].Texts[text_mode];
                                if (verse.Words.Count > word.NumberInVerse)
                                {
                                    text += " " + verse.Words[word.NumberInVerse].Texts[text_mode];
                                }
                                if (!result.Contains(text))
                                {
                                    result.Add(text);
                                }
                            }
                        }
                        else // if no such root, search for the matching root_word by its verse position and get its root and then get all root_words
                        {
                            string new_root = GetRoot(text_mode, root);
                            if (!String.IsNullOrEmpty(new_root))
                            {
                                // get all pre-identified root_word's verses at key
                                List<Word> root_words = root_words_dictionary[new_root];
                                foreach (Word root_word in root_words)
                                {
                                    Verse verse = book.Verses[root_word.Verse.Number - 1];
                                    string text = "";
                                    if (root_word.NumberInVerse - 2 >= 0)
                                    {
                                        text += verse.Words[root_word.NumberInVerse - 2].Texts[text_mode];
                                    }
                                    text += " " + verse.Words[root_word.NumberInVerse - 1].Texts[text_mode];
                                    if (verse.Words.Count > root_word.NumberInVerse)
                                    {
                                        text += " " + verse.Words[root_word.NumberInVerse].Texts[text_mode];
                                    }
                                    if (!result.Contains(text))
                                    {
                                        result.Add(text);
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // log exception
                }
            }
        }
        return result;
    }
    public static List<Verse> GetRelatedVerses(string text_mode, string root)
    {
        List<Verse> result = new List<Verse>();
        Book book = Book.Instance;
        if (book != null)
        {
            Dictionary<string, List<Word>> root_words_dictionary = DataAccess.QuranRootWords;
            if (root.Length > 0)
            {
                try
                {
                    if (root_words_dictionary != null)
                    {
                        if (root_words_dictionary.ContainsKey(root))
                        {
                            // get all pre-identified root_word's verses at key
                            List<Word> words = root_words_dictionary[root];
                            foreach (Word word in words)
                            {
                                Verse verse = book.Verses[word.Verse.Number - 1];
                                if (!result.Contains(verse))
                                {
                                    result.Add(verse);
                                }
                            }
                        }
                        else // if no such root, search for the matching root_word by its verse position and get its root and then get all root_words
                        {
                            string new_root = GetRoot(text_mode, root);
                            if (!String.IsNullOrEmpty(new_root))
                            {
                                // get all pre-identified root_word's verses at key
                                List<Word> root_words = root_words_dictionary[new_root];
                                foreach (Word root_word in root_words)
                                {
                                    Verse verse = book.Verses[root_word.Verse.Number - 1];
                                    if (!result.Contains(verse))
                                    {
                                        result.Add(verse);
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // log exception
                }
            }
        }
        return result;
    }
    public static List<Phrase> FindPhrases(string text_mode, FindScope find_scope, Selection current_selection, List<Verse> previous_result, string roots, int multiplicity)
    {
        List<Phrase> previous_phrases = null;
        List<Verse> previous_verses = null;

        string[] parts = roots.Split(' ');
        if (parts.Length == 0)
        {
            return null;
        }
        else if (parts.Length == 1)
        {
            return DoFindPhrases(text_mode, find_scope, current_selection, previous_result, roots, multiplicity);
        }
        else if (parts.Length > 1) // enable nested searches
        {
            if (roots.Length > 1) // enable nested searches
            {
                List<Phrase> current_phrases = null;
                List<Verse> current_verses = null;

                foreach (string part in parts)
                {
                    if (part.StartsWith("-"))
                    {
                        current_phrases = DoFindPhrases(text_mode, find_scope, current_selection, previous_verses, part.Remove(0, 1), 0); // multiplicity = 0 for exclude
                    }
                    else if (part.StartsWith("+"))
                    {
                        current_phrases = DoFindPhrases(text_mode, find_scope, current_selection, previous_verses, part.Remove(0, 1), multiplicity);
                    }
                    else
                    {
                        current_phrases = DoFindPhrases(text_mode, find_scope, current_selection, previous_verses, part, multiplicity);
                    }
                    current_verses = new List<Verse>(GetVerses(current_phrases));

                    // if first result
                    if (previous_verses == null)
                    {
                        // fill it up with a copy of the first root search result
                        previous_phrases = new List<Phrase>(current_phrases);
                        previous_verses = new List<Verse>(current_verses);

                        // prepare for nested search by search
                        find_scope = FindScope.SearchResult;
                    }
                    else // subsequent search result
                    {
                        previous_verses = new List<Verse>(current_verses);

                        List<Phrase> union_phrases = new List<Phrase>(current_phrases);
                        foreach (Phrase phrase in previous_phrases)
                        {
                            if (current_verses.Contains(phrase.Verse))
                            {
                                union_phrases.Add(phrase);
                            }
                        }
                        previous_phrases = union_phrases;
                    }
                }
            }
        }
        return previous_phrases;
    }
    public static List<Verse> Intersect(List<Verse> source, List<Verse> target)
    {
        List<Verse> result = new List<Verse>();
        foreach (Verse item in source)
        {
            if (target.Contains(item))
            {
                result.Add(item);
            }
        }
        return result;
    }
    public static List<Verse> Union(List<Verse> source, List<Verse> target)
    {
        List<Verse> result = new List<Verse>(source);
        foreach (Verse item in target)
        {
            if (!source.Contains(item))
            {
                result.Add(item);
            }
        }
        return result;
    }

    private static List<Phrase> DoFindPhrases(string text_mode, FindScope find_scope, Selection current_selection, List<Verse> previous_result, string root, int multiplicity)
    {
        List<Phrase> result = new List<Phrase>();
        Book book = Book.Instance;
        if (book != null)
        {
            List<Verse> source = GetSourceVerses(text_mode, find_scope, current_selection, previous_result);
            result = DoFindPhrases(text_mode, source, find_scope, current_selection, previous_result, root, multiplicity);
        }
        return result;
    }
    private static List<Phrase> DoFindPhrases(string text_mode, List<Verse> source, FindScope find_scope, Selection current_selection, List<Verse> previous_result, string root, int multiplicity)
    {
        List<Phrase> result = new List<Phrase>();
        List<Verse> found_verses = new List<Verse>();
        Dictionary<string, List<Word>> root_words_dictionary = DataAccess.QuranRootWords;
        if (source != null)
        {
            if (source.Count > 0)
            {
                Book book = Book.Instance;
                if (book != null)
                {
                    if (root.Length > 0)
                    {
                        try
                        {
                            if (root_words_dictionary != null)
                            {
                                List<Word> root_words = null;
                                if (root_words_dictionary.ContainsKey(root))
                                {
                                    // get all pre-identified root_words
                                    root_words = root_words_dictionary[root];
                                }
                                else // if no such root, search for the matching root_word by its verse position and get its root and then get all root_words
                                {
                                    string new_root = GetRoot(text_mode, root);
                                    if (!String.IsNullOrEmpty(new_root))
                                    {
                                        // get all pre-identified root_words for new root
                                        root_words = root_words_dictionary[new_root];
                                    }
                                }

                                if (root_words != null)
                                {
                                    result = GetPhrasesWithRootWords(text_mode, source, root_words, multiplicity);
                                    foreach (Phrase phrase in result)
                                    {
                                        if (!found_verses.Contains(phrase.Verse))
                                        {
                                            found_verses.Add(phrase.Verse);
                                        }
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // log exception
                        }
                    }
                }
            }
        }
        return result;
    }
    private static List<Phrase> GetPhrasesWithRootWords(string text_mode, List<Verse> source, List<Word> root_words, int multiplicity)
    {
        List<Phrase> result = new List<Phrase>();
        if (source != null)
        {
            if (source.Count > 0)
            {
                Book book = Book.Instance;
                if (book != null)
                {
                    Dictionary<Verse, int> multiplicity_dictionary = new Dictionary<Verse, int>();
                    foreach (Word word in root_words)
                    {
                        Verse verse = book.Verses[word.Verse.Number - 1];
                        if (source.Contains(verse))
                        {
                            if (multiplicity_dictionary.ContainsKey(verse))
                            {
                                multiplicity_dictionary[verse]++;
                            }
                            else // first find
                            {
                                multiplicity_dictionary.Add(verse, 1);
                            }
                        }
                    }

                    if (multiplicity == 0) // verses not containg word
                    {
                        foreach (Verse verse in source)
                        {
                            if (!multiplicity_dictionary.ContainsKey(verse))
                            {
                                Phrase phrase = new Phrase(verse, text_mode, "", 0);
                                result.Add(phrase);
                            }
                        }
                    }
                    else // add only matching multiplicity or wildcard (-1)
                    {
                        foreach (Word root_word in root_words)
                        {
                            Verse verse = book.Verses[root_word.Verse.Number - 1];
                            if ((multiplicity == -1) || (multiplicity_dictionary[verse] == multiplicity))
                            {
                                if (source.Contains(verse))
                                {
                                    string verse_text = verse.GetText(text_mode);
                                    int word_index = root_word.NumberInVerse - 1;
                                    string[] parts = verse_text.Split(' ');
                                    string word_text = parts[word_index];
                                    int word_position = 0;
                                    int i = 0;
                                    while (i < word_index)
                                    {
                                        word_position += parts[i].Length + 1; // 1 for space after word
                                        i++;
                                    }
                                    Phrase phrase = new Phrase(verse, text_mode, word_text, word_position);
                                    result.Add(phrase);
                                }
                            }
                        }
                    }
                }
            }
        }
        return result;
    }

    // find by similarity
    public static List<Verse> FindVerses(string text_mode, FindScope find_scope, Selection current_selection, List<Verse> previous_result, Verse verse, FindBySimilarityMethod similarity_method, double similarity_percentage)
    {
        return DoFindVerses(text_mode, find_scope, current_selection, previous_result, verse, similarity_method, similarity_percentage);
    }
    private static List<Verse> DoFindVerses(string text_mode, FindScope find_scope, Selection current_selection, List<Verse> previous_result, Verse verse, FindBySimilarityMethod similarity_method, double similarity_percentage)
    {
        List<Verse> source = null;
        Book book = Book.Instance;
        if (book != null)
        {
            source = GetSourceVerses(text_mode, find_scope, current_selection, previous_result);
        }
        return DoFindVerses(text_mode, source, find_scope, current_selection, previous_result, verse, similarity_method, similarity_percentage);
    }
    private static List<Verse> DoFindVerses(string text_mode, List<Verse> source, FindScope find_scope, Selection current_selection, List<Verse> previous_result, Verse verse, FindBySimilarityMethod find_similarity_method, double similarity_percentage)
    {
        List<Verse> result = new List<Verse>();
        if (source != null)
        {
            if (source.Count > 0)
            {
                if (verse != null)
                {
                    try
                    {
                        switch (find_similarity_method)
                        {
                            case FindBySimilarityMethod.SimilarText:
                                {
                                    for (int j = 0; j < source.Count; j++)
                                    {
                                        if (verse.GetText(text_mode).IsSimilarTo(source[j].GetText(text_mode), similarity_percentage))
                                        {
                                            result.Add(source[j]);
                                        }
                                    }
                                }
                                break;
                            case FindBySimilarityMethod.SimilarWords:
                                {
                                    for (int j = 0; j < source.Count; j++)
                                    {
                                        if (verse.GetText(text_mode).HasSimilarWordsTo(source[j].GetText(text_mode), (int)Math.Round((Math.Min(verse.Words.Count, source[j].Words.Count) * similarity_percentage)), 1.0))
                                        {
                                            result.Add(source[j]);
                                        }
                                    }
                                }
                                break;
                            case FindBySimilarityMethod.SimilarStart:
                                {
                                    for (int j = 0; j < source.Count; j++)
                                    {
                                        if (verse.GetText(text_mode).HasSimilarFirstWordTo(source[j].GetText(text_mode), similarity_percentage))
                                        {
                                            result.Add(source[j]);
                                        }
                                    }
                                }
                                break;
                            case FindBySimilarityMethod.SimilarEnd:
                                {
                                    for (int j = 0; j < source.Count; j++)
                                    {
                                        if (verse.GetText(text_mode).HasSimilarLastWordTo(source[j].GetText(text_mode), similarity_percentage))
                                        {
                                            result.Add(source[j]);
                                        }
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
        }
        return result;
    }

    public static List<List<Verse>> FindVerseRanges(string text_mode, FindScope find_scope, Selection current_selection, List<Verse> previous_result, FindBySimilarityMethod similarity_method, double similarity_percentage)
    {
        return DoFindVerseRanges(text_mode, find_scope, current_selection, previous_result, similarity_method, similarity_percentage);
    }
    private static List<List<Verse>> DoFindVerseRanges(string text_mode, FindScope find_scope, Selection current_selection, List<Verse> previous_result, FindBySimilarityMethod similarity_method, double similarity_percentage)
    {
        List<Verse> source = null;
        Book book = Book.Instance;
        if (book != null)
        {
            source = GetSourceVerses(text_mode, find_scope, current_selection, previous_result);
        }
        return DoFindVerseRanges(text_mode, source, find_scope, current_selection, previous_result, similarity_method, similarity_percentage);
    }
    private static List<List<Verse>> DoFindVerseRanges(string text_mode, List<Verse> source, FindScope find_scope, Selection current_selection, List<Verse> previous_result, FindBySimilarityMethod find_similarity_method, double similarity_percentage)
    {
        List<List<Verse>> result = new List<List<Verse>>();
        Dictionary<Verse, List<Verse>> verse_ranges = new Dictionary<Verse, List<Verse>>(); // need dictionary to check if key exist
        bool[] already_compared = new bool[Verse.MAX_NUMBER];
        if (source != null)
        {
            if (source.Count > 0)
            {
                try
                {
                    switch (find_similarity_method)
                    {
                        case FindBySimilarityMethod.SimilarText:
                            {
                                for (int i = 0; i < source.Count - 1; i++)
                                {
                                    for (int j = i + 1; j < source.Count; j++)
                                    {
                                        if (!already_compared[j])
                                        {
                                            if (source[i].GetText(text_mode).IsSimilarTo(source[j].GetText(text_mode), similarity_percentage))
                                            {
                                                if (!verse_ranges.ContainsKey(source[i])) // first time matching verses found
                                                {
                                                    List<Verse> similar_verses = new List<Verse>();
                                                    verse_ranges.Add(source[i], similar_verses);
                                                    similar_verses.Add(source[i]);
                                                    similar_verses.Add(source[j]);
                                                    already_compared[i] = true;
                                                    already_compared[j] = true;
                                                }
                                                else // matching verses already exists
                                                {
                                                    List<Verse> similar_verses = verse_ranges[source[i]];
                                                    similar_verses.Add(source[j]);
                                                    already_compared[j] = true;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case FindBySimilarityMethod.SimilarWords:
                            {
                                for (int i = 0; i < source.Count - 1; i++)
                                {
                                    for (int j = i + 1; j < source.Count; j++)
                                    {
                                        if (!already_compared[j])
                                        {
                                            if (source[i].GetText(text_mode).HasSimilarWordsTo(source[j].GetText(text_mode), (int)Math.Round((Math.Min(source[i].Words.Count, source[j].Words.Count) * similarity_percentage)), 1.0))
                                            {
                                                if (!verse_ranges.ContainsKey(source[i])) // first time matching verses found
                                                {
                                                    List<Verse> similar_verses = new List<Verse>();
                                                    verse_ranges.Add(source[i], similar_verses);
                                                    similar_verses.Add(source[i]);
                                                    similar_verses.Add(source[j]);
                                                    already_compared[i] = true;
                                                    already_compared[j] = true;
                                                }
                                                else // matching verses already exists
                                                {
                                                    List<Verse> similar_verses = verse_ranges[source[i]];
                                                    similar_verses.Add(source[j]);
                                                    already_compared[j] = true;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case FindBySimilarityMethod.SimilarStart:
                            {
                                for (int i = 0; i < source.Count - 1; i++)
                                {
                                    for (int j = i + 1; j < source.Count; j++)
                                    {
                                        if (!already_compared[j])
                                        {
                                            if (source[j].GetText(text_mode).HasSimilarFirstWordTo(source[j].GetText(text_mode), similarity_percentage))
                                            {
                                                if (!verse_ranges.ContainsKey(source[i])) // first time matching verses found
                                                {
                                                    List<Verse> similar_verses = new List<Verse>();
                                                    verse_ranges.Add(source[i], similar_verses);
                                                    similar_verses.Add(source[i]);
                                                    similar_verses.Add(source[j]);
                                                    already_compared[i] = true;
                                                    already_compared[j] = true;
                                                }
                                                else // matching verses already exists
                                                {
                                                    List<Verse> similar_verses = verse_ranges[source[i]];
                                                    similar_verses.Add(source[j]);
                                                    already_compared[j] = true;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case FindBySimilarityMethod.SimilarEnd:
                            {
                                for (int i = 0; i < source.Count - 1; i++)
                                {
                                    for (int j = i + 1; j < source.Count; j++)
                                    {
                                        if (!already_compared[j])
                                        {
                                            if (source[i].GetText(text_mode).HasSimilarLastWordTo(source[j].GetText(text_mode), similarity_percentage))
                                            {
                                                if (!verse_ranges.ContainsKey(source[i])) // first time matching verses found
                                                {
                                                    List<Verse> similar_verses = new List<Verse>();
                                                    verse_ranges.Add(source[i], similar_verses);
                                                    similar_verses.Add(source[i]);
                                                    similar_verses.Add(source[j]);
                                                    already_compared[i] = true;
                                                    already_compared[j] = true;
                                                }
                                                else // matching verses already exists
                                                {
                                                    List<Verse> similar_verses = verse_ranges[source[i]];
                                                    similar_verses.Add(source[j]);
                                                    already_compared[j] = true;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
                catch
                {
                    return null;
                }
            }
        }

        // copy dictionary to list of list
        if (verse_ranges.Count > 0)
        {
            foreach (List<Verse> verse_range in verse_ranges.Values)
            {
                result.Add(verse_range);
            }
        }
        return result;
    }

    // find by numbers Words
    public static List<Word> FindWords(string text_mode, FindScope find_scope, Selection current_selection, List<Verse> previous_result,
        int number, int letter_count, int unique_letter_count, long value,
        NumberType number_number_type, NumberType letter_count_number_type, NumberType unique_letter_count_number_type, NumberType value_number_type,
        ComparisonOperator number_operator, ComparisonOperator letter_count_operator, ComparisonOperator unique_letter_count_operator, ComparisonOperator value_operator, string numerology_system_name,
        bool add_to_letter_l_number, bool add_to_letter_w_number, bool add_to_letter_v_number, bool add_to_letter_c_number, bool add_to_letter_l_distance, bool add_to_letter_w_distance, bool add_to_letter_v_distance, bool add_to_letter_c_distance, bool add_to_word_w_number, bool add_to_word_v_number, bool add_to_word_c_number, bool add_to_word_w_distance, bool add_to_word_v_distance, bool add_to_word_c_distance, bool add_to_verse_v_number, bool add_to_verse_c_number, bool add_to_verse_v_distance, bool add_to_verse_c_distance, bool add_to_chapter_c_number)
    {
        return DoFindWords(text_mode, find_scope, current_selection, previous_result,
                            number, letter_count, unique_letter_count, value,
                            number_number_type, letter_count_number_type, unique_letter_count_number_type, value_number_type,
                            number_operator, letter_count_operator, unique_letter_count_operator, value_operator, numerology_system_name,
                            add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);
    }
    private static List<Word> DoFindWords(string text_mode, FindScope find_scope, Selection current_selection, List<Verse> previous_result,
        int number, int letter_count, int unique_letter_count, long value,
        NumberType number_number_type, NumberType letter_count_number_type, NumberType unique_letter_count_number_type, NumberType value_number_type,
        ComparisonOperator number_operator, ComparisonOperator letter_count_operator, ComparisonOperator unique_letter_count_operator, ComparisonOperator value_operator, string numerology_system_name,
        bool add_to_letter_l_number, bool add_to_letter_w_number, bool add_to_letter_v_number, bool add_to_letter_c_number, bool add_to_letter_l_distance, bool add_to_letter_w_distance, bool add_to_letter_v_distance, bool add_to_letter_c_distance, bool add_to_word_w_number, bool add_to_word_v_number, bool add_to_word_c_number, bool add_to_word_w_distance, bool add_to_word_v_distance, bool add_to_word_c_distance, bool add_to_verse_v_number, bool add_to_verse_c_number, bool add_to_verse_v_distance, bool add_to_verse_c_distance, bool add_to_chapter_c_number)
    {
        List<Verse> source = null;
        Book book = Book.Instance;
        if (book != null)
        {
            source = GetSourceVerses(text_mode, find_scope, current_selection, previous_result);
        }
        return DoFindWords(text_mode, source,
                            number, letter_count, unique_letter_count, value,
                            number_number_type, letter_count_number_type, unique_letter_count_number_type, value_number_type,
                            number_operator, letter_count_operator, unique_letter_count_operator, value_operator, numerology_system_name,
                            add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);
    }
    private static List<Word> DoFindWords(string text_mode, List<Verse> source,
        int number, int letter_count, int unique_letter_count, long value,
        NumberType number_number_type, NumberType letter_count_number_type, NumberType unique_letter_count_number_type, NumberType value_number_type,
        ComparisonOperator number_operator, ComparisonOperator letter_count_operator, ComparisonOperator unique_letter_count_operator, ComparisonOperator value_operator, string numerology_system_name,
        bool add_to_letter_l_number, bool add_to_letter_w_number, bool add_to_letter_v_number, bool add_to_letter_c_number, bool add_to_letter_l_distance, bool add_to_letter_w_distance, bool add_to_letter_v_distance, bool add_to_letter_c_distance, bool add_to_word_w_number, bool add_to_word_v_number, bool add_to_word_c_number, bool add_to_word_w_distance, bool add_to_word_v_distance, bool add_to_word_c_distance, bool add_to_verse_v_number, bool add_to_verse_c_number, bool add_to_verse_v_distance, bool add_to_verse_c_distance, bool add_to_chapter_c_number)
    {
        List<Word> result = new List<Word>(); // fill it up, then filter out condition by condition
        foreach (Verse verse in source)
        {
            result.AddRange(verse.Words);
        }

        List<Word> temp = new List<Word>();
        if (source != null)
        {
            if (source.Count > 0)
            {
                if (number > 0)
                {
                    temp.Clear();
                    for (int i = 0; i < result.Count; i++)
                    {
                        if (Numbers.Compare(result[i].NumberInVerse, number, number_operator))
                        {
                            temp.Add(result[i]);
                        }
                    }
                    result.Clear();
                    result.AddRange(temp);
                }

                if (letter_count > 0)
                {
                    temp.Clear();
                    foreach (Word word in result)
                    {
                        if (Numbers.Compare(word.Letters.Count, letter_count, letter_count_operator))
                        {
                            temp.Add(word);
                        }
                    }
                    result.Clear();
                    result.AddRange(temp);
                }

                if (unique_letter_count > 0)
                {
                    temp.Clear();
                    foreach (Word word in result)
                    {
                        if (Numbers.Compare(word.UniqueLetters.Count, unique_letter_count, unique_letter_count_operator))
                        {
                            temp.Add(word);
                        }
                    }
                    result.Clear();
                    result.AddRange(temp);
                }

                if (value > 0)
                {
                    temp.Clear();
                    foreach (Word word in result)
                    {
                        if (Numbers.Compare(CalculateValue(word, numerology_system_name, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number), value, value_operator))
                        {
                            temp.Add(word);
                        }
                    }
                    result.Clear();
                    result.AddRange(temp);
                }


                if (number_number_type != NumberType.None)
                {
                    temp.Clear();
                    for (int i = 0; i < result.Count; i++)
                    {
                        if (Numbers.IsNumberType(result[i].NumberInVerse, number_number_type))
                        {
                            temp.Add(result[i]);
                        }
                    }
                    result.Clear();
                    result.AddRange(temp);
                }

                if (letter_count_number_type != NumberType.None)
                {
                    temp.Clear();
                    foreach (Word word in result)
                    {
                        if (Numbers.IsNumberType(word.Letters.Count, letter_count_number_type))
                        {
                            temp.Add(word);
                        }
                    }
                    result.Clear();
                    result.AddRange(temp);
                }

                if (unique_letter_count_number_type != NumberType.None)
                {
                    temp.Clear();
                    foreach (Word word in result)
                    {
                        if (Numbers.IsNumberType(word.UniqueLetters.Count, unique_letter_count_number_type))
                        {
                            temp.Add(word);
                        }
                    }
                    result.Clear();
                    result.AddRange(temp);
                }

                if (value_number_type != NumberType.None)
                {
                    temp.Clear();
                    foreach (Word word in result)
                    {
                        if (Numbers.IsNumberType(CalculateValue(word, numerology_system_name, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number), value_number_type))
                        {
                            temp.Add(word);
                        }
                    }
                    result.Clear();
                    result.AddRange(temp);
                }
            }
        }
        return result;
    }

    // find by numbers WordRanges within verses, not across verse boundaries
    private static long AddToUniqueLetters(ref List<char> unique_letters, string text)
    {
        long count = 0;
        foreach (char character in text)
        {
            if (character == ' ') continue;

            if (!Constants.STOP_MARKS.Contains(character))
            {
                if (!unique_letters.Contains(character))
                {
                    unique_letters.Add(character);
                    count++;
                }
            }
        }
        return count;
    }
    public static List<List<Word>> FindWordRanges(string text_mode, FindScope find_scope, Selection current_selection, List<Verse> previous_result,
        int word_count, int letter_count, int unique_letter_count, long value,
        NumberType word_count_number_type, NumberType letter_count_number_type, NumberType unique_letter_count_number_type, NumberType value_number_type,
        ComparisonOperator word_count_operator, ComparisonOperator letter_count_operator, ComparisonOperator unique_letter_count_operator, ComparisonOperator value_operator, string numerology_system_name,
        bool add_to_letter_l_number, bool add_to_letter_w_number, bool add_to_letter_v_number, bool add_to_letter_c_number, bool add_to_letter_l_distance, bool add_to_letter_w_distance, bool add_to_letter_v_distance, bool add_to_letter_c_distance, bool add_to_word_w_number, bool add_to_word_v_number, bool add_to_word_c_number, bool add_to_word_w_distance, bool add_to_word_v_distance, bool add_to_word_c_distance, bool add_to_verse_v_number, bool add_to_verse_c_number, bool add_to_verse_v_distance, bool add_to_verse_c_distance, bool add_to_chapter_c_number)
    {
        return DoFindWordRanges(text_mode, find_scope, current_selection, previous_result, word_count, letter_count, unique_letter_count, value,
                            word_count_number_type, letter_count_number_type, unique_letter_count_number_type, value_number_type,
                            word_count_operator, letter_count_operator, unique_letter_count_operator, value_operator, numerology_system_name,
                            add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);
    }
    private static List<List<Word>> DoFindWordRanges(string text_mode, FindScope find_scope, Selection current_selection, List<Verse> previous_result,
        int word_count, int letter_count, int unique_letter_count, long value,
        NumberType word_count_number_type, NumberType letter_count_number_type, NumberType unique_letter_count_number_type, NumberType value_number_type,
        ComparisonOperator word_count_operator, ComparisonOperator letter_count_operator, ComparisonOperator unique_letter_count_operator, ComparisonOperator value_operator, string numerology_system_name,
        bool add_to_letter_l_number, bool add_to_letter_w_number, bool add_to_letter_v_number, bool add_to_letter_c_number, bool add_to_letter_l_distance, bool add_to_letter_w_distance, bool add_to_letter_v_distance, bool add_to_letter_c_distance, bool add_to_word_w_number, bool add_to_word_v_number, bool add_to_word_c_number, bool add_to_word_w_distance, bool add_to_word_v_distance, bool add_to_word_c_distance, bool add_to_verse_v_number, bool add_to_verse_c_number, bool add_to_verse_v_distance, bool add_to_verse_c_distance, bool add_to_chapter_c_number)
    {
        List<Verse> source = null;
        Book book = Book.Instance;
        if (book != null)
        {
            source = GetSourceVerses(text_mode, find_scope, current_selection, previous_result);
        }
        return DoFindWordRanges(text_mode, source, word_count, letter_count, unique_letter_count, value,
                            word_count_number_type, letter_count_number_type, unique_letter_count_number_type, value_number_type,
                            word_count_operator, letter_count_operator, unique_letter_count_operator, value_operator, numerology_system_name,
                            add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);
    }
    private static List<List<Word>> DoFindWordRanges(string text_mode, List<Verse> source,
        int word_count, int letter_count, int unique_letter_count, long value,
        NumberType word_count_number_type, NumberType letter_count_number_type, NumberType unique_letter_count_number_type, NumberType value_number_type,
        ComparisonOperator word_count_operator, ComparisonOperator letter_count_operator, ComparisonOperator unique_letter_count_operator, ComparisonOperator value_operator, string numerology_system_name,
        bool add_to_letter_l_number, bool add_to_letter_w_number, bool add_to_letter_v_number, bool add_to_letter_c_number, bool add_to_letter_l_distance, bool add_to_letter_w_distance, bool add_to_letter_v_distance, bool add_to_letter_c_distance, bool add_to_word_w_number, bool add_to_word_v_number, bool add_to_word_c_number, bool add_to_word_w_distance, bool add_to_word_v_distance, bool add_to_word_c_distance, bool add_to_verse_v_number, bool add_to_verse_c_number, bool add_to_verse_v_distance, bool add_to_verse_c_distance, bool add_to_chapter_c_number)
    {
        List<List<Word>> result = null;
        if (source != null)
        {
            if (source.Count > 0)
            {
                if (letter_count_number_type == NumberType.None)
                {
                    if (letter_count > 0)
                    {
                        if (result == null) // if this is the first condition
                        {
                            result = new List<List<Word>>();

                            foreach (Verse verse in source) // don't cross verse boundaries, process each verse separately
                            {
                                for (int i = 0; i < verse.Words.Count - 1; i++)
                                {
                                    List<Word> word_range = new List<Word>();
                                    word_range.Add(verse.Words[i]);

                                    long total = verse.Words[i].Letters.Count;
                                    for (int j = i + 1; j < verse.Words.Count; j++)
                                    {
                                        total += verse.Words[j].Letters.Count;
                                        if (total > letter_count) // limit exceeded
                                        {
                                            break; // and try next starting word
                                        }

                                        word_range.Add(verse.Words[j]);
                                        if (word_count > 0) // word_count is specified
                                        {
                                            if (word_range.Count == word_count)
                                            {
                                                if (Numbers.Compare(total, letter_count, letter_count_operator))
                                                {
                                                    result.Add(word_range);
                                                    break; // and try next starting word
                                                }
                                            }

                                            if (word_range.Count > word_count) // limit exceeded
                                            {
                                                break; // and try next starting word
                                            }
                                        }
                                        else // no specified word_count
                                        {
                                            if (Numbers.Compare(total, letter_count, letter_count_operator))
                                            {
                                                result.Add(word_range);
                                                break; // and try next starting word
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else // filter existing result
                        {
                            for (int i = 0; i < result.Count; i++) // don't cross verse boundaries, process each verse separately
                            {
                                long total = 0;
                                foreach (Word word in result[i])
                                {
                                    total += word.Letters.Count;
                                }

                                if (!Numbers.Compare(total, letter_count, letter_count_operator))
                                {
                                    result.Remove(result[i]);
                                    i--; // self-healing
                                }
                            }
                        }
                    }
                }
                else //if (letter_count_number_type != NumberType.None)
                {
                    if (result == null) // if this is the first condition
                    {
                        result = new List<List<Word>>();

                        foreach (Verse verse in source) // don't cross verse boundaries, process each verse separately
                        {
                            for (int i = 0; i < verse.Words.Count - 1; i++)
                            {
                                List<Word> word_range = new List<Word>();
                                word_range.Add(verse.Words[i]);

                                long total = verse.Words[i].Letters.Count;
                                for (int j = i + 1; j < verse.Words.Count; j++)
                                {
                                    total += verse.Words[j].Letters.Count;
                                    word_range.Add(verse.Words[j]);
                                    if (word_count > 0) // word_count is specified
                                    {
                                        if (word_range.Count == word_count)
                                        {
                                            if (Numbers.IsNumberType(total, letter_count_number_type)) // match found
                                            {
                                                result.Add(word_range);
                                                break; // and try next starting word
                                            }
                                        }

                                        if (word_range.Count > word_count) // limit exceeded
                                        {
                                            break; // and try next starting word
                                        }
                                    }
                                    else // no specified word_count
                                    {
                                        if (Numbers.IsNumberType(total, letter_count_number_type)) // match found
                                        {
                                            result.Add(word_range);
                                            break; // and try next starting word
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else // filter existing result
                    {
                        for (int i = 0; i < result.Count; i++) // don't cross verse boundaries, process each verse separately
                        {
                            long total = 0;
                            foreach (Word word in result[i])
                            {
                                total += word.Letters.Count;
                            }

                            if (!Numbers.IsNumberType(total, letter_count_number_type)) // not a match
                            {
                                result.Remove(result[i]);
                                i--; // self-healing
                            }
                        }
                    }
                }

                if (unique_letter_count_number_type == NumberType.None)
                {
                    if (unique_letter_count > 0)
                    {
                        List<char> unique_letters = new List<char>();

                        if (result == null) // if this is the first condition
                        {
                            result = new List<List<Word>>();

                            foreach (Verse verse in source) // don't cross verse boundaries, process each verse separately
                            {
                                for (int i = 0; i < verse.Words.Count - 1; i++)
                                {
                                    List<Word> word_range = new List<Word>();
                                    word_range.Add(verse.Words[i]);

                                    unique_letters.Clear();

                                    long total = AddToUniqueLetters(ref unique_letters, verse.Words[i].Texts[text_mode]);
                                    for (int j = i + 1; j < verse.Words.Count; j++)
                                    {
                                        total += AddToUniqueLetters(ref unique_letters, verse.Words[j].Texts[text_mode]);
                                        if (total > unique_letter_count) // limit exceeded
                                        {
                                            break; // and try next starting word
                                        }

                                        word_range.Add(verse.Words[j]);
                                        if (word_count > 0) // word_count is specified
                                        {
                                            if (word_range.Count == word_count)
                                            {
                                                if (Numbers.Compare(total, unique_letter_count, unique_letter_count_operator))
                                                {
                                                    result.Add(word_range);
                                                    break; // and try next starting word
                                                }
                                            }

                                            if (word_range.Count > word_count) // limit exceeded
                                            {
                                                break; // and try next starting word
                                            }
                                        }
                                        else // no specified word_count
                                        {
                                            if (Numbers.Compare(total, unique_letter_count, unique_letter_count_operator))
                                            {
                                                result.Add(word_range);
                                                break; // and try next starting word
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else // filter existing result
                        {
                            for (int i = 0; i < result.Count; i++) // don't cross verse boundaries, process each verse separately
                            {
                                long total = 0;
                                unique_letters.Clear();
                                foreach (Word word in result[i])
                                {
                                    total += AddToUniqueLetters(ref unique_letters, word.Texts[text_mode]);
                                }

                                if (!Numbers.Compare(total, unique_letter_count, unique_letter_count_operator))
                                {
                                    result.Remove(result[i]);
                                    i--; // self-healing
                                }
                            }
                        }
                    }
                }
                else //if (unique_letter_count_number_type != NumberType.None)
                {
                    List<char> unique_letters = new List<char>();
                    if (result == null) // if this is the first condition
                    {
                        result = new List<List<Word>>();

                        foreach (Verse verse in source) // don't cross verse boundaries, process each verse separately
                        {
                            for (int i = 0; i < verse.Words.Count - 1; i++)
                            {
                                List<Word> word_range = new List<Word>();
                                word_range.Add(verse.Words[i]);

                                unique_letters.Clear();
                                long total = AddToUniqueLetters(ref unique_letters, verse.Words[i].Texts[text_mode]);
                                for (int j = i + 1; j < verse.Words.Count; j++)
                                {
                                    total += AddToUniqueLetters(ref unique_letters, verse.Words[j].Texts[text_mode]);
                                    word_range.Add(verse.Words[j]);
                                    if (word_count > 0) // word_count is specified
                                    {
                                        if (word_range.Count == word_count)
                                        {
                                            if (Numbers.IsNumberType(total, unique_letter_count_number_type)) // match found
                                            {
                                                result.Add(word_range);
                                                break; // and try next starting word
                                            }
                                        }

                                        if (word_range.Count > word_count) // limit exceeded
                                        {
                                            break; // and try next starting word
                                        }
                                    }
                                    else // no specified word_count
                                    {
                                        if (Numbers.IsNumberType(total, unique_letter_count_number_type)) // match found
                                        {
                                            result.Add(word_range);
                                            break; // and try next starting word
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else // filter existing result
                    {
                        for (int i = 0; i < result.Count; i++) // don't cross verse boundaries, process each verse separately
                        {
                            long total = 0;
                            unique_letters.Clear();
                            foreach (Word word in result[i])
                            {
                                total += AddToUniqueLetters(ref unique_letters, word.Texts[text_mode]);
                            }

                            if (!Numbers.IsNumberType(total, unique_letter_count_number_type)) // not a match
                            {
                                result.Remove(result[i]);
                                i--; // self-healing
                            }
                        }
                    }
                }


                if (value_number_type == NumberType.None)
                {
                    if (value > 0)
                    {
                        if (result == null) // if this is the first condition
                        {
                            result = new List<List<Word>>();

                            foreach (Verse verse in source) // don't cross verse boundaries, process each verse separately
                            {
                                for (int i = 0; i < verse.Words.Count - 1; i++)
                                {
                                    List<Word> word_range = new List<Word>();
                                    word_range.Add(verse.Words[i]);

                                    long total = CalculateValue(verse.Words[i], numerology_system_name, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);
                                    for (int j = i + 1; j < verse.Words.Count; j++)
                                    {
                                        total += CalculateValue(verse.Words[j], numerology_system_name, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);
                                        if (total > value) // limit exceeded
                                        {
                                            break; // and try next starting word
                                        }

                                        word_range.Add(verse.Words[j]);
                                        if (word_count > 0) // word_count is specified
                                        {
                                            if (word_range.Count == word_count)
                                            {
                                                if (Numbers.Compare(total, value, value_operator))
                                                {
                                                    result.Add(word_range);
                                                    break; // and try next starting word
                                                }
                                            }

                                            if (word_range.Count > word_count) // limit exceeded
                                            {
                                                break; // and try next starting word
                                            }
                                        }
                                        else // no specified word_count
                                        {
                                            if (Numbers.Compare(total, value, value_operator))
                                            {
                                                result.Add(word_range);
                                                break; // and try next starting word
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else // filter existing result
                        {
                            for (int i = 0; i < result.Count; i++) // don't cross verse boundaries, process each verse separately
                            {
                                long total = 0;
                                foreach (Word word in result[i])
                                {
                                    total += CalculateValue(word, numerology_system_name, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);
                                }

                                if (!Numbers.Compare(total, value, value_operator))
                                {
                                    result.Remove(result[i]);
                                    i--; // self-healing
                                }
                            }
                        }
                    }
                }
                else //if (value_number_type != NumberType.None)
                {
                    if (result == null) // if this is the first condition
                    {
                        result = new List<List<Word>>();

                        foreach (Verse verse in source) // don't cross verse boundaries, process each verse separately
                        {
                            for (int i = 0; i < verse.Words.Count - 1; i++)
                            {
                                List<Word> word_range = new List<Word>();
                                word_range.Add(verse.Words[i]);

                                long total = CalculateValue(verse.Words[i], numerology_system_name, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);
                                for (int j = i + 1; j < verse.Words.Count; j++)
                                {
                                    total += CalculateValue(verse.Words[j], numerology_system_name, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);
                                    word_range.Add(verse.Words[j]);
                                    if (word_count > 0) // word_count is specified
                                    {
                                        if (word_range.Count == word_count)
                                        {
                                            if (Numbers.IsNumberType(total, value_number_type)) // match found
                                            {
                                                result.Add(word_range);
                                                break; // and try next starting word
                                            }
                                        }

                                        if (word_range.Count > word_count) // limit exceeded
                                        {
                                            break; // and try next starting word
                                        }
                                    }
                                    else // no specified word_count
                                    {
                                        if (Numbers.IsNumberType(total, value_number_type)) // match found
                                        {
                                            result.Add(word_range);
                                            break; // and try next starting word
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else // filter existing result
                    {
                        for (int i = 0; i < result.Count; i++) // don't cross verse boundaries, process each verse separately
                        {
                            long total = 0;
                            foreach (Word word in result[i])
                            {
                                total += CalculateValue(word, numerology_system_name, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);
                            }

                            if (!Numbers.IsNumberType(total, value_number_type)) // not a match
                            {
                                result.Remove(result[i]);
                                i--; // self-healing
                            }
                        }
                    }
                }
            }
        }
        return result;
    }

    // find by numbers Verses
    public static List<Verse> FindVerses(string text_mode, FindScope find_scope, Selection current_selection, List<Verse> previous_result,
        int number, int word_count, int letter_count, int unique_letter_count, long value,
        NumberType number_number_type, NumberType word_count_number_type, NumberType letter_count_number_type, NumberType unique_letter_count_number_type, NumberType value_number_type,
        ComparisonOperator number_operator, ComparisonOperator word_count_operator, ComparisonOperator letter_count_operator, ComparisonOperator unique_letter_count_operator, ComparisonOperator value_operator, string numerology_system_name,
        bool add_to_letter_l_number, bool add_to_letter_w_number, bool add_to_letter_v_number, bool add_to_letter_c_number, bool add_to_letter_l_distance, bool add_to_letter_w_distance, bool add_to_letter_v_distance, bool add_to_letter_c_distance, bool add_to_word_w_number, bool add_to_word_v_number, bool add_to_word_c_number, bool add_to_word_w_distance, bool add_to_word_v_distance, bool add_to_word_c_distance, bool add_to_verse_v_number, bool add_to_verse_c_number, bool add_to_verse_v_distance, bool add_to_verse_c_distance, bool add_to_chapter_c_number)
    {
        return DoFindVerses(text_mode, find_scope, current_selection, previous_result,
                            number, word_count, letter_count, unique_letter_count, value,
                            number_number_type, word_count_number_type, letter_count_number_type, unique_letter_count_number_type, value_number_type,
                            number_operator, word_count_operator, letter_count_operator, unique_letter_count_operator, value_operator, numerology_system_name,
                            add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);
    }
    private static List<Verse> DoFindVerses(string text_mode, FindScope find_scope, Selection current_selection, List<Verse> previous_result,
        int number, int word_count, int letter_count, int unique_letter_count, long value,
        NumberType number_number_type, NumberType word_count_number_type, NumberType letter_count_number_type, NumberType unique_letter_count_number_type, NumberType value_number_type,
        ComparisonOperator number_operator, ComparisonOperator word_count_operator, ComparisonOperator letter_count_operator, ComparisonOperator unique_letter_count_operator, ComparisonOperator value_operator, string numerology_system_name,
        bool add_to_letter_l_number, bool add_to_letter_w_number, bool add_to_letter_v_number, bool add_to_letter_c_number, bool add_to_letter_l_distance, bool add_to_letter_w_distance, bool add_to_letter_v_distance, bool add_to_letter_c_distance, bool add_to_word_w_number, bool add_to_word_v_number, bool add_to_word_c_number, bool add_to_word_w_distance, bool add_to_word_v_distance, bool add_to_word_c_distance, bool add_to_verse_v_number, bool add_to_verse_c_number, bool add_to_verse_v_distance, bool add_to_verse_c_distance, bool add_to_chapter_c_number)
    {
        List<Verse> source = null;
        Book book = Book.Instance;
        if (book != null)
        {
            source = GetSourceVerses(text_mode, find_scope, current_selection, previous_result);
        }
        return DoFindVerses(text_mode, source,
                            number, word_count, letter_count, unique_letter_count, value,
                            number_number_type, word_count_number_type, letter_count_number_type, unique_letter_count_number_type, value_number_type,
                            number_operator, word_count_operator, letter_count_operator, unique_letter_count_operator, value_operator, numerology_system_name,
                            add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);
    }
    private static List<Verse> DoFindVerses(string text_mode, List<Verse> source,
        int number, int word_count, int letter_count, int unique_letter_count, long value,
        NumberType number_number_type, NumberType word_count_number_type, NumberType letter_count_number_type, NumberType unique_letter_count_number_type, NumberType value_number_type,
        ComparisonOperator number_operator, ComparisonOperator word_count_operator, ComparisonOperator letter_count_operator, ComparisonOperator unique_letter_count_operator, ComparisonOperator value_operator, string numerology_system_name,
        bool add_to_letter_l_number, bool add_to_letter_w_number, bool add_to_letter_v_number, bool add_to_letter_c_number, bool add_to_letter_l_distance, bool add_to_letter_w_distance, bool add_to_letter_v_distance, bool add_to_letter_c_distance, bool add_to_word_w_number, bool add_to_word_v_number, bool add_to_word_c_number, bool add_to_word_w_distance, bool add_to_word_v_distance, bool add_to_word_c_distance, bool add_to_verse_v_number, bool add_to_verse_c_number, bool add_to_verse_v_distance, bool add_to_verse_c_distance, bool add_to_chapter_c_number)
    {
        List<Verse> result = new List<Verse>(source); // fill it up, then filter out condition by condition
        List<Verse> temp = new List<Verse>();
        if (source != null)
        {
            if (source.Count > 0)
            {
                if (number > 0)
                {
                    temp.Clear();
                    for (int i = 0; i < result.Count; i++)
                    {
                        if (Numbers.Compare(result[i].NumberInChapter, number, number_operator))
                        {
                            temp.Add(result[i]);
                        }
                    }
                    result.Clear();
                    result.AddRange(temp);
                }

                if (word_count > 0)
                {
                    temp.Clear();
                    foreach (Verse verse in result)
                    {
                        if (Numbers.Compare(verse.Words.Count, word_count, word_count_operator))
                        {
                            temp.Add(verse);
                        }
                    }
                    result.Clear();
                    result.AddRange(temp);
                }

                if (letter_count > 0)
                {
                    temp.Clear();
                    foreach (Verse verse in result)
                    {
                        if (Numbers.Compare(verse.LetterCount, letter_count, letter_count_operator))
                        {
                            temp.Add(verse);
                        }
                    }
                    result.Clear();
                    result.AddRange(temp);
                }

                if (unique_letter_count > 0)
                {
                    temp.Clear();
                    foreach (Verse verse in result)
                    {
                        if (Numbers.Compare(verse.UniqueLetters.Count, unique_letter_count, unique_letter_count_operator))
                        {
                            temp.Add(verse);
                        }
                    }
                    result.Clear();
                    result.AddRange(temp);
                }

                if (value > 0)
                {
                    temp.Clear();
                    foreach (Verse verse in result)
                    {
                        if (Numbers.Compare(CalculateValue(verse, numerology_system_name, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number), value, value_operator))
                        {
                            temp.Add(verse);
                        }
                    }
                    result.Clear();
                    result.AddRange(temp);
                }


                if (number_number_type != NumberType.None)
                {
                    if (number_number_type != NumberType.Any)
                    {
                        temp.Clear();
                        foreach (Verse verse in result)
                        {
                            if (Numbers.IsNumberType(verse.NumberInChapter, number_number_type))
                            {
                                temp.Add(verse);
                            }
                        }
                    }
                    else
                    {
                        // group verses by number
                        bool[] processed = new bool[result.Count];

                        temp.Clear();
                        for (int i = 0; i < result.Count - 1; i++)
                        {
                            if (processed[i]) continue;

                            bool verse_i_added = false;
                            for (int j = i + 1; j < result.Count; j++)
                            {
                                if (processed[j]) continue;

                                if (result[i].NumberInChapter == result[j].NumberInChapter)
                                {
                                    if (!verse_i_added)
                                    {
                                        temp.Add(result[i]);
                                        verse_i_added = true;
                                        processed[i] = true;
                                    }

                                    temp.Add(result[j]);
                                    processed[j] = true;
                                }
                            }
                        }
                    }
                    result.Clear();
                    result.AddRange(temp);
                }

                if (word_count_number_type != NumberType.None)
                {
                    if (word_count_number_type != NumberType.Any)
                    {
                        temp.Clear();
                        foreach (Verse verse in result)
                        {
                            if (Numbers.IsNumberType(verse.Words.Count, word_count_number_type))
                            {
                                temp.Add(verse);
                            }
                        }
                    }
                    else
                    {
                        // group verses by word_count
                        bool[] processed = new bool[result.Count];

                        temp.Clear();
                        for (int i = 0; i < result.Count - 1; i++)
                        {
                            if (processed[i]) continue;

                            bool verse_i_added = false;
                            for (int j = i + 1; j < result.Count; j++)
                            {
                                if (processed[j]) continue;

                                if (result[i].Words.Count == result[j].Words.Count)
                                {
                                    if (!verse_i_added)
                                    {
                                        temp.Add(result[i]);
                                        verse_i_added = true;
                                        processed[i] = true;
                                    }

                                    temp.Add(result[j]);
                                    processed[j] = true;
                                }
                            }
                        }
                    }
                    result.Clear();
                    result.AddRange(temp);
                }

                if (letter_count_number_type != NumberType.None)
                {
                    if (letter_count_number_type != NumberType.Any)
                    {
                        temp.Clear();
                        foreach (Verse verse in result)
                        {
                            if (Numbers.IsNumberType(verse.LetterCount, letter_count_number_type))
                            {
                                temp.Add(verse);
                            }
                        }
                    }
                    else
                    {
                        // group verses by letter_count
                        bool[] processed = new bool[result.Count];

                        temp.Clear();
                        for (int i = 0; i < result.Count - 1; i++)
                        {
                            if (processed[i]) continue;

                            bool verse_i_added = false;
                            for (int j = i + 1; j < result.Count; j++)
                            {
                                if (processed[j]) continue;

                                if (result[i].LetterCount == result[j].LetterCount)
                                {
                                    if (!verse_i_added)
                                    {
                                        temp.Add(result[i]);
                                        verse_i_added = true;
                                        processed[i] = true;
                                    }

                                    temp.Add(result[j]);
                                    processed[j] = true;
                                }
                            }
                        }
                    }
                    result.Clear();
                    result.AddRange(temp);
                }

                if (unique_letter_count_number_type != NumberType.None)
                {
                    if (unique_letter_count_number_type != NumberType.Any)
                    {
                        temp.Clear();
                        foreach (Verse verse in result)
                        {
                            if (Numbers.IsNumberType(verse.UniqueLetters.Count, unique_letter_count_number_type))
                            {
                                temp.Add(verse);
                            }
                        }
                    }
                    else
                    {
                        // group verses by unique_letter_count
                        bool[] processed = new bool[result.Count];

                        temp.Clear();
                        for (int i = 0; i < result.Count - 1; i++)
                        {
                            if (processed[i]) continue;

                            bool verse_i_added = false;
                            for (int j = i + 1; j < result.Count; j++)
                            {
                                if (processed[j]) continue;

                                if (result[i].UniqueLetters.Count == result[j].UniqueLetters.Count)
                                {
                                    if (!verse_i_added)
                                    {
                                        temp.Add(result[i]);
                                        verse_i_added = true;
                                        processed[i] = true;
                                    }

                                    temp.Add(result[j]);
                                    processed[j] = true;
                                }
                            }
                        }
                    }
                    result.Clear();
                    result.AddRange(temp);
                }

                if (value_number_type != NumberType.None)
                {
                    if (value_number_type != NumberType.Any)
                    {
                        temp.Clear();
                        foreach (Verse verse in result)
                        {
                            if (Numbers.IsNumberType(CalculateValue(verse, numerology_system_name, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number), value_number_type))
                            {
                                temp.Add(verse);
                            }
                        }
                    }
                    else
                    {
                        // group verses by value
                        bool[] processed = new bool[result.Count];

                        temp.Clear();
                        for (int i = 0; i < result.Count - 1; i++)
                        {
                            if (processed[i]) continue;
                            long verse_i_value = CalculateValue(result[i], numerology_system_name, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);

                            bool verse_i_added = false;
                            for (int j = i + 1; j < result.Count; j++)
                            {
                                if (processed[j]) continue;
                                long verse_j_value = CalculateValue(result[j], numerology_system_name, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);

                                if (verse_i_value == verse_j_value)
                                {
                                    if (!verse_i_added)
                                    {
                                        temp.Add(result[i]);
                                        verse_i_added = true;
                                        processed[i] = true;
                                    }

                                    temp.Add(result[j]);
                                    processed[j] = true;
                                }
                            }
                        }
                    }
                    result.Clear();
                    result.AddRange(temp);
                }
            }
        }
        return result;
    }

    // find by numbers VerseRanges
    public static List<List<Verse>> FindVerseRanges(string text_mode, FindScope find_scope, Selection current_selection, List<Verse> previous_result,
        int verse_count, int word_count, int letter_count, int unique_letter_count, long value,
        NumberType verse_count_number_type, NumberType word_count_number_type, NumberType letter_count_number_type, NumberType unique_letter_count_number_type, NumberType value_number_type,
        ComparisonOperator verse_count_operator, ComparisonOperator word_count_operator, ComparisonOperator letter_count_operator, ComparisonOperator unique_letter_count_operator, ComparisonOperator value_operator, string numerology_system_name,
        bool add_to_letter_l_number, bool add_to_letter_w_number, bool add_to_letter_v_number, bool add_to_letter_c_number, bool add_to_letter_l_distance, bool add_to_letter_w_distance, bool add_to_letter_v_distance, bool add_to_letter_c_distance, bool add_to_word_w_number, bool add_to_word_v_number, bool add_to_word_c_number, bool add_to_word_w_distance, bool add_to_word_v_distance, bool add_to_word_c_distance, bool add_to_verse_v_number, bool add_to_verse_c_number, bool add_to_verse_v_distance, bool add_to_verse_c_distance, bool add_to_chapter_c_number)
    {
        return DoFindVerseRanges(text_mode, find_scope, current_selection, previous_result, verse_count, word_count, letter_count, unique_letter_count, value,
                            verse_count_number_type, word_count_number_type, letter_count_number_type, unique_letter_count_number_type, value_number_type,
                            verse_count_operator, word_count_operator, letter_count_operator, unique_letter_count_operator, value_operator, numerology_system_name,
                            add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);
    }
    private static List<List<Verse>> DoFindVerseRanges(string text_mode, FindScope find_scope, Selection current_selection, List<Verse> previous_result,
        int verse_count, int word_count, int letter_count, int unique_letter_count, long value,
        NumberType verse_count_number_type, NumberType word_count_number_type, NumberType letter_count_number_type, NumberType unique_letter_count_number_type, NumberType value_number_type,
        ComparisonOperator verse_count_operator, ComparisonOperator word_count_operator, ComparisonOperator letter_count_operator, ComparisonOperator unique_letter_count_operator, ComparisonOperator value_operator, string numerology_system_name,
        bool add_to_letter_l_number, bool add_to_letter_w_number, bool add_to_letter_v_number, bool add_to_letter_c_number, bool add_to_letter_l_distance, bool add_to_letter_w_distance, bool add_to_letter_v_distance, bool add_to_letter_c_distance, bool add_to_word_w_number, bool add_to_word_v_number, bool add_to_word_c_number, bool add_to_word_w_distance, bool add_to_word_v_distance, bool add_to_word_c_distance, bool add_to_verse_v_number, bool add_to_verse_c_number, bool add_to_verse_v_distance, bool add_to_verse_c_distance, bool add_to_chapter_c_number)
    {
        List<Verse> source = null;
        Book book = Book.Instance;
        if (book != null)
        {
            source = GetSourceVerses(text_mode, find_scope, current_selection, previous_result);
        }
        return DoFindVerseRanges(text_mode, source, verse_count, word_count, letter_count, unique_letter_count, value,
                            verse_count_number_type, word_count_number_type, letter_count_number_type, unique_letter_count_number_type, value_number_type,
                            verse_count_operator, word_count_operator, letter_count_operator, unique_letter_count_operator, value_operator, numerology_system_name,
                            add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);
    }
    private static List<List<Verse>> DoFindVerseRanges(string text_mode, List<Verse> source,
        int verse_count, int word_count, int letter_count, int unique_letter_count, long value,
        NumberType verse_count_number_type, NumberType word_count_number_type, NumberType letter_count_number_type, NumberType unique_letter_count_number_type, NumberType value_number_type,
        ComparisonOperator verse_count_operator, ComparisonOperator word_count_operator, ComparisonOperator letter_count_operator, ComparisonOperator unique_letter_count_operator, ComparisonOperator value_operator, string numerology_system_name,
        bool add_to_letter_l_number, bool add_to_letter_w_number, bool add_to_letter_v_number, bool add_to_letter_c_number, bool add_to_letter_l_distance, bool add_to_letter_w_distance, bool add_to_letter_v_distance, bool add_to_letter_c_distance, bool add_to_word_w_number, bool add_to_word_v_number, bool add_to_word_c_number, bool add_to_word_w_distance, bool add_to_word_v_distance, bool add_to_word_c_distance, bool add_to_verse_v_number, bool add_to_verse_c_number, bool add_to_verse_v_distance, bool add_to_verse_c_distance, bool add_to_chapter_c_number)
    {
        List<List<Verse>> result = null;
        if (source != null)
        {
            if (source.Count > 0)
            {
                if (word_count_number_type == NumberType.None)
                {
                    if (word_count > 0)
                    {
                        if (result == null) // if this is the first condition
                        {
                            result = new List<List<Verse>>();

                            for (int i = 0; i < source.Count; i++)
                            {
                                List<Verse> verse_range = new List<Verse>();
                                verse_range.Add(source[i]);

                                long total = source[i].Words.Count;
                                for (int j = i + 1; j < source.Count; j++)
                                {
                                    total += source[j].Words.Count;
                                    if (total > word_count) // limit exceeded
                                    {
                                        break; // and try next starting verse
                                    }

                                    verse_range.Add(source[j]);
                                    if (verse_count > 0) // verse_count is specified
                                    {
                                        if (verse_range.Count == verse_count)
                                        {
                                            if (Numbers.Compare(total, word_count, word_count_operator))
                                            {
                                                result.Add(verse_range);
                                                break; // and try next starting verse
                                            }
                                        }

                                        if (verse_range.Count > verse_count) // limit exceeded
                                        {
                                            break; // and try next starting verse
                                        }
                                    }
                                    else // no specified verse_count
                                    {
                                        if (Numbers.Compare(total, word_count, word_count_operator))
                                        {
                                            result.Add(verse_range);
                                            break; // and try next starting verse
                                        }
                                    }
                                }
                            }
                        }
                        else // filter existing result
                        {
                            for (int i = 0; i < result.Count; i++)
                            {
                                long total = 0;
                                foreach (Verse verse in result[i])
                                {
                                    total += verse.Words.Count;
                                }

                                if (!Numbers.Compare(total, word_count, word_count_operator))
                                {
                                    result.Remove(result[i]);
                                    i--; // self-healing
                                }
                            }
                        }
                    }
                }
                else //if (word_count_number_type != NumberType.None)
                {
                    if (result == null) // if this is the first condition
                    {
                        result = new List<List<Verse>>();

                        for (int i = 0; i < source.Count; i++)
                        {
                            List<Verse> verse_range = new List<Verse>();
                            verse_range.Add(source[i]);

                            long total = source[i].Words.Count;
                            for (int j = i + 1; j < source.Count; j++)
                            {
                                total += source[j].Words.Count;
                                verse_range.Add(source[j]);
                                if (verse_count > 0) // verse_count is specified
                                {
                                    if (verse_range.Count == verse_count)
                                    {
                                        if (Numbers.IsNumberType(total, word_count_number_type)) // match found
                                        {
                                            result.Add(verse_range);
                                            break; // and try next starting verse
                                        }
                                    }

                                    if (verse_range.Count > verse_count) // limit exceeded
                                    {
                                        break; // and try next starting verse
                                    }
                                }
                                else // no specified verse_count
                                {
                                    if (Numbers.IsNumberType(total, word_count_number_type)) // match found
                                    {
                                        result.Add(verse_range);
                                        break; // and try next starting verse
                                    }
                                }
                            }
                        }
                    }
                    else // filter existing result
                    {
                        for (int i = 0; i < result.Count; i++)
                        {
                            long total = 0;
                            foreach (Verse verse in result[i])
                            {
                                total += verse.Words.Count;
                            }

                            if (!Numbers.IsNumberType(total, word_count_number_type)) // not a match
                            {
                                result.Remove(result[i]);
                                i--; // self-healing
                            }
                        }
                    }
                }


                if (letter_count_number_type == NumberType.None)
                {
                    if (letter_count > 0)
                    {
                        if (result == null) // if this is the first condition
                        {
                            result = new List<List<Verse>>();

                            for (int i = 0; i < source.Count; i++)
                            {
                                List<Verse> verse_range = new List<Verse>();
                                verse_range.Add(source[i]);

                                long total = source[i].LetterCount;
                                for (int j = i + 1; j < source.Count; j++)
                                {
                                    total += source[j].LetterCount;
                                    if (total > letter_count) // limit exceeded
                                    {
                                        break; // and try next starting verse
                                    }

                                    verse_range.Add(source[j]);
                                    if (verse_count > 0) // verse_count is specified
                                    {
                                        if (verse_range.Count == verse_count)
                                        {
                                            if (Numbers.Compare(total, letter_count, letter_count_operator))
                                            {
                                                result.Add(verse_range);
                                                break; // and try next starting verse
                                            }
                                        }

                                        if (verse_range.Count > verse_count) // limit exceeded
                                        {
                                            break; // and try next starting verse
                                        }
                                    }
                                    else // no specified verse_count
                                    {
                                        if (Numbers.Compare(total, letter_count, letter_count_operator))
                                        {
                                            result.Add(verse_range);
                                            break; // and try next starting verse
                                        }
                                    }
                                }
                            }
                        }
                        else // filter existing result
                        {
                            for (int i = 0; i < result.Count; i++)
                            {
                                long total = 0;
                                foreach (Verse verse in result[i])
                                {
                                    total += verse.LetterCount;
                                }

                                if (!Numbers.Compare(total, letter_count, letter_count_operator))
                                {
                                    result.Remove(result[i]);
                                    i--; // self-healing
                                }
                            }
                        }
                    }
                }
                else //if (letter_count_number_type != NumberType.None)
                {
                    if (result == null) // if this is the first condition
                    {
                        result = new List<List<Verse>>();

                        for (int i = 0; i < source.Count; i++)
                        {
                            List<Verse> verse_range = new List<Verse>();
                            verse_range.Add(source[i]);

                            long total = source[i].LetterCount;
                            for (int j = i + 1; j < source.Count; j++)
                            {
                                total += source[j].LetterCount;
                                verse_range.Add(source[j]);
                                if (verse_count > 0) // verse_count is specified
                                {
                                    if (verse_range.Count == verse_count)
                                    {
                                        if (Numbers.IsNumberType(total, letter_count_number_type)) // match found
                                        {
                                            result.Add(verse_range);
                                            break; // and try next starting verse
                                        }
                                    }

                                    if (verse_range.Count > verse_count) // limit exceeded
                                    {
                                        break; // and try next starting verse
                                    }
                                }
                                else // no specified verse_count
                                {
                                    if (Numbers.IsNumberType(total, letter_count_number_type)) // match found
                                    {
                                        result.Add(verse_range);
                                        break; // and try next starting verse
                                    }
                                }
                            }
                        }
                    }
                    else // filter existing result
                    {
                        for (int i = 0; i < result.Count; i++)
                        {
                            long total = 0;
                            foreach (Verse verse in result[i])
                            {
                                total += verse.LetterCount;
                            }

                            if (!Numbers.IsNumberType(total, letter_count_number_type)) // not a match
                            {
                                result.Remove(result[i]);
                                i--; // self-healing
                            }
                        }
                    }
                }

                if (unique_letter_count_number_type == NumberType.None)
                {
                    if (unique_letter_count > 0)
                    {
                        List<char> unique_letters = new List<char>();

                        if (result == null) // if this is the first condition
                        {
                            result = new List<List<Verse>>();

                            for (int i = 0; i < source.Count; i++)
                            {
                                List<Verse> verse_range = new List<Verse>();
                                verse_range.Add(source[i]);

                                unique_letters.Clear();
                                long total = AddToUniqueLetters(ref unique_letters, source[i].GetText(text_mode));
                                for (int j = i + 1; j < source.Count; j++)
                                {
                                    total += AddToUniqueLetters(ref unique_letters, source[j].GetText(text_mode));
                                    if (total > unique_letter_count) // limit exceeded
                                    {
                                        break; // and try next starting verse
                                    }

                                    verse_range.Add(source[j]);
                                    if (verse_count > 0) // verse_count is specified
                                    {
                                        if (verse_range.Count == verse_count)
                                        {
                                            if (Numbers.Compare(total, unique_letter_count, unique_letter_count_operator))
                                            {
                                                result.Add(verse_range);
                                                break; // and try next starting verse
                                            }
                                        }

                                        if (verse_range.Count > verse_count) // limit exceeded
                                        {
                                            break; // and try next starting verse
                                        }
                                    }
                                    else // no specified verse_count
                                    {
                                        if (Numbers.Compare(total, unique_letter_count, unique_letter_count_operator))
                                        {
                                            result.Add(verse_range);
                                            break; // and try next starting verse
                                        }
                                    }
                                }
                            }
                        }
                        else // filter existing result
                        {
                            for (int i = 0; i < result.Count; i++)
                            {
                                long total = 0;
                                unique_letters.Clear();
                                foreach (Verse verse in result[i])
                                {
                                    total += AddToUniqueLetters(ref unique_letters, verse.GetText(text_mode));
                                }

                                if (!Numbers.Compare(total, unique_letter_count, unique_letter_count_operator))
                                {
                                    result.Remove(result[i]);
                                    i--; // self-healing
                                }
                            }
                        }
                    }
                }
                else //if (unique_letter_count_number_type != NumberType.None)
                {
                    List<char> unique_letters = new List<char>();
                    if (result == null) // if this is the first condition
                    {
                        result = new List<List<Verse>>();

                        for (int i = 0; i < source.Count; i++)
                        {
                            List<Verse> verse_range = new List<Verse>();
                            verse_range.Add(source[i]);

                            unique_letters.Clear();
                            long total = AddToUniqueLetters(ref unique_letters, source[i].GetText(text_mode));
                            for (int j = i + 1; j < source.Count; j++)
                            {
                                total += AddToUniqueLetters(ref unique_letters, source[j].GetText(text_mode));
                                verse_range.Add(source[j]);
                                if (verse_count > 0) // verse_count is specified
                                {
                                    if (verse_range.Count == verse_count)
                                    {
                                        if (Numbers.IsNumberType(total, unique_letter_count_number_type)) // match found
                                        {
                                            result.Add(verse_range);
                                            break; // and try next starting verse
                                        }
                                    }

                                    if (verse_range.Count > verse_count) // limit exceeded
                                    {
                                        break; // and try next starting verse
                                    }
                                }
                                else // no specified verse_count
                                {
                                    if (Numbers.IsNumberType(total, unique_letter_count_number_type)) // match found
                                    {
                                        result.Add(verse_range);
                                        break; // and try next starting verse
                                    }
                                }
                            }
                        }
                    }
                    else // filter existing result
                    {
                        for (int i = 0; i < result.Count; i++)
                        {
                            long total = 0;
                            unique_letters.Clear();
                            foreach (Verse verse in result[i])
                            {
                                total += AddToUniqueLetters(ref unique_letters, verse.GetText(text_mode));
                            }

                            if (!Numbers.IsNumberType(total, unique_letter_count_number_type)) // not a match
                            {
                                result.Remove(result[i]);
                                i--; // self-healing
                            }
                        }
                    }
                }

                if (value_number_type == NumberType.None)
                {
                    if (value > 0)
                    {
                        if (result == null) // if this is the first condition
                        {
                            result = new List<List<Verse>>();

                            for (int i = 0; i < source.Count; i++)
                            {
                                List<Verse> verse_range = new List<Verse>();
                                verse_range.Add(source[i]);

                                long total = CalculateValue(source[i], numerology_system_name, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);
                                for (int j = i + 1; j < source.Count; j++)
                                {
                                    total += CalculateValue(source[j], numerology_system_name, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);
                                    if (total > value) // limit exceeded
                                    {
                                        break; // and try next starting verse
                                    }

                                    verse_range.Add(source[j]);
                                    if (verse_count > 0) // verse_count is specified
                                    {
                                        if (verse_range.Count == verse_count)
                                        {
                                            if (Numbers.Compare(total, value, value_operator))
                                            {
                                                result.Add(verse_range);
                                                break; // and try next starting verse
                                            }
                                        }

                                        if (verse_range.Count > verse_count) // limit exceeded
                                        {
                                            break; // and try next starting verse
                                        }
                                    }
                                    else // no specified verse_count
                                    {
                                        if (Numbers.Compare(total, value, value_operator))
                                        {
                                            result.Add(verse_range);
                                            break; // and try next starting verse
                                        }
                                    }
                                }
                            }
                        }
                        else // filter existing result
                        {
                            for (int i = 0; i < result.Count; i++)
                            {
                                long total = 0;
                                foreach (Verse verse in result[i])
                                {
                                    total += CalculateValue(verse, numerology_system_name, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);
                                }

                                if (!Numbers.Compare(total, value, value_operator))
                                {
                                    result.Remove(result[i]);
                                    i--; // self-healing
                                }
                            }
                        }
                    }
                }
                else //if (value_number_type != NumberType.None)
                {
                    if (result == null) // if this is the first condition
                    {
                        result = new List<List<Verse>>();

                        for (int i = 0; i < source.Count; i++)
                        {
                            List<Verse> verse_range = new List<Verse>();
                            verse_range.Add(source[i]);

                            long total = CalculateValue(source[i], numerology_system_name, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);
                            for (int j = i + 1; j < source.Count; j++)
                            {
                                total += CalculateValue(source[j], numerology_system_name, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);
                                verse_range.Add(source[j]);
                                if (verse_count > 0) // verse_count is specified
                                {
                                    if (verse_range.Count == verse_count)
                                    {
                                        if (Numbers.IsNumberType(total, value_number_type)) // match found
                                        {
                                            result.Add(verse_range);
                                            break; // and try next starting verse
                                        }
                                    }

                                    if (verse_range.Count > verse_count) // limit exceeded
                                    {
                                        break; // and try next starting verse
                                    }
                                }
                                else // no specified verse_count
                                {
                                    if (Numbers.IsNumberType(total, value_number_type)) // match found
                                    {
                                        result.Add(verse_range);
                                        break; // and try next starting verse
                                    }
                                }
                            }
                        }
                    }
                    else // filter existing result
                    {
                        for (int i = 0; i < result.Count; i++)
                        {
                            long total = 0;
                            foreach (Verse verse in result[i])
                            {
                                total += CalculateValue(verse, numerology_system_name, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);
                            }

                            if (!Numbers.IsNumberType(total, value_number_type)) // not a match
                            {
                                result.Remove(result[i]);
                                i--; // self-healing
                            }
                        }
                    }
                }
            }
        }
        return result;
    }

    // find by numbers Chapters
    public static List<Chapter> FindChapters(string text_mode, FindScope find_scope, Selection current_selection, List<Verse> previous_result,
        int number, int verse_count, int word_count, int letter_count, int unique_letter_count, long value,
        NumberType number_number_type, NumberType verse_count_number_type, NumberType word_count_number_type, NumberType letter_count_number_type, NumberType unique_letter_count_number_type, NumberType value_number_type,
        ComparisonOperator number_operator, ComparisonOperator verse_count_operator, ComparisonOperator word_count_operator, ComparisonOperator letter_count_operator, ComparisonOperator unique_letter_count_operator, ComparisonOperator value_operator, string numerology_system_name,
        bool add_to_letter_l_number, bool add_to_letter_w_number, bool add_to_letter_v_number, bool add_to_letter_c_number, bool add_to_letter_l_distance, bool add_to_letter_w_distance, bool add_to_letter_v_distance, bool add_to_letter_c_distance, bool add_to_word_w_number, bool add_to_word_v_number, bool add_to_word_c_number, bool add_to_word_w_distance, bool add_to_word_v_distance, bool add_to_word_c_distance, bool add_to_verse_v_number, bool add_to_verse_c_number, bool add_to_verse_v_distance, bool add_to_verse_c_distance, bool add_to_chapter_c_number)
    {
        return DoFindChapters(text_mode, find_scope, current_selection, previous_result,
                            number, verse_count, word_count, letter_count, unique_letter_count, value,
                            number_number_type, verse_count_number_type, word_count_number_type, letter_count_number_type, unique_letter_count_number_type, value_number_type,
                            number_operator, verse_count_operator, word_count_operator, letter_count_operator, unique_letter_count_operator, value_operator, numerology_system_name,
                            add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);
    }
    private static List<Chapter> DoFindChapters(string text_mode, FindScope find_scope, Selection current_selection, List<Verse> previous_result,
        int number, int verse_count, int word_count, int letter_count, int unique_letter_count, long value,
        NumberType number_number_type, NumberType verse_count_number_type, NumberType word_count_number_type, NumberType letter_count_number_type, NumberType unique_letter_count_number_type, NumberType value_number_type,
        ComparisonOperator number_operator, ComparisonOperator verse_count_operator, ComparisonOperator word_count_operator, ComparisonOperator letter_count_operator, ComparisonOperator unique_letter_count_operator, ComparisonOperator value_operator, string numerology_system_name,
        bool add_to_letter_l_number, bool add_to_letter_w_number, bool add_to_letter_v_number, bool add_to_letter_c_number, bool add_to_letter_l_distance, bool add_to_letter_w_distance, bool add_to_letter_v_distance, bool add_to_letter_c_distance, bool add_to_word_w_number, bool add_to_word_v_number, bool add_to_word_c_number, bool add_to_word_w_distance, bool add_to_word_v_distance, bool add_to_word_c_distance, bool add_to_verse_v_number, bool add_to_verse_c_number, bool add_to_verse_v_distance, bool add_to_verse_c_distance, bool add_to_chapter_c_number)
    {
        List<Verse> source = null;
        Book book = Book.Instance;
        if (book != null)
        {
            source = GetSourceVerses(text_mode, find_scope, current_selection, previous_result);
        }
        return DoFindChapters(text_mode, source,
                            number, verse_count, word_count, letter_count, unique_letter_count, value,
                            number_number_type, verse_count_number_type, word_count_number_type, letter_count_number_type, unique_letter_count_number_type, value_number_type,
                            number_operator, verse_count_operator, word_count_operator, letter_count_operator, unique_letter_count_operator, value_operator, numerology_system_name,
                            add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);
    }
    private static List<Chapter> DoFindChapters(string text_mode, List<Verse> source,
        int number, int verse_count, int word_count, int letter_count, int unique_letter_count, long value,
        NumberType number_number_type, NumberType verse_count_number_type, NumberType word_count_number_type, NumberType letter_count_number_type, NumberType unique_letter_count_number_type, NumberType value_number_type,
        ComparisonOperator number_operator, ComparisonOperator verse_count_operator, ComparisonOperator word_count_operator, ComparisonOperator letter_count_operator, ComparisonOperator unique_letter_count_operator, ComparisonOperator value_operator, string numerology_system_name,
        bool add_to_letter_l_number, bool add_to_letter_w_number, bool add_to_letter_v_number, bool add_to_letter_c_number, bool add_to_letter_l_distance, bool add_to_letter_w_distance, bool add_to_letter_v_distance, bool add_to_letter_c_distance, bool add_to_word_w_number, bool add_to_word_v_number, bool add_to_word_c_number, bool add_to_word_w_distance, bool add_to_word_v_distance, bool add_to_word_c_distance, bool add_to_verse_v_number, bool add_to_verse_c_number, bool add_to_verse_v_distance, bool add_to_verse_c_distance, bool add_to_chapter_c_number)
    {
        List<Chapter> result = new List<Chapter>();
        List<Verse> current_source = new List<Verse>(source); // fill it up, then filter out condition by condition
        List<Verse> temp = new List<Verse>();
        if (source != null)
        {
            if (source.Count > 0)
            {
                if (number > 0)
                {
                    temp.Clear();
                    foreach (Verse verse in current_source)
                    {
                        if (verse.Chapter != null)
                        {
                            if (Numbers.Compare(verse.Chapter.Number, number, number_operator))
                            {
                                temp.Add(verse);
                            }
                        }
                    }
                    current_source.Clear();
                    current_source.AddRange(temp);
                }

                if (verse_count > 0)
                {
                    temp.Clear();
                    foreach (Verse verse in current_source)
                    {
                        if (verse.Chapter != null)
                        {
                            if (Numbers.Compare(verse.Chapter.Verses.Count, verse_count, verse_count_operator))
                            {
                                temp.Add(verse);
                            }
                        }
                    }
                    current_source.Clear();
                    current_source.AddRange(temp);
                }

                if (word_count > 0)
                {
                    temp.Clear();
                    foreach (Verse verse in current_source)
                    {
                        if (verse.Chapter != null)
                        {
                            if (Numbers.Compare(verse.Chapter.WordCount, word_count, word_count_operator))
                            {
                                temp.Add(verse);
                            }
                        }
                    }
                    current_source.Clear();
                    current_source.AddRange(temp);
                }

                if (letter_count > 0)
                {
                    temp.Clear();
                    foreach (Verse verse in current_source)
                    {
                        if (verse.Chapter != null)
                        {
                            if (Numbers.Compare(verse.Chapter.LetterCount, letter_count, letter_count_operator))
                            {
                                temp.Add(verse);
                            }
                        }
                    }
                    current_source.Clear();
                    current_source.AddRange(temp);
                }

                if (unique_letter_count > 0)
                {
                    temp.Clear();
                    foreach (Verse verse in current_source)
                    {
                        if (verse.Chapter != null)
                        {
                            if (Numbers.Compare(verse.Chapter.UniqueLetters.Count, unique_letter_count, unique_letter_count_operator))
                            {
                                temp.Add(verse);
                            }
                        }
                    }
                    current_source.Clear();
                    current_source.AddRange(temp);
                }

                if (value > 0)
                {
                    temp.Clear();
                    foreach (Verse verse in current_source)
                    {
                        if (verse.Chapter != null)
                        {
                            if (Numbers.Compare(CalculateValue(verse.Chapter.Verses, numerology_system_name, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number), value, value_operator))
                            {
                                temp.Add(verse);
                            }
                        }
                    }
                    current_source.Clear();
                    current_source.AddRange(temp);
                }


                if (number_number_type != NumberType.None)
                {
                    if (number_number_type != NumberType.Any)
                    {
                        temp.Clear();
                        foreach (Verse verse in current_source)
                        {
                            if (verse.Chapter != null)
                            {
                                if (Numbers.IsNumberType(verse.Chapter.Number, number_number_type))
                                {
                                    temp.Add(verse);
                                }
                            }
                        }
                    }
                    else
                    {
                        // group chapters by number
                        // nothing to do as chapters have unique numbers
                    }
                    current_source.Clear();
                    current_source.AddRange(temp);
                }

                if (verse_count_number_type != NumberType.None)
                {
                    if (verse_count_number_type != NumberType.Any)
                    {
                        temp.Clear();
                        foreach (Verse verse in current_source)
                        {
                            if (verse.Chapter != null)
                            {
                                if (Numbers.IsNumberType(verse.Chapter.Verses.Count, verse_count_number_type))
                                {
                                    temp.Add(verse);
                                }
                            }
                        }
                    }
                    else
                    {
                        // group chapters by verse_count
                        List<Chapter> chapters = Book.GetChapters(current_source);
                        bool[] processed = new bool[chapters.Count];

                        temp.Clear();
                        for (int i = 0; i < chapters.Count - 1; i++)
                        {
                            if (processed[i]) continue;

                            bool chapter_i_added = false;
                            for (int j = i + 1; j < chapters.Count; j++)
                            {
                                if (processed[j]) continue;

                                if (chapters[i].Verses.Count == chapters[j].Verses.Count)
                                {
                                    if (!chapter_i_added)
                                    {
                                        temp.AddRange(chapters[i].Verses);
                                        chapter_i_added = true;
                                        processed[i] = true;
                                    }

                                    temp.AddRange(chapters[j].Verses);
                                    processed[j] = true;
                                }
                            }
                        }
                    }
                    current_source.Clear();
                    current_source.AddRange(temp);
                }

                if (word_count_number_type != NumberType.None)
                {
                    if (word_count_number_type != NumberType.Any)
                    {
                        temp.Clear();
                        foreach (Verse verse in current_source)
                        {
                            if (verse.Chapter != null)
                            {
                                if (Numbers.IsNumberType(verse.Chapter.WordCount, word_count_number_type))
                                {
                                    temp.Add(verse);
                                }
                            }
                        }
                    }
                    else
                    {
                        // group chapters by word_count
                        List<Chapter> chapters = Book.GetChapters(current_source);
                        bool[] processed = new bool[chapters.Count];

                        temp.Clear();
                        for (int i = 0; i < chapters.Count - 1; i++)
                        {
                            if (processed[i]) continue;

                            bool chapter_i_added = false;
                            for (int j = i + 1; j < chapters.Count; j++)
                            {
                                if (processed[j]) continue;

                                if (chapters[i].WordCount == chapters[j].WordCount)
                                {
                                    if (!chapter_i_added)
                                    {
                                        temp.AddRange(chapters[i].Verses);
                                        chapter_i_added = true;
                                        processed[i] = true;
                                    }

                                    temp.AddRange(chapters[j].Verses);
                                    processed[j] = true;
                                }
                            }
                        }
                    }
                    current_source.Clear();
                    current_source.AddRange(temp);
                }

                if (letter_count_number_type != NumberType.None)
                {
                    if (letter_count_number_type != NumberType.Any)
                    {
                        temp.Clear();
                        foreach (Verse verse in current_source)
                        {
                            if (verse.Chapter != null)
                            {
                                if (Numbers.IsNumberType(verse.Chapter.LetterCount, letter_count_number_type))
                                {
                                    temp.Add(verse);
                                }
                            }
                        }
                    }
                    else
                    {
                        // group chapters by letter_count
                        List<Chapter> chapters = Book.GetChapters(current_source);
                        bool[] processed = new bool[chapters.Count];

                        temp.Clear();
                        for (int i = 0; i < chapters.Count - 1; i++)
                        {
                            if (processed[i]) continue;

                            bool chapter_i_added = false;
                            for (int j = i + 1; j < chapters.Count; j++)
                            {
                                if (processed[j]) continue;

                                if (chapters[i].LetterCount == chapters[j].LetterCount)
                                {
                                    if (!chapter_i_added)
                                    {
                                        temp.AddRange(chapters[i].Verses);
                                        chapter_i_added = true;
                                        processed[i] = true;
                                    }

                                    temp.AddRange(chapters[j].Verses);
                                    processed[j] = true;
                                }
                            }
                        }
                    }
                    current_source.Clear();
                    current_source.AddRange(temp);
                }

                if (unique_letter_count_number_type != NumberType.None)
                {
                    if (unique_letter_count_number_type != NumberType.Any)
                    {
                        temp.Clear();
                        foreach (Verse verse in current_source)
                        {
                            if (verse.Chapter != null)
                            {
                                if (Numbers.IsNumberType(verse.Chapter.UniqueLetters.Count, unique_letter_count_number_type))
                                {
                                    temp.Add(verse);
                                }
                            }
                        }
                    }
                    else
                    {
                        // group chapters by unique_letter_count
                        List<Chapter> chapters = Book.GetChapters(current_source);
                        bool[] processed = new bool[chapters.Count];

                        temp.Clear();
                        for (int i = 0; i < chapters.Count - 1; i++)
                        {
                            if (processed[i]) continue;

                            bool chapter_i_added = false;
                            for (int j = i + 1; j < chapters.Count; j++)
                            {
                                if (processed[j]) continue;

                                if (chapters[i].UniqueLetters.Count == chapters[j].UniqueLetters.Count)
                                {
                                    if (!chapter_i_added)
                                    {
                                        temp.AddRange(chapters[i].Verses);
                                        chapter_i_added = true;
                                        processed[i] = true;
                                    }

                                    temp.AddRange(chapters[j].Verses);
                                    processed[j] = true;
                                }
                            }
                        }
                    }
                    current_source.Clear();
                    current_source.AddRange(temp);
                }

                if (value_number_type != NumberType.None)
                {
                    if (value_number_type != NumberType.Any)
                    {
                        temp.Clear();
                        foreach (Verse verse in current_source)
                        {
                            if (verse.Chapter != null)
                            {
                                if (Numbers.IsNumberType(CalculateValue(verse.Chapter.Verses, numerology_system_name, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number), value_number_type))
                                {
                                    temp.Add(verse);
                                }
                            }
                        }
                    }
                    else
                    {
                        // group chapters by value
                        List<Chapter> chapters = Book.GetChapters(current_source);
                        bool[] processed = new bool[chapters.Count];

                        temp.Clear();
                        for (int i = 0; i < chapters.Count - 1; i++)
                        {
                            if (processed[i]) continue;
                            long chapter_i_value = CalculateValue(chapters[i].Verses, numerology_system_name, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);

                            bool chapter_i_added = false;
                            for (int j = i + 1; j < chapters.Count; j++)
                            {
                                if (processed[j]) continue;
                                long chapter_j_value = CalculateValue(chapters[j].Verses, numerology_system_name, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);

                                if (chapter_i_value == chapter_j_value)
                                {
                                    if (!chapter_i_added)
                                    {
                                        temp.AddRange(chapters[i].Verses);
                                        chapter_i_added = true;
                                        processed[i] = true;
                                    }

                                    temp.AddRange(chapters[j].Verses);
                                    processed[j] = true;
                                }
                            }
                        }
                    }
                    current_source.Clear();
                    current_source.AddRange(temp);
                }
            }
        }

        // fill result
        int current_chapter_number = -1;
        foreach (Verse verse in current_source)
        {
            if (verse.Chapter != null)
            {
                if (current_chapter_number != verse.Chapter.Number)
                {
                    current_chapter_number = verse.Chapter.Number;
                    result.Add(verse.Chapter);
                }
            }
        }
        return result;
    }

    // find by numbers ChaptersWithAllVersesHaving
    public static List<Chapter> FindChaptersWithAllVersesHaving(string text_mode, FindScope find_scope, Selection current_selection, List<Verse> previous_result,
        int word_count, int letter_count, int unique_letter_count, long value,
        NumberType word_count_number_type, NumberType letter_count_number_type, NumberType unique_letter_count_number_type, NumberType value_number_type,
        ComparisonOperator word_count_operator, ComparisonOperator letter_count_operator, ComparisonOperator unique_letter_count_operator, ComparisonOperator value_operator, string numerology_system_name,
        bool add_to_letter_l_number, bool add_to_letter_w_number, bool add_to_letter_v_number, bool add_to_letter_c_number, bool add_to_letter_l_distance, bool add_to_letter_w_distance, bool add_to_letter_v_distance, bool add_to_letter_c_distance, bool add_to_word_w_number, bool add_to_word_v_number, bool add_to_word_c_number, bool add_to_word_w_distance, bool add_to_word_v_distance, bool add_to_word_c_distance, bool add_to_verse_v_number, bool add_to_verse_c_number, bool add_to_verse_v_distance, bool add_to_verse_c_distance, bool add_to_chapter_c_number)
    {
        return DoFindChaptersWithAllVersesHaving(text_mode, find_scope, current_selection, previous_result, word_count, letter_count, unique_letter_count, value,
                            word_count_number_type, letter_count_number_type, unique_letter_count_number_type, value_number_type,
                            word_count_operator, letter_count_operator, unique_letter_count_operator, value_operator, numerology_system_name,
                            add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);
    }
    private static List<Chapter> DoFindChaptersWithAllVersesHaving(string text_mode, FindScope find_scope, Selection current_selection, List<Verse> previous_result,
        int word_count, int letter_count, int unique_letter_count, long value,
        NumberType word_count_number_type, NumberType letter_count_number_type, NumberType unique_letter_count_number_type, NumberType value_number_type,
        ComparisonOperator word_count_operator, ComparisonOperator letter_count_operator, ComparisonOperator unique_letter_count_operator, ComparisonOperator value_operator, string numerology_system_name,
        bool add_to_letter_l_number, bool add_to_letter_w_number, bool add_to_letter_v_number, bool add_to_letter_c_number, bool add_to_letter_l_distance, bool add_to_letter_w_distance, bool add_to_letter_v_distance, bool add_to_letter_c_distance, bool add_to_word_w_number, bool add_to_word_v_number, bool add_to_word_c_number, bool add_to_word_w_distance, bool add_to_word_v_distance, bool add_to_word_c_distance, bool add_to_verse_v_number, bool add_to_verse_c_number, bool add_to_verse_v_distance, bool add_to_verse_c_distance, bool add_to_chapter_c_number)
    {
        List<Verse> source = null;
        Book book = Book.Instance;
        if (book != null)
        {
            source = GetSourceVerses(text_mode, find_scope, current_selection, previous_result);
        }
        return DoFindChaptersWithAllVersesHaving(text_mode, source, word_count, letter_count, unique_letter_count, value,
                            word_count_number_type, letter_count_number_type, unique_letter_count_number_type, value_number_type,
                            word_count_operator, letter_count_operator, unique_letter_count_operator, value_operator, numerology_system_name,
                            add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);
    }
    private static List<Chapter> DoFindChaptersWithAllVersesHaving(string text_mode, List<Verse> source,
        int word_count, int letter_count, int unique_letter_count, long value,
        NumberType word_count_number_type, NumberType letter_count_number_type, NumberType unique_letter_count_number_type, NumberType value_number_type,
        ComparisonOperator word_count_operator, ComparisonOperator letter_count_operator, ComparisonOperator unique_letter_count_operator, ComparisonOperator value_operator, string numerology_system_name,
        bool add_to_letter_l_number, bool add_to_letter_w_number, bool add_to_letter_v_number, bool add_to_letter_c_number, bool add_to_letter_l_distance, bool add_to_letter_w_distance, bool add_to_letter_v_distance, bool add_to_letter_c_distance, bool add_to_word_w_number, bool add_to_word_v_number, bool add_to_word_c_number, bool add_to_word_w_distance, bool add_to_word_v_distance, bool add_to_word_c_distance, bool add_to_verse_v_number, bool add_to_verse_c_number, bool add_to_verse_v_distance, bool add_to_verse_c_distance, bool add_to_chapter_c_number)
    {
        List<Chapter> result = new List<Chapter>();
        if (source != null)
        {
            if (source.Count > 0)
            {
                List<Verse> found_verses = new List<Verse>();
                foreach (Verse verse in source)
                {
                    if (IsVerseMatching(verse,
                                        word_count, letter_count, unique_letter_count, value,
                                        word_count_number_type, letter_count_number_type, unique_letter_count_number_type, value_number_type,
                                        word_count_operator, letter_count_operator, unique_letter_count_operator, value_operator, numerology_system_name,
                            add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number)
                        )
                    {
                        found_verses.Add(verse);
                    }
                }

                // fill found_chapters
                int current_chapter_number = -1;
                int current_verse_count = 0;
                foreach (Verse verse in found_verses)
                {
                    if (verse.Chapter != null)
                    {
                        // mark new chapter beginning
                        if (current_chapter_number != verse.Chapter.Number) // new chapter
                        {
                            current_chapter_number = verse.Chapter.Number; // backup
                            current_verse_count = 0;
                        }
                        current_verse_count++;

                        // add chapter if all its verses match criteria
                        if (current_verse_count == verse.Chapter.Verses.Count)
                        {
                            result.Add(verse.Chapter);
                        }
                    }
                }
            }
        }
        return result;
    }
    private static bool IsVerseMatching(Verse verse,
        int word_count, int letter_count, int unique_letter_count, long value,
        NumberType word_count_number_type, NumberType letter_count_number_type, NumberType unique_letter_count_number_type, NumberType value_number_type,
        ComparisonOperator word_count_operator, ComparisonOperator letter_count_operator, ComparisonOperator unique_letter_count_operator, ComparisonOperator value_operator, string numerology_system_name,
        bool add_to_letter_l_number, bool add_to_letter_w_number, bool add_to_letter_v_number, bool add_to_letter_c_number, bool add_to_letter_l_distance, bool add_to_letter_w_distance, bool add_to_letter_v_distance, bool add_to_letter_c_distance, bool add_to_word_w_number, bool add_to_word_v_number, bool add_to_word_c_number, bool add_to_word_w_distance, bool add_to_word_v_distance, bool add_to_word_c_distance, bool add_to_verse_v_number, bool add_to_verse_c_number, bool add_to_verse_v_distance, bool add_to_verse_c_distance, bool add_to_chapter_c_number)
    {
        if (verse != null)
        {
            string[] parts = numerology_system_name.Split('_');
            string text_mode = parts[0];

            if (word_count > 0)
            {
                if (!Numbers.Compare(verse.Words.Count, word_count, word_count_operator))
                {
                    return false;
                }
            }

            if (letter_count > 0)
            {
                if (!Numbers.Compare(verse.LetterCount, letter_count, letter_count_operator))
                {
                    return false;
                }
            }

            if (unique_letter_count > 0)
            {
                if (!Numbers.Compare(verse.UniqueLetters.Count, unique_letter_count, unique_letter_count_operator))
                {
                    return false;
                }
            }

            if (value > 0)
            {
                if (!Numbers.Compare(CalculateValue(verse, numerology_system_name, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number), value, value_operator))
                {
                    return false;
                }
            }


            if (word_count_number_type != NumberType.None)
            {
                if (word_count_number_type != NumberType.Any)
                {
                    if (!Numbers.IsNumberType(verse.Words.Count, word_count_number_type))
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }

            if (letter_count_number_type != NumberType.None)
            {
                if (letter_count_number_type != NumberType.Any)
                {
                    if (!Numbers.IsNumberType(verse.LetterCount, letter_count_number_type))
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }

            if (unique_letter_count_number_type != NumberType.None)
            {
                if (unique_letter_count_number_type != NumberType.Any)
                {
                    if (!Numbers.IsNumberType(verse.UniqueLetters.Count, unique_letter_count_number_type))
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }

            if (value_number_type != NumberType.None)
            {
                if (value_number_type != NumberType.Any)
                {
                    if (!Numbers.IsNumberType(CalculateValue(verse, numerology_system_name, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number), value_number_type))
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
        }

        return true;
    }

    // find by prostration type
    public static List<Verse> FindVerses(string text_mode, FindScope find_scope, Selection current_selection, List<Verse> previous_result, FindByProstrationType prostration_type)
    {
        return DoFindVerses(text_mode, find_scope, current_selection, previous_result, prostration_type);
    }
    private static List<Verse> DoFindVerses(string text_mode, FindScope find_scope, Selection current_selection, List<Verse> previous_result, FindByProstrationType prostration_type)
    {
        List<Verse> source = null;
        Book book = Book.Instance;
        if (book != null)
        {
            source = GetSourceVerses(text_mode, find_scope, current_selection, previous_result);
        }
        return DoFindVerses(text_mode, source, prostration_type);
    }
    private static List<Verse> DoFindVerses(string text_mode, List<Verse> source, FindByProstrationType prostration_type)
    {
        List<Verse> result = new List<Verse>();
        if (source != null)
        {
            if (source.Count > 0)
            {
                try
                {
                    switch (prostration_type)
                    {
                        case FindByProstrationType.None:
                            {
                                // add nothing

                                //foreach (Verse verse in source)
                                //{
                                //    if (verse.Prostration == null)
                                //    {
                                //        s_found_verses.Add(verse);
                                //    }
                                //}
                            }
                            break;
                        case FindByProstrationType.Obligatory:
                            {
                                foreach (Verse verse in source)
                                {
                                    if (verse.Prostration != null)
                                    {
                                        if (verse.Prostration.Type == ProstrationType.Obligatory)
                                        {
                                            result.Add(verse);
                                        }
                                    }
                                }
                            }
                            break;
                        case FindByProstrationType.Recommended:
                            {
                                foreach (Verse verse in source)
                                {
                                    if (verse.Prostration != null)
                                    {
                                        if (verse.Prostration.Type == ProstrationType.Recommended)
                                        {
                                            result.Add(verse);
                                        }
                                    }
                                }
                            }
                            break;
                        case FindByProstrationType.Both:
                            {
                                foreach (Verse verse in source)
                                {
                                    if (verse.Prostration != null)
                                    {
                                        result.Add(verse);
                                    }
                                }
                            }
                            break;
                    }
                }
                catch
                {
                    return null;
                }
            }
        }
        return result;
    }

    // find by revelation place
    public static List<Chapter> FindChapters(string text_mode, FindScope find_scope, Selection current_selection, List<Verse> previous_result, FindByRevelationPlace revelation_place)
    {
        return DoFindChapters(text_mode, find_scope, current_selection, previous_result, revelation_place);
    }
    private static List<Chapter> DoFindChapters(string text_mode, FindScope find_scope, Selection current_selection, List<Verse> previous_result, FindByRevelationPlace revelation_place)
    {
        List<Verse> source = null;
        Book book = Book.Instance;
        if (book != null)
        {
            source = GetSourceVerses(text_mode, find_scope, current_selection, previous_result);
        }
        return DoFindChapters(text_mode, source, revelation_place);
    }
    private static List<Chapter> DoFindChapters(string text_mode, List<Verse> source, FindByRevelationPlace revelation_place)
    {
        List<Chapter> result = new List<Chapter>();
        List<Verse> found_verses = new List<Verse>();
        if (source != null)
        {
            if (source.Count > 0)
            {
                try
                {
                    foreach (Verse verse in source)
                    {
                        switch (revelation_place)
                        {
                            case FindByRevelationPlace.None:
                                {
                                    // add nothing
                                }
                                break;
                            case FindByRevelationPlace.Makkah:
                                {
                                    if (verse.Chapter != null)
                                    {
                                        if (verse.Chapter.RevelationPlace == RevelationPlace.Makkah)
                                        {
                                            found_verses.Add(verse);
                                        }
                                    }
                                }
                                break;
                            case FindByRevelationPlace.Medina:
                                {
                                    if (verse.Chapter != null)
                                    {
                                        if (verse.Chapter.RevelationPlace == RevelationPlace.Medina)
                                        {
                                            found_verses.Add(verse);
                                        }
                                    }
                                }
                                break;
                            case FindByRevelationPlace.Both:
                                {
                                    found_verses.Add(verse);
                                }
                                break;
                        }
                    }
                }
                catch
                {
                    return null;
                }
            }
        }

        int current_chapter_number = -1;
        foreach (Verse verse in found_verses)
        {
            if (verse.Chapter != null)
            {
                if (current_chapter_number != verse.Chapter.Number)
                {
                    current_chapter_number = verse.Chapter.Number;
                    result.Add(verse.Chapter);
                }
            }
        }
        return result;
    }

    // find by phrase letter frequency sum
    private static int CalculateLetterFrequencySum(string text, string phrase, FindByFrequencySumType sum_type)
    {
        int result = 0;

        if (sum_type == FindByFrequencySumType.NoDuplicateLetters)
        {
            phrase = phrase.RemoveDuplicates();
        }
        phrase = phrase.Replace("\r", "");
        phrase = phrase.Replace("\n", "");
        phrase = phrase.Replace(" ", "");

        for (int i = 0; i < phrase.Length; i++)
        {
            int frequency = 0;
            for (int j = 0; j < text.Length; j++)
            {
                if (Constants.ARABIC_DIGITS.Contains(text[j])) continue;
                if (Constants.STOP_MARKS.Contains(text[j])) continue;
                if (text[j] == '{') continue;
                if (text[j] == '}') continue;

                if (phrase[i] == text[j])
                {
                    frequency++;
                }
            }

            if (frequency > 0)
            {
                result += frequency;
            }
        }

        return result;
    }
    public static List<Verse> FindVerses(string text_mode, FindScope find_scope, Selection current_selection, List<Verse> previous_result, string phrase, int phrase_letter_frequency_sum, FindByFrequencySumType sum_type)
    {
        return DoFindVerses(text_mode, find_scope, current_selection, previous_result, phrase, phrase_letter_frequency_sum, sum_type);
    }
    private static List<Verse> DoFindVerses(string text_mode, FindScope find_scope, Selection current_selection, List<Verse> previous_result, string phrase, int phrase_letter_frequency_sum, FindByFrequencySumType sum_type)
    {
        List<Verse> source = null;
        Book book = Book.Instance;
        if (book != null)
        {
            source = GetSourceVerses(text_mode, find_scope, current_selection, previous_result);
        }
        return DoFindVerses(text_mode, source, phrase, phrase_letter_frequency_sum, sum_type);
    }
    private static List<Verse> DoFindVerses(string text_mode, List<Verse> source, string phrase, int phrase_letter_frequency_sum, FindByFrequencySumType sum_type)
    {
        List<Verse> result = new List<Verse>();
        if (source != null)
        {
            if (source.Count > 0)
            {
                if ((phrase.Length > 0) || (phrase_letter_frequency_sum > 0))
                {
                    foreach (Verse verse in source)
                    {
                        if (CalculateLetterFrequencySum(verse.GetText(text_mode), phrase, sum_type) == phrase_letter_frequency_sum)
                        {
                            result.Add(verse);
                        }
                    }
                }
            }
        }
        return result;
    }

    public static void GenerateFullBookStatistics(string numerology_system_name, bool add_to_letter_l_number, bool add_to_letter_w_number, bool add_to_letter_v_number, bool add_to_letter_c_number, bool add_to_letter_l_distance, bool add_to_letter_w_distance, bool add_to_letter_v_distance, bool add_to_letter_c_distance, bool add_to_word_w_number, bool add_to_word_v_number, bool add_to_word_c_number, bool add_to_word_w_distance, bool add_to_word_v_distance, bool add_to_word_c_distance, bool add_to_verse_v_number, bool add_to_verse_c_number, bool add_to_verse_v_distance, bool add_to_verse_c_distance, bool add_to_chapter_c_number)
    {
        GenerateBookStatistics(numerology_system_name, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);

        string[] parts = numerology_system_name.Split('_');
        string text_mode = parts[0];
        GenerateWordFrequency(text_mode);
        GenerateLetterFrequency(text_mode);
        GenerateWordLetterDigits(text_mode);
    }
    public static void GenerateBookStatistics(string numerology_system_name, bool add_to_letter_l_number, bool add_to_letter_w_number, bool add_to_letter_v_number, bool add_to_letter_c_number, bool add_to_letter_l_distance, bool add_to_letter_w_distance, bool add_to_letter_v_distance, bool add_to_letter_c_distance, bool add_to_word_w_number, bool add_to_word_v_number, bool add_to_word_c_number, bool add_to_word_w_distance, bool add_to_word_v_distance, bool add_to_word_c_distance, bool add_to_verse_v_number, bool add_to_verse_c_number, bool add_to_verse_v_distance, bool add_to_verse_c_distance, bool add_to_chapter_c_number)
    {
        string directory = "Statistics";
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string filename = directory + "/" + "Chapters_" + numerology_system_name + ".txt";

        string[] parts = numerology_system_name.Split('_');
        string text_mode = parts[0];

        Book book = Book.Instance;
        if (book != null)
        {
            try
            {
                if (s_numerology_systems != null)
                {
                    using (StreamWriter writer = new StreamWriter(filename, false, Encoding.Unicode))
                    {
                        writer.Write("Chapter" + "\t" + "#" + "\t" + "Verses" + "\t" + "Words" + "\t" + "Letters" + "\t" + "Unique" + "\t" + "Value" + "\t" + "P" + "\t" + "AP" + "\t" + "PP" + "\t" + "C" + "\t" + "AC" + "\t" + "PC" + "\t");
                        foreach (char character in s_numerology_systems[numerology_system_name].Keys)
                        {
                            if (Constants.ARABIC_DIGITS.Contains(character)) continue;
                            if (Constants.STOP_MARKS.Contains(character)) continue;
                            if (character == '{') continue;
                            if (character == '}') continue;
                            writer.Write(character.ToString() + "\t");
                        }
                        writer.Write("PrimeFactors");
                        writer.WriteLine();

                        foreach (Chapter chapter in book.Chapters)
                        {
                            writer.Write(chapter.Name + "\t");
                            writer.Write(chapter.Number.ToString() + "\t");
                            writer.Write(chapter.Verses.Count.ToString() + "\t");

                            int chapter_word_count = 0;
                            int chapter_letter_count = 0;
                            int chapter_unique_letter_count = 0;
                            long chapter_numerology_value = 0;
                            foreach (Verse verse in chapter.Verses)
                            {
                                chapter_word_count += verse.Words.Count;
                                chapter_letter_count += verse.LetterCount;
                                chapter_numerology_value += Server.CalculateValue(verse, numerology_system_name, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);
                            }
                            chapter_unique_letter_count = chapter.UniqueLetters.Count;
                            writer.Write(chapter_word_count.ToString() + "\t");
                            writer.Write(chapter_letter_count.ToString() + "\t");
                            writer.Write(chapter_unique_letter_count.ToString() + "\t");
                            writer.Write(chapter_numerology_value.ToString() + "\t");

                            long p = Numbers.IndexOfPrime(chapter_numerology_value);
                            long ap = Numbers.IndexOfAdditivePrime(chapter_numerology_value);
                            long pp = Numbers.IndexOfPurePrime(chapter_numerology_value);
                            long c = Numbers.IndexOfComposite(chapter_numerology_value);
                            long ac = Numbers.IndexOfAdditiveComposite(chapter_numerology_value);
                            long pc = Numbers.IndexOfPureComposite(chapter_numerology_value);
                            writer.Write((p == -1 ? "-" : p.ToString()) + "\t"
                                           + (ap == -1 ? "-" : ap.ToString()) + "\t"
                                           + (pp == -1 ? "-" : pp.ToString()) + "\t"
                                           + (c == -1 ? "-" : c.ToString()) + "\t"
                                           + (ac == -1 ? "-" : ac.ToString()) + "\t"
                                           + (pc == -1 ? "-" : pc.ToString())
                                         );
                            writer.Write("\t");

                            foreach (char character in s_numerology_systems[numerology_system_name].Keys)
                            {
                                if (Constants.ARABIC_DIGITS.Contains(character)) continue;
                                if (Constants.STOP_MARKS.Contains(character)) continue;
                                if (character == '{') continue;
                                if (character == '}') continue;
                                writer.Write(chapter.GetLetterFrequency(character) + "\t");
                            }

                            writer.Write(Numbers.FactorizeToString(chapter_numerology_value));

                            writer.WriteLine();
                        }
                    }
                }
            }
            catch
            {
                // silence IO error in case running from read-only media (CD/DVD)
            }

            // show file content after save
            System.Diagnostics.Process.Start("Notepad.exe", filename);
        }
    }
    private static void GenerateWordFrequency(string text_mode)
    {
        string directory = "Statistics";
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string filename = directory + "/" + "Words_" + text_mode + ".txt";
        List<WordStatistic> word_statistics = new List<WordStatistic>();
        Book book = Book.Instance;
        if (book != null)
        {
            word_statistics.Clear();
            foreach (Verse verse in book.Verses)
            {
                foreach (Word word in verse.Words)
                {
                    bool is_found = false;
                    for (int j = 0; j < word_statistics.Count; j++)
                    {
                        if (word.Texts[text_mode] == word_statistics[j].Word.Texts[text_mode])
                        {
                            is_found = true;
                            word_statistics[j].Frequency++;
                        }
                    }

                    if (!is_found)
                    {
                        WordStatistic word_statistic = new WordStatistic();
                        word_statistic.Word = word;
                        word_statistic.Frequency++;
                        word_statistics.Add(word_statistic);
                    }
                }
            }

            try
            {
                using (StreamWriter writer = new StreamWriter(filename, false, Encoding.Unicode))
                {
                    writer.WriteLine("Word" + "\t" + "Frequency");
                    foreach (WordStatistic word_statistic in word_statistics)
                    {
                        writer.WriteLine(word_statistic.Word.Texts[text_mode] + "\t" + (word_statistic.Frequency));
                    }
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
    private static void GenerateLetterFrequency(string text_mode)
    {
        string directory = "Statistics";
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string filename = directory + "/" + "Letters_" + text_mode + ".txt";
        Book book = Book.Instance;
        if (book != null)
        {
            StringBuilder str = new StringBuilder();
            foreach (Verse verse in book.Verses)
            {
                str.AppendLine(verse.GetText(text_mode));
            }
            str.Remove(str.Length - 2, 2);
            string text = str.ToString();

            List<LetterStatistic> letter_statistics = new List<LetterStatistic>();
            letter_statistics.Clear();
            for (int i = 0; i < text.Length; i++)
            {
                if ((text[i] == ' ') || (text[i] == '\n') || (text[i] == '\r'))
                {
                    continue;
                }

                bool is_found = false;
                for (int j = 0; j < letter_statistics.Count; j++)
                {
                    if (text[i] == letter_statistics[j].Letter)
                    {
                        is_found = true;
                        letter_statistics[j].Frequency++;
                    }
                }

                if (!is_found)
                {
                    LetterStatistic letter_statistic = new LetterStatistic();
                    letter_statistic.Letter = text[i];
                    letter_statistic.Frequency++;
                    letter_statistics.Add(letter_statistic);
                }
            }

            try
            {
                using (StreamWriter writer = new StreamWriter(filename, false, Encoding.Unicode))
                {
                    writer.WriteLine("Letter" + "\t" + "Frequency");
                    foreach (LetterStatistic letter_statistic in letter_statistics)
                    {
                        writer.WriteLine(letter_statistic.Letter.ToString() + "\t" + (letter_statistic.Frequency));
                    }
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
    private static void GenerateWordLetterDigits(string text_mode)
    {
        string directory = "Statistics";
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string filename = directory + "/" + "Digits_" + text_mode + ".txt";
        Book book = Book.Instance;
        if (book != null)
        {
            List<int> words_per_chapters = new List<int>();
            List<int> words_digits_per_chapters = new List<int>();
            List<int> letters_per_chapters = new List<int>();
            List<int> letters_digits_per_chapters = new List<int>();
            foreach (Chapter chapter in book.Chapters)
            {
                int words_per_chapter = 0;
                int letters_per_chapter = 0;
                foreach (Verse verse in chapter.Verses)
                {
                    words_per_chapter += verse.Words.Count;
                    letters_per_chapter += verse.LetterCount;
                }

                words_per_chapters.Add(words_per_chapter);
                letters_per_chapters.Add(letters_per_chapter);
                words_digits_per_chapters.AddRange(Numbers.GetDigits(words_per_chapter));
                letters_digits_per_chapters.AddRange(Numbers.GetDigits(letters_per_chapter));
            }

            try
            {
                using (StreamWriter writer = new StreamWriter(filename, false, Encoding.Unicode))
                {
                    writer.WriteLine("Words" + "\t" + "Digits" + "\t" + "Letters" + "\t" + "Digits");
                    for (int i = 0; i < letters_digits_per_chapters.Count; i++)
                    {
                        writer.Write("{0}\t", (i < words_per_chapters.Count) ? words_per_chapters[i].ToString() : "");
                        writer.Write("{0}\t", (i < words_digits_per_chapters.Count) ? words_digits_per_chapters[i].ToString() : "");
                        writer.Write("{0}\t", (i < letters_per_chapters.Count) ? letters_per_chapters[i].ToString() : "");
                        writer.Write("{0}", (i < letters_digits_per_chapters.Count) ? letters_digits_per_chapters[i].ToString() : "");
                        writer.WriteLine();
                    }
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

    // help messages
    private static List<string> s_help_messages = null;
    public static List<string> HelpMessages
    {
        get
        {
            if (s_help_messages == null)
            {
                s_help_messages = DataAccess.LoadLines("Help" + "/" + "HelpMessages.txt");
            }
            return s_help_messages;
        }
    }
}
