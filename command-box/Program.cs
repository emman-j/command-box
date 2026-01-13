using System.Text;

namespace command_box
{
    internal class Program
    {
        static void Main(string[] args)
        {
            CommandsManager commandsManager = new CommandsManager(WriteLine);
            while (true)
            { 
                if (args.Length == 0)
                {
                    Write("> ");
                    string input = ReadLineWithAutoComplete(commandsManager.Commands);

                    if (string.IsNullOrWhiteSpace(input))
                        continue;

                    args = input.Split(' ');
                }

                string command = args[0];

                switch (command.ToLower())
                {
                    case "-h":
                    case "help":
                        commandsManager.ShowHelp();
                        args = Array.Empty<string>();
                        continue;
                    case "dir":
                        commandsManager.ShowDirectories();
                        args = Array.Empty<string>();
                        continue;
                    case "exit":
                    case "quit":
                    case "-q":
                        return;
                }

                string[] commandArgs = args.Skip(1).ToArray();
                commandsManager.ExecuteCommand(command, commandArgs);
                args = Array.Empty<string>();
            }
        }
        private static void WriteLine(string message = "")
        {
            Console.WriteLine("> " + message);
        }
        private static void Write(string message = "")
        {
            Console.Write(message);
        }
        private static string ReadLineWithAutoComplete(Commands commands)
        {
            StringBuilder input = new StringBuilder();
            int currentIndex = 0;
            int matchIndex = 0;
            string lastmatchInput = string.Empty;
            int promptLength = 2; // Length of "> "

            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    return input.ToString();
                }
                else if (key.Key == ConsoleKey.Tab)
                {
                    string currentInput = string.IsNullOrWhiteSpace(lastmatchInput) ? input.ToString() : lastmatchInput;

                    if (string.IsNullOrWhiteSpace(lastmatchInput))
                        lastmatchInput = currentInput;

                    var matches = commands
                        .Where(c => c.Name.StartsWith(currentInput, StringComparison.OrdinalIgnoreCase))
                        .OrderBy(c => c.Name)
                        .ToList();

                    if (matches.Count == 0)
                        continue;

                    ClearCurrentLine(promptLength);
                    input.Clear();
                    if (matches.Count == 1)
                    {
                        // Single match - auto-complete
                        input.Append(matches[0].Name);
                        currentIndex = input.Length;
                    }
                    else if (matches.Count > 1)
                    {
                        // Multiple matches - cycle through

                        if (matchIndex >= matches.Count)
                            matchIndex = 0;

                        input.Append(matches[matchIndex].Name);
                        currentIndex = input.Length;
                        matchIndex++;
                    }
                    Write("> " + input.ToString());
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    lastmatchInput = string.Empty;

                    if (input.Length > 0 && currentIndex > 0)
                    {
                        input.Remove(currentIndex - 1, 1);
                        currentIndex--;
                        ClearCurrentLine(promptLength);
                        Write("> " + input.ToString());
                        // Move cursor back to current position
                        if (currentIndex < input.Length)
                        {
                            Console.SetCursorPosition(promptLength + currentIndex, Console.CursorTop);
                        }
                    }
                }
                else if (key.Key == ConsoleKey.LeftArrow)
                {
                    lastmatchInput = string.Empty;

                    if (currentIndex > 0)
                    {
                        currentIndex--;
                        Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                    }
                }
                else if (key.Key == ConsoleKey.RightArrow)
                {
                    lastmatchInput = string.Empty;

                    if (currentIndex < input.Length)
                    {
                        currentIndex++;
                        Console.SetCursorPosition(Console.CursorLeft + 1, Console.CursorTop);
                    }
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    lastmatchInput = string.Empty;

                    input.Insert(currentIndex, key.KeyChar);
                    currentIndex++;
                    ClearCurrentLine(promptLength);
                    Write("> " + input.ToString());

                    // Move cursor back to current position
                    if (currentIndex < input.Length)
                    {
                        Console.SetCursorPosition(promptLength + currentIndex, Console.CursorTop);
                    }
                }
            }
        }

        private static void ClearCurrentLine(int promptLength = 0)
        {
            int currentTop = Console.CursorTop;
            Console.SetCursorPosition(0, currentTop);
            Write(new string(' ', Console.WindowWidth - 1));
            Console.SetCursorPosition(0, currentTop);
        }
    }
}