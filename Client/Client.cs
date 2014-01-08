using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Model;

public class Client
{
    #region ### Server Proxy Methods ###

    public static bool Initialize()
    {
        return Server.Initialize();
    }
    public static bool Initialized
    {
        get { return Server.Initialized; }
    }
    public static List<string> TextModes
    {
        get { return Server.TextModes; }
    }
    public static Dictionary<string, Translation> Translations
    {
        get { return Server.Translations; }
    }
    public static Dictionary<string, Recitation> Recitations
    {
        get { return Server.Recitations; }
    }
    public static Dictionary<string, Dictionary<char, long>> NumerologySystems
    {
        get { return Server.NumerologySystems; }
    }

    public static List<string> HelpMessages
    {
        get { return Server.HelpMessages; }
    }

    public static string GetRoot(string text_mode, string text)
    {
        return Server.GetRoot(text_mode, text);
    }
    public static List<string> GetRoots(string text_mode, string text)
    {
        return Server.GetRoots(text_mode, text);
    }
    public static List<string> GetRootsWith(string text_mode, string text, bool anywhere_in_word)
    {
        return Server.GetRootsWith(text_mode, text, anywhere_in_word);
    }
    public static List<string> GetRelatedWords(string text_mode, string root)
    {
        return Server.GetRelatedWords(text_mode, root);
    }
    public static List<string> GetRelatedWordsWithNeighbours(string text_mode, string root)
    {
        return Server.GetRelatedWordsWithNeighbours(text_mode, root);
    }
    public static List<Verse> GetRelatedVerses(string text_mode, string root)
    {
        return Server.GetRelatedVerses(text_mode, root);
    }

    public static void UpdateNumerologySystems(string text_mode, string text, bool is_dynamic)
    {
        Server.UpdateNumerologySystems(text_mode, text, is_dynamic);
    }

    public static string GetTranslationKey(string name)
    {
        return Server.GetTranslationKey(name);
    }
    public static void LoadTranslationBook(string title)
    {
        Server.LoadTranslationBook(title);
    }
    public static void UnloadTranslationBook(string title)
    {
        Server.UnloadTranslationBook(title);
    }
    public static void LoadSimplificationRules()
    {
        Server.LoadSimplificationRules();
    }
    public static void LoadNumerologySystems()
    {
        Server.LoadNumerologySystems();
    }
    public static void BuildSimplifiedBook(string to_text_mode)
    {
        Server.SimplifyBook(to_text_mode);
    }
    public static string SimplifyText(string text, string to_text_mode)
    {
        return Server.SimplifyText(text, to_text_mode);
    }

    public static long CalculateValue(Word word, string numerology_system_name, bool add_to_letter_l_number, bool add_to_letter_w_number, bool add_to_letter_v_number, bool add_to_letter_c_number, bool add_to_letter_l_distance, bool add_to_letter_w_distance, bool add_to_letter_v_distance, bool add_to_letter_c_distance, bool add_to_word_w_number, bool add_to_word_v_number, bool add_to_word_c_number, bool add_to_word_w_distance, bool add_to_word_v_distance, bool add_to_word_c_distance, bool add_to_verse_v_number, bool add_to_verse_c_number, bool add_to_verse_v_distance, bool add_to_verse_c_distance, bool add_to_chapter_c_number)
    {
        return Server.CalculateValue(word, numerology_system_name, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);
    }
    public static long CalculateValue(Verse verse, string numerology_system_name, bool add_to_letter_l_number, bool add_to_letter_w_number, bool add_to_letter_v_number, bool add_to_letter_c_number, bool add_to_letter_l_distance, bool add_to_letter_w_distance, bool add_to_letter_v_distance, bool add_to_letter_c_distance, bool add_to_word_w_number, bool add_to_word_v_number, bool add_to_word_c_number, bool add_to_word_w_distance, bool add_to_word_v_distance, bool add_to_word_c_distance, bool add_to_verse_v_number, bool add_to_verse_c_number, bool add_to_verse_v_distance, bool add_to_verse_c_distance, bool add_to_chapter_c_number)
    {
        return Server.CalculateValue(verse, numerology_system_name, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);
    }
    public static long CalculateValue(List<Verse> verses, string numerology_system_name, bool add_to_letter_l_number, bool add_to_letter_w_number, bool add_to_letter_v_number, bool add_to_letter_c_number, bool add_to_letter_l_distance, bool add_to_letter_w_distance, bool add_to_letter_v_distance, bool add_to_letter_c_distance, bool add_to_word_w_number, bool add_to_word_v_number, bool add_to_word_c_number, bool add_to_word_w_distance, bool add_to_word_v_distance, bool add_to_word_c_distance, bool add_to_verse_v_number, bool add_to_verse_c_number, bool add_to_verse_v_distance, bool add_to_verse_c_distance, bool add_to_chapter_c_number)
    {
        return Server.CalculateValue(verses, numerology_system_name, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);
    }
    public static long CalculateValue(Chapter chapter, string numerology_system_name, bool add_to_letter_l_number, bool add_to_letter_w_number, bool add_to_letter_v_number, bool add_to_letter_c_number, bool add_to_letter_l_distance, bool add_to_letter_w_distance, bool add_to_letter_v_distance, bool add_to_letter_c_distance, bool add_to_word_w_number, bool add_to_word_v_number, bool add_to_word_c_number, bool add_to_word_w_distance, bool add_to_word_v_distance, bool add_to_word_c_distance, bool add_to_verse_v_number, bool add_to_verse_c_number, bool add_to_verse_v_distance, bool add_to_verse_c_distance, bool add_to_chapter_c_number)
    {
        return Server.CalculateValue(chapter, numerology_system_name, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);
    }
    public static long CalculateValue(Verse verse, int word_index, string numerology_system_name, bool add_to_letter_l_number, bool add_to_letter_w_number, bool add_to_letter_v_number, bool add_to_letter_c_number, bool add_to_letter_l_distance, bool add_to_letter_w_distance, bool add_to_letter_v_distance, bool add_to_letter_c_distance, bool add_to_word_w_number, bool add_to_word_v_number, bool add_to_word_c_number, bool add_to_word_w_distance, bool add_to_word_v_distance, bool add_to_word_c_distance, bool add_to_verse_v_number, bool add_to_verse_c_number, bool add_to_verse_v_distance, bool add_to_verse_c_distance, bool add_to_chapter_c_number)
    {
        return Server.CalculateValue(verse, word_index, numerology_system_name, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);
    }
    public static long CalculateValue(Verse verse, int word_index, int letter_index, string numerology_system_name, bool add_to_letter_l_number, bool add_to_letter_w_number, bool add_to_letter_v_number, bool add_to_letter_c_number, bool add_to_letter_l_distance, bool add_to_letter_w_distance, bool add_to_letter_v_distance, bool add_to_letter_c_distance, bool add_to_word_w_number, bool add_to_word_v_number, bool add_to_word_c_number, bool add_to_word_w_distance, bool add_to_word_v_distance, bool add_to_word_c_distance, bool add_to_verse_v_number, bool add_to_verse_c_number, bool add_to_verse_v_distance, bool add_to_verse_c_distance, bool add_to_chapter_c_number)
    {
        return Server.CalculateValue(verse, word_index, letter_index, numerology_system_name, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);
    }
    public static long CalculateValue(List<Verse> verses, int first_verse_letter_number, int last_verse_letter_number, string numerology_system_name, bool add_to_letter_l_number, bool add_to_letter_w_number, bool add_to_letter_v_number, bool add_to_letter_c_number, bool add_to_letter_l_distance, bool add_to_letter_w_distance, bool add_to_letter_v_distance, bool add_to_letter_c_distance, bool add_to_word_w_number, bool add_to_word_v_number, bool add_to_word_c_number, bool add_to_word_w_distance, bool add_to_word_v_distance, bool add_to_word_c_distance, bool add_to_verse_v_number, bool add_to_verse_c_number, bool add_to_verse_v_distance, bool add_to_verse_c_distance, bool add_to_chapter_c_number)
    {
        return Server.CalculateValue(verses, first_verse_letter_number, last_verse_letter_number, numerology_system_name, add_to_letter_l_number, add_to_letter_w_number, add_to_letter_v_number, add_to_letter_c_number, add_to_letter_l_distance, add_to_letter_w_distance, add_to_letter_v_distance, add_to_letter_c_distance, add_to_word_w_number, add_to_word_v_number, add_to_word_c_number, add_to_word_w_distance, add_to_word_v_distance, add_to_word_c_distance, add_to_verse_v_number, add_to_verse_c_number, add_to_verse_v_distance, add_to_verse_c_distance, add_to_chapter_c_number);
    }
    public static long CalculateValue(string user_text, string m_numerology_system_name)
    {
        return Server.CalculateValue(user_text, m_numerology_system_name);
    }

    public Dictionary<string, int> GetWordsWith(string text, bool anywhere_in_word)
    {
        List<Verse> verses = Server.GetSourceVerses(m_text_mode, m_find_scope, m_selection, m_found_verses);
        if (verses != null)
        {
            return Server.GetWordsWith(m_text_mode, verses, text, anywhere_in_word);
        }
        return null;
    }
    public Dictionary<string, int> GetCurrentWords(string text, bool anywhere_in_word)
    {
        List<Verse> verses = Server.GetSourceVerses(m_text_mode, m_find_scope, m_selection, m_found_verses);
        if (verses != null)
        {
            return Server.GetCurrentWords(m_text_mode, verses, text, anywhere_in_word);
        }
        return null;
    }
    public Dictionary<string, int> GetNextWords(string text, bool anywhere_in_word)
    {
        List<Verse> verses = Server.GetSourceVerses(m_text_mode, m_find_scope, m_selection, m_found_verses);
        if (verses != null)
        {
            return Server.GetNextWords(m_text_mode, verses, text, anywhere_in_word);
        }
        return null;
    }

    public List<Phrase> FindPhrases(string text, FindByTextLanguageType language_type, string translation, FindByTextLocation text_location, bool case_sensitive, FindByTextWordness wordness, int multiplicity, bool anywhere_in_word, bool all_text_modes)
    {
        if (all_text_modes)
        {
            m_found_phrases = Server.FindPhrases("all", m_find_scope, m_selection, m_found_verses, text, language_type, translation, text_location, case_sensitive, wordness, multiplicity, anywhere_in_word);
        }
        else
        {
            m_found_phrases = Server.FindPhrases(m_text_mode, m_find_scope, m_selection, m_found_verses, text, language_type, translation, text_location, case_sensitive, wordness, multiplicity, anywhere_in_word);
        }

        if (m_found_phrases != null)
        {
            m_found_verses = new List<Verse>();
            foreach (Phrase phrase in m_found_phrases)
            {
                if (!m_found_verses.Contains(phrase.Verse))
                {
                    m_found_verses.Add(phrase.Verse);
                }
            }

            IsTranslation = (language_type == FindByTextLanguageType.Translation);
        }
        return m_found_phrases;
    }
    public List<Phrase> FindPhrases(string root, int multiplicity)
    {
        m_found_phrases = Server.FindPhrases(m_text_mode, m_find_scope, m_selection, m_found_verses, root, multiplicity);
        if (m_found_phrases != null)
        {
            m_found_verses = new List<Verse>();
            foreach (Phrase phrase in m_found_phrases)
            {
                if (!m_found_verses.Contains(phrase.Verse))
                {
                    m_found_verses.Add(phrase.Verse);
                }
            }
        }
        return m_found_phrases;
    }
    public List<Verse> FindVerses(Verse verse, FindBySimilarityMethod similarity_method, double similarity_percentage)
    {
        m_found_verses = Server.FindVerses(m_text_mode, m_find_scope, m_selection, m_found_verses, verse, similarity_method, similarity_percentage);
        return m_found_verses;
    }
    public List<List<Verse>> FindVerseRanges(FindBySimilarityMethod similarity_method, double similarity_percentage)
    {
        m_found_verse_ranges = Server.FindVerseRanges(m_text_mode, m_find_scope, m_selection, m_found_verses, similarity_method, similarity_percentage);
        if (m_found_verse_ranges != null)
        {
            m_found_verses = new List<Verse>();
            foreach (List<Verse> verse_range in m_found_verse_ranges)
            {
                m_found_verses.AddRange(verse_range);
            }
        }
        return m_found_verse_ranges;
    }
    public List<Word> FindWords(int number, int letter_count, int unique_letter_count, long value,
        NumberType number_number_type, NumberType letter_count_number_type, NumberType unique_letter_count_number_type, NumberType value_number_type,
        ComparisonOperator number_operator, ComparisonOperator letter_count_operator, ComparisonOperator unique_letter_count_operator, ComparisonOperator value_operator)
    {
        m_found_words = Server.FindWords(m_text_mode, m_find_scope, m_selection, m_found_verses,
                            number, letter_count, unique_letter_count, value,
                            number_number_type, letter_count_number_type, unique_letter_count_number_type, value_number_type,
                            number_operator, letter_count_operator, unique_letter_count_operator, value_operator, m_numerology_system_name,
                            m_add_to_letter_l_number, m_add_to_letter_w_number, m_add_to_letter_v_number, m_add_to_letter_c_number, m_add_to_letter_l_distance, m_add_to_letter_w_distance, m_add_to_letter_v_distance, m_add_to_letter_c_distance, m_add_to_word_w_number, m_add_to_word_v_number, m_add_to_word_c_number, m_add_to_word_w_distance, m_add_to_word_v_distance, m_add_to_word_c_distance, m_add_to_verse_v_number, m_add_to_verse_c_number, m_add_to_verse_v_distance, m_add_to_verse_c_distance, m_add_to_chapter_c_number);
        if (m_found_words != null)
        {
            m_found_verses = new List<Verse>();
            m_found_phrases = new List<Phrase>();
            foreach (Word word in m_found_words)
            {
                Verse verse = word.Verse;
                if (!m_found_verses.Contains(verse))
                {
                    m_found_verses.Add(verse);
                }

                Phrase phrase = new Phrase(word.Verse, m_text_mode, word.Texts[m_text_mode], word.Positions[m_text_mode]);
                m_found_phrases.Add(phrase);
            }
        }
        return m_found_words;
    }
    public List<List<Word>> FindWordRanges(int word_count, int letter_count, int unique_letter_count, long value,
        NumberType word_count_number_type, NumberType letter_count_number_type, NumberType unique_letter_count_number_type, NumberType value_number_type,
        ComparisonOperator word_count_operator, ComparisonOperator letter_count_operator, ComparisonOperator unique_letter_count_operator, ComparisonOperator value_operator)
    {
        m_found_word_ranges = Server.FindWordRanges(m_text_mode, m_find_scope, m_selection, m_found_verses,
                            word_count, letter_count, unique_letter_count, value,
                            word_count_number_type, letter_count_number_type, unique_letter_count_number_type, value_number_type,
                            word_count_operator, letter_count_operator, unique_letter_count_operator, value_operator, m_numerology_system_name,
                            m_add_to_letter_l_number, m_add_to_letter_w_number, m_add_to_letter_v_number, m_add_to_letter_c_number, m_add_to_letter_l_distance, m_add_to_letter_w_distance, m_add_to_letter_v_distance, m_add_to_letter_c_distance, m_add_to_word_w_number, m_add_to_word_v_number, m_add_to_word_c_number, m_add_to_word_w_distance, m_add_to_word_v_distance, m_add_to_word_c_distance, m_add_to_verse_v_number, m_add_to_verse_c_number, m_add_to_verse_v_distance, m_add_to_verse_c_distance, m_add_to_chapter_c_number);
        if (m_found_word_ranges != null)
        {
            m_found_verses = new List<Verse>();
            m_found_phrases = new List<Phrase>();
            foreach (List<Word> word_range in m_found_word_ranges)
            {
                if (word_range.Count > 0)
                {
                    Verse verse = word_range[0].Verse;
                    if (!m_found_verses.Contains(verse))
                    {
                        m_found_verses.Add(verse);
                    }

                    string word_range_text = word_range[0].Texts[m_text_mode];
                    if (word_range.Count > 1)
                    {
                        for (int i = 1; i < word_range.Count; i++)
                        {
                            word_range_text += " " + word_range[i].Texts[m_text_mode];
                        }
                    }

                    Phrase phrase = new Phrase(verse, m_text_mode, word_range_text, word_range[0].Positions[m_text_mode]);
                    m_found_phrases.Add(phrase);
                }
            }
        }
        return m_found_word_ranges;
    }
    public List<Verse> FindVerses(int number, int word_count, int letter_count, int unique_letter_count, long value,
        NumberType number_number_type, NumberType word_count_number_type, NumberType letter_count_number_type, NumberType unique_letter_count_number_type, NumberType value_number_type,
        ComparisonOperator number_operator, ComparisonOperator word_count_operator, ComparisonOperator letter_count_operator, ComparisonOperator unique_letter_count_operator, ComparisonOperator value_operator)
    {
        m_found_verses = Server.FindVerses(m_text_mode, m_find_scope, m_selection, m_found_verses,
                            number, word_count, letter_count, unique_letter_count, value,
                            number_number_type, word_count_number_type, letter_count_number_type, unique_letter_count_number_type, value_number_type,
                            number_operator, word_count_operator, letter_count_operator, unique_letter_count_operator, value_operator, m_numerology_system_name,
                            m_add_to_letter_l_number, m_add_to_letter_w_number, m_add_to_letter_v_number, m_add_to_letter_c_number, m_add_to_letter_l_distance, m_add_to_letter_w_distance, m_add_to_letter_v_distance, m_add_to_letter_c_distance, m_add_to_word_w_number, m_add_to_word_v_number, m_add_to_word_c_number, m_add_to_word_w_distance, m_add_to_word_v_distance, m_add_to_word_c_distance, m_add_to_verse_v_number, m_add_to_verse_c_number, m_add_to_verse_v_distance, m_add_to_verse_c_distance, m_add_to_chapter_c_number);
        return m_found_verses;
    }
    public List<List<Verse>> FindVerseRanges(int verse_count, int word_count, int letter_count, int unique_letter_count, long value,
        NumberType verse_count_number_type, NumberType word_count_number_type, NumberType letter_count_number_type, NumberType unique_letter_count_number_type, NumberType value_number_type,
        ComparisonOperator verse_count_operator, ComparisonOperator word_count_operator, ComparisonOperator letter_count_operator, ComparisonOperator unique_letter_count_operator, ComparisonOperator value_operator)
    {
        m_found_verse_ranges = Server.FindVerseRanges(m_text_mode, m_find_scope, m_selection, m_found_verses,
                            verse_count, word_count, letter_count, unique_letter_count, value,
                            verse_count_number_type, word_count_number_type, letter_count_number_type, unique_letter_count_number_type, value_number_type,
                            verse_count_operator, word_count_operator, letter_count_operator, unique_letter_count_operator, value_operator, m_numerology_system_name,
                            m_add_to_letter_l_number, m_add_to_letter_w_number, m_add_to_letter_v_number, m_add_to_letter_c_number, m_add_to_letter_l_distance, m_add_to_letter_w_distance, m_add_to_letter_v_distance, m_add_to_letter_c_distance, m_add_to_word_w_number, m_add_to_word_v_number, m_add_to_word_c_number, m_add_to_word_w_distance, m_add_to_word_v_distance, m_add_to_word_c_distance, m_add_to_verse_v_number, m_add_to_verse_c_number, m_add_to_verse_v_distance, m_add_to_verse_c_distance, m_add_to_chapter_c_number);
        if (m_found_verse_ranges != null)
        {
            m_found_verses = new List<Verse>();
            foreach (List<Verse> verse_range in m_found_verse_ranges)
            {
                m_found_verses.AddRange(verse_range);
            }
        }
        return m_found_verse_ranges;
    }
    public List<Chapter> FindChapters(int number, int verse_count, int word_count, int letter_count, int unique_letter_count, long value,
        NumberType number_number_type, NumberType verse_count_number_type, NumberType word_count_number_type, NumberType letter_count_number_type, NumberType unique_letter_count_number_type, NumberType value_number_type,
        ComparisonOperator number_operator, ComparisonOperator verse_count_operator, ComparisonOperator word_count_operator, ComparisonOperator letter_count_operator, ComparisonOperator unique_letter_count_operator, ComparisonOperator value_operator)
    {
        m_found_chapters = Server.FindChapters(m_text_mode, m_find_scope, m_selection, m_found_verses,
                            number, verse_count, word_count, letter_count, unique_letter_count, value,
                            number_number_type, verse_count_number_type, word_count_number_type, letter_count_number_type, unique_letter_count_number_type, value_number_type,
                            number_operator, verse_count_operator, word_count_operator, letter_count_operator, unique_letter_count_operator, value_operator, m_numerology_system_name,
                            m_add_to_letter_l_number, m_add_to_letter_w_number, m_add_to_letter_v_number, m_add_to_letter_c_number, m_add_to_letter_l_distance, m_add_to_letter_w_distance, m_add_to_letter_v_distance, m_add_to_letter_c_distance, m_add_to_word_w_number, m_add_to_word_v_number, m_add_to_word_c_number, m_add_to_word_w_distance, m_add_to_word_v_distance, m_add_to_word_c_distance, m_add_to_verse_v_number, m_add_to_verse_c_number, m_add_to_verse_v_distance, m_add_to_verse_c_distance, m_add_to_chapter_c_number);
        if (m_found_chapters != null)
        {
            m_found_verses = new List<Verse>();
            foreach (Chapter chapter in m_found_chapters)
            {
                m_found_verses.AddRange(chapter.Verses);
            }
        }
        return m_found_chapters;
    }
    public List<Chapter> FindChaptersWithAllVersesHaving(int word_count, int letter_count, int unique_letter_count, long value,
        NumberType word_count_number_type, NumberType letter_count_number_type, NumberType unique_letter_count_number_type, NumberType value_number_type,
        ComparisonOperator word_count_operator, ComparisonOperator letter_count_operator, ComparisonOperator unique_letter_count_operator, ComparisonOperator value_operator)
    {
        m_found_chapters = Server.FindChaptersWithAllVersesHaving(m_text_mode, m_find_scope, m_selection, m_found_verses,
                            word_count, letter_count, unique_letter_count, value,
                            word_count_number_type, letter_count_number_type, unique_letter_count_number_type, value_number_type,
                            word_count_operator, letter_count_operator, unique_letter_count_operator, value_operator, m_numerology_system_name,
                            m_add_to_letter_l_number, m_add_to_letter_w_number, m_add_to_letter_v_number, m_add_to_letter_c_number, m_add_to_letter_l_distance, m_add_to_letter_w_distance, m_add_to_letter_v_distance, m_add_to_letter_c_distance, m_add_to_word_w_number, m_add_to_word_v_number, m_add_to_word_c_number, m_add_to_word_w_distance, m_add_to_word_v_distance, m_add_to_word_c_distance, m_add_to_verse_v_number, m_add_to_verse_c_number, m_add_to_verse_v_distance, m_add_to_verse_c_distance, m_add_to_chapter_c_number);
        if (m_found_chapters != null)
        {
            m_found_verses = new List<Verse>();
            foreach (Chapter chapter in m_found_chapters)
            {
                m_found_verses.AddRange(chapter.Verses);
            }
        }
        return m_found_chapters;
    }
    public List<Verse> FindVerses(FindByProstrationType prostration_type)
    {
        m_found_verses = Server.FindVerses(m_text_mode, m_find_scope, m_selection, m_found_verses, prostration_type);
        return m_found_verses;
    }
    public List<Chapter> FindChapters(FindByRevelationPlace revelation_place)
    {
        m_found_chapters = Server.FindChapters(m_text_mode, m_find_scope, m_selection, m_found_verses, revelation_place);
        return m_found_chapters;
    }
    public List<Verse> FindVerses(string phrase, int phrase_letter_frequency_sum, FindByFrequencySumType sum_type)
    {
        m_found_verses = Server.FindVerses(m_text_mode, m_find_scope, m_selection, m_found_verses, phrase, phrase_letter_frequency_sum, sum_type);
        return m_found_verses;
    }

    public void GenerateFullBookStatistics()
    {
        Server.GenerateFullBookStatistics(m_numerology_system_name,
            m_add_to_letter_l_number, m_add_to_letter_w_number, m_add_to_letter_v_number, m_add_to_letter_c_number, m_add_to_letter_l_distance, m_add_to_letter_w_distance, m_add_to_letter_v_distance, m_add_to_letter_c_distance, m_add_to_word_w_number, m_add_to_word_v_number, m_add_to_word_c_number, m_add_to_word_w_distance, m_add_to_word_v_distance, m_add_to_word_c_distance, m_add_to_verse_v_number, m_add_to_verse_c_number, m_add_to_verse_v_distance, m_add_to_verse_c_distance, m_add_to_chapter_c_number);
    }
    public void GenerateBookStatistics()
    {
        Server.GenerateBookStatistics(m_numerology_system_name,
            m_add_to_letter_l_number, m_add_to_letter_w_number, m_add_to_letter_v_number, m_add_to_letter_c_number, m_add_to_letter_l_distance, m_add_to_letter_w_distance, m_add_to_letter_v_distance, m_add_to_letter_c_distance, m_add_to_word_w_number, m_add_to_word_v_number, m_add_to_word_c_number, m_add_to_word_w_distance, m_add_to_word_v_distance, m_add_to_word_c_distance, m_add_to_verse_v_number, m_add_to_verse_c_number, m_add_to_verse_v_distance, m_add_to_verse_c_distance, m_add_to_chapter_c_number);
    }

    #endregion ### Server Proxy Methods ###

    private static string DEFAULT_TEXT_MODE = Server.DEFAULT_TEXT_MODE;
    private static string DEFAULT_LETTER_ORDER = Server.DEFAULT_LETTER_ORDER;
    private static string DEFAULT_LETTER_VALUES = Server.DEFAULT_LETTER_VALUES;
    public static string DEFAULT_NUMEROLOGY_SYSTEM = DEFAULT_TEXT_MODE + "_" + DEFAULT_LETTER_ORDER + "_" + DEFAULT_LETTER_VALUES;

    private string m_machine = "";
    public string Machine
    {
        get { return m_machine; }
        //set
        //{
        //    if (machine != value)
        //    {
        //        machine = value;
        //    }
        //}
    }

    private string m_username = "";
    public string Username
    {
        get { return m_username; }
        //set
        //{
        //    if (username != value)
        //    {
        //        username = value;
        //    }
        //}
    }

    private string m_password = "";
    public string Password
    {
        get { return m_password; }
        set
        {
            if (m_password != value)
            {
                m_password = value;
            }
        }
    }

    public Client(string machine, string username, string password)
    {
        m_machine = machine;
        m_username = username;
        m_password = password;
        NumerologySystemName = DEFAULT_TEXT_MODE + "_" + DEFAULT_LETTER_ORDER + "_" + DEFAULT_LETTER_VALUES;
    }

    private string m_numerology_system_name = "";
    public string NumerologySystemName
    {
        get { return m_numerology_system_name; }
        set
        {
            if (m_numerology_system_name != value)
            {
                m_numerology_system_name = value;

                string[] parts = m_numerology_system_name.Split('_');
                if (parts.Length == 3)
                {
                    if (m_text_mode != parts[0])
                    {
                        m_text_mode = parts[0];

                        UpdateBookNumbersAndWordLetters(m_text_mode);
                    }

                    if (m_letter_order_system != parts[1])
                    {
                        m_letter_order_system = parts[1];
                    }

                    if (m_letter_value_system != parts[2])
                    {
                        m_letter_value_system = parts[2];
                    }

                    if (m_is_dynamic)
                    {
                        if (m_is_highlighted_text)
                        {
                            // CurrentText-wide numerology system
                            Server.UpdateNumerologySystems(m_text_mode, "", true);
                        }
                        else
                        {
                            // Selection-wide numerology system
                            if (m_selection != null)
                            {
                                Server.UpdateNumerologySystems(m_text_mode, m_selection.GetText(m_text_mode), true);
                            }
                        }
                    }
                    else
                    {
                        // Book-wide numerology system
                        if (Book.Instance != null)
                        {
                            Server.UpdateNumerologySystems(m_text_mode, Book.Instance.GetText(m_text_mode), false);
                        }
                    }
                }
            }
        }
    }
    public static void UpdateBookNumbersAndWordLetters(string text_mode)
    {
        Book book = Book.Instance;
        if (book != null)
        {
            if (book.Verses.Count > 0)
            {
                if (book.Verses[0].Words.Count > 0)
                {
                    if (book.Verses[0].Words[0].Texts.ContainsKey(text_mode))
                    {
                        // update Word.Letters
                        foreach (Verse verse in book.Verses)
                        {
                            foreach (Word word in verse.Words)
                            {
                                word.Letters.Clear();
                                int number_in_word = 0;
                                foreach (char character in word.Texts[text_mode])
                                {
                                    if (character == ' ') continue;
                                    if (character == '\r') continue;
                                    if (character == '\n') continue;
                                    if (Constants.STOP_MARKS.Contains(character)) continue;

                                    number_in_word++;
                                    Letter letter = new Letter(word, character, number_in_word);
                                    word.Letters.Add(letter);
                                }
                            }
                        }
                        // update chapter/verse/word/letter numbers/distances to new text_mode
                        book.UpdateBook(text_mode);
                    }
                }
            }
        }
    }

    public Dictionary<char, long> NumerologySystem
    {
        get
        {
            if (Client.NumerologySystems != null)
            {
                if (Client.NumerologySystems.ContainsKey(m_numerology_system_name))
                {
                    return Client.NumerologySystems[m_numerology_system_name];
                }
            }
            return null;
        }
        set
        {
            if (Client.NumerologySystems != null)
            {
                if (Client.NumerologySystems.ContainsKey(m_numerology_system_name))
                {
                    Client.NumerologySystems[m_numerology_system_name] = value;
                }
            }
        }
    }

    public void ResetNumerologySystem()
    {
        m_is_dynamic = false;
        m_is_highlighted_text = false;
        m_add_to_letter_l_number = false;
        m_add_to_letter_w_number = false;
        m_add_to_letter_v_number = false;
        m_add_to_letter_c_number = false;
        m_add_to_letter_l_distance = false;
        m_add_to_letter_w_distance = false;
        m_add_to_letter_v_distance = false;
        m_add_to_letter_c_distance = false;
        m_add_to_word_w_number = false;
        m_add_to_word_v_number = false;
        m_add_to_word_c_number = false;
        m_add_to_word_w_distance = false;
        m_add_to_word_v_distance = false;
        m_add_to_word_c_distance = false;
        m_add_to_verse_v_number = false;
        m_add_to_verse_c_number = false;
        m_add_to_verse_v_distance = false;
        m_add_to_verse_c_distance = false;
        m_add_to_chapter_c_number = false;

        NumerologySystemName = DEFAULT_TEXT_MODE + "_" + DEFAULT_LETTER_ORDER + "_" + DEFAULT_LETTER_VALUES;

        // restore numerology system in case user has edited it
        if (Book.Instance != null)
        {
            Server.UpdateNumerologySystems(DEFAULT_TEXT_MODE, Book.Instance.GetText(m_text_mode), true);
        }
    }

    private string m_text_mode = "";
    public string TextMode
    {
        get { return m_text_mode; }
        set
        {
            if (m_text_mode != value)
            {
                NumerologySystemName = value + "_" + m_letter_order_system + "_" + m_letter_value_system;
            }
        }
    }
    private string m_letter_order_system = "";
    public string LetterOrderSystem
    {
        get { return m_letter_order_system; }
        set
        {
            if (m_letter_order_system != value)
            {
                NumerologySystemName = m_text_mode + "_" + value + "_" + m_letter_value_system;
            }
        }
    }
    private string m_letter_value_system = "";
    public string LetterValueSystem
    {
        get { return m_letter_value_system; }
        set
        {
            if (m_letter_value_system != value)
            {
                NumerologySystemName = m_text_mode + "_" + m_letter_order_system + "_" + value;
            }
        }
    }

    private bool m_is_dynamic = false;
    public bool IsDynamic
    {
        get { return m_is_dynamic; }
        set
        {
            m_is_dynamic = value;
            NumerologySystemName = m_numerology_system_name;
        }
    }
    private bool m_is_highlighted_text = false;
    public bool IsHighlightedText
    {
        get { return m_is_highlighted_text; }
        set
        {
            m_is_highlighted_text = value;
            NumerologySystemName = m_numerology_system_name;
        }
    }

    private bool m_add_to_letter_l_number = false;
    private bool m_add_to_letter_w_number = false;
    private bool m_add_to_letter_v_number = false;
    private bool m_add_to_letter_c_number = false;
    private bool m_add_to_letter_l_distance = false;
    private bool m_add_to_letter_w_distance = false;
    private bool m_add_to_letter_v_distance = false;
    private bool m_add_to_letter_c_distance = false;
    private bool m_add_to_word_w_number = false;
    private bool m_add_to_word_v_number = false;
    private bool m_add_to_word_c_number = false;
    private bool m_add_to_word_w_distance = false;
    private bool m_add_to_word_v_distance = false;
    private bool m_add_to_word_c_distance = false;
    private bool m_add_to_verse_v_number = false;
    private bool m_add_to_verse_c_number = false;
    private bool m_add_to_verse_v_distance = false;
    private bool m_add_to_verse_c_distance = false;
    private bool m_add_to_chapter_c_number = false;

    public bool AddToLetterLNumber
    {
        get { return m_add_to_letter_l_number; }
        set { m_add_to_letter_l_number = value; }
    }
    public bool AddToLetterWNumber
    {
        get { return m_add_to_letter_w_number; }
        set { m_add_to_letter_w_number = value; }
    }
    public bool AddToLetterVNumber
    {
        get { return m_add_to_letter_v_number; }
        set { m_add_to_letter_v_number = value; }
    }
    public bool AddToLetterCNumber
    {
        get { return m_add_to_letter_c_number; }
        set { m_add_to_letter_c_number = value; }
    }
    public bool AddToLetterLDistance
    {
        get { return m_add_to_letter_l_distance; }
        set { m_add_to_letter_l_distance = value; }
    }
    public bool AddToLetterWDistance
    {
        get { return m_add_to_letter_w_distance; }
        set { m_add_to_letter_w_distance = value; }
    }
    public bool AddToLetterVDistance
    {
        get { return m_add_to_letter_v_distance; }
        set { m_add_to_letter_v_distance = value; }
    }
    public bool AddToLetterCDistance
    {
        get { return m_add_to_letter_c_distance; }
        set { m_add_to_letter_c_distance = value; }
    }
    public bool AddToWordWNumber
    {
        get { return m_add_to_word_w_number; }
        set { m_add_to_word_w_number = value; }
    }
    public bool AddToWordVNumber
    {
        get { return m_add_to_word_v_number; }
        set { m_add_to_word_v_number = value; }
    }
    public bool AddToWordCNumber
    {
        get { return m_add_to_word_c_number; }
        set { m_add_to_word_c_number = value; }
    }
    public bool AddToWordWDistance
    {
        get { return m_add_to_word_w_distance; }
        set { m_add_to_word_w_distance = value; }
    }
    public bool AddToWordVDistance
    {
        get { return m_add_to_word_v_distance; }
        set { m_add_to_word_v_distance = value; }
    }
    public bool AddToWordCDistance
    {
        get { return m_add_to_word_c_distance; }
        set { m_add_to_word_c_distance = value; }
    }
    public bool AddToVerseVNumber
    {
        get { return m_add_to_verse_v_number; }
        set { m_add_to_verse_v_number = value; }
    }
    public bool AddToVerseCNumber
    {
        get { return m_add_to_verse_c_number; }
        set { m_add_to_verse_c_number = value; }
    }
    public bool AddToVerseVDistance
    {
        get { return m_add_to_verse_v_distance; }
        set { m_add_to_verse_v_distance = value; }
    }
    public bool AddToVerseCDistance
    {
        get { return m_add_to_verse_c_distance; }
        set { m_add_to_verse_c_distance = value; }
    }
    public bool AddToChapterCNumber
    {
        get { return m_add_to_chapter_c_number; }
        set { m_add_to_chapter_c_number = value; }
    }

    // used for non-Quran text, user text, or F11 wordwraped text only
    public long CalculateValue(string user_text)
    {
        return CalculateValue(user_text, m_numerology_system_name);
    }

    // used for Quran text only
    public long CalculateValue(Word word)
    {
        return CalculateValue(word, m_numerology_system_name,
                            m_add_to_letter_l_number, m_add_to_letter_w_number, m_add_to_letter_v_number, m_add_to_letter_c_number, m_add_to_letter_l_distance, m_add_to_letter_w_distance, m_add_to_letter_v_distance, m_add_to_letter_c_distance, m_add_to_word_w_number, m_add_to_word_v_number, m_add_to_word_c_number, m_add_to_word_w_distance, m_add_to_word_v_distance, m_add_to_word_c_distance, m_add_to_verse_v_number, m_add_to_verse_c_number, m_add_to_verse_v_distance, m_add_to_verse_c_distance, m_add_to_chapter_c_number);
    }
    public long CalculateValue(Verse verse)
    {
        return CalculateValue(verse, m_numerology_system_name,
                            m_add_to_letter_l_number, m_add_to_letter_w_number, m_add_to_letter_v_number, m_add_to_letter_c_number, m_add_to_letter_l_distance, m_add_to_letter_w_distance, m_add_to_letter_v_distance, m_add_to_letter_c_distance, m_add_to_word_w_number, m_add_to_word_v_number, m_add_to_word_c_number, m_add_to_word_w_distance, m_add_to_word_v_distance, m_add_to_word_c_distance, m_add_to_verse_v_number, m_add_to_verse_c_number, m_add_to_verse_v_distance, m_add_to_verse_c_distance, m_add_to_chapter_c_number);
    }
    public long CalculateValue(List<Verse> verses)
    {
        return CalculateValue(verses, m_numerology_system_name,
                            m_add_to_letter_l_number, m_add_to_letter_w_number, m_add_to_letter_v_number, m_add_to_letter_c_number, m_add_to_letter_l_distance, m_add_to_letter_w_distance, m_add_to_letter_v_distance, m_add_to_letter_c_distance, m_add_to_word_w_number, m_add_to_word_v_number, m_add_to_word_c_number, m_add_to_word_w_distance, m_add_to_word_v_distance, m_add_to_word_c_distance, m_add_to_verse_v_number, m_add_to_verse_c_number, m_add_to_verse_v_distance, m_add_to_verse_c_distance, m_add_to_chapter_c_number);
    }
    public long CalculateValue(Chapter chapter)
    {
        return CalculateValue(chapter, m_numerology_system_name,
                            m_add_to_letter_l_number, m_add_to_letter_w_number, m_add_to_letter_v_number, m_add_to_letter_c_number, m_add_to_letter_l_distance, m_add_to_letter_w_distance, m_add_to_letter_v_distance, m_add_to_letter_c_distance, m_add_to_word_w_number, m_add_to_word_v_number, m_add_to_word_c_number, m_add_to_word_w_distance, m_add_to_word_v_distance, m_add_to_word_c_distance, m_add_to_verse_v_number, m_add_to_verse_c_number, m_add_to_verse_v_distance, m_add_to_verse_c_distance, m_add_to_chapter_c_number);
    }
    public long CalculateValue(Verse verse, int word_index)
    {
        return CalculateValue(verse, word_index, m_numerology_system_name,
                            m_add_to_letter_l_number, m_add_to_letter_w_number, m_add_to_letter_v_number, m_add_to_letter_c_number, m_add_to_letter_l_distance, m_add_to_letter_w_distance, m_add_to_letter_v_distance, m_add_to_letter_c_distance, m_add_to_word_w_number, m_add_to_word_v_number, m_add_to_word_c_number, m_add_to_word_w_distance, m_add_to_word_v_distance, m_add_to_word_c_distance, m_add_to_verse_v_number, m_add_to_verse_c_number, m_add_to_verse_v_distance, m_add_to_verse_c_distance, m_add_to_chapter_c_number);
    }
    public long CalculateValue(Verse verse, int word_index, int letter_index)
    {
        return CalculateValue(verse, word_index, letter_index, m_numerology_system_name,
                            m_add_to_letter_l_number, m_add_to_letter_w_number, m_add_to_letter_v_number, m_add_to_letter_c_number, m_add_to_letter_l_distance, m_add_to_letter_w_distance, m_add_to_letter_v_distance, m_add_to_letter_c_distance, m_add_to_word_w_number, m_add_to_word_v_number, m_add_to_word_c_number, m_add_to_word_w_distance, m_add_to_word_v_distance, m_add_to_word_c_distance, m_add_to_verse_v_number, m_add_to_verse_c_number, m_add_to_verse_v_distance, m_add_to_verse_c_distance, m_add_to_chapter_c_number);
    }
    public long CalculateValue(List<Verse> verses, int first_verse_letter_number, int last_verse_letter_number)
    {
        return CalculateValue(verses, first_verse_letter_number, last_verse_letter_number, m_numerology_system_name,
                            m_add_to_letter_l_number, m_add_to_letter_w_number, m_add_to_letter_v_number, m_add_to_letter_c_number, m_add_to_letter_l_distance, m_add_to_letter_w_distance, m_add_to_letter_v_distance, m_add_to_letter_c_distance, m_add_to_word_w_number, m_add_to_word_v_number, m_add_to_word_c_number, m_add_to_word_w_distance, m_add_to_word_v_distance, m_add_to_word_c_distance, m_add_to_verse_v_number, m_add_to_verse_c_number, m_add_to_verse_v_distance, m_add_to_verse_c_distance, m_add_to_chapter_c_number);
    }
    public List<long> CalculateAllVerseValues(List<Verse> verses)
    {
        List<long> result = new List<long>();
        foreach (Verse verse in verses)
        {
            long value = CalculateValue(verse, m_numerology_system_name,
                            m_add_to_letter_l_number, m_add_to_letter_w_number, m_add_to_letter_v_number, m_add_to_letter_c_number, m_add_to_letter_l_distance, m_add_to_letter_w_distance, m_add_to_letter_v_distance, m_add_to_letter_c_distance, m_add_to_word_w_number, m_add_to_word_v_number, m_add_to_word_c_number, m_add_to_word_w_distance, m_add_to_word_v_distance, m_add_to_word_c_distance, m_add_to_verse_v_number, m_add_to_verse_c_number, m_add_to_verse_v_distance, m_add_to_verse_c_distance, m_add_to_chapter_c_number);
            result.Add(value);
        }
        return result;
    }
    public List<long> CalculateAllWordValues(List<Verse> verses)
    {
        List<long> result = new List<long>();
        foreach (Verse verse in verses)
        {
            for (int w = 0; w < verse.Words.Count; w++)
            {
                long value = CalculateValue(verse, w, m_numerology_system_name,
                            m_add_to_letter_l_number, m_add_to_letter_w_number, m_add_to_letter_v_number, m_add_to_letter_c_number, m_add_to_letter_l_distance, m_add_to_letter_w_distance, m_add_to_letter_v_distance, m_add_to_letter_c_distance, m_add_to_word_w_number, m_add_to_word_v_number, m_add_to_word_c_number, m_add_to_word_w_distance, m_add_to_word_v_distance, m_add_to_word_c_distance, m_add_to_verse_v_number, m_add_to_verse_c_number, m_add_to_verse_v_distance, m_add_to_verse_c_distance, m_add_to_chapter_c_number);
                result.Add(value);
            }
        }
        return result;
    }
    public List<long> CalculateAllLetterValues(List<Verse> verses)
    {
        List<long> letter_values = new List<long>();
        foreach (Verse verse in verses)
        {
            for (int i = 0; i < verse.Words.Count; i++)
            {
                for (int j = 0; j < verse.Words[i].Texts[m_text_mode].Length; j++)
                {
                    if ((verse.Words[i].Texts[m_text_mode][j] == ' ') || (verse.Words[i].Texts[m_text_mode][j] == '\n') || (verse.Words[i].Texts[m_text_mode][j] == '\r'))
                    {
                        continue;
                    }

                    long value = CalculateValue(verse, i, j, m_numerology_system_name,
                            m_add_to_letter_l_number, m_add_to_letter_w_number, m_add_to_letter_v_number, m_add_to_letter_c_number, m_add_to_letter_l_distance, m_add_to_letter_w_distance, m_add_to_letter_v_distance, m_add_to_letter_c_distance, m_add_to_word_w_number, m_add_to_word_v_number, m_add_to_word_c_number, m_add_to_word_w_distance, m_add_to_word_v_distance, m_add_to_word_c_distance, m_add_to_verse_v_number, m_add_to_verse_c_number, m_add_to_verse_v_distance, m_add_to_verse_c_distance, m_add_to_chapter_c_number);
                    letter_values.Add(value);
                }
            }
        }
        return letter_values;
    }
    public long MaximumVerseValue
    {
        get
        {
            long result = 0;
            Book book = Book.Instance;
            if (book != null)
            {
                foreach (Verse verse in book.Verses)
                {
                    long value = CalculateValue(verse, m_numerology_system_name,
                            m_add_to_letter_l_number, m_add_to_letter_w_number, m_add_to_letter_v_number, m_add_to_letter_c_number, m_add_to_letter_l_distance, m_add_to_letter_w_distance, m_add_to_letter_v_distance, m_add_to_letter_c_distance, m_add_to_word_w_number, m_add_to_word_v_number, m_add_to_word_c_number, m_add_to_word_w_distance, m_add_to_word_v_distance, m_add_to_word_c_distance, m_add_to_verse_v_number, m_add_to_verse_c_number, m_add_to_verse_v_distance, m_add_to_verse_c_distance, m_add_to_chapter_c_number);
                    if (result < value)
                    {
                        result = value;
                    }
                }
            }
            return result;
        }
    }
    public long MaximumWordValue
    {
        get
        {
            long result = 0;
            Book book = Book.Instance;
            if (book != null)
            {
                foreach (Verse verse in book.Verses)
                {
                    for (int w = 0; w < verse.Words.Count; w++)
                    {
                        long value = CalculateValue(verse, w, m_numerology_system_name,
                            m_add_to_letter_l_number, m_add_to_letter_w_number, m_add_to_letter_v_number, m_add_to_letter_c_number, m_add_to_letter_l_distance, m_add_to_letter_w_distance, m_add_to_letter_v_distance, m_add_to_letter_c_distance, m_add_to_word_w_number, m_add_to_word_v_number, m_add_to_word_c_number, m_add_to_word_w_distance, m_add_to_word_v_distance, m_add_to_word_c_distance, m_add_to_verse_v_number, m_add_to_verse_c_number, m_add_to_verse_v_distance, m_add_to_verse_c_distance, m_add_to_chapter_c_number);
                        if (result < value)
                        {
                            result = value;
                        }
                    }
                }
            }
            return result;
        }
    }
    public long MaximumLetterValue
    {
        get
        {
            long result = 0;
            Dictionary<char, long> numerology_system = Server.NumerologySystems[m_numerology_system_name];
            foreach (long value in numerology_system.Values)
            {
                if (result < value)
                {
                    result = value;
                }
            }
            return result;
        }
    }

    // many bookmarks per per book per client
    private List<Bookmark> m_bookmarks = new List<Bookmark>();
    public List<Bookmark> Bookmarks
    {
        get { return m_bookmarks; }
    }
    private Bookmark m_current_bookmark = null;
    public Bookmark CurrentBookmark
    {
        get { return m_current_bookmark; }
    }
    private int m_current_bookmark_index = -1;
    public int CurrentBookmarkIndex
    {
        get
        {
            if (m_bookmarks != null)
            {
                for (int i = 0; i < m_bookmarks.Count; i++)
                {
                    if (m_bookmarks[i] == m_current_bookmark)
                    {
                        if (i == m_current_bookmark_index)
                        {
                            return i;
                        }
                        else
                        {
                            throw new Exception("current=" + m_current_bookmark_index + "\t\tbookmark_index=" + i);
                        }
                    }
                }
            }
            return -1;
        }
    }
    public int GetBookmarkIndex(Bookmark bookmark)
    {
        if (m_bookmarks != null)
        {
            for (int i = 0; i < m_bookmarks.Count; i++)
            {
                if (m_bookmarks[i] == bookmark)
                {
                    return i;
                }
            }
        }
        return -1;
    }
    public Bookmark GetBookmark(SelectionScope scope, List<int> indexes)
    {
        if (m_bookmarks != null)
        {
            foreach (Bookmark bookmark in m_bookmarks)
            {
                if (bookmark.Selection.Scope == scope)
                {
                    if (bookmark.Selection.Indexes.Count == indexes.Count)
                    {
                        int matching_indexes = 0;
                        for (int i = 0; i < bookmark.Selection.Indexes.Count; i++)
                        {
                            if (bookmark.Selection.Indexes[i] == indexes[i])
                            {
                                matching_indexes++;
                            }
                        }
                        if (indexes.Count == matching_indexes)
                        {
                            return bookmark;
                        }
                    }
                }
            }
        }
        return null;
    }
    public Bookmark GotoBookmark(Selection selection)
    {
        Bookmark bookmark = null;
        if (selection != null)
        {
            bookmark = GetBookmark(selection.Scope, selection.Indexes);
            if (bookmark != null)
            {
                m_current_bookmark = bookmark;
                m_current_bookmark_index = GetBookmarkIndex(bookmark);
            }
        }
        return bookmark;
    }
    public Bookmark GotoBookmark(SelectionScope scope, List<int> indexes)
    {
        Bookmark bookmark = GetBookmark(scope, indexes);
        if (bookmark != null)
        {
            m_current_bookmark = bookmark;
            m_current_bookmark_index = GetBookmarkIndex(bookmark);
        }
        return bookmark;
    }
    public Bookmark GotoNextBookmark()
    {
        if (m_bookmarks != null)
        {
            if (m_bookmarks.Count > 0)
            {
                if (m_current_bookmark_index < m_bookmarks.Count - 1)
                {
                    m_current_bookmark_index++;
                    m_current_bookmark = m_bookmarks[m_current_bookmark_index];
                }
            }
        }
        return m_current_bookmark;
    }
    public Bookmark GotoPreviousBookmark()
    {
        if (m_bookmarks != null)
        {
            if (m_bookmarks.Count > 0)
            {
                if (m_current_bookmark_index > 0)
                {
                    m_current_bookmark_index--;
                    m_current_bookmark = m_bookmarks[m_current_bookmark_index];
                }
            }
        }
        return m_current_bookmark;
    }
    public Bookmark AddOverwriteBookmark(Selection selection, string note)
    {
        Bookmark bookmark = GetBookmark(selection.Scope, selection.Indexes);
        if (bookmark != null) // overwrite existing bookmark
        {
            bookmark.Note = note;
            bookmark.LastModifiedTime = DateTime.Now;
            m_current_bookmark = bookmark;
        }
        else // create a new bookmark
        {
            bookmark = new Bookmark(selection, note);
            m_bookmarks.Insert(m_current_bookmark_index + 1, bookmark);
            m_current_bookmark_index++;
            m_current_bookmark = m_bookmarks[m_current_bookmark_index];
        }
        return m_current_bookmark;
    }
    public void DeleteCurrentBookmark()
    {
        Bookmark current_bookmark = CurrentBookmark;
        if (current_bookmark != null)
        {
            if (m_bookmarks != null)
            {
                m_bookmarks.Remove(current_bookmark);
                if (m_bookmarks.Count == 0) // no bookmark to display
                {
                    m_current_bookmark_index = -1;
                    m_current_bookmark = null;
                }
                else // there are bookmarks still
                {
                    // if index becomes outside list, move back into list
                    if (m_current_bookmark_index == m_bookmarks.Count)
                    {
                        m_current_bookmark_index = m_bookmarks.Count - 1;
                    }
                    m_current_bookmark = m_bookmarks[m_current_bookmark_index];
                }
            }
        }
    }
    public void ClearBookmarks()
    {
        if (m_bookmarks != null)
        {
            m_bookmarks.Clear();
            m_current_bookmark_index = -1;
            m_current_bookmark = null;
        }
    }
    public void SaveBookmarks()
    {
        Book book = Book.Instance;
        if (book != null)
        {
            string directory = "Bookmarks";
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string filename = directory + "/" + book.Title + ".txt";
            try
            {
                using (StreamWriter writer = new StreamWriter(filename, false, Encoding.Unicode))
                {
                    if (m_bookmarks != null)
                    {
                        foreach (Bookmark bookmark in m_bookmarks)
                        {
                            if (bookmark.Note.Length > 0)
                            {
                                string scope_str = bookmark.Selection.Scope.ToString();

                                StringBuilder str = new StringBuilder();
                                for (int i = 0; i < bookmark.Selection.Indexes.Count; i++)
                                {
                                    str.Append((bookmark.Selection.Indexes[i] + 1).ToString() + "+");
                                }
                                if (str.Length > 0)
                                {
                                    str.Remove(str.Length - "+".Length, "+".Length);
                                }

                                string created_time = bookmark.CreatedTime.ToString(Globals.DATETIME_FORMAT);
                                string last_modified_time = bookmark.LastModifiedTime.ToString(Globals.DATETIME_FORMAT);
                                string note = bookmark.Note;

                                string line = scope_str + "," + str.ToString() + "," + created_time + "," + last_modified_time + "," + note;
                                writer.WriteLine(line);
                            }
                        }
                    }
                }
            }
            catch
            {
                // silence IO error in case running from read-only media (CD/DVD)
            }
        }
    }
    public void LoadBookmarks()
    {
        Book book = Book.Instance;
        if (book != null)
        {
            string directory = "Bookmarks";
            string filename = directory + "/" + book.Title + ".txt";
            if (File.Exists(filename))
            {
                using (StreamReader reader = File.OpenText(filename))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        string[] parts = line.Split(',');
                        if (parts.Length == 5)
                        {
                            try
                            {
                                SelectionScope scope = (SelectionScope)Enum.Parse(typeof(SelectionScope), parts[0]);

                                string part = parts[1].Trim();
                                string[] sub_parts = part.Split('+');
                                List<int> indexes = new List<int>();
                                foreach (string sub_part in sub_parts)
                                {
                                    indexes.Add(int.Parse(sub_part.Trim()) - 1);
                                }
                                Selection selection = new Selection(book, scope, indexes);

                                DateTime created_time = DateTime.ParseExact(parts[2], Globals.DATETIME_FORMAT, null);
                                DateTime last_modified_time = DateTime.ParseExact(parts[3], Globals.DATETIME_FORMAT, null);
                                string note = parts[4];

                                LoadBookmark(selection, note, created_time, last_modified_time);
                            }
                            catch
                            {
                                throw new Exception("Invalid data format in " + filename);
                            }
                        }
                    }
                }
            }
        }
    }
    public void LoadBookmark(Selection selection, string note, DateTime created_time, DateTime last_modified_time)
    {
        if (m_bookmarks != null)
        {
            Bookmark bookmark = AddOverwriteBookmark(selection, note);
            if (bookmark != null)
            {
                bookmark.CreatedTime = created_time;
                bookmark.LastModifiedTime = last_modified_time;
            }
        }
    }

    // ONLY one selection per client per all texts
    private Selection m_selection = null;
    public Selection Selection
    {
        get
        {
            Book book = Book.Instance;
            if (book != null)
            {
                return m_selection;
            }
            return null;
        }
        set
        {
            Book book = Book.Instance;
            if (book != null)
            {
                m_selection = value;
            }
        }
    }
    private void InitializeSelection()
    {
        Book book = Book.Instance;
        if (book != null)
        {
            if (m_selection != null)
            {
                m_selection = new Selection(book, SelectionScope.Chapter, new List<int>() { 0 });
            }
        }
    }

    private bool m_is_translation = false;
    public bool IsTranslation
    {
        set { m_is_translation = value; }
        get { return m_is_translation; }
    }
    private FindScope m_find_scope = FindScope.Book;
    public FindScope FindScope
    {
        set { m_find_scope = value; }
        get { return m_find_scope; }
    }
    private List<Verse> m_found_verses = null;
    public List<Verse> FoundVerses
    {
        set { m_found_verses = value; }
        get { return m_found_verses; }
    }
    private List<Phrase> m_found_phrases = null;
    public List<Phrase> FoundPhrases
    {
        set { m_found_phrases = value; }
        get
        {
            return m_found_phrases;
        }
    }
    /// <summary>
    /// Update positions of all phrases from phrase text mode to client's current text mode
    /// </summary>
    /// <returns></returns>
    public void UpdatePhrasePositions()
    {
        if (m_found_phrases != null)
        {
            if (m_found_phrases.Count > 0)
            {
                string from_text_mode = m_found_phrases[0].TextMode;
                Server.UpdatePhrasePositions(ref m_found_phrases, from_text_mode, m_text_mode);
            }
        }
    }
    private List<Word> m_found_words = null;
    public List<Word> FoundWords
    {
        set { m_found_words = value; }
        get { return m_found_words; }
    }
    private List<List<Word>> m_found_word_ranges = null;
    public List<List<Word>> FoundWordRanges
    {
        set { m_found_word_ranges = value; }
        get { return m_found_word_ranges; }
    }
    private List<List<Verse>> m_found_verse_ranges = null;
    public List<List<Verse>> FoundVerseRanges
    {
        set { m_found_verse_ranges = value; }
        get { return m_found_verse_ranges; }
    }
    private List<Chapter> m_found_chapters = null;
    public List<Chapter> FoundChapters
    {
        set { m_found_chapters = value; }
        get { return m_found_chapters; }
    }

    private string m_history_directory = "History";
    public class FindHistoryItem
    {
        public string FindText = "";
        public FindByTextLanguageType LanguageType = FindByTextLanguageType.Arabic;
        public string Translator = "";
        public List<Verse> Verses = new List<Verse>();
        public List<Phrase> Phrases = new List<Phrase>();
        public string Header = "";
    }
    private List<FindHistoryItem> m_find_history = new List<FindHistoryItem>();
    public List<FindHistoryItem> FindHistory
    {
        get { return m_find_history; }
    }
    private int m_find_history_index = -1;
    public int FindHistoryIndex
    {
        get { return m_find_history_index; }
    }
    public FindHistoryItem CurrentFindHistoryItem
    {
        get
        {
            if (m_find_history != null)
            {
                if ((m_find_history_index >= 0) && (m_find_history_index < m_find_history.Count))
                {
                    return m_find_history[m_find_history_index];
                }
            }
            return null;
        }
    }
    public void AddFindHistoryItem(FindHistoryItem item)
    {
        if (item != null)
        {
            if (m_find_history != null)
            {
                m_find_history.Add(item);
                m_find_history_index = m_find_history.Count - 1;
                m_found_verses = item.Verses;
                m_found_phrases = item.Phrases;
            }
        }
    }
    public void DeleteFindHistoryItem(FindHistoryItem item)
    {
        if (item != null)
        {
            if (m_find_history != null)
            {
                m_find_history.Remove(item);
                m_find_history_index = m_find_history.Count - 1;
                if ((m_find_history_index >= 0) && (m_find_history_index < m_find_history.Count))
                {
                    m_found_verses = m_find_history[m_find_history_index].Verses;
                    m_found_phrases = m_find_history[m_find_history_index].Phrases;
                }
            }
        }
    }
    public void DeleteCurrentFindHistoryItem()
    {
        if (m_find_history != null)
        {
            if ((m_find_history_index >= 0) && (m_find_history_index < m_find_history.Count))
            {
                FindHistoryItem item = m_find_history[m_find_history_index];
                if (item != null)
                {
                    m_find_history.Remove(item);
                    m_find_history_index = m_find_history.Count - 1;

                    if (m_find_history.Count == 0) // all items deleted
                    {
                        m_find_history_index = -1;
                    }
                    else // there are still some item(s)
                    {
                        // if index becomes outside list, move back into list
                        if (m_find_history_index == m_find_history.Count)
                        {
                            m_find_history_index = m_find_history.Count - 1;
                        }
                    }

                    if ((m_find_history_index >= 0) && (m_find_history_index < m_find_history.Count))
                    {
                        m_found_verses = m_find_history[m_find_history_index].Verses;
                        m_found_phrases = m_find_history[m_find_history_index].Phrases;
                    }
                }
            }
        }
    }
    public void ClearFindHistory()
    {
        if (m_find_history != null)
        {
            m_find_history.Clear();
            m_find_history_index = -1;

            m_found_verses = null;
            m_found_phrases = null;
        }
    }
    public FindHistoryItem GotoPreviousFindHistory()
    {
        FindHistoryItem result = null;
        if (m_find_history != null)
        {
            if ((m_find_history_index > 0) && (m_find_history_index < m_find_history.Count))
            {
                m_find_history_index--;
                result = m_find_history[m_find_history_index];
            }
        }
        return result;
    }
    public FindHistoryItem GotoNextFindHistory()
    {
        FindHistoryItem result = null;
        if (m_find_history != null)
        {
            if ((m_find_history_index >= -1) && (m_find_history_index < m_find_history.Count - 1))
            {
                m_find_history_index++;
                result = m_find_history[m_find_history_index];
            }
        }
        return result;
    }
    public void SaveFindHistory()
    {
        if (m_find_history != null)
        {
            string user_history_directory = m_history_directory + "/" + m_username;
            if (!Directory.Exists(user_history_directory))
            {
                Directory.CreateDirectory(user_history_directory);
            }

            string filename = user_history_directory + "/" + "FindHistory" + ".txt";
            try
            {
                using (StreamWriter writer = new StreamWriter(filename, false, Encoding.Unicode))
                {
                    foreach (FindHistoryItem item in m_find_history)
                    {
                        StringBuilder str = new StringBuilder();
                        str.Append("FindText" + "\t" + item.FindText + "\r\n");
                        str.Append("Header" + "\t" + item.Header + "\r\n");
                        str.Append("LanguageType" + "\t" + item.LanguageType + "\r\n");
                        str.Append("Translation" + "\t" + item.Translator + "\r\n");

                        if ((item.Phrases != null) && (item.Phrases.Count > 0))
                        {
                            foreach (Phrase phrase in item.Phrases)
                            {
                                str.Append(phrase.Verse.Number.ToString() + "," + phrase.Text + "," + phrase.Position.ToString() + "\r\n");
                            }
                        }
                        else
                        {
                            foreach (Verse verse in item.Verses)
                            {
                                str.Append(verse.Number.ToString() + "," + "" + "," + 0 + "\r\n");
                            }
                        }
                        str.Append("\r\n"); // END_OF_FIND_HISTORY_ITEM
                        writer.Write(str.ToString());
                    }
                }
            }
            catch
            {
                // silence IO error in case running from read-only media (CD/DVD)
            }
        }
    }
    public void LoadFindHistory()
    {
        FindHistoryItem item = null;
        string line = "";
        string[] parts = null;

        Book book = Book.Instance;
        if (book != null)
        {
            string filename = m_history_directory + "/" + m_username + "/" + "FindHistory" + ".txt";
            if (File.Exists(filename))
            {
                using (StreamReader reader = File.OpenText(filename))
                {
                    try
                    {
                        if (!reader.EndOfStream)
                        {
                            do
                            {
                                item = new FindHistoryItem();
                                AddFindHistoryItem(item);

                                line = reader.ReadLine();
                                parts = line.Split('\t');
                                if ((parts.Length == 2) && (parts[0] == "FindText"))
                                {
                                    item.FindText = parts[1];

                                    line = reader.ReadLine();
                                    parts = line.Split('\t');
                                    if ((parts.Length == 2) && (parts[0] == "Header"))
                                    {
                                        item.Header = parts[1];

                                        line = reader.ReadLine();
                                        parts = line.Split('\t');
                                        if ((parts.Length == 2) && (parts[0] == "LanguageType"))
                                        {
                                            item.LanguageType = (FindByTextLanguageType)Enum.Parse(typeof(FindByTextLanguageType), parts[1]);

                                            line = reader.ReadLine();
                                            parts = line.Split('\t');
                                            if ((parts.Length == 2) && (parts[0] == "Translation"))
                                            {
                                                item.Translator = parts[1];

                                                while (true)
                                                {
                                                    line = reader.ReadLine();
                                                    if ((line.Length == 0) || (line.StartsWith("---")))
                                                    {
                                                        break; // END_OF_FIND_HISTORY_ITEM 
                                                    }

                                                    parts = line.Split(',');
                                                    if (parts.Length == 3) // Phrase.Verse.Number, Phrase.Text, Phrase.Position
                                                    {
                                                        int verse_index = int.Parse(parts[0].Trim()) - 1;
                                                        if ((verse_index >= 0) && (verse_index < book.Verses.Count))
                                                        {
                                                            Verse verse = book.Verses[verse_index];
                                                            if (!item.Verses.Contains(verse))
                                                            {
                                                                item.Verses.Add(verse);
                                                            }

                                                            string phrase_text = parts[1];
                                                            if (phrase_text.Length > 0)
                                                            {
                                                                int phrase_position = int.Parse(parts[2].Trim());
                                                                Phrase phrase = new Phrase(verse, m_text_mode, phrase_text.Trim(), phrase_position);
                                                                item.Phrases.Add(phrase);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            } while (!reader.EndOfStream);
                        }
                    }
                    catch
                    {
                        throw new Exception("Invalid " + filename + " format.");
                    }
                }
            }
        }
    }

    public class BrowseHistoryItem : Selection
    {
        public BrowseHistoryItem(Selection selection)
            : base(selection.Book, selection.Scope, selection.Indexes)
        {
        }
    }
    private List<BrowseHistoryItem> m_browse_history = new List<BrowseHistoryItem>();
    public List<BrowseHistoryItem> BrowseHistory
    {
        get { return m_browse_history; }
    }
    private int m_browse_history_index = -1;
    public int BrowseHistoryIndex
    {
        get { return m_browse_history_index; }
    }
    public BrowseHistoryItem CurrentBrowseHistoryItem
    {
        get
        {
            if (m_browse_history != null)
            {
                if ((m_browse_history_index >= 0) && (m_browse_history_index < m_browse_history.Count))
                {
                    return m_browse_history[m_browse_history_index];
                }
            }
            return null;
        }
    }
    public void AddBrowseHistoryItem(BrowseHistoryItem item)
    {
        if (m_browse_history != null)
        {
            m_browse_history.Add(item);
            m_browse_history_index = m_browse_history.Count - 1;
        }
    }
    public void DeleteBrowseHistoryItem(BrowseHistoryItem item)
    {
        if (m_browse_history != null)
        {
            m_browse_history.Remove(item);
            m_browse_history_index = m_browse_history.Count - 1;
        }
    }
    public void DeleteCurrentBrowseHistoryItem()
    {
        if (m_browse_history != null)
        {
            if ((m_browse_history_index >= 0) && (m_browse_history_index < m_browse_history.Count))
            {
                BrowseHistoryItem item = m_browse_history[m_browse_history_index];
                m_browse_history.Remove(item);
                m_browse_history_index = m_browse_history.Count - 1;
            }

            if (m_browse_history.Count == 0) // all items deleted
            {
                m_browse_history_index = -1;
            }
            else // there are still some item(s)
            {
                // if index becomes outside list, move back into list
                if (m_browse_history_index == m_browse_history.Count)
                {
                    m_browse_history_index = m_browse_history.Count - 1;
                }
            }
        }
    }
    public void ClearBrowseHistory()
    {
        if (m_browse_history != null)
        {
            m_browse_history.Clear();
            m_browse_history_index = -1;
        }
    }
    public BrowseHistoryItem GotoPreviousBrowseHistory()
    {
        BrowseHistoryItem result = null;
        if (m_browse_history != null)
        {
            if ((m_browse_history_index > 0) && (m_browse_history_index < m_browse_history.Count))
            {
                m_browse_history_index--;
                result = m_browse_history[m_browse_history_index];
            }
        }
        return result;
    }
    public BrowseHistoryItem GotoNextBrowseHistory()
    {
        BrowseHistoryItem result = null;
        if (m_browse_history != null)
        {
            if ((m_browse_history_index >= -1) && (m_browse_history_index < m_browse_history.Count - 1))
            {
                m_browse_history_index++;
                result = m_browse_history[m_browse_history_index];
            }
        }
        return result;
    }
    public void SaveBrowseHistory()
    {
        if (m_browse_history != null)
        {
            string user_history_directory = m_history_directory + "/" + m_username;
            if (!Directory.Exists(user_history_directory))
            {
                Directory.CreateDirectory(user_history_directory);
            }

            string filename = user_history_directory + "/" + "BrowseHistory" + ".txt";
            try
            {
                using (StreamWriter writer = new StreamWriter(filename, false, Encoding.Unicode))
                {
                    StringBuilder str = new StringBuilder();
                    foreach (BrowseHistoryItem item in m_browse_history)
                    {
                        if (item.Indexes.Count > 0)
                        {
                            str.Append(item.Scope.ToString() + "\t");
                            foreach (int index in item.Indexes)
                            {
                                str.Append(index.ToString() + ",");
                            }
                            str.Remove(str.Length - ",".Length, ",".Length);
                            str.Append("\r\n");
                        }
                    }
                    writer.Write(str.ToString());
                }
            }
            catch
            {
                // silence IO error in case running from read-only media (CD/DVD)
            }
        }
    }
    public void LoadBrowseHistory()
    {
        string filename = m_history_directory + "/" + m_username + "/" + "BrowseHistory" + ".txt";
        if (File.Exists(filename))
        {
            using (StreamReader reader = File.OpenText(filename))
            {
                try
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        string[] parts = line.Split('\t');
                        if (parts.Length == 2)
                        {
                            SelectionScope scope = (SelectionScope)Enum.Parse(typeof(SelectionScope), parts[0]);
                            List<int> indexes = new List<int>();

                            string[] subparts = parts[1].Split(',');
                            foreach (string subpart in subparts)
                            {
                                try
                                {
                                    int index = int.Parse(subpart);
                                    indexes.Add(index);
                                }
                                catch
                                {
                                    continue;
                                }
                            }

                            BrowseHistoryItem item = new BrowseHistoryItem(m_selection);
                            AddBrowseHistoryItem(item);
                        }
                    }
                }
                catch
                {
                    throw new Exception("Invalid " + filename + " format.");
                }
            }
        }
    }

    private List<LetterStatistic> m_letter_statistics = new List<LetterStatistic>();
    public List<LetterStatistic> LetterStatistics
    {
        get { return m_letter_statistics; }
    }
    public void CalculateLetterStatistics(string text)
    {
        if (String.IsNullOrEmpty(text)) return;

        text = text.Replace("\r", "");
        text = text.Replace("\n", "");
        text = text.Replace(" ", "");
        m_letter_statistics.Clear();
        for (int i = 0; i < text.Length; i++)
        {
            if (Constants.ARABIC_DIGITS.Contains(text[i])) continue;
            if (Constants.STOP_MARKS.Contains(text[i])) continue;
            if (text[i] == '{') continue;
            if (text[i] == '}') continue;

            bool is_found = false;
            for (int j = 0; j < m_letter_statistics.Count; j++)
            {
                if (text[i] == m_letter_statistics[j].Letter)
                {
                    is_found = true;
                    m_letter_statistics[j].Frequency++;
                }
            }

            if (!is_found)
            {
                LetterStatistic letter_statistic = new LetterStatistic();
                letter_statistic.Order = m_letter_statistics.Count + 1;
                letter_statistic.Letter = text[i];
                letter_statistic.Frequency++;
                m_letter_statistics.Add(letter_statistic);
            }
        }
    }
    public void SortLetterStatistics(StatisticSortMethod sort_method)
    {
        LetterStatistic.SortMethod = sort_method;
        m_letter_statistics.Sort();
        if (LetterStatistic.SortOrder == StatisticSortOrder.Ascending)
        {
            LetterStatistic.SortOrder = StatisticSortOrder.Descending;
        }
        else
        {
            LetterStatistic.SortOrder = StatisticSortOrder.Ascending;
        }
    }
    public void SaveLetterStatistics(string filename, string text)
    {
        if (String.IsNullOrEmpty(filename)) return;
        if (String.IsNullOrEmpty(text)) return;

        string directory = "Statistics";
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        filename = directory + "/" + filename;
        try
        {
            using (StreamWriter writer = new StreamWriter(filename, false, Encoding.Unicode))
            {
                writer.WriteLine("----------------------------------------");
                writer.WriteLine("Text");
                writer.WriteLine("----------------------------------------");
                writer.WriteLine(text);
                writer.WriteLine("----------------------------------------");
                writer.WriteLine();
                writer.WriteLine("----------------------------------------");
                writer.WriteLine("Order" + "\t" + "Letter" + "\t" + "Frequency");
                writer.WriteLine("----------------------------------------");
                int count = 0;
                int frequency_sum = 0;
                foreach (LetterStatistic letter_statistic in m_letter_statistics)
                {
                    writer.WriteLine(letter_statistic.Order.ToString() + "\t" + letter_statistic.Letter.ToString() + '\t' + letter_statistic.Frequency.ToString());
                    count++;
                    frequency_sum += letter_statistic.Frequency;
                }
                writer.WriteLine("----------------------------------------");
                writer.WriteLine("Total" + "\t" + count.ToString() + "\t" + frequency_sum.ToString());
                writer.WriteLine("----------------------------------------");
            }
        }
        catch
        {
            // silence IO error in case running from read-only media (CD/DVD)
        }

        // show file content after save
        System.Diagnostics.Process.Start("Notepad.exe", filename);
    }
    public void SaveLetterValuation(string filename, string text)
    {
        string directory = "Statistics";
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        filename = directory + "/" + filename;
        try
        {
            using (StreamWriter writer = new StreamWriter(filename, false, Encoding.Unicode))
            {
                writer.WriteLine("----------------------------------------");
                if (IsDynamic)
                {
                    if (IsHighlightedText)
                    {
                        StringBuilder str = new StringBuilder();
                        writer.Write(NumerologySystemName + "\r\n" + "Scope = Highlighted Text");
                    }
                    else
                    {
                        StringBuilder str = new StringBuilder();
                        foreach (int index in Selection.Indexes)
                        {
                            str.Append((index + 1).ToString() + ", ");
                        }
                        if (str.Length > 0)
                        {
                            str.Remove(str.Length - ", ".Length, ", ".Length);
                        }
                        writer.Write(NumerologySystemName + "\r\n" + "Scope = " + Selection.Scope.ToString() + ((Selection.Indexes.Count > 1) ? "s " : " ") + str.ToString());
                    }
                }
                else
                {
                    writer.Write(NumerologySystemName + "\r\n" + "Scope = Entire Book");
                }

                if (
                    (m_add_to_letter_l_number) ||
                    (m_add_to_letter_w_number) ||
                    (m_add_to_letter_v_number) ||
                    (m_add_to_letter_c_number) ||
                    (m_add_to_letter_l_distance) ||
                    (m_add_to_letter_w_distance) ||
                    (m_add_to_letter_v_distance) ||
                    (m_add_to_letter_c_distance)
                    )
                {
                    writer.WriteLine();
                    writer.Write("AddToLetter");
                }
                if (m_add_to_letter_l_number)
                {
                    writer.Write("\t" + "L");
                }
                if (m_add_to_letter_w_number)
                {
                    writer.Write("\t" + "W");
                }
                if (m_add_to_letter_v_number)
                {
                    writer.Write("\t" + "V");
                }
                if (m_add_to_letter_c_number)
                {
                    writer.Write("\t" + "C");
                }
                if (m_add_to_letter_l_distance)
                {
                    writer.Write("\t" + "∆L");
                }
                if (m_add_to_letter_w_distance)
                {
                    writer.Write("\t" + "∆W");
                }
                if (m_add_to_letter_v_distance)
                {
                    writer.Write("\t" + "∆V");
                }
                if (m_add_to_letter_c_distance)
                {
                    writer.Write("\t" + "∆C");
                }

                if (
                    (m_add_to_word_w_number) ||
                    (m_add_to_word_v_number) ||
                    (m_add_to_word_c_number) ||
                    (m_add_to_word_w_distance) ||
                    (m_add_to_word_v_distance) ||
                    (m_add_to_word_c_distance)
                    )
                {
                    writer.WriteLine();
                    writer.Write("AddToWord");
                }
                if (m_add_to_word_w_number)
                {
                    writer.Write("\t" + "W");
                }
                if (m_add_to_word_v_number)
                {
                    writer.Write("\t" + "V");
                }
                if (m_add_to_word_c_number)
                {
                    writer.Write("\t" + "C");
                }
                if (m_add_to_word_w_distance)
                {
                    writer.Write("\t" + "∆W");
                }
                if (m_add_to_word_v_distance)
                {
                    writer.Write("\t" + "∆V");
                }
                if (m_add_to_word_c_distance)
                {
                    writer.Write("\t" + "∆C");
                }

                if (
                    (m_add_to_verse_v_number) ||
                    (m_add_to_verse_c_number) ||
                    (m_add_to_verse_v_distance) ||
                    (m_add_to_verse_c_distance)
                    )
                {
                    writer.WriteLine();
                    writer.Write("AddToVerse");
                }
                if (m_add_to_verse_v_number)
                {
                    writer.Write("\t" + "V");
                }
                if (m_add_to_verse_c_number)
                {
                    writer.Write("\t" + "C");
                }
                if (m_add_to_verse_v_distance)
                {
                    writer.Write("\t" + "∆V");
                }
                if (m_add_to_verse_c_distance)
                {
                    writer.Write("\t" + "∆C");
                }

                if (m_add_to_chapter_c_number)
                {
                    writer.WriteLine();
                    writer.Write("AddToChapter");
                }
                if (m_add_to_chapter_c_number)
                {
                    writer.Write("\t" + "C");
                }

                writer.WriteLine();
                writer.WriteLine();
                writer.WriteLine("----------------------------------------");
                writer.WriteLine("Letter" + "\t" + "Value");
                writer.WriteLine("----------------------------------------");
                if (Server.NumerologySystems.ContainsKey(NumerologySystemName))
                {
                    Dictionary<char, long> numerology_system = Server.NumerologySystems[NumerologySystemName];
                    foreach (char key in numerology_system.Keys)
                    {
                        if (Constants.ARABIC_DIGITS.Contains(key)) continue;
                        if (Constants.STOP_MARKS.Contains(key)) continue;
                        if (key == '{') continue;
                        if (key == '}') continue;

                        writer.WriteLine(key + "\t" + numerology_system[key].ToString());
                    }
                }
                writer.WriteLine("----------------------------------------");
                writer.WriteLine();
                writer.WriteLine("Text");
                writer.WriteLine("----------------------------------------");
                writer.WriteLine(text);
                writer.WriteLine("----------------------------------------");
                writer.WriteLine();
            }
        }
        catch
        {
            // silence IO error in case running from read-only media (CD/DVD)
        }

        // show file content after save
        System.Diagnostics.Process.Start("Notepad.exe", filename);
    }
    public void SaveNumerologySystem()
    {
        string directory = "Values";
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string filename = directory + "/" + NumerologySystemName + ".txt";
        try
        {
            using (StreamWriter writer = new StreamWriter(filename, false, Encoding.Unicode))
            {
                if (Server.NumerologySystems.ContainsKey(NumerologySystemName))
                {
                    Dictionary<char, long> numerology_system = Server.NumerologySystems[NumerologySystemName];
                    foreach (char key in numerology_system.Keys)
                    {
                        if (Constants.ARABIC_DIGITS.Contains(key)) continue;
                        if (Constants.STOP_MARKS.Contains(key)) continue;
                        if (key == '{') continue;
                        if (key == '}') continue;

                        writer.WriteLine(key + "\t" + numerology_system[key].ToString());
                    }
                }
            }
        }
        catch
        {
            // silence IO error in case running from read-only media (CD/DVD)
        }
    }

    private List<LetterStatistic> m_phrase_letter_statistics = new List<LetterStatistic>();
    public List<LetterStatistic> PhraseLetterStatistics
    {
        get { return m_phrase_letter_statistics; }
    }
    public int CalculatePhraseLetterStatistics(string text, string phrase, FindByFrequencySumType sum_type)
    {
        if (String.IsNullOrEmpty(text)) return 0;
        if (String.IsNullOrEmpty(phrase)) return 0;

        if (sum_type == FindByFrequencySumType.NoDuplicateLetters)
        {
            phrase = phrase.RemoveDuplicates();
        }
        phrase = phrase.Replace("\r", "");
        phrase = phrase.Replace("\n", "");
        phrase = phrase.Replace(" ", "");

        int phrase_letter_frequency_sum = 0;
        m_phrase_letter_statistics.Clear();
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
                LetterStatistic phrase_letter_statistic = new LetterStatistic();
                phrase_letter_statistic.Order = m_phrase_letter_statistics.Count + 1;
                phrase_letter_statistic.Letter = phrase[i];
                phrase_letter_statistic.Frequency = frequency;
                m_phrase_letter_statistics.Add(phrase_letter_statistic);
                phrase_letter_frequency_sum += frequency;
            }
        }

        return phrase_letter_frequency_sum;
    }
    public void SortPhraseLetterStatistics(StatisticSortMethod sort_method)
    {
        LetterStatistic.SortMethod = sort_method;
        m_phrase_letter_statistics.Sort();
        if (LetterStatistic.SortOrder == StatisticSortOrder.Ascending)
        {
            LetterStatistic.SortOrder = StatisticSortOrder.Descending;
        }
        else
        {
            LetterStatistic.SortOrder = StatisticSortOrder.Ascending;
        }
    }
    public void SavePhraseLetterStatistics(string filename, string text, string phrase)
    {
        if (String.IsNullOrEmpty(filename)) return;
        if (String.IsNullOrEmpty(text)) return;

        string directory = "Statistics";
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        filename = directory + "/Phrase_" + filename;
        try
        {
            using (StreamWriter writer = new StreamWriter(filename, false, Encoding.Unicode))
            {
                writer.WriteLine("----------------------------------------");
                writer.WriteLine("Text");
                writer.WriteLine("----------------------------------------");
                writer.WriteLine(text);
                writer.WriteLine("----------------------------------------");
                writer.WriteLine("Phrase");
                writer.WriteLine("----------------------------------------");
                writer.WriteLine(phrase);
                writer.WriteLine("----------------------------------------");
                writer.WriteLine("Order" + "\t" + "Letter" + "\t" + "Frequency");
                writer.WriteLine("----------------------------------------");
                int count = m_phrase_letter_statistics.Count;
                int frequency_sum = 0;
                for (int i = 0; i < count; i++)
                {
                    writer.WriteLine(m_phrase_letter_statistics[i].Order.ToString() + "\t" + m_phrase_letter_statistics[i].Letter.ToString() + '\t' + m_phrase_letter_statistics[i].Frequency.ToString());
                    frequency_sum += m_phrase_letter_statistics[i].Frequency;
                }
                writer.WriteLine("----------------------------------------");
                writer.WriteLine("Total" + "\t" + count.ToString() + "\t" + frequency_sum.ToString());
                writer.WriteLine("----------------------------------------");
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
