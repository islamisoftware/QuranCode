using System;
using System.Collections.Generic;
using System.IO;

public static class Touch
{
    public static void TouchFiles(string path, List<string> ignore_folders)
    {
        DoTouchFiles(path, "*.*", DateTime.Now, ignore_folders);
    }
    public static void TouchFiles(string path, DateTime datetime, List<string> ignore_folders)
    {
        DoTouchFiles(path, "*.*", datetime, ignore_folders);
    }
    public static void TouchFiles(string path, string file_pattern, List<string> ignore_folders)
    {
        DoTouchFiles(path, file_pattern, DateTime.Now, ignore_folders);
    }
    public static void TouchFiles(string path, string file_pattern, DateTime datetime, List<string> ignore_folders)
    {
        DoTouchFiles(path, file_pattern, datetime, ignore_folders);
    }
    private static void DoTouchFiles(string path, string file_pattern, DateTime datetime, List<string> ignore_folders)
    {
        if ((path == ".") || (path == ".\\"))
        {
            path = Directory.GetCurrentDirectory();
        }

        DirectoryInfo folder = new DirectoryInfo(path);
        if (folder != null)
        {
            if (ignore_folders != null)
            {
                if (ignore_folders.Contains(folder.Name))
                {
                    return;
                }
            }

            FileInfo[] files = folder.GetFiles(file_pattern);
            if ((files != null) && (files.Length > 0))
            {
                foreach (FileInfo file in files)
                {
                    try
                    {
                        bool is_readonly_folder = false;
                        if ((file.Attributes & FileAttributes.ReadOnly) != 0)
                        {
                            file.Attributes ^= FileAttributes.ReadOnly;
                            is_readonly_folder = true;
                        }
                        file.CreationTime = datetime;
                        file.LastWriteTime = datetime;
                        file.LastAccessTime = datetime;
                        if (is_readonly_folder)
                        {
                            file.Attributes |= FileAttributes.ReadOnly;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("ERROR: " + file.FullName + " has " + ex.Message);
                    }
                }
            }
        }
    }

    public static void TouchTree(string path, List<string> ignore_folders)
    {
        DoTouchTree(path, "*.*", DateTime.Now, ignore_folders);
    }
    public static void TouchTree(string path, DateTime datetime, List<string> ignore_folders)
    {
        DoTouchTree(path, "*.*", datetime, ignore_folders);
    }
    public static void TouchTree(string path, string file_pattern, List<string> ignore_folders)
    {
        DoTouchTree(path, "*.*", DateTime.Now, ignore_folders);
    }
    public static void TouchTree(string path, string file_pattern, DateTime datetime, List<string> ignore_folders)
    {
        DoTouchTree(path, file_pattern, datetime, ignore_folders);
    }
    private static void DoTouchTree(string path, string file_pattern, DateTime datetime, List<string> ignore_folders)
    {
        if ((path == ".") || (path == ".\\"))
        {
            path = Directory.GetCurrentDirectory();
        }

        DirectoryInfo folder = new DirectoryInfo(path);
        if (folder != null)
        {
            if (ignore_folders != null)
            {
                if (ignore_folders.Contains(folder.Name))
                {
                    return;
                }
            }
            
            try
            {
                bool is_readonly_folder = false;
                if ((folder.Attributes & FileAttributes.ReadOnly) != 0)
                {
                    folder.Attributes ^= FileAttributes.ReadOnly;
                    is_readonly_folder = true;
                }
                folder.CreationTime = datetime;
                folder.LastWriteTime = datetime;
                folder.LastAccessTime = datetime;
                if (is_readonly_folder)
                {
                    folder.Attributes |= FileAttributes.ReadOnly;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + folder.FullName + " has " + ex.Message);
            }

            DoTouchFiles(path, file_pattern, datetime, ignore_folders);

            DirectoryInfo[] folders = folder.GetDirectories();
            if ((folders != null) && (folders.Length > 0))
            {
                foreach (DirectoryInfo subfolder in folders)
                {
                    DoTouchTree(subfolder.FullName, file_pattern, datetime, ignore_folders);
                }
            }
        }
    }
}
