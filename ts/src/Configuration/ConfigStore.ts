import { ConfigFile } from './ConfigFile';
import { ConfigFileScope } from './ConfigFileScope';
import { ConfigValue, ConfigValueHelpers, ConfigSetting } from './ConfigValue';
import { PathHelpers } from '../Helpers/PathHelpers';
import { KnownSettings } from './KnownSettings';

/**
 * Manages configuration settings across different scopes.
 */
export class ConfigStore {
  private static _instance: ConfigStore;
  private _configFiles: ConfigFile[] = [];
  private _commandLineSettings: Map<string, any> = new Map();
  private _loadedKnownConfigFiles = false;

  private constructor() {
    // Private constructor for singleton
  }

  /**
   * Gets the singleton instance of ConfigStore.
   */
  static get instance(): ConfigStore {
    if (!ConfigStore._instance) {
      ConfigStore._instance = new ConfigStore();
    }
    return ConfigStore._instance;
  }

  /**
   * Loads a configuration file from the specified path.
   */
  async loadConfigFile(fileName: string): Promise<void> {
    await this.ensureLoaded();
    
    const configFile = await ConfigFile.fromFile(fileName, ConfigFileScope.FileName);
    this._configFiles.push(configFile);
  }

  /**
   * Loads multiple configuration files.
   */
  async loadConfigFiles(fileNames: string[]): Promise<void> {
    await this.ensureLoaded();
    
    for (const fileName of fileNames) {
      const configFile = await ConfigFile.fromFile(fileName, ConfigFileScope.FileName);
      this._configFiles.push(configFile);
    }
  }

  /**
   * Gets a configuration value from any scope (command line -> filename -> local -> user -> global).
   */
  async getFromAnyScope(key: string): Promise<ConfigValue | undefined> {
    await this.ensureLoaded();

    const dotNotationKey = KnownSettings.toDotNotation(key);
    const isSecret = KnownSettings.isSecret(dotNotationKey);

    // 1. Check command line settings (highest priority)
    if (this._commandLineSettings.has(dotNotationKey)) {
      const value = this._commandLineSettings.get(dotNotationKey);
      return ConfigValueHelpers.create(value, ConfigFileScope.Any, 'Command line', isSecret);
    }

    // 2. Check filename scope files
    for (const configFile of this._configFiles) {
      if (configFile.scope === ConfigFileScope.FileName) {
        const value = configFile.get(dotNotationKey);
        if (value !== undefined) {
          return ConfigValueHelpers.create(value, ConfigFileScope.FileName, configFile.fileName, isSecret);
        }
      }
    }

    // 3. Check local scope
    const localValue = await this.getFromScope(dotNotationKey, ConfigFileScope.Local);
    if (ConfigValueHelpers.hasValue(localValue)) {
      return localValue;
    }

    // 4. Check user scope
    const userValue = await this.getFromScope(dotNotationKey, ConfigFileScope.User);
    if (ConfigValueHelpers.hasValue(userValue)) {
      return userValue;
    }

    // 5. Check global scope
    const globalValue = await this.getFromScope(dotNotationKey, ConfigFileScope.Global);
    if (ConfigValueHelpers.hasValue(globalValue)) {
      return globalValue;
    }

    return undefined;
  }

  /**
   * Gets a configuration value from a specific scope.
   */
  async getFromScope(key: string, scope: ConfigFileScope): Promise<ConfigValue | undefined> {
    // Match C# behavior: if scope is Any, redirect to getFromAnyScope
    if (scope === ConfigFileScope.Any) {
      return this.getFromAnyScope(key);
    }

    await this.ensureLoaded();

    const dotNotationKey = KnownSettings.toDotNotation(key);
    const isSecret = KnownSettings.isSecret(dotNotationKey);

    const configFile = this._configFiles.find(cf => cf.scope === scope);
    if (configFile) {
      const value = configFile.get(dotNotationKey);
      if (value !== undefined) {
        const source = this.getScopeDisplayName(scope);
        return ConfigValueHelpers.create(value, scope, source, isSecret);
      }
    }

    return undefined;
  }

  /**
   * Gets a configuration value from a specific file.
   */
  async getFromFileName(key: string, fileName: string): Promise<ConfigValue | undefined> {
    await this.ensureLoaded();

    const dotNotationKey = KnownSettings.toDotNotation(key);
    const isSecret = KnownSettings.isSecret(dotNotationKey);

    const configFile = this._configFiles.find(cf => 
      cf.scope === ConfigFileScope.FileName && cf.fileName === fileName
    );

    if (configFile) {
      const value = configFile.get(dotNotationKey);
      if (value !== undefined) {
        return ConfigValueHelpers.create(value, ConfigFileScope.FileName, fileName, isSecret);
      }
    }

    return undefined;
  }

  /**
   * Sets a configuration value in a specific scope.
   */
  async set(key: string, value: ConfigSetting, scope: ConfigFileScope, createIfMissing = true): Promise<void> {
    await this.ensureLoaded();

    const dotNotationKey = KnownSettings.toDotNotation(key);
    let configFile = this._configFiles.find(cf => cf.scope === scope);

    if (!configFile && createIfMissing) {
      const fileName = PathHelpers.getConfigFilePath(scope as any);
      configFile = await ConfigFile.fromFile(fileName, scope);
      this._configFiles.push(configFile);
    }

    if (configFile) {
      configFile.set(dotNotationKey, value);
      await configFile.save();
    }
  }

  /**
   * Sets a configuration value in a specific file.
   */
  async setInFile(key: string, value: ConfigSetting, fileName: string): Promise<void> {
    await this.ensureLoaded();

    const dotNotationKey = KnownSettings.toDotNotation(key);
    let configFile = this._configFiles.find(cf => 
      cf.scope === ConfigFileScope.FileName && cf.fileName === fileName
    );

    if (!configFile) {
      configFile = await ConfigFile.fromFile(fileName, ConfigFileScope.FileName);
      this._configFiles.push(configFile);
    }

    configFile.set(dotNotationKey, value);
    await configFile.save();
  }

  /**
   * Clears a configuration value from a specific scope.
   */
  async clear(key: string, scope: ConfigFileScope): Promise<void> {
    await this.ensureLoaded();

    const dotNotationKey = KnownSettings.toDotNotation(key);
    const configFile = this._configFiles.find(cf => cf.scope === scope);

    if (configFile) {
      configFile.clear(dotNotationKey);
      await configFile.save();
    }
  }

  /**
   * Adds a value to a list setting.
   */
  async addToList(key: string, value: string, scope: ConfigFileScope): Promise<void> {
    const currentValue = await this.getFromScope(key, scope);
    let newList: string[];

    if (ConfigValueHelpers.hasValue(currentValue)) {
      if (Array.isArray(currentValue!.value)) {
        newList = [...currentValue!.value, value];
      } else {
        newList = [String(currentValue!.value), value];
      }
    } else {
      newList = [value];
    }

    await this.set(key, newList, scope);
  }

  /**
   * Removes a value from a list setting.
   */
  async removeFromList(key: string, value: string, scope: ConfigFileScope): Promise<void> {
    const currentValue = await this.getFromScope(key, scope);

    if (ConfigValueHelpers.hasValue(currentValue) && Array.isArray(currentValue!.value)) {
      const newList = currentValue!.value.filter(item => item !== value);
      if (newList.length > 0) {
        await this.set(key, newList, scope);
      } else {
        await this.clear(key, scope);
      }
    }
  }

  /**
   * Lists all configuration values from a specific scope.
   */
  async listValuesFromScope(scope: ConfigFileScope): Promise<Record<string, ConfigValue>> {
    await this.ensureLoaded();
    
    const result: Record<string, ConfigValue> = {};
    const configFile = this._configFiles.find(cf => cf.scope === scope);

    if (configFile) {
      const allValues = configFile.getAll();
      const source = this.getScopeDisplayName(scope);
      this.flattenObject(allValues, '', result, scope, source);
    }

    return result;
  }

  /**
   * Lists all configuration values from command line settings.
   */
  listFromCommandLineSettings(): Record<string, ConfigValue> {
    const result: Record<string, ConfigValue> = {};

    for (const [key, value] of this._commandLineSettings.entries()) {
      const isSecret = KnownSettings.isSecret(key);
      result[key] = ConfigValueHelpers.create(value, ConfigFileScope.Any, 'Command line', isSecret);
    }

    return result;
  }

  /**
   * Sets a command line setting.
   */
  setCommandLineSetting(key: string, value: any): void {
    const dotNotationKey = KnownSettings.toDotNotation(key);
    this._commandLineSettings.set(dotNotationKey, value);
  }

  /**
   * Ensures that known configuration files are loaded.
   */
  private async ensureLoaded(): Promise<void> {
    if (this._loadedKnownConfigFiles) {
      return;
    }

    // Load known configuration files in order (global, user, local)
    const scopes: ConfigFileScope[] = [ConfigFileScope.Global, ConfigFileScope.User, ConfigFileScope.Local];

    for (const scope of scopes) {
      try {
        const fileName = PathHelpers.getConfigFilePath(scope as any);
        const configFile = await ConfigFile.fromFile(fileName, scope);
        this._configFiles.push(configFile);
        // Config file loaded successfully
      } catch (error) {
        // Continue - file may not exist or be accessible
        // Continue - file may not exist
      }
    }

    this._loadedKnownConfigFiles = true;
  }

  /**
   * Gets the display name for a configuration scope.
   */
  private getScopeDisplayName(scope: ConfigFileScope): string {
    switch (scope) {
      case ConfigFileScope.Global:
        return 'Global';
      case ConfigFileScope.User:
        return 'User';
      case ConfigFileScope.Local:
        return 'Local';
      case ConfigFileScope.FileName:
        return 'File';
      default:
        return 'Unknown';
    }
  }

  /**
   * Flattens a nested object into dot notation keys.
   */
  private flattenObject(
    obj: any,
    prefix: string,
    result: Record<string, ConfigValue>,
    scope: ConfigFileScope,
    source: string
  ): void {
    for (const [key, value] of Object.entries(obj)) {
      const fullKey = prefix ? `${prefix}.${key}` : key;
      const isSecret = KnownSettings.isSecret(fullKey);

      if (value !== null && typeof value === 'object' && !Array.isArray(value)) {
        this.flattenObject(value, fullKey, result, scope, source);
      } else {
        result[fullKey] = ConfigValueHelpers.create(value, scope, source, isSecret);
      }
    }
  }
}