using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Model
{
    public class Word
    {
        private Verse verse = null;
        public Verse Verse
        {
            get { return verse; }
        }

        private int number = -1;
        public int Number
        {
            set { number = value; }
            get
            {
                if (number == -1)
                {
                    if (verse.Book != null)
                    {
                        number = 0;
                        foreach (Verse v in verse.Book.Verses)
                        {
                            if (v.Number == verse.Number)
                            {
                                number += number_in_verse;
                                break;
                            }
                            number += v.Words.Count;
                        }
                    }
                    else
                    {
                        number = -1;
                    }
                }
                return number;
            }
        }

        private int number_in_chapter = -1;
        public int NumberInChapter
        {
            set { number_in_chapter = value; }
            get
            {
                if (number_in_chapter == -1)
                {
                    if (verse != null)
                    {
                        if (verse.Chapter != null)
                        {
                            number_in_chapter = 0;
                            foreach (Verse v in verse.Chapter.Verses)
                            {
                                if (v.Number == verse.Number)
                                {
                                    number_in_chapter += number_in_verse;
                                    break;
                                }
                                number_in_chapter += v.Words.Count;
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

        private int number_in_verse;
        public int NumberInVerse
        {
            set { number_in_verse = value; }
            get { return number_in_verse; }
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
                if (verse != null)
                {
                    if (verse.Chapter != null)
                    {
                        return (this.verse.Chapter.Number.ToString() + ":" + verse.NumberInChapter.ToString() + ":" + number_in_verse.ToString());
                    }
                }
                return "XXX:XXX:XXX";
            }
        }

        private string transliteration = null;
        public string Transliteration
        {
            get
            {
                if (transliteration == null)
                {
                    if (this.Verse.Translations.ContainsKey("en.transliteration"))
                    {
                        string verse_transliteration = this.Verse.Translations["en.transliteration"];
                        string[] parts = verse_transliteration.Split(' ');
                        transliteration = parts[this.NumberInVerse - 1];
                    }
                }
                return transliteration;
            }
        }

        private string meaning;
        public string Meaning
        {
            set { meaning = value; }
            get { return meaning; }
        }

        private List<WordPart> parts = null;
        public List<WordPart> Parts
        {
            get { return parts; }
        }

        private List<string> roots = null;
        public List<string> Roots
        {
            set { roots = value; }
            get { return roots; }
        }

        private string corpus_root = "";
        public string CorpusRoot
        {
            get
            {
                if (String.IsNullOrEmpty(corpus_root))
                {
                    foreach (WordPart part in parts)
                    {
                        if (!String.IsNullOrEmpty(part.Features.Root))
                        {
                            corpus_root = part.Features.Root.ToArabic() + "|";
                        }
                    }
                    if (corpus_root.Length > 0)
                    {
                        corpus_root = corpus_root.Remove(corpus_root.Length - 1, 1);
                    }
                }
                return corpus_root;
            }
        }

        private string corpus_lemma = "";
        public string CorpusLemma
        {
            get
            {
                if (String.IsNullOrEmpty(corpus_lemma))
                {
                    foreach (WordPart part in parts)
                    {
                        if (!String.IsNullOrEmpty(part.Features.Lemma))
                        {
                            corpus_lemma = part.Features.Lemma.ToArabic() + "|";
                        }
                    }
                    if (corpus_lemma.Length > 0)
                    {
                        corpus_lemma = corpus_lemma.Remove(corpus_lemma.Length - 1, 1);
                    }
                }
                return corpus_lemma;
            }
        }

        private string corpus_special_group = "";
        public string CorpusSpecialGroup
        {
            get
            {
                if (String.IsNullOrEmpty(corpus_special_group))
                {
                    foreach (WordPart part in parts)
                    {
                        if (!String.IsNullOrEmpty(part.Features.SpecialGroup))
                        {
                            corpus_special_group += part.Features.SpecialGroup.ToArabic() + "|";
                        }
                    }
                    if (corpus_special_group.Length > 0)
                    {
                        corpus_special_group = corpus_special_group.Remove(corpus_special_group.Length - 1, 1);
                    }
                }
                return corpus_special_group;
            }
        }

        private string arabic_grammar = "";
        public string ArabicGrammar
        {
            get
            {
                if (String.IsNullOrEmpty(arabic_grammar))
                {
                    StringBuilder result = new StringBuilder();

                    StringBuilder str = new StringBuilder();
                    string previous_word_part_address = "";
                    for (int i = 0; i < Parts.Count; i++)
                    {
                        if (previous_word_part_address == Parts[i].Word.Address)
                        {
                            // continue with current word
                            str.Append(Parts[i].ToArabic() + "\r\n");
                        }
                        else // new word
                        {
                            previous_word_part_address = Parts[i].Word.Address;

                            // finish up previous word
                            if (str.Length > 2)
                            {
                                str.Remove(str.Length - 2, 2);
                                result.Append(str.ToString());
                                // clear str for new word
                                str.Length = 0;
                            }

                            // continue with current word
                            str.Append(Parts[i].ToArabic() + "\r\n");
                        }

                        if (i == Parts.Count - 1)
                        {
                            // finish up last word
                            if (str.Length > 2)
                            {
                                str.Remove(str.Length - 2, 2);
                                result.Append(str.ToString());
                            }
                        }
                    }

                    arabic_grammar = result.ToString();
                }

                return arabic_grammar;
            }
        }
        private string english_grammar = "";
        public string EnglishGrammar
        {
            get
            {
                if (String.IsNullOrEmpty(english_grammar))
                {
                    StringBuilder result = new StringBuilder();

                    StringBuilder str = new StringBuilder();
                    string previous_word_part_address = "";
                    for (int i = 0; i < Parts.Count; i++)
                    {
                        if (previous_word_part_address == Parts[i].Word.Address)
                        {
                            // continue with current word
                            str.Append(Parts[i].ToEnglish() + "\r\n");
                        }
                        else // new word
                        {
                            previous_word_part_address = Parts[i].Word.Address;

                            // finish up previous word
                            if (str.Length > 2)
                            {
                                str.Remove(str.Length - 2, 2);
                                result.Append(str.ToString());
                                // clear str for new word
                                str.Length = 0;
                            }

                            // continue with current word
                            str.Append(Parts[i].ToEnglish() + "\r\n");
                        }

                        if (i == Parts.Count - 1)
                        {
                            // finish up last word
                            if (str.Length > 2)
                            {
                                str.Remove(str.Length - 2, 2);
                                result.Append(str.ToString());
                            }
                        }
                    }

                    english_grammar = result.ToString();
                }

                return english_grammar;
            }
        }
        private string grammar = "";
        public string Grammar
        {
            get
            {
                if (String.IsNullOrEmpty(grammar))
                {
                    StringBuilder result = new StringBuilder();

                    StringBuilder str = new StringBuilder();
                    string previous_word_part_address = "";
                    for (int i = 0; i < Parts.Count; i++)
                    {
                        if (previous_word_part_address == Parts[i].Word.Address)
                        {
                            // continue with current word
                            str.Append(Parts[i].ToGrammar() + "\r\n");
                        }
                        else // new word
                        {
                            previous_word_part_address = Parts[i].Word.Address;

                            // finish up previous word
                            if (str.Length > 2)
                            {
                                str.Remove(str.Length - 2, 2);
                                result.Append(str.ToString());
                                // clear str for new word
                                str.Length = 0;
                            }

                            // continue with current word
                            str.Append(Parts[i].ToGrammar() + "\r\n");
                        }

                        if (i == Parts.Count - 1)
                        {
                            // finish up last word
                            if (str.Length > 2)
                            {
                                str.Remove(str.Length - 2, 2);
                                result.Append(str.ToString());
                            }
                        }
                    }

                    grammar = result.ToString();
                }

                return grammar;
            }
        }

        public int GetOccurrence(string text_mode)
        {
            int occurrence = 0;
            Book book = Book.Instance;
            if (book != null)
            {
                bool stop = false;
                foreach (Verse verse in book.Verses)
                {
                    if (!stop)
                    {
                        foreach (Word word in verse.Words)
                        {
                            if (word.Texts[text_mode] == this.Texts[text_mode])
                            {
                                occurrence++;
                            }

                            if (word == this)
                            {
                                stop = true;
                                break;
                            }
                        }
                    }
                }
            }
            return occurrence;
        }
        public int GetOccurrences(string text_mode)
        {
            int occurrences = 0;
            Book book = Book.Instance;
            if (book != null)
            {
                foreach (Verse verse in book.Verses)
                {
                    foreach (Word word in verse.Words)
                    {
                        if (word.Texts[text_mode] == this.Texts[text_mode])
                        {
                            occurrences++;
                        }
                    }
                }
            }
            return occurrences;
        }

        private List<Letter> letters = null;
        public List<Letter> Letters
        {
            get { return letters; }
        }

        private List<Letter> unique_letters = null;
        public List<Letter> UniqueLetters
        {
            get
            {
                if (unique_letters == null)
                {
                    unique_letters = new List<Letter>();
                    foreach (Letter letter in this.Letters)
                    {
                        if (!unique_letters.Contains(letter))
                        {
                            unique_letters.Add(letter);
                        }
                    }
                }
                return unique_letters;
            }
        }

        public Word(Verse verse, int number_in_verse, int position_in_verse, string text)
        {
            this.verse = verse;
            this.number_in_verse = number_in_verse;
            this.distance_to_previous = new Distance();
            this.Positions["Original"] = position_in_verse;
            this.Texts["Original"] = text;

            if (Globals.EDITION == Edition.Grammar)
            {
                this.parts = new List<WordPart>();
            }

            this.letters = new List<Letter>();
        }

        public Dictionary<string, string> Texts = new Dictionary<string, string>();
        public Dictionary<string, int> Positions = new Dictionary<string, int>();
    }
}
