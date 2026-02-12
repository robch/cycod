# Hook Architecture Gaps - To Be Addressed in Attempt 2+

## Critical Gap: Message Addition Hook Coverage

### Current State: Only 1 of 5 Message Additions Has Hook Coverage

| # | Message Type | Location | Current Hooks | Status |
|---|-------------|----------|---------------|--------|
| 1 | User (initial) | `FunctionCallingChat.CompleteChatStreamingAsync:84` | ❌ NONE | Before pipeline starts |
| 2 | Assistant (w/ tool calls) | `FunctionExecutionStage.ExecuteAsync:37` | ❌ NONE | Inside stage, no hooks |
| 3 | Tool (results) | `FunctionExecutionStage.ExecuteAsync:79` | ❌ NONE | Inside stage, no hooks |
| 4 | User (injected content) | `FunctionExecutionStage.ExecuteAsync:95` | ❌ NONE | Inside stage, no hooks |
| 5 | Assistant (final) | `MessagePersistenceStage.ExecuteAsync:26` | ✅ PreMessageAdd / PostMessageAdd | Only one with hooks! |

**Problem:** 80% of message additions bypass hooks entirely!

---

## What Needs to Be Added

### 1. Additional Hook Points (Add to HookPoint.cs)

```csharp
public enum HookPoint
{
    // ... existing hooks ...
    
    // User message hooks
    PreUserMessageAdd,        // Before user message added
    PostUserMessageAdd,       // After user message added
    
    // Assistant message variants
    PreAssistantMessageAdd,   // Before any assistant message
    PostAssistantMessageAdd,  // After any assistant message
    PreAssistantMessageWithToolCalls,  // Specific to messages with tool calls
    PostAssistantMessageWithToolCalls, // Specific to messages with tool calls
    
    // Tool result hooks (PreFunctionResultAdd exists but unused)
    PreToolResultsAdd,        // Before tool results added to conversation
    PostToolResultsAdd,       // After tool results added to conversation
    
    // Injected content hooks
    PreInjectedContentAdd,    // Before system injects additional user content
    PostInjectedContentAdd,   // After system injects additional user content
}
```

### 2. Hook Execution at ALL Message Additions

**Option A: Inline Hook Execution**
Add hook execution directly in stages before/after each message add:
```csharp
// In FunctionExecutionStage, line 37:
await ExecuteHooks(HookPoint.PreAssistantMessageWithToolCalls, context);
context.Messages.Add(new ChatMessage(ChatRole.Assistant, assistantContent));
await ExecuteHooks(HookPoint.PostAssistantMessageWithToolCalls, context);
```

**Option B: Helper Method**
Create `AddMessageWithHooks()` helper:
```csharp
private async Task AddMessageWithHooks(
    ChatContext context, 
    ChatMessage message, 
    HookPoint preHook, 
    HookPoint postHook)
{
    await ExecuteHooks(preHook, context);
    context.Messages.Add(message);
    await ExecuteHooks(postHook, context);
}
```

**Option C: Centralized Message Add Stage**
Create `MessageAddStage` that all message additions funnel through:
- Stages queue messages in `context.Pending.MessagesToAdd`
- MessageAddStage processes the queue with hooks
- Requires refactoring all direct `.Messages.Add()` calls

**Recommendation:** Option B (helper method) - Good balance of simplicity and coverage.

### 3. Fluent Builder API - Semantic Methods

Add extension methods or methods on `ChatPipeline` for user-friendly hook registration:

```csharp
public static class ChatPipelineExtensions
{
    // User input hooks
    public static IChatPipeline AddPreUserInput(this IChatPipeline pipeline, IHookHandler handler);
    public static IChatPipeline AddPostUserInput(this IChatPipeline pipeline, IHookHandler handler);
    
    // Assistant message hooks
    public static IChatPipeline AddPreAssistantMessage(this IChatPipeline pipeline, IHookHandler handler);
    public static IChatPipeline AddPostAssistantMessage(this IChatPipeline pipeline, IHookHandler handler);
    
    // Tool call hooks
    public static IChatPipeline AddPreToolCall(this IChatPipeline pipeline, IHookHandler handler);
    public static IChatPipeline AddPostToolCall(this IChatPipeline pipeline, IHookHandler handler);
    public static IChatPipeline AddPreToolResults(this IChatPipeline pipeline, IHookHandler handler);
    public static IChatPipeline AddPostToolResults(this IChatPipeline pipeline, IHookHandler handler);
    
    // AI streaming hooks
    public static IChatPipeline AddPreAIStreaming(this IChatPipeline pipeline, IHookHandler handler);
    public static IChatPipeline AddPostAIStreaming(this IChatPipeline pipeline, IHookHandler handler);
    
    // Loop control hooks
    public static IChatPipeline AddPreLoopContinue(this IChatPipeline pipeline, IHookHandler handler);
    public static IChatPipeline AddPostLoopIteration(this IChatPipeline pipeline, IHookHandler handler);
    
    // ... etc for all hook points
}
```

### 4. Multiple Overloads for Each Semantic Method

Each semantic method should have overloads for different registration styles:

```csharp
// Handler object
public static IChatPipeline AddPostUserInput(
    this IChatPipeline pipeline, 
    IHookHandler handler)
{
    return pipeline.AddHook(HookPoint.PostUserInput, handler);
}

// Async lambda
public static IChatPipeline AddPostUserInput(
    this IChatPipeline pipeline, 
    Func<ChatContext, HookData, Task<HookResult>> handler)
{
    return pipeline.AddHook(HookPoint.PostUserInput, new LambdaHookHandler(handler));
}

// Sync lambda (auto-wrapped in Task)
public static IChatPipeline AddPostUserInput(
    this IChatPipeline pipeline, 
    Action<ChatContext, HookData> handler)
{
    return pipeline.AddHook(HookPoint.PostUserInput, new ActionHookHandler(handler));
}

// Async lambda with just context (no HookData)
public static IChatPipeline AddPostUserInput(
    this IChatPipeline pipeline, 
    Func<ChatContext, Task> handler)
{
    return pipeline.AddHook(HookPoint.PostUserInput, 
        new SimpleLambdaHookHandler(handler));
}

// Sync action with just context
public static IChatPipeline AddPostUserInput(
    this IChatPipeline pipeline, 
    Action<ChatContext> handler)
{
    return pipeline.AddHook(HookPoint.PostUserInput, 
        new SimpleActionHookHandler(handler));
}

// With priority and name
public static IChatPipeline AddPostUserInput(
    this IChatPipeline pipeline, 
    IHookHandler handler,
    int priority,
    string name)
{
    handler.Priority = priority;
    handler.Name = name;
    return pipeline.AddHook(HookPoint.PostUserInput, handler);
}
```

**Helper classes needed:**
```csharp
internal class LambdaHookHandler : IHookHandler
{
    private readonly Func<ChatContext, HookData, Task<HookResult>> _handler;
    public string Name { get; set; } = "LambdaHook";
    public int Priority { get; set; } = 0;
    
    public LambdaHookHandler(Func<ChatContext, HookData, Task<HookResult>> handler)
    {
        _handler = handler;
    }
    
    public async Task<HookResult> HandleAsync(ChatContext context, HookData data)
    {
        return await _handler(context, data);
    }
}

internal class ActionHookHandler : IHookHandler
{
    private readonly Action<ChatContext, HookData> _handler;
    // ... similar pattern
}

internal class SimpleLambdaHookHandler : IHookHandler
{
    private readonly Func<ChatContext, Task> _handler;
    // ... similar pattern, returns HookResult.Continue()
}

internal class SimpleActionHookHandler : IHookHandler
{
    private readonly Action<ChatContext> _handler;
    // ... similar pattern, returns HookResult.Continue()
}
```

### 5. Generic Catch-All Hook Methods

Methods that register at MULTIPLE hook points for catch-all scenarios:

```csharp
// Fires before ANY message is added
public static IChatPipeline AddPreMessageAdd(
    this IChatPipeline pipeline, 
    IHookHandler handler)
{
    return pipeline
        .AddHook(HookPoint.PreUserMessageAdd, handler)
        .AddHook(HookPoint.PreAssistantMessageAdd, handler)
        .AddHook(HookPoint.PreAssistantMessageWithToolCalls, handler)
        .AddHook(HookPoint.PreToolResultsAdd, handler)
        .AddHook(HookPoint.PreInjectedContentAdd, handler);
}

// Fires after ANY message is added
public static IChatPipeline AddPostMessageAdd(
    this IChatPipeline pipeline, 
    IHookHandler handler)
{
    return pipeline
        .AddHook(HookPoint.PostUserMessageAdd, handler)
        .AddHook(HookPoint.PostAssistantMessageAdd, handler)
        .AddHook(HookPoint.PostAssistantMessageWithToolCalls, handler)
        .AddHook(HookPoint.PostToolResultsAdd, handler)
        .AddHook(HookPoint.PostInjectedContentAdd, handler);
}

// With overloads for lambda/action like semantic methods
public static IChatPipeline AddPreMessageAdd(
    this IChatPipeline pipeline, 
    Action<ChatContext, HookData> handler)
{
    return AddPreMessageAdd(pipeline, new ActionHookHandler(handler));
}
// ... etc
```

---

## Example Usage (What Users Will Experience)

### Current API (Verbose, Not Discoverable):
```csharp
var pipeline = new ChatPipeline()
    .AddStage(new AIStreamingStage())
    .AddStage(new FunctionExecutionStage())
    .AddHook(HookPoint.PostUserInput, new MyUserInputHook())  // Not clear what this does
    .AddHook(HookPoint.PreFunctionExecution, new MyToolHook());
```

### Future API (Semantic, Discoverable, Flexible):
```csharp
var pipeline = new ChatPipeline()
    .AddStage(new AIStreamingStage())
    .AddStage(new FunctionExecutionStage())
    
    // Semantic methods - clear intent
    .AddPostUserInput(new MyUserInputHook())
    .AddPreToolCall(new MyToolApprovalHook())
    
    // Lambda overloads - concise
    .AddPostAssistantMessage((context, data) => 
    {
        Console.WriteLine($"Assistant said: {context.Pending.ResponseContent}");
        return Task.FromResult(HookResult.Continue());
    })
    
    // Simple action overload - even more concise
    .AddPostToolResults(context => 
    {
        // Analyze tool results
        LogToolUsage(context);
    })
    
    // Generic catch-all - for cross-cutting concerns
    .AddPreMessageAdd(new SecurityFilterHook())  // Fires before ANY message add
    .AddPostMessageAdd(new ConversationLoggerHook());  // Fires after ANY message add
```

---

## Implementation Priority

### High Priority (Blocks Real Usage):
1. ✅ Add missing hook points to HookPoint enum
2. ✅ Implement hook execution at all 5 message addition points
3. ✅ Create AddMessageWithHooks() helper or refactor to MessageAddStage

### Medium Priority (Usability):
4. ✅ Create semantic builder methods (AddPostUserInput, AddPreToolCall, etc.)
5. ✅ Create lambda/action overloads for semantic methods
6. ✅ Create helper hook handler classes (LambdaHookHandler, ActionHookHandler, etc.)

### Lower Priority (Nice to Have):
7. ⭐ Create generic catch-all methods (AddPreMessageAdd, AddPostMessageAdd)
8. ⭐ Add priority and name overloads
9. ⭐ Create pipeline builder class for more complex scenarios

---

## Files That Need Changes

### New Files to Create:
1. `src/cycod/ChatPipeline/ChatPipelineExtensions.cs` - Semantic builder methods
2. `src/cycod/ChatPipeline/Hooks/LambdaHookHandler.cs` - Lambda wrapper
3. `src/cycod/ChatPipeline/Hooks/ActionHookHandler.cs` - Action wrapper
4. `src/cycod/ChatPipeline/Hooks/SimpleLambdaHookHandler.cs` - Simple lambda wrapper
5. `src/cycod/ChatPipeline/Hooks/SimpleActionHookHandler.cs` - Simple action wrapper

### Files to Modify:
1. `src/cycod/ChatPipeline/HookPoint.cs` - Add missing hook points
2. `src/cycod/ChatPipeline/Stages/FunctionExecutionStage.cs` - Add hooks around 3 message additions
3. `src/cycod/ChatClient/FunctionCallingChat.cs` - Move user message add into pipeline OR add hooks
4. `src/cycod/ChatPipeline/ChatPipeline.cs` - Possibly add AddMessageWithHooks() helper

---

## Testing Strategy

Once implemented, need tests for:

1. **Hook execution coverage** - Verify hooks fire for all 5 message additions
2. **Builder API** - Verify semantic methods register at correct hook points
3. **Lambda overloads** - Verify all overload variations work
4. **Generic catch-all** - Verify AddPreMessageAdd fires for all message types
5. **Hook ordering** - Verify priority works across multiple hooks
6. **Hook failure** - Verify pipeline continues if hook throws

---

## Why This Matters

Without comprehensive hook coverage:
- ❌ Can't intercept user messages before processing
- ❌ Can't modify assistant messages with tool calls
- ❌ Can't intercept tool results before adding to history
- ❌ Can't implement security filtering on all messages
- ❌ Can't implement conversation logging comprehensively
- ❌ Can't implement content moderation on all message types

**This gap blocks most real-world hook use cases!**

---

## Related: Interrupt Hook Example

Once this is fixed, the InterruptionHook could work like:

```csharp
var pipeline = ChatPipelineFactory.CreateStandardPipeline()
    .AddPreAIStreaming(new InterruptionHook(interruptManager))
    .AddPreToolCall(new InterruptionHook(interruptManager))
    .AddPostAssistantMessage(context => 
    {
        // Check for interruption after each assistant response
        if (interruptManager.IsInterrupted())
        {
            context.State.ShouldExitLoop = true;
        }
    });
```

Much cleaner than the current approach!

---

**Status:** Documented for Attempt 2+
**Next Step:** Review this gap analysis, then implement in next iteration
