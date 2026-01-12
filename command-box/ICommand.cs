using command_box.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace command_box
{
    public interface ICommand
    {
        string Name { get; }
        string Description { get; }
        string CommandPath { get; }
        string Usage { get; }
        CommandType Type { get; }
        string Execute(string[] args);
    }
}
