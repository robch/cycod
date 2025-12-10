using System;
using System.ClientModel;
using System.ClientModel.Primitives;

using Amazon;
using Amazon.BedrockRuntime;
using Anthropic;
using Anthropic.Foundry;
using Azure;
using Azure.AI.OpenAI;
using GeminiDotnet;
using GeminiDotnet.Extensions.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;

public static class ChatClientFactory
{
    public static IChatClient? CreateAnthropicChatClientWithApiKey(out ChatOptions? options)
    {
        var model = EnvironmentHelpers.FindEnvVar("ANTHROPIC_MODEL_NAME") ?? "claude-sonnet-4-20250514";
        var apiKey = EnvironmentHelpers.FindEnvVar("ANTHROPIC_API_KEY") ?? throw new EnvVarSettingException("ANTHROPIC_API_KEY is not set.");
        var endpoint = EnvironmentHelpers.FindEnvVar("ANTHROPIC_ENDPOINT");

        // Create HttpClient with traffic logging
        var httpClient = new HttpClient(new LogTrafficHttpMessageHandler());

        // Create standard Anthropic client
        var client = !string.IsNullOrEmpty(endpoint)
            ? new AnthropicClient() { APIKey = apiKey, BaseUrl = new Uri(endpoint), HttpClient = httpClient }
            : new AnthropicClient() { APIKey = apiKey, HttpClient = httpClient };
        
        var chatClient = client.AsIChatClient();
        ConsoleHelpers.WriteDebugLine($"Using Anthropic endpoint: {endpoint ?? "default (api.anthropic.com)"}");
        
        options = new ChatOptions
        {
            ModelId = model,
            ToolMode = ChatToolMode.Auto,
            MaxOutputTokens = 4000
        };

        ConsoleHelpers.WriteDebugLine("Using Anthropic API key for authentication");
        return chatClient;
    }

    public static IChatClient? CreateAzureAnthropicChatClientWithApiKey(out ChatOptions? options)
    {
        var model = EnvironmentHelpers.FindEnvVar("AZURE_ANTHROPIC_MODEL_NAME") ?? "claude-sonnet-4-5";
        var apiKey = EnvironmentHelpers.FindEnvVar("AZURE_ANTHROPIC_API_KEY") ?? throw new EnvVarSettingException("AZURE_ANTHROPIC_API_KEY is not set.");
        var endpoint = EnvironmentHelpers.FindEnvVar("AZURE_ANTHROPIC_ENDPOINT") ?? throw new EnvVarSettingException("AZURE_ANTHROPIC_ENDPOINT is not set.");

        // Create HttpClient with traffic logging
        var httpClient = new HttpClient(new LogTrafficHttpMessageHandler());

        // Extract resource name from Azure endpoint (e.g., "my-resource" from "https://my-resource.services.ai.azure.com/anthropic")
        var uri = new Uri(endpoint);
        var resourceName = uri.Host.Split('.')[0];
        
        // Create Azure Foundry credentials and client
        var credentials = new AnthropicFoundryApiKeyCredentials(apiKey, resourceName);
        var foundryClient = new AnthropicFoundryClient(credentials) { HttpClient = httpClient };
        var chatClient = foundryClient.AsIChatClient();
        
        ConsoleHelpers.WriteDebugLine($"Using Azure Anthropic endpoint: {endpoint}");
        ConsoleHelpers.WriteDebugLine($"Azure resource name: {resourceName}");
        
        options = new ChatOptions
        {
            ModelId = model,
            ToolMode = ChatToolMode.Auto,
            MaxOutputTokens = 4000
        };

        ConsoleHelpers.WriteDebugLine("Using Azure Anthropic API key for authentication");
        return chatClient;
    }

    public static IChatClient CreateAzureOpenAIChatClientWithApiKey()
    {
        var deployment = EnvironmentHelpers.FindEnvVar("AZURE_OPENAI_CHAT_DEPLOYMENT") ?? throw new EnvVarSettingException("AZURE_OPENAI_CHAT_DEPLOYMENT is not set.");
        var endpoint = EnvironmentHelpers.FindEnvVar("AZURE_OPENAI_ENDPOINT") ?? throw new EnvVarSettingException("AZURE_OPENAI_ENDPOINT is not set.");
        var apiKey = EnvironmentHelpers.FindEnvVar("AZURE_OPENAI_API_KEY") ?? throw new EnvVarSettingException("AZURE_OPENAI_API_KEY is not set.");

        var client = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey), InitAzureOpenAIClientOptions());
        var chatClient = client.GetChatClient(deployment);

        ConsoleHelpers.WriteDebugLine("Using Azure OpenAI API key for authentication");
        return chatClient.AsIChatClient();
    }

    public static IChatClient CreateOpenAIChatClientWithApiKey()
    {
        var model = EnvironmentHelpers.FindEnvVar("OPENAI_CHAT_MODEL_NAME") ?? "gpt-4o";
        var apiKey = EnvironmentHelpers.FindEnvVar("OPENAI_API_KEY") ?? throw new EnvVarSettingException("OPENAI_API_KEY is not set.");
        var endpoint = EnvironmentHelpers.FindEnvVar("OPENAI_ENDPOINT");

        var chatClient = new ChatClient(model, new ApiKeyCredential(apiKey), InitOpenAIClientOptions(endpoint));

        ConsoleHelpers.WriteDebugLine("Using OpenAI API key for authentication");
        return chatClient.AsIChatClient();
    }

    public static IChatClient? CreateGeminiChatClient(out ChatOptions? options)
    {
        var apiKey = EnvironmentHelpers.FindEnvVar("GOOGLE_GEMINI_API_KEY") ?? throw new EnvVarSettingException("GOOGLE_GEMINI_API_KEY is not set.");
        var modelId = EnvironmentHelpers.FindEnvVar("GOOGLE_GEMINI_MODEL_ID") ?? "gemini-2.5-flash-preview-04-17";

        var clientOptions = new GeminiClientOptions { ApiKey = apiKey, ApiVersion = GeminiApiVersions.V1Beta };
        var client = new GeminiChatClient(clientOptions);
        options = new ChatOptions
        {
            ModelId = modelId
        };
        
        ConsoleHelpers.WriteDebugLine("Using Google Gemini API credentials for authentication");
        return client;
    }
    
    public static IChatClient CreateGrokChatClientWithApiKey()
    {
        var model = EnvironmentHelpers.FindEnvVar("GROK_MODEL_NAME") ?? "grok-3";
        var apiKey = EnvironmentHelpers.FindEnvVar("GROK_API_KEY") ?? throw new EnvVarSettingException("GROK_API_KEY is not set.");
        var endpoint = EnvironmentHelpers.FindEnvVar("GROK_ENDPOINT") ?? "https://api.x.ai/v1";

        var chatClient = new ChatClient(model, new ApiKeyCredential(apiKey), InitOpenAIClientOptions(endpoint));

        ConsoleHelpers.WriteDebugLine("Using Grok API key for authentication");
        return chatClient.AsIChatClient();
    }
    
    public static IChatClient? CreateAWSBedrockChatClient(out ChatOptions? options)
    {
        var accessKey = EnvironmentHelpers.FindEnvVar("AWS_BEDROCK_ACCESS_KEY") ?? throw new EnvVarSettingException("AWS_BEDROCK_ACCESS_KEY is not set.");
        var secretKey = EnvironmentHelpers.FindEnvVar("AWS_BEDROCK_SECRET_KEY") ?? throw new EnvVarSettingException("AWS_BEDROCK_SECRET_KEY is not set.");
        var region = EnvironmentHelpers.FindEnvVar("AWS_BEDROCK_REGION") ?? "us-east-1";
        var modelId = EnvironmentHelpers.FindEnvVar("AWS_BEDROCK_MODEL_ID") ?? "anthropic.claude-3-7-sonnet-20250219-v1:0";

        var regionEndpoint = RegionEndpoint.GetBySystemName(region);
        var runtime = new AmazonBedrockRuntimeClient(accessKey, secretKey, regionEndpoint);
        var chatClient = runtime.AsIChatClient();
        
        options = new ChatOptions
        {
            ModelId = modelId,
            ToolMode = ChatToolMode.Auto,
            MaxOutputTokens = 4000
        };

        ConsoleHelpers.WriteDebugLine("Using AWS Bedrock API credentials for authentication");
        return chatClient;
    }

    public static IChatClient CreateCopilotChatClientWithGitHubToken()
    {
        var model = EnvironmentHelpers.FindEnvVar("COPILOT_MODEL_NAME") ?? "claude-sonnet-4";
        var endpoint = EnvironmentHelpers.FindEnvVar("COPILOT_API_ENDPOINT") ?? "https://api.githubcopilot.com";
        var githubToken = EnvironmentHelpers.FindEnvVar("GITHUB_TOKEN") ?? throw new EnvVarSettingException("GITHUB_TOKEN is not set. Run 'cycod github login' to authenticate with GitHub Copilot.");
        var integrationId = EnvironmentHelpers.FindEnvVar("COPILOT_INTEGRATION_ID") ?? string.Empty;
        var editorVersion = EnvironmentHelpers.FindEnvVar("COPILOT_EDITOR_VERSION") ?? "vscode/1.80.1";

        // Get the Copilot token using the GitHub token
        var helper = new GitHubCopilotHelper();
        var tokenDetails = helper.GetCopilotTokenDetailsSync(githubToken);

        if (string.IsNullOrEmpty(tokenDetails.token))
        {
            throw new EnvVarSettingException("Failed to get a valid Copilot token from GitHub. Please run 'cycod github login' to authenticate.");
        }

        // Create options with the initial auth header
        var options = InitOpenAIClientOptions(endpoint, $"Bearer {tokenDetails.token}");

        // Create the refresh policy with automatic token refresh capability
        var refreshPolicy = CopilotTokenRefreshPolicy.CreateWithAutoRefresh(
            tokenDetails.token!,
            tokenDetails.expires_at!.Value,
            githubToken,
            helper);

        // Add the vision + refresh + interaction policies to the pipeline
        options.AddPolicy(refreshPolicy, PipelinePosition.BeforeTransport);
        options.AddPolicy(new VisionHeaderPolicy(), PipelinePosition.BeforeTransport);
        options.AddPolicy(new InteractionHeadersPolicy(new InteractionService()), PipelinePosition.BeforeTransport);

        var integrationIdOk = !string.IsNullOrEmpty(integrationId);
        if (integrationIdOk) options.AddPolicy(new CustomHeaderPolicy("Copilot-Integration-Id", integrationId!), PipelinePosition.BeforeTransport);

        var impersonateVsCodeEditor = !integrationIdOk;
        if (impersonateVsCodeEditor) options.AddPolicy(new CustomHeaderPolicy("Editor-Version", editorVersion), PipelinePosition.BeforeTransport);

        var chatClient = new ChatClient(model, new ApiKeyCredential(" "), options);

        ConsoleHelpers.WriteDebugLine("Using GitHub Copilot token for authentication (with auto-refresh)");
        return chatClient.AsIChatClient();
    }

    public static IChatClient CreateTestChatClient()
    {
        // You can customize responses via environment variables or config if desired
        var responses = new Dictionary<string, string>();
        
        // Check for custom test responses (optional)
        var titleResponse = EnvironmentHelpers.FindEnvVar("TEST_TITLE_RESPONSE");
        if (!string.IsNullOrEmpty(titleResponse))
        {
            responses["title"] = titleResponse;
            responses["generate a title"] = titleResponse;
            responses["create a title"] = titleResponse;
        }
        
        var defaultResponse = EnvironmentHelpers.FindEnvVar("TEST_DEFAULT_RESPONSE") ?? "This is a test response from TestChatClient.";
        var modelId = EnvironmentHelpers.FindEnvVar("TEST_MODEL_ID") ?? "test-model";
        
        var chatClient = new TestChatClient(responses, defaultResponse, modelId);

        ConsoleHelpers.WriteDebugLine("Using test chat client for predictable responses");
        return chatClient;
    }

    private static IChatClient? TryCreateChatClientFromPreferredProvider(out ChatOptions? options)
    {
        var preferredProvider = ConfigStore.Instance.GetFromAnyScope(KnownSettings.AppPreferredProvider).AsString()?.ToLowerInvariant();

        options = null;
        if (!string.IsNullOrEmpty(preferredProvider))
        {
            ConsoleHelpers.WriteDebugLine($"Using preferred provider: {preferredProvider}");

            // Try to create client based on preference
            if (preferredProvider == "test")
            {
                options = null;  // Test client doesn't need options
                return CreateTestChatClient();
            }
            else if ((preferredProvider == "copilot-github" || preferredProvider == "copilot") && !string.IsNullOrEmpty(EnvironmentHelpers.FindEnvVar("GITHUB_TOKEN")))
            {
                return CreateCopilotChatClientWithGitHubToken();
            }
            else if (preferredProvider == "anthropic" && !string.IsNullOrEmpty(EnvironmentHelpers.FindEnvVar("ANTHROPIC_API_KEY")))
            {
                return CreateAnthropicChatClientWithApiKey(out options);
            }
            else if ((preferredProvider == "aws" || preferredProvider == "bedrock" || preferredProvider == "aws-bedrock") && 
                    !string.IsNullOrEmpty(EnvironmentHelpers.FindEnvVar("AWS_BEDROCK_ACCESS_KEY")) &&
                    !string.IsNullOrEmpty(EnvironmentHelpers.FindEnvVar("AWS_BEDROCK_SECRET_KEY")))
            {
                return CreateAWSBedrockChatClient(out options);
            }
            else if ((preferredProvider == "google" || preferredProvider == "gemini" || preferredProvider == "google-gemini") && 
                    !string.IsNullOrEmpty(EnvironmentHelpers.FindEnvVar("GOOGLE_GEMINI_API_KEY")))
            {
                return CreateGeminiChatClient(out options);
            }
            else if ((preferredProvider == "grok" || preferredProvider == "x.ai") && 
                    !string.IsNullOrEmpty(EnvironmentHelpers.FindEnvVar("GROK_API_KEY")))
            {
                return CreateGrokChatClientWithApiKey();
            }
            else if ((preferredProvider == "azure-openai" || preferredProvider == "azure") && !string.IsNullOrEmpty(EnvironmentHelpers.FindEnvVar("AZURE_OPENAI_API_KEY")))
            {
                return CreateAzureOpenAIChatClientWithApiKey();
            }
            else if ((preferredProvider == "azure-anthropic") && !string.IsNullOrEmpty(EnvironmentHelpers.FindEnvVar("AZURE_ANTHROPIC_API_KEY")))
            {
                return CreateAzureAnthropicChatClientWithApiKey(out options);
            }
            else if (preferredProvider == "openai" && !string.IsNullOrEmpty(EnvironmentHelpers.FindEnvVar("OPENAI_API_KEY")))
            {
                return CreateOpenAIChatClientWithApiKey();
            }

            // If preferred provider credentials aren't available, warn the user
            ConsoleHelpers.WriteWarning($"Preferred provider '{preferredProvider}' credentials not found. Falling back to default selection.");
            ConsoleHelpers.WriteLine(overrideQuiet: true);
        }

        return null;
    }

    private static IChatClient? TryCreateChatClientFromEnv(out ChatOptions? options)
    {
        ConsoleHelpers.WriteDebugLine("Creating chat client from environment variables...");
        options = null;

        if (!string.IsNullOrEmpty(EnvironmentHelpers.FindEnvVar("GITHUB_TOKEN")))
        {
            return CreateCopilotChatClientWithGitHubToken();
        }

        if (!string.IsNullOrEmpty(EnvironmentHelpers.FindEnvVar("AZURE_ANTHROPIC_API_KEY")))
        {
            return CreateAzureAnthropicChatClientWithApiKey(out options);
        }

        if (!string.IsNullOrEmpty(EnvironmentHelpers.FindEnvVar("ANTHROPIC_API_KEY")))
        {
            return CreateAnthropicChatClientWithApiKey(out options);
        }

        if (!string.IsNullOrEmpty(EnvironmentHelpers.FindEnvVar("GOOGLE_GEMINI_API_KEY")))
        {
            return CreateGeminiChatClient(out options);
        }
        
        if (!string.IsNullOrEmpty(EnvironmentHelpers.FindEnvVar("GROK_API_KEY")))
        {
            return CreateGrokChatClientWithApiKey();
        }
        
        if (!string.IsNullOrEmpty(EnvironmentHelpers.FindEnvVar("AWS_BEDROCK_ACCESS_KEY")) && 
            !string.IsNullOrEmpty(EnvironmentHelpers.FindEnvVar("AWS_BEDROCK_SECRET_KEY")))
        {
            return CreateAWSBedrockChatClient(out options);
        }

        if (!string.IsNullOrEmpty(EnvironmentHelpers.FindEnvVar("AZURE_OPENAI_API_KEY")))
        {
            return CreateAzureOpenAIChatClientWithApiKey();
        }

        if (!string.IsNullOrEmpty(EnvironmentHelpers.FindEnvVar("OPENAI_API_KEY")))
        {
            return CreateOpenAIChatClientWithApiKey();
        }

        return null;
    }

    public static IChatClient CreateChatClient(out ChatOptions? options)
    {
        // First try to create client from preferred provider
        var client = TryCreateChatClientFromPreferredProvider(out options);

        // If that fails, try to create from environment variables
        client ??= TryCreateChatClientFromEnv(out options);

        // If no client could be created, throw an exception with helpful message
        if (client == null)
        {
            var message =
                string.Join('\n',
                    @"No valid environment variables found.

                    To use Anthropic, please set:
                    - ANTHROPIC_API_KEY
                    - ANTHROPIC_MODEL_NAME (optional)
                    
                    To use AWS Bedrock, please set:
                    - AWS_BEDROCK_ACCESS_KEY
                    - AWS_BEDROCK_SECRET_KEY
                    - AWS_BEDROCK_REGION (optional, default: us-east-1)
                    - AWS_BEDROCK_MODEL_ID (optional, default: anthropic.claude-3-7-sonnet-20250219-v1:0)
                    
                    To use Azure OpenAI, please set:
                    - AZURE_OPENAI_API_KEY
                    - AZURE_OPENAI_ENDPOINT
                    - AZURE_OPENAI_CHAT_DEPLOYMENT

                    To use GitHub Copilot with token authentication, please set:
                    - GITHUB_TOKEN
                    - COPILOT_API_ENDPOINT (optional)
                    - COPILOT_INTEGRATION_ID (optional)
                    - COPILOT_EDITOR_VERSION (optional)
                    - COPILOT_MODEL_NAME (optional)

                    To use Google Gemini, please set:
                    - GOOGLE_GEMINI_API_KEY
                    - GOOGLE_GEMINI_MODEL_ID (optional, default: gemini-2.5-flash-preview-04-17)

                    To use Grok, please set:
                    - GROK_API_KEY
                    - GROK_ENDPOINT (optional, default: https://api.x.ai/v1)
                    - GROK_MODEL_NAME (optional, default: grok-3)

                    To use OpenAI, please set:
                    - OPENAI_API_KEY
                    - OPENAI_ENDPOINT (optional)
                    - OPENAI_CHAT_MODEL_NAME (optional)

                    To use Test provider for development/testing, please set:
                    - TEST_DEFAULT_RESPONSE (optional)
                    - TEST_TITLE_RESPONSE (optional)"
                .Split(new[] { '\n' })
                .Select(line => line.Trim()));

            throw new EnvVarSettingException(message);
        }

        return client;
    }

    private static AzureOpenAIClientOptions InitAzureOpenAIClientOptions()
    {
        var options = new AzureOpenAIClientOptions();
        InitPipelineOptionsPolicies(options);
        return options;
    }

    private static OpenAIClientOptions InitOpenAIClientOptions(string? endpoint = null, string? authHeader = null)
    {
        var options = new OpenAIClientOptions();
        InitPipelineOptionsPolicies(options);

        var endpointOk = !string.IsNullOrEmpty(endpoint);
        if (endpointOk) options.Endpoint = new Uri(endpoint!);

        var authHeaderOk = authHeader != null;
        if (authHeaderOk) options.AddPolicy(new CustomHeaderPolicy("Authorization", authHeader!), PipelinePosition.BeforeTransport);

        return options;
    }

    private static ClientPipelineOptions InitPipelineOptionsPolicies(ClientPipelineOptions options)
    {
        options.AddPolicy(new CustomJsonPropertyRemovalPolicy("tool_choice"), PipelinePosition.PerCall);
        options.AddPolicy(new FixNullFunctionArgsPolicy(), PipelinePosition.PerCall);
        options.AddPolicy(new LogTrafficEventPolicy(), PipelinePosition.PerCall);

        options.RetryPolicy = new ClientRetryPolicy(maxRetries: 10);

        // Apply timeout if configured
        var timeoutSetting = ConfigStore.Instance.GetFromAnyScope(KnownSettings.AppChatCompletionTimeout);
        if (timeoutSetting.AsInt() > 0)
        {
            int timeoutSeconds = timeoutSetting.AsInt();
            ConsoleHelpers.WriteDebugLine($"Setting chat completion timeout to {timeoutSeconds} seconds");
            options.NetworkTimeout = TimeSpan.FromSeconds(timeoutSeconds);
        }

        return options;
    }
}
