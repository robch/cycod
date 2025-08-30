import { RunnableProcessResult } from './RunnableProcessResult';
export declare class RunnableProcess {
    private _hasStarted;
    private _hasExited;
    private _fileName?;
    private _arguments?;
    constructor(fileName?: string, args?: string);
    get hasStarted(): boolean;
    get hasExited(): boolean;
    runAsync(): Promise<RunnableProcessResult>;
    static runAsync(fileName: string, args?: string): Promise<RunnableProcessResult>;
}
//# sourceMappingURL=RunnableProcess.d.ts.map