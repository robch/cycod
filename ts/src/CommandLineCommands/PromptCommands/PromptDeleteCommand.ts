import * as fs from 'fs';
import * as path from 'path';
import { PromptBaseCommand } from './PromptBaseCommand';
import { ConfigFileScope } from '../../Configuration/ConfigFileScope';
import { PromptFileHelpers } from '../../Helpers/PromptFileHelpers';
import { ConsoleHelpers } from '../../Helpers/ConsoleHelpers';

/**
 * Command to delete a prompt.
 */
export class PromptDeleteCommand extends PromptBaseCommand {
  public promptName?: string;

  constructor() {
    super();
    this.scope = ConfigFileScope.Any;
  }

  getCommandName(): string {
    return 'prompt delete';
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

    try {
      let output = `Deleted: ${promptFilePath}`;

      // Check for file references
      const content = fs.readFileSync(promptFilePath, 'utf8').trim();
      let referencedFilePath: string | null = null;
      
      if (content.startsWith('@')) {
        referencedFilePath = content.substring(1);
      }

      // Delete the main prompt file
      fs.unlinkSync(promptFilePath);
      ConsoleHelpers.writeLine(output);

      // Delete the referenced file if it exists
      if (referencedFilePath && fs.existsSync(referencedFilePath)) {
        fs.unlinkSync(referencedFilePath);
        ConsoleHelpers.writeLine(`Deleted: ${referencedFilePath}`);
      }

      // Delete any additional files if they exist (following C# pattern)
      const directory = path.dirname(promptFilePath);
      const fileBase = path.basename(promptFilePath, '.prompt');
      
      try {
        const files = fs.readdirSync(directory);
        const additionalFiles = files.filter(file => {
          const regex = new RegExp(`^${fileBase}-.*\\.prompt$`);
          return regex.test(file);
        });

        for (const additionalFile of additionalFiles) {
          const additionalFilePath = path.join(directory, additionalFile);
          fs.unlinkSync(additionalFilePath);
          ConsoleHelpers.writeLine(`Deleted: ${additionalFilePath}`);
        }
      } catch (err) {
        // Ignore errors when looking for additional files
      }

      return 0;
    } catch (ex) {
      const error = ex as Error;
      ConsoleHelpers.writeErrorLine(`Error deleting prompt: ${error.message}`);
      return 1;
    }
  }
}