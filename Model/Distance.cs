using System;
using System.Text;
using System.Collections.Generic;

namespace Model
{
    /// <summary>
    /// Distance in letters, words, verses, and chapters between two same letters, words, or verses.
    /// </summary>
    public class Distance
    {
        /// <summary>
        /// distance to previous same letter in letters
        /// </summary>
        public int dL = 0;

        /// <summary>
        /// distance to previous same letter/word in words
        /// </summary>
        public int dW = 0;

        /// <summary>
        /// distance to previous same letter/word/verse in verses
        /// </summary>
        public int dV = 0;

        /// <summary>
        /// distance to previous same letter/word/verse in chapters
        /// </summary>
        public int dC = 0;
    }
}
