using System;
using System.Text;
using System.Collections.Generic;

namespace Model
{
    /// <summary>
    /// Position of a letter, word, verse, or chapter in their parents.
    /// </summary>
    public class Position
    {
        /// <summary>
        /// letter position in word
        /// </summary>
        public int L = 0;

        /// <summary>
        /// word position in verse
        /// </summary>
        public int W = 0;

        /// <summary>
        /// verse position in chapter
        /// </summary>
        public int V = 0;

        /// <summary>
        /// chapter position in the Quran
        /// </summary>
        public int C = 0;
    }
}
