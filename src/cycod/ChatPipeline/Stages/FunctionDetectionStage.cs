namespace CycoDev.ChatPipeline.Stages;

/// <summary>
/// Stage that detects function calls from the AI response.
/// </summary>
public class FunctionDetectionStage : IPipelineStage
{
    public string Name => "FunctionDetection";
    public HookPoint PreHookPoint => HookPoint.PostAIStreaming;
    public HookPoint PostHookPoint => HookPoint.PostFunctionDetection;
    
    public async Task<StageResult> ExecuteAsync(ChatContext context)
    {
        if (context.FunctionCallDetector == null)
        {
            return StageResult.Continue();
        }
        
        // Get ready-to-call function calls
        var readyToCallFunctionCalls = context.FunctionCallDetector.GetReadyToCallFunctionCalls();
        
        if (readyToCallFunctionCalls.Count > 0)
        {
            // Store pending tool calls
            context.Pending.PendingToolCalls = readyToCallFunctionCalls;
            return StageResult.Continue();
        }
        
        // No function calls detected - we're done with this iteration
        return StageResult.Continue();
    }
}
