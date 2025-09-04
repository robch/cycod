import * as fs from 'fs';
import * as path from 'path';
import { ConfigFileScope } from './ConfigFileScope';
import { ConfigFileHelpers } from './ConfigFileHelpers';

/**
 * Provides methods for working with files across different scope directories.
 */
export class ScopeFileHelpers {
    public static GetScopeFromPath(filePath: string): ConfigFileScope {
        if (filePath.includes(ConfigFileHelpers.GetGlobalScopeDirectory())) return ConfigFileScope.Global;
        if (filePath.includes(ConfigFileHelpers.GetUserScopeDirectory())) return ConfigFileScope.User;
        if (filePath.includes(ConfigFileHelpers.GetLocalScopeDirectory())) return ConfigFileScope.Local;
        return ConfigFileScope.FileName;
    }

    /**
     * Finds a file in a specific scope's subdirectory.
     * @param fileName The name of the file to find
     * @param subDir The subdirectory name within the scope directory
     * @param scope The scope to search in
     * @returns The full path to the file if found, null otherwise
     */
    public static FindFileInScope(fileName: string, subDir: string, scope: ConfigFileScope): string | null {
        const scopeDir = ConfigFileHelpers.GetScopeDirectoryPath(scope);
        if (scopeDir == null) {
            console.debug(`FindFileInScope: Scope directory for ${scope} not found.`);
            return null;
        }

        const subDirPath = path.join(scopeDir, subDir);
        if (!fs.existsSync(subDirPath)) {
            console.debug(`FindFileInScope: Subdirectory ${subDir} does not exist in scope ${scope}.`);
            return null;
        }

        const combined = path.join(subDirPath, fileName);
        const existingFile = fs.existsSync(combined) ? combined : null;

        console.debug(existingFile != null
            ? `FindFileInScope: Found file ${fileName} in scope ${scope} at ${existingFile}`
            : `FindFileInScope: File ${fileName} not found in scope ${scope}.`);

        return existingFile;
    }

    /**
     * Finds a file in any scope's subdirectory, with optional parent directory search.
     * @param fileName The name of the file to find
     * @param subDir The subdirectory name within the scope directory  
     * @param searchParents Whether to search parent directories if the file is not found in any scope
     * @returns The full path to the file if found, null otherwise
     */
    public static FindFileInAnyScope(fileName: string, subDir: string, searchParents: boolean = false): string | null {
        // First, check the current directory for the bare file
        const bareFilePath = path.join(process.cwd(), fileName);
        if (fs.existsSync(bareFilePath)) {
            console.debug(`FindFileInAnyScope: Found bare file in current directory: ${bareFilePath}`);
            return bareFilePath;
        }

        // Then check each scope in order: Local, User, Global
        for (const scope of [ConfigFileScope.Local, ConfigFileScope.User, ConfigFileScope.Global]) {
            const fileInScope = ScopeFileHelpers.FindFileInScope(fileName, subDir, scope);
            if (fileInScope != null) {
                console.debug(`FindFileInAnyScope: Found file in scope ${scope}: ${fileInScope}`);
                return fileInScope;
            }
        }

        // If not found in any scope and searchParents is true, search parent directories
        if (searchParents) {
            const existing = ScopeFileHelpers.FindFileSearchParents('.cycod', subDir, fileName);
            console.debug(existing != null
                ? `FindFileInAnyScope: Found file in parent directory: ${existing}`
                : `FindFileInAnyScope: File ${fileName} not found in any scope or parent directories.`);
            return existing;
        }

        return null;
    }

    /**
     * Finds a directory in a specific scope.
     * @param subDir The subdirectory name within the scope directory
     * @param scope The scope to search in
     * @returns The full path to the directory if found, null otherwise
     */
    public static FindDirectoryInScope(subDir: string, scope: ConfigFileScope): string | null {
        const scopeDir = ConfigFileHelpers.GetScopeDirectoryPath(scope);
        if (scopeDir == null) {
            console.debug(`FindDirectoryInScope: Scope directory for ${scope} not found.`);
            return null;
        }

        const dirPath = path.join(scopeDir, subDir);
        const existing = fs.existsSync(dirPath) ? dirPath : null;

        console.debug(existing != null
            ? `FindDirectoryInScope: Found directory ${subDir} in scope ${scope} at ${existing}`
            : `FindDirectoryInScope: Directory ${subDir} not found in scope ${scope}.`);
        return existing;
    }

    /**
     * Finds a directory in any scope, with optional parent directory search.
     * @param subDir The subdirectory name within the scope directory
     * @param searchParents Whether to search parent directories if the directory is not found in any scope
     * @returns The full path to the directory if found, null otherwise
     */
    public static FindDirectoryInAnyScope(subDir: string, searchParents: boolean = false): string | null {
        // Check each scope in order: Local, User, Global
        for (const scope of [ConfigFileScope.Local, ConfigFileScope.User, ConfigFileScope.Global]) {
            const dirInScope = ScopeFileHelpers.FindDirectoryInScope(subDir, scope);
            if (dirInScope != null) {
                console.debug(`FindDirectoryInAnyScope: Found directory in scope ${scope}: ${dirInScope}`);
                return dirInScope;
            }
        }

        // If not found in any scope and searchParents is true, search parent directories
        if (searchParents) {
            const existing = ScopeFileHelpers.FindDirectorySearchParents('.cycod', subDir);
            console.debug(existing != null
                ? `FindDirectoryInAnyScope: Found directory in parent directory: ${existing}`
                : `FindDirectoryInAnyScope: Directory ${subDir} not found in any scope or parent directories.`);
            return existing;
        }

        console.debug(`FindDirectoryInAnyScope: Directory ${subDir} not found in any scope.`);
        return null;
    }

    /**
     * Ensures a directory exists in the specified scope.
     * @param subDir The subdirectory name within the scope directory
     * @param scope The scope to create the directory in
     * @returns The full path to the directory
     */
    public static EnsureDirectoryInScope(subDir: string, scope: ConfigFileScope): string {
        const scopeDir = ConfigFileHelpers.GetScopeDirectoryPath(scope);
        if (scopeDir == null) {
            throw new Error(`Could not get scope directory path for ${scope}`);
        }

        const dirPath = path.join(scopeDir, subDir);
        fs.mkdirSync(dirPath, { recursive: true });

        console.debug(`EnsureDirectoryInScope: Ensured directory ${subDir} in scope ${scope} at ${dirPath}`);
        return dirPath;
    }

    /**
     * Finds all files matching a pattern in a specific scope.
     * @param pattern The search pattern
     * @param subDir The subdirectory name within the scope directory
     * @param scope The scope to search in
     * @returns An array of file paths matching the pattern
     */
    public static FindFilesInScope(pattern: string, subDir: string, scope: ConfigFileScope): string[] {
        const dirPath = ScopeFileHelpers.FindDirectoryInScope(subDir, scope);
        if (dirPath == null || !fs.existsSync(dirPath)) {
            return [];
        }

        return fs.readdirSync(dirPath)
            .filter(file => file.match(pattern))
            .map(file => path.join(dirPath, file));
    }

    private static FindFileSearchParents(configDir: string, subDir: string, fileName: string): string | null {
        let currentDir = process.cwd();
        
        while (currentDir !== path.dirname(currentDir)) {
            const configPath = path.join(currentDir, configDir);
            const subDirPath = path.join(configPath, subDir);
            const filePath = path.join(subDirPath, fileName);
            
            if (fs.existsSync(filePath)) {
                return filePath;
            }
            
            currentDir = path.dirname(currentDir);
        }
        
        return null;
    }

    private static FindDirectorySearchParents(configDir: string, subDir: string): string | null {
        let currentDir = process.cwd();
        
        while (currentDir !== path.dirname(currentDir)) {
            const configPath = path.join(currentDir, configDir);
            const subDirPath = path.join(configPath, subDir);
            
            if (fs.existsSync(subDirPath)) {
                return subDirPath;
            }
            
            currentDir = path.dirname(currentDir);
        }
        
        return null;
    }
}