export declare class FileHelpers {
    static findFiles(pathPattern: string, pattern?: string): string[];
    static fileExists(filePath: string): boolean;
    static directoryExists(dirPath: string): boolean;
    static readAllText(filePath: string): string;
    static writeAllText(filePath: string, content: string): void;
    static writeTextToTempFile(content: string, extension?: string): string;
    static deleteFile(filePath: string): boolean;
    static copyFile(source: string, destination: string): void;
}
//# sourceMappingURL=FileHelpers.d.ts.map