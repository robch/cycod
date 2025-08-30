"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.RunnableProcess = void 0;
const RunnableProcessBuilder_1 = require("./RunnableProcessBuilder");
class RunnableProcess {
    constructor(fileName, args) {
        this._hasStarted = false;
        this._hasExited = false;
        this._fileName = fileName;
        this._arguments = args;
    }
    get hasStarted() {
        return this._hasStarted;
    }
    get hasExited() {
        return this._hasExited;
    }
    async runAsync() {
        if (!this._fileName) {
            throw new Error('FileName must be specified');
        }
        this._hasStarted = true;
        try {
            const builder = new RunnableProcessBuilder_1.RunnableProcessBuilder()
                .withFileName(this._fileName)
                .withArguments(this._arguments || '');
            const result = await builder.runAsync();
            this._hasExited = true;
            return result;
        }
        catch (error) {
            this._hasExited = true;
            throw error;
        }
    }
    static async runAsync(fileName, args) {
        const process = new RunnableProcess(fileName, args);
        return await process.runAsync();
    }
}
exports.RunnableProcess = RunnableProcess;
//# sourceMappingURL=RunnableProcess.js.map