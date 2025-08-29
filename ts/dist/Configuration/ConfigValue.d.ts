import { ConfigFileScope } from './ConfigFileScope';
/**
 * Represents a configuration value with its source information.
 */
export interface ConfigValue {
    value: any;
    scope: ConfigFileScope;
    source: string;
    isSecret?: boolean;
}
/**
 * Represents a configuration setting that can be a string, list, or object.
 */
export type ConfigSetting = string | string[] | Record<string, any>;
/**
 * Helper functions for working with ConfigValue objects.
 */
export declare class ConfigValueHelpers {
    /**
     * Creates a new ConfigValue instance.
     */
    static create(value: any, scope: ConfigFileScope, source: string, isSecret?: boolean): ConfigValue;
    /**
     * Checks if a ConfigValue has a meaningful value.
     */
    static hasValue(configValue?: ConfigValue): boolean;
    /**
     * Gets the display value for a ConfigValue (masking secrets).
     */
    static getDisplayValue(configValue: ConfigValue): string;
    /**
     * Gets the location display name for a ConfigValue.
     */
    static getLocationDisplayName(configValue: ConfigValue): string;
}
//# sourceMappingURL=ConfigValue.d.ts.map