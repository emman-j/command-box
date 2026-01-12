using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace command_box.Global
{
    public static class Paths
    {
        //Root path for the application folder
        public static string AppDataPath { get => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "command-box"); }
        public static string DataDir { get => Path.Combine(AppDataPath, "data"); }
        public static string SettingDir { get => Path.Combine(AppDataPath,"settings"); }
        public static string ScriptsDir { get => Path.Combine(AppDataPath, "scripts"); }
        public static string LogsDir { get => Path.Combine(DataDir, "logs"); }
        public static string ErrorLogsDir { get => Path.Combine(LogsDir, "error logs"); }
        public static string ConsoleLogsDir { get => Path.Combine(LogsDir, "console logs"); }
    }
}
