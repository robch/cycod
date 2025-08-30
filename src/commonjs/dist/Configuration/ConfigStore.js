"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.ConfigStore = void 0;
const ConfigFile_1 = require("./ConfigFile");
const ConfigValue_1 = require("./ConfigValue");
const ConfigSource_1 = require("./ConfigSource");
const ConfigFileScope_1 = require("./ConfigFileScope");
class ConfigStore {
    static get instance() {
        return ConfigStore._instance;
    }
    constructor() {
        this._configFiles = [];
        this._commandLineSettings = new Map();
        this._loadedKnownConfigFiles = false;
    }
    loadConfigFile(fileName) {
        this.ensureLoaded();
        const configFile = ConfigFile_1.ConfigFile.fromFile(fileName, ConfigFileScope_1.ConfigFileScope.FileName);
        this._configFiles.push(configFile);
    }
    loadConfigFiles(fileNames) {
        this.ensureLoaded();
        for (const fileName of fileNames) {
            const configFile = ConfigFile_1.ConfigFile.fromFile(fileName, ConfigFileScope_1.ConfigFileScope.FileName);
            this._configFiles.push(configFile);
        }
    }
    setFromCommandLine(key, value) {
        const dotNotationKey = this.toDotNotation(key);
        this._commandLineSettings.set(dotNotationKey, value);
    }
    getFromAnyScope(key) {
        this.ensureLoaded();
        const dotNotationKey = this.toDotNotation(key);
        const isSecret = this.isSecret(dotNotationKey);
        // 1. Check command line settings (highest priority)
        if (this._commandLineSettings.has(dotNotationKey)) {
            const value = this._commandLineSettings.get(dotNotationKey);
            return new ConfigValue_1.ConfigValue(value, ConfigSource_1.ConfigSource.CommandLine, isSecret);
        }
        // 2. Check environment variables
        const envVarKey = this.toEnvironmentVariable(dotNotationKey);
        const envValue = process.env[envVarKey];
        if (envValue !== undefined) {
            return new ConfigValue_1.ConfigValue(envValue, ConfigSource_1.ConfigSource.EnvironmentVariable, isSecret);
        }
        // 3. Check config files in priority order
        const fileScopes = [
            ConfigFileScope_1.ConfigFileScope.FileName,
            ConfigFileScope_1.ConfigFileScope.Local,
            ConfigFileScope_1.ConfigFileScope.User,
            ConfigFileScope_1.ConfigFileScope.Global
        ];
        for (const scope of fileScopes) {
            const configFile = this._configFiles.find(c => c.scope === scope);
            if (configFile) {
                const value = this.getFromConfig(dotNotationKey, configFile);
                if (!value.isNotFoundNullOrEmpty()) {
                    const source = this.configSourceFromScope(scope);
                    const configValue = new ConfigValue_1.ConfigValue(value.value, source, isSecret);
                    configValue.file = configFile;
                    return configValue;
                }
            }
        }
        // Not found
        return new ConfigValue_1.ConfigValue(null, ConfigSource_1.ConfigSource.NotFound, isSecret);
    }
    getFromConfig(key, configFile) {
        const settings = configFile.settings;
        const value = this.getNestedValue(settings, key.split('.'));
        if (value !== undefined) {
            const source = this.configSourceFromScope(configFile.scope);
            return new ConfigValue_1.ConfigValue(value, source);
        }
        return new ConfigValue_1.ConfigValue(null, ConfigSource_1.ConfigSource.NotFound);
    }
    getNestedValue(obj, keyParts) {
        if (keyParts.length === 0 || obj == null) {
            return undefined;
        }
        if (keyParts.length === 1) {
            return obj[keyParts[0]];
        }
        return this.getNestedValue(obj[keyParts[0]], keyParts.slice(1));
    }
    configSourceFromScope(scope) {
        switch (scope) {
            case ConfigFileScope_1.ConfigFileScope.Global:
                return ConfigSource_1.ConfigSource.GlobalConfig;
            case ConfigFileScope_1.ConfigFileScope.User:
                return ConfigSource_1.ConfigSource.UserConfig;
            case ConfigFileScope_1.ConfigFileScope.Local:
                return ConfigSource_1.ConfigSource.LocalConfig;
            case ConfigFileScope_1.ConfigFileScope.FileName:
                return ConfigSource_1.ConfigSource.ConfigFileName;
            default:
                return ConfigSource_1.ConfigSource.Default;
        }
    }
    ensureLoaded() {
        if (!this._loadedKnownConfigFiles) {
            this._loadedKnownConfigFiles = true;
            // TODO: Load known config files based on scope
        }
    }
    toDotNotation(key) {
        // Simple implementation - in reality this would be more complex
        return key.replace(/([A-Z])/g, '.$1').toLowerCase().replace(/^\./, '');
    }
    toEnvironmentVariable(key) {
        return key.toUpperCase().replace(/\./g, '_');
    }
    isSecret(key) {
        // Simple implementation - check if key contains common secret patterns
        const secretPatterns = ['password', 'key', 'token', 'secret'];
        const lowerKey = key.toLowerCase();
        return secretPatterns.some(pattern => lowerKey.includes(pattern));
    }
}
exports.ConfigStore = ConfigStore;
ConfigStore._instance = new ConfigStore();
//# sourceMappingURL=ConfigStore.js.map