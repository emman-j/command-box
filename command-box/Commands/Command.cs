using System.Diagnostics;
using command_box.Enums;
using command_box.Interfaces;

namespace command_box.Commands
{
    public class Command : ICommand
    {
        public string Name { get; }
        public string Description { get; }
        public string CommandPath { get; }
        public string Usage { get; }
        public CommandType Type { get; }
        public override string ToString() => $"{Name}";

        public Command(string name, string description, string commandPath, string usage, CommandType type)
        { 
            Name = name;
            Description = description;
            CommandPath = commandPath;
            Usage = usage;
            Type = type;
        }

        public void Execute(string[] args)
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            string argString = string.Join(" ", args);

            switch (Type)
            {
                case CommandType.Batch:
                    psi.FileName = "cmd.exe";
                    psi.Arguments = $"/c \"{CommandPath} {argString}\"";
                    break;
                case CommandType.PowerShell:
                    psi.FileName = "powershell.exe";
                    psi.Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{CommandPath}\" {argString}";
                    break;
                case CommandType.Python:
                    psi.FileName = "python";
                    psi.Arguments = $"\"{CommandPath}\" {argString}";
                    break;
            }

            psi.RedirectStandardOutput = false; // Let output go directly to console
            psi.RedirectStandardError = false;  // Let errors go directly to console
            psi.RedirectStandardInput = false;  // Let input come directly from console
            psi.UseShellExecute = false;
            psi.CreateNoWindow = false;         // Show in same console window

            using (Process process = Process.Start(psi))
            {
                process?.WaitForExit();
            }
        }
    }
}
