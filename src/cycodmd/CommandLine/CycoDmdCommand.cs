abstract class CycoDmdCommand : Command
{
    public CycoDmdCommand()
    {
        InstructionsList = new List<string>();
        UseBuiltInFunctions = false;
        SaveChatHistory = string.Empty;
        SaveOutput = string.Empty;
    }

    override public Task<object> ExecuteAsync(bool interactive)
    {
        throw new NotImplementedException("ExecuteAsync is not implemented for CycoDmdCommand.");
    }

    public List<string> InstructionsList;
    public bool UseBuiltInFunctions;
    public string SaveChatHistory;

    public string SaveOutput;
}
