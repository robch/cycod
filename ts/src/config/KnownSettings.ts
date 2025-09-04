// Known configuration settings - matches C# KnownSettings.cs exactly
export class KnownSettings {
  // App settings
  public static readonly AppMaxOutputTokens = "App.MaxOutputTokens";
  public static readonly AppMaxPromptTokens = "App.MaxPromptTokens";
  public static readonly AppMaxToolTokens = "App.MaxToolTokens";
  public static readonly AppMaxChatTokens = "App.MaxChatTokens";
  public static readonly AppPreferredProvider = "App.PreferredProvider";
  public static readonly AppAutoSaveChatHistory = "App.AutoSaveChatHistory";
  public static readonly AppAutoSaveTrajectory = "App.AutoSaveTrajectory";
  public static readonly AppAutoApprove = "App.AutoApprove";
  public static readonly AppAutoDeny = "App.AutoDeny";

  // API Key settings (these would typically be stored as environment variable references)
  public static readonly AnthropicApiKey = "Anthropic.ApiKey";
  public static readonly AnthropicModelName = "Anthropic.ModelName";
  public static readonly OpenAIApiKey = "openai.apiKey";  // Match actual config key format
  public static readonly OpenAIModelName = "openai.chatModelName";  // Match actual config key format
  public static readonly AzureOpenAIApiKey = "Azure.OpenAI.ApiKey";
  public static readonly AzureOpenAIEndpoint = "Azure.OpenAI.Endpoint";
  public static readonly AzureOpenAIChatDeployment = "Azure.OpenAI.ChatDeployment";
  public static readonly AWSBedrockAccessKey = "AWS.Bedrock.AccessKey";
  public static readonly AWSBedrockSecretKey = "AWS.Bedrock.SecretKey";
  public static readonly AWSBedrockRegion = "AWS.Bedrock.Region";
  public static readonly AWSBedrockModelId = "AWS.Bedrock.ModelId";
  public static readonly GoogleGeminiApiKey = "Google.Gemini.ApiKey";
  public static readonly GoogleGeminiModelName = "Google.Gemini.ModelName";
  public static readonly GitHubToken = "GitHub.Token";
  public static readonly CopilotApiEndpoint = "Copilot.ApiEndpoint";
  public static readonly CopilotIntegrationId = "Copilot.IntegrationId";
  public static readonly CopilotEditorVersion = "Copilot.EditorVersion";
  public static readonly CopilotModelName = "Copilot.ModelName";
  public static readonly GrokApiKey = "Grok.ApiKey";
  public static readonly GrokModelName = "Grok.ModelName";

  // Environment variable mappings (matches C# KnownSettings.EnvironmentVariableNames)
  public static readonly EnvironmentVariableNames: Record<string, string> = {
    [KnownSettings.AppMaxOutputTokens]: "CYCOD_MAX_OUTPUT_TOKENS",
    [KnownSettings.AppMaxPromptTokens]: "CYCOD_MAX_PROMPT_TOKENS",
    [KnownSettings.AppMaxToolTokens]: "CYCOD_MAX_TOOL_TOKENS",
    [KnownSettings.AppMaxChatTokens]: "CYCOD_MAX_CHAT_TOKENS",
    [KnownSettings.AppPreferredProvider]: "CYCOD_PREFERRED_PROVIDER",
    [KnownSettings.AppAutoSaveChatHistory]: "CYCOD_AUTO_SAVE_CHAT_HISTORY",
    [KnownSettings.AppAutoSaveTrajectory]: "CYCOD_AUTO_SAVE_TRAJECTORY",
    [KnownSettings.AppAutoApprove]: "CYCOD_AUTO_APPROVE",
    [KnownSettings.AppAutoDeny]: "CYCOD_AUTO_DENY",

    [KnownSettings.AnthropicApiKey]: "ANTHROPIC_API_KEY",
    [KnownSettings.AnthropicModelName]: "ANTHROPIC_MODEL_NAME",
    [KnownSettings.OpenAIApiKey]: "OPENAI_API_KEY",
    [KnownSettings.OpenAIModelName]: "OPENAI_MODEL_NAME",
    [KnownSettings.AzureOpenAIApiKey]: "AZURE_OPENAI_API_KEY",
    [KnownSettings.AzureOpenAIEndpoint]: "AZURE_OPENAI_ENDPOINT",
    [KnownSettings.AzureOpenAIChatDeployment]: "AZURE_OPENAI_CHAT_DEPLOYMENT",
    [KnownSettings.AWSBedrockAccessKey]: "AWS_BEDROCK_ACCESS_KEY",
    [KnownSettings.AWSBedrockSecretKey]: "AWS_BEDROCK_SECRET_KEY",
    [KnownSettings.AWSBedrockRegion]: "AWS_BEDROCK_REGION",
    [KnownSettings.AWSBedrockModelId]: "AWS_BEDROCK_MODEL_ID",
    [KnownSettings.GoogleGeminiApiKey]: "GOOGLE_GEMINI_API_KEY",
    [KnownSettings.GoogleGeminiModelName]: "GOOGLE_GEMINI_MODEL_NAME",
    [KnownSettings.GitHubToken]: "GITHUB_TOKEN",
    [KnownSettings.CopilotApiEndpoint]: "COPILOT_API_ENDPOINT",
    [KnownSettings.CopilotIntegrationId]: "COPILOT_INTEGRATION_ID",
    [KnownSettings.CopilotEditorVersion]: "COPILOT_EDITOR_VERSION",
    [KnownSettings.CopilotModelName]: "COPILOT_MODEL_NAME",
    [KnownSettings.GrokApiKey]: "GROK_API_KEY",
    [KnownSettings.GrokModelName]: "GROK_MODEL_NAME"
  };

  // All valid app settings (matches C# AppSettings array)
  public static readonly AppSettings: string[] = [
    KnownSettings.AppMaxOutputTokens,
    KnownSettings.AppMaxPromptTokens,
    KnownSettings.AppMaxToolTokens,
    KnownSettings.AppMaxChatTokens,
    KnownSettings.AppPreferredProvider,
    KnownSettings.AppAutoSaveChatHistory,
    KnownSettings.AppAutoSaveTrajectory,
    KnownSettings.AppAutoApprove,
    KnownSettings.AppAutoDeny
  ];
}

// Helper function to get environment variable value (matches C# EnvironmentHelpers.FindEnvVar)
export function findEnvVar(envVarName: string): string | null {
  return process.env[envVarName] || null;
}

// Helper function to get environment variable with default (matches C# pattern)
export function findEnvVarWithDefault(envVarName: string, defaultValue: string): string {
  return process.env[envVarName] || defaultValue;
}

// Helper function to get setting value from config store first, then environment variables
export async function findSettingValue(configKey: string): Promise<string | null> {
  const { ConfigStore } = await import('./ConfigStore');
  const configStore = new ConfigStore();
  
  // First try config store
  const configValue = await configStore.getFromAnyScope(configKey);
  if (configValue?.value) {
    return configValue.value.toString();
  }
  
  // Then try environment variable
  const envVarName = KnownSettings.EnvironmentVariableNames[configKey];
  if (envVarName) {
    return findEnvVar(envVarName);
  }
  
  return null;
}

// Helper function with default fallback
export async function findSettingValueWithDefault(configKey: string, defaultValue: string): Promise<string> {
  const value = await findSettingValue(configKey);
  return value || defaultValue;
}