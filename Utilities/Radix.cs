/*
 * Created by SharpDevelop.
 * User: 	rob tillaart
 * Date: 	29/09/2005
 * Time: 	19:18
 * Version:	1.00
 */
using System;

public class Radix
{
    static string digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    static string ErrRadixTooLarge1 = "RadixError: radix larger than 36.";
    static string ErrRadixTooLarge2 = "RadixError: radix larger than 1000000.";
    static string ErrRadixTooSmall = "RadixError: radix smaller than 2.";
    static string ErrRadixFormat = "RadixError: number not in radix format.";
    static string ErrRadixDecode = "RadixError: generic decode error.";
    static string ErrRadixNoSymbolFormat = "RadixError: number not in symbolic_display format.";

    public static string Spaces(string value, int number)
    {
        return Spaces(value, number, ' ');
    }
    public static string Spaces(string value, int number, char separator)
    {
        string result = "";
        int j = 0;
        for (int i = value.Length - 1; i >= 0; i--)
        {
            j++;
            result = value[i] + result;
            if (j % number == 0) result = separator + result;
        }
        if (result[0] == separator) result = result.Substring(1);
        return result;
    }
    private static void CheckArg(long radix, bool symbolic_display)
    {
        if ((radix > 36) && (!symbolic_display))
        {
            throw new Exception(ErrRadixTooLarge1);
        }
        if (radix > 1000000)
        {
            throw new Exception(ErrRadixTooLarge2);
        }
        if (radix < 2)
        {
            throw new Exception(ErrRadixTooSmall);
        }
    }

    public static string Encode(long value, long radix)
    {
        return Encode(value, radix, false);
    }
    public static string Encode(long value, long radix, bool symbolic_display)
    {
        // check parameters
        CheckArg(radix, symbolic_display);

        // work in positive domain
        long t = Math.Abs(value);

        // return value
        string result = "";

        if (symbolic_display)
        {
            if (t == 0)
            {
                result = ",0";
            }
            while (t > 0)
            {
                // split of one digit
                long r = t % radix;
                // convert it and add it to the return string
                result = "," + r.ToString() + result;
                t = (t - r) / radix;
            }
            result = result.Substring(1);			// strip one ','
            // add sign
            if (value < 0)
            {
                result = "-," + result;
            }
            if (value == 0)
            {
                result = "0";
            }
            result = "[(" + radix + ")," + result + "]";
        }
        else
        {
            if (t == 0)
            {
                result = "0";
            }
            while (t > 0)
            {
                // split of one digit
                long r = t % radix;
                // convert it and add it to the return string
                result = digits[(int)r] + result;
                t = (t - r) / radix;
            }
            if (value < 0)
            {
                // add sign
                result = "-" + result;
            }
            if (value == 0)
            {
                result = "0";
            }
        }
        return result;
    }
    public static long Decode(string value, long radix)
    {
        return Decode(value, radix, false);
    }
    public static long Decode(string value, long radix, bool symbolic_display)
    {
        if (String.IsNullOrEmpty(value)) return 0L;
        CheckArg(radix, symbolic_display);

        long result = 0;
        try
        {
            if (symbolic_display)
            {
                string ws = value.Trim();
                if (ws[0] != '[')
                {
                    throw new Exception(ErrRadixNoSymbolFormat);
                }
                // strip [(
                ws = ws.Substring(2);
                // get radix
                int pos = ws.IndexOf(')');
                long tr = Int64.Parse(ws.Substring(0, pos));
                // strip it
                ws = ws.Substring(pos + 2);
                ws = ws.Remove(ws.Length - 1, 1);		// strip ]

                char sign = ws[0];
                int si = 1;
                if ((sign == '-') || (sign == '+'))
                {
                    if (sign == '-') si = -1;
                    ws = ws.Substring(2);				// skip sign and ,
                }

                string[] t = ws.Split(',');
                for (int i = 0; i < t.Length; i++)
                {
                    result *= radix;
                    long l = long.Parse(t[i]);
                    if (l >= radix) throw new Exception(ErrRadixFormat);
                    result += l;
                }
                // add sign
                result *= si;
            }
            else
            {
                string ws = value.Trim();
                char sign = ws[0];
                int si = 1;
                if ((sign == '-') || (sign == '+'))
                {
                    if (sign == '-') si = -1;
                    ws = ws.Substring(1);
                }

                for (int i = 0; i < ws.Length; i++)
                {
                    result *= radix;
                    char c = ws[i];
                    long l = digits.IndexOf(c);
                    if (l >= radix) break;//AliAdams //throw new Exception(ErrRadixFormat);
                    result += l;
                }
                // add sign
                result *= si;
            }
        }
        catch
        {
            throw new Exception(ErrRadixDecode);
        }

        return result;
    }

    public static string Encode(double value, long radix)
    {
        return Encode(value, radix, false);
    }
    public static string Encode(double value, long radix, bool symbolic_display)
    {
        CheckArg(radix, symbolic_display);

        double t = Math.Abs(value);

        // first part before decimal point
        long t1 = (long)t;

        // t2 holds part after decimal point
        double t2 = t - t1;

        // return value;
        string result = "";

        if (symbolic_display)
        {
            if (value == 0.0)
            {
                result = ",0";
            }
            // process part before decimal point
            while (t1 > 0)
            {
                long r = t1 % radix;
                result = "," + r.ToString() + result;
                t1 = (t1 - r) / radix;
            }
            result = result.Substring(1);	// strip one ','

            // after the decimal point
            if (t2 > 0.0)
            {
                result += ",.,";
            }
            int maxdigit = 50; // to prevent endless loop
            while (t2 > 0)
            {
                long r = (long)(t2 * radix);
                result += r.ToString() + ",";
                t2 = (t2 * radix) - r;

                // forced break after maxdigits
                maxdigit--;
                if (maxdigit == 0) break;
            }
            result = result.Substring(0, result.Length - 1);	// strip one ','
            if (value < 0)
            {
                result = "-," + result;
            }
            result = "[(" + radix + ")," + result + "]";
        }
        else
        {
            if (value == 0.0)
            {
                result = "0";
            }
            // process part before decimal point
            while (t1 > 0)
            {
                long r = t1 % radix;
                result = digits[(int)r] + result;
                t1 = (t1 - r) / radix;
            }

            // after the decimal point
            if (t2 > 0.0)
            {
                result += ".";
            }
            int maxdigit = 50; // to prevent endless loop
            while (t2 > 0)
            {
                long r = (long)(t2 * radix);
                result += digits[(int)r];
                t2 = (t2 * radix) - r;

                // forced break after 10 digits
                maxdigit--;
                if (maxdigit == 0) break;
            }
            if (value < 0)
            {
                result = "-" + result;
            }
        }
        return result;
    }
    public static void Decode(string value, long radix, out double result)
    {
        Decode(value, radix, out result, false);
    }
    public static void Decode(string value, long radix, out double result, bool symbolic_display)
    {
        result = 0; // out param must be filled before return
        if (String.IsNullOrEmpty(value)) return;
        CheckArg(radix, symbolic_display);

        try
        {
            double tradix = 1;
            if (symbolic_display)
            {
                string ws = value.Trim();
                // strip [(
                ws = ws.Substring(2);
                // get radix
                int pos = ws.IndexOf(')');
                long tr = Int64.Parse(ws.Substring(0, pos));
                // strip it
                ws = ws.Substring(pos + 2);
                ws = ws.Remove(ws.Length - 1, 1);		// strip ]

                char sign = ws[0];
                int si = 1;
                if ((sign == '-') || (sign == '+'))
                {
                    if (sign == '-') si = -1;
                    ws = ws.Substring(2);					// skip sign and ,
                }

                string[] t = ws.Split(',');
                bool before = true;
                for (int i = 0; i < t.Length; i++)
                {
                    if (t[i] == ".")
                    {
                        before = false;
                        continue;
                    }
                    // next 'digit'
                    long l = long.Parse(t[i]);
                    if (l >= radix) throw new Exception(ErrRadixFormat);

                    if (before)
                    {
                        // process before dec. point
                        result *= radix;
                        result += l;
                    }
                    else
                    {
                        // process after decimal point
                        tradix *= radix;
                        result += l / tradix;
                    }
                }

                // add sign
                result *= si;
            }
            else
            {
                string ws = value.Trim();
                char sign = ws[0];
                int si = 1;
                if ((sign == '-') || (sign == '+'))
                {
                    if (sign == '-') si = -1;
                    ws = ws.Substring(1);
                }

                bool before = true;
                for (int i = 0; i < ws.Length; i++)
                {
                    if (ws[i] == '.')
                    {
                        before = false;
                        continue;
                    }
                    // next 'digit'
                    long l = digits.IndexOf(ws[i]);
                    if (l >= radix) throw new Exception(ErrRadixFormat);

                    if (before)
                    {
                        // process before dec. point
                        result *= radix;
                        result += l;
                    }
                    else
                    {
                        // process after decimal point
                        tradix *= radix;
                        result += digits.IndexOf(ws[i]) / tradix;
                    }
                }
                // add sign
                result *= si;
            }
        }
        catch
        {
            throw new Exception(ErrRadixDecode);
        }
    }

    public static void Main(string[] args)
    {
        string s = "";
        long l = 0;
        double d = 0.0;

        // run once with symbolic_displaybolic notation false
        // and once with symbolic_displaybolic notation true
        for (int tr = 0; tr < 2; tr++)
        {
            bool symbolic_display = (tr == 1);

            // TEST RUN
            Console.WriteLine("TESTRUN 1: 10000 integers in base 2 to 36");
            Console.WriteLine(DateTime.Now.ToString("mm:ss:fff"));
            for (long i = 0; i < 10000; i++)
            {
                for (int r = 2; r <= 36; r++)
                {
                    s = Radix.Encode(i, r, symbolic_display);
                    l = Radix.Decode(s, r, symbolic_display);
                    if (i != l)
                    {
                        Console.WriteLine("error: r i l {0} {1} {2}", r, i, l);
                    }
                    d = Radix.Decode(s, r, symbolic_display);
                    if (((long)d) != i)
                    {
                        Console.WriteLine("error: r, l d {0} {1} {2}", r, l, d);
                    }
                }
            }
            Console.WriteLine(DateTime.Now.ToString("mm:ss:fff"));
            Console.WriteLine();

            Console.WriteLine("TESTRUN 2: two well known doubles in base 2 to 36");
            Console.ReadLine();
            Console.WriteLine("\n PI \n");
            for (int r = 2; r <= 36; r++)
            {
                s = Radix.Encode(Math.PI, r, symbolic_display);
                d = Radix.Decode(s, r, symbolic_display);
                if (Math.PI != d)
                {
                    Console.WriteLine("error: radix {0} : {1}  {2}", r, d, Math.PI);
                }
            }

            Console.WriteLine("\n  E \n");
            for (int r = 2; r <= 36; r++)
            {
                s = Radix.Encode(Math.E, r, symbolic_display);
                d = Radix.Decode(s, r, symbolic_display);
                if (Math.E != d)
                {
                    Console.WriteLine("error: radix {0} : {1}  {2}", r, d, Math.E);
                }
            }
            Console.WriteLine();

            Console.WriteLine("TESTRUN 3: force an error while decoding");
            Console.ReadLine();
            try
            {
                l = Radix.Decode("C000", 7, symbolic_display);
                Console.WriteLine("{0}", l);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine();
            }
        }

        Console.WriteLine("TESTRUN 4: some usage");

        s = Radix.Encode(100, 4, true);
        Console.WriteLine(s);
        l = Radix.Decode(s, 4, true);
        Console.WriteLine(l);
        s = Radix.Encode(100, 16);
        Console.WriteLine(s);
        Console.WriteLine();
        s = Radix.Encode(1000000000000, 11);
        Console.WriteLine(s);
        Console.WriteLine(Radix.Spaces(s, 2, '.'));
        Console.WriteLine(Radix.Spaces(s, 4, '.'));
        Console.WriteLine(Radix.Spaces(s, 4));
        Console.WriteLine();

        Console.WriteLine("Convert a hexadecimal IP address");
        l = Radix.Decode("C0FFCCBB", 16);
        s = Radix.Encode(l, 256, true);
        Console.WriteLine(s);
        l = Radix.Decode(s, 256, true);
        Console.WriteLine(l.ToString());
        s = Radix.Encode(l, 16);
        Console.WriteLine(s);

        Console.WriteLine();

        for (int i = 0; i <= 100; i++)
        {
            Console.Write(Radix.Encode(i, 13) + " ");
        }

        Console.WriteLine();
        Console.WriteLine("ready");
    }
}
