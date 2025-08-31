import * as fs from 'fs-extra';
import * as path from 'path';
import * as os from 'os';
import { ConfigFileScope } from '../types';

export class PromptStore {
  private static readonly PROMPTS_DIR_NAME = '.cycod';
  private static readonly PROMPTS_SUBDIR = 'prompts';

  private getPromptsDirectory(scope: ConfigFileScope): string {
    switch (scope) {
      case ConfigFileScope.Local:
        return path.join(process.cwd(), PromptStore.PROMPTS_DIR_NAME, PromptStore.PROMPTS_SUBDIR);
      case ConfigFileScope.User:
        return path.join(os.homedir(), PromptStore.PROMPTS_DIR_NAME, PromptStore.PROMPTS_SUBDIR);
      case ConfigFileScope.Global:
        // For now, use a system-wide location. In production, this might be /etc/cycod or similar
        return path.join('/tmp', 'cycod-global', PromptStore.PROMPTS_SUBDIR);
      default:
        throw new Error(`Invalid scope: ${scope}`);
    }
  }

  private getPromptPath(name: string, scope: ConfigFileScope): string {
    return path.join(this.getPromptsDirectory(scope), `${name}.prompt`);
  }

  private validatePromptName(name: string): void {
    // Check for invalid characters in prompt name
    if (name.includes('/') || name.includes('\\') || name.includes('..') || name.includes('\0')) {
      throw new Error(`Invalid prompt name: ${name}. Prompt names cannot contain path separators or special characters.`);
    }
    
    if (name.trim() === '') {
      throw new Error('Prompt name cannot be empty.');
    }
  }

  async ensurePromptsDirectory(scope: ConfigFileScope): Promise<void> {
    const promptsDir = this.getPromptsDirectory(scope);
    await fs.ensureDir(promptsDir);
  }

  async findPromptFile(name: string, scope: ConfigFileScope = ConfigFileScope.Any): Promise<{ path: string, scope: ConfigFileScope } | null> {
    // Remove leading slash if present (matching C# behavior)
    if (name.startsWith('/')) {
      name = name.substring(1);
    }

    if (scope !== ConfigFileScope.Any) {
      const promptPath = this.getPromptPath(name, scope);
      if (await fs.pathExists(promptPath)) {
        return { path: promptPath, scope };
      }
      return null;
    }

    // Check in priority order: Local > User > Global
    const scopes = [ConfigFileScope.Local, ConfigFileScope.User, ConfigFileScope.Global];
    for (const s of scopes) {
      const promptPath = this.getPromptPath(name, s);
      if (await fs.pathExists(promptPath)) {
        return { path: promptPath, scope: s };
      }
    }
    
    return null;
  }

  async createPrompt(name: string, content: string, scope: ConfigFileScope = ConfigFileScope.Local): Promise<string> {
    this.validatePromptName(name);
    
    // Remove leading slash if present (matching C# behavior) 
    if (name.startsWith('/')) {
      name = name.substring(1);
    }
    
    // Check if prompt already exists in any scope
    const existingPrompt = await this.findPromptFile(name);
    if (existingPrompt) {
      throw new Error(`Error: Prompt '${name}' already exists.`);
    }
    
    await this.ensurePromptsDirectory(scope);
    const promptPath = this.getPromptPath(name, scope);
    
    await fs.writeFile(promptPath, content, 'utf8');
    return promptPath;
  }

  async createPromptFromFile(name: string, filePath: string, scope: ConfigFileScope = ConfigFileScope.Local): Promise<string> {
    // Check if source file exists
    if (!await fs.pathExists(filePath)) {
      throw new Error(`File not found: ${filePath}`);
    }
    
    const content = await fs.readFile(filePath, 'utf8');
    return await this.createPrompt(name, content, scope);
  }

  async listPromptsInScope(scope: ConfigFileScope): Promise<string[]> {
    const promptsDir = this.getPromptsDirectory(scope);
    
    if (!await fs.pathExists(promptsDir)) {
      return [];
    }
    
    const files = await fs.readdir(promptsDir);
    return files
      .filter(file => file.endsWith('.prompt'))
      .map(file => path.basename(file, '.prompt'))
      .sort();
  }

  async getAllScopesWithPrompts(): Promise<Array<{ scope: ConfigFileScope, location: string, prompts: string[] }>> {
    const scopes = [ConfigFileScope.Global, ConfigFileScope.User, ConfigFileScope.Local];
    const results = [];

    for (const scope of scopes) {
      const location = this.getPromptsDirectory(scope);
      const prompts = await this.listPromptsInScope(scope);
      results.push({ scope, location, prompts });
    }

    return results;
  }

  async getPrompt(name: string, scope: ConfigFileScope = ConfigFileScope.Any): Promise<{ content: string, path: string, scope: ConfigFileScope }> {
    this.validatePromptName(name);
    
    const promptInfo = await this.findPromptFile(name, scope);
    if (!promptInfo) {
      throw new Error(`Error showing prompt: Prompt '${name}' not found.`);
    }
    
    let content = await fs.readFile(promptInfo.path, 'utf8');
    content = content.trim();
    
    // Handle @ file references (matching C# behavior)
    if (content.startsWith('@')) {
      const referencedFilePath = content.substring(1);
      if (await fs.pathExists(referencedFilePath)) {
        content = await fs.readFile(referencedFilePath, 'utf8');
      }
    }
    
    return { content, path: promptInfo.path, scope: promptInfo.scope };
  }

  async deletePrompt(name: string, scope: ConfigFileScope = ConfigFileScope.Any): Promise<void> {
    this.validatePromptName(name);
    
    const promptInfo = await this.findPromptFile(name, scope);
    if (!promptInfo) {
      throw new Error(`Error deleting prompt: Prompt '${name}' not found.`);
    }
    
    await fs.remove(promptInfo.path);
  }
}