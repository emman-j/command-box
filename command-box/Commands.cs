using command_box.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace command_box
{
    public class Commands : Collection<Command>
    {

        public Commands() { }

        public void LoadCommandsFromDirectory(string directoryPath)
        {
            if(!Directory.Exists(directoryPath))
                throw new DirectoryNotFoundException($"The scripts directory '{directoryPath}' does not exist.");


            Console.WriteLine($"Loading commands from directory...");

            foreach (string file in Directory.GetFiles(directoryPath))
            {
                string name = Path.GetFileNameWithoutExtension(file);
                string ext = Path.GetExtension(file).ToLower();

                Console.WriteLine($" - {name}");

                CommandType type;
                switch (ext)
                {
                    case ".bat":
                        type = CommandType.Batch;
                        break;
                    case ".ps1":
                        type = CommandType.PowerShell;
                        break;
                    case ".py":
                        type = CommandType.Python;
                        break;
                    default:
                        continue;
                }

                var metadata = ParseMetadata(file);
                this.Add(new Command(
                    name,
                    metadata["description"],
                    file,
                    metadata["usage"],
                    type
                ));
            }
        }

        private static Dictionary<string, string> ParseMetadata(string path)
        {
            var meta = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var line in File.ReadLines(path).Take(20))
            {
                if (!line.Contains("@meta", StringComparison.OrdinalIgnoreCase))
                    continue;

                // Extract the part after @meta
                var metaIndex = line.IndexOf("@meta", StringComparison.OrdinalIgnoreCase);
                if (metaIndex == -1) continue;

                var afterMeta = line.Substring(metaIndex + 5).Trim(); // +5 for "@meta"
                var parts = afterMeta.Split('=', 2);

                if (parts.Length == 2)
                    meta[parts[0].Trim()] = parts[1].Trim();
            }
            return meta;
        }
    }
}
