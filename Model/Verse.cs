using System;
using System.Text;
using System.Collections.Generic;

namespace Model
{
    public class Verse
    {
        public const int MIN_NUMBER = 1;
        public const int MAX_NUMBER = 6236;

        private Book book = null;
        public Book Book
        {
            get { return book; }
            set { book = value; }
        }

        private Chapter chapter = null;
        public Chapter Chapter
        {
            get { return chapter; }
            set { chapter = value; }
        }

        private int number;
        public int Number
        {
            get { return number; }
            set { number = value; }
        }

        private int number_in_chapter = -1;
        public int NumberInChapter
        {
            set { number_in_chapter = value; }
            get
            {
                if (number_in_chapter == -1)
                {
                    if (this.book != null)
                    {
                        if (this.book.Chapters != null)
                        {
                            number_in_chapter = number;
                            foreach (Chapter chapter in this.book.Chapters)
                            {
                                if (chapter == this.chapter)
                                {
                                    break;
                                }
                                number_in_chapter -= chapter.Verses.Count;
                            }
                        }
                        else
                        {
                            number_in_chapter = -1;
                        }
                    }
                }
                return number_in_chapter;
            }
        }

        private Distance distance_to_previous = null;
        public Distance DistanceToPrevious
        {
            set { distance_to_previous = value; }
            get { return distance_to_previous; }
        }

        public string Address
        {
            get
            {
                if (chapter != null)
                {
                    return (this.chapter.Number.ToString() + ":" + NumberInChapter.ToString());
                }
                return "0:0";
            }
        }
        public string PaddedAddress
        {
            get
            {
                if (chapter != null)
                {
                    return (this.chapter.Number.ToString("000") + ":" + NumberInChapter.ToString("000"));
                }
                return "000:000";
            }
        }
        public string ArabicAddress
        {
            get
            {
                if (chapter != null)
                {
                    return (this.chapter.Number.ToArabic() + "_" + NumberInChapter.ToArabic());
                }
                return "٠:٠";
            }
        }

        private Station station = null;
        public Station Station
        {
            get { return station; }
            set { station = value; }
        }

        private Part part = null;
        public Part Part
        {
            get { return part; }
            set { part = value; }
        }

        private Group group = null;
        public Group Group
        {
            get { return group; }
            set { group = value; }
        }

        private Quarter quarter = null;
        public Quarter Quarter
        {
            get { return quarter; }
            set { quarter = value; }
        }

        private Bowing bowing = null;
        public Bowing Bowing
        {
            get { return bowing; }
            set { bowing = value; }
        }

        private Page page = null;
        public Page Page
        {
            get { return page; }
            set { page = value; }
        }

        private Prostration prostration = null;
        public Prostration Prostration
        {
            get { return prostration; }
            set { prostration = value; }
        }

        private long value;
        public long Value
        {
            get { return value; }
            set { this.value = value; }
        }

        private List<Word> words = null;
        public List<Word> Words
        {
            get { return words; }
        }

        public int LetterCount
        {
            get
            {
                int letter_count = 0;
                foreach (Word word in this.words)
                {
                    letter_count += word.Letters.Count;
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
                    foreach (Word word in this.words)
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
                return unique_letters;
            }
        }
        public int GetLetterFrequency(char character)
        {
            int result = 0;
            foreach (Word word in this.words)
            {
                foreach (Letter letter in word.Letters)
                {
                    if (letter.Character == character)
                    {
                        result++;
                    }
                }
            }
            return result;
        }

        public const string OPEN_BRACKET = "{";
        public const string CLOSE_BRACKET = "}";
        public string Endmark
        {
            get
            {
                if (s_include_endline)
                {
                    if (s_include_number)
                    {
                        return " " + OPEN_BRACKET + NumberInChapter.ToString().ToArabicNumber() + CLOSE_BRACKET + "\r\n";
                    }
                    else
                    {
                        return "\r\n";
                    }
                }
                else // no endline
                {
                    if (s_include_number)
                    {
                        return " " + OPEN_BRACKET + NumberInChapter.ToString().ToArabicNumber() + CLOSE_BRACKET + " ";
                    }
                    else
                    {
                        return "                        ";
                    }
                }
            }
        }

        private static bool s_include_number = false;
        public static bool IncludeNumber
        {
            get { return s_include_number; }
            set { s_include_number = value; }
        }

        private static bool s_include_endline = true;
        public static bool IncludeEndLine
        {
            get { return s_include_endline; }
            set { s_include_endline = value; }
        }

        /// <summary>
        /// Create a verse with the Original text, use SetText method to add simplified texts later.
        /// </summary>
        /// <param name="number"></param>
        /// <param name="text"></param>
        public Verse(int number, string text)
        {
            this.number = number;
            this.distance_to_previous = new Distance();
            text = text.Replace("\r", "");
            text = text.Replace("\n", "");
            this.text = text;

            this.words = new List<Word>();
            int word_number_in_verse = 0;
            int word_position_in_verse = 0;
            string[] word_texts = text.Split(' ');
            foreach (string word_text in word_texts)
            {
                // skip stop marks (1-letter words), except real Quranic 1-letter words
                if ((word_text.Length == 1) && (word_text != "ص") && (word_text != "ق") && (word_text != "ن"))
                {
                    word_position_in_verse += 2; // stop mark and space after it
                    continue; // skip stop mark
                }
                else
                {
                    word_number_in_verse++;
                    Word word = new Word(this, word_number_in_verse, word_position_in_verse, word_text);
                    this.words.Add(word);
                }

                // in all cases
                word_position_in_verse += word_text.Length + 1; // 1 for space
            }
        }

        private string text = ""; // original text with stop marks
        public string GetText(string text_mode)
        {
            if (text_mode == "Original")
            {
                return text;
            }

            if (this.words.Count > 0)
            {
                if (this.words[0].Texts.ContainsKey(text_mode))
                {
                    StringBuilder str = new StringBuilder();
                    foreach (Word word in this.words)
                    {
                        str.Append(word.Texts[text_mode] + " ");
                    }
                    str.Remove(str.Length - 1, 1); // remove last space
                    return str.ToString();
                }
            }

            return "";
        }
        public void SetText(string text_mode, string text)
        {
            if (text_mode == "Original")
            {
                this.text = text;
            }

            string[] word_texts = text.Split(' ');
            if (word_texts.Length >= this.words.Count) // > if Original with stop marks
            {
                int index = 0;
                int position = 0;
                foreach (string part in word_texts)
                {
                    if (text_mode == "Original")
                    {
                        // skip stop marks (1-letter words), except real Quranic 1-letter words
                        if ((part.Length == 1) && (part != "ص") && (part != "ق") && (part != "ن"))
                        {
                            position += 2; // stop mark and space after it
                            continue; // skip stop mark
                        }
                    }

                    this.words[index].Texts[text_mode] = part;
                    this.words[index].Positions[text_mode] = position;
                    position += part.Length + 1; // 1 for space after word

                    index++;
                }
            }
        }

        public Dictionary<string, string> Translations = new Dictionary<string, string>();
    }
}
