using Azure;
using Azure.AI.OpenAI;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.ClientModel.Primitives;

public static class ChatClientFactory
{
    public static ChatClient CreateAzureOpenAIChatClient()
    {
        var options = new AzureOpenAIClientOptions();
        options.AddPolicy(new LogTrafficEventPolicy(), PipelinePosition.PerCall);
        options.RetryPolicy = new ClientRetryPolicy(maxRetries: 10);

        string deployment = EnvironmentHelpers.FindEnvVar("AZURE_OPENAI_CHAT_DEPLOYMENT") ?? throw new InvalidOperationException("AZURE_OPENAI_CHAT_DEPLOYMENT is not set.");
        string endpoint = EnvironmentHelpers.FindEnvVar("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
        string apiKey = EnvironmentHelpers.FindEnvVar("AZURE_OPENAI_API_KEY") ?? throw new InvalidOperationException("AZURE_OPENAI_API_KEY is not set.");

        var client = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey), options);
        return client.GetChatClient(deployment);
    }

    public static ChatClient CreateOpenAIChatClient()
    {
        var options = new OpenAIClientOptions();
        options.AddPolicy(new LogTrafficEventPolicy(), PipelinePosition.PerCall);
        options.RetryPolicy = new ClientRetryPolicy(maxRetries: 10);

        var model = EnvironmentHelpers.FindEnvVar("OPENAI_CHAT_MODEL_NAME") ?? "gpt-4o";
        var apiKey = EnvironmentHelpers.FindEnvVar("OPENAI_API_KEY") ?? throw new InvalidOperationException("OPENAI_API_KEY is not set.");

        return new ChatClient(model, new ApiKeyCredential(apiKey), options);
    }

    public static ChatClient CreateCopilotChatClientWithToken()
    {
        var model = EnvironmentHelpers.FindEnvVar("COPILOT_MODEL_NAME") ?? "claude-3.7-sonnet";
        var endpoint = EnvironmentHelpers.FindEnvVar("COPILOT_API_ENDPOINT") ?? "https://api.githubcopilot.com";
        var integrationId = EnvironmentHelpers.FindEnvVar("COPILOT_INTEGRATION_ID") ?? throw new InvalidOperationException("COPILOT_INTEGRATION_ID is not set.");
        var githubToken = EnvironmentHelpers.FindEnvVar("GITHUB_TOKEN") ?? throw new InvalidOperationException("GITHUB_TOKEN is not set.");

        var options = new OpenAIClientOptions();
        options.AddPolicy(new LogTrafficEventPolicy(), PipelinePosition.PerCall);
        options.RetryPolicy = new ClientRetryPolicy(maxRetries: 10);
        options.Endpoint = new Uri(endpoint);
        
        // Use GitHub token for authorization
        options.AddPolicy(new CustomHeaderPolicy("Copilot-Integration-Id", integrationId), PipelinePosition.BeforeTransport);
        options.AddPolicy(new CustomHeaderPolicy("Authorization", $"Bearer {githubToken}"), PipelinePosition.BeforeTransport);

        return new ChatClient(model, new ApiKeyCredential(" "), options);
    }

    public static ChatClient CreateCopilotChatClient()
    {
        var model = EnvironmentHelpers.FindEnvVar("COPILOT_MODEL_NAME") ?? "claude-3.7-sonnet";
        var endpoint = EnvironmentHelpers.FindEnvVar("COPILOT_API_ENDPOINT") ?? "https://api.githubcopilot.com";
        var integrationId = EnvironmentHelpers.FindEnvVar("COPILOT_INTEGRATION_ID") ?? throw new InvalidOperationException("COPILOT_INTEGRATION_ID is not set.");
        var hmacKey = EnvironmentHelpers.FindEnvVar("COPILOT_HMAC_KEY") ?? throw new InvalidOperationException("COPILOT_HMAC_KEY is not set.");

        var options = new OpenAIClientOptions();
        options.AddPolicy(new LogTrafficEventPolicy(), PipelinePosition.PerCall);
        options.RetryPolicy = new ClientRetryPolicy(maxRetries: 10);
        options.Endpoint = new Uri(endpoint);

        var hmacValue = HMACHelper.Encode(hmacKey);
        options.AddPolicy(new CustomHeaderPolicy("Request-HMAC", hmacValue), PipelinePosition.BeforeTransport);
        options.AddPolicy(new CustomHeaderPolicy("Copilot-Integration-Id", integrationId), PipelinePosition.BeforeTransport);
        options.AddPolicy(new CustomHeaderPolicy("Authorization", ""), PipelinePosition.BeforeTransport);
        options.AddPolicy(new LogTrafficEventPolicy(), PipelinePosition.PerCall);

        return new ChatClient(model, new ApiKeyCredential(" "), options);
    }

    public static ChatClient CreateChatClientFromEnv()
    {
        ConsoleHelpers.WriteDebugLine("Creating chat client from environment variables...");
        
        if (!string.IsNullOrEmpty(EnvironmentHelpers.FindEnvVar("GITHUB_TOKEN")))
        {
            ConsoleHelpers.WriteDebugLine("Using GitHub token for Copilot authentication");
            return CreateCopilotChatClientWithToken();
        }
        
        if (!string.IsNullOrEmpty(EnvironmentHelpers.FindEnvVar("COPILOT_HMAC_KEY")))
        {
            ConsoleHelpers.WriteDebugLine("Using HMAC for Copilot authentication");
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

                To use GitHub Copilot with token authentication, please set:
                - GITHUB_TOKEN
                - COPILOT_INTEGRATION_ID
                - COPILOT_API_ENDPOINT (optional)
                - COPILOT_MODEL_NAME (optional)

                To use GitHub Copilot with HMAC authentication, please set:
                - COPILOT_HMAC_KEY
                - COPILOT_INTEGRATION_ID
                - COPILOT_API_ENDPOINT (optional)
                - COPILOT_MODEL_NAME (optional)"
            .Split(new[] { '\n' })
            .Select(line => line.Trim()));

        throw new InvalidOperationException(message);
    }
}
