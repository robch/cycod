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
            // Cycodmd request contains the word "title", so return a fake title
            _responses["title"] = "Test Conversation Title";
            // To burn time, return a long-winded story
            _responses["waste time"] = "There once was a man named Clive who, in the midst of a perfectly ordinary Thursday afternoon, realized he had run out of the specific brand of oat milk that, in his estimation, struck the perfect balance between viscosity and moral superiority. Not just any oat milk would do—no, Clive’s preferred brand was Thistle & Loom, the one with the faintly pretentious minimalist label that whispered rather than shouted its sustainability credentials. He could have ordered it online, of course, but the last time he did that, the delivery driver left it behind a hedge, and the local raccoons staged what could only be described as a crime scene of beige liquid and shredded carton.";
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