import { ConfigValue } from './ConfigValue';
import { ConfigSource } from './ConfigSource';

export class ConfigDisplayHelpers {
    public static DisplayConfigSettings(location: string | null, config: Map<string, ConfigValue>, indentLevel: number = 2): void {
        if (location != null) {
            // CommonDisplayHelpers.WriteLocationHeader(location);
            console.log(`=== ${location} ===`);
        }

        if (config.size === 0) {
            console.log(`${' '.repeat(indentLevel)}No configuration settings found.`);
            return;
        }

        // Sort the keys with non-dotted keys first, then dotted keys
        const sortedKeys = ConfigDisplayHelpers.SortKeysWithNonDottedFirst(Array.from(config.keys()));
        
        let hasDisplayedNonDotted = false;
        let hasDisplayedDotted = false;

        for (const key of sortedKeys) {
            // If we're switching from non-dotted to dotted keys, add a line break
            if (!hasDisplayedDotted && key.includes('.')) {
                if (hasDisplayedNonDotted) {
                    console.log("");
                }
                hasDisplayedDotted = true;
            }
            
            if (!key.includes('.')) {
                hasDisplayedNonDotted = true;
            }
            
            const value = config.get(key);
            if (value) {
                ConfigDisplayHelpers.DisplayConfigValue(key, value, indentLevel);
            }
        }
    }
    
    public static DisplayConfigValue(key: string, value: ConfigValue, indentLevel: number = 2, showLocation: boolean = false): void {
        if (showLocation) {
            let location: string | null;
            switch (value.Source) {
                case ConfigSource.CommandLine:
                    location = "Command line (specified)";
                    break;
                case ConfigSource.EnvironmentVariable:
                    location = "Environment variable (specified)";
                    break;
                case ConfigSource.ConfigFileName:
                    location = `${value.File?.FileName} (specified)`;
                    break;
                case ConfigSource.LocalConfig:
                    location = `${value.File?.FileName} (local)`;
                    break;
                case ConfigSource.UserConfig:
                    location = `${value.File?.FileName} (user)`;
                    break;
                case ConfigSource.GlobalConfig:
                    location = `${value.File?.FileName} (global)`;
                    break;
                default:
                    location = null;
                    break;
            }

            showLocation = !!location;
            if (showLocation) {
                console.log(`=== ${location} ===`);
            }
        }

        const indent = ' '.repeat(indentLevel);
        
        // If it's a list type in memory (actual List objects)
        if (Array.isArray(value.Value)) {
            const list = value.AsList();
            ConfigDisplayHelpers.DisplayList(key, list, indentLevel);
            return;
        }
        
        // Get value to display, obfuscating if it's a secret
        const displayValue = value.IsSecret
            ? value.AsObfuscated() ?? "(empty)"
            : !value.IsNotFoundNullOrEmpty()
                ? value.Value?.toString() ?? "(null)"
                : "(not found or empty)";
                            
        console.log(`${indent}${key}: ${displayValue}`);
    }
    
    public static DisplayList(key: string, list: string[], indentLevel: number = 2): void {
        const keyIndent = ' '.repeat(indentLevel);
        const valueIndent = ' '.repeat(indentLevel + 2);
        
        if (list.length > 0) {
            console.log(`${keyIndent}${key}:`);
            for (const item of list) {
                console.log(`${valueIndent}- ${item}`);
            }
        } else {
            console.log(`${keyIndent}${key}: (empty list)`);
        }
    }
    
    private static SortKeysWithNonDottedFirst(keys: string[]): string[] {
        // Split keys into two groups: non-dotted and dotted
        const nonDottedKeys: string[] = [];
        const dottedKeys: string[] = [];
        
        for (const key of keys) {
            if (key.includes('.')) {
                dottedKeys.push(key);
            } else {
                nonDottedKeys.push(key);
            }
        }
        
        // Sort each group alphabetically (case-insensitive)
        nonDottedKeys.sort((a, b) => a.toLowerCase().localeCompare(b.toLowerCase()));
        dottedKeys.sort((a, b) => a.toLowerCase().localeCompare(b.toLowerCase()));
        
        // Combine the groups with non-dotted keys first
        return [...nonDottedKeys, ...dottedKeys];
    }
}