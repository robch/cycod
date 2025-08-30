import * as fs from 'fs';
import * as path from 'path';
import { sync as globSync } from 'glob';
import * as os from 'os';

export class FileHelpers {
    static findFiles(pathPattern: string, pattern?: string): string[] {
        if (pattern) {
            const combined = path.join(pathPattern, pattern);
            return this.findFiles(combined);
        }

        try {
            // Handle glob patterns
            return globSync(pathPattern, { cwd: process.cwd() });
        } catch (error) {
            console.error(`Error finding files: ${(error as Error).message}`);
            return [];
        }
    }

    static fileExists(filePath: string): boolean {
        try {
            return fs.existsSync(filePath) && fs.statSync(filePath).isFile();
        } catch {
            return false;
        }
    }

    static directoryExists(dirPath: string): boolean {
        try {
            return fs.existsSync(dirPath) && fs.statSync(dirPath).isDirectory();
        } catch {
            return false;
        }
    }

    static readAllText(filePath: string): string {
        return fs.readFileSync(filePath, 'utf-8');
    }

    static writeAllText(filePath: string, content: string): void {
        const dir = path.dirname(filePath);
        if (!fs.existsSync(dir)) {
            fs.mkdirSync(dir, { recursive: true });
        }
        fs.writeFileSync(filePath, content, 'utf-8');
    }

    static writeTextToTempFile(content: string, extension?: string): string {
        const tempDir = os.tmpdir();
        const fileName = `temp_${Date.now()}_${Math.random().toString(36).substring(2, 15)}${extension || '.tmp'}`;
        const tempFilePath = path.join(tempDir, fileName);
        
        this.writeAllText(tempFilePath, content);
        return tempFilePath;
    }

    static deleteFile(filePath: string): boolean {
        try {
            fs.unlinkSync(filePath);
            return true;
        } catch {
            return false;
        }
    }

    static copyFile(source: string, destination: string): void {
        const destDir = path.dirname(destination);
        if (!fs.existsSync(destDir)) {
            fs.mkdirSync(destDir, { recursive: true });
        }
        fs.copyFileSync(source, destination);
    }
}