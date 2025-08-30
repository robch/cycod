"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.CommandLineException = void 0;
class CommandLineException extends Error {
    constructor(message, helpTopic) {
        super(message);
        this.name = 'CommandLineException';
        this._helpTopic = helpTopic;
    }
    getHelpTopic() {
        return this._helpTopic;
    }
}
exports.CommandLineException = CommandLineException;
//# sourceMappingURL=CommandLineException.js.map