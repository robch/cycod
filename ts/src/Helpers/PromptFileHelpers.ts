import * as fs from 'fs';
import * as path from 'path';
import { ConfigFileScope } from '../Configuration/ConfigFileScope';
import { PathHelpers } from './PathHelpers';

/**
 * Provides methods for working with prompt files across different scopes.
 */
export class PromptFileHelpers {
  
  /**
   * Finds the prompt directory in the specified scope.
   */
  static findPromptDirectoryInScope(scope: ConfigFileScope, create: boolean = false): string | null {
    const scopeDir = PathHelpers.getScopeDirectoryPath(scope);
    if (!scopeDir) return null;
    
    const promptDir = path.join(scopeDir, 'prompts');
    
    if (create && !fs.existsSync(promptDir)) {
      fs.mkdirSync(promptDir, { recursive: true });
    }
    
    return fs.existsSync(promptDir) ? promptDir : (create ? promptDir : null);
  }

  /**
   * Saves a prompt with the provided text to a file in the specified scope.
   */
  static savePrompt(promptName: string, promptText: string, scope: ConfigFileScope = ConfigFileScope.Local): string {
    const promptDirectory = this.findPromptDirectoryInScope(scope, true)!;
    const fileName = path.join(promptDirectory, promptName + '.prompt');
    fs.writeFileSync(fileName, promptText, 'utf8');
    return fileName;
  }

  /**
   * Finds a prompt file across scopes (local, user, global) or in a specific scope.
   */
  static findPromptFile(promptName: string, scope: ConfigFileScope = ConfigFileScope.Any): string | null {
    const promptFileName = `${promptName}.prompt`;
    
    if (scope === ConfigFileScope.Any) {
      // Search in order: local, user, global
      const scopes = [ConfigFileScope.Local, ConfigFileScope.User, ConfigFileScope.Global];
      
      for (const searchScope of scopes) {
        const promptDir = this.findPromptDirectoryInScope(searchScope);
        if (promptDir) {
          const promptFilePath = path.join(promptDir, promptFileName);
          if (fs.existsSync(promptFilePath)) {
            return promptFilePath;
          }
        }
      }
      
      return null;
    } else {
      // Search in specific scope
      const promptDir = this.findPromptDirectoryInScope(scope);
      if (!promptDir) return null;
      
      const promptFilePath = path.join(promptDir, promptFileName);
      return fs.existsSync(promptFilePath) ? promptFilePath : null;
    }
  }

  /**
   * Gets the text content of a prompt file.
   */
  static getPromptText(promptName: string, scope: ConfigFileScope = ConfigFileScope.Any): string | null {
    const promptFilePath = this.findPromptFile(promptName, scope);
    if (!promptFilePath || !fs.existsSync(promptFilePath)) {
      return null;
    }

    let content = fs.readFileSync(promptFilePath, 'utf8').trim();
    
    // If content starts with @ symbol, it's a reference to another file
    if (content.startsWith('@')) {
      const referencedFilePath = content.substring(1);
      if (fs.existsSync(referencedFilePath)) {
        return fs.readFileSync(referencedFilePath, 'utf8');
      }
    }
    
    return content;
  }

  /**
   * Gets the scope from a file path.
   */
  static getScopeFromPath(filePath: string): ConfigFileScope {
    if (filePath.includes('/.cycod/')) return ConfigFileScope.Local;
    if (filePath.includes('/.local/share/.cycod/')) return ConfigFileScope.Global;
    return ConfigFileScope.User; // Default assumption
  }

  /**
   * Lists all prompt files in a specific scope.
   */
  static listPromptsInScope(scope: ConfigFileScope): string[] {
    const promptDir = this.findPromptDirectoryInScope(scope);
    if (!promptDir || !fs.existsSync(promptDir)) {
      return [];
    }

    return fs.readdirSync(promptDir)
      .filter(file => file.endsWith('.prompt'))
      .map(file => path.basename(file, '.prompt'))
      .sort();
  }
}