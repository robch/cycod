using Microsoft.Extensions.AI;
using System.Runtime.CompilerServices;

/// <summary>
/// A test chat client that returns predefined static responses.
/// Useful for testing scenarios where you need predictable AI responses.
/// </summary>
public class TestChatClient : IChatClient
{
    // Timing constants with environment variable overrides
    private const int DEFAULT_RESPONSE_DELAY_MS = 100;    // Fast by default
    private const int DEFAULT_WASTE_TIME_DELAY_MS = 3000; // Longer for time burning
    private const int DEFAULT_STREAMING_DELAY_MS = 50;
    private const int DEFAULT_STREAMING_CHUNK_SIZE = 3; // Words per chunk
    
    private readonly Dictionary<string, string> _responses;
    private readonly string _defaultResponse;
    private readonly string _modelId;
    private readonly int _responseDelayMs;
    private readonly int _wasteTimeDelayMs;
    private readonly int _streamingDelayMs;
    private readonly int _streamingChunkSize;

    public TestChatClient(Dictionary<string, string>? responses = null, string? defaultResponse = null, string? modelId = null)
    {
        _responses = responses ?? new Dictionary<string, string>();
        _defaultResponse = defaultResponse ?? "This is a test response from TestChatClient.";
        _modelId = modelId ?? "test-model";
        
        // Initialize timing settings with environment overrides
        _responseDelayMs = GetResponseDelay();
        _wasteTimeDelayMs = GetWasteTimeDelay();
        _streamingDelayMs = GetStreamingDelay();
        _streamingChunkSize = GetStreamingChunkSize();
        
        // Add some default responses for common scenarios
        if (!_responses.Any())
        {
            // Cycodmd request contains the word "title", so return a fake title
            _responses["title"] = "Test Conversation Title";
            // To burn time with configurable delay
            _responses["waste time"] = $"Waited {_wasteTimeDelayMs}ms (TEST_WASTE_TIME_DELAY_MS).";
        }
    }
    
    private static int GetResponseDelay() =>
        int.TryParse(Environment.GetEnvironmentVariable("TEST_RESPONSE_DELAY_MS"), out var delay) 
            ? delay : DEFAULT_RESPONSE_DELAY_MS;
    
    private static int GetWasteTimeDelay() =>
        int.TryParse(Environment.GetEnvironmentVariable("TEST_WASTE_TIME_DELAY_MS"), out var delay) 
            ? delay : DEFAULT_WASTE_TIME_DELAY_MS;
    
    private static int GetStreamingDelay() =>
        int.TryParse(Environment.GetEnvironmentVariable("TEST_STREAMING_DELAY_MS"), out var delay) 
            ? delay : DEFAULT_STREAMING_DELAY_MS;
            
    private static int GetStreamingChunkSize() =>
        int.TryParse(Environment.GetEnvironmentVariable("TEST_STREAMING_CHUNK_SIZE"), out var size) 
            ? size : DEFAULT_STREAMING_CHUNK_SIZE;

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
        var lastUserMessage = GetLastUserMessage(chatMessages);
        var isWasteTimeRequest = IsWasteTimeRequest(lastUserMessage);
        var delayMs = isWasteTimeRequest ? _wasteTimeDelayMs : _responseDelayMs;
        
        await Task.Delay(delayMs, cancellationToken);

        var response = GetResponseForMessages(chatMessages);
        
        // Create usage content with 0 tokens (since no real API call was made)
        var usageContent = new UsageContent(new UsageDetails 
        { 
            InputTokenCount = 0, 
            OutputTokenCount = 0 
        });
        
        return new ChatResponse([
            new ChatMessage(ChatRole.Assistant, response),
            new ChatMessage(ChatRole.Assistant, [usageContent])
        ]);
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> chatMessages, 
        ChatOptions? options = null, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var lastUserMessage = GetLastUserMessage(chatMessages);
        var isWasteTimeRequest = IsWasteTimeRequest(lastUserMessage);
        var delayMs = isWasteTimeRequest ? _wasteTimeDelayMs : _responseDelayMs;
        
        await Task.Delay(delayMs, cancellationToken);

        var response = GetResponseForMessages(chatMessages);
        
        // Split response into words for more realistic streaming
        var words = response.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        // Stream words in chunks
        for (int i = 0; i < words.Length; i += _streamingChunkSize)
        {
            var chunk = string.Join(" ", words.Skip(i).Take(_streamingChunkSize));
            if (i + _streamingChunkSize < words.Length) chunk += " "; // Add space except for last chunk
            
            yield return new ChatResponseUpdate
            {
                Role = ChatRole.Assistant,
                Contents = [new TextContent(chunk)]
            };
            
            await Task.Delay(_streamingDelayMs, cancellationToken);
        }

        // Yield usage content with 0 tokens (since no real API call was made)
        yield return new ChatResponseUpdate
        {
            Role = ChatRole.Assistant,
            Contents = [new UsageContent(new UsageDetails 
            { 
                InputTokenCount = 0, 
                OutputTokenCount = 0 
            })]
        };

        // Final update to indicate completion
        yield return new ChatResponseUpdate
        {
            Role = ChatRole.Assistant,
            FinishReason = ChatFinishReason.Stop
        };
    }

    private static string GetLastUserMessage(IEnumerable<ChatMessage> chatMessages) =>
        chatMessages
            .Where(m => m.Role == ChatRole.User)
            .LastOrDefault()?.Text?.ToLowerInvariant() ?? string.Empty;
    
    private static bool IsWasteTimeRequest(string userMessage) =>
        userMessage.Contains("waste time");

    private string GetResponseForMessages(IEnumerable<ChatMessage> chatMessages)
    {
        var lastUserMessage = GetLastUserMessage(chatMessages);

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