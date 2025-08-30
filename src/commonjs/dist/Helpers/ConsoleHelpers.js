"use strict";
var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    var desc = Object.getOwnPropertyDescriptor(m, k);
    if (!desc || ("get" in desc ? !m.__esModule : desc.writable || desc.configurable)) {
      desc = { enumerable: true, get: function() { return m[k]; } };
    }
    Object.defineProperty(o, k2, desc);
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || (function () {
    var ownKeys = function(o) {
        ownKeys = Object.getOwnPropertyNames || function (o) {
            var ar = [];
            for (var k in o) if (Object.prototype.hasOwnProperty.call(o, k)) ar[ar.length] = k;
            return ar;
        };
        return ownKeys(o);
    };
    return function (mod) {
        if (mod && mod.__esModule) return mod;
        var result = {};
        if (mod != null) for (var k = ownKeys(mod), i = 0; i < k.length; i++) if (k[i] !== "default") __createBinding(result, mod, k[i]);
        __setModuleDefault(result, mod);
        return result;
    };
})();
Object.defineProperty(exports, "__esModule", { value: true });
exports.ConsoleHelpers = void 0;
const readline = __importStar(require("readline"));
const fs = __importStar(require("fs"));
class ConsoleHelpers {
    static configureDebug(debug) {
        this._debug = this._debug || debug;
        this.writeDebugLine(`Debug: ${this._debug}`);
    }
    static configure(debug, verbose, quiet) {
        process.stdout.setEncoding('utf8');
        this._debug = debug;
        this._verbose = verbose;
        this._quiet = quiet;
        this.writeDebugLine(`Debug: ${this._debug}`);
        this.writeDebugLine(`Verbose: ${this._verbose}`);
        this.writeDebugLine(`Quiet: ${this._quiet}`);
    }
    static isQuiet() {
        return this._quiet;
    }
    static isVerbose() {
        return this._verbose;
    }
    static isDebug() {
        return this._debug;
    }
    static displayStatus(status) {
        if (!this._debug && !this._verbose)
            return;
        if (!process.stdout.isTTY)
            return;
        // Simple status display - in C# this was more complex with cursor manipulation
        process.stdout.write(`\r${status}`);
    }
    static writeDebugLine(message) {
        if (this._debug) {
            console.error(`DEBUG: ${message}`);
        }
    }
    static writeVerboseLine(message) {
        if (this._verbose) {
            console.log(message);
        }
    }
    static writeLine(message, overrideQuiet = false) {
        if (!this._quiet || overrideQuiet) {
            console.log(message);
        }
    }
    static writeError(message) {
        console.error(message);
    }
    static getAllLinesFromStdin() {
        // This is a synchronous version - in real implementation you'd want async
        try {
            const input = fs.readFileSync(process.stdin.fd, 'utf-8');
            return input.split(/\r?\n/);
        }
        catch {
            return [];
        }
    }
    static isStandardInputReference(fileName) {
        return fileName === '-' || fileName === '/dev/stdin';
    }
    static async readLineAsync(prompt) {
        const rl = readline.createInterface({
            input: process.stdin,
            output: process.stdout,
        });
        return new Promise((resolve) => {
            rl.question(prompt || '', (answer) => {
                rl.close();
                resolve(answer);
            });
        });
    }
}
exports.ConsoleHelpers = ConsoleHelpers;
ConsoleHelpers._debug = false;
ConsoleHelpers._verbose = false;
ConsoleHelpers._quiet = false;
ConsoleHelpers._printLock = {};
//# sourceMappingURL=ConsoleHelpers.js.map