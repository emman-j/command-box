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
    public class CacheCommand : ICommand
    {
        private readonly string _cacheFilePath = Paths.Cache;
        private readonly string _scriptsDirectory = Paths.ScriptsDir;
        public string Name => "cache";
        public string Description => "Manages the cache (save, load, refresh, clear).";
        public string CommandPath => throw new NotImplementedException();
        public string Usage => "cache [save|load|refresh|clear]";
        public CommandType Type => CommandType.Internal;

        public WriteLineDelegate WriteLine { get; set; }
        public CommandsCollection Commands { get; set; }

        public CacheCommand()
        { 
            WriteLine = Console.WriteLine;
            Commands = new CommandsCollection(WriteLine);
        }
        public CacheCommand(CommandsCollection commands, WriteLineDelegate writeLine) : this()
        {
            WriteLine = writeLine;
            Commands = commands;
        }
        public override string ToString() => $"{Name}";
        public void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                WriteLine("Usage: cache [save|load|refresh|clear]");
                return;
            }

            switch (args[0])
            {
                case "save":
                    Commands.SaveCache(_cacheFilePath);
                    break;
                case "load":
                    Commands.LoadCache(_cacheFilePath);
                    break;
                case "clear":
                    Commands.ClearCache(_cacheFilePath);
                    break;
                case "refresh":
                    Commands.RefreshCache(_scriptsDirectory, _cacheFilePath);
                    break;
                default:
                    WriteLine("Usage: cache [save|load|refresh|clear]");
                    break;
            }
        }
    }
}
