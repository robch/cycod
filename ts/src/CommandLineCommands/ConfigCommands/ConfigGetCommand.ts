import { ConfigBaseCommand } from './ConfigBaseCommand';
import { ConfigFileScope } from '../../Configuration/ConfigFileScope';
import { ConfigValueHelpers } from '../../Configuration/ConfigValue';
import { KnownSettings } from '../../Configuration/KnownSettings';
import { ConsoleHelpers } from '../../Helpers/ConsoleHelpers';
import { PathHelpers } from '../../Helpers/PathHelpers';

/**
 * Command to get a configuration setting.
 */
export class ConfigGetCommand extends ConfigBaseCommand {
  public key?: string;

  getCommandName(): string {
    return 'config get';
  }

  isEmpty(): boolean {
    return !this.key || this.key.trim() === '';
  }

  async executeAsync(_interactive: boolean): Promise<number> {
    return await this.executeGet(this.key!, this.scope ?? ConfigFileScope.Any, this.configFileName);
  }

  private async executeGet(key: string, scope: ConfigFileScope, configFileName?: string): Promise<number> {
    ConsoleHelpers.writeDebugLine(`ExecuteGet; key: ${key}; scope: ${scope}; configFileName: ${configFileName}`);
    
    if (!key || key.trim() === '') {
      throw new Error('Error: No key specified.');
    }

    // Normalize the key if it's a known setting
    let normalizedKey = key;
    if (KnownSettings.isKnown(key)) {
      normalizedKey = KnownSettings.getCanonicalForm(key);
    }

    const isFileNameScope = scope === ConfigFileScope.FileName && !!configFileName;
    const value = isFileNameScope
      ? await this._configStore.getFromFileName(normalizedKey, configFileName!)
      : await this._configStore.getFromScope(normalizedKey, scope);

    this.displayConfigValue(normalizedKey, value);
    return 0;
  }

  private displayConfigValue(key: string, configValue: any): void {
    if (!ConfigValueHelpers.hasValue(configValue)) {
      // For missing values, show like the C# version
      ConsoleHelpers.writeLine(`${key}: (not found or empty)`);
      return;
    }

    // Show location header first (like C# WriteLocationHeader)
    const locationPath = this.getLocationPath(configValue);
    if (locationPath) {
      ConsoleHelpers.displayLocationHeader(locationPath);
    }

    // Then show indented key: value (like C# DisplayConfigValue)
    const displayValue = ConfigValueHelpers.getDisplayValue(configValue);
    if (displayValue === 'YAML_ARRAY_FORMAT' && Array.isArray(configValue.value)) {
      // Display array in YAML format with dashes
      ConsoleHelpers.writeLine(`  ${key}:`, true);
      for (const item of configValue.value) {
        ConsoleHelpers.writeLine(`    - ${item}`, true);
      }
    } else {
      ConsoleHelpers.writeLine(`  ${key}: ${displayValue}`, true);
    }
  }

  private getLocationPath(configValue: any): string {
    if (!configValue || !configValue.scope) return '';
    
    switch (configValue.scope) {
      case ConfigFileScope.Global:
        return `${PathHelpers.getConfigFilePath('global')} (global)`;
      case ConfigFileScope.User:
        return `${PathHelpers.getConfigFilePath('user')} (user)`;
      case ConfigFileScope.Local:
        return `${PathHelpers.getConfigFilePath('local')} (local)`;
      case ConfigFileScope.FileName:
        return `${configValue.source} (specified)`;
      default:
        return `${configValue.source} (${configValue.scope})`;
    }
  }
}