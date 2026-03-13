using command_box.Commands;
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
    public class HelpCommand : ICommand
    {
        public string Name => "help";
        public string Description => "Show all available commands";
        public string CommandPath => throw new NotImplementedException();
        public string Usage => "help";
        public CommandType Type => CommandType.Internal;

        public WriteLineDelegate WriteLine { get; set; }
        public CommandsCollection Commands { get; set; }

        public HelpCommand()
        {
            WriteLine = Console.WriteLine;
            Commands = new CommandsCollection(WriteLine);
        }
        public HelpCommand(CommandsCollection commands, WriteLineDelegate writeLine) : this()
        {
            WriteLine = writeLine;
            Commands = commands;
        }
        public override string ToString() => $"{Name}";
        public void Execute(string[] args)
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
    }
}
