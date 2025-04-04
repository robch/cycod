# Step-by-Step Transition Guide

This guide outlines the specific steps to transition the chatx codebase from using OpenAI's ChatCompletions API directly to using Microsoft.Extensions.AI's IChatClient interface.

## Prerequisites

1. Add the required NuGet packages:
   ```
   dotnet add package Microsoft.Extensions.AI.Abstractions
   dotnet add package Microsoft.Extensions.AI
   dotnet add package Microsoft.Extensions.AI.OpenAI
   ```

## Step 1: Create Message Conversion Utilities

1. Create a new file `MessageConversionExtensions.cs` with utility methods for converting between OpenAI.Chat.ChatMessage and Microsoft.Extensions.AI.ChatMessage.
2. Implement conversion methods for all message types (System, User, Assistant, Tool).
3. Add methods for converting between ChatCompletionOptions and ChatOptions.

## Step 2: Update ChatClientFactory

1. Change all return types from `OpenAI.Chat.ChatClient` to `Microsoft.Extensions.AI.IChatClient`.
2. For each client creation method, use the AsIChatClient() extension method to convert the OpenAI client.
3. Update InitOpenAIClientOptions() and InitAzureOpenAIClientOptions() if needed.
4. Add optional methods for creating clients with additional capabilities (logging, caching).

## Step 3: Create Function Call Adapter

1. Create a new `FunctionCallDetector` class that can detect and extract function calls from ChatResponseUpdate instances.
2. Implement methods to convert between Microsoft.Extensions.AI function calls and OpenAI tool calls.
3. Add support for accumulating partial function calls across multiple streaming updates.

## Step 4: Update FunctionCallingChat

1. Change the _chatClient field type from OpenAI.Chat.ChatClient to Microsoft.Extensions.AI.IChatClient.
2. Update the constructor to handle IChatClient and convert tools for the ChatOptions.
3. Update the _messages field to use Microsoft.Extensions.AI.ChatMessage.
4. Adapt the CompleteChatStreamingAsync method to use IChatClient.GetStreamingResponseAsync.
5. Modify function call detection to work with ChatResponseUpdate instead of StreamingChatCompletionUpdate.
6. Update the history management to handle both message formats as needed.

## Step 5: Testing

1. Create test cases for each provider:
   - OpenAI
   - Azure OpenAI
   - GitHub Copilot with token
   - GitHub Copilot with HMAC
2. Test function calling with simple and complex functions.
3. Verify that streaming responses work correctly.
4. Ensure message history is properly maintained.

## Detailed Implementation Steps

### 1. Create Message Conversion Extension Methods

```csharp
// In MessageConversionExtensions.cs
public static ChatMessage ToExtensionsAIChatMessage(this OpenAI.Chat.ChatMessage message)
{
    // Implementation details in sample code
}

public static OpenAI.Chat.ChatMessage ToOpenAIChatMessage(this ChatMessage message)
{
    // Implementation details in sample code
}
```

### 2. Update ChatClientFactory Method Signatures and Implementations

```csharp
// Change this:
public static ChatClient CreateOpenAIChatClientWithApiKey()

// To this:
public static IChatClient CreateOpenAIChatClientWithApiKey()
{
    // Same implementation but with .AsIChatClient() at the end
    var chatClient = new ChatClient(model, new ApiKeyCredential(apiKey), InitOpenAIClientOptions());
    return chatClient.AsIChatClient();
}
```

### 3. Update FunctionCallingChat Constructor

```csharp
// Change this:
public FunctionCallingChat(ChatClient openAIClient, string systemPrompt, FunctionFactory factory, int? maxOutputTokens = null)

// To this:
public FunctionCallingChat(IChatClient chatClient, string systemPrompt, FunctionFactory factory, int? maxOutputTokens = null)
{
    // Similar implementation but with adapters for message formats and tools
}
```

### 4. Update CompleteChatStreamingAsync Method

```csharp
public async Task<string> CompleteChatStreamingAsync(
    string userPrompt,
    Action<IList<ChatMessage>>? messageCallback = null,
    Action<StreamingChatCompletionUpdate>? streamingCallback = null,
    Action<string, string, string?>? functionCallCallback = null)
{
    // Add the user message
    _messages.Add(new ChatMessage(ChatRole.User, new TextContent(userPrompt)));
    
    // Implementation details in sample code
}
```

## Potential Issues and Solutions

1. **Message Format Differences**:
   - OpenAI uses separate classes for different message roles
   - Microsoft.Extensions.AI uses a single ChatMessage class with role property
   - Solution: Use the conversion extensions to map between formats

2. **Function Calling Differences**:
   - OpenAI has ToolCalls property on AssistantChatMessage
   - Microsoft.Extensions.AI uses FunctionCallContent in message contents
   - Solution: Create adapters to detect and convert function calls

3. **Streaming Response Format**:
   - OpenAI has StreamingChatCompletionUpdate
   - Microsoft.Extensions.AI has ChatResponseUpdate
   - Solution: Create adapter to convert between update formats

4. **ChatOptions vs ChatCompletionOptions**:
   - Different property names and structure
   - Solution: Create conversion methods for options

## Final Verification

After completing the transition:

1. Verify all API providers still work
2. Check that function calling works as expected
3. Ensure message history is properly maintained
4. Confirm streaming responses work correctly