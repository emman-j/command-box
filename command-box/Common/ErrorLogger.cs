using command_box.Delegates;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace command_box.Common
{
    public sealed class ErrorLogger
    {
        private static readonly object _lock = new();
        private string _logFilePath;
        public List<Exception> Exceptions { get; set; } = new List<Exception>();
        public WriteLineDelegate WriteLine { get; set; } = Console.WriteLine;


        #region Singleton

        private static readonly ErrorLogger _instance;

        public static ErrorLogger Instance
        {
            get
            {
                return _instance;
            }
        }

        static ErrorLogger()
        {
            _instance = new ErrorLogger();
        }

        private ErrorLogger()
        {

        }

        #endregion

        public void Initialize(string logPath, WriteLineDelegate writeLine = null)
        {
            _logFilePath = logPath;
            _logFilePath = Path.Combine(logPath, $"err_{DateTime.Now:MMddyy}");
            if (writeLine != null)
                WriteLine = writeLine;
        }

        public static string GetLogEntry(string sender, Exception ex, string operation)
        {
            string exceptionMessage = ex.Message;
            exceptionMessage = string.IsNullOrWhiteSpace(ex.InnerException?.ToString())
                ? exceptionMessage
                : exceptionMessage + Environment.NewLine + ex.InnerException;

            // Split and indent each line of the stack trace
            string stackTrace = ex.StackTrace ?? "No stack trace available.";
            string indentedStackTrace = string.Join(
                Environment.NewLine,
                stackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                          .Select(line => "      " + line) // 6-space indent
            );

            string logEntry = string.IsNullOrEmpty(operation) ?
                $"[{sender}] {exceptionMessage}{Environment.NewLine}" :
                $"[{sender}] [{operation}] {exceptionMessage}{Environment.NewLine}";

            logEntry = logEntry + $"Stack Trace:{Environment.NewLine}{indentedStackTrace}{Environment.NewLine}{Environment.NewLine}";

            return logEntry;
        }
        public void LogException(Type sourceType, Exception ex, [CallerMemberName] string operation = "", bool showError = true)
        {
            string className = sourceType?.Name ?? "Unknown";
            LogExceptionInternal(className, ex, operation, showError);
        }

        public void LogException(object sender, Exception ex, [CallerMemberName] string operation = "", bool showError = true)
        {
            string className = sender?.GetType().Name ?? "Unknown";
            LogExceptionInternal(className, ex, operation, showError);
        }

        private void LogExceptionInternal(string sender, Exception ex, string operation, bool showError)
        {
            string log = GetLogEntry(sender, ex, operation);
            log = $"{DateTime.Now:hh:mm:ss} {log}";
            EnsureLogFilePath();
            AppendToLogFile(log);

            if (!showError) return;
            ShowErrorMessage(ex);
        }
        private void ShowErrorMessage(Exception ex)
        {
            string message = ex.Message;
            string exceptionType = ex.GetType().Name;
            string errorTitle = string.IsNullOrWhiteSpace(exceptionType) ? "CommandBox Error" : $"CommandBox {exceptionType} Error";
            string fullMessage = message + "\n\nSee logs for more detail.";

            WriteLine($"!!!!!!!!!!! {exceptionType} ERROR !!!!!!!!!!!");
            WriteLine(errorTitle);
            WriteLine(fullMessage);
        }

        private void AppendToLogFile(string message)
        {
            if (string.IsNullOrWhiteSpace(_logFilePath)) return;

            try
            {
                // Append each log immediately - survives crashes
                // Lock to prevent concurrent writes from corrupting the file
                lock (_lock)
                {
                    File.AppendAllLines(_logFilePath, new[] { message });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to write log: {ex.Message}");
                // Don't call OnErrorOccurred here to avoid infinite loop
            }
        }
        private void EnsureLogFilePath()
        {
            if (!Directory.Exists(Path.GetDirectoryName(_logFilePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(_logFilePath));

            if (!File.Exists(_logFilePath))
                File.Create(_logFilePath).Close();
            else
            {
                //_logFilePath = GetUniqueFileName(_logFilePath);
                //File.Create(_logFilePath).Close();
            }
        }
        private string GetUniqueFileName(string filePath)
        {
            string directory = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string extension = Path.GetExtension(filePath);

            int counter = 1;
            string newFilePath = filePath;

            while (File.Exists(newFilePath))
            {
                newFilePath = Path.Combine(directory, $"{fileName} ({counter}){extension}");
                counter++;
            }
            return newFilePath;
        }

    }
}
