using System;
using System.Text;
using System.Collections.Generic;

namespace Model
{
    public class WordInfo
    {
        public string Text;
        public string Transliteration;
        public List<string> Roots;
        public List<string> Meanings;
        public int Frequency;
        public List<string> Addresses;
        public List<WordPart> WordParts;

        public override string ToString()
        {
            StringBuilder str = new StringBuilder();

            str.Append(Text + "\t");

            str.Append(Transliteration + "\t");

            if (Roots != null)
            {
                foreach (string root in Roots)
                {
                    str.Append(root + "|");
                }
                str.Remove(str.Length - 1, 1);
            }
            str.Append("\t");

            if (Meanings != null)
            {
                foreach (string meaning in Meanings)
                {
                    str.Append(meaning + "|");
                }
                str.Remove(str.Length - 1, 1);
            }
            str.Append("\t");

            str.Append(Frequency.ToString() + "\t");

            if (Addresses != null)
            {
                foreach (string address in Addresses)
                {
                    str.Append(address + "|");
                }
                str.Remove(str.Length - 1, 1);
            }

            foreach (WordPart word_part in WordParts)
            {
                str.Append(word_part.ToString() + "\r\n");
            }
            str.Remove(str.Length - 2, 2);

            return str.ToString();
        }
    }
}
