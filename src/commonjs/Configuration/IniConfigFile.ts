import * as fs from 'fs';
import { ConfigFile } from './ConfigFile';
import { ConfigFileScope } from './ConfigFileScope';
import { KnownSettings } from './KnownSettings';

/**
 * Handles reading and writing INI-style configuration files.
 */
export class IniConfigFile extends ConfigFile {
    public constructor(filePath: string, scope: ConfigFileScope) {
        super(filePath, scope);
    }

    protected ReadSettings(fileName: string): Map<string, any> {
        const result = new Map<string, any>();

        if (!fs.existsSync(fileName)) return result;

        try {
            const content = fs.readFileSync(fileName, 'utf-8');
            const lines = content.split('\n');
            
            for (const line of lines) {
                const trimmedLine = line.trim();
                
                // Skip empty lines and comments
                if (!trimmedLine || trimmedLine.startsWith('#') || trimmedLine.startsWith(';')) {
                    continue;
                }

                // Parse KEY=VALUE format
                const equalsPos = trimmedLine.indexOf('=');
                if (equalsPos > 0) {
                    const key = trimmedLine.substring(0, equalsPos).trim();
                    const value = trimmedLine.substring(equalsPos + 1).trim();

                    // Check if this is a list represented as comma-separated values
                    if (value.includes(',')) {
                        // Convert comma-separated string to string array
                        const listValue = value.split(',')
                                            .map(v => v.trim())
                                            .filter(v => v.length > 0);
                        
                        // Convert from flat format to hierarchical format
                        const dotNotationKey = KnownSettings.ToDotNotation(key);
                        this.SetNestedValue(result, dotNotationKey.split('.'), listValue);
                    } else {
                        // Convert from flat format to hierarchical format
                        const dotNotationKey = KnownSettings.ToDotNotation(key);
                        this.SetNestedValue(result, dotNotationKey.split('.'), value);
                    }
                }
            }
        } catch (ex: any) {
            console.error(`Error reading INI configuration file: ${ex.message}`);
        }

        return result;
    }

    protected WriteSettings(fileName: string, settings?: Map<string, any> | null): void {
        try {
            const flatData = this.FlattenDictionary(this.Settings);
            let content = '';

            for (const [key, value] of flatData) {
                const envKey = KnownSettings.ToEnvironmentVariable(key);
                const valueStr = value?.toString() ?? '';
                content += `${envKey}=${valueStr}\n`;
            }

            fs.writeFileSync(fileName, content);
        } catch (ex: any) {
            console.error(`Error writing INI configuration file: ${ex.message}`);
        }
    }

    private SetNestedValue(data: Map<string, any>, keyParts: string[], value: any): void {
        if (keyParts.length === 1) {
            data.set(keyParts[0], value);
            return;
        }

        if (!data.has(keyParts[0])) {
            data.set(keyParts[0], new Map<string, any>());
        }

        const nested = data.get(keyParts[0]);
        if (nested instanceof Map) {
            this.SetNestedValue(nested, keyParts.slice(1), value);
        } else {
            // Replace non-Map value with a Map
            const newMap = new Map<string, any>();
            data.set(keyParts[0], newMap);
            this.SetNestedValue(newMap, keyParts.slice(1), value);
        }
    }

    private FlattenDictionary(data: Map<string, any>, prefix: string = ''): Map<string, any> {
        const result = new Map<string, any>();

        for (const [key, value] of data) {
            const fullKey = prefix ? `${prefix}.${key}` : key;

            if (value instanceof Map) {
                // Recursively flatten nested Maps
                for (const [nestedKey, nestedValue] of this.FlattenDictionary(value, fullKey)) {
                    result.set(nestedKey, nestedValue);
                }
            } else if (Array.isArray(value)) {
                // Handle array values
                // In INI format, we'll just concatenate array items with a separator
                result.set(fullKey, value.join(','));
            } else {
                result.set(fullKey, value);
            }
        }

        return result;
    }
}