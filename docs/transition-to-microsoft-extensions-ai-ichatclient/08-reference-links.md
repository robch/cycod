# Reference Links for Microsoft.Extensions.AI.IChatClient Transition

This document provides links to important reference files and code sections related to the transition from OpenAI's ChatCompletions API to Microsoft.Extensions.AI.IChatClient.

## Microsoft.Extensions.AI Implementation

### Core Interfaces and Abstractions

- **[IChatClient Interface](c:/src/Microsoft.AI.Extensions/src/Libraries/Microsoft.Extensions.AI.Abstractions/ChatCompletion/IChatClient.cs)**: The core interface that our implementation will target
- **[ChatMessage Class](c:/src/Microsoft.AI.Extensions/src/Libraries/Microsoft.Extensions.AI.Abstractions/ChatCompletion/ChatMessage.cs)**: Represents messages in the Microsoft.Extensions.AI system
- **[ChatRole Class](c:/src/Microsoft.AI.Extensions/src/Libraries/Microsoft.Extensions.AI.Abstractions/ChatCompletion/ChatRole.cs)**: Defines roles for messages (System, User, Assistant, Tool)
- **[ChatOptions Class](c:/src/Microsoft.AI.Extensions/src/Libraries/Microsoft.Extensions.AI.Abstractions/ChatCompletion/ChatOptions.cs)**: Configuration options for chat requests
- **[ChatResponse Class](c:/src/Microsoft.AI.Extensions/src/Libraries/Microsoft.Extensions.AI.Abstractions/ChatCompletion/ChatResponse.cs)**: Container for chat responses
- **[ChatResponseUpdate Class](c:/src/Microsoft.AI.Extensions/src/Libraries/Microsoft.Extensions.AI.Abstractions/ChatCompletion/ChatResponseUpdate.cs)**: Used for streaming responses

### Content Types

- **[AIContent Class](c:/src/Microsoft.AI.Extensions/src/Libraries/Microsoft.Extensions.AI.Abstractions/Contents/AIContent.cs)**: Base class for different content types
- **[TextContent Class](c:/src/Microsoft.AI.Extensions/src/Libraries/Microsoft.Extensions.AI.Abstractions/Contents/TextContent.cs)**: For text content
- **[FunctionCallContent Class](c:/src/Microsoft.AI.Extensions/src/Libraries/Microsoft.Extensions.AI.Abstractions/Contents/FunctionCallContent.cs)**: Represents function calls
- **[FunctionResultContent Class](c:/src/Microsoft.AI.Extensions/src/Libraries/Microsoft.Extensions.AI.Abstractions/Contents/FunctionResultContent.cs)**: Represents function results

### Function Calling

- **[AIFunction Class](c:/src/Microsoft.AI.Extensions/src/Libraries/Microsoft.Extensions.AI.Abstractions/Functions/AIFunction.cs)**: Represents a function with its metadata
- **[AITool Class](c:/src/Microsoft.AI.Extensions/src/Libraries/Microsoft.Extensions.AI.Abstractions/AITool.cs)**: Base class for tools (including functions)

### OpenAI Implementation

- **[OpenAIChatClient Class](c:/src/Microsoft.AI.Extensions/src/Libraries/Microsoft.Extensions.AI.OpenAI/OpenAIChatClient.cs)**: Adapter that converts OpenAI clients to IChatClient
- **[OpenAIClientExtensions Class](c:/src/Microsoft.AI.Extensions/src/Libraries/Microsoft.Extensions.AI.OpenAI/OpenAIClientExtensions.cs)**: Extension methods including AsIChatClient()

### Middleware Components

- **[ChatClientBuilder Class](c:/src/Microsoft.AI.Extensions/src/Libraries/Microsoft.Extensions.AI/ChatCompletion/ChatClientBuilder.cs)**: Builder pattern for creating chat client pipelines
- **[LoggingChatClient Class](c:/src/Microsoft.AI.Extensions/src/Libraries/Microsoft.Extensions.AI/ChatCompletion/LoggingChatClient.cs)**: Middleware for logging
- **[DistributedCachingChatClient Class](c:/src/Microsoft.AI.Extensions/src/Libraries/Microsoft.Extensions.AI/ChatCompletion/DistributedCachingChatClient.cs)**: Middleware for caching
- **[FunctionInvokingChatClient Class](c:/src/Microsoft.AI.Extensions/src/Libraries/Microsoft.Extensions.AI/ChatCompletion/FunctionInvokingChatClient.cs)**: Middleware for function invocation

## Current chatx Implementation

### Chat Client Implementation

- **[ChatClientFactory Class](./src/ChatClient/ChatClientFactory.cs)**: Creates OpenAI.Chat.ChatClient instances
- **[FunctionCallingChat Class](./src/ChatClient/FunctionCallingChat.cs)**: Uses ChatClient for interactions with the model

### Function Calling Implementation

- **[FunctionFactory Class](./src/FunctionCalling/FunctionFactory.cs)**: Discovers and registers functions
- **[FunctionCallContext Class](./src/FunctionCalling/FunctionCallContext.cs)**: Manages function calling interactions
- **[HelperFunctionDescriptionAttribute Class](./src/FunctionCalling/HelperFunctionDescriptionAttribute.cs)**: Provides metadata for functions

## Key Conversion Points

The transition requires updating several key components:

1. **ChatClientFactory Methods**:
   - Change return types from `ChatClient` to `IChatClient`
   - Use the `AsIChatClient()` extension method
   - Example: `return chatClient.AsIChatClient();`

2. **FunctionCallingChat Class**:
   - Update to accept `IChatClient` instead of `ChatClient`
   - Convert between Microsoft.Extensions.AI and OpenAI message formats
   - Adapt function call detection to work with `ChatResponseUpdate`

3. **Message Conversion**:
   - Create utility methods to convert between OpenAI's `ChatMessage` and Microsoft.Extensions.AI's `ChatMessage`
   - Handle role conversions (System, User, Assistant, Tool)
   - Convert content parts correctly

4. **Function Call Handling**:
   - Create a detector to identify function calls in `ChatResponseUpdate`
   - Convert between Microsoft.Extensions.AI's function call format and our existing format
   - Maintain backward compatibility with our function calling approach

## Implementation Examples

The Microsoft.Extensions.AI.OpenAI implementation provides several useful examples of how to convert between the systems:

- **Message Conversion**: See `ToOpenAIChatMessages()` and `ToAIContent()` methods in OpenAIChatClient
- **Options Conversion**: See `ToOpenAIOptions()` method in OpenAIChatClient
- **Function Call Handling**: See `FromOpenAIStreamingChatCompletionAsync()` method in OpenAIChatClient

## Additional Resources

- [Microsoft.Extensions.AI Documentation](https://learn.microsoft.com/dotnet/ai/)
- [OpenAI SDK on GitHub](https://github.com/openai/openai-dotnet)