"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.Command = void 0;
class Command {
    constructor() { }
    getHelpTopic() {
        const topic = this.getCommandName();
        const ok = !!(topic && topic.trim());
        return ok ? topic : "usage";
    }
    validate() {
        return this;
    }
}
exports.Command = Command;
//# sourceMappingURL=Command.js.map