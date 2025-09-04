import { ColorHelpers } from './ColorHelpers';
import { ConsoleColor } from './Colors';

/**
 * Console I/O operations helper class
 */
export class ConsoleHelpers {
    public static configureDebug(debug: boolean): void
    {
        ConsoleHelpers._debug = ConsoleHelpers._debug || debug;
        ConsoleHelpers.writeDebugLine(`Debug: ${ConsoleHelpers._debug}`);
    }

    public static configure(debug: boolean, verbose: boolean, quiet: boolean): void
    {
        // Node.js uses UTF-8 by default
        process.stdout.setEncoding('utf8');

        ConsoleHelpers._debug = debug;
        ConsoleHelpers._verbose = verbose;
        ConsoleHelpers._quiet = quiet;

        ConsoleHelpers.writeDebugLine(`Debug: ${ConsoleHelpers._debug}`);
        ConsoleHelpers.writeDebugLine(`Verbose: ${ConsoleHelpers._verbose}`);
        ConsoleHelpers.writeDebugLine(`Quiet: ${ConsoleHelpers._quiet}`);
    }

    public static isQuiet(): boolean
    {
        return ConsoleHelpers._quiet;
    }

    public static isVerbose(): boolean
    {
        return ConsoleHelpers._verbose;
    }

    public static isDebug(): boolean
    {
        return ConsoleHelpers._debug;
    }

    public static displayStatus(status: string): void
    {
        if (!ConsoleHelpers._debug && !ConsoleHelpers._verbose) return;
        if (process.stdout.isTTY === false) return;

        ConsoleHelpers.displayStatusErase();
        process.stdout.write('\r' + status);
        ConsoleHelpers._cchLastStatus = status.length;
        if (ConsoleHelpers._debug) {
            // Small delay for debug mode
            setTimeout(() => {}, 1);
        }
    }

    public static displayStatusErase(): void
    {
        if (!ConsoleHelpers._debug && !ConsoleHelpers._verbose) return;
        if (ConsoleHelpers._cchLastStatus <= 0) return;

        const eraseLastStatus = '\r' + ' '.repeat(ConsoleHelpers._cchLastStatus) + '\r';
        process.stdout.write(eraseLastStatus);
        ConsoleHelpers._cchLastStatus = 0;
    }

    public static write(message: string, foregroundColor?: ConsoleColor | null, overrideQuiet: boolean = false): void
    {
        ConsoleHelpers.write(message, foregroundColor, null, overrideQuiet);
    }

    public static write(message: string, foregroundColor?: ConsoleColor | null, backgroundColor?: ConsoleColor | null, overrideQuiet: boolean = false): void
    {
        if (ConsoleHelpers._quiet && !overrideQuiet) return;

        ConsoleHelpers.writeWithColorWithoutScrollSmear(message, foregroundColor, backgroundColor);
    }

    public static writeLine(message: string = '', color?: ConsoleColor | null, overrideQuiet: boolean = false): void
    {
        ConsoleHelpers.writeLine(message, color, null, overrideQuiet);
    }

    public static writeLine(message: string, foregroundColor?: ConsoleColor | null, backgroundColor?: ConsoleColor | null, overrideQuiet: boolean = false): void
    {
        if (ConsoleHelpers._quiet && !overrideQuiet) return;
        ConsoleHelpers.write(message + '\n', foregroundColor, backgroundColor, overrideQuiet);
    }

    public static writeLineIfNotEmpty(message: string): void
    {
        if (!message) return;
        ConsoleHelpers.writeLine(message);
    }

    public static writeWarning(message: string): void
    {
        ConsoleHelpers.write(message, ConsoleColor.Black, ConsoleColor.Yellow, true);
    }

    public static writeWarningLine(message: string): void
    {
        ConsoleHelpers.writeLine(message, ConsoleColor.Black, ConsoleColor.Yellow, true);
    }
    
    public static writeError(message: string): void
    {
        ConsoleHelpers.write(message, ConsoleColor.White, ConsoleColor.Red, true);
    }

    public static writeErrorLine(message: string): void
    {
        ConsoleHelpers.writeLine(message, ConsoleColor.White, ConsoleColor.Red, true);
    }

    public static writeDebug(message: string): void
    {
        if (!ConsoleHelpers._debug) return;
        ConsoleHelpers.write(message, ConsoleColor.Cyan);
    }

    public static writeDebugLine(message: string = ''): void
    {
        if (!ConsoleHelpers._debug) return;
        ConsoleHelpers.writeLine(message, ConsoleColor.Cyan);
    }

    public static writeDebugHexDump(message: string, title?: string | null): void
    {
        const noMessage = !message;
        if (noMessage) {
            ConsoleHelpers.writeDebugLine(`${title}\n  0000: (empty)`);
            return;
        }

        let i = 0;
        for (const ch of message) {
            if (i % 16 === 0) {
                const safeTitle = title?.replace(/\r/g, '\\r').replace(/\n/g, '\\n');
                ConsoleHelpers.writeDebug(i === 0 ? `${safeTitle}\n` : '\n');
                ConsoleHelpers.writeDebug(`  ${i.toString(16).padStart(4, '0')}: `);
            }

            ConsoleHelpers.writeDebug(`${ch.charCodeAt(0).toString(16).padStart(2, '0').toUpperCase()} `);
            i++;
        }
        ConsoleHelpers.writeDebugLine();
    }


    public static isStandardInputReference(fileName: string): boolean
    {
        return fileName === '-' || fileName === 'stdin' || fileName === 'STDIN' || fileName === 'STDIN:';
    }

    public static getAllLinesFromStdin(): string[]
    {
        return ConsoleHelpers._allLinesFromStdin == null
            ? ConsoleHelpers.readAllLinesFromStdin()
            : ConsoleHelpers._allLinesFromStdin;
    }

    public static readKey(intercept: boolean = false): { key: string, keyCode: number } | null
    {
        if (!process.stdin.isTTY) {
            // Input is redirected, read line
            const readline = require('readline');
            const rl = readline.createInterface({ input: process.stdin, output: process.stdout });
            
            return new Promise((resolve) => {
                rl.question('', (line: string) => {
                    rl.close();
                    
                    if (line == null) {
                        resolve(null);
                        return;
                    }
                    
                    const trimmedLine = line.trimEnd();
                    const treatAsEnter = trimmedLine.length === 0 || trimmedLine === '\n' || trimmedLine === '\r' || trimmedLine === '\r\n';
                    if (treatAsEnter) {
                        resolve({ key: '\n', keyCode: 13 });
                        return;
                    }
                    
                    const firstChar = trimmedLine[0];
                    const isAlphaNumeric = /[a-zA-Z0-9\p{P}\p{S}]/u.test(firstChar);
                    if (isAlphaNumeric) {
                        resolve({ key: firstChar, keyCode: firstChar.charCodeAt(0) });
                        return;
                    }
                    
                    switch (firstChar) {
                        case '\t':
                            resolve({ key: '\t', keyCode: 9 });
                            break;
                        case ' ':
                            resolve({ key: ' ', keyCode: 32 });
                            break;
                        default:
                            resolve(null);
                    }
                });
            }) as any;
        }
        
        // For TTY, we'd need additional setup for raw key reading
        // This is a simplified version - in practice you might want to use a library like 'keypress'
        return null;
    }

    private static readAllLinesFromStdin(): string[]
    {
        ConsoleHelpers._allLinesFromStdin = [];
        
        // In Node.js, we need to read from stdin synchronously
        const fs = require('fs');
        try {
            const input = fs.readFileSync(process.stdin.fd, 'utf8');
            ConsoleHelpers._allLinesFromStdin = input.split(/\r?\n/);
            // Remove last empty line if present
            if (ConsoleHelpers._allLinesFromStdin[ConsoleHelpers._allLinesFromStdin.length - 1] === '') {
                ConsoleHelpers._allLinesFromStdin.pop();
            }
        } catch (error) {
            // If stdin is not available or empty, return empty array
            ConsoleHelpers._allLinesFromStdin = [];
        }

        return ConsoleHelpers._allLinesFromStdin;
    }

    private static writeWithColorWithoutScrollSmear(message: string, foregroundColor?: ConsoleColor | null, backgroundColor?: ConsoleColor | null): void
    {
        const lines = message
            .split('\n')
            .map(line => line.replace(/\r$/, ''));
        
        for (let i = 0; i < lines.length; i++) {
            if (i > 0) process.stdout.write('\n');
            ConsoleHelpers.writeWithColor(lines[i], foregroundColor, backgroundColor);
        }
    }

    private static writeWithColor(message: string, foregroundColor?: ConsoleColor | null, backgroundColor?: ConsoleColor | null): void
    {
        // In Node.js, we use ANSI escape codes for colors
        let output = '';
        
        if (foregroundColor != null) {
            const mappedColor = ColorHelpers.mapColor(foregroundColor);
            output += ConsoleHelpers.getAnsiColorCode(mappedColor, false);
        }
        
        if (backgroundColor != null) {
            const mappedColor = ColorHelpers.mapColor(backgroundColor);
            output += ConsoleHelpers.getAnsiColorCode(mappedColor, true);
        }
        
        output += message;
        
        // Reset colors
        if (foregroundColor != null || backgroundColor != null) {
            output += '\x1b[0m';
        }
        
        process.stdout.write(output);
    }
   
    private static _debug: boolean = false;
    private static _verbose: boolean = false;
    private static _quiet: boolean = false;

    private static _cchLastStatus: number = 0;

    private static _allLinesFromStdin: string[] | null = null;
    
    private static getAnsiColorCode(color: ConsoleColor, isBackground: boolean): string {
        const colorMap: { [key in ConsoleColor]: number } = {
            [ConsoleColor.Black]: 0,
            [ConsoleColor.DarkBlue]: 4,
            [ConsoleColor.DarkGreen]: 2,
            [ConsoleColor.DarkCyan]: 6,
            [ConsoleColor.DarkRed]: 1,
            [ConsoleColor.DarkMagenta]: 5,
            [ConsoleColor.DarkYellow]: 3,
            [ConsoleColor.Gray]: 7,
            [ConsoleColor.DarkGray]: 8,
            [ConsoleColor.Blue]: 12,
            [ConsoleColor.Green]: 10,
            [ConsoleColor.Cyan]: 14,
            [ConsoleColor.Red]: 9,
            [ConsoleColor.Magenta]: 13,
            [ConsoleColor.Yellow]: 11,
            [ConsoleColor.White]: 15
        };
        
        const colorCode = colorMap[color] || 7; // Default to gray
        const baseCode = isBackground ? 40 : 30;
        
        if (colorCode < 8) {
            return `\x1b[${baseCode + colorCode}m`;
        } else {
            return `\x1b[${baseCode + 60 + colorCode - 8}m`;
        }
    }
}