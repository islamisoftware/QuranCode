using System;
using System.Collections.Generic;

namespace Model
{
    public class Letter
    {
        private Word word = null;
        public Word Word
        {
            get { return word; }
        }

        private int number = 0;
        public int Number
        {
            set { number = value; }
            get
            {
                if (number == 0)
                {
                    if (Book.Instance != null)
                    {
                        bool break_outer_loop = false;
                        foreach (Verse verse in Book.Instance.Verses)
                        {
                            if (verse.Number != this.word.Verse.Number)
                            {
                                number += verse.LetterCount;

                                foreach (Word word in verse.Words)
                                {
                                    if (word.Number != this.word.Number)
                                    {
                                        number += word.Letters.Count;
                                    }
                                    else // found my word
                                    {
                                        number += number_in_word;

                                        break_outer_loop = true;
                                        break;
                                    }
                                }
                                if (break_outer_loop)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
                return number;
            }
        }

        private int number_in_chapter = 0;
        public int NumberInChapter
        {
            set { number_in_chapter = value; }
            get
            {
                if (number_in_chapter == 0)
                {
                    if (this.word.Verse != null)
                    {
                        if (this.word.Verse.Chapter != null)
                        {
                            bool break_outer_loop = false;
                            foreach (Verse verse in this.word.Verse.Chapter.Verses)
                            {
                                if (verse.Number != this.word.Verse.Number)
                                {
                                    number_in_chapter += verse.LetterCount;

                                    foreach (Word word in verse.Words)
                                    {
                                        if (word.Number != this.word.Number)
                                        {
                                            number_in_chapter += word.Letters.Count;
                                        }
                                        else // found my word
                                        {
                                            number_in_chapter += number_in_word;

                                            break_outer_loop = true;
                                            break;
                                        }
                                    }
                                    if (break_outer_loop)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                return number_in_chapter;
            }
        }

        private int number_in_verse = 0;
        public int NumberInVerse
        {
            set { number_in_verse = value; }
            get
            {
                if (number_in_verse == 0)
                {
                    if (this.word.Verse != null)
                    {
                        foreach (Word word in this.word.Verse.Words)
                        {
                            if (word.Number != this.word.Number)
                            {
                                number_in_verse += word.Letters.Count;
                            }
                            else
                            {
                                number_in_verse += number_in_word;
                                break;
                            }
                        }
                    }
                }
                return number_in_verse;
            }
        }

        private int number_in_word;
        public int NumberInWord
        {
            set { number_in_word = value; }
            get { return number_in_word; }
        }

        private Distance distance_to_previous = null;
        public Distance DistanceToPrevious
        {
            set { distance_to_previous = value; }
            get { return distance_to_previous; }
        }

        private char character;
        public char Character
        {
            get { return character; }
        }

        public Letter(Word word, char character, int number_in_word)
        {
            this.word = word;
            this.character = character;
            this.number_in_word = number_in_word;
            this.distance_to_previous = new Distance();
        }
    }
}
