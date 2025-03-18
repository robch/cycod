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
        return command + " & echo " + Marker + "%ERRORLEVEL%";
    }

    protected override int ParseExitCode(string markerOutput)
    {
        markerOutput = markerOutput.Replace(Marker, "").TrimStart();
        return int.TryParse(markerOutput, out int ec) ? ec : 0;
    }
}
