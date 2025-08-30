import * as fs from 'fs';
import { PromptBaseCommand } from './PromptBaseCommand';
import { ConfigFileScope } from '../../Configuration/ConfigFileScope';
import { PromptFileHelpers } from '../../Helpers/PromptFileHelpers';
import { PromptDisplayHelpers } from '../../Helpers/PromptDisplayHelpers';
import { ConsoleHelpers } from '../../Helpers/ConsoleHelpers';

/**
 * Command to view the content of a specific prompt.
 */
export class PromptGetCommand extends PromptBaseCommand {
  public promptName?: string;

  constructor() {
    super();
    this.scope = ConfigFileScope.Any;
  }

  getCommandName(): string {
    return 'prompt get';
  }

  isEmpty(): boolean {
    return !this.promptName;
  }

  async executeAsync(_interactive: boolean): Promise<number> {
    if (!this.promptName) {
      ConsoleHelpers.writeErrorLine('Error: Prompt name is required.');
      return 1;
    }

    const scope = this.scope ?? ConfigFileScope.Any;
    const promptFilePath = PromptFileHelpers.findPromptFile(this.promptName, scope);
    
    if (!promptFilePath || !fs.existsSync(promptFilePath)) {
      const scopeMessage = scope === ConfigFileScope.Any
        ? `Error: Prompt '${this.promptName}' not found in any scope.`
        : `Error: Prompt '${this.promptName}' not found in ${scope} scope.`;
      ConsoleHelpers.writeErrorLine(scopeMessage);
      return 1;
    }

    // Read and display the prompt content
    let content = fs.readFileSync(promptFilePath, 'utf8').trim();
    
    // Handle file references
    if (content.startsWith('@')) {
      const referencedFilePath = content.substring(1);
      if (fs.existsSync(referencedFilePath)) {
        content = fs.readFileSync(referencedFilePath, 'utf8');
      }
    }
    
    const foundInScope = PromptFileHelpers.getScopeFromPath(promptFilePath);
    PromptDisplayHelpers.displayPrompt(this.promptName, promptFilePath, foundInScope, content);

    return 0;
  }
}