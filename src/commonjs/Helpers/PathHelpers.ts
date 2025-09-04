import * as path from 'path';
import * as fs from 'fs';

/**
 * Helper class for working with file system paths in Node.js.
 */
export class PathHelpers {
    /**
     * Combines two path components into a single path.
     * @param path1 The first path component
     * @param path2 The second path component
     * @returns The combined path, or null if the operation fails
     */
    public static Combine(path1: string, path2: string): string | null {
        try {
            return path.join(path1, path2);
        } catch {
            return null;
        }
    }

    /**
     * Combines three path components into a single path.
     * @param path1 The first path component
     * @param path2 The second path component
     * @param path3 The third path component
     * @returns The combined path, or null if the operation fails
     */
    public static CombineThree(path1: string, path2: string, path3: string): string | null {
        try {
            return path.join(path1, path2, path3);
        } catch {
            return null;
        }
    }

    /**
     * Combines a base path with multiple path components.
     * @param path1 The base path
     * @param path2s Array of path components to combine with the base path
     * @returns An array of combined paths
     */
    public static CombineMultiple(path1: string, path2s: string[]): string[] {
        const list: string[] = [];
        
        for (const path2 of path2s) {
            const combined = PathHelpers.Combine(path1, path2);
            if (combined === null) continue;

            list.push(path2 && path2.trim() !== ''
                ? combined
                : path1);
        }
        
        return list;
    }

    /**
     * Normalizes a path by resolving it to an absolute path and optionally making it relative to the current working directory.
     * @param outputDirectory The directory path to normalize
     * @returns The normalized path
     */
    public static NormalizePath(outputDirectory: string): string {
        try {
            const normalized = path.resolve(outputDirectory);
            const cwd = process.cwd();
            
            return normalized.startsWith(cwd) && normalized.length > cwd.length + 1
                ? normalized.substring(cwd.length + 1)
                : normalized;
        } catch {
            return outputDirectory;
        }
    }
}
