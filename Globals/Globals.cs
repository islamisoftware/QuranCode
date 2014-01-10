using System;

public enum Edition { Lite, Standard, Grammar, Research }

public static class Globals
{
    public static string VERSION = "4.1.2"; // updated by Version.bat (together with all AssemblyInfo of all assemblies)
    public static Edition EDITION = Edition.Grammar;

    public static string SHORT_VERSION
    {
        get
        {
            if (EDITION == Edition.Lite)
            {
                return ("v" + VERSION + "L");
            }
            else if (EDITION == Edition.Standard)
            {
                return ("v" + VERSION + "");
            }
            else if (EDITION == Edition.Grammar)
            {
                return ("v" + VERSION + "G");
            }
            else if (EDITION == Edition.Research)
            {
                return ("v" + VERSION + "R");
            }
            else
            {
                return ("v" + VERSION + "X"); // Invalid Edition
            }
        }
    }
    
    public static string LONG_VERSION
    {
        get
        {
            return (VERSION + " - " + EDITION.ToString() + " Edition");
        }
    }

    public static string OUTPUT_FILE_EXT = ".csv"; // to open in Excel
    public static string DATETIME_FORMAT = "yyyy-MM-dd HH:mm:ss";
    public static string NUMBER_FORMAT = "000";
    public static int DEFAULT_FREQUENCY = 1433; // Hz frequency for WAVMaker
}
