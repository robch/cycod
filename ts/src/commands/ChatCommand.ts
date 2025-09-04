import { Command } from 'commander';
import { ChatMessage } from '../types';
import { ConsoleHelpers } from '../helpers/ConsoleHelpers';
import { FunctionCallingChat } from '../chat/FunctionCallingChat';
import { ChatClientFactory } from '../chat/ChatClientFactory';

export class ChatCommand {
  // Core properties matching C# implementation
  public systemPrompt?: string;
  public systemPromptAdds: string[] = [];
  public userPromptAdds: string[] = [];
  
  public maxPromptTokenTarget: number = 50000;  // DefaultMaxPromptTokenTarget
  public maxToolTokenTarget: number = 50000;    // DefaultMaxToolTokenTarget
  public maxOutputTokens?: number;
  public maxChatTokenTarget: number = 160000;   // DefaultMaxChatTokenTarget
  
  public loadMostRecentChatHistory: boolean = false;
  public inputChatHistory?: string;
  public outputChatHistory?: string;
  public outputTrajectory?: string;
  
  public autoSaveOutputChatHistory?: string;
  public autoSaveOutputTrajectory?: string;
  
  public inputInstructions: string[] = [];
  public useTemplates: boolean = true;
  
  public useMcps: string[] = [];
  public imagePatterns: string[] = [];
  public variables: Record<string, string> = {};
  
  // Private state matching C# implementation
  private assistantResponseCharsSinceLabel: number = 0;
  private assistantResponseNeedsLF: boolean = false;
  private totalTokensIn: number = 0;
  private totalTokensOut: number = 0;
  private approvedFunctionCallNames: Set<string> = new Set();
  private deniedFunctionCallNames: Set<string> = new Set();

  static createCommand(): Command {
    const chatCmd = new Command('chat');
    
    chatCmd
      .description('Start chat session with AI assistant')
      .option('--input <text>', 'Single input line or question')
      .option('--inputs <text...>', 'Chain multiple questions/requests')
      .option('-q, --question <text>', 'Shortcut for non-interactive, quiet mode')
      .option('--interactive [boolean]', 'Enable/disable interactive mode', true)
      .option('--quiet', 'Suppress verbose output')
      .option('--output-chat-history [filename]', 'Save conversation')
      .option('--input-chat-history <filename>', 'Load previous conversation')
      .option('--chat-history <filename>', 'Use same filename for input and output')
      .option('--continue', 'Resume most recent chat history')
      .option('--system-prompt <text>', 'Completely replace default system behavior')
      .option('--add-system-prompt <text>', 'Augment existing default system prompt')
      .option('--var <name=value>', 'Define a variable', (value, previous: Record<string, string>) => {
        const vars = previous || {};
        const [name, val] = value.split('=', 2);
        if (name && val !== undefined) {
          vars[name] = val;
        }
        return vars;
      }, {} as Record<string, string>)
      .option('--vars <name=value...>', 'Define multiple variables')
      .option('--foreach <spec>', 'Run command with different variable values in parallel')
      .option('--threads <number>', 'Specify maximum parallel processes', parseInt)
      .option('--profile <name>', 'Load specific configuration profile')
      .option('--max-output-tokens <number>', 'Maximum output tokens', parseInt)
      .action(async (options, command) => {
        const chatCommand = new ChatCommand();
        await chatCommand.execute(options, command);
      });

    return chatCmd;
  }

  async execute(commandOptions: any, command: Command): Promise<number> {
    try {
      // Process options and set properties
      this.processOptions(commandOptions);
      
      // Determine if we're in interactive mode
      const hasInputInstructions = this.inputInstructions.length > 0;
      const interactive = commandOptions.interactive && !hasInputInstructions;
      
      // Check if we have input when not in interactive mode
      if (!interactive && !hasInputInstructions) {
        ConsoleHelpers.writeWarning('\nNo input instructions provided. Exiting.');
        return 1;
      }

      // Create the chat client and function calling chat (matching C# ChatCommand.cs:99-100)
      const { client: chatClient, options: clientOptions } = await ChatClientFactory.createChatClient();
      const systemPrompt = this.groundSystemPrompt();
      const chat = new FunctionCallingChat(chatClient, systemPrompt, this.maxOutputTokens);

      // Load chat history if specified
      if (this.inputChatHistory) {
        await chat.loadChatHistory(
          this.inputChatHistory,
          this.maxPromptTokenTarget,
          this.maxToolTokenTarget,
          this.maxChatTokenTarget
        );
      }

      // Add user prompt messages
      if (this.userPromptAdds.length > 0) {
        chat.addUserMessages(
          this.userPromptAdds,
          this.maxPromptTokenTarget,
          this.maxChatTokenTarget
        );
      }

      // Main chat loop - matching C# while loop structure
      while (true) {
        this.displayUserPrompt();
        
        const userPrompt = interactive 
          ? await this.interactivelyReadLineOrSimulateInput(this.inputInstructions, 'exit')
          : this.readLineOrSimulateInput(this.inputInstructions, 'exit');
          
        if (!userPrompt || userPrompt.trim() === '' || userPrompt === 'exit') {
          break;
        }

        // Handle chat commands (slash commands)
        const { skipAssistant, replaceUserPrompt } = await this.tryHandleChatCommand(chat, userPrompt);
        if (skipAssistant) {
          continue;
        }

        const shouldReplaceUserPrompt = !!replaceUserPrompt;
        if (shouldReplaceUserPrompt) {
          this.displayPromptReplacement(userPrompt, replaceUserPrompt!);
        }

        const giveAssistant = shouldReplaceUserPrompt ? replaceUserPrompt! : userPrompt;

        this.displayAssistantLabel();
        
        // Complete chat with streaming
        const response = await this.completeChatStreamingAsync(
          chat,
          giveAssistant,
          [],  // imageFiles - empty for now
          (messages) => this.handleUpdateMessages(messages),
          (update) => this.handleStreamingChatCompletionUpdate(update)
        );
        
        ConsoleHelpers.writeLine('\n', true);
      }

      return 0;
    } catch (error) {
      console.error('Chat command error:', error);
      return 1;
    }
  }

  private processOptions(options: any): void {
    // Process input instructions
    if (options.input) {
      this.inputInstructions.push(options.input);
    }
    if (options.inputs) {
      this.inputInstructions.push(...options.inputs);
    }
    if (options.question) {
      this.inputInstructions.push(options.question);
    }

    // Process system prompt options
    if (options.systemPrompt) {
      this.systemPrompt = options.systemPrompt;
    }
    if (options.addSystemPrompt) {
      this.systemPromptAdds.push(options.addSystemPrompt);
    }

    // Process chat history options
    if (options.inputChatHistory) {
      this.inputChatHistory = options.inputChatHistory;
    }
    if (options.outputChatHistory) {
      this.outputChatHistory = options.outputChatHistory;
    }
    if (options.chatHistory) {
      this.inputChatHistory = options.chatHistory;
      this.outputChatHistory = options.chatHistory;
    }
    if (options.continue) {
      this.loadMostRecentChatHistory = true;
    }

    // Process variables
    if (options.var) {
      this.variables = { ...this.variables, ...options.var };
    }

    // Process token limits
    if (options.maxOutputTokens) {
      this.maxOutputTokens = options.maxOutputTokens;
    }
  }

  private groundSystemPrompt(): string {
    let systemPrompt = this.systemPrompt || this.getBuiltInSystemPrompt();
    
    // Add system prompt additions
    if (this.systemPromptAdds.length > 0) {
      const additions = this.systemPromptAdds.join('\n\n');
      systemPrompt = `${systemPrompt}\n\n${additions}`;
    }

    // Process templates if enabled
    if (this.useTemplates) {
      systemPrompt = this.processTemplate(systemPrompt);
    }

    return systemPrompt.trim();
  }

  private getBuiltInSystemPrompt(): string {
    return 'You are a helpful AI assistant.';
  }

  private processTemplate(template: string): string {
    if (!template) return template;
    
    // Simple variable substitution - replace {variableName} with values
    let processed = template;
    for (const [name, value] of Object.entries(this.variables)) {
      const pattern = new RegExp(`\\{${name}\\}`, 'g');
      processed = processed.replace(pattern, value);
    }
    
    // Built-in variables
    const now = new Date();
    processed = processed.replace(/\{date\}/g, now.toISOString().split('T')[0]);
    processed = processed.replace(/\{time\}/g, now.toTimeString().split(' ')[0]);
    processed = processed.replace(/\{year\}/g, now.getFullYear().toString());
    processed = processed.replace(/\{month\}/g, (now.getMonth() + 1).toString().padStart(2, '0'));
    processed = processed.replace(/\{day\}/g, now.getDate().toString().padStart(2, '0'));
    
    return processed;
  }

  private async tryHandleChatCommand(chat: FunctionCallingChat, userPrompt: string): Promise<{ skipAssistant: boolean; replaceUserPrompt?: string }> {
    if (userPrompt.startsWith('/save')) {
      const fileName = userPrompt.substring('/save'.length).trim();
      return { skipAssistant: this.handleSaveChatHistoryCommand(chat, fileName) };
    } else if (userPrompt === '/clear') {
      return { skipAssistant: this.handleClearChatHistoryCommand(chat) };
    } else if (userPrompt === '/cost') {
      return { skipAssistant: this.handleShowCostCommand() };
    } else if (userPrompt === '/help') {
      return { skipAssistant: this.handleHelpCommand() };
    }

    return { skipAssistant: false };
  }

  private handleSaveChatHistoryCommand(chat: FunctionCallingChat, fileName?: string): boolean {
    const useDefaultFileName = !fileName || fileName.trim() === '';
    const finalFileName = useDefaultFileName ? 'chat-history.jsonl' : fileName;

    ConsoleHelpers.writeWithColor(`Saving ${finalFileName} ...`, 'yellow', true);
    chat.saveChatHistoryToFile(finalFileName);
    ConsoleHelpers.writeWithColor('Saved!\n', 'yellow', true);

    return true;
  }

  private handleClearChatHistoryCommand(chat: FunctionCallingChat): boolean {
    chat.clearChatHistory();
    this.totalTokensIn = 0;
    this.totalTokensOut = 0;
    ConsoleHelpers.writeWithColor('Cleared chat history.\n', 'yellow', true);
    return true;
  }

  private handleShowCostCommand(): boolean {
    ConsoleHelpers.writeWithColor(`Tokens: ${this.totalTokensIn} in, ${this.totalTokensOut} out\n`, 'yellow', true);
    return true;
  }

  private handleHelpCommand(): boolean {
    const helpText = `
  BUILT-IN

    /save     Save chat history to file
    /clear    Clear chat history
    /cost     Show token usage statistics
    /help     Show this help message

  EXTERNAL

    /files    List files matching pattern
    /file     Get contents of a file
    /find     Find content in files

    /search   Search the web
    /get      Get content from URL

    /run      Run a command
    /image    Add image file(s) to conversation

  PROMPTS

    No custom prompts found.

`;
    ConsoleHelpers.writeLine(helpText, true);
    return true;
  }

  private readLineOrSimulateInput(inputInstructions: string[], defaultOnEndOfInput?: string): string | null {
    if (inputInstructions.length > 0) {
      const input = inputInstructions.shift();
      if (input && input.trim() !== '') {
        ConsoleHelpers.writeLine(input);
        return input;
      }
    }
    return defaultOnEndOfInput || null;
  }

  private async interactivelyReadLineOrSimulateInput(inputInstructions: string[], defaultOnEndOfInput?: string): Promise<string | null> {
    const input = this.readLineOrSimulateInput(inputInstructions);
    if (input !== null) return input;

    // For interactive mode, we'd use readline here
    // For now, return the default
    return defaultOnEndOfInput || null;
  }

  private displayUserPrompt(): void {
    ConsoleHelpers.writeWithColor('\rUser: ', 'green');
  }

  private displayPromptReplacement(userPrompt: string, replaceUserPrompt: string): void {
    ConsoleHelpers.writeWithColor(`\rUser: ${userPrompt} => ${replaceUserPrompt}`, 'gray', true);
  }

  private displayAssistantLabel(): void {
    ConsoleHelpers.writeWithColor('\nAssistant: ', 'green');
    this.assistantResponseCharsSinceLabel = 0;
  }

  private displayAssistantResponse(text: string): void {
    if (this.assistantResponseCharsSinceLabel === 0 && text.startsWith('\n')) {
      text = text.trimStart();
    }

    ConsoleHelpers.writeWithColor(text, 'white', true);

    this.assistantResponseCharsSinceLabel += text.length;
    this.assistantResponseNeedsLF = !text.endsWith('\n');
  }

  private async completeChatStreamingAsync(
    chat: FunctionCallingChat,
    userPrompt: string,
    imageFiles: string[],
    messageCallback?: (messages: ChatMessage[]) => void,
    streamingCallback?: (update: any) => void
  ): Promise<string> {
    try {
      return await chat.completeChatStreamingAsync(
        userPrompt,
        imageFiles,
        messageCallback,
        streamingCallback
      );
    } catch (error) {
      console.error('Error during chat completion:', error);
      throw error;
    }
  }

  private handleUpdateMessages(messages: ChatMessage[]): void {
    // Handle auto-save and message updates
    // For now, just track the messages
  }

  private handleStreamingChatCompletionUpdate(update: any): void {
    // Extract token usage information if available
    if (update.usage) {
      this.totalTokensIn += update.usage.input_tokens || 0;
      this.totalTokensOut += update.usage.output_tokens || 0;
    }

    // Extract and display text content
    if (update.delta?.text || update.content) {
      const text = update.delta?.text || update.content || '';
      this.displayAssistantResponse(text);
    }
  }
}