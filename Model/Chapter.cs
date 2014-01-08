using System;
using System.Collections.Generic;
using System.Text;

namespace Model
{
    public class Chapter
    {
        public const int MIN_NUMBER = 1;
        public const int MAX_NUMBER = 114;
        public const int MIN_VERSE_NUMBER = 3;
        public const int MAX_VERSE_NUMBER = 286;

        private List<Verse> verses = null;
        public List<Verse> Verses
        {
            get { return verses; }
        }

        private long value;
        public long Value
        {
            get { return value; }
            set { this.value = value; }
        }

        private int number;
        public int Number
        {
            get { return number; }
            set { number = value; }
        }

        private string name;
        public string Name
        {
            get { return name; }
        }

        private string transliterated_name;
        public string TransliteratedName
        {
            get { return transliterated_name; }
        }

        private string english_name;
        public string EnglishName
        {
            get { return english_name; }
        }

        private int revelation_order;
        public int RevelationOrder
        {
            get { return revelation_order; }
        }

        private RevelationPlace revelation_place;
        public RevelationPlace RevelationPlace
        {
            get { return revelation_place; }
        }

        private int bowing_count;
        public int BowingCount
        {
            get { return bowing_count; }
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

        public Chapter(int number,
                        string name,
                        string transliterated_name,
                        string english_name,
                        int revelation_order,
                        RevelationPlace revelation_place,
                        int bowing_count,
                        List<Verse> verses)
        {
            this.number = number;
            this.name = name;
            this.transliterated_name = transliterated_name;
            this.english_name = english_name;
            this.revelation_order = revelation_order;
            this.revelation_place = revelation_place;
            this.bowing_count = bowing_count;
            this.verses = verses;
            if (this.verses != null)
            {
                foreach (Verse verse in this.verses)
                {
                    verse.Chapter = this;
                }
            }
        }

        public string GetText(string text_mode)
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
