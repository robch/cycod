"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.CycoDevProgramInfo = void 0;
class CycoDevProgramInfo {
    constructor() {
        this.name = "cycodjs";
        this.version = "1.0.0";
        this.description = "AI-powered CLI application for chat-based interaction with AI assistants";
        this.author = "Rob Chambers";
    }
    getDisplayName() {
        return `${this.name} v${this.version}`;
    }
    getFullDescription() {
        return `${this.name} - ${this.description}`;
    }
}
exports.CycoDevProgramInfo = CycoDevProgramInfo;
//# sourceMappingURL=CycoDevProgramInfo.js.map