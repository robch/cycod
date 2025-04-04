using Microsoft.Extensions.AI;
using OpenAI.Chat;
using System.Text.Json;

public class FunctionCallingChat
{
    public FunctionCallingChat(IChatClient chatClient, string systemPrompt, FunctionFactory factory, int? maxOutputTokens = null)
    {
        _systemPrompt = systemPrompt;
        _functionFactory = factory;
        _chatClient = chatClient;

        _messages = new List<ChatMessage>();
        _options = new ChatOptions();

        if (maxOutputTokens.HasValue) _options.MaxOutputTokens = maxOutputTokens.Value;

        // Add tools to options
        var tools = _functionFactory.GetChatTools();
        foreach (var tool in tools)
        {
            // Convert OpenAI.Chat.ChatTool to Microsoft.Extensions.AI.AITool
            if (tool.Type == "function")
            {
                var functionSchema = tool.FunctionParameters.ToString();
                var aiFunction = new AIFunction(
                    tool.FunctionName, 
                    tool.FunctionDescription, 
                    functionSchema);
                
                _options.Tools.Add(aiFunction);
            }
        }

        // Create our function call detector and context
        _functionCallDetector = new FunctionCallDetector();
        _functionCallContext = new FunctionCallContext(_functionFactory, ConvertToOpenAIChatMessages(_messages));
        
        ClearChatHistory();
    }

    public void ClearChatHistory()
    {
        _messages.Clear();
        _messages.Add(new ChatMessage(ChatRole.System, new TextContent(_systemPrompt)));

        foreach (var userMessage in _userMessageAdds)
        {
            _messages.Add(new ChatMessage(ChatRole.User, new TextContent(userMessage)));
        }
    }
    
    public void AddUserMessage(string userMessage, int tokenTrimTarget = 0)
    {
        _userMessageAdds.Add(userMessage);
        _messages.Add(new ChatMessage(ChatRole.User, new TextContent(userMessage)));

        if (tokenTrimTarget > 0)
        {
            // You may need to adapt this to work with Microsoft.Extensions.AI message trimming
            // _messages.TryTrimToTarget(tokenTrimTarget);
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
            // You may need to adapt this to work with Microsoft.Extensions.AI message trimming
            // _messages.TryTrimToTarget(tokenTrimTarget);
        }
    }

    public void LoadChatHistory(string fileName, int tokenTrimTarget = 0)
    {
        // You'll need to adapt this method to work with Microsoft.Extensions.AI message types
        // _messages.ReadChatHistoryFromFile(fileName);
        
        // Load the chat history from the file
        var json = File.ReadAllText(fileName);
        var openAIMessages = JsonSerializer.Deserialize<List<OpenAI.Chat.ChatMessage>>(json);
        
        if (openAIMessages != null)
        {
            // Convert to Microsoft.Extensions.AI messages
            _messages = openAIMessages.Select(m => m.ToExtensionsAIChatMessage()).ToList();
        }

        if (tokenTrimTarget > 0)
        {
            // Adapt token trimming for Microsoft.Extensions.AI messages
            // _messages.TryTrimToTarget(tokenTrimTarget);
        }
    }

    public void SaveChatHistoryToFile(string fileName)
    {
        // Convert messages to OpenAI format for serialization
        var openAIMessages = _messages.Select(m => m.ToOpenAIChatMessage()).ToList();
        
        // Serialize and save
        var json = JsonSerializer.Serialize(openAIMessages);
        File.WriteAllText(fileName, json);
    }

    public async Task<string> CompleteChatStreamingAsync(
        string userPrompt,
        Action<IList<ChatMessage>>? messageCallback = null,
        Action<StreamingChatCompletionUpdate>? streamingCallback = null,
        Action<string, string, string?>? functionCallCallback = null)
    {
        // Add the user message
        _messages.Add(new ChatMessage(ChatRole.User, new TextContent(userPrompt)));
        
        // Call the message callback if provided
        if (messageCallback != null)
        {
            messageCallback(_messages);
        }

        var contentToReturn = string.Empty;
        while (true)
        {
            var responseContent = string.Empty;
            _functionCallDetector.Clear();
            
            // Use the IChatClient to get a streaming response
            await foreach (var update in _chatClient.GetStreamingResponseAsync(_messages, _options))
            {
                // Check for function calls in the update
                _functionCallDetector.CheckForFunctionCall(update);
                
                // Extract text content from the update
                foreach (var content in update.Contents)
                {
                    if (content is TextContent textContent)
                    {
                        responseContent += textContent.Text;
                        contentToReturn += textContent.Text;
                    }
                }

                // Convert the update to an OpenAI StreamingChatCompletionUpdate for backward compatibility
                var openAIUpdate = CreateStreamingUpdate(update);
                streamingCallback?.Invoke(openAIUpdate);
            }

            // Get the function calls from the detector
            var functionCalls = _functionCallDetector.GetCompleteFunctionCalls();
            
            // If there are function calls, process them
            if (functionCalls.Count > 0)
            {
                // Create an OpenAI-compatible representation of the function calls
                var openAIFunctionCalls = new List<StreamingChatToolCallUpdate>();
                foreach (var call in functionCalls)
                {
                    openAIFunctionCalls.Add(_functionCallDetector.ConvertToStreamingToolCallUpdate(call));
                }
                
                // Update our OpenAI-compatible function call context with the function calls
                var openAIMessages = _messages.ToOpenAIChatMessages();
                _functionCallContext = new FunctionCallContext(_functionFactory, openAIMessages);
                
                // Process the function calls using our existing context
                foreach (var call in openAIFunctionCalls)
                {
                    _functionCallContext.AppendToolCall(call);
                }
                
                // Try to call the functions
                var openAIMessageCallback = messageCallback != null 
                    ? new Action<IList<OpenAI.Chat.ChatMessage>>(msgs => messageCallback?.Invoke(msgs.ToExtensionsAIChatMessages())) 
                    : null;
                    
                if (_functionCallContext.TryCallFunctions(responseContent, functionCallCallback, openAIMessageCallback))
                {
                    // Get the updated messages
                    var updatedMessages = _functionCallContext.GetMessages();
                    
                    // Update our messages list with the OpenAI messages
                    _messages = updatedMessages.ToExtensionsAIChatMessages();
                    
                    // Continue processing
                    _functionCallContext.Clear();
                    continue;
                }
            }

            // Add the assistant message to the history
            _messages.Add(new ChatMessage(ChatRole.Assistant, new TextContent(responseContent)));
            
            // Call the message callback if provided
            if (messageCallback != null)
            {
                messageCallback(_messages);
            }

            return contentToReturn;
        }
    }

    /// <summary>
    /// Creates an OpenAI StreamingChatCompletionUpdate from a ChatResponseUpdate
    /// </summary>
    private StreamingChatCompletionUpdate CreateStreamingUpdate(ChatResponseUpdate update)
    {
        // Create a compatible StreamingChatCompletionUpdate
        var result = new StreamingChatCompletionUpdate
        {
            // Map the properties across
            FinishReason = MapFinishReason(update.FinishReason)
        };
        
        // Add content parts
        foreach (var content in update.Contents)
        {
            if (content is TextContent textContent)
            {
                result.ContentUpdate.Add(ChatMessageContentPart.CreateTextPart(textContent.Text));
            }
            // Add other content types as needed
        }
        
        return result;
    }
    
    /// <summary>
    /// Maps a Microsoft.Extensions.AI finish reason to an OpenAI finish reason
    /// </summary>
    private ChatFinishReason MapFinishReason(FinishReason? reason)
    {
        if (reason == null) return ChatFinishReason.Unspecified;
        
        return reason.Value switch
        {
            FinishReason.Stop => ChatFinishReason.Stopped,
            FinishReason.Length => ChatFinishReason.Length,
            FinishReason.ToolCalls => ChatFinishReason.FunctionCall,
            FinishReason.ContentFilter => ChatFinishReason.ContentFilter,
            _ => ChatFinishReason.Unspecified
        };
    }
    
    /// <summary>
    /// Converts a list of ChatMessage to a list of OpenAI ChatMessage
    /// </summary>
    private List<OpenAI.Chat.ChatMessage> ConvertToOpenAIChatMessages(IEnumerable<ChatMessage> messages)
    {
        return messages.Select(m => m.ToOpenAIChatMessage()).ToList();
    }
    
    /// <summary>
    /// Converts a list of OpenAI ChatMessage to a list of ChatMessage
    /// </summary>
    private List<ChatMessage> ConvertToExtensionsAIChatMessages(IEnumerable<OpenAI.Chat.ChatMessage> messages)
    {
        return messages.Select(m => m.ToExtensionsAIChatMessage()).ToList();
    }

    private readonly string _systemPrompt;
    private readonly List<string> _userMessageAdds = new();

    private readonly FunctionFactory _functionFactory;
    private readonly FunctionCallDetector _functionCallDetector;
    private FunctionCallContext _functionCallContext;
    private readonly ChatOptions _options;
    private readonly IChatClient _chatClient;
    private List<ChatMessage> _messages;
}