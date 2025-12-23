using Microsoft.Extensions.AI;
using CycoDev.ChatPipeline;

public class FunctionCallingChat : IAsyncDisposable
{
    /// <summary>
    /// The conversation data and operations.
    /// </summary>
    public Conversation Conversation { get; private set; }
    
    /// <summary>
    /// The notification management system.
    /// </summary>
    public NotificationManager Notifications { get; private set; }
    
    /// <summary>
    /// The chat pipeline used for processing conversations.
    /// Can be accessed to add hooks or inspect configuration.
    /// </summary>
    public IChatPipeline Pipeline { get; private set; }

    /// <summary>
    /// Clears the conversation history and reinitializes with the original system prompt and persistent user messages.
    /// </summary>
    public void ClearChatHistory()
    {
        Conversation.Clear(_systemPrompt);
    }

    public FunctionCallingChat(
        IChatClient chatClient, 
        string systemPrompt, 
        FunctionFactory factory, 
        ChatOptions? options, 
        int? maxOutputTokens = null,
        IChatPipeline? pipeline = null)
    {
        _systemPrompt = systemPrompt;
        _functionFactory = factory;
        _functionCallDetector = new FunctionCallDetector();
        
        // Use provided pipeline or create default
        Pipeline = pipeline ?? ChatPipelineFactory.CreateStandardPipeline();

        // Initialize composition objects
        Conversation = new Conversation();
        Notifications = new NotificationManager();

        var useMicrosoftExtensionsAIFunctionCalling = false; // Can't use this for now; (1) doesn't work with copilot w/ all models, (2) functionCallCallback not available
        _chatClient = useMicrosoftExtensionsAIFunctionCalling
            ? chatClient.AsBuilder().UseFunctionInvocation().Build()
            : chatClient;

        var tools = _functionFactory.GetAITools().ToList();
        ConsoleHelpers.WriteDebugLine($"FunctionCallingChat: Found {tools.Count} tools in FunctionFactory");

        _options = new ChatOptions()
        {
            ModelId = options?.ModelId,
            ToolMode = options?.ToolMode,
            Tools = tools,
            MaxOutputTokens = maxOutputTokens.HasValue
                ? maxOutputTokens.Value
                : options?.MaxOutputTokens,
        };

        ClearChatHistory();
    }





    public async Task<string> CompleteChatStreamingAsync(
        string userPrompt,
        Action<IList<ChatMessage>>? messageCallback = null,
        Action<ChatResponseUpdate>? streamingCallback = null,
        Func<string, string?, bool>? approveFunctionCall = null,
        Action<string, string, object?>? functionCallCallback = null)
    {
        return await CompleteChatStreamingAsync(
            userPrompt, 
            new List<string>(), 
            messageCallback, 
            streamingCallback, 
            approveFunctionCall, 
            functionCallCallback);
    }

    public async Task<string> CompleteChatStreamingAsync(
        string userPrompt,
        IEnumerable<string> imageFiles,
        Action<IList<ChatMessage>>? messageCallback = null,
        Action<ChatResponseUpdate>? streamingCallback = null,
        Func<string, string?, bool>? approveFunctionCall = null,
        Action<string, string, object?>? functionCallCallback = null)
    {
        var message = CreateUserMessageWithImages(userPrompt, imageFiles);
        
        // TODO: Execute PreUserMessageAdd hook here once architecture supports it
        // The user message is added before pipeline execution starts, so hooks
        // cannot be executed at this point. Consider moving user message addition
        // into the pipeline as a first stage (UserInputStage) to enable hook execution.
        
        Conversation.Messages.Add(message);
        messageCallback?.Invoke(Conversation.Messages);
        
        // TODO: Execute PostUserMessageAdd hook here once architecture supports it

        // Create context for pipeline
        var context = ChatPipelineFactory.CreateContext(
            _chatClient,
            _options,
            _functionCallDetector,
            _functionFactory,
            Conversation,
            userPrompt,
            imageFiles,
            messageCallback,
            streamingCallback,
            approveFunctionCall,
            functionCallCallback);
        
        // Execute pipeline (use stored pipeline instance)
        var result = await Pipeline.ExecuteAsync(context);
        
        return result.Content;
    }

    public async ValueTask DisposeAsync()
    {
        if (_functionFactory is McpFunctionFactory mcpFactory)
        {
            await mcpFactory.DisposeAsync();
        }
    }

    private ChatMessage CreateUserMessageWithImages(string userPrompt, IEnumerable<string> imageFiles)
    {
        var hasImages = imageFiles.Any();
        var needsPrompt = string.IsNullOrEmpty(userPrompt) && !hasImages;
        var updatedPrompt = needsPrompt ? "=>" : userPrompt;

        var message = new ChatMessage(ChatRole.User, updatedPrompt);
        
        foreach (var imageFile in imageFiles)
        {
            if (File.Exists(imageFile))
            {
                try
                {
                    var imageBytes = File.ReadAllBytes(imageFile);
                    var mediaType = ImageResolver.GetMediaTypeFromFileExtension(imageFile);
                    message.Contents.Add(new DataContent(imageBytes, mediaType));
                }
                catch (Exception ex)
                {
                    ConsoleHelpers.LogException(ex, $"Failed to load image {imageFile}");
                }
            }
        }
        
        return message;
    }

    private readonly string _systemPrompt;

    private readonly FunctionFactory _functionFactory;
    private readonly FunctionCallDetector _functionCallDetector;
    private readonly ChatOptions _options;
    private readonly IChatClient _chatClient;

}
