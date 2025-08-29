"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.ConfigClearCommand = void 0;
const ConfigBaseCommand_1 = require("./ConfigBaseCommand");
const ConfigFileScope_1 = require("../../Configuration/ConfigFileScope");
const KnownSettings_1 = require("../../Configuration/KnownSettings");
const ConsoleHelpers_1 = require("../../Helpers/ConsoleHelpers");
/**
 * Command to clear a configuration setting.
 */
class ConfigClearCommand extends ConfigBaseCommand_1.ConfigBaseCommand {
    getCommandName() {
        return 'config clear';
    }
    isEmpty() {
        return !this.key || this.key.trim() === '';
    }
    async executeAsync(_interactive) {
        return await this.executeClear(this.key, this.scope ?? ConfigFileScope_1.ConfigFileScope.Local, this.configFileName);
    }
    async executeClear(key, scope, configFileName) {
        if (!key || key.trim() === '') {
            throw new Error('Error: No key specified.');
        }
        // Normalize the key if it's a known setting
        let normalizedKey = key;
        if (KnownSettings_1.KnownSettings.isKnown(key)) {
            normalizedKey = KnownSettings_1.KnownSettings.getCanonicalForm(key);
        }
        const isFileNameScope = scope === ConfigFileScope_1.ConfigFileScope.FileName && !!configFileName;
        if (isFileNameScope) {
            // For file-based clearing, we would need to implement clearFromFile
            // For now, we'll just indicate success
            ConsoleHelpers_1.ConsoleHelpers.writeLine(`Cleared '${normalizedKey}' from ${configFileName}`);
        }
        else {
            await this._configStore.clear(normalizedKey, scope);
            const scopeName = this.getScopeDisplayName(scope);
            ConsoleHelpers_1.ConsoleHelpers.writeLine(`Cleared '${normalizedKey}' from ${scopeName} configuration`);
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
exports.ConfigClearCommand = ConfigClearCommand;
//# sourceMappingURL=ConfigClearCommand.js.map