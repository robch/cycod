import { ChatMessage } from '../types';
import { ChatHistoryHelpers } from '../helpers/ChatHistoryHelpers';

// Interface for different AI clients
export interface IChatClient {
  createStreamingResponse(messages: ChatMessage[]): AsyncIterableIterator<any>;
  createResponse(messages: ChatMessage[]): Promise<any>;
}

export interface StreamingCallbacks {
  messageCallback?: (messages: ChatMessage[]) => void;
  streamingCallback?: (update: any) => void;
  approveFunctionCall?: (name: string, args?: string) => boolean;
  functionCallCallback?: (name: string, args: string, result: any) => void;
}

export class FunctionCallingChat {
  private messages: ChatMessage[] = [];
  private systemPrompt: string;
  private chatClient: IChatClient;
  private maxOutputTokens?: number;
  private userMessageAdds: ChatMessage[] = [];

  constructor(
    chatClient: IChatClient, 
    systemPrompt: string, 
    maxOutputTokens?: number
  ) {
    this.chatClient = chatClient;
    this.systemPrompt = systemPrompt;
    this.maxOutputTokens = maxOutputTokens;
    this.clearChatHistory();
  }

  clearChatHistory(): void {
    this.messages = [];
    this.messages.push({ role: 'system', content: this.systemPrompt });
    this.messages.push(...this.userMessageAdds);
  }

  addUserMessage(
    userMessage: string, 
    maxPromptTokenTarget?: number, 
    maxChatTokenTarget?: number
  ): void {
    this.userMessageAdds.push({ role: 'user', content: userMessage });
    
    // Apply token trimming if targets are specified
    if (maxPromptTokenTarget || maxChatTokenTarget) {
      this.tryTrimToTarget(this.userMessageAdds, maxPromptTokenTarget, maxChatTokenTarget);
    }

    this.messages.push({ role: 'user', content: userMessage });
    
    // Apply token trimming if targets are specified
    if (maxPromptTokenTarget || maxChatTokenTarget) {
      this.tryTrimToTarget(this.messages, maxPromptTokenTarget, maxChatTokenTarget);
    }
  }

  addUserMessages(
    userMessages: string[], 
    maxPromptTokenTarget?: number, 
    maxChatTokenTarget?: number
  ): void {
    for (const userMessage of userMessages) {
      this.addUserMessage(userMessage, maxPromptTokenTarget);
    }

    if (maxChatTokenTarget) {
      this.tryTrimToTarget(this.messages, undefined, maxChatTokenTarget);
    }
  }

  async loadChatHistory(
    fileName: string, 
    maxPromptTokenTarget?: number, 
    maxToolTokenTarget?: number, 
    maxChatTokenTarget?: number
  ): Promise<void> {
    const historyMessages = await ChatHistoryHelpers.readChatHistoryFromFile(fileName);
    
    // Check if we have a system message in the history
    const hasSystemMessage = historyMessages.some(m => m.role === 'system');
    if (hasSystemMessage) {
      this.messages = [];
    }

    this.messages.push(...historyMessages);
    this.fixDanglingToolCalls();
    this.tryTrimToTarget(
      this.messages, 
      maxPromptTokenTarget, 
      maxChatTokenTarget, 
      maxToolTokenTarget
    );
  }

  saveChatHistoryToFile(fileName: string): void {
    ChatHistoryHelpers.saveChatHistoryToFile(this.messages, fileName);
  }

  async completeChatStreamingAsync(
    userPrompt: string,
    imageFiles: string[] = [],
    messageCallback?: (messages: ChatMessage[]) => void,
    streamingCallback?: (update: any) => void,
    approveFunctionCall?: (name: string, args?: string) => boolean,
    functionCallCallback?: (name: string, args: string, result: any) => void
  ): Promise<string> {
    // Add user message with images support (simplified for now)
    const message = this.createUserMessageWithImages(userPrompt, imageFiles);
    this.messages.push(message);
    messageCallback?.(this.messages);

    let contentToReturn = '';
    
    while (true) {
      let responseContent = '';
      
      try {
        // Stream the response from the AI client
        for await (const update of this.chatClient.createStreamingResponse(this.messages)) {
          // Extract text content from the update
          const content = this.extractTextFromUpdate(update);
          
          if (content) {
            responseContent += content;
            contentToReturn += content;
          }

          // Call the streaming callback
          streamingCallback?.(update);
        }

        // Try to handle any function calls
        const shouldContinue = await this.tryCallFunctions(
          responseContent,
          approveFunctionCall,
          functionCallCallback,
          messageCallback
        );

        if (shouldContinue) {
          continue; // Continue the loop for function call responses
        }

        // Add the assistant's response to messages
        this.messages.push({ role: 'assistant', content: responseContent });
        messageCallback?.(this.messages);

        return contentToReturn;
      } catch (error) {
        console.error('Error in chat streaming:', error);
        throw error;
      }
    }
  }

  private createUserMessageWithImages(userPrompt: string, imageFiles: string[]): ChatMessage {
    // For now, just return text message. Image support can be added later
    return { role: 'user', content: userPrompt };
  }

  private extractTextFromUpdate(update: any): string {
    // Different AI providers have different response formats
    // This is a simplified version - we'll enhance this when we implement specific clients
    
    // Anthropic format
    if (update.delta?.text) {
      return update.delta.text;
    }
    
    // OpenAI format
    if (update.choices?.[0]?.delta?.content) {
      return update.choices[0].delta.content;
    }
    
    // Generic content field
    if (update.content) {
      return update.content;
    }
    
    return '';
  }

  private async tryCallFunctions(
    responseContent: string,
    approveFunctionCall?: (name: string, args?: string) => boolean,
    functionCallCallback?: (name: string, args: string, result: any) => void,
    messageCallback?: (messages: ChatMessage[]) => void
  ): Promise<boolean> {
    // Function calling logic would go here
    // For now, we'll implement a basic version
    
    // Check if the response contains function calls
    // This is a simplified implementation - real function calling is more complex
    
    return false; // No function calls to handle
  }

  private fixDanglingToolCalls(): void {
    // Remove any incomplete tool call sequences
    // This matches the C# behavior for cleaning up chat history
    
    for (let i = this.messages.length - 1; i >= 0; i--) {
      const message = this.messages[i];
      
      // Look for assistant messages that might have incomplete tool calls
      if (message.role === 'assistant' && message.content.includes('function_call')) {
        // Check if there's a corresponding tool result
        const hasToolResult = i + 1 < this.messages.length && 
          this.messages[i + 1].role === 'user' &&
          this.messages[i + 1].content.includes('function_result');
        
        if (!hasToolResult) {
          // Remove the dangling function call
          this.messages.splice(i, 1);
        }
      }
    }
  }

  private tryTrimToTarget(
    messages: ChatMessage[], 
    maxPromptTokenTarget?: number, 
    maxChatTokenTarget?: number, 
    maxToolTokenTarget?: number
  ): void {
    // Token counting and trimming logic
    // For now, implement a basic version based on character count
    // Real implementation would use proper tokenization
    
    if (!maxPromptTokenTarget && !maxChatTokenTarget && !maxToolTokenTarget) {
      return;
    }

    // Rough approximation: 1 token ≈ 4 characters
    const approximateTokenCount = (text: string) => Math.ceil(text.length / 4);
    
    let totalTokens = 0;
    for (const message of messages) {
      totalTokens += approximateTokenCount(message.content);
    }

    const maxTokens = maxChatTokenTarget || maxPromptTokenTarget || 100000;
    
    // If we're over the limit, trim from the middle (keep system prompt and recent messages)
    while (totalTokens > maxTokens && messages.length > 2) {
      // Find the oldest non-system message to remove
      let indexToRemove = -1;
      for (let i = 1; i < messages.length - 1; i++) {
        if (messages[i].role !== 'system') {
          indexToRemove = i;
          break;
        }
      }
      
      if (indexToRemove === -1) break;
      
      const removedMessage = messages[indexToRemove];
      totalTokens -= approximateTokenCount(removedMessage.content);
      messages.splice(indexToRemove, 1);
    }
  }

  // Getter for messages (for testing/debugging)
  getMessages(): ChatMessage[] {
    return [...this.messages];
  }

  // Get message count
  getMessageCount(): number {
    return this.messages.length;
  }
}