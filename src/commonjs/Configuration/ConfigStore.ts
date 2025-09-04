import { ConfigFile } from './ConfigFile';
import { ConfigValue } from './ConfigValue';
import { ConfigSource } from './ConfigSource';
import { ConfigFileScope } from './ConfigFileScope';
import { ConfigFileHelpers } from './ConfigFileHelpers';
import { KnownSettings } from './KnownSettings';

/**
 * Manages configuration settings across different scopes.
 */
export class ConfigStore {
    public static get Instance(): ConfigStore {
        return ConfigStore._instance;
    }

    protected constructor() {
        this._loadedKnownConfigFiles = false;
        this._commandLineSettings = new Map<string, any>();
        this._configFiles = [];
    }

    private static _instance = new ConfigStore();
    private _loadedKnownConfigFiles: boolean;
    private _commandLineSettings: Map<string, any>;
    private _configFiles: ConfigFile[] = [];

    public LoadConfigFile(fileName: string): void {
        this.EnsureLoaded();

        console.debug(`ConfigStore.LoadConfig; loading config file from ${fileName}`);
        const configFile = ConfigFile.FromFile(fileName, ConfigFileScope.FileName);
        this._configFiles.push(configFile);
    }

    public LoadConfigFiles(fileNames: string[]): void {
        this.EnsureLoaded();

        for (const fileName of fileNames) {
            console.debug(`ConfigStore.LoadConfig; loading config file from ${fileName}`);
            const configFile = ConfigFile.FromFile(fileName, ConfigFileScope.FileName);
            this._configFiles.push(configFile);
        }
    }

    public GetFromAnyScope(key: string): ConfigValue {
        this.EnsureLoaded();

        const dotNotationKey = KnownSettings.ToDotNotation(key);
        const isSecret = KnownSettings.IsSecret(dotNotationKey);

        // 1. First, check command line settings (highest priority)
        if (this._commandLineSettings.has(dotNotationKey)) {
            const cmdLineValue = this._commandLineSettings.get(dotNotationKey);
            console.debug(`ConfigStore.GetFromAnyScope; Found '${dotNotationKey}' in command line settings`);
            return new ConfigValue(cmdLineValue, ConfigSource.CommandLine, isSecret);
        }

        // 2. Try environment variable 
        const envVarKey = KnownSettings.ToEnvironmentVariable(dotNotationKey);
        const envValue = this.TryGetFromEnv(envVarKey);
        if (envValue) {
            console.debug(`ConfigStore.GetFromAnyScope; Found '${dotNotationKey}' in environment variable: ${envVarKey}`);
            return envValue;
        }

        // 3. Then search the non-local, user, and global config files
        for (const configFile of this._configFiles.filter(c => 
            c.Scope !== ConfigFileScope.Local && 
            c.Scope !== ConfigFileScope.User && 
            c.Scope !== ConfigFileScope.Global)) {
            const value = this.GetFromConfig(dotNotationKey, configFile);
            if (!value.IsNotFoundNullOrEmpty()) {
                const source = this.ConfigSourceFromScope(configFile.Scope);
                console.debug(`ConfigStore.GetFromAnyScope; Found '${dotNotationKey}' in specified config file: ${configFile.FileName}`);
                const result = new ConfigValue(value.Value, source, isSecret);
                result.File = configFile;
                return result;
            }
        }

        // 4. Then search the config files in order of priority (Local, User, Global)
        const localConfigFile = this._configFiles.find(c => c.Scope === ConfigFileScope.Local);
        if (localConfigFile) {
            const value = this.GetFromConfig(dotNotationKey, localConfigFile);
            if (!value.IsNotFoundNullOrEmpty()) {
                const source = this.ConfigSourceFromScope(localConfigFile.Scope);
                console.debug(`ConfigStore.GetFromAnyScope; Found '${dotNotationKey}' in local config file: ${localConfigFile.FileName}`);
                const result = new ConfigValue(value.Value, source, isSecret);
                result.File = localConfigFile;
                return result;
            }
        }
        
        const userConfigFile = this._configFiles.find(c => c.Scope === ConfigFileScope.User);
        if (userConfigFile) {
            const value = this.GetFromConfig(dotNotationKey, userConfigFile);
            if (!value.IsNotFoundNullOrEmpty()) {
                const source = this.ConfigSourceFromScope(userConfigFile.Scope);
                console.debug(`ConfigStore.GetFromAnyScope; Found '${dotNotationKey}' in user config file: ${userConfigFile.FileName}`);
                const result = new ConfigValue(value.Value, source, isSecret);
                result.File = userConfigFile;
                return result;
            }
        }
        
        const globalConfigFile = this._configFiles.find(c => c.Scope === ConfigFileScope.Global);
        if (globalConfigFile) {
            const value = this.GetFromConfig(dotNotationKey, globalConfigFile);
            if (!value.IsNotFoundNullOrEmpty()) {
                const source = this.ConfigSourceFromScope(globalConfigFile.Scope);
                console.debug(`ConfigStore.GetFromAnyScope; Found '${dotNotationKey}' in global config file: ${globalConfigFile.FileName}`);
                const result = new ConfigValue(value.Value, source, isSecret);
                result.File = globalConfigFile;
                return result;
            }
        }

        console.debug(`ConfigStore.GetFromAnyScope; Setting '${dotNotationKey}' not found in any scope`);
        return new ConfigValue();
    }

    public GetAllFromScope(scope: ConfigFileScope): Map<string, ConfigValue> {
        this.EnsureLoaded();

        const result = new Map<string, ConfigValue>();
        const configFile = this.ConfigFileFromScope(scope);
        
        if (configFile) {
            this.FlattenSettings(configFile.Settings, '', result, configFile);
        }

        return result;
    }

    public SetInCommandLine(key: string, value: any): void {
        const dotNotationKey = KnownSettings.ToDotNotation(key);
        this._commandLineSettings.set(dotNotationKey, value);
        console.debug(`ConfigStore.SetInCommandLine; Set '${dotNotationKey}' to '${value}'`);
    }

    public Set(key: string, value: any, scope: ConfigFileScope, save: boolean = true): boolean {
        const configFile = this.ConfigFileFromScope(scope, true);
        if (!configFile) return false;
        return this.SetInConfigFile(key, value, configFile, save);
    }

    public SetInConfigFile(key: string, value: any, configFile: ConfigFile, save: boolean = true): boolean {
        this.EnsureLoaded();

        const dotNotationKey = KnownSettings.ToDotNotation(key);
        const keyParts = dotNotationKey.split('.');
        
        this.SetNestedValue(configFile.Settings, keyParts, value);
        
        if (save) {
            configFile.Save();
        }

        console.debug(`ConfigStore.SetInConfigFile; Set '${dotNotationKey}' to '${value}' in ${configFile.FileName}`);
        return true;
    }

    public Clear(key: string, scope: ConfigFileScope, save: boolean = true): boolean {
        const configFile = this.ConfigFileFromScope(scope, true);
        if (!configFile) return false;
        return this.ClearInConfigFile(key, configFile, save);
    }

    public ClearInConfigFile(key: string, configFile: ConfigFile, save: boolean = true): boolean {
        this.EnsureLoaded();

        const dotNotationKey = KnownSettings.ToDotNotation(key);
        const keyParts = dotNotationKey.split('.');
        if (keyParts.length === 0) return false;
        
        if (keyParts.length === 1) {
            if (configFile.Settings.has(keyParts[0])) {
                configFile.Settings.delete(keyParts[0]);
                if (save) configFile.Save();
                return true;
            }
            return false;
        }

        // Navigate to the parent map
        let parent = configFile.Settings;
        for (let i = 0; i < keyParts.length - 1; i++) {
            if (!parent.has(keyParts[i]) || !(parent.get(keyParts[i]) instanceof Map)) {
                return false;
            }
            parent = parent.get(keyParts[i]) as Map<string, any>;
        }

        // Remove the key from the parent
        const lastKey = keyParts[keyParts.length - 1];
        if (parent.has(lastKey)) {
            parent.delete(lastKey);
            if (save) configFile.Save();
            return true;
        }

        return false;
    }

    public AddToList(key: string, value: string, fileName: string, save: boolean = true): boolean {
        const configFile = this._configFiles.find(f => f.FileName === fileName);
        if (!configFile) return false;

        return this.AddToListInConfigFile(key, value, configFile, save);
    }

    public AddToListInConfigFile(key: string, value: string, configFile: ConfigFile, save: boolean = true): boolean {
        const currentValue = this.GetFromConfig(key, configFile);
        const result = currentValue.AddToList(value);
        
        if (result) {
            this.SetInConfigFile(key, currentValue.Value, configFile, save);
        }
        
        return result;
    }

    public RemoveFromList(key: string, value: string, fileName: string, save: boolean = true): boolean {
        const configFile = this._configFiles.find(f => f.FileName === fileName);
        if (!configFile) return false;

        return this.RemoveFromListInConfigFile(key, value, configFile, save);
    }

    public RemoveFromListInConfigFile(key: string, value: string, configFile: ConfigFile, save: boolean = true): boolean {
        const currentValue = this.GetFromConfig(key, configFile);
        const result = currentValue.RemoveFromList(value);
        
        if (result) {
            this.SetInConfigFile(key, currentValue.Value, configFile, save);
        }
        
        return result;
    }

    private EnsureLoaded(): void {
        if (!this._loadedKnownConfigFiles) {
            this.LoadKnownConfigFiles();
            this._loadedKnownConfigFiles = true;
        }
    }

    private LoadKnownConfigFiles(): void {
        const scopes = [ConfigFileScope.Local, ConfigFileScope.User, ConfigFileScope.Global];
        
        for (const scope of scopes) {
            const configFile = this.ConfigFileFromScope(scope);
            if (configFile) {
                this._configFiles.push(configFile);
            }
        }
    }

    private ConfigFileFromScope(scope: ConfigFileScope, forceCreate: boolean = false): ConfigFile | null {
        const existingFile = this._configFiles.find(f => f.Scope === scope);
        if (existingFile) return existingFile;

        const configFileName = ConfigFileHelpers.GetConfigFileName(scope);
        if (!configFileName) return null;

        if (forceCreate) {
            return ConfigFile.FromFile(configFileName, scope);
        }

        const foundFile = ConfigFileHelpers.FindConfigFile(scope);
        return foundFile ? ConfigFile.FromFile(foundFile, scope) : null;
    }

    private GetFromConfig(key: string, configFile: ConfigFile): ConfigValue {
        const dotNotationKey = KnownSettings.ToDotNotation(key);
        const keyParts = dotNotationKey.split('.');
        
        let current: any = configFile.Settings;
        for (const part of keyParts) {
            if (current instanceof Map && current.has(part)) {
                current = current.get(part);
            } else {
                return new ConfigValue();
            }
        }

        const isSecret = KnownSettings.IsSecret(dotNotationKey);
        return new ConfigValue(current, ConfigSource.NotFound, isSecret);
    }

    private TryGetFromEnv(envVarKey: string): ConfigValue | null {
        const value = process.env[envVarKey];
        if (value !== undefined) {
            const dotNotationKey = KnownSettings.ToDotNotation(envVarKey);
            const isSecret = KnownSettings.IsSecret(dotNotationKey);
            return new ConfigValue(value, ConfigSource.EnvironmentVariable, isSecret);
        }
        return null;
    }

    private ConfigSourceFromScope(scope: ConfigFileScope): ConfigSource {
        switch (scope) {
            case ConfigFileScope.Global:
                return ConfigSource.GlobalConfig;
            case ConfigFileScope.User:
                return ConfigSource.UserConfig;
            case ConfigFileScope.Local:
                return ConfigSource.LocalConfig;
            case ConfigFileScope.FileName:
                return ConfigSource.ConfigFileName;
            default:
                return ConfigSource.NotFound;
        }
    }

    private SetNestedValue(settings: Map<string, any>, keyParts: string[], value: any): void {
        if (keyParts.length === 1) {
            settings.set(keyParts[0], value);
            return;
        }

        if (!settings.has(keyParts[0])) {
            settings.set(keyParts[0], new Map<string, any>());
        }

        const nested = settings.get(keyParts[0]);
        if (nested instanceof Map) {
            this.SetNestedValue(nested, keyParts.slice(1), value);
        } else {
            const newMap = new Map<string, any>();
            settings.set(keyParts[0], newMap);
            this.SetNestedValue(newMap, keyParts.slice(1), value);
        }
    }

    private FlattenSettings(settings: Map<string, any>, prefix: string, result: Map<string, ConfigValue>, configFile: ConfigFile): void {
        for (const [key, value] of settings) {
            const fullKey = prefix ? `${prefix}.${key}` : key;
            
            if (value instanceof Map) {
                this.FlattenSettings(value, fullKey, result, configFile);
            } else {
                const isSecret = KnownSettings.IsSecret(fullKey);
                const source = this.ConfigSourceFromScope(configFile.Scope);
                const configValue = new ConfigValue(value, source, isSecret);
                configValue.File = configFile;
                result.set(fullKey, configValue);
            }
        }
    }
}