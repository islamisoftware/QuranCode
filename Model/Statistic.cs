using System;
using System.Collections.Generic;

namespace Model
{
    //private static CultureInfo arabic = new CultureInfo("ar-SA");
    //// Get the standard StringComparers.
    //private static StringComparer invCmp = StringComparer.InvariantCulture;
    //private static StringComparer invICCmp = StringComparer.InvariantCultureIgnoreCase;
    //private static StringComparer currCmp = StringComparer.CurrentCulture;
    //private static StringComparer currICCmp = StringComparer.CurrentCultureIgnoreCase;
    //private static StringComparer ordCmp = StringComparer.Ordinal;
    //private static StringComparer ordICCmp = StringComparer.OrdinalIgnoreCase;
    //// Create a StringComparer that uses the Turkish culture and ignores case.
    //private static StringComparer arabicICComp = StringComparer.Create(arabic, true);

    public enum StatisticSortMethod { ByOrder, ByLetter, ByFrequency }
    public enum StatisticSortOrder { None, Ascending, Descending }

    public class LetterStatistic : IComparable<LetterStatistic>
    {
        public int Order;
        public char Letter;
        public int Frequency;

        public static StatisticSortMethod SortMethod;
        public static StatisticSortOrder SortOrder;

        public int CompareTo(LetterStatistic obj)
        {
            if (SortOrder == StatisticSortOrder.Ascending)
            {
                if (SortMethod == StatisticSortMethod.ByOrder)
                {
                    return this.Order.CompareTo(obj.Order);
                }
                else if (SortMethod == StatisticSortMethod.ByLetter)
                {
                    return this.Letter.CompareTo(obj.Letter);
                }
                else if (SortMethod == StatisticSortMethod.ByFrequency)
                {
                    if (this.Frequency.CompareTo(obj.Frequency) == 0)
                    {
                        return this.Order.CompareTo(obj.Order);
                    }
                    return this.Frequency.CompareTo(obj.Frequency);
                }
                else
                {
                    return this.Frequency.CompareTo(obj.Frequency);
                }
            }
            else
            {
                if (SortMethod == StatisticSortMethod.ByOrder)
                {
                    return obj.Order.CompareTo(this.Order);
                }
                else if (SortMethod == StatisticSortMethod.ByLetter)
                {
                    return obj.Letter.CompareTo(this.Letter);
                }
                else if (SortMethod == StatisticSortMethod.ByFrequency)
                {
                    if (obj.Frequency.CompareTo(this.Frequency) == 0)
                    {
                        return obj.Order.CompareTo(this.Order);
                    }
                    return obj.Frequency.CompareTo(this.Frequency);
                }
                else
                {
                    return obj.Frequency.CompareTo(this.Frequency);
                }
            }
        }
    }

    public class WordStatistic : IComparable<WordStatistic>
    {
        public int Order;
        public Word Word;
        public int Frequency;

        public static StatisticSortMethod SortMethod;
        public static StatisticSortOrder SortOrder;

        public int CompareTo(WordStatistic obj)
        {
            if (SortOrder == StatisticSortOrder.Ascending)
            {
                if (SortMethod == StatisticSortMethod.ByOrder)
                {
                    return this.Order.CompareTo(obj.Order);
                }
                else if (SortMethod == StatisticSortMethod.ByLetter)
                {
                    return this.Word.Texts["Original"].CompareTo(obj.Word.Texts["Original"]);
                }
                else if (SortMethod == StatisticSortMethod.ByFrequency)
                {
                    if (this.Frequency.CompareTo(obj.Frequency) == 0)
                    {
                        return this.Order.CompareTo(obj.Order);
                    }
                    return this.Frequency.CompareTo(obj.Frequency);
                }
                else
                {
                    if (this.Frequency.CompareTo(obj.Frequency) == 0)
                    {
                        return this.Order.CompareTo(obj.Order);
                    }
                    return this.Frequency.CompareTo(obj.Frequency);
                }
            }
            else
            {
                if (SortMethod == StatisticSortMethod.ByOrder)
                {
                    return obj.Order.CompareTo(this.Order);
                }
                else if (SortMethod == StatisticSortMethod.ByLetter)
                {
                    return obj.Word.Texts["Original"].CompareTo(this.Word.Texts["Original"]);
                }
                else if (SortMethod == StatisticSortMethod.ByFrequency)
                {
                    if (obj.Frequency.CompareTo(this.Frequency) == 0)
                    {
                        return obj.Order.CompareTo(this.Order);
                    }
                    return obj.Frequency.CompareTo(this.Frequency);
                }
                else
                {
                    if (obj.Frequency.CompareTo(this.Frequency) == 0)
                    {
                        return obj.Order.CompareTo(this.Order);
                    }
                    return obj.Frequency.CompareTo(this.Frequency);
                }
            }
        }
    }
}

