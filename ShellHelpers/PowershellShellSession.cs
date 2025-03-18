using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public class PowershellShellSession : ShellSession
{
    // Ensure only one session is active.
    private static PowershellShellSession? _instance;
    public static PowershellShellSession Instance => _instance ??= new PowershellShellSession();

    public override string Marker => "___POWERSHELL_COMMAND_DONE___";

    protected override ProcessStartInfo GetProcessStartInfo()
    {
        return new ProcessStartInfo
        {
            FileName = "powershell",
            Arguments = "-NoExit -Command -", // Keeps the session open.
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
    }

    protected override string WrapCommand(string command)
    {
        // In PowerShell, you might do:
        // command ; echo "<marker>$LASTEXITCODE"
        return "try { " + command + " } catch { if (-not $LASTEXITCODE) { $LASTEXITCODE = 1 } } ; Write-Output \"" + Marker + "$LASTEXITCODE\"";
    }

    protected override int ParseExitCode(string markerOutput)
    {
        if (markerOutput.Length > Marker.Length &&
            int.TryParse(markerOutput.Substring(Marker.Length).Replace("\"", ""), out int ec))
            return ec;
        return 0;
    }
}