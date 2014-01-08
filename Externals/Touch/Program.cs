using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            if (args.Length == 0)
            {
                Touch.TouchTree(".", "*.*", DateTime.Today, null);
            }
            else if ((args.Length == 1) && ((args[0].Contains("/?")) || (args[0].Contains("/help"))))
            {
                Console.WriteLine("Usage: Touch [folder] [files] [yyyy-mm-dd] [HH:MM:SS] [-ignore_foler1 ignore_foler2 ...]");
                return;
            }
            else
            {
                string path = "";
                string file_pattern = "";
                DateTime datetime = DateTime.Now;
                List<string> ignore_folders = new List<string>();

                if (args.Length == 1)
                {
                    path = args[0];
                }
                else if (args.Length == 2)
                // path *.txt
                // path -ignore_folder1
                // path 0:00
                // path 2001-02-23
                // path 2001/02/23
                {
                    path = args[0];

                    if (args[1].Contains("."))
                    {
                        file_pattern = args[1];
                    }
                    else if (args[1].StartsWith("-"))
                    {
                        args[1] = args[1].Remove(0, 1);
                        ignore_folders.Add(args[1]);
                    }
                    else if (args[1].Contains(":"))
                    {
                        try
                        {
                            datetime = DateTime.Parse(datetime.Date.ToShortDateString() + " " + args[1]);
                        }
                        catch
                        {
                            datetime = DateTime.Now;
                        }
                    }
                    else if ((args[1].Contains("-")) || (args[1].Contains("/")))
                    {
                        try
                        {
                            datetime = DateTime.Parse(args[1] + " " + datetime.TimeOfDay);
                        }
                        catch (FormatException)
                        {
                            datetime = DateTime.Now;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Usage: Touch [folder] [files] [yyyy-mm-dd] [HH:MM:SS] [-ignore_foler1 ignore_foler2 ...]");
                        return;
                    }
                }
                else if (args.Length >= 3)
                // path *.txt -ignore_folder1 {args[3] ...]
                // path *.txt 01:23 {args[3] ...]
                // path *.txt 2001-02-23 {args[3] ...]
                // path *.txt 2001/02/23 {args[3] ...]
                // path -ignore_folder1 ignore_folder2 {args[3] ...]
                // path 01:23 -ignore_folder1 {args[3] ...]
                // path 2001-02-23 -ignore_folder1 {args[3] ...]
                // path 2001/02/23 -ignore_folder1 {args[3] ...]
                // path 2001-02-23 01:23 {args[3] ...]
                // path 2001/02/23 01:23 {args[3] ...]
                {
                    path = args[0];

                    if (args[1].Contains("."))
                    {
                        file_pattern = args[1];

                        // process args[2]
                        if (args[2].StartsWith("-"))
                        {
                            args[2] = args[2].Remove(0, 1);
                            ignore_folders.Add(args[2]);
                        }
                        else if (args[2].Contains(":"))
                        {
                            try
                            {
                                datetime = DateTime.Parse(datetime.Date.ToShortDateString() + " " + args[2]);
                            }
                            catch
                            {
                                datetime = DateTime.Now;
                            }
                        }
                        else if ((args[2].Contains("-")) || (args[2].Contains("/")))
                        {
                            try
                            {
                                datetime = DateTime.Parse(args[2] + " " + datetime.TimeOfDay);
                            }
                            catch (FormatException)
                            {
                                datetime = DateTime.Now;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Usage: Touch [folder] [files] [yyyy-mm-dd] [HH:MM:SS] [-ignore_foler1 ignore_foler2 ...]");
                            return;
                        }
                    }
                    else if (args[1].StartsWith("-"))
                    {
                        args[1] = args[1].Remove(0, 1);
                        ignore_folders.Add(args[1]);

                        // process args[2]
                        if (
                            (!args[2].StartsWith("-")) &&
                            (!args[2].Contains(":")) &&
                            (!args[2].Contains("-")) &&
                            (!args[2].Contains("/"))
                           )
                        {
                            args[2] = args[2].Remove(0, 1);
                            ignore_folders.Add(args[2]);
                        }
                        else
                        {
                            Console.WriteLine("Usage: Touch [folder] [files] [yyyy-mm-dd] [HH:MM:SS] [-ignore_foler1 ignore_foler2 ...]");
                            return;
                        }
                    }
                    else if (args[1].Contains(":"))
                    {
                        try
                        {
                            datetime = DateTime.Parse(datetime.Date.ToShortDateString() + " " + args[1]);
                        }
                        catch
                        {
                            datetime = DateTime.Now;
                        }

                        // process args[2]
                        if (args[2].StartsWith("-"))
                        {
                            args[2] = args[2].Remove(0, 1);
                            ignore_folders.Add(args[2]);
                        }
                        else
                        {
                            Console.WriteLine("Usage: Touch [folder] [files] [yyyy-mm-dd] [HH:MM:SS] [-ignore_foler1 ignore_foler2 ...]");
                            return;
                        }
                    }
                    else if ((args[1].Contains("-")) || (args[1].Contains("/")))
                    {
                        try
                        {
                            datetime = DateTime.Parse(args[1] + " " + datetime.TimeOfDay);
                        }
                        catch (FormatException ex)
                        {
                            datetime = DateTime.Now;
                        }

                        // process args[2]
                        if (args[2].StartsWith("-"))
                        {
                            args[2] = args[2].Remove(0, 1);
                            ignore_folders.Add(args[2]);
                        }
                        else if (args[2].Contains(":"))
                        {
                            try
                            {
                                datetime = DateTime.Parse(datetime.Date.ToShortDateString() + " " + args[2]);
                            }
                            catch
                            {
                                datetime = DateTime.Now;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Usage: Touch [folder] [files] [yyyy-mm-dd] [HH:MM:SS] [-ignore_foler1 ignore_foler2 ...]");
                            return;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Usage: Touch [folder] [files] [yyyy-mm-dd] [HH:MM:SS] [-ignore_foler1 ignore_foler2 ...]");
                        return;
                    }

                    // process args[3] args[4] args[5] ...
                    for (int i = 3; i < args.Length; i++)
                    {
                        args[i] = args[i].Remove(0, 1);
                        ignore_folders.Add(args[i]);
                    }
                }


                if (Directory.Exists(path)) // path is a folder
                {
                    if (file_pattern.Length == 0)
                    {
                        Touch.TouchTree(path, datetime, ignore_folders);
                    }
                    else
                    {
                        Touch.TouchTree(path, file_pattern, datetime, ignore_folders);
                    }
                }
                else  // path is a file pattern
                {
                    file_pattern = path;
                    Touch.TouchFiles(".", file_pattern, datetime, ignore_folders);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
