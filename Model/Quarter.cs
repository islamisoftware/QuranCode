using System;
using System.Text;
using System.Collections.Generic;

namespace Model
{
    public class Quarter
    {
        public const int MIN_NUMBER = 1;
        public const int MAX_NUMBER = 240;

        private List<Verse> verses = null;
        public List<Verse> Verses
        {
            get { return verses; }
        }

        private int number;
        public int Number
        {
            get { return number; }
        }

        public Quarter(int number, List<Verse> verses)
        {
            this.number = number;
            this.verses = verses;
            if (this.verses != null)
            {
                foreach (Verse verse in this.verses)
                {
                    verse.Quarter = this;
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
