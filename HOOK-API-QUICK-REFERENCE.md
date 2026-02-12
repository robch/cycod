# Hook API Quick Reference

## Overview
The pipeline now supports comprehensive hook coverage at all critical points in the conversation flow. Use semantic extension methods for easy, discoverable hook registration.

## Quick Start

```csharp
using CycoDev.ChatPipeline;

var pipeline = ChatPipelineFactory.CreateStandardPipeline()
    // Add hooks using semantic methods
    .AddPostUserInput(context => {
        Console.WriteLine($"User said: {context.UserPrompt}");
    })
    .AddPreToolCall(context => {
        Console.WriteLine("About to call functions...");
    })
    .AddPostToolResults(context => {
        Console.WriteLine("Functions completed!");
    });
```

## All Available Hook Methods

### User Input
```csharp
.AddPostUserInput(...)           // After user input captured
.AddPreUserMessageAdd(...)       // Before user message added
.AddPostUserMessageAdd(...)      // After user message added
```

### AI Streaming
```csharp
.AddPreAIStreaming(...)          // Before AI streaming starts
.AddPostAIStreaming(...)         // After AI streaming completes
```

### Assistant Messages
```csharp
.AddPreAssistantMessage(...)                    // Before any assistant message
.AddPostAssistantMessage(...)                   // After any assistant message
.AddPreAssistantMessageWithToolCalls(...)       // Before assistant message with tool calls
.AddPostAssistantMessageWithToolCalls(...)      // After assistant message with tool calls
```

### Tool Calls
```csharp
.AddPostToolDetection(...)       // After tool calls detected
.AddPreToolCall(...)             // Before function execution
.AddPostToolCall(...)            // After function execution
```

### Tool Results
```csharp
.AddPreToolResults(...)          // Before tool results added
.AddPostToolResults(...)         // After tool results added
```

### Injected Content
```csharp
.AddPreInjectedContent(...)      // Before injected content added
.AddPostInjectedContent(...)     // After injected content added
```

### Loop Control
```csharp
.AddPreLoopContinue(...)         // Before loop continues
.AddPostLoopIteration(...)       // After loop iteration
```

### Pipeline Lifecycle
```csharp
.AddPrePipelineStart(...)        // Before pipeline starts
.AddPostPipelineComplete(...)    // After pipeline completes
```

### Catch-All (Fires for ALL message types)
```csharp
.AddPreMessageAdd(...)           // Before ANY message added
.AddPostMessageAdd(...)          // After ANY message added
```

## Overload Options

Each method has 5 overloads for maximum flexibility:

### 1. Full Async Lambda (Full Control)
```csharp
.AddPostUserInput((context, data) => 
{
    // Full access to context and hook data
    // Can return HookResult to control pipeline
    Console.WriteLine($"Hook: {data.HookPoint} at {data.Timestamp}");
    return Task.FromResult(HookResult.Continue());
})
```

### 2. Simple Async Lambda (Most Common)
```csharp
.AddPostUserInput(async context => 
{
    // Just context, no hook data
    // Automatically returns HookResult.Continue()
    await LogToDatabase(context.Messages);
})
```

### 3. Simple Sync Action (Most Concise)
```csharp
.AddPostUserInput(context => 
{
    // Simplest form - sync code only
    Console.WriteLine($"Message count: {context.Messages.Count}");
})
```

### 4. Full Sync Action
```csharp
.AddPostUserInput((context, data) => 
{
    // Sync version with full access
    Log($"Stage: {data.StageName}");
})
```

### 5. Handler Object (Advanced)
```csharp
.AddPostUserInput(new MyCustomHookHandler())
```

## Common Use Cases

### 1. Logging All Messages
```csharp
.AddPostMessageAdd(context => 
{
    var lastMessage = context.Messages.LastOrDefault();
    Logger.Log($"[{lastMessage?.Role}] {lastMessage?.Text}");
})
```

### 2. Tool Call Approval/Logging
```csharp
.AddPreToolCall(context => 
{
    var toolNames = string.Join(", ", 
        context.Pending.PendingToolCalls.Select(t => t.Name));
    Console.WriteLine($"About to call: {toolNames}");
})
```

### 3. Content Filtering
```csharp
.AddPreUserMessageAdd(context => 
{
    var userMessage = context.Messages.LastOrDefault(m => m.Role == ChatRole.User);
    if (ContainsBadWords(userMessage?.Text))
    {
        throw new InvalidOperationException("Inappropriate content detected");
    }
})
```

### 4. Usage Tracking
```csharp
.AddPostAIStreaming(context => 
{
    var responseLength = context.Pending.ResponseContent.Length;
    UsageTracker.RecordTokens(responseLength);
})
```

### 5. Interruption Handling
```csharp
.AddPreAIStreaming(context => 
{
    if (InterruptManager.IsInterrupted())
    {
        context.State.ShouldExitLoop = true;
    }
})
```

### 6. Conversation History Management
```csharp
.AddPostPipelineComplete(context => 
{
    if (context.Messages.Count > 100)
    {
        // Trim old messages
        var toRemove = context.Messages.Count - 100;
        context.Messages.RemoveRange(0, toRemove);
    }
})
```

## Advanced: Custom Hook Handler

```csharp
public class MyCustomHook : IHookHandler
{
    public string Name => "MyCustomHook";
    public int Priority => 0;  // Lower = executes first
    
    public async Task<HookResult> HandleAsync(ChatContext context, HookData data)
    {
        // Your custom logic here
        await DoSomethingAsync(context);
        
        // Control pipeline behavior
        return HookResult.Continue();  // Or Skip/Exit
    }
}

// Use it:
pipeline.AddPostUserInput(new MyCustomHook());
```

## Hook Result Options

```csharp
// Continue normally
return HookResult.Continue();

// Skip next stage
return HookResult.Skip();

// Exit pipeline loop
return HookResult.Exit();

// Modify context
var result = HookResult.Continue();
result.ModifiedContext = true;
return result;
```

## Best Practices

1. **Use simple lambdas** for most hooks - they're concise and readable
2. **Use catch-all hooks** (AddPreMessageAdd/AddPostMessageAdd) for cross-cutting concerns like logging
3. **Keep hooks fast** - they execute synchronously in the pipeline flow
4. **Don't throw exceptions** from hooks unless you want to stop the pipeline
5. **Use Priority** for ordering when multiple hooks are at the same point
6. **Modify context cautiously** - it affects downstream stages

## Examples

### Security Filter
```csharp
pipeline.AddPreMessageAdd(context => 
{
    var lastMsg = context.Messages.LastOrDefault();
    if (SecurityFilter.IsSuspicious(lastMsg?.Text))
    {
        Logger.Warn("Suspicious content blocked");
        throw new SecurityException("Content blocked");
    }
});
```

### Token Budgeting
```csharp
var tokenBudget = new TokenBudget(maxTokens: 100000);

pipeline
    .AddPreAIStreaming(context => 
    {
        if (!tokenBudget.CanUseTokens(estimatedCost: 1000))
        {
            context.State.ShouldExitLoop = true;
        }
    })
    .AddPostAIStreaming(context => 
    {
        var used = EstimateTokens(context.Pending.ResponseContent);
        tokenBudget.ConsumeTokens(used);
    });
```

### Conversation Analysis
```csharp
var analyzer = new ConversationAnalyzer();

pipeline.AddPostMessageAdd(context => 
{
    analyzer.AnalyzeMessage(context.Messages.Last());
    
    if (analyzer.DetectLooping())
    {
        Console.WriteLine("Warning: Conversation may be looping!");
    }
});
```

## Migration Guide

### Old Way (Low-Level)
```csharp
pipeline.AddHook(HookPoint.PostUserInput, new MyHook());
pipeline.AddHook(HookPoint.PreFunctionExecution, new MyToolHook());
```

### New Way (Semantic)
```csharp
pipeline
    .AddPostUserInput(new MyHook())
    .AddPreToolCall(new MyToolHook());
```

### Even Better (Lambdas)
```csharp
pipeline
    .AddPostUserInput(context => DoSomething(context))
    .AddPreToolCall(context => DoToolStuff(context));
```

## Troubleshooting

**Q: My hook isn't firing**
- Check that you're registering the hook BEFORE calling pipeline.ExecuteAsync()
- Verify you're using the correct hook point for your use case

**Q: Pipeline stops after my hook**
- Your hook is probably throwing an exception
- Wrap in try-catch or return HookResult.Continue()

**Q: Hook fires multiple times**
- Catch-all hooks (AddPreMessageAdd/AddPostMessageAdd) fire for ALL message types
- Use specific hooks if you only want specific message types

**Q: Hooks execute in wrong order**
- Set Priority property on your IHookHandler (lower = first)
- Default priority is 0

---

For more details, see:
- `IMPLEMENTATION-COMPLETE.md` - Full implementation details
- `todo/hook-architecture-gaps.md` - Original requirements
- `src/cycod/ChatPipeline/ChatPipelineExtensions.cs` - Source code
