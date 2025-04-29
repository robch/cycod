using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Text;
using System.Threading;

static class ProcessHelpers
{
    public static async Task<(string, int)> RunShellCommandAsync(string script, string shell, int timeout = int.MaxValue)
    {
        GetShellProcessNameAndArgs(script, shell, out var processName, out var arguments);
        return await RunProcessAsync(processName, arguments, timeout);
    }

    public static async Task<(string, int)> RunProcessAsync(string processName, string arguments, int timeout = int.MaxValue)
    {
        var startInfo = new ProcessStartInfo()
        {
            FileName = processName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        var sbOut = new StringBuilder();
        var sbErr = new StringBuilder();
        var sbMerged = new StringBuilder();

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var outDoneSignal = new ManualResetEvent(false);
        var errDoneSignal = new ManualResetEvent(false);
        process.OutputDataReceived += (sender, e) => AppendLineOrSignal(e.Data, sbOut, sbMerged, outDoneSignal);
        process.ErrorDataReceived += (sender, e) => AppendLineOrSignal(e.Data, sbErr, sbMerged, errDoneSignal);
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        var exitedNotKilled = WaitForExit(process, timeout);
        if (exitedNotKilled)
        {
            await Task.Run(() => {
                outDoneSignal.WaitOne();
                errDoneSignal.WaitOne();
            });
        }

        return (sbMerged.ToString(), process.ExitCode);
    }

    private static void GetShellProcessNameAndArgs(string script, string shell, out string processName, out string arguments)
    {
        switch (shell)
        {
            case "cmd":
                processName = "cmd.exe";
                arguments = $"/c {script.Replace("\n", " & ")}";
                break;
            case "bash":
                processName = "bash";
                arguments = $"-c \"{script}\"";
                break;
            case "powershell":
                processName = "powershell.exe";
                arguments = $"-Command \"{script}\"";
                break;
            default:
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    processName = "cmd.exe";
                    arguments = $"/c {script.Replace("\n", " & ")}";
                }
                else
                {
                    processName = "bash";
                    arguments = $"-c \"{script}\"";
                }
                break;
        }
    }

    private static void AppendLineOrSignal(string text, StringBuilder sb1, StringBuilder sb2, ManualResetEvent signal)
    {
        if (text != null)
        {
            sb1.AppendLine(text);
            sb2.AppendLine(text);
        }
        else
        {
            signal.Set();
        }
    }

    private static bool WaitForExit(Process process, int timeout)
    {
        var completed = process.WaitForExit(timeout);
        if (!completed)
        {
            var name = process.ProcessName;
            var message = $"Timedout! Stopping process ({name})...";
            ConsoleHelpers.PrintDebugLine(message);

            try
            {
                message = $"Timedout! Sending <ctrl-c> ...";
                ConsoleHelpers.PrintDebugLine(message);

                process.StandardInput.WriteLine("\x3"); // try ctrl-c first
                process.StandardInput.Close();

                ConsoleHelpers.PrintDebugLine($"{message} Sent!");

                completed = process.WaitForExit(200);
            }
            catch (Exception ex)
            {
                ConsoleHelpers.PrintDebugLine($"Timedout! Failed to send <ctrl-c>: {ex.Message}");
            }

            message = "Timedout! Sent <ctrl-c>" + (completed ? "; stopped" : "; trying Kill()");
            ConsoleHelpers.PrintDebugLine(message);

            if (!completed)
            {
                message = $"Timedout! Killing process ({name})...";
                ConsoleHelpers.PrintDebugLine(message);

                process.Kill();

                message = process.HasExited ? $"{message} Done." : $"{message} Failed!";
                ConsoleHelpers.PrintDebugLine(message);
            }
        }

        return completed;
    }
}
