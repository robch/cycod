using System.Threading.Tasks;

namespace CycoDj.CommandLine;

public abstract class CycoDjCommand : Command
{
    // Common properties for instructions support
    public string? Instructions { get; set; }
    public bool UseBuiltInFunctions { get; set; } = false;
    public string? SaveChatHistory { get; set; }
    
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
    
    /// <summary>
    /// Apply instructions to output if --instructions was provided
    /// </summary>
    protected string ApplyInstructionsIfProvided(string output)
    {
        if (string.IsNullOrEmpty(Instructions))
        {
            return output;
        }
        
        return AiInstructionProcessor.ApplyInstructions(
            Instructions, 
            output, 
            UseBuiltInFunctions, 
            SaveChatHistory);
    }
}
