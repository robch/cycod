/**
 * Known configuration settings and their properties.
 */
export declare class KnownSettings {
    private static readonly _knownSettings;
    private static readonly _aliases;
    /**
     * Checks if a setting is a known configuration setting.
     */
    static isKnown(key: string): boolean;
    /**
     * Gets the canonical form of a setting key.
     */
    static getCanonicalForm(key: string): string;
    /**
     * Checks if a setting is a secret (should be masked in display).
     */
    static isSecret(key: string): boolean;
    /**
     * Gets the description for a setting.
     */
    static getDescription(key: string): string | undefined;
    /**
     * Gets all known setting keys.
     */
    static getAllKnownSettings(): string[];
    /**
     * Converts a key to dot notation format.
     */
    static toDotNotation(key: string): string;
    /**
     * Converts a key from dot notation to underscore format.
     */
    static toUnderscoreNotation(key: string): string;
}
//# sourceMappingURL=KnownSettings.d.ts.map