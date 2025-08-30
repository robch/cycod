"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.HelpCommand = void 0;
const Command_1 = require("../CommandLine/Command");
class HelpCommand extends Command_1.Command {
    constructor() {
        super();
    }
    getCommandName() {
        return "help";
    }
    isEmpty() {
        return false;
    }
    async executeAsync(interactive) {
        throw new Error('Not implemented');
    }
}
exports.HelpCommand = HelpCommand;
//# sourceMappingURL=HelpCommand.js.map