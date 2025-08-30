import { ConfigValue } from './ConfigValue';
export declare class ConfigStore {
    private static _instance;
    private _configFiles;
    private _commandLineSettings;
    private _loadedKnownConfigFiles;
    static get instance(): ConfigStore;
    protected constructor();
    loadConfigFile(fileName: string): void;
    loadConfigFiles(fileNames: string[]): void;
    setFromCommandLine(key: string, value: any): void;
    getFromAnyScope(key: string): ConfigValue;
    private getFromConfig;
    private getNestedValue;
    private configSourceFromScope;
    private ensureLoaded;
    private toDotNotation;
    private toEnvironmentVariable;
    private isSecret;
}
//# sourceMappingURL=ConfigStore.d.ts.map