namespace command_box.Common
{
    public static class Paths
    {
        //Root path for the application folder
        public static string AppDataPath { get => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "command-box"); }

        // Directoies within the application folder
        public static string DataDir { get => Path.Combine(AppDataPath, "data"); }
        public static string SettingsDir { get => Path.Combine(AppDataPath,"settings"); }
        public static string ScriptsDir { get => Path.Combine(AppDataPath, "scripts"); }
        public static string LogsDir { get => Path.Combine(DataDir, "logs"); }
        public static string ErrorLogsDir { get => Path.Combine(LogsDir, "error logs"); }
        public static string ConsoleLogsDir { get => Path.Combine(LogsDir, "console logs"); }

        //Filepaths
        public static string Cache { get => Path.Combine(DataDir, "cache"); }
        public static string History { get => Path.Combine(DataDir, "history"); }
    }
}
