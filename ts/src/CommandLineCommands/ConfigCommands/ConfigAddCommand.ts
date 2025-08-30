import { ConfigBaseCommand } from './ConfigBaseCommand';
import { ConfigFileScope } from '../../Configuration/ConfigFileScope';
import { KnownSettings } from '../../Configuration/KnownSettings';
import { ConsoleHelpers } from '../../Helpers/ConsoleHelpers';

/**
 * Command to add a value to a list configuration setting.
 */
export class ConfigAddCommand extends ConfigBaseCommand {
  public key?: string;
  public value?: string;

  getCommandName(): string {
    return 'config add';
  }

  isEmpty(): boolean {
    return !this.key || this.key.trim() === '' || this.value === undefined;
  }

  async executeAsync(_interactive: boolean): Promise<number> {
    return await this.executeAdd(
      this.key!,
      this.value!,
      this.scope ?? ConfigFileScope.Local
    );
  }

  private async executeAdd(
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

    await this._configStore.addToList(normalizedKey, value, scope);

    // Display the updated list in YAML format
    const updatedValue = await this._configStore.getFromScope(normalizedKey, scope);
    if (updatedValue && Array.isArray(updatedValue.value)) {
      ConsoleHelpers.writeLine(`${normalizedKey}:`);
      for (const item of updatedValue.value) {
        ConsoleHelpers.writeLine(`- ${item}`);
      }
    }

    return 0;
  }

}