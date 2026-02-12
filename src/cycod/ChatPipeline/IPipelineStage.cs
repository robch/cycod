namespace CycoDev.ChatPipeline;

/// <summary>
/// Represents one step in the pipeline that processes the chat context.
/// </summary>
public interface IPipelineStage
{
    /// <summary>
    /// The name of this stage.
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Hook point that fires before this stage executes.
    /// </summary>
    HookPoint PreHookPoint { get; }
    
    /// <summary>
    /// Hook point that fires after this stage executes.
    /// </summary>
    HookPoint PostHookPoint { get; }
    
    /// <summary>
    /// Executes this stage of the pipeline.
    /// </summary>
    /// <param name="context">The chat context to process.</param>
    /// <returns>Result indicating whether to continue, skip, or exit.</returns>
    Task<StageResult> ExecuteAsync(ChatContext context);
}
