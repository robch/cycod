/**
 * Defines known configuration settings with standardized formats and access rules.
 */
export class KnownSettings {
    // Setting Keys (Dot Notation)

    // Anthropic settings
    public static readonly AnthropicApiKey = "Anthropic.ApiKey";
    public static readonly AnthropicModelName = "Anthropic.ModelName";
    
    // AWS Bedrock settings
    public static readonly AWSBedrockAccessKey = "AWS.Bedrock.AccessKey";
    public static readonly AWSBedrockSecretKey = "AWS.Bedrock.SecretKey";
    public static readonly AWSBedrockRegion = "AWS.Bedrock.Region";
    public static readonly AWSBedrockModelId = "AWS.Bedrock.ModelId";
    
    // Google Gemini settings
    public static readonly GoogleGeminiApiKey = "Google.Gemini.ApiKey";
    public static readonly GoogleGeminiModelId = "Google.Gemini.ModelId";
    
    // Grok settings
    public static readonly GrokApiKey = "Grok.ApiKey";
    public static readonly GrokModelName = "Grok.ModelName";
    public static readonly GrokEndpoint = "Grok.Endpoint";

    // Azure OpenAI settings
    public static readonly AzureOpenAIApiKey = "Azure.OpenAI.ApiKey";
    public static readonly AzureOpenAIEndpoint = "Azure.OpenAI.Endpoint";
    public static readonly AzureOpenAIChatDeployment = "Azure.OpenAI.ChatDeployment";
    
    // OpenAI settings
    public static readonly OpenAIApiKey = "OpenAI.ApiKey";
    public static readonly OpenAIEndpoint = "OpenAI.Endpoint";
    public static readonly OpenAIChatModelName = "OpenAI.ChatModelName";
    
    // GitHub settings
    public static readonly GitHubToken = "GitHub.Token";
    
    // Copilot settings
    public static readonly CopilotModelName = "Copilot.ModelName";
    public static readonly CopilotApiEndpoint = "Copilot.ApiEndpoint";
    public static readonly CopilotIntegrationId = "Copilot.IntegrationId";
    public static readonly CopilotEditorVersion = "Copilot.EditorVersion";

    // Application settings
    public static readonly AppMaxPromptTokens = "App.MaxPromptTokens";
    public static readonly AppMaxOutputTokens = "App.MaxOutputTokens";
    public static readonly AppMaxToolTokens = "App.MaxToolTokens";
    public static readonly AppMaxChatTokens = "App.MaxChatTokens";
    public static readonly AppPreferredProvider = "App.PreferredProvider";
    public static readonly AppAutoSaveChatHistory = "App.AutoSaveChatHistory";
    public static readonly AppAutoSaveTrajectory = "App.AutoSaveTrajectory";
    public static readonly AppChatCompletionTimeout = "App.ChatCompletionTimeout";
    public static readonly AppAutoApprove = "App.AutoApprove";
    public static readonly AppAutoDeny = "App.AutoDeny";
    
    // Secret Settings
    
    /**
     * Collection of known setting names that should be treated as secrets.
     */
    private static readonly _secretSettings = new Set<string>([
        // Anthropic secrets
        KnownSettings.AnthropicApiKey.toLowerCase(),
        
        // AWS Bedrock secrets
        KnownSettings.AWSBedrockAccessKey.toLowerCase(),
        KnownSettings.AWSBedrockSecretKey.toLowerCase(),
        
        // Google Gemini secrets
        KnownSettings.GoogleGeminiApiKey.toLowerCase(),
        
        // Grok secrets
        KnownSettings.GrokApiKey.toLowerCase(),
        
        // Azure OpenAI secrets
        KnownSettings.AzureOpenAIApiKey.toLowerCase(),
        
        // OpenAI secrets
        KnownSettings.OpenAIApiKey.toLowerCase(),
        
        // GitHub secrets
        KnownSettings.GitHubToken.toLowerCase(),
    ]);
    
    // Setting Format Mappings
    
    /**
     * Mapping from dot notation to environment variable format.
     */
    private static readonly _dotToEnvVarMap = new Map<string, string>([
        // Anthropic mappings
        [KnownSettings.AnthropicApiKey.toLowerCase(), "ANTHROPIC_API_KEY"],
        [KnownSettings.AnthropicModelName.toLowerCase(), "ANTHROPIC_MODEL_NAME"],
        
        // AWS Bedrock mappings
        [KnownSettings.AWSBedrockAccessKey.toLowerCase(), "AWS_BEDROCK_ACCESS_KEY"],
        [KnownSettings.AWSBedrockSecretKey.toLowerCase(), "AWS_BEDROCK_SECRET_KEY"],
        [KnownSettings.AWSBedrockRegion.toLowerCase(), "AWS_BEDROCK_REGION"],
        [KnownSettings.AWSBedrockModelId.toLowerCase(), "AWS_BEDROCK_MODEL_ID"],
        
        // Google Gemini mappings
        [KnownSettings.GoogleGeminiApiKey.toLowerCase(), "GOOGLE_GEMINI_API_KEY"],
        [KnownSettings.GoogleGeminiModelId.toLowerCase(), "GOOGLE_GEMINI_MODEL_ID"],

        // Grok mappings
        [KnownSettings.GrokApiKey.toLowerCase(), "GROK_API_KEY"],
        [KnownSettings.GrokModelName.toLowerCase(), "GROK_MODEL_NAME"],
        [KnownSettings.GrokEndpoint.toLowerCase(), "GROK_ENDPOINT"],

        // Azure OpenAI mappings
        [KnownSettings.AzureOpenAIApiKey.toLowerCase(), "AZURE_OPENAI_API_KEY"],
        [KnownSettings.AzureOpenAIEndpoint.toLowerCase(), "AZURE_OPENAI_ENDPOINT"],
        [KnownSettings.AzureOpenAIChatDeployment.toLowerCase(), "AZURE_OPENAI_CHAT_DEPLOYMENT"],
        
        // OpenAI mappings
        [KnownSettings.OpenAIApiKey.toLowerCase(), "OPENAI_API_KEY"],
        [KnownSettings.OpenAIEndpoint.toLowerCase(), "OPENAI_ENDPOINT"],
        [KnownSettings.OpenAIChatModelName.toLowerCase(), "OPENAI_CHAT_MODEL_NAME"],
        
        // GitHub mappings
        [KnownSettings.GitHubToken.toLowerCase(), "GITHUB_TOKEN"],
        
        // Copilot mappings
        [KnownSettings.CopilotModelName.toLowerCase(), "COPILOT_MODEL_NAME"],
        [KnownSettings.CopilotApiEndpoint.toLowerCase(), "COPILOT_API_ENDPOINT"],
        [KnownSettings.CopilotIntegrationId.toLowerCase(), "COPILOT_INTEGRATION_ID"],
        [KnownSettings.CopilotEditorVersion.toLowerCase(), "COPILOT_EDITOR_VERSION"],
        
        // Application settings
        [KnownSettings.AppMaxPromptTokens.toLowerCase(), "CYCOD_MAX_PROMPT_TOKENS"],
        [KnownSettings.AppMaxOutputTokens.toLowerCase(), "CYCOD_MAX_OUTPUT_TOKENS"],
        [KnownSettings.AppMaxToolTokens.toLowerCase(), "CYCOD_MAX_TOOL_TOKENS"],
        [KnownSettings.AppMaxChatTokens.toLowerCase(), "CYCOD_MAX_CHAT_TOKENS"],
        [KnownSettings.AppPreferredProvider.toLowerCase(), "CYCOD_PREFERRED_PROVIDER"],
        [KnownSettings.AppAutoSaveChatHistory.toLowerCase(), "CYCOD_AUTO_SAVE_CHAT_HISTORY"],
        [KnownSettings.AppAutoSaveTrajectory.toLowerCase(), "CYCOD_AUTO_SAVE_TRAJECTORY"],
        [KnownSettings.AppChatCompletionTimeout.toLowerCase(), "CYCOD_CHAT_COMPLETION_TIMEOUT"],
        [KnownSettings.AppAutoApprove.toLowerCase(), "CYCOD_AUTO_APPROVE"],
        [KnownSettings.AppAutoDeny.toLowerCase(), "CYCOD_AUTO_DENY"]
    ]);
    
    /**
     * Mapping from dot notation to CLI option format.
     */
    private static readonly _dotToCLIOptionMap = new Map<string, string>([
        // Anthropic mappings
        [KnownSettings.AnthropicApiKey.toLowerCase(), "--anthropic-api-key"],
        [KnownSettings.AnthropicModelName.toLowerCase(), "--anthropic-model-name"],
        
        // AWS Bedrock mappings
        [KnownSettings.AWSBedrockAccessKey.toLowerCase(), "--aws-bedrock-access-key"],
        [KnownSettings.AWSBedrockSecretKey.toLowerCase(), "--aws-bedrock-secret-key"],
        [KnownSettings.AWSBedrockModelId.toLowerCase(), "--aws-bedrock-model-id"],
        [KnownSettings.AWSBedrockRegion.toLowerCase(), "--aws-bedrock-region"],
        
        // Google Gemini mappings
        [KnownSettings.GoogleGeminiApiKey.toLowerCase(), "--google-gemini-api-key"],
        [KnownSettings.GoogleGeminiModelId.toLowerCase(), "--google-gemini-model-id"],

        // Grok mappings
        [KnownSettings.GrokApiKey.toLowerCase(), "--grok-api-key"],
        [KnownSettings.GrokModelName.toLowerCase(), "--grok-model-name"],
        [KnownSettings.GrokEndpoint.toLowerCase(), "--grok-endpoint"],

        // Azure OpenAI mappings
        [KnownSettings.AzureOpenAIApiKey.toLowerCase(), "--azure-openai-api-key"],
        [KnownSettings.AzureOpenAIEndpoint.toLowerCase(), "--azure-openai-endpoint"],
        [KnownSettings.AzureOpenAIChatDeployment.toLowerCase(), "--azure-openai-chat-deployment"],
        
        // OpenAI mappings
        [KnownSettings.OpenAIApiKey.toLowerCase(), "--openai-api-key"],
        [KnownSettings.OpenAIEndpoint.toLowerCase(), "--openai-endpoint"],
        [KnownSettings.OpenAIChatModelName.toLowerCase(), "--openai-chat-model-name"],
        
        // GitHub mappings
        [KnownSettings.GitHubToken.toLowerCase(), "--github-token"],
        
        // Copilot mappings
        [KnownSettings.CopilotModelName.toLowerCase(), "--copilot-model-name"],
        [KnownSettings.CopilotApiEndpoint.toLowerCase(), "--copilot-api-endpoint"],
        [KnownSettings.CopilotIntegrationId.toLowerCase(), "--copilot-integration-id"],
        [KnownSettings.CopilotEditorVersion.toLowerCase(), "--copilot-editor-version"],
        
        // Application settings
        [KnownSettings.AppMaxPromptTokens.toLowerCase(), "--max-prompt-tokens"],
        [KnownSettings.AppMaxOutputTokens.toLowerCase(), "--max-output-tokens"],
        [KnownSettings.AppMaxToolTokens.toLowerCase(), "--max-tool-tokens"],
        [KnownSettings.AppMaxChatTokens.toLowerCase(), "--max-chat-tokens"],
        [KnownSettings.AppAutoSaveChatHistory.toLowerCase(), "--auto-save-chat-history"],
        [KnownSettings.AppAutoSaveTrajectory.toLowerCase(), "--auto-save-trajectory"],
        [KnownSettings.AppChatCompletionTimeout.toLowerCase(), "--chat-completion-timeout"],
        [KnownSettings.AppAutoApprove.toLowerCase(), "--auto-approve"],
        [KnownSettings.AppAutoDeny.toLowerCase(), "--auto-deny"]
    ]);
    
    /**
     * Mapping from environment variable format to dot notation.
     */
    private static get _envVarToDotMap(): Map<string, string> {
        return KnownSettings._envVarToDotMapInstance;
    }
    
    /**
     * Mapping from CLI option format to dot notation.
     */
    private static get _cliOptionToDotMap(): Map<string, string> {
        return KnownSettings._cliOptionToDotMapInstance;
    }

    /**
     * Collection of settings that can have multiple values.
     */
    private static readonly _multiValueSettings = [
        KnownSettings.AppAutoApprove,
        KnownSettings.AppAutoDeny
    ];


    // Category Groupings

    /**
     * Collection of settings for Anthropic integration.
     */
    public static readonly AnthropicSettings = new Set<string>([
        KnownSettings.AnthropicApiKey,
        KnownSettings.AnthropicModelName
    ]);
    
    /**
     * Collection of settings for AWS Bedrock integration.
     */
    public static readonly AWSBedrockSettings = new Set<string>([
        KnownSettings.AWSBedrockAccessKey,
        KnownSettings.AWSBedrockSecretKey,
        KnownSettings.AWSBedrockRegion,
        KnownSettings.AWSBedrockModelId
    ]);
    
    /**
     * Collection of settings for Google Gemini integration.
     */
    public static readonly GoogleGeminiSettings = new Set<string>([
        KnownSettings.GoogleGeminiApiKey,
        KnownSettings.GoogleGeminiModelId
    ]);
    
    /**
     * Collection of settings for Grok integration.
     */
    public static readonly GrokSettings = new Set<string>([
        KnownSettings.GrokApiKey,
        KnownSettings.GrokModelName,
        KnownSettings.GrokEndpoint
    ]);
    
    /**
     * Collection of settings for Azure OpenAI integration.
     */
    public static readonly AzureOpenAISettings = new Set<string>([
        KnownSettings.AzureOpenAIApiKey,
        KnownSettings.AzureOpenAIEndpoint,
        KnownSettings.AzureOpenAIChatDeployment
    ]);
    
    /**
     * Collection of settings for OpenAI integration.
     */
    public static readonly OpenAISettings = new Set<string>([
        KnownSettings.OpenAIApiKey,
        KnownSettings.OpenAIEndpoint,
        KnownSettings.OpenAIChatModelName
    ]);
    
    /**
     * Collection of settings for GitHub integration.
     */
    public static readonly GitHubSettings = new Set<string>([
        KnownSettings.GitHubToken
    ]);
    
    /**
     * Collection of settings for Copilot integration.
     */
    public static readonly CopilotSettings = new Set<string>([
        KnownSettings.CopilotModelName,
        KnownSettings.CopilotApiEndpoint,
        KnownSettings.CopilotIntegrationId,
        KnownSettings.CopilotEditorVersion
    ]);
    
    /**
     * Collection of settings for application configuration.
     */
    public static readonly AppSettings = new Set<string>([
        KnownSettings.AppMaxPromptTokens,
        KnownSettings.AppMaxOutputTokens,
        KnownSettings.AppMaxToolTokens,
        KnownSettings.AppMaxChatTokens,
        KnownSettings.AppPreferredProvider,
        KnownSettings.AppAutoSaveChatHistory,
        KnownSettings.AppAutoSaveTrajectory,
        KnownSettings.AppChatCompletionTimeout,
        KnownSettings.AppAutoApprove,
        KnownSettings.AppAutoDeny
    ]);
    
    
    // Static initialization - create reverse mappings
    private static _envVarToDotMapInstance: Map<string, string>;
    private static _cliOptionToDotMapInstance: Map<string, string>;
    
    static {
        // Initialize reverse mappings
        KnownSettings._envVarToDotMapInstance = new Map<string, string>();
        for (const [key, value] of KnownSettings._dotToEnvVarMap) {
            KnownSettings._envVarToDotMapInstance.set(value.toLowerCase(), key);
        }
        
        KnownSettings._cliOptionToDotMapInstance = new Map<string, string>();
        for (const [key, value] of KnownSettings._dotToCLIOptionMap) {
            KnownSettings._cliOptionToDotMapInstance.set(value.toLowerCase(), key);
        }
    }
    
    /**
     * Checks if the given key is a known setting.
     * @param key The key to check (in any format).
     * @returns True if the key is a known setting, false otherwise.
     */
    public static IsKnown(key: string): boolean {
        const lowerKey = key.toLowerCase();
        if (KnownSettings._dotToEnvVarMap.has(lowerKey)) return true;
        if (KnownSettings._envVarToDotMap.has(lowerKey)) return true;
        if (KnownSettings._cliOptionToDotMap.has(lowerKey)) return true;
        return false;
    }
    
    /**
     * Checks if the given key is a secret value that should be obfuscated.
     * @param key The key to check (in any format).
     * @returns True if the key is a secret, false otherwise.
     */
    public static IsSecret(key: string): boolean {
        const dotNotationKey = KnownSettings.ToDotNotation(key);
        return KnownSettings._secretSettings.has(dotNotationKey.toLowerCase());
    }
    
    /**
     * Gets all known setting names in dot notation format.
     * @returns Collection of all known setting names.
     */
    public static GetAllKnownSettings(): string[] {
        return Array.from(KnownSettings._dotToEnvVarMap.keys());
    }

    /**
     * Checks if the given key can have multiple values.
     * @param key
     * @returns True if the key can have multiple values, false otherwise.
     */
    public static IsMultiValue(key: string): boolean {
        const dotNotationKey = KnownSettings.ToDotNotation(key);
        return KnownSettings._multiValueSettings.some(setting => 
            setting.toLowerCase() === dotNotationKey.toLowerCase()
        );
    }
    
    /**
     * Gets the canonical form of a known setting key.
     * @param key The key to normalize (in any format).
     * @returns The canonical form of the key, or the original key if not found.
     */
    public static GetCanonicalForm(key: string): string {
        // First normalize to dot notation
        const normalized = KnownSettings.ToDotNotation(key);
        
        // Find the exact match in our known settings (case-insensitive)
        for (const knownSetting of KnownSettings._dotToEnvVarMap.keys()) {
            if (knownSetting.toLowerCase() === normalized.toLowerCase()) {
                return knownSetting;
            }
        }
        
        return normalized;
    }
    
    /**
     * Converts a setting name to environment variable format.
     * @param key The setting name in any format.
     * @returns The equivalent environment variable name.
     */
    public static ToEnvironmentVariable(key: string): string {
        if (KnownSettings.IsEnvironmentVariableFormat(key)) return key;

        // Try to map to an environment variable
        const dotNotationKey = KnownSettings.ToDotNotation(key);
        const envVarKey = KnownSettings._dotToEnvVarMap.get(dotNotationKey.toLowerCase());
        if (envVarKey) {
            return envVarKey;
        }

        // Otherwise, just return it
        return key;
    }
    
    /**
     * Converts a setting name to CLI option format.
     * @param key The setting name in any format.
     * @returns The equivalent CLI option.
     */
    public static ToCLIOption(key: string): string {
        if (KnownSettings.IsCLIOptionFormat(key)) return key;
        
        // Try to map to a CLI option
        const dotNotationKey = KnownSettings.ToDotNotation(key);
        const cliOption = KnownSettings._dotToCLIOptionMap.get(dotNotationKey.toLowerCase());
        if (cliOption) {
            return cliOption;
        }
        
        // Otherwise, use a general algorithm
        const parts = dotNotationKey.split('.');
        const kebabParts = parts.map(p => KnownSettings.ToKebabCase(p));
        return "--" + kebabParts.join("-").toLowerCase();
    }
    
    /**
     * Converts a setting name to dot notation format.
     * @param key The setting name in any format.
     * @returns The equivalent dot notation.
     */
    public static ToDotNotation(key: string): string {
        // If it's an environment variable format
        if (KnownSettings.IsEnvironmentVariableFormat(key)) {
            const dotNotation = KnownSettings._envVarToDotMap.get(key.toLowerCase());
            if (dotNotation) {
                return dotNotation;
            }
        }
        
        // If it's a CLI option format
        if (KnownSettings.IsCLIOptionFormat(key)) {
            const dotNotation = KnownSettings._cliOptionToDotMap.get(key.toLowerCase());
            if (dotNotation) {
                return dotNotation;
            }
            
            // Remove leading -- and convert kebab-case to PascalCase with dots
            const trimmed = key.replace(/^--+/, '');
            const parts = trimmed.split('-');
            for (let i = 0; i < parts.length; i++) {
                if (parts[i].length > 0) {
                    parts[i] = parts[i].charAt(0).toUpperCase() + parts[i].substring(1).toLowerCase();
                }
            }
            
            return parts.join('.');
        }

        return key;
    }
    
    /**
     * Determines if the given key is in environment variable format.
     * @param key The key to check.
     * @returns True if the key is in environment variable format, false otherwise.
     */
    public static IsEnvironmentVariableFormat(key: string): boolean {
        return /^[A-Z0-9_]+$/.test(key);
    }
    
    /**
     * Determines if the given key is in CLI option format.
     * @param key The key to check.
     * @returns True if the key is in CLI option format, false otherwise.
     */
    public static IsCLIOptionFormat(key: string): boolean {
        return key.startsWith('--');
    }
    
    /**
     * Converts a string from PascalCase to kebab-case.
     * @param input The PascalCase string to convert.
     * @returns The kebab-case version of the input string.
     */
    private static ToKebabCase(input: string): string {
        if (!input) {
            return input;
        }
        
        // Insert a hyphen before each uppercase letter that follows a lowercase letter
        const result = input.replace(/(?<!^)([A-Z])/g, '-$1');
        return result.toLowerCase();
    }
}