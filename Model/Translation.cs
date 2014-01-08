using System;
using System.Collections.Generic;

namespace Model
{
    public class Translation
    {
        public const string DEFAULT_URL_PREFIX = "http://tanzil.net/trans/";
        public const string DEFAULT_FILE_TYPE = "txt";
        public const string DEFAULT_ICON_URL_PREFIX = "http://tanzil.net/pub/img/flag/";
        public static string UrlPrefix = DEFAULT_URL_PREFIX;
        public static string FileType = DEFAULT_FILE_TYPE;
        public static string IconUrlPrefix = DEFAULT_ICON_URL_PREFIX;

        public string Url;
        public string Flag;
        public string Language;
        public string Translator;
        public string Name; // Language - Translator
    }
}

