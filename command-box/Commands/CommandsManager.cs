using System.Reflection;
using command_box.Common;
using command_box.Delegates;
using command_box.Enums;

namespace command_box.Commands
{
    public class CommandsManager
    {
        private readonly Dictionary<string, string> _directories = new Dictionary<string, string>()
        {
            {"Scripts Directory", Paths.ScriptsDir },
            {"Data Directory", Paths.DataDir },
            {"Settings Directory", Paths.SettingsDir },
            {"Error Logs Directory", Paths.ErrorLogsDir },
            {"Console Logs Directory", Paths.ConsoleLogsDir }
        };

        public static readonly string Build = "alpha";
        public static readonly string AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static readonly string AppVersion = $"{AssemblyVersion}-{Build}";
        public CommandsCollection Commands { get; private set; } = new CommandsCollection();
        public List<string> CommandsHistory { get; set; } = new List<string>();
        public WriteLineDelegate WriteLine { get; set; }

        public CommandsManager(WriteLineDelegate writeLine = null)
        {
            WriteLine = writeLine ?? Console.WriteLine;
            EnsureAppDirectory();
            LoadCommands();
        }

        private void EnsureAppDirectory()
        {
            WriteLine($"Ensuring application directories exist...");
            foreach (var kvp in _directories)
            {
                string dirName = kvp.Key;
                string dirPath = kvp.Value;

                WriteLine($" - {dirName}: {dirPath}");
                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);
            }
        }
        private void LoadCommands()
        {
            Commands = new CommandsCollection(WriteLine);

            InitializeInternalCommands();

            if (!File.Exists(Paths.Cache))
            {
                Commands.LoadCommandsFromDirectory(Paths.ScriptsDir);
                Commands.SaveCache(Paths.Cache);
                return;
            }

            Commands.LoadCache(Paths.Cache);
            LoadHistory();
        }
        private void InitializeInternalCommands()
        {
            Commands.Add(new Command("help", "Show all available commands", "", "help", CommandType.Internal)
            {
                Action = ShowHelp
            });

            Commands.Add(new Command("dir", "Show all application directories", "", "dir", CommandType.Internal)
            {
                Action = ShowDirectories
            });

            Commands.Add(new Command("cache", "Manages the cache (save, load, refresh, clear).", "", "cache [save|load|refresh|clear]", CommandType.Internal)
            {
                Action = Cache
            });

            Commands.Add(new Command("history", "Manages the history (save, load, clear).", "", "history [save|load|clear]", CommandType.Internal)
            { 
                Action = History
            });
        }
        public void SaveHistory() => ExecuteCommand("history", new string[] { "save" });
        private void LoadHistory() => ExecuteCommand("history", new string[] { "load" });
        public void ExecuteCommand(string commandName, string[] args)
        {
            Command command = Commands.FirstOrDefault(c => c.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));
            if (command == null)
            {
                WriteLine($"Command '{commandName}' not found.");
                return;
            }
            WriteLine($"Executing command: {command.Name}");
            command.Execute(args);
        }
    }
}
