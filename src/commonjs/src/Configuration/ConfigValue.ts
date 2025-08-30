import { ConfigSource } from './ConfigSource';
import { ConfigFile } from './ConfigFile';

export class ConfigValue {
    private _value: any;
    readonly source: ConfigSource;
    readonly isSecret: boolean;
    file?: ConfigFile;

    constructor(value?: any, source?: ConfigSource, isSecret?: boolean) {
        this._value = value ?? null;
        this.source = source ?? ConfigSource.Default;
        this.isSecret = isSecret ?? false;
    }

    get value(): any {
        return this._value;
    }

    asString(): string | undefined {
        return this._value?.toString();
    }

    asObfuscated(): string | undefined {
        if (!this.isSecret || this._value == null) {
            return this.asString();
        }

        const valueStr = this._value.toString() || '';
        if (valueStr.length <= 4) {
            return "****";
        }

        // Show first 2 and last 2 characters, obfuscate the rest
        const obfuscatedLength = Math.min(60, valueStr.length - 4);
        return valueStr.substring(0, 2) + '*'.repeat(obfuscatedLength) + valueStr.substring(valueStr.length - 2);
    }

    asInt(defaultValue: number = 0): number {
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

    asBool(defaultValue: boolean = false): boolean {
        if (typeof this._value === 'boolean') {
            return this._value;
        }

        if (typeof this._value === 'string') {
            const lower = this._value.toLowerCase();
            return lower === 'true' || this._value === '1';
        }

        if (typeof this._value === 'number') {
            return this._value !== 0;
        }

        return defaultValue;
    }

    asList(): string[] {
        if (this._value == null) {
            return [];
        }

        if (Array.isArray(this._value)) {
            return this._value.map(item => item?.toString() || '');
        }

        if (typeof this._value === 'string') {
            if (this._value === '[]') {
                return [];
            }
            return [this._value];
        }

        // Handle other iterable types
        if (typeof this._value[Symbol.iterator] === 'function' && typeof this._value !== 'string') {
            const result: string[] = [];
            for (const item of this._value) {
                result.push(item?.toString() || '');
            }
            return result;
        }

        return [];
    }

    isNotFound(): boolean {
        return this.source === ConfigSource.NotFound;
    }

    isNotFoundNullOrEmpty(): boolean {
        if (this.source === ConfigSource.NotFound) {
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

    addToList(value: string): boolean {
        const list = this.asList();
        if (!list.includes(value)) {
            list.push(value);
            this._value = list;
            return true;
        }
        return false;
    }

    removeFromList(value: string): boolean {
        const list = this.asList();
        const index = list.indexOf(value);
        if (index !== -1) {
            list.splice(index, 1);
            this._value = list;
            return true;
        }
        return false;
    }

    clear(): void {
        this._value = null;
    }

    set(value: any): void {
        this._value = value;
    }

    toString(): string {
        if (this._value == null) {
            return '';
        }

        if (Array.isArray(this._value)) {
            return this._value.map(item => item?.toString() || '').join(', ');
        }

        return this._value.toString() || '';
    }
}