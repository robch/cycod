# Step 4: Update ChatClientFactory

## Plan
Update the `ChatClientFactory` class to return `Microsoft.Extensions.AI.IChatClient` instances instead of `OpenAI.Chat.ChatClient` instances. This involves:

1. Changing all return types from `ChatClient` to `IChatClient`
2. Using the `AsIChatClient()` extension method to convert OpenAI clients
3. Maintaining custom headers and authentication methods for different providers
4. Adding optional methods for creating clients with additional middleware capabilities

## Implementation
Updated the ChatClientFactory.cs file with the following changes:

1. Changed all method signatures to return `IChatClient` instead of `ChatClient`:
   - `CreateAzureOpenAIChatClientWithApiKey()`
   - `CreateOpenAIChatClientWithApiKey()`
   - `CreateCopilotChatClientWithGitHubToken()`
   - `CreateCopilotChatClientWithHmacKey()`
   - `CreateChatClient()`
   - `TryCreateChatClientFromPreferredProvider()`
   - `TryCreateChatClientFromEnv()`

2. Added the `AsIChatClient()` extension method from Microsoft.Extensions.AI.OpenAI to convert ChatClient instances to IChatClient instances.

3. Added new methods for creating clients with additional capabilities:
   - `CreateChatClientWithLogging()` - Creates a client with logging middleware
   - `CreateChatClientWithCache()` - Creates a client with distributed cache middleware

4. Added necessary using statements:
   - `using Microsoft.Extensions.AI;`
   - `using Microsoft.Extensions.AI.OpenAI;`

✅ ChatClientFactory has been successfully updated to return IChatClient instances.