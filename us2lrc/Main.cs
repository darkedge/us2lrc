using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace us2lrc
{
    class USFormat
    {
        public class Tags
        {
            // == Mandatory tags ==
            public const string Title = "#TITLE:";
            public const string Artist = "#ARTIST:";
            public const string Mp3 = "#MP3:";
            public const string Gap = "#GAP:";
            public const string Bpm = "#BPM:";

            // == Optional tags ==
            public const string Genre = "#GENRE:";
            public const string Edition = "#EDITION:";
            public const string Cover = "#COVER:";
            public const string Video = "#VIDEO:";
            public const string Background = "#BACKGROUND:";
            public const string Relative = "#RELATIVE:";
        }

        // == Mandatory tags ==

        // Title of the song
        public string Title;
        // Artist behind the song
        public string Artist;
        // The name of the MP3 being used for this song.
        public string Mp3;
        // The amount of time, in milliseconds, before the lyrics start.
        public int Gap;
        // Beats per minute.
        public float Bpm;

        // == Optional tags ==

        // The genre of the song.
        public string Genre;
        // Typically refers to the SingStar edition, if applicable, that the .txt file is taken from.
        public string Edition;
        // Typically the single/album art appropriate for the song, to be displayed on the song selection screen.
        public string Cover;
        // The name of the video file used for this song.
        public string Video;
        // If you don't have a video file, then you may prefer to have a background image displayed instead of a plain background or visualization.
        public string Background;
        // This is an unusual tag that I will talk about later. It is simply set to YES or NO.
        public bool Relative;
    }

    class Converter
    {
        static void ConvertUSToTXT(string path, string outPath)
        {
            Console.WriteLine("Converting file: " + path);

            // Read file
            using (StreamReader rdr = new StreamReader(path))
            {
                string line;
                while ((line = rdr.ReadLine()) != null)
                {
                    var matches = Regex.Match(line, "(?#<TAG>.*):(?<VALUE>.*)");
                    if (matches.Success)
                    {
                        Console.WriteLine(matches.Groups["TAG"].Value);
                        Console.WriteLine(matches.Groups["VALUE"].Value);
                    }
                }
            }

            //FileStream file = new FileStream("highscores.txt", FileMode.Append, FileAccess.Write);

            // Create destination text file
            string fileWOExtension = Path.GetFileNameWithoutExtension(path);
            string outFile = Path.Combine(outPath, fileWOExtension + ".lrc");
            using (StreamWriter file = new StreamWriter(outFile))
            {
                // Write file

                /*
                foreach (string line in lines)
                {
                    // If the line doesn't contain the word 'Second', write the line to the file. 
                    if (!line.Contains("Second"))
                    {
                        file.WriteLine(line);
                    }
                }
                 * */
            }
        }

        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: \"input directory\" \"output directory\"");
                return;
            }

            // Check if input directory exists
            string inPath = args[0];
            if (!Directory.Exists(inPath))
            {
                Console.WriteLine("Could not find input directory! Exiting.");
                return;
            }

            // Find/Create output directory
            string outPath = args[1];
            Directory.CreateDirectory(outPath);

            // Put all txt files in root directory into array.
            string[] inFiles = Directory.GetFiles(inPath, "*.txt"); // <-- Case-insensitive

            foreach (string name in inFiles)
            {
                ConvertUSToTXT(name, outPath);
            }
        }
    }
}
