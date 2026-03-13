using command_box.Common;
using command_box.Delegates;
using command_box.Enums;
using command_box.Interfaces;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace command_box.Commands
{
    public class CommandsCollection : Collection<ICommand>
    {
        [JsonIgnore]
        public WriteLineDelegate WriteLine { get; set; }
        public event Action<Exception, string> OnError;

        public CommandsCollection()
        {
            WriteLine = Console.WriteLine;
            OnError += OnErrorOccured;
        }
        public CommandsCollection(IEnumerable<ICommand> commands) : this()
        {
            AddRange(commands);
        }
        public CommandsCollection(WriteLineDelegate writeLine) : this() 
        {
            WriteLine = writeLine;
        }
        private void HandleError(Exception ex, [CallerMemberName] string operation = "")
            => OnError?.Invoke(ex, operation);
        private void OnErrorOccured(Exception exception, string operation)
        {
            ErrorLogger.Instance?.LogException(this, exception, operation);
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

        private bool ValidateMetadata(Dictionary<string, string> metadata, string scriptName, out string errorMessage)
        {
            var requiredFields = new[] { "description", "usage" };
            var missing = requiredFields.Where(field => !metadata.ContainsKey(field)).ToList();

            if (missing.Any())
            {
                errorMessage = $"Missing required metadata: {string.Join(", ", missing.Select(f => $"@meta {f}"))}";
                return false;
            }

            errorMessage = null;
            return true;
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
        public ICommand? GetByName(string name)
        {
            return this.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
        public CommandsCollection GetByType(CommandType type)
        {
            return new CommandsCollection(this.Where(c => c.Type == type));
        }
        public CommandsCollection GetAllExceptType(CommandType type)
        {
            return new CommandsCollection(this.Where(c => c.Type != type));
        }

        public void LoadCommandsFromDirectory(string[] directoryPaths)
        {
            int loaded = 0;
            int skipped = 0;
            int errors = 0;

            foreach (var directory in directoryPaths)
            {
                if (!Directory.Exists(directory))
                {
                    WriteLine($"Directory not found: {directory}");
                    return;
                }

                WriteLine($"Loading commands from {Path.GetFileName(directory)}...");

                foreach (string file in Directory.GetFiles(directory))
                {
                    string name = Path.GetFileNameWithoutExtension(file);
                    string ext = Path.GetExtension(file).ToLower();

                    CommandType type;
                    bool requiresMetadata = true;
                    bool isShortcut = false;
                    string actualPath = file;

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
                        case ".lnk":  // ← Handle shortcuts
                            try
                            {
                                actualPath = ShortcutResolver.ResolveShortcut(file);

                                if (string.IsNullOrWhiteSpace(actualPath) || !File.Exists(actualPath))
                                {
                                    WriteLine($" - {name}");
                                    WriteLine($"Error: Shortcut target not found");
                                    errors++;
                                    continue;
                                }

                                // Determine type based on target
                                string targetExt = Path.GetExtension(actualPath).ToLower();
                                if (targetExt == ".exe")
                                {
                                    type = CommandType.Executable;
                                    requiresMetadata = false;
                                    isShortcut = true;
                                }
                                else
                                {
                                    WriteLine($" - {name}");
                                    WriteLine($"Skipped: Shortcut target is not an executable ({targetExt})");
                                    skipped++;
                                    continue;
                                }
                            }
                            catch (Exception ex)
                            {
                                WriteLine($" - {name}");
                                WriteLine($"Error resolving shortcut: {ex.Message}");
                                errors++;
                                continue;
                            }
                            break;
                        case ".exe":
                            type = CommandType.Executable;
                            requiresMetadata = false;
                            break;
                        default:
                            continue; // Skip non-script/non-executable files
                    }

                    WriteLine($" - {name}");

                    try
                    {
                        Dictionary<string, string> metadata;

                        if (requiresMetadata)
                        {
                            metadata = ParseMetadata(actualPath);

                            if (!ValidateMetadata(metadata, name, out string errorMessage))
                            {
                                WriteLine($"Skipped: {errorMessage}");
                                skipped++;
                                continue;
                            }
                        }
                        else
                        {
                            // For executables, try to get metadata
                            metadata = TryGetExecutableMetadata(actualPath);
                        }

                        Add(new Command(
                            name,
                            metadata.GetValueOrDefault("description", $"{name} executable"),
                            actualPath,  // Use resolved path, not .lnk path
                            metadata.GetValueOrDefault("usage", name),
                            type
                        ));

                        if (isShortcut)
                            WriteLine($"Loaded (shortcut > {Path.GetFileName(actualPath)})");
                        else
                            WriteLine($"Loaded");

                        loaded++;
                    }
                    catch (Exception ex)
                    {
                        WriteLine($"   ✗ Error: {ex.Message}");
                        errors++;
                    }
                }
            }
           
            WriteLine();
            WriteLine($"Summary: {loaded} loaded, {skipped} skipped, {errors} errors");
        }

        private Dictionary<string, string> TryGetExecutableMetadata(string exePath)
        {
            var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(exePath);

                if (!string.IsNullOrWhiteSpace(versionInfo.FileDescription))
                    metadata["description"] = versionInfo.FileDescription;
                else
                    metadata["description"] = Path.GetFileNameWithoutExtension(exePath);

                if (!string.IsNullOrWhiteSpace(versionInfo.Comments))
                    metadata["usage"] = versionInfo.Comments;
                else
                    metadata["usage"] = Path.GetFileNameWithoutExtension(exePath);
            }
            catch
            {
                // If we can't read version info, use filename
                metadata["description"] = Path.GetFileNameWithoutExtension(exePath);
                metadata["usage"] = Path.GetFileNameWithoutExtension(exePath);
            }

            return metadata;
        }
        public void SaveCache(string cachePath)
        {
            try
            {
                WriteLine($"Saving commands cache...");
                var scriptCommands = GetAllExceptType(CommandType.Internal);
                string json = JsonConvert.SerializeObject(scriptCommands, Formatting.Indented);
                File.WriteAllText(cachePath, json);
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }
        public void LoadCache(string cachePath)
        {
            try
            {
                if (!File.Exists(cachePath))
                    throw new DirectoryNotFoundException($"The scripts cache '{cachePath}' does not exist.");

                CommandsCollection c = GetAllExceptType(CommandType.Internal);
                RemoveRange(c);

                WriteLine($"Loading commands from cache...");
                string json = File.ReadAllText(cachePath);

                // Deserialize as List<Command> to avoid issues with Collection<ICommand>
                var commands = JsonConvert.DeserializeObject<List<Command>>(json);

                if (commands != null)
                {
                    AddRange(commands);
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }
        public void RefreshCache(string[] directoryPaths, string cachePath)
        {
            try
            {
                WriteLine($"Refeshing cache...");
                ClearCache(cachePath);
                LoadCommandsFromDirectory(directoryPaths);
                SaveCache(cachePath);
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }
        public void ClearCache(string cachePath)
        {
            try
            {
                CommandsCollection commands = GetAllExceptType(CommandType.Internal);
                RemoveRange(commands);
                if (File.Exists(cachePath))
                {
                    WriteLine($"Clearing commands cache...");
                    File.Delete(cachePath);
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }
    }
}
