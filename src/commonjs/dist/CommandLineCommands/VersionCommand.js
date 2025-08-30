"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.VersionCommand = void 0;
const Command_1 = require("../CommandLine/Command");
class VersionCommand extends Command_1.Command {
    constructor() {
        super();
    }
    getCommandName() {
        return "version";
    }
    isEmpty() {
        return false;
    }
    async executeAsync(interactive) {
        // TODO: Implement version display logic
        console.log("Version: 1.0.0"); // Placeholder
        return 0;
    }
}
exports.VersionCommand = VersionCommand;
//# sourceMappingURL=VersionCommand.js.map