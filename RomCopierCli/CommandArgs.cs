using CommandSerializer.Attributes;
using RomCopier.Consoles;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RomCopier
{
    public class CommandArgs
    {
        [PositionalParameter(Name ="INPUT_DIR", Required = true)]
        public string Input { get; set; }

        [PositionalParameter(Name = "OUTPUT_DIR", Required = true)]
        public string Output { get; set; }

        [Parameter(Action = "summary", Alias = 's', HelpText = "Display a summary for the input and the options used.")]
        public bool Stats { get; set; }

        [Parameter(Action = "dry-run", Alias = 'd', HelpText = "Do the operations without actually copying the files")]
        public bool Simulate { get; set; }

        [Parameter(Action = "bare", Alias = 'b', HelpText = "Display the filename only")]
        public bool Simple { get; set; }

        [Parameter(Action = "countries", Alias = 'c', Required = false,
            HelpText = "Enumarate countries using coma separated list. The ordering is important. The available values: {USA, Japan, Europe, World} Default: USA")]
        public string Countries { get; set; } = Country.USA.ToString();

        [Parameter(Action = "recursive", Alias = 'r', HelpText = "Search recursively in the different folders")]
        public bool Recursive { get; set; }

        [Parameter(Action = "interactive", Alias = 'i', HelpText = "Ask if you want to overwrite the file")]
        public bool Interactive { get; set; }

        [Parameter(Action = "force", Alias = 'f', HelpText = "Overwrite if file already exists. Overrides -u -i")]
        public bool Overwrite { get; set; }

        [Parameter(Action = "update", Alias = 'u', HelpText = "Check if file exists and skip it. Overrides -i")]
        public bool Update { get; set; }

        [Parameter(Action = "uncompress", Alias = 'z', HelpText = "un-compress the ROM from the archive.")]
        public bool Uncompress { get; set; }

        [Parameter(Action = "help", Alias = 'h', HelpText = "Display this help")]
        public bool Help { get; set; }

        public void Validate()
        {
            if (!Directory.Exists(Input))
                throw new DirectoryNotFoundException($"{Input} is not a directory.");

            if (!Directory.Exists(Output))
                throw new DirectoryNotFoundException($"{Output} is not a directory.");
        }

        public string GetSummary(List<string> files, List<string> remainingFiles)
        {
            var sb = new StringBuilder();

            sb.AppendLine("### Statistics ###");
            sb.AppendLine();
            sb.AppendLine("[Source]");
            sb.AppendLine($"  Directory:  {Input}");
            sb.AppendLine($"  File Count: {files.Count}");
            sb.AppendLine();
            sb.AppendLine("[Destination]");
            sb.AppendLine($"  Directory:  {Output}");
            sb.AppendLine($"  File Count: {remainingFiles.Count}");
            sb.AppendLine($"  Size:       {SizeOnDisk(remainingFiles)}");
            sb.AppendLine();
            sb.AppendLine("[Flags]");
            sb.AppendLine($"   Countries: {Countries}");
            sb.AppendLine($"   Recursive: {Recursive}");
            sb.AppendLine($"   Force:     {Overwrite}");
            sb.AppendLine();

            return sb.ToString();
        }

        private static string SizeOnDisk(List<string> files)
        {
            long counter = 0;
            Parallel.ForEach(files, x => Interlocked.Add(ref counter, new FileInfo(x).Length));
            return Utility.SizeSuffix(counter, 2);
        }
    }
}
