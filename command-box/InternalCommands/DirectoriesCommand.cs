using command_box.Commands;
using command_box.Common;
using command_box.Delegates;
using command_box.Enums;
using command_box.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace command_box.InternalCommands
{
    public class DirectoriesCommand : ICommand
    {
        private readonly Dictionary<string, string> _directories = new Dictionary<string, string>()
        {
            {"Scripts Directory", Paths.ScriptsDir },
            {"Data Directory", Paths.DataDir },
            {"Settings Directory", Paths.SettingsDir },
            {"Error Logs Directory", Paths.ErrorLogsDir },
            {"Console Logs Directory", Paths.ConsoleLogsDir }
        };

        public string Name => "dir";
        public string Description => "Show all application directories";
        public string CommandPath => throw new NotImplementedException();
        public string Usage => "dir";
        public CommandType Type => CommandType.Internal;

        public WriteLineDelegate WriteLine { get; set; }

        public DirectoriesCommand()
        {
            WriteLine = Console.WriteLine;
        }
        public DirectoriesCommand(WriteLineDelegate writeLine) : this()
        {
            WriteLine = writeLine;
        }
        public override string ToString() => $"{Name}";
        public void Execute(string[] args)
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
    }
}
