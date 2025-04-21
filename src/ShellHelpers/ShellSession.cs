using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

public abstract class ShellSession
{
    public ShellSession()
    {
        lock (_sessions)
        {
            _sessions.Add(this);
        }
    }

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
                    var line = args.Data.TrimEnd(new char[] { '\r', '\n' });
                    if (ConsoleHelpers.IsVerbose())
                    {
                        ConsoleHelpers.WriteLine(line, ConsoleColor.DarkMagenta);
                    }
                    _stdoutBuffer.AppendLine(line);
                    _mergedBuffer.AppendLine(line);
                }
            }
        };

        _process.ErrorDataReceived += (sender, args) =>
        {
            if (args.Data != null)
            {
                lock (_lock)
                {
                    var line = args.Data.TrimEnd(new char[] { '\r', '\n' });
                    if (ConsoleHelpers.IsVerbose())
                    {
                        ConsoleHelpers.WriteErrorLine(line);
                    }
                    _stderrBuffer.AppendLine(line);
                    _mergedBuffer.AppendLine(line);
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
    public async Task<(string stdout, string stderr, string merged, int exitCode)> ExecuteCommandAsync(string command, int timeoutMs = 10000)
    {
        EnsureProcess();
        lock (_lock)
        {
            _stdoutBuffer.Clear();
            _stderrBuffer.Clear();
            _mergedBuffer.Clear();
        }

        var wrappedCommand = WrapCommand(command);
        _process!.StandardInput.WriteLine(wrappedCommand);
        _process.StandardInput.Flush();

        await WaitForMarkerAsync(timeoutMs);

        var exitCode = ParseExitCode();
        var stdOut = StripMarker(_stdoutBuffer);
        var stdErr = StripMarker(_stderrBuffer);
        var merged = StripMarker(_mergedBuffer);

        return (stdOut, stdErr, merged, exitCode);
    }

    private int ParseExitCode()
    {
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

        var markerOutput = stdout.Substring(markerIndex).Trim();
        return ParseExitCode(markerOutput);
    }

    private string StripMarker(StringBuilder sb)
    {
        string output;
        lock (_lock)
        {
            output = sb.ToString();
        }

        var lines = output
            .Split('\n', StringSplitOptions.None)
            .Select(line => line.TrimEnd(new char[] { '\r', '\n' }))
            .Where(line => !line.Contains(Marker));

        return string.Join("\n", lines);
    }

    private async Task<string> WaitForMarkerAsync(int timeoutMs)
    {
        var pattern = $@"{Marker}\s*(-?\d+)";
        var regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.Multiline);

        int waited = 0;
        while (waited < timeoutMs)
        {
            string currentOutput;
            lock (_lock)
            {
                currentOutput = _stdoutBuffer.ToString();
            }
            if (regex.IsMatch(currentOutput))
            {
                return currentOutput;
            }
            await Task.Delay(100);
            waited += 100;
        }
        throw new TimeoutException($"<waiting {timeoutMs}ms for command to finish>\n{_mergedBuffer.ToString()}");
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

    public static void ShutdownAll()
    {
        lock (_sessions)
        {
            foreach (var session in _sessions)
            {
                session.Shutdown();
            }
        }
    }

    protected Process? _process;
    protected readonly StringBuilder _stdoutBuffer = new StringBuilder();
    protected readonly StringBuilder _stderrBuffer = new StringBuilder();
    protected readonly StringBuilder _mergedBuffer = new StringBuilder();
    protected readonly object _lock = new object();

    private static List<ShellSession> _sessions = new List<ShellSession>();
}
