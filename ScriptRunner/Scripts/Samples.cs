#region QuranCode Object Model
//----------------------------
// Client.Book
// Book.Verses
// Book.Chapters.Verses
// Book.Stations.Verses
// Book.Parts.Verses
// Book.Groups.Verses
// Book.Quarters.Verses
// Book.Bowings.Verses
// Book.Pages.Verses
// Verse.Words
// Client.Books              // readonly, all Books[key] where key = Original, Simplified## where ## = 28, 29, 31, 37
// Client.TranslationBooks
// Client.Bookmarks
// Client.Selection.Verses   // readonly, current selection (chapter, station, part, ...)
// Client.LetterStatistics   // readonly, statistics for current selection or highlighted text
//----------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using System.IO;
using Model;

public class MyScript : IScriptRunner
{
    private bool OnlyKeyLetters(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;
        List<Verse> verses = client.Selection.Verses;

        StringBuilder str = new StringBuilder();
        foreach (Verse verse in verses)
        {
            string verse_text = verse.Text;
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
        string filename = client.NumerologySystemName + "_" + "OnlyKeyLetters" + Globals.OUTPUT_FILE_EXT;
        ScriptRunner.SaveText(filename, str.ToString());
        ScriptRunner.DisplayFile(filename);
        return false; // so not to close Script window
    }
    private bool OnlyInitialLetters(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;
        List<Verse> verses = client.Selection.Verses;

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
        try
        {
            StringBuilder str = new StringBuilder();
            foreach (Verse verse in verses)
            {
                str.AppendLine(verse.Text);
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

            string filename = client.NumerologySystemName + "_" + "OnlyInitialLetters" + Globals.OUTPUT_FILE_EXT;
            ScriptRunner.SaveText(filename, str.ToString());
            ScriptRunner.DisplayFile(filename);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }

    private bool JumpLettersByX(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;

        try
        {
            string verses_text = client.Selection.Text;
            char[] result = DoJumpLettersByX(client, verses_text, extra);

            string filename = client.NumerologySystemName + "_" + "JumpLettersByX" + Globals.OUTPUT_FILE_EXT;
            ScriptRunner.SaveLetters(filename, result);
            ScriptRunner.DisplayFile(filename);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    private bool JumpLettersByValues(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;

        try
        {
            string verses_text = client.Selection.Text;
            char[] result = DoJumpLettersByValues(client, verses_text, extra);

            string filename = client.NumerologySystemName + "_" + "JumpLettersByValues" + Globals.OUTPUT_FILE_EXT;
            ScriptRunner.SaveLetters(filename, result);
            ScriptRunner.DisplayFile(filename);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    private bool JumpLettersByPrimeNumbers(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;

        try
        {
            string verses_text = client.Selection.Text;
            char[] result = DoJumpLettersByPrimeNumbers(client, verses_text, extra);

            string filename = client.NumerologySystemName + "_" + "JumpLettersByPrimeNumbers" + Globals.OUTPUT_FILE_EXT;
            ScriptRunner.SaveLetters(filename, result);
            ScriptRunner.DisplayFile(filename);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    private bool JumpLettersByFibonacciNumbers(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;

        // 0, 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144, ...
        try
        {
            string verses_text = client.Selection.Text;
            char[] result = DoJumpLettersByFibonacciNumbers(client, verses_text, extra);

            string filename = client.NumerologySystemName + "_" + "JumpLettersByFibonacciNumbers" + Globals.OUTPUT_FILE_EXT;
            ScriptRunner.SaveLetters(filename, result);
            ScriptRunner.DisplayFile(filename);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    private bool JumpLettersByPiDigits(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;

        // 3.14159265358979323846264338327950288419716939937510582097494459230781640628620899862803482534211706798214808651328230664709384460955058223172535940812848111745028410270193852110555964462294895493038196442881097566593344612847564823378678316527120190914564856692346034861045432664821339360726024914127372458700660631558817488152092096282925409171536436789259036001133053054882046652138414695194151160943305727036575959195309218611738193261179310511854807446237996274956735188575272489122793818301194912983367336244065664308602139494639522473719070217986094370277053921717629317675238467481846766940513200056812714526356082778577134275778960917363717872146844090122495343014654958537105079227968925892354201995611212902196086403441815981362977477130996051870721134999999837297804995105973173281609631859502445945534690830264252230825334468503526193118817101000313783875288658753320838142061717766914730359825349042875546873115956286388235378759375195778185778053217122680661300192787661119590921642019893809525720106548586327...
        try
        {
            string verses_text = client.Selection.Text;
            char[] result = DoJumpLettersByPiDigits(client, verses_text, "");

            string filename = client.NumerologySystemName + "_" + "JumpLettersByPiDigits" + Globals.OUTPUT_FILE_EXT;
            ScriptRunner.SaveLetters(filename, result);
            ScriptRunner.DisplayFile(filename);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    private bool JumpLettersByEulerDigits(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;

        // 2.71828182845904523536028747135266249775724709369995957496696762772407663035354759457138217852516642742746639193200305992181741359662904357290033429526059563073813232862794349076323382988075319525101901157383418793070215408914993488416750924476146066808226480016847741185374234544243710753907774499206955170276183860626133138458300075204493382656029760673711320070932870912744374704723069697720931014169283681902551510865746377211125238978442505695369677078544996996794686445490598793163688923009879312773617821542499922957635148220826989519366803318252886939849646510582093923982948879332036250944311730123819706841614039701983767932068328237646480429531180232878250981945581530175671736133206981125099618188159304169035159888851934580727386673858942287922849989208680582574927961048419844436346324496848756023362482704197862320900216099023530436994184914631409343173814364054625315209618369088870701676839642437814059271456354906130310720851038375051011574770417189861068739696552126715468895703503540212340784981933432106...
        try
        {
            string verses_text = client.Selection.Text;
            char[] result = DoJumpLettersByEulerDigits(client, verses_text, "");

            string filename = client.NumerologySystemName + "_" + "JumpLettersByEulerDigits" + Globals.OUTPUT_FILE_EXT;
            ScriptRunner.SaveLetters(filename, result);
            ScriptRunner.DisplayFile(filename);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    private bool JumpLettersByGoldenRatioDigits(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;

        // 1.61803398874989484820458683436563811772030917980576286213544862270526046281890244970720720418939113748475408807538689175212663386222353693179318006076672635443338908659593958290563832266131992829026788067520876689250171169620703222104321626954862629631361443814975870122034080588795445474924618569536486444924104432077134494704956584678850987433944221254487706647809158846074998871240076521705751797883416625624940758906970400028121042762177111777805315317141011704666599146697987317613560067087480710131795236894275219484353056783002287856997829778347845878228911097625003026961561700250464338243776486102838312683303724292675263116533924731671112115881863851331620384005222165791286675294654906811317159934323597349498509040947621322298101726107059611645629909816290555208524790352406020172799747175342777592778625619432082750513121815628551222480939471234145170223735805772786160086883829523045926478780178899219902707769038953219681986151437803149974110692608867429622675756052317277752035361393621076738937645560606059...
        try
        {
            string verses_text = client.Selection.Text;
            char[] result = DoJumpLettersByGoldenRatioDigits(client, verses_text, "");

            string filename = client.NumerologySystemName + "_" + "JumpLettersByGoldenRatioDigits" + Globals.OUTPUT_FILE_EXT;
            ScriptRunner.SaveLetters(filename, result);
            ScriptRunner.DisplayFile(filename);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    private char[] DoJumpLettersByX(Client client, string verses_text, string extra)
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
    private char[] DoJumpLettersByValues(Client client, string verses_text, string extra)
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
    private char[] DoJumpLettersByPrimeNumbers(Client client, string verses_text, string extra)
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
    private char[] DoJumpLettersByFibonacciNumbers(Client client, string verses_text, string extra)
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
    private char[] DoJumpLettersByPiDigits(Client client, string verses_text, string extra)
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
    private char[] DoJumpLettersByEulerDigits(Client client, string verses_text, string extra)
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
    private char[] DoJumpLettersByGoldenRatioDigits(Client client, string verses_text, string extra)
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

    private bool PrimeLetters(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;

        try
        {
            string verses_text = client.Selection.Text;
            char[] result = DoPrimeLetters(client, verses_text, extra);

            string filename = client.NumerologySystemName + "_" + "PrimeLetters" + Globals.OUTPUT_FILE_EXT;
            ScriptRunner.SaveLetters(filename, result);
            ScriptRunner.DisplayFile(filename);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    private bool AdditivePrimeLetters(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;

        try
        {
            string verses_text = client.Selection.Text;
            char[] result = DoAdditivePrimeLetters(client, verses_text, extra);

            string filename = client.NumerologySystemName + "_" + "AdditivePrimeLetters" + Globals.OUTPUT_FILE_EXT;
            ScriptRunner.SaveLetters(filename, result);
            ScriptRunner.DisplayFile(filename);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    private bool PurePrimeLetters(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;

        try
        {
            string verses_text = client.Selection.Text;
            char[] result = DoPurePrimeLetters(client, verses_text, extra);

            string filename = client.NumerologySystemName + "_" + "PurePrimeLetters" + Globals.OUTPUT_FILE_EXT;
            ScriptRunner.SaveLetters(filename, result);
            ScriptRunner.DisplayFile(filename);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    private bool FibonacciLetters(Client client, string extra)
    {
        // 0, 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144, ...
        if (client == null) return false;
        if (client.Selection == null) return false;

        try
        {
            string verses_text = client.Selection.Text;
            char[] result = DoFibonacciLetters(client, verses_text, extra);

            string filename = client.NumerologySystemName + "_" + "FibonacciLetters" + Globals.OUTPUT_FILE_EXT;
            ScriptRunner.SaveLetters(filename, result);
            ScriptRunner.DisplayFile(filename);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    private char[] DoPrimeLetters(Client client, string verses_text, string extra)
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
    private char[] DoAdditivePrimeLetters(Client client, string verses_text, string extra)
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
    private char[] DoPurePrimeLetters(Client client, string verses_text, string extra)
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
    private char[] DoFibonacciLetters(Client client, string verses_text, string extra)
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

    public bool PrimeWords(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;

        try
        {
            List<Verse> verses = client.Selection.Verses;
            List<string> result = DoPrimeWords(client, verses, extra);

            string filename = client.NumerologySystemName + "_" + "PrimeWords" + Globals.OUTPUT_FILE_EXT;
            ScriptRunner.SaveWords(filename, result);
            ScriptRunner.DisplayFile(filename);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    public bool AdditivePrimeWords(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;

        try
        {
            List<Verse> verses = client.Selection.Verses;
            List<string> result = DoAdditivePrimeWords(client, verses, extra);

            string filename = client.NumerologySystemName + "_" + "AdditivePrimeWords" + Globals.OUTPUT_FILE_EXT;
            ScriptRunner.SaveWords(filename, result);
            ScriptRunner.DisplayFile(filename);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    public bool PurePrimeWords(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;

        try
        {
            List<Verse> verses = client.Selection.Verses;
            List<string> result = DoPurePrimeWords(client, verses, extra);

            string filename = client.NumerologySystemName + "_" + "PurePrimeWords" + Globals.OUTPUT_FILE_EXT;
            ScriptRunner.SaveWords(filename, result);
            ScriptRunner.DisplayFile(filename);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    public bool FibonacciWords(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;

        try
        {
            List<Verse> verses = client.Selection.Verses;
            List<string> result = DoFibonacciWords(client, verses, extra);

            string filename = client.NumerologySystemName + "_" + "FibonacciWords" + Globals.OUTPUT_FILE_EXT;
            ScriptRunner.SaveWords(filename, result);
            ScriptRunner.DisplayFile(filename);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    private List<string> DoPrimeWords(Client client, List<Verse> verses, string extra)
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
                result.Add(words[i].Text + " ");
            }
        }

        return result;
    }
    private List<string> DoAdditivePrimeWords(Client client, List<Verse> verses, string extra)
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
                result.Add(words[i].Text + " ");
            }
        }

        return result;
    }
    private List<string> DoPurePrimeWords(Client client, List<Verse> verses, string extra)
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
                result.Add(words[i].Text + " ");
            }
        }

        return result;
    }
    private List<string> DoFibonacciWords(Client client, List<Verse> verses, string extra)
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
                result.Add(words[i].Text + " ");
            }
        }

        return result;
    }

    private bool ChapterVersesSound(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;
        List<Verse> verses = client.Selection.Verses;

        try
        {
            List<Chapter> chapters = Book.GetChapters(verses);

            List<long> values = new List<long>();
            foreach (Chapter chapter in chapters)
            {
                values.Add(chapter.Verses.Count);
            }

            string filename = client.NumerologySystemName + "_" + "ChapterVerses" + Globals.OUTPUT_FILE_EXT;
            DoSaveAndPlayWAVFile(filename, values, extra);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    private bool ChapterWordsSound(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;
        List<Verse> verses = client.Selection.Verses;

        try
        {
            List<Chapter> chapters = Book.GetChapters(verses);

            List<long> values = new List<long>();
            foreach (Chapter chapter in chapters)
            {
                values.Add(chapter.WordCount);
            }

            string filename = client.NumerologySystemName + "_" + "ChapterWords" + Globals.OUTPUT_FILE_EXT;
            DoSaveAndPlayWAVFile(filename, values, extra);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    private bool VerseWordsSound(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;
        List<Verse> verses = client.Selection.Verses;

        try
        {
            List<long> values = new List<long>();
            foreach (Verse verse in verses)
            {
                values.Add(verse.Words.Count);
            }

            string filename = client.NumerologySystemName + "_" + "VerseWords" + Globals.OUTPUT_FILE_EXT;
            DoSaveAndPlayWAVFile(filename, values, extra);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    private bool ChapterLettersSound(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;
        List<Verse> verses = client.Selection.Verses;

        try
        {
            List<Chapter> chapters = Book.GetChapters(verses);

            List<long> values = new List<long>();
            foreach (Chapter chapter in chapters)
            {
                values.Add(chapter.LetterCount);
            }

            string filename = client.NumerologySystemName + "_" + "ChapterLetters" + Globals.OUTPUT_FILE_EXT;
            DoSaveAndPlayWAVFile(filename, values, extra);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    private bool VerseLettersSound(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;
        List<Verse> verses = client.Selection.Verses;

        try
        {
            List<long> values = new List<long>();
            foreach (Verse verse in verses)
            {
                values.Add(verse.LetterCount);
            }

            string filename = client.NumerologySystemName + "_" + "VerseLetters" + Globals.OUTPUT_FILE_EXT;
            DoSaveAndPlayWAVFile(filename, values, extra);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    private bool WordLettersSound(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;
        List<Verse> verses = client.Selection.Verses;

        try
        {
            List<long> values = new List<long>();
            foreach (Verse verse in verses)
            {
                foreach (Word word in verse.Words)
                {
                    values.Add(word.LetterCount);
                }
            }

            string filename = client.NumerologySystemName + "_" + "WordLetters" + Globals.OUTPUT_FILE_EXT;
            DoSaveAndPlayWAVFile(filename, values, extra);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    private bool ChapterValuesSound(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;
        List<Verse> verses = client.Selection.Verses;

        try
        {
            List<Chapter> chapters = Book.GetChapters(verses);

            List<long> values = new List<long>();
            foreach (Chapter chapter in chapters)
            {
                values.Add(chapter.Value);
            }

            string filename = client.NumerologySystemName + "_" + "ChapterValues" + Globals.OUTPUT_FILE_EXT;
            DoSaveAndPlayWAVFile(filename, values, extra);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    private bool VerseValuesSound(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;
        List<Verse> verses = client.Selection.Verses;

        try
        {
            List<long> values = new List<long>();
            foreach (Verse verse in verses)
            {
                values.Add(verse.Value);
            }

            string filename = client.NumerologySystemName + "_" + "VerseValues" + Globals.OUTPUT_FILE_EXT;
            DoSaveAndPlayWAVFile(filename, values, extra);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    private bool WordValuesSound(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;
        List<Verse> verses = client.Selection.Verses;

        try
        {
            List<long> values = new List<long>();
            foreach (Verse verse in verses)
            {
                foreach (Word word in verse.Words)
                {
                    values.Add(client.CalculateValue(word));
                }
            }

            string filename = client.NumerologySystemName + "_" + "WordValues" + Globals.OUTPUT_FILE_EXT;
            DoSaveAndPlayWAVFile(filename, values, extra);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    private bool LetterValuesSound(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;
        List<Verse> verses = client.Selection.Verses;

        try
        {
            List<long> values = new List<long>();
            foreach (Verse verse in verses)
            {
                for (int w = 0; w < verse.Words.Count; w++)
                {
                    for (int l = 0; l < verse.Words[w].Text.Length; l++)
                    {
                        values.Add(client.CalculateValue(verse, w, l));
                    }
                }
            }

            string filename = client.NumerologySystemName + "_" + "LetterValues" + Globals.OUTPUT_FILE_EXT;
            DoSaveAndPlayWAVFile(filename, values, extra);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    private bool ChapterValueDiffsSound(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;
        List<Verse> verses = client.Selection.Verses;

        try
        {
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
            DoSaveAndPlayWAVFile(filename, values, extra);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    private bool VerseValueDiffsSound(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;
        List<Verse> verses = client.Selection.Verses;

        try
        {
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
            DoSaveAndPlayWAVFile(filename, values, extra);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    private bool WordValueDiffsSound(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;
        List<Verse> verses = client.Selection.Verses;

        try
        {
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
            DoSaveAndPlayWAVFile(filename, values, extra);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    private bool LetterValueDiffsSound(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;
        List<Verse> verses = client.Selection.Verses;

        try
        {
            List<long> values = new List<long>();
            foreach (Verse verse in verses)
            {
                for (int w = 0; w < verse.Words.Count; w++)
                {
                    for (int l = 0; l < verse.Words[w].Text.Length; l++)
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
            DoSaveAndPlayWAVFile(filename, values, extra);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    private bool SameLetterGapsSound(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;
        List<Verse> verses = client.Selection.Verses;

        try
        {
            StringBuilder str = new StringBuilder();
            foreach (Verse verse in verses)
            {
                str.Append(verse.Text);
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
            DoSaveAndPlayWAVFile(filename, values, extra);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    private bool SameLetterBackwardGapsSound(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;
        List<Verse> verses = client.Selection.Verses;

        try
        {
            StringBuilder str = new StringBuilder();
            foreach (Verse verse in verses)
            {
                str.Append(verse.Text);
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
            DoSaveAndPlayWAVFile(filename, values, extra);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    private void DoSaveAndPlayWAVFile(string filename, List<long> values, string extra)
    {
        ScriptRunner.SaveValues(filename, values);
        ScriptRunner.DisplayFile(filename); // filename.csv
        int frequency = 0;
        if (extra.Length == 0)
        {
            // update ref filename.csv to filename.wav
            ScriptRunner.GenerateWAVFile(ref filename, values, Globals.DEFAULT_FREQUENCY);
        }
        else if (int.TryParse(extra, out frequency))
        {
            // update ref filename.csv to filename.wav
            ScriptRunner.GenerateWAVFile(ref filename, values, frequency);
        }
        else
        {
            throw new Exception("Invalid frequency value = " + extra);
        }
        // play filename.wav file
        ScriptRunner.PlayWAVFile(filename);
    }

    private bool AllahWords(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;
        List<Verse> verses = client.Selection.Verses;

        try
        {
            string filename = "AllahWords" + Globals.OUTPUT_FILE_EXT;
            if (Client.Books.ContainsKey("Simplified29"))
            {
                verses = Client.Books["Simplified29"].Verses;
            }
            string result = DoAllahWords(client, verses);
            ScriptRunner.SaveText(filename, result);
            ScriptRunner.DisplayFile(filename);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    private bool NotAllahWords(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;
        List<Verse> verses = client.Selection.Verses;

        try
        {
            string filename = "NotAllahWords" + Globals.OUTPUT_FILE_EXT;
            if (Client.Books.ContainsKey("Simplified29"))
            {
                verses = Client.Books["Simplified29"].Verses;
            }
            string result = DoNotAllahWords(client, verses);
            ScriptRunner.SaveText(filename, result);
            ScriptRunner.DisplayFile(filename);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    private bool WordMeaning(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;
        List<Verse> verses = client.Selection.Verses;

        try
        {
            string filename = "WordMeaning" + Globals.OUTPUT_FILE_EXT;
            string result = DoWordMeaning(client, verses);
            ScriptRunner.SaveText(filename, result);
            ScriptRunner.DisplayFile(filename);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    private bool WordTransliteration(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;
        List<Verse> verses = client.Selection.Verses;

        try
        {
            string filename = "WordTransliteration" + Globals.OUTPUT_FILE_EXT;
            string result = DoWordTransliteration(client, verses);
            ScriptRunner.SaveText(filename, result);
            ScriptRunner.DisplayFile(filename);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    private bool WordRoots(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;
        List<Verse> verses = client.Selection.Verses;

        try
        {
            string filename = "WordRoots" + Globals.OUTPUT_FILE_EXT;
            string result = DoWordRoots(client, verses);
            ScriptRunner.SaveText(filename, result);
            ScriptRunner.DisplayFile(filename);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    private bool WordFrequency(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;
        List<Verse> verses = client.Selection.Verses;

        try
        {
            string filename = "WordFrequency" + Globals.OUTPUT_FILE_EXT;
            string result = DoWordFrequency(client, verses);
            ScriptRunner.SaveText(filename, result);
            ScriptRunner.DisplayFile(filename);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    private bool WordOccurrences(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;
        List<Verse> verses = client.Selection.Verses;

        try
        {
            string filename = "WordOccurrences" + Globals.OUTPUT_FILE_EXT;
            string result = DoWordOccurrences(client, verses);
            ScriptRunner.SaveText(filename, result);
            ScriptRunner.DisplayFile(filename);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    private bool WordInformation(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;
        List<Verse> verses = client.Selection.Verses;

        try
        {
            string filename = "WordInformation" + Globals.OUTPUT_FILE_EXT;
            string result = DoWordInformation(client, verses);
            ScriptRunner.SaveText(filename, result);
            ScriptRunner.DisplayFile(filename);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    private string DoAllahWords(Client client, List<Verse> verses)
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
                            "Address" + Globals.DELIMITER +
                            "Text" + Globals.DELIMITER +
                            "Verse" + Globals.DELIMITER +
                            "Word"
                          );

                    int count = 0;
                    foreach (Verse verse in verses)
                    {
                        foreach (Word word in verse.Words)
                        {
                            if (
                                (word.Text == "الله") ||
                                (word.Text == "ءالله") ||
                                (word.Text == "ابالله") ||
                                (word.Text == "اللهم") ||
                                (word.Text == "بالله") ||
                                (word.Text == "تالله") ||
                                (word.Text == "فالله") ||
                                (word.Text == "والله") ||
                                (word.Text == "وتالله") ||
                                (word.Text == "لله") ||
                                (word.Text == "فلله") ||
                                (word.Text == "ولله")
                              )
                            {
                                count++;
                                str.AppendLine(
                                                count + Globals.DELIMITER +
                                                word.Address + Globals.DELIMITER +
                                                word.Text + Globals.DELIMITER +
                                                verse.Address + Globals.DELIMITER +
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
    private string DoNotAllahWords(Client client, List<Verse> verses)
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
                            "#" + Globals.DELIMITER +
                            "Address" + Globals.DELIMITER +
                            "Text" + Globals.DELIMITER +
                            "Verse" + Globals.DELIMITER +
                            "Word"
                          );

                    int count = 0;
                    foreach (Verse verse in verses)
                    {
                        foreach (Word word in verse.Words)
                        {
                            if (
                                (word.Text == "الضلله") ||
                                (word.Text == "الكلله") ||
                                (word.Text == "خلله") ||
                                (word.Text == "خللها") ||
                                (word.Text == "خللهما") ||
                                (word.Text == "سلله") ||
                                (word.Text == "ضلله") ||
                                (word.Text == "ظلله") ||
                                (word.Text == "ظللها") ||
                                (word.Text == "كلله") ||
                                (word.Text == "للهدي") ||
                                (word.Text == "وظللهم") ||
                                (word.Text == "يضلله") ||
                                (word.Text == "اللهب") ||
                                (word.Text == "اللهو")
                              )
                            {
                                count++;
                                str.AppendLine(
                                                count + Globals.DELIMITER +
                                                word.Address + Globals.DELIMITER +
                                                word.Text + Globals.DELIMITER +
                                                verse.Address + Globals.DELIMITER +
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
    private string DoWordMeaning(Client client, List<Verse> verses)
    {
        StringBuilder str = new StringBuilder();
        if (verses != null)
        {
            if (verses.Count > 0)
            {
                str.AppendLine
                    (
                        "Address" + Globals.DELIMITER +
                        "Chapter" + Globals.DELIMITER +
                        "Verse" + Globals.DELIMITER +
                        "Word" + Globals.DELIMITER +
                        "Text" + Globals.DELIMITER +
                        "Meaning"
                      );

                foreach (Verse verse in verses)
                {
                    foreach (Word word in verse.Words)
                    {
                        int w = word.NumberInVerse - 1; // to access info in other books by index
                        int w1 = word.Verse.UnsortedFirstWordNumber - 1;
                        try
                        {
                            str.AppendLine(
                                word.Address + Globals.DELIMITER +
                                verse.Chapter.Number.ToString() + Globals.DELIMITER +
                                verse.NumberInChapter.ToString() + Globals.DELIMITER +
                                word.NumberInVerse.ToString() + Globals.DELIMITER +
                                word.Text + Globals.DELIMITER +
                                Client.WordMeanings[w1 + w]
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
    private string DoWordTransliteration(Client client, List<Verse> verses)
    {
        StringBuilder str = new StringBuilder();
        if (verses != null)
        {
            if (verses.Count > 0)
            {
                if (Client.TranslationBooks.ContainsKey("en.transliteration"))
                {
                    str.AppendLine
                        (
                            "Address" + Globals.DELIMITER +
                            "Chapter" + Globals.DELIMITER +
                            "Verse" + Globals.DELIMITER +
                            "Word" + Globals.DELIMITER +
                            "Text" + Globals.DELIMITER +
                            "Transliteration"
                          );

                    foreach (Verse verse in verses)
                    {
                        int v = verse.CompilationOrder - 1;  // to access info in other books by index
                        foreach (Word word in verse.Words)
                        {
                            int w = word.NumberInVerse - 1; // to access info in other books by index
                            try
                            {
                                str.AppendLine(
                                    word.Address + Globals.DELIMITER +
                                    verse.Chapter.Number.ToString() + Globals.DELIMITER +
                                    verse.NumberInChapter.ToString() + Globals.DELIMITER +
                                    word.NumberInVerse.ToString() + Globals.DELIMITER +
                                    word.Text + Globals.DELIMITER +
                                    Client.TranslationBooks["en.transliteration"].Verses[v].Words[w].Text
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
        }
        return str.ToString();
    }
    private string DoWordRoots(Client client, List<Verse> verses)
    {
        StringBuilder str = new StringBuilder();
        if (verses != null)
        {
            if (verses.Count > 0)
            {
                if (Client.TranslationBooks.ContainsKey("en.transliteration"))
                {
                    str.AppendLine
                        (
                            "Address" + Globals.DELIMITER +
                            "Chapter" + Globals.DELIMITER +
                            "Verse" + Globals.DELIMITER +
                            "Word" + Globals.DELIMITER +
                            "Text" + Globals.DELIMITER +
                            "Roots"
                          );

                    foreach (Verse verse in verses)
                    {
                        int v = verse.CompilationOrder - 1;  // to access info in other books by index
                        foreach (Word word in verse.Words)
                        {
                            int w = word.NumberInVerse - 1; // to access info in other books by index
                            try
                            {
                                List<string> roots = Client.GetRoots(client.TextMode, word.Text);
                                StringBuilder roots_str = new StringBuilder();
                                foreach (string root in roots)
                                {
                                    roots_str.Append(root + Globals.SUB_DELIMITER);
                                }
                                if (roots_str.Length > 0)
                                {
                                    roots_str.Remove(roots_str.Length - Globals.SUB_DELIMITER.Length, Globals.SUB_DELIMITER.Length);
                                }

                                str.AppendLine(
                                    word.Address + Globals.DELIMITER +
                                    verse.Chapter.Number.ToString() + Globals.DELIMITER +
                                    verse.NumberInChapter.ToString() + Globals.DELIMITER +
                                    word.NumberInVerse.ToString() + Globals.DELIMITER +
                                    word.Text + Globals.DELIMITER +
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
        }
        return str.ToString();
    }
    private string DoWordFrequency(Client client, List<Verse> verses)
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
                            if (!word_infos.ContainsKey(word.Text))
                            {
                                WordInfo word_info = new WordInfo();
                                word_info.Text = word.Text;
                                word_info.Frequency = 1;
                                word_info.Addresses = new List<string>();
                                word_info.Addresses.Add(word.Address);

                                word_infos.Add(word.Text, word_info);
                            }
                            else // word already exists
                            {
                                word_infos[word.Text].Frequency++;
                                word_infos[word.Text].Addresses.Add(word.Address);
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
                    "Word" + Globals.DELIMITER +
                    "Frequency" + Globals.DELIMITER +
                    "Addresses"
                );
                foreach (string key in word_infos.Keys)
                {
                    StringBuilder addresses = new StringBuilder();
                    if (word_infos[key].Addresses != null)
                    {
                        foreach (string address in word_infos[key].Addresses)
                        {
                            addresses.Append(address + Globals.SUB_DELIMITER);
                        }
                        addresses.Remove(addresses.Length - Globals.SUB_DELIMITER.Length, Globals.SUB_DELIMITER.Length);
                    }

                    str.AppendLine
                    (
                        word_infos[key].Text + Globals.DELIMITER +
                        word_infos[key].Frequency + Globals.DELIMITER +
                        addresses
                   );
                }
            }
        }
        return str.ToString();
    }
    private string DoWordOccurrences(Client client, List<Verse> verses)
    {
        StringBuilder str = new StringBuilder();
        if (verses != null)
        {
            if (verses.Count > 0)
            {
                str.AppendLine
                (
                    "Text" + Globals.DELIMITER +
                    "Occurrence" + Globals.DELIMITER +
                    "Frequency"
                );

                Dictionary<string, int> word_occurrences = new Dictionary<string, int>();
                Dictionary<string, int> word_frequencies = new Dictionary<string, int>();
                DoCalculateWordOccurencesAndFrequencies(verses, ref word_occurrences, ref word_frequencies);

                foreach (Verse verse in verses)
                {
                    foreach (Word word in verse.Words)
                    {
                        str.AppendLine
                        (
                            word.Text + Globals.DELIMITER +
                            word_occurrences[word.UnsortedAddress].ToString() + Globals.DELIMITER +
                            word_frequencies[word.UnsortedAddress].ToString()
                        );
                    }
                }
            }
        }
        return str.ToString();
    }
    private string DoWordInformation(Client client, List<Verse> verses)
    {
        StringBuilder str = new StringBuilder();
        if (verses != null)
        {
            if (verses.Count > 0)
            {
                if (Client.TranslationBooks.ContainsKey("en.transliteration"))
                {
                    str.AppendLine
                    (
                        "Address" + Globals.DELIMITER +
                        "Chapter" + Globals.DELIMITER +
                        "Verse" + Globals.DELIMITER +
                        "Word" + Globals.DELIMITER +
                        "Text" + Globals.DELIMITER +
                        "Transliteration" + Globals.DELIMITER +
                        "Roots" + Globals.DELIMITER +
                        "Meaning" + Globals.DELIMITER +
                        "Occurrence" + Globals.DELIMITER +
                        "Frequency"
                    );

                    Dictionary<string, int> word_occurrences = new Dictionary<string, int>();
                    Dictionary<string, int> word_frequencies = new Dictionary<string, int>();
                    DoCalculateWordOccurencesAndFrequencies(verses, ref word_occurrences, ref word_frequencies);

                    foreach (Verse verse in verses)
                    {
                        int v = verse.CompilationOrder - 1;  // to access info in other books by index
                        foreach (Word word in verse.Words)
                        {
                            int w = word.NumberInVerse - 1; // to access info in other books by index
                            int w1 = word.Verse.UnsortedFirstWordNumber - 1;

                            List<string> roots = Client.GetRoots(client.TextMode, word.Text);
                            StringBuilder roots_str = new StringBuilder();
                            foreach (string root in roots)
                            {
                                roots_str.Append(root + Globals.SUB_DELIMITER);
                            }
                            if (roots_str.Length > 0)
                            {
                                roots_str.Remove(roots_str.Length - Globals.SUB_DELIMITER.Length, Globals.SUB_DELIMITER.Length);
                            }

                            str.AppendLine
                            (
                                word.Address + Globals.DELIMITER +
                                verse.Chapter.Number.ToString() + Globals.DELIMITER +
                                verse.NumberInChapter.ToString() + Globals.DELIMITER +
                                word.NumberInVerse.ToString() + Globals.DELIMITER +
                                word.Text + Globals.DELIMITER +
                                Client.TranslationBooks["en.transliteration"].Verses[v].Words[w].Text + Globals.DELIMITER +
                                roots_str.ToString() + Globals.DELIMITER +
                                Client.WordMeanings[w1 + w] + Globals.DELIMITER +
                                word_occurrences[word.UnsortedAddress].ToString() + Globals.DELIMITER +
                                word_frequencies[word.UnsortedAddress].ToString()
                            );
                        }
                    }
                }
            }
        }
        return str.ToString();
    }
    private void DoCalculateWordOccurencesAndFrequencies(List<Verse> verses, ref Dictionary<string, int> word_occurrences, ref Dictionary<string, int> word_frequencies)
    {
        Dictionary<string, int> text_frequencies = new Dictionary<string, int>();
        foreach (Verse verse in verses)
        {
            foreach (Word word in verse.Words)
            {
                if (text_frequencies.ContainsKey(word.Text))
                {
                    text_frequencies[word.Text]++;
                    word_occurrences[word.UnsortedAddress] = text_frequencies[word.Text];
                }
                else
                {
                    text_frequencies.Add(word.Text, 1);
                    word_occurrences.Add(word.UnsortedAddress, 1);
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
                    if (key == word.Text)
                    {
                        if (word_frequencies.ContainsKey(word.UnsortedAddress))
                        {
                            word_frequencies[word.UnsortedAddress] += text_frequencies[word.Text];
                        }
                        else
                        {
                            word_frequencies.Add(word.UnsortedAddress, text_frequencies[word.Text]);
                        }
                    }
                }
            }
        }
    }

    private bool VerseStatistics(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;
        List<Verse> verses = client.Selection.Verses;

        try
        {
            string result = DoVerseStatistics(client, verses);

            string filename = client.NumerologySystemName + "_" + "VerseStatistics" + Globals.OUTPUT_FILE_EXT;
            ScriptRunner.SaveText(filename, result);
            ScriptRunner.DisplayFile(filename);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    private bool ChapterStatistics(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;
        List<Verse> verses = client.Selection.Verses;

        try
        {
            List<Chapter> chapters = Book.GetChapters(verses);
            string result = DoChapterStatistics(client, chapters);

            string filename = client.NumerologySystemName + "_" + "ChapterStatistics" + Globals.OUTPUT_FILE_EXT;
            ScriptRunner.SaveText(filename, result);
            ScriptRunner.DisplayFile(filename);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    private bool VerseValueDigitSums(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;
        List<Verse> verses = client.Selection.Verses;

        try
        {
            StringBuilder str = new StringBuilder();
            foreach (Verse verse in verses)
            {
                long value = verse.Value;
                int digit_sum = Numbers.DigitSum(value);
                str.Append(verse.PaddedAddress + "\t" + value.ToString(Globals.NUMBER_FORMAT) + "\t" + digit_sum.ToString() + "\r\n");
            }

            string filename = client.NumerologySystemName + "_" + "VerseValueDigitSums" + Globals.OUTPUT_FILE_EXT;
            ScriptRunner.SaveText(filename, str.ToString());
            ScriptRunner.DisplayFile(filename);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    private bool ChapterValueDigitSums(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;
        List<Verse> verses = client.Selection.Verses;

        try
        {
            StringBuilder str = new StringBuilder();
            List<Chapter> chapters = Book.GetChapters(verses);
            foreach (Chapter chapter in chapters)
            {
                long value = chapter.Value;
                int digit_sum = Numbers.DigitSum(value);
                str.Append(chapter.Number + "\t" + value.ToString(Globals.NUMBER_FORMAT) + "\t" + digit_sum.ToString() + "\r\n");
            }

            string filename = client.NumerologySystemName + "_" + "ChapterValueDigitSums" + Globals.OUTPUT_FILE_EXT;
            ScriptRunner.SaveText(filename, str.ToString());
            ScriptRunner.DisplayFile(filename);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    private bool VerseValueXDigitSum(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;
        List<Verse> verses = client.Selection.Verses;

        try
        {
            // Query the whole book, not just the current selection
            verses = client.Book.Verses;

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
                        return false;  // invalid extra data
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
                    str.Append(verse.Text + "\t");
                    str.Append("\r\n");
                }
            }

            string filename = client.NumerologySystemName + "_" + "VerseValueXDigitSum" + "_" + extra + "_" + DateTime.Now.ToString("yyyyMMdd_hhmmss") + ".csv";
            ScriptRunner.SaveText(filename, str.ToString());
            ScriptRunner.DisplayFile(filename);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    private bool PrimeVerseValueXDigitSum(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;
        List<Verse> verses = client.Selection.Verses;

        try
        {
            // Query the whole book, not just the current selection
            verses = client.Book.Verses;

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
                        return false;  // invalid extra data
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
                    str.Append(verse.Text + "\t");
                    str.Append("\r\n");
                }
            }

            string filename = client.NumerologySystemName + "_" + "PrimeVerseValueXDigitSum" + "_" + extra + "_" + DateTime.Now.ToString("yyyyMMdd_hhmmss") + ".csv";
            ScriptRunner.SaveText(filename, str.ToString());
            ScriptRunner.DisplayFile(filename);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    private bool ConcatenateChapterVerses(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;
        List<Verse> verses = client.Selection.Verses;

        try
        {
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
            ScriptRunner.SaveText(filename, str.ToString());
            ScriptRunner.DisplayFile(filename);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    private bool ConcatenateChaptersVerses(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;
        List<Verse> verses = client.Selection.Verses;

        try
        {
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
            ScriptRunner.SaveText(filename, str.ToString());
            ScriptRunner.DisplayFile(filename);
            return false; // so not to close Script window
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return false; // to stay in the Script window
        }
    }
    private string DoVerseStatistics(Client client, List<Verse> verses)
    {
        StringBuilder str = new StringBuilder();

        str.Append("#" + Globals.DELIMITER + "Page" + Globals.DELIMITER + "Chapter" + Globals.DELIMITER + "Verse" + Globals.DELIMITER + "Words" + Globals.DELIMITER + "Letters" + Globals.DELIMITER + "Unique" + Globals.DELIMITER + "Value" + Globals.DELIMITER + "P" + Globals.DELIMITER + "AP" + Globals.DELIMITER + "PP" + Globals.DELIMITER + "C" + Globals.DELIMITER + "AC" + Globals.DELIMITER + "PC" + Globals.DELIMITER);

        Dictionary<char, long> numerology_system = Client.NumerologySystems[client.NumerologySystemName];
        foreach (char key in numerology_system.Keys)
        {
            str.Append(key.ToString() + Globals.DELIMITER);
        }
        str.Append("PrimeFactors" + Globals.DELIMITER + "Text" + Globals.DELIMITER + "Translation");
        str.Append("\r\n");

        int count = 0;
        foreach (Verse verse in verses)
        {
            count++;
            str.Append(verse.Number.ToString() + Globals.DELIMITER);
            str.Append(verse.Page.Number.ToString() + Globals.DELIMITER);
            str.Append(verse.Chapter.Number.ToString() + Globals.DELIMITER);
            str.Append(verse.NumberInChapter.ToString() + Globals.DELIMITER);
            str.Append(verse.Words.Count.ToString() + Globals.DELIMITER);
            str.Append(verse.LetterCount.ToString() + Globals.DELIMITER);
            str.Append(verse.UniqueLetters.Count.ToString() + Globals.DELIMITER);

            long value = verse.Value;
            str.Append(value.ToString() + Globals.DELIMITER);

            long p = Numbers.IndexOfPrime(value);
            long ap = Numbers.IndexOfAdditivePrime(value);
            long pp = Numbers.IndexOfPurePrime(value);
            long c = Numbers.IndexOfComposite(value);
            long ac = Numbers.IndexOfAdditiveComposite(value);
            long pc = Numbers.IndexOfPureComposite(value);
            str.Append((p == -1 ? "-" : p.ToString()) + Globals.DELIMITER
                           + (ap == -1 ? "-" : ap.ToString()) + Globals.DELIMITER
                           + (pp == -1 ? "-" : pp.ToString()) + Globals.DELIMITER
                           + (c == -1 ? "-" : c.ToString()) + Globals.DELIMITER
                           + (ac == -1 ? "-" : ac.ToString()) + Globals.DELIMITER
                           + (pc == -1 ? "-" : pc.ToString())
                         );
            str.Append(Globals.DELIMITER);

            foreach (char key in numerology_system.Keys)
            {
                str.Append(verse.GetLetterFrequency(key).ToString() + Globals.DELIMITER);
            }
            str.Append(Numbers.FactorizeToString(value) + Globals.DELIMITER);
            str.Append(verse.Text);
            str.Append("\r\n");
        }
        return str.ToString();
    }
    private string DoChapterStatistics(Client client, List<Chapter> chapters)
    {
        StringBuilder str = new StringBuilder();
        str.Append("#" + Globals.DELIMITER + "Name" + Globals.DELIMITER + "Page" + Globals.DELIMITER + "Chapter" + Globals.DELIMITER + "Verses" + Globals.DELIMITER + "Words" + Globals.DELIMITER + "Letters" + Globals.DELIMITER + "Unique" + Globals.DELIMITER + "Value" + Globals.DELIMITER + "P" + Globals.DELIMITER + "AP" + Globals.DELIMITER + "PP" + Globals.DELIMITER + "C" + Globals.DELIMITER + "AC" + Globals.DELIMITER + "PC" + Globals.DELIMITER);

        Dictionary<char, long> numerology_system = Client.NumerologySystems[client.NumerologySystemName];
        foreach (char key in numerology_system.Keys)
        {
            str.Append(key.ToString() + Globals.DELIMITER);
        }

        str.Append("PrimeFactors");
        str.Append("\r\n");
        foreach (Chapter chapter in chapters)
        {
            str.Append(chapter.Number.ToString() + Globals.DELIMITER);
            str.Append(chapter.Name + Globals.DELIMITER);
            str.Append(chapter.Verses[0].Page.Number.ToString() + Globals.DELIMITER);
            str.Append(chapter.CompilationOrder.ToString() + Globals.DELIMITER);
            str.Append(chapter.Verses.Count.ToString() + Globals.DELIMITER);

            long chapter_value = chapter.Value;
            str.Append(chapter.WordCount.ToString() + Globals.DELIMITER);
            str.Append(chapter.LetterCount.ToString() + Globals.DELIMITER);
            str.Append(chapter.UniqueLetters.Count.ToString() + Globals.DELIMITER);
            str.Append(chapter_value.ToString() + Globals.DELIMITER);

            long p = Numbers.IndexOfPrime(chapter_value);
            long ap = Numbers.IndexOfAdditivePrime(chapter_value);
            long pp = Numbers.IndexOfPurePrime(chapter_value);
            long c = Numbers.IndexOfComposite(chapter_value);
            long ac = Numbers.IndexOfAdditiveComposite(chapter_value);
            long pc = Numbers.IndexOfPureComposite(chapter_value);
            str.Append((p == -1 ? "-" : p.ToString()) + Globals.DELIMITER
                           + (ap == -1 ? "-" : ap.ToString()) + Globals.DELIMITER
                           + (pp == -1 ? "-" : pp.ToString()) + Globals.DELIMITER
                           + (c == -1 ? "-" : c.ToString()) + Globals.DELIMITER
                           + (ac == -1 ? "-" : ac.ToString()) + Globals.DELIMITER
                           + (pc == -1 ? "-" : pc.ToString())
                         );
            str.Append(Globals.DELIMITER);

            foreach (char key in numerology_system.Keys)
            {
                str.Append(chapter.GetLetterFrequency(key) + Globals.DELIMITER);
            }
            str.Append(Numbers.FactorizeToString(chapter_value));
            str.Append("\r\n");
        }
        return str.ToString();
    }

    /// <summary>
    /// Write your C# script inside this method.
    /// Don't change its name or parameter list
    /// This method is an example of finding all verses rhat have a prime value
    /// and a P, AP, PP, or a given digit_sum (e.g 8 or 19, or 0 -> any)
    /// </summary>
    /// <param name="client">Client object holding a reference to the currently selected Book object in TextMode (eg Simplified29)</param>
    /// <param name="extra">any user parameter in the TextBox next to the EXE button (ex Frequency, LettersToJump, DigitSum target, etc)</param>
    /// <returns>true to disply back in QuranCode matching verses. false to keep script window open</returns>
    private bool MyMethod(Client client, string extra)
    {
        if (client == null) return false;
        if (client.Selection == null) return false;
        List<Verse> verses = client.Selection.Verses;

        if (client.Book != null)
        {
            // OPTIONAL: query the whole book, not just the current verses
            verses = client.Book.Verses;

            client.FoundVerses = new List<Verse>();
            foreach (Verse verse in verses)
            {
                long value = verse.Value;

                bool digit_sum_ok = false;
                int target;

                if (extra == "")
                {
                    target = 0; // any digit sum
                    digit_sum_ok = true;
                }
                else if (extra.ToUpper() == "P") // target == prime digit sum
                {
                    digit_sum_ok = Numbers.IsPrime(Numbers.DigitSum(value));
                }
                else if (extra.ToUpper() == "AP") // target == additive prime digit sum
                {
                    digit_sum_ok = Numbers.IsAdditivePrime(Numbers.DigitSum(value));
                }
                else if (extra.ToUpper() == "PP") // target == pure prime digit sum
                {
                    digit_sum_ok = Numbers.IsPurePrime(Numbers.DigitSum(value));
                }
                else
                {
                    if (int.TryParse(extra, out target))
                    {
                        digit_sum_ok = (Numbers.DigitSum(value) == target);
                    }
                    else
                    {
                        return false; // to stay in the Script window
                    }
                }

                if (digit_sum_ok && Numbers.IsPrime(value))
                {
                    client.FoundVerses.Add(verse);
                }
            }
            return true; // to close Script window and show result
        }
        return false; // to stay in the Script window
    }

    /// <summary>
    /// Run implements IScriptRunner interface to be invoked by QuranCode application
    /// </summary>
    /// <param name="args">any number and type of arguments</param>
    /// <returns>return any type</returns>
    public object Run(object[] args)
    {
        try
        {
            if (args.Length == 2)   // ScriptMethod(Client, string)
            {
                Client client = args[0] as Client;
                string extra = args[1].ToString();

                if (client != null)
                {
                    //return MyMethod(client, extra);
                    //return PrimeVerseValueXDigitSum(client, extra);
                    return VerseValuesSound(client, extra);
                    //return JumpLettersByFibonacciNumbers(client, extra);
                }
            }
            return null;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
            return null;
        }
    }
}
