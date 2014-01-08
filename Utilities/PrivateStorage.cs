using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.IsolatedStorage;

public static class PrivateStorage
{
    private static IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForDomain();

    public static void SaveText(string path, string text)
    {
        IsolatedStorageFileStream stream = new IsolatedStorageFileStream(path, FileMode.Create, store);
        try
        {
            using (StreamWriter writer = new StreamWriter(stream, Encoding.Unicode))
            {
                writer.WriteLine(text);
            }
        }
        catch
        {
            // silence IO error in case running from read-only media (CD/DVD)
        }
    }
    public static void SaveLetters(string path, char[] characters)
    {
        IsolatedStorageFileStream stream = new IsolatedStorageFileStream(path, FileMode.Create, store);
        try
        {
            using (StreamWriter writer = new StreamWriter(stream, Encoding.Unicode))
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
        catch
        {
            // silence IO error in case running from read-only media (CD/DVD)
        }
    }
    public static void SaveWords(string path, List<string> words)
    {
        IsolatedStorageFileStream stream = new IsolatedStorageFileStream(path, FileMode.Create, store);
        try
        {
            using (StreamWriter writer = new StreamWriter(stream, Encoding.Unicode))
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
        catch
        {
            // silence IO error in case running from read-only media (CD/DVD)
        }
    }
    public static void SaveValues(string path, List<long> values)
    {
        IsolatedStorageFileStream stream = new IsolatedStorageFileStream(path, FileMode.Create, store);
        try
        {
            using (StreamWriter writer = new StreamWriter(stream, Encoding.Unicode))
            {
                foreach (long value in values)
                {
                    writer.WriteLine(value.ToString(Globals.NUMBER_FORMAT));
                }
            }
        }
        catch
        {
            // silence IO error in case running from read-only media (CD/DVD)
        }
    }
    public static void DisplayFile(string path)
    {
        IsolatedStorageFileStream stream = new IsolatedStorageFileStream(path, FileMode.Create, store);
        if (stream != null)
        {
            try
            {
                using (StreamReader reader = new StreamReader(stream, Encoding.Unicode))
                {
                    string content = reader.ReadToEnd();
                }
            }
            catch
            {
                // silence IO error in case running from read-only media (CD/DVD)
            }
        }
        // display content here
        //string fullpath = Path.GetFullPath(path);
        //System.Diagnostics.Process.Start("Notepad.exe", fullpath);
    }
}
