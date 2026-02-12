# Pipeline Hook Usage Examples

This document demonstrates how to use the new pipeline hook system in cycod.

## Basic Usage (Default Pipeline)

```csharp
// Default behavior - uses standard pipeline with no hooks
var chat = new FunctionCallingChat(chatClient, systemPrompt, factory, options);
await chat.CompleteChatStreamingAsync("Hello");
```

## Custom Pipeline with Hooks

### Example 1: Simple Logging Hook

```csharp
// Create pipeline with logging hook
var pipeline = ChatPipelineFactory.CreateStandardPipeline()
    .AddPostUserInput(context => 
    {
        Console.WriteLine($"[LOG] User said: {context.UserPrompt}");
    });

// Pass custom pipeline to FunctionCallingChat
var chat = new FunctionCallingChat(
    chatClient, 
    systemPrompt, 
    factory, 
    options,
    maxOutputTokens: null,
    pipeline: pipeline);

await chat.CompleteChatStreamingAsync("Hello");
// Output: [LOG] User said: Hello
```

### Example 2: Tool Call Monitoring

```csharp
var pipeline = ChatPipelineFactory.CreateStandardPipeline()
    .AddPreToolCall(context => 
    {
        var toolNames = string.Join(", ", 
            context.Pending.PendingToolCalls.Select(t => t.Name));
        Console.WriteLine($"[TOOL] About to call: {toolNames}");
    })
    .AddPostToolResults(context => 
    {
        Console.WriteLine($"[TOOL] Completed {context.Pending.PendingToolCalls.Count} tool calls");
    });

var chat = new FunctionCallingChat(chatClient, systemPrompt, factory, options, pipeline: pipeline);
```

### Example 3: Conversation Analytics

```csharp
var messageCount = 0;

var pipeline = ChatPipelineFactory.CreateStandardPipeline()
    .AddPostMessageAdd(context => 
    {
        messageCount++;
        Console.WriteLine($"[STATS] Total messages: {messageCount}");
        Console.WriteLine($"[STATS] In context: {context.Messages.Count}");
    });

var chat = new FunctionCallingChat(chatClient, systemPrompt, factory, options, pipeline: pipeline);
```

### Example 4: Content Filtering

```csharp
var pipeline = ChatPipelineFactory.CreateStandardPipeline()
    .AddPreUserMessageAdd(context => 
    {
        var userMsg = context.UserPrompt;
        if (userMsg.Contains("badword"))
        {
            Console.WriteLine("[FILTER] Inappropriate content detected!");
            throw new InvalidOperationException("Content policy violation");
        }
    });

var chat = new FunctionCallingChat(chatClient, systemPrompt, factory, options, pipeline: pipeline);
```

### Example 5: Multiple Hooks

```csharp
var pipeline = ChatPipelineFactory.CreateStandardPipeline()
    // Log all user input
    .AddPostUserInput(context => 
        Logger.Log($"User: {context.UserPrompt}"))
    
    // Monitor AI streaming
    .AddPreAIStreaming(context => 
        Logger.Log("AI is thinking..."))
    .AddPostAIStreaming(context => 
        Logger.Log($"AI responded with {context.Pending.ResponseContent.Length} chars"))
    
    // Track tool usage
    .AddPreToolCall(context => 
        Metrics.IncrementToolCallCount())
    
    // Catch-all message logging
    .AddPostMessageAdd(context => 
        Logger.Log($"Message added: {context.Messages.Last().Role}"));

var chat = new FunctionCallingChat(chatClient, systemPrompt, factory, options, pipeline: pipeline);
```

### Example 6: Adding Hooks After Construction

```csharp
var chat = new FunctionCallingChat(chatClient, systemPrompt, factory, options);

// Access pipeline and add hooks later
chat.Pipeline.AddPostUserInput(context => 
{
    Console.WriteLine("Hook added after construction!");
});

await chat.CompleteChatStreamingAsync("Hello");
```

### Example 7: Async Hooks

```csharp
var pipeline = ChatPipelineFactory.CreateStandardPipeline()
    .AddPostUserInput(async context => 
    {
        // Can use async operations in hooks
        await DatabaseLogger.LogUserInputAsync(context.UserPrompt);
    });

var chat = new FunctionCallingChat(chatClient, systemPrompt, factory, options, pipeline: pipeline);
```

### Example 8: Hook with Full Control

```csharp
var pipeline = ChatPipelineFactory.CreateStandardPipeline()
    .AddPreToolCall((context, hookData) => 
    {
        Console.WriteLine($"Hook: {hookData.HookPoint}");
        Console.WriteLine($"Stage: {hookData.StageName}");
        Console.WriteLine($"Time: {hookData.Timestamp}");
        
        // Return HookResult for advanced control
        return Task.FromResult(HookResult.Continue());
    });

var chat = new FunctionCallingChat(chatClient, systemPrompt, factory, options, pipeline: pipeline);
```

### Example 9: Custom Hook Handler

```csharp
public class UsageTrackingHook : IHookHandler
{
    public string Name => "UsageTracker";
    public int Priority => 0;
    
    public async Task<HookResult> HandleAsync(ChatContext context, HookData data)
    {
        await UsageDatabase.RecordEventAsync(data.HookPoint.ToString());
        return HookResult.Continue();
    }
}

var pipeline = ChatPipelineFactory.CreateStandardPipeline()
    .AddPostAIStreaming(new UsageTrackingHook());

var chat = new FunctionCallingChat(chatClient, systemPrompt, factory, options, pipeline: pipeline);
```

## Available Hook Points

All semantic hook methods are available:

### User Input
- `AddPostUserInput()` - After user input captured
- `AddPreUserMessageAdd()` - Before user message added
- `AddPostUserMessageAdd()` - After user message added

### AI Streaming
- `AddPreAIStreaming()` - Before AI streaming
- `AddPostAIStreaming()` - After AI streaming

### Assistant Messages
- `AddPreAssistantMessage()` - Before assistant message
- `AddPostAssistantMessage()` - After assistant message
- `AddPreAssistantMessageWithToolCalls()` - Before assistant message with tools
- `AddPostAssistantMessageWithToolCalls()` - After assistant message with tools

### Tool Calls
- `AddPostToolDetection()` - After tool calls detected
- `AddPreToolCall()` - Before function execution
- `AddPostToolCall()` - After function execution

### Tool Results
- `AddPreToolResults()` - Before tool results added
- `AddPostToolResults()` - After tool results added

### Injected Content
- `AddPreInjectedContent()` - Before injected content
- `AddPostInjectedContent()` - After injected content

### Loop Control
- `AddPreLoopContinue()` - Before loop continues
- `AddPostLoopIteration()` - After loop iteration

### Pipeline Lifecycle
- `AddPrePipelineStart()` - Before pipeline starts
- `AddPostPipelineComplete()` - After pipeline completes

### Catch-All Hooks
- `AddPreMessageAdd()` - Before ANY message added
- `AddPostMessageAdd()` - After ANY message added

## Hook Method Overloads

Each hook method has 5 overloads:

```csharp
// 1. Full async lambda with result
.AddPostUserInput((context, data) => Task.FromResult(HookResult.Continue()))

// 2. Simple async lambda
.AddPostUserInput(async context => await DoSomethingAsync(context))

// 3. Simple sync action (most common)
.AddPostUserInput(context => Console.WriteLine("User input"))

// 4. Full sync action
.AddPostUserInput((context, data) => Console.WriteLine($"Stage: {data.StageName}"))

// 5. Handler object
.AddPostUserInput(new MyCustomHookHandler())
```

## Best Practices

1. **Keep hooks fast** - They execute synchronously in the pipeline flow
2. **Use simple lambdas** for most cases - cleaner and more readable
3. **Use catch-all hooks** (AddPreMessageAdd/AddPostMessageAdd) for cross-cutting concerns
4. **Don't throw exceptions** unless you want to stop the pipeline
5. **Add hooks before execution** - Configure pipeline before passing to FunctionCallingChat

## Limitations

- User message hooks (PreUserMessageAdd/PostUserMessageAdd) are not yet executed due to architectural constraint
- User message is added before pipeline starts, so hooks can't fire
- TODO: Move user message addition into pipeline as UserInputStage to enable these hooks

## See Also

- `HOOK-API-QUICK-REFERENCE.md` - Complete API reference
- `IMPLEMENTATION-COMPLETE.md` - Implementation details
- `src/cycod/ChatPipeline/ChatPipelineExtensions.cs` - Source code
