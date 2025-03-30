using OpenAI.Chat;

public class FunctionCallingChat
{
    public FunctionCallingChat(ChatClient openAIClient, string systemPrompt, FunctionFactory factory, int? maxOutputTokens = null)
    {
        _systemPrompt = systemPrompt;
        _functionFactory = factory;
        _chatClient = openAIClient;

        _messages = new List<ChatMessage>();
        _options = new ChatCompletionOptions();

        if (maxOutputTokens.HasValue) _options.MaxOutputTokenCount = maxOutputTokens.Value;

        foreach (var tool in _functionFactory.GetChatTools())
        {
            _options.Tools.Add(tool);
        }

        _functionCallContext = new FunctionCallContext(_functionFactory, _messages);
        ClearChatHistory();
    }

    public void ClearChatHistory()
    {
        _messages.Clear();
        _messages.Add(ChatMessage.CreateSystemMessage(_systemPrompt));

        foreach (var userMessage in _userMessageAdds)
        {
            _messages.Add(ChatMessage.CreateUserMessage(userMessage));
        }
    }
    
    public void AddUserMessage(string userMessage)
    {
        _userMessageAdds.Add(userMessage);
        _messages.Add(ChatMessage.CreateUserMessage(userMessage));
    }
    
    public void AddUserMessages(IEnumerable<string> userMessages)
    {
        foreach (var userMessage in userMessages)
        {
            AddUserMessage(userMessage);
        }
    }

    public void LoadChatHistory(string fileName)
    {
        _messages.ReadChatHistoryFromFile(fileName);
    }

    public void SaveChatHistoryToFile(string fileName)
    {
        _messages.SaveChatHistoryToFile(fileName);
    }

    public async Task<string> CompleteChatStreamingAsync(
        string userPrompt,
        Action<IList<ChatMessage>>? messageCallback = null,
        Action<StreamingChatCompletionUpdate>? streamingCallback = null,
        Action<string, string, string?>? functionCallCallback = null)
    {
        _messages.Add(ChatMessage.CreateUserMessage(userPrompt));
        if (messageCallback != null) messageCallback(_messages);

        var contentToReturn = string.Empty;
        while (true)
        {
            var responseContent = string.Empty;
            var response = _chatClient.CompleteChatStreamingAsync(_messages, _options);
            await foreach (var update in response)
            {
                _functionCallContext.CheckForUpdate(update);

                var content = string.Join("", update.ContentUpdate
                    .Where(x => x.Kind == ChatMessageContentPartKind.Text)
                    .Select(x => x.Text)
                    .ToList());

                if (update.FinishReason == ChatFinishReason.ContentFilter)
                {
                    content = $"{content}\nWARNING: Content filtered!";
                }

                // if (string.IsNullOrEmpty(content))
                //     continue;

                responseContent += content;
                contentToReturn += content;

                streamingCallback?.Invoke(update);
            }

            if (_functionCallContext.TryCallFunctions(responseContent, functionCallCallback, messageCallback))
            {
                _functionCallContext.Clear();
                continue;
            }

            _messages.Add(ChatMessage.CreateAssistantMessage(responseContent));
            if (messageCallback != null) messageCallback(_messages);

            return contentToReturn;
        }
    }

    private readonly string _systemPrompt;
    private readonly List<string> _userMessageAdds = new();

    private readonly FunctionFactory _functionFactory;
    private readonly FunctionCallContext _functionCallContext;
    private readonly ChatCompletionOptions _options;
    private readonly ChatClient _chatClient;
    private readonly List<ChatMessage> _messages;
}
