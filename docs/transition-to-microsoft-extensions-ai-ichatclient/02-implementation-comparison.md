# Comparison Between Current Implementation and Microsoft.Extensions.AI

## Message Models

### Current (OpenAI.Chat)
```csharp
// OpenAI SDK
var assistantMessage = new AssistantChatMessage(toolCalls);
var userMessage = new UserChatMessage("What is the weather?");
var systemMessage = new SystemChatMessage("You are a helpful assistant");
var toolMessage = new ToolChatMessage(toolCallId, result);
```

### Target (Microsoft.Extensions.AI)
```csharp
// Microsoft.Extensions.AI
var assistantMessage = new ChatMessage(ChatRole.Assistant, "Response content");
var userMessage = new ChatMessage(ChatRole.User, "What is the weather?");
var systemMessage = new ChatMessage(ChatRole.System, "You are a helpful assistant");
var toolMessage = new ChatMessage(ChatRole.Tool, new FunctionResultContent(callId, result));
```

## Function Calling

### Current (Our Implementation)
```csharp
// Our custom function calling implementation
var functionFactory = new FunctionFactory();
functionFactory.AddFunctions(typeof(MyFunctions));

var functionCallContext = new FunctionCallContext(functionFactory, messages);
var toolCalls = functionCallContext.TryCallFunctions(responseContent, callback, messageCallback);
```

### Target (Microsoft.Extensions.AI Approach - Not Adopting)
```csharp
// Microsoft.Extensions.AI function calling (for reference only)
IChatClient client = new OpenAIChatClient(chatClient)
    .AsBuilder()
    .UseFunctionInvocation()
    .Build();

ChatOptions options = new() { 
    Tools = [AIFunctionFactory.Create(GetCurrentWeather)] 
};

var response = client.GetResponseAsync("Should I wear a rain coat?", options);
```

### Our Target Approach (Keeping Our Implementation)
```csharp
// Our implementation adapted to IChatClient
var functionFactory = new FunctionFactory();
functionFactory.AddFunctions(typeof(MyFunctions));

var chatClient = ChatClientFactory.CreateChatClient();
var functionCallingChat = new FunctionCallingChat(chatClient, systemPrompt, functionFactory);

// Will maintain our function call processing
var response = await functionCallingChat.CompleteChatStreamingAsync(
    "Should I wear a rain coat?", 
    messageCallback, 
    streamingCallback, 
    functionCallCallback);
```

## Client Creation

### Current
```csharp
// Create an OpenAI ChatClient directly
var chatClient = new ChatClient(model, new ApiKeyCredential(apiKey), options);

// Create a client via our factory
var client = ChatClientFactory.CreateOpenAIChatClientWithApiKey();
```

### Target
```csharp
// Create an IChatClient
IChatClient chatClient = new OpenAIChatClient(new ChatClient(model, new ApiKeyCredential(apiKey), options));

// Create a client via our updated factory
IChatClient client = ChatClientFactory.CreateOpenAIChatClientWithApiKey();
```

## Streaming

### Current
```csharp
// Current streaming implementation
var response = _chatClient.CompleteChatStreamingAsync(_messages, _options);
await foreach (var update in response)
{
    _functionCallContext.CheckForUpdate(update);
    // Process content update...
}
```

### Target
```csharp
// Using IChatClient streaming
await foreach (var update in _chatClient.GetStreamingResponseAsync(_messages, _options))
{
    // We'll need to adapt the updates to check for function calls
    // Process content update...
}
```

## Key Differences

1. **Message Structure**:
   - OpenAI.Chat uses specific message classes for different roles
   - Microsoft.Extensions.AI uses a unified ChatMessage with role and content

2. **Content Handling**:
   - OpenAI.Chat uses ChatMessageContentPart for different content types
   - Microsoft.Extensions.AI uses AIContent subclasses for different content types

3. **Function Calling**:
   - Our implementation uses custom FunctionFactory and attributes
   - Microsoft.Extensions.AI uses AIFunction and AIFunctionFactory

4. **Response Handling**:
   - OpenAI.Chat provides StreamingChatCompletionUpdate
   - Microsoft.Extensions.AI provides ChatResponseUpdate