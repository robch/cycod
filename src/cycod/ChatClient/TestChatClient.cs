using Microsoft.Extensions.AI;
using System.Runtime.CompilerServices;

/// <summary>
/// A test chat client that returns predefined static responses.
/// Useful for testing scenarios where you need predictable AI responses.
/// </summary>
public class TestChatClient : IChatClient
{
    private readonly Dictionary<string, string> _responses;
    private readonly string _defaultResponse;
    private readonly string _modelId;

    public TestChatClient(Dictionary<string, string>? responses = null, string? defaultResponse = null, string? modelId = null)
    {
        _responses = responses ?? new Dictionary<string, string>();
        _defaultResponse = defaultResponse ?? "This is a test response from TestChatClient.";
        _modelId = modelId ?? "test-model";
        
        // Add some default responses for common scenarios
        if (!_responses.Any())
        {
            _responses["title"] = "Test Conversation Title";
            _responses["summarize"] = "Test Summary: This is a test summary of the conversation.";
            _responses["hello"] = "Hello! This is a test response.";
            _responses["test"] = "Test successful!";
            _responses["generate a title"] = "AI Implementation Discussion";
            _responses["create a title"] = "Custom Chat Title";
            _responses["what should this conversation be called"] = "Dynamic Conversation Name";
        }
    }

    public ChatClientMetadata Metadata => new("test-provider", new Uri("https://test.example.com"), _modelId);

    public TService? GetService<TService>(object? key = null) where TService : class
    {
        return null;
    }

    public object? GetService(Type serviceType, object? key = null)
    {
        return null;
    }

    public async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> chatMessages, 
        ChatOptions? options = null, 
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(10, cancellationToken); // Simulate slight delay

        var response = GetResponseForMessages(chatMessages);
        
        return new ChatResponse(new ChatMessage(ChatRole.Assistant, response));
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> chatMessages, 
        ChatOptions? options = null, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.Delay(10, cancellationToken); // Simulate slight delay

        var response = GetResponseForMessages(chatMessages);
        
        // Simulate streaming by yielding characters one by one
        for (int i = 0; i < response.Length; i++)
        {
            yield return new ChatResponseUpdate
            {
                Role = ChatRole.Assistant,
                Contents = [new TextContent(response[i].ToString())]
            };
            
            await Task.Delay(1, cancellationToken); // Small delay between characters
        }

        // Final update to indicate completion
        yield return new ChatResponseUpdate
        {
            Role = ChatRole.Assistant,
            FinishReason = ChatFinishReason.Stop
        };
    }

    private string GetResponseForMessages(IEnumerable<ChatMessage> chatMessages)
    {
        var lastUserMessage = chatMessages
            .Where(m => m.Role == ChatRole.User)
            .LastOrDefault()?.Text?.ToLowerInvariant() ?? string.Empty;

        // Look for keyword matches in the user message
        foreach (var kvp in _responses)
        {
            if (lastUserMessage.Contains(kvp.Key.ToLowerInvariant()))
            {
                return kvp.Value;
            }
        }

        return _defaultResponse;
    }

    public void Dispose()
    {
        // Nothing to dispose in test implementation
    }
}