using Microsoft.Extensions.AI;

namespace CycoDev.ChatPipeline;

/// <summary>
/// Represents the result of a stage execution.
/// </summary>
public class StageResult
{
    /// <summary>
    /// Whether the stage executed successfully.
    /// </summary>
    public bool Success { get; set; } = true;
    
    /// <summary>
    /// Whether the pipeline should continue to the next stage.
    /// </summary>
    public bool ShouldContinue { get; set; } = true;
    
    /// <summary>
    /// Whether the pipeline should exit the conversation loop.
    /// </summary>
    public bool ShouldExitLoop { get; set; } = false;
    
    /// <summary>
    /// Optional error message if the stage failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Optional exception if the stage failed.
    /// </summary>
    public Exception? Exception { get; set; }
    
    public static StageResult Continue() => new StageResult { ShouldContinue = true };
    public static StageResult ExitLoop() => new StageResult { ShouldExitLoop = true };
    public static StageResult Skip() => new StageResult { ShouldContinue = false };
    public static StageResult Error(string message, Exception? ex = null) => new StageResult 
    { 
        Success = false, 
        ErrorMessage = message,
        Exception = ex
    };
}

/// <summary>
/// Represents the result of a hook execution.
/// </summary>
public class HookResult
{
    /// <summary>
    /// Whether the hook executed successfully.
    /// </summary>
    public bool Success { get; set; } = true;
    
    /// <summary>
    /// Whether the pipeline should skip the upcoming stage.
    /// </summary>
    public bool ShouldSkipStage { get; set; } = false;
    
    /// <summary>
    /// Whether the pipeline should exit the conversation loop.
    /// </summary>
    public bool ShouldExitLoop { get; set; } = false;
    
    /// <summary>
    /// Whether the hook modified the context in a way that requires re-evaluation.
    /// </summary>
    public bool ModifiedContext { get; set; } = false;
    
    /// <summary>
    /// Optional error message if the hook failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    public static HookResult Continue() => new HookResult();
    public static HookResult SkipStage() => new HookResult { ShouldSkipStage = true };
    public static HookResult ExitLoop() => new HookResult { ShouldExitLoop = true };
    public static HookResult Modified() => new HookResult { ModifiedContext = true };
}

/// <summary>
/// Represents the final result of pipeline execution.
/// </summary>
public class ChatResult
{
    /// <summary>
    /// The final content returned from the conversation turn.
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether the conversation completed successfully.
    /// </summary>
    public bool Success { get; set; } = true;
    
    /// <summary>
    /// Whether the conversation was interrupted by the user.
    /// </summary>
    public bool WasInterrupted { get; set; } = false;
    
    /// <summary>
    /// Optional error message if the conversation failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    public static ChatResult Completed(string content) => new ChatResult { Content = content };
    public static ChatResult Interrupted() => new ChatResult { WasInterrupted = true };
    public static ChatResult Failed(string message) => new ChatResult { Success = false, ErrorMessage = message };
}
