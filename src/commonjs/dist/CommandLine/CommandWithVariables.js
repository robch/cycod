"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.CommandWithVariables = void 0;
const Command_1 = require("./Command");
class CommandWithVariables extends Command_1.Command {
    constructor() {
        super(...arguments);
        this.variables = new Map();
        this.forEachVariables = [];
    }
}
exports.CommandWithVariables = CommandWithVariables;
//# sourceMappingURL=CommandWithVariables.js.map