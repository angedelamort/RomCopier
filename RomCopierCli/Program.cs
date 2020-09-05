using CommandSerializer;
using RomCopier.Consoles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace RomCopier
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var options = CommandSerializer<CommandArgs>.Parse(args);
                options.Validate();

                try
                {
                    var parser = new ConsoleParser();
                    parser.Init(options.Countries);
                    var listOption = options.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                    var files = (from file in Directory.EnumerateFiles(options.Input, "*.*", listOption) select file).ToList();
                    var remainingFiles = parser.Filter(files);

                    if (options.Help)
                        Console.Out.WriteLine(CommandSerializer<CommandArgs>.GetHelp(Console.WindowWidth));
                    else if (options.Stats)
                        Console.Out.WriteLine(options.GetSummary(files, remainingFiles));
                    else
                        Copy(remainingFiles, options);
                }
                catch (Exception ex)
                {
                    Console.Out.WriteLine(ex);
                }
            }
            catch (Exception)
            {
                Console.Out.WriteLine(CommandSerializer<CommandArgs>.GetHelp(Console.WindowWidth));
            }
        }

        private static string Number(long current, long total)
        {
            var digits = Math.Floor(Math.Log10(total) + 1);
            return $"[{current.ToString("D" + digits)}/{total}]";
        }

        private static void Copy(List<string> remainingFiles, CommandArgs options)
        {
            long i = 0;
            foreach (var src in remainingFiles)
            {
                i++;
                var dst = Path.Combine(options.Output, Path.GetFileName(src));
                try
                {
                    var exists = File.Exists(dst); // TODO: check CRC? and SIZE?
                    var copy = false;
                    if (exists)
                    {
                        if (options.Overwrite)
                        {
                            Console.Out.WriteLine(options.Simple
                                ? Path.GetFileName(src)
                                : $"{Number(i, remainingFiles.Count)} Overwriting '{dst}' with '{src}'");
                            copy = true;
                        }
                        else if (options.Update)
                        {
                            if (!options.Simple)
                                Console.Out.WriteLine($"{Number(i, remainingFiles.Count)} Skipping {src}");
                        }
                        else
                        {
                            if (options.Interactive)
                            {
                                Console.Out.WriteLine($"Are you sure you want to override the file '{dst}'? (y/n)");
                                var key = Console.ReadKey(true);
                                if (key.Key == ConsoleKey.K)
                                    copy = true;
                            }
                            
                            if (!copy && !options.Simple)
                                Console.Out.WriteLine($"{Number(i, remainingFiles.Count)} Skipping '{src}'");
                        }
                    }
                    else
                    {
                        Console.Out.WriteLine(options.Simple
                            ? Path.GetFileName(src)
                            : $"{Number(i, remainingFiles.Count)} Copying from {src} to {dst}");
                        copy = true;
                    }

                    if (!options.Simulate && copy)
                    {
                        if (options.Uncompress)
                        {
                            using (var archive = ArchiveFactory.Open(src))
                            {
                                foreach (var entry in archive.Entries)
                                {
                                    if (!entry.IsDirectory)
                                    {
                                        entry.WriteToDirectory(options.Output, new ExtractionOptions
                                        {
                                            ExtractFullPath = false,
                                            Overwrite = true
                                        });
                                    }
                                }
                            }
                        }
                        else
                        {
                            File.Copy(src, dst);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.Out.WriteLine($"*** Could not copy file {src} because of {e.Message}");
                }
            }
        }
    }
}
