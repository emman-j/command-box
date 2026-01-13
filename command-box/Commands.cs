using command_box.Enums;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace command_box
{
    public class Commands : Collection<Command>
    {
        [JsonIgnore]
        public WriteLineDelegate WriteLine { get; set; }

        public Commands()
        {
            WriteLine = Console.WriteLine;
        }

        public Commands(IEnumerable<Command> commands) : this()
        {
            AddRange(commands);
        }
        public Commands(WriteLineDelegate writeLine) : this() 
        {
            WriteLine = writeLine;
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
        public void AddRange(IEnumerable<Command> commands)
        {
            foreach (var command in commands)
            {
                this.Add(command);
            }
        }

        public void RemoveRange(IEnumerable<Command> commands)
        {
            foreach (var command in commands)
            {
                this.Remove(command);
            }
        }
        public void SaveCache(string cachePath)
        {
            WriteLine($"Saving commands cache...");
            Commands commands = new Commands(this.Where(command => command.Type != CommandType.None).OrderBy(c => c.Name));
            string json = JsonConvert.SerializeObject(commands, Formatting.Indented);
            File.WriteAllText(cachePath, json);
        }
        public void LoadCache(string cachePath)
        {
            if (!File.Exists(cachePath))
                throw new DirectoryNotFoundException($"The scripts cache '{cachePath}' does not exist.");

            Commands c = new Commands(this.Where(command => command.Type != CommandType.None));
            RemoveRange(c);

            WriteLine($"Loading commands from cache...");
            string json = File.ReadAllText(cachePath);
            Commands commands = JsonConvert.DeserializeObject<Commands>(json);
            AddRange(commands);
        }
        public void RefreshCache(string directoryPath, string cachePath)
        {
            WriteLine($"Refeshing cache...");
            ClearCache(cachePath);
            LoadCommandsFromDirectory(directoryPath);
            SaveCache(cachePath);
        }
        public void ClearCache(string cachePath)
        {
            Commands commands = new Commands(this.Where(command => command.Type != CommandType.None));
            RemoveRange(commands);
            if (File.Exists(cachePath))
            {
                WriteLine($"Clearing commands cache...");
                File.Delete(cachePath);
            }
        }
        public void LoadCommandsFromDirectory(string directoryPath)
        {
            if(!Directory.Exists(directoryPath))
                throw new DirectoryNotFoundException($"The scripts directory '{directoryPath}' does not exist.");

            WriteLine($"Loading commands from directory...");

            foreach (string file in Directory.GetFiles(directoryPath))
            {
                string name = Path.GetFileNameWithoutExtension(file);
                string ext = Path.GetExtension(file).ToLower();

                WriteLine($" - {name}");

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

                Command command = new Command(
                    name,
                    metadata["description"],
                    file,
                    metadata["usage"],
                    type
                );

                this.Add(command);
            }
        }

    }
}
