using System;
using System.IO;
using System.Linq;

namespace us2lrc
{
    class ConverterMain
                    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: us2lrc.exe \"input directory\" \"output directory\"");
                Console.WriteLine("Example: us2lrc.exe \"C:\\mylyrics \"C:\\mylyrics\\output\"");
                Console.WriteLine("Example: us2lrc.exe \".\" \"output\"");
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
            if (!Directory.Exists(outPath))
            {
            Directory.CreateDirectory(outPath);
            }
            

            // Put all txt files in root directory into array.
            string[] inFiles = Directory.GetFiles(inPath, "*.txt"); // <-- Case-insensitive

            

            foreach (var file in inFiles
                .Select(name => new Converter(name)))
            {
                try
                {
                    Console.WriteLine("Converting file: {0}", file.GetSourceName());
                    file.Convert();
                    file.Save(outPath);
                }
                catch (Exception e)
            {
                    Console.Error.WriteLine("\tCouldn't parse file, error message: {0}", e.Message );
                }
                

            }

            Console.WriteLine( inFiles.Length + " files processed.");
        }
    }
}
