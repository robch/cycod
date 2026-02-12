using Microsoft.Extensions.AI;

namespace CycoDev.ChatPipeline.Stages;

/// <summary>
/// Stage that streams AI responses and accumulates content.
/// </summary>
public class AIStreamingStage : IPipelineStage
{
    public string Name => "AIStreaming";
    public HookPoint PreHookPoint => HookPoint.PreAIStreaming;
    public HookPoint PostHookPoint => HookPoint.PostAIStreaming;
    
    public async Task<StageResult> ExecuteAsync(ChatContext context)
    {
        if (context.ChatClient == null)
        {
            return StageResult.Error("ChatClient is null");
        }
        
        var responseContent = string.Empty;
        var contentToReturn = string.Empty;
        
        try
        {
            // Stream AI response
            await foreach (var update in context.ChatClient.GetStreamingResponseAsync(
                context.Messages, 
                context.ChatOptions, 
                context.CancellationToken))
            {
                // Check for cancellation
                context.CancellationToken.ThrowIfCancellationRequested();
                
                // Let function call detector process the update
                context.FunctionCallDetector?.CheckForFunctionCall(update);
                
                // Extract text content
                var content = string.Join("", update.Contents
                    .Where(c => c is TextContent)
                    .Cast<TextContent>()
                    .Select(c => c.Text)
                    .ToList());
                
                // Check for content filter
                if (update.FinishReason == ChatFinishReason.ContentFilter)
                {
                    content = $"{content}\nWARNING: Content filtered!";
                }
                
                // Accumulate content
                if (!string.IsNullOrEmpty(content))
                {
                    responseContent += content;
                    contentToReturn += content;
                    context.Pending.StreamingContent.Append(content);
                }
                
                // Store update
                context.Pending.StreamingUpdates.Add(update);
                
                // Invoke streaming callback
                context.Callbacks.StreamingCallback?.Invoke(update);
            }
            
            // Store the response content
            context.Pending.ResponseContent = responseContent;
            context.ContentToReturn = contentToReturn;
            
            return StageResult.Continue();
        }
        catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
        {
            // Interrupted - mark state and propagate
            context.State.IsInterrupted = true;
            throw;
        }
    }
}
