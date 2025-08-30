import { ConfigFile } from './ConfigFile';
import { ConfigFileScope } from './ConfigFileScope';
export declare class YamlConfigFile extends ConfigFile {
    constructor(filePath: string, scope: ConfigFileScope);
    protected readSettings(fileName: string): Record<string, any>;
    protected writeSettings(fileName: string, settings?: Record<string, any>): void;
    private normalizeNestedDictionaries;
}
//# sourceMappingURL=YamlConfigFile.d.ts.map