# FunctionCallingChat Transition Design

## Overview

The `FunctionCallingChat` class needs to be updated to work with `Microsoft.Extensions.AI.IChatClient` instead of `OpenAI.Chat.ChatClient`. This document outlines the changes required for this transition while maintaining our existing function calling capabilities.

## Current Implementation

Currently, `FunctionCallingChat` uses `OpenAI.Chat.ChatClient` to:
1. Manage chat messages
2. Submit requests to the model
3. Process streaming responses
4. Detect function calls in the streaming responses
5. Execute functions via `FunctionCallContext`
6. Add function results back to the message history

The key components include:
- `_chatClient`: An OpenAI.Chat.ChatClient instance
- `_functionFactory`: A FunctionFactory that creates and manages function definitions
- `_functionCallContext`: A FunctionCallContext that handles function call detection and execution
- `_messages`: A list of OpenAI.Chat.ChatMessage instances representing the conversation history
- `_options`: ChatCompletionOptions for configuring requests

## Target Implementation

The updated `FunctionCallingChat` will work with `IChatClient` but maintain our existing function calling approach. Key changes include:

1. Change `_chatClient` type from `OpenAI.Chat.ChatClient` to `Microsoft.Extensions.AI.IChatClient`
2. Convert between Microsoft.Extensions.AI's `ChatMessage` and OpenAI's `ChatMessage` types
3. Update message handling to work with `IChatClient`'s content model
4. Adapt the function calling detection to work with `ChatResponseUpdate` instead of `StreamingChatCompletionUpdate`
5. Maintain backward compatibility for the public API

## Implementation Details

### Constructor Update

```csharp
public FunctionCallingChat(IChatClient chatClient, string systemPrompt, FunctionFactory factory, int? maxOutputTokens = null)
{
    _systemPrompt = systemPrompt;
    _functionFactory = factory;
    _chatClient = chatClient;

    _messages = new List<ChatMessage>(); // Now Microsoft.Extensions.AI.ChatMessage
    _options = new ChatOptions(); // Now Microsoft.Extensions.AI.ChatOptions

    if (maxOutputTokens.HasValue) _options.MaxOutputTokens = maxOutputTokens.Value;

    // Add tools to options
    foreach (var tool in _functionFactory.GetChatTools())
    {
        // Need to convert OpenAI.Chat.ChatTool to Microsoft.Extensions.AI.AITool
        _options.Tools.Add(ConvertToAITool(tool));
    }

    _functionCallContext = new FunctionCallContext(_functionFactory, _messages);
    ClearChatHistory();
}
```

### Message Conversion

We need to convert between OpenAI's ChatMessage and Microsoft.Extensions.AI's ChatMessage:

```csharp
private static ChatMessage ConvertToExtensionsAIChatMessage(OpenAI.Chat.ChatMessage message)
{
    var role = message switch
    {
        SystemChatMessage => ChatRole.System,
        UserChatMessage => ChatRole.User,
        AssistantChatMessage => ChatRole.Assistant,
        ToolChatMessage => ChatRole.Tool,
        _ => new ChatRole(message.Role.ToString())
    };

    var chatMessage = new ChatMessage { Role = role };
    
    // Convert content parts to AIContent
    foreach (var part in message.Content)
    {
        if (part.Kind == ChatMessageContentPartKind.Text)
        {
            chatMessage.Contents.Add(new TextContent(part.Text));
        }
        // Convert other content types as needed
    }

    return chatMessage;
}

private static OpenAI.Chat.ChatMessage ConvertToOpenAIChatMessage(ChatMessage message)
{
    // Convert in the other direction
    var content = new List<ChatMessageContentPart>();
    foreach (var item in message.Contents)
    {
        if (item is TextContent textContent)
        {
            content.Add(ChatMessageContentPart.CreateTextPart(textContent.Text));
        }
        // Convert other content types
    }

    return message.Role == ChatRole.System ? new SystemChatMessage(content) :
           message.Role == ChatRole.User ? new UserChatMessage(content) :
           message.Role == ChatRole.Assistant ? new AssistantChatMessage(content) :
           message.Role == ChatRole.Tool ? new ToolChatMessage("", "") : // Would need to extract tool call ID and result
           new AssistantChatMessage(content); // Default case
}
```

### Function Call Detection Adapter

We need to adapt function call detection from `StreamingChatCompletionUpdate` to `ChatResponseUpdate`:

```csharp
public async Task<string> CompleteChatStreamingAsync(
    string userPrompt,
    Action<IList<ChatMessage>>? messageCallback = null,
    Action<ChatResponseUpdate>? streamingCallback = null,
    Action<string, string, string?>? functionCallCallback = null)
{
    var extensionsAIMessage = new ChatMessage(ChatRole.User, new TextContent(userPrompt));
    _messages.Add(extensionsAIMessage);
    
    if (messageCallback != null) messageCallback(_messages);

    var contentToReturn = string.Empty;
    while (true)
    {
        var responseContent = string.Empty;
        
        // Use IChatClient to get streaming response
        await foreach (var update in _chatClient.GetStreamingResponseAsync(_messages, _options))
        {
            // Need to adapt the update to check for function calls
            var functionCallFound = CheckForFunctionCall(update);
            
            foreach (var content in update.Contents)
            {
                if (content is TextContent textContent)
                {
                    responseContent += textContent.Text;
                    contentToReturn += textContent.Text;
                }
            }

            streamingCallback?.Invoke(update);
        }

        // Adapt function call detection and handling
        if (_functionCallContext.TryCallFunctions(responseContent, functionCallCallback, messages => messageCallback?.Invoke(messages)))
        {
            _functionCallContext.Clear();
            continue;
        }

        var assistantMessage = new ChatMessage(ChatRole.Assistant, new TextContent(responseContent));
        _messages.Add(assistantMessage);
        
        if (messageCallback != null) messageCallback(_messages);

        return contentToReturn;
    }
}

private bool CheckForFunctionCall(ChatResponseUpdate update)
{
    // Extract function call info from ChatResponseUpdate
    bool functionCallFound = false;
    
    foreach (var content in update.Contents)
    {
        if (content is FunctionCallContent functionCall)
        {
            // Add to function call context
            functionCallFound = true;
        }
    }
    
    return functionCallFound;
}
```

### Updated FunctionCallContext

`FunctionCallContext` needs to be updated to work with `Microsoft.Extensions.AI.ChatMessage` and `ChatResponseUpdate`:

```csharp
public class FunctionCallContext
{
    public FunctionCallContext(FunctionFactory functionFactory, IList<ChatMessage> messages)
    {
        _functionFactory = functionFactory;
        _messages = messages;
    }

    // Need to update this to work with ChatResponseUpdate
    public bool CheckForUpdate(ChatResponseUpdate update)
    {
        var updated = false;
        
        foreach (var content in update.Contents)
        {
            if (content is FunctionCallContent functionCall)
            {
                // Update function call builder
                _toolCallsBuilder.Append(functionCall);
                updated = true;
            }
        }
        
        return updated;
    }
    
    // Other methods need similar adaptations
}
```

## Backward Compatibility Considerations

To maintain backward compatibility:

1. Keep the same public API for `FunctionCallingChat`
2. Internally adapt between OpenAI and Microsoft.Extensions.AI types
3. Convert function call detection/handling to work with `ChatResponseUpdate`
4. Ensure message history is properly maintained with Microsoft.Extensions.AI message types