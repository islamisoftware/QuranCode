#region QuranCode Object Model
//----------------------------
// Book
// Book.Verses
// Book.Chapters.Verses
// Book.Stations.Verses
// Book.Parts.Verses
// Book.Groups.Verses
// Book.Quarters.Verses
// Book.Bowings.Verses
// Book.Pages.Verses
// Verse.Words
// Word.Letters
// Client.Bookmarks
// Client.Selection         // readonly, current selection (chapter, station, part, ... , verse, word, letter)
// Client.LetterStatistics  // readonly, statistics for current selection or highlighted text
//----------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Model;

public static partial class Research
{
    /* private methods are available in Research Edition only */
    /* public  methods are available in all editions */

    private static string s_research_directory = "Research";
    static Research()
    {
        if (!Directory.Exists(s_research_directory))
        {
            Directory.CreateDirectory(s_research_directory);
        }
    }

    // No word splitting is allowed, thus all the following methods are for research IDEAS only
    ///////////////////////////////////////////////////////////////////////////////////////////
    // ٦_١١٥	وتمت كلمت ربك صدقا وعدلا لا مبدل لكلمته وهو السميع العليم
    // 6:115	And the Words of your Lord has been prefected full of Truth and Justice:
    //          None can change His Words, and He is the All-hearing, All-knowing.
    ///////////////////////////////////////////////////////////////////////////////////////////
    private static void KeyLettersOnly(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;
        List<Verse> verses = Book.Instance.Verses;

        StringBuilder str = new StringBuilder();
        foreach (Verse verse in verses)
        {
            string verse_text = verse.GetText(client.TextMode);
            verse_text = verse_text.Replace("ف", "");
            verse_text = verse_text.Replace("ف", "");
            verse_text = verse_text.Replace("ز", "");
            verse_text = verse_text.Replace("ء", "");
            verse_text = verse_text.Replace("خ", "");
            verse_text = verse_text.Replace("ش", "");
            verse_text = verse_text.Replace("ظ", "");
            verse_text = verse_text.Replace("ج", "");
            verse_text = verse_text.Replace("ث", "");
            str.AppendLine(verse_text);
        }
        string filename = client.NumerologySystemName + "_" + "KeyLettersOnly" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        PublicStorage.SaveText(path, str.ToString());
        PublicStorage.DisplayFile(path);
    }
    private static void InitialLettersOnly(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;
        List<Verse> verses = Book.Instance.Verses;

        //الم
        //الم
        //المص
        //الر
        //الر
        //المر
        //الر
        //الر
        //الر
        //كهيعص
        //طه
        //طسم
        //طس
        //طسم
        //الم
        //الم
        //الم
        //الم
        //يس
        //ص
        //حم
        //حم
        //حم
        //حم
        //حم
        //حم
        //حم
        //ق
        //ن
        //-----------------
        //ا
        //ل
        //م
        //ص
        //ر
        //ك
        //ه
        //ي
        //ع
        //ط
        //س
        //ح
        //ق
        //ن
        //-----------------
        //ا	13
        //ل	13
        //م	17
        //ص	3
        //ر	6
        //ك	1
        //ه	2
        //ي	2
        //ع	1
        //ط	4
        //س	4
        //ح	7
        //ق	1
        //ن	1

        StringBuilder str = new StringBuilder();
        foreach (Verse verse in verses)
        {
            str.AppendLine(verse.GetText(client.TextMode));
        }
        str.Replace("ب", "");
        str.Replace("د", "");
        str.Replace("و", "");
        str.Replace("ت", "");
        str.Replace("ذ", "");
        str.Replace("غ", "");
        str.Replace("ض", "");
        str.Replace("ف", "");
        str.Replace("ز", "");
        str.Replace("ء", "");
        str.Replace("خ", "");
        str.Replace("ش", "");
        str.Replace("ظ", "");
        str.Replace("ج", "");
        str.Replace("ث", "");

        string filename = client.NumerologySystemName + "_" + "InitialLettersOnly" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        PublicStorage.SaveText(path, str.ToString());
        PublicStorage.DisplayFile(path);
    }
    private static void _________________________________(Client client, string extra)
    {
    }
    private static void JumpLettersByX(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;

        string verses_text = client.Selection.GetText(client.TextMode);
        char[] result = DoJumpLettersByX(client, verses_text, extra);

        string filename = client.NumerologySystemName + "_" + "JumpLettersByX" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        PublicStorage.SaveLetters(path, result);
        PublicStorage.DisplayFile(path);
    }
    private static void JumpLettersByValues(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;

        string verses_text = client.Selection.GetText(client.TextMode);
        char[] result = DoJumpLettersByValues(client, verses_text, extra);

        string filename = client.NumerologySystemName + "_" + "JumpLettersByValues" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        PublicStorage.SaveLetters(path, result);
        PublicStorage.DisplayFile(path);
    }
    private static void JumpLettersByPrimeNumbers(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;

        string verses_text = client.Selection.GetText(client.TextMode);
        char[] result = DoJumpLettersByPrimeNumbers(client, verses_text, extra);

        string filename = client.NumerologySystemName + "_" + "JumpLettersByPrimeNumbers" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        PublicStorage.SaveLetters(path, result);
        PublicStorage.DisplayFile(path);
    }
    private static void JumpLettersByAdditivePrimeNumbers(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;

        string verses_text = client.Selection.GetText(client.TextMode);
        char[] result = DoJumpLettersByAdditivePrimeNumbers(client, verses_text, extra);

        string filename = client.NumerologySystemName + "_" + "JumpLettersByAdditivePrimeNumbers" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        PublicStorage.SaveLetters(path, result);
        PublicStorage.DisplayFile(path);
    }
    private static void JumpLettersByPurePrimeNumbers(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;

        string verses_text = client.Selection.GetText(client.TextMode);
        char[] result = DoJumpLettersByPurePrimeNumbers(client, verses_text, extra);

        string filename = client.NumerologySystemName + "_" + "JumpLettersByPurePrimeNumbers" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        PublicStorage.SaveLetters(path, result);
        PublicStorage.DisplayFile(path);
    }
    private static void JumpLettersByFibonacciNumbers(Client client, string extra)
    {
        // 0, 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144, ...
        if (client == null) return;
        if (client.Selection == null) return;

        string verses_text = client.Selection.GetText(client.TextMode);
        char[] result = DoJumpLettersByFibonacciNumbers(client, verses_text, extra);

        string filename = client.NumerologySystemName + "_" + "JumpLettersByFibonacciNumbers" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        PublicStorage.SaveLetters(path, result);
        PublicStorage.DisplayFile(path);
    }
    private static void JumpLettersByPiDigits(Client client, string extra)
    {
        // 3.14159265358979323846264338327950288419716939937510582097494459230781640628620899862803482534211706798214808651328230664709384460955058223172535940812848111745028410270193852110555964462294895493038196442881097566593344612847564823378678316527120190914564856692346034861045432664821339360726024914127372458700660631558817488152092096282925409171536436789259036001133053054882046652138414695194151160943305727036575959195309218611738193261179310511854807446237996274956735188575272489122793818301194912983367336244065664308602139494639522473719070217986094370277053921717629317675238467481846766940513200056812714526356082778577134275778960917363717872146844090122495343014654958537105079227968925892354201995611212902196086403441815981362977477130996051870721134999999837297804995105973173281609631859502445945534690830264252230825334468503526193118817101000313783875288658753320838142061717766914730359825349042875546873115956286388235378759375195778185778053217122680661300192787661119590921642019893809525720106548586327...
        if (client == null) return;
        if (client.Selection == null) return;

        string verses_text = client.Selection.GetText(client.TextMode);
        char[] result = DoJumpLettersByPiDigits(client, verses_text, "");

        string filename = client.NumerologySystemName + "_" + "JumpLettersByPiDigits" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        PublicStorage.SaveLetters(path, result);
        PublicStorage.DisplayFile(path);
    }
    private static void JumpLettersByEulerDigits(Client client, string extra)
    {
        // 2.71828182845904523536028747135266249775724709369995957496696762772407663035354759457138217852516642742746639193200305992181741359662904357290033429526059563073813232862794349076323382988075319525101901157383418793070215408914993488416750924476146066808226480016847741185374234544243710753907774499206955170276183860626133138458300075204493382656029760673711320070932870912744374704723069697720931014169283681902551510865746377211125238978442505695369677078544996996794686445490598793163688923009879312773617821542499922957635148220826989519366803318252886939849646510582093923982948879332036250944311730123819706841614039701983767932068328237646480429531180232878250981945581530175671736133206981125099618188159304169035159888851934580727386673858942287922849989208680582574927961048419844436346324496848756023362482704197862320900216099023530436994184914631409343173814364054625315209618369088870701676839642437814059271456354906130310720851038375051011574770417189861068739696552126715468895703503540212340784981933432106...
        if (client == null) return;
        if (client.Selection == null) return;

        string verses_text = client.Selection.GetText(client.TextMode);
        char[] result = DoJumpLettersByEulerDigits(client, verses_text, "");

        string filename = client.NumerologySystemName + "_" + "JumpLettersByEulerDigits" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        PublicStorage.SaveLetters(path, result);
        PublicStorage.DisplayFile(path);
    }
    private static void JumpLettersByGoldenRatioDigits(Client client, string extra)
    {
        // 1.61803398874989484820458683436563811772030917980576286213544862270526046281890244970720720418939113748475408807538689175212663386222353693179318006076672635443338908659593958290563832266131992829026788067520876689250171169620703222104321626954862629631361443814975870122034080588795445474924618569536486444924104432077134494704956584678850987433944221254487706647809158846074998871240076521705751797883416625624940758906970400028121042762177111777805315317141011704666599146697987317613560067087480710131795236894275219484353056783002287856997829778347845878228911097625003026961561700250464338243776486102838312683303724292675263116533924731671112115881863851331620384005222165791286675294654906811317159934323597349498509040947621322298101726107059611645629909816290555208524790352406020172799747175342777592778625619432082750513121815628551222480939471234145170223735805772786160086883829523045926478780178899219902707769038953219681986151437803149974110692608867429622675756052317277752035361393621076738937645560606059...
        if (client == null) return;
        if (client.Selection == null) return;

        string verses_text = client.Selection.GetText(client.TextMode);
        char[] result = DoJumpLettersByGoldenRatioDigits(client, verses_text, "");

        string filename = client.NumerologySystemName + "_" + "JumpLettersByGoldenRatioDigits" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        PublicStorage.SaveLetters(path, result);
        PublicStorage.DisplayFile(path);
    }
    private static char[] DoJumpLettersByX(Client client, string verses_text, string extra)
    {
        int count = verses_text.Length;
        char[] result = new char[count];
        //text = verses_text.Replace("\r\n", "_"); // will miss 15 \n without \r before them
        verses_text = verses_text.Replace("\r", "");
        verses_text = verses_text.Replace("\n", "_");
        int new_count = verses_text.Length;
        int i = 0;
        int r = 0;

        int step;
        try
        {
            step = int.Parse(extra);
        }
        catch
        {
            step = 1;
        }
        if (step > 0)
        {
            while (i < new_count)
            {
                if (verses_text[i] == '_')
                {
                    result[r++] = '\r';
                    result[r++] = '\n';
                }
                else
                {
                    result[r++] = verses_text[i];
                }
                i += step;
            }
        }
        return result;
    }
    private static char[] DoJumpLettersByValues(Client client, string verses_text, string extra)
    {
        int count = verses_text.Length;
        char[] result = new char[count];
        //text = verses_text.Replace("\r\n", "_"); // will miss 15 \n without \r before them
        verses_text = verses_text.Replace("\r", "");
        verses_text = verses_text.Replace("\n", "_");
        int new_count = verses_text.Length;
        int i = 0;
        int r = 0;
        while (i < new_count)
        {
            if (verses_text[i] == '_')
            {
                result[r++] = '\r';
                result[r++] = '\n';
                i++; // skip '_'
            }
            else
            {
                result[r++] = verses_text[i];
                try
                {
                    i += (int)client.CalculateValue(verses_text[i].ToString());
                }
                catch
                {
                    // skip exceptions (in Abjad)
                    i += 1;
                }
            }
        }
        return result;
    }
    private static char[] DoJumpLettersByPrimeNumbers(Client client, string verses_text, string extra)
    {
        //text = verses_text.Replace("\r\n", "_"); // will miss 15 \n without \r before them
        verses_text = verses_text.Replace("\r", "");
        verses_text = verses_text.Replace("\n", "_");
        int count = verses_text.Length;
        char[] result = new char[count];
        int r = 0; // result index
        int l = 0; // letter index
        for (int i = 0; i < count - 1; i++) // count - 1 to ignore last newline
        {
            if (Numbers.IsPrime(l + 1))
            {
                if (verses_text[i] == '_')
                {
                    result[r++] = '\r';
                    result[r++] = '\n';
                    i++; // skip '_'
                }
                result[r++] = verses_text[i];
            }
            l++;
        }
        return result;
    }
    private static char[] DoJumpLettersByAdditivePrimeNumbers(Client client, string verses_text, string extra)
    {
        //text = text.Replace("\r\n", "_"); // will miss 15 \n without \r before them
        verses_text = verses_text.Replace("\r", "");
        verses_text = verses_text.Replace("\n", "_");
        int count = verses_text.Length;
        char[] result = new char[count];
        int r = 0; // result index
        int l = 0; // letter index
        for (int i = 0; i < count - 1; i++) // count - 1 to ignore last newline
        {
            if (Numbers.IsAdditivePrime(l + 1))
            {
                if (verses_text[i] == '_')
                {
                    result[r++] = '\r';
                    result[r++] = '\n';
                    i++; // skip '_'
                }
                result[r++] = verses_text[i];
            }
            l++;
        }
        return result;
    }
    private static char[] DoJumpLettersByPurePrimeNumbers(Client client, string verses_text, string extra)
    {
        //text = text.Replace("\r\n", "_"); // will miss 15 \n without \r before them
        verses_text = verses_text.Replace("\r", "");
        verses_text = verses_text.Replace("\n", "_");
        int count = verses_text.Length;
        char[] result = new char[count];
        int r = 0; // result index
        int l = 0; // letter index
        for (int i = 0; i < count - 1; i++) // count - 1 to ignore last newline
        {
            if (Numbers.IsPurePrime(l + 1))
            {
                if (verses_text[i] == '_')
                {
                    result[r++] = '\r';
                    result[r++] = '\n';
                    i++; // skip '_'
                }
                result[r++] = verses_text[i];
            }
            l++;
        }
        return result;
    }
    private static char[] DoJumpLettersByFibonacciNumbers(Client client, string verses_text, string extra)
    {
        //text = text.Replace("\r\n", "_"); // will miss 15 \n without \r before them
        verses_text = verses_text.Replace("\r", "");
        verses_text = verses_text.Replace("\n", "_");
        int count = verses_text.Length;
        char[] result = new char[count];
        int r = 0; // result index
        int l = 0; // letter index
        int N1 = 0;
        int N2 = 1;
        int Fn = N1 + N2;
        result[r++] = verses_text[0]; // add first 1 of Fibonacci numbers (1, 1, 2, 3, 5, 8, ...)
        for (int i = 0; i < count - 1; i++) // count - 1 to ignore last newline
        {
            if (l == (Fn - 1))
            {
                if (verses_text[i] == '_')
                {
                    result[r++] = '\r';
                    result[r++] = '\n';
                    i++; // skip '_'
                }
                result[r++] = verses_text[i];

                // next fibonacci number
                N1 = N2;
                N2 = Fn;
                Fn = N1 + N2;
                if (Fn >= count)
                {
                    break;
                }
            }
            l++;
        }
        return result;
    }
    private static char[] DoJumpLettersByPiDigits(Client client, string verses_text, string extra)
    {
        //text = text.Replace("\r\n", "_"); // will miss 15 \n without \r before them
        verses_text = verses_text.Replace("\r", "");
        verses_text = verses_text.Replace("\n", "_");
        int count = verses_text.Length;
        char[] result = new char[count];
        int r = 0; // result index
        int d = 0; // digit index
        for (int i = 0; i < count; ) // i++) // advance inside loop
        {
            // advance pi_digit amount ignoring white spaces
            int j_count = Numbers.PiDigits[d++];
            for (int j = 0; j < j_count; j++)
            {
                if (i < count)
                {
                    if (verses_text[i] == '_')
                    {
                        result[r++] = '\r';
                        result[r++] = '\n';
                    }
                }

                // in all cases, advance to next letter
                i++;
            }

            // add the 0th-based letter to result
            if ((i - 1) < count)
            {
                result[r++] = verses_text[i - 1];
            }
        }
        string result_str = new String(result);
        result = result_str.ToCharArray();
        return result;
    }
    private static char[] DoJumpLettersByEulerDigits(Client client, string verses_text, string extra)
    {
        //text = text.Replace("\r\n", "_"); // will miss 15 \n without \r before them
        verses_text = verses_text.Replace("\r", "");
        verses_text = verses_text.Replace("\n", "_");
        int count = verses_text.Length;
        char[] result = new char[count];
        int r = 0; // result index
        int d = 0; // digit index
        for (int i = 0; i < count; ) // i++) // advance inside loop
        {
            // advance e_digit amount ignoring white spaces
            int j_count = Numbers.EDigits[d++];
            for (int j = 0; j < j_count; j++)
            {
                if (i < count)
                {
                    if (verses_text[i] == '_')
                    {
                        result[r++] = '\r';
                        result[r++] = '\n';
                    }
                }

                // in all cases, advance to next letter
                i++;
            }

            // add the 0th-based letter to result
            if ((i - 1) < count)
            {
                result[r++] = verses_text[i - 1];
            }
        }
        string result_str = new String(result);
        result = result_str.ToCharArray();
        return result;
    }
    private static char[] DoJumpLettersByGoldenRatioDigits(Client client, string verses_text, string extra)
    {
        //text = text.Replace("\r\n", "_"); // will miss 15 \n without \r before them
        verses_text = verses_text.Replace("\r", "");
        verses_text = verses_text.Replace("\n", "_");
        int count = verses_text.Length;
        char[] result = new char[count];
        int r = 0; // result index
        int d = 0; // digit index
        for (int i = 0; i < count; ) // i++) // advance inside loop
        {
            // advance phi_digit amount ignoring white spaces
            int j_count = Numbers.PhiDigits[d++];
            for (int j = 0; j < j_count; j++)
            {
                if (i < count)
                {
                    if (verses_text[i] == '_')
                    {
                        result[r++] = '\r';
                        result[r++] = '\n';
                    }
                }

                // in all cases, advance to next letter
                i++;
            }

            // add the 0th-based letter to result
            if ((i - 1) < count)
            {
                result[r++] = verses_text[i - 1];
            }
        }
        string result_str = new String(result);
        result = result_str.ToCharArray();
        return result;
    }

    public static void JumpWordsByX(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;

        List<Verse> verses = Book.Instance.Verses;
        List<string> result = DoJumpWordsByX(client, verses, extra);

        string filename = client.NumerologySystemName + "_" + "JumpWordsByX" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        PublicStorage.SaveWords(path, result);
        PublicStorage.DisplayFile(path);
    }
    public static void JumpWordsByPrimeNumbers(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;

        List<Verse> verses = Book.Instance.Verses;
        List<string> result = DoJumpWordsByPrimeNumbers(client, verses, extra);

        string filename = client.NumerologySystemName + "_" + "JumpWordsByPrimeNumbers" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        PublicStorage.SaveWords(path, result);
        PublicStorage.DisplayFile(path);
    }
    public static void JumpWordsByAdditivePrimeNumbers(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;

        List<Verse> verses = Book.Instance.Verses;
        List<string> result = DoJumpWordsByAdditivePrimeNumbers(client, verses, extra);

        string filename = client.NumerologySystemName + "_" + "JumpWordsByAdditivePrimeNumbers" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        PublicStorage.SaveWords(path, result);
        PublicStorage.DisplayFile(path);
    }
    public static void JumpWordsByPurePrimeNumbers(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;

        List<Verse> verses = Book.Instance.Verses;
        List<string> result = DoJumpWordsByPurePrimeNumbers(client, verses, extra);

        string filename = client.NumerologySystemName + "_" + "JumpWordsByPurePrimeNumbers" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        PublicStorage.SaveWords(path, result);
        PublicStorage.DisplayFile(path);
    }
    public static void JumpWordsByFibonacciNumbers(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;

        List<Verse> verses = Book.Instance.Verses;
        List<string> result = DoJumpWordsByFibonacciNumbers(client, verses, extra);

        string filename = client.NumerologySystemName + "_" + "JumpWordsByFibonacciNumbers" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        PublicStorage.SaveWords(path, result);
        PublicStorage.DisplayFile(path);
    }
    public static void JumpWordsByPiDigits(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;

        List<Verse> verses = Book.Instance.Verses;
        List<string> result = DoJumpWordsByPiDigits(client, verses, extra);

        string filename = client.NumerologySystemName + "_" + "JumpWordsByPiDigits" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        PublicStorage.SaveWords(path, result);
        PublicStorage.DisplayFile(path);
    }
    public static void JumpWordsByEulerDigits(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;

        List<Verse> verses = Book.Instance.Verses;
        List<string> result = DoJumpWordsByEulerDigits(client, verses, extra);

        string filename = client.NumerologySystemName + "_" + "JumpWordsByEulerDigits" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        PublicStorage.SaveWords(path, result);
        PublicStorage.DisplayFile(path);
    }
    public static void JumpWordsByGoldenRatioDigits(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;

        List<Verse> verses = Book.Instance.Verses;
        List<string> result = DoJumpWordsByGoldenRatioDigits(client, verses, extra);

        string filename = client.NumerologySystemName + "_" + "JumpWordsByGoldenRatioDigits" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        PublicStorage.SaveWords(path, result);
        PublicStorage.DisplayFile(path);
    }
    private static List<string> DoJumpWordsByX(Client client, List<Verse> verses, string extra)
    {
        List<string> result = new List<string>();

        int step;
        try
        {
            step = int.Parse(extra);
        }
        catch
        {
            step = 1;
        }

        List<Word> words = new List<Word>();
        foreach (Verse verse in verses)
        {
            words.AddRange(verse.Words);
            words.Add(null); // to mark end of verse
        }
        words.RemoveAt(words.Count - 1);

        for (int i = 0; i < words.Count; i++)
        {
            if (words[i] == null)
            {
                result.Add("\r\n");
                words.RemoveAt(i);
            }

            if ((i % step) == 0)
            {
                result.Add(words[i].Texts[client.TextMode] + " ");
            }
        }

        return result;
    }
    private static List<string> DoJumpWordsByPrimeNumbers(Client client, List<Verse> verses, string extra)
    {
        List<string> result = new List<string>();

        List<Word> words = new List<Word>();
        foreach (Verse verse in verses)
        {
            words.AddRange(verse.Words);
            words.Add(null); // to mark end of verse
        }
        words.RemoveAt(words.Count - 1);

        for (int i = 0; i < words.Count; i++)
        {
            if (words[i] == null)
            {
                result.Add("\r\n");
                words.RemoveAt(i);
            }

            if (Numbers.Primes.Contains(i + 1))
            {
                result.Add(words[i].Texts[client.TextMode] + " ");
            }
        }

        return result;
    }
    private static List<string> DoJumpWordsByAdditivePrimeNumbers(Client client, List<Verse> verses, string extra)
    {
        List<string> result = new List<string>();

        List<Word> words = new List<Word>();
        foreach (Verse verse in verses)
        {
            words.AddRange(verse.Words);
            words.Add(null); // to mark end of verse
        }
        words.RemoveAt(words.Count - 1);

        for (int i = 0; i < words.Count; i++)
        {
            if (words[i] == null)
            {
                result.Add("\r\n");
                words.RemoveAt(i);
            }

            if (Numbers.AdditivePrimes.Contains(i + 1))
            {
                result.Add(words[i].Texts[client.TextMode] + " ");
            }
        }

        return result;
    }
    private static List<string> DoJumpWordsByPurePrimeNumbers(Client client, List<Verse> verses, string extra)
    {
        List<string> result = new List<string>();

        List<Word> words = new List<Word>();
        foreach (Verse verse in verses)
        {
            words.AddRange(verse.Words);
            words.Add(null); // to mark end of verse
        }
        words.RemoveAt(words.Count - 1);

        for (int i = 0; i < words.Count; i++)
        {
            if (words[i] == null)
            {
                result.Add("\r\n");
                words.RemoveAt(i);
            }

            if (Numbers.PurePrimes.Contains(i + 1))
            {
                result.Add(words[i].Texts[client.TextMode] + " ");
            }
        }

        return result;
    }
    private static List<string> DoJumpWordsByFibonacciNumbers(Client client, List<Verse> verses, string extra)
    {
        List<string> result = new List<string>();

        List<Word> words = new List<Word>();
        foreach (Verse verse in verses)
        {
            words.AddRange(verse.Words);
            words.Add(null); // to mark end of verse
        }
        words.RemoveAt(words.Count - 1);

        for (int i = 0; i < words.Count; i++)
        {
            if (words[i] == null)
            {
                result.Add("\r\n");
                words.RemoveAt(i);
            }

            if (Numbers.Fibonaccis.Contains(i + 1))
            {
                result.Add(words[i].Texts[client.TextMode] + " ");
            }
        }

        return result;
    }
    private static List<string> DoJumpWordsByPiDigits(Client client, List<Verse> verses, string extra)
    {
        List<string> result = new List<string>();

        List<Word> words = new List<Word>();
        foreach (Verse verse in verses)
        {
            words.AddRange(verse.Words);
            words.Add(null); // to mark end of verse
        }
        words.RemoveAt(words.Count - 1);

        for (int i = 0; i < words.Count; i++)
        {
            if (words[i] == null)
            {
                result.Add("\r\n");
                words.RemoveAt(i);
            }


            //if (Numbers.PiDigits.Contains(i + 1))
            {
                result.Add(words[i].Texts[client.TextMode] + " ");
            }
        }

        return result;
    }
    private static List<string> DoJumpWordsByEulerDigits(Client client, List<Verse> verses, string extra)
    {
        List<string> result = new List<string>();

        return result;
    }
    private static List<string> DoJumpWordsByGoldenRatioDigits(Client client, List<Verse> verses, string extra)
    {
        List<string> result = new List<string>();

        return result;
    }

    public static void __________________________________(Client client, string extra)
    {
    }
    public static void PrimeLetters(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;

        string verses_text = client.Selection.GetText(client.TextMode);
        char[] result = DoPrimeLetters(client, verses_text, extra);

        string filename = client.NumerologySystemName + "_" + "PrimeLetters" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        PublicStorage.SaveLetters(path, result);
        PublicStorage.DisplayFile(path);
    }
    public static void AdditivePrimeLetters(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;

        string verses_text = client.Selection.GetText(client.TextMode);
        char[] result = DoAdditivePrimeLetters(client, verses_text, extra);

        string filename = client.NumerologySystemName + "_" + "AdditivePrimeLetters" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        PublicStorage.SaveLetters(path, result);
        PublicStorage.DisplayFile(path);
    }
    public static void PurePrimeLetters(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;

        string verses_text = client.Selection.GetText(client.TextMode);
        char[] result = DoPurePrimeLetters(client, verses_text, extra);

        string filename = client.NumerologySystemName + "_" + "PurePrimeLetters" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        PublicStorage.SaveLetters(path, result);
        PublicStorage.DisplayFile(path);
    }
    public static void FibonacciLetters(Client client, string extra)
    {
        // 0, 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144, ...
        if (client == null) return;
        if (client.Selection == null) return;

        string verses_text = client.Selection.GetText(client.TextMode);
        char[] result = DoFibonacciLetters(client, verses_text, extra);

        string filename = client.NumerologySystemName + "_" + "FibonacciLetters" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        PublicStorage.SaveLetters(path, result);
        PublicStorage.DisplayFile(path);
    }
    private static char[] DoPrimeLetters(Client client, string verses_text, string extra)
    {
        //text = verses_text.Replace("\r\n", "_"); // will miss 15 \n without \r before them
        verses_text = verses_text.Replace("\r", "");
        verses_text = verses_text.Replace("\n", "_");
        int count = verses_text.Length;
        char[] result = new char[count];
        int r = 0; // result index
        int l = 0; // letter index
        for (int i = 0; i < count - 1; i++) // count - 1 to ignore last newline
        {
            if (Numbers.IsPrime(l + 1))
            {
                if (verses_text[i] == '_')
                {
                    result[r++] = '\r';
                    result[r++] = '\n';
                    i++; // skip '_'
                }
                result[r++] = verses_text[i];
            }
            l++;
        }
        return result;
    }
    private static char[] DoAdditivePrimeLetters(Client client, string verses_text, string extra)
    {
        //text = text.Replace("\r\n", "_"); // will miss 15 \n without \r before them
        verses_text = verses_text.Replace("\r", "");
        verses_text = verses_text.Replace("\n", "_");
        int count = verses_text.Length;
        char[] result = new char[count];
        int r = 0; // result index
        int l = 0; // letter index
        for (int i = 0; i < count - 1; i++) // count - 1 to ignore last newline
        {
            if (Numbers.IsAdditivePrime(l + 1))
            {
                if (verses_text[i] == '_')
                {
                    result[r++] = '\r';
                    result[r++] = '\n';
                    i++; // skip '_'
                }
                result[r++] = verses_text[i];
            }
            l++;
        }
        return result;
    }
    private static char[] DoPurePrimeLetters(Client client, string verses_text, string extra)
    {
        //text = text.Replace("\r\n", "_"); // will miss 15 \n without \r before them
        verses_text = verses_text.Replace("\r", "");
        verses_text = verses_text.Replace("\n", "_");
        int count = verses_text.Length;
        char[] result = new char[count];
        int r = 0; // result index
        int l = 0; // letter index
        for (int i = 0; i < count - 1; i++) // count - 1 to ignore last newline
        {
            if (Numbers.IsPurePrime(l + 1))
            {
                if (verses_text[i] == '_')
                {
                    result[r++] = '\r';
                    result[r++] = '\n';
                    i++; // skip '_'
                }
                result[r++] = verses_text[i];
            }
            l++;
        }
        return result;
    }
    private static char[] DoFibonacciLetters(Client client, string verses_text, string extra)
    {
        //text = text.Replace("\r\n", "_"); // will miss 15 \n without \r before them
        verses_text = verses_text.Replace("\r", "");
        verses_text = verses_text.Replace("\n", "_");
        int count = verses_text.Length;
        char[] result = new char[count];
        int r = 0; // result index
        int l = 0; // letter index
        for (int i = 0; i < count - 1; i++) // count - 1 to ignore last newline
        {
            if (Numbers.IsFibonacci(l + 1))
            {
                if (verses_text[i] == '_')
                {
                    result[r++] = '\r';
                    result[r++] = '\n';
                    i++; // skip '_'
                }
                result[r++] = verses_text[i];
            }
            l++;
        }
        return result;
    }

    public static void ___________________________________(Client client, string extra)
    {
    }

    public static void PrimeWords(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;

        List<Verse> verses = Book.Instance.Verses;
        List<string> result = DoPrimeWords(client, verses, extra);

        string filename = client.NumerologySystemName + "_" + "PrimeWords" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        PublicStorage.SaveWords(path, result);
        PublicStorage.DisplayFile(path);
    }
    public static void AdditivePrimeWords(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;

        List<Verse> verses = Book.Instance.Verses;
        List<string> result = DoAdditivePrimeWords(client, verses, extra);

        string filename = client.NumerologySystemName + "_" + "AdditivePrimeWords" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        PublicStorage.SaveWords(path, result);
        PublicStorage.DisplayFile(path);
    }
    public static void PurePrimeWords(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;

        List<Verse> verses = Book.Instance.Verses;
        List<string> result = DoPurePrimeWords(client, verses, extra);

        string filename = client.NumerologySystemName + "_" + "PurePrimeWords" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        PublicStorage.SaveWords(path, result);
        PublicStorage.DisplayFile(path);
    }
    public static void FibonacciWords(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;

        List<Verse> verses = Book.Instance.Verses;
        List<string> result = DoFibonacciWords(client, verses, extra);

        string filename = client.NumerologySystemName + "_" + "FibonacciWords" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        PublicStorage.SaveWords(path, result);
        PublicStorage.DisplayFile(path);
    }
    private static List<string> DoPrimeWords(Client client, List<Verse> verses, string extra)
    {
        List<string> result = new List<string>();

        List<Word> words = new List<Word>();
        foreach (Verse verse in verses)
        {
            words.AddRange(verse.Words);
            words.Add(null); // to mark end of verse
        }
        words.RemoveAt(words.Count - 1);

        for (int i = 0; i < words.Count; i++)
        {
            if (words[i] == null)
            {
                result.Add("\r\n");
                words.RemoveAt(i);
            }

            if (Numbers.IsPrime(i + 1))
            {
                result.Add(words[i].Texts[client.TextMode] + " ");
            }
        }

        return result;
    }
    private static List<string> DoAdditivePrimeWords(Client client, List<Verse> verses, string extra)
    {
        List<string> result = new List<string>();

        List<Word> words = new List<Word>();
        foreach (Verse verse in verses)
        {
            words.AddRange(verse.Words);
            words.Add(null); // to mark end of verse
        }
        words.RemoveAt(words.Count - 1);

        for (int i = 0; i < words.Count; i++)
        {
            if (words[i] == null)
            {
                result.Add("\r\n");
                words.RemoveAt(i);
            }

            if (Numbers.IsAdditivePrime(i + 1))
            {
                result.Add(words[i].Texts[client.TextMode] + " ");
            }
        }

        return result;
    }
    private static List<string> DoPurePrimeWords(Client client, List<Verse> verses, string extra)
    {
        List<string> result = new List<string>();

        List<Word> words = new List<Word>();
        foreach (Verse verse in verses)
        {
            words.AddRange(verse.Words);
            words.Add(null); // to mark end of verse
        }
        words.RemoveAt(words.Count - 1);

        for (int i = 0; i < words.Count; i++)
        {
            if (words[i] == null)
            {
                result.Add("\r\n");
                words.RemoveAt(i);
            }

            if (Numbers.IsPurePrime(i + 1))
            {
                result.Add(words[i].Texts[client.TextMode] + " ");
            }
        }

        return result;
    }
    private static List<string> DoFibonacciWords(Client client, List<Verse> verses, string extra)
    {
        List<string> result = new List<string>();

        List<Word> words = new List<Word>();
        foreach (Verse verse in verses)
        {
            words.AddRange(verse.Words);
            words.Add(null); // to mark end of verse
        }
        words.RemoveAt(words.Count - 1);

        for (int i = 0; i < words.Count; i++)
        {
            if (words[i] == null)
            {
                result.Add("\r\n");
                words.RemoveAt(i);
            }

            if (Numbers.IsFibonacci(i + 1))
            {
                result.Add(words[i].Texts[client.TextMode] + " ");
            }
        }

        return result;
    }

    public static void ____________________________________(Client client, string extra)
    {
    }
    public static void ChapterVersesSound(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;
        List<Verse> verses = Book.Instance.Verses;

        List<Chapter> chapters = Book.GetChapters(verses);

        List<long> values = new List<long>();
        foreach (Chapter chapter in chapters)
        {
            values.Add(chapter.Verses.Count);
        }

        string filename = client.NumerologySystemName + "_" + "ChapterVerses" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        DoSaveAndPlayWAVFile(path, values, extra);
    }
    public static void ChapterWordsSound(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;
        List<Verse> verses = Book.Instance.Verses;

        List<Chapter> chapters = Book.GetChapters(verses);

        List<long> values = new List<long>();
        foreach (Chapter chapter in chapters)
        {
            values.Add(chapter.WordCount);
        }

        string filename = client.NumerologySystemName + "_" + "ChapterWords" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        DoSaveAndPlayWAVFile(path, values, extra);
    }
    public static void ChapterLettersSound(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;
        List<Verse> verses = Book.Instance.Verses;

        List<Chapter> chapters = Book.GetChapters(verses);

        List<long> values = new List<long>();
        foreach (Chapter chapter in chapters)
        {
            values.Add(chapter.LetterCount);
        }

        string filename = client.NumerologySystemName + "_" + "ChapterLetters" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        DoSaveAndPlayWAVFile(path, values, extra);
    }
    public static void ChapterValuesSound(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;
        List<Verse> verses = Book.Instance.Verses;

        List<Chapter> chapters = Book.GetChapters(verses);

        List<long> values = new List<long>();
        foreach (Chapter chapter in chapters)
        {
            values.Add(chapter.Value);
        }

        string filename = client.NumerologySystemName + "_" + "ChapterValues" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        DoSaveAndPlayWAVFile(path, values, extra);
    }
    private static void ChapterValueDiffsSound(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;
        List<Verse> verses = Book.Instance.Verses;

        List<Chapter> chapters = Book.GetChapters(verses);

        List<long> values = new List<long>();
        foreach (Chapter chapter in chapters)
        {
            values.Add(chapter.Value);
        }

        List<long> value_diffs = new List<long>();
        for (int i = 0; i < values.Count - 1; i++)
        {
            long value_diff = values[i + 1] - values[i];
            value_diffs.Add(value_diff);
        }

        string filename = client.NumerologySystemName + "_" + "ChapterValueDiffs" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        DoSaveAndPlayWAVFile(path, values, extra);
    }
    public static void VerseWordsSound(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;
        List<Verse> verses = Book.Instance.Verses;

        List<long> values = new List<long>();
        foreach (Verse verse in verses)
        {
            values.Add(verse.Words.Count);
        }

        string filename = client.NumerologySystemName + "_" + "VerseWords" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        DoSaveAndPlayWAVFile(path, values, extra);
    }
    public static void VerseLettersSound(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;
        List<Verse> verses = Book.Instance.Verses;

        List<long> values = new List<long>();
        foreach (Verse verse in verses)
        {
            values.Add(verse.LetterCount);
        }

        string filename = client.NumerologySystemName + "_" + "VerseLetters" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        DoSaveAndPlayWAVFile(path, values, extra);
    }
    public static void VerseValuesSound(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;
        List<Verse> verses = Book.Instance.Verses;

        List<long> values = new List<long>();
        foreach (Verse verse in verses)
        {
            values.Add(verse.Value);
        }

        string filename = client.NumerologySystemName + "_" + "VerseValues" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        DoSaveAndPlayWAVFile(path, values, extra);
    }
    private static void VerseValueDiffsSound(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;
        List<Verse> verses = Book.Instance.Verses;

        List<long> values = new List<long>();
        foreach (Verse verse in verses)
        {
            values.Add(verse.Value);
        }

        List<long> value_diffs = new List<long>();
        for (int i = 0; i < values.Count - 1; i++)
        {
            long value_diff = values[i + 1] - values[i];
            value_diffs.Add(value_diff);
        }

        string filename = client.NumerologySystemName + "_" + "VerseValueDiffs" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        DoSaveAndPlayWAVFile(path, values, extra);
    }
    public static void WordLettersSound(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;
        List<Verse> verses = Book.Instance.Verses;

        List<long> values = new List<long>();
        foreach (Verse verse in verses)
        {
            foreach (Word word in verse.Words)
            {
                values.Add(word.Letters.Count);
            }
        }

        string filename = client.NumerologySystemName + "_" + "WordLetters" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        DoSaveAndPlayWAVFile(path, values, extra);
    }
    public static void WordValuesSound(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;
        List<Verse> verses = Book.Instance.Verses;

        List<long> values = new List<long>();
        foreach (Verse verse in verses)
        {
            foreach (Word word in verse.Words)
            {
                values.Add(client.CalculateValue(word));
            }
        }

        string filename = client.NumerologySystemName + "_" + "WordValues" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        DoSaveAndPlayWAVFile(path, values, extra);
    }
    private static void WordValueDiffsSound(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;
        List<Verse> verses = Book.Instance.Verses;

        List<long> values = new List<long>();
        foreach (Verse verse in verses)
        {
            foreach (Word word in verse.Words)
            {
                values.Add(client.CalculateValue(word));
            }
        }

        List<long> value_diffs = new List<long>();
        for (int i = 0; i < values.Count - 1; i++)
        {
            long value_diff = values[i + 1] - values[i];
            value_diffs.Add(value_diff);
        }

        string filename = client.NumerologySystemName + "_" + "WordValueDiffs" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        DoSaveAndPlayWAVFile(path, values, extra);
    }
    public static void LetterValuesSound(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;
        List<Verse> verses = Book.Instance.Verses;

        List<long> values = new List<long>();
        foreach (Verse verse in verses)
        {
            for (int w = 0; w < verse.Words.Count; w++)
            {
                for (int l = 0; l < verse.Words[w].Texts[client.TextMode].Length; l++)
                {
                    values.Add(client.CalculateValue(verse, w, l));
                }
            }
        }

        string filename = client.NumerologySystemName + "_" + "LetterValues" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        DoSaveAndPlayWAVFile(path, values, extra);
    }
    private static void LetterValueDiffsSound(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;
        List<Verse> verses = Book.Instance.Verses;

        List<long> values = new List<long>();
        foreach (Verse verse in verses)
        {
            for (int w = 0; w < verse.Words.Count; w++)
            {
                for (int l = 0; l < verse.Words[w].Texts[client.TextMode].Length; l++)
                {
                    values.Add(client.CalculateValue(verse, w, l));
                }
            }
        }

        List<long> value_diffs = new List<long>();
        for (int i = 0; i < values.Count - 1; i++)
        {
            long value_diff = values[i + 1] - values[i];
            value_diffs.Add(value_diff);
        }

        string filename = client.NumerologySystemName + "_" + "LetterValueDiffs" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        DoSaveAndPlayWAVFile(path, values, extra);
    }
    private static void SameLetterGapsSound(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;
        List<Verse> verses = Book.Instance.Verses;

        StringBuilder str = new StringBuilder();
        foreach (Verse verse in verses)
        {
            str.Append(verse.GetText(client.TextMode));
        }
        str.Replace("\r", "");
        str.Replace("\n", "");
        str.Replace(" ", "");
        int count = str.Length;
        List<long> values = new List<long>(count);

        for (int i = 0; i < count; i++)
        {
            bool found = false;
            for (int j = i + 1; j < count; j++)
            {
                if (str[j] == str[i])
                {
                    values.Add(j - i);
                    found = true;
                    break; // j
                }
            }
            if (!found) // round roubin
            {
                for (int j = 0; j < i + 1; j++)
                {
                    if (str[j] == str[i])
                    {
                        values.Add(j - i + count);
                        found = true;
                        break; // j
                    }
                }
            }
            if (!found) // unique letter
            {
                values.Add(0);
            }
        }

        string filename = client.NumerologySystemName + "_" + "SameLetterGaps" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        DoSaveAndPlayWAVFile(path, values, extra);
    }
    private static void SameLetterBackwardGapsSound(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;
        List<Verse> verses = Book.Instance.Verses;

        StringBuilder str = new StringBuilder();
        foreach (Verse verse in verses)
        {
            str.Append(verse.GetText(client.TextMode));
        }
        str.Replace("\r", "");
        str.Replace("\n", "");
        str.Replace(" ", "");
        int count = str.Length;
        List<long> values = new List<long>(count);

        for (int i = 0; i < count; i++)
        {
            bool found = false;
            for (int j = i - 1; j >= 0; j--)
            {
                if (str[j] == str[i])
                {
                    values.Add(-j + i);
                    found = true;
                    break; // j
                }
            }
            if (!found) // round roubin
            {
                for (int j = count - 1; j > i; j--)
                {
                    if (str[j] == str[i])
                    {
                        values.Add(-j + i + count);
                        found = true;
                        break; // j
                    }
                }
            }
            if (!found) // unique letter
            {
                values.Add(0);
            }
        }

        string filename = client.NumerologySystemName + "_" + "SameLetterBackwardGaps" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        DoSaveAndPlayWAVFile(path, values, extra);
    }
    private static void DoSaveAndPlayWAVFile(string path, List<long> values, string extra)
    {
        PublicStorage.SaveValues(path, values);
        PublicStorage.DisplayFile(path); // *.csv
        int frequency = 0;
        if (extra.Length == 0)
        {
            // update ref path.csv to .wav
            WAVFile.GenerateWAVFile(ref path, values, Globals.DEFAULT_FREQUENCY);
        }
        else if (int.TryParse(extra, out frequency))
        {
            // update ref path.csv to .wav
            WAVFile.GenerateWAVFile(ref path, values, frequency);
        }
        else
        {
            throw new Exception("Invalid frequency value = " + extra);
        }
        // play .wav file
        WAVFile.PlayWAVFile(path);
    }

    public static void ______________________________________(Client client, string extra)
    {
    }
    public static void AllahWords(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;
        List<Verse> verses = Book.Instance.Verses;

        string filename = "AllahWords" + Globals.OUTPUT_FILE_EXT;
        string result = DoAllahWords(client, verses);
        string path = s_research_directory + "/" + filename;
        PublicStorage.SaveText(path, result);
        PublicStorage.DisplayFile(path);
    }
    public static void NotAllahWords(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;
        List<Verse> verses = Book.Instance.Verses;

        string filename = "NotAllahWords" + Globals.OUTPUT_FILE_EXT;
        string result = DoNotAllahWords(client, verses);
        string path = s_research_directory + "/" + filename;
        PublicStorage.SaveText(path, result);
        PublicStorage.DisplayFile(path);
    }
    public static void WordMeaning(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;
        List<Verse> verses = Book.Instance.Verses;

        string filename = "WordMeaning" + Globals.OUTPUT_FILE_EXT;
        string result = DoWordMeaning(client, verses);
        string path = s_research_directory + "/" + filename;
        PublicStorage.SaveText(path, result);
        PublicStorage.DisplayFile(path);
    }
    public static void WordTransliteration(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;
        List<Verse> verses = Book.Instance.Verses;

        string filename = "WordTransliteration" + Globals.OUTPUT_FILE_EXT;
        string result = DoWordTransliteration(client, verses);
        string path = s_research_directory + "/" + filename;
        PublicStorage.SaveText(path, result);
        PublicStorage.DisplayFile(path);
    }
    public static void WordRoots(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;
        List<Verse> verses = Book.Instance.Verses;

        string filename = "WordRoots" + Globals.OUTPUT_FILE_EXT;
        string result = DoWordRoots(client, verses);
        string path = s_research_directory + "/" + filename;
        PublicStorage.SaveText(path, result);
        PublicStorage.DisplayFile(path);
    }
    public static void WordFrequency(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;
        List<Verse> verses = Book.Instance.Verses;

        string filename = "WordFrequency" + Globals.OUTPUT_FILE_EXT;
        string result = DoWordFrequency(client, verses);
        string path = s_research_directory + "/" + filename;
        PublicStorage.SaveText(path, result);
        PublicStorage.DisplayFile(path);
    }
    public static void WordOccurrences(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;
        List<Verse> verses = Book.Instance.Verses;

        string filename = "WordOccurrences" + Globals.OUTPUT_FILE_EXT;
        string result = DoWordOccurrences(client, verses);
        string path = s_research_directory + "/" + filename;
        PublicStorage.SaveText(path, result);
        PublicStorage.DisplayFile(path);
    }
    public static void WordInformation(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;
        List<Verse> verses = Book.Instance.Verses;

        string filename = "WordInformation" + Globals.OUTPUT_FILE_EXT;
        string result = DoWordInformation(client, verses);
        string path = s_research_directory + "/" + filename;
        PublicStorage.SaveText(path, result);
        PublicStorage.DisplayFile(path);
    }
    public static void WordPartInformation(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;
        List<Verse> verses = Book.Instance.Verses;

        string filename = "WordPartInformation" + Globals.OUTPUT_FILE_EXT;
        string result = DoWordPartInformation(client, verses);
        string path = s_research_directory + "/" + filename;
        PublicStorage.SaveText(path, result);
        PublicStorage.DisplayFile(path);
    }
    private static string DoAllahWords(Client client, List<Verse> verses)
    {
        StringBuilder str = new StringBuilder();
        if (verses != null)
        {
            if (verses.Count > 0)
            {
                try
                {
                    str.AppendLine
                        (
                            "#" + "\t" +
                            "Address" + "\t" +
                            "Text" + "\t" +
                            "Verse" + "\t" +
                            "Word"
                          );

                    int count = 0;
                    foreach (Verse verse in verses)
                    {
                        foreach (Word word in verse.Words)
                        {
                            if (
                                (word.Texts[client.TextMode] == "الله") ||
                                (word.Texts[client.TextMode] == "ءالله") ||
                                (word.Texts[client.TextMode] == "ابالله") ||
                                (word.Texts[client.TextMode] == "اللهم") ||
                                (word.Texts[client.TextMode] == "بالله") ||
                                (word.Texts[client.TextMode] == "تالله") ||
                                (word.Texts[client.TextMode] == "فالله") ||
                                (word.Texts[client.TextMode] == "والله") ||
                                (word.Texts[client.TextMode] == "وتالله") ||
                                (word.Texts[client.TextMode] == "لله") ||
                                (word.Texts[client.TextMode] == "فلله") ||
                                (word.Texts[client.TextMode] == "ولله")
                              )
                            {
                                count++;
                                str.AppendLine(
                                                count + "\t" +
                                                word.Address + "\t" +
                                                word.Texts[client.TextMode] + "\t" +
                                                verse.Address + "\t" +
                                                word.NumberInVerse
                                              );
                            }
                        }
                    }
                }
                catch
                {
                    // log exception
                }
            }
        }
        return str.ToString();
    }
    private static string DoNotAllahWords(Client client, List<Verse> verses)
    {
        StringBuilder str = new StringBuilder();
        if (verses != null)
        {
            if (verses.Count > 0)
            {
                try
                {
                    str.AppendLine
                        (
                            "#" + "\t" +
                            "Address" + "\t" +
                            "Text" + "\t" +
                            "Verse" + "\t" +
                            "Word"
                          );

                    int count = 0;
                    foreach (Verse verse in verses)
                    {
                        foreach (Word word in verse.Words)
                        {
                            if (
                                (word.Texts[client.TextMode] == "الضلله") ||
                                (word.Texts[client.TextMode] == "الكلله") ||
                                (word.Texts[client.TextMode] == "خلله") ||
                                (word.Texts[client.TextMode] == "خللها") ||
                                (word.Texts[client.TextMode] == "خللهما") ||
                                (word.Texts[client.TextMode] == "سلله") ||
                                (word.Texts[client.TextMode] == "ضلله") ||
                                (word.Texts[client.TextMode] == "ظلله") ||
                                (word.Texts[client.TextMode] == "ظللها") ||
                                (word.Texts[client.TextMode] == "كلله") ||
                                (word.Texts[client.TextMode] == "للهدي") ||
                                (word.Texts[client.TextMode] == "وظللهم") ||
                                (word.Texts[client.TextMode] == "يضلله") ||
                                (word.Texts[client.TextMode] == "اللهب") ||
                                (word.Texts[client.TextMode] == "اللهو")
                              )
                            {
                                count++;
                                str.AppendLine(
                                                count + "\t" +
                                                word.Address + "\t" +
                                                word.Texts[client.TextMode] + "\t" +
                                                verse.Address + "\t" +
                                                word.NumberInVerse
                                              );
                            }
                        }
                    }
                }
                catch
                {
                    // log exception
                }
            }
        }
        return str.ToString();
    }
    private static string DoWordMeaning(Client client, List<Verse> verses)
    {
        StringBuilder str = new StringBuilder();
        if (verses != null)
        {
            if (verses.Count > 0)
            {
                str.AppendLine
                    (
                        "Address" + "\t" +
                        "Chapter" + "\t" +
                        "Verse" + "\t" +
                        "Word" + "\t" +
                        "Text" + "\t" +
                        "Meaning"
                      );

                foreach (Verse verse in verses)
                {
                    foreach (Word word in verse.Words)
                    {
                        try
                        {
                            str.AppendLine(
                                word.Address + "\t" +
                                verse.Chapter.Number.ToString() + "\t" +
                                verse.NumberInChapter.ToString() + "\t" +
                                word.NumberInVerse.ToString() + "\t" +
                                word.Texts[client.TextMode] + "\t" +
                                word.Meaning
                               );
                        }
                        catch
                        {
                            // log exception
                        }
                    }
                }
            }
        }
        return str.ToString();
    }
    private static string DoWordTransliteration(Client client, List<Verse> verses)
    {
        StringBuilder str = new StringBuilder();
        if (verses != null)
        {
            if (verses.Count > 0)
            {
                str.AppendLine
                    (
                        "Address" + "\t" +
                        "Chapter" + "\t" +
                        "Verse" + "\t" +
                        "Word" + "\t" +
                        "Text" + "\t" +
                        "Transliteration"
                      );

                foreach (Verse verse in verses)
                {
                    foreach (Word word in verse.Words)
                    {
                        try
                        {
                            str.AppendLine(
                                word.Address + "\t" +
                                verse.Chapter.Number.ToString() + "\t" +
                                verse.NumberInChapter.ToString() + "\t" +
                                word.NumberInVerse.ToString() + "\t" +
                                word.Texts[client.TextMode] + "\t" +
                                word.Transliteration
                               );
                        }
                        catch
                        {
                            // log exception
                        }
                    }
                }
            }
        }
        return str.ToString();
    }
    private static string DoWordRoots(Client client, List<Verse> verses)
    {
        StringBuilder str = new StringBuilder();
        if (verses != null)
        {
            if (verses.Count > 0)
            {
                str.AppendLine
                    (
                        "Address" + "\t" +
                        "Chapter" + "\t" +
                        "Verse" + "\t" +
                        "Word" + "\t" +
                        "Text" + "\t" +
                        "Roots"
                      );

                foreach (Verse verse in verses)
                {
                    foreach (Word word in verse.Words)
                    {
                        try
                        {
                            List<string> roots = word.Roots;
                            StringBuilder roots_str = new StringBuilder();
                            foreach (string root in roots)
                            {
                                roots_str.Append(root + "|");
                            }
                            if (roots_str.Length > 0)
                            {
                                roots_str.Remove(roots_str.Length - 1, 1);
                            }

                            str.AppendLine(
                                word.Address + "\t" +
                                verse.Chapter.Number.ToString() + "\t" +
                                verse.NumberInChapter.ToString() + "\t" +
                                word.NumberInVerse.ToString() + "\t" +
                                word.Texts[client.TextMode] + "\t" +
                                roots_str.ToString()
                               );
                        }
                        catch
                        {
                            // log exception
                        }
                    }
                }
            }
        }
        return str.ToString();
    }
    private static string DoWordFrequency(Client client, List<Verse> verses)
    {
        StringBuilder str = new StringBuilder();
        if (verses != null)
        {
            if (verses.Count > 0)
            {
                Dictionary<string, WordInfo> word_infos = new Dictionary<string, WordInfo>();
                foreach (Verse verse in verses)
                {
                    foreach (Word word in verse.Words)
                    {
                        try
                        {
                            if (!word_infos.ContainsKey(word.Texts[client.TextMode]))
                            {
                                WordInfo word_info = new WordInfo();
                                word_info.Text = word.Texts[client.TextMode];
                                word_info.Frequency = 1;
                                word_info.Addresses = new List<string>();
                                word_info.Addresses.Add(word.Address);

                                word_infos.Add(word.Texts[client.TextMode], word_info);
                            }
                            else // word already exists
                            {
                                word_infos[word.Texts[client.TextMode]].Frequency++;
                                word_infos[word.Texts[client.TextMode]].Addresses.Add(word.Address);
                            }
                        }
                        catch
                        {
                            // log exception
                        }
                    }
                }

                str.AppendLine
                (
                    "Word" + "\t" +
                    "Frequency" + "\t" +
                    "Addresses"
                );
                foreach (string key in word_infos.Keys)
                {
                    StringBuilder addresses = new StringBuilder();
                    if (word_infos[key].Addresses != null)
                    {
                        foreach (string address in word_infos[key].Addresses)
                        {
                            addresses.Append(address + "|");
                        }
                        addresses.Remove(addresses.Length - 1, 1);
                    }

                    str.AppendLine
                    (
                        word_infos[key].Text + "\t" +
                        word_infos[key].Frequency + "\t" +
                        addresses
                   );
                }
            }
        }
        return str.ToString();
    }
    private static string DoWordOccurrences(Client client, List<Verse> verses)
    {
        StringBuilder str = new StringBuilder();
        if (verses != null)
        {
            if (verses.Count > 0)
            {
                str.AppendLine
                (
                    "Text" + "\t" +
                    "Occurrence" + "\t" +
                    "Frequency"
                );

                Dictionary<string, int> word_occurrences = new Dictionary<string, int>();
                Dictionary<string, int> word_frequencies = new Dictionary<string, int>();
                DoCalculateWordOccurencesAndFrequencies(client.TextMode, verses, ref word_occurrences, ref word_frequencies);

                foreach (Verse verse in verses)
                {
                    foreach (Word word in verse.Words)
                    {
                        str.AppendLine
                        (
                            word.Texts[client.TextMode] + "\t" +
                            word_occurrences[word.Address].ToString() + "\t" +
                            word_frequencies[word.Address].ToString()
                        );
                    }
                }
            }
        }
        return str.ToString();
    }
    private static void DoCalculateWordOccurencesAndFrequencies(string text_mode, List<Verse> verses, ref Dictionary<string, int> word_occurrences, ref Dictionary<string, int> word_frequencies)
    {
        Dictionary<string, int> text_frequencies = new Dictionary<string, int>();
        foreach (Verse verse in verses)
        {
            foreach (Word word in verse.Words)
            {
                if (text_frequencies.ContainsKey(word.Texts[text_mode]))
                {
                    text_frequencies[word.Texts[text_mode]]++;
                    word_occurrences[word.Address] = text_frequencies[word.Texts[text_mode]];
                }
                else
                {
                    text_frequencies.Add(word.Texts[text_mode], 1);
                    word_occurrences.Add(word.Address, 1);
                }
            }
        }

        // sum up all word_text frequencies for all word_addresses with the same word_text
        foreach (Verse verse in verses)
        {
            foreach (Word word in verse.Words)
            {
                foreach (string key in text_frequencies.Keys)
                {
                    if (key == word.Texts[text_mode])
                    {
                        if (word_frequencies.ContainsKey(word.Address))
                        {
                            word_frequencies[word.Address] += text_frequencies[word.Texts[text_mode]];
                        }
                        else
                        {
                            word_frequencies.Add(word.Address, text_frequencies[word.Texts[text_mode]]);
                        }
                    }
                }
            }
        }
    }
    private static string DoWordInformation(Client client, List<Verse> verses)
    {
        StringBuilder str = new StringBuilder();
        if (verses != null)
        {
            if (verses.Count > 0)
            {
                if (Globals.EDITION == Edition.Grammar)
                {
                    str.AppendLine
                    (
                        "Address" + "\t" +
                        "Chapter" + "\t" +
                        "Verse" + "\t" +
                        "Word" + "\t" +
                        "Text" + "\t" +
                        "Transliteration" + "\t" +
                        "Roots" + "\t" +
                        "Root" + "\t" +
                        "Lemma" + "\t" +
                        "SpecialGroup" + "\t" +
                        "Meaning" + "\t" +
                        "Occurrence" + "\t" +
                        "Frequency"
                    );
                }
                else
                {
                    str.AppendLine
                    (
                        "Address" + "\t" +
                        "Chapter" + "\t" +
                        "Verse" + "\t" +
                        "Word" + "\t" +
                        "Text" + "\t" +
                        "Transliteration" + "\t" +
                        "Roots" + "\t" +
                        "Meaning" + "\t" +
                        "Occurrence" + "\t" +
                        "Frequency"
                    );
                }

                Dictionary<string, int> word_occurrences = new Dictionary<string, int>();
                Dictionary<string, int> word_frequencies = new Dictionary<string, int>();
                DoCalculateWordOccurencesAndFrequencies(client.TextMode, verses, ref word_occurrences, ref word_frequencies);

                foreach (Verse verse in verses)
                {
                    foreach (Word word in verse.Words)
                    {
                        List<string> roots = word.Roots;
                        StringBuilder roots_str = new StringBuilder();
                        foreach (string root in roots)
                        {
                            roots_str.Append(root + "|");
                        }
                        if (roots_str.Length > 0)
                        {
                            roots_str.Remove(roots_str.Length - 1, 1);
                        }

                        if (Globals.EDITION == Edition.Grammar)
                        {
                            str.AppendLine
                            (
                                word.Address + "\t" +
                                verse.Chapter.Number.ToString() + "\t" +
                                verse.NumberInChapter.ToString() + "\t" +
                                word.NumberInVerse.ToString() + "\t" +
                                word.Texts[client.TextMode] + "\t" +
                                word.Transliteration + "\t" +
                                roots_str.ToString() + "\t" +
                                word.CorpusRoot + "\t" +
                                word.CorpusLemma + "\t" +
                                word.CorpusSpecialGroup + "\t" +
                                word.Meaning + "\t" +
                                word_occurrences[word.Address].ToString() + "\t" +
                                word_frequencies[word.Address].ToString()
                            );
                        }
                        else
                        {
                            str.AppendLine
                            (
                                word.Address + "\t" +
                                verse.Chapter.Number.ToString() + "\t" +
                                verse.NumberInChapter.ToString() + "\t" +
                                word.NumberInVerse.ToString() + "\t" +
                                word.Texts[client.TextMode] + "\t" +
                                word.Transliteration + "\t" +
                                roots_str.ToString() + "\t" +
                                word.Meaning + "\t" +
                                word_occurrences[word.Address].ToString() + "\t" +
                                word_frequencies[word.Address].ToString()
                            );
                        }
                    }
                }
            }
        }
        return str.ToString();
    }
    private static string DoWordPartInformation(Client client, List<Verse> verses)
    {
        StringBuilder str = new StringBuilder();
        if (verses != null)
        {
            if (verses.Count > 0)
            {
                str.AppendLine
                (
                    "Address" + "\t" +
                    "Chapter" + "\t" +
                    "Verse" + "\t" +
                    "Word" + "\t" +
                    "Part" + "\t" +
                    "Text" + "\t" +
                    "Buckwalter" + "\t" +
                    "Tag" + "\t" +
                    "Type" + "\t" +
                    "Position" + "\t" +
                    "Attribute" + "\t" +
                    "Qualifier" + "\t" +
                    "PersonDegree" + "\t" +
                    "PersonGender" + "\t" +
                    "PersonNumber" + "\t" +
                    "Mood" + "\t" +
                    "Lemma" + "\t" +
                    "Root" + "\t" +
                    "SpecialGroup" + "\t" +
                    "WordAddress"
                );

                foreach (Verse verse in verses)
                {
                    foreach (Word word in verse.Words)
                    {
                        foreach (WordPart part in word.Parts)
                        {
                            str.AppendLine
                            (
                                part.Address + "\t" +
                                part.Word.Verse.Chapter.Number.ToString() + "\t" +
                                part.Word.Verse.NumberInChapter.ToString() + "\t" +
                                part.Word.NumberInVerse.ToString() + "\t" +
                                part.NumberInWord.ToString() + "\t" +
                                part.Text + "\t" +
                                part.Buckwalter + "\t" +
                                part.Tag + "\t" +
                                part.Features.ToTable() + "\t" +
                                part.Word.Address
                            );
                            break;
                        }
                    }
                }
            }
        }
        return str.ToString();
    }

    public static void _______________________________________(Client client, string extra)
    {
    }
    public static void VerseStatistics(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;
        List<Verse> verses = Book.Instance.Verses;

        string result = DoVerseStatistics(client, verses);

        string filename = client.NumerologySystemName + "_" + "VerseStatistics" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        PublicStorage.SaveText(path, result);
        PublicStorage.DisplayFile(path);
    }
    public static void ChapterStatistics(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;
        List<Verse> verses = Book.Instance.Verses;

        List<Chapter> chapters = Book.GetChapters(verses);
        string result = DoChapterStatistics(client, chapters);

        string filename = client.NumerologySystemName + "_" + "ChapterStatistics" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        PublicStorage.SaveText(path, result);
        PublicStorage.DisplayFile(path);
    }
    private static void VerseValueDigitSums(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;
        List<Verse> verses = Book.Instance.Verses;

        StringBuilder str = new StringBuilder();
        foreach (Verse verse in verses)
        {
            long value = verse.Value;
            int digit_sum = Numbers.DigitSum(value);
            str.Append(verse.Address + "\t" + value.ToString(Globals.NUMBER_FORMAT) + "\t" + digit_sum.ToString() + "\r\n");
        }

        string filename = client.NumerologySystemName + "_" + "VerseValueDigitSums" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        PublicStorage.SaveText(path, str.ToString());
        PublicStorage.DisplayFile(path);
    }
    private static void ChapterValueDigitSums(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;
        List<Verse> verses = Book.Instance.Verses;

        StringBuilder str = new StringBuilder();
        List<Chapter> chapters = Book.GetChapters(verses);
        foreach (Chapter chapter in chapters)
        {
            long value = chapter.Value;
            int digit_sum = Numbers.DigitSum(value);
            str.Append(chapter.Number + "\t" + value.ToString(Globals.NUMBER_FORMAT) + "\t" + digit_sum.ToString() + "\r\n");
        }

        string filename = client.NumerologySystemName + "_" + "ChapterValueDigitSums" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        PublicStorage.SaveText(path, str.ToString());
        PublicStorage.DisplayFile(path);
    }
    private static void VerseValueWithXDigitSum(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;
        List<Verse> verses = Book.Instance.Verses;

        try
        {
            StringBuilder str = new StringBuilder();
            str.Append("#" + "\t" + "Verse" + "\t" + "Address" + "\t" + "Words" + "\t" + "Letters" + "\t" + "Unique" + "\t" + "Value" + "\t" + "DigitSum" + "\t" + "Text");
            str.Append("\r\n");

            int count = 0;
            foreach (Verse verse in verses)
            {
                long value = verse.Value;

                bool extra_condition = false;
                if (extra == "") // target == any digit sum
                {
                    extra_condition = true;
                }
                else if (extra.ToUpper() == "P") // target == prime digit sum
                {
                    extra_condition = Numbers.IsPrime(Numbers.DigitSum(value));
                }
                else if (extra.ToUpper() == "AP") // target == additive prime digit sum
                {
                    extra_condition = Numbers.IsAdditivePrime(Numbers.DigitSum(value));
                }
                else if (extra.ToUpper() == "PP") // target == pure prime digit sum
                {
                    extra_condition = Numbers.IsPurePrime(Numbers.DigitSum(value));
                }
                else if (extra.ToUpper() == "C") // target == composite digit sum
                {
                    extra_condition = Numbers.IsComposite(Numbers.DigitSum(value));
                }
                else if (extra.ToUpper() == "AC") // target == additive composite digit sum
                {
                    extra_condition = Numbers.IsAdditiveComposite(Numbers.DigitSum(value));
                }
                else if (extra.ToUpper() == "PC") // target == pure composite digit sum
                {
                    extra_condition = Numbers.IsPureComposite(Numbers.DigitSum(value));
                }
                else
                {
                    int target;
                    if (int.TryParse(extra, out target))
                    {
                        if (target == 0) // target == any digit sum
                        {
                            extra_condition = true;
                        }
                        else
                        {
                            extra_condition = (Numbers.DigitSum(value) == target);
                        }
                    }
                    else
                    {
                        return;  // invalid extra data
                    }
                }

                if (extra_condition)
                {
                    count++;
                    str.Append(count.ToString() + "\t");
                    str.Append(verse.Number.ToString() + "\t");
                    str.Append(verse.Address.ToString() + "\t");
                    str.Append(verse.Words.Count.ToString() + "\t");
                    str.Append(verse.LetterCount.ToString() + "\t");
                    str.Append(verse.UniqueLetters.Count.ToString() + "\t");
                    str.Append(value.ToString() + "\t");
                    str.Append(Numbers.DigitSum(value).ToString() + "\t");
                    str.Append(verse.GetText(client.TextMode) + "\t");
                    str.Append("\r\n");
                }
            }

            string filename = client.NumerologySystemName + "_" + "VerseValueWithXDigitSum" + "_" + extra + "_" + DateTime.Now.ToString("yyyyMMdd_hhmmss") + ".csv";
            string path = s_research_directory + "/" + filename;
            PublicStorage.SaveText(path, str.ToString());
            PublicStorage.DisplayFile(path);
        }
        catch
        {
            // log exception
        }
    }
    private static void PrimeVerseValueWithXDigitSum(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;
        List<Verse> verses = Book.Instance.Verses;

        try
        {
            StringBuilder str = new StringBuilder();
            str.Append("#" + "\t" + "Verse" + "\t" + "Address" + "\t" + "Words" + "\t" + "Letters" + "\t" + "Unique" + "\t" + "Value" + "\t" + "DigitSum" + "\t" + "Text");
            str.Append("\r\n");

            int count = 0;
            foreach (Verse verse in verses)
            {
                long value = verse.Value;

                bool extra_condition = false;
                if (extra == "") // target == any digit sum
                {
                    extra_condition = true;
                }
                else if (extra.ToUpper() == "P") // target == prime digit sum
                {
                    extra_condition = Numbers.IsPrime(Numbers.DigitSum(value));
                }
                else if (extra.ToUpper() == "AP") // target == additive prime digit sum
                {
                    extra_condition = Numbers.IsAdditivePrime(Numbers.DigitSum(value));
                }
                else if (extra.ToUpper() == "PP") // target == pure prime digit sum
                {
                    extra_condition = Numbers.IsPurePrime(Numbers.DigitSum(value));
                }
                else if (extra.ToUpper() == "C") // target == composite digit sum
                {
                    extra_condition = Numbers.IsComposite(Numbers.DigitSum(value));
                }
                else if (extra.ToUpper() == "AC") // target == additive composite digit sum
                {
                    extra_condition = Numbers.IsAdditiveComposite(Numbers.DigitSum(value));
                }
                else if (extra.ToUpper() == "PC") // target == pure composite digit sum
                {
                    extra_condition = Numbers.IsPureComposite(Numbers.DigitSum(value));
                }
                else
                {
                    int target;
                    if (int.TryParse(extra, out target))
                    {
                        if (target == 0) // target == any digit sum
                        {
                            extra_condition = true;
                        }
                        else
                        {
                            extra_condition = (Numbers.DigitSum(value) == target);
                        }
                    }
                    else
                    {
                        return;  // invalid extra data
                    }
                }

                if ((Numbers.IsPrime(value) && extra_condition))
                {
                    count++;
                    str.Append(count.ToString() + "\t");
                    str.Append(verse.Number.ToString() + "\t");
                    str.Append(verse.Address.ToString() + "\t");
                    str.Append(verse.Words.Count.ToString() + "\t");
                    str.Append(verse.LetterCount.ToString() + "\t");
                    str.Append(verse.UniqueLetters.Count.ToString() + "\t");
                    str.Append(value.ToString() + "\t");
                    str.Append(Numbers.DigitSum(value).ToString() + "\t");
                    str.Append(verse.GetText(client.TextMode) + "\t");
                    str.Append("\r\n");
                }
            }

            string filename = client.NumerologySystemName + "_" + "PrimeVerseValueWithXDigitSum" + "_" + extra + "_" + DateTime.Now.ToString("yyyyMMdd_hhmmss") + ".csv";
            string path = s_research_directory + "/" + filename;
            PublicStorage.SaveText(path, str.ToString());
            PublicStorage.DisplayFile(path);
        }
        catch
        {
            // log exception
        }
    }
    private static void ConcatenateChapterVerses(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;
        List<Verse> verses = Book.Instance.Verses;

        List<Chapter> chapters = Book.GetChapters(verses);

        StringBuilder str = new StringBuilder();
        foreach (Chapter chapter in chapters)
        {
            str.Append(chapter.Number.ToString());
            foreach (Verse verse in chapter.Verses)
            {
                str.Append(verse.NumberInChapter.ToString());
            }
        }
        str.Append("\r\n" + str.Length); // 12705 digits != prime 12791 of the Amazing Quran Statistics on heliwave.com

        string filename = "ConcatenateChapterVerses" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        PublicStorage.SaveText(path, str.ToString());
        PublicStorage.DisplayFile(path);
    }
    private static void ConcatenateChaptersVerses(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;
        List<Verse> verses = Book.Instance.Verses;

        List<Chapter> chapters = Book.GetChapters(verses);

        StringBuilder str = new StringBuilder();
        foreach (Chapter chapter in chapters)
        {
            foreach (Verse verse in chapter.Verses)
            {
                str.Append(chapter.Number.ToString());
                str.Append(verse.NumberInChapter.ToString());
            }
        }
        str.Append("\r\n" + str.Length); // 12705 digits != prime 12791 of the Amazing Quran Statistics on heliwave.com

        string filename = "ConcatenateChaptersVerses" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        PublicStorage.SaveText(path, str.ToString());
        PublicStorage.DisplayFile(path);
    }
    private static void WordLetterSums(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;
        List<Verse> verses = Book.Instance.Verses;

        string result = DoWordLetterSums(client, verses);

        string filename = client.NumerologySystemName + "_" + "WordLetterSums" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        PublicStorage.SaveText(path, result);
        PublicStorage.DisplayFile(path);
    }
    private static string DoVerseStatistics(Client client, List<Verse> verses)
    {
        StringBuilder str = new StringBuilder();

        str.Append("#" + "\t" + "Page" + "\t" + "Chapter" + "\t" + "Verse" + "\t" + "Words" + "\t" + "Letters" + "\t" + "Unique" + "\t" + "Value" + "\t" + "P" + "\t" + "AP" + "\t" + "PP" + "\t" + "C" + "\t" + "AC" + "\t" + "PC" + "\t");

        Dictionary<char, long> numerology_system = Client.NumerologySystems[client.NumerologySystemName];
        foreach (char key in numerology_system.Keys)
        {
            str.Append(key.ToString() + "\t");
        }
        str.Append("PrimeFactors" + "\t" + "Text" + "\t" + "Translation");
        str.Append("\r\n");

        int count = 0;
        foreach (Verse verse in verses)
        {
            count++;
            str.Append(verse.Number.ToString() + "\t");
            str.Append(verse.Page.Number.ToString() + "\t");
            str.Append(verse.Chapter.Number.ToString() + "\t");
            str.Append(verse.NumberInChapter.ToString() + "\t");
            str.Append(verse.Words.Count.ToString() + "\t");
            str.Append(verse.LetterCount.ToString() + "\t");
            str.Append(verse.UniqueLetters.Count.ToString() + "\t");

            long value = verse.Value;
            str.Append(value.ToString() + "\t");

            long p = Numbers.IndexOfPrime(value);
            long ap = Numbers.IndexOfAdditivePrime(value);
            long pp = Numbers.IndexOfPurePrime(value);
            long c = Numbers.IndexOfComposite(value);
            long ac = Numbers.IndexOfAdditiveComposite(value);
            long pc = Numbers.IndexOfPureComposite(value);
            str.Append((p == -1 ? "-" : p.ToString()) + "\t"
                           + (ap == -1 ? "-" : ap.ToString()) + "\t"
                           + (pp == -1 ? "-" : pp.ToString()) + "\t"
                           + (c == -1 ? "-" : c.ToString()) + "\t"
                           + (ac == -1 ? "-" : ac.ToString()) + "\t"
                           + (pc == -1 ? "-" : pc.ToString())
                         );
            str.Append("\t");

            foreach (char key in numerology_system.Keys)
            {
                str.Append(verse.GetLetterFrequency(key).ToString() + "\t");
            }
            str.Append(Numbers.FactorizeToString(value) + "\t");
            str.Append(verse.GetText(client.TextMode));
            str.Append("\r\n");
        }

        return str.ToString();
    }
    private static string DoChapterStatistics(Client client, List<Chapter> chapters)
    {
        StringBuilder str = new StringBuilder();
        str.Append("#" + "\t" + "Name" + "\t" + "Page" + "\t" + "Chapter" + "\t" + "Verses" + "\t" + "Words" + "\t" + "Letters" + "\t" + "Unique" + "\t" + "Value" + "\t" + "P" + "\t" + "AP" + "\t" + "PP" + "\t" + "C" + "\t" + "AC" + "\t" + "PC" + "\t");

        Dictionary<char, long> numerology_system = Client.NumerologySystems[client.NumerologySystemName];
        foreach (char key in numerology_system.Keys)
        {
            str.Append(key.ToString() + "\t");
        }

        str.Append("PrimeFactors");
        str.Append("\r\n");
        foreach (Chapter chapter in chapters)
        {
            str.Append(chapter.Number.ToString() + "\t");
            str.Append(chapter.Name + "\t");
            str.Append(chapter.Verses[0].Page.Number.ToString() + "\t");
            str.Append(chapter.Number.ToString() + "\t");
            str.Append(chapter.Verses.Count.ToString() + "\t");

            long chapter_value = chapter.Value;
            str.Append(chapter.WordCount.ToString() + "\t");
            str.Append(chapter.LetterCount.ToString() + "\t");
            str.Append(chapter.UniqueLetters.Count.ToString() + "\t");
            str.Append(chapter_value.ToString() + "\t");

            long p = Numbers.IndexOfPrime(chapter_value);
            long ap = Numbers.IndexOfAdditivePrime(chapter_value);
            long pp = Numbers.IndexOfPurePrime(chapter_value);
            long c = Numbers.IndexOfComposite(chapter_value);
            long ac = Numbers.IndexOfAdditiveComposite(chapter_value);
            long pc = Numbers.IndexOfPureComposite(chapter_value);
            str.Append((p == -1 ? "-" : p.ToString()) + "\t"
                           + (ap == -1 ? "-" : ap.ToString()) + "\t"
                           + (pp == -1 ? "-" : pp.ToString()) + "\t"
                           + (c == -1 ? "-" : c.ToString()) + "\t"
                           + (ac == -1 ? "-" : ac.ToString()) + "\t"
                           + (pc == -1 ? "-" : pc.ToString())
                         );
            str.Append("\t");

            foreach (char key in numerology_system.Keys)
            {
                str.Append(chapter.GetLetterFrequency(key) + "\t");
            }
            str.Append(Numbers.FactorizeToString(chapter_value));
            str.Append("\r\n");
        }

        return str.ToString();
    }
    private static string DoWordLetterSums(Client client, List<Verse> verses)
    {
        StringBuilder str = new StringBuilder();

        str.Append("#" + "\t" + "Chapter" + "\t" + "Verse" + "\t" + "Words" + "\t" + "DivBy19" + "\t" + "Letters" + "\t" + "DivBy19" + "\t" + "Unique" + "\t" + "DivBy19" + "\t" + "Sum" + "\t" + "DivBy19" + "\t" + "DivBy19Count" + "\t" + "PrimeFactors" + "\t" + "Text");
        str.Append("\r\n");

        int count = 0;
        foreach (Verse verse in verses)
        {
            count++;

            str.Append(verse.Number.ToString() + "\t");

            str.Append(verse.Chapter.Number.ToString() + "\t");

            str.Append(verse.NumberInChapter.ToString() + "\t");

            int words = verse.Words.Count;
            str.Append(words.ToString() + "\t");
            List<long> words_factors = Numbers.Factorize(words);
            str.Append(words_factors.Contains(19L).ToString() + "\t");

            int letters = verse.LetterCount;
            str.Append(letters.ToString() + "\t");
            List<long> letters_factors = Numbers.Factorize(letters);
            str.Append(letters_factors.Contains(19L).ToString() + "\t");

            int unique_letters = verse.UniqueLetters.Count;
            str.Append(unique_letters.ToString() + "\t");
            List<long> unique_letters_factors = Numbers.Factorize(unique_letters);
            str.Append(unique_letters_factors.Contains(19L).ToString() + "\t");

            long sum = words + letters + unique_letters;
            str.Append(sum.ToString() + "\t");
            List<long> sum_factors = Numbers.Factorize(sum);
            str.Append(sum_factors.Contains(19L).ToString() + "\t");

            int divisble_by_19_count = 0;
            if (words_factors.Contains(19L)) divisble_by_19_count++;
            if (letters_factors.Contains(19L)) divisble_by_19_count++;
            if (unique_letters_factors.Contains(19L)) divisble_by_19_count++;
            if (sum_factors.Contains(19L)) divisble_by_19_count++;
            str.Append(divisble_by_19_count.ToString() + "\t");

            string sum_factors_string = Numbers.FactorizeToString(sum);
            str.Append(sum_factors_string + "\t");

            str.Append(verse.GetText(client.TextMode));
            str.Append("\r\n");
        }

        return str.ToString();
    }

    private static void ________________________________________(Client client, string extra)
    {
    }
    private static void PrimeVerseNumbers(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;
        List<Verse> verses = Book.Instance.Verses;

        StringBuilder str = new StringBuilder();

        List<long> values = new List<long>();
        foreach (Verse verse in verses)
        {
            if (Numbers.IsPrime(verse.NumberInChapter))
            {
                values.Add(verse.NumberInChapter);
            }
            else
            {
                values.Add(0);
            }
        }

        foreach (int value in values)
        {
            str.Append(value.ToString(Globals.NUMBER_FORMAT) + "\r\n");
        }

        string filename = client.NumerologySystemName + "_" + "PrimeVerseNumbers" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        DoSaveAndPlayWAVFile(path, values, extra);
    }
    private static void CompositeVerseNumbers(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;
        List<Verse> verses = Book.Instance.Verses;

        StringBuilder str = new StringBuilder();

        List<long> values = new List<long>();
        foreach (Verse verse in verses)
        {
            if (Numbers.IsComposite(verse.NumberInChapter))
            {
                values.Add(verse.NumberInChapter);
            }
            else
            {
                values.Add(0);
            }
        }

        foreach (int value in values)
        {
            str.Append(value.ToString(Globals.NUMBER_FORMAT) + "\r\n");
        }

        string filename = client.NumerologySystemName + "_" + "CompositeVerseNumbers" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        DoSaveAndPlayWAVFile(path, values, extra);
    }
    private static void PrimeChapterPlusVerses(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;
        List<Verse> verses = Book.Instance.Verses;

        StringBuilder str = new StringBuilder();
        List<Chapter> chapters = Book.GetChapters(verses);

        List<long> values = new List<long>();
        foreach (Chapter chapter in chapters)
        {
            int sum = chapter.Number + chapter.Verses.Count;
            if (Numbers.IsPrime(sum))
            {
                values.Add(sum);
            }
            else
            {
                values.Add(0);
            }
        }

        foreach (int value in values)
        {
            str.Append(value.ToString(Globals.NUMBER_FORMAT) + "\r\n");
        }

        string filename = client.NumerologySystemName + "_" + "PrimeChapterPlusVerses" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        DoSaveAndPlayWAVFile(path, values, extra);
    }
    private static void CompositeChapterPlusVerses(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;
        List<Verse> verses = Book.Instance.Verses;

        StringBuilder str = new StringBuilder();
        List<Chapter> chapters = Book.GetChapters(verses);

        List<long> values = new List<long>();
        foreach (Chapter chapter in chapters)
        {
            int sum = chapter.Number + chapter.Verses.Count;
            if (Numbers.IsComposite(sum))
            {
                values.Add(sum);
            }
            else
            {
                values.Add(0);
            }
        }

        foreach (int value in values)
        {
            str.Append(value.ToString(Globals.NUMBER_FORMAT) + "\r\n");
        }

        string filename = client.NumerologySystemName + "_" + "CompositeChapterPlusVerses" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        DoSaveAndPlayWAVFile(path, values, extra);
    }
    private static void DigitsInChapterNumbers(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;
        List<Verse> verses = Book.Instance.Verses;

        StringBuilder str = new StringBuilder();
        List<Chapter> chapters = Book.GetChapters(verses);

        List<long> values = new List<long>();
        foreach (Chapter chapter in chapters)
        {
            int digits = Numbers.DigitCount(chapter.Number);
            values.Add(digits);
        }

        foreach (int value in values)
        {
            str.Append(value.ToString(Globals.NUMBER_FORMAT) + "\r\n");
        }

        string filename = client.NumerologySystemName + "_" + "DigitsInChapterNumbers" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        DoSaveAndPlayWAVFile(path, values, extra);
    }
    private static void DigitsInVerseNumbers(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;
        List<Verse> verses = Book.Instance.Verses;

        StringBuilder str = new StringBuilder();

        List<long> values = new List<long>();
        foreach (Verse verse in verses)
        {
            int digits = Numbers.DigitCount(verse.NumberInChapter);
            values.Add(digits);
        }

        foreach (int value in values)
        {
            str.Append(value.ToString(Globals.NUMBER_FORMAT) + "\r\n");
        }

        string filename = client.NumerologySystemName + "_" + "DigitsInVerseNumbers" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        DoSaveAndPlayWAVFile(path, values, extra);
    }
    private static void DigitsInPrimeChapterPlusVerses(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;
        List<Verse> verses = Book.Instance.Verses;

        StringBuilder str = new StringBuilder();
        List<Chapter> chapters = Book.Instance.Chapters;

        List<long> values = new List<long>();
        foreach (Chapter chapter in chapters)
        {
            int digits = Numbers.DigitCount(chapter.Number);
            foreach (Verse verse in chapter.Verses)
            {
                digits += Numbers.DigitCount(verse.NumberInChapter);
            }
            if (Numbers.IsPrime(digits))
            {
                values.Add(digits);
            }
            else
            {
                values.Add(0);
            }
        }

        foreach (int value in values)
        {
            str.Append(value.ToString(Globals.NUMBER_FORMAT) + "\r\n");
        }

        string filename = client.NumerologySystemName + "_" + "DigitsInPrimeChapterPlusVerses" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        DoSaveAndPlayWAVFile(path, values, extra);
    }
    private static void DigitsInCompositeChapterPlusVerses(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;
        List<Verse> verses = Book.Instance.Verses;

        StringBuilder str = new StringBuilder();
        List<Chapter> chapters = Book.Instance.Chapters;

        List<long> values = new List<long>();
        foreach (Chapter chapter in chapters)
        {
            int digits = Numbers.DigitCount(chapter.Number);
            foreach (Verse verse in chapter.Verses)
            {
                digits += Numbers.DigitCount(verse.NumberInChapter);
            }
            if (Numbers.IsComposite(digits))
            {
                values.Add(digits);
            }
            else
            {
                values.Add(0);
            }
        }

        foreach (int value in values)
        {
            str.Append(value.ToString(Globals.NUMBER_FORMAT) + "\r\n");
        }

        string filename = client.NumerologySystemName + "_" + "DigitsInCompositeChapterPlusVerses" + Globals.OUTPUT_FILE_EXT;
        string path = s_research_directory + "/" + filename;
        DoSaveAndPlayWAVFile(path, values, extra);
    }

    // Research Edition only because it uses class Letter
    private static void _________________________________________________(Client client, string extra)
    {
    }
    private class NumerologySystem
    {
        // input
        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                if (name != value)
                {
                    name = value;
                    string[] parts = name.Split('_');
                    if (parts.Length == 3)
                    {
                        TextMode = parts[0];
                        LetterOrder = parts[1];
                        LetterValues = parts[2];
                    }
                    else
                    {
                        throw new Exception("Name doesn't have exactly 2 underscores !!!");
                    }
                }
            }
        }
        public string TextMode = "";
        public string LetterOrder = "";
        public string LetterValues = "";
        public TextScope TextScope = TextScope.Book;
        public bool AddToLetterLNumber = false;
        public bool AddToLetterWNumber = false;
        public bool AddToLetterVNumber = false;
        public bool AddToLetterCNumber = false;
        public bool AddToLetterLDistance = false;
        public bool AddToLetterWDistance = false;
        public bool AddToLetterVDistance = false;
        public bool AddToLetterCDistance = false;
        public bool AddToWordWNumber = false;
        public bool AddToWordVNumber = false;
        public bool AddToWordCNumber = false;
        public bool AddToWordWDistance = false;
        public bool AddToWordVDistance = false;
        public bool AddToWordCDistance = false;
        public bool AddToVerseVNumber = false;
        public bool AddToVerseCNumber = false;
        public bool AddToVerseVDistance = false;
        public bool AddToVerseCDistance = false;
        public bool AddToChapterCNumber = false;

        // output
        public Dictionary<char, long> Values = new Dictionary<char, long>();
        public long BismAllah = 0L;
        public long AlFatihaIndex = 0L;
        public NumberType AlFatihaIndexType = NumberType.Prime;
        public long Difference = long.MaxValue;

        public string ToSummary()
        {
            return (TextMode +
                    "\t" + LetterOrder +
                    "\t" + LetterValues +
                    "\t" + TextScope.ToString() +
                    "\t" + AddToLetterLNumber.ToString() +
                    "\t" + AddToLetterWNumber.ToString() +
                    "\t" + AddToLetterVNumber.ToString() +
                    "\t" + AddToLetterCNumber.ToString() +
                    "\t" + AddToLetterLDistance.ToString() +
                    "\t" + AddToLetterWDistance.ToString() +
                    "\t" + AddToLetterVDistance.ToString() +
                    "\t" + AddToLetterCDistance.ToString() +
                    "\t" + AddToWordWNumber.ToString() +
                    "\t" + AddToWordVNumber.ToString() +
                    "\t" + AddToWordCNumber.ToString() +
                    "\t" + AddToWordWDistance.ToString() +
                    "\t" + AddToWordVDistance.ToString() +
                    "\t" + AddToWordCDistance.ToString() +
                    "\t" + AddToVerseVNumber.ToString() +
                    "\t" + AddToVerseCNumber.ToString() +
                    "\t" + AddToVerseVDistance.ToString() +
                    "\t" + AddToVerseCDistance.ToString() +
                    "\t" + AddToChapterCNumber.ToString() +
                    "\t" + BismAllah.ToString() +
                    "\t" + AlFatihaIndex.ToString() +
                    "\t" + Difference.ToString()
                );
        }
        public override string ToString()
        {
            StringBuilder str = new StringBuilder();

            str.AppendLine(Name);
            foreach (char key in Values.Keys)
            {
                str.AppendLine(key.ToString() + "\t" + Values[key].ToString());
            }

            str.Append("\r\n" + "TextScope = " + TextScope);
            str.Append("\r\n" + "Add to letter  value:");
            if (AddToLetterLNumber) str.Append("\t" + "L");
            if (AddToLetterWNumber) str.Append("\t" + "W");
            if (AddToLetterVNumber) str.Append("\t" + "V");
            if (AddToLetterCNumber) str.Append("\t" + "C");
            if (AddToLetterLDistance) str.Append("\t" + "∆L");
            if (AddToLetterWDistance) str.Append("\t" + "∆W");
            if (AddToLetterVDistance) str.Append("\t" + "∆V");
            if (AddToLetterCDistance) str.Append("\t" + "∆C");
            str.Append("\r\n" + "Add to word    value:");
            if (AddToWordWNumber) str.Append("\t" + "W");
            if (AddToWordVNumber) str.Append("\t" + "V");
            if (AddToWordCNumber) str.Append("\t" + "C");
            if (AddToWordWDistance) str.Append("\t" + "∆W");
            if (AddToWordVDistance) str.Append("\t" + "∆V");
            if (AddToWordCDistance) str.Append("\t" + "∆C");
            str.Append("\r\n" + "Add to verse   value:");
            if (AddToVerseVNumber) str.Append("\t" + "V");
            if (AddToVerseCNumber) str.Append("\t" + "C");
            if (AddToVerseVDistance) str.Append("\t" + "∆V");
            if (AddToVerseCDistance) str.Append("\t" + "∆C");
            str.Append("\r\n" + "Add to chapter value:");
            if (AddToChapterCNumber) str.Append("\t" + "C");

            str.AppendLine();
            str.AppendLine();
            str.AppendLine(
                "Difference = BismAllah - AlFatiha" + AlFatihaIndexType.ToString() + "Index\r\n" +
                "           = " + BismAllah.ToString() + " - " + AlFatihaIndex.ToString() + "\r\n" +
                "           = " + Difference.ToString());

            return str.ToString();
        }
    }
    private static void FindBismAllahIsAlFatihaSystem(Client client, string extra)
    {
        if (client == null) return;
        if (client.Selection == null) return;

        // backup client settings
        string BackupNumerologySystemName = client.NumerologySystemName;
        string BackupTextMode = client.TextMode;
        Selection BackupSelection = client.Selection;
        TextScope BackupTextScope = client.TextScope;
        bool BackupAddToLetterLNumber = client.AddToLetterLNumber;
        bool BackupAddToLetterWNumber = client.AddToLetterWNumber;
        bool BackupAddToLetterVNumber = client.AddToLetterVNumber;
        bool BackupAddToLetterCNumber = client.AddToLetterCNumber;
        bool BackupAddToLetterLDistance = client.AddToLetterLDistance;
        bool BackupAddToLetterWDistance = client.AddToLetterWDistance;
        bool BackupAddToLetterVDistance = client.AddToLetterVDistance;
        bool BackupAddToLetterCDistance = client.AddToLetterCDistance;
        bool BackupAddToWordWNumber = client.AddToWordWNumber;
        bool BackupAddToWordVNumber = client.AddToWordVNumber;
        bool BackupAddToWordCNumber = client.AddToWordCNumber;
        bool BackupAddToWordWDistance = client.AddToWordWDistance;
        bool BackupAddToWordVDistance = client.AddToWordVDistance;
        bool BackupAddToWordCDistance = client.AddToWordCDistance;
        bool BackupAddToVerseVNumber = client.AddToVerseVNumber;
        bool BackupAddToVerseCNumber = client.AddToVerseCNumber;
        bool BackupAddToVerseVDistance = client.AddToVerseVDistance;
        bool BackupAddToVerseCDistance = client.AddToVerseCDistance;
        bool BackupAddToChapterCNumber = client.AddToChapterCNumber;

        NumberType[] number_types = (NumberType[])Enum.GetValues(typeof(NumberType));
        foreach (NumberType number_type in number_types)
        {
            DateTime start_time = DateTime.Now;

            if ((number_type == NumberType.Prime) || (number_type == NumberType.AdditivePrime) || (number_type == NumberType.PurePrime))
            {
                long minimum_difference = long.MaxValue;
                List<NumerologySystem> all_numerology_systems = new List<NumerologySystem>();
                List<NumerologySystem> zero_difference_numerology_systems = new List<NumerologySystem>();
                List<NumerologySystem> best_difference_numerology_systems = new List<NumerologySystem>();

                // cache keys because Client.NumerologySystems
                // will be swapped inside CalculateBismAllahAndAlFatihaDifference
                List<string> keys = new List<string>(Client.NumerologySystems.Keys);
                //foreach (string key in keys)
                {
                    ////??? using all keys is too slow so use DEFAULT_NUMEROLOGY_SYSTEM for now
                    string key = Client.DEFAULT_NUMEROLOGY_SYSTEM;
                    //// the following combination finds zero-difference system
                    ////Simplified29_Alphabet_Primes which is DEFAULT_NUMEROLOGY_SYSTEM
                    ////AddToLetterLNumber   = True
                    ////AddToLetterWNumber     = True
                    ////AddToWordVDistance    = True
                    ////AddToVerseVNumber    = True
                    ////BismAllah - AlFatihaPrimeIndex  = 1146 - 1146 = 0

                    //if (key.Contains("Images")) continue;
                    //if (key.Contains("Original")) continue;
                    //if (key.Contains("Simplified29Shadda")) continue;
                    //if (key.Contains("Simplified29Waw")) continue;
                    //if (key.Contains("English")) continue;

                    int PERMUTATIONS = 524288;
                    for (int i = 0; i < PERMUTATIONS; i++)
                    {
                        NumerologySystem numerology_system = new NumerologySystem();
                        numerology_system.Name = key;
                        numerology_system.AddToLetterLNumber = ((i & 262144) != 0);
                        numerology_system.AddToLetterWNumber = ((i & 131072) != 0);
                        numerology_system.AddToLetterVNumber = ((i & 65536) != 0);
                        numerology_system.AddToLetterCNumber = ((i & 32768) != 0);
                        numerology_system.AddToLetterLDistance = ((i & 16384) != 0);
                        numerology_system.AddToLetterWDistance = ((i & 8192) != 0);
                        numerology_system.AddToLetterVDistance = ((i & 4096) != 0);
                        numerology_system.AddToLetterCDistance = ((i & 2048) != 0);
                        numerology_system.AddToWordWNumber = ((i & 1024) != 0);
                        numerology_system.AddToWordVNumber = ((i & 512) != 0);
                        numerology_system.AddToWordCNumber = ((i & 256) != 0);
                        numerology_system.AddToWordWDistance = ((i & 128) != 0);
                        numerology_system.AddToWordVDistance = ((i & 64) != 0);
                        numerology_system.AddToWordCDistance = ((i & 32) != 0);
                        numerology_system.AddToVerseVNumber = ((i & 16) != 0);
                        numerology_system.AddToVerseCNumber = ((i & 8) != 0);
                        numerology_system.AddToVerseVDistance = ((i & 4) != 0);
                        numerology_system.AddToVerseCDistance = ((i & 2) != 0);
                        numerology_system.AddToChapterCNumber = ((i & 1) != 0);

                        // this will update the numerology system including its count of letters and their values (Values.Count)
                        long difference = CalculateBismAllahAndAlFatihaDifference(ref numerology_system, number_type);

                        all_numerology_systems.Add(numerology_system);

                        if (difference == 0L)
                        {
                            zero_difference_numerology_systems.Add(numerology_system);
                            Console.WriteLine(numerology_system.ToSummary());
                            // break;   // don't break in case there are more systems with zero difference
                        }
                        else
                        {
                            if (Math.Abs(difference) > minimum_difference)
                            {
                                continue;
                            }
                            else if (Math.Abs(difference) == minimum_difference)
                            {
                                best_difference_numerology_systems.Add(numerology_system);
                            }
                            else if (Math.Abs(difference) < minimum_difference)
                            {
                                minimum_difference = Math.Abs(difference);
                                best_difference_numerology_systems.Clear();

                                best_difference_numerology_systems.Add(numerology_system);
                            }
                        }
                    } // PERMUTATIONS loop
                } // keys loop

                string filename = "FindBismAllahIsAlFatiha" + number_type.ToString() + "IndexSystem" + Globals.OUTPUT_FILE_EXT;
                string path = s_research_directory + "/" + filename;

                StringBuilder str = new StringBuilder();
                str.AppendLine("TextMode" +
                                "\t" + "LetterOrder" +
                                "\t" + "LetterValues" +
                                "\t" + "TextScope" +
                                "\t" + "AddToLetterLNumber" +
                                "\t" + "AddToLetterWNumber" +
                                "\t" + "AddToLetterVNumber" +
                                "\t" + "AddToLetterLDistance" +
                                "\t" + "AddToLetterWDistance" +
                                "\t" + "AddToLetterVDistance" +
                                "\t" + "AddToLetterLWVNumber" +
                                "\t" + "AddToLetterVWLNumber" +
                                "\t" + "AddToLetterLWVDistance" +
                                "\t" + "AddToLetterVWLDistance" +
                                "\t" + "AddToWordWNumber" +
                                "\t" + "AddToWordVNumber" +
                                "\t" + "AddToWordWDistance" +
                                "\t" + "AddToWordVDistance" +
                                "\t" + "AddToWordWVNumber" +
                                "\t" + "AddToWordVWNumber" +
                                "\t" + "AddToWordWVDistance" +
                                "\t" + "AddToWordVWDistance" +
                                "\t" + "AddToVerseVNumber" +
                                "\t" + "AddToVerseVDistance" +
                                "\t" + "BismAllah" +
                                "\t" + "AlFatiha" + number_type.ToString() + "Index" +
                                "\t" + "Difference"
                            );

                if (all_numerology_systems != null)
                {
                    if (all_numerology_systems.Count > 0)
                    {
                        foreach (NumerologySystem numerology_system in all_numerology_systems)
                        {
                            str.AppendLine(numerology_system.ToSummary());
                        }
                    }
                }

                str.AppendLine();
                str.AppendLine(all_numerology_systems.Count.ToString() + " systems were tested.");
                str.AppendLine(best_difference_numerology_systems.Count.ToString() + " best-difference systems were found.");
                str.AppendLine(zero_difference_numerology_systems.Count.ToString() + " zero-difference systems were found.");
                str.AppendLine();
                if (best_difference_numerology_systems.Count > 0)
                {
                    str.AppendLine("------------- Best-Difference Systems -------------");
                    foreach (NumerologySystem numerology_system in best_difference_numerology_systems)
                    {
                        str.AppendLine(numerology_system.ToSummary());
                        str.AppendLine();
                    }
                    str.AppendLine("-----------------------------------------------");
                    str.AppendLine();
                }
                if (zero_difference_numerology_systems.Count > 0)
                {
                    str.AppendLine();
                    str.AppendLine("------------- Zero-Difference Systems -------------");
                    foreach (NumerologySystem numerology_system in zero_difference_numerology_systems)
                    {
                        str.AppendLine(numerology_system.ToString());
                        str.AppendLine();
                    }
                    str.AppendLine("---------------------------------------------------");
                    str.AppendLine();
                }

                TimeSpan elapsed_time = DateTime.Now - start_time;
                str.AppendLine("Elapsed Time: " + elapsed_time.ToString());

                PublicStorage.SaveText(path, str.ToString());
                PublicStorage.DisplayFile(path);

                // free memeory
                for (int i = 0; i < all_numerology_systems.Count; i++)
                {
                    all_numerology_systems[i] = null;
                }
                for (int i = 0; i < zero_difference_numerology_systems.Count; i++)
                {
                    zero_difference_numerology_systems[i] = null;
                }
                for (int i = 0; i < best_difference_numerology_systems.Count; i++)
                {
                    best_difference_numerology_systems[i] = null;
                }
                all_numerology_systems = null;
                zero_difference_numerology_systems = null;
                best_difference_numerology_systems = null;
                GC.Collect();
            }
        }

        // restore client settings
        client.TextMode = BackupTextMode;
        client.Selection = BackupSelection;
        client.NumerologySystemName = BackupNumerologySystemName;
        string text = "";
        switch (BackupTextScope)
        {
            case TextScope.Book:
                {
                    text = Book.Instance.GetText(client.TextMode);
                    Client.UpdateNumerologySystems(client.TextMode, text, true);
                }
                break;
            case TextScope.Selection:
                {
                    text = client.Selection.GetText(client.TextMode);
                    Client.UpdateNumerologySystems(client.TextMode, text, false);
                }
                break;
            case TextScope.HighlightedText:
                {
                    text = client.Selection.GetText(client.TextMode);
                    Client.UpdateNumerologySystems(client.TextMode, text, false);
                }
                break;
            default:
                {
                }
                break;
        }

        client.AddToLetterLNumber = BackupAddToLetterLNumber;
        client.AddToLetterWNumber = BackupAddToLetterWNumber;
        client.AddToLetterVNumber = BackupAddToLetterVNumber;
        client.AddToLetterCNumber = BackupAddToLetterCNumber;
        client.AddToLetterLDistance = BackupAddToLetterLDistance;
        client.AddToLetterWDistance = BackupAddToLetterWDistance;
        client.AddToLetterVDistance = BackupAddToLetterVDistance;
        client.AddToLetterCDistance = BackupAddToLetterCDistance;
        client.AddToWordWNumber = BackupAddToWordWNumber;
        client.AddToWordVNumber = BackupAddToWordVNumber;
        client.AddToWordCNumber = BackupAddToWordCNumber;
        client.AddToWordWDistance = BackupAddToWordWDistance;
        client.AddToWordVDistance = BackupAddToWordVDistance;
        client.AddToWordCDistance = BackupAddToWordCDistance;
        client.AddToVerseVNumber = BackupAddToVerseVNumber;
        client.AddToVerseCNumber = BackupAddToVerseCNumber;
        client.AddToVerseVDistance = BackupAddToVerseVDistance;
        client.AddToVerseCDistance = BackupAddToVerseCDistance;
        client.AddToChapterCNumber = BackupAddToChapterCNumber;
    }
    private static string s_previous_numerology_system_name;
    private static TextScope s_previous_numerology_system_text_scope;
    private static long CalculateBismAllahAndAlFatihaDifference(ref NumerologySystem numerology_system, NumberType index_type)
    {
        long value_difference = long.MaxValue;

        if (numerology_system != null)
        {
            Book book = Book.Instance;
            if (book != null)
            {
                // only update if numerology system name OR text_scope has changed
                if (
                    (numerology_system.Name == Client.DEFAULT_NUMEROLOGY_SYSTEM)
                    ||
                    (numerology_system.Name != s_previous_numerology_system_name)
                    ||
                    (numerology_system.TextScope != s_previous_numerology_system_text_scope)
                   )
                {
                    s_previous_numerology_system_name = numerology_system.Name;
                    s_previous_numerology_system_text_scope = numerology_system.TextScope;

                    switch (numerology_system.TextScope)
                    {
                        case TextScope.Book:
                            {
                                Client.UpdateNumerologySystems(numerology_system.TextMode, book.GetText(numerology_system.TextMode), true);
                            }
                            break;
                        case TextScope.Selection:
                            {
                                Client.UpdateNumerologySystems(numerology_system.TextMode, book.Chapters[0].GetText(numerology_system.TextMode), false);
                            }
                            break;
                        case TextScope.HighlightedText:
                            {
                                Client.UpdateNumerologySystems(numerology_system.TextMode, book.Verses[0].GetText(numerology_system.TextMode), false);
                            }
                            break;
                        default:
                            {
                            }
                            break;
                    }

                    ///////////////////////////////////////////////////////////////////
                    //// update all chapter and verse Values
                    //foreach (Chapter chapter in book.Chapters)
                    //{
                    //    Client.CalculateValue(chapter,
                    //                          numerology_system.Name,
                    //                          numerology_system.AddToLetterLNumber,
                    //                          numerology_system.AddToLetterWNumber,
                    //                          numerology_system.AddToLetterVNumber,
                    //                          numerology_system.AddToLetterCNumber,
                    //                          numerology_system.AddToLetterLDistance,
                    //                          numerology_system.AddToLetterWDistance,
                    //                          numerology_system.AddToLetterVDistance,
                    //                          numerology_system.AddToLetterCDistance,
                    //                          numerology_system.AddToWordWNumber,
                    //                          numerology_system.AddToWordVNumber,
                    //                          numerology_system.AddToWordCNumber,
                    //                          numerology_system.AddToWordWDistance,
                    //                          numerology_system.AddToWordVDistance,
                    //                          numerology_system.AddToWordCDistance,
                    //                          numerology_system.AddToVerseVNumber,
                    //                          numerology_system.AddToVerseCNumber,
                    //                          numerology_system.AddToVerseVDistance,
                    //                          numerology_system.AddToVerseCDistance,
                    //                          numerology_system.AddToChapterCNumber
                    //                         );
                    //}
                    ///////////////////////////////////////////////////////////////////
                }
                // update numerology_system with values
                numerology_system.Values = new Dictionary<char, long>(Client.NumerologySystems[numerology_system.Name]);

                long bismAllah_value = 0L;
                long alfatiha_value = 0L;
                // update Values of only chapter Al-Fatiha and its verses
                foreach (Chapter chapter in book.Chapters)
                {
                    if (chapter.Number == 1)
                    {
                        Client.CalculateValue(chapter,
                                              numerology_system.Name,
                                              numerology_system.AddToLetterLNumber,
                                              numerology_system.AddToLetterWNumber,
                                              numerology_system.AddToLetterVNumber,
                                              numerology_system.AddToLetterCNumber,
                                              numerology_system.AddToLetterLDistance,
                                              numerology_system.AddToLetterWDistance,
                                              numerology_system.AddToLetterVDistance,
                                              numerology_system.AddToLetterCDistance,
                                              numerology_system.AddToWordWNumber,
                                              numerology_system.AddToWordVNumber,
                                              numerology_system.AddToWordCNumber,
                                              numerology_system.AddToWordWDistance,
                                              numerology_system.AddToWordVDistance,
                                              numerology_system.AddToWordCDistance,
                                              numerology_system.AddToVerseVNumber,
                                              numerology_system.AddToVerseCNumber,
                                              numerology_system.AddToVerseVDistance,
                                              numerology_system.AddToVerseCDistance,
                                              numerology_system.AddToChapterCNumber
                                             );

                        alfatiha_value = chapter.Value;
                        bismAllah_value = chapter.Verses[0].Value;
                        break;
                    }
                }

                long alfatiha_value_index = -1L;
                switch (index_type)
                {
                    case NumberType.Prime:
                        {
                            alfatiha_value_index = Numbers.IsPrime(alfatiha_value) ? Numbers.IndexOfPrime(alfatiha_value) : Numbers.IndexOfComposite(alfatiha_value);
                        }
                        break;
                    case NumberType.AdditivePrime:
                        {
                            alfatiha_value_index = Numbers.IsPrime(alfatiha_value) ? Numbers.IndexOfAdditivePrime(alfatiha_value) : Numbers.IndexOfAdditiveComposite(alfatiha_value);
                        }
                        break;
                    case NumberType.PurePrime:
                        {
                            alfatiha_value_index = Numbers.IsPrime(alfatiha_value) ? Numbers.IndexOfPurePrime(alfatiha_value) : Numbers.IndexOfPureComposite(alfatiha_value);
                        }
                        break;
                }

                if (alfatiha_value_index > -1L)
                {
                    value_difference = bismAllah_value - alfatiha_value_index;
                }

                numerology_system.BismAllah = bismAllah_value;
                numerology_system.AlFatihaIndex = alfatiha_value_index;
                numerology_system.AlFatihaIndexType = index_type;
                numerology_system.Difference = value_difference;
            }
        }

        return value_difference;
    }
}
