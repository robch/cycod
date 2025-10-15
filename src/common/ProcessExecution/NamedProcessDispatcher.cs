using System;
using System.Threading.Tasks;
using ShellExecution.Results;

namespace ProcessExecution
{
    /// <summary>
    /// Dispatches named process operations to the appropriate manager based on runtime type determination.
    /// Acts as a coordinator between shell processes and background processes.
    /// </summary>
    public static class NamedProcessDispatcher
    {
        /// <summary>
        /// Gets output from a named process (shell or background).
        /// </summary>
        /// <param name="name">The name of the process</param>
        /// <param name="outputType">Type of output to retrieve (stdout, stderr, all)</param>
        /// <param name="clearBuffer">Whether to clear the output buffer (background processes only)</param>
        /// <param name="waitTimeMs">Time to wait before retrieving output</param>
        /// <param name="waitPattern">Pattern to wait for in output</param>
        /// <returns>Process output as string</returns>
        public static async Task<string> GetOutputAsync(
            string name,
            string outputType = "all",
            bool clearBuffer = false,
            int waitTimeMs = 0,
            string waitPattern = "")
        {
            var nameEmpty = string.IsNullOrEmpty(name);
            if (nameEmpty) return "Error: Process name cannot be empty";

            ConsoleHelpers.WriteDebugLine($"[OUTPUT] Looking for process '{name}'");

            // Check if it's a shell process first
            var shell = NamedShellProcessManager.Instance.GetShell(name);
            var isShellProcess = shell != null;
            ConsoleHelpers.WriteDebugLine($"[OUTPUT] Process '{name}' is shell process: {isShellProcess}");
            if (isShellProcess)
                return await GetShellProcessOutput(name, outputType, waitTimeMs, waitPattern);

            // Check if it's a background process
            var isBackgroundProcess = NamedProcessManager.ProcessExists(name);
            ConsoleHelpers.WriteDebugLine($"[OUTPUT] Process '{name}' exists as background process: {isBackgroundProcess}");
            if (isBackgroundProcess)
                return await GetBackgroundProcessOutput(name, outputType, clearBuffer, waitTimeMs, waitPattern);

            ConsoleHelpers.WriteDebugLine($"[OUTPUT] Process '{name}' not found in either shell or background tracking");
            return $"Error: No shell or background process found with name '{name}'";
        }

        /// <summary>
        /// Sends input to a named process (shell or background).
        /// </summary>
        /// <param name="name">The name of the process</param>
        /// <param name="inputText">Text to send as input</param>
        /// <returns>Success message or error</returns>
        public static async Task<string> SendInputAsync(string name, string inputText)
        {
            var nameEmpty = string.IsNullOrEmpty(name);
            if (nameEmpty) return "Error: Process name cannot be empty";

            // Check if it's a shell process first
            var shell = NamedShellProcessManager.Instance.GetShell(name);
            var isShellProcess = shell != null;
            if (isShellProcess)
            {
                var success = await NamedShellProcessManager.Instance.SendInputToShellAsync(name, inputText);
                return success 
                    ? $"Input sent to shell process '{name}'"
                    : $"Failed to send input to shell process '{name}'";
            }

            // Check if it's a background process
            var isBackgroundProcess = NamedProcessManager.IsProcessRunning(name);
            if (isBackgroundProcess)
            {
                var success = NamedProcessManager.SendInputToProcess(name, inputText);
                return success 
                    ? $"Input sent to background process '{name}'"
                    : $"Failed to send input to background process '{name}'";
            }

            return $"Error: No shell or background process found with name '{name}'";
        }

        /// <summary>
        /// Terminates a named process.
        /// </summary>
        /// <param name="name">The name of the process</param>
        /// <param name="force">Whether to force termination</param>
        /// <returns>Termination result message</returns>
        public static string TerminateProcess(string name, bool force = false)
        {
            var nameEmpty = string.IsNullOrEmpty(name);
            if (nameEmpty) return "Error: Process name cannot be empty";

            // Check if it's a shell process first
            var shell = NamedShellProcessManager.Instance.GetShell(name);
            var isShellProcess = shell != null;
            if (isShellProcess)
            {
                var result = NamedShellProcessManager.Instance.TerminateShell(name, force);
                return result.Success 
                    ? $"Shell process '{name}' terminated"
                    : $"Failed to terminate shell process '{name}': {result.ErrorMessage}";
            }

            // Check if it's a background process
            var isBackgroundProcess = NamedProcessManager.IsProcessRunning(name);
            if (isBackgroundProcess)
            {
                var result = NamedProcessManager.TerminateProcess(name, force);
                return result.Success 
                    ? $"Background process '{name}' terminated"
                    : $"Failed to terminate background process '{name}': {result.ErrorMessage}";
            }

            return $"Error: No shell or background process found with name '{name}'";
        }

        #region Private Helper Methods

        private static async Task<string> GetShellProcessOutput(
            string name, 
            string outputType, 
            int waitTimeMs, 
            string waitPattern)
        {
            var shouldWaitForPattern = !string.IsNullOrEmpty(waitPattern);
            if (shouldWaitForPattern)
            {
                var timeoutMs = waitTimeMs > 0 ? waitTimeMs : -1;
                var patternOutput = await NamedShellProcessManager.Instance.WaitForShellOutputAsync(name, waitPattern, timeoutMs);
                var patternNotFound = patternOutput == null;
                if (patternNotFound) 
                    return $"Timeout waiting for pattern '{waitPattern}' in shell '{name}'";
            }

            var shouldWaitForTime = waitTimeMs > 0;
            if (shouldWaitForTime) await Task.Delay(waitTimeMs);

            // Get shell directly from the manager
            var shell = NamedShellProcessManager.Instance.GetShell(name);
            if (shell == null) return $"Error: Shell '{name}' not found";
            
            // Get output directly from the shell's buffer without executing commands
            string output;
            switch (outputType.ToLowerInvariant())
            {
                case "stdout":
                    output = shell.GetCurrentOutput();
                    break;
                case "stderr":
                    output = shell.GetCurrentError();
                    break;
                default:
                    output = shell.GetCurrentMergedOutput();
                    break;
            }
            return output ?? string.Empty;
        }

        private static async Task<string> GetBackgroundProcessOutput(
            string name, 
            string outputType, 
            bool clearBuffer, 
            int waitTimeMs, 
            string waitPattern)
        {
            var shouldWaitForPattern = !string.IsNullOrEmpty(waitPattern);
            if (shouldWaitForPattern)
            {
                var timeoutMs = waitTimeMs > 0 ? waitTimeMs : -1;
                var matched = NamedProcessManager.WaitForProcessOutput(name, waitPattern, timeoutMs, out _);
                var patternNotFound = !matched;
                if (patternNotFound) 
                    return $"Timeout waiting for pattern '{waitPattern}' in process '{name}'";
            }

            var shouldWaitForTime = waitTimeMs > 0;
            if (shouldWaitForTime) await Task.Delay(waitTimeMs);

            var outputResult = NamedProcessManager.GetProcessOutput(name, outputType, clearBuffer);
            return outputResult.ToAiString();
        }

        private static async Task<string> WaitForBackgroundProcessExit(string name, int timeoutMs)
        {
            var startTime = DateTime.Now;
            var timeout = timeoutMs > 0 ? TimeSpan.FromMilliseconds(timeoutMs) : TimeSpan.MaxValue;

            while (DateTime.Now - startTime < timeout)
            {
                var isStillRunning = NamedProcessManager.IsProcessRunning(name);
                if (!isStillRunning)
                    return $"Background process '{name}' exited";

                await Task.Delay(100);
            }

            return $"Timeout waiting for background process '{name}' to exit";
        }

        #endregion
    }
}