import * as path from 'path';
import { ConfigFileScope } from './ConfigFileScope';
import { YamlConfigFile } from './YamlConfigFile';
import { IniConfigFile } from './IniConfigFile';

export abstract class ConfigFile {
    public static FromFile(filePath: string, scope: ConfigFileScope): ConfigFile {
        const extension = path.extname(filePath).toLowerCase();
        switch (extension) {
            case '.yaml':
            case '.yml':
                return new YamlConfigFile(filePath, scope);
            default:
                return new IniConfigFile(filePath, scope);
        }
    }

    public get FileName(): string { 
        return this._fileName; 
    }
    
    public get Scope(): ConfigFileScope { 
        return this._scope; 
    }

    public get Settings(): Map<string, any> {
        this.EnsureLoaded();
        return this._settings!;
    }

    protected set Settings(value: Map<string, any>) {
        this._settings = value;
    }

    public Save(): void {
        this.EnsureLoaded();
        this.WriteSettings(this._fileName);
    }

    protected constructor(fileName: string, scope: ConfigFileScope = ConfigFileScope.FileName) {
        this._fileName = fileName;
        this._scope = scope;
    }

    protected abstract ReadSettings(fileName: string): Map<string, any>;

    protected abstract WriteSettings(fileName: string, settings?: Map<string, any> | null): void;

    protected EnsureLoaded(): void {
        if (this._settings == null) {
            this._settings = this.ReadSettings(this._fileName);
        }
    }

    private readonly _fileName: string;
    private readonly _scope: ConfigFileScope;
    private _settings: Map<string, any> | null = null;
}