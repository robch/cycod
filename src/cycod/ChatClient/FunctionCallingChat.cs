using Microsoft.Extensions.AI;
using System.Threading;

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
    /// Clears the conversation history and reinitializes with the original system prompt and persistent user messages.
    /// </summary>
    public void ClearChatHistory()
    {
        Conversation.Clear(_systemPrompt);
    }

    public FunctionCallingChat(IChatClient chatClient, string systemPrompt, FunctionFactory factory, ChatOptions? options, int? maxOutputTokens = null)
    {
        _systemPrompt = systemPrompt;
        _functionFactory = factory;
        _functionCallDetector = new FunctionCallDetector();

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
        Action<string, string, object?>? functionCallCallback = null,
        CancellationToken cancellationToken = default)
    {
        return await CompleteChatStreamingAsync(
            userPrompt, 
            new List<string>(), 
            messageCallback, 
            streamingCallback, 
            approveFunctionCall, 
            functionCallCallback,
            cancellationToken);
    }

    public async Task<string> CompleteChatStreamingAsync(
        string userPrompt,
        IEnumerable<string> imageFiles,
        Action<IList<ChatMessage>>? messageCallback = null,
        Action<ChatResponseUpdate>? streamingCallback = null,
        Func<string, string?, bool>? approveFunctionCall = null,
        Action<string, string, object?>? functionCallCallback = null,
        CancellationToken cancellationToken = default)
    {
        var message = CreateUserMessageWithImages(userPrompt, imageFiles);
        
        Conversation.Messages.Add(message);
        messageCallback?.Invoke(Conversation.Messages);

        var contentToReturn = string.Empty;
        while (true)
        {
            var responseContent = string.Empty;
            await foreach (var update in _chatClient.GetStreamingResponseAsync(Conversation.Messages, _options, cancellationToken))
            {
                // Check for cancellation before processing each update
                cancellationToken.ThrowIfCancellationRequested();
                
                _functionCallDetector.CheckForFunctionCall(update);

                var content = string.Join("", update.Contents
                    .Where(c => c is TextContent)
                    .Cast<TextContent>()
                    .Select(c => c.Text)
                    .ToList());

                if (update.FinishReason == ChatFinishReason.ContentFilter)
                {
                    content = $"{content}\nWARNING: Content filtered!";
                }

                responseContent += content;
                contentToReturn += content;

                streamingCallback?.Invoke(update);
            }

            if (TryCallFunctions(responseContent, approveFunctionCall, functionCallCallback, messageCallback))
            {
                _functionCallDetector.Clear();
                continue;
            }

            Conversation.Messages.Add(new ChatMessage(ChatRole.Assistant, responseContent));
            messageCallback?.Invoke(Conversation.Messages);

            return contentToReturn;
        }
    }

    private bool TryCallFunctions(string responseContent, Func<string, string?, bool>? approveFunctionCall, Action<string, string, object?>? functionCallCallback, Action<IList<ChatMessage>>? messageCallback)
    {
        var noFunctionsToCall = !_functionCallDetector.HasFunctionCalls();
        if (noFunctionsToCall) return false;
        
        var readyToCallFunctionCalls = _functionCallDetector.GetReadyToCallFunctionCalls();

        var emptyResponseContent = string.IsNullOrEmpty(responseContent);
        if (emptyResponseContent) responseContent = "Calling function(s)...";

        var assistantContent = readyToCallFunctionCalls
            .AsAIContentList()
            .Prepend(new TextContent(responseContent))
            .ToList();
        Conversation.Messages.Add(new ChatMessage(ChatRole.Assistant, assistantContent));
        messageCallback?.Invoke(Conversation.Messages);

        var functionCallResults = CallFunctions(readyToCallFunctionCalls, approveFunctionCall, functionCallCallback);

        var attachToToolMessage = functionCallResults
            .Where(c => c is FunctionResultContent)
            .Cast<AIContent>()
            .ToList();

        Conversation.Messages.Add(new ChatMessage(ChatRole.Tool, attachToToolMessage));
        messageCallback?.Invoke(Conversation.Messages);

        var otherContentToAttach = functionCallResults
            .Where(c => c is not FunctionResultContent)
            .ToList();
        if (otherContentToAttach.Any())
        {
            var hasTextContent = otherContentToAttach.Any(c => c is TextContent);
            if (!hasTextContent)
            {
                otherContentToAttach.Insert(0, new TextContent("attached content:"));
            }

            Conversation.Messages.Add(new ChatMessage(ChatRole.User, otherContentToAttach));
            messageCallback?.Invoke(Conversation.Messages);
        }

        return true;
    }

    private List<AIContent> CallFunctions(List<FunctionCallDetector.ReadyToCallFunctionCall> readyToCallFunctionCalls, Func<string, string?, bool>? approveFunctionCall, Action<string, string, object?>? functionCallCallback)
    {
        ConsoleHelpers.WriteDebugLine($"Calling functions: {string.Join(", ", readyToCallFunctionCalls.Select(call => call.Name))}");

        var functionResultContents = new List<AIContent>();
        foreach (var functionCall in readyToCallFunctionCalls)
        {
            var approved = approveFunctionCall?.Invoke(functionCall.Name, functionCall.Arguments) ?? true;

            var functionResult = approved
                ? CallFunction(functionCall, functionCallCallback)
                : DontCallFunction(functionCall, functionCallCallback);

            var asDataContent = functionResult as DataContent;
            if (asDataContent != null)
            {
                functionResultContents.Add(asDataContent);
                functionResultContents.Add(new FunctionResultContent(functionCall.CallId, "attaching data content"));
            }
            else
            {
                functionResultContents.Add(new FunctionResultContent(functionCall.CallId, functionResult));
            }
        }

        return functionResultContents;
    }

    private object CallFunction(FunctionCallDetector.ReadyToCallFunctionCall functionCall, Action<string, string, object?>? functionCallCallback)
    {
        functionCallCallback?.Invoke(functionCall.Name, functionCall.Arguments, null);

        ConsoleHelpers.WriteDebugLine($"Calling function: {functionCall.Name} with arguments: {functionCall.Arguments}");
        var functionResult = _functionFactory.TryCallFunction(functionCall.Name, functionCall.Arguments, out var functionResponse)
            ? functionResponse ?? "Function call succeeded"
            : $"Function not found or failed to execute: {functionResponse}";
        ConsoleHelpers.WriteDebugLine($"Function call result: {functionResult}");

        functionCallCallback?.Invoke(functionCall.Name, functionCall.Arguments, functionResult);

        return functionResult;
    }

    private object DontCallFunction(FunctionCallDetector.ReadyToCallFunctionCall functionCall, Action<string, string, object?>? functionCallCallback)
    {
        functionCallCallback?.Invoke(functionCall.Name, functionCall.Arguments, null);

        ConsoleHelpers.WriteDebugLine($"Function call not approved: {functionCall.Name} with arguments: {functionCall.Arguments}");
        var functionResult = "User did not approve function call";

        functionCallCallback?.Invoke(functionCall.Name, functionCall.Arguments, functionResult);

        return functionResult;
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
