using System.Reflection;
using command_box.Common;
using command_box.Delegates;
using command_box.Enums;
using command_box.Interfaces;
using command_box.InternalCommands;

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
                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);
            }

            WriteLine($"    - use dir to see all app directories");
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

            WriteLine($"Use help to see all available commands");
        }
        private void InitializeInternalCommands()
        {
            Commands.Add(new HelpCommand(Commands, WriteLine));
            Commands.Add(new DirectoriesCommand(WriteLine));
            Commands.Add(new CacheCommand(Commands, WriteLine));
            Commands.Add(new HistoryCommand(CommandsHistory, WriteLine));
        }
        public void SaveHistory() => ExecuteCommand("history", new string[] { "save" });
        private void LoadHistory() => ExecuteCommand("history", new string[] { "load" });
        public void ExecuteCommand(string commandName, string[] args)
        {
            ICommand command = Commands.GetByName(commandName);
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
