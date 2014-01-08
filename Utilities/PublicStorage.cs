using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;

public static class PublicStorage
{
    public static void SaveText(string path, string text)
    {
        using (StreamWriter writer = new StreamWriter(path, false, Encoding.Unicode))
        {
            writer.WriteLine(text);
        }
    }
    public static void SaveLetters(string path, char[] characters)
    {
        using (StreamWriter writer = new StreamWriter(path, false, Encoding.Unicode))
        {
            foreach (char character in characters)
            {
                if (character == '\0')
                {
                    break;
                }
                writer.Write(character);
            }
        }
    }
    public static void SaveWords(string path, List<string> words)
    {
        using (StreamWriter writer = new StreamWriter(path, false, Encoding.Unicode))
        {
            foreach (string word in words)
            {
                if (String.IsNullOrEmpty(word))
                {
                    break;
                }
                writer.Write(word);
            }
        }
    }
    public static void SaveValues(string path, List<long> values)
    {
        using (StreamWriter writer = new StreamWriter(path, false, Encoding.Unicode))
        {
            foreach (long value in values)
            {
                writer.WriteLine(value.ToString(Globals.NUMBER_FORMAT));
            }
        }
    }
    public static void DisplayFile(string path)
    {
        System.Diagnostics.Process.Start("Notepad.exe", path);
    }
}
