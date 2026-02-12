using CycoDev.ChatPipeline.Stages;
using Microsoft.Extensions.AI;

namespace CycoDev.ChatPipeline;

/// <summary>
/// Factory for creating standard chat pipelines.
/// </summary>
public static class ChatPipelineFactory
{
    /// <summary>
    /// Creates a standard conversation pipeline with all stages.
    /// </summary>
    public static IChatPipeline CreateStandardPipeline()
    {
        return new ChatPipeline()
            .AddStage(new AIStreamingStage())
            .AddStage(new FunctionDetectionStage())
            .AddStage(new FunctionExecutionStage())
            .AddStage(new MessagePersistenceStage())
            .AddStage(new LoopDecisionStage());
    }
    
    /// <summary>
    /// Creates a chat context from conversation parameters.
    /// </summary>
    public static ChatContext CreateContext(
        IChatClient chatClient,
        ChatOptions options,
        FunctionCallDetector functionCallDetector,
        FunctionFactory functionFactory,
        Conversation conversation,
        string userPrompt,
        IEnumerable<string> imageFiles,
        Action<IList<ChatMessage>>? messageCallback = null,
        Action<ChatResponseUpdate>? streamingCallback = null,
        Func<string, string?, bool>? approveFunctionCall = null,
        Action<string, string, object?>? functionCallCallback = null,
        CancellationToken cancellationToken = default)
    {
        var context = new ChatContext
        {
            ChatClient = chatClient,
            ChatOptions = options,
            FunctionCallDetector = functionCallDetector,
            FunctionFactory = functionFactory,
            Messages = conversation.Messages,
            UserPrompt = userPrompt,
            ImageFiles = imageFiles,
            CancellationToken = cancellationToken,
            Callbacks = new ChatCallbacks
            {
                MessageCallback = messageCallback,
                StreamingCallback = streamingCallback,
                ApproveFunctionCall = approveFunctionCall,
                FunctionCallCallback = functionCallCallback
            }
        };
        
        return context;
    }
}
