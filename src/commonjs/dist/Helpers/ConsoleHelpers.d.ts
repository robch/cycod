export declare class ConsoleHelpers {
    private static _debug;
    private static _verbose;
    private static _quiet;
    private static _printLock;
    static configureDebug(debug: boolean): void;
    static configure(debug: boolean, verbose: boolean, quiet: boolean): void;
    static isQuiet(): boolean;
    static isVerbose(): boolean;
    static isDebug(): boolean;
    static displayStatus(status: string): void;
    static writeDebugLine(message: string): void;
    static writeVerboseLine(message: string): void;
    static writeLine(message: string, overrideQuiet?: boolean): void;
    static writeError(message: string): void;
    static getAllLinesFromStdin(): string[];
    static isStandardInputReference(fileName: string): boolean;
    static readLineAsync(prompt?: string): Promise<string>;
}
//# sourceMappingURL=ConsoleHelpers.d.ts.map