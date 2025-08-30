import { RunnableProcessResult, TimeoutStrategy } from './RunnableProcessResult';
export declare class RunnableProcessBuilder {
    private _fileName?;
    private _arguments?;
    private _workingDirectory?;
    private _environmentVariables?;
    private _standardInput?;
    private _timeout?;
    private _timeoutStrategy;
    private _encoding;
    withFileName(fileName: string): RunnableProcessBuilder;
    withArguments(args: string | string[]): RunnableProcessBuilder;
    withWorkingDirectory(directory: string): RunnableProcessBuilder;
    withEnvironmentVariables(envVars: Record<string, string>): RunnableProcessBuilder;
    withStandardInput(input: string): RunnableProcessBuilder;
    withTimeout(timeoutMs: number): RunnableProcessBuilder;
    withTimeoutStrategy(strategy: TimeoutStrategy): RunnableProcessBuilder;
    withEncoding(encoding: BufferEncoding): RunnableProcessBuilder;
    runAsync(): Promise<RunnableProcessResult>;
    private handleTimeout;
    private resolveExecutablePath;
}
//# sourceMappingURL=RunnableProcessBuilder.d.ts.map