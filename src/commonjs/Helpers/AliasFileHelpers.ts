import * as path from 'path';
import { ConfigFileScope } from '../Configuration/ConfigFileScope';
import { ScopeFileHelpers } from '../Configuration/ScopeFileHelpers';
import { FileHelpers } from './FileHelpers';
import { ConsoleHelpers } from './ConsoleHelpers';

export class AliasFileHelpers {
    /**
     * Finds the alias directory in the specified scope.
     * @param scope The scope to search in
     * @param create Whether to create the directory if it doesn't exist
     * @returns The path to the alias directory, or null if not found
     */
    public static findAliasDirectoryInScope(scope: ConfigFileScope, create: boolean = false): string | null {
        return create
            ? ScopeFileHelpers.ensureDirectoryInScope('aliases', scope)
            : ScopeFileHelpers.findDirectoryInScope('aliases', scope);
    }

    /**
     * Saves an alias with the provided options to a file in the specified scope.
     * @param aliasName Name of the alias
     * @param allOptions Command-line options to save in the alias
     * @param scope The scope to save the alias to
     * @returns List of saved file paths
     */
    public static saveAlias(aliasName: string, allOptions: string[], scope: ConfigFileScope = ConfigFileScope.Local): string[] {
        const filesSaved: string[] = [];

        const aliasDirectory = AliasFileHelpers.findAliasDirectoryInScope(scope, true)!;
        const fileName = path.join(aliasDirectory, aliasName + '.alias');

        const possibilities = ['--save-alias', '--save-local-alias', '--save-user-alias', '--save-global-alias'];
        const saveAliasOption = allOptions.filter(x => possibilities.includes(x)).pop();

        const optionPosition = allOptions.indexOf('--save-alias');
        const filtered = allOptions.filter((_, index) => optionPosition < 0 || index < optionPosition || index > optionPosition + 1);

        const options = filtered
            .filter(x => !possibilities.includes(x))
            .map(x => AliasFileHelpers.singleLineOrNewAtFile(x, fileName, filesSaved));

        const asMultiLineString = options.join('\n');
        FileHelpers.writeAllText(fileName, asMultiLineString);

        filesSaved.unshift(fileName);
        return filesSaved;
    }

    /**
     * Handles multiline text by saving it to a separate file if needed
     * @param text The text to process
     * @param baseFileName Base file name for additional files
     * @param additionalFiles Reference to list of additional files
     * @returns Single line text or @filename reference
     */
    private static singleLineOrNewAtFile(text: string, baseFileName: string, additionalFiles: string[]): string {
        const isMultiLine = text.includes('\n') || text.includes('\r');
        if (!isMultiLine) return text;

        const additionalFileCount = additionalFiles.length + 1;
        const additionalFileName = FileHelpers.getFileNameFromTemplate(baseFileName, '{filepath}/{filebase}-{fileext}-' + additionalFileCount + '.txt')!;
        additionalFiles.push(additionalFileName);

        FileHelpers.writeAllText(additionalFileName, text);

        return '@' + additionalFileName;
    }

    /**
     * Finds an alias file across all scopes (local, user, global) with optional parent directory search.
     * @param aliasName The name of the alias to find
     * @returns The full path to the alias file if found, null otherwise
     */
    public static findAliasFile(aliasName: string): string | null {
        const aliasFileName = `${aliasName}.alias`;
        const aliasFilePath = ScopeFileHelpers.findFileInAnyScope(aliasFileName, 'aliases', true);

        ConsoleHelpers.writeDebugLine(aliasFilePath !== null
            ? `FindAliasFile; found alias in scope: ${aliasFilePath}`
            : `FindAliasFile; alias NOT FOUND in any scope: ${aliasName}`);
        return aliasFilePath;
    }

    public static findAliasInScope(aliasName: string, scope: ConfigFileScope): string | null {
        const aliasFileName = `${aliasName}.alias`;
        const aliasFilePath = ScopeFileHelpers.findFileInScope(aliasFileName, 'aliases', scope);

        ConsoleHelpers.writeDebugLine(aliasFilePath !== null
            ? `FindAliasInScope; found alias in scope: ${aliasFilePath}`
            : `FindAliasInScope; alias NOT FOUND in scope: ${aliasName}`);
        return aliasFilePath;
    }
}