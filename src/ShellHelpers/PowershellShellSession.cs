using System;
using System.Diagnostics;
using System.IO;
using System.Text;
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
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };
    }

    protected override string WrapCommand(string command)
    {
        return "try { [Console]::OutputEncoding = [System.Text.Encoding]::UTF8; " + command + " } catch { if (-not $LASTEXITCODE) { $LASTEXITCODE = 1 } } ; Write-Output ('" + Marker + "', $LASTEXITCODE) ;";
    }

    protected override int ParseExitCode(string markerOutput)
    {
        markerOutput = markerOutput.Replace(Marker, "").TrimStart();
        return int.TryParse(markerOutput, out int ec) ? ec : 0;
    }
}
