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
export class ConfigValueHelpers {
  /**
   * Creates a new ConfigValue instance.
   */
  static create(
    value: any,
    scope: ConfigFileScope,
    source: string,
    isSecret = false
  ): ConfigValue {
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
  static hasValue(configValue?: ConfigValue): boolean {
    return configValue !== undefined && configValue.value !== undefined && configValue.value !== null;
  }

  /**
   * Gets the display value for a ConfigValue (masking secrets).
   */
  static getDisplayValue(configValue: ConfigValue): string {
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
  static getLocationDisplayName(configValue: ConfigValue): string {
    switch (configValue.scope) {
      case ConfigFileScope.Global:
        return 'Global';
      case ConfigFileScope.User:
        return 'User';
      case ConfigFileScope.Local:
        return 'Local';
      case ConfigFileScope.FileName:
        return configValue.source;
      default:
        return configValue.source;
    }
  }
}