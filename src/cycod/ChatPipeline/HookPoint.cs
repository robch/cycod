namespace CycoDev.ChatPipeline;

/// <summary>
/// Defines the points in the pipeline where hooks can be registered.
/// </summary>
public enum HookPoint
{
    /// <summary>
    /// Before the pipeline starts executing.
    /// </summary>
    PrePipelineStart,
    
    /// <summary>
    /// After user input is captured, before AI processing.
    /// </summary>
    PostUserInput,
    
    /// <summary>
    /// Before user message is added to conversation.
    /// </summary>
    PreUserMessageAdd,
    
    /// <summary>
    /// After user message is added to conversation.
    /// </summary>
    PostUserMessageAdd,
    
    /// <summary>
    /// Before AI streaming begins.
    /// </summary>
    PreAIStreaming,
    
    /// <summary>
    /// After AI streaming completes, before adding message to conversation.
    /// </summary>
    PostAIStreaming,
    
    /// <summary>
    /// Before adding assistant message to conversation.
    /// </summary>
    PreMessageAdd,
    
    /// <summary>
    /// Before assistant message with tool calls is added to conversation.
    /// </summary>
    PreAssistantMessageWithToolCalls,
    
    /// <summary>
    /// After assistant message with tool calls is added to conversation.
    /// </summary>
    PostAssistantMessageWithToolCalls,
    
    /// <summary>
    /// After function calls are detected, before function execution.
    /// </summary>
    PostFunctionDetection,
    
    /// <summary>
    /// Before function calls are executed.
    /// </summary>
    PreFunctionExecution,
    
    /// <summary>
    /// After function calls are executed, before adding results to conversation.
    /// </summary>
    PostFunctionExecution,
    
    /// <summary>
    /// Before function results are added to conversation.
    /// </summary>
    PreFunctionResultAdd,
    
    /// <summary>
    /// Before tool results are added to conversation.
    /// </summary>
    PreToolResultsAdd,
    
    /// <summary>
    /// After tool results are added to conversation.
    /// </summary>
    PostToolResultsAdd,
    
    /// <summary>
    /// Before injected content (additional user content) is added to conversation.
    /// </summary>
    PreInjectedContentAdd,
    
    /// <summary>
    /// After injected content (additional user content) is added to conversation.
    /// </summary>
    PostInjectedContentAdd,
    
    /// <summary>
    /// After messages are added to conversation.
    /// </summary>
    PostMessageAdd,
    
    /// <summary>
    /// Before loop continues to next iteration.
    /// </summary>
    PreLoopContinue,
    
    /// <summary>
    /// After loop iteration completes.
    /// </summary>
    PostLoopIteration,
    
    /// <summary>
    /// After the pipeline completes executing.
    /// </summary>
    PostPipelineComplete
}
