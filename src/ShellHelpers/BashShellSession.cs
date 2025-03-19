using System;
using System.Diagnostics;
using System.Text;

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
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };
    }

    protected override string WrapCommand(string command)
    {
        // Try to set UTF-8 locale if available, but don't fail if it's not
        return "{ export LC_ALL=C.UTF-8 2>/dev/null || export LC_ALL=en_US.UTF-8 2>/dev/null || true; " + command + " ; EC=$?; echo " + Marker + "$EC; }";
    }

    protected override int ParseExitCode(string markerOutput)
    {
        markerOutput = markerOutput.Replace(Marker, "").TrimStart();
        return int.TryParse(markerOutput, out int ec) ? ec : 0;
    }
}
