"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.ConfigGetCommand = void 0;
const ConfigBaseCommand_1 = require("./ConfigBaseCommand");
const ConfigFileScope_1 = require("../../Configuration/ConfigFileScope");
const ConfigValue_1 = require("../../Configuration/ConfigValue");
const KnownSettings_1 = require("../../Configuration/KnownSettings");
const ConsoleHelpers_1 = require("../../Helpers/ConsoleHelpers");
const PathHelpers_1 = require("../../Helpers/PathHelpers");
/**
 * Command to get a configuration setting.
 */
class ConfigGetCommand extends ConfigBaseCommand_1.ConfigBaseCommand {
    getCommandName() {
        return 'config get';
    }
    isEmpty() {
        return !this.key || this.key.trim() === '';
    }
    async executeAsync(_interactive) {
        return await this.executeGet(this.key, this.scope ?? ConfigFileScope_1.ConfigFileScope.Any, this.configFileName);
    }
    async executeGet(key, scope, configFileName) {
        ConsoleHelpers_1.ConsoleHelpers.writeDebugLine(`ExecuteGet; key: ${key}; scope: ${scope}; configFileName: ${configFileName}`);
        if (!key || key.trim() === '') {
            throw new Error('Error: No key specified.');
        }
        // Normalize the key if it's a known setting
        let normalizedKey = key;
        if (KnownSettings_1.KnownSettings.isKnown(key)) {
            normalizedKey = KnownSettings_1.KnownSettings.getCanonicalForm(key);
        }
        const isFileNameScope = scope === ConfigFileScope_1.ConfigFileScope.FileName && !!configFileName;
        const value = isFileNameScope
            ? await this._configStore.getFromFileName(normalizedKey, configFileName)
            : await this._configStore.getFromScope(normalizedKey, scope);
        this.displayConfigValue(normalizedKey, value);
        return 0;
    }
    displayConfigValue(key, configValue) {
        if (!ConfigValue_1.ConfigValueHelpers.hasValue(configValue)) {
            // For missing values, show like the C# version
            ConsoleHelpers_1.ConsoleHelpers.writeLine(`${key}: (not found or empty)`);
            return;
        }
        // Show location header first (like C# WriteLocationHeader)
        const locationPath = this.getLocationPath(configValue);
        if (locationPath) {
            ConsoleHelpers_1.ConsoleHelpers.displayLocationHeader(locationPath);
        }
        // Then show indented key: value (like C# DisplayConfigValue)
        const displayValue = ConfigValue_1.ConfigValueHelpers.getDisplayValue(configValue);
        ConsoleHelpers_1.ConsoleHelpers.writeLine(`  ${key}: ${displayValue}`, true);
    }
    getLocationPath(configValue) {
        if (!configValue || !configValue.scope)
            return '';
        switch (configValue.scope) {
            case ConfigFileScope_1.ConfigFileScope.Global:
                return `${PathHelpers_1.PathHelpers.getConfigFilePath('global')} (global)`;
            case ConfigFileScope_1.ConfigFileScope.User:
                return `${PathHelpers_1.PathHelpers.getConfigFilePath('user')} (user)`;
            case ConfigFileScope_1.ConfigFileScope.Local:
                return `${PathHelpers_1.PathHelpers.getConfigFilePath('local')} (local)`;
            case ConfigFileScope_1.ConfigFileScope.FileName:
                return `${configValue.source} (specified)`;
            default:
                return `${configValue.source} (${configValue.scope})`;
        }
    }
}
exports.ConfigGetCommand = ConfigGetCommand;
//# sourceMappingURL=ConfigGetCommand.js.map