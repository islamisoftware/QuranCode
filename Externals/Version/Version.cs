using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: Version path 1.2.3 [-ignore_foler1 ignore_foler2 ...]");
            Console.WriteLine();
            Console.WriteLine("This program will replace all AssemblyVersions except");
            Console.WriteLine("This program will add .* to AssemblyVersion");
            Console.WriteLine("This program will add .0 to AssemblyFileVersion");
            Console.WriteLine("This program will update the VERSION variable in Globals\\Globals.cs");
            return;
        }
        string path = args[0];
        string new_version = args[1];

        if (args.Length > 2)
        {
            List<string> ignore_folders = new List<string>();
            for (int i = 2; i < args.Length; i++)
            {
                if (args[i].StartsWith("-"))
                {
                    args[i] = args[i].Remove(0, 1);
                }
                ignore_folders.Add(args[i]);
            }
            UpdateVersion(path, new_version, ignore_folders);
        }
        else
        {
            UpdateVersion(path, new_version, null);
        }
    }

    public static void UpdateVersion(string path, string new_version, List<string> ignore_folders)
    {
        if ((path == ".") || (path == ".\\"))
        {
            path = Directory.GetCurrentDirectory();
        }

        DirectoryInfo folder = new DirectoryInfo(path);
        if (folder != null)
        {
            DoUpdateGlobals(path, new_version);

            DoUpdateAssemblyInfos(path, new_version, ignore_folders);

            DirectoryInfo[] folders = folder.GetDirectories();
            if ((folders != null) && (folders.Length > 0))
            {
                foreach (DirectoryInfo subfolder in folders)
                {
                    if (ignore_folders != null)
                    {
                        if (ignore_folders.Contains(subfolder.Name))
                        {
                            continue;
                        }
                    }

                    UpdateVersion(subfolder.FullName, new_version, null);
                }
            }
        }
    }
    private static void DoUpdateGlobals(string path, string new_version)
    {
        if ((path == ".") || (path == ".\\"))
        {
            path = Directory.GetCurrentDirectory();
        }

        DirectoryInfo folder = new DirectoryInfo(path);
        if (folder != null)
        {
            string filename = "Globals\\Globals.cs";
            if (File.Exists(filename))
            {
                try
                {
                    string content = LoadFile(filename);
                    content = Regex.Replace(content, "VERSION = \"\\d+(\\.\\d+(\\.\\d+)?)?", "VERSION = \"" + new_version);
                    SaveFile(filename, content);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR: " + filename + " has " + ex.Message);
                }
            }
        }
    }
    private static void DoUpdateAssemblyInfos(string path, string new_version, List<string> ignore_folders)
    {
        if ((path == ".") || (path == ".\\"))
        {
            path = Directory.GetCurrentDirectory();
        }

        DirectoryInfo folder = new DirectoryInfo(path);
        if (folder != null)
        {
            FileInfo[] files = folder.GetFiles("AssemblyInfo.cs");
            if ((files != null) && (files.Length > 0))
            {
                foreach (FileInfo file in files)
                {
                    if (ignore_folders != null)
                    {
                        if (ignore_folders.Contains(file.Directory.Name))
                        {
                            continue;
                        }
                    }

                    try
                    {
                        string content = LoadFile(file.FullName);
                        content = Regex.Replace(content, "AssemblyVersion\\(\"\\d+\\.\\d+(\\.\\d+(\\.\\d+)?)?", "AssemblyVersion(\"" + new_version + ".*");
                        content = Regex.Replace(content, "AssemblyFileVersion\\(\"\\d+\\.\\d+(\\.\\d+(\\.\\d+)?)?", "AssemblyFileVersion(\"" + new_version + ".0");
                        content = Regex.Replace(content, "\\.\\*\\.\\*", ".*");
                        SaveFile(file.FullName, content);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("ERROR: " + file.FullName + " has " + ex.Message);
                    }
                }
            }
        }
    }

    private static string LoadFile(string filename)
    {
        using (StreamReader reader = new StreamReader(filename))
        {
            if (reader != null)
            {
                return reader.ReadToEnd();
            }
        }
        return "";
    }
    private static void SaveFile(string filename, string content)
    {
        using (StreamWriter writer = new StreamWriter(filename))
        {
            if (writer != null)
            {
                writer.Write(content);
            }
        }
    }
}
