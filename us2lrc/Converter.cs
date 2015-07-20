using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace us2lrc
{
    public class Converter
    {
        public Encoding Encoding { get; set; }
        private readonly string _fileName;
        private const string byField = "Converted using us2lrc - https://github.com/darkedge/us2lrc";

        private SongFile _songFile;
        private ICollection<string> _lines;
        private const String converterField = "us2lrc";

        public Converter(String fileName)
        {
            _fileName = fileName;
            Encoding = Encoding.Default;
        }

        public void Convert()
        {
            _songFile = new SongFile();

            // Read file
            try
            {
                using (StreamReader rdr = new StreamReader(_fileName, Encoding))
                {
                    while (!rdr.EndOfStream)
                    {
                        var line = rdr.ReadLine();
                        if (String.IsNullOrEmpty(line))
                        {
                            continue;
                        }

                        if (TagsMatch(line, ref _songFile))
                        {
                            continue;
                        }
                        else
                        {
                            var errors = _songFile.IsValid().ToArray();
                            if (errors.Any())
                            {
                                foreach (var error in errors)
                                {
                                    Console.WriteLine("\t" + error);
                                }
                                break;
                            }
                            else
                            {
                                
                                _lines = WriteNotes(line, rdr, _songFile).ToArray();
                                break;
                            }
                        }
                    }
                }
            }
            catch (NoteException)
            {
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Enhanced LRC format
        private static IEnumerable<string> WriteNotes(string firstLine, StreamReader rdr, SongFile songFile)
        {
            HashSet<string> notes = new HashSet<string>(new []{":", "*", "F"});
            HashSet<string> endNote = new HashSet<string>(new[] { "-"});
            string line = firstLine;
            TimeSpan lastTime = TimeSpan.Zero;
            StringBuilder sb = new StringBuilder();
            do
            {
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                if (line.Equals("P1", StringComparison.InvariantCultureIgnoreCase))
                {
                    Console.WriteLine("Can't parse multiline, only parsing p1");
                }
                else if (line.Equals("P2", StringComparison.InvariantCultureIgnoreCase))
                {
                    yield break;
                }
                
                string[] columns = line.Split(new[] {' '}, 5);
                if (columns.Length > 1)
                {
                    var columnType = columns[0];

                    // number of beats into the song at which point this syllable appears.
                    

                    var beatsColumn = columns[1];
                    double beats;
                    if (!double.TryParse(beatsColumn, out beats))
                    {
                        throw new NoteException(String.Format("Line '{0}' couldn't parse the beat start.", line));
                    }


                    TimeSpan startTime = TimeSpan.FromMinutes(beats / songFile.bpm) + TimeSpan.FromMilliseconds(songFile.gap);
                    if (notes.Contains(columnType, StringComparer.InvariantCultureIgnoreCase))
                    {
                        // : - Regular note
                        // * - Golden note
                        // F - Freestyle note
                        // LRC has no support to distinguish these
                        if (columns.Length == 5)
                        {

                            sb.Append(startTime.ToLyricTiming(sb.Length == 0));

                            // Add syllable
                            var syllable = columns[4];
                            sb.Append(syllable);
                        }
                        else
                        {
                            Console.WriteLine("Weird song column found: '{0}'", line);
                        }
                        var durationColumn = columns[2];
                        double duration;
                        if (!double.TryParse(durationColumn, out duration))
                        {
                            throw new NoteException(String.Format("Line '{0}' couldn't duration of the beat.", line));
                        }

                        lastTime = startTime + TimeSpan.FromMinutes(duration /songFile.bpm);
                    }
                    else if (endNote.Contains(columnType, StringComparer.InvariantCultureIgnoreCase))
                    {
                        // Close current line
                        sb.Append(lastTime.ToLyricTiming(false));
                        yield return sb.ToString();
                        sb.Length = 0;
                        sb.Capacity = 16;
                    }
                }

                line = rdr.ReadLine();
            } while (!rdr.EndOfStream);

            if (line != null && line.Equals("E", StringComparison.CurrentCultureIgnoreCase) && sb.Length > 0)
            {
                sb.Append(lastTime.ToLyricTiming(false));
                yield return sb.ToString();
            }
        }

        private bool TagsMatch(string line, ref SongFile songFile)
        {
            var match = Regex.Match(line, "^#(?<TAG>.*):(?<VALUE>.*)");
            if (match.Success)
            {
                string tag = match.Groups["TAG"].Value;
                string val = match.Groups["VALUE"].Value;
                if (tag == "TITLE")
                {
                    songFile.title = val;
                    return true;
                }
                else if (tag == "ARTIST")
                {
                    songFile.artist = val;
                    return true;
                }
                else if (tag == "GAP")
                {
                    double gap;
                    val = val.Replace(",", CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator);
                    double.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out gap);
                    songFile.gap = gap;
                    return true;
                }
                else if (tag == "BPM")
                {
                    double bpm;
                    val = val.Replace(",", CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator);
                    double.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out bpm);
                    songFile.bpm = bpm * 4.0;
                    return true;
                }
                else if (tag == "RELATIVE")
                {
                    if (val.Equals("YES", StringComparison.InvariantCulture))
                    {
                        throw new NotSupportedException("#Relative tag is not supported");
                    }
                    return true;
                }
                else if (tag.Equals("VIDEO", StringComparison.InvariantCultureIgnoreCase))
                {
                    songFile.video = val;
                    return true;
                }
                else if (tag.Equals("YEAR", StringComparison.InvariantCultureIgnoreCase))
                {
                    songFile.year = val;
                    return true;
                }
                else if (tag.Equals("LANGUAGE", StringComparison.InvariantCultureIgnoreCase))
                {
                    songFile.language = val;
                    return true;
                }
                else if (tag.Equals("GENRE", StringComparison.InvariantCultureIgnoreCase))
                {
                    songFile.genre = val;
                    return true;
                }
                else if (tag.Equals("MP3", StringComparison.InvariantCultureIgnoreCase))
                {
                    songFile.mp3 = val;
                    return true;
                }
                else
                {
                    Console.WriteLine("\tUnknown tag '{0}' with value '{1}', ignoring.", tag, val);
                    return true;
                }
            }

            return false;
        }

        public void Save(string outPath)
        {
            //Get OutFile
            string outFile = GetOutFilePath(_fileName, outPath, ".lrc");
            // Create destination text file
            WriteFile(outFile, _songFile, _lines);
        }

        private void WriteFile(string outFile, SongFile songFile, IEnumerable<String> lines)
        {
            using (Stream stream = File.OpenWrite(outFile))
            {
                using (var writer = new StreamWriter(stream, Encoding))
                {
                    writer.WriteLine(string.Format("[ar:{0}]", songFile.artist));
                    writer.WriteLine(string.Format("[ti:{0}]", songFile.title));
                    writer.WriteLine(string.Format("[by:{0}]", byField));
                    writer.WriteLine(string.Format("[re:{0}]", converterField));

                    foreach (var line in lines)
                    {
                        writer.WriteLine(line);
                    }
                }
            }
        }

        private static string GetOutFilePath(string path, string outPath, string extension = null)
        {
            string fileWOExtension = extension != null ?  Path.GetFileNameWithoutExtension(path) + extension : Path.GetFileName(path) ;
            var fullPath = Path.GetFullPath(outPath);
            return Path.Combine(fullPath, fileWOExtension);
        }

        public String GetSourceName()
        {
            return Path.GetFileName(_fileName);
        }
    }

    internal class NoteException : Exception
    {
        public NoteException(string message) : base(message)
        {
            
        }
    }
}