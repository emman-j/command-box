using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace command_box.Enums
{
    public enum CommandType
    {
        None,
        Internal,
        Batch,
        PowerShell,
        Shell,
        Python,
        Node,
    }
}
