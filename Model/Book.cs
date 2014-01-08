using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Model
{
    public class Book
    {
        private static string s_data_directory = "Data";

        private string title;
        public string Title
        {
            get { return title; }
            set { title = value; }
        }

        private List<Chapter> chapters = null;
        public List<Chapter> Chapters
        {
            get { return chapters; }
        }

        private List<Chapter> chronological_chapters = null;
        public List<Chapter> ChronologicalChapters
        {
            get
            {
                if (chronological_chapters == null)
                {
                    chronological_chapters = new List<Chapter>();
                    foreach (Chapter chapter in chapters)
                    {
                        chronological_chapters.Add(null);
                    }
                    foreach (Chapter chapter in chapters)
                    {
                        chronological_chapters[chapter.RevelationOrder - 1] = chapter;
                    }
                }
                return chronological_chapters;
            }
        }

        private List<Station> stations = null;
        public List<Station> Stations
        {
            get { return stations; }
        }

        private List<Part> parts = null;
        public List<Part> Parts
        {
            get { return parts; }
        }

        private List<Group> groups = null;
        public List<Group> Groups
        {
            get { return groups; }
        }

        private List<Quarter> group_quarters = null;
        public List<Quarter> Quarters
        {
            get { return group_quarters; }
        }

        private List<Bowing> bowings = null;
        public List<Bowing> Bowings
        {
            get { return bowings; }
        }

        private List<Page> pages = null;
        public List<Page> Pages
        {
            get { return pages; }
        }

        private List<Prostration> prostrations = null;
        public List<Prostration> Prostrations
        {
            get { return prostrations; }
        }

        private List<Verse> verses = null;
        public List<Verse> Verses
        {
            get { return verses; }
        }

        public int WordCount
        {
            get
            {
                int word_count = 0;
                foreach (Verse verse in this.verses)
                {
                    word_count += verse.Words.Count;
                }
                return word_count;
            }
        }
        public int LetterCount
        {
            get
            {
                int letter_count = 0;
                foreach (Verse verse in this.verses)
                {
                    foreach (Word word in verse.Words)
                    {
                        letter_count += word.Letters.Count;
                    }
                }
                return letter_count;
            }
        }
        private List<Letter> unique_letters = null;
        public List<Letter> UniqueLetters
        {
            get
            {
                if (unique_letters == null)
                {
                    unique_letters = new List<Letter>();
                    foreach (Verse verse in this.verses)
                    {
                        foreach (Word word in verse.Words)
                        {
                            foreach (Letter letter in word.Letters)
                            {
                                if (!unique_letters.Contains(letter))
                                {
                                    unique_letters.Add(letter);
                                }
                            }
                        }
                    }
                }
                return unique_letters;
            }
        }
        public int GetLetterFrequency(char character)
        {
            int result = 0;
            foreach (Verse verse in this.verses)
            {
                foreach (Word word in verse.Words)
                {
                    foreach (Letter letter in word.Letters)
                    {
                        if (letter.Character == character)
                        {
                            result++;
                        }
                    }
                }
            }
            return result;
        }

        private Dictionary<string, List<string>> unique_wordss = null;
        private List<string> GetUniqueWords(string text_mode)
        {
            if (unique_wordss == null)
            {
                unique_wordss = new Dictionary<string, List<string>>();
            }
            List<string> unique_words = new List<string>();

            foreach (Verse verse in verses)
            {
                foreach (Word word in verse.Words)
                {
                    if (!unique_words.Contains(word.Texts[text_mode]))
                    {
                        unique_words.Add(word.Texts[text_mode]);
                    }
                }
            }
            return unique_words;
        }

        public int GetVerseNumber(int chapter_number, int verse_number_in_chapter)
        {
            foreach (Chapter chapter in this.chapters)
            {
                if (chapter.Number == chapter_number)
                {
                    foreach (Verse verse in chapter.Verses)
                    {
                        if (verse.NumberInChapter == verse_number_in_chapter)
                        {
                            return verse.Number;
                        }
                    }
                }
            }
            return 0;
        }
        public Verse GetVerseByVerseNumber(long number)
        {
            if ((number > 0) && (number < this.verses.Count))
            {
                int index = (int)number - 1;
                return this.verses[index];
            }
            return null;
        }
        public Verse GetVerseByWordNumber(long number)
        {
            if ((number > 0) && (number < this.WordCount))
            {
                foreach (Chapter chapter in this.chapters)
                {
                    foreach (Verse verse in chapter.Verses)
                    {
                        if (number > verse.Words.Count)
                        {
                            number -= verse.Words.Count;
                        }
                        else
                        {
                            return verse;
                        }
                    }
                }
            }
            return null;
        }
        public Verse GetVerseByLetterNumber(long number)
        {
            if ((number > 0) && (number < this.LetterCount))
            {
                foreach (Chapter chapter in this.chapters)
                {
                    foreach (Verse verse in chapter.Verses)
                    {
                        foreach (Word word in verse.Words)
                        {
                            int letter_count = word.Letters.Count;
                            if (number > letter_count)
                            {
                                number -= letter_count;
                            }
                            else
                            {
                                return verse;
                            }
                        }
                    }
                }
            }
            return null;
        }

        // Singlton pattern.
        // ONE Book only with verse-level translations and simplified_texts
        private static readonly object thread_safe_lock = new object();
        private static Book s_instance = null;
        public static Book Instance
        {
            get
            {
                //if (s_instance == null)
                //{
                //    throw new Exception("Must first call CreateInstance with a title and " + Verse.MAX_NUMBER + " verses.");
                //}
                return s_instance;
            }
        }
        public static Book CreateInstance(string title, List<Verse> verses)
        {
            lock (thread_safe_lock)
            {
                if (s_instance == null)
                {
                    s_instance = new Book(title, verses);
                }
            }
            return s_instance;
        }
        private Book(string title, List<Verse> verses)
        {
            this.title = title;
            this.verses = verses;

            if (this.verses != null)
            {
                foreach (Verse verse in this.verses)
                {
                    verse.Book = this;
                }

                this.chapters = new List<Chapter>();
                this.stations = new List<Station>();
                this.parts = new List<Part>();
                this.groups = new List<Group>();
                this.group_quarters = new List<Quarter>();
                this.bowings = new List<Bowing>();
                this.pages = new List<Page>();
                this.prostrations = new List<Prostration>();

                this.min_words = 1;
                int word_count = 0;
                foreach (Verse verse in this.verses)
                {
                    word_count += verse.Words.Count;
                }
                this.max_words = word_count;

                this.min_letters = 1;
                this.max_letters = int.MaxValue; // verse.Letters is not populated yet

                SetupPartitions();
            }
        }
        private void SetupPartitions()
        {
            if (s_quran_metadata == null)
            {
                LoadQuranMetadata();
            }

            if (s_quran_metadata != null)
            {
                // setup Chapters
                for (int i = 0; i < s_quran_metadata.Chapters.Length; i++)
                {
                    int number = s_quran_metadata.Chapters[i].Number;
                    int verse_count = s_quran_metadata.Chapters[i].Verses;
                    int start_verse = s_quran_metadata.Chapters[i].StartVerse;
                    int end_verse = start_verse + verse_count;
                    string name = s_quran_metadata.Chapters[i].Name;
                    string transliterated_name = s_quran_metadata.Chapters[i].TransliteratedName;
                    string english_name = s_quran_metadata.Chapters[i].EnglishName;
                    int revelation_order = s_quran_metadata.Chapters[i].RevelationOrder;
                    RevelationPlace revelation_place = s_quran_metadata.Chapters[i].RevelationPlace;
                    int bowing_count = s_quran_metadata.Chapters[i].Bowings;

                    List<Verse> verses = new List<Verse>();
                    if (this.verses != null)
                    {
                        for (int j = start_verse; j < end_verse; j++)
                        {
                            int index = j - 1;
                            if ((index >= 0) && (index < this.verses.Count))
                            {
                                Verse verse = this.verses[index];
                                verses.Add(verse);
                            }
                            else
                            {
                                break;
                            }
                        }

                        Chapter chapter = new Chapter(number, name, transliterated_name, english_name, revelation_order, revelation_place, bowing_count, verses);
                        this.chapters.Add(chapter);
                    }
                }

                // setup Bowings
                for (int i = 0; i < s_quran_metadata.Bowings.Length; i++)
                {
                    int number = s_quran_metadata.Bowings[i].Number;
                    int start_chapter = s_quran_metadata.Bowings[i].StartChapter;
                    int start_chapter_verse = s_quran_metadata.Bowings[i].StartChapterVerse;

                    int start_verse = 0;
                    for (int j = 0; j < start_chapter - 1; j++)
                    {
                        start_verse += this.chapters[j].Verses.Count;
                    }
                    start_verse += start_chapter_verse;

                    int next_bowing_start_verse = 0;
                    if (i < s_quran_metadata.Bowings.Length - 1)
                    {
                        int next_bowing_start_chapter = s_quran_metadata.Bowings[i + 1].StartChapter;
                        int next_bowing_start_chapter_verse = s_quran_metadata.Bowings[i + 1].StartChapterVerse;
                        for (int j = 0; j < next_bowing_start_chapter - 1; j++)
                        {
                            next_bowing_start_verse += this.chapters[j].Verses.Count;
                        }
                        next_bowing_start_verse += next_bowing_start_chapter_verse;
                    }
                    else
                    {
                        next_bowing_start_verse = Verse.MAX_NUMBER + 1;
                    }

                    int end_verse = next_bowing_start_verse;

                    List<Verse> verses = new List<Verse>();
                    for (int j = start_verse; j < end_verse; j++)
                    {
                        int index = j - 1;
                        if ((index >= 0) && (index < this.verses.Count))
                        {
                            Verse verse = this.verses[index];
                            verses.Add(verse);
                        }
                        else
                        {
                            break;
                        }
                    }

                    Bowing bowing = new Bowing(number, verses);
                    this.bowings.Add(bowing);
                }

                // setup Group
                for (int i = 0; i < s_quran_metadata.Groups.Length; i++)
                {
                    int number = s_quran_metadata.Groups[i].Number;
                    int start_chapter = s_quran_metadata.Groups[i].StartChapter;
                    int start_chapter_verse = s_quran_metadata.Groups[i].StartChapterVerse;

                    int start_verse = 0;
                    for (int j = 0; j < start_chapter - 1; j++)
                    {
                        start_verse += this.chapters[j].Verses.Count;
                    }
                    start_verse += start_chapter_verse;

                    int next_group_start_verse = 0;
                    if (i < s_quran_metadata.Groups.Length - 1)
                    {
                        int next_group_start_chapter = s_quran_metadata.Groups[i + 1].StartChapter;
                        int next_group_start_chapter_verse = s_quran_metadata.Groups[i + 1].StartChapterVerse;
                        for (int j = 0; j < next_group_start_chapter - 1; j++)
                        {
                            next_group_start_verse += this.chapters[j].Verses.Count;
                        }
                        next_group_start_verse += next_group_start_chapter_verse;
                    }
                    else
                    {
                        next_group_start_verse = Verse.MAX_NUMBER + 1;
                    }

                    int end_verse = next_group_start_verse;

                    List<Verse> verses = new List<Verse>();
                    for (int j = start_verse; j < end_verse; j++)
                    {
                        int index = j - 1;
                        if ((index >= 0) && (index < this.verses.Count))
                        {
                            Verse verse = this.verses[index];
                            verses.Add(verse);
                        }
                        else
                        {
                            break;
                        }
                    }

                    Model.Group group = new Model.Group(number, verses);
                    this.groups.Add(group);
                }

                // setup Quarters
                for (int i = 0; i < s_quran_metadata.Quarters.Length; i++)
                {
                    int number = s_quran_metadata.Quarters[i].Number;
                    int start_chapter = s_quran_metadata.Quarters[i].StartChapter;
                    int start_chapter_verse = s_quran_metadata.Quarters[i].StartChapterVerse;

                    int start_verse = 0;
                    for (int j = 0; j < start_chapter - 1; j++)
                    {
                        start_verse += this.chapters[j].Verses.Count;
                    }
                    start_verse += start_chapter_verse;

                    int next_group_quarter_start_verse = 0;
                    if (i < s_quran_metadata.Quarters.Length - 1)
                    {
                        int next_group_quarter_start_chapter = s_quran_metadata.Quarters[i + 1].StartChapter;
                        int next_group_quarter_start_chapter_verse = s_quran_metadata.Quarters[i + 1].StartChapterVerse;
                        for (int j = 0; j < next_group_quarter_start_chapter - 1; j++)
                        {
                            next_group_quarter_start_verse += this.chapters[j].Verses.Count;
                        }
                        next_group_quarter_start_verse += next_group_quarter_start_chapter_verse;
                    }
                    else
                    {
                        next_group_quarter_start_verse = Verse.MAX_NUMBER + 1;
                    }

                    int end_verse = next_group_quarter_start_verse;

                    List<Verse> verses = new List<Verse>();
                    for (int j = start_verse; j < end_verse; j++)
                    {
                        int index = j - 1;
                        if ((index >= 0) && (index < this.verses.Count))
                        {
                            Verse verse = this.verses[index];
                            verses.Add(verse);
                        }
                        else
                        {
                            break;
                        }
                    }

                    Quarter group_quarter = new Quarter(number, verses);
                    this.group_quarters.Add(group_quarter);
                }

                // setup Pages
                for (int i = 0; i < s_quran_metadata.Pages.Length; i++)
                {
                    int number = s_quran_metadata.Pages[i].Number;
                    int start_chapter = s_quran_metadata.Pages[i].StartChapter;
                    int start_chapter_verse = s_quran_metadata.Pages[i].StartChapterVerse;

                    int start_verse = 0;
                    for (int j = 0; j < start_chapter - 1; j++)
                    {
                        start_verse += this.chapters[j].Verses.Count;
                    }
                    start_verse += start_chapter_verse;

                    int next_page_start_verse = 0;
                    if (i < s_quran_metadata.Pages.Length - 1)
                    {
                        int next_page_start_chapter = s_quran_metadata.Pages[i + 1].StartChapter;
                        int next_page_start_chapter_verse = s_quran_metadata.Pages[i + 1].StartChapterVerse;
                        for (int j = 0; j < next_page_start_chapter - 1; j++)
                        {
                            next_page_start_verse += this.chapters[j].Verses.Count;
                        }
                        next_page_start_verse += next_page_start_chapter_verse;
                    }
                    else
                    {
                        next_page_start_verse = Verse.MAX_NUMBER + 1;
                    }

                    int end_verse = next_page_start_verse;

                    List<Verse> verses = new List<Verse>();
                    for (int j = start_verse; j < end_verse; j++)
                    {
                        int index = j - 1;
                        if ((index >= 0) && (index < this.verses.Count))
                        {
                            Verse verse = this.verses[index];
                            verses.Add(verse);
                        }
                        else
                        {
                            break;
                        }
                    }

                    Page page = new Page(number, verses);
                    this.pages.Add(page);
                }

                // setup Parts
                for (int i = 0; i < s_quran_metadata.Parts.Length; i++)
                {
                    int number = s_quran_metadata.Parts[i].Number;
                    int start_chapter = s_quran_metadata.Parts[i].StartChapter;
                    int start_chapter_verse = s_quran_metadata.Parts[i].StartChapterVerse;

                    int start_verse = 0;
                    for (int j = 0; j < start_chapter - 1; j++)
                    {
                        start_verse += this.chapters[j].Verses.Count;
                    }
                    start_verse += start_chapter_verse;

                    int next_part_start_verse = 0;
                    if (i < s_quran_metadata.Parts.Length - 1)
                    {
                        int next_part_start_chapter = s_quran_metadata.Parts[i + 1].StartChapter;
                        int next_part_start_chapter_verse = s_quran_metadata.Parts[i + 1].StartChapterVerse;
                        for (int j = 0; j < next_part_start_chapter - 1; j++)
                        {
                            next_part_start_verse += this.chapters[j].Verses.Count;
                        }
                        next_part_start_verse += next_part_start_chapter_verse;
                    }
                    else
                    {
                        next_part_start_verse = Verse.MAX_NUMBER + 1;
                    }

                    int end_verse = next_part_start_verse;

                    List<Verse> verses = new List<Verse>();
                    for (int j = start_verse; j < end_verse; j++)
                    {
                        int index = j - 1;
                        if ((index >= 0) && (index < this.verses.Count))
                        {
                            Verse verse = this.verses[index];
                            verses.Add(verse);
                        }
                        else
                        {
                            break;
                        }
                    }

                    Part part = new Part(number, verses);
                    this.parts.Add(part);
                }

                // setup Stations
                for (int i = 0; i < s_quran_metadata.Stations.Length; i++)
                {
                    int number = s_quran_metadata.Stations[i].Number;
                    int start_chapter = s_quran_metadata.Stations[i].StartChapter;
                    int start_chapter_verse = s_quran_metadata.Stations[i].StartChapterVerse;

                    int start_verse = 0;
                    for (int j = 0; j < start_chapter - 1; j++)
                    {
                        start_verse += this.chapters[j].Verses.Count;
                    }
                    start_verse += start_chapter_verse;

                    int next_station_start_verse = 0;
                    if (i < s_quran_metadata.Stations.Length - 1)
                    {
                        int next_station_start_chapter = s_quran_metadata.Stations[i + 1].StartChapter;
                        int next_station_start_chapter_verse = s_quran_metadata.Stations[i + 1].StartChapterVerse;
                        for (int j = 0; j < next_station_start_chapter - 1; j++)
                        {
                            next_station_start_verse += this.chapters[j].Verses.Count;
                        }
                        next_station_start_verse += next_station_start_chapter_verse;
                    }
                    else
                    {
                        next_station_start_verse = Verse.MAX_NUMBER + 1;
                    }

                    int end_verse = next_station_start_verse;

                    List<Verse> verses = new List<Verse>();
                    for (int j = start_verse; j < end_verse; j++)
                    {
                        int index = j - 1;
                        if ((index >= 0) && (index < this.verses.Count))
                        {
                            Verse verse = this.verses[index];
                            verses.Add(verse);
                        }
                        else
                        {
                            break;
                        }
                    }

                    Station station = new Station(number, verses);
                    this.stations.Add(station);
                }

                // setup Prostration
                for (int i = 0; i < s_quran_metadata.Prostrations.Length; i++)
                {
                    int number = s_quran_metadata.Prostrations[i].Number;
                    int chapter = s_quran_metadata.Prostrations[i].Chapter;
                    int chapter_verse = s_quran_metadata.Prostrations[i].ChapterVerse;
                    ProstrationType type = s_quran_metadata.Prostrations[i].Type;

                    int start_verse = 0;
                    for (int j = 0; j < chapter - 1; j++)
                    {
                        start_verse += this.chapters[j].Verses.Count;
                    }
                    start_verse += chapter_verse;

                    if (this.verses.Count > start_verse - 1)
                    {
                        Verse verse = this.verses[start_verse - 1];
                        Prostration prostration = new Prostration(number, type, verse);
                        this.prostrations.Add(prostration);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        // update chapter/verse/word/letter numbers/distances
        public void UpdateBook(string text_mode)
        {
            // update verse/word/letter numbers
            UpdateNumbers();

            // update same verse/word/letter distances
            UpdateDistancesToPrevious(SelectionScope.Book, text_mode);
        }
        private void UpdateNumbers()
        {
            int chapter_number = 1;
            int verse_number = 1;
            int word_number = 1;
            int letter_number = 1;

            if (this.chapters != null)
            {
                // re-build book verses in case of chapter sorting
                this.verses.Clear();
                foreach (Chapter chapter in this.chapters)
                {
                    this.verses.AddRange(chapter.Verses);
                }

                // update verse/word/letter numbers
                foreach (Chapter chapter in this.chapters)
                {
                    chapter.Number = chapter_number++;

                    int verse_number_in_chapter = 1;
                    int word_number_in_chapter = 1;
                    int letter_number_in_chapter = 1;
                    foreach (Verse verse in chapter.Verses)
                    {
                        verse.Number = verse_number++;
                        verse.NumberInChapter = verse_number_in_chapter++;

                        int word_number_in_verse = 1;
                        int letter_number_in_verse = 1;
                        foreach (Word word in verse.Words)
                        {
                            word.Number = word_number++;
                            word.NumberInChapter = word_number_in_chapter++;
                            word.NumberInVerse = word_number_in_verse++;

                            int letter_number_in_word = 1;
                            foreach (Letter letter in word.Letters)
                            {
                                letter.Number = letter_number++;
                                letter.NumberInChapter = letter_number_in_chapter++;
                                letter.NumberInVerse = letter_number_in_verse++;
                                letter.NumberInWord = letter_number_in_word++;
                            }
                        }
                    }
                }
            }
        }
        private void UpdateDistancesToPrevious(SelectionScope scope, string text_mode)
        {
            ////////////////////////////////////////////////////////////////////////////////////
            // reset positions and distances for each chapter
            ////////////////////////////////////////////////////////////////////////////////////
            // there are no repeated chapters so there is no distance to previous same chapter
            // foreach chapter: calculate distance to its previous occurence
            //Dictionary<string, int> chapter_previous_chapter_numbers = new Dictionary<string, int>();

            // foreach verse: calculate distance to its previous occurence
            Dictionary<string, int> verse_previous_verse_numbers = new Dictionary<string, int>();
            Dictionary<string, int> verse_previous_chapter_numbers = new Dictionary<string, int>();

            // foreach word: calculate distance to its previous occurence
            Dictionary<string, int> word_previous_word_numbers = new Dictionary<string, int>();
            Dictionary<string, int> word_previous_verse_numbers = new Dictionary<string, int>();
            Dictionary<string, int> word_previous_chapter_numbers = new Dictionary<string, int>();

            // foreach letter: calculate distance to its previous occurence
            Dictionary<char, int> letter_previous_letter_numbers = new Dictionary<char, int>();
            Dictionary<char, int> letter_previous_word_numbers = new Dictionary<char, int>();
            Dictionary<char, int> letter_previous_verse_numbers = new Dictionary<char, int>();
            Dictionary<char, int> letter_previous_chapter_numbers = new Dictionary<char, int>();
            ////////////////////////////////////////////////////////////////////////////////////

            if (this.chapters != null)
            {
                foreach (Chapter chapter in this.chapters)
                {
                    // there are no repeated chapters so there is no distance to previous same chapter

                    if (scope == SelectionScope.Chapter)
                    {
                        //chapter_previous_chapter_numbers.Clear();
                        verse_previous_verse_numbers.Clear();
                        verse_previous_chapter_numbers.Clear();
                        word_previous_word_numbers.Clear();
                        word_previous_verse_numbers.Clear();
                        word_previous_chapter_numbers.Clear();
                        letter_previous_letter_numbers.Clear();
                        letter_previous_word_numbers.Clear();
                        letter_previous_verse_numbers.Clear();
                        letter_previous_chapter_numbers.Clear();
                    }

                    foreach (Verse verse in chapter.Verses)
                    {
                        string verse_text = verse.GetText(text_mode);
                        if (!verse_previous_verse_numbers.ContainsKey(verse_text))
                        {
                            verse.DistanceToPrevious.dL = -1; // non-applicable
                            verse.DistanceToPrevious.dW = -1; // non-applicable
                            verse.DistanceToPrevious.dV = 0;
                            verse.DistanceToPrevious.dC = 0;
                            verse_previous_verse_numbers.Add(verse_text, verse.Number);
                            verse_previous_chapter_numbers.Add(verse_text, verse.Chapter.Number);
                        }
                        else
                        {
                            verse.DistanceToPrevious.dL = -1; // non-applicable
                            verse.DistanceToPrevious.dW = -1; // non-applicable
                            verse.DistanceToPrevious.dV = verse.Number - verse_previous_verse_numbers[verse_text];
                            verse.DistanceToPrevious.dC = verse.Chapter.Number - verse_previous_chapter_numbers[verse_text];

                            // save latest chapter and verse numbers for next iteration
                            verse_previous_verse_numbers[verse_text] = verse.Number;
                            verse_previous_chapter_numbers[verse_text] = verse.Chapter.Number;
                        }

                        foreach (Word word in verse.Words)
                        {
                            string word_text = word.Texts[text_mode];
                            if (!word_previous_verse_numbers.ContainsKey(word_text))
                            {
                                word.DistanceToPrevious.dL = -1; // non-applicable
                                word.DistanceToPrevious.dW = 0;
                                word.DistanceToPrevious.dV = 0;
                                word.DistanceToPrevious.dC = 0;

                                word_previous_word_numbers.Add(word_text, word.Number);
                                word_previous_verse_numbers.Add(word_text, word.Verse.Number);
                                word_previous_chapter_numbers.Add(word_text, word.Verse.Chapter.Number);
                            }
                            else
                            {
                                word.DistanceToPrevious.dL = -1; // non-applicable
                                word.DistanceToPrevious.dW = word.Number - word_previous_word_numbers[word_text];
                                word.DistanceToPrevious.dV = word.Verse.Number - word_previous_verse_numbers[word_text];
                                word.DistanceToPrevious.dC = word.Verse.Chapter.Number - word_previous_chapter_numbers[word_text];

                                // save latest chapter, verse and word numbers for next iteration
                                word_previous_word_numbers[word_text] = word.Number;
                                word_previous_verse_numbers[word_text] = word.Verse.Number;
                                word_previous_chapter_numbers[word_text] = word.Verse.Chapter.Number;
                            }

                            foreach (Letter letter in word.Letters)
                            {
                                if (!letter_previous_verse_numbers.ContainsKey(letter.Character))
                                {
                                    letter.DistanceToPrevious.dL = 0;
                                    letter.DistanceToPrevious.dW = 0;
                                    letter.DistanceToPrevious.dV = 0;
                                    letter.DistanceToPrevious.dC = 0;

                                    letter_previous_letter_numbers.Add(letter.Character, letter.Number);
                                    letter_previous_word_numbers.Add(letter.Character, letter.Word.Number);
                                    letter_previous_verse_numbers.Add(letter.Character, letter.Word.Verse.Number);
                                    letter_previous_chapter_numbers.Add(letter.Character, letter.Word.Verse.Chapter.Number);
                                }
                                else
                                {
                                    letter.DistanceToPrevious.dL = letter.Number - letter_previous_letter_numbers[letter.Character];
                                    letter.DistanceToPrevious.dW = letter.Word.Number - letter_previous_word_numbers[letter.Character];
                                    letter.DistanceToPrevious.dV = letter.Word.Verse.Number - letter_previous_verse_numbers[letter.Character];
                                    letter.DistanceToPrevious.dC = letter.Word.Verse.Chapter.Number - letter_previous_chapter_numbers[letter.Character];

                                    // save latest chapter, verse, word and letter numbers for next iteration
                                    letter_previous_letter_numbers[letter.Character] = letter.Number;
                                    letter_previous_word_numbers[letter.Character] = letter.Word.Number;
                                    letter_previous_verse_numbers[letter.Character] = letter.Word.Verse.Number;
                                    letter_previous_chapter_numbers[letter.Character] = letter.Word.Verse.Chapter.Number;
                                }
                            }
                        }
                    }
                }
            }
        }


        //chapter verses  start_verse name  transliterated_name english_name  revelation_place  revelation_order  bowings
        //part  start_chapter start_chapter_verse
        //group start_chapter start_chapter_verse
        //quarter start_chapter start_chapter_verse
        //station start_chapter start_chapter_verse
        //bowing start_chapter start_chapter_verse
        //page  start_chapter start_chapter_verse
        //prostration chapter chapter_verse type
        private class QuranMetadataChapter
        {
            public int Number;
            public int Verses;
            public int StartVerse;
            public string Name;
            public string TransliteratedName;
            public string EnglishName;
            public RevelationPlace RevelationPlace;
            public int RevelationOrder;
            public int Bowings;
        }
        private class QuranMetadataPart
        {
            public int Number;
            public int StartChapter;
            public int StartChapterVerse;
        }
        private class QuranMetadataGroup
        {
            public int Number;
            public int StartChapter;
            public int StartChapterVerse;
        }
        private class QuranMetadataQuarter
        {
            public int Number;
            public int StartChapter;
            public int StartChapterVerse;
        }
        private class QuranMetadataStation
        {
            public int Number;
            public int StartChapter;
            public int StartChapterVerse;
        }
        private class QuranMetadataBowing
        {
            public int Number;
            public int StartChapter;
            public int StartChapterVerse;
        }
        private class QuranMetadataPage
        {
            public int Number;
            public int StartChapter;
            public int StartChapterVerse;
        }
        private class QuranMetadataProstration
        {
            public int Number;
            public int Chapter;
            public int ChapterVerse;
            public ProstrationType Type;
        }
        private class QuranMetadata
        {
            public QuranMetadataChapter[] Chapters = new QuranMetadataChapter[Chapter.MAX_NUMBER];
            public QuranMetadataPart[] Parts = new QuranMetadataPart[Part.MAX_NUMBER];
            public QuranMetadataGroup[] Groups = new QuranMetadataGroup[Group.MAX_NUMBER];
            public QuranMetadataQuarter[] Quarters = new QuranMetadataQuarter[Quarter.MAX_NUMBER];
            public QuranMetadataStation[] Stations = new QuranMetadataStation[Station.MAX_NUMBER];
            public QuranMetadataBowing[] Bowings = new QuranMetadataBowing[Bowing.MAX_NUMBER];
            public QuranMetadataPage[] Pages = new QuranMetadataPage[Page.MAX_NUMBER];
            public QuranMetadataProstration[] Prostrations = new QuranMetadataProstration[Prostration.MAX_NUMBER];
        }
        private static QuranMetadata s_quran_metadata = null;
        private static void LoadQuranMetadata()
        {
            if (Directory.Exists(s_data_directory))
            {
                string filename = s_data_directory + "/" + "quran-metadata.txt";
                if (File.Exists(filename))
                {
                    s_quran_metadata = new QuranMetadata();
                    using (StreamReader reader = File.OpenText(filename))
                    {
                        try
                        {
                            //chapter verses  start_verse name  transliterated_name english_name  revelation_place  revelation_order  bowings
                            for (int i = 0; i < Chapter.MAX_NUMBER; i++)
                            {
                                string line = reader.ReadLine();
                                if (line.Length == 0) { i--; continue; }
                                if (line.StartsWith("//")) { i--; continue; }
                                string[] parts = line.Split('\t');
                                QuranMetadataChapter info = new QuranMetadataChapter();
                                info.Number = int.Parse(parts[0]);
                                info.Verses = int.Parse(parts[1]);
                                info.StartVerse = int.Parse(parts[2]);
                                info.Name = parts[3];
                                info.TransliteratedName = parts[4];
                                info.EnglishName = parts[5];
                                info.RevelationPlace = (RevelationPlace)Enum.Parse(typeof(RevelationPlace), parts[6]);
                                info.RevelationOrder = int.Parse(parts[7]);
                                info.Bowings = int.Parse(parts[8]);
                                s_quran_metadata.Chapters[i] = info;
                            }

                            //part  start_chapter start_chapter_verse
                            for (int i = 0; i < Part.MAX_NUMBER; i++)
                            {
                                string line = reader.ReadLine();
                                if (line.Length == 0) { i--; continue; }
                                if (line.StartsWith("//")) { i--; continue; }
                                string[] parts = line.Split('\t');
                                QuranMetadataPart info = new QuranMetadataPart();
                                info.Number = int.Parse(parts[0]);
                                info.StartChapter = int.Parse(parts[1]);
                                info.StartChapterVerse = int.Parse(parts[2]);
                                s_quran_metadata.Parts[i] = info;
                            }

                            //group start_chapter start_chapter_verse
                            for (int i = 0; i < Group.MAX_NUMBER; i++)
                            {
                                string line = reader.ReadLine();
                                if (line.Length == 0) { i--; continue; }
                                if (line.StartsWith("//")) { i--; continue; }
                                string[] Groups = line.Split('\t');
                                QuranMetadataGroup info = new QuranMetadataGroup();
                                info.Number = int.Parse(Groups[0]);
                                info.StartChapter = int.Parse(Groups[1]);
                                info.StartChapterVerse = int.Parse(Groups[2]);
                                s_quran_metadata.Groups[i] = info;
                            }

                            //quarter start_chapter start_chapter_verse
                            for (int i = 0; i < Quarter.MAX_NUMBER; i++)
                            {
                                string line = reader.ReadLine();
                                if (line.Length == 0) { i--; continue; }
                                if (line.StartsWith("//")) { i--; continue; }
                                string[] Quarters = line.Split('\t');
                                QuranMetadataQuarter info = new QuranMetadataQuarter();
                                info.Number = int.Parse(Quarters[0]);
                                info.StartChapter = int.Parse(Quarters[1]);
                                info.StartChapterVerse = int.Parse(Quarters[2]);
                                s_quran_metadata.Quarters[i] = info;
                            }

                            //station start_chapter start_chapter_verse
                            for (int i = 0; i < Station.MAX_NUMBER; i++)
                            {
                                string line = reader.ReadLine();
                                if (line.Length == 0) { i--; continue; }
                                if (line.StartsWith("//")) { i--; continue; }
                                string[] Stations = line.Split('\t');
                                QuranMetadataStation info = new QuranMetadataStation();
                                info.Number = int.Parse(Stations[0]);
                                info.StartChapter = int.Parse(Stations[1]);
                                info.StartChapterVerse = int.Parse(Stations[2]);
                                s_quran_metadata.Stations[i] = info;
                            }

                            //bowing start_chapter start_chapter_verse
                            for (int i = 0; i < Bowing.MAX_NUMBER; i++)
                            {
                                string line = reader.ReadLine();
                                if (line.Length == 0) { i--; continue; }
                                if (line.StartsWith("//")) { i--; continue; }
                                string[] Bowings = line.Split('\t');
                                QuranMetadataBowing info = new QuranMetadataBowing();
                                info.Number = int.Parse(Bowings[0]);
                                info.StartChapter = int.Parse(Bowings[1]);
                                info.StartChapterVerse = int.Parse(Bowings[2]);
                                s_quran_metadata.Bowings[i] = info;
                            }

                            //page  start_chapter start_chapter_verse
                            for (int i = 0; i < Page.MAX_NUMBER; i++)
                            {
                                string line = reader.ReadLine();
                                if (line.Length == 0) { i--; continue; }
                                if (line.StartsWith("//")) { i--; continue; }
                                string[] Pages = line.Split('\t');
                                QuranMetadataPage info = new QuranMetadataPage();
                                info.Number = int.Parse(Pages[0]);
                                info.StartChapter = int.Parse(Pages[1]);
                                info.StartChapterVerse = int.Parse(Pages[2]);
                                s_quran_metadata.Pages[i] = info;
                            }

                            //prostration chapter chapter_verse type
                            for (int i = 0; i < Prostration.MAX_NUMBER; i++)
                            {
                                string line = reader.ReadLine();
                                if (line.Length == 0) { i--; continue; }
                                if (line.StartsWith("//")) { i--; continue; }
                                string[] parts = line.Split('\t');
                                QuranMetadataProstration info = new QuranMetadataProstration();
                                info.Number = int.Parse(parts[0]);
                                info.Chapter = int.Parse(parts[1]);
                                info.ChapterVerse = int.Parse(parts[2]);
                                info.Type = (ProstrationType)Enum.Parse(typeof(ProstrationType), parts[3]);
                                s_quran_metadata.Prostrations[i] = info;
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("LoadQuranMetadata: " + ex.Message);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get chapters with ALL their verses used inside the parameter "verses".
        /// Chapters with some verses used in the parameter "verses" are not returned.  
        /// </summary>
        /// <param name="verses"></param>
        /// <returns></returns>
        public static List<Chapter> GetChapters(List<Verse> verses)
        {
            List<Chapter> result = new List<Chapter>();
            Chapter chapter = null;
            foreach (Verse verse in verses)
            {
                if (chapter != verse.Chapter)
                {
                    chapter = verse.Chapter;
                    if (!result.Contains(chapter))
                    {
                        bool include_chapter = true;
                        foreach (Verse chapter_verse in chapter.Verses)
                        {
                            if (!verses.Contains(chapter_verse))
                            {
                                include_chapter = false;
                                break;
                            }
                        }

                        if (include_chapter)
                        {
                            result.Add(chapter);
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Get verses with ALL their words used inside the parameter "words".
        /// Verses with some words used in the parameter "words" are not returned.  
        /// </summary>
        /// <param name="words"></param>
        /// <returns></returns>
        public static List<Verse> GetVerses(List<Word> words)
        {
            List<Verse> result = new List<Verse>();
            Verse verse = null;
            foreach (Word word in words)
            {
                if (verse != word.Verse)
                {
                    verse = word.Verse;
                    if (!result.Contains(verse))
                    {
                        bool include_verse = true;
                        foreach (Word verse_word in verse.Words)
                        {
                            if (!words.Contains(verse_word))
                            {
                                include_verse = false;
                                break;
                            }
                        }

                        if (include_verse)
                        {
                            result.Add(verse);
                        }
                    }
                }
            }
            return result;
        }

        public string GetText(string text_mode)
        {
            if (this.Verses.Count > 0)
            {
                if (this.Verses[0].Words.Count > 0)
                {
                    if (this.Verses[0].Words[0].Texts.ContainsKey(text_mode))
                    {
                        StringBuilder str = new StringBuilder();
                        if (this.verses != null)
                        {
                            foreach (Verse verse in this.verses)
                            {
                                str.AppendLine(verse.GetText(text_mode));
                            }
                            str.Remove(str.Length - 2, 2);
                        }
                        return str.ToString();
                    }
                }
            }
            return "";
        }

        private int min_words;
        public int MinWords
        {
            get { return min_words; }
        }
        private int max_words;
        public int MaxWords
        {
            get { return max_words; }
        }
        private int min_letters;
        public int MinLetters
        {
            get { return min_letters; }
        }
        private int max_letters;
        public int MaxLetters
        {
            get { return max_letters; }
        }
    }
}
