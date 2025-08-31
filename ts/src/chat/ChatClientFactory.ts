import Anthropic from '@anthropic-ai/sdk';
import OpenAI from 'openai';
import { IChatClient } from './FunctionCallingChat';
import { ChatMessage } from '../types';

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

// Factory class matching C# ChatClientFactory
export class ChatClientFactory {
  static createChatClient(): IChatClient {
    // Check environment variables to determine which client to create
    // This matches the C# implementation pattern
    
    const anthropicApiKey = process.env.ANTHROPIC_API_KEY;
    if (anthropicApiKey) {
      const model = process.env.ANTHROPIC_MODEL_NAME || 'claude-3-5-sonnet-20241022';
      console.log('Using Anthropic API key for authentication'); // Match C# debug output
      return new AnthropicChatClient(anthropicApiKey, model);
    }

    const openaiApiKey = process.env.OPENAI_API_KEY;
    if (openaiApiKey) {
      const model = process.env.OPENAI_MODEL_NAME || 'gpt-4o';
      console.log('Using OpenAI API key for authentication'); // Match C# debug output
      return new OpenAIChatClient(openaiApiKey, model);
    }

    // Azure OpenAI support
    const azureEndpoint = process.env.AZURE_OPENAI_ENDPOINT;
    const azureApiKey = process.env.AZURE_OPENAI_API_KEY;
    const azureDeployment = process.env.AZURE_OPENAI_CHAT_DEPLOYMENT;
    
    if (azureEndpoint && azureApiKey && azureDeployment) {
      console.log('Using Azure OpenAI API key for authentication'); // Match C# debug output
      return this.createAzureOpenAIChatClient(azureEndpoint, azureApiKey, azureDeployment);
    }

    // GitHub Copilot support (if available)
    const githubToken = process.env.GITHUB_TOKEN;
    if (githubToken) {
      console.log('Using GitHub Copilot for authentication'); // Match C# debug output
      // GitHub Copilot client would be implemented here
      // For now, fall back to a default
    }

    throw new Error('No AI provider configuration found. Please set ANTHROPIC_API_KEY, OPENAI_API_KEY, or Azure OpenAI environment variables.');
  }

  private static createAzureOpenAIChatClient(endpoint: string, apiKey: string, deployment: string): IChatClient {
    // For now, fall back to regular OpenAI client
    // TODO: Implement proper Azure OpenAI support
    console.log('Azure OpenAI support not yet implemented, falling back to OpenAI client');
    return new OpenAIChatClient(apiKey);
  }

  // Static method to create Anthropic client specifically (matching C# pattern)
  static createAnthropicChatClientWithApiKey(): { client: IChatClient; options: any } {
    const model = process.env.ANTHROPIC_MODEL_NAME || 'claude-3-7-sonnet-latest';
    const apiKey = process.env.ANTHROPIC_API_KEY;
    
    if (!apiKey) {
      throw new Error('ANTHROPIC_API_KEY is not set.');
    }

    const client = new AnthropicChatClient(apiKey, model);
    const options = {
      modelId: model,
      toolMode: 'auto',
      maxOutputTokens: 4000
    };

    console.log('Using Anthropic API key for authentication'); // Match C# debug output
    return { client, options };
  }

  // Static method to create OpenAI client specifically (matching C# pattern)
  static createOpenAIChatClientWithApiKey(): IChatClient {
    const apiKey = process.env.OPENAI_API_KEY;
    
    if (!apiKey) {
      throw new Error('OPENAI_API_KEY is not set.');
    }

    console.log('Using OpenAI API key for authentication'); // Match C# debug output
    return new OpenAIChatClient(apiKey);
  }
}