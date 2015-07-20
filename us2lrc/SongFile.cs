using System;
using System.Collections.Generic;

namespace us2lrc
{
    public class SongFile
    {
        public string title { get; set; }
        public string artist { get; set; }
        public double bpm { get; set; }
        public double gap { get; set; }
        public string video { get; set; }
        public string year { get; set; }
        public string language { get; set; }
        public string genre { get; set; }
        public string mp3 { get; set; }

        public IEnumerable<String> IsValid()
        {
            if (bpm <= 0)
            {
                yield return String.Format("BPM of {0} is not valid.", bpm);
            }
            if (String.IsNullOrEmpty(title))
            {
                yield return "Title not found.";
            }
            if (String.IsNullOrEmpty(artist))
            {
                yield return "Artist not found.";
            }
        }
    }
}