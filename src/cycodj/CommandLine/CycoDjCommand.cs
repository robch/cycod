using System.Threading.Tasks;

namespace CycoDj.CommandLine;

public abstract class CycoDjCommand : Command
{
    public override bool IsEmpty()
    {
        return false;
    }

    public override string GetCommandName()
    {
        return GetType().Name.Replace("Command", "").ToLowerInvariant();
    }

    public override async Task<object> ExecuteAsync(bool interactive)
    {
        var result = await ExecuteAsync();
        return result;
    }

    public abstract Task<int> ExecuteAsync();
}
