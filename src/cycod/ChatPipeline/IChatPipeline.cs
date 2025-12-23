namespace CycoDev.ChatPipeline;

/// <summary>
/// Orchestrates the execution of pipeline stages and hook invocations.
/// </summary>
public interface IChatPipeline
{
    /// <summary>
    /// Adds a stage to the pipeline.
    /// </summary>
    /// <param name="stage">The stage to add.</param>
    /// <returns>This pipeline for fluent configuration.</returns>
    IChatPipeline AddStage(IPipelineStage stage);
    
    /// <summary>
    /// Adds a hook at a specific point in the pipeline.
    /// </summary>
    /// <param name="hookPoint">The point where the hook should execute.</param>
    /// <param name="handler">The hook handler to execute.</param>
    /// <returns>This pipeline for fluent configuration.</returns>
    IChatPipeline AddHook(HookPoint hookPoint, IHookHandler handler);
    
    /// <summary>
    /// Executes the pipeline with the given context.
    /// </summary>
    /// <param name="context">The chat context to process.</param>
    /// <returns>The result of the conversation turn.</returns>
    Task<ChatResult> ExecuteAsync(ChatContext context);
}
