using System.Collections.ObjectModel;
using command_box.Delegates;
using command_box.Enums;
using command_box.Interfaces;
using Newtonsoft.Json;

namespace command_box.Commands
{
    public class CommandsCollection : Collection<ICommand>
    {
        [JsonIgnore]
        public WriteLineDelegate WriteLine { get; set; }

        public CommandsCollection()
        {
            WriteLine = Console.WriteLine;
        }
        public CommandsCollection(IEnumerable<ICommand> commands) : this()
        {
            AddRange(commands);
        }
        public CommandsCollection(WriteLineDelegate writeLine) : this() 
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

        public void AddRange(IEnumerable<ICommand> commands)
        {
            foreach (var command in commands)
            {
                Add(command);
            }
        }
        public void RemoveRange(IEnumerable<ICommand> commands)
        {
            foreach (var command in commands)
            {
                Remove(command);
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

                Add(command);
            }
        }
        public void SaveCache(string cachePath)
        {
            WriteLine($"Saving commands cache...");
            CommandsCollection commands = new CommandsCollection(this.Where(command => command.Type != CommandType.Internal).OrderBy(c => c.Name));
            string json = JsonConvert.SerializeObject(commands, Formatting.Indented);
            File.WriteAllText(cachePath, json);
        }
        public void LoadCache(string cachePath)
        {
            if (!File.Exists(cachePath))
                throw new DirectoryNotFoundException($"The scripts cache '{cachePath}' does not exist.");

            CommandsCollection c = new CommandsCollection(this.Where(command => command.Type != CommandType.Internal));
            RemoveRange(c);

            WriteLine($"Loading commands from cache...");
            string json = File.ReadAllText(cachePath);
            CommandsCollection commands = JsonConvert.DeserializeObject<CommandsCollection>(json);
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
            CommandsCollection commands = new CommandsCollection(this.Where(command => command.Type != CommandType.Internal));
            RemoveRange(commands);
            if (File.Exists(cachePath))
            {
                WriteLine($"Clearing commands cache...");
                File.Delete(cachePath);
            }
        }
    }
}
