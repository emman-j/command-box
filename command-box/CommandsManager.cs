using command_box.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace command_box
{
    public class CommandsManager
    {
        private Dictionary<string, string> _directories = new Dictionary<string, string>()
        {
            {"Scripts Directory", Paths.ScriptsDir },
            {"Data Directory", Paths.DataDir },
            {"Settings Directory", Paths.SettingsDir },
            {"Error Logs Directory", Paths.ErrorLogsDir },
            {"Console Logs Directory", Paths.ConsoleLogsDir }
        };

        public static string Build = "alpha";
        public static string AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static string AppVersion = $"{AssemblyVersion}-{Build}";
        public CommandsCollection Commands { get; private set; }
        public List<string> CommandsHistory { get; set; } = new List<string>();
        public WriteLineDelegate WriteLine { get; set; }


        public CommandsManager(WriteLineDelegate writeLine = null)
        {
            if (writeLine == null)
                WriteLine = Console.WriteLine;
            WriteLine = writeLine;
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
            Commands.Add(new Command("help", "Show all available commands", "", "help", Enums.CommandType.Internal)
            {
                Action = ShowHelp
            });

            Commands.Add(new Command("dir", "Show all application directories", "", "dir", Enums.CommandType.Internal)
            {
                Action = ShowDirectories
            });

            Commands.Add(new Command("cache", "Manages the cache (save, load, refresh, clear).", "", "cache [save|load|refresh|clear]", Enums.CommandType.Internal)
            {
                Action = Cache
            });

            Commands.Add(new Command("history", "Manages the history (save, load, clear).", "", "history [save|load|clear]", Enums.CommandType.Internal)
            { 
                Action = History
            });
        }
        public void SaveHistory()
        {
            WriteLine("Saving command history...");
            File.WriteAllLines(Paths.History, CommandsHistory);
        }
        private void LoadHistory()
        {
            if (!File.Exists(Paths.History))
                return;
            WriteLine("Loading command history...");
            var historyLines = File.ReadAllLines(Paths.History);
            CommandsHistory.Clear();
            CommandsHistory.AddRange(historyLines);
        }
        private void ClearHistory()
        {
            WriteLine("Clearing command history...");
            CommandsHistory.Clear();
            if (File.Exists(Paths.History))
                File.Delete(Paths.History);
        }

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

        public void ShowDirectories(string[] args)
        {
            WriteLine();
            WriteLine("-------------------------------------------------------");
            WriteLine($"Application Directory");
            WriteLine("-------------------------------------------------------");

            int maxNameLength = _directories.Max(c => c.Key.Length);

            foreach (var kvp in _directories)
            {
                string dirName = kvp.Key;
                string dirPath = kvp.Value;

                string paddedName = dirName.PadRight(maxNameLength);
                WriteLine($"{paddedName} : {dirPath}");
            }

            WriteLine("-------------------------------------------------------");
        }
        public void ShowHelp(string[] args)
        {
            WriteLine();
            WriteLine("-------------------------------------------------------");
            WriteLine("Available Commands");
            WriteLine("-------------------------------------------------------");

            int maxNameLength = Commands.Max(c => c.Name.Length);

            foreach (var command in Commands)
            {
                string paddedName = command.Name.PadRight(maxNameLength);
                WriteLine($"{paddedName} : {command.Description}");

                string indent = new string(' ', maxNameLength);
                WriteLine($"{indent} : {command.Usage}");
            }

            WriteLine("-------------------------------------------------------");
        }

        private void Cache(string[] args)
        {
            if (args.Length == 0)
            {
                WriteLine("Usage: cache [save|load|refresh|clear]");
                return;
            }

            switch (args[0])
            {
                case "save":
                    Commands.SaveCache(Paths.Cache);
                    break;
                case "load":
                    Commands.LoadCache(Paths.Cache);
                    break;
                case "clear":
                    Commands.ClearCache(Paths.Cache);
                    break;
                case "refresh":
                    Commands.RefreshCache(Paths.ScriptsDir, Paths.Cache);
                    break;
                default:
                    WriteLine("Usage: cache [save|load|refresh|clear]");
                    break;
            }
        }
        private void History(string[] args)
        {
            if (args.Length == 0)
            {
                WriteLine("Usage: history [save|load|clear]");
                return;
            }

            switch (args[0])
            {
                case "save":
                    SaveHistory();
                    break;
                case "load":
                    LoadHistory();
                    break;
                case "clear":
                    ClearHistory();
                    break;
                default:
                    WriteLine("Usage: history [save|load|clear]");
                    break;
            }
        }
    }
}
