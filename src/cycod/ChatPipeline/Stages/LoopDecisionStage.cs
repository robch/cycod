namespace CycoDev.ChatPipeline.Stages;

/// <summary>
/// Stage that decides whether to continue the conversation loop or exit.
/// </summary>
public class LoopDecisionStage : IPipelineStage
{
    public string Name => "LoopDecision";
    public HookPoint PreHookPoint => HookPoint.PreLoopContinue;
    public HookPoint PostHookPoint => HookPoint.PostLoopIteration;
    
    public async Task<StageResult> ExecuteAsync(ChatContext context)
    {
        // If functions were called, continue loop to get next AI response
        if (context.State.FunctionsWereCalled)
        {
            // Clear function state for next iteration
            context.State.FunctionsWereCalled = false;
            context.Pending.PendingToolCalls.Clear();
            context.Pending.PendingToolResults.Clear();
            context.Pending.ResponseContent = string.Empty;
            context.Pending.StreamingContent.Clear();
            context.Pending.StreamingUpdates.Clear();
            context.FunctionCallDetector?.Clear();
            
            // Continue to next iteration
            return StageResult.Continue();
        }
        
        // No functions were called - we're done
        return StageResult.ExitLoop();
    }
}
