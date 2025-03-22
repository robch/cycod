using System.Diagnostics;
using System.Text;

public abstract class ShellSession
{
    protected Process? _process;
    protected readonly StringBuilder _stdoutBuffer = new StringBuilder();
    protected readonly StringBuilder _stderrBuffer = new StringBuilder();
    protected readonly object _lock = new object();

    // This marker will be used to indicate when a command has finished.
    public abstract string Marker { get; }

    // Each derived class supplies its own ProcessStartInfo.
    protected abstract ProcessStartInfo GetProcessStartInfo();

    // Wraps a user command so that it outputs the marker and exit code in a shell-specific format.
    protected abstract string WrapCommand(string command);

    // Parses the exit code from the marker output.
    protected abstract int ParseExitCode(string markerOutput);

    // Makes sure the process is running.
    protected void EnsureProcess()
    {
        if (_process != null && !_process.HasExited)
            return;

        var psi = GetProcessStartInfo();
        _process = new Process { StartInfo = psi, EnableRaisingEvents = true };

        _process.OutputDataReceived += (sender, args) =>
        {
            if (args.Data != null)
            {
                lock (_lock)
                {
                    if (ConsoleHelpers.IsVerbose() && args.Data.Contains(Marker) == false)
                    {
                        ConsoleHelpers.WriteLine(args.Data, ConsoleColor.DarkCyan);
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
                    if (ConsoleHelpers.IsVerbose() && args.Data.Contains(Marker) == false)
                    {
                        ConsoleHelpers.WriteErrorLine(args.Data);
                    }
                    _stderrBuffer.AppendLine(args.Data);
                }
            }
        };

        _process.Start();
        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();

        // Optionally write an initial marker to signal readiness.
        _process.StandardInput.WriteLine(WrapCommand("echo Ready"));
        _process.StandardInput.Flush();
        WaitForMarkerAsync(5000).Wait();
    }

    // Executes a command and waits for the marker.
    public async Task<(string stdout, string stderr, int exitCode)> ExecuteCommandAsync(string command, int timeoutMs = 60000)
    {
        EnsureProcess();
        lock (_lock)
        {
            _stdoutBuffer.Clear();
            _stderrBuffer.Clear();
        }

        string wrappedCommand = WrapCommand(command);
        _process!.StandardInput.WriteLine(wrappedCommand);
        _process.StandardInput.Flush();

        await WaitForMarkerAsync(timeoutMs);

        string stdout;
        lock (_lock)
        {
            stdout = _stdoutBuffer.ToString();
        }
        int markerIndex = stdout.LastIndexOf(Marker, StringComparison.Ordinal);
        if (markerIndex < 0)
        {
            throw new Exception("Marker not found in output.");
        }

        // Extract the part containing the marker and exit code.
        string markerOutput = stdout.Substring(markerIndex).Trim();
        int exitCode = ParseExitCode(markerOutput);

        // Everything before the marker is command output.
        stdout = stdout.Substring(0, markerIndex).TrimEnd();
        return (stdout, _stderrBuffer.ToString(), exitCode);
    }

    private async Task<string> WaitForMarkerAsync(int timeoutMs)
    {
        int waited = 0;
        while (waited < timeoutMs)
        {
            string currentOutput;
            lock (_lock)
            {
                currentOutput = _stdoutBuffer.ToString();
            }
            if (currentOutput.Contains(Marker))
            {
                return currentOutput;
            }
            await Task.Delay(100);
            waited += 100;
        }
        throw new TimeoutException("Command timed out.");
    }

    public void Shutdown()
    {
        if (_process != null && !_process.HasExited)
        {
            try { _process.Kill(); } catch { }
            _process.Dispose();
            _process = null;
        }
    }
}
