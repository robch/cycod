import { ConfigFileScope } from './ConfigFileScope';
export declare abstract class ConfigFile {
    private _fileName;
    private _scope;
    protected _settings?: Record<string, any>;
    static fromFile(filePath: string, scope: ConfigFileScope): ConfigFile;
    get fileName(): string;
    get scope(): ConfigFileScope;
    get settings(): Record<string, any>;
    protected set settings(value: Record<string, any>);
    constructor(fileName: string, scope?: ConfigFileScope);
    save(): void;
    protected abstract readSettings(fileName: string): Record<string, any>;
    protected abstract writeSettings(fileName: string, settings?: Record<string, any>): void;
    protected ensureLoaded(): void;
}
//# sourceMappingURL=ConfigFile.d.ts.map