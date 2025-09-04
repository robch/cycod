import * as fs from 'fs';
import * as path from 'path';
import { ProgramInfo } from './ProgramInfo';
import { ConsoleHelpers } from './ConsoleHelpers';

/**
 * Helper class for working with embedded files/resources in Node.js.
 * In Node.js, we simulate embedded resources using files in a resources directory.
 */
export class EmbeddedFileHelpers {
    private static readonly ResourcesPath = path.join(__dirname, '../../../resources');

    /**
     * Gets all available embedded resource file names.
     * @returns Array of resource file names
     */
    public static GetEmbeddedStreamFileNames(): string[] {
        try {
            // In Node.js, we'll look for files in a resources directory
            if (!fs.existsSync(EmbeddedFileHelpers.ResourcesPath)) {
                return [];
            }

            const files: string[] = [];
            EmbeddedFileHelpers.collectFiles(EmbeddedFileHelpers.ResourcesPath, files);
            
            // Convert absolute paths to resource names similar to .NET manifest resource names
            return files.map(filePath => {
                const relativePath = path.relative(EmbeddedFileHelpers.ResourcesPath, filePath);
                return `${ProgramInfo.Name}.${relativePath.replace(/[/\\]/g, '.')}`;
            });
        } catch (error) {
            ConsoleHelpers.WriteDebugLine(`Error reading embedded resources: ${error}`);
            return [];
        }
    }

    /**
     * Checks if an embedded resource exists.
     * @param fileName The name of the resource file to check
     * @returns True if the resource exists
     */
    public static EmbeddedStreamExists(fileName: string): boolean {
        const allResourceNames = EmbeddedFileHelpers.GetEmbeddedStreamFileNames();
        const resourceName = allResourceNames
            .filter(name => name.toLowerCase().endsWith(fileName.toLowerCase()))
            .sort((a, b) => a.length - b.length)[0];

        const found = resourceName !== undefined;
        if (found) return true;

        const allResourceNamesStr = allResourceNames.join('\n  ');
        ConsoleHelpers.WriteDebugLine(`DEBUG: Embedded resources (${allResourceNames.length}):\n\n  ${allResourceNamesStr}\n`);

        return false;
    }

    /**
     * Reads the content of an embedded resource file.
     * @param fileName The name of the resource file to read
     * @returns The file content as a string, or null if not found
     */
    public static ReadEmbeddedStream(fileName: string): string | null {
        const allResourceNames = EmbeddedFileHelpers.GetEmbeddedStreamFileNames();
        const resourceName = allResourceNames
            .filter(name => name.toLowerCase().endsWith(fileName.toLowerCase()))
            .sort((a, b) => a.length - b.length)[0];

        if (!resourceName) return null;

        try {
            // Convert resource name back to file path
            const resourcePath = resourceName.substring(`${ProgramInfo.Name}.`.length);
            const filePath = path.join(EmbeddedFileHelpers.ResourcesPath, resourcePath.replace(/\./g, path.sep));

            if (!fs.existsSync(filePath)) {
                return null;
            }

            return fs.readFileSync(filePath, 'utf8');
        } catch (error) {
            ConsoleHelpers.WriteDebugLine(`Error reading embedded resource '${fileName}': ${error}`);
            return null;
        }
    }

    /**
     * Recursively collects all files from a directory.
     * @param dir The directory to scan
     * @param files Array to collect file paths into
     */
    private static collectFiles(dir: string, files: string[]): void {
        try {
            const entries = fs.readdirSync(dir, { withFileTypes: true });
            
            for (const entry of entries) {
                const fullPath = path.join(dir, entry.name);
                
                if (entry.isDirectory()) {
                    EmbeddedFileHelpers.collectFiles(fullPath, files);
                } else if (entry.isFile()) {
                    files.push(fullPath);
                }
            }
        } catch (error) {
            // Silently ignore directory read errors
        }
    }
}
