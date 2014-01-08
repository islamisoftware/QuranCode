                  QuranCode 1433
             Quran Research Platform
             (c) 2011-2014 Ali Adams
                www.qurancode.com

QuranCode is a free open-source software tool that is built
to enable numerical and similarity research into the Quran.

QuranCode uses 3-layer architecture with a fully OO object model.
Layer 1: Data Access Layer that read/write Quran text and meta files.
Layer 2: Business Logic Layer that provides a Client API for search by
         text/root/similarity/number and many valuation systems.
Layer 3: Presentation Layer that is a clean one-form user interface written
         in WinForms .NET 2.0 to stay compatible with old XP machines.
         New derived projects can use the first two layers without any change
         and only replace the third layer on top of the Client API directly. 

QuranCode Object Model:
//----------------------------
// Server.Book (Singleton)
//
// Book.Verses
// Book.Chapters.Verses
// Book.Stations.Verses
// Book.Parts.Verses
// Book.Groups.Verses
// Book.Quarters.Verses
// Book.Bowings.Verses
// Book.Pages.Verses
//
// Verse.Words
// Verse.WordInfos
// Verse.WordParts
//
// Client.Translations
// Client.Bookmarks
// Client.Selection
//
// Client.FoundChapters
// Client.FoundVerseRanges
// Client.FoundVerses
// Client.FoundPhrases
// Client.FoundWordRanges
// Client.FoundWords
//
// Client.LetterStatistics
// Client.WordStatistics
//----------------------------

where:
  Book = Quran		(1..*) (original and simplified text)
  Chapter = Sura	(1-114)
  Verse = Aya		(1-6236)
  Station = Menzal	(1-7)
  Part = Juz'		(1-30)
  Group = Hisb		(1-60)
  Quarter = Rubi3	(1-240)
  Bowing = Ruk3a	(1-556)

Please feel free to use the software for any purpose GOD Almighty agrees with.

Be a guiding light!

Ali Adams
God > infinity
www.heliwave.com
