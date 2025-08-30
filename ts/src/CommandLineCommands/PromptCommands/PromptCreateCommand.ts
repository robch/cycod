import { PromptBaseCommand } from './PromptBaseCommand';
import { ConfigFileScope } from '../../Configuration/ConfigFileScope';
import { PromptFileHelpers } from '../../Helpers/PromptFileHelpers';
import { PromptDisplayHelpers } from '../../Helpers/PromptDisplayHelpers';
import { ConsoleHelpers } from '../../Helpers/ConsoleHelpers';

/**
 * Command to create a new prompt.
 */
export class PromptCreateCommand extends PromptBaseCommand {
  public promptName?: string;
  public promptText?: string;

  constructor() {
    super();
    this.scope = ConfigFileScope.Local; // Default to local scope
  }

  getCommandName(): string {
    return 'prompt create';
  }

  isEmpty(): boolean {
    return !this.promptName || !this.promptText;
  }

  async executeAsync(_interactive: boolean): Promise<number> {
    if (!this.promptName) {
      ConsoleHelpers.writeErrorLine('Error: Prompt name is required.');
      return 1;
    }
    
    if (!this.promptText) {
      ConsoleHelpers.writeErrorLine('Error: Prompt text is required.');
      return 1;
    }

    try {
      // Remove leading slash if someone added it (for consistency)
      let cleanName = this.promptName;
      if (cleanName.startsWith('/')) {
        cleanName = cleanName.substring(1);
      }
      
      // Check if the prompt already exists
      const existingPromptFile = PromptFileHelpers.findPromptFile(cleanName);
      if (existingPromptFile) {
        ConsoleHelpers.writeErrorLine(`Error: Prompt '${cleanName}' already exists.`);
        return 1;
      }
      
      // Save the prompt
      const fileName = PromptFileHelpers.savePrompt(cleanName, this.promptText, this.scope ?? ConfigFileScope.Local);
      PromptDisplayHelpers.displaySavedPromptFile(fileName);
      
      return 0;
    } catch (ex) {
      const error = ex as Error;
      ConsoleHelpers.writeErrorLine(`Error creating prompt: ${error.message}`);
      return 1;
    }
  }
}