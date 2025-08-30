import { ConfigFileScope } from '../Configuration/ConfigFileScope';
import { ConsoleHelpers } from './ConsoleHelpers';
import { PromptFileHelpers } from './PromptFileHelpers';
import { PathHelpers } from './PathHelpers';

/**
 * Provides methods for displaying prompt information.
 */
export class PromptDisplayHelpers {
  
  /**
   * Gets a display name for the prompt directory location in the specified scope.
   */
  static getLocationDisplayName(scope: ConfigFileScope): string | null {
    const promptPath = PromptFileHelpers.findPromptDirectoryInScope(scope);
    if (promptPath) {
      return this.formatLocationHeader(promptPath, scope);
    }
    
    const scopeDir = PathHelpers.getScopeDirectoryPath(scope);
    if (!scopeDir) return null;

    const promptsDir = require('path').join(scopeDir, 'prompts');
    return this.formatLocationHeader(promptsDir, scope);
  }

  /**
   * Formats a location header for display.
   */
  static formatLocationHeader(path: string, scope: ConfigFileScope): string {
    return `${path} (${scope.toString().toLowerCase()})`;
  }

  /**
   * Displays prompts for a specific scope.
   */
  static displayPrompts(scope: ConfigFileScope): void {
    const locationDisplay = this.getLocationDisplayName(scope);
    if (!locationDisplay) return;
    
    const promptNames = PromptFileHelpers.listPromptsInScope(scope);
    this.displayItemList(locationDisplay, promptNames);
  }

  /**
   * Displays a list of items with a location header.
   */
  static displayItemList(locationHeader: string, items: string[]): void {
    ConsoleHelpers.writeLine(`LOCATION: ${locationHeader}`);
    ConsoleHelpers.writeLine('');

    if (items.length === 0) {
      ConsoleHelpers.writeLine('  No items found in this scope');
    } else {
      for (const item of items) {
        ConsoleHelpers.writeLine(`  ${item}`);
      }
    }
  }

  /**
   * Displays a single prompt with content preview.
   */
  static displayPrompt(name: string, promptFilePath: string, scope: ConfigFileScope, content: string): void {
    const location = this.formatLocationHeader(promptFilePath, scope);
    this.displayItem(name, content, location, true);
  }

  /**
   * Displays a single item with content.
   */
  static displayItem(name: string, content: string, location: string, _limitValue: boolean = false): void {
    ConsoleHelpers.writeLine(`LOCATION: ${location}`);
    ConsoleHelpers.writeLine('');
    ConsoleHelpers.writeLine(`  ${name}`);
    ConsoleHelpers.writeLine('');
    
    // Display content with indentation
    const lines = content.split('\n');
    for (const line of lines) {
      ConsoleHelpers.writeLine(`    ${line}`);
    }
  }

  /**
   * Displays information about saved prompt files.
   */
  static displaySavedPromptFile(fileName: string): void {
    ConsoleHelpers.writeLine(`Saved: ${fileName}`);
    ConsoleHelpers.writeLine('');
    
    // Extract prompt name from filename
    const path = require('path');
    const promptName = path.basename(fileName, '.prompt');
    ConsoleHelpers.writeLine(`USAGE: /${promptName} (in chat)`);
  }
}