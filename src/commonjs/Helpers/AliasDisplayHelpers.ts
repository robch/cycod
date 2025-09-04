import * as fs from 'fs';
import * as path from 'path';
import { ConfigFileScope } from '../Configuration/ConfigFileScope';
import { ConfigFileHelpers } from '../Configuration/ConfigFileHelpers';
import { AliasFileHelpers } from './AliasFileHelpers';
import { CommonDisplayHelpers } from './CommonDisplayHelpers';
import { ConsoleHelpers } from './ConsoleHelpers';
import { ProgramInfo } from './ProgramInfo';

/**
 * Provides methods for displaying alias information.
 */
export class AliasDisplayHelpers {
    /**
     * Gets a display name for the alias directory location in the specified scope.
     * @param scope The scope to get the location display name for
     * @returns A string representing the location, or null if the scope is invalid
     */
    public static getLocationDisplayName(scope: ConfigFileScope): string | null {
        const aliasPath = AliasFileHelpers.findAliasDirectoryInScope(scope);
        if (aliasPath !== null) {
            return CommonDisplayHelpers.formatLocationHeader(aliasPath, scope);
        }
        
        const scopeDir = ConfigFileHelpers.getScopeDirectoryPath(scope);
        if (scopeDir === null) return null;

        return CommonDisplayHelpers.formatLocationHeader(path.join(scopeDir, 'aliases'), scope);
    }

    /**
     * Displays aliases for a specific scope.
     * @param scope The scope to display aliases for
     */
    public static displayAliases(scope: ConfigFileScope): void {
        // Get the location display name
        const locationDisplay = AliasDisplayHelpers.getLocationDisplayName(scope);
        if (locationDisplay === null) return;
        
        // Find the directory to check for aliases
        const aliasDir = AliasFileHelpers.findAliasDirectoryInScope(scope);
        
        // Check for aliases only if the directory exists
        let aliasFiles: string[] = [];
        if (aliasDir !== null && fs.existsSync(aliasDir)) {
            aliasFiles = fs.readdirSync(aliasDir)
                .filter(file => file.endsWith('.alias'))
                .map(file => path.join(aliasDir, file))
                .sort((a, b) => path.parse(a).name.localeCompare(path.parse(b).name));
        }

        const aliasNames = aliasFiles
            .map(file => path.parse(file).name);

        // Display the aliases with location header
        CommonDisplayHelpers.displayItemList(locationDisplay, aliasNames);
    }

    /**
     * Displays a single alias with content.
     * @param name Name of the alias
     * @param content Content of the alias
     * @param fileName File path of the alias
     * @param scope Scope of the alias
     */
    public static displayAlias(name: string, content: string, fileName: string, scope: ConfigFileScope): void {
        const location = CommonDisplayHelpers.formatLocationHeader(fileName, scope);
        CommonDisplayHelpers.displayItem(name, content, location);
    }

    /**
     * Displays a list of alias files.
     * @param aliasFiles The list of alias files to display
     * @param indentLevel The level of indentation for display
     */
    public static displayAliasFiles(aliasFiles: string[], indentLevel: number = CommonDisplayHelpers.DefaultIndentLevel): void {
        const indent = ' '.repeat(indentLevel);
        
        if (aliasFiles.length === 0) {
            ConsoleHelpers.writeLine(`${indent}No aliases found in this scope`, true);
            return;
        }

        for (const aliasFile of aliasFiles) {
            const aliasName = path.parse(aliasFile).name;
            ConsoleHelpers.writeLine(`${indent}${aliasName}`, true);
        }
    }

    /**
     * Displays information about saved alias files.
     * @param filesSaved List of saved files
     */
    public static displaySavedAliasFiles(filesSaved: string[]): void {
        CommonDisplayHelpers.displaySavedFiles(filesSaved, `${ProgramInfo.name} [...] --{name}`);
    }
}