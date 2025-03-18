using System.Diagnostics;

public class CmdShellSession : ShellSession
{
    // Ensure only one session is active.
    private static CmdShellSession? _instance;
    public static CmdShellSession Instance => _instance ??= new CmdShellSession();

    public override string Marker => "___CMD_COMMAND_DONE___";

    protected override ProcessStartInfo GetProcessStartInfo()
    {
        return new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = "/K", // /K keeps the session alive
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
    }

    protected override string WrapCommand(string command)
    {
        // In cmd.exe, you might write:
        // command & echo <marker>%ERRORLEVEL%
        return command + " & echo " + Marker + "%ERRORLEVEL%";
    }

    protected override int ParseExitCode(string markerOutput)
    {
        // Parse the exit code from the marker output.
        if (markerOutput.Length > Marker.Length &&
            int.TryParse(markerOutput.Substring(Marker.Length), out int ec))
            return ec;
        return 0;
    }
}
