using CycoDev.ChatPipeline.Hooks;

namespace CycoDev.ChatPipeline;

/// <summary>
/// Extension methods for ChatPipeline to provide semantic, fluent hook registration.
/// </summary>
public static class ChatPipelineExtensions
{
    #region User Input Hooks
    
    // PostUserInput (existing hook point)
    public static IChatPipeline AddPostUserInput(this IChatPipeline pipeline, IHookHandler handler)
        => pipeline.AddHook(HookPoint.PostUserInput, handler);
    
    public static IChatPipeline AddPostUserInput(this IChatPipeline pipeline, Func<ChatContext, HookData, Task<HookResult>> handler)
        => pipeline.AddHook(HookPoint.PostUserInput, new LambdaHookHandler(handler));
    
    public static IChatPipeline AddPostUserInput(this IChatPipeline pipeline, Action<ChatContext, HookData> handler)
        => pipeline.AddHook(HookPoint.PostUserInput, new ActionHookHandler(handler));
    
    public static IChatPipeline AddPostUserInput(this IChatPipeline pipeline, Func<ChatContext, Task> handler)
        => pipeline.AddHook(HookPoint.PostUserInput, new SimpleLambdaHookHandler(handler));
    
    public static IChatPipeline AddPostUserInput(this IChatPipeline pipeline, Action<ChatContext> handler)
        => pipeline.AddHook(HookPoint.PostUserInput, new SimpleActionHookHandler(handler));
    
    // PreUserMessageAdd
    public static IChatPipeline AddPreUserMessageAdd(this IChatPipeline pipeline, IHookHandler handler)
        => pipeline.AddHook(HookPoint.PreUserMessageAdd, handler);
    
    public static IChatPipeline AddPreUserMessageAdd(this IChatPipeline pipeline, Func<ChatContext, HookData, Task<HookResult>> handler)
        => pipeline.AddHook(HookPoint.PreUserMessageAdd, new LambdaHookHandler(handler));
    
    public static IChatPipeline AddPreUserMessageAdd(this IChatPipeline pipeline, Action<ChatContext, HookData> handler)
        => pipeline.AddHook(HookPoint.PreUserMessageAdd, new ActionHookHandler(handler));
    
    public static IChatPipeline AddPreUserMessageAdd(this IChatPipeline pipeline, Func<ChatContext, Task> handler)
        => pipeline.AddHook(HookPoint.PreUserMessageAdd, new SimpleLambdaHookHandler(handler));
    
    public static IChatPipeline AddPreUserMessageAdd(this IChatPipeline pipeline, Action<ChatContext> handler)
        => pipeline.AddHook(HookPoint.PreUserMessageAdd, new SimpleActionHookHandler(handler));
    
    // PostUserMessageAdd
    public static IChatPipeline AddPostUserMessageAdd(this IChatPipeline pipeline, IHookHandler handler)
        => pipeline.AddHook(HookPoint.PostUserMessageAdd, handler);
    
    public static IChatPipeline AddPostUserMessageAdd(this IChatPipeline pipeline, Func<ChatContext, HookData, Task<HookResult>> handler)
        => pipeline.AddHook(HookPoint.PostUserMessageAdd, new LambdaHookHandler(handler));
    
    public static IChatPipeline AddPostUserMessageAdd(this IChatPipeline pipeline, Action<ChatContext, HookData> handler)
        => pipeline.AddHook(HookPoint.PostUserMessageAdd, new ActionHookHandler(handler));
    
    public static IChatPipeline AddPostUserMessageAdd(this IChatPipeline pipeline, Func<ChatContext, Task> handler)
        => pipeline.AddHook(HookPoint.PostUserMessageAdd, new SimpleLambdaHookHandler(handler));
    
    public static IChatPipeline AddPostUserMessageAdd(this IChatPipeline pipeline, Action<ChatContext> handler)
        => pipeline.AddHook(HookPoint.PostUserMessageAdd, new SimpleActionHookHandler(handler));
    
    #endregion
    
    #region AI Streaming Hooks
    
    // PreAIStreaming
    public static IChatPipeline AddPreAIStreaming(this IChatPipeline pipeline, IHookHandler handler)
        => pipeline.AddHook(HookPoint.PreAIStreaming, handler);
    
    public static IChatPipeline AddPreAIStreaming(this IChatPipeline pipeline, Func<ChatContext, HookData, Task<HookResult>> handler)
        => pipeline.AddHook(HookPoint.PreAIStreaming, new LambdaHookHandler(handler));
    
    public static IChatPipeline AddPreAIStreaming(this IChatPipeline pipeline, Action<ChatContext, HookData> handler)
        => pipeline.AddHook(HookPoint.PreAIStreaming, new ActionHookHandler(handler));
    
    public static IChatPipeline AddPreAIStreaming(this IChatPipeline pipeline, Func<ChatContext, Task> handler)
        => pipeline.AddHook(HookPoint.PreAIStreaming, new SimpleLambdaHookHandler(handler));
    
    public static IChatPipeline AddPreAIStreaming(this IChatPipeline pipeline, Action<ChatContext> handler)
        => pipeline.AddHook(HookPoint.PreAIStreaming, new SimpleActionHookHandler(handler));
    
    // PostAIStreaming
    public static IChatPipeline AddPostAIStreaming(this IChatPipeline pipeline, IHookHandler handler)
        => pipeline.AddHook(HookPoint.PostAIStreaming, handler);
    
    public static IChatPipeline AddPostAIStreaming(this IChatPipeline pipeline, Func<ChatContext, HookData, Task<HookResult>> handler)
        => pipeline.AddHook(HookPoint.PostAIStreaming, new LambdaHookHandler(handler));
    
    public static IChatPipeline AddPostAIStreaming(this IChatPipeline pipeline, Action<ChatContext, HookData> handler)
        => pipeline.AddHook(HookPoint.PostAIStreaming, new ActionHookHandler(handler));
    
    public static IChatPipeline AddPostAIStreaming(this IChatPipeline pipeline, Func<ChatContext, Task> handler)
        => pipeline.AddHook(HookPoint.PostAIStreaming, new SimpleLambdaHookHandler(handler));
    
    public static IChatPipeline AddPostAIStreaming(this IChatPipeline pipeline, Action<ChatContext> handler)
        => pipeline.AddHook(HookPoint.PostAIStreaming, new SimpleActionHookHandler(handler));
    
    #endregion
    
    #region Assistant Message Hooks
    
    // PreAssistantMessage (using PreMessageAdd)
    public static IChatPipeline AddPreAssistantMessage(this IChatPipeline pipeline, IHookHandler handler)
        => pipeline.AddHook(HookPoint.PreMessageAdd, handler);
    
    public static IChatPipeline AddPreAssistantMessage(this IChatPipeline pipeline, Func<ChatContext, HookData, Task<HookResult>> handler)
        => pipeline.AddHook(HookPoint.PreMessageAdd, new LambdaHookHandler(handler));
    
    public static IChatPipeline AddPreAssistantMessage(this IChatPipeline pipeline, Action<ChatContext, HookData> handler)
        => pipeline.AddHook(HookPoint.PreMessageAdd, new ActionHookHandler(handler));
    
    public static IChatPipeline AddPreAssistantMessage(this IChatPipeline pipeline, Func<ChatContext, Task> handler)
        => pipeline.AddHook(HookPoint.PreMessageAdd, new SimpleLambdaHookHandler(handler));
    
    public static IChatPipeline AddPreAssistantMessage(this IChatPipeline pipeline, Action<ChatContext> handler)
        => pipeline.AddHook(HookPoint.PreMessageAdd, new SimpleActionHookHandler(handler));
    
    // PostAssistantMessage (using PostMessageAdd)
    public static IChatPipeline AddPostAssistantMessage(this IChatPipeline pipeline, IHookHandler handler)
        => pipeline.AddHook(HookPoint.PostMessageAdd, handler);
    
    public static IChatPipeline AddPostAssistantMessage(this IChatPipeline pipeline, Func<ChatContext, HookData, Task<HookResult>> handler)
        => pipeline.AddHook(HookPoint.PostMessageAdd, new LambdaHookHandler(handler));
    
    public static IChatPipeline AddPostAssistantMessage(this IChatPipeline pipeline, Action<ChatContext, HookData> handler)
        => pipeline.AddHook(HookPoint.PostMessageAdd, new ActionHookHandler(handler));
    
    public static IChatPipeline AddPostAssistantMessage(this IChatPipeline pipeline, Func<ChatContext, Task> handler)
        => pipeline.AddHook(HookPoint.PostMessageAdd, new SimpleLambdaHookHandler(handler));
    
    public static IChatPipeline AddPostAssistantMessage(this IChatPipeline pipeline, Action<ChatContext> handler)
        => pipeline.AddHook(HookPoint.PostMessageAdd, new SimpleActionHookHandler(handler));
    
    // PreAssistantMessageWithToolCalls
    public static IChatPipeline AddPreAssistantMessageWithToolCalls(this IChatPipeline pipeline, IHookHandler handler)
        => pipeline.AddHook(HookPoint.PreAssistantMessageWithToolCalls, handler);
    
    public static IChatPipeline AddPreAssistantMessageWithToolCalls(this IChatPipeline pipeline, Func<ChatContext, HookData, Task<HookResult>> handler)
        => pipeline.AddHook(HookPoint.PreAssistantMessageWithToolCalls, new LambdaHookHandler(handler));
    
    public static IChatPipeline AddPreAssistantMessageWithToolCalls(this IChatPipeline pipeline, Action<ChatContext, HookData> handler)
        => pipeline.AddHook(HookPoint.PreAssistantMessageWithToolCalls, new ActionHookHandler(handler));
    
    public static IChatPipeline AddPreAssistantMessageWithToolCalls(this IChatPipeline pipeline, Func<ChatContext, Task> handler)
        => pipeline.AddHook(HookPoint.PreAssistantMessageWithToolCalls, new SimpleLambdaHookHandler(handler));
    
    public static IChatPipeline AddPreAssistantMessageWithToolCalls(this IChatPipeline pipeline, Action<ChatContext> handler)
        => pipeline.AddHook(HookPoint.PreAssistantMessageWithToolCalls, new SimpleActionHookHandler(handler));
    
    // PostAssistantMessageWithToolCalls
    public static IChatPipeline AddPostAssistantMessageWithToolCalls(this IChatPipeline pipeline, IHookHandler handler)
        => pipeline.AddHook(HookPoint.PostAssistantMessageWithToolCalls, handler);
    
    public static IChatPipeline AddPostAssistantMessageWithToolCalls(this IChatPipeline pipeline, Func<ChatContext, HookData, Task<HookResult>> handler)
        => pipeline.AddHook(HookPoint.PostAssistantMessageWithToolCalls, new LambdaHookHandler(handler));
    
    public static IChatPipeline AddPostAssistantMessageWithToolCalls(this IChatPipeline pipeline, Action<ChatContext, HookData> handler)
        => pipeline.AddHook(HookPoint.PostAssistantMessageWithToolCalls, new ActionHookHandler(handler));
    
    public static IChatPipeline AddPostAssistantMessageWithToolCalls(this IChatPipeline pipeline, Func<ChatContext, Task> handler)
        => pipeline.AddHook(HookPoint.PostAssistantMessageWithToolCalls, new SimpleLambdaHookHandler(handler));
    
    public static IChatPipeline AddPostAssistantMessageWithToolCalls(this IChatPipeline pipeline, Action<ChatContext> handler)
        => pipeline.AddHook(HookPoint.PostAssistantMessageWithToolCalls, new SimpleActionHookHandler(handler));
    
    #endregion
    
    #region Tool Call Hooks
    
    // PostFunctionDetection (after tool calls detected)
    public static IChatPipeline AddPostToolDetection(this IChatPipeline pipeline, IHookHandler handler)
        => pipeline.AddHook(HookPoint.PostFunctionDetection, handler);
    
    public static IChatPipeline AddPostToolDetection(this IChatPipeline pipeline, Func<ChatContext, HookData, Task<HookResult>> handler)
        => pipeline.AddHook(HookPoint.PostFunctionDetection, new LambdaHookHandler(handler));
    
    public static IChatPipeline AddPostToolDetection(this IChatPipeline pipeline, Action<ChatContext, HookData> handler)
        => pipeline.AddHook(HookPoint.PostFunctionDetection, new ActionHookHandler(handler));
    
    public static IChatPipeline AddPostToolDetection(this IChatPipeline pipeline, Func<ChatContext, Task> handler)
        => pipeline.AddHook(HookPoint.PostFunctionDetection, new SimpleLambdaHookHandler(handler));
    
    public static IChatPipeline AddPostToolDetection(this IChatPipeline pipeline, Action<ChatContext> handler)
        => pipeline.AddHook(HookPoint.PostFunctionDetection, new SimpleActionHookHandler(handler));
    
    // PreToolCall (before function execution)
    public static IChatPipeline AddPreToolCall(this IChatPipeline pipeline, IHookHandler handler)
        => pipeline.AddHook(HookPoint.PreFunctionExecution, handler);
    
    public static IChatPipeline AddPreToolCall(this IChatPipeline pipeline, Func<ChatContext, HookData, Task<HookResult>> handler)
        => pipeline.AddHook(HookPoint.PreFunctionExecution, new LambdaHookHandler(handler));
    
    public static IChatPipeline AddPreToolCall(this IChatPipeline pipeline, Action<ChatContext, HookData> handler)
        => pipeline.AddHook(HookPoint.PreFunctionExecution, new ActionHookHandler(handler));
    
    public static IChatPipeline AddPreToolCall(this IChatPipeline pipeline, Func<ChatContext, Task> handler)
        => pipeline.AddHook(HookPoint.PreFunctionExecution, new SimpleLambdaHookHandler(handler));
    
    public static IChatPipeline AddPreToolCall(this IChatPipeline pipeline, Action<ChatContext> handler)
        => pipeline.AddHook(HookPoint.PreFunctionExecution, new SimpleActionHookHandler(handler));
    
    // PostToolCall (after function execution)
    public static IChatPipeline AddPostToolCall(this IChatPipeline pipeline, IHookHandler handler)
        => pipeline.AddHook(HookPoint.PostFunctionExecution, handler);
    
    public static IChatPipeline AddPostToolCall(this IChatPipeline pipeline, Func<ChatContext, HookData, Task<HookResult>> handler)
        => pipeline.AddHook(HookPoint.PostFunctionExecution, new LambdaHookHandler(handler));
    
    public static IChatPipeline AddPostToolCall(this IChatPipeline pipeline, Action<ChatContext, HookData> handler)
        => pipeline.AddHook(HookPoint.PostFunctionExecution, new ActionHookHandler(handler));
    
    public static IChatPipeline AddPostToolCall(this IChatPipeline pipeline, Func<ChatContext, Task> handler)
        => pipeline.AddHook(HookPoint.PostFunctionExecution, new SimpleLambdaHookHandler(handler));
    
    public static IChatPipeline AddPostToolCall(this IChatPipeline pipeline, Action<ChatContext> handler)
        => pipeline.AddHook(HookPoint.PostFunctionExecution, new SimpleActionHookHandler(handler));
    
    #endregion
    
    #region Tool Results Hooks
    
    // PreToolResults
    public static IChatPipeline AddPreToolResults(this IChatPipeline pipeline, IHookHandler handler)
        => pipeline.AddHook(HookPoint.PreToolResultsAdd, handler);
    
    public static IChatPipeline AddPreToolResults(this IChatPipeline pipeline, Func<ChatContext, HookData, Task<HookResult>> handler)
        => pipeline.AddHook(HookPoint.PreToolResultsAdd, new LambdaHookHandler(handler));
    
    public static IChatPipeline AddPreToolResults(this IChatPipeline pipeline, Action<ChatContext, HookData> handler)
        => pipeline.AddHook(HookPoint.PreToolResultsAdd, new ActionHookHandler(handler));
    
    public static IChatPipeline AddPreToolResults(this IChatPipeline pipeline, Func<ChatContext, Task> handler)
        => pipeline.AddHook(HookPoint.PreToolResultsAdd, new SimpleLambdaHookHandler(handler));
    
    public static IChatPipeline AddPreToolResults(this IChatPipeline pipeline, Action<ChatContext> handler)
        => pipeline.AddHook(HookPoint.PreToolResultsAdd, new SimpleActionHookHandler(handler));
    
    // PostToolResults
    public static IChatPipeline AddPostToolResults(this IChatPipeline pipeline, IHookHandler handler)
        => pipeline.AddHook(HookPoint.PostToolResultsAdd, handler);
    
    public static IChatPipeline AddPostToolResults(this IChatPipeline pipeline, Func<ChatContext, HookData, Task<HookResult>> handler)
        => pipeline.AddHook(HookPoint.PostToolResultsAdd, new LambdaHookHandler(handler));
    
    public static IChatPipeline AddPostToolResults(this IChatPipeline pipeline, Action<ChatContext, HookData> handler)
        => pipeline.AddHook(HookPoint.PostToolResultsAdd, new ActionHookHandler(handler));
    
    public static IChatPipeline AddPostToolResults(this IChatPipeline pipeline, Func<ChatContext, Task> handler)
        => pipeline.AddHook(HookPoint.PostToolResultsAdd, new SimpleLambdaHookHandler(handler));
    
    public static IChatPipeline AddPostToolResults(this IChatPipeline pipeline, Action<ChatContext> handler)
        => pipeline.AddHook(HookPoint.PostToolResultsAdd, new SimpleActionHookHandler(handler));
    
    #endregion
    
    #region Injected Content Hooks
    
    // PreInjectedContent
    public static IChatPipeline AddPreInjectedContent(this IChatPipeline pipeline, IHookHandler handler)
        => pipeline.AddHook(HookPoint.PreInjectedContentAdd, handler);
    
    public static IChatPipeline AddPreInjectedContent(this IChatPipeline pipeline, Func<ChatContext, HookData, Task<HookResult>> handler)
        => pipeline.AddHook(HookPoint.PreInjectedContentAdd, new LambdaHookHandler(handler));
    
    public static IChatPipeline AddPreInjectedContent(this IChatPipeline pipeline, Action<ChatContext, HookData> handler)
        => pipeline.AddHook(HookPoint.PreInjectedContentAdd, new ActionHookHandler(handler));
    
    public static IChatPipeline AddPreInjectedContent(this IChatPipeline pipeline, Func<ChatContext, Task> handler)
        => pipeline.AddHook(HookPoint.PreInjectedContentAdd, new SimpleLambdaHookHandler(handler));
    
    public static IChatPipeline AddPreInjectedContent(this IChatPipeline pipeline, Action<ChatContext> handler)
        => pipeline.AddHook(HookPoint.PreInjectedContentAdd, new SimpleActionHookHandler(handler));
    
    // PostInjectedContent
    public static IChatPipeline AddPostInjectedContent(this IChatPipeline pipeline, IHookHandler handler)
        => pipeline.AddHook(HookPoint.PostInjectedContentAdd, handler);
    
    public static IChatPipeline AddPostInjectedContent(this IChatPipeline pipeline, Func<ChatContext, HookData, Task<HookResult>> handler)
        => pipeline.AddHook(HookPoint.PostInjectedContentAdd, new LambdaHookHandler(handler));
    
    public static IChatPipeline AddPostInjectedContent(this IChatPipeline pipeline, Action<ChatContext, HookData> handler)
        => pipeline.AddHook(HookPoint.PostInjectedContentAdd, new ActionHookHandler(handler));
    
    public static IChatPipeline AddPostInjectedContent(this IChatPipeline pipeline, Func<ChatContext, Task> handler)
        => pipeline.AddHook(HookPoint.PostInjectedContentAdd, new SimpleLambdaHookHandler(handler));
    
    public static IChatPipeline AddPostInjectedContent(this IChatPipeline pipeline, Action<ChatContext> handler)
        => pipeline.AddHook(HookPoint.PostInjectedContentAdd, new SimpleActionHookHandler(handler));
    
    #endregion
    
    #region Loop Control Hooks
    
    // PreLoopContinue
    public static IChatPipeline AddPreLoopContinue(this IChatPipeline pipeline, IHookHandler handler)
        => pipeline.AddHook(HookPoint.PreLoopContinue, handler);
    
    public static IChatPipeline AddPreLoopContinue(this IChatPipeline pipeline, Func<ChatContext, HookData, Task<HookResult>> handler)
        => pipeline.AddHook(HookPoint.PreLoopContinue, new LambdaHookHandler(handler));
    
    public static IChatPipeline AddPreLoopContinue(this IChatPipeline pipeline, Action<ChatContext, HookData> handler)
        => pipeline.AddHook(HookPoint.PreLoopContinue, new ActionHookHandler(handler));
    
    public static IChatPipeline AddPreLoopContinue(this IChatPipeline pipeline, Func<ChatContext, Task> handler)
        => pipeline.AddHook(HookPoint.PreLoopContinue, new SimpleLambdaHookHandler(handler));
    
    public static IChatPipeline AddPreLoopContinue(this IChatPipeline pipeline, Action<ChatContext> handler)
        => pipeline.AddHook(HookPoint.PreLoopContinue, new SimpleActionHookHandler(handler));
    
    // PostLoopIteration
    public static IChatPipeline AddPostLoopIteration(this IChatPipeline pipeline, IHookHandler handler)
        => pipeline.AddHook(HookPoint.PostLoopIteration, handler);
    
    public static IChatPipeline AddPostLoopIteration(this IChatPipeline pipeline, Func<ChatContext, HookData, Task<HookResult>> handler)
        => pipeline.AddHook(HookPoint.PostLoopIteration, new LambdaHookHandler(handler));
    
    public static IChatPipeline AddPostLoopIteration(this IChatPipeline pipeline, Action<ChatContext, HookData> handler)
        => pipeline.AddHook(HookPoint.PostLoopIteration, new ActionHookHandler(handler));
    
    public static IChatPipeline AddPostLoopIteration(this IChatPipeline pipeline, Func<ChatContext, Task> handler)
        => pipeline.AddHook(HookPoint.PostLoopIteration, new SimpleLambdaHookHandler(handler));
    
    public static IChatPipeline AddPostLoopIteration(this IChatPipeline pipeline, Action<ChatContext> handler)
        => pipeline.AddHook(HookPoint.PostLoopIteration, new SimpleActionHookHandler(handler));
    
    #endregion
    
    #region Pipeline Lifecycle Hooks
    
    // PrePipelineStart
    public static IChatPipeline AddPrePipelineStart(this IChatPipeline pipeline, IHookHandler handler)
        => pipeline.AddHook(HookPoint.PrePipelineStart, handler);
    
    public static IChatPipeline AddPrePipelineStart(this IChatPipeline pipeline, Func<ChatContext, HookData, Task<HookResult>> handler)
        => pipeline.AddHook(HookPoint.PrePipelineStart, new LambdaHookHandler(handler));
    
    public static IChatPipeline AddPrePipelineStart(this IChatPipeline pipeline, Action<ChatContext, HookData> handler)
        => pipeline.AddHook(HookPoint.PrePipelineStart, new ActionHookHandler(handler));
    
    public static IChatPipeline AddPrePipelineStart(this IChatPipeline pipeline, Func<ChatContext, Task> handler)
        => pipeline.AddHook(HookPoint.PrePipelineStart, new SimpleLambdaHookHandler(handler));
    
    public static IChatPipeline AddPrePipelineStart(this IChatPipeline pipeline, Action<ChatContext> handler)
        => pipeline.AddHook(HookPoint.PrePipelineStart, new SimpleActionHookHandler(handler));
    
    // PostPipelineComplete
    public static IChatPipeline AddPostPipelineComplete(this IChatPipeline pipeline, IHookHandler handler)
        => pipeline.AddHook(HookPoint.PostPipelineComplete, handler);
    
    public static IChatPipeline AddPostPipelineComplete(this IChatPipeline pipeline, Func<ChatContext, HookData, Task<HookResult>> handler)
        => pipeline.AddHook(HookPoint.PostPipelineComplete, new LambdaHookHandler(handler));
    
    public static IChatPipeline AddPostPipelineComplete(this IChatPipeline pipeline, Action<ChatContext, HookData> handler)
        => pipeline.AddHook(HookPoint.PostPipelineComplete, new ActionHookHandler(handler));
    
    public static IChatPipeline AddPostPipelineComplete(this IChatPipeline pipeline, Func<ChatContext, Task> handler)
        => pipeline.AddHook(HookPoint.PostPipelineComplete, new SimpleLambdaHookHandler(handler));
    
    public static IChatPipeline AddPostPipelineComplete(this IChatPipeline pipeline, Action<ChatContext> handler)
        => pipeline.AddHook(HookPoint.PostPipelineComplete, new SimpleActionHookHandler(handler));
    
    #endregion
    
    #region Generic Catch-All Hooks
    
    /// <summary>
    /// Adds a hook that fires before ANY message is added to the conversation.
    /// This includes user messages, assistant messages, tool results, and injected content.
    /// </summary>
    public static IChatPipeline AddPreMessageAdd(this IChatPipeline pipeline, IHookHandler handler)
    {
        return pipeline
            .AddHook(HookPoint.PreUserMessageAdd, handler)
            .AddHook(HookPoint.PreMessageAdd, handler)
            .AddHook(HookPoint.PreAssistantMessageWithToolCalls, handler)
            .AddHook(HookPoint.PreToolResultsAdd, handler)
            .AddHook(HookPoint.PreInjectedContentAdd, handler);
    }
    
    /// <summary>
    /// Adds a hook that fires before ANY message is added to the conversation.
    /// </summary>
    public static IChatPipeline AddPreMessageAdd(this IChatPipeline pipeline, Func<ChatContext, HookData, Task<HookResult>> handler)
    {
        var hookHandler = new LambdaHookHandler(handler);
        return AddPreMessageAdd(pipeline, hookHandler);
    }
    
    /// <summary>
    /// Adds a hook that fires before ANY message is added to the conversation.
    /// </summary>
    public static IChatPipeline AddPreMessageAdd(this IChatPipeline pipeline, Action<ChatContext, HookData> handler)
    {
        var hookHandler = new ActionHookHandler(handler);
        return AddPreMessageAdd(pipeline, hookHandler);
    }
    
    /// <summary>
    /// Adds a hook that fires before ANY message is added to the conversation.
    /// </summary>
    public static IChatPipeline AddPreMessageAdd(this IChatPipeline pipeline, Func<ChatContext, Task> handler)
    {
        var hookHandler = new SimpleLambdaHookHandler(handler);
        return AddPreMessageAdd(pipeline, hookHandler);
    }
    
    /// <summary>
    /// Adds a hook that fires before ANY message is added to the conversation.
    /// </summary>
    public static IChatPipeline AddPreMessageAdd(this IChatPipeline pipeline, Action<ChatContext> handler)
    {
        var hookHandler = new SimpleActionHookHandler(handler);
        return AddPreMessageAdd(pipeline, hookHandler);
    }
    
    /// <summary>
    /// Adds a hook that fires after ANY message is added to the conversation.
    /// This includes user messages, assistant messages, tool results, and injected content.
    /// </summary>
    public static IChatPipeline AddPostMessageAdd(this IChatPipeline pipeline, IHookHandler handler)
    {
        return pipeline
            .AddHook(HookPoint.PostUserMessageAdd, handler)
            .AddHook(HookPoint.PostMessageAdd, handler)
            .AddHook(HookPoint.PostAssistantMessageWithToolCalls, handler)
            .AddHook(HookPoint.PostToolResultsAdd, handler)
            .AddHook(HookPoint.PostInjectedContentAdd, handler);
    }
    
    /// <summary>
    /// Adds a hook that fires after ANY message is added to the conversation.
    /// </summary>
    public static IChatPipeline AddPostMessageAdd(this IChatPipeline pipeline, Func<ChatContext, HookData, Task<HookResult>> handler)
    {
        var hookHandler = new LambdaHookHandler(handler);
        return AddPostMessageAdd(pipeline, hookHandler);
    }
    
    /// <summary>
    /// Adds a hook that fires after ANY message is added to the conversation.
    /// </summary>
    public static IChatPipeline AddPostMessageAdd(this IChatPipeline pipeline, Action<ChatContext, HookData> handler)
    {
        var hookHandler = new ActionHookHandler(handler);
        return AddPostMessageAdd(pipeline, hookHandler);
    }
    
    /// <summary>
    /// Adds a hook that fires after ANY message is added to the conversation.
    /// </summary>
    public static IChatPipeline AddPostMessageAdd(this IChatPipeline pipeline, Func<ChatContext, Task> handler)
    {
        var hookHandler = new SimpleLambdaHookHandler(handler);
        return AddPostMessageAdd(pipeline, hookHandler);
    }
    
    /// <summary>
    /// Adds a hook that fires after ANY message is added to the conversation.
    /// </summary>
    public static IChatPipeline AddPostMessageAdd(this IChatPipeline pipeline, Action<ChatContext> handler)
    {
        var hookHandler = new SimpleActionHookHandler(handler);
        return AddPostMessageAdd(pipeline, hookHandler);
    }
    
    #endregion
}
