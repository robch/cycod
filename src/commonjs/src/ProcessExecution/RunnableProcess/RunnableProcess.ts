import { RunnableProcessBuilder } from './RunnableProcessBuilder';
import { RunnableProcessResult } from './RunnableProcessResult';

export class RunnableProcess {
    private _hasStarted: boolean = false;
    private _hasExited: boolean = false;
    private _fileName?: string;
    private _arguments?: string;

    constructor(fileName?: string, args?: string) {
        this._fileName = fileName;
        this._arguments = args;
    }

    get hasStarted(): boolean {
        return this._hasStarted;
    }

    get hasExited(): boolean {
        return this._hasExited;
    }

    async runAsync(): Promise<RunnableProcessResult> {
        if (!this._fileName) {
            throw new Error('FileName must be specified');
        }

        this._hasStarted = true;
        
        try {
            const builder = new RunnableProcessBuilder()
                .withFileName(this._fileName)
                .withArguments(this._arguments || '');

            const result = await builder.runAsync();
            this._hasExited = true;
            return result;
        } catch (error) {
            this._hasExited = true;
            throw error;
        }
    }

    static async runAsync(fileName: string, args?: string): Promise<RunnableProcessResult> {
        const process = new RunnableProcess(fileName, args);
        return await process.runAsync();
    }
}