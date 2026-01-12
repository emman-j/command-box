using command_box.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace command_box
{
    public class Command : ICommand
    {
        public string Name { get; }
        public string Description { get; }

        public string CommandPath { get; }

        public string Usage { get; }

        public CommandType Type { get; }

        public override string ToString() => $"{Name}";

        public Command(string name, string description, string path, string usage, CommandType type) 
        { 
            Name = name;
            Description = description;
            CommandPath = path;
            Usage = usage;
            Type = type;
        }

        public string Execute(string[] args)
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
                    psi.FileName = "python"; // Assumes python is in PATH
                    psi.Arguments = $"\"{CommandPath}\" {argString}";
                    break;
            }

            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;

            using Process process = Process.Start(psi);
            process.WaitForExit();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            if (!string.IsNullOrEmpty(error))
                output += Environment.NewLine + error;

            return output.Trim();
        }
    }
}
