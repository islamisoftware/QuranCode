using System;

namespace Model
{
    public enum TextScope { Book, Selection, HighlightedText };
    public enum SelectionScope { Book, Chapter, Page, Station, Part, Group, Quarter, Bowing, Verse, Word, Letter };

    public enum FindScope { Book, Selection, SearchResult };
    public enum FindType { Text, Similarity, Numbers, Frequency, Prostration, Revelation };

    public enum FindByTextSearchType { Exact, Proximity, Root };
    public enum FindByTextLanguageType { Arabic, Translation };
    public enum FindByTextLocation { Anywhere, AtStart, AtMiddle, AtEnd, AllWords, AnyWord };
    public enum FindByTextWordness { WholeWord, PartOfWord, Any };

    public enum FindBySimilarityMethod { SimilarText, SimilarWords, SimilarStart, SimilarEnd };
    public enum FindBySimilaritySource { CurrentVerse, AllVerses };

    public enum FindByNumbersTextRange { Word, WordRange, Verse, VerseRange, Chapter, ChapterVerses };

    public enum FindByFrequencySumType { NoDuplicateLetters, DuplicateLetters };

    public enum ProstrationType { Recommended, Obligatory };
    public enum FindByProstrationType { None, Obligatory, Recommended, Both };

    public enum RevelationPlace { Makkah, Medina };
    public enum FindByRevelationPlace { None, Makkah, Medina, Both };
}
