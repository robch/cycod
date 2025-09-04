import * as fs from 'fs';
import * as path from 'path';
import { glob } from 'glob';
import { ConsoleHelpers } from './ConsoleHelpers';
import { DirectoryHelpers } from './DirectoryHelpers';
import { PathHelpers } from './PathHelpers';
import { OS } from './OS';
import { ProgramInfo } from './ProgramInfo';

/**
 * File system operations helper class
 */
export class FileHelpers {
    public static findFiles(path: string, pattern: string): string[]
    {
        const combined = PathHelpers.combine(path, pattern);
        return combined != null
            ? FileHelpers.findFiles(combined)
            : [];
    }

    public static findFiles(fileNames: string): string[]
    {
        const results: string[] = [];
        const currentDir = process.cwd();
        
        const items = fileNames.split(/[;\r\n]/).filter(item => item.trim() !== '');
        
        for (const item of items) {
            ConsoleHelpers.writeDebugLine(`  Searching for files '${fileNames}'`);

            const i1 = item.lastIndexOf(path.sep);
            const i2 = item.lastIndexOf('/');
            const hasPath = i1 >= 0 || i2 >= 0;

            const pathLen = Math.max(i1, i2);
            let searchPath = !hasPath ? currentDir : item.substring(0, pathLen);
            const pattern = !hasPath ? item : item.substring(pathLen + 1);

            let recursive = false;
            if (searchPath.endsWith('**')) {
                searchPath = searchPath.substring(0, searchPath.length - 2).replace(/[\/\\]+$/, '');
                if (!searchPath) searchPath = '.';
                recursive = true;
            }

            if (!fs.existsSync(searchPath)) continue;

            try {
                const globPattern = recursive 
                    ? path.join(searchPath, '**', pattern)
                    : path.join(searchPath, pattern);
                const files = glob.sync(globPattern);
                results.push(...files);
            } catch (error) {
                // Continue with next item if glob fails
            }
        }

        return results;
    }

    public static findFilesInOsPath(fileName: string): string[]
    {
        const pathEnv = process.env.PATH || '';
        const lookIn = pathEnv.split(path.delimiter);
        const found: string[] = [];
        
        for (const dir of lookIn) {
            try {
                const files = fs.readdirSync(dir)
                    .filter(file => file === fileName || file.match(fileName.replace(/\*/g, '.*')))
                    .map(file => path.join(dir, file));
                found.push(...files);
            } catch (error) {
                // Continue with next directory if access fails
            }
        }
        
        return found;
    }

    public static findFileSearchParents(...paths: string[]): string | null {
        return FileHelpers.findFileSearchParentsWithCreate(paths, false);
    }

    public static findFileSearchParentsWithCreate(paths: string[], createIfNotFound: boolean): string | null
    {
        let current = process.cwd();
        
        while (current != null) {
            const combined = path.join(current, ...paths);
            if (fs.existsSync(combined)) {
                return combined;
            }

            const parent = path.dirname(current);
            current = parent !== current ? parent : null;
        }

        if (createIfNotFound) {
            current = process.cwd();
            const combined = path.join(current, ...paths);
            DirectoryHelpers.ensureDirectoryForFileExists(combined);
            FileHelpers.writeAllText(combined, '');
            return combined;
        }

        return null;
    }

    public static fileExists(fileName: string | null | undefined): boolean
    {
        return fileName != null && fileName !== '' && (fs.existsSync(fileName) || fileName === '-');
    }

    public static isFileMatch(fileName: string, includeFileContainsPatternList: RegExp[], excludeFileContainsPatternList: RegExp[]): boolean
    {
        const checkContent = includeFileContainsPatternList.length > 0 || excludeFileContainsPatternList.length > 0;
        if (!checkContent) return true;

        try {
            ConsoleHelpers.displayStatus(`Processing: ${fileName} ...`);

            const content = FileHelpers.readAllText(fileName);
            const includeFile = includeFileContainsPatternList.every(regex => regex.test(content));
            const excludeFile = excludeFileContainsPatternList.length > 0 && excludeFileContainsPatternList.some(regex => regex.test(content));

            return includeFile && !excludeFile;
        } catch (error) {
            return false;
        } finally {
            ConsoleHelpers.displayStatusErase();
        }
    }

    public static getFileNameFromTemplate(fileName: string, template: string | null | undefined): string | null
    {
        if (!template) return template;

        const filePath = path.dirname(fileName);
        const fileBase = path.basename(fileName, path.extname(fileName));
        const fileExt = path.extname(fileName).replace(/^\./, '');
        const now = new Date();
        const timeStamp = now.toISOString().replace(/[-T:]/g, '').substring(0, 14); // yyyyMMddHHmmss
        const time = now.getTime().toString();

        ConsoleHelpers.writeDebugLine(`filePath: ${filePath}`);
        ConsoleHelpers.writeDebugLine(`fileBase: ${fileBase}`);
        ConsoleHelpers.writeDebugLine(`fileExt: ${fileExt}`);
        ConsoleHelpers.writeDebugLine(`timeStamp: ${timeStamp}`);

        return template
            .replace(/{fileName}/g, fileName)
            .replace(/{filename}/g, fileName)
            .replace(/{filePath}/g, filePath)
            .replace(/{filepath}/g, filePath)
            .replace(/{fileBase}/g, fileBase)
            .replace(/{filebase}/g, fileBase)
            .replace(/{fileExt}/g, fileExt)
            .replace(/{fileext}/g, fileExt)
            .replace(/{timeStamp}/g, timeStamp)
            .replace(/{timestamp}/g, timeStamp)
            .replace(/{time}/g, time)
            .replace(/[ \/\\]+$/, '');
    }

    public static readIgnoreFile(ignoreFile: string): { excludeGlobs: string[], excludeFileNamePatternList: RegExp[] }
    {
        ConsoleHelpers.writeDebugLine(`ReadIgnoreFile: ignoreFile: ${ignoreFile}`);

        const excludeGlobs: string[] = [];
        const excludeFileNamePatternList: RegExp[] = [];

        if (!fs.existsSync(ignoreFile)) {
            return { excludeGlobs, excludeFileNamePatternList };
        }

        const isWindows = OS.isWindows();
        const lines = FileHelpers.readAllLines(ignoreFile);
        
        for (const line of lines) {
            const assumeIsGlob = line.includes('/') || line.includes('\\');
            if (assumeIsGlob) {
                const excludeGlob = PathHelpers.combine(path.dirname(ignoreFile), line) || line;
                ConsoleHelpers.writeDebugLine(`ReadIgnoreFile; ignore glob: ${excludeGlob}`);
                excludeGlobs.push(excludeGlob);
            } else {
                ConsoleHelpers.writeDebugLine(`ReadIgnoreFile; exclude pattern: ${line}`);
                excludeFileNamePatternList.push(isWindows
                    ? new RegExp(line, 'i')
                    : new RegExp(line));
            }
        }
        
        return { excludeGlobs, excludeFileNamePatternList };
    }

    public static filesFromGlobs(globs: string[]): string[]
    {
        const results: string[] = [];
        for (const globPattern of globs) {
            results.push(...FileHelpers.filesFromGlob(globPattern));
        }
        return results;
    }

    public static filesFromGlob(globPattern: string): string[]
    {
        ConsoleHelpers.displayStatus(`Finding files: ${globPattern} ...`);
        try {
            if (globPattern === '-') return [globPattern]; // special case for stdin

            const files = glob.sync(FileHelpers.makeRelativePath(globPattern));
            return files.map(file => FileHelpers.makeRelativePath(path.resolve(file)));
        } catch (error) {
            return [];
        } finally {
            ConsoleHelpers.displayStatusErase();
        }
    }

    public static findMatchingFiles(
        globs: string[],
        excludeGlobs: string[] = [],
        excludeFileNamePatternList: RegExp[] = [],
        includeFileContainsPatternList: RegExp[] = [],
        excludeFileContainsPatternList: RegExp[] = []): string[]
    {
        const excludeFiles = new Set(FileHelpers.filesFromGlobs(excludeGlobs));
        const files = FileHelpers.filesFromGlobs(globs)
            .filter(file => !excludeFiles.has(file))
            .filter(file => !excludeFileNamePatternList.some(regex => regex.test(path.basename(file))));

        ConsoleHelpers.writeDebugLine(`DEBUG: 1: Found files (${files.length}): `);
        files.forEach(x => ConsoleHelpers.writeDebugLine(`DEBUG: 1: - ${x}`));
        ConsoleHelpers.writeDebugLine('');

        if (files.length === 0) {
            ConsoleHelpers.writeLine(`## Pattern: ${globs.join(' ')}\n\n - No files found\n`);
            return [];
        }

        const filtered = files.filter(file => FileHelpers.isFileMatch(file, includeFileContainsPatternList, excludeFileContainsPatternList));
        if (filtered.length === 0) {
            ConsoleHelpers.writeLine(`## Pattern: ${globs.join(' ')}\n\n - No files matched criteria\n`);
            return [];
        }

        const distinct = [...new Set(filtered)];
        ConsoleHelpers.writeDebugLine(`DEBUG: 2: Found files (${distinct.length} distinct/filtered): `);
        distinct.forEach(x => ConsoleHelpers.writeDebugLine(`DEBUG: 2: - ${x}`));

        return distinct;
    }

    public static makeRelativePath(fullPath: string): string
    {
        const currentDirectory = process.cwd() + path.sep;
        fullPath = path.resolve(fullPath);

        if (fullPath.toLowerCase().startsWith(currentDirectory.toLowerCase())) {
            return fullPath.substring(currentDirectory.length);
        }

        return path.relative(process.cwd(), fullPath);
    }

    public static readAllText(fileName: string): string
    {
        const content = ConsoleHelpers.isStandardInputReference(fileName)
            ? ConsoleHelpers.getAllLinesFromStdin().join('\n')
            : fs.readFileSync(fileName, 'utf8');

        return content;
    }

    public static readAllLines(fileName: string): string[]
    {
        const lines = ConsoleHelpers.isStandardInputReference(fileName)
            ? ConsoleHelpers.getAllLinesFromStdin()
            : fs.readFileSync(fileName, 'utf8').split(/\r?\n/);

        return lines;
    }

    public static writeAllText(fileName: string, content: string, saveToFolderOnAccessDenied?: string | null): string
    {
        try {
            DirectoryHelpers.ensureDirectoryForFileExists(fileName);
            fs.writeFileSync(fileName, content, 'utf8');
        } catch (error) {
            const trySavingElsewhere = saveToFolderOnAccessDenied != null && saveToFolderOnAccessDenied !== '';
            if (trySavingElsewhere) {
                const userProfileFolder = process.env.HOME || process.env.USERPROFILE || '';
                const trySavingToFolder = path.join(userProfileFolder, saveToFolderOnAccessDenied!);

                const fileNameWithoutFolder = path.basename(fileName);
                fileName = path.join(trySavingToFolder, fileNameWithoutFolder);

                FileHelpers.writeAllText(fileName, content, null);
            }
        }
        return fileName;
    }

    public static void AppendAllText(string fileName, string trajectoryContent)
    {
        DirectoryHelpers.EnsureDirectoryForFileExists(fileName);
        File.AppendAllText(fileName, trajectoryContent, Encoding.UTF8);
    }

    public static string? WriteTextToTempFile(string? text, string? extension = null)
    {
        if (text != null && text !== '') {
            const os = require('os');
            const tempFile = path.join(os.tmpdir(), `temp_${Date.now()}_${Math.random().toString(36).substring(7)}`);
            let finalTempFile = tempFile;
            
            if (extension != null && extension !== '') {
                finalTempFile = `${tempFile}.${extension.replace(/^\./, '')}`;
            }

            fs.writeFileSync(finalTempFile, text, 'utf8');
            return finalTempFile;
        }
        return null;
    }

    public static getFriendlyLastModified(filePath: string): string
    {
        const stats = fs.statSync(filePath);
        const modified = stats.mtime;
        const now = new Date();
        const modifiedSeconds = Math.floor((now.getTime() - modified.getTime()) / 1000);
        const modifiedMinutes = Math.floor(modifiedSeconds / 60);
        const modifiedHours = Math.floor(modifiedSeconds / 3600);
        const modifiedDays = Math.floor(modifiedSeconds / 86400);

        const formatted =
            modifiedMinutes < 1 ? 'just now' :
            modifiedMinutes === 1 ? '1 minute ago' :
            modifiedMinutes < 60 ? `${modifiedMinutes} minutes ago` :
            modifiedHours === 1 ? '1 hour ago' :
            modifiedHours < 24 ? `${modifiedHours} hours ago` :
            modifiedDays === 1 ? '1 day ago' :
            modifiedDays < 7 ? `${modifiedDays} days ago` :
            modified.toString();

        return formatted;
    }

    public static getFriendlySize(filePath: string): string
    {
        const stats = fs.statSync(filePath);
        const size = stats.size;
        const sizeFormatted = size >= 1024 * 1024 * 1024
            ? `${Math.floor(size / (1024 * 1024 * 1024))} GB`
            : size >= 1024 * 1024
                ? `${Math.floor(size / (1024 * 1024))} MB`
                : size >= 1024
                    ? `${Math.floor(size / 1024)} KB`
                    : `${size} bytes`;
        return sizeFormatted;
    }

    public static getMarkdownLanguage(extension: string): string
    {
        switch (extension) {
            case '.bat': return 'batch';
            case '.bmp': return 'markdown';
            case '.cpp': return 'cpp';
            case '.cs': return 'csharp';
            case '.csproj': return 'xml';
            case '.css': return 'css';
            case '.docx': return 'markdown';
            case '.gif': return 'markdown';
            case '.go': return 'go';
            case '.html': return 'html';
            case '.java': return 'java';
            case '.jpeg': return 'markdown';
            case '.jpg': return 'markdown';
            case '.js': return 'javascript';
            case '.json': return 'json';
            case '.kt': return 'kotlin';
            case '.m': return 'objective-c';
            case '.md': return 'markdown';
            case '.pdf': return 'markdown';
            case '.php': return 'php';
            case '.pl': return 'perl';
            case '.png': return 'markdown';
            case '.pptx': return 'markdown';
            case '.py': return 'python';
            case '.r': return 'r';
            case '.rb': return 'ruby';
            case '.rs': return 'rust';
            case '.scala': return 'scala';
            case '.sh': return 'bash';
            case '.sln': return 'xml';
            case '.sql': return 'sql';
            case '.swift': return 'swift';
            case '.ts': return 'typescript';
            case '.xml': return 'xml';
            case '.yaml': return 'yaml';
            case '.yml': return 'yaml';
            default: return 'plaintext';
        }
    }

    public static generateUniqueFileNameFromUrl(url: string, saveToFolder: string): string
    {
        DirectoryHelpers.ensureDirectoryExists(saveToFolder);

        const urlObj = new URL(url);
        const urlPath = urlObj.hostname + urlObj.pathname + urlObj.search;

        const parts = urlPath.split(FileHelpers._invalidFileNameCharsForWeb)
            .map(p => p.trim())
            .filter(p => p !== '');

        const check = path.join(saveToFolder, parts.join('-'));
        if (!fs.existsSync(check)) return check;

        while (true) {
            const guidPart = Math.random().toString(36).substring(2, 10);
            const fileTimePart = Date.now().toString();
            const tryThis = check + '-' + fileTimePart + '-' + guidPart;
            if (!fs.existsSync(tryThis)) return tryThis;
        }
    }

    public static getProgramAssemblyFileInfo(): { fullName: string, directoryName: string | null }
    {
        // In Node.js, we use the executable path or main script path
        let assemblyPath = process.execPath;
        
        // For development scenarios, use the main module filename
        if (require.main && require.main.filename) {
            assemblyPath = require.main.filename;
        }
        
        return {
            fullName: assemblyPath,
            directoryName: path.dirname(assemblyPath)
        };
    }

    private static getInvalidFileNameCharsForWeb(): RegExp
    {
        // Create a regex pattern for invalid filename characters
        // Including all non-alphanumeric characters except hyphens
        return /[^a-zA-Z0-9\-]/g;
    }

    private static _invalidFileNameCharsForWeb = FileHelpers.getInvalidFileNameCharsForWeb();
}
