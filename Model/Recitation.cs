using System;
using System.Collections.Generic;

namespace Model
{
    public class Recitation
    {
        public const string DEFAULT_URL_PREFIX = "http://www.everyayah.com/data/";
        public const string DEFAULT_FILE_TYPE = "mp3";
        public static string UrlPrefix = DEFAULT_URL_PREFIX;
        public static string FileType = DEFAULT_FILE_TYPE;

        public string Url;
        public string Folder;
        public string Language;
        public string Reciter;
        public int Quality;
        public string Name; // Language - Reciter
    }
}

