import { ConfigFileScope } from './ConfigFileScope';
import { ConfigValue, ConfigSetting } from './ConfigValue';
/**
 * Manages configuration settings across different scopes.
 */
export declare class ConfigStore {
    private static _instance;
    private _configFiles;
    private _commandLineSettings;
    private _loadedKnownConfigFiles;
    private constructor();
    /**
     * Gets the singleton instance of ConfigStore.
     */
    static get instance(): ConfigStore;
    /**
     * Loads a configuration file from the specified path.
     */
    loadConfigFile(fileName: string): Promise<void>;
    /**
     * Loads multiple configuration files.
     */
    loadConfigFiles(fileNames: string[]): Promise<void>;
    /**
     * Gets a configuration value from any scope (command line -> filename -> local -> user -> global).
     */
    getFromAnyScope(key: string): Promise<ConfigValue | undefined>;
    /**
     * Gets a configuration value from a specific scope.
     */
    getFromScope(key: string, scope: ConfigFileScope): Promise<ConfigValue | undefined>;
    /**
     * Gets a configuration value from a specific file.
     */
    getFromFileName(key: string, fileName: string): Promise<ConfigValue | undefined>;
    /**
     * Sets a configuration value in a specific scope.
     */
    set(key: string, value: ConfigSetting, scope: ConfigFileScope, createIfMissing?: boolean): Promise<void>;
    /**
     * Sets a configuration value in a specific file.
     */
    setInFile(key: string, value: ConfigSetting, fileName: string): Promise<void>;
    /**
     * Clears a configuration value from a specific scope.
     */
    clear(key: string, scope: ConfigFileScope): Promise<void>;
    /**
     * Adds a value to a list setting.
     */
    addToList(key: string, value: string, scope: ConfigFileScope): Promise<void>;
    /**
     * Removes a value from a list setting.
     */
    removeFromList(key: string, value: string, scope: ConfigFileScope): Promise<void>;
    /**
     * Lists all configuration values from a specific scope.
     */
    listValuesFromScope(scope: ConfigFileScope): Promise<Record<string, ConfigValue>>;
    /**
     * Lists all configuration values from command line settings.
     */
    listFromCommandLineSettings(): Record<string, ConfigValue>;
    /**
     * Sets a command line setting.
     */
    setCommandLineSetting(key: string, value: any): void;
    /**
     * Ensures that known configuration files are loaded.
     */
    private ensureLoaded;
    /**
     * Gets the display name for a configuration scope.
     */
    private getScopeDisplayName;
    /**
     * Flattens a nested object into dot notation keys.
     */
    private flattenObject;
}
//# sourceMappingURL=ConfigStore.d.ts.map