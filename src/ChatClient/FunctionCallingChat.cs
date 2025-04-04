using Microsoft.Extensions.AI;
using System.Text.Json;

public class FunctionCallingChat
{
    public FunctionCallingChat(IChatClient chatClient, string systemPrompt, FunctionFactory factory, int? maxOutputTokens = null)
    {
        _systemPrompt = systemPrompt;
        _functionFactory = factory;
        _chatClient = chatClient.AsBuilder().UseFunctionInvocation().Build();

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
        // TODO: deal with functionCallCallback
        
        _messages.Add(new ChatMessage(ChatRole.User, userPrompt));
        messageCallback?.Invoke(_messages);

        var responseContent = string.Empty;
        await foreach (var update in _chatClient.GetStreamingResponseAsync(_messages, _options))
        {
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
            streamingCallback?.Invoke(update);
        }

        _messages.Add(new ChatMessage(ChatRole.Assistant, responseContent));
        messageCallback?.Invoke(_messages);

        return responseContent;
    }

    private readonly string _systemPrompt;
    private readonly List<string> _userMessageAdds = new();

    private readonly FunctionFactory _functionFactory;
    private readonly ChatOptions _options;
    private readonly IChatClient _chatClient;
    private List<ChatMessage> _messages;
}
