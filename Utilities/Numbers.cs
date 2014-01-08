using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

// 1 is the number unit == the father of all numbers, primes are the mothers of their multiples
// prime: whole number with two distinct factors, 1 and itself == mother of its multiples
// additive prime: prime with prime digit sum
// pure prime: additive prime with prime digits
public enum NumberType
{
    Any,                    // any integer number from 1 to infinity
    Odd,                    // indivisible by 2
    Even,                   // divisible by 2
    Prime,                  // indivisible by any other number (divisibility by 1 never divides a number into smaller parts)
    AdditivePrime,          // prime with prime digit sum
    PurePrime,              // additive prime with prime digits
    Composite,              // divisible by some prime (child of the mother prime)
    AdditiveComposite,      // composite with composite digit sum
    PureComposite,          // additiver composite with composite digits
    MersennePrime,          // p of prime 2^p - 1
    Fibonacci,              // n2 = n0 + n1 and n2/n1 ~= the golden ratio
    Tetrahedral,            // n*(n+1)*(n+2)/6   = 1, 4, 10, 20, 35, 56, 84, 120, 165, 220, 286, 364, 455, 560, 680, 816, 969, 1140, 1330, 1540, 1771, 2024, 2300, 2600, 2925, 3276, 3654, 4060, 4495, 4960, 5456, 5984, 6545, 7140, 7770, 8436, 9139, 9880, 10660, 11480, 12341, 13244, 14190, 15180, ...
    Pyramidal,              // n*(n+1)*(2*n+1)/6 = 1, 5, 14, 30, 55, 91, 140, 204, 285, 385, 506, 650, 819, 1015, 1240, 1496, 1785, 2109, 2470, 2870, 3311, 3795, 4324, 4900, 5525, 6201, 6930, 7714, 8555, 9455, 10416, 11440, 12529, 13685, 14910, 16206, 17575, 19019, 20540, 22140, 23821, 25585, 27434, 29370, ...
    Cubic,                  // n^3               = 1, 8, 27, 64, 125, ...
    None                    // not a number
};


//http://en.wikipedia.org/wiki/Polygon
//----------------------------------------------------
//Ones		        Tens			Final suffix
//----------------------------------------------------
//1 hen-		    10 deca-		-gon 
//2 do-		        20 -cosa- 
//3 tri-		    30 triaconta- 
//4 tetra-	        40 tetraconta- 
//5 penta-	        50 pentaconta- 
//6 hexa-		    60 hexaconta- 
//7 hepta-	        70 heptaconta- 
//8 octa-		    80 octaconta- 
//9 ennea-/nona-	90 enneaconta-/nonaconta- 
//----------------------------------------------------
//
//Examples:
//7-gon  = hepta-                   -gon		= heptagon
//19-gon = nona-    deca-           -gon 		= nonadecagon 
//42-gon = do-      tetraconta-     -gon 		= dotetracontagon 
//50-gon =          pentaconta-     -gon		= pentacontagon 
//
//N-gon (for N > 99)			                = N-gon
//100-gon					                    = 100-gon
//----------------------------------------------------
public enum PolygonType
{
    Triangular,
    Square,
    Pentagonal,
    Hexagonal,
    Heptagonal,
    Octagonal,
    Nonagonal,
    Decagonal,
    Hendecagonal,
    Dodecagonal,
    Tridecagonal,
    Tetradecagonal,
    Pentadecagonal,
    Hexadecagonal,
    Heptadecagonal,
    Octadecagonal,
    Nonadecagonal,
    Icosagonal,
    Icosihenagonal,
    Icosidigonal,
    Icositrigonal,
    Icositetragonal
};

// https://oeis.org  Chemical polyhex hydrocarbons with 19 hexagons
public enum PolyhexType
{
    C2hPolyhexHydrocarbon,
    C2vPolyhexHydrocarbon
};


// = ≠ ≡ < ≤ > ≥
public enum ComparisonOperator { Equal, NotEqual, LessThan, LessThanOrEqual, GreaterThan, GreaterThanOrEqual, Unknown };

// + - * / % (remainder)
public enum ArithmeticOperator { Plus, Minus, Multiple, Divide, Modulus };

public static class Numbers
{
    // pi = circumference / diameter ~= 355/113
    // e = Euler's number = 0SUM∞(1/n!)
    // phi is golden ratio = (sqrt(5)+1)/2
    public const double PI = 3.141592653589793238462643383279D;
    public const double E = 2.718281828459045235360287471352D;
    public const double PHI = 1.618033988749894848204586834365D;

    private static int MAX_COUNT =
        int.MaxValue /
        (
            (Globals.EDITION == Edition.Lite) ? (4096) :
            (Globals.EDITION == Edition.Standard) ? (1024) :
            (Globals.EDITION == Edition.Grammar) ? (1024) :
            (Globals.EDITION == Edition.Research) ? (256) : 1024
        );

    private static string s_constants_directory = "Numbers";
    static Numbers()
    {
        if (!Directory.Exists(s_constants_directory))
        {
            Directory.CreateDirectory(s_constants_directory);
        }

        // >20Mb files for users to download so instead generate it at runtime when needed
        ////GeneratePrimes();
        ////GenerateAdditivePrimes();
        ////GeneratePurePrimes();
        ////GenerateComposites();
        ////GenerateAdditiveComposites();
        ////GeneratePureComposites();

        ////SavePrimes();
        ////SaveAdditivePrimes();
        ////SavePurePrimes();
        ////SaveComposites();
        ////SaveAdditiveComposites();
        ////SavePureComposites();

        //LoadPrimes();
        //LoadAdditivePrimes();
        //LoadPurePrimes();
        //LoadComposites();
        //LoadAdditiveComposites();
        //LoadPureComposites();
    }

    private static bool s_is_one_prime = true;
    public static bool IsOnePrime
    {
        get { return s_is_one_prime; }
        set { s_is_one_prime = value; }
    }

    private static string s_pi_filename = "pi.txt";
    private static string s_e_filename = "e.txt";
    private static string s_phi_filename = "phi.txt";
    private static int[] s_pi_digits = null;
    private static int[] s_e_digits = null;
    private static int[] s_phi_digits = null;
    public static int[] PiDigits
    {
        get
        {
            if (s_pi_digits == null)
            {
                GeneratePiDigits();
            }
            return s_pi_digits;
        }
    }
    public static int[] EDigits
    {
        get
        {
            if (s_e_digits == null)
            {
                GenerateEDigits();
            }
            return s_e_digits;
        }
    }
    public static int[] PhiDigits
    {
        get
        {
            if (s_phi_digits == null)
            {
                GeneratePhiDigits();
            }
            return s_phi_digits;
        }
    }
    private static void GeneratePiDigits()
    {
        string filename = s_constants_directory + "/" + s_pi_filename;
        if (File.Exists(filename))
        {
            using (StreamReader reader = new StreamReader(filename))
            {
                //s_pi_digits = new int[s_pi_limit];
                string content = reader.ReadToEnd();
                s_pi_digits = new int[content.Length - 1];
                s_pi_digits[0] = int.Parse(content[0] + "");
                for (int i = 1; i < content.Length - 1; i++)
                {
                    s_pi_digits[i] = int.Parse(content[i + 1] + "");
                }
            }
        }
    }
    private static void GenerateEDigits()
    {
        string filename = s_constants_directory + "/" + s_e_filename;
        if (File.Exists(filename))
        {
            using (StreamReader reader = new StreamReader(filename))
            {
                //s_e_digits = new int[s_e_limit];
                string content = reader.ReadToEnd();
                s_e_digits = new int[content.Length - 1];
                s_e_digits[0] = int.Parse(content[0] + "");
                for (int i = 1; i < content.Length - 1; i++)
                {
                    s_e_digits[i] = int.Parse(content[i + 1] + "");
                }
            }
        }
    }
    private static void GeneratePhiDigits()
    {
        string filename = s_constants_directory + "/" + s_phi_filename;
        if (File.Exists(filename))
        {
            using (StreamReader reader = new StreamReader(filename))
            {
                //s_phi_digits = new int[s_phi_limit];
                string content = reader.ReadToEnd();
                s_phi_digits = new int[content.Length - 1];
                s_phi_digits[0] = int.Parse(content[0] + "");
                for (int i = 1; i < content.Length - 1; i++)
                {
                    s_phi_digits[i] = int.Parse(content[i + 1] + "");
                }
            }
        }
    }

    public static bool IsOdd(long number)
    {
        return IsOdd(number, 2);
    }
    public static bool IsEven(long number)
    {
        return IsEven(number, 2);
    }
    public static bool IsOdd(long number, long prime)
    {
        return ((number % prime) != 0);
    }
    public static bool IsEven(long number, long prime)
    {
        return ((number % prime) == 0);
    }

    private static int s_series_limit = 100;
    //http://en.wikipedia.org/wiki/Polygon_number
    //ith number of Polygon(sides=N) = ( (N - 2)*i*i - (N - 4)*i ) / 2
    //----------------------------------------------------------------------------------------
    //N   Name            Formula            i = 1 2 3 4 5 6 7 8 9 10             OEIS number
    //----------------------------------------------------------------------------------------
    //3   Triangular      ½(1n² + 1n)         1 3 6 10 15 21 28 36 45 55           A000217 
    //4   Square          ½(2n² - 0n)         1 4 9 16 25 36 49 64 81 100          A000290 
    //5   Pentagonal      ½(3n² - 1n)         1 5 12 22 35 51 70 92 117 145        A000326 
    //6   Hexagonal       ½(4n² - 2n)         1 6 15 28 45 66 91 120 153 190       A000384 
    //7   Heptagonal      ½(5n² - 3n)         1 7 18 34 55 81 112 148 189 235      A000566 
    //8   Octagonal       ½(6n² - 4n)         1 8 21 40 65 96 133 176 225 280      A000567 
    //9   Nonagonal       ½(7n² - 5n)         1 9 24 46 75 111 154 204 261 325     A001106 
    //10  Decagonal       ½(8n² - 6n)         1 10 27 52 85 126 175 232 297 370    A001107 
    //11  Hendecagonal    ½(9n² - 7n)         1 11 30 58 95 141 196 260 333 415    A051682 
    //12  Dodecagonal     ½(10n² - 8n)        1 12 33 64 105 156 217 288 369 460   A051624 
    //13  Tridecagonal    ½(11n² - 9n)        1 13 36 70 115 171 238 316 405 505   A051865 
    //14  Tetradecagonal  ½(12n² - 10n)       1 14 39 76 125 186 259 344 441 550   A051866 
    //15  Pentadecagonal  ½(13n² - 11n)       1 15 42 82 135 201 280 372 477 595   A051867 
    //16  Hexadecagonal   ½(14n² - 12n)       1 16 45 88 145 216 301 400 513 640   A051868 
    //17  Heptadecagonal  ½(15n² - 13n)       1 17 48 94 155 231 322 428 549 685   A051869 
    //18  Octadecagonal   ½(16n² - 14n)       1 18 51 100 165 246 343 456 585 730  A051870 
    //19  Nonadecagonal   ½(17n² - 15n)       1 19 54 106 175 261 364 484 621 775  A051871 
    //20  Icosagonal      ½(18n² - 16n)       1 20 57 112 185 276 385 512 657 820  A051872 
    //21  Icosihenagonal  ½(19n² - 17n)       1 21 60 118 195 291 406 540 693 865  A051873 
    //22  Icosidigonal    ½(20n² - 18n)       1 22 63 124 205 306 427 568 729 910  A051874 
    //23  Icositrigonal   ½(21n² - 19n)       1 23 66 130 215 321 448 596 765 955  A051875 
    //24  Icositetragonal ½(22n² - 20n)       1 24 69 136 225 336 469 624 801 1000 A051876 
    //----------------------------------------------------------------------------------------
    private static Dictionary<int, List<long>> s_polygon_numbers_dictionary = new Dictionary<int, List<long>>();
    public static List<long> PolygonNumbers(int sides)
    {
        if (!s_polygon_numbers_dictionary.ContainsKey(sides))
        {
            GeneratePolygonNumbers(sides);
        }

        if (s_polygon_numbers_dictionary.ContainsKey(sides))
        {
            return s_polygon_numbers_dictionary[sides];
        }
        else
        {
            return null;
        }
    }
    private static void GeneratePolygonNumbers(int sides)
    {
        List<long> polygon_numbers = new List<long>(s_series_limit);
        for (int n = 1; n <= s_series_limit; n++)
        {
            long number = ((sides - 2) * n * n - (sides - 4) * n) / 2;
            polygon_numbers.Add(number);
        }
        s_polygon_numbers_dictionary.Add(sides, polygon_numbers);
    }
    public static bool IsPolygonNumber(int sides, long number)
    {
        if (s_polygon_numbers_dictionary.ContainsKey(sides))
        {
            return (s_polygon_numbers_dictionary[sides].Contains(number));
        }
        else
        {
            return false;
        }
    }
    public static List<long> Triangulars
    {
        get
        {
            return PolygonNumbers(3);
        }
    }
    public static List<long> Squares
    {
        get
        {
            return PolygonNumbers(4);
        }
    }
    public static List<long> Pentagonals
    {
        get
        {
            return PolygonNumbers(5);
        }
    }
    public static List<long> Hexagonals
    {
        get
        {
            return PolygonNumbers(6);
        }
    }
    public static List<long> Heptagonals
    {
        get
        {
            return PolygonNumbers(7);
        }
    }
    public static List<long> Octagonals
    {
        get
        {
            return PolygonNumbers(8);
        }
    }
    public static List<long> Nonagonals
    {
        get
        {
            return PolygonNumbers(9);
        }
    }
    public static List<long> Decagonals
    {
        get
        {
            return PolygonNumbers(10);
        }
    }
    public static List<long> Hendecagonals
    {
        get
        {
            return PolygonNumbers(11);
        }
    }
    public static List<long> Dodecagonals
    {
        get
        {
            return PolygonNumbers(12);
        }
    }
    public static List<long> Tridecagonals
    {
        get
        {
            return PolygonNumbers(13);
        }
    }
    public static List<long> Tetradecagonals
    {
        get
        {
            return PolygonNumbers(14);
        }
    }
    public static List<long> Pentadecagonals
    {
        get
        {
            return PolygonNumbers(15);
        }
    }
    public static List<long> Hexadecagonals
    {
        get
        {
            return PolygonNumbers(16);
        }
    }
    public static List<long> Heptadecagonals
    {
        get
        {
            return PolygonNumbers(17);
        }
    }
    public static List<long> Octadecagonals
    {
        get
        {
            return PolygonNumbers(18);
        }
    }
    public static List<long> Nonadecagonals
    {
        get
        {
            return PolygonNumbers(19);
        }
    }
    public static List<long> Icosagonals
    {
        get
        {
            return PolygonNumbers(20);
        }
    }
    public static List<long> Icosihenagonals
    {
        get
        {
            return PolygonNumbers(21);
        }
    }
    public static List<long> Icosidigonals
    {
        get
        {
            return PolygonNumbers(22);
        }
    }
    public static List<long> Icositrigonals
    {
        get
        {
            return PolygonNumbers(23);
        }
    }
    public static List<long> Icositetragonals
    {
        get
        {
            return PolygonNumbers(24);
        }
    }
    public static bool IsTriangular(long number)
    {
        return (PolygonNumbers(3).Contains(number));
    }
    public static bool IsSquare(long number)
    {
        return (PolygonNumbers(4).Contains(number));
    }
    public static bool IsPentagonal(long number)
    {
        return (PolygonNumbers(5).Contains(number));
    }
    public static bool IsHexagonal(long number)
    {
        return (PolygonNumbers(6).Contains(number));
    }
    public static bool IsHeptagonal(long number)
    {
        return (PolygonNumbers(7).Contains(number));
    }
    public static bool IsOctagonal(long number)
    {
        return (PolygonNumbers(8).Contains(number));
    }
    public static bool IsNonagonal(long number)
    {
        return (PolygonNumbers(9).Contains(number));
    }
    public static bool IsDecagonal(long number)
    {
        return (PolygonNumbers(10).Contains(number));
    }
    public static bool IsHendecagonal(long number)
    {
        return (PolygonNumbers(11).Contains(number));
    }
    public static bool IsDodecagonal(long number)
    {
        return (PolygonNumbers(12).Contains(number));
    }
    public static bool IsTridecagonal(long number)
    {
        return (PolygonNumbers(13).Contains(number));
    }
    public static bool IsTetradecagonal(long number)
    {
        return (PolygonNumbers(14).Contains(number));
    }
    public static bool IsPentadecagonal(long number)
    {
        return (PolygonNumbers(15).Contains(number));
    }
    public static bool IsHexadecagonal(long number)
    {
        return (PolygonNumbers(16).Contains(number));
    }
    public static bool IsHeptadecagonal(long number)
    {
        return (PolygonNumbers(17).Contains(number));
    }
    public static bool IsOctadecagonal(long number)
    {
        return (PolygonNumbers(18).Contains(number));
    }
    public static bool IsNonadecagonal(long number)
    {
        return (PolygonNumbers(19).Contains(number));
    }
    public static bool IsIcosagonal(long number)
    {
        return (PolygonNumbers(20).Contains(number));
    }
    public static bool IsIcosihenagonal(long number)
    {
        return (PolygonNumbers(21).Contains(number));
    }
    public static bool IsIcosidigonal(long number)
    {
        return (PolygonNumbers(22).Contains(number));
    }
    public static bool IsIcositrigonal(long number)
    {
        return (PolygonNumbers(23).Contains(number));
    }
    public static bool IsIcositetragonal(long number)
    {
        return (PolygonNumbers(24).Contains(number));
    }

    //http://en.wikipedia.org/wiki/CenteredPolygonal_number
    // ith number of CenteredPolygon(sides=N) = (((N * i)/2) * (i-1)) + 1
    // Whereas a prime number p cannot be a polygon number, many centered polygon numbers are primes.
    private static Dictionary<int, List<long>> s_centered_polygon_numbers_dictionary = new Dictionary<int, List<long>>();
    public static List<long> CenteredPolygonNumbers(int sides)
    {
        if (!s_centered_polygon_numbers_dictionary.ContainsKey(sides))
        {
            GenerateCenteredPolygonNumbers(sides);
        }

        if (s_centered_polygon_numbers_dictionary.ContainsKey(sides))
        {
            return s_centered_polygon_numbers_dictionary[sides];
        }
        else
        {
            return null;
        }
    }
    private static void GenerateCenteredPolygonNumbers(int sides)
    {
        List<long> polygon_numbers = new List<long>(s_series_limit);
        for (int n = 1; n <= s_series_limit; n++)
        {
            long number = (int)(((sides * n) / 2.0) * (n - 1)) + 1;
            polygon_numbers.Add(number);
        }
        s_centered_polygon_numbers_dictionary.Add(sides, polygon_numbers);
    }
    public static bool IsCenteredPolygonNumber(int sides, long number)
    {
        if (s_centered_polygon_numbers_dictionary.ContainsKey(sides))
        {
            return (s_centered_polygon_numbers_dictionary[sides].Contains(number));
        }
        else
        {
            return false;
        }
    }
    public static List<long> CenteredTriangulars
    {
        get
        {
            return CenteredPolygonNumbers(3);
        }
    }
    public static List<long> CenteredSquares
    {
        get
        {
            return CenteredPolygonNumbers(4);
        }
    }
    public static List<long> CenteredPentagonals
    {
        get
        {
            return CenteredPolygonNumbers(5);
        }
    }
    public static List<long> CenteredHexagonals
    {
        get
        {
            return CenteredPolygonNumbers(6);
        }
    }
    public static List<long> CenteredHeptagonals
    {
        get
        {
            return CenteredPolygonNumbers(7);
        }
    }
    public static List<long> CenteredOctagonals
    {
        get
        {
            return CenteredPolygonNumbers(8);
        }
    }
    public static List<long> CenteredNonagonals
    {
        get
        {
            return CenteredPolygonNumbers(9);
        }
    }
    public static List<long> CenteredDecagonals
    {
        get
        {
            return CenteredPolygonNumbers(10);
        }
    }
    public static List<long> CenteredHendecagonals
    {
        get
        {
            return CenteredPolygonNumbers(11);
        }
    }
    public static List<long> CenteredDodecagonals
    {
        get
        {
            return CenteredPolygonNumbers(12);
        }
    }
    public static List<long> CenteredTridecagonals
    {
        get
        {
            return CenteredPolygonNumbers(13);
        }
    }
    public static List<long> CenteredTetradecagonals
    {
        get
        {
            return CenteredPolygonNumbers(14);
        }
    }
    public static List<long> CenteredPentadecagonals
    {
        get
        {
            return CenteredPolygonNumbers(15);
        }
    }
    public static List<long> CenteredHexadecagonals
    {
        get
        {
            return CenteredPolygonNumbers(16);
        }
    }
    public static List<long> CenteredHeptadecagonals
    {
        get
        {
            return CenteredPolygonNumbers(17);
        }
    }
    public static List<long> CenteredOctadecagonals
    {
        get
        {
            return CenteredPolygonNumbers(18);
        }
    }
    public static List<long> CenteredNonadecagonals
    {
        get
        {
            return CenteredPolygonNumbers(19);
        }
    }
    public static List<long> CenteredIcosagonals
    {
        get
        {
            return CenteredPolygonNumbers(20);
        }
    }
    public static List<long> CenteredIcosihenagonals
    {
        get
        {
            return CenteredPolygonNumbers(21);
        }
    }
    public static List<long> CenteredIcosidigonals
    {
        get
        {
            return CenteredPolygonNumbers(22);
        }
    }
    public static List<long> CenteredIcositrigonals
    {
        get
        {
            return CenteredPolygonNumbers(23);
        }
    }
    public static List<long> CenteredIcositetragonals
    {
        get
        {
            return CenteredPolygonNumbers(24);
        }
    }
    public static bool IsCenteredTriangular(long number)
    {
        return (CenteredPolygonNumbers(3).Contains(number));
    }
    public static bool IsCenteredSquare(long number)
    {
        return (CenteredPolygonNumbers(4).Contains(number));
    }
    public static bool IsCenteredPentagonal(long number)
    {
        return (CenteredPolygonNumbers(5).Contains(number));
    }
    public static bool IsCenteredHexagonal(long number)
    {
        return (CenteredPolygonNumbers(6).Contains(number));
    }
    public static bool IsCenteredHeptagonal(long number)
    {
        return (CenteredPolygonNumbers(7).Contains(number));
    }
    public static bool IsCenteredOctagonal(long number)
    {
        return (CenteredPolygonNumbers(8).Contains(number));
    }
    public static bool IsCenteredNonagonal(long number)
    {
        return (CenteredPolygonNumbers(9).Contains(number));
    }
    public static bool IsCenteredDecagonal(long number)
    {
        return (CenteredPolygonNumbers(10).Contains(number));
    }
    public static bool IsCenteredHendecagonal(long number)
    {
        return (CenteredPolygonNumbers(11).Contains(number));
    }
    public static bool IsCenteredDodecagonal(long number)
    {
        return (CenteredPolygonNumbers(12).Contains(number));
    }
    public static bool IsCenteredTridecagonal(long number)
    {
        return (CenteredPolygonNumbers(13).Contains(number));
    }
    public static bool IsCenteredTetradecagonal(long number)
    {
        return (CenteredPolygonNumbers(14).Contains(number));
    }
    public static bool IsCenteredPentadecagonal(long number)
    {
        return (CenteredPolygonNumbers(15).Contains(number));
    }
    public static bool IsCenteredHexadecagonal(long number)
    {
        return (CenteredPolygonNumbers(16).Contains(number));
    }
    public static bool IsCenteredHeptadecagonal(long number)
    {
        return (CenteredPolygonNumbers(17).Contains(number));
    }
    public static bool IsCenteredOctadecagonal(long number)
    {
        return (CenteredPolygonNumbers(18).Contains(number));
    }
    public static bool IsCenteredNonadecagonal(long number)
    {
        return (CenteredPolygonNumbers(19).Contains(number));
    }
    public static bool IsCenteredIcosagonal(long number)
    {
        return (CenteredPolygonNumbers(20).Contains(number));
    }
    public static bool IsCenteredIcosihenagonal(long number)
    {
        return (CenteredPolygonNumbers(21).Contains(number));
    }
    public static bool IsCenteredIcosidigonal(long number)
    {
        return (CenteredPolygonNumbers(22).Contains(number));
    }
    public static bool IsCenteredIcositrigonal(long number)
    {
        return (CenteredPolygonNumbers(23).Contains(number));
    }
    public static bool IsCenteredIcositetragonal(long number)
    {
        return (CenteredPolygonNumbers(24).Contains(number));
    }

    // https://oeis.org  // Chemical polyhex hydrocarbons with 19 hexagons
    // Number of Isomers of polyhex hydrocarbons with C_(2h) symmetry with nineteen hexagons
    // 3, 17, 66, 189, 589, 1677, 3829, 7948, 15649, 25543, 26931, 15472 
    // Number of isomers of polyhex hydrocarbons with C_(2v) symmetry with nineteen hexagons
    // 3, 17, 14, 92, 60, 316, 175, 814, 495, 2323, 1402, 6037, 3113, 12851, 6200, 24710, 11851, 46152, 18123, 72151, 18007, 74547, 8970, 40141
    private static Dictionary<int, List<long>> s_polyhex_numbers_dictionary = new Dictionary<int, List<long>>();
    public static List<long> PolyhexNumbers(int hexagons)
    {
        if (!s_polyhex_numbers_dictionary.ContainsKey(hexagons))
        {
            GeneratePolyhexNumbers(hexagons);
        }

        if (s_polyhex_numbers_dictionary.ContainsKey(hexagons))
        {
            return s_polyhex_numbers_dictionary[hexagons];
        }
        else
        {
            return null;
        }
    }
    private static void GeneratePolyhexNumbers(int hexagons)
    {
        //List<long> polyhex_numbers = new List<long>(s_series_limit);
        //for (int n = 1; n <= s_series_limit; n++)
        //{
        //    long number = ((hexagons - 2) * n * n - (hexagons - 4) * n) / 2;
        //    polyhex_numbers.Add(number);
        //}
        //s_polyhex_numbers_dictionary.Add(hexagons, polyhex_numbers);

        List<long> polyhex_numbers = null;
        if (hexagons == 1)
        {
            polyhex_numbers = new List<long>() { 3, 17, 66, 189, 589, 1677, 3829, 7948, 15649, 25543, 26931, 15472 };
        }
        else if (hexagons == 2)
        {
            polyhex_numbers = new List<long>() { 3, 17, 14, 92, 60, 316, 175, 814, 495, 2323, 1402, 6037, 3113, 12851, 6200, 24710, 11851, 46152, 18123, 72151, 18007, 74547, 8970, 40141 };
        }
        else
        {
            //
        }
        s_polyhex_numbers_dictionary.Add(hexagons, polyhex_numbers);
    }
    public static bool IsPolyhexNumber(int hexagons, long number)
    {
        if (s_polyhex_numbers_dictionary.ContainsKey(hexagons))
        {
            return (s_polyhex_numbers_dictionary[hexagons].Contains(number));
        }
        else
        {
            return false;
        }
    }
    public static List<long> C2hPolyhexHydrocarbons
    {
        get
        {
            return PolyhexNumbers(1);
        }
    }
    public static List<long> C2vPolyhexHydrocarbons
    {
        get
        {
            return PolyhexNumbers(2);
        }
    }
    public static bool IsC2hPolyhexHydrocarbons(long number)
    {
        return (PolyhexNumbers(1).Contains(number));
    }
    public static bool IsC2vPolyhexHydrocarbons(long number)
    {
        return (PolyhexNumbers(2).Contains(number));
    }

    //Tetrahedral,            // n*(n+1)*(n+2)/6   = 1, 4, 10, 20, 35, 56, 84, 120, 165, 220, 286, 364, 455, 560, 680, 816, 969, 1140, 1330, 1540, 1771, 2024, 2300, 2600, 2925, 3276, 3654, 4060, 4495, 4960, 5456, 5984, 6545, 7140, 7770, 8436, 9139, 9880, 10660, 11480, 12341, 13244, 14190, 15180, ...
    private static int s_tetrahedrals_limit = s_series_limit;
    private static List<long> s_tetrahedrals = null;
    public static List<long> Tetrahedrals
    {
        get
        {
            if (s_tetrahedrals == null)
            {
                GenerateTetrahedrals();
            }
            return s_tetrahedrals;
        }
    }
    private static void GenerateTetrahedrals()
    {
        int max = s_tetrahedrals_limit;
        s_tetrahedrals = new List<long>(max);
        for (int number = 1; number <= max; number++)
        {
            long result = (number * (number + 1) * (number + 2)) / 6;
            s_tetrahedrals.Add(result);
        }
    }
    public static bool IsTetrahedral(long number)
    {
        return (Tetrahedrals.Contains(number));
    }

    //Pyramidal,              // n*(n+1)*(2*n+1)/6 = 1, 5, 14, 30, 55, 91, 140, 204, 285, 385, 506, 650, 819, 1015, 1240, 1496, 1785, 2109, 2470, 2870, 3311, 3795, 4324, 4900, 5525, 6201, 6930, 7714, 8555, 9455, 10416, 11440, 12529, 13685, 14910, 16206, 17575, 19019, 20540, 22140, 23821, 25585, 27434, 29370, ...
    private static int s_pyramidals_limit = s_series_limit;
    private static List<long> s_pyramidals = null;
    public static List<long> Pyramidals
    {
        get
        {
            if (s_pyramidals == null)
            {
                GeneratePyramidals();
            }
            return s_pyramidals;
        }
    }
    private static void GeneratePyramidals()
    {
        int max = s_pyramidals_limit;
        s_pyramidals = new List<long>(max);
        for (int number = 1; number <= max; number++)
        {
            long result = (number * (number + 1) * ((2 * number) + 1)) / 6;
            s_pyramidals.Add(result);
        }
    }
    public static bool IsPyramidal(long number)
    {
        return (Pyramidals.Contains(number));
    }

    //Cubic,                  // n^3               = 1, 8, 27, 64, 125, ...
    private static int s_cubics_limit = s_series_limit;
    private static List<long> s_cubics = null;
    public static List<long> Cubics
    {
        get
        {
            if (s_cubics == null)
            {
                GenerateCubics();
            }
            return s_cubics;
        }
    }
    private static void GenerateCubics()
    {
        int max = s_cubics_limit;
        s_cubics = new List<long>(max);
        for (int number = 1; number <= max; number++)
        {
            s_cubics.Add(number * number * number);
        }
    }
    public static bool IsCubic(long number)
    {
        return (Cubics.Contains(number));
    }

    // Pivot numbers for books with pivot/support chapters (eg Quran has N=114, O=N/2=57, O=P1*P2=3*19 has P3=Sum(1..57)=29 verses)
    private static int s_pivots_limit = s_series_limit;
    private static Dictionary<string, List<long>> s_pivots_dictionary = null;
    public static List<long> Pivots(string name)
    {
        if (s_pivots_dictionary == null)
        {
            GeneratePivots();
        }

        if (s_pivots_dictionary.ContainsKey(name))
        {
            return s_pivots_dictionary[name];
        }
        else
        {
            return null;
        }
    }
    private static void GeneratePivots()
    {
        int max = s_pivots_limit;
        s_pivots_dictionary = new Dictionary<string, List<long>>(6);

        // N is the number of chapters in a candidate book (eg 114)
        List<long> pivot_N_numbers = new List<long>(max);
        List<long> pivot_O_numbers = new List<long>(max);
        List<long> pivot_P1_numbers = new List<long>(max);
        List<long> pivot_P2_numbers = new List<long>(max);
        List<long> pivot_P3_numbers = new List<long>(max);
        List<long> pivot_S_numbers = new List<long>(max);

        for (int i = 1; i <= 1000000; i++)
        {
            int N = 2 + 4 * i;  // 2, 6, 10, 14, 18, 22, ... 114, ...

            // O = pivot chapter = last chapter in the top half (eg 57)
            int O = N / 2;
            if (IsOdd(O))
            {
                List<long> factors = Factorize(O);
                if (factors.Count == 2)         // O is pseudo-prime
                {
                    int P1 = (int)factors[0];   // (eg 3)
                    int P2 = (int)factors[1];   // (eg 19)
                    int P3 = (O / 2) + 1;       // (eg 29)
                    if (IsPrime(P3))
                    {
                        int S = P1 * P3;        // supporting chapter in bottom half (eg 87)
                        if ((S > O) && (S <= N))
                        {
                            pivot_N_numbers.Add(N);
                            pivot_O_numbers.Add(O);
                            pivot_P1_numbers.Add(P1);
                            pivot_P2_numbers.Add(P2);
                            pivot_P3_numbers.Add(P3);
                            pivot_S_numbers.Add(S);

                            if (pivot_N_numbers.Count == max)
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }
        s_pivots_dictionary.Add("PivotBooks", pivot_N_numbers);
        s_pivots_dictionary.Add("PivotChapters", pivot_O_numbers);
        s_pivots_dictionary.Add("PivotChapterFirstFactors", pivot_P1_numbers);
        s_pivots_dictionary.Add("PivotChapterSecondFactors", pivot_P2_numbers);
        s_pivots_dictionary.Add("PivotChapterVerses", pivot_P3_numbers);
        s_pivots_dictionary.Add("PivotSupportChapters", pivot_S_numbers);
    }
    public static bool IsPivot(string name, long number)
    {
        if (s_pivots_dictionary.ContainsKey(name))
        {
            return (s_pivots_dictionary[name].Contains(number));
        }
        else
        {
            return false;
        }
    }

    public static bool IsNumberType(long number, NumberType number_type)
    {
        switch (number_type)
        {
            case NumberType.Any:
                {
                    return true;
                }
            case NumberType.Odd:
                {
                    return (Numbers.IsOdd(number));
                }
            case NumberType.Even:
                {
                    return (Numbers.IsEven(number));
                }
            case NumberType.PurePrime:
                {
                    return (Numbers.IsPurePrime(number));
                }
            case NumberType.AdditivePrime:
                {
                    return (Numbers.IsAdditivePrime(number));
                }
            case NumberType.Prime:
                {
                    return (Numbers.IsPrime(number));
                }
            case NumberType.PureComposite:
                {
                    return (Numbers.IsPureComposite(number));
                }
            case NumberType.AdditiveComposite:
                {
                    return (Numbers.IsAdditiveComposite(number));
                }
            case NumberType.Composite:
                {
                    return (Numbers.IsComposite(number));
                }
            case NumberType.MersennePrime:
                {
                    return (Numbers.IsMersennePrime(number));
                }
            case NumberType.Fibonacci:
                {
                    return (Numbers.IsFibonacci(number));
                }
            case NumberType.Tetrahedral:
                {
                    return (Numbers.IsTetrahedral(number));
                }
            case NumberType.Pyramidal:
                {
                    return (Numbers.IsPyramidal(number));
                }
            case NumberType.Cubic:
                {
                    return (Numbers.IsCubic(number));
                }
            default:
                {
                    return false;
                }
        }
    }
    public static bool Compare(long number1, long number2, ComparisonOperator comparison_operator)
    {
        switch (comparison_operator)
        {
            case ComparisonOperator.Equal:
                {
                    return (number1 == number2);
                }
            case ComparisonOperator.NotEqual:
                {
                    return (number1 != number2);
                }
            case ComparisonOperator.LessThan:
                {
                    return (number1 < number2);
                }
            case ComparisonOperator.LessThanOrEqual:
                {
                    return (number1 <= number2);
                }
            case ComparisonOperator.GreaterThan:
                {
                    return (number1 > number2);
                }
            case ComparisonOperator.GreaterThanOrEqual:
                {
                    return (number1 >= number2);
                }
            case ComparisonOperator.Unknown:
            default:
                {
                    return false;
                }
        }
    }

    private static long PrimeCount(long limit)
    {
        long result = 0;
        for (long i = 0; i <= limit; i++)
        {
            if (IsPrime(i))
            {
                result++;
            }
        }
        return result;
    }
    private static long AdditivePrimeCount(long limit)
    {
        int result = 0;
        for (long i = 0; i <= limit; i++)
        {
            if (IsAdditivePrime(i))
            {
                result++;
            }
        }
        return result;
    }
    private static long PurePrimeCount(long limit)
    {
        int result = 0;
        for (long i = 0; i <= limit; i++)
        {
            if (IsPurePrime(i))
            {
                result++;
            }
        }
        return result;
    }

    public static bool IsPrime(long number)
    {
        if (number <= 0)        // primes are positive
            return false;

        if (number == 1)
            return s_is_one_prime;

        if (number == 2)        // 2 is the only even prime
            return true;

        if (number % 2 == 0)    // exclude even numbers
            return false;

        long sqrt = (long)Math.Sqrt(number);
        for (long i = 3; i <= sqrt; i += 2)
        {
            if ((number % i) == 0)
            {
                return false;
            }
        }
        return true;
    }
    public static bool IsPrime(string value, long radix)
    {
        long number = Radix.Decode(value, radix);
        return IsPrime(number);
    }
    public static bool IsAdditivePrime(long number)
    {
        if (IsPrime(number))
        {
            return IsPrime(DigitSum(number));
        }
        return false;
    }
    public static bool IsAdditivePrime(string value, long radix)
    {
        if (IsPrime(value, radix))
        {
            return IsPrime(DigitSum(value));
        }
        return false;
    }
    public static bool IsPurePrime(long number)
    {
        if (IsAdditivePrime(number))
        {
            return IsPrimeDigits(number);
        }
        return false;
    }
    public static bool IsPurePrime(string value, long radix)
    {
        if (IsAdditivePrime(value, radix))
        {
            return IsPrimeDigits(value);
        }
        return false;
    }
    public static bool IsComposite(long number)
    {
        if (number <= 0)        // primes are positive
            return true;

        if (number == 1)
            return !s_is_one_prime;

        if (number == 2)        // 2 is the only even prime
            return false;

        if (number % 2 == 0)    // exclude even numbers
            return true;

        long sqrt = (long)Math.Sqrt(number);
        for (long i = 3; i <= sqrt; i += 2)
        {
            if ((number % i) == 0)
            {
                return true;
            }
        }
        return false;
    }
    public static bool IsComposite(string value, long radix)
    {
        long number = Radix.Decode(value, radix);
        return IsComposite(number);
    }
    public static bool IsAdditiveComposite(long number)
    {
        if (IsComposite(number))
        {
            return IsComposite(DigitSum(number));
        }
        return false;
    }
    public static bool IsAdditiveComposite(string value, long radix)
    {
        if (IsComposite(value, radix))
        {
            return IsComposite(DigitSum(value));
        }
        return false;
    }
    public static bool IsPureComposite(long number)
    {
        if (IsAdditiveComposite(number))
        {
            return IsCompositeDigits(number);
        }
        return false;
    }
    public static bool IsPureComposite(string value, long radix)
    {
        if (IsAdditiveComposite(value, radix))
        {
            return IsCompositeDigits(value);
        }
        return false;
    }

    public static int GetDigitValue(char c)
    {
        int result = -1;
        if (Char.IsDigit(c)) // 0..9
        {
            result = (int)char.GetNumericValue(c);
        }
        else // A..Z
        {
            result = c.CompareTo('A') + 10;
        }
        return result;
    }
    public static List<int> GetDigits(long number)
    {
        List<int> result = new List<int>();
        string str = number.ToString();
        for (int i = 0; i < str.Length; i++)
        {
            result.Add((int)Char.GetNumericValue(str[i]));
        }
        return result;
    }
    public static List<char> GetDigits(string value)
    {
        List<char> result = new List<char>();
        for (int i = 0; i < value.Length; i++)
        {
            result.Add(value[i]);
        }
        return result;
    }
    public static int DigitCount(long number)
    {
        return DigitCount(number.ToString());
    }
    public static int DigitCount(string value)
    {
        return ((value.StartsWith("-")) ? value.Length - 1 : value.Length);
    }
    public static int DigitSum(long number)
    {
        return DigitSum(number.ToString());
    }
    public static int DigitSum(string value)
    {
        int result = 0;
        try
        {
            if (value.Length > 0)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    char c = value[i];
                    if (c == '-') continue;
                    result += GetDigitValue(c);
                }
            }
        }
        catch
        {
            return 0;
        }
        return result;
    }
    public static int DigitalRoot(long number)
    {
        return DigitalRoot(number.ToString());
    }
    public static int DigitalRoot(string value)
    {
        int result = 0;
        do
        {
            result = DigitSum(result);
        } while (result.ToString().Length > 1);
        return result;
    }
    public static bool IsPrimeDigits(long number)
    {
        return IsPrimeDigits(number.ToString());
    }
    public static bool IsPrimeDigits(string value)
    {
        if (value.Length > 0)
        {
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (c == '-') continue;
                long digit = GetDigitValue(c);
                if (!IsPrime(digit))
                {
                    return false;
                }
            }
            return true;
        }
        return false;
    }
    public static bool IsCompositeDigits(long number)
    {
        return IsCompositeDigits(number.ToString());
    }
    public static bool IsCompositeDigits(string value)
    {
        if (value.Length > 0)
        {
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (c == '-') continue;
                long digit = GetDigitValue(c);
                if (!IsComposite(digit))
                {
                    return false;
                }
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// Check if three numbers are additive primes and their L2R and R2L concatinations are additive primes too.
    /// <para>Example:</para>
    /// <para>Quran chapter The Key has:</para>
    /// <para>(7, 29, 139) are primes with primes digit sums (7=7, 2+9=11, 1+3+9=13)</para>
    /// <para>and 729139, 139297 primes with prime digit sum (1+3+9+2+9+7=31)</para>
    /// </summary>
    /// <param name="n1"></param>
    /// <param name="n2"></param>
    /// <param name="n3"></param>
    /// <returns></returns>
    public static bool ArePrimeTriplets(long n1, long n2, long n3)
    {
        if (
            Numbers.IsAdditivePrime(n1)
            &&
            Numbers.IsAdditivePrime(n2)
            &&
            Numbers.IsAdditivePrime(n3)
            )
        {
            try
            {
                long l2r = long.Parse(n1.ToString() + n2.ToString() + n3.ToString());
                long r2l = long.Parse(n3.ToString() + n2.ToString() + n1.ToString());
                if (
                    Numbers.IsAdditivePrime(l2r)
                    &&
                    Numbers.IsAdditivePrime(r2l)
                    )
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        return false;
    }

    private static List<long> s_primes = null;
    private static List<long> s_additive_primes = null;
    private static List<long> s_pure_primes = null;
    private static string s_primes_filename = "primes.txt";
    private static string s_additive_primes_filename = "additiveprimes.txt";
    private static string s_pure_primes_filename = "pureprimes.txt";
    public static List<int> SieveOfEratosthenes(int limit)
    {
        // guard against parameter out of range
        if ((limit < 2) || (limit > (int)(int.MaxValue * 0.9999999)))
        {
            return new List<int>();
        }

        // Legendre's constant to approximate the number of primes below N
        int max_primes = (int)Math.Ceiling((limit / (Math.Log(limit) - 1.08366)));
        if (max_primes < 1)
        {
            max_primes = 1;
        }
        List<int> primes = new List<int>(max_primes);

        // bit array to cross out multiples of primes successively
        // from N^2, jumping 2N at a time (to skip even multiples)
        BitArray candidates = new BitArray(limit + 1, true);

        // add number 2 as prime
        primes.Add(2);
        //// no need to cross out evens as we are skipping them anyway
        //// and cross out all its multiples
        //for (int j = 2 * 2; j <= limit; j += 2)
        //{
        //    candidates[j] = false;
        //}

        // get the ceiling of sqrt of N
        int sqrt_of_limit = (int)Math.Ceiling(Math.Sqrt(limit));

        // start from 3 and skip even numbers
        // don't go beyond limit or overflow into negative
        for (int i = 3; (i > 0 && i <= limit); i += 2)
        {
            // if not-crossed out candidate yet
            if (candidates[i])
            {
                // add candidate
                primes.Add(i);

                // upto the sqrt of N
                if (i <= sqrt_of_limit)
                {
                    // and cross out non-even multiples from i*i and skip even i multiples
                    // don't go beyond limit, or overflow into negative
                    for (int j = i * i; (j > 0 && j <= limit); j += 2 * i)
                    {
                        candidates[j] = false;
                    }
                }
            }
        }
        return primes;
    }
    public static List<long> Primes
    {
        get
        {
            if (s_primes == null)
            {
                GeneratePrimes();
            }
            return s_primes;
        }
    }
    public static List<long> AdditivePrimes
    {
        get
        {
            if (s_additive_primes == null)
            {
                GenerateAdditivePrimes();
            }
            return s_additive_primes;
        }
    }
    public static List<long> PurePrimes
    {
        get
        {
            if (s_pure_primes == null)
            {
                GeneratePurePrimes();
            }
            return s_pure_primes;
        }
    }
    public static int IndexOfPrime(long number)
    {
        if (s_primes == null)
        {
            GeneratePrimes();
        }
        return BinarySearch(s_primes, number);
    }
    public static int IndexOfAdditivePrime(long number)
    {
        if (s_additive_primes == null)
        {
            GenerateAdditivePrimes();
        }
        return BinarySearch(s_additive_primes, number);
    }
    public static int IndexOfPurePrime(long number)
    {
        if (s_pure_primes == null)
        {
            GeneratePurePrimes();
        }
        return BinarySearch(s_pure_primes, number);
    }
    private static void GeneratePrimes()
    {
        if (s_primes == null)
        {
            int max = MAX_COUNT;
            BitArray composites = new BitArray(max + 1);

            s_primes = new List<long>(max);

            if (Numbers.IsOnePrime)
            {
                s_primes.Add(1);
            }
            s_primes.Add(2);

            // process odd numbers // 3, 5, 7, 9, 11, ...
            long sqrt = (long)Math.Sqrt(max) + 1;
            for (int i = 3; i <= max; i += 2)
            {
                if (!composites[i])
                {
                    s_primes.Add(i);

                    // mark off multiples of i starting from i*i and skipping even "i"s
                    if (i < sqrt)
                    {
                        for (int j = i * i; j <= max; j += 2 * i)
                        {
                            composites[j] = true;
                        }
                    }
                }
            }
        }
    }
    private static void GenerateAdditivePrimes()
    {
        if (s_additive_primes == null)
        {
            if (s_primes == null)
            {
                GeneratePrimes();
            }

            if (s_primes != null)
            {
                int max = s_primes.Count;
                s_additive_primes = new List<long>(max / 2);
                for (int i = 0; i < max; i++)
                {
                    if (IsPrime(DigitSum(s_primes[i])))
                    {
                        s_additive_primes.Add(s_primes[i]);
                    }
                }
            }
        }
    }
    private static void GeneratePurePrimes()
    {
        if (s_pure_primes == null)
        {
            if (s_additive_primes == null)
            {
                GenerateAdditivePrimes();
            }

            if (s_additive_primes != null)
            {
                int max = s_additive_primes.Count;
                s_pure_primes = new List<long>(max / 2);
                for (int i = 0; i < max; i++)
                {
                    if (IsPrimeDigits(s_additive_primes[i]))
                    {
                        s_pure_primes.Add(s_additive_primes[i]);
                    }
                }
            }
        }
    }
    private static void SavePrimes()
    {
        if (s_primes != null)
        {
            string filename = s_constants_directory + "/" + s_primes_filename;
            using (StreamWriter writer = new StreamWriter(filename))
            {
                foreach (long number in s_primes)
                {
                    writer.WriteLine(number);
                }
            }
        }
    }
    private static void SaveAdditivePrimes()
    {
        if (s_additive_primes != null)
        {
            string filename = s_constants_directory + "/" + s_additive_primes_filename;
            using (StreamWriter writer = new StreamWriter(filename))
            {
                foreach (long number in s_additive_primes)
                {
                    writer.WriteLine(number);
                }
            }
        }
    }
    private static void SavePurePrimes()
    {
        if (s_pure_primes != null)
        {
            string filename = s_constants_directory + "/" + s_pure_primes_filename;
            using (StreamWriter writer = new StreamWriter(filename))
            {
                foreach (long number in s_pure_primes)
                {
                    writer.WriteLine(number);
                }
            }
        }
    }
    private static void LoadPrimes()
    {
        s_primes = new List<long>();

        string filename = s_constants_directory + "/" + s_primes_filename;
        if (File.Exists(filename))
        {
            using (StreamReader reader = new StreamReader(filename))
            {
                string line = "";
                while (!reader.EndOfStream)
                {
                    try
                    {
                        line = reader.ReadLine();
                        s_primes.Add(long.Parse(line));
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(line + " is not a long in " + filename + "\r\n" + ex.Message);
                    }
                }
            }
        }
    }
    private static void LoadAdditivePrimes()
    {
        s_additive_primes = new List<long>();

        string filename = s_constants_directory + "/" + s_additive_primes_filename;
        if (File.Exists(filename))
        {
            using (StreamReader reader = new StreamReader(filename))
            {
                string line = "";
                while (!reader.EndOfStream)
                {
                    try
                    {
                        line = reader.ReadLine();
                        s_additive_primes.Add(long.Parse(line));
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(line + " is not a long in " + filename + "\r\n" + ex.Message);
                    }
                }
            }
        }
    }
    private static void LoadPurePrimes()
    {
        s_pure_primes = new List<long>();

        string filename = s_constants_directory + "/" + s_pure_primes_filename;
        if (File.Exists(filename))
        {
            using (StreamReader reader = new StreamReader(filename))
            {
                string line = "";
                while (!reader.EndOfStream)
                {
                    try
                    {
                        line = reader.ReadLine();
                        s_pure_primes.Add(long.Parse(line));
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(line + " is not a long in " + filename + "\r\n" + ex.Message);
                    }
                }
            }
        }
    }

    private static List<long> s_composites = null;
    private static List<long> s_additive_composites = null;
    private static List<long> s_pure_composites = null;
    private static string s_composites_filename = "composites.txt";
    private static string s_additive_composites_filename = "additivecomposites.txt";
    private static string s_pure_composites_filename = "purecomposites.txt";
    public static List<long> Composites
    {
        get
        {
            if (s_composites == null)
            {
                GenerateComposites();
            }
            return s_composites;
        }
    }
    public static List<long> AdditiveComposites
    {
        get
        {
            if (s_additive_composites == null)
            {
                GenerateAdditiveComposites();
            }
            return s_additive_composites;
        }
    }
    public static List<long> PureComposites
    {
        get
        {
            if (s_pure_composites == null)
            {
                GeneratePureComposites();
            }
            return s_pure_composites;
        }
    }
    public static int IndexOfComposite(long number)
    {
        if (s_composites == null)
        {
            GenerateComposites();
        }
        return BinarySearch(s_composites, number);
    }
    public static int IndexOfAdditiveComposite(long number)
    {
        if (s_additive_composites == null)
        {
            GenerateAdditiveComposites();
        }
        return BinarySearch(s_additive_composites, number);
    }
    public static int IndexOfPureComposite(long number)
    {
        if (s_pure_composites == null)
        {
            GeneratePureComposites();
        }
        return BinarySearch(s_pure_composites, number);
    }
    private static void GenerateComposites()
    {
        if (s_composites == null)
        {
            int max = MAX_COUNT;
            BitArray composites = new BitArray(max + 1);

            s_composites = new List<long>(max);

            for (int i = 4; i <= MAX_COUNT; i += 2)
            {
                composites[i] = true;
            }

            // process odd numbers // 3, 5, 7, 9, 11, ...
            long sqrt = (long)Math.Sqrt(max) + 1;
            for (int i = 3; i <= max; i += 2)
            {
                if (!composites[i])
                {
                    // mark off multiples of i
                    if (i <= sqrt)
                    {
                        for (int j = i * i; j <= max; j += 2 * i)
                        {
                            composites[j] = true;
                        }
                    }
                }
            }

            for (int i = 4; i <= max; i++)
            {
                if (composites[i])
                {
                    s_composites.Add(i);
                }
            }
        }
    }
    private static void GenerateAdditiveComposites()
    {
        if (s_additive_composites == null)
        {
            if (s_composites == null)
            {
                GenerateComposites();
            }

            if (s_composites != null)
            {
                int max = s_composites.Count;
                s_additive_composites = new List<long>(max / 2);
                for (int i = 0; i < max; i++)
                {
                    if (IsComposite(DigitSum(s_composites[i])))
                    {
                        s_additive_composites.Add(s_composites[i]);
                    }
                }
            }
        }
    }
    private static void GeneratePureComposites()
    {
        if (s_pure_composites == null)
        {
            if (s_additive_composites == null)
            {
                GenerateAdditiveComposites();
            }

            if (s_additive_composites != null)
            {
                int max = s_additive_composites.Count;
                s_pure_composites = new List<long>(max / 2);
                for (int i = 0; i < max; i++)
                {
                    if (IsCompositeDigits(s_additive_composites[i]))
                    {
                        s_pure_composites.Add(s_additive_composites[i]);
                    }
                }
            }
        }
    }
    private static void SaveComposites()
    {
        if (s_composites != null)
        {
            string filename = s_constants_directory + "/" + s_composites_filename;
            using (StreamWriter writer = new StreamWriter(filename))
            {
                foreach (long number in s_composites)
                {
                    writer.WriteLine(number);
                }
            }
        }
    }
    private static void SaveAdditiveComposites()
    {
        if (s_additive_composites != null)
        {
            string filename = s_constants_directory + "/" + s_additive_composites_filename;
            using (StreamWriter writer = new StreamWriter(filename))
            {
                foreach (long number in s_additive_composites)
                {
                    writer.WriteLine(number);
                }
            }
        }
    }
    private static void SavePureComposites()
    {
        if (s_pure_composites != null)
        {
            string filename = s_constants_directory + "/" + s_pure_composites_filename;
            using (StreamWriter writer = new StreamWriter(filename))
            {
                foreach (long number in s_pure_composites)
                {
                    writer.WriteLine(number);
                }
            }
        }
    }
    private static void LoadComposites()
    {
        s_composites = new List<long>();

        string filename = s_constants_directory + "/" + s_composites_filename;
        if (File.Exists(filename))
        {
            using (StreamReader reader = new StreamReader(filename))
            {
                string line = "";
                while (!reader.EndOfStream)
                {
                    try
                    {
                        line = reader.ReadLine();
                        s_composites.Add(long.Parse(line));
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(line + " is not a long in " + filename + "\r\n" + ex.Message);
                    }
                }
            }
        }
    }
    private static void LoadAdditiveComposites()
    {
        s_additive_composites = new List<long>();

        string filename = s_constants_directory + "/" + s_additive_composites_filename;
        if (File.Exists(filename))
        {
            using (StreamReader reader = new StreamReader(filename))
            {
                string line = "";
                while (!reader.EndOfStream)
                {
                    try
                    {
                        line = reader.ReadLine();
                        s_additive_composites.Add(long.Parse(line));
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(line + " is not a long in " + filename + "\r\n" + ex.Message);
                    }
                }
            }
        }
    }
    private static void LoadPureComposites()
    {
        s_pure_composites = new List<long>();

        string filename = s_constants_directory + "/" + s_pure_composites_filename;
        if (File.Exists(filename))
        {
            using (StreamReader reader = new StreamReader(filename))
            {
                string line = "";
                while (!reader.EndOfStream)
                {
                    try
                    {
                        line = reader.ReadLine();
                        s_pure_composites.Add(long.Parse(line));
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(line + " is not a long in " + filename + "\r\n" + ex.Message);
                    }
                }
            }
        }
    }

    private static List<long> s_mersenne_primes = null;
    public static List<long> MersennePrimes
    {
        get
        {
            if (s_mersenne_primes == null)
            {
                GenerateMersennePrimes();
            }
            return s_mersenne_primes;
        }
    }
    private static void GenerateMersennePrimes()
    {
        s_mersenne_primes = new List<long>() 
            { 
              1, 2, 3, 5, 7, 13, 17, 19, 31, 61, 89, 107, 127, 521, 607, 1279, 2203, 2281, 3217,
              4253, 4423, 9689, 9941, 11213, 19937, 21701, 23209, 44497, 86243, 110503, 132049,
              216091, 756839, 859433, 1257787, 1398269, 2976221, 3021377, 6972593, 13466917,
              20996011, 24036583, 25964951, 30402457, 32582657, 37156667, 42643801, 43112609, 57885161
            };
    }
    /// <summary>
    /// <para>Mersenne Prime is 2^p - 1 for some prime p exponent power</para>
    /// <para>the first 47 known Mersenne powers in 2012 are:</para>
    /// <para>2, 3, 5, 7, 13, 17, 19, 31, 61, 89, 107, 127, 521, 607, 1279, 2203, 2281, 3217,</para>
    /// <para>4253, 4423, 9689, 9941, 11213, 19937, 21701, 23209, 44497, 86243, 110503, 132049,</para>
    /// <para>216091, 756839, 859433, 1257787, 1398269, 2976221, 3021377, 6972593, 13466917,</para>
    /// <para>20996011, 24036583, 25964951, 30402457, 32582657, 37156667, 42643801, 43112609, 57885161</para>
    /// </summary>
    /// <param name="number"></param>
    /// <returns>bool</returns>
    public static bool IsMersennePrime(long number)
    {
        return MersennePrimes.Contains(number);
    }

    public static List<long> Factorize(long number)
    {
        List<long> result = new List<long>();
        if (number < 0)
        {
            result.Add(-1);
            number *= -1;
        }

        if ((number >= 0) && (number <= 2))
        {
            result.Add(number);
        }
        else // if (number > 2)
        {
            // if number has a prime factor add it to factors,
            // number /= p,
            // reloop until  number == 1
            while (number != 1)
            {
                if ((number % 2) == 0) // if even number
                {
                    result.Add(2);
                    number /= 2;
                }
                else // trial divide by all primes upto sqrt(number)
                {
                    long max = (long)(Math.Sqrt(number)) + 1;	// extra 1 for double calculation errors

                    bool is_factor_found = false;
                    for (long p = 3; p <= max; p += 2)		// should only use primes, but odds will do for now
                    {
                        if ((number % p) == 0)
                        {
                            is_factor_found = true;
                            result.Add(p);
                            number /= p;
                            break; // for loop, reloop while
                        }
                    }

                    // if no prime factor found the number must be prime in the first place
                    if (!is_factor_found)
                    {
                        result.Add(number);
                        break; // while loop
                    }
                }
            }
        }
        return result;
    }
    public static string FactorizeToString(long number)
    {
        StringBuilder str = new StringBuilder();
        List<long> prime_factors = Factorize(number);
        foreach (long prime_factor in prime_factors)
        {
            str.Append(prime_factor.ToString() + " * ");
        }
        if (str.Length > 0)
        {
            str.Remove(str.Length - " * ".Length, " * ".Length);
        }
        return str.ToString();
    }

    private static int s_fibonaccis_limit = 100;        // F99 = 3736710778780434371
    private static List<long> s_fibonaccis = null;
    public static List<long> Fibonaccis
    {
        get
        {
            if (s_fibonaccis == null)
            {
                GenerateFibonaccis();
            }
            return s_fibonaccis;
        }
    }
    public static int IndexOfFibonacci(long number)
    {
        if (s_fibonaccis == null)
        {
            GenerateFibonaccis();
        }
        return BinarySearch(s_fibonaccis, number);
    }
    private static void GenerateFibonaccis()
    {
        int max = s_fibonaccis_limit;
        s_fibonaccis = new List<long>((int)(max));
        s_fibonaccis.Add(1); // 0th item
        s_fibonaccis.Add(1); // 1st item
        for (int i = 2; i < max; i++)
        {
            long number = s_fibonaccis[i - 1] + s_fibonaccis[i - 2];
            s_fibonaccis.Add(number);
        }
    }
    public static bool IsFibonacci(long number)
    {
        return Fibonaccis.Contains(number);
    }

    private static int s_log2s_limit = 63;              // 2^0 .. 2^63 - after that it is -ve
    private static List<long> s_log2s = null;
    public static List<long> Log2s
    {
        get
        {
            if (s_log2s == null)
            {
                GenerateLog2s();
            }
            return s_log2s;
        }
    }
    public static int IndexOfLog2(long number)
    {
        if (s_log2s == null)
        {
            GenerateLog2s();
        }
        return BinarySearch(s_log2s, number);
    }
    private static void GenerateLog2s()
    {
        int max = s_log2s_limit;
        s_log2s = new List<long>((int)(max));
        for (int i = 0; i < max; i++)
        {
            s_log2s.Add((long)Math.Pow(2, i));
        }
    }
    public static bool IsLog2(long number)
    {
        return Log2s.Contains(number);
    }

    private static int s_log10s_limit = 19;             // 10^0 .. 10^18 - after that it is -ve
    private static List<long> s_log10s = null;
    public static List<long> Log10s
    {
        get
        {
            if (s_log10s == null)
            {
                GenerateLog10s();
            }
            return s_log10s;
        }
    }
    public static int IndexOfLog10(long number)
    {
        if (s_log10s == null)
        {
            GenerateLog10s();
        }
        return BinarySearch(s_log10s, number);
    }
    private static void GenerateLog10s()
    {
        int max = s_log10s_limit;
        s_log10s = new List<long>((int)(max));
        for (int i = 0; i < max; i++)
        {
            s_log10s.Add((long)Math.Pow(10, i));
        }
    }
    public static bool IsLog10(long number)
    {
        return Log10s.Contains(number);
    }

    public static int BinarySearch(IList<long> sorted_list, long number)
    {
        if (sorted_list == null) return -1;
        if (sorted_list.Count < 1) return -1;

        int min = 0;
        int max = sorted_list.Count - 1;
        int old_mid = -1;
        int mid;
        while ((mid = (min + max) / 2) != old_mid)
        {
            if (number == sorted_list[min]) { return min; }

            if (number == sorted_list[max]) { return max; }

            if (number == sorted_list[mid]) { return mid; }
            else if (number < sorted_list[mid]) { max = mid; }
            else /*if (number > sorted_list[mid])*/ { min = mid; }

            old_mid = mid;
        }

        return -1;
    }
    public static void QuickSort(IList<long> list, int min, int max)
    {
        if (list == null) return;
        if (list.Count < 1) return;
        if (min > max) return;
        if ((min < 0) || (max >= list.Count)) return;

        int lo = min;
        int hi = max;
        long mid = list[(lo + hi) / 2];	// uses copy constructor

        do
        {
            while (list[lo] < mid)		// uses comparison operator
                lo++;
            while (mid < list[hi])
                hi--;

            if (lo <= hi)
            {
                long temp = list[hi];
                list[hi] = list[lo];
                list[hi] = temp;
                lo++;
                hi--;
            }
        }
        while (lo <= hi);

        if (hi > min)
            QuickSort(list, min, hi);
        if (lo < max)
            QuickSort(list, lo, max);
    }

    private static string s_prime_marker = "+";
    private static string s_additive_prime_marker = "++";
    private static string s_pure_prime_marker = "+++";
    public static string AnnotatePrime(long number)
    {
        return ((Numbers.IsPrime(number)
             ? ((Numbers.IsPurePrime(number) ? s_pure_prime_marker
             : Numbers.IsAdditivePrime(number) ? s_additive_prime_marker
             : s_prime_marker))
             : "")
             + number);
    }
    public static string GetLegend()
    {
        string result = "";
        result += "\r\n" + "----------------------------------------------------------------------------------";
        result += "\r\n" + "{0,3} => Prime number   = Number divisible by itself and 1 only " + s_prime_marker;
        result += "\r\n" + "{0,3} => Additive Prime = Prime number with a prime digit sum " + s_additive_prime_marker;
        result += "\r\n" + "{0,3} => Pure Prime     = Prime number with prime digits and a prime digit sum " + s_pure_prime_marker;
        result += "\r\n" + "----------------------------------------------------------------------------------";
        return result;
    }

    // http://digitalbush.com/2010/02/26/sieve-of-eratosthenes-in-csharp/

    //IList<int> FindPrimes(int max)
    //{
    //    var result = new List<int>((int)(max / (Math.Log(max) - 1.08366)));
    //    var maxSquareRoot = Math.Sqrt(max);
    //    var eliminated = new System.Collections.BitArray(max + 1);
    //    result.Add(2);
    //    for (int i = 3; i <= max; i += 2)
    //    {
    //        if (!eliminated[i])
    //        {
    //            if (i < maxSquareRoot)
    //            {
    //                for (int j = i * i; j <= max; j += 2 * i)
    //                    eliminated[j] = true;
    //            }
    //            result.Add(i);
    //        }
    //    }
    //    return result;
    //}

    // Algorithm Optimizations
    // I cut my work in half by treating the special case of '2'.
    // We know that 2 is prime and all even numbers thereafter are not.
    // So, we'll add two immediately and then start looping at 3 only checking odd numbers from there forward.

    // After we've found a prime, we only need to eliminate numbers from it's square and forward.
    // Let's say we want to find all prime numbers up to 100 and we've just identified 7 as a prime.
    // Per the algorithm, I'll need to eliminate 2*7, 3*7 ,4*7, 5*7, 6*7, 7*7 ,8*7 ,9*7, 10*7 ,11*7, 12*7 ,13*7 and 14*7.
    // None of the even multiples matter (even times an odd is always even) and none of the multiples
    // up to the square of the prime matter since we've already done those multiples in previous loops.
    // So really we only have to eliminate 7*7, 9*7, 11*7 and 13*7.
    // That's a 9 fewer iterations and those savings become more fruitful the deeper you go!

    // The last optimization is the square root calculation and check.
    // We know from above that we only need to start eliminating beginning at the square of the current prime.
    // Therefore it also makes sense that we can stop even trying once we get past the to square root of the max.
    // This saves a bunch more iterations.

    // Language Optimizations
    // Originally I had started by returning an IEnumerable<int>.
    // I wasn't using the list you see above and instead I was using yield return i.
    // I really like that syntax, but once I got to the VB.net version (Coming Soon!),
    // I didn't have a direct translation for the yield keyword.
    // I took the lazy route in the VB version and just stuffed it all into a list and returned that.
    // To my surprise it was faster! I went back and changed the C# version above and it performed better.
    // I'm not sure why, but I'm going with it.

    // What do you think that you get when do a sizeof(bool) in C#?
    // I was surprised to find out that my trusty booleans actually take up a whole byte instead of a single bit.
    // I speculate that there is a performance benefit that all of your types fit into a byte level offset in memory.
    // I was thrilled to find out that we have a BitArray class that is useful for situations above
    // when you need to store a lot of booleans and you need them to only take up a bit in memory.
    // I'm not sure it helped anything, but I feel better knowing I'm using the least amount of memory possible.

    // Conclusion
    // Despite the fact that I know C# really well, I'm very thrilled that I was able to learn a few things about the language.
    // Also, I'm really happy with the performance of this reference implementation.
    // On my machine (2.66 GHz Core2 Duo and 2 GB of RAM) I can find all of the primes under 1,000,000 in 19ms.
    // I think I've squeezed all I can out of this version.
    // Please let me know if you see something I missed or did wrong and I'll make adjustments.

    // EDIT: I just added one more optimization that's worth noting.
    // Instead of constructing my list with an empty constructor, I can save a several milliseconds 
    // off the larger sets by specifying a start size of the internal array structure behind the list.
    // If I set this size at or slightly above the end count of prime numbers,
    // then I avoid a lot of costly array copying as the array bounds keep getting hit.
    // It turns out that there is quite a bit of math involved in accurately predicting the number of primes underneath a given number.
    // I chose to cheat and just use Legendre's constant with the Prime Number Theorem which is close enough for my purposes.
    // I can now calculate all primes under 1,000,000 in 10ms on my machine. Neat!
    //private static List<int> GeneratePrimesUsingSieveOfEratosthenes(int limit)
    //{
    //    // guard against parameter out of range
    //    if (limit < 2)
    //    {
    //        return new List<int>();
    //    }

    //    // Legendre's constant to approximate the number of primes below N
    //    int max_primes = (int)Math.Ceiling((limit / (Math.Log(limit) - 1.08366)));
    //    if (max_primes < 1)
    //    {
    //        max_primes = 1;
    //    }
    //    List<int> primes = new List<int>(max_primes);

    //    // bit array to cross out multiples of primes successively
    //    BitArray candidates = new BitArray(limit + 1, true);

    //    // add number 2 as prime
    //    primes.Add(2);
    //    // and cross out all its multiples
    //    for (int j = 2 * 2; j <= limit; j += 2)
    //    {
    //        candidates[j] = false;
    //    }

    //    // get the ceiling of sqrt of N
    //    int limit_sqrt = (int)Math.Ceiling(Math.Sqrt(limit));

    //    // start from 3 and skip even numbers
    //    // don't go beyond limit or overflow into negative
    //    for (int i = 3; (i > 0 && i <= limit); i += 2)
    //    {
    //        if (candidates[i])
    //        {
    //            // add not-crossed out candidate
    //            primes.Add(i);

    //            // upto the sqrt of N
    //            if (i <= limit_sqrt)
    //            {
    //                // and cross out non-even multiples from i*i and skip even i multiples
    //                // don't go beyond limit, or overflow into negative
    //                for (int j = i * i; (j > 0 && j <= limit); j += 2 * i)
    //                {
    //                    candidates[j] = false;
    //                }
    //            }
    //        }
    //    }

    //    return primes;
    //}
    //private static List<int> GeneratePrimesUsingDivisionTrial(int limit)
    //{
    //    // guard against parameter out of range
    //    if (limit < 2)
    //    {
    //        return new List<int>();
    //    }

    //    // Legendre's constant to approximate the number of primes below N
    //    int max_primes = (int)Math.Ceiling((limit / (Math.Log(limit) - 1.08366)));
    //    if (max_primes < 1)
    //    {
    //        max_primes = 1;
    //    }
    //    List<int> primes = new List<int>(max_primes);

    //    primes.Add(2);

    //    for (int i = 3; i <= limit; i += 2)
    //    {
    //        bool is_prime = true;
    //        for (int j = 3; j <= (int)Math.Sqrt(i); j += 2)
    //        {
    //            if (i % j == 0)
    //            {
    //                is_prime = false;
    //                break;
    //            }
    //        }

    //        if (is_prime)
    //        {
    //            primes.Add(i);
    //        }
    //    }

    //    return primes;
    //}

    // DO NOT USE FOR IMPORTANT COMMUNICATIONS
    // Microsoft is required by law to provide a backdoor to the government
    // in their System.Security.Cryptography to spy on their citizens under for security purposes
    public static void GenerateRSAKeys()
    {
        GenerateRSAKeys(null);
    }
    public static void GenerateRSAKeys(string username)
    {
        try
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd-HH.mm.ss");
            if (!String.IsNullOrEmpty(username))
            {
                username = username.Replace(" ", "");
                username = username.Replace(".", "");
                username = username.Replace("\b", "");
                username = username.Replace("\t", "");
                username = username.Replace("\r", "");
                username = username.Replace("\n", "");
            }
            else
            {
                username = "";
            }

            int key_length = 16 * 1024; // in bits
            RSACryptoServiceProvider provider = new RSACryptoServiceProvider(key_length);

            string private_key_filename = s_constants_directory + "/" + timestamp + "_" + username + ((username.Length == 0) ? "" : "_") + "PrivateKey.xml";
            using (StreamWriter writer = new StreamWriter(private_key_filename, false, Encoding.Unicode))
            {
                writer.WriteLine(provider.ToXmlString(true));
            }

            string public_key_filename = s_constants_directory + "/" + timestamp + "_" + username + ((username.Length == 0) ? "" : "_") + "PublicKey.xml";
            using (StreamWriter writer = new StreamWriter(public_key_filename, false, Encoding.Unicode))
            {
                writer.WriteLine(provider.ToXmlString(false));
            }
        }
        catch
        {
            // silence IO error in case running from read-only media (CD/DVD)
        }
    }
    private static void RSATest()
    {
        var publicPrivateRsa = new RSACryptoServiceProvider
            (
                new CspParameters()
                {
                    KeyContainerName = "PublicPrivateKeys",
                    Flags = CspProviderFlags.UseMachineKeyStore
                    //Flags = CspProviderFlags.UseDefaultKeyContainer 
                }
            )
            {
                PersistKeyInCsp = true,
            };

        var publicRsa = new RSACryptoServiceProvider(
                new CspParameters()
                {
                    KeyContainerName = "PublicKey",
                    Flags = CspProviderFlags.UseMachineKeyStore
                    //Flags = CspProviderFlags.UseDefaultKeyContainer 
                }
            )
            {
                PersistKeyInCsp = true
            };


        //Export the key. 
        publicRsa.ImportParameters(publicPrivateRsa.ExportParameters(false));
        Console.WriteLine(publicRsa.ToXmlString(false));
        Console.WriteLine(publicPrivateRsa.ToXmlString(false));
        //Dispose those two CSPs. 
        using (publicRsa)
        {
            publicRsa.Clear();
        }
        using (publicPrivateRsa)
        {
            publicRsa.Clear();
        }


        //Retrieve keys
        publicPrivateRsa = new RSACryptoServiceProvider(
                new CspParameters()
                {
                    KeyContainerName = "PublicPrivateKeys",
                    Flags = CspProviderFlags.UseMachineKeyStore
                    //Flags = CspProviderFlags.UseDefaultKeyContainer 
                }
            );

        publicRsa = new RSACryptoServiceProvider(
                new CspParameters()
                {
                    KeyContainerName = "PublicKey",
                    Flags = CspProviderFlags.UseMachineKeyStore
                    //Flags = CspProviderFlags.UseDefaultKeyContainer 
                }
            );
        Console.WriteLine(publicRsa.ToXmlString(false));
        Console.WriteLine(publicPrivateRsa.ToXmlString(false));
        using (publicRsa)
        {
            publicRsa.Clear();
        }
        using (publicPrivateRsa)
        {
            publicRsa.Clear();
        }
    }
}
