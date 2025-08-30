import * as fs from 'fs-extra';
import * as path from 'path';
import * as os from 'os';

export class PromptStore {
  private static readonly PROMPTS_DIR_NAME = '.cycod';
  private static readonly PROMPTS_SUBDIR = 'prompts';

  private getPromptsDirectory(): string {
    return path.join(os.homedir(), PromptStore.PROMPTS_DIR_NAME, PromptStore.PROMPTS_SUBDIR);
  }

  private getPromptPath(name: string): string {
    return path.join(this.getPromptsDirectory(), `${name}.txt`);
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

  async ensurePromptsDirectory(): Promise<void> {
    const promptsDir = this.getPromptsDirectory();
    await fs.ensureDir(promptsDir);
  }

  async createPrompt(name: string, content: string): Promise<void> {
    this.validatePromptName(name);
    
    await this.ensurePromptsDirectory();
    const promptPath = this.getPromptPath(name);
    
    // Check if prompt already exists
    if (await fs.pathExists(promptPath)) {
      throw new Error(`Prompt '${name}' already exists. Use 'prompt delete ${name}' first if you want to replace it.`);
    }
    
    await fs.writeFile(promptPath, content, 'utf8');
  }

  async createPromptFromFile(name: string, filePath: string): Promise<void> {
    // Check if source file exists
    if (!await fs.pathExists(filePath)) {
      throw new Error(`File not found: ${filePath}`);
    }
    
    const content = await fs.readFile(filePath, 'utf8');
    await this.createPrompt(name, content);
  }

  async listPrompts(): Promise<string[]> {
    const promptsDir = this.getPromptsDirectory();
    
    if (!await fs.pathExists(promptsDir)) {
      return [];
    }
    
    const files = await fs.readdir(promptsDir);
    return files
      .filter(file => file.endsWith('.txt'))
      .map(file => path.basename(file, '.txt'))
      .sort();
  }

  async getPrompt(name: string): Promise<string> {
    this.validatePromptName(name);
    
    const promptPath = this.getPromptPath(name);
    
    if (!await fs.pathExists(promptPath)) {
      throw new Error(`Prompt '${name}' not found.`);
    }
    
    return await fs.readFile(promptPath, 'utf8');
  }

  async deletePrompt(name: string): Promise<void> {
    this.validatePromptName(name);
    
    const promptPath = this.getPromptPath(name);
    
    if (!await fs.pathExists(promptPath)) {
      throw new Error(`Prompt '${name}' not found.`);
    }
    
    await fs.remove(promptPath);
  }

  async promptExists(name: string): Promise<boolean> {
    const promptPath = this.getPromptPath(name);
    return await fs.pathExists(promptPath);
  }
}