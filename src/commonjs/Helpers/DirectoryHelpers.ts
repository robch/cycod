import * as fs from 'fs';
import * as path from 'path';
import { ConsoleHelpers } from './ConsoleHelpers';

/**
 * Directory operations helper class
 */
export class DirectoryHelpers {
    public static findOrCreateDirectorySearchParents(...paths: string[]): string
    {
        return DirectoryHelpers.findDirectorySearchParentsWithCreate(paths, true)!;
    }

    public static findDirectorySearchParents(...paths: string[]): string | null
    {
        return DirectoryHelpers.findDirectorySearchParentsWithCreate(paths, false);
    }

    public static findDirectorySearchParentsWithCreate(paths: string[], createIfNotFound: boolean): string | null
    {
        let current = process.cwd();
        
        while (current != null) {
            const combined = path.join(current, ...paths);
            if (fs.existsSync(combined) && fs.statSync(combined).isDirectory()) {
                return combined;
            }

            const parent = path.dirname(current);
            current = parent !== current ? parent : null;
        }

        if (createIfNotFound) {
            current = process.cwd();
            const combined = path.join(current, ...paths);
            return DirectoryHelpers.ensureDirectoryExists(combined);
        }

        return null;
    }
    
    public static ensureDirectoryExists(folder: string): string
    {
        try {
            const validFolderName = folder != null && folder !== '';
            if (validFolderName && !fs.existsSync(folder)) {
                fs.mkdirSync(folder, { recursive: true });
            }
        } catch (ex) {
            if (!fs.existsSync(folder)) {
                ConsoleHelpers.writeErrorLine(`Error creating directory: ${(ex as Error).message}`);
                throw ex;
            }
        }
        return folder;
    }

    public static ensureDirectoryForFileExists(fileName: string): void
    {
        DirectoryHelpers.ensureDirectoryExists(path.dirname(fileName) || '.');
    }
}
