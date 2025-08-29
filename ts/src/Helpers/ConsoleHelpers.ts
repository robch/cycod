/**
 * Helper functions for console output.
 */
export class ConsoleHelpers {
  private static _quiet = false;
  private static _debug = false;

  /**
   * Sets the quiet mode.
   */
  static setQuiet(quiet: boolean): void {
    this._quiet = quiet;
  }

  /**
   * Sets the debug mode.
   */
  static setDebug(debug: boolean): void {
    this._debug = debug;
  }

  /**
   * Writes a line to console (respects quiet mode unless overridden).
   */
  static writeLine(message = '', overrideQuiet = false): void {
    if (!this._quiet || overrideQuiet) {
      console.log(message);
    }
  }

  /**
   * Writes an error message to console.
   */
  static writeErrorLine(message: string): void {
    console.error(message);
  }

  /**
   * Writes a debug message to console (only if debug mode is enabled).
   */
  static writeDebugLine(message: string): void {
    if (this._debug) {
      console.log(`[DEBUG] ${message}`);
    }
  }

  /**
   * Writes a warning message to console.
   */
  static writeWarningLine(message: string): void {
    console.warn(message);
  }

  /**
   * Formats a configuration value for display.
   */
  static formatConfigValue(key: string, value: any, location?: string): string {
    let displayValue: string;

    if (value === undefined || value === null) {
      displayValue = '';
    } else if (Array.isArray(value)) {
      displayValue = `[${value.join(', ')}]`;
    } else if (typeof value === 'object') {
      displayValue = JSON.stringify(value);
    } else {
      displayValue = String(value);
    }

    let result = `${key}=${displayValue}`;
    
    if (location) {
      result += ` (${location})`;
    }

    return result;
  }

  /**
   * Displays a configuration setting with its location.
   */
  static displayConfigSetting(key: string, value: any, location?: string): void {
    const formatted = this.formatConfigValue(key, value, location);
    this.writeLine(formatted);
  }

  /**
   * Displays a location header in the original cycod format.
   */
  static displayLocationHeader(locationPath: string): void {
    this.writeLine(`LOCATION: ${locationPath}`, true);
    this.writeLine('', true);
  }

  /**
   * Displays a list of configuration settings.
   */
  static displayConfigSettings(locationPath: string, settings: Record<string, any>): void {
    this.displayLocationHeader(locationPath);

    if (Object.keys(settings).length === 0) {
      this.writeLine('  No configuration settings found.', true);
      return;
    }

    // Sort keys to match original: non-dotted first, then dotted
    const sortedKeys = this.sortKeysWithNonDottedFirst(Object.keys(settings));
    
    for (const key of sortedKeys) {
      const value = settings[key];
      let displayValue: string;
      
      if (typeof value === 'object' && 'value' in value) {
        // This is a ConfigValue object
        displayValue = value.isSecret ? this.maskSecret(value.value) : this.formatValue(value.value);
      } else {
        displayValue = this.formatValue(value);
      }
      
      this.writeLine(`  ${key}: ${displayValue}`, true);
    }
  }

  /**
   * Sorts keys with non-dotted keys first, then dotted keys.
   */
  private static sortKeysWithNonDottedFirst(keys: string[]): string[] {
    const nonDotted = keys.filter(k => !k.includes('.')).sort();
    const dotted = keys.filter(k => k.includes('.')).sort();
    return [...nonDotted, ...dotted];
  }

  /**
   * Masks secret values like tokens to match original format.
   */
  private static maskSecret(value: any): string {
    const str = String(value);
    if (str.length <= 4) {
      return '***';
    }
    // Original shows sk************************************************************0A pattern
    // Show first 2 characters + fixed number of asterisks + last 2 characters  
    const stars = '*'.repeat(60); // Match the original asterisk count
    return str.slice(0, 2) + stars + str.slice(-2);
  }

  /**
   * Formats a value for display.
   */
  private static formatValue(value: any): string {
    if (Array.isArray(value)) {
      return `[${value.join(', ')}]`;
    }
    if (typeof value === 'object' && value !== null) {
      return JSON.stringify(value);
    }
    return String(value);
  }
}