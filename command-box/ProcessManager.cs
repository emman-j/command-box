using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace command_box
{
    public class ProcessManager
    {
        private WriteLineDelegate _writeLine;
        private Process _process;
        private bool _isRunning = false;
        public delegate void WriteLineDelegate(string message);

        public string LastOutput { get; private set; }

        public ProcessManager(WriteLineDelegate writeLine)
        {
            _writeLine = writeLine;
        }
        public void RunExternalApp(string exePath, string arguments = "", bool synchronousRead = false)
        {
            if (_isRunning)
            {
                _writeLine($"[{Path.GetFileNameWithoutExtension(_process.StartInfo.FileName)}] Process is running. End process first to start [{Path.GetFileNameWithoutExtension(exePath)}]");
                return;
            }

            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = arguments,   
                    RedirectStandardInput = true,   // Allow sending input
                    RedirectStandardOutput = true,  // Capture the output
                    RedirectStandardError = true,   // Capture any error output
                    UseShellExecute = false,        // Required for redirection
                    CreateNoWindow = true           // Do not show the console window
                }
            };

            _process.OutputDataReceived += new DataReceivedEventHandler(ProcessOutputHandler);
            _process.ErrorDataReceived += new DataReceivedEventHandler(ProcessErrorHandler);

            // Start the process
            _process.Start();
            _isRunning = true;

            // Start reading output and error streams asynchronously
            if (!synchronousRead)
            {
                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();
            }
        }
        public void StopExternalApp()
        {
            if (_process == null || _process.HasExited)
            {
                _writeLine("No process is currently running.");
                _isRunning = false;
                return;
            }

            try
            {
                _writeLine($"Stopping [{Path.GetFileNameWithoutExtension(_process.StartInfo.FileName)}]...");

                // Try to close gracefully first
                _process.CloseMainWindow();

                // Give it some time to exit gracefully
                if (!_process.WaitForExit(3000)) // wait up to 3 seconds
                {
                    _writeLine("Process did not exit in time — killing...");
                    _process.Kill(); // true = kill entire process tree
                    _process.WaitForExit();
                }

                _writeLine("Process stopped successfully.");
            }
            catch (Exception ex)
            {
                _writeLine($"Error stopping process: {ex.Message}");
            }
            finally
            {
                _isRunning = false;
                _process?.Dispose();
                _process = null;
            }
        }

        private void ProcessOutputHandler(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                LastOutput = e.Data;
                _writeLine("> " + e.Data);
            }
        }
        private void ProcessErrorHandler(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                LastOutput = e.Data;
                _writeLine("> ERROR: " + e.Data);
            }
        }
        public bool IsProcessRunning()
        {
            return _process != null && !_process.HasExited;
        }

        // Stop the external app
        public void StopProcess()
        {
            if (_process != null && !_process.HasExited)
            {
                _process.Kill();
            }
        }
    }
}
