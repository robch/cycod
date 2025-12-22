namespace CycoDev.ChatPipeline;

/// <summary>
/// Implementation of the chat pipeline that orchestrates stages and hooks.
/// </summary>
public class ChatPipeline : IChatPipeline
{
    private readonly List<IPipelineStage> _stages = new();
    private readonly Dictionary<HookPoint, List<IHookHandler>> _hooks = new();
    
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
        
        // Sort by priority (lower numbers execute first)
        _hooks[hookPoint] = _hooks[hookPoint].OrderBy(h => h.Priority).ToList();
        
        return this;
    }
    
    public async Task<ChatResult> ExecuteAsync(ChatContext context)
    {
        // Set up hook executor so stages can execute hooks
        context.HookExecutor = ExecuteHooksAsync;
        
        try
        {
            // Execute pre-pipeline hooks
            await ExecuteHooksAsync(HookPoint.PrePipelineStart, context, null);
            
            // Main pipeline execution loop
            while (!context.State.ShouldExitLoop)
            {
                context.State.LoopIteration++;
                
                foreach (var stage in _stages)
                {
                    // Check if we should skip this stage
                    if (context.State.ShouldSkipNextStage)
                    {
                        context.State.ShouldSkipNextStage = false;
                        continue;
                    }
                    
                    // Check if we should exit
                    if (context.State.ShouldExitLoop)
                    {
                        break;
                    }
                    
                    // Update current stage
                    context.State.CurrentStage = stage.Name;
                    
                    // Execute pre-stage hooks
                    var preHookData = new HookData
                    {
                        StageName = stage.Name,
                        HookPoint = stage.PreHookPoint
                    };
                    
                    var preHookResult = await ExecuteHooksAsync(stage.PreHookPoint, context, preHookData);
                    
                    // Check if hooks requested skip or exit
                    if (preHookResult.ShouldSkipStage)
                    {
                        continue;
                    }
                    
                    if (preHookResult.ShouldExitLoop)
                    {
                        context.State.ShouldExitLoop = true;
                        break;
                    }
                    
                    // Execute the stage
                    var stageResult = await stage.ExecuteAsync(context);
                    
                    // Check stage result
                    if (!stageResult.Success)
                    {
                        return ChatResult.Failed(stageResult.ErrorMessage ?? "Stage execution failed");
                    }
                    
                    if (stageResult.ShouldExitLoop)
                    {
                        context.State.ShouldExitLoop = true;
                    }
                    
                    if (!stageResult.ShouldContinue)
                    {
                        continue;
                    }
                    
                    // Execute post-stage hooks
                    var postHookData = new HookData
                    {
                        StageName = stage.Name,
                        HookPoint = stage.PostHookPoint,
                        StageResult = stageResult
                    };
                    
                    var postHookResult = await ExecuteHooksAsync(stage.PostHookPoint, context, postHookData);
                    
                    // Check if hooks requested exit
                    if (postHookResult.ShouldExitLoop)
                    {
                        context.State.ShouldExitLoop = true;
                        break;
                    }
                }
                
                // Execute post-loop-iteration hooks
                await ExecuteHooksAsync(HookPoint.PostLoopIteration, context, null);
            }
            
            // Execute post-pipeline hooks
            await ExecuteHooksAsync(HookPoint.PostPipelineComplete, context, null);
            
            // Return result
            if (context.State.IsInterrupted)
            {
                return ChatResult.Interrupted();
            }
            
            return ChatResult.Completed(context.ContentToReturn);
        }
        catch (OperationCanceledException)
        {
            context.State.IsInterrupted = true;
            return ChatResult.Interrupted();
        }
        catch (Exception ex)
        {
            return ChatResult.Failed($"Pipeline execution failed: {ex.Message}");
        }
    }
    
    private async Task<HookResult> ExecuteHooksAsync(HookPoint hookPoint, ChatContext context, HookData? data)
    {
        if (!_hooks.ContainsKey(hookPoint))
        {
            return HookResult.Continue();
        }
        
        var combinedResult = HookResult.Continue();
        
        foreach (var hook in _hooks[hookPoint])
        {
            try
            {
                var hookData = data ?? new HookData { HookPoint = hookPoint };
                var result = await hook.HandleAsync(context, hookData);
                
                // Combine results - any hook can request skip or exit
                if (result.ShouldSkipStage)
                {
                    combinedResult.ShouldSkipStage = true;
                }
                
                if (result.ShouldExitLoop)
                {
                    combinedResult.ShouldExitLoop = true;
                }
                
                if (result.ModifiedContext)
                {
                    combinedResult.ModifiedContext = true;
                }
            }
            catch (Exception)
            {
                // Hook failures don't stop pipeline execution
                // TODO: Add logging
            }
        }
        
        return combinedResult;
    }
}
