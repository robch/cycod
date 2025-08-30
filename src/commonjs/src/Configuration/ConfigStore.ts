import { ConfigFile } from './ConfigFile';
import { ConfigValue } from './ConfigValue';
import { ConfigSource } from './ConfigSource';
import { ConfigFileScope } from './ConfigFileScope';

export class ConfigStore {
    private static _instance: ConfigStore = new ConfigStore();
    private _configFiles: ConfigFile[] = [];
    private _commandLineSettings: Map<string, any> = new Map();
    private _loadedKnownConfigFiles: boolean = false;

    static get instance(): ConfigStore {
        return ConfigStore._instance;
    }

    protected constructor() {}

    loadConfigFile(fileName: string): void {
        this.ensureLoaded();
        const configFile = ConfigFile.fromFile(fileName, ConfigFileScope.FileName);
        this._configFiles.push(configFile);
    }

    loadConfigFiles(fileNames: string[]): void {
        this.ensureLoaded();
        for (const fileName of fileNames) {
            const configFile = ConfigFile.fromFile(fileName, ConfigFileScope.FileName);
            this._configFiles.push(configFile);
        }
    }

    setFromCommandLine(key: string, value: any): void {
        const dotNotationKey = this.toDotNotation(key);
        this._commandLineSettings.set(dotNotationKey, value);
    }

    getFromAnyScope(key: string): ConfigValue {
        this.ensureLoaded();
        
        const dotNotationKey = this.toDotNotation(key);
        const isSecret = this.isSecret(dotNotationKey);

        // 1. Check command line settings (highest priority)
        if (this._commandLineSettings.has(dotNotationKey)) {
            const value = this._commandLineSettings.get(dotNotationKey);
            return new ConfigValue(value, ConfigSource.CommandLine, isSecret);
        }

        // 2. Check environment variables
        const envVarKey = this.toEnvironmentVariable(dotNotationKey);
        const envValue = process.env[envVarKey];
        if (envValue !== undefined) {
            return new ConfigValue(envValue, ConfigSource.EnvironmentVariable, isSecret);
        }

        // 3. Check config files in priority order
        const fileScopes = [
            ConfigFileScope.FileName,
            ConfigFileScope.Local,
            ConfigFileScope.User,
            ConfigFileScope.Global
        ];

        for (const scope of fileScopes) {
            const configFile = this._configFiles.find(c => c.scope === scope);
            if (configFile) {
                const value = this.getFromConfig(dotNotationKey, configFile);
                if (!value.isNotFoundNullOrEmpty()) {
                    const source = this.configSourceFromScope(scope);
                    const configValue = new ConfigValue(value.value, source, isSecret);
                    configValue.file = configFile;
                    return configValue;
                }
            }
        }

        // Not found
        return new ConfigValue(null, ConfigSource.NotFound, isSecret);
    }

    private getFromConfig(key: string, configFile: ConfigFile): ConfigValue {
        const settings = configFile.settings;
        const value = this.getNestedValue(settings, key.split('.'));
        
        if (value !== undefined) {
            const source = this.configSourceFromScope(configFile.scope);
            return new ConfigValue(value, source);
        }

        return new ConfigValue(null, ConfigSource.NotFound);
    }

    private getNestedValue(obj: any, keyParts: string[]): any {
        if (keyParts.length === 0 || obj == null) {
            return undefined;
        }

        if (keyParts.length === 1) {
            return obj[keyParts[0]];
        }

        return this.getNestedValue(obj[keyParts[0]], keyParts.slice(1));
    }

    private configSourceFromScope(scope: ConfigFileScope): ConfigSource {
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
                return ConfigSource.Default;
        }
    }

    private ensureLoaded(): void {
        if (!this._loadedKnownConfigFiles) {
            this._loadedKnownConfigFiles = true;
            // TODO: Load known config files based on scope
        }
    }

    private toDotNotation(key: string): string {
        // Simple implementation - in reality this would be more complex
        return key.replace(/([A-Z])/g, '.$1').toLowerCase().replace(/^\./, '');
    }

    private toEnvironmentVariable(key: string): string {
        return key.toUpperCase().replace(/\./g, '_');
    }

    private isSecret(key: string): boolean {
        // Simple implementation - check if key contains common secret patterns
        const secretPatterns = ['password', 'key', 'token', 'secret'];
        const lowerKey = key.toLowerCase();
        return secretPatterns.some(pattern => lowerKey.includes(pattern));
    }
}