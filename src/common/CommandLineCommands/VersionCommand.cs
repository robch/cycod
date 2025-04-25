
public class VersionCommand : Command
{
    public VersionCommand()
    {
    }

    override public string GetCommandName()
    {
        return "version";
    }

    public override bool IsEmpty()
    {
        return false;
    }

    public Task<int> ExecuteAsync(bool interactive)
    {
        Console.WriteLine($"Version: {VersionInfo.GetVersion()}");
        return Task.FromResult(0);
    }
}