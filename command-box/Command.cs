using System.Diagnostics;
using command_box.Enums;
using Newtonsoft.Json;

namespace command_box
{
    public class Command : ICommand
    {
        public string Name { get; }
        public string Description { get; }
        public string CommandPath { get; }
        public string Usage { get; }
        public CommandType Type { get; }
        
        [JsonIgnore]
        public Action<string[]> Action { get; set; }
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
            switch (Type)
            { 
                case CommandType.None:
                    Action?.Invoke(args);
                    break;
                default:
                    RunProcess(args);
                    break;
            }
        }

        private void RunProcess(string[] args)
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
                process.WaitForExit();
            }
        }
    }
}
