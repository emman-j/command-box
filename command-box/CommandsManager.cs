using command_box.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace command_box
{
    public class CommandsManager
    {
        public Commands Commands { get; private set; }

        public CommandsManager()
        {
            EnsureAppDirectory();
            LoadCommands();
        }

        private void EnsureAppDirectory()
        {
            Dictionary<string, string> directories = new Dictionary<string, string>()
            { 
                {"Scripts Directory", Paths.ScriptsDir },
                {"Data Directory", Paths.DataDir },
                {"Settings Directory", Paths.SettingsDir },
                {"Error Logs Directory", Paths.ErrorLogsDir },
                {"Console Logs Directory", Paths.ConsoleLogsDir }
            };
            Console.WriteLine($"Ensuring application directories exist...");
            foreach (var kvp in directories) 
            {
                string dirName = kvp.Key;
                string dirPath = kvp.Value;

                Console.WriteLine($" - {dirName}: {dirPath}");
                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);
            }
        }
        private void LoadCommands()
        {
            Commands = new Commands();
            Commands.LoadCommandsFromDirectory(Paths.ScriptsDir);
        }

        public void ExecuteCommand(string commandName, string[] args)
        {
            Command command = Commands.FirstOrDefault(c => c.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));
            if (command == null)
            {
                Console.WriteLine($"Command '{commandName}' not found.");
                return;
            }
            Console.WriteLine($"Executing command: {command.Name}");
            Console.WriteLine(command.Execute(args));
        }
        public void ShowHelp()
        {
            Console.WriteLine("Available Commands:");
            foreach (var command in Commands)
            {
                Console.WriteLine(command.ToString());
                Console.WriteLine($"Description: {command.Description}");
                Console.WriteLine($"Usage: {command.Usage}");
                Console.WriteLine();
            }
        }
    }
}
