# Pipeline & Hook Architecture Redesign - Specification

## Executive Summary

This document proposes a complete architectural redesign of the chat streaming loop in `FunctionCallingChat` to replace the current monolithic implementation with a clean, extensible pipeline-based architecture that supports unlimited hook points for extending conversation behavior.

---

## Part 1: The Problem - Why We Need This (#3)

### Current Architecture Analysis

#### The Inner AI Loop Location

The core chat interaction loop exists in `FunctionCallingChat.CompleteChatStreamingAsync()`. This is the "inner AI loop" that:
1. Gets user input (via callbacks)
2. Streams AI responses (via `await foreach`)
3. Detects and accumulates function calls
4. Executes functions
5. Loops back for more AI responses

#### What Master/Main Looked Like (The Good Old Days)

**Original `CompleteChatStreamingAsync` (Master - Clean & Simple):**
```
Lines: ~35
Complexity: Simple
Responsibilities: 1 (delegate to AI client and handle basic flow)
Exception Handling: Single try/catch
State Management: Minimal (local variables only)
```

**Key Characteristics:**
- Single outer `while(true)` loop for conversation turns
- Single inner `await foreach` for streaming AI responses  
- Clean function call detection and execution
- Simple, focused, easy to understand
- No complex state management
- Single responsibility: orchestrate AI streaming

**What Was Good:**
- ✅ Easy to understand the flow
- ✅ Easy to test
- ✅ No side effects on class state
- ✅ Clear separation of concerns
- ✅ Follows "keep methods short" guideline (~35 lines)

#### What the Interrupt PR Did (The Mess)

**After Interrupt Changes:**
```
Lines: ~85
Complexity: High
Responsibilities: 4+ (streaming + interrupt + cancellation + display buffer management)
Exception Handling: Nested try/catch blocks
State Management: Complex (multiple fields, cancellation tokens, display buffers)
```

**Changes Made:**
1. Added `CancellationToken` parameter and propagation
2. Wrapped streaming in try/catch for `OperationCanceledException`
3. Added display buffer trimming logic on cancellation
4. Wrapped function calling in try/catch for `UserWantsControlException`
5. Added complex state coordination between stages

**What Went Wrong:**
- ❌ Violates Single Responsibility Principle (4+ responsibilities)
- ❌ Violates "keep methods short" (85+ lines)
- ❌ Complex nested exception handling
- ❌ Tight coupling to class state fields
- ❌ Multiple return paths (hard to follow)
- ❌ Similar exception handling in different places
- ❌ Side effects on global state
- ❌ Hard to test (requires complex setup)
- ❌ Not extensible (where would you add new concerns?)

#### The Companion Method: ChatCommand.CompleteChatStreamingAsync

**Original (Master - Beautiful):**
```
Lines: 29
Complexity: Low
Responsibilities: 1 (delegate and handle exceptions)
```

This was a perfect thin wrapper that just added exception handling around the `FunctionCallingChat` call.

**After Interrupt Changes:**
```
Lines: ~80
Complexity: Very High
Responsibilities: 5+ (wrapping + interrupt management + state management + cleanup)
```

**What Went Wrong:**
- ❌ Created/managed `_interruptTokenSource` (state pollution)
- ❌ Created/managed `SimpleInterruptManager` 
- ❌ Complex `Task.WhenAny` racing logic
- ❌ Nested try/catch blocks
- ❌ Finally block cleaning up multiple fields
- ❌ Multiple exception paths with similar handling

#### HandleFunctionCallApproval - Already Problematic

Even in master, this method was questionable:
```
Lines: ~65
Complexity: High  
Pattern: Infinite loop with multiple exits
Issues: UI code mixed with business logic
```

**Problems (Both Old and New):**
- ❌ 65+ line method (violates guidelines)
- ❌ Infinite `while(true)` loop with multiple return points
- ❌ UI rendering code mixed with approval logic
- ❌ Repeated UI code (displays same thing 3x per iteration)
- ❌ Complex nested conditionals (7+ branches)
- ❌ Hard to test UI interactions
- ❌ Cannot reuse approval logic without UI

The interrupt PR made this worse by:
- Changed return type from `bool` to `FunctionCallDecision` enum
- Added escape key handling for user control
- Added more conditional branches
- Added more complexity to already complex method

### The Core Problem

**The inner AI loop is a critical, complex piece of functionality that will need to grow MORE complex over time, not less.**

Future requirements include:
- Hook points for message interception and modification
- Hook points for tool call interception  
- Hook points for conversation state analysis
- Hook points for content filtering
- Hook points for conversation branching/forking
- Hook points for custom flow control

**Cramming all this into monolithic methods will create unmaintainable spaghetti code.**

### Why Current Approach Fails

1. **No Separation of Concerns**: Everything mixed together in one method
2. **No Extension Points**: Nowhere to cleanly add new behavior
3. **State Pollution**: Direct field manipulation makes testing hard
4. **Tight Coupling**: Hard to change one aspect without affecting others
5. **No Composition**: Can't mix and match behaviors
6. **Poor Testability**: Must mock entire complex object graph
7. **No Reusability**: Logic trapped in specific contexts

### What We Need Instead

An architecture that:
- ✅ Separates concerns into focused, single-responsibility components
- ✅ Provides clear extension points (hooks) for adding behavior
- ✅ Keeps state management explicit and testable
- ✅ Allows composition of behaviors
- ✅ Makes the core loop flow clear and understandable
- ✅ Enables testing each concern independently
- ✅ Supports future growth without complexity explosion

---

## Part 2: The Solution - Pipeline + Hook Architecture (#1)

### High-Level Design

Replace the monolithic inner loop with a **Pipeline Pattern** where:
- Each stage of the conversation loop is a separate, focused component
- Hooks can be registered at any point to observe/modify behavior
- The core loop becomes a simple pipeline executor
- Complexity is managed through composition, not one big method

### Core Concepts

#### 1. Pipeline
A pipeline is an ordered sequence of stages that execute in order. Each stage can:
- Access and modify the conversation context
- Decide to continue, skip, or exit
- Trigger hooks before and after execution

#### 2. Stages
A stage represents one logical step in the conversation loop:
- `UserInputStage` - Handles user input (or simulated input)
- `AIStreamingStage` - Streams AI responses
- `FunctionDetectionStage` - Detects function calls in AI response
- `FunctionExecutionStage` - Executes detected function calls
- `MessagePersistenceStage` - Adds messages to conversation history
- `LoopDecisionStage` - Decides whether to continue or exit loop

#### 3. Hooks
Hooks are extension points where custom behavior can be injected:
- Registered at specific `HookPoint` locations in the pipeline
- Receive full context (conversation state, pending data, stage info)
- Can observe, modify, or control flow
- Execute in registered order
- Can be added/removed dynamically

#### 4. Context
The context is the "object model" that flows through the pipeline:
- Contains conversation messages (the array you can muck with)
- Contains pending data (stuff between stages)
- Contains execution state and metadata
- Provides the "mucking around" API for hooks

### Architecture Layers

```
┌─────────────────────────────────────────────────┐
│          ChatCommand (Presentation)             │
│  - User interaction                             │
│  - Display formatting                           │
│  - Command handling                             │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│      ChatPipeline (Orchestration)               │
│  - Stage execution                              │
│  - Hook invocation                              │
│  - Flow control                                 │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│      Pipeline Stages (Business Logic)           │
│  - Focused, single-responsibility              │
│  - Composable and testable                     │
│  - No direct dependencies on each other        │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│      Hooks (Extensions)                         │
│  - Interrupts                                   │
│  - Content filtering                            │
│  - Conversation analysis                        │
│  - Tool call interception                       │
│  - Custom behaviors                             │
└─────────────────────────────────────────────────┘
```

### Hook Points (Extension Points)

Based on requirements, hooks can be registered at these points:

1. **PostUserInput** / **PreAIStreaming**
   - After user input captured, before AI processing
   - Hook can modify user message, conversation history, or inject system prompts
   - Use cases: Input validation, content filtering, context injection

2. **PostAIStreaming** / **PreMessageAdd**
   - After assistant response, before adding to message array
   - Hook can modify assistant response, analyze content, trigger actions
   - Use cases: Content moderation, response analysis, response modification

3. **PostFunctionDetection** / **PreToolCall**
   - After tool request detected, before tool execution
   - Hook can modify/block/replace tool calls
   - Use cases: Tool permission checking, tool call caching, tool result mocking

4. **PostToolCall** / **PreToolResultAdd**
   - After tool execution, before adding result to messages
   - Hook can modify tool results, cache results, analyze outcomes
   - Use cases: Result validation, result transformation, error handling

5. **PostMessageAdd** / **PreLoopContinue**
   - After message added to array, before loop continues
   - Hook can analyze conversation state, trigger side effects
   - Use cases: State tracking, conversation analysis, persistence

6. **PostLoopIteration** / **PreNextIteration**
   - At bottom of loop before continuing
   - Hook can decide to exit loop, modify flow, clean up state
   - Use cases: Conversation completion detection, flow control

### The ChatContext Object Model

The context is the "god object" that hooks can manipulate:

#### Primary Components

**1. Message Array (The Main Data)**
```
Messages: List<ChatMessage>
  - The conversation history
  - Hooks can add, remove, modify, reorder messages
  - This is the primary "mucking around" target
```

**2. Pending Data (Between-Stage Data)**
```
Pending:
  - PendingUserMessage: ChatMessage?
  - PendingAssistantMessage: ChatMessage?
  - PendingToolCalls: List<FunctionCall>
  - PendingToolResults: List<FunctionResult>
  - StreamingContent: StringBuilder
  - StreamingUpdates: List<ChatResponseUpdate>
```

**3. Execution State**
```
State:
  - CurrentStage: string
  - IsInterrupted: bool
  - ShouldExitLoop: bool
  - ShouldSkipNextStage: bool
  - LoopIteration: int
```

**4. Metadata & Extensions**
```
Properties: Dictionary<string, object>
  - Arbitrary key-value store for hooks
  - Enables stateful hooks across stages
  - Hook-to-hook communication

Instructions: List<ChatInstruction>
  - Queue of modifications to apply
  - Can inject system prompts, modify behavior
```

#### Capabilities (What Hooks Can Do)

**Message Array Manipulation:**
- Add messages at any position
- Remove messages (conversation pruning)
- Modify message content or metadata
- Reorder messages (conversation restructuring)
- Replace messages entirely

**Pending Data Hijacking:**
- Intercept content before it becomes a message
- Modify streaming content in real-time
- Block or modify tool calls before execution
- Transform tool results before persistence

**Flow Control:**
- Skip upcoming stages
- Exit the loop early
- Redirect to different pipeline
- Rollback previous stages
- Fork conversation branches

**State Management:**
- Track cross-stage state in Properties
- Communicate between hooks
- Persist conversation metadata
- Maintain hook-specific context

### Core Interfaces

#### IPipelineStage
```
Represents one step in the pipeline

Properties:
  - Name: string
  - PreHookPoint: HookPoint
  - PostHookPoint: HookPoint

Methods:
  - ExecuteAsync(ChatContext context): Task<StageResult>
  - GetMetadata(): StageMetadata
```

#### IHookHandler
```
Represents a behavior that can be injected at hook points

Methods:
  - HandleAsync(ChatContext context, HookData data): Task<HookResult>
  - Priority: int (for ordering multiple hooks at same point)
```

#### IChatPipeline
```
Orchestrates stage execution and hook invocation

Methods:
  - AddStage(IPipelineStage stage): IChatPipeline
  - AddHook(HookPoint point, IHookHandler handler): IChatPipeline
  - ExecuteAsync(ChatContext context): Task<ChatResult>
```

#### ChatContext
```
The mutable context object that flows through pipeline

Properties:
  - Messages: List<ChatMessage>
  - Pending: PendingData
  - State: ChatState  
  - Properties: Dictionary<string, object>
  - Instructions: List<ChatInstruction>

Methods:
  - Clone(): ChatContext
  - Snapshot(): ChatSnapshot
  - RestoreSnapshot(ChatSnapshot): void
```

### Pipeline Execution Flow

```
1. Create ChatContext with initial user message
2. Create ChatPipeline and register stages
3. Register hooks at desired points
4. Execute pipeline:
   
   For each stage:
     a. Execute pre-stage hooks (in priority order)
     b. Check if hooks requested skip/exit
     c. Execute stage logic
     d. Execute post-stage hooks (in priority order)
     e. Check stage and hook results
     f. Continue to next stage or exit
   
5. Return final result from context
```

### How This Solves The Problems

#### Problem: Monolithic Methods
**Solution:** Each stage is 20-30 lines, single responsibility

#### Problem: No Extension Points  
**Solution:** Unlimited hooks at defined points

#### Problem: State Pollution
**Solution:** All state in explicit ChatContext object

#### Problem: Tight Coupling
**Solution:** Stages don't know about each other, only context

#### Problem: Hard to Test
**Solution:** Test each stage independently, mock context

#### Problem: Can't Add Features
**Solution:** Add hooks without modifying core pipeline

#### Problem: Complex Exception Handling
**Solution:** Each stage handles its own exceptions, hooks can intercept

### Example Pipeline Configuration

```
var pipeline = new ChatIterationPipeline()
    .AddStage(new UserInputStage())
    .AddStage(new AIStreamingStage(chatClient))
    .AddStage(new FunctionDetectionStage())
    .AddStage(new FunctionExecutionStage(functionFactory))
    .AddStage(new MessagePersistenceStage())
    .AddStage(new LoopDecisionStage());

// Register interrupt handling
pipeline.AddHook(HookPoint.PreAIStreaming, new InterruptionHook(interruptManager));
pipeline.AddHook(HookPoint.PreFunctionExecution, new InterruptionHook(interruptManager));

// Register content moderation
pipeline.AddHook(HookPoint.PostAIStreaming, new ContentModerationHook());

// Register conversation analysis
pipeline.AddHook(HookPoint.PostMessageAdd, new ConversationAnalysisHook());

var result = await pipeline.ExecuteAsync(context);
```

### How Current Features Map to New Architecture

#### Interrupt Handling (Current PR Feature)
**Old:** Complex nested try/catch, state management in CompleteChatStreamingAsync
**New:** Clean hook registered at PreAIStreaming and PreFunctionExecution
```
No changes to core pipeline needed
Just register InterruptionHook
Hook checks for interrupts and controls flow
```

#### Function Call Approval
**Old:** HandleFunctionCallApproval with 65-line infinite loop
**New:** FunctionApprovalHook at PreFunctionExecution
```
Hook displays UI, gets approval decision
Can modify/block/approve function calls
Returns HookResult to control execution
```

#### Exception Handling
**Old:** Multiple try/catch blocks scattered throughout
**New:** ExceptionHandlingHook at multiple points or stage-level handling
```
Each stage handles its own exceptions
Hooks can intercept and recover
Context maintains error state
```

#### Display Buffer Management
**Old:** Field on ChatCommand, complex trimming logic
**New:** DisplayBufferHook that tracks streaming content
```
Hook maintains display buffer in context.Properties
On interrupt, hook trims content appropriately
No pollution of command class
```

### Migration Strategy

#### Phase 1: Create Infrastructure (No Behavior Change)
1. Define core interfaces (IPipelineStage, IHookHandler, IChatPipeline)
2. Define ChatContext object model
3. Implement basic ChatPipeline executor
4. Create stages that wrap existing logic

#### Phase 2: Migrate Current Functionality
1. Move streaming logic to AIStreamingStage
2. Move function detection to FunctionDetectionStage  
3. Move function execution to FunctionExecutionStage
4. Update CompleteChatStreamingAsync to use pipeline

#### Phase 3: Convert Features to Hooks
1. Create InterruptionHook from interrupt PR changes
2. Create FunctionApprovalHook from HandleFunctionCallApproval
3. Create DisplayBufferHook from display buffer logic
4. Register hooks in pipeline configuration

#### Phase 4: Cleanup & Testing
1. Remove old monolithic methods
2. Add comprehensive tests for stages and hooks
3. Add integration tests for pipeline
4. Document hook development guide

### Benefits

1. **Extensibility**: Add new behavior without modifying core code
2. **Testability**: Test each component independently
3. **Clarity**: Clear separation of concerns, easy to understand flow
4. **Flexibility**: Compose different behaviors for different scenarios
5. **Maintainability**: Changes isolated to specific stages/hooks
6. **Reusability**: Stages and hooks can be reused across contexts

---

## Performance & Responsiveness Considerations

### Critical Concern: Interrupt Responsiveness

**Question:** If hooks only run between stages, won't that make interrupts (double-ESC) slow/unresponsive?

**Answer:** No - the pipeline architecture maintains identical responsiveness to the current interrupt PR implementation.

### How Responsiveness is Preserved

#### Current Interrupt PR Implementation
```
SimpleInterruptManager:
  - Background task polls keyboard every 10ms
  - Detects double-ESC pattern within 500ms window
  - Sets CancellationToken when interrupt detected

AIStreamingStage:
  - await foreach (..., cancellationToken)  
  - Checks cancellationToken.ThrowIfCancellationRequested() on each update
  - Immediately throws OperationCanceledException

Result: ~10ms worst-case delay for interrupt detection
```

#### Pipeline + Hook Implementation
```
InterruptionHook (Ambient Hook):
  - Starts background monitoring (same as SimpleInterruptManager)
  - Polls keyboard every 10ms
  - Sets shared CancellationToken when double-ESC detected

AIStreamingStage (Long-Running Stage):
  - Receives CancellationToken from pipeline context
  - await foreach (..., cancellationToken)
  - Checks cancellationToken.ThrowIfCancellationRequested() on each update
  - Same immediate cancellation behavior

Result: Identical ~10ms worst-case delay - NO DEGRADATION
```

### Two Types of Hooks

**1. Stage Boundary Hooks** (Most hooks)
- Execute between pipeline stages
- Used for: Message manipulation, flow control, analysis
- Examples: PostUserInput, PreToolCall, PostMessageAdd
- Timing: Not time-critical (between stages is fine)

**2. Ambient/Continuous Hooks** (Special cases)
- Execute continuously during long-running stages
- Used for: Interrupts, real-time monitoring, async events
- Pattern: Use CancellationToken or event-based mechanisms
- Examples: InterruptionHook, TimeoutHook
- Timing: Critical (must respond immediately)

### Long-Running Stages Use CancellationToken Pattern

Any stage that might run for >100ms should:

1. **Accept CancellationToken** from ChatContext
2. **Check token frequently** during processing
3. **Throw OperationCanceledException** when cancelled
4. **Allow hooks to control token** via context

**Stages that need this:**
- `AIStreamingStage` - Can stream for seconds/minutes
- `FunctionExecutionStage` - Some tools run for seconds
- `NetworkRequestStage` - Network calls can be slow

**Stages that don't need this:**
- `FunctionDetectionStage` - Runs in milliseconds
- `MessagePersistenceStage` - Instant operation
- `LoopDecisionStage` - Simple boolean check

### How Interrupt Hook Works (Detailed)

```
1. Pipeline starts, registers InterruptionHook at construction

2. InterruptionHook.Initialize():
   - Creates SimpleInterruptManager
   - Starts background keyboard monitoring
   - Stores reference to ChatContext.CancellationTokenSource

3. Pipeline begins executing stages

4. User presses ESC ESC:
   - SimpleInterruptManager detects pattern (within 10ms)
   - InterruptionHook.OnInterruptDetected():
     - Sets ChatContext.CancellationTokenSource.Cancel()
     - Sets ChatContext.State.IsInterrupted = true
   
5. Currently executing stage (e.g., AIStreamingStage):
   - Next iteration of await foreach
   - Checks cancellationToken (< 1ms later)
   - Throws OperationCanceledException
   
6. Pipeline catches exception:
   - Invokes PostStageErrorHooks
   - Checks ChatContext.State.IsInterrupted
   - Returns gracefully with partial content

Total time: 10ms (polling) + <1ms (cancellation check) = ~10ms
```

### Responsiveness Guarantee

The pipeline architecture **guarantees** that:

1. **No responsiveness degradation** vs current implementation
2. **Same polling frequency** (10ms background monitoring)
3. **Same cancellation mechanism** (CancellationToken)
4. **Same exception handling** (OperationCanceledException)
5. **Additional benefit**: Can add other ambient hooks (timeouts, health checks) using same pattern

### Performance Overhead

**Hook invocation overhead:**
- Stage boundary hooks: ~0.01ms per hook (negligible)
- Ambient hooks: Zero overhead (run independently)
- Total overhead per iteration: <0.1ms (unmeasurable)

**Memory overhead:**
- ChatContext: ~1KB per conversation turn
- Hook registrations: ~100 bytes per hook
- Total: <10KB typical (negligible)

### Why This Wasn't Obvious in the Spec

The original spec focused on stage boundary hooks because they're the common case. Ambient hooks (like interrupts) are a special pattern that:
- Don't follow typical "before/after stage" model
- Use async patterns (CancellationToken, events) instead
- Work continuously rather than at specific points
- Are rare (most hooks are stage boundary hooks)

But they're **fully supported** and **perform identically** to direct implementations.

---

## Part 3: Implementation Examples (#2)

### Section 2.1: Core Interfaces & Types

These are the foundational contracts that define the pipeline architecture.

#### IChatPipeline Interface

The main orchestrator that executes stages and invokes hooks.

```csharp
/// <summary>
/// Orchestrates the execution of pipeline stages with hook support.
/// Manages the conversation flow from user input through AI response to function execution.
/// </summary>
public interface IChatPipeline
{
    /// <summary>
    /// Adds a stage to the pipeline. Stages execute in the order they are added.
    /// </summary>
    /// <param name="stage">The stage to add to the pipeline</param>
    /// <returns>The pipeline instance for fluent chaining</returns>
    IChatPipeline AddStage(IPipelineStage stage);
    
    /// <summary>
    /// Registers a hook handler at a specific hook point.
    /// Multiple hooks can be registered at the same point and will execute in priority order.
    /// </summary>
    /// <param name="hookPoint">The point in the pipeline where the hook should execute</param>
    /// <param name="handler">The hook handler to execute</param>
    /// <returns>The pipeline instance for fluent chaining</returns>
    IChatPipeline AddHook(HookPoint hookPoint, IHookHandler handler);
    
    /// <summary>
    /// Executes the pipeline with the given context.
    /// Runs all stages in order, invoking hooks at appropriate points.
    /// </summary>
    /// <param name="context">The chat context containing conversation state and data</param>
    /// <returns>The result of the pipeline execution</returns>
    Task<ChatResult> ExecuteAsync(ChatContext context);
    
    /// <summary>
    /// Gets all stages currently registered in the pipeline.
    /// </summary>
    IReadOnlyList<IPipelineStage> Stages { get; }
    
    /// <summary>
    /// Gets all hooks registered in the pipeline, organized by hook point.
    /// </summary>
    IReadOnlyDictionary<HookPoint, IReadOnlyList<IHookHandler>> Hooks { get; }
}
```

#### IPipelineStage Interface

Represents one logical step in the conversation loop.

```csharp
/// <summary>
/// Represents a single stage in the chat pipeline.
/// Each stage should have a single, focused responsibility.
/// </summary>
public interface IPipelineStage
{
    /// <summary>
    /// The name of this stage (for logging and debugging).
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// The hook point that fires before this stage executes.
    /// </summary>
    HookPoint PreHookPoint { get; }
    
    /// <summary>
    /// The hook point that fires after this stage executes.
    /// </summary>
    HookPoint PostHookPoint { get; }
    
    /// <summary>
    /// Executes the stage logic.
    /// Should be focused, single-responsibility, and <30 lines of code.
    /// </summary>
    /// <param name="context">The chat context with full read/write access</param>
    /// <returns>The result indicating whether to continue, skip, or exit</returns>
    Task<StageResult> ExecuteAsync(ChatContext context);
    
    /// <summary>
    /// Gets metadata about this stage for logging/debugging.
    /// </summary>
    StageMetadata GetMetadata();
}
```

#### IHookHandler Interface

Extension point for custom behavior at any hook point.

```csharp
/// <summary>
/// Handles custom behavior at a specific hook point in the pipeline.
/// Hooks can observe, modify, or control the flow of execution.
/// </summary>
public interface IHookHandler
{
    /// <summary>
    /// The name of this hook (for logging and debugging).
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Priority for ordering when multiple hooks registered at same point.
    /// Lower numbers execute first. Default is 100.
    /// </summary>
    int Priority { get; }
    
    /// <summary>
    /// Executes the hook logic.
    /// Can read/modify context, control flow, or trigger side effects.
    /// </summary>
    /// <param name="context">The chat context with full read/write access</param>
    /// <param name="data">Additional data about the current execution point</param>
    /// <returns>The result indicating how execution should proceed</returns>
    Task<HookResult> HandleAsync(ChatContext context, HookData data);
}
```

#### HookPoint Enum

Defines all extension points in the pipeline.

```csharp
/// <summary>
/// Defines the points in the pipeline where hooks can be registered.
/// These represent key moments in the conversation flow where custom behavior can be injected.
/// </summary>
public enum HookPoint
{
    /// <summary>
    /// After user input is captured, before AI processing begins.
    /// Use for: Input validation, content filtering, context injection.
    /// </summary>
    PostUserInput = 1,
    
    /// <summary>
    /// Just before AI streaming begins (same timing as PostUserInput).
    /// Use for: Pre-processing, interrupt monitoring setup.
    /// </summary>
    PreAIStreaming = 2,
    
    /// <summary>
    /// After AI streaming completes, before adding to message array.
    /// Use for: Content moderation, response analysis, response modification.
    /// </summary>
    PostAIStreaming = 3,
    
    /// <summary>
    /// Just before adding assistant message to array (same timing as PostAIStreaming).
    /// Use for: Final message modifications, metadata injection.
    /// </summary>
    PreMessageAdd = 4,
    
    /// <summary>
    /// After function calls detected, before execution begins.
    /// Use for: Tool permission checking, tool call caching, function approval UI.
    /// </summary>
    PostFunctionDetection = 5,
    
    /// <summary>
    /// Just before function execution (same timing as PostFunctionDetection).
    /// Use for: Function call interception, mocking, blocking.
    /// </summary>
    PreToolCall = 6,
    
    /// <summary>
    /// After function execution, before adding result to messages.
    /// Use for: Result validation, transformation, error handling.
    /// </summary>
    PostToolCall = 7,
    
    /// <summary>
    /// Just before adding tool result to array (same timing as PostToolCall).
    /// Use for: Final result modifications, caching.
    /// </summary>
    PreToolResultAdd = 8,
    
    /// <summary>
    /// After message added to array, before loop continues.
    /// Use for: State tracking, conversation analysis, persistence.
    /// </summary>
    PostMessageAdd = 9,
    
    /// <summary>
    /// Just before continuing to next loop iteration (same timing as PostMessageAdd).
    /// Use for: Loop control, cleanup, state transitions.
    /// </summary>
    PreLoopContinue = 10,
    
    /// <summary>
    /// At bottom of loop iteration, before starting next one.
    /// Use for: Completion detection, flow control, state cleanup.
    /// </summary>
    PostLoopIteration = 11,
    
    /// <summary>
    /// Just before next iteration begins (same timing as PostLoopIteration).
    /// Use for: Loop setup, pre-iteration validation.
    /// </summary>
    PreNextIteration = 12
}
```

#### Result Types

**StageResult** - Returned by stages to control execution flow:

```csharp
/// <summary>
/// Result of a pipeline stage execution.
/// </summary>
public class StageResult
{
    /// <summary>
    /// Whether the stage completed successfully.
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Whether to continue to the next stage.
    /// </summary>
    public bool ShouldContinue { get; set; } = true;
    
    /// <summary>
    /// Whether to exit the entire pipeline.
    /// </summary>
    public bool ShouldExit { get; set; }
    
    /// <summary>
    /// Optional error message if stage failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Optional data to pass to subsequent stages/hooks.
    /// </summary>
    public object? Data { get; set; }
    
    // Factory methods
    public static StageResult Continue() => new() { Success = true, ShouldContinue = true };
    public static StageResult Exit() => new() { Success = true, ShouldExit = true };
    public static StageResult Skip() => new() { Success = true, ShouldContinue = false };
    public static StageResult Error(string message) => new() { Success = false, ErrorMessage = message };
}
```

**HookResult** - Returned by hooks to control execution:

```csharp
/// <summary>
/// Result of a hook execution.
/// </summary>
public class HookResult
{
    /// <summary>
    /// Whether the hook completed successfully.
    /// </summary>
    public bool Success { get; set; } = true;
    
    /// <summary>
    /// Whether to skip the associated stage.
    /// </summary>
    public bool ShouldSkipStage { get; set; }
    
    /// <summary>
    /// Whether to exit the entire pipeline.
    /// </summary>
    public bool ShouldExitPipeline { get; set; }
    
    /// <summary>
    /// Optional modified result to override stage result.
    /// </summary>
    public StageResult? OverrideStageResult { get; set; }
    
    /// <summary>
    /// Optional error message if hook failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    // Factory methods
    public static HookResult Continue() => new();
    public static HookResult SkipStage() => new() { ShouldSkipStage = true };
    public static HookResult ExitPipeline() => new() { ShouldExitPipeline = true };
    public static HookResult Error(string message) => new() { Success = false, ErrorMessage = message };
}
```

**ChatResult** - Final result from pipeline execution:

```csharp
/// <summary>
/// Final result of the entire pipeline execution.
/// </summary>
public class ChatResult
{
    /// <summary>
    /// Whether the pipeline completed successfully.
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// The final content to return (typically the AI's response).
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether the conversation should continue (loop again).
    /// </summary>
    public bool ShouldContinue { get; set; }
    
    /// <summary>
    /// Optional error information if pipeline failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// The final state of the conversation context.
    /// </summary>
    public ChatContext? FinalContext { get; set; }
    
    // Factory methods
    public static ChatResult Success(string content, bool shouldContinue = false) 
        => new() { Success = true, Content = content, ShouldContinue = shouldContinue };
    
    public static ChatResult Failure(string error) 
        => new() { Success = false, ErrorMessage = error };
}
```

#### Supporting Types

**StageMetadata** - Information about a stage:

```csharp
/// <summary>
/// Metadata about a pipeline stage for debugging/logging.
/// </summary>
public class StageMetadata
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TimeSpan EstimatedDuration { get; set; }
    public bool IsLongRunning { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
}
```

**HookData** - Context passed to hooks:

```csharp
/// <summary>
/// Data passed to hooks during execution.
/// Provides context about where and why the hook is being invoked.
/// </summary>
public class HookData
{
    /// <summary>
    /// The hook point where this hook is executing.
    /// </summary>
    public HookPoint HookPoint { get; set; }
    
    /// <summary>
    /// The name of the stage associated with this hook point.
    /// </summary>
    public string StageName { get; set; } = string.Empty;
    
    /// <summary>
    /// When this hook is executing.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// The result from the stage (if this is a post-stage hook).
    /// </summary>
    public StageResult? StageResult { get; set; }
    
    /// <summary>
    /// Arbitrary properties for stage-specific data.
    /// </summary>
    public Dictionary<string, object> Properties { get; set; } = new();
}
```

---

**Next:** Section 2.2 - ChatContext Object Model

---

### Section 2.2: ChatContext Object Model

The `ChatContext` is the central "god object" that flows through the pipeline. It provides the complete "mucking around" API that hooks use to manipulate conversation state.

#### Full Implementation

See: [examples/ChatContext.cs](examples/ChatContext.cs)

The implementation includes:
- **ChatContext** - Main context class with Messages, Pending, State, Properties
- **PendingData** - Staging area for data between stages
- **ChatState** - Execution state and flow control
- **Supporting types** - ChatInstruction, FunctionCall, FunctionResult, Snapshots

#### Key Capabilities

**1. Message Array Manipulation**
```csharp
// Add messages
context.Messages.Add(new ChatMessage(ChatRole.User, "Hello"));

// Remove messages (conversation pruning)
context.Messages.RemoveAt(context.Messages.Count - 1);

// Modify messages
context.Messages[0].Contents = newContent;

// Reorder messages (conversation restructuring)
var systemMsg = context.Messages.First(m => m.Role == ChatRole.System);
context.Messages.Remove(systemMsg);
context.Messages.Insert(0, systemMsg);
```

**2. Pending Data Hijacking**
```csharp
// Intercept content before it becomes a message
if (context.Pending.StreamingContent.ToString().Contains("ERROR"))
{
    context.Pending.StreamingContent.Clear();
    context.Pending.StreamingContent.Append("I encountered an issue...");
}

// Modify tool calls before execution
foreach (var toolCall in context.Pending.PendingToolCalls)
{
    if (toolCall.Name == "dangerous_operation")
    {
        toolCall.Arguments = SanitizeArguments(toolCall.Arguments);
    }
}

// Transform tool results before persistence
context.Pending.PendingToolResults[0].Content = FilterSensitiveData(result.Content);
```

**3. Flow Control**
```csharp
// Skip upcoming stages
context.Pending.ShouldSkipNextStage = true;

// Exit the loop early
context.Pending.ShouldExitLoop = true;
context.State.ShouldExitLoop = true;

// Redirect to different stage
context.Pending.RedirectToStage = "ErrorRecoveryStage";

// Request interrupt
context.State.IsInterrupted = true;
context.CancellationTokenSource.Cancel();
```

**4. Cross-Hook State Management**
```csharp
// Store state for later hooks
context.Properties["ConversationAnalysis"] = new AnalysisResults();

// Retrieve state from earlier hooks
if (context.Properties.TryGetValue("ConversationAnalysis", out var analysis))
{
    var results = (AnalysisResults)analysis;
    // Use results...
}

// Track complex state across stages
var tracker = context.Properties.GetOrAdd("StateTracker", () => new StateTracker());
tracker.RecordEvent("tool_called", toolName);
```

**5. Instruction Queue**
```csharp
// Queue system prompt injection
context.Instructions.Add(new ChatInstruction
{
    Type = InstructionType.SystemPrompt,
    Content = "Please be more concise in your responses."
});

// Queue model parameter changes
context.Instructions.Add(new ChatInstruction
{
    Type = InstructionType.ModelParameter,
    Parameters = { ["temperature"] = 0.7 }
});
```

**6. Snapshot & Rollback**
```csharp
// Create snapshot before risky operation
var snapshot = context.Snapshot();

try
{
    // Do something risky
    await TryExperimentalFeature(context);
}
catch
{
    // Rollback to snapshot
    context.RestoreSnapshot(snapshot);
}
```

**7. Conversation Forking**
```csharp
// Fork conversation for parallel exploration
var experimentalBranch = context.Clone();
experimentalBranch.Properties["Branch"] = "experimental";

// Process both branches
var mainTask = ProcessMainBranch(context);
var experimentalTask = ProcessExperimentalBranch(experimentalBranch);

await Task.WhenAll(mainTask, experimentalTask);
```

#### Design Rationale

**Why a "God Object"?**
- Hooks need full visibility into conversation state
- Avoids parameter explosion (passing 10+ objects to every hook)
- Enables unforeseen "whacko things" without API changes
- Makes hook signatures consistent and simple

**Why Mutable?**
- Hooks need to modify state (that's the whole point)
- Immutability would require complex copy-on-write patterns
- Snapshots provide rollback when needed
- Clone() provides forking when needed

**Why Properties Dictionary?**
- Enables arbitrary cross-hook communication
- No need to extend context for every new hook type
- Type-safe wrappers can be added as extensions
- Flexible for future requirements

---

**Next:** Section 2.3 - Pipeline Infrastructure

---

### Section 2.3: Pipeline Infrastructure

The `ChatPipeline` class is the orchestrator that executes stages and invokes hooks in the correct order.

#### Full Implementation

See: [examples/ChatPipeline.cs](examples/ChatPipeline.cs)

The implementation includes:
- **ChatPipeline** - Main executor with stage and hook management
- **ChatPipelineBuilder** - Fluent builder for pipeline configuration
- Hook ordering by priority
- Exception handling at stage and hook levels
- Support for stage skipping and pipeline exit

#### Execution Flow

```
1. For each stage in pipeline:
   
   a. Update context.State.CurrentStage
   
   b. Execute pre-stage hooks:
      - Call all hooks at stage.PreHookPoint
      - Execute in priority order (lower numbers first)
      - Check for skip/exit requests
   
   c. If skip requested:
      - Continue to next stage
   
   d. Execute stage:
      - Call stage.ExecuteAsync(context)
      - Catch and record any exceptions
      - Convert to StageResult
   
   e. Execute post-stage hooks:
      - Call all hooks at stage.PostHookPoint
      - Pass stage result to hooks
      - Allow hooks to override result
      - Check for exit requests
   
   f. Check exit conditions:
      - Hook requested exit
      - Stage requested exit
      - Context flags exit
      - If exiting, return with partial content
   
   g. Check for errors:
      - If stage failed, return error result
   
   h. Handle redirects:
      - If context.Pending.RedirectToStage set
      - Jump to named stage (if found)
   
   i. Continue to next stage

2. All stages complete:
   - Extract final content from context
   - Return success result
```

#### Exception Handling Strategy

**Stage Exceptions:**
```csharp
try
{
    stageResult = await stage.ExecuteAsync(context);
}
catch (Exception ex)
{
    context.State.LastError = ex;
    stageResult = StageResult.Error($"Stage {stage.Name} failed: {ex.Message}");
}
```
- Stages exceptions are caught and converted to error results
- Pipeline can decide to continue or exit based on result
- Error is recorded in context for hooks to inspect

**Hook Exceptions:**
```csharp
try
{
    var result = await handler.HandleAsync(context, hookData);
    // Process result...
}
catch (Exception ex)
{
    Console.WriteLine($"Hook {handler.Name} threw exception: {ex.Message}");
    context.State.LastError = ex;
    // Continue to next hook - don't fail pipeline
}
```
- Hook exceptions are caught and logged
- Pipeline continues executing (fail-soft)
- Could be made configurable (fail-fast vs. continue-on-error)

**Cancellation:**
```csharp
catch (OperationCanceledException) when (context.CancellationTokenSource.Token.IsCancellationRequested)
{
    context.State.IsInterrupted = true;
    return CreateInterruptedResult(context);
}
```
- Cancellation (interrupts) handled gracefully
- Returns partial content if available
- Sets IsInterrupted flag for logging/analysis

#### Hook Aggregation

When multiple hooks are registered at the same point:

1. **Execute in priority order** (lower numbers first)
2. **Aggregate results**:
   - Any hook can request skip → stage is skipped
   - Any hook can request exit → pipeline exits immediately
   - Last hook's override wins (for stage result overrides)
3. **Continue-on-error**:
   - Hook failures don't fail pipeline
   - Logged but execution continues
   - Could be made configurable

#### Builder Pattern

For clean pipeline construction:

```csharp
var pipeline = new ChatPipelineBuilder()
    .WithStage(new AIStreamingStage(chatClient))
    .WithStage(new FunctionDetectionStage())
    .WithStage(new FunctionExecutionStage(factory))
    .WithStage(new MessagePersistenceStage())
    .WithHook(HookPoint.PreAIStreaming, new InterruptionHook(interruptManager))
    .WithHook(HookPoint.PostAIStreaming, new ContentModerationHook())
    .WithCancellation(cancellationTokenSource)
    .WithInterruptSupport(interruptManager)
    .WithExceptionHandling(exceptionHandler)
    .Build();
```

#### Key Design Decisions

**1. Fail-Soft for Hooks**
- Hooks are extensions, not core functionality
- A failing hook shouldn't crash the conversation
- Logged for visibility but execution continues
- Critical hooks should handle their own errors

**2. Priority-Based Ordering**
- Simple numeric priority (default 100)
- Lower numbers execute first
- Allows control over hook execution order
- No complex dependency graphs needed

**3. Hook Result Aggregation**
- Any hook can veto (skip/exit)
- Last override wins (could be "first wins" instead)
- Keeps aggregation logic simple
- Can be extended with more sophisticated rules

**4. Stage Isolation**
- Stages don't know about each other
- Only communicate through context
- Makes stages independently testable
- Enables stage reuse in different pipelines

**5. Context as Communication**
- All state flows through context
- No hidden state or side channels
- Makes flow explicit and debuggable
- Enables snapshot/rollback

---

**Next:** Section 2.4 - Example Stages

---

### Section 2.4: Example Stages

Stages are focused, single-responsibility components that perform one step in the conversation loop. Each stage is <30 lines of core logic.

#### Full Implementation

See: [examples/PipelineStages.cs](examples/PipelineStages.cs)

The implementation includes five core stages:
- **AIStreamingStage** - Streams AI responses (long-running, cancellable)
- **FunctionDetectionStage** - Detects function calls (fast, synchronous)
- **FunctionExecutionStage** - Executes functions (long-running, cancellable)
- **MessagePersistenceStage** - Adds messages to history (fast, synchronous)
- **LoopDecisionStage** - Decides loop continuation (fast, decision-only)

#### Stage Pattern

All stages follow this pattern:

```csharp
public class ExampleStage : IPipelineStage
{
    // 1. Identity
    public string Name => "ExampleStage";
    public HookPoint PreHookPoint => HookPoint.PreExample;
    public HookPoint PostHookPoint => HookPoint.PostExample;
    
    // 2. Dependencies (injected via constructor)
    private readonly IDependency _dependency;
    
    public ExampleStage(IDependency dependency)
    {
        _dependency = dependency;
    }
    
    // 3. Execute logic (focused, <30 lines)
    public async Task<StageResult> ExecuteAsync(ChatContext context)
    {
        // a. Get cancellation token if long-running
        var cancellationToken = context.CancellationTokenSource.Token;
        
        // b. Do the work (single responsibility)
        await DoSomethingFocusedAsync(context, cancellationToken);
        
        // c. Update context with results
        context.Pending.SomeData = result;
        
        // d. Return appropriate result
        return StageResult.Continue();
    }
    
    // 4. Metadata for debugging/logging
    public StageMetadata GetMetadata()
    {
        return new StageMetadata
        {
            Name = Name,
            Description = "What this stage does",
            EstimatedDuration = TimeSpan.FromSeconds(1),
            IsLongRunning = false
        };
    }
}
```

#### AIStreamingStage - Key Points

**Responsibility:** Stream AI responses and accumulate content

**Cancellation Support:**
```csharp
await foreach (var update in _chatClient.GetStreamingResponseAsync(
    context.Messages, 
    _options, 
    cancellationToken))
{
    // Check cancellation frequently for responsiveness
    cancellationToken.ThrowIfCancellationRequested();
    
    // Accumulate content
    context.Pending.StreamingContent.Append(update.Text);
}
```

**Why This Works for Interrupts:**
- Checks cancellation token on every streaming update
- Update frequency ~10-50ms (maintains <10ms interrupt responsiveness)
- Gracefully handles cancellation, returns partial content
- No polling needed - async cancellation pattern

**Output:**
- `context.Pending.PendingAssistantMessage` - Accumulated AI response

#### FunctionDetectionStage - Key Points

**Responsibility:** Detect function calls in AI response

**Fast & Simple:**
```csharp
foreach (var update in context.Pending.StreamingUpdates)
{
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
```

**No External Dependencies:**
- Just analyzes data already in context
- Synchronous, completes in <1ms
- No error handling needed (simple data transformation)

**Output:**
- `context.Pending.PendingToolCalls` - List of detected function calls

#### FunctionExecutionStage - Key Points

**Responsibility:** Execute detected function calls

**Cancellation Support:**
```csharp
foreach (var toolCall in context.Pending.PendingToolCalls)
{
    cancellationToken.ThrowIfCancellationRequested();
    
    var function = _functionFactory.GetFunction(toolCall.Name);
    var result = await function.InvokeAsync(toolCall.Arguments, cancellationToken);
    
    context.Pending.PendingToolResults.Add(result);
}
```

**Error Handling:**
- Per-function try/catch (one failure doesn't stop others)
- Records errors in FunctionResult
- Continues execution even on errors

**Output:**
- `context.Pending.PendingToolResults` - Results from all function calls

#### MessagePersistenceStage - Key Points

**Responsibility:** Move pending data into Messages array

**Simple Transfer:**
```csharp
if (context.Pending.PendingUserMessage != null)
{
    context.Messages.Add(context.Pending.PendingUserMessage);
    context.Pending.PendingUserMessage = null;
}

if (context.Pending.PendingAssistantMessage != null)
{
    context.Messages.Add(context.Pending.PendingAssistantMessage);
    context.Pending.PendingAssistantMessage = null;
}

foreach (var toolResult in context.Pending.PendingToolResults)
{
    context.Messages.Add(CreateToolResultMessage(toolResult));
}
```

**Why This is a Separate Stage:**
- Hooks can intercept BEFORE messages are committed
- PreMessageAdd hooks can modify/block messages
- PostMessageAdd hooks can observe what was added
- Clean separation between "what we're about to do" and "what we did"

#### LoopDecisionStage - Key Points

**Responsibility:** Decide whether to continue loop or exit

**Decision Logic:**
```csharp
context.State.LoopIteration++;

if (context.State.ShouldExitLoop || context.Pending.ShouldExitLoop)
    return StageResult.Exit();

if (context.State.IsInterrupted)
    return StageResult.Exit();

if (context.Pending.PendingToolCalls.Count > 0)
    return StageResult.Continue(); // More work to do

return StageResult.Exit(); // All done
```

**Why This is a Separate Stage:**
- Makes loop logic explicit (not buried in conditionals)
- Hooks can influence decision (PostLoopIteration)
- Easy to modify decision logic (add max iterations, timeout, etc.)
- Testable in isolation

#### Stage Composition

These stages compose into the full conversation loop:

```
Loop:
  1. AIStreamingStage          → Get AI response
  2. FunctionDetectionStage    → Find function calls
  3. FunctionExecutionStage    → Execute functions
  4. MessagePersistenceStage   → Save to history
  5. LoopDecisionStage         → Continue or exit?
     ↓
  If Continue: Back to step 1
  If Exit: Return result
```

This replaces the old 85-line monolithic method with 5 focused stages of ~20-25 lines each.

#### Design Benefits

**Single Responsibility:**
- Each stage does exactly one thing
- Easy to understand at a glance
- Easy to modify independently

**Testability:**
- Mock context, call ExecuteAsync
- No complex setup required
- Test each stage in isolation

**Reusability:**
- Stages can be reused in different pipelines
- Can create variations (DebugAIStreamingStage, MockFunctionExecutionStage)
- Can compose different pipelines for different scenarios

**Extensibility:**
- Add new stages without modifying existing ones
- Hooks provide even finer-grained extension points
- Clear contracts via interfaces

---

**Next:** Section 2.5 - Example Hooks

---

### Section 2.5: Example Hooks

Hooks are the clean replacements for the messy code added in the interrupt PR and other places. They're focused, testable, and composable.

#### Full Implementation

See: [examples/PipelineHooks.cs](examples/PipelineHooks.cs)

The implementation includes:
- **InterruptionHook** - Replaces SimpleInterruptManager integration (was 30+ lines of complex code)
- **FunctionApprovalHook** - Replaces HandleFunctionCallApproval (was 65-line infinite loop)
- **DisplayBufferHook** - Replaces display buffer management (was scattered across multiple methods)
- **ConversationAnalysisHook** - Example of "whacko thing" that analyzes and modifies conversation
- **ConversationForkingHook** - Example of advanced "whacko thing" that creates parallel branches

#### Hook Pattern

All hooks follow this pattern:

```csharp
public class ExampleHook : IHookHandler
{
    // 1. Identity
    public string Name => "Example";
    public int Priority => 100; // Lower = runs first
    
    // 2. Dependencies (injected via constructor)
    private readonly IDependency _dependency;
    
    public ExampleHook(IDependency dependency)
    {
        _dependency = dependency;
    }
    
    // 3. Handle logic (check hook point, modify context)
    public async Task<HookResult> HandleAsync(ChatContext context, HookData data)
    {
        // a. Check if this is the hook point we care about
        if (data.HookPoint != HookPoint.PostUserInput)
        {
            return HookResult.Continue();
        }
        
        // b. Do the work
        var result = await _dependency.ProcessAsync(context);
        
        // c. Modify context
        context.Messages.Add(newMessage);
        context.Properties["ExampleData"] = result;
        
        // d. Return appropriate result
        return HookResult.Continue();
    }
}
```

#### InterruptionHook - Replaces Interrupt PR Complexity

**What it replaces:**
```
OLD (scattered across ChatCommand):
- SimpleInterruptManager creation (3 lines)
- Task.WhenAny racing logic (5 lines)
- Cancellation token management (4 lines)
- Exception handling (15 lines)
- Cleanup in finally block (5 lines)
Total: ~32 lines of complex, coupled code
```

**NEW (clean hook):**
```csharp
// Just register the hook
pipeline.AddHook(HookPoint.PreAIStreaming, new InterruptionHook(interruptManager));

// Hook handles:
// - Starting/stopping monitoring
// - Setting cancellation token
// - Requesting pipeline exit
// - Cleanup
Total: 1 line to use, ~60 lines of focused hook code
```

**How it works:**
1. Starts monitoring on PreAIStreaming hook
2. Runs background task waiting for double-ESC
3. When interrupt detected, sets cancellation token
4. Stages check token and throw OperationCanceledException
5. Pipeline catches and exits gracefully
6. Stops monitoring on PostLoopIteration

**Responsiveness maintained:**
- Same 10ms polling as interrupt PR
- Same CancellationToken pattern
- No degradation

#### FunctionApprovalHook - Replaces 65-Line Method

**What it replaces:**
```
OLD (ChatCommand.HandleFunctionCallApproval):
- 65-line method with infinite loop
- UI code mixed with approval logic
- 7+ nested conditionals
- Repeated display code
- Hard to test
```

**NEW (clean hook):**
```csharp
pipeline.AddHook(HookPoint.PreToolCall, new FunctionApprovalHook(approvalUI));

// Hook handles:
// - Checking auto-approve
// - Prompting user with UI
// - Tracking session approvals
// - Handling denials
// - Handling ESC ESC during approval
Total: 1 line to use, ~100 lines of focused hook code
```

**Key improvements:**
- UI separated into IFunctionApprovalUI interface
- Business logic (approval tracking) in hook
- No infinite loops
- Clean conditional structure
- Easy to test (mock UI)
- ESC ESC handling integrated cleanly

**How approval works:**
```
1. Hook checks each pending tool call
2. If auto-approve or already approved: continue
3. Else: Call approvalUI.PromptForApprovalAsync()
4. Based on decision:
   - Approved: Keep in pending list
   - ApprovedForSession: Remember + keep in pending
   - Denied: Remove from pending, add denial result
   - UserWantsControl: Set interrupt flag, exit pipeline
5. If all denied: Return SkipStage (skip execution)
```

#### DisplayBufferHook - Replaces Scattered Logic

**What it replaces:**
```
OLD (scattered across ChatCommand and FunctionCallingChat):
- _displayBuffer field on ChatCommand
- Display buffer initialization
- Buffer trimming logic in multiple places
- Complex substring calculations
- Unclear ownership
Total: ~20 lines scattered across 3 methods
```

**NEW (clean hook):**
```csharp
pipeline.AddHook(HookPoint.PreAIStreaming, new DisplayBufferHook());
pipeline.AddHook(HookPoint.PostAIStreaming, new DisplayBufferHook());
pipeline.AddHook(HookPoint.PostLoopIteration, new DisplayBufferHook());

// Hook handles:
// - Tracking display buffer in context.Properties
// - Trimming on interrupt
// - Clearing after successful iteration
Total: 3 registrations, ~50 lines of focused hook code
```

**How it works:**
- PreAIStreaming: Clear display buffer
- PostAIStreaming: Save streamed content as display buffer
- PostAIStreaming (if interrupted): Trim to display buffer length
- PostLoopIteration: Clear buffer if not interrupted

#### ConversationAnalysisHook - Example "Whacko Thing"

**Capability demonstration:**
```csharp
public async Task<HookResult> HandleAsync(ChatContext context, HookData data)
{
    // Analyze conversation
    var analysis = await _analyzer.AnalyzeAsync(context.Messages);
    
    // Modify messages based on analysis
    if (analysis.MessageCount > 20)
    {
        // Inject system prompt for conciseness
        context.Instructions.Add(new ChatInstruction
        {
            Type = InstructionType.SystemPrompt,
            Content = "Please be more concise."
        });
    }
    
    // Rewrite conversation for better context
    if (analysis.HasRepetitivePatterns)
    {
        var summary = await _analyzer.SummarizeRecentContext(context.Messages);
        context.Messages.Insert(0, new ChatMessage(ChatRole.System, summary));
    }
    
    return HookResult.Continue();
}
```

**What this enables:**
- Automatic conversation summarization
- Dynamic system prompt injection
- Pattern-based conversation modifications
- Cross-message analysis
- All without changing pipeline code

#### ConversationForkingHook - Advanced "Whacko Thing"

**Capability demonstration:**
```csharp
// Fork conversation for parallel exploration
var experimentalBranch = context.Clone();
experimentalBranch.Properties["Branch"] = "experimental";

// Process in parallel with different pipeline
_ = Task.Run(async () =>
{
    var experimentalPipeline = CreateExperimentalPipeline();
    var result = await experimentalPipeline.ExecuteAsync(experimentalBranch);
    context.Properties["ExperimentalResult"] = result;
});
```

**What this enables:**
- Parallel conversation exploration
- A/B testing of AI responses
- Experimental feature testing
- Multi-agent conversations
- All without modifying core pipeline

#### Hook Composition

Multiple hooks work together cleanly:

```csharp
var pipeline = new ChatPipelineBuilder()
    .WithStage(new AIStreamingStage(chatClient))
    .WithStage(new FunctionExecutionStage(factory))
    .WithStage(new MessagePersistenceStage())
    
    // Core functionality hooks
    .WithHook(HookPoint.PreAIStreaming, new InterruptionHook(interruptManager))
    .WithHook(HookPoint.PreToolCall, new FunctionApprovalHook(approvalUI))
    .WithHook(HookPoint.PostAIStreaming, new DisplayBufferHook())
    
    // Analysis and modification hooks
    .WithHook(HookPoint.PostUserInput, new ConversationAnalysisHook(analyzer))
    .WithHook(HookPoint.PostAIStreaming, new ContentModerationHook())
    
    // Advanced hooks
    .WithHook(HookPoint.PostMessageAdd, new ConversationForkingHook())
    .WithHook(HookPoint.PostToolCall, new ToolResultCachingHook())
    
    .Build();
```

**Execution:**
1. Hooks run in priority order at each point
2. Each hook can observe/modify context
3. Each hook can control flow (skip/exit)
4. Hooks don't know about each other
5. Clean, composable, testable

#### Comparison: Old vs New

**Interrupt Handling:**
- OLD: 80-line CompleteChatStreamingAsync with nested try/catch
- NEW: 60-line InterruptionHook + 1-line registration

**Function Approval:**
- OLD: 65-line HandleFunctionCallApproval with infinite loop
- NEW: 100-line FunctionApprovalHook + 1-line registration + UI interface

**Display Buffer:**
- OLD: Scattered across 3 methods, unclear ownership
- NEW: 50-line DisplayBufferHook + 3 registrations

**Totals:**
- OLD: ~165 lines of complex, coupled code
- NEW: ~210 lines of focused hook code (30% more code, but 10x cleaner)

**But the real win:**
- NEW: Unlimited extensibility via hooks
- NEW: Each concern testable independently
- NEW: No changes to core pipeline for new features
- NEW: Clean separation of concerns

---

**Next:** Section 2.6 - Integration Examples

---

---

## Implementation Tasks

### Task 1: Define Core Interfaces
- [ ] Create `IChatPipeline` interface
- [ ] Create `IPipelineStage` interface  
- [ ] Create `IHookHandler` interface
- [ ] Define `HookPoint` enum with all hook points
- [ ] Define `StageResult`, `HookResult`, `ChatResult` types

### Task 2: Implement ChatContext
- [ ] Create `ChatContext` class with all properties
- [ ] Create `PendingData` class
- [ ] Create `ChatState` class
- [ ] Implement Clone() and Snapshot() functionality
- [ ] Add helper methods for common operations

### Task 3: Implement Pipeline Infrastructure
- [ ] Create `ChatPipeline` class implementing `IChatPipeline`
- [ ] Implement stage execution with hook invocation
- [ ] Implement hook ordering and priority
- [ ] Add error handling and recovery
- [ ] Add logging/debugging support

### Task 4: Create Core Stages
- [ ] Create `AIStreamingStage` (wraps existing streaming logic)
- [ ] Create `FunctionDetectionStage` (wraps function call detection)
- [ ] Create `FunctionExecutionStage` (wraps function execution)
- [ ] Create `MessagePersistenceStage` (adds messages to context)
- [ ] Create `LoopDecisionStage` (determines loop continuation)

### Task 5: Create Hook Implementations
- [ ] Create `InterruptionHook` (replaces current interrupt logic)
- [ ] Create `FunctionApprovalHook` (replaces HandleFunctionCallApproval)
- [ ] Create `DisplayBufferHook` (manages display buffer)
- [ ] Create `ExceptionHandlingHook` (centralized error handling)

### Task 6: Integrate with ChatCommand
- [ ] Update `ChatCommand.CompleteChatStreamingAsync` to use pipeline
- [ ] Remove old monolithic implementation
- [ ] Configure pipeline with default hooks
- [ ] Add configuration methods for custom hooks

### Task 7: Testing
- [ ] Unit tests for each stage
- [ ] Unit tests for each hook
- [ ] Integration tests for pipeline execution
- [ ] Tests for hook ordering and priority
- [ ] Tests for error handling and recovery
- [ ] Tests for ChatContext manipulation

### Task 8: Documentation
- [ ] Document hook development guide
- [ ] Document stage development guide
- [ ] Document common patterns and recipes
- [ ] Update architecture documentation
- [ ] Add examples and tutorials

---

## Success Criteria

1. **Code Quality**
   - All stages are <30 lines, single responsibility
   - No methods >50 lines anywhere in pipeline code
   - High test coverage (>80%)
   - All existing functionality preserved

2. **Extensibility**
   - Can add new hook without modifying pipeline
   - Can add new stage without modifying other stages
   - Can compose different pipelines for different scenarios

3. **Clarity**
   - Pipeline flow is obvious from code structure
   - Each stage purpose is immediately clear
   - Hook points are well-documented and intuitive

4. **Performance**
   - No performance degradation vs current implementation
   - Hook invocation overhead is negligible
   - Can disable hooks for performance-critical scenarios

---

## Future Enhancements

Once base architecture is in place:

1. **Pipeline Variants**
   - Debug pipeline with extra logging hooks
   - Testing pipeline with mock stages
   - Performance pipeline with minimal hooks

2. **Advanced Hooks**
   - Conversation forking (parallel branches)
   - Time-travel (rollback and retry)
   - Conversation summarization
   - Multi-agent conversations

3. **Dynamic Configuration**
   - Load hooks from configuration
   - Enable/disable hooks at runtime
   - Hot-reload hook implementations

4. **Observability**
   - Pipeline execution tracing
   - Hook performance metrics
   - Conversation state visualization

---

## Appendix: Reference to Current Code

### Files to Modify
- `src/cycod/ChatClient/FunctionCallingChat.cs` - Main refactoring target
- `src/cycod/CommandLineCommands/ChatCommand.cs` - Integration point

### Current Problematic Methods
- `FunctionCallingChat.CompleteChatStreamingAsync()` - Lines 79-165 (~85 lines)
- `ChatCommand.CompleteChatStreamingAsync()` - Lines 518-597 (~80 lines)  
- `ChatCommand.HandleFunctionCallApproval()` - Lines 861-932 (~70 lines)

### Key Dependencies
- `Microsoft.Extensions.AI` - Chat client interfaces
- `FunctionCallDetector` - Function call detection logic
- `FunctionFactory` / `McpFunctionFactory` - Tool execution

---

## Questions for Review

1. **Hook Granularity**: Are the proposed hook points at the right level, or do we need more/fewer?

   **ANSWER**: Yes. Perfect (for now... everything can be 'changed in future' with new info)

2. **Context API**: Is the ChatContext object model complete for "mucking around" needs?

   **ANSWER**: Yes. Perfect. :-)

3. **Performance**: Any concerns about hook invocation overhead?

   **ANSWER**: No. Seems good for this prototype of implementation w/ this plan.

4. **Backwards Compatibility**: Should we maintain old API temporarily during migration?

   **ANSWER**: Zero care about compatibility from a code api/function name/class/etc... I don't think anything above changes files/options inputs/outputs...

5. **Hook Ordering**: Is priority-based ordering sufficient, or do we need dependency chains?

   **ANSWER**: This is sufficient for now.

6. **Error Handling**: Should hooks be able to handle errors from previous hooks?

   **ANSWER**: No. Hooks should not handle errors from previous hooks; error handling should be centralized in the pipeline.

---

**Next Steps:** Review this specification, provide feedback, then proceed with Part 3 (Implementation Examples) based on any adjustments needed.

1. [x] Reviewed!
2. Feedback provided to questions above...
