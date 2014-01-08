using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class TextAnalyzer
{
    public static bool s_skip_whole_number = false;
    public static int s_max_positions = 10;
    public static int s_cache_size = 4 * 1024 * 1024;

    private class CharRepetition
    {
        public int length;
        public int frequency;
        //public long position = -1L;
        //public override string ToString()
        //{
        //    StringBuilder str = new StringBuilder();
        //    str.Append(length + "," + frequency + "," + position);
        //    return str.ToString();
        //}

        // firsy MAX_FOUND_POSITIONS positions AND +1 for last position
        public List<long> positions = new List<long>(s_max_positions + 1);

        //public override string ToString()
        //{
        //    StringBuilder str = new StringBuilder();
        //    str.Append(length + ", " + frequency + ", ");
        //    // if too many positions, show first only
        //    for (int i = 0; i < positions.Count; i++)
        //    {
        //        if (i == MAX_FOUND_POSITIONS)
        //        {
        //            if (i < positions.Count - 1)
        //            {
        //                str.Append("..._");
        //            }
        //            str.Append(positions[positions.Count - 1]);
        //            break;
        //        }
        //
        //        str.Append(positions[i]);
        //        if (i < (positions.Count - 1))
        //        {
        //            str.Append("_");
        //        }
        //    }
        //    return str.ToString();
        //}
        public override string ToString()
        {
            StringBuilder str = new StringBuilder();
            str.Append(length + "," + frequency + ",");
            for (int i = 0; i < positions.Count; i++)
            {
                if (i == s_max_positions)
                {
                    if (i < frequency - 1)
                    {
                        str.Append("...,");
                    }
                    str.Append(positions[positions.Count - 1]);
                    break;
                }

                str.Append(positions[i]);
                if (i < (positions.Count - 1))
                {
                    str.Append(",");
                }
            }
            return str.ToString();
        }
    };
    private static void ProcessCharRepetition(char previous_char, int repetition_length, long position, SortedDictionary<char, SortedDictionary<int, CharRepetition>> char_repetitions_table)
    {
        try
        {
            // if char exist
            if (char_repetitions_table.ContainsKey(previous_char))
            {
                // if repetition_length exist, increment frequency
                if ((char_repetitions_table[previous_char]).ContainsKey(repetition_length))
                {
                    ((char_repetitions_table[previous_char])[repetition_length]).frequency++;
                    //if (((char_repetitions_table[previous_char])[repetition_length]).position == -1L)
                    //{
                    //    ((char_repetitions_table[previous_char])[repetition_length]).position = (position - (repetition_length - 1));
                    //}
                    if (((char_repetitions_table[previous_char])[repetition_length]).positions.Count <= s_max_positions)
                    {
                        ((char_repetitions_table[previous_char])[repetition_length]).positions.Add(position - (repetition_length - 1));
                    }
                    else
                    {
                        ((char_repetitions_table[previous_char])[repetition_length]).positions[s_max_positions] = (position - (repetition_length - 1));
                    }
                }
                else // char exist but new repetition_length
                {
                    CharRepetition char_repetition = new CharRepetition();
                    char_repetition.length = repetition_length;
                    char_repetition.frequency = 1;
                    //if (char_repetition.position == -1L)
                    //{
                    //    char_repetition.position = (position - (repetition_length - 1));
                    //}
                    if (char_repetition.positions.Count <= s_max_positions)
                    {
                        char_repetition.positions.Add(position - (repetition_length - 1));
                    }
                    else
                    {
                        char_repetition.positions[s_max_positions] = (position - (repetition_length - 1));
                    }
                    char_repetitions_table[previous_char].Add(repetition_length, char_repetition);
                }
            }
            else // new char, new repetition_length
            {
                CharRepetition char_repetition = new CharRepetition();
                char_repetition.length = repetition_length;
                char_repetition.frequency = 1;
                //if (char_repetition.position == -1L)
                //{
                //    char_repetition.position = (position - (repetition_length - 1));
                //}
                if (char_repetition.positions.Count <= s_max_positions)
                {
                    char_repetition.positions.Add(position - (repetition_length - 1));
                }
                else
                {
                    char_repetition.positions[s_max_positions] = (position - (repetition_length - 1));
                }
                SortedDictionary<int, CharRepetition> char_repetitions = new SortedDictionary<int, CharRepetition>();
                char_repetitions.Add(repetition_length, char_repetition);
                char_repetitions_table.Add(previous_char, char_repetitions);
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }

    // scalable loading blocks of content into RAM
    public static string AnalyzeFile(string filename)
    {
        Console.WriteLine("_________________________________________________________________");
        Console.WriteLine("Analyzing " + filename + " ...");
        Console.WriteLine("_________________________________________________________________");

        CalcCharFrequencies(filename);
        string result = CalcCharRepetitions(filename);
        PrimeFile(filename);
        CalcCharFrequencies(filename + ".prime");
        CalcCharRepetitions(filename + ".prime");
        return result;
    }
    public static string CalcCharFrequencies(string filename)
    {
        DateTime start_time = DateTime.Now;

        SortedDictionary<char, int> char_frequency_table = new SortedDictionary<char, int>();
        StreamReader reader = null;
        try
        {
            FileInfo file_info = new FileInfo(filename);
            long file_size = file_info.Length;

            reader = File.OpenText(filename);
            char[] cache = new char[s_cache_size];
            long blocks = 0;
            while (!reader.EndOfStream)
            {
                // read a new block and overwrite cache
                int read_data_length = reader.ReadBlock(cache, 0, s_cache_size);
                blocks++;
                if (cache.Length > 1)
                {
                    s_skip_whole_number = Char.IsDigit(cache[0]) && (cache[1] == '.');
                }

                long percentage = ((((blocks - 1) * s_cache_size) + read_data_length) * 100) / file_size;
                Console.ForegroundColor = ConsoleColor.Yellow;
                TimeSpan timespan = DateTime.Now - start_time;
                DateTime datetime = new DateTime(timespan.Ticks);
                Console.Write("\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\bCalculating digit frequencies {0}%   {1} days {2}", percentage, timespan.Days, datetime.ToString(" HH:mm:ss"));
                Console.ForegroundColor = ConsoleColor.White;

                // skip whole number
                if ((blocks == 1) && s_skip_whole_number)
                {
                    cache[0] = 'N';
                }
                for (int i = 0; i < read_data_length; i++)
                {
                    if ((cache[i] != 'N') && (cache[i] != '.'))
                    {
                        if (char_frequency_table.ContainsKey(cache[i]))
                        {
                            char_frequency_table[cache[i]]++;
                        }
                        else
                        {
                            char_frequency_table.Add(cache[i], 1);
                        }
                    }
                }
            } // end while
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex);
            Console.ForegroundColor = ConsoleColor.White;
        }
        finally
        {
            if (reader != null)
            {
                reader.Close();
                reader.Dispose();
            }
        }

        // write out statistics
        StringBuilder str = new StringBuilder();
        str.Append("Char,Frequency\r\n");
        foreach (char key in char_frequency_table.Keys)
        {
            str.Append(String.Format("{0},{1}\r\n", key, char_frequency_table[key]));
        }
        WriteFile(filename + ".frequency.csv", str.ToString());
        Console.WriteLine("\r\n" + str.ToString());
        return str.ToString();
    }
    public static string CalcCharRepetitions(string filename)
    {
        DateTime start_time = DateTime.Now;

        char current_char = '\0';
        char previous_char = '\0';
        int repetition_length = 0;
        SortedDictionary<char, SortedDictionary<int, CharRepetition>> char_repetitions_table = new SortedDictionary<char, SortedDictionary<int, CharRepetition>>();
        StreamReader reader = null;
        try
        {
            long current_position = 0;

            FileInfo file_info = new FileInfo(filename);
            long file_size = file_info.Length;

            reader = File.OpenText(filename);
            char[] cache = new char[s_cache_size];
            long blocks = 0;
            while (!reader.EndOfStream)
            {
                // read a new block and overwrite cache
                int read_data_length = reader.ReadBlock(cache, 0, s_cache_size);
                blocks++;
                if (cache.Length > 1)
                {
                    s_skip_whole_number = Char.IsDigit(cache[0]) && (cache[1] == '.');
                }

                long percentage = ((((blocks - 1) * s_cache_size) + read_data_length) * 100) / file_size;
                Console.ForegroundColor = ConsoleColor.Yellow;
                TimeSpan timespan = DateTime.Now - start_time;
                DateTime datetime = new DateTime(timespan.Ticks);
                Console.Write("\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\bCalculating digit repetitions {0}%   {1} days {2}", percentage, timespan.Days, datetime.ToString(" HH:mm:ss"));
                Console.ForegroundColor = ConsoleColor.White;

                // skip whole number
                if ((blocks == 1) && s_skip_whole_number)
                {
                    cache[0] = 'N';
                }
                for (int i = 0; i < read_data_length; i++)
                {
                    if ((cache[i] != 'N') && (cache[i] != '.'))
                    {
                        // save position
                        if (s_skip_whole_number == true)
                        {
                            current_position = ((blocks - 1) * s_cache_size) + i - 2;
                        }
                        else
                        {
                            current_position = ((blocks - 1) * s_cache_size) + i;
                        }

                        current_char = cache[i];
                        if (previous_char != '\0')
                        {
                            if (current_char == previous_char)
                            {
                                repetition_length++;
                            }
                            else // a new char
                            {
                                // save old char repetition_length
                                ProcessCharRepetition(previous_char, repetition_length, current_position, char_repetitions_table);

                                // new char repetition_length = 1 as new char
                                repetition_length = 1;
                            }
                        }
                        else
                        {
                            // first char repetition_length = 1 as just started
                            repetition_length = 1;
                        }

                        // save current_char
                        previous_char = current_char;
                    }
                }
            } // end of file

            // process last char
            ProcessCharRepetition(previous_char, repetition_length, current_position, char_repetitions_table);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex);
            Console.ForegroundColor = ConsoleColor.White;
        }
        finally
        {
            if (reader != null)
            {
                reader.Close();
                reader.Dispose();
            }
        }

        // write out statistics
        StringBuilder str = new StringBuilder();
        //str.Append("Char,Repeat,Times,Position\r\n");
        str.Append("Char,Repeat,Times,Positions\r\n");
        foreach (char charkey in char_repetitions_table.Keys)
        {
            SortedDictionary<int, CharRepetition> value = char_repetitions_table[charkey];
            foreach (int key in value.Keys)
            {
                str.Append(String.Format("{0},{1}\r\n", charkey, value[key].ToString()));
            }
        }
        WriteFile(filename + ".repetition.csv", str.ToString());
        Console.WriteLine("\r\n" + str.ToString());
        return str.ToString();
    }
    public static void PrimeFile(string filename)
    {
        DateTime start_time = DateTime.Now;

        StreamReader reader = null;
        StreamWriter writer = null;
        try
        {
            reader = File.OpenText(filename);
            string primefilename = filename + ".prime";
            writer = File.CreateText(primefilename);
            StringBuilder str = new StringBuilder(s_cache_size);

            FileInfo file_info = new FileInfo(filename);
            long file_size = file_info.Length;

            reader = File.OpenText(filename);
            char[] cache = new char[s_cache_size];
            long blocks = 0;
            while (!reader.EndOfStream)
            {
                // clear StringBuilder
                str.Length = 0;

                // read a new block and overwrite cache
                int read_data_length = reader.ReadBlock(cache, 0, s_cache_size);
                blocks++;
                if (cache.Length > 1)
                {
                    s_skip_whole_number = Char.IsDigit(cache[0]) && (cache[1] == '.');
                }

                long percentage = ((((blocks - 1) * s_cache_size) + read_data_length) * 100) / file_size;
                Console.ForegroundColor = ConsoleColor.Yellow;
                TimeSpan timespan = DateTime.Now - start_time;
                DateTime datetime = new DateTime(timespan.Ticks);
                Console.Write("\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\bPriming digits {0}%   {1} days {2}", percentage, timespan.Days, datetime.ToString(" HH:mm:ss"));
                Console.ForegroundColor = ConsoleColor.Gray;

                // skip whole number
                if ((blocks == 1) && s_skip_whole_number)
                {
                    cache[0] = 'N';
                }
                for (int i = 0; i < read_data_length; i++)
                {
                    char c = cache[i];
                    if (Char.IsDigit(c))
                    {
                        if ( // prime digit
                              (c == '1') ||
                              (c == '2') ||
                              (c == '3') ||
                              (c == '5') ||
                              (c == '7')
                            )
                        {
                            str.Append("1");
                        }
                        else if ( // composite digit
                              (c == '0') ||
                              (c == '4') ||
                              (c == '6') ||
                              (c == '8') ||
                              (c == '9')
                            )
                        {
                            str.Append("0");
                        }
                        else // other digits in the UnicodeCategory.DecimalDigitNumber category
                        {
                            str.Append(c);
                        }
                        //// IsNumber determines whether a Char is of any numeric Unicode category.
                        //// In addition to including digits, numbers include characters, fractions, subscripts, superscripts, Roman numerals, currency numerators, and encircled numbers.
                        //// This method contrasts with the IsDigit method, which determines whether a Char is a radix-10 digit.
                        //if (Char.IsNumber(c))
                        //{
                        //    if ( // prime digit
                        //          (c == '1') ||
                        //          (c == '2') ||
                        //          (c == '3') ||
                        //          (c == '5') ||
                        //          (c == '7')
                        //        )
                        //    {
                        //        str.Append("1");
                        //    }
                        //    else if ( // composite digit
                        //          (c == '0') ||
                        //          (c == '4') ||
                        //          (c == '6') ||
                        //          (c == '8') ||
                        //          (c == '9')
                        //        )
                        //    {
                        //        str.Append("0");
                        //    }
                        //    else // Unicode characters, fractions, subscripts, superscripts, Roman numerals, currency numerators, and encircled numbers.
                        //    {
                        //        str.Append(c);
                        //    }
                    }
                    else
                    {
                        str.Append(c);
                    }
                }
                writer.Write(str.ToString());
            }
            Console.WriteLine("\r\n" + "Digits were converted to 1 if prime, 0 if composite.\r\n");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex);
            Console.ForegroundColor = ConsoleColor.White;
        }
        finally
        {
            if (reader != null)
            {
                reader.Close();
                reader.Dispose();
            }
            if (writer != null)
            {
                writer.Dispose();
                writer.Close();
            }
        }
    }
    public static void CleanPiFastFile(string filename)
    {
        Console.WriteLine("Cleaning  " + filename + " ...");

        StreamReader reader = null;
        StreamWriter writer = null;
        try
        {
            reader = File.OpenText(filename);

            if (File.Exists(filename + ".clean"))
            {
                File.Delete(filename + ".clean");
            }
            writer = File.CreateText(filename + ".clean");

            // read first line
            string line = reader.ReadLine();
            // huge line throws exception

            // skip file header
            if (line != null)
            {
                if (line.Contains("PiFast"))
                {
                    int start = -1;
                    while ((line = reader.ReadLine()) != null)
                    {
                        // skip all lines until actual value line
                        start = line.IndexOf(" = ");
                        if (start == -1) continue;

                        line = line.Remove(0, start + 3);
                        writer.Write(line);
                        break;
                    }

                    // remove trailing running numbers after ":"
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] parts = line.Split(':');
                        if (parts.Length == 2)
                        {
                            // remove spaces
                            parts[0] = parts[0].Replace(" ", "");
                        }
                        // in all cases
                        writer.Write(parts[0]);
                    }
                }
                else // other file formats
                {    // small clean file already so just copy it

                    // restart reading file by blocks
                    if (reader != null)
                    {
                        reader.Close();
                        reader.Dispose();
                    }
                    if (writer != null)
                    {
                        writer.Dispose();
                        writer.Close();
                    }
                    if (File.Exists(filename + ".clean"))
                    {
                        File.Delete(filename + ".clean");
                    }
                    File.Copy(filename, filename + ".clean");
                }
            }
        }
        catch (OutOfMemoryException)
        {
            // restart reading file by blocks
            if (reader != null)
            {
                reader.Close();
                reader.Dispose();
            }
            reader = File.OpenText(filename);

            // huge clean file already so just copy it
            StringBuilder str = new StringBuilder(s_cache_size);
            char[] cache = new char[s_cache_size];
            while (!reader.EndOfStream)
            {
                // clear StringBuilder
                str.Length = 0;

                // read a new block and overwrite cache
                int count = reader.ReadBlock(cache, 0, cache.Length);
                for (int i = 0; i < count; i++)
                {
                    str.Append(cache[i]);
                }
                writer.Write(str.ToString());
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex);
            Console.ForegroundColor = ConsoleColor.White;
        }
        finally
        {
            if (reader != null)
            {
                reader.Close();
                reader.Dispose();
            }
            if (writer != null)
            {
                writer.Dispose();
                writer.Close();
            }
        }
    }
    public static string FindInFile(string filename, string pattern)
    {
        // Doesn't work if pattern length > CACHE_SIZE
        // which is very unlikely indeed

        DateTime start_time = DateTime.Now;

        List<long> positions = new List<long>();

        StreamReader reader = null;
        try
        {
            FileInfo file_info = new FileInfo(filename);
            long file_size = file_info.Length;

            reader = File.OpenText(filename);
            char[] cache = new char[s_cache_size];
            long blocks = 0;
            int pattern_index = 0;
            while (!reader.EndOfStream)
            {
                // read a new block and overwrite cache
                int read_data_length = reader.ReadBlock(cache, 0, s_cache_size);
                blocks++;
                if (cache.Length > 1)
                {
                    s_skip_whole_number = Char.IsDigit(cache[0]) && (cache[1] == '.');
                }

                long percentage = ((((blocks - 1) * s_cache_size) + read_data_length) * 100) / file_size;
                Console.ForegroundColor = ConsoleColor.Yellow;
                TimeSpan timespan = DateTime.Now - start_time;
                DateTime datetime = new DateTime(timespan.Ticks);
                Console.Write("\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\bFinding \"{0}\"... {1}%   {2} days {2}", pattern, percentage, timespan.Days, datetime.ToString(" HH:mm:ss"));
                Console.ForegroundColor = ConsoleColor.White;

                for (int i = 0; i < read_data_length; i++)
                {
                    bool found = true;
                    if (cache[i] == pattern[pattern_index]) // found start of pattern
                    {
                        // check rest of pattern
                        for (int j = 1 + pattern_index; j < pattern.Length; j++)
                        {
                            // if current block finshed before the pattern finishes
                            if ((i + j) >= read_data_length)
                            {
                                // then continue finding the rest of the pattern in the next block
                                pattern_index = j; // remember pattern_index for next block
                                i += j; // break i loop --> read a new block (if any) //???

                                // make sure not add position yet
                                found = false;
                                break; // j loop
                            }
                            else // find in current block
                            {
                                if (cache[i + j] != pattern[j])
                                {
                                    found = false;
                                    break; // j loop
                                }
                            }
                        }

                        // add position if found
                        if (found == true)
                        {
                            long skip_by = s_skip_whole_number ? 2 : 0;
                            positions.Add(((blocks - 1) * s_cache_size) + i + 1 - pattern_index - skip_by);
                            i += pattern.Length;
                        }
                    }
                    else // new block does not continue pattern
                    {
                        found = false;
                        pattern_index = 0;
                    }
                }

            } // end while
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex);
            Console.ForegroundColor = ConsoleColor.White;
        }
        finally
        {
            if (reader != null)
            {
                reader.Close();
                reader.Dispose();
            }
        }

        StringBuilder str = new StringBuilder();
        str.Append(String.Format("{0} matches of \"{1}\" found at positions:\r\n", positions.Count, pattern));
        foreach (long position in positions)
        {
            str.Append(position.ToString() + ",");
        }
        str.Append("\r\n");
        WriteFile(filename + ".pattern.csv", str.ToString());
        Console.WriteLine("\r\n" + str.ToString());
        return str.ToString();
    }

    // not scalable loading full content into RAM
    private static string ReadFile(string filename)
    {
        StringBuilder str = new StringBuilder();
        StreamReader reader = null;
        try
        {
            reader = File.OpenText(filename);

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                str.AppendLine(line);
            }
            return str.ToString();
        }
        finally
        {
            if (reader != null)
            {
                reader.Close();
                reader.Dispose();
            }
        }
    }
    private static void WriteFile(string filename, string content)
    {
        StreamWriter writer = null;
        try
        {
            writer = File.CreateText(filename);
            writer.Write(content);
        }
        finally
        {
            if (writer != null)
            {
                writer.Dispose();
                writer.Close();
            }
        }
    }
    private static void AppendFile(string filename, string content)
    {
        StreamWriter writer = null;
        try
        {
            writer = File.AppendText(filename);
            writer.Write(content);
        }
        finally
        {
            if (writer != null)
            {
                writer.Dispose();
                writer.Close();
            }
        }
    }
    public static bool ReplaceInText(ref string text, string search_text, string replace_text)
    {
        try
        {
            text = text.Replace(search_text, replace_text);
            return true;
        }
        catch
        {
            return false;
        }
    }
    public static bool ReplaceInFile(string filename, string search_text, string replace_text)
    {
        try
        {
            string contents = "";

            StreamReader reader = new StreamReader(filename);
            if (reader != null)
            {
                contents = reader.ReadToEnd();
                reader.Close();
                reader.Dispose();

                contents = contents.Replace(search_text, replace_text);

                StreamWriter writer = new StreamWriter(filename);
                if (writer != null)
                {
                    writer.Write(contents);
                    writer.Close();
                    writer.Dispose();
                }
            }
            return true;
        }
        catch
        {
            return false;
        }
    }
}
