"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.ConfigSetCommand = void 0;
const ConfigBaseCommand_1 = require("./ConfigBaseCommand");
const ConfigFileScope_1 = require("../../Configuration/ConfigFileScope");
const ConfigValue_1 = require("../../Configuration/ConfigValue");
const KnownSettings_1 = require("../../Configuration/KnownSettings");
const ConsoleHelpers_1 = require("../../Helpers/ConsoleHelpers");
/**
 * Command to set a configuration setting.
 */
class ConfigSetCommand extends ConfigBaseCommand_1.ConfigBaseCommand {
    getCommandName() {
        return 'config set';
    }
    isEmpty() {
        return !this.key || this.key.trim() === '' || this.value === undefined;
    }
    async executeAsync(_interactive) {
        return await this.executeSet(this.key, this.value, this.scope ?? ConfigFileScope_1.ConfigFileScope.Local, this.configFileName);
    }
    async executeSet(key, value, scope, configFileName) {
        if (!key || key.trim() === '') {
            throw new Error('Error: No key specified.');
        }
        if (value === undefined) {
            throw new Error('Error: No value specified.');
        }
        // Validate and normalize the key against known settings
        let normalizedKey = key;
        if (!KnownSettings_1.KnownSettings.isKnown(key)) {
            const allKnownSettings = KnownSettings_1.KnownSettings.getAllKnownSettings().sort();
            ConsoleHelpers_1.ConsoleHelpers.writeWarningLine(`Warning: Unknown setting '${key}'. Valid settings are: ${allKnownSettings.join(', ')}`);
            // Continue with the original key without normalization
        }
        else {
            // Use the canonical form for storage of known settings
            normalizedKey = KnownSettings_1.KnownSettings.getCanonicalForm(key);
        }
        // Try to parse as a list if the value is enclosed in brackets
        if (value.startsWith('[') && value.endsWith(']')) {
            const listContent = value.substring(1, value.length - 2);
            const listValue = [];
            if (listContent.trim() !== '') {
                const items = listContent.split(',');
                for (const item of items) {
                    listValue.push(item.trim());
                }
            }
            const isFileNameScope = scope === ConfigFileScope_1.ConfigFileScope.FileName && !!configFileName;
            if (isFileNameScope) {
                await this._configStore.setInFile(normalizedKey, listValue, configFileName);
            }
            else {
                await this._configStore.set(normalizedKey, listValue, scope, true);
            }
            this.displayList(normalizedKey, listValue);
        }
        else {
            const isFileNameScope = scope === ConfigFileScope_1.ConfigFileScope.FileName && !!configFileName;
            if (isFileNameScope) {
                await this._configStore.setInFile(normalizedKey, value, configFileName);
            }
            else {
                await this._configStore.set(normalizedKey, value, scope, true);
            }
            const configValue = isFileNameScope
                ? await this._configStore.getFromFileName(normalizedKey, configFileName)
                : await this._configStore.getFromScope(normalizedKey, scope);
            this.displayConfigValue(normalizedKey, configValue);
        }
        return 0;
    }
    displayConfigValue(key, value) {
        if (!ConfigValue_1.ConfigValueHelpers.hasValue(value)) {
            ConsoleHelpers_1.ConsoleHelpers.writeLine(`${key}= (not set)`);
            return;
        }
        const displayValue = ConfigValue_1.ConfigValueHelpers.getDisplayValue(value);
        const location = ConfigValue_1.ConfigValueHelpers.getLocationDisplayName(value);
        ConsoleHelpers_1.ConsoleHelpers.writeLine(`${key}=${displayValue} (${location})`);
    }
    displayList(key, list) {
        ConsoleHelpers_1.ConsoleHelpers.writeLine(`${key}=[${list.join(', ')}]`);
    }
}
exports.ConfigSetCommand = ConfigSetCommand;
//# sourceMappingURL=ConfigSetCommand.js.map