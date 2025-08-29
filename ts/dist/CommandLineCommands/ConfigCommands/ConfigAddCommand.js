"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.ConfigAddCommand = void 0;
const ConfigBaseCommand_1 = require("./ConfigBaseCommand");
const ConfigFileScope_1 = require("../../Configuration/ConfigFileScope");
const KnownSettings_1 = require("../../Configuration/KnownSettings");
const ConsoleHelpers_1 = require("../../Helpers/ConsoleHelpers");
/**
 * Command to add a value to a list configuration setting.
 */
class ConfigAddCommand extends ConfigBaseCommand_1.ConfigBaseCommand {
    getCommandName() {
        return 'config add';
    }
    isEmpty() {
        return !this.key || this.key.trim() === '' || this.value === undefined;
    }
    async executeAsync(_interactive) {
        return await this.executeAdd(this.key, this.value, this.scope ?? ConfigFileScope_1.ConfigFileScope.Local);
    }
    async executeAdd(key, value, scope) {
        if (!key || key.trim() === '') {
            throw new Error('Error: No key specified.');
        }
        if (value === undefined) {
            throw new Error('Error: No value specified.');
        }
        // Normalize the key if it's a known setting
        let normalizedKey = key;
        if (KnownSettings_1.KnownSettings.isKnown(key)) {
            normalizedKey = KnownSettings_1.KnownSettings.getCanonicalForm(key);
        }
        await this._configStore.addToList(normalizedKey, value, scope);
        const scopeName = this.getScopeDisplayName(scope);
        ConsoleHelpers_1.ConsoleHelpers.writeLine(`Added '${value}' to '${normalizedKey}' in ${scopeName} configuration`);
        // Display the updated list
        const updatedValue = await this._configStore.getFromScope(normalizedKey, scope);
        if (updatedValue && Array.isArray(updatedValue.value)) {
            ConsoleHelpers_1.ConsoleHelpers.writeLine(`${normalizedKey}=[${updatedValue.value.join(', ')}]`);
        }
        return 0;
    }
    getScopeDisplayName(scope) {
        switch (scope) {
            case ConfigFileScope_1.ConfigFileScope.Global:
                return 'Global';
            case ConfigFileScope_1.ConfigFileScope.User:
                return 'User';
            case ConfigFileScope_1.ConfigFileScope.Local:
                return 'Local';
            default:
                return 'Unknown';
        }
    }
}
exports.ConfigAddCommand = ConfigAddCommand;
//# sourceMappingURL=ConfigAddCommand.js.map