import * as path from 'path';
import * as os from 'os';

export class PathHelpers {
    static combine(basePath: string, ...paths: string[]): string | null {
        try {
            return path.join(basePath, ...paths);
        } catch {
            return null;
        }
    }

    static getAbsolutePath(relativePath: string, basePath?: string): string {
        if (path.isAbsolute(relativePath)) {
            return relativePath;
        }
        
        const base = basePath || process.cwd();
        return path.resolve(base, relativePath);
    }

    static getRelativePath(from: string, to: string): string {
        return path.relative(from, to);
    }

    static getFileName(filePath: string): string {
        return path.basename(filePath);
    }

    static getFileNameWithoutExtension(filePath: string): string {
        const fileName = path.basename(filePath);
        const ext = path.extname(fileName);
        return fileName.slice(0, -ext.length);
    }

    static getDirectoryName(filePath: string): string {
        return path.dirname(filePath);
    }

    static getExtension(filePath: string): string {
        return path.extname(filePath);
    }

    static changeExtension(filePath: string, newExtension: string): string {
        const dir = path.dirname(filePath);
        const name = path.basename(filePath, path.extname(filePath));
        const ext = newExtension.startsWith('.') ? newExtension : `.${newExtension}`;
        return path.join(dir, name + ext);
    }

    static getTempPath(): string {
        return os.tmpdir();
    }

    static getHomePath(): string {
        return os.homedir();
    }

    static normalizePath(filePath: string): string {
        return path.normalize(filePath);
    }

    static isPathRooted(filePath: string): boolean {
        return path.isAbsolute(filePath);
    }
}