import { ConfigBaseCommand } from './ConfigBaseCommand';
import { ConfigFileScope } from '../../Configuration/ConfigFileScope';
import { KnownSettings } from '../../Configuration/KnownSettings';
import { ConsoleHelpers } from '../../Helpers/ConsoleHelpers';

/**
 * Command to remove a value from a list configuration setting.
 */
export class ConfigRemoveCommand extends ConfigBaseCommand {
  public key?: string;
  public value?: string;

  getCommandName(): string {
    return 'config remove';
  }

  isEmpty(): boolean {
    return !this.key || this.key.trim() === '' || this.value === undefined;
  }

  async executeAsync(_interactive: boolean): Promise<number> {
    return await this.executeRemove(
      this.key!,
      this.value!,
      this.scope ?? ConfigFileScope.Local
    );
  }

  private async executeRemove(
    key: string,
    value: string,
    scope: ConfigFileScope
  ): Promise<number> {
    if (!key || key.trim() === '') {
      throw new Error('Error: No key specified.');
    }
    if (value === undefined) {
      throw new Error('Error: No value specified.');
    }

    // Normalize the key if it's a known setting
    let normalizedKey = key;
    if (KnownSettings.isKnown(key)) {
      normalizedKey = KnownSettings.getCanonicalForm(key);
    }

    await this._configStore.removeFromList(normalizedKey, value, scope);
    
    const scopeName = this.getScopeDisplayName(scope);
    ConsoleHelpers.writeLine(`Removed '${value}' from '${normalizedKey}' in ${scopeName} configuration`);

    // Display the updated list
    const updatedValue = await this._configStore.getFromScope(normalizedKey, scope);
    if (updatedValue && Array.isArray(updatedValue.value)) {
      ConsoleHelpers.writeLine(`${normalizedKey}=[${updatedValue.value.join(', ')}]`);
    } else {
      ConsoleHelpers.writeLine(`${normalizedKey}= (empty)`);
    }

    return 0;
  }

  private getScopeDisplayName(scope: ConfigFileScope): string {
    switch (scope) {
      case ConfigFileScope.Global:
        return 'Global';
      case ConfigFileScope.User:
        return 'User';
      case ConfigFileScope.Local:
        return 'Local';
      default:
        return 'Unknown';
    }
  }
}