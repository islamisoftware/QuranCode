using System;
using System.Text;

namespace Model
{
    public class Phrase
    {
        private Verse verse = null;
        public Verse Verse
        {
            get { return verse; }
        }

        private string text;
        public string Text
        {
            get { return text; }
        }

        private string text_mode;
        public string TextMode
        {
            get { return text_mode; }
        }

        private int position;
        public int Position
        {
            get { return position; }
        }

        public Phrase(Verse verse, string text_mode, string text, int position)
        {
            this.verse = verse;
            this.text_mode = text_mode;
            this.text = text;
            this.position = position;
        }
    }
}
