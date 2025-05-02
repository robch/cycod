using System;
using System.Diagnostics;
using System.Text;

public class CmdShellSession : ShellSession
{
    protected override ProcessStartInfo GetProcessStartInfo()
    {
        return new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = "/K chcp 65001", // /K keeps the session alive, chcp 65001 sets UTF-8 code page
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
        return command + " & echo " + Marker + "%ERRORLEVEL%";
    }

    protected override int ParseExitCode(string markerOutput)
    {
        markerOutput = markerOutput.Replace(Marker, "").TrimStart();
        return int.TryParse(markerOutput, out int ec) ? ec : 0;
    }

    public override string Marker => "___CMD_COMMAND_DONE___";
    public static CmdShellSession Instance => _instance ??= new CmdShellSession();
    private static CmdShellSession? _instance;
}
