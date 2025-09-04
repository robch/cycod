import Anthropic from '@anthropic-ai/sdk';
import OpenAI from 'openai';
import { IChatClient } from './FunctionCallingChat';
import { ChatMessage } from '../types';
import { ConfigStore } from '../config/ConfigStore';
import { KnownSettings, findEnvVar, findEnvVarWithDefault, findSettingValue, findSettingValueWithDefault } from '../config/KnownSettings';

// Anthropic client implementation
class AnthropicChatClient implements IChatClient {
  private client: Anthropic;
  private model: string;

  constructor(apiKey: string, model: string = 'claude-3-5-sonnet-20241022') {
    this.client = new Anthropic({ apiKey });
    this.model = model;
  }

  async *createStreamingResponse(messages: ChatMessage[]): AsyncIterableIterator<any> {
    // Convert our ChatMessage format to Anthropic format
    const anthropicMessages = this.convertToAnthropicFormat(messages);
    
    const stream = await this.client.messages.create({
      model: this.model,
      max_tokens: 4000,
      messages: anthropicMessages.messages,
      system: anthropicMessages.system,
      stream: true,
    });

    for await (const event of stream) {
      if (event.type === 'content_block_delta' && event.delta?.type === 'text_delta') {
        yield {
          delta: { text: event.delta.text },
          content: event.delta.text
        };
      }
    }
  }

  async createResponse(messages: ChatMessage[]): Promise<any> {
    const anthropicMessages = this.convertToAnthropicFormat(messages);
    
    return await this.client.messages.create({
      model: this.model,
      max_tokens: 4000,
      messages: anthropicMessages.messages,
      system: anthropicMessages.system,
    });
  }

  private convertToAnthropicFormat(messages: ChatMessage[]): { messages: any[], system?: string } {
    const systemMessages = messages.filter(m => m.role === 'system');
    const nonSystemMessages = messages.filter(m => m.role !== 'system');

    const system = systemMessages.map(m => m.content).join('\n\n');

    const anthropicMessages = nonSystemMessages.map(m => ({
      role: m.role,
      content: m.content
    }));

    return {
      messages: anthropicMessages,
      system: system || undefined
    };
  }
}

// OpenAI client implementation
class OpenAIChatClient implements IChatClient {
  private client: OpenAI;
  private model: string;

  constructor(apiKey: string, model: string = 'gpt-4o') {
    this.client = new OpenAI({ apiKey });
    this.model = model;
  }

  async *createStreamingResponse(messages: ChatMessage[]): AsyncIterableIterator<any> {
    const openaiMessages = messages.map(m => ({
      role: m.role as 'system' | 'user' | 'assistant',
      content: m.content
    }));

    const stream = await this.client.chat.completions.create({
      model: this.model,
      messages: openaiMessages,
      stream: true,
    });

    for await (const chunk of stream) {
      const delta = chunk.choices[0]?.delta;
      if (delta?.content) {
        yield {
          delta: { text: delta.content },
          content: delta.content,
          choices: chunk.choices
        };
      }
    }
  }

  async createResponse(messages: ChatMessage[]): Promise<any> {
    const openaiMessages = messages.map(m => ({
      role: m.role as 'system' | 'user' | 'assistant',
      content: m.content
    }));

    return await this.client.chat.completions.create({
      model: this.model,
      messages: openaiMessages,
    });
  }
}

// Interfaces for client options (matching C# ChatOptions)
export interface ChatOptions {
  modelId?: string;
  toolMode?: string;
  maxOutputTokens?: number;
}

// Factory class matching C# ChatClientFactory exactly
export class ChatClientFactory {
  /**
   * Main method to create chat client - matches C# CreateChatClient(out ChatOptions? options) exactly
   */
  static async createChatClient(): Promise<{ client: IChatClient; options: ChatOptions | null }> {
    // First try to create client from preferred provider (matches C# exactly)
    let result = await this.tryCreateChatClientFromPreferredProvider();
    
    // If that fails, try to create from environment variables (matches C# exactly)
    if (!result) {
      result = await this.tryCreateChatClientFromEnv();
    }

    // If no client could be created, throw an exception with helpful message (matches C# exactly)
    if (!result) {
      const message = [
        "No valid environment variables found.",
        "",
        "To use Anthropic, please set:",
        "- ANTHROPIC_API_KEY",
        "- ANTHROPIC_MODEL_NAME (optional)",
        "",
        "To use OpenAI, please set:",
        "- OPENAI_API_KEY", 
        "- OPENAI_MODEL_NAME (optional)",
        "",
        "To use Azure OpenAI, please set:",
        "- AZURE_OPENAI_API_KEY",
        "- AZURE_OPENAI_ENDPOINT",
        "- AZURE_OPENAI_CHAT_DEPLOYMENT",
        "",
        "To use GitHub Copilot with token authentication, please set:",
        "- GITHUB_TOKEN",
        "- COPILOT_API_ENDPOINT (optional)",
        "- COPILOT_INTEGRATION_ID (optional)",
        "",
        "Or use config commands to set preferred provider:",
        "  cycodjs config set App.PreferredProvider anthropic",
        "  cycodjs config set App.PreferredProvider openai",
        "  cycodjs config set App.PreferredProvider azure-openai"
      ].join('\n');
      
      throw new Error(message);
    }

    return result;
  }

  /**
   * Try to create client from preferred provider set in config - matches C# TryCreateChatClientFromPreferredProvider exactly
   */
  private static async tryCreateChatClientFromPreferredProvider(): Promise<{ client: IChatClient; options: ChatOptions | null } | null> {
    const configStore = new ConfigStore();
    const configValue = await configStore.getFromAnyScope(KnownSettings.AppPreferredProvider);
    const preferredProvider = configValue?.value?.toString()?.toLowerCase();

    if (!preferredProvider) {
      return null;
    }

    console.log(`Using preferred provider: ${preferredProvider}`); // Match C# debug output

    // Try to create client based on preference (matches C# logic exactly)
    if ((preferredProvider === 'copilot-github' || preferredProvider === 'copilot') && 
        (await findSettingValue(KnownSettings.GitHubToken) || findEnvVar('GITHUB_TOKEN'))) {
      return await this.createCopilotChatClientWithGitHubToken();
    }
    else if (preferredProvider === 'anthropic' && 
            (await findSettingValue(KnownSettings.AnthropicApiKey) || findEnvVar('ANTHROPIC_API_KEY'))) {
      return await this.createAnthropicChatClientWithApiKey();
    }
    else if ((preferredProvider === 'aws' || preferredProvider === 'bedrock' || preferredProvider === 'aws-bedrock') && 
            (await findSettingValue(KnownSettings.AWSBedrockAccessKey) || findEnvVar('AWS_BEDROCK_ACCESS_KEY')) &&
            (await findSettingValue(KnownSettings.AWSBedrockSecretKey) || findEnvVar('AWS_BEDROCK_SECRET_KEY'))) {
      return await this.createAWSBedrockChatClient();
    }
    else if ((preferredProvider === 'google' || preferredProvider === 'gemini' || preferredProvider === 'google-gemini') && 
            (await findSettingValue(KnownSettings.GoogleGeminiApiKey) || findEnvVar('GOOGLE_GEMINI_API_KEY'))) {
      return await this.createGeminiChatClient();
    }
    else if ((preferredProvider === 'grok' || preferredProvider === 'x.ai') && 
            (await findSettingValue(KnownSettings.GrokApiKey) || findEnvVar('GROK_API_KEY'))) {
      return await this.createGrokChatClient();
    }
    else if ((preferredProvider === 'azure' || preferredProvider === 'azure-openai') && 
            (await findSettingValue(KnownSettings.AzureOpenAIApiKey) || findEnvVar('AZURE_OPENAI_API_KEY')) &&
            (await findSettingValue(KnownSettings.AzureOpenAIEndpoint) || findEnvVar('AZURE_OPENAI_ENDPOINT')) &&
            (await findSettingValue(KnownSettings.AzureOpenAIChatDeployment) || findEnvVar('AZURE_OPENAI_CHAT_DEPLOYMENT'))) {
      return await this.createAzureOpenAIChatClientWithApiKey();
    }
    else if (preferredProvider === 'openai' && 
            (await findSettingValue(KnownSettings.OpenAIApiKey) || findEnvVar('OPENAI_API_KEY'))) {
      return await this.createOpenAIChatClientWithApiKey();
    }

    console.log(`Preferred provider '${preferredProvider}' not available or missing required environment variables.`);
    return null;
  }

  /**
   * Try to create client from environment variables and config - matches C# TryCreateChatClientFromEnv exactly
   */
  private static async tryCreateChatClientFromEnv(): Promise<{ client: IChatClient; options: ChatOptions | null } | null> {
    // Try providers in the same order as C# implementation
    if (await findSettingValue(KnownSettings.GitHubToken) || findEnvVar('GITHUB_TOKEN')) {
      return await this.createCopilotChatClientWithGitHubToken();
    }
    else if (await findSettingValue(KnownSettings.AnthropicApiKey) || findEnvVar('ANTHROPIC_API_KEY')) {
      return await this.createAnthropicChatClientWithApiKey();
    }
    else if ((await findSettingValue(KnownSettings.AWSBedrockAccessKey) || findEnvVar('AWS_BEDROCK_ACCESS_KEY')) && 
            (await findSettingValue(KnownSettings.AWSBedrockSecretKey) || findEnvVar('AWS_BEDROCK_SECRET_KEY'))) {
      return await this.createAWSBedrockChatClient();
    }
    else if (await findSettingValue(KnownSettings.GoogleGeminiApiKey) || findEnvVar('GOOGLE_GEMINI_API_KEY')) {
      return await this.createGeminiChatClient();
    }
    else if (await findSettingValue(KnownSettings.GrokApiKey) || findEnvVar('GROK_API_KEY')) {
      return await this.createGrokChatClient();
    }
    else if ((await findSettingValue(KnownSettings.AzureOpenAIApiKey) || findEnvVar('AZURE_OPENAI_API_KEY')) && 
            (await findSettingValue(KnownSettings.AzureOpenAIEndpoint) || findEnvVar('AZURE_OPENAI_ENDPOINT')) &&
            (await findSettingValue(KnownSettings.AzureOpenAIChatDeployment) || findEnvVar('AZURE_OPENAI_CHAT_DEPLOYMENT'))) {
      return await this.createAzureOpenAIChatClientWithApiKey();
    }
    else if (await findSettingValue(KnownSettings.OpenAIApiKey) || findEnvVar('OPENAI_API_KEY')) {
      return await this.createOpenAIChatClientWithApiKey();
    }

    return null;
  }

  // Individual provider creation methods (matching C# method signatures)
  
  static async createAnthropicChatClientWithApiKey(): Promise<{ client: IChatClient; options: ChatOptions }> {
    const model = await findSettingValueWithDefault(KnownSettings.AnthropicModelName, 'claude-3-7-sonnet-latest');
    const apiKey = await findSettingValue(KnownSettings.AnthropicApiKey);
    
    if (!apiKey) {
      throw new Error('ANTHROPIC_API_KEY is not set.');
    }

    const client = new AnthropicChatClient(apiKey, model);
    const options: ChatOptions = {
      modelId: model,
      toolMode: 'auto',
      maxOutputTokens: 4000
    };

    console.log('Using Anthropic API key for authentication'); // Match C# debug output
    return { client, options };
  }

  static async createOpenAIChatClientWithApiKey(): Promise<{ client: IChatClient; options: ChatOptions }> {
    const apiKey = await findSettingValue(KnownSettings.OpenAIApiKey);
    const model = await findSettingValueWithDefault(KnownSettings.OpenAIModelName, 'gpt-4o');
    
    if (!apiKey) {
      throw new Error('OPENAI_API_KEY is not set.');
    }

    console.log('Using OpenAI API key for authentication'); // Match C# debug output
    const client = new OpenAIChatClient(apiKey, model);
    const options: ChatOptions = {
      modelId: model,
      toolMode: 'auto',
      maxOutputTokens: 4000
    };

    return { client, options };
  }

  static async createAzureOpenAIChatClientWithApiKey(): Promise<{ client: IChatClient; options: ChatOptions }> {
    const deployment = await findSettingValue(KnownSettings.AzureOpenAIChatDeployment);
    const endpoint = await findSettingValue(KnownSettings.AzureOpenAIEndpoint);
    const apiKey = await findSettingValue(KnownSettings.AzureOpenAIApiKey);

    if (!deployment) throw new Error('AZURE_OPENAI_CHAT_DEPLOYMENT is not set.');
    if (!endpoint) throw new Error('AZURE_OPENAI_ENDPOINT is not set.');
    if (!apiKey) throw new Error('AZURE_OPENAI_API_KEY is not set.');

    console.log('Using Azure OpenAI API key for authentication'); // Match C# debug output
    
    // For now, fall back to regular OpenAI client
    // TODO: Implement proper Azure OpenAI support
    console.log('Azure OpenAI support not yet implemented, falling back to OpenAI client');
    return await this.createOpenAIChatClientWithApiKey();
  }

  // Placeholder implementations for other providers (to be implemented later)
  static async createCopilotChatClientWithGitHubToken(): Promise<{ client: IChatClient; options: ChatOptions }> {
    console.log('GitHub Copilot support not yet implemented'); // Match C# debug output
    throw new Error('GitHub Copilot support not yet implemented');
  }

  static async createAWSBedrockChatClient(): Promise<{ client: IChatClient; options: ChatOptions }> {
    console.log('AWS Bedrock support not yet implemented'); // Match C# debug output
    throw new Error('AWS Bedrock support not yet implemented');
  }

  static async createGeminiChatClient(): Promise<{ client: IChatClient; options: ChatOptions }> {
    console.log('Google Gemini support not yet implemented'); // Match C# debug output
    throw new Error('Google Gemini support not yet implemented');
  }

  static async createGrokChatClient(): Promise<{ client: IChatClient; options: ChatOptions }> {
    console.log('Grok support not yet implemented'); // Match C# debug output
    throw new Error('Grok support not yet implemented');
  }

  // Legacy method for backwards compatibility
  static async createChatClient_Legacy(): Promise<IChatClient> {
    const result = await this.createChatClient();
    return result.client;
  }
}