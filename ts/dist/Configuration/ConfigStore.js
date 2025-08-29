"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.ConfigStore = void 0;
const ConfigFile_1 = require("./ConfigFile");
const ConfigFileScope_1 = require("./ConfigFileScope");
const ConfigValue_1 = require("./ConfigValue");
const PathHelpers_1 = require("../Helpers/PathHelpers");
const KnownSettings_1 = require("./KnownSettings");
/**
 * Manages configuration settings across different scopes.
 */
class ConfigStore {
    constructor() {
        this._configFiles = [];
        this._commandLineSettings = new Map();
        this._loadedKnownConfigFiles = false;
        // Private constructor for singleton
    }
    /**
     * Gets the singleton instance of ConfigStore.
     */
    static get instance() {
        if (!ConfigStore._instance) {
            ConfigStore._instance = new ConfigStore();
        }
        return ConfigStore._instance;
    }
    /**
     * Loads a configuration file from the specified path.
     */
    async loadConfigFile(fileName) {
        await this.ensureLoaded();
        const configFile = await ConfigFile_1.ConfigFile.fromFile(fileName, ConfigFileScope_1.ConfigFileScope.FileName);
        this._configFiles.push(configFile);
    }
    /**
     * Loads multiple configuration files.
     */
    async loadConfigFiles(fileNames) {
        await this.ensureLoaded();
        for (const fileName of fileNames) {
            const configFile = await ConfigFile_1.ConfigFile.fromFile(fileName, ConfigFileScope_1.ConfigFileScope.FileName);
            this._configFiles.push(configFile);
        }
    }
    /**
     * Gets a configuration value from any scope (command line -> filename -> local -> user -> global).
     */
    async getFromAnyScope(key) {
        await this.ensureLoaded();
        const dotNotationKey = KnownSettings_1.KnownSettings.toDotNotation(key);
        const isSecret = KnownSettings_1.KnownSettings.isSecret(dotNotationKey);
        // 1. Check command line settings (highest priority)
        if (this._commandLineSettings.has(dotNotationKey)) {
            const value = this._commandLineSettings.get(dotNotationKey);
            return ConfigValue_1.ConfigValueHelpers.create(value, ConfigFileScope_1.ConfigFileScope.Any, 'Command line', isSecret);
        }
        // 2. Check filename scope files
        for (const configFile of this._configFiles) {
            if (configFile.scope === ConfigFileScope_1.ConfigFileScope.FileName) {
                const value = configFile.get(dotNotationKey);
                if (value !== undefined) {
                    return ConfigValue_1.ConfigValueHelpers.create(value, ConfigFileScope_1.ConfigFileScope.FileName, configFile.fileName, isSecret);
                }
            }
        }
        // 3. Check local scope
        const localValue = await this.getFromScope(dotNotationKey, ConfigFileScope_1.ConfigFileScope.Local);
        if (ConfigValue_1.ConfigValueHelpers.hasValue(localValue)) {
            return localValue;
        }
        // 4. Check user scope
        const userValue = await this.getFromScope(dotNotationKey, ConfigFileScope_1.ConfigFileScope.User);
        if (ConfigValue_1.ConfigValueHelpers.hasValue(userValue)) {
            return userValue;
        }
        // 5. Check global scope
        const globalValue = await this.getFromScope(dotNotationKey, ConfigFileScope_1.ConfigFileScope.Global);
        if (ConfigValue_1.ConfigValueHelpers.hasValue(globalValue)) {
            return globalValue;
        }
        return undefined;
    }
    /**
     * Gets a configuration value from a specific scope.
     */
    async getFromScope(key, scope) {
        // Match C# behavior: if scope is Any, redirect to getFromAnyScope
        if (scope === ConfigFileScope_1.ConfigFileScope.Any) {
            return this.getFromAnyScope(key);
        }
        await this.ensureLoaded();
        const dotNotationKey = KnownSettings_1.KnownSettings.toDotNotation(key);
        const isSecret = KnownSettings_1.KnownSettings.isSecret(dotNotationKey);
        const configFile = this._configFiles.find(cf => cf.scope === scope);
        if (configFile) {
            const value = configFile.get(dotNotationKey);
            if (value !== undefined) {
                const source = this.getScopeDisplayName(scope);
                return ConfigValue_1.ConfigValueHelpers.create(value, scope, source, isSecret);
            }
        }
        return undefined;
    }
    /**
     * Gets a configuration value from a specific file.
     */
    async getFromFileName(key, fileName) {
        await this.ensureLoaded();
        const dotNotationKey = KnownSettings_1.KnownSettings.toDotNotation(key);
        const isSecret = KnownSettings_1.KnownSettings.isSecret(dotNotationKey);
        const configFile = this._configFiles.find(cf => cf.scope === ConfigFileScope_1.ConfigFileScope.FileName && cf.fileName === fileName);
        if (configFile) {
            const value = configFile.get(dotNotationKey);
            if (value !== undefined) {
                return ConfigValue_1.ConfigValueHelpers.create(value, ConfigFileScope_1.ConfigFileScope.FileName, fileName, isSecret);
            }
        }
        return undefined;
    }
    /**
     * Sets a configuration value in a specific scope.
     */
    async set(key, value, scope, createIfMissing = true) {
        await this.ensureLoaded();
        const dotNotationKey = KnownSettings_1.KnownSettings.toDotNotation(key);
        let configFile = this._configFiles.find(cf => cf.scope === scope);
        if (!configFile && createIfMissing) {
            const fileName = PathHelpers_1.PathHelpers.getConfigFilePath(scope);
            configFile = await ConfigFile_1.ConfigFile.fromFile(fileName, scope);
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
    async setInFile(key, value, fileName) {
        await this.ensureLoaded();
        const dotNotationKey = KnownSettings_1.KnownSettings.toDotNotation(key);
        let configFile = this._configFiles.find(cf => cf.scope === ConfigFileScope_1.ConfigFileScope.FileName && cf.fileName === fileName);
        if (!configFile) {
            configFile = await ConfigFile_1.ConfigFile.fromFile(fileName, ConfigFileScope_1.ConfigFileScope.FileName);
            this._configFiles.push(configFile);
        }
        configFile.set(dotNotationKey, value);
        await configFile.save();
    }
    /**
     * Clears a configuration value from a specific scope.
     */
    async clear(key, scope) {
        await this.ensureLoaded();
        const dotNotationKey = KnownSettings_1.KnownSettings.toDotNotation(key);
        const configFile = this._configFiles.find(cf => cf.scope === scope);
        if (configFile) {
            configFile.clear(dotNotationKey);
            await configFile.save();
        }
    }
    /**
     * Adds a value to a list setting.
     */
    async addToList(key, value, scope) {
        const currentValue = await this.getFromScope(key, scope);
        let newList;
        if (ConfigValue_1.ConfigValueHelpers.hasValue(currentValue)) {
            if (Array.isArray(currentValue.value)) {
                newList = [...currentValue.value, value];
            }
            else {
                newList = [String(currentValue.value), value];
            }
        }
        else {
            newList = [value];
        }
        await this.set(key, newList, scope);
    }
    /**
     * Removes a value from a list setting.
     */
    async removeFromList(key, value, scope) {
        const currentValue = await this.getFromScope(key, scope);
        if (ConfigValue_1.ConfigValueHelpers.hasValue(currentValue) && Array.isArray(currentValue.value)) {
            const newList = currentValue.value.filter(item => item !== value);
            if (newList.length > 0) {
                await this.set(key, newList, scope);
            }
            else {
                await this.clear(key, scope);
            }
        }
    }
    /**
     * Lists all configuration values from a specific scope.
     */
    async listValuesFromScope(scope) {
        await this.ensureLoaded();
        const result = {};
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
    listFromCommandLineSettings() {
        const result = {};
        for (const [key, value] of this._commandLineSettings.entries()) {
            const isSecret = KnownSettings_1.KnownSettings.isSecret(key);
            result[key] = ConfigValue_1.ConfigValueHelpers.create(value, ConfigFileScope_1.ConfigFileScope.Any, 'Command line', isSecret);
        }
        return result;
    }
    /**
     * Sets a command line setting.
     */
    setCommandLineSetting(key, value) {
        const dotNotationKey = KnownSettings_1.KnownSettings.toDotNotation(key);
        this._commandLineSettings.set(dotNotationKey, value);
    }
    /**
     * Ensures that known configuration files are loaded.
     */
    async ensureLoaded() {
        if (this._loadedKnownConfigFiles) {
            return;
        }
        // Load known configuration files in order (global, user, local)
        const scopes = [ConfigFileScope_1.ConfigFileScope.Global, ConfigFileScope_1.ConfigFileScope.User, ConfigFileScope_1.ConfigFileScope.Local];
        for (const scope of scopes) {
            try {
                const fileName = PathHelpers_1.PathHelpers.getConfigFilePath(scope);
                const configFile = await ConfigFile_1.ConfigFile.fromFile(fileName, scope);
                this._configFiles.push(configFile);
                // Config file loaded successfully
            }
            catch (error) {
                // Continue - file may not exist or be accessible
                // Continue - file may not exist
            }
        }
        this._loadedKnownConfigFiles = true;
    }
    /**
     * Gets the display name for a configuration scope.
     */
    getScopeDisplayName(scope) {
        switch (scope) {
            case ConfigFileScope_1.ConfigFileScope.Global:
                return 'Global';
            case ConfigFileScope_1.ConfigFileScope.User:
                return 'User';
            case ConfigFileScope_1.ConfigFileScope.Local:
                return 'Local';
            case ConfigFileScope_1.ConfigFileScope.FileName:
                return 'File';
            default:
                return 'Unknown';
        }
    }
    /**
     * Flattens a nested object into dot notation keys.
     */
    flattenObject(obj, prefix, result, scope, source) {
        for (const [key, value] of Object.entries(obj)) {
            const fullKey = prefix ? `${prefix}.${key}` : key;
            const isSecret = KnownSettings_1.KnownSettings.isSecret(fullKey);
            if (value !== null && typeof value === 'object' && !Array.isArray(value)) {
                this.flattenObject(value, fullKey, result, scope, source);
            }
            else {
                result[fullKey] = ConfigValue_1.ConfigValueHelpers.create(value, scope, source, isSecret);
            }
        }
    }
}
exports.ConfigStore = ConfigStore;
//# sourceMappingURL=ConfigStore.js.map