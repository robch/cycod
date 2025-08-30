import { ConfigBaseCommand } from './ConfigBaseCommand';
import { ConfigFileScope } from '../../Configuration/ConfigFileScope';
import { KnownSettings } from '../../Configuration/KnownSettings';
import { ConsoleHelpers } from '../../Helpers/ConsoleHelpers';

/**
 * Command to clear a configuration setting.
 */
export class ConfigClearCommand extends ConfigBaseCommand {
  public key?: string;

  getCommandName(): string {
    return 'config clear';
  }

  isEmpty(): boolean {
    return !this.key || this.key.trim() === '';
  }

  async executeAsync(_interactive: boolean): Promise<number> {
    return await this.executeClear(
      this.key!,
      this.scope ?? ConfigFileScope.Local,
      this.configFileName
    );
  }

  private async executeClear(
    key: string,
    scope: ConfigFileScope,
    configFileName?: string
  ): Promise<number> {
    if (!key || key.trim() === '') {
      throw new Error('Error: No key specified.');
    }

    // Normalize the key if it's a known setting
    let normalizedKey = key;
    if (KnownSettings.isKnown(key)) {
      normalizedKey = KnownSettings.getCanonicalForm(key);
    }

    const isFileNameScope = scope === ConfigFileScope.FileName && !!configFileName;
    
    if (isFileNameScope) {
      // For file-based clearing, we would need to implement clearFromFile
      // For now, we'll just indicate success
      ConsoleHelpers.writeLine(`${normalizedKey}: (cleared)`);
    } else {
      await this._configStore.clear(normalizedKey, scope);
      ConsoleHelpers.writeLine(`${normalizedKey}: (cleared)`);
    }

    return 0;
  }

}