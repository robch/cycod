using Azure;
using Azure.AI.OpenAI;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using Azure.Identity;

public static class ChatClientFactory
{
    public static ChatClient CreateAzureOpenAIChatClient()
    {
        string deployment = EnvironmentHelpers.FindEnvVar("AZURE_OPENAI_CHAT_DEPLOYMENT") ?? throw new InvalidOperationException("AZURE_OPENAI_CHAT_DEPLOYMENT is not set.");
        string endpoint = EnvironmentHelpers.FindEnvVar("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
        string apiKey = EnvironmentHelpers.FindEnvVar("AZURE_OPENAI_API_KEY") ?? throw new InvalidOperationException("AZURE_OPENAI_API_KEY is not set.");

        var client = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
        return client.GetChatClient(deployment);
    }

    public static ChatClient CreateOpenAIChatClient()
    {
        var model = EnvironmentHelpers.FindEnvVar("OPENAI_CHAT_MODEL_NAME") ?? "gpt-4o";
        var apiKey = EnvironmentHelpers.FindEnvVar("OPENAI_API_KEY") ?? throw new InvalidOperationException("OPENAI_API_KEY is not set.");

        return new ChatClient(model, new ApiKeyCredential(apiKey));
    }

    public static ChatClient CreateCopilotChatClient()
    {
        string model = EnvironmentHelpers.FindEnvVar("COPILOT_MODEL_NAME") ?? "claude-3.7-sonnet";
        string endpoint = EnvironmentHelpers.FindEnvVar("COPILOT_API_ENDPOINT") ?? "https://api.githubcopilot.com";
        string integrationId = EnvironmentHelpers.FindEnvVar("COPILOT_INTEGRATION_ID") ?? throw new InvalidOperationException("COPILOT_INTEGRATION_ID is not set.");
        string hmacKey = EnvironmentHelpers.FindEnvVar("COPILOT_HMAC_KEY") ?? throw new InvalidOperationException("COPILOT_HMAC_KEY is not set.");

        return new CopilotOpenAIChatClient(model, hmacKey, integrationId, endpoint);
    }

    public static ChatClient CreateChatClientFromEnv()
    {
        ConsoleHelpers.WriteDebugLine("Creating chat client from environment variables...");
        if (!string.IsNullOrEmpty(EnvironmentHelpers.FindEnvVar("COPILOT_HMAC_KEY")))
        {
            return CreateCopilotChatClient();
        }

        if (!string.IsNullOrEmpty(EnvironmentHelpers.FindEnvVar("AZURE_OPENAI_API_KEY")))
        {
            return CreateAzureOpenAIChatClient();
        }

        if (!string.IsNullOrEmpty(EnvironmentHelpers.FindEnvVar("OPENAI_API_KEY")))
        {
            return CreateOpenAIChatClient();
        }

        var message =
            string.Join('\n',
                @"No valid environment variables found.

                To use OpenAI, please set:
                - OPENAI_API_KEY
                - OPENAI_CHAT_MODEL_NAME (optional)

                To use Azure OpenAI, please set:
                - AZURE_OPENAI_API_KEY
                - AZURE_OPENAI_ENDPOINT
                - AZURE_OPENAI_CHAT_DEPLOYMENT

                To use GitHub Copilot, please set:
                - COPILOT_HMAC_KEY
                - COPILOT_INTEGRATION_ID
                - COPILOT_API_ENDPOINT (optional)
                - COPILOT_MODEL_NAME (optional)"
            .Split(new[] { '\n' })
            .Select(line => line.Trim()));

        throw new InvalidOperationException(message);
    }
}
