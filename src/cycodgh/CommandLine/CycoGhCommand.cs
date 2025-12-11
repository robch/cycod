abstract class CycoGhCommand : Command
{
    public CycoGhCommand()
    {
        SaveOutput = string.Empty;
    }

    override public Task<object> ExecuteAsync(bool interactive)
    {
        throw new NotImplementedException("ExecuteAsync is not implemented for CycoGhCommand.");
    }

    public string SaveOutput;
}
