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
    public class HistoryCommand : ICommand
    {
        private readonly string _historyFilePath = Paths.History;

        public string Name => "history";
        public string Description => "Manages the history (save, load, clear).";
        public string CommandPath => throw new NotImplementedException();
        public string Usage => "history [save|load|clear]";
        public CommandType Type => CommandType.Internal;

        public WriteLineDelegate WriteLine { get; set; }
        public List<string> CommandsHistory { get; set; }

        public HistoryCommand()
        {
            WriteLine = Console.WriteLine;
            CommandsHistory = new List<string>();
        }
        public HistoryCommand(List<string> commands, WriteLineDelegate writeLine) : this()
        {
            WriteLine = writeLine;
            CommandsHistory = commands;
        }
        public override string ToString() => $"{Name}";
        public void Execute(string[] args)
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

        public void SaveHistory()
        {
            WriteLine("Saving command history...");
            File.WriteAllLines(_historyFilePath, CommandsHistory);
        }
        private void LoadHistory()
        {
            if (!File.Exists(_historyFilePath))
                return;
            WriteLine("Loading command history...");
            var historyLines = File.ReadAllLines(_historyFilePath);
            CommandsHistory.Clear();
            CommandsHistory.AddRange(historyLines);
        }
        private void ClearHistory()
        {
            WriteLine("Clearing command history...");
            CommandsHistory.Clear();
            if (File.Exists(_historyFilePath))
                File.Delete(_historyFilePath);
        }
    }
}
