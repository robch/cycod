
public class VersionCommand : Command
{
    public VersionCommand()
    {
    }

    public override string GetCommandName()
    {
        return "version";
    }

    public override bool IsEmpty()
    {
        return false;
    }

    public override async Task<object> ExecuteAsync(bool interactive)
    {
        return await Task.Run(() => 
        {
            ConsoleHelpers.WriteLine($"Version: {VersionInfo.GetVersion()}", overrideQuiet: true);
            return 0;
        });
    }
}