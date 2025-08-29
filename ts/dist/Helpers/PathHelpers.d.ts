/**
 * Helper functions for working with file paths.
 */
export declare class PathHelpers {
    /**
     * Gets the user's home directory.
     */
    static getHomeDirectory(): string;
    /**
     * Gets the global configuration directory.
     */
    static getGlobalConfigDirectory(): string;
    /**
     * Gets the user configuration directory.
     */
    static getUserConfigDirectory(): string;
    /**
     * Gets the local configuration directory (current working directory).
     */
    static getLocalConfigDirectory(): string;
    /**
     * Gets the configuration file path for a given scope.
     */
    static getConfigFilePath(scope: 'global' | 'user' | 'local'): string;
    /**
     * Ensures that a directory exists.
     */
    static ensureDirectory(dirPath: string): Promise<void>;
    /**
     * Expands a path with tilde (~) to the full home directory path.
     */
    static expandPath(filePath: string): string;
    /**
     * Normalizes a file path.
     */
    static normalizePath(filePath: string): string;
    /**
     * Checks if a path is absolute.
     */
    static isAbsolute(filePath: string): boolean;
    /**
     * Resolves a path relative to the current working directory.
     */
    static resolve(filePath: string): string;
}
//# sourceMappingURL=PathHelpers.d.ts.map