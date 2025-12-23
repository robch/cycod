namespace CycoDev.ChatPipeline;

/// <summary>
/// Data passed to hooks during execution.
/// </summary>
public class HookData
{
    /// <summary>
    /// The name of the stage that triggered this hook.
    /// </summary>
    public string StageName { get; set; } = string.Empty;
    
    /// <summary>
    /// The hook point where this hook is executing.
    /// </summary>
    public HookPoint HookPoint { get; set; }
    
    /// <summary>
    /// Timestamp when the hook was invoked.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// The result from the stage execution (for post-stage hooks).
    /// </summary>
    public StageResult? StageResult { get; set; }
}

/// <summary>
/// Represents a behavior that can be injected at hook points.
/// </summary>
public interface IHookHandler
{
    /// <summary>
    /// The name of this hook.
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Priority for ordering multiple hooks at the same point.
    /// Lower numbers execute first.
    /// </summary>
    int Priority { get; }
    
    /// <summary>
    /// Handles the hook invocation.
    /// </summary>
    /// <param name="context">The chat context that can be observed/modified.</param>
    /// <param name="data">Additional data about the hook invocation.</param>
    /// <returns>Result indicating whether to continue, skip, or modify behavior.</returns>
    Task<HookResult> HandleAsync(ChatContext context, HookData data);
}
