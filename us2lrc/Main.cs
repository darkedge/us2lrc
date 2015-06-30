using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace us2lrc
{
    class Line
    {
        public float startTime = 0.0f;
        public float duration = 0.0f;
    }
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

        public static string[] TAGS_MANDATORY = { "TITLE", "ARTIST", "MP3", "GAP", "BPM" };

        // These are not used for now
        


        

        /*
        // == Mandatory tags ==

        // Title of the song
        public string Title = null;
        // Artist behind the song
        public string Artist = null;
        // The name of the MP3 being used for this song.
        public string Mp3 = null;
        // The amount of time, in milliseconds, before the lyrics start.
        public int Gap = 0;
        // Beats per minute.
        public float Bpm = 0.0f;

        // == Optional tags ==

        // The genre of the song.
        public string Genre = null;
        // Typically refers to the SingStar edition, if applicable, that the .txt file is taken from.
        public string Edition = null;
        // Typically the single/album art appropriate for the song, to be displayed on the song selection screen.
        public string Cover = null;
        // The name of the video file used for this song.
        public string Video = null;
        // If you don't have a video file, then you may prefer to have a background image displayed instead of a plain background or visualization.
        public string Background = null;
        // This is an unusual tag that I will talk about later. It is simply set to YES or NO.
        public bool Relative = false;
         * */
    }

    class Converter
    {
        static void ConvertUSToTXT(string path, string outPath)
        {
            Console.WriteLine("Converting file: " + path);
            //USFormat format = new USFormat();

            Dictionary<string, string> tagValues = new Dictionary<string, string>();
            List<string> lines = new List<string>();
            string str = "";
            string last = "";

            // Read file
            using (StreamReader rdr = new StreamReader(path))
            {
                string line;
                while ((line = rdr.ReadLine()) != null)
                {
                    // For each line
                    var match = Regex.Match(line, "^#(?<TAG>.*):(?<VALUE>.*)");
                    if (match.Success)
                    {
                        for (int i = 0; i < USFormat.TAGS_MANDATORY.Length; i++)
                        {
                            string tag = match.Groups["TAG"].Value;
                            if (tag == USFormat.TAGS_MANDATORY[i])
                            {
                                tagValues[tag] = match.Groups["VALUE"].Value;
                                break;
                            }
                        }
                    }
                    else
                    {
                        string[] c = line.Split(new char[] { ' ' }, 5);
                        if (c.Length > 0)
                        {
                            if (c[0] == ":" || c[0] == "*" || c[0] == "F")
                            {
                                // : - Regular note
                                // * - Golden note
                                // F - Freestyle note
                                // LRC has no support to distinguish these
                                if (c.Length == 5)
                                {
                                    // number of beats into the song at which point this syllable appears.
                                    double minutes = double.Parse(c[1]) / double.Parse(tagValues["BPM"]);

                                    TimeSpan startTime = TimeSpan.FromMinutes(minutes) + TimeSpan.FromMilliseconds(double.Parse(tagValues["GAP"]));
                                    if (str.Length == 0)
                                    {
                                        // First note of the line
                                        str += string.Format("[{0:00}:{1:00}.{2:00}]", startTime.Minutes, startTime.Seconds, startTime.Milliseconds / 10);
                                    }
                                    else
                                    {
                                        // Rest of syllables
                                        str += string.Format("<{0:00}:{1:00}.{2:00}>", startTime.Minutes, startTime.Seconds, startTime.Milliseconds / 10);
                                    }

                                    // number of beats that the note goes on for
                                    double durationMinutes = double.Parse(c[1]) / double.Parse(tagValues["BPM"]);
                                    TimeSpan lastTime = startTime + TimeSpan.FromMinutes(durationMinutes);
                                    last = string.Format("<{0:00}:{1:00}.{2:00}>", lastTime.Minutes, lastTime.Seconds, lastTime.Milliseconds / 10);

                                    // "I don't have a list of which numbers correspond to which notes, though I believe that '0' is C1"
                                    //int pitch = int.Parse(c[3]); // Ignored

                                    // Add syllable
                                    str += c[4];
                                }
                                else
                                {
                                    Console.WriteLine("Found note with missing columns!");
                                }
                            }
                            else if (c[0].StartsWith("-") || c[0] == "E")
                            {
                                // Add duration of last note
                                str += last;
                                // Close current line
                                lines.Add(str);
                                str = "";
                            }
                        }
                    }
                }
            }

            //FileStream file = new FileStream("highscores.txt", FileMode.Append, FileAccess.Write);

            // Create destination text file
            string fileWOExtension = Path.GetFileNameWithoutExtension(path);
            string outFile = Path.Combine(outPath, fileWOExtension + ".lrc");
            using (StreamWriter file = new StreamWriter(outFile))
            {
                foreach (string line in lines)
                {
                    file.WriteLine(line);
                }
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
                //try
                //{
                    ConvertUSToTXT(name, outPath);
                //}
                //catch (Exception e)
                //{
                //    Console.WriteLine(e.Message);
                //}
            }
        }
    }
}
