using Microsoft.Extensions.AI;
using System.Text.Json;

public class FunctionCallingChat
{
    public FunctionCallingChat(IChatClient chatClient, string systemPrompt, FunctionFactory factory, int? maxOutputTokens = null)
    {
        _systemPrompt = systemPrompt;
        _functionFactory = factory;
        _functionCallDetector = new FunctionCallDetector();

        var useMicrosoftExtensionsAIFunctionCalling = false; // Can't use this for now; (1) doesn't work with copilot w/ all models, (2) functionCallCallback not available
        _chatClient = useMicrosoftExtensionsAIFunctionCalling
            ? chatClient.AsBuilder().UseFunctionInvocation().Build()
            : chatClient;

        _messages = new List<ChatMessage>();
        _options = new ChatOptions()
        {
            Tools = _functionFactory.GetAITools().ToList(),
            ToolMode = null
        };

        if (maxOutputTokens.HasValue) _options.MaxOutputTokens = maxOutputTokens.Value;

        ClearChatHistory();
    }

    public void ClearChatHistory()
    {
        _messages.Clear();
        _messages.Add(new ChatMessage(ChatRole.System, _systemPrompt));

        foreach (var userMessage in _userMessageAdds)
        {
            _messages.Add(new ChatMessage(ChatRole.User, userMessage));
        }
    }
    
    public void AddUserMessage(string userMessage, int tokenTrimTarget = 0)
    {
        _userMessageAdds.Add(userMessage);
        _messages.Add(new ChatMessage(ChatRole.User, userMessage));

        if (tokenTrimTarget > 0)
        {
            _messages.TryTrimToTarget(tokenTrimTarget);
        }
    }
    
    public void AddUserMessages(IEnumerable<string> userMessages, int tokenTrimTarget = 0)
    {
        foreach (var userMessage in userMessages)
        {
            AddUserMessage(userMessage);
        }

        if (tokenTrimTarget > 0)
        {
            _messages.TryTrimToTarget(tokenTrimTarget);
        }
    }

    public void LoadChatHistory(string fileName, int tokenTrimTarget = 0)
    {
        _messages.ReadChatHistoryFromFile(fileName);

        if (tokenTrimTarget > 0)
        {
            _messages.TryTrimToTarget(tokenTrimTarget);
        }
    }

    public void SaveChatHistoryToFile(string fileName)
    {
        _messages.SaveChatHistoryToFile(fileName);
    }

    public async Task<string> CompleteChatStreamingAsync(
        string userPrompt,
        Action<IList<ChatMessage>>? messageCallback = null,
        Action<ChatResponseUpdate>? streamingCallback = null,
        Action<string, string, string?>? functionCallCallback = null)
    {
        _messages.Add(new ChatMessage(ChatRole.User, userPrompt));
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

            if (_functionCallDetector.HasFunctionCalls())
            {
                // TODO: Refactor into separate function(s)
                var readyToCallFunctionCalls = _functionCallDetector.GetReadyToCallFunctionCalls();
                ConsoleHelpers.WriteDebugLine($"Function calls ready: {string.Join(", ", readyToCallFunctionCalls.Select(call => call.Name))}");

                var assistantToolCalls = readyToCallFunctionCalls
                    .Select(call => new FunctionCallContent(
                        call.CallId,
                        call.Name,
                        JsonSerializer.Deserialize<Dictionary<string, object?>>(call.Arguments)))
                    .Cast<AIContent>()
                    .ToList();
                _messages.Add(new ChatMessage(ChatRole.Assistant, assistantToolCalls));
                messageCallback?.Invoke(_messages);

                ConsoleHelpers.WriteDebugLine($"Calling functions: {string.Join(", ", readyToCallFunctionCalls.Select(call => call.Name))}");
                var functionResultContents = new List<FunctionResultContent>();
                foreach (var functionCall in readyToCallFunctionCalls)
                {
                    functionCallCallback?.Invoke(functionCall.Name, functionCall.Arguments, null);

                    ConsoleHelpers.WriteDebugLine($"Calling function: {functionCall.Name} with arguments: {functionCall.Arguments}");
                    var functionResult = _functionFactory.TryCallFunction(functionCall.Name, functionCall.Arguments, out var functionResponse)
                        ? functionResponse ?? "Function call succeeded"
                        : "Function not found or failed to execute";
                    ConsoleHelpers.WriteDebugLine($"Function call result: {functionResult}");

                    functionResultContents.Add(new FunctionResultContent(functionCall.CallId, functionResult));
                    functionCallCallback?.Invoke(functionCall.Name, functionCall.Arguments, functionResult);
                }

                var toolMessageContent = new List<AIContent>();
                toolMessageContent.Add(new TextContent(responseContent));
                toolMessageContent.AddRange(functionResultContents);

                _functionCallDetector.Clear();
                _messages.Add(new ChatMessage(ChatRole.Tool, toolMessageContent));
                messageCallback?.Invoke(_messages);

                continue;
            }

            _messages.Add(new ChatMessage(ChatRole.Assistant, responseContent));
            messageCallback?.Invoke(_messages);

            return contentToReturn;
        }
    }

    private readonly string _systemPrompt;
    private readonly List<string> _userMessageAdds = new();

    private readonly FunctionFactory _functionFactory;
    private readonly FunctionCallDetector _functionCallDetector;
    private readonly ChatOptions _options;
    private readonly IChatClient _chatClient;
    private List<ChatMessage> _messages;
}
