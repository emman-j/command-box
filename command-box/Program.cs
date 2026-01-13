using System.Text;

namespace command_box
{
    internal class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

            CommandsManager commandsManager = new CommandsManager(WriteLine);
            while (true)
            { 
                if (args.Length == 0)
                {
                    Write("> ");
                    string input = ReadLineWithAutoComplete(commandsManager);

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
                    case "cache":
                        commandsManager.Cache(args.Skip(1).ToArray());
                        args = Array.Empty<string>();
                        continue;
                    case "cls":
                    case "clear":
                        Console.Clear();
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

        private static void OnProcessExit(object? sender, EventArgs e)
        {

        }

        private static void WriteLine(string message = "")
        {
            Console.WriteLine("> " + message);
        }
        private static void Write(string message = "")
        {
            Console.Write(message);
        }
        private static string ReadLineWithAutoComplete(CommandsManager commandsmanager)
        {
            Commands commands = commandsmanager.Commands;
            StringBuilder input = new StringBuilder();
            int currentIndex = 0;

            // For cycling through matches
            int matchIndex = 0;
            string lastmatchInput = string.Empty;
            int promptLength = 2; // Length of "> "

            // For browsing history
            int historyIndex = commandsmanager.CommandsHistory.Count;
            int historyCount = commandsmanager.CommandsHistory.Count;
            string currentEdit = string.Empty;

            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(intercept: true);

                switch (key.Key)
                {
                    case ConsoleKey.Enter:
                        Console.WriteLine();

                        // Only add non-empty inputs to history
                        if (!string.IsNullOrWhiteSpace(input.ToString()))
                            commandsmanager.CommandsHistory.Add(input.ToString());

                        return input.ToString();
                    case ConsoleKey.Tab:
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
                        continue;
                    case ConsoleKey.Backspace:
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
                        continue;
                    case ConsoleKey.LeftArrow:
                        lastmatchInput = string.Empty;

                        if (currentIndex > 0)
                        {
                            currentIndex--;
                            Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                        }
                        continue;
                    case ConsoleKey.RightArrow:
                        lastmatchInput = string.Empty;

                        if (currentIndex < input.Length)
                        {
                            currentIndex++;
                            Console.SetCursorPosition(Console.CursorLeft + 1, Console.CursorTop);
                        }
                        continue;
                    case ConsoleKey.UpArrow:
                        if (historyCount == 0)
                            continue;

                        // Save current input before browsing history (only once)
                        if (historyIndex == historyCount)
                            currentEdit = input.ToString();

                        // Move back in history
                        if (historyIndex > 0)
                        {
                            historyIndex--;
                            ClearCurrentLine(promptLength);
                            input.Clear();
                            input.Append(commandsmanager.CommandsHistory[historyIndex]);
                            currentIndex = input.Length;
                            Write("> " + input.ToString());
                        }
                        continue;

                    case ConsoleKey.DownArrow:
                        if (historyCount == 0)
                            continue;

                        // Move forward in history
                        if (historyIndex < historyCount)
                        {
                            historyIndex++;
                            ClearCurrentLine(promptLength);
                            input.Clear();

                            if (historyIndex == historyCount)
                            {
                                // Restore the input that was being typed
                                input.Append(currentEdit);
                            }
                            else
                            {
                                input.Append(commandsmanager.CommandsHistory[historyIndex]);
                            }

                            currentIndex = input.Length;
                            Write("> " + input.ToString());
                        }
                        continue;
                    case ConsoleKey.Escape:
                        continue;
                    default:
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
                        continue;
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