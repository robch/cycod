using Microsoft.Extensions.AI;

namespace CycoDev.ChatPipeline.Stages;

/// <summary>
/// Stage that adds messages to the conversation history.
/// </summary>
public class MessagePersistenceStage : IPipelineStage
{
    public string Name => "MessagePersistence";
    public HookPoint PreHookPoint => HookPoint.PreMessageAdd;
    public HookPoint PostHookPoint => HookPoint.PostMessageAdd;
    
    public async Task<StageResult> ExecuteAsync(ChatContext context)
    {
        // If functions were called, messages were already added in FunctionExecutionStage
        if (context.State.FunctionsWereCalled)
        {
            return StageResult.Continue();
        }
        
        // Add final assistant message if we have response content
        if (!string.IsNullOrEmpty(context.Pending.ResponseContent))
        {
            var assistantMessage = new ChatMessage(ChatRole.Assistant, context.Pending.ResponseContent);
            context.Messages.Add(assistantMessage);
            
            // Invoke message callback
            context.Callbacks.MessageCallback?.Invoke(context.Messages);
        }
        
        return StageResult.Continue();
    }
}
