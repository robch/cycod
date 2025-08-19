using System.ComponentModel;

public class ShellCommandToolHelperFunctions
{
    [Description("Run commands in a bash shell (uses Git Bash on Windows). This persistent session maintains state across commands. " +
        "You can run commands such as Python, Node.js and Go, and install packages with winget (on Windows), pip, npm, or go. " +
        "If a command times out, the session is reset. Otherwise, returns the output of the command. " +
        "WARNING: This is a blocking operation that waits for command completion before returning.")]
    public async Task<string> RunBashCommand(
        [Description("The bash command to run.")] string command,
        [Description("Timeout in milliseconds for the command execution.")] int timeoutMs = 60000)
    {
        try
        {
            var result = await BashShellSession.Instance.ExecuteCommandAsync(command, timeoutMs);
            var output = result.MergedOutput;

            if (result.IsTimeout)
            {
                output = Environment.NewLine + $"<timed out and killed process - environment state has been reset>";
            }
            else if (result.ExitCode != 0)
            {
                output += Environment.NewLine + $"<exited with exit code {result.ExitCode}>";
            }

            return output;
        }
        catch (Exception ex)
        {
            return $"<exited with error: {ex.Message}>";
        }
    }

    [Description("Run commands in a cmd shell on Windows. This persistent session maintains state across commands. " +
        "You can run commands such as Python, Node.js and Go, and install packages with winget, pip, npm, or go. " +
        "If a command times out, the session is reset. Otherwise, returns the output of the command. " +
        "WARNING: This is a blocking operation that waits for command completion before returning.")]
    public async Task<string> RunCmdCommand(
        [Description("The cmd command to run.")] string command,
        [Description("Timeout in milliseconds for the command execution.")] int timeoutMs = 60000)
    {
        try
        {
            var result = await CmdShellSession.Instance.ExecuteCommandAsync(command, timeoutMs);
            var output = result.MergedOutput;

            if (result.IsTimeout)
            {
                output += Environment.NewLine + $"<timed out and killed process - environment state has been reset>";
            }
            else if (result.ExitCode != 0)
            {
                output += Environment.NewLine + $"<exited with exit code {result.ExitCode}>";
            }

            return output;
        }
        catch (Exception ex)
        {
            return $"<exited with error: {ex.Message}>";
        }
    }

    [Description("Run commands in a PowerShell shell. This persistent session maintains state across commands. " +
        "You can run commands such as Python, Node.js and Go, and install packages with winget (on Windows), pip, npm, or go. " +
        "If a command times out, the session is reset. Otherwise, returns the output of the command." +
        "WARNING: This is a blocking operation that waits for command completion before returning.")]
    public async Task<string> RunPowershellCommand(
        [Description("The PowerShell command to run.")] string command,
        [Description("Timeout in milliseconds for the command execution.")] int timeoutMs = 60000)
    {
        try
        {
            var result = await PowershellShellSession.Instance.ExecuteCommandAsync(command, timeoutMs);
            var output = result.MergedOutput;

            if (result.IsTimeout)
            {
                output += Environment.NewLine + $"<timed out and killed process - environment state has been reset>";
            }
            else if (result.ExitCode != 0)
            {
                output += Environment.NewLine + $"<exited with exit code {result.ExitCode}>";
            }

            return output;
        }
        catch (Exception ex)
        {
            return $"<exited with error: {ex.Message}>";
        }
    }
}

