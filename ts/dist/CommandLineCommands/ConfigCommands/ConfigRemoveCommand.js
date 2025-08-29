"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.ConfigRemoveCommand = void 0;
const ConfigBaseCommand_1 = require("./ConfigBaseCommand");
const ConfigFileScope_1 = require("../../Configuration/ConfigFileScope");
const KnownSettings_1 = require("../../Configuration/KnownSettings");
const ConsoleHelpers_1 = require("../../Helpers/ConsoleHelpers");
/**
 * Command to remove a value from a list configuration setting.
 */
class ConfigRemoveCommand extends ConfigBaseCommand_1.ConfigBaseCommand {
    getCommandName() {
        return 'config remove';
    }
    isEmpty() {
        return !this.key || this.key.trim() === '' || this.value === undefined;
    }
    async executeAsync(_interactive) {
        return await this.executeRemove(this.key, this.value, this.scope ?? ConfigFileScope_1.ConfigFileScope.Local);
    }
    async executeRemove(key, value, scope) {
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
        await this._configStore.removeFromList(normalizedKey, value, scope);
        const scopeName = this.getScopeDisplayName(scope);
        ConsoleHelpers_1.ConsoleHelpers.writeLine(`Removed '${value}' from '${normalizedKey}' in ${scopeName} configuration`);
        // Display the updated list
        const updatedValue = await this._configStore.getFromScope(normalizedKey, scope);
        if (updatedValue && Array.isArray(updatedValue.value)) {
            ConsoleHelpers_1.ConsoleHelpers.writeLine(`${normalizedKey}=[${updatedValue.value.join(', ')}]`);
        }
        else {
            ConsoleHelpers_1.ConsoleHelpers.writeLine(`${normalizedKey}= (empty)`);
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
exports.ConfigRemoveCommand = ConfigRemoveCommand;
//# sourceMappingURL=ConfigRemoveCommand.js.map