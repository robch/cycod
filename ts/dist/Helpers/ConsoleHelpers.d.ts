/**
 * Helper functions for console output.
 */
export declare class ConsoleHelpers {
    private static _quiet;
    private static _debug;
    /**
     * Sets the quiet mode.
     */
    static setQuiet(quiet: boolean): void;
    /**
     * Sets the debug mode.
     */
    static setDebug(debug: boolean): void;
    /**
     * Writes a line to console (respects quiet mode unless overridden).
     */
    static writeLine(message?: string, overrideQuiet?: boolean): void;
    /**
     * Writes an error message to console.
     */
    static writeErrorLine(message: string): void;
    /**
     * Writes a debug message to console (only if debug mode is enabled).
     */
    static writeDebugLine(message: string): void;
    /**
     * Writes a warning message to console.
     */
    static writeWarningLine(message: string): void;
    /**
     * Formats a configuration value for display.
     */
    static formatConfigValue(key: string, value: any, location?: string): string;
    /**
     * Displays a configuration setting with its location.
     */
    static displayConfigSetting(key: string, value: any, location?: string): void;
    /**
     * Displays a location header in the original cycod format.
     */
    static displayLocationHeader(locationPath: string): void;
    /**
     * Displays a list of configuration settings.
     */
    static displayConfigSettings(locationPath: string, settings: Record<string, any>): void;
    /**
     * Sorts keys with non-dotted keys first, then dotted keys.
     */
    private static sortKeysWithNonDottedFirst;
    /**
     * Masks secret values like tokens to match original format.
     */
    private static maskSecret;
    /**
     * Formats a value for display.
     */
    private static formatValue;
}
//# sourceMappingURL=ConsoleHelpers.d.ts.map