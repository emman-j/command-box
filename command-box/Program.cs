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

                string[] args = Console.ReadLine().Split(' ');
                string command = args[0];

                switch(command.ToLower())
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
    }
}
