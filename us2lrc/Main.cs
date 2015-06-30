using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace us2lrc
{
    class Converter
    {
        static void ConvertUSToTXT(string path, string outPath)
        {
            Console.WriteLine("Converting file: " + path);
            Dictionary<string, string> tagValues = new Dictionary<string, string>();
            string[] TAGS_MANDATORY = { "TITLE", "ARTIST", "MP3", "GAP", "BPM" };
            List<string> lines = new List<string>();
            string str = "";
            string last = "";
            double bpm = 0.0;
            double gap = 0.0;

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
                        for (int i = 0; i < TAGS_MANDATORY.Length; i++)
                        {
                            string tag = match.Groups["TAG"].Value;
                            if (tag == TAGS_MANDATORY[i])
                            {
                                tagValues[tag] = match.Groups["VALUE"].Value;
                                if (tag == "BPM")
                                {
                                    double.TryParse(tagValues["BPM"], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out bpm);
                                }
                                if (tag == "GAP")
                                {
                                    double.TryParse(tagValues["GAP"], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out gap);
                                }
                                break;
                            }
                        }
                    }
                    else
                    {
                        string[] columns = line.Split(new char[] { ' ' }, 5);
                        if (columns.Length > 0)
                        {
                            if (columns[0] == ":" || columns[0] == "*" || columns[0] == "F")
                            {
                                // : - Regular note
                                // * - Golden note
                                // F - Freestyle note
                                // LRC has no support to distinguish these
                                if (columns.Length == 5)
                                {
                                    // number of beats into the song at which point this syllable appears.
                                    double minutes = double.Parse(columns[1]) / bpm;

                                    TimeSpan startTime = TimeSpan.FromMinutes(minutes) + TimeSpan.FromMilliseconds(gap);
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
                                    double durationMinutes = double.Parse(columns[2]) / bpm;
                                    TimeSpan lastTime = startTime + TimeSpan.FromMinutes(durationMinutes);
                                    last = string.Format("<{0:00}:{1:00}.{2:00}>", lastTime.Minutes, lastTime.Seconds, lastTime.Milliseconds / 10);

                                    // "I don't have a list of which numbers correspond to which notes, though I believe that '0' is C1"
                                    //int pitch = int.Parse(c[3]); // Ignored

                                    // Add syllable
                                    str += columns[4];
                                }
                                else
                                {
                                    Console.WriteLine("Found note with missing columns!");
                                }
                            }
                            else if (columns[0].StartsWith("-") || columns[0] == "E")
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
                ConvertUSToTXT(name, outPath);
            }
        }
    }
}
