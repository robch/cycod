using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class FunctionCallingChat
{
    private readonly string _openAISystemPrompt;
    private readonly FunctionFactory _functionFactory;
    private readonly FunctionCallContext _functionCallContext;
    private readonly ChatCompletionOptions _options;
    private readonly ChatClient _chatClient;
    private readonly List<ChatMessage> _messages;

    public FunctionCallingChat(ChatClient openAIClient, string openAISystemPrompt, FunctionFactory factory)
    {
        _openAISystemPrompt = openAISystemPrompt;
        _functionFactory = factory;
        _chatClient = openAIClient;

        _messages = new List<ChatMessage>();
        _options = new ChatCompletionOptions();

        foreach (var tool in _functionFactory.GetChatTools())
        {
            _options.Tools.Add(tool);
        }

        _functionCallContext = new FunctionCallContext(_functionFactory, _messages);
        ClearConversation();
    }

    public void ClearConversation()
    {
        _messages.Clear();
        _messages.Add(ChatMessage.CreateSystemMessage(_openAISystemPrompt));
    }

    public async Task<string> GetChatCompletionsStreamingAsync(string userPrompt, Action<StreamingChatCompletionUpdate>? callback = null, Action<string, string, string?>? functionCallCallback = null)
    {
        _messages.Add(ChatMessage.CreateUserMessage(userPrompt));
        var responseContent = string.Empty;

        while (true)
        {
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

                if (string.IsNullOrEmpty(content))
                    continue;

                responseContent += content;
                callback?.Invoke(update);
            }

            if (_functionCallContext.TryCallFunctions(responseContent, functionCallCallback))
            {
                _functionCallContext.Clear();
                continue;
            }

            _messages.Add(ChatMessage.CreateAssistantMessage(responseContent));
            return responseContent;
        }
    }
}
