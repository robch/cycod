export declare class PathHelpers {
    static combine(basePath: string, ...paths: string[]): string | null;
    static getAbsolutePath(relativePath: string, basePath?: string): string;
    static getRelativePath(from: string, to: string): string;
    static getFileName(filePath: string): string;
    static getFileNameWithoutExtension(filePath: string): string;
    static getDirectoryName(filePath: string): string;
    static getExtension(filePath: string): string;
    static changeExtension(filePath: string, newExtension: string): string;
    static getTempPath(): string;
    static getHomePath(): string;
    static normalizePath(filePath: string): string;
    static isPathRooted(filePath: string): boolean;
}
//# sourceMappingURL=PathHelpers.d.ts.map