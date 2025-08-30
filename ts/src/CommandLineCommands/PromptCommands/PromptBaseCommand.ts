import { ConfigFileScope } from '../../Configuration/ConfigFileScope';

/**
 * Base class for all prompt-related commands.
 */
export abstract class PromptBaseCommand {
  public scope?: ConfigFileScope;

  constructor() {
    // Base constructor
  }

  /**
   * Gets the command name.
   */
  abstract getCommandName(): string;

  /**
   * Executes the command.
   */
  abstract executeAsync(interactive: boolean): Promise<number>;

  /**
   * Indicates if the command is empty.
   */
  isEmpty(): boolean {
    return false;
  }
}