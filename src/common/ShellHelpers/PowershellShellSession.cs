using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class PowershellShellSession : ShellSession
{
    protected override ProcessStartInfo GetProcessStartInfo()
    {
        return new ProcessStartInfo
        {
            FileName = GetPowerShellProcessFileName(),
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
        return "try { " +
               "[Console]::OutputEncoding = [System.Text.Encoding]::UTF8; " + 
               "$global:LASTEXITCODE = 0; " +  // Initialize to 0 before command
               command + 
               " } catch { " +
               "if (-not $LASTEXITCODE) { $global:LASTEXITCODE = 1 } " + 
               "} finally { " +
               "if (-not $?) { if ($LASTEXITCODE -eq 0) { $global:LASTEXITCODE = 1 } }" +  // If $? is false but exit code is 0, set it to 1
               "Write-Output \"" + Marker + "$LASTEXITCODE\"" +  // Output marker with exit code
               " };";
    }

    protected override int ParseExitCode(string markerOutput)
    {
        markerOutput = markerOutput.Replace(Marker, "").TrimStart();
        return int.TryParse(markerOutput, out int ec) ? ec : 0;
    }
    
    private string GetPowerShellProcessFileName()
    {
        return _powerShellProcessFileName ??= CheckPowerShellProcessFileName();
    }


    private string CheckPowerShellProcessFileName()
    {
        var pwshOk = TryGetPwshVersion(out var _);
        if (pwshOk) return _powerShellCrossPlatProcessFileName;

        var onWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        if (onWindows) return _powerShellWindowsLegacyProcessFileName;

        throw new PlatformNotSupportedException("PowerShell Core (pwsh) is not installed. On Linux/macOS, PowerShell Core is required.");
    }

    private bool TryGetPwshVersion(out Version? version)
    {
        version = null;
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _powerShellCrossPlatProcessFileName,
                    Arguments = "-Version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            
            process.Start();
            process.WaitForExit();
            
            var ok = process.ExitCode == 0;
            if (!ok)
            {
                ConsoleHelpers.WriteDebugLine($"PowerShell version check failed (exit code= {process.ExitCode}).");
                return false;
            }

            var output = process.StandardOutput.ReadToEnd().Trim();
            ConsoleHelpers.WriteDebugLine($"PowerShell version output: {output}");

            var versionPartOfOutput = output
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .LastOrDefault();
            
            var versionOk = versionPartOfOutput != null;
            if (!versionOk)
            {
                ConsoleHelpers.WriteDebugLine("PowerShell version output parsing failed.");
                return false;
            }

            var versionParsed = Version.TryParse(versionPartOfOutput, out version);
            if (!versionParsed)
            {
                ConsoleHelpers.WriteDebugLine($"PowerShell version parsing failed: {versionPartOfOutput}");
                return false;
            }

            ConsoleHelpers.WriteDebugLine($"PowerShell version check: {ok}, version: {version}");
            return ok;
        }
        catch (Exception) // Ignore all exceptions, treat as not installed
        {
            ConsoleHelpers.WriteDebugLine("PowerShell version check failed.");
            return false;
        }
    }

    public override string Marker => "___POWERSHELL_COMMAND_DONE___";
    public static PowershellShellSession Instance => _instance ??= new PowershellShellSession();
    private static PowershellShellSession? _instance;
    private static string? _powerShellProcessFileName;
    private const string _powerShellCrossPlatProcessFileName = "pwsh";
    private const string _powerShellWindowsLegacyProcessFileName = "powershell";
}
