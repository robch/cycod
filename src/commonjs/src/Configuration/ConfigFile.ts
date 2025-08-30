import * as path from 'path';
import { ConfigFileScope } from './ConfigFileScope';

export abstract class ConfigFile {
    private _fileName: string;
    private _scope: ConfigFileScope;
    protected _settings?: Record<string, any>;

    static fromFile(filePath: string, scope: ConfigFileScope): ConfigFile {
        const ext = path.extname(filePath).toLowerCase();
        
        switch (ext) {
            case '.yaml':
            case '.yml':
                // Dynamically import to avoid circular dependency
                const { YamlConfigFile } = require('./YamlConfigFile');
                return new YamlConfigFile(filePath, scope);
            default:
                // Dynamically import to avoid circular dependency
                const { IniConfigFile } = require('./IniConfigFile');
                return new IniConfigFile(filePath, scope);
        }
    }

    get fileName(): string {
        return this._fileName;
    }

    get scope(): ConfigFileScope {
        return this._scope;
    }

    get settings(): Record<string, any> {
        this.ensureLoaded();
        return this._settings!;
    }

    protected set settings(value: Record<string, any>) {
        this._settings = value;
    }

    constructor(fileName: string, scope: ConfigFileScope = ConfigFileScope.FileName) {
        this._fileName = fileName;
        this._scope = scope;
    }

    save(): void {
        this.ensureLoaded();
        this.writeSettings(this._fileName);
    }

    protected abstract readSettings(fileName: string): Record<string, any>;
    protected abstract writeSettings(fileName: string, settings?: Record<string, any>): void;

    protected ensureLoaded(): void {
        if (this._settings === undefined) {
            this._settings = this.readSettings(this._fileName);
        }
    }
}