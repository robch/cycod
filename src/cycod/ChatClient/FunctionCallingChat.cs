using Microsoft.Extensions.AI;

public class FunctionCallingChat : IAsyncDisposable
{
    /// <summary>
    /// Metadata for the current conversation.
    /// </summary>
    public ConversationMetadata? Metadata { get; private set; }
    
    /// <summary>
    /// Pending title notification to show before next assistant response.
    /// </summary>
    private string? _pendingTitleNotification;

    /// <summary>
    /// Updates the conversation metadata.
    /// </summary>
    /// <param name="metadata">New metadata to set</param>
    public void UpdateMetadata(ConversationMetadata? metadata)
    {
        Metadata = metadata;
    }
    
    /// <summary>
    /// Sets a pending title notification to show before the next assistant response.
    /// </summary>
    /// <param name="newTitle">The new title to notify about</param>
    public void SetPendingTitleNotification(string newTitle)
    {
        _pendingTitleNotification = newTitle;
    }
    
    /// <summary>
    /// Checks if there's a pending title notification.
    /// </summary>
    /// <returns>True if there's a pending notification</returns>
    public bool HasPendingTitleNotification()
    {
        return !string.IsNullOrEmpty(_pendingTitleNotification);
    }
    
    /// <summary>
    /// Gets and clears the pending title notification.
    /// </summary>
    /// <returns>The pending notification, or null if none</returns>
    public string? GetAndClearPendingTitleNotification()
    {
        var notification = _pendingTitleNotification;
        _pendingTitleNotification = null;
        return notification;
    }

    public FunctionCallingChat(IChatClient chatClient, string systemPrompt, FunctionFactory factory, ChatOptions? options, int? maxOutputTokens = null)
    {
        _systemPrompt = systemPrompt;
        _functionFactory = factory;
        _functionCallDetector = new FunctionCallDetector();

        var useMicrosoftExtensionsAIFunctionCalling = false; // Can't use this for now; (1) doesn't work with copilot w/ all models, (2) functionCallCallback not available
        _chatClient = useMicrosoftExtensionsAIFunctionCalling
            ? chatClient.AsBuilder().UseFunctionInvocation().Build()
            : chatClient;

        var tools = _functionFactory.GetAITools().ToList();
        ConsoleHelpers.WriteDebugLine($"FunctionCallingChat: Found {tools.Count} tools in FunctionFactory");

        _messages = new List<ChatMessage>();
        _options = new ChatOptions()
        {
            ModelId = options?.ModelId,
            ToolMode = options?.ToolMode,
            Tools = tools,
            MaxOutputTokens = maxOutputTokens.HasValue
                ? maxOutputTokens.Value
                : options?.MaxOutputTokens,
        };

        if (maxOutputTokens.HasValue) _options.MaxOutputTokens = maxOutputTokens.Value;

        ClearChatHistory();
    }

    public void ClearChatHistory()
    {
        _messages.Clear();
        _messages.Add(new ChatMessage(ChatRole.System, _systemPrompt));
        _messages.AddRange(_userMessageAdds);
    }

    public void AddUserMessage(string userMessage, int maxPromptTokenTarget = 0, int maxChatTokenTarget = 0)
    {
        _userMessageAdds.Add(new ChatMessage(ChatRole.User, userMessage));
        _userMessageAdds.TryTrimToTarget(
            maxPromptTokenTarget: maxPromptTokenTarget,
            maxChatTokenTarget: maxChatTokenTarget);

        _messages.Add(new ChatMessage(ChatRole.User, userMessage));
        _messages.TryTrimToTarget(
            maxPromptTokenTarget: maxPromptTokenTarget,
            maxChatTokenTarget: maxChatTokenTarget);
    }
    
    public void AddUserMessages(IEnumerable<string> userMessages, int maxPromptTokenTarget = 0, int maxChatTokenTarget = 0)
    {
        foreach (var userMessage in userMessages)
        {
            AddUserMessage(userMessage, maxPromptTokenTarget);
        }

        _messages.TryTrimToTarget(maxChatTokenTarget: maxChatTokenTarget);
    }

    public void LoadChatHistory(string fileName, int maxPromptTokenTarget = 0, int maxToolTokenTarget = 0, int maxChatTokenTarget = 0, bool useOpenAIFormat = ChatHistoryDefaults.UseOpenAIFormat)
    {
        // Load messages and metadata
        var (metadata, messages) = AIExtensionsChatHelpers.ReadChatHistoryFromFileWithMetadata(fileName, useOpenAIFormat);
        
        // Store metadata
        Metadata = metadata;
        
        // If no metadata found, create default using current time
        if (Metadata == null)
        {
            Metadata = ConversationMetadataHelpers.CreateDefault();
        }

        // Clear and repopulate messages
        var hasSystemMessage = messages.Any(x => x.Role == ChatRole.System);
        if (hasSystemMessage) _messages.Clear();

        _messages.AddRange(messages);
        _messages.FixDanglingToolCalls();
        _messages.TryTrimToTarget(maxPromptTokenTarget, maxToolTokenTarget, maxChatTokenTarget);
    }

    public void SaveChatHistoryToFile(string fileName, bool useOpenAIFormat = ChatHistoryDefaults.UseOpenAIFormat, string? saveToFolderOnAccessDenied = null)
    {
        // Initialize metadata if not present (empty for now, ready for future properties)
        if (Metadata == null)
        {
            Metadata = ConversationMetadataHelpers.CreateDefault();
        }

        // Save with metadata (file system handles creation/modification times)
        _messages.SaveChatHistoryToFile(fileName, Metadata, useOpenAIFormat, saveToFolderOnAccessDenied);
    }

    public void SaveTrajectoryToFile(string fileName, bool useOpenAIFormat = ChatHistoryDefaults.UseOpenAIFormat, string? saveToFolderOnAccessDenied = null)
    {
        _messages.SaveTrajectoryToFile(fileName, useOpenAIFormat, saveToFolderOnAccessDenied);
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
        
        _messages.Add(message);
        messageCallback?.Invoke(_messages);

        var contentToReturn = string.Empty;
        while (true)
        {
            var responseContent = string.Empty;
            await foreach (var update in _chatClient.GetStreamingResponseAsync(_messages, _options))
            {
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

            _messages.Add(new ChatMessage(ChatRole.Assistant, responseContent));
            messageCallback?.Invoke(_messages);

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
        _messages.Add(new ChatMessage(ChatRole.Assistant, assistantContent));
        messageCallback?.Invoke(_messages);

        var functionCallResults = CallFunctions(readyToCallFunctionCalls, approveFunctionCall, functionCallCallback);

        var attachToToolMessage = functionCallResults
            .Where(c => c is FunctionResultContent)
            .Cast<AIContent>()
            .ToList();

        _messages.Add(new ChatMessage(ChatRole.Tool, attachToToolMessage));
        messageCallback?.Invoke(_messages);

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

            _messages.Add(new ChatMessage(ChatRole.User, otherContentToAttach));
            messageCallback?.Invoke(_messages);
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
    private readonly List<ChatMessage> _userMessageAdds = new();

    private readonly FunctionFactory _functionFactory;
    private readonly FunctionCallDetector _functionCallDetector;
    private readonly ChatOptions _options;
    private readonly IChatClient _chatClient;
    private List<ChatMessage> _messages;
}
