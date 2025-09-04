import { ConfigSource } from './ConfigSource';
import { ConfigFile } from './ConfigFile';

export class ConfigValue {
    public constructor();
    public constructor(value: any);
    public constructor(value: any, source: ConfigSource);
    public constructor(value: any, source: ConfigSource, isSecret: boolean);
    public constructor(value?: any, source?: ConfigSource, isSecret?: boolean) {
        this._value = value ?? null;
        this.Source = source ?? ConfigSource.NotFound;
        this.IsSecret = isSecret ?? false;
    }

    public get Value(): any { 
        return this._value; 
    }
    
    /**
     * Gets the source of this configuration value.
     */
    public readonly Source: ConfigSource;

    /**
     * Gets the configuration file associated with this value, if any.
     */
    public File: ConfigFile | null = null;

    /**
     * Gets whether this value should be treated as a secret.
     */
    public readonly IsSecret: boolean;

    public AsString(): string | null {
        return this._value?.toString() ?? null;
    }
    
    /**
     * Returns an obfuscated version of the value if it's a secret.
     * @returns The obfuscated value if it's a secret, otherwise the normal string representation.
     */
    public AsObfuscated(): string | null {
        if (!this.IsSecret || this._value == null) {
            return this.AsString();
        }
        
        const valueStr = this._value.toString() || '';
        if (valueStr.length <= 4) {
            return '****';
        }
        
        // Show first 2 and last 2 characters, obfuscate the rest
        return valueStr.substring(0, 2) + '*'.repeat(Math.min(60, valueStr.length - 4)) + valueStr.substring(valueStr.length - 2);
    }

    public AsInt(defaultValue: number = 0): number {
        if (typeof this._value === 'number') {
            return this._value;
        }

        if (typeof this._value === 'string') {
            const parsed = parseInt(this._value, 10);
            if (!isNaN(parsed)) {
                return parsed;
            }
        }

        return defaultValue;
    }

    public AsBool(defaultValue: boolean = false): boolean {
        if (typeof this._value === 'boolean') {
            return this._value;
        }

        if (typeof this._value === 'string') {
            const lower = this._value.toLowerCase();
            if (lower === 'true' || lower === 'false') {
                return lower === 'true';
            }
            return lower === 'true' || this._value === '1';
        }

        if (typeof this._value === 'number') {
            return this._value !== 0;
        }

        return defaultValue;
    }

    public AsList(): string[] {
        // Handle null value
        if (this._value == null) {
            console.debug('AsList: null value');
            return [];
        }

        // Handle arrays
        if (Array.isArray(this._value)) {
            const list = this._value.map(item => item?.toString() ?? '');
            console.debug(`AsList: Array [${list.join(', ')}]`);
            return list;
        }

        // Handle single string
        if (typeof this._value === 'string') {
            // Check if it's an empty array representation from YAML
            if (this._value === '[]') {
                console.debug('AsList: empty array string representation');
                return [];
            }
            
            console.debug(`AsList: single string ${this._value}`);
            return [this._value];
        }

        // Handle other iterable types (but not strings)
        if (this._value && typeof this._value[Symbol.iterator] === 'function' && typeof this._value !== 'string') {
            const list: string[] = [];
            for (const item of this._value) {
                list.push(item?.toString() ?? '');
            }
            console.debug(`AsList: Iterable [${list.join(', ')}]`);
            return list;
        }

        // Default - empty list
        console.debug(`AsList: unknown type (${typeof this._value}) - returning empty list`);
        return [];
    }

    public IsNotFound(): boolean {
        return this.Source === ConfigSource.NotFound;
    }

    public IsNotFoundNullOrEmpty(): boolean {
        if (this.Source === ConfigSource.NotFound) {
            return true;
        }

        if (this._value == null) {
            return true;
        }

        if (typeof this._value === 'string') {
            return this._value.length === 0;
        }

        if (Array.isArray(this._value)) {
            return this._value.length === 0;
        }

        return false;
    }

    public AddToList(value: string): boolean {
        const list = this.AsList();
        if (!list.includes(value)) {
            list.push(value);
            this._value = list;
            return true;
        }

        return false;
    }

    public RemoveFromList(value: string): boolean {
        const list = this.AsList();
        const index = list.indexOf(value);
        if (index > -1) {
            list.splice(index, 1);
            this._value = list;
            return true;
        }

        return false;
    }

    public Clear(): void {
        this._value = null;
    }

    public Set(value: any): void {
        this._value = value;
    }

    public toString(): string {
        if (this._value == null) {
            return '';
        }

        if (Array.isArray(this._value)) {
            return this._value.map(item => item?.toString() ?? '').join(', ');
        }

        return this._value.toString() ?? '';
    }

    private _value: any;
}