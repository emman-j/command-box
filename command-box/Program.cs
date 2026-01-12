using System.Text;

namespace command_box
{
    internal class Program
    {

        static void Main()
        {
            CommandsManager commandsManager = new CommandsManager();

            bool running = true;

            while (running)
            { 
                if(!running)
                    break;

                string input = ReadLineWithAutoComplete(commandsManager.Commands);

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                string[] args = input.Split(' ');
                string command = args[0];

                switch (command.ToLower())
                {
                    case "-h":
                    case "help":
                        commandsManager.ShowHelp();
                        continue;
                    case "exit":
                    case "quit":
                    case "-q":
                        running = false;
                        continue;
                }

                string[] commandArgs = args.Skip(1).ToArray();
                commandsManager.ExecuteCommand(command, commandArgs);
            }
        }

        private static string ReadLineWithAutoComplete(Commands commands)
        {
            StringBuilder input = new StringBuilder();
            int currentIndex = 0;

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
                    string currentInput = input.ToString();
                    var matches = commands
                        .Where(c => c.Name.StartsWith(currentInput, StringComparison.OrdinalIgnoreCase))
                        .OrderBy(c => c.Name)
                        .ToList();

                    if (matches.Count == 1)
                    {
                        // Single match - auto-complete
                        ClearCurrentLine();
                        input.Clear();
                        input.Append(matches[0].Name);
                        Console.Write(input.ToString());
                        currentIndex = input.Length;
                    }
                    else if (matches.Count > 1)
                    {
                        // Multiple matches - show options
                        Console.WriteLine();
                        Console.WriteLine("Possible completions:");
                        foreach (var match in matches)
                        {
                            Console.WriteLine($"  {match.Name} - {match.Description}");
                        }
                        Console.Write(input.ToString());
                    }
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    if (input.Length > 0 && currentIndex > 0)
                    {
                        input.Remove(currentIndex - 1, 1);
                        currentIndex--;
                        ClearCurrentLine();
                        Console.Write(input.ToString());
                        // Move cursor back to current position
                        if (currentIndex < input.Length)
                        {
                            Console.SetCursorPosition(currentIndex, Console.CursorTop);
                        }
                    }
                }
                else if (key.Key == ConsoleKey.LeftArrow)
                {
                    if (currentIndex > 0)
                    {
                        currentIndex--;
                        Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                    }
                }
                else if (key.Key == ConsoleKey.RightArrow)
                {
                    if (currentIndex < input.Length)
                    {
                        currentIndex++;
                        Console.SetCursorPosition(Console.CursorLeft + 1, Console.CursorTop);
                    }
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    input.Insert(currentIndex, key.KeyChar);
                    currentIndex++;
                    ClearCurrentLine();
                    Console.Write(input.ToString());
                    // Move cursor back to current position
                    if (currentIndex < input.Length)
                    {
                        Console.SetCursorPosition(currentIndex, Console.CursorTop);
                    }
                }
            }
        }

        private static void ClearCurrentLine()
        {
            int currentLeft = Console.CursorLeft;
            int currentTop = Console.CursorTop;
            Console.SetCursorPosition(0, currentTop);
            Console.Write(new string(' ', Console.WindowWidth - 1));
            Console.SetCursorPosition(0, currentTop);
        }
    }
}