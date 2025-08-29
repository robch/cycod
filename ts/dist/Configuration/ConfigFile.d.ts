import { ConfigFileScope } from './ConfigFileScope';
import { ConfigSetting } from './ConfigValue';
/**
 * Abstract base class for configuration files.
 */
export declare abstract class ConfigFile {
    protected _fileName: string;
    protected _scope: ConfigFileScope;
    protected _data: Record<string, any>;
    constructor(fileName: string, scope: ConfigFileScope);
    get fileName(): string;
    get scope(): ConfigFileScope;
    /**
     * Loads the configuration file from disk.
     */
    abstract load(): Promise<void>;
    /**
     * Saves the configuration file to disk.
     */
    abstract save(): Promise<void>;
    /**
     * Gets a configuration value by key.
     */
    get(key: string): any;
    /**
     * Sets a configuration value by key.
     */
    set(key: string, value: ConfigSetting): void;
    /**
     * Clears a configuration value by key.
     */
    clear(key: string): void;
    /**
     * Gets all configuration values.
     */
    getAll(): Record<string, any>;
    /**
     * Checks if the file exists.
     */
    exists(): Promise<boolean>;
    /**
     * Creates a ConfigFile instance from a file path.
     */
    static fromFile(fileName: string, scope: ConfigFileScope): Promise<ConfigFile>;
    /**
     * Gets a nested value from an object using dot notation.
     */
    protected getNestedValue(obj: any, key: string): any;
    /**
     * Sets a nested value in an object using dot notation.
     */
    protected setNestedValue(obj: any, key: string, value: any): void;
    /**
     * Deletes a nested value from an object using dot notation.
     */
    protected deleteNestedValue(obj: any, key: string): void;
}
/**
 * YAML configuration file implementation.
 */
export declare class YamlConfigFile extends ConfigFile {
    load(): Promise<void>;
    save(): Promise<void>;
}
/**
 * INI configuration file implementation (basic).
 */
export declare class IniConfigFile extends ConfigFile {
    load(): Promise<void>;
    save(): Promise<void>;
    private parseIni;
    private stringifyIni;
}
//# sourceMappingURL=ConfigFile.d.ts.map