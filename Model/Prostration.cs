using System;
using System.Collections.Generic;

namespace Model
{
    public class Prostration
    {
        public const int MIN_NUMBER = 1;
        public const int MAX_NUMBER = 15;

        private int number;
        public int Number
        {
            get { return number; }
        }

        private ProstrationType type;
        public ProstrationType Type
        {
            get { return type; }
        }

        private Verse verse = null;
        public Verse Verse
        {
            get { return verse; }
        }

        public Prostration(int number, ProstrationType type, Verse verse)
        {
            this.number = number;
            this.type = type;
            this.verse = verse;
            if (verse != null)
            {
                verse.Prostration = this;
            }
        }

        public string GetText(string text_mode)
        {
            return verse.GetText(text_mode);
        }
    }
}
