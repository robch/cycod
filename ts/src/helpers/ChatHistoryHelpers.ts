import * as fs from 'fs-extra';
import * as path from 'path';
import { ChatMessage } from '../types';

export class ChatHistoryHelpers {
  /**
   * Finds the most recent chat history file across all scopes.
   * Matches C# ChatHistoryFileHelpers.FindMostRecentChatHistoryFile()
   */
  static findMostRecentChatHistoryFile(): string | null {
    const potentialDirectories = [
      process.cwd(), // Current directory
      path.join(process.cwd(), '.cycod', 'history'), // Local scope history
      path.join(require('os').homedir(), '.cycod', 'history'), // User scope history
    ];

    const historyFiles: Array<{ path: string; mtime: Date }> = [];

    for (const dir of potentialDirectories) {
      if (fs.existsSync(dir)) {
        try {
          const files = fs.readdirSync(dir);
          const chatHistoryFiles = files.filter(f => 
            f.startsWith('chat-history') && f.endsWith('.jsonl') ||
            f.startsWith('exception-chat-history') && f.endsWith('.jsonl')
          );

          for (const file of chatHistoryFiles) {
            const fullPath = path.join(dir, file);
            const stats = fs.statSync(fullPath);
            historyFiles.push({
              path: fullPath,
              mtime: stats.mtime
            });
          }
        } catch (error) {
          // Ignore directories we can't read
          continue;
        }
      }
    }

    // Sort by modification time (most recent first)
    historyFiles.sort((a, b) => b.mtime.getTime() - a.mtime.getTime());

    const mostRecent = historyFiles[0]?.path || null;
    if (mostRecent) {
      console.log(`Loading: ${mostRecent}\n`); // Match C# output format
    }

    return mostRecent;
  }

  /**
   * Saves chat history to a JSONL file.
   * Matches C# ChatMessageHelpers.SaveChatHistoryToFile()
   */
  static saveChatHistoryToFile(messages: ChatMessage[], fileName: string): void {
    try {
      const jsonl = this.messagesToJsonl(messages);
      
      // Ensure directory exists
      const dir = path.dirname(fileName);
      if (dir !== '.' && !fs.existsSync(dir)) {
        fs.mkdirSync(dir, { recursive: true });
      }

      fs.writeFileSync(fileName, jsonl, 'utf8');
    } catch (error) {
      console.error(`Warning: Failed to save chat history to '${fileName}': ${error}`);
      
      // Try saving to user profile directory as fallback
      // This matches the C# behavior with saveToFolderOnAccessDenied
      const userProfileFolder = require('os').homedir();
      const fallbackFolder = path.join(userProfileFolder, '.cycod', 'exceptions');
      
      try {
        fs.ensureDirSync(fallbackFolder);
        const fallbackPath = path.join(fallbackFolder, path.basename(fileName));
        fs.writeFileSync(fallbackPath, this.messagesToJsonl(messages), 'utf8');
        console.log(`Saved to fallback location: ${fallbackPath}`);
      } catch (fallbackError) {
        console.error(`Failed to save to fallback location: ${fallbackError}`);
        throw error;
      }
    }
  }

  /**
   * Reads chat history from a JSONL file.
   * Matches C# ChatMessageHelpers.ReadChatHistoryFromFile()
   */
  static async readChatHistoryFromFile(fileName: string): Promise<ChatMessage[]> {
    try {
      if (!fs.existsSync(fileName)) {
        throw new Error(`Chat history file not found: ${fileName}`);
      }

      const jsonl = fs.readFileSync(fileName, 'utf8');
      return this.jsonlToMessages(jsonl);
    } catch (error) {
      console.error(`Error reading chat history from '${fileName}': ${error}`);
      throw error;
    }
  }

  /**
   * Converts messages to JSONL format.
   * Matches C# ChatMessageHelpers.AsJsonl()
   */
  static messagesToJsonl(messages: ChatMessage[]): string {
    const jsonLines = messages
      .map(message => this.messageToJson(message))
      .filter(json => json !== null)
      .map(json => json!);

    return jsonLines.join('\n');
  }

  /**
   * Converts JSONL to messages.
   * Matches C# AIExtensionsChatHelpers.ChatMessagesFromJsonl()
   */
  static jsonlToMessages(jsonl: string): ChatMessage[] {
    const messages: ChatMessage[] = [];
    
    const lines = jsonl.split(/\r?\n/);
    for (const line of lines) {
      if (line.trim() === '') continue;

      try {
        const message = this.jsonToMessage(line);
        if (message) {
          messages.push(message);
        }
      } catch (error) {
        console.warn(`Skipping invalid JSON line: ${line}`);
        continue;
      }
    }

    return messages;
  }

  /**
   * Converts a single message to JSON.
   * Matches C# ChatMessage.AsJson()
   */
  private static messageToJson(message: ChatMessage): string | null {
    try {
      return JSON.stringify(message);
    } catch (error) {
      console.warn(`Failed to serialize message: ${error}`);
      return null;
    }
  }

  /**
   * Converts JSON to a single message.
   * Matches C# AIExtensionsChatHelpers.ChatMessageFromJson()
   */
  private static jsonToMessage(json: string): ChatMessage | null {
    try {
      const parsed = JSON.parse(json);
      
      // Validate the message structure
      if (!parsed.role || !parsed.content) {
        console.warn(`Invalid message structure: missing role or content`);
        return null;
      }

      if (!['system', 'user', 'assistant'].includes(parsed.role)) {
        console.warn(`Invalid message role: ${parsed.role}`);
        return null;
      }

      return {
        role: parsed.role,
        content: parsed.content
      };
    } catch (error) {
      console.warn(`Error parsing JSON message: ${error}`);
      return null;
    }
  }

  /**
   * Generates a default chat history filename with timestamp.
   * Matches C# behavior for auto-generated filenames.
   */
  static generateChatHistoryFileName(): string {
    const timestamp = new Date().toISOString()
      .replace(/[:.]/g, '-')
      .replace('T', '-')
      .substring(0, 19);
    return `chat-history-${timestamp}.jsonl`;
  }

  /**
   * Ensures the chat history directory exists in the specified scope.
   * Matches C# ChatHistoryFileHelpers behavior.
   */
  static ensureChatHistoryDirectoryExists(scope: 'local' | 'user' | 'global' = 'local'): string {
    let historyDir: string;

    switch (scope) {
      case 'global':
        // Global scope - system-wide directory
        historyDir = path.join('/etc', 'cycod', 'history');
        break;
      case 'user':
        // User scope - user's home directory
        historyDir = path.join(require('os').homedir(), '.cycod', 'history');
        break;
      case 'local':
      default:
        // Local scope - current directory
        historyDir = path.join(process.cwd(), '.cycod', 'history');
        break;
    }

    try {
      fs.ensureDirSync(historyDir);
    } catch (error) {
      console.warn(`Warning: Could not create history directory ${historyDir}: ${error}`);
      // Fall back to current directory
      historyDir = process.cwd();
    }

    return historyDir;
  }

  /**
   * Gets the full path for a chat history file, creating directories as needed.
   * Matches C# FileHelpers.GetFileNameFromTemplate behavior.
   */
  static getFullChatHistoryPath(fileName: string, scope: 'local' | 'user' | 'global' = 'local'): string {
    // If it's already an absolute path, use it as-is
    if (path.isAbsolute(fileName)) {
      return fileName;
    }

    // If it's just a filename, put it in the appropriate scope directory
    if (fileName === path.basename(fileName)) {
      const historyDir = this.ensureChatHistoryDirectoryExists(scope);
      return path.join(historyDir, fileName);
    }

    // If it's a relative path, resolve it relative to current directory
    return path.resolve(fileName);
  }
}