using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cycod.ChatPipeline;

/// <summary>
/// Main pipeline implementation that orchestrates stage execution and hook invocation.
/// Manages the conversation flow from user input through AI response to function execution.
/// </summary>
public class ChatPipeline : IChatPipeline
{
    private readonly List<IPipelineStage> _stages = new();
    private readonly Dictionary<HookPoint, List<IHookHandler>> _hooks = new();
    
    public IReadOnlyList<IPipelineStage> Stages => _stages.AsReadOnly();
    public IReadOnlyDictionary<HookPoint, IReadOnlyList<IHookHandler>> Hooks => 
        _hooks.ToDictionary(kv => kv.Key, kv => (IReadOnlyList<IHookHandler>)kv.Value.AsReadOnly());
    
    public IChatPipeline AddStage(IPipelineStage stage)
    {
        _stages.Add(stage);
        return this;
    }
    
    public IChatPipeline AddHook(HookPoint hookPoint, IHookHandler handler)
    {
        if (!_hooks.ContainsKey(hookPoint))
        {
            _hooks[hookPoint] = new List<IHookHandler>();
        }
        
        _hooks[hookPoint].Add(handler);
        
        // Sort by priority (lower numbers first)
        _hooks[hookPoint] = _hooks[hookPoint].OrderBy(h => h.Priority).ToList();
        
        return this;
    }
    
    public async Task<ChatResult> ExecuteAsync(ChatContext context)
    {
        try
        {
            foreach (var stage in _stages)
            {
                // Update current stage in context
                context.State.CurrentStage = stage.Name;
                
                // Execute pre-stage hooks
                var preHookResult = await ExecuteHooksAsync(stage.PreHookPoint, context, null);
                
                if (preHookResult.ShouldExitPipeline)
                {
                    return CreateExitResult(context, "Pre-stage hook requested exit");
                }
                
                if (preHookResult.ShouldSkipStage || context.Pending.ShouldSkipNextStage)
                {
                    context.Pending.ShouldSkipNextStage = false; // Reset flag
                    continue;
                }
                
                // Execute the stage
                StageResult stageResult;
                try
                {
                    stageResult = await stage.ExecuteAsync(context);
                }
                catch (Exception ex)
                {
                    // Stage threw exception - record and decide how to handle
                    context.State.LastError = ex;
                    stageResult = StageResult.Error($"Stage {stage.Name} failed: {ex.Message}");
                }
                
                // Execute post-stage hooks
                var postHookResult = await ExecuteHooksAsync(stage.PostHookPoint, context, stageResult);
                
                // Allow hooks to override stage result
                if (postHookResult.OverrideStageResult != null)
                {
                    stageResult = postHookResult.OverrideStageResult;
                }
                
                // Check for exit conditions
                if (postHookResult.ShouldExitPipeline || 
                    stageResult.ShouldExit || 
                    context.State.ShouldExitLoop || 
                    context.Pending.ShouldExitLoop)
                {
                    return CreateExitResult(context, "Pipeline exit requested");
                }
                
                // Check for stage failure
                if (!stageResult.Success)
                {
                    return ChatResult.Failure(stageResult.ErrorMessage ?? "Stage failed");
                }
                
                // Check if we should continue
                if (!stageResult.ShouldContinue)
                {
                    continue;
                }
                
                // Handle redirect
                if (!string.IsNullOrEmpty(context.Pending.RedirectToStage))
                {
                    var redirectStage = _stages.FirstOrDefault(s => s.Name == context.Pending.RedirectToStage);
                    if (redirectStage != null)
                    {
                        context.Pending.RedirectToStage = null;
                        // Would need to implement stage jumping logic here
                        // For now, just continue normally
                    }
                }
            }
            
            // All stages completed successfully
            return CreateSuccessResult(context);
        }
        catch (OperationCanceledException) when (context.CancellationTokenSource.Token.IsCancellationRequested)
        {
            // Graceful cancellation (e.g., user interrupt)
            context.State.IsInterrupted = true;
            return CreateInterruptedResult(context);
        }
        catch (Exception ex)
        {
            // Unexpected exception
            context.State.LastError = ex;
            return ChatResult.Failure($"Pipeline execution failed: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Executes all hooks registered at a specific hook point.
    /// </summary>
    private async Task<HookResult> ExecuteHooksAsync(
        HookPoint hookPoint, 
        ChatContext context, 
        StageResult? stageResult)
    {
        if (!_hooks.TryGetValue(hookPoint, out var handlers) || handlers.Count == 0)
        {
            return HookResult.Continue();
        }
        
        var hookData = new HookData
        {
            HookPoint = hookPoint,
            StageName = context.State.CurrentStage,
            StageResult = stageResult
        };
        
        var aggregatedResult = HookResult.Continue();
        
        foreach (var handler in handlers)
        {
            try
            {
                var result = await handler.HandleAsync(context, hookData);
                
                // Aggregate results (any hook can request skip/exit)
                if (result.ShouldSkipStage)
                {
                    aggregatedResult.ShouldSkipStage = true;
                }
                
                if (result.ShouldExitPipeline)
                {
                    aggregatedResult.ShouldExitPipeline = true;
                    // Exit early if any hook requests it
                    return aggregatedResult;
                }
                
                // Last hook's override wins (could be more sophisticated)
                if (result.OverrideStageResult != null)
                {
                    aggregatedResult.OverrideStageResult = result.OverrideStageResult;
                }
                
                if (!result.Success)
                {
                    // Hook failed - log but continue to next hook
                    // Could make this configurable (fail-fast vs. continue-on-error)
                    Console.WriteLine($"Hook {handler.Name} failed: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                // Hook threw exception - log but don't crash pipeline
                Console.WriteLine($"Hook {handler.Name} threw exception: {ex.Message}");
                context.State.LastError = ex;
            }
        }
        
        return aggregatedResult;
    }
    
    private ChatResult CreateSuccessResult(ChatContext context)
    {
        // Extract final content from context
        var lastAssistantMessage = context.Messages
            .LastOrDefault(m => m.Role == ChatRole.Assistant);
        
        var content = lastAssistantMessage?.Text ?? string.Empty;
        
        return new ChatResult
        {
            Success = true,
            Content = content,
            ShouldContinue = !context.State.ShouldExitLoop,
            FinalContext = context
        };
    }
    
    private ChatResult CreateExitResult(ChatContext context, string reason)
    {
        var lastAssistantMessage = context.Messages
            .LastOrDefault(m => m.Role == ChatRole.Assistant);
        
        var content = lastAssistantMessage?.Text ?? string.Empty;
        
        return new ChatResult
        {
            Success = true,
            Content = content,
            ShouldContinue = false,
            FinalContext = context
        };
    }
    
    private ChatResult CreateInterruptedResult(ChatContext context)
    {
        // Use streaming content if available (partial response)
        var content = context.Pending.StreamingContent.ToString();
        
        if (string.IsNullOrEmpty(content))
        {
            // Fall back to last assistant message
            var lastMessage = context.Messages.LastOrDefault(m => m.Role == ChatRole.Assistant);
            content = lastMessage?.Text ?? string.Empty;
        }
        
        return new ChatResult
        {
            Success = true,
            Content = content,
            ShouldContinue = false,
            FinalContext = context
        };
    }
}

/// <summary>
/// Builder for creating pipelines with fluent configuration.
/// </summary>
public class ChatPipelineBuilder
{
    private readonly ChatPipeline _pipeline = new();
    
    /// <summary>
    /// Adds a stage to the pipeline.
    /// </summary>
    public ChatPipelineBuilder WithStage(IPipelineStage stage)
    {
        _pipeline.AddStage(stage);
        return this;
    }
    
    /// <summary>
    /// Adds a hook to the pipeline.
    /// </summary>
    public ChatPipelineBuilder WithHook(HookPoint hookPoint, IHookHandler handler)
    {
        _pipeline.AddHook(hookPoint, handler);
        return this;
    }
    
    /// <summary>
    /// Adds cancellation support with a cancellation token source.
    /// </summary>
    public ChatPipelineBuilder WithCancellation(CancellationTokenSource cts)
    {
        // Would configure pipeline to use this token source
        return this;
    }
    
    /// <summary>
    /// Adds interrupt support (double-ESC).
    /// </summary>
    public ChatPipelineBuilder WithInterruptSupport(IInterruptManager interruptManager)
    {
        // Would add interrupt hook
        return this;
    }
    
    /// <summary>
    /// Adds exception handling configuration.
    /// </summary>
    public ChatPipelineBuilder WithExceptionHandling(IExceptionHandler handler)
    {
        // Would add exception handling hooks
        return this;
    }
    
    /// <summary>
    /// Builds and returns the configured pipeline.
    /// </summary>
    public IChatPipeline Build()
    {
        return _pipeline;
    }
}
