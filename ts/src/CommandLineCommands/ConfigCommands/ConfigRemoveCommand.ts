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

    // Display the updated list in YAML format
    const updatedValue = await this._configStore.getFromScope(normalizedKey, scope);
    if (updatedValue && Array.isArray(updatedValue.value) && updatedValue.value.length > 0) {
      ConsoleHelpers.writeLine(`${normalizedKey}:`);
      for (const item of updatedValue.value) {
        ConsoleHelpers.writeLine(`- ${item}`);
      }
    } else {
      ConsoleHelpers.writeLine(`${normalizedKey}: (empty)`);
    }

    return 0;
  }

}