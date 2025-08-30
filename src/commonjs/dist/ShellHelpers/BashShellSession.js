"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.BashShellSession = void 0;
const ShellSession_1 = require("./ShellSession");
class BashShellSession extends ShellSession_1.ShellSession {
    getShellType() {
        return ShellSession_1.ShellType.Bash;
    }
}
exports.BashShellSession = BashShellSession;
//# sourceMappingURL=BashShellSession.js.map