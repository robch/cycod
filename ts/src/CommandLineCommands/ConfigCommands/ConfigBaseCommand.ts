import { ConfigStore } from '../../Configuration/ConfigStore';
import { ConfigFileScope } from '../../Configuration/ConfigFileScope';

/**
 * Base class for configuration commands.
 */
export abstract class ConfigBaseCommand {
  protected readonly _configStore: ConfigStore;
  
  public scope?: ConfigFileScope;
  public configFileName?: string;

  constructor() {
    this._configStore = ConfigStore.instance;
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
   * Checks if the command has all required parameters.
   */
  isEmpty(): boolean {
    return false;
  }
}