using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.AI;

namespace Cycod.ChatPipeline;

/// <summary>
/// The central context object that flows through the pipeline.
/// Contains all conversation state, pending data, and provides the "mucking around" API.
/// Hooks and stages have full read/write access to manipulate conversation flow.
/// </summary>
public class ChatContext
{
    /// <summary>
    /// The conversation message array - primary data structure for "mucking around".
    /// Hooks can add, remove, modify, or reorder messages at any time.
    /// </summary>
    public List<ChatMessage> Messages { get; set; } = new();
    
    /// <summary>
    /// Staging area for data between pipeline stages.
    /// Contains content that hasn't been committed to Messages yet.
    /// </summary>
    public PendingData Pending { get; set; } = new();
    
    /// <summary>
    /// Current execution state and flow control.
    /// </summary>
    public ChatState State { get; set; } = new();
    
    /// <summary>
    /// Arbitrary key-value store for hooks to share state.
    /// Enables cross-stage state management and hook-to-hook communication.
    /// </summary>
    public Dictionary<string, object> Properties { get; set; } = new();
    
    /// <summary>
    /// Queue of instructions to apply to the conversation.
    /// Hooks can inject system prompts, modify behavior, etc.
    /// </summary>
    public List<ChatInstruction> Instructions { get; set; } = new();
    
    /// <summary>
    /// Cancellation token for long-running operations (AI streaming, function calls).
    /// Used by interrupt hooks to cancel execution gracefully.
    /// </summary>
    public CancellationTokenSource CancellationTokenSource { get; set; } = new();
    
    /// <summary>
    /// Creates a deep clone of this context for branching/forking conversations.
    /// </summary>
    public ChatContext Clone()
    {
        return new ChatContext
        {
            Messages = Messages.Select(m => new ChatMessage(m.Role, m.Contents)).ToList(),
            Pending = Pending.Clone(),
            State = State.Clone(),
            Properties = new Dictionary<string, object>(Properties),
            Instructions = new List<ChatInstruction>(Instructions),
            CancellationTokenSource = new CancellationTokenSource()
        };
    }
    
    /// <summary>
    /// Creates a lightweight snapshot for rollback scenarios.
    /// </summary>
    public ChatSnapshot Snapshot()
    {
        return new ChatSnapshot
        {
            MessageCount = Messages.Count,
            PendingSnapshot = Pending.Snapshot(),
            StateSnapshot = State.Snapshot(),
            Timestamp = DateTime.UtcNow
        };
    }
    
    /// <summary>
    /// Restores the context to a previous snapshot.
    /// Useful for time-travel debugging or retry scenarios.
    /// </summary>
    public void RestoreSnapshot(ChatSnapshot snapshot)
    {
        // Remove messages added after snapshot
        if (Messages.Count > snapshot.MessageCount)
        {
            Messages.RemoveRange(snapshot.MessageCount, Messages.Count - snapshot.MessageCount);
        }
        
        Pending.Restore(snapshot.PendingSnapshot);
        State.Restore(snapshot.StateSnapshot);
    }
}

/// <summary>
/// Staging area for data between pipeline stages.
/// This is the "in-between" space where hooks can intercept and modify content
/// before it becomes official messages.
/// </summary>
public class PendingData
{
    /// <summary>
    /// User message about to be added to the conversation.
    /// </summary>
    public ChatMessage? PendingUserMessage { get; set; }
    
    /// <summary>
    /// Assistant message about to be added to the conversation.
    /// </summary>
    public ChatMessage? PendingAssistantMessage { get; set; }
    
    /// <summary>
    /// Function calls detected and ready for execution.
    /// </summary>
    public List<FunctionCall> PendingToolCalls { get; set; } = new();
    
    /// <summary>
    /// Function results ready to be added to conversation.
    /// </summary>
    public List<FunctionResult> PendingToolResults { get; set; } = new();
    
    /// <summary>
    /// Raw streaming content being accumulated from AI.
    /// Can be modified in real-time by hooks.
    /// </summary>
    public StringBuilder StreamingContent { get; set; } = new();
    
    /// <summary>
    /// Individual streaming updates from AI (for detailed analysis).
    /// </summary>
    public List<ChatResponseUpdate> StreamingUpdates { get; set; } = new();
    
    /// <summary>
    /// Whether to skip the next stage in the pipeline.
    /// </summary>
    public bool ShouldSkipNextStage { get; set; }
    
    /// <summary>
    /// Whether to exit the loop after this iteration.
    /// </summary>
    public bool ShouldExitLoop { get; set; }
    
    /// <summary>
    /// Optional stage name to redirect to (for complex flow control).
    /// </summary>
    public string? RedirectToStage { get; set; }
    
    public PendingData Clone()
    {
        return new PendingData
        {
            PendingUserMessage = PendingUserMessage,
            PendingAssistantMessage = PendingAssistantMessage,
            PendingToolCalls = new List<FunctionCall>(PendingToolCalls),
            PendingToolResults = new List<FunctionResult>(PendingToolResults),
            StreamingContent = new StringBuilder(StreamingContent.ToString()),
            StreamingUpdates = new List<ChatResponseUpdate>(StreamingUpdates),
            ShouldSkipNextStage = ShouldSkipNextStage,
            ShouldExitLoop = ShouldExitLoop,
            RedirectToStage = RedirectToStage
        };
    }
    
    public PendingDataSnapshot Snapshot()
    {
        return new PendingDataSnapshot
        {
            StreamingContentLength = StreamingContent.Length,
            ToolCallCount = PendingToolCalls.Count,
            ToolResultCount = PendingToolResults.Count
        };
    }
    
    public void Restore(PendingDataSnapshot snapshot)
    {
        if (StreamingContent.Length > snapshot.StreamingContentLength)
        {
            StreamingContent.Remove(snapshot.StreamingContentLength, 
                StreamingContent.Length - snapshot.StreamingContentLength);
        }
        
        if (PendingToolCalls.Count > snapshot.ToolCallCount)
        {
            PendingToolCalls.RemoveRange(snapshot.ToolCallCount, 
                PendingToolCalls.Count - snapshot.ToolCallCount);
        }
        
        if (PendingToolResults.Count > snapshot.ToolResultCount)
        {
            PendingToolResults.RemoveRange(snapshot.ToolResultCount, 
                PendingToolResults.Count - snapshot.ToolResultCount);
        }
    }
}

/// <summary>
/// Current execution state and flow control flags.
/// </summary>
public class ChatState
{
    /// <summary>
    /// The name of the currently executing stage.
    /// </summary>
    public string CurrentStage { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether an interrupt has been requested (double-ESC).
    /// </summary>
    public bool IsInterrupted { get; set; }
    
    /// <summary>
    /// Whether the pipeline should exit after the current stage.
    /// </summary>
    public bool ShouldExitLoop { get; set; }
    
    /// <summary>
    /// Current loop iteration count (for debugging/analysis).
    /// </summary>
    public int LoopIteration { get; set; }
    
    /// <summary>
    /// When the conversation started.
    /// </summary>
    public DateTime ConversationStartTime { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Total tokens consumed in this conversation (if tracked).
    /// </summary>
    public int TotalTokens { get; set; }
    
    /// <summary>
    /// Any error that occurred during execution.
    /// </summary>
    public Exception? LastError { get; set; }
    
    public ChatState Clone()
    {
        return new ChatState
        {
            CurrentStage = CurrentStage,
            IsInterrupted = IsInterrupted,
            ShouldExitLoop = ShouldExitLoop,
            LoopIteration = LoopIteration,
            ConversationStartTime = ConversationStartTime,
            TotalTokens = TotalTokens,
            LastError = LastError
        };
    }
    
    public ChatStateSnapshot Snapshot()
    {
        return new ChatStateSnapshot
        {
            LoopIteration = LoopIteration,
            TotalTokens = TotalTokens
        };
    }
    
    public void Restore(ChatStateSnapshot snapshot)
    {
        LoopIteration = snapshot.LoopIteration;
        TotalTokens = snapshot.TotalTokens;
    }
}

/// <summary>
/// Represents an instruction to modify conversation behavior.
/// Hooks can queue these up to affect subsequent processing.
/// </summary>
public class ChatInstruction
{
    public InstructionType Type { get; set; }
    public string Content { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public enum InstructionType
{
    /// <summary>
    /// Inject a system prompt before next AI call.
    /// </summary>
    SystemPrompt,
    
    /// <summary>
    /// Modify the AI model parameters (temperature, etc.).
    /// </summary>
    ModelParameter,
    
    /// <summary>
    /// Add a constraint to function calling.
    /// </summary>
    FunctionConstraint,
    
    /// <summary>
    /// Change the conversation mode/behavior.
    /// </summary>
    ModeChange
}

/// <summary>
/// Represents a function call detected in AI response.
/// </summary>
public class FunctionCall
{
    public string CallId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Arguments { get; set; } = string.Empty;
}

/// <summary>
/// Represents the result of a function execution.
/// </summary>
public class FunctionResult
{
    public string CallId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool Success { get; set; } = true;
    public Exception? Error { get; set; }
}

/// <summary>
/// Lightweight snapshot for rollback scenarios.
/// </summary>
public class ChatSnapshot
{
    public int MessageCount { get; set; }
    public PendingDataSnapshot PendingSnapshot { get; set; } = new();
    public ChatStateSnapshot StateSnapshot { get; set; } = new();
    public DateTime Timestamp { get; set; }
}

public class PendingDataSnapshot
{
    public int StreamingContentLength { get; set; }
    public int ToolCallCount { get; set; }
    public int ToolResultCount { get; set; }
}

public class ChatStateSnapshot
{
    public int LoopIteration { get; set; }
    public int TotalTokens { get; set; }
}
