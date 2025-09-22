using Microsoft.Extensions.AI;
using System.Threading.Tasks;

/// <summary>
/// Command for compacting chat history files using AI-based summarization.
/// </summary>
public class ChatCompactCommand : Command
{
    public FileInfo? File { get; set; }
    public string? OutputFile { get; set; }
    public string? CompactionMode { get; set; }
    public int PreserveMessages { get; set; }
    public int MaxChatTokenTarget { get; set; }
    public int MaxPromptTokenTarget { get; set; }
    public int MaxToolTokenTarget { get; set; }
    
    // Default values - same as ChatCommand
    private const int DefaultMaxPromptTokenTarget = 50000;
    private const int DefaultMaxToolTokenTarget = 50000;
    private const int DefaultMaxChatTokenTarget = 160000;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ChatCompactCommand"/> class.
    /// </summary>
    public ChatCompactCommand()
    {
    }
    
    public override bool IsEmpty()
    {
        return File == null;
    }
    
    public override string GetCommandName()
    {
        return "chat compact";
    }
    
    /// <summary>
    /// Handles the command execution.
    /// </summary>
    public override async Task<object> ExecuteAsync(bool interactive)
    {
        // Load configuration defaults - same pattern as ChatCommand
        MaxPromptTokenTarget = ConfigStore.Instance.GetFromAnyScope(KnownSettings.AppMaxPromptTokens).AsInt(DefaultMaxPromptTokenTarget);
        MaxToolTokenTarget = ConfigStore.Instance.GetFromAnyScope(KnownSettings.AppMaxToolTokens).AsInt(DefaultMaxToolTokenTarget);
        MaxChatTokenTarget = ConfigStore.Instance.GetFromAnyScope(KnownSettings.AppMaxChatTokens).AsInt(DefaultMaxChatTokenTarget);
        
        // Load compaction settings if not explicitly set
        if (string.IsNullOrEmpty(CompactionMode))
        {
            CompactionMode = ConfigStore.Instance.GetFromAnyScope(KnownSettings.AppCompactionMode).AsString() ?? "full";
        }
        
        if (PreserveMessages == 0)
        {
            PreserveMessages = ConfigStore.Instance.GetFromAnyScope(KnownSettings.AppPreserveMessages).AsInt(ChatHistoryCompactionHelper.DefaultPreserveMessages);
        }
        
        if (File == null)
        {
            ConsoleHelpers.WriteErrorLine("Error: No file specified. Provide a chat history file as an argument or use --file option.");
            return 1;
        }
        
        if (!File.Exists)
        {
            ConsoleHelpers.WriteErrorLine($"Error: File not found: {File.FullName}");
            return 1;
        }
        
        // Determine output file
        string outputFilePath = !string.IsNullOrEmpty(OutputFile) 
            ? OutputFile 
            : Path.Combine(
                File.DirectoryName ?? "", 
                Path.GetFileNameWithoutExtension(File.Name) + "-compacted" + File.Extension);
        
        // Parse compaction mode
        var mode = ChatHistoryCompactionHelper.ParseCompactionMode(CompactionMode);
        
        // If mode is None, inform the user and exit
        if (mode == ChatHistoryCompactionHelper.CompactionMode.None)
        {
            ConsoleHelpers.WriteLine("Compaction mode set to 'none'. No compaction will be performed.");
            return 0;
        }
        
        ConsoleHelpers.WriteLine($"Compacting chat history: {File.FullName}");
        ConsoleHelpers.WriteLine($"  Mode: {mode}");
        ConsoleHelpers.WriteLine($"  Preserving last {PreserveMessages} messages");
        ConsoleHelpers.WriteLine($"  Target max chat tokens: {MaxChatTokenTarget}");
        ConsoleHelpers.WriteLine($"  Max prompt tokens: {MaxPromptTokenTarget}");
        ConsoleHelpers.WriteLine($"  Max tool tokens: {MaxToolTokenTarget}");
        
        try
        {
            // Load the chat history
            var messages = new List<ChatMessage>();
            messages.ReadChatHistoryFromFile(File.FullName, ChatHistoryDefaults.UseOpenAIFormat);
            
            int originalCount = messages.Count;
            
            // Perform the compaction
            var compacted = await ChatHistoryCompactionHelper.CompactChatHistoryAsync(
                messages, 
                mode,
                MaxChatTokenTarget,
                PreserveMessages,
                MaxPromptTokenTarget,
                MaxToolTokenTarget);
            
            if (!compacted)
            {
                ConsoleHelpers.WriteWarningLine("No compaction was performed. The history may already be under the token limit.");
                return 0;
            }
            
            // Save the compacted history
            messages.SaveChatHistoryToFile(outputFilePath, ChatHistoryDefaults.UseOpenAIFormat);
            
            ConsoleHelpers.WriteLine($"Compacted {originalCount} messages to {messages.Count} messages");
            ConsoleHelpers.WriteLine($"Saved to: {outputFilePath}");
            
            return 0;
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteErrorLine($"Error compacting chat history: {ex.Message}");
            return 1;
        }
    }
}