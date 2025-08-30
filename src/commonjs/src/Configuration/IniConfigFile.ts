import * as fs from 'fs';
// @ts-ignore
import * as ini from 'ini';
import { ConfigFile } from './ConfigFile';
import { ConfigFileScope } from './ConfigFileScope';

export class IniConfigFile extends ConfigFile {
    constructor(filePath: string, scope: ConfigFileScope) {
        super(filePath, scope);
    }

    protected readSettings(fileName: string): Record<string, any> {
        const result: Record<string, any> = {};

        if (!fs.existsSync(fileName)) {
            return result;
        }

        try {
            const content = fs.readFileSync(fileName, 'utf-8');
            const parsed = ini.parse(content);
            
            // Convert flat INI structure to nested object
            for (const [key, value] of Object.entries(parsed)) {
                this.setNestedValue(result, key.split('.'), value);
            }

            return result;
        } catch (error) {
            console.error(`Error reading INI configuration file: ${(error as Error).message}`);
            return result;
        }
    }

    protected writeSettings(fileName: string, settings?: Record<string, any>): void {
        try {
            const flatData = this.flattenDictionary(this.settings);
            const iniContent = ini.stringify(flatData);
            fs.writeFileSync(fileName, iniContent);
        } catch (error) {
            console.error(`Error writing INI configuration file: ${(error as Error).message}`);
        }
    }

    private setNestedValue(data: Record<string, any>, keyParts: string[], value: any): void {
        if (keyParts.length === 1) {
            data[keyParts[0]] = value;
            return;
        }

        if (!data[keyParts[0]]) {
            data[keyParts[0]] = {};
        }

        if (typeof data[keyParts[0]] === 'object') {
            this.setNestedValue(data[keyParts[0]], keyParts.slice(1), value);
        } else {
            // Replace non-object value with an object
            const newObj = {};
            data[keyParts[0]] = newObj;
            this.setNestedValue(newObj, keyParts.slice(1), value);
        }
    }

    private flattenDictionary(data: Record<string, any>, prefix: string = ''): Record<string, any> {
        const result: Record<string, any> = {};

        for (const [key, value] of Object.entries(data)) {
            const flatKey = prefix ? `${prefix}.${key}` : key;

            if (typeof value === 'object' && value !== null && !Array.isArray(value)) {
                // Recursively flatten nested objects
                Object.assign(result, this.flattenDictionary(value, flatKey));
            } else if (Array.isArray(value)) {
                // Handle array values by joining with comma
                result[flatKey] = value.join(',');
            } else {
                result[flatKey] = value;
            }
        }

        return result;
    }
}