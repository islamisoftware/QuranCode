using System;
using System.Text;
using System.Collections.Generic;

namespace Model
{
    public class Selection
    {
        private Book book;
        public Book Book
        {
            get { return book; }
        }

        private SelectionScope scope;  // e.g. Chapter
        public SelectionScope Scope
        {
            get { return scope; }
        }

        private List<int> indexes;       // e.g. Chapter indexes
        public List<int> Indexes
        {
            get { return indexes; }
        }

        private List<Verse> verses = null;
        public List<Verse> Verses
        {
            get
            {
                if (verses == null)
                {
                    verses = new List<Verse>();
                    switch (scope)
                    {
                        case SelectionScope.Book:
                            {
                                verses.AddRange(book.Verses);
                            }
                            break;
                        case SelectionScope.Chapter:
                            {
                                foreach (int index in indexes)
                                {
                                    foreach (Chapter chapter in book.Chapters)
                                    {
                                        if ((chapter.Number - 1) == index)
                                        {
                                            verses.AddRange(chapter.Verses);
                                            break;
                                        }
                                    }
                                }
                            }
                            break;
                        case SelectionScope.Page:
                            {
                                foreach (int index in indexes)
                                {
                                    if ((index >= 0) && (index < book.Pages.Count))
                                    {
                                        verses.AddRange(book.Pages[index].Verses);
                                    }
                                }
                            }
                            break;
                        case SelectionScope.Station:
                            {
                                foreach (int index in indexes)
                                {
                                    if ((index >= 0) && (index < book.Stations.Count))
                                    {
                                        verses.AddRange(book.Stations[index].Verses);
                                    }
                                }
                            }
                            break;
                        case SelectionScope.Part:
                            {
                                foreach (int index in indexes)
                                {
                                    if ((index >= 0) && (index < book.Parts.Count))
                                    {
                                        verses.AddRange(book.Parts[index].Verses);
                                    }
                                }
                            }
                            break;
                        case SelectionScope.Group:
                            {
                                foreach (int index in indexes)
                                {
                                    if ((index >= 0) && (index < book.Groups.Count))
                                    {
                                        verses.AddRange(book.Groups[index].Verses);
                                    }
                                }
                            }
                            break;
                        case SelectionScope.Quarter:
                            {
                                foreach (int index in indexes)
                                {
                                    if ((index >= 0) && (index < book.Quarters.Count))
                                    {
                                        verses.AddRange(book.Quarters[index].Verses);
                                    }
                                }
                            }
                            break;
                        case SelectionScope.Bowing:
                            {
                                foreach (int index in indexes)
                                {
                                    if ((index >= 0) && (index < book.Bowings.Count))
                                    {
                                        verses.AddRange(book.Bowings[index].Verses);
                                    }
                                }
                            }
                            break;
                        case SelectionScope.Verse:
                            {
                                foreach (int index in indexes)
                                {
                                    if ((index >= 0) && (index < book.Verses.Count))
                                    {
                                        verses.Add(book.Verses[index]);
                                    }
                                }
                            }
                            break;
                        case SelectionScope.Word:
                            {
                                foreach (int index in indexes)
                                {
                                    Verse verse = book.GetVerseByWordNumber(index + 1);
                                    if (verse != null)
                                    {
                                        verses.Add(verse);
                                    }
                                }
                            }
                            break;
                        case SelectionScope.Letter:
                            {
                                foreach (int index in indexes)
                                {
                                    Verse verse = book.GetVerseByLetterNumber(index + 1);
                                    if (verse != null)
                                    {
                                        verses.Add(verse);
                                    }
                                }
                            }
                            break;
                        default: // Unknown
                            break;
                    }
                }
                return verses;
            }
        }

        private List<Chapter> chapters = null;
        public List<Chapter> Chapters
        {
            get
            {
                if (chapters == null)
                {
                    chapters = new List<Chapter>();
                    switch (scope)
                    {
                        case SelectionScope.Book:
                            {
                                chapters.AddRange(book.Chapters);
                            }
                            break;
                        case SelectionScope.Chapter:
                            {
                                foreach (int index in indexes)
                                {
                                    foreach (Chapter chapter in book.Chapters)
                                    {
                                        if ((chapter.Number - 1) == index)
                                        {
                                            chapters.Add(chapter);
                                            break;
                                        }
                                    }
                                }
                            }
                            break;
                        case SelectionScope.Page:
                            {
                                foreach (int index in indexes)
                                {
                                    if ((index >= 0) && (index < book.Pages.Count))
                                    {
                                        foreach (Verse verse in book.Pages[index].Verses)
                                        {
                                            if (!chapters.Contains(verse.Chapter))
                                            {
                                                chapters.Add(verse.Chapter);
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case SelectionScope.Station:
                            {
                                foreach (int index in indexes)
                                {
                                    if ((index >= 0) && (index < book.Stations.Count))
                                    {
                                        foreach (Verse verse in book.Stations[index].Verses)
                                        {
                                            if (!chapters.Contains(verse.Chapter))
                                            {
                                                chapters.Add(verse.Chapter);
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case SelectionScope.Part:
                            {
                                foreach (int index in indexes)
                                {
                                    if ((index >= 0) && (index < book.Parts.Count))
                                    {
                                        foreach (Verse verse in book.Parts[index].Verses)
                                        {
                                            if (!chapters.Contains(verse.Chapter))
                                            {
                                                chapters.Add(verse.Chapter);
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case SelectionScope.Group:
                            {
                                foreach (int index in indexes)
                                {
                                    if ((index >= 0) && (index < book.Groups.Count))
                                    {
                                        foreach (Verse verse in book.Groups[index].Verses)
                                        {
                                            if (!chapters.Contains(verse.Chapter))
                                            {
                                                chapters.Add(verse.Chapter);
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case SelectionScope.Quarter:
                            {
                                foreach (int index in indexes)
                                {
                                    if ((index >= 0) && (index < book.Quarters.Count))
                                    {
                                        foreach (Verse verse in book.Quarters[index].Verses)
                                        {
                                            if (!chapters.Contains(verse.Chapter))
                                            {
                                                chapters.Add(verse.Chapter);
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case SelectionScope.Bowing:
                            {
                                foreach (int index in indexes)
                                {
                                    if ((index >= 0) && (index < book.Bowings.Count))
                                    {
                                        foreach (Verse verse in book.Bowings[index].Verses)
                                        {
                                            if (!chapters.Contains(verse.Chapter))
                                            {
                                                chapters.Add(verse.Chapter);
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case SelectionScope.Verse:
                            {
                                foreach (int index in indexes)
                                {
                                    if ((index >= 0) && (index < book.Verses.Count))
                                    {
                                        if (!chapters.Contains(book.Verses[index].Chapter))
                                        {
                                            chapters.Add(book.Verses[index].Chapter);
                                        }
                                    }
                                }
                            }
                            break;
                        case SelectionScope.Word:
                            {
                                foreach (int index in indexes)
                                {
                                    Verse verse = book.GetVerseByWordNumber(index + 1);
                                    if (verse != null)
                                    {
                                        if (!chapters.Contains(verse.Chapter))
                                        {
                                            chapters.Add(verse.Chapter);
                                        }
                                    }
                                }
                            }
                            break;
                        case SelectionScope.Letter:
                            {
                                foreach (int index in indexes)
                                {
                                    Verse verse = book.GetVerseByLetterNumber(index + 1);
                                    if (verse != null)
                                    {
                                        if (!chapters.Contains(verse.Chapter))
                                        {
                                            chapters.Add(verse.Chapter);
                                        }
                                    }
                                }
                            }
                            break;
                        default: // Unknown
                            break;
                    }
                }
                return chapters;
            }
        }

        public Selection(Book book, SelectionScope scope, List<int> indexes)
        {
            this.book = book;
            this.scope = scope;
            this.indexes = new List<int>(indexes);
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
