"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.ConfigListCommand = void 0;
const ConfigBaseCommand_1 = require("./ConfigBaseCommand");
const ConfigFileScope_1 = require("../../Configuration/ConfigFileScope");
const ConsoleHelpers_1 = require("../../Helpers/ConsoleHelpers");
const PathHelpers_1 = require("../../Helpers/PathHelpers");
/**
 * Command to list configuration settings.
 */
class ConfigListCommand extends ConfigBaseCommand_1.ConfigBaseCommand {
    constructor() {
        super();
        this.scope = ConfigFileScope_1.ConfigFileScope.Any;
    }
    getCommandName() {
        return 'config list';
    }
    async executeAsync(_interactive) {
        return this.scope === ConfigFileScope_1.ConfigFileScope.FileName && this.configFileName
            ? await this.executeList(this.configFileName)
            : await this.executeList(this.scope ?? ConfigFileScope_1.ConfigFileScope.Any);
    }
    async executeList(scopeOrFileName) {
        // Check if it's a known ConfigFileScope enum value
        const isScope = Object.values(ConfigFileScope_1.ConfigFileScope).includes(scopeOrFileName);
        if (!isScope) {
            await this.displayConfigSettings(scopeOrFileName);
            return 0;
        }
        const scope = scopeOrFileName;
        const isAnyScope = scope === ConfigFileScope_1.ConfigFileScope.Any;
        if (isAnyScope || scope === ConfigFileScope_1.ConfigFileScope.Global) {
            await this.displayConfigSettings(ConfigFileScope_1.ConfigFileScope.Global);
            if (isAnyScope)
                ConsoleHelpers_1.ConsoleHelpers.writeLine('', true);
        }
        if (isAnyScope || scope === ConfigFileScope_1.ConfigFileScope.User) {
            await this.displayConfigSettings(ConfigFileScope_1.ConfigFileScope.User);
            if (isAnyScope)
                ConsoleHelpers_1.ConsoleHelpers.writeLine('', true);
        }
        if (isAnyScope || scope === ConfigFileScope_1.ConfigFileScope.Local) {
            await this.displayConfigSettings(ConfigFileScope_1.ConfigFileScope.Local);
            if (isAnyScope)
                ConsoleHelpers_1.ConsoleHelpers.writeLine('', true);
        }
        if (isAnyScope) {
            const commandLineValues = this._configStore.listFromCommandLineSettings();
            if (Object.keys(commandLineValues).length > 0) {
                const location = 'Command line (specified)';
                ConsoleHelpers_1.ConsoleHelpers.displayConfigSettings(location, commandLineValues);
                ConsoleHelpers_1.ConsoleHelpers.writeLine('', true);
            }
        }
        return 0;
    }
    async displayConfigSettings(scopeOrFileName) {
        // Check if it's a known ConfigFileScope enum value
        const isScope = Object.values(ConfigFileScope_1.ConfigFileScope).includes(scopeOrFileName);
        if (!isScope) {
            const location = `${scopeOrFileName} (specified)`;
            // For file-based listing, we would need to implement listValuesFromFile
            // For now, we'll skip this implementation
            ConsoleHelpers_1.ConsoleHelpers.displayConfigSettings(location, {});
        }
        else {
            const scope = scopeOrFileName;
            const location = this.getScopeDisplayName(scope);
            const values = await this._configStore.listValuesFromScope(scope);
            ConsoleHelpers_1.ConsoleHelpers.displayConfigSettings(location, values);
        }
    }
    getScopeDisplayName(scope) {
        const configPath = PathHelpers_1.PathHelpers.getConfigFilePath(scope);
        return `${configPath} (${scope.toString().toLowerCase()})`;
    }
}
exports.ConfigListCommand = ConfigListCommand;
//# sourceMappingURL=ConfigListCommand.js.map