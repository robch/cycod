using System.Diagnostics;

public class BashShellSession : ShellSession
{
    // Ensure only one session is active.
    private static BashShellSession? _instance;
    public static BashShellSession Instance => _instance ??= new BashShellSession();

    public override string Marker => "___BEGIN_END_COMMAND_MARKER___";

    protected override ProcessStartInfo GetProcessStartInfo()
    {
        return new ProcessStartInfo
        {
            FileName = "bash",
            Arguments = "--norc --noprofile -i",
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
    }

    protected override string WrapCommand(string command)
    {
        // Wraps the command to output the marker and exit code.
        // In bash, you can use: { command; EC=$?; echo <marker>$EC; }
        return "{ " + command + " ; EC=$?; echo " + Marker + "$EC; }";
    }

    protected override int ParseExitCode(string markerOutput)
    {
        // Assumes markerOutput is of the form "___BEGIN_END_COMMAND_MARKER___<exitCode>"
        if (markerOutput.Length > Marker.Length &&
            int.TryParse(markerOutput.Substring(Marker.Length), out int ec))
            return ec;
        return 0;
    }
}
