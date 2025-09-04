import { ConsoleHelpers } from './ConsoleHelpers';
import { MarkdownHelpers } from './MarkdownHelpers';

export interface ExpectResult {
    success: boolean;
    failedReason?: string;
}

export class ExpectHelper {
    public static checkOutput(output: string, expected?: string, unexpected?: string): ExpectResult {
        const outputLines = output
            .split('\n')
            .map(line => line.replace(/\r$/, ''));
        
        const expectedLines = expected
            ? expected.split('\n')
                .filter(line => line.length > 0)
                .map(line => line.replace(/\r$/, ''))
            : [];
            
        const unexpectedLines = unexpected
            ? unexpected.split('\n')
                .filter(line => line.length > 0)
                .map(line => line.replace(/\r$/, ''))
            : [];

        return ExpectHelper.checkLines(outputLines, expectedLines, unexpectedLines);
    }

    public static checkLines(lines: string[], expected: string[], unexpected: string[]): ExpectResult {
        const helper = new ExpectHelper(lines, expected, unexpected);
        const result = helper.expect();

        if (!result) {
            return {
                success: false,
                failedReason: helper._details.trim()
            };
        }

        return { success: true };
    }

    private _allLines: string[];
    private _expected: string[] | null;
    private _unexpected: string[] | null;
    private _details: string = '';
    private _unmatchedInput: string = '';
    private _foundUnexpected: boolean = false;
    private _expectedIndex: number = 0;

    private constructor(lines: string[], expected: string[], unexpected: string[]) {
        this._allLines = lines;
        this._expected = expected.length > 0 ? [...expected] : null;
        this._unexpected = unexpected.length > 0 ? [...unexpected] : null;
    }

    private expect(): boolean {
        for (const line of this._allLines) {
            if (this._expected !== null) this.checkExpected(line);
            if (this._unexpected !== null) this.checkUnexpected(line);
        }

        const allExpectedFound = this._expected === null || this._expectedIndex >= this._expected.length;
        if (!allExpectedFound && this._expected !== null) {
            const codeBlock = MarkdownHelpers.getCodeBlock(this._unmatchedInput);
            const message = `UNEXPECTED: Couldn't find '${this._expected[this._expectedIndex]}' in:\n${codeBlock}`;
            this._details += message + '\n';
        }

        return !this._foundUnexpected && allExpectedFound;
    }

    private checkExpected(line: string): void {
        ConsoleHelpers.writeDebugHexDump(line, `CheckExpected: Adding '${line}'`);
        this._unmatchedInput += line + '\n';
        ConsoleHelpers.writeDebugHexDump(this._unmatchedInput, 'CheckExpected: Unmatched is now:');
        
        while (this._expected !== null && this._expectedIndex < this._expected.length) {
            const pattern = this._expected[this._expectedIndex];
            const check = this._unmatchedInput;

            try {
                const regex = new RegExp(pattern);
                const match = regex.exec(check);
                if (match === null) {
                    ConsoleHelpers.writeDebugLine(`CheckExpected: No match for '${pattern}' in unmatched!\nCheckExpected: ---`);
                    break; // continue reading input...
                }

                ConsoleHelpers.writeDebugHexDump(check, `CheckExpected: Matched '${pattern}' at ${match.index.toString(16)} (${match[0].length.toString(16)} char(s)) in:`);
                this._unmatchedInput = this._unmatchedInput.substring(match.index + match[0].length);
                ConsoleHelpers.writeDebugHexDump(this._unmatchedInput, 'CheckExpected: After removing, unmatched is now:');

                this._expectedIndex++;
            } catch (error) {
                // If regex is invalid, treat as literal string match
                const index = check.indexOf(pattern);
                if (index === -1) {
                    ConsoleHelpers.writeDebugLine(`CheckExpected: No match for '${pattern}' in unmatched!\nCheckExpected: ---`);
                    break;
                }

                ConsoleHelpers.writeDebugHexDump(check, `CheckExpected: Matched '${pattern}' at ${index.toString(16)} (${pattern.length.toString(16)} char(s)) in:`);
                this._unmatchedInput = this._unmatchedInput.substring(index + pattern.length);
                ConsoleHelpers.writeDebugHexDump(this._unmatchedInput, 'CheckExpected: After removing, unmatched is now:');

                this._expectedIndex++;
            }
        }
    }

    private checkUnexpected(line: string): void {
        if (this._unexpected === null) return;

        for (const pattern of this._unexpected) {
            try {
                const regex = new RegExp(pattern);
                const match = regex.exec(line);
                if (match === null) continue; // check more patterns

                this._foundUnexpected = true;
                const message = `UNEXPECTED: Found '${pattern}' in '${line}'`;
                this._details += message + '\n';
            } catch (error) {
                // If regex is invalid, treat as literal string match
                if (line.includes(pattern)) {
                    this._foundUnexpected = true;
                    const message = `UNEXPECTED: Found '${pattern}' in '${line}'`;
                    this._details += message + '\n';
                }
            }
        }
    }
}