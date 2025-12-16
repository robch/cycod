using System;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;

namespace Cycod.ChatPipeline.Stages;

/// <summary>
/// Stage that streams AI responses and accumulates content.
/// This is a long-running stage that supports cancellation.
/// </summary>
public class AIStreamingStage : IPipelineStage
{
    private readonly IChatClient _chatClient;
    private readonly ChatOptions? _options;
    
    public string Name => "AIStreaming";
    public HookPoint PreHookPoint => HookPoint.PreAIStreaming;
    public HookPoint PostHookPoint => HookPoint.PostAIStreaming;
    
    public AIStreamingStage(IChatClient chatClient, ChatOptions? options = null)
    {
        _chatClient = chatClient;
        _options = options;
    }
    
    public async Task<StageResult> ExecuteAsync(ChatContext context)
    {
        // Get cancellation token from context
        var cancellationToken = context.CancellationTokenSource.Token;
        
        // Clear previous streaming data
        context.Pending.StreamingContent.Clear();
        context.Pending.StreamingUpdates.Clear();
        
        try
        {
            // Stream AI response with cancellation support
            await foreach (var update in _chatClient.GetStreamingResponseAsync(
                context.Messages, 
                _options, 
                cancellationToken))
            {
                // Check for cancellation frequently (maintains responsiveness)
                cancellationToken.ThrowIfCancellationRequested();
                
                // Accumulate streaming updates
                context.Pending.StreamingUpdates.Add(update);
                
                // Accumulate text content
                if (update.Text != null)
                {
                    context.Pending.StreamingContent.Append(update.Text);
                }
                
                // Invoke streaming callback if provided
                context.Properties.TryGetValue("StreamingCallback", out var callback);
                if (callback is Action<ChatResponseUpdate> streamingCallback)
                {
                    streamingCallback(update);
                }
            }
            
            // Create pending assistant message from accumulated content
            var content = context.Pending.StreamingContent.ToString();
            context.Pending.PendingAssistantMessage = new ChatMessage(ChatRole.Assistant, content);
            
            return StageResult.Continue();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Graceful cancellation - return what we have so far
            var partialContent = context.Pending.StreamingContent.ToString();
            if (!string.IsNullOrEmpty(partialContent))
            {
                context.Pending.PendingAssistantMessage = new ChatMessage(ChatRole.Assistant, partialContent);
            }
            
            // Re-throw so pipeline can handle interrupt
            throw;
        }
    }
    
    public StageMetadata GetMetadata()
    {
        return new StageMetadata
        {
            Name = Name,
            Description = "Streams AI responses with cancellation support",
            EstimatedDuration = TimeSpan.FromSeconds(5),
            IsLongRunning = true,
            Properties = { ["SupportsCancellation"] = true }
        };
    }
}

/// <summary>
/// Stage that detects function calls in the AI response.
/// This is a fast, synchronous stage.
/// </summary>
public class FunctionDetectionStage : IPipelineStage
{
    public string Name => "FunctionDetection";
    public HookPoint PreHookPoint => HookPoint.PostFunctionDetection;
    public HookPoint PostHookPoint => HookPoint.PreToolCall;
    
    public Task<StageResult> ExecuteAsync(ChatContext context)
    {
        // Clear previous function call data
        context.Pending.PendingToolCalls.Clear();
        
        // Check if we have streaming updates to analyze
        if (context.Pending.StreamingUpdates.Count == 0)
        {
            return Task.FromResult(StageResult.Continue());
        }
        
        // Detect function calls from streaming updates
        foreach (var update in context.Pending.StreamingUpdates)
        {
            // Check for function call content
            foreach (var content in update.Contents)
            {
                if (content is FunctionCallContent functionCall)
                {
                    context.Pending.PendingToolCalls.Add(new FunctionCall
                    {
                        CallId = functionCall.CallId,
                        Name = functionCall.Name,
                        Arguments = functionCall.Arguments?.ToString() ?? "{}"
                    });
                }
            }
        }
        
        // If no function calls detected, we're done
        if (context.Pending.PendingToolCalls.Count == 0)
        {
            return Task.FromResult(StageResult.Continue());
        }
        
        // Function calls detected - will proceed to execution stage
        return Task.FromResult(StageResult.Continue());
    }
    
    public StageMetadata GetMetadata()
    {
        return new StageMetadata
        {
            Name = Name,
            Description = "Detects function calls in AI response",
            EstimatedDuration = TimeSpan.FromMilliseconds(10),
            IsLongRunning = false
        };
    }
}

/// <summary>
/// Stage that executes detected function calls.
/// This can be long-running depending on the functions.
/// </summary>
public class FunctionExecutionStage : IPipelineStage
{
    private readonly IFunctionFactory _functionFactory;
    
    public string Name => "FunctionExecution";
    public HookPoint PreHookPoint => HookPoint.PreToolCall;
    public HookPoint PostHookPoint => HookPoint.PostToolCall;
    
    public FunctionExecutionStage(IFunctionFactory functionFactory)
    {
        _functionFactory = functionFactory;
    }
    
    public async Task<StageResult> ExecuteAsync(ChatContext context)
    {
        // If no tool calls, nothing to do
        if (context.Pending.PendingToolCalls.Count == 0)
        {
            return StageResult.Continue();
        }
        
        // Get cancellation token for long-running functions
        var cancellationToken = context.CancellationTokenSource.Token;
        
        // Clear previous results
        context.Pending.PendingToolResults.Clear();
        
        // Execute each function call
        foreach (var toolCall in context.Pending.PendingToolCalls)
        {
            try
            {
                // Check for cancellation before each call
                cancellationToken.ThrowIfCancellationRequested();
                
                // Get function from factory
                var function = _functionFactory.GetFunction(toolCall.Name);
                if (function == null)
                {
                    context.Pending.PendingToolResults.Add(new FunctionResult
                    {
                        CallId = toolCall.CallId,
                        Content = $"Function '{toolCall.Name}' not found",
                        Success = false
                    });
                    continue;
                }
                
                // Invoke function callback if provided (for approval/logging)
                context.Properties.TryGetValue("FunctionCallCallback", out var callback);
                if (callback is Action<string, string> functionCallCallback)
                {
                    functionCallCallback(toolCall.Name, toolCall.Arguments);
                }
                
                // Execute the function
                var result = await function.InvokeAsync(toolCall.Arguments, cancellationToken);
                
                context.Pending.PendingToolResults.Add(new FunctionResult
                {
                    CallId = toolCall.CallId,
                    Content = result?.ToString() ?? string.Empty,
                    Success = true
                });
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Function was cancelled - add cancelled result
                context.Pending.PendingToolResults.Add(new FunctionResult
                {
                    CallId = toolCall.CallId,
                    Content = "Function execution cancelled",
                    Success = false
                });
                
                // Re-throw to let pipeline handle
                throw;
            }
            catch (Exception ex)
            {
                // Function failed - record error
                context.Pending.PendingToolResults.Add(new FunctionResult
                {
                    CallId = toolCall.CallId,
                    Content = $"Error: {ex.Message}",
                    Success = false,
                    Error = ex
                });
            }
        }
        
        // If we executed any functions, we need to loop back for more AI
        if (context.Pending.PendingToolResults.Count > 0)
        {
            return StageResult.Continue();
        }
        
        return StageResult.Continue();
    }
    
    public StageMetadata GetMetadata()
    {
        return new StageMetadata
        {
            Name = Name,
            Description = "Executes detected function calls",
            EstimatedDuration = TimeSpan.FromSeconds(2),
            IsLongRunning = true,
            Properties = { ["SupportsCancellation"] = true }
        };
    }
}

/// <summary>
/// Stage that adds messages to the conversation history.
/// This is a fast, synchronous stage.
/// </summary>
public class MessagePersistenceStage : IPipelineStage
{
    public string Name => "MessagePersistence";
    public HookPoint PreHookPoint => HookPoint.PreMessageAdd;
    public HookPoint PostHookPoint => HookPoint.PostMessageAdd;
    
    public Task<StageResult> ExecuteAsync(ChatContext context)
    {
        // Add pending user message if present
        if (context.Pending.PendingUserMessage != null)
        {
            context.Messages.Add(context.Pending.PendingUserMessage);
            context.Pending.PendingUserMessage = null;
        }
        
        // Add pending assistant message if present
        if (context.Pending.PendingAssistantMessage != null)
        {
            context.Messages.Add(context.Pending.PendingAssistantMessage);
            context.Pending.PendingAssistantMessage = null;
        }
        
        // Add pending tool results as messages
        foreach (var toolResult in context.Pending.PendingToolResults)
        {
            var resultMessage = new ChatMessage(
                ChatRole.Tool,
                new FunctionResultContent(toolResult.CallId, toolResult.Content)
            );
            context.Messages.Add(resultMessage);
        }
        
        if (context.Pending.PendingToolResults.Count > 0)
        {
            context.Pending.PendingToolResults.Clear();
        }
        
        return Task.FromResult(StageResult.Continue());
    }
    
    public StageMetadata GetMetadata()
    {
        return new StageMetadata
        {
            Name = Name,
            Description = "Adds pending messages to conversation history",
            EstimatedDuration = TimeSpan.FromMilliseconds(1),
            IsLongRunning = false
        };
    }
}

/// <summary>
/// Stage that decides whether to continue the loop or exit.
/// This is a fast, decision-only stage.
/// </summary>
public class LoopDecisionStage : IPipelineStage
{
    public string Name => "LoopDecision";
    public HookPoint PreHookPoint => HookPoint.PostLoopIteration;
    public HookPoint PostHookPoint => HookPoint.PreNextIteration;
    
    public Task<StageResult> ExecuteAsync(ChatContext context)
    {
        // Increment loop iteration
        context.State.LoopIteration++;
        
        // Check if we have more function calls to process
        var hasPendingFunctionCalls = context.Pending.PendingToolCalls.Count > 0;
        
        // Check if context says to exit
        if (context.State.ShouldExitLoop || context.Pending.ShouldExitLoop)
        {
            return Task.FromResult(StageResult.Exit());
        }
        
        // Check if we're interrupted
        if (context.State.IsInterrupted)
        {
            return Task.FromResult(StageResult.Exit());
        }
        
        // If we have pending function calls, continue looping
        if (hasPendingFunctionCalls)
        {
            return Task.FromResult(StageResult.Continue());
        }
        
        // No more work - exit loop
        return Task.FromResult(StageResult.Exit());
    }
    
    public StageMetadata GetMetadata()
    {
        return new StageMetadata
        {
            Name = Name,
            Description = "Decides whether to continue loop or exit",
            EstimatedDuration = TimeSpan.FromMilliseconds(1),
            IsLongRunning = false
        };
    }
}

/// <summary>
/// Placeholder interface for function factory.
/// Actual implementation would depend on existing function calling infrastructure.
/// </summary>
public interface IFunctionFactory
{
    IFunction? GetFunction(string name);
}

public interface IFunction
{
    Task<object?> InvokeAsync(string arguments, CancellationToken cancellationToken);
}
