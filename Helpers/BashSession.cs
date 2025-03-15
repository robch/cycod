using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Implements a persistent bash session to execute commands with state maintained across calls.
/// </summary>
public class BashSession
{
    private Process? _process;
    private readonly StringBuilder _stdoutBuffer = new StringBuilder();
    private readonly StringBuilder _stderrBuffer = new StringBuilder();
    private readonly object _lock = new object();
    private readonly bool _verbose = false;
    private const string CommandDoneMarker = "___BEGIN_END_COMMAND_MARKER___";

    // Ensure only one session is active.
    private static BashSession? _instance;
    public static BashSession Instance => _instance ??= new BashSession();

    private BashSession() { }

    // Ensure the bash process is running.
    private void EnsureProcess()
    {
        if (_process != null && !_process.HasExited)
        {
            return;
        }
        var psi = new ProcessStartInfo
        {
            FileName = "bash",
            Arguments = "--norc --noprofile -i",
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        _process = new Process { StartInfo = psi, EnableRaisingEvents = true };
        _process.OutputDataReceived += (sender, args) =>
        {
            if (args.Data != null)
            {
                lock (_lock)
                {
                    if (_verbose && args.Data.Contains(CommandDoneMarker) == false)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        Console.WriteLine(args.Data);
                    }
                    _stdoutBuffer.AppendLine(args.Data);
                }
            }
        };
        _process.ErrorDataReceived += (sender, args) =>
        {
            if (args.Data != null)
            {
                lock (_lock)
                {
                    if (_verbose && args.Data.Contains(CommandDoneMarker) == false)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(args.Data);
                    }
                    _stderrBuffer.AppendLine(args.Data);
                }
            }
        };

        _process.Start();
        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();

        // Write an initial marker so that the shell is ready.
        _process.StandardInput.WriteLine("echo " + CommandDoneMarker + "0");
        WaitForMarkerAsync().Wait();
    }

    // Sends a command to the bash process and waits for the marker to appear.
    public async Task<(string stdout, string stderr, int exitCode)> ExecuteCommandAsync(string command, int timeoutMs = 60000)
    {
        EnsureProcess();
        if (_process == null) throw new InvalidOperationException("Bash process unavailable.");

        // Clear previous output.
        lock (_lock)
        {
            _stdoutBuffer.Clear();
            _stderrBuffer.Clear();
        }

        // Write the command, and then echo the marker with exit status.
        // Group the command in braces.
        _process.StandardInput.WriteLine("{ " + command);
        _process.StandardInput.WriteLine("EC=$?; echo " + CommandDoneMarker + "$EC; }");
        _process.StandardInput.Flush();

        // Wait and look for the marker.
        await WaitForMarkerAsync(timeoutMs);

        var stdout = _stdoutBuffer.ToString();
        var stderr = _stderrBuffer.ToString();

        int markerIndex = stdout.LastIndexOf(CommandDoneMarker, StringComparison.Ordinal);
        if (markerIndex < 0)
        {
            throw new Exception("Marker not found in bash output.");
        }
        // The marker is expected to be like "___COMMAND_DONE___<exitcode>"
        string trimmed = stdout.Substring(markerIndex).Trim();
        int exitCode = 0;
        if (trimmed.Length > CommandDoneMarker.Length)
        {
            int.TryParse(trimmed.Substring(CommandDoneMarker.Length), out exitCode);
        }

        // Everything before marker is the command output
        stdout = stdout.Substring(0, markerIndex).TrimEnd();
        return (stdout, stderr, exitCode);
    }

    private async Task<string> WaitForMarkerAsync(int timeoutMs = 60000)
    {
        int waited = 0;
        while (waited < timeoutMs)
        {
            string currentOutput;
            lock (_lock)
            {
                currentOutput = _stdoutBuffer.ToString();
            }
            if (currentOutput.Contains(CommandDoneMarker))
            {
                return currentOutput;
            }
            await Task.Delay(100);
            waited += 100;
        }
        throw new TimeoutException("Bash command timed out.");
    }

    // Shutdown the current bash process.
    public void Shutdown()
    {
        if (_process != null && !_process.HasExited)
        {
            try
            {
                _process.Kill();
            }
            catch { }
            _process.Dispose();
            _process = null;
        }
    }
}
