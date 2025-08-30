import { ConfigBaseCommand } from './ConfigBaseCommand';
import { ConfigFileScope } from '../../Configuration/ConfigFileScope';
import { ConsoleHelpers } from '../../Helpers/ConsoleHelpers';
import { PathHelpers } from '../../Helpers/PathHelpers';

/**
 * Command to list configuration settings.
 */
export class ConfigListCommand extends ConfigBaseCommand {
  constructor() {
    super();
    this.scope = ConfigFileScope.Any;
  }

  getCommandName(): string {
    return 'config list';
  }

  async executeAsync(_interactive: boolean): Promise<number> {
    return this.scope === ConfigFileScope.FileName && this.configFileName
      ? await this.executeList(this.configFileName!)
      : await this.executeList(this.scope ?? ConfigFileScope.Any);
  }

  private async executeList(configFileName: string): Promise<number>;
  private async executeList(scope: ConfigFileScope): Promise<number>;
  private async executeList(scopeOrFileName: ConfigFileScope | string): Promise<number> {
    // Check if it's a known ConfigFileScope enum value
    const isScope = Object.values(ConfigFileScope).includes(scopeOrFileName as ConfigFileScope);
    
    if (!isScope) {
      await this.displayConfigSettings(scopeOrFileName as string);
      return 0;
    }

    const scope = scopeOrFileName as ConfigFileScope;
    const isAnyScope = scope === ConfigFileScope.Any;
    
    if (isAnyScope) {
      // Show all scopes
      await this.displayConfigSettings(ConfigFileScope.Global);
      ConsoleHelpers.writeLine('', true);
      await this.displayConfigSettings(ConfigFileScope.User);
      ConsoleHelpers.writeLine('', true);
      await this.displayConfigSettings(ConfigFileScope.Local);
      ConsoleHelpers.writeLine('', true);
      
      const commandLineValues = this._configStore.listFromCommandLineSettings();
      if (Object.keys(commandLineValues).length > 0) {
        const location = 'Command line (specified)';
        ConsoleHelpers.displayConfigSettings(location, commandLineValues);
        ConsoleHelpers.writeLine('', true);
      }
    } else {
      // Show only the specific scope requested
      await this.displayConfigSettings(scope);
    }
    
    return 0;
  }

  private async displayConfigSettings(fileName: string): Promise<void>;
  private async displayConfigSettings(scope: ConfigFileScope): Promise<void>;
  private async displayConfigSettings(scopeOrFileName: ConfigFileScope | string): Promise<void> {
    // Check if it's a known ConfigFileScope enum value
    const isScope = Object.values(ConfigFileScope).includes(scopeOrFileName as ConfigFileScope);
    
    if (!isScope) {
      const location = `${scopeOrFileName} (specified)`;
      // For file-based listing, we would need to implement listValuesFromFile
      // For now, we'll skip this implementation
      ConsoleHelpers.displayConfigSettings(location, {});
    } else {
      const scope = scopeOrFileName as ConfigFileScope;
      const location = this.getScopeDisplayName(scope);
      const values = await this._configStore.listValuesFromScope(scope);
      ConsoleHelpers.displayConfigSettings(location, values);
    }
  }

  private getScopeDisplayName(scope: ConfigFileScope): string {
    const configPath = PathHelpers.getConfigFilePath(scope as any);
    return `${configPath} (${scope.toString().toLowerCase()})`;
  }
}