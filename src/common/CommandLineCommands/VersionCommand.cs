
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
            Console.WriteLine($"Version: {VersionInfo.GetVersion()}");
            return 0;
        });
    }
}