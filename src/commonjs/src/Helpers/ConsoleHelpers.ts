import * as readline from 'readline';
import * as fs from 'fs';

export class ConsoleHelpers {
    private static _debug: boolean = false;
    private static _verbose: boolean = false;
    private static _quiet: boolean = false;
    private static _printLock = {};

    static configureDebug(debug: boolean): void {
        this._debug = this._debug || debug;
        this.writeDebugLine(`Debug: ${this._debug}`);
    }

    static configure(debug: boolean, verbose: boolean, quiet: boolean): void {
        process.stdout.setEncoding('utf8');
        
        this._debug = debug;
        this._verbose = verbose;
        this._quiet = quiet;

        this.writeDebugLine(`Debug: ${this._debug}`);
        this.writeDebugLine(`Verbose: ${this._verbose}`);
        this.writeDebugLine(`Quiet: ${this._quiet}`);
    }

    static isQuiet(): boolean {
        return this._quiet;
    }

    static isVerbose(): boolean {
        return this._verbose;
    }

    static isDebug(): boolean {
        return this._debug;
    }

    static displayStatus(status: string): void {
        if (!this._debug && !this._verbose) return;
        if (!process.stdout.isTTY) return;

        // Simple status display - in C# this was more complex with cursor manipulation
        process.stdout.write(`\r${status}`);
    }

    static writeDebugLine(message: string): void {
        if (this._debug) {
            console.error(`DEBUG: ${message}`);
        }
    }

    static writeVerboseLine(message: string): void {
        if (this._verbose) {
            console.log(message);
        }
    }

    static writeLine(message: string, overrideQuiet: boolean = false): void {
        if (!this._quiet || overrideQuiet) {
            console.log(message);
        }
    }

    static writeError(message: string): void {
        console.error(message);
    }

    static getAllLinesFromStdin(): string[] {
        // This is a synchronous version - in real implementation you'd want async
        try {
            const input = fs.readFileSync(process.stdin.fd, 'utf-8');
            return input.split(/\r?\n/);
        } catch {
            return [];
        }
    }

    static isStandardInputReference(fileName: string): boolean {
        return fileName === '-' || fileName === '/dev/stdin';
    }

    static async readLineAsync(prompt?: string): Promise<string> {
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