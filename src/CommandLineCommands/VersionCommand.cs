
class VersionCommand : Command
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

    public List<Task<int>> ExecuteAsync(bool interactive)
    {
        Console.WriteLine($"Version: {VersionInfo.GetVersion()}");
        return new List<Task<int>> { Task.FromResult(0) };
    }
}