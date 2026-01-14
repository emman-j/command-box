using command_box.Enums;

namespace command_box.Interfaces
{
    public interface ICommand
    {
        string Name { get; }
        string Description { get; }
        string CommandPath { get; }
        string Usage { get; }
        CommandType Type { get; }
        void Execute(string[] args);
    }
}
