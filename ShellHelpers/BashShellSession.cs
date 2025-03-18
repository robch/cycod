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
        return "{ " + command + " ; EC=$?; echo " + Marker + "$EC; }";
    }

    protected override int ParseExitCode(string markerOutput)
    {
        markerOutput = markerOutput.Replace(Marker, "").TrimStart();
        return int.TryParse(markerOutput, out int ec) ? ec : 0;
    }
}
