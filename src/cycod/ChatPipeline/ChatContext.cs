using Microsoft.Extensions.AI;
using System.Text;

namespace CycoDev.ChatPipeline;

/// <summary>
/// The mutable context object that flows through the pipeline.
/// Contains conversation state, pending data, and execution metadata.
/// </summary>
public class ChatContext
{
    /// <summary>
    /// The conversation messages - the main data structure that hooks can manipulate.
    /// </summary>
    public List<ChatMessage> Messages { get; set; } = new();
    
    /// <summary>
    /// Pending data that exists between pipeline stages.
    /// </summary>
    public PendingData Pending { get; set; } = new();
    
    /// <summary>
    /// Current execution state.
    /// </summary>
    public ChatState State { get; set; } = new();
    
    /// <summary>
    /// Arbitrary key-value store for hooks to maintain state across stages.
    /// </summary>
    public Dictionary<string, object> Properties { get; set; } = new();
    
    /// <summary>
    /// Instructions for modifying conversation behavior.
    /// </summary>
    public List<ChatInstruction> Instructions { get; set; } = new();
    
    /// <summary>
    /// The original user prompt for this conversation turn.
    /// </summary>
    public string UserPrompt { get; set; } = string.Empty;
    
    /// <summary>
    /// Image files attached to the user message.
    /// </summary>
    public IEnumerable<string> ImageFiles { get; set; } = Enumerable.Empty<string>();
    
    /// <summary>
    /// The accumulated content to return at the end.
    /// </summary>
    public string ContentToReturn { get; set; } = string.Empty;
    
    /// <summary>
    /// The chat client for making AI calls.
    /// </summary>
    public IChatClient? ChatClient { get; set; }
    
    /// <summary>
    /// The chat options for AI calls.
    /// </summary>
    public ChatOptions? ChatOptions { get; set; }
    
    /// <summary>
    /// The function call detector.
    /// </summary>
    public FunctionCallDetector? FunctionCallDetector { get; set; }
    
    /// <summary>
    /// The function factory for executing tools.
    /// </summary>
    public FunctionFactory? FunctionFactory { get; set; }
    
    /// <summary>
    /// Callbacks for pipeline events.
    /// </summary>
    public ChatCallbacks Callbacks { get; set; } = new();
    
    /// <summary>
    /// Cancellation token for interrupting long-running operations.
    /// </summary>
    public CancellationToken CancellationToken { get; set; } = CancellationToken.None;
    
    /// <summary>
    /// Hook executor function - set by the pipeline to allow stages to execute hooks.
    /// </summary>
    public Func<HookPoint, ChatContext, HookData?, Task<HookResult>>? HookExecutor { get; set; }
    
    /// <summary>
    /// Creates a shallow clone of this context.
    /// </summary>
    public ChatContext Clone()
    {
        return new ChatContext
        {
            Messages = new List<ChatMessage>(Messages),
            Pending = Pending.Clone(),
            State = State.Clone(),
            Properties = new Dictionary<string, object>(Properties),
            Instructions = new List<ChatInstruction>(Instructions),
            UserPrompt = UserPrompt,
            ImageFiles = ImageFiles,
            ContentToReturn = ContentToReturn,
            ChatClient = ChatClient,
            ChatOptions = ChatOptions,
            FunctionCallDetector = FunctionCallDetector,
            FunctionFactory = FunctionFactory,
            Callbacks = Callbacks,
            CancellationToken = CancellationToken,
            HookExecutor = HookExecutor
        };
    }
}

/// <summary>
/// Pending data that exists between pipeline stages.
/// </summary>
public class PendingData
{
    /// <summary>
    /// User message about to be added to conversation.
    /// </summary>
    public ChatMessage? PendingUserMessage { get; set; }
    
    /// <summary>
    /// Assistant message about to be added to conversation.
    /// </summary>
    public ChatMessage? PendingAssistantMessage { get; set; }
    
    /// <summary>
    /// Function calls detected from AI response.
    /// </summary>
    public List<FunctionCallDetector.ReadyToCallFunctionCall> PendingToolCalls { get; set; } = new();
    
    /// <summary>
    /// Function call results about to be added to conversation.
    /// </summary>
    public List<AIContent> PendingToolResults { get; set; } = new();
    
    /// <summary>
    /// Content accumulated during streaming.
    /// </summary>
    public StringBuilder StreamingContent { get; set; } = new();
    
    /// <summary>
    /// Raw streaming updates from AI.
    /// </summary>
    public List<ChatResponseUpdate> StreamingUpdates { get; set; } = new();
    
    /// <summary>
    /// The AI response content for the current turn.
    /// </summary>
    public string ResponseContent { get; set; } = string.Empty;
    
    public PendingData Clone()
    {
        return new PendingData
        {
            PendingUserMessage = PendingUserMessage,
            PendingAssistantMessage = PendingAssistantMessage,
            PendingToolCalls = new List<FunctionCallDetector.ReadyToCallFunctionCall>(PendingToolCalls),
            PendingToolResults = new List<AIContent>(PendingToolResults),
            StreamingContent = new StringBuilder(StreamingContent.ToString()),
            StreamingUpdates = new List<ChatResponseUpdate>(StreamingUpdates),
            ResponseContent = ResponseContent
        };
    }
}

/// <summary>
/// Current execution state of the pipeline.
/// </summary>
public class ChatState
{
    /// <summary>
    /// Name of the currently executing stage.
    /// </summary>
    public string CurrentStage { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether the pipeline was interrupted by the user.
    /// </summary>
    public bool IsInterrupted { get; set; } = false;
    
    /// <summary>
    /// Whether the pipeline should exit the conversation loop.
    /// </summary>
    public bool ShouldExitLoop { get; set; } = false;
    
    /// <summary>
    /// Whether the next stage should be skipped.
    /// </summary>
    public bool ShouldSkipNextStage { get; set; } = false;
    
    /// <summary>
    /// Current loop iteration number.
    /// </summary>
    public int LoopIteration { get; set; } = 0;
    
    /// <summary>
    /// Whether functions were called in this iteration.
    /// </summary>
    public bool FunctionsWereCalled { get; set; } = false;
    
    public ChatState Clone()
    {
        return new ChatState
        {
            CurrentStage = CurrentStage,
            IsInterrupted = IsInterrupted,
            ShouldExitLoop = ShouldExitLoop,
            ShouldSkipNextStage = ShouldSkipNextStage,
            LoopIteration = LoopIteration,
            FunctionsWereCalled = FunctionsWereCalled
        };
    }
}

/// <summary>
/// Instruction for modifying conversation behavior.
/// </summary>
public class ChatInstruction
{
    public string Type { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>
/// Callbacks for pipeline events.
/// </summary>
public class ChatCallbacks
{
    public Action<IList<ChatMessage>>? MessageCallback { get; set; }
    public Action<ChatResponseUpdate>? StreamingCallback { get; set; }
    public Func<string, string?, bool>? ApproveFunctionCall { get; set; }
    public Action<string, string, object?>? FunctionCallCallback { get; set; }
}
