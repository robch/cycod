"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.ConfigBaseCommand = void 0;
const ConfigStore_1 = require("../../Configuration/ConfigStore");
/**
 * Base class for configuration commands.
 */
class ConfigBaseCommand {
    constructor() {
        this._configStore = ConfigStore_1.ConfigStore.instance;
    }
    /**
     * Checks if the command has all required parameters.
     */
    isEmpty() {
        return false;
    }
}
exports.ConfigBaseCommand = ConfigBaseCommand;
//# sourceMappingURL=ConfigBaseCommand.js.map