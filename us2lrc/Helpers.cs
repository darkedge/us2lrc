using System;

namespace us2lrc
{
    public static class Helpers
    {
        public static string ToLyricTiming(this TimeSpan timeSpan)
        {
            return String.Format("{0:00}:{1:00}.{2:00}", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds / 10);
        }

        public static string ToLyricTiming(this TimeSpan timeSpan, bool firstNote)
        {
            if (firstNote)
                return "[" + ToLyricTiming(timeSpan) + "]";
            return "<" + ToLyricTiming(timeSpan) + ">";
        }
    }
}