import { ConfigSource } from './ConfigSource';
import { ConfigFile } from './ConfigFile';
export declare class ConfigValue {
    private _value;
    readonly source: ConfigSource;
    readonly isSecret: boolean;
    file?: ConfigFile;
    constructor(value?: any, source?: ConfigSource, isSecret?: boolean);
    get value(): any;
    asString(): string | undefined;
    asObfuscated(): string | undefined;
    asInt(defaultValue?: number): number;
    asBool(defaultValue?: boolean): boolean;
    asList(): string[];
    isNotFound(): boolean;
    isNotFoundNullOrEmpty(): boolean;
    addToList(value: string): boolean;
    removeFromList(value: string): boolean;
    clear(): void;
    set(value: any): void;
    toString(): string;
}
//# sourceMappingURL=ConfigValue.d.ts.map