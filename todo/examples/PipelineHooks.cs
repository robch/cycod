using System;
using System.Threading.Tasks;

namespace Cycod.ChatPipeline.Hooks;

/// <summary>
/// Hook that handles user interrupts (double-ESC).
/// This is an "ambient" hook that runs continuously, not just at stage boundaries.
/// Replaces the complex interrupt logic from the interrupt PR.
/// </summary>
public class InterruptionHook : IHookHandler, IDisposable
{
    private readonly ISimpleInterruptManager _interruptManager;
    private Task? _monitoringTask;
    private bool _isMonitoring;
    
    public string Name => "Interruption";
    public int Priority => 0; // Highest priority - run first
    
    public InterruptionHook(ISimpleInterruptManager interruptManager)
    {
        _interruptManager = interruptManager;
    }
    
    public async Task<HookResult> HandleAsync(ChatContext context, HookData data)
    {
        // Start monitoring on first Pre AI Streaming hook
        if (data.HookPoint == HookPoint.PreAIStreaming && !_isMonitoring)
        {
            StartMonitoring(context);
        }
        
        // Check for interrupt at any hook point
        if (_interruptManager.IsInterruptRequested())
        {
            // Set cancellation token
            context.CancellationTokenSource.Cancel();
            
            // Set interrupt flag
            context.State.IsInterrupted = true;
            
            // Stop monitoring
            StopMonitoring();
            
            // Request pipeline exit
            return HookResult.ExitPipeline();
        }
        
        // Stop monitoring after loop completes
        if (data.HookPoint == HookPoint.PostLoopIteration && _isMonitoring)
        {
            StopMonitoring();
        }
        
        return HookResult.Continue();
    }
    
    private void StartMonitoring(ChatContext context)
    {
        _isMonitoring = true;
        _interruptManager.StartMonitoring();
        
        // Start background task that will set cancellation token when interrupt detected
        _monitoringTask = Task.Run(async () =>
        {
            var interruptResult = await _interruptManager.WaitForInterruptAsync();
            
            if (interruptResult.WasInterrupted)
            {
                context.CancellationTokenSource.Cancel();
                context.State.IsInterrupted = true;
            }
        });
    }
    
    private void StopMonitoring()
    {
        _isMonitoring = false;
        _interruptManager.StopMonitoring();
    }
    
    public void Dispose()
    {
        StopMonitoring();
        _monitoringTask?.Dispose();
    }
}

/// <summary>
/// Hook that handles function call approval UI.
/// Replaces the 65-line HandleFunctionCallApproval method.
/// </summary>
public class FunctionApprovalHook : IHookHandler
{
    private readonly IFunctionApprovalUI _approvalUI;
    private readonly HashSet<string> _approvedFunctions = new();
    private readonly bool _autoApprove;
    
    public string Name => "FunctionApproval";
    public int Priority => 10; // Run before execution, but after interrupt check
    
    public FunctionApprovalHook(IFunctionApprovalUI approvalUI, bool autoApprove = false)
    {
        _approvalUI = approvalUI;
        _autoApprove = autoApprove;
    }
    
    public async Task<HookResult> HandleAsync(ChatContext context, HookData data)
    {
        // Only handle at PreToolCall hook point
        if (data.HookPoint != HookPoint.PreToolCall)
        {
            return HookResult.Continue();
        }
        
        // No tool calls to approve
        if (context.Pending.PendingToolCalls.Count == 0)
        {
            return HookResult.Continue();
        }
        
        // Check each pending tool call for approval
        var approvedCalls = new List<FunctionCall>();
        var deniedCalls = new List<FunctionCall>();
        
        foreach (var toolCall in context.Pending.PendingToolCalls)
        {
            // Auto-approve if enabled
            if (_autoApprove)
            {
                approvedCalls.Add(toolCall);
                continue;
            }
            
            // Already approved this session
            if (_approvedFunctions.Contains(toolCall.Name))
            {
                approvedCalls.Add(toolCall);
                continue;
            }
            
            // Ask user for approval
            var decision = await _approvalUI.PromptForApprovalAsync(
                toolCall.Name, 
                toolCall.Arguments,
                context.CancellationTokenSource.Token);
            
            switch (decision)
            {
                case FunctionCallDecision.Approved:
                    approvedCalls.Add(toolCall);
                    break;
                    
                case FunctionCallDecision.ApprovedForSession:
                    _approvedFunctions.Add(toolCall.Name);
                    approvedCalls.Add(toolCall);
                    break;
                    
                case FunctionCallDecision.Denied:
                    deniedCalls.Add(toolCall);
                    break;
                    
                case FunctionCallDecision.UserWantsControl:
                    // User pressed ESC ESC during approval
                    context.State.IsInterrupted = true;
                    
                    // Deny all pending calls
                    foreach (var call in context.Pending.PendingToolCalls)
                    {
                        context.Pending.PendingToolResults.Add(new FunctionResult
                        {
                            CallId = call.CallId,
                            Content = "Function call cancelled by user",
                            Success = false
                        });
                    }
                    
                    // Exit pipeline
                    return HookResult.ExitPipeline();
            }
        }
        
        // Update pending tool calls with only approved ones
        context.Pending.PendingToolCalls.Clear();
        context.Pending.PendingToolCalls.AddRange(approvedCalls);
        
        // Add denial results for denied calls
        foreach (var deniedCall in deniedCalls)
        {
            context.Pending.PendingToolResults.Add(new FunctionResult
            {
                CallId = deniedCall.CallId,
                Content = "Function call denied by user",
                Success = false
            });
        }
        
        // If all calls were denied, skip execution stage
        if (approvedCalls.Count == 0)
        {
            return HookResult.SkipStage();
        }
        
        return HookResult.Continue();
    }
}

/// <summary>
/// Hook that manages display buffer for interrupt scenarios.
/// Replaces display buffer trimming logic from interrupt PR.
/// </summary>
public class DisplayBufferHook : IHookHandler
{
    public string Name => "DisplayBuffer";
    public int Priority => 100; // Normal priority
    
    public Task<HookResult> HandleAsync(ChatContext context, HookData data)
    {
        switch (data.HookPoint)
        {
            case HookPoint.PreAIStreaming:
                // Clear display buffer at start of streaming
                context.Properties["DisplayBuffer"] = string.Empty;
                break;
                
            case HookPoint.PostAIStreaming:
                // Save display buffer after streaming
                var streamedContent = context.Pending.StreamingContent.ToString();
                context.Properties["DisplayBuffer"] = streamedContent;
                break;
                
            case HookPoint.PostLoopIteration:
                // Clear display buffer after successful iteration
                if (!context.State.IsInterrupted)
                {
                    context.Properties["DisplayBuffer"] = string.Empty;
                }
                break;
        }
        
        // If interrupted, trim content to display buffer
        if (context.State.IsInterrupted && data.HookPoint == HookPoint.PostAIStreaming)
        {
            var displayBuffer = context.Properties.GetValueOrDefault("DisplayBuffer") as string ?? "";
            var streamingContent = context.Pending.StreamingContent.ToString();
            
            if (!string.IsNullOrEmpty(displayBuffer) && streamingContent.Length > displayBuffer.Length)
            {
                // Trim streaming content to what was actually displayed
                context.Pending.StreamingContent.Clear();
                context.Pending.StreamingContent.Append(displayBuffer);
            }
        }
        
        return Task.FromResult(HookResult.Continue());
    }
}

/// <summary>
/// Hook that analyzes conversation patterns and provides suggestions.
/// Example of a "whacko thing" hook that modifies conversation flow.
/// </summary>
public class ConversationAnalysisHook : IHookHandler
{
    private readonly IConversationAnalyzer _analyzer;
    
    public string Name => "ConversationAnalysis";
    public int Priority => 50; // Mid priority
    
    public ConversationAnalysisHook(IConversationAnalyzer analyzer)
    {
        _analyzer = analyzer;
    }
    
    public async Task<HookResult> HandleAsync(ChatContext context, HookData data)
    {
        // Only analyze after user input
        if (data.HookPoint != HookPoint.PostUserInput)
        {
            return HookResult.Continue();
        }
        
        // Analyze conversation patterns
        var analysis = await _analyzer.AnalyzeAsync(context.Messages);
        
        // Store analysis for other hooks
        context.Properties["ConversationAnalysis"] = analysis;
        
        // If conversation is getting long, suggest summarization
        if (analysis.MessageCount > 20)
        {
            context.Instructions.Add(new ChatInstruction
            {
                Type = InstructionType.SystemPrompt,
                Content = "The conversation is getting long. Please be more concise."
            });
        }
        
        // If user is asking repetitive questions, inject context
        if (analysis.HasRepetitivePatterns)
        {
            var summary = await _analyzer.SummarizeRecentContext(context.Messages);
            context.Messages.Insert(0, new ChatMessage(ChatRole.System, 
                $"Recent context summary: {summary}"));
        }
        
        // If analysis suggests tool use, ensure tools are available
        if (analysis.SuggestsToolUse && !context.Properties.ContainsKey("ToolsEnabled"))
        {
            context.Properties["SuggestEnablingTools"] = true;
        }
        
        return HookResult.Continue();
    }
}

/// <summary>
/// Hook that implements conversation forking for experimental branches.
/// Example of advanced "whacko thing" that creates parallel conversations.
/// </summary>
public class ConversationForkingHook : IHookHandler
{
    public string Name => "ConversationForking";
    public int Priority => 200; // Low priority - runs after most hooks
    
    public async Task<HookResult> HandleAsync(ChatContext context, HookData data)
    {
        // Only fork at specific trigger point
        if (data.HookPoint != HookPoint.PostMessageAdd)
        {
            return HookResult.Continue();
        }
        
        // Only fork at certain conversation depth
        if (context.Messages.Count != 5)
        {
            return HookResult.Continue();
        }
        
        // Check if forking is enabled
        if (!context.Properties.GetValueOrDefault("EnableForking", false))
        {
            return HookResult.Continue();
        }
        
        // Create experimental branch
        var experimentalBranch = context.Clone();
        experimentalBranch.Properties["Branch"] = "experimental";
        experimentalBranch.Properties["ParentBranch"] = "main";
        
        // Start parallel processing (fire and forget)
        _ = Task.Run(async () =>
        {
            try
            {
                // Process experimental branch with different parameters
                var experimentalPipeline = CreateExperimentalPipeline();
                var result = await experimentalPipeline.ExecuteAsync(experimentalBranch);
                
                // Store result for later comparison
                context.Properties["ExperimentalResult"] = result;
            }
            catch (Exception ex)
            {
                // Log experimental failure but don't crash main branch
                Console.WriteLine($"Experimental branch failed: {ex.Message}");
            }
        });
        
        return HookResult.Continue();
    }
    
    private IChatPipeline CreateExperimentalPipeline()
    {
        // Create a variation of the pipeline with different settings
        return new ChatPipelineBuilder()
            .WithStage(/* ... different configuration ... */)
            .Build();
    }
}

// Supporting interfaces and enums

public interface ISimpleInterruptManager
{
    void StartMonitoring();
    void StopMonitoring();
    bool IsInterruptRequested();
    Task<InterruptResult> WaitForInterruptAsync();
}

public class InterruptResult
{
    public bool WasInterrupted { get; set; }
    public DateTime Timestamp { get; set; }
}

public interface IFunctionApprovalUI
{
    Task<FunctionCallDecision> PromptForApprovalAsync(
        string functionName, 
        string arguments, 
        CancellationToken cancellationToken);
}

public enum FunctionCallDecision
{
    Approved,
    ApprovedForSession,
    Denied,
    UserWantsControl
}

public interface IConversationAnalyzer
{
    Task<ConversationAnalysis> AnalyzeAsync(List<ChatMessage> messages);
    Task<string> SummarizeRecentContext(List<ChatMessage> messages);
}

public class ConversationAnalysis
{
    public int MessageCount { get; set; }
    public bool HasRepetitivePatterns { get; set; }
    public bool SuggestsToolUse { get; set; }
}
