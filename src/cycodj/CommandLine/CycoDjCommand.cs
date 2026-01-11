using System.Threading.Tasks;

namespace CycoDj.CommandLine;

public abstract class CycoDjCommand : Command
{
    // Common properties for instructions support
    public string? Instructions { get; set; }
    public bool UseBuiltInFunctions { get; set; } = false;
    public string? SaveChatHistory { get; set; }
    
    // Common properties for time filtering
    public DateTime? After { get; set; }
    public DateTime? Before { get; set; }
    
    // Common properties for output
    public string? SaveOutput { get; set; }
    
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
    
    /// <summary>
    /// Save output to file if --save-output was provided
    /// Returns true if output was saved (command should not print to console)
    /// </summary>
    protected bool SaveOutputIfRequested(string output)
    {
        if (string.IsNullOrEmpty(SaveOutput))
        {
            return false;
        }
        
        // Just use SaveOutput directly - FileHelpers.GetFileNameFromTemplate doesn't do template expansion like we thought
        // For now, use the filename as-is
        var fileName = SaveOutput;
        
        // Write output to file
        File.WriteAllText(fileName, output);
        
        ConsoleHelpers.WriteLine($"Output saved to: {fileName}", ConsoleColor.Green);
        
        return true;
    }
}
