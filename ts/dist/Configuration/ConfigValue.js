"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.ConfigValueHelpers = void 0;
const ConfigFileScope_1 = require("./ConfigFileScope");
/**
 * Helper functions for working with ConfigValue objects.
 */
class ConfigValueHelpers {
    /**
     * Creates a new ConfigValue instance.
     */
    static create(value, scope, source, isSecret = false) {
        return {
            value,
            scope,
            source,
            isSecret,
        };
    }
    /**
     * Checks if a ConfigValue has a meaningful value.
     */
    static hasValue(configValue) {
        return configValue !== undefined && configValue.value !== undefined && configValue.value !== null;
    }
    /**
     * Gets the display value for a ConfigValue (masking secrets).
     */
    static getDisplayValue(configValue) {
        if (!this.hasValue(configValue)) {
            return '';
        }
        if (configValue.isSecret) {
            return '***';
        }
        if (Array.isArray(configValue.value)) {
            return `[${configValue.value.join(', ')}]`;
        }
        if (typeof configValue.value === 'object') {
            return JSON.stringify(configValue.value);
        }
        return String(configValue.value);
    }
    /**
     * Gets the location display name for a ConfigValue.
     */
    static getLocationDisplayName(configValue) {
        switch (configValue.scope) {
            case ConfigFileScope_1.ConfigFileScope.Global:
                return 'Global';
            case ConfigFileScope_1.ConfigFileScope.User:
                return 'User';
            case ConfigFileScope_1.ConfigFileScope.Local:
                return 'Local';
            case ConfigFileScope_1.ConfigFileScope.FileName:
                return configValue.source;
            default:
                return configValue.source;
        }
    }
}
exports.ConfigValueHelpers = ConfigValueHelpers;
//# sourceMappingURL=ConfigValue.js.map