import { ConfigBaseCommand } from './ConfigBaseCommand';
import { ConfigFileScope } from '../../Configuration/ConfigFileScope';
import { ConfigValueHelpers } from '../../Configuration/ConfigValue';
import { KnownSettings } from '../../Configuration/KnownSettings';
import { ConsoleHelpers } from '../../Helpers/ConsoleHelpers';

/**
 * Command to set a configuration setting.
 */
export class ConfigSetCommand extends ConfigBaseCommand {
  public key?: string;
  public value?: string;

  getCommandName(): string {
    return 'config set';
  }

  isEmpty(): boolean {
    return !this.key || this.key.trim() === '' || this.value === undefined;
  }

  async executeAsync(_interactive: boolean): Promise<number> {
    return await this.executeSet(
      this.key!,
      this.value!,
      this.scope ?? ConfigFileScope.Local,
      this.configFileName
    );
  }

  private async executeSet(
    key: string,
    value: string,
    scope: ConfigFileScope,
    configFileName?: string
  ): Promise<number> {
    if (!key || key.trim() === '') {
      throw new Error('Error: No key specified.');
    }
    if (value === undefined) {
      throw new Error('Error: No value specified.');
    }

    // Validate and normalize the key against known settings
    let normalizedKey = key;
    if (!KnownSettings.isKnown(key)) {
      const allKnownSettings = KnownSettings.getAllKnownSettings().sort();
      ConsoleHelpers.writeWarningLine(
        `Warning: Unknown setting '${key}'. Valid settings are: ${allKnownSettings.join(', ')}`
      );
      // Continue with the original key without normalization
    } else {
      // Use the canonical form for storage of known settings
      normalizedKey = KnownSettings.getCanonicalForm(key);
    }

    // Try to parse as a list if the value is enclosed in brackets
    if (value.startsWith('[') && value.endsWith(']')) {
      const listContent = value.substring(1, value.length - 2);
      const listValue: string[] = [];
      
      if (listContent.trim() !== '') {
        const items = listContent.split(',');
        for (const item of items) {
          listValue.push(item.trim());
        }
      }
      
      const isFileNameScope = scope === ConfigFileScope.FileName && !!configFileName;
      if (isFileNameScope) {
        await this._configStore.setInFile(normalizedKey, listValue, configFileName!);
      } else {
        await this._configStore.set(normalizedKey, listValue, scope, true);
      }

      this.displayList(normalizedKey, listValue);
    } else {
      const isFileNameScope = scope === ConfigFileScope.FileName && !!configFileName;
      if (isFileNameScope) {
        await this._configStore.setInFile(normalizedKey, value, configFileName!);
      } else {
        await this._configStore.set(normalizedKey, value, scope, true);
      }

      const configValue = isFileNameScope
        ? await this._configStore.getFromFileName(normalizedKey, configFileName!)
        : await this._configStore.getFromScope(normalizedKey, scope);

      this.displayConfigValue(normalizedKey, configValue);
    }

    return 0;
  }

  private displayConfigValue(key: string, configValue: any): void {
    // Show location header first (like C# WriteLocationHeader)
    const locationPath = this.getLocationPath(configValue);
    if (locationPath) {
      ConsoleHelpers.displayLocationHeader(locationPath);
    }
    
    // Then show indented key: value
    const displayValue = ConfigValueHelpers.getDisplayValue(configValue);
    ConsoleHelpers.writeLine(`  ${key}: ${displayValue}`, true);
  }

  private displayList(key: string, list: string[]): void {
    // Format list as YAML-style list with dashes
    ConsoleHelpers.writeLine(`${key}:`);
    for (const item of list) {
      ConsoleHelpers.writeLine(`- ${item}`);
    }
  }

  private getLocationPath(configValue: any): string | undefined {
    if (!ConfigValueHelpers.hasValue(configValue)) {
      return undefined;
    }

    // Use the same method as ConfigGetCommand
    if (!configValue || !configValue.scope) return undefined;
    
    const { PathHelpers } = require('../../Helpers/PathHelpers');
    switch (configValue.scope) {
      case 'global':
        return `${PathHelpers.getConfigFilePath('global')} (global)`;
      case 'user':
        return `${PathHelpers.getConfigFilePath('user')} (user)`;
      case 'local':
        return `${PathHelpers.getConfigFilePath('local')} (local)`;
      default:
        return `${configValue.source} (${configValue.scope})`;
    }
  }
}