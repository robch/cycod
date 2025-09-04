import * as fs from 'fs';
import * as path from 'path';
import * as child_process from 'child_process';
import { FileHelpers } from './FileHelpers';
import { ConsoleHelpers } from './ConsoleHelpers';
import { OS } from './OS';

export interface RunnableProcessResult {
    standardOutput: string;
    standardError: string;
    mergedOutput: string;
    exitCode: number;
    completionState: ProcessCompletionState;
}

export enum ProcessCompletionState {
    Completed,
    TimedOut,
    Killed
}

export class RunnableProcessBuilder {
    private _fileName: string = '';
    private _arguments: string = '';
    private _workingDirectory?: string;
    private _timeout?: number;
    private _commandLine: string = '';
    private _envVars?: { [key: string]: string };
    private _standardInput?: string;

    public withFileName(fileName: string): RunnableProcessBuilder {
        this._fileName = fileName;
        return this;
    }

    public withArguments(args: string): RunnableProcessBuilder {
        this._arguments = args;
        return this;
    }

    public withCommandLine(commandLine: string): RunnableProcessBuilder {
        this._commandLine = commandLine;
        return this;
    }

    public withWorkingDirectory(workingDirectory?: string): RunnableProcessBuilder {
        this._workingDirectory = workingDirectory;
        return this;
    }

    public withEnvironmentVariables(envVars?: { [key: string]: string }): RunnableProcessBuilder {
        this._envVars = envVars;
        return this;
    }

    public withStandardInput(input?: string): RunnableProcessBuilder {
        this._standardInput = input;
        return this;
    }

    public withTimeout(timeout?: number): RunnableProcessBuilder {
        this._timeout = timeout;
        return this;
    }

    public async runAsync(): Promise<RunnableProcessResult> {
        // Parse command line if provided
        if (this._commandLine) {
            const { fileName, arguments: args } = ProcessHelpers.splitCommand(this._commandLine);
            this._fileName = fileName;
            this._arguments = args;
        }

        return new Promise<RunnableProcessResult>((resolve) => {
            let stdOut = '';
            let stdErr = '';
            let merged = '';
            let completed = false;

            const options: child_process.SpawnOptions = {
                cwd: this._workingDirectory,
                shell: true,
                stdio: ['pipe', 'pipe', 'pipe'],
                env: this._envVars ? { ...process.env, ...this._envVars } : process.env
            };

            const args = this._arguments ? ProcessHelpers.splitArguments(this._arguments) : [];
            const child = child_process.spawn(this._fileName, args, options);

            child.stdout?.on('data', (data) => {
                const str = data.toString();
                stdOut += str;
                merged += str;
            });

            child.stderr?.on('data', (data) => {
                const str = data.toString();
                stdErr += str;
                merged += str;
            });

            // Handle standard input
            if (this._standardInput && child.stdin) {
                child.stdin.write(this._standardInput);
                child.stdin.end();
            }

            // Setup timeout if specified
            let timeoutHandle: NodeJS.Timeout | undefined;
            if (this._timeout) {
                timeoutHandle = setTimeout(() => {
                    if (!completed) {
                        completed = true;
                        child.kill();
                        resolve({
                            standardOutput: stdOut,
                            standardError: stdErr,
                            mergedOutput: merged,
                            exitCode: -1,
                            completionState: ProcessCompletionState.TimedOut
                        });
                    }
                }, this._timeout);
            }

            child.on('close', (code) => {
                if (!completed) {
                    completed = true;
                    if (timeoutHandle) clearTimeout(timeoutHandle);
                    resolve({
                        standardOutput: stdOut,
                        standardError: stdErr,
                        mergedOutput: merged,
                        exitCode: code || 0,
                        completionState: ProcessCompletionState.Completed
                    });
                }
            });

            child.on('error', (error) => {
                if (!completed) {
                    completed = true;
                    if (timeoutHandle) clearTimeout(timeoutHandle);
                    resolve({
                        standardOutput: stdOut,
                        standardError: stdErr + error.message,
                        mergedOutput: merged + error.message,
                        exitCode: -1,
                        completionState: ProcessCompletionState.Killed
                    });
                }
            });
        });
    }

    public run(): RunnableProcessResult {
        // For compatibility with synchronous calls, use a blocking pattern
        const result = this.runAsync();
        return result as any; // Note: This breaks the promise chain but maintains API compatibility
    }
}

export class ProcessHelpers {
    private static _cliCache: Map<string, string> = new Map();

    public static runShellScript(
        shell: string, 
        script: string, 
        scriptArgs?: string, 
        workingDirectory?: string, 
        envVars?: { [key: string]: string }, 
        input?: string, 
        timeout?: number
    ): RunnableProcessResult {
        return ProcessHelpers.runShellScriptAsync(shell, script, scriptArgs, workingDirectory, envVars, input, timeout) as any;
    }

    public static async runShellScriptAsync(
        shell: string, 
        script: string, 
        scriptArgs?: string, 
        workingDirectory?: string, 
        envVars?: { [key: string]: string }, 
        input?: string, 
        timeout?: number
    ): Promise<RunnableProcessResult> {
        const filesToDelete: string[] = [];

        try {
            const scriptFileExt = ProcessHelpers.getShellScriptFileExtension(shell);
            const scriptWrapped = ProcessHelpers.wrapScriptContent(shell, script);
            const scriptFileName = FileHelpers.writeTextToTempFile(scriptWrapped, scriptFileExt)!;
            filesToDelete.push(scriptFileName);

            const { shellProcessName, shellArgsFormat } = ProcessHelpers.getShellProcessNameAndArgsFormat(shell);
            ConsoleHelpers.writeDebugLine(`RunShellScriptAsync: ${shellProcessName} ${shellArgsFormat}`);
            const shellArgsFormatted = shellArgsFormat
                .replace('{0}', scriptFileName)
                .replace('{1}', scriptArgs || '')
                .trim();
            ConsoleHelpers.writeDebugLine(`RunShellScriptAsync: ${shellProcessName} ${shellArgsFormatted}`);

            const hasScriptFileName = shellArgsFormatted.includes(scriptFileName);
            if (!hasScriptFileName) {
                throw new Error(`Script file name not found in shell arguments: ${shellArgsFormatted}`);
            }

            const builder = new RunnableProcessBuilder()
                .withFileName(shellProcessName)
                .withArguments(shellArgsFormatted)
                .withWorkingDirectory(workingDirectory)
                .withEnvironmentVariables(envVars)
                .withStandardInput(input)
                .withTimeout(timeout);

            return await builder.runAsync();
        } finally {
            const skipDelete = ConsoleHelpers.isDebug();
            if (!skipDelete) {
                filesToDelete.forEach(x => {
                    try {
                        fs.unlinkSync(x);
                    } catch {
                        // Ignore file deletion errors
                    }
                });
            }
        }
    }

    public static runProcess(
        command: string, 
        workingDirectory?: string, 
        envVars?: { [key: string]: string }, 
        input?: string, 
        timeout?: number
    ): RunnableProcessResult {
        return ProcessHelpers.runProcessAsync(command, workingDirectory, envVars, input, timeout) as any;
    }

    public static async runProcessAsync(
        command: string, 
        workingDirectory?: string, 
        envVars?: { [key: string]: string }, 
        input?: string, 
        timeout?: number
    ): Promise<RunnableProcessResult> {
        const processBuilder = new RunnableProcessBuilder()
            .withCommandLine(command)
            .withWorkingDirectory(workingDirectory)
            .withEnvironmentVariables(envVars)
            .withStandardInput(input)
            .withTimeout(timeout);

        return await processBuilder.runAsync();
    }

    public static async runProcessAsync2(processName: string, args: string, timeout: number = Number.MAX_SAFE_INTEGER): Promise<[string, number]> {
        return new Promise((resolve) => {
            let stdOut = '';
            let stdErr = '';
            let merged = '';

            const child = child_process.spawn(processName, ProcessHelpers.splitArguments(args), {
                stdio: ['pipe', 'pipe', 'pipe']
            });

            child.stdout?.on('data', (data) => {
                const str = data.toString();
                stdOut += str;
                merged += str;
            });

            child.stderr?.on('data', (data) => {
                const str = data.toString();
                stdErr += str;
                merged += str;
            });

            const timeoutHandle = setTimeout(() => {
                child.kill();
            }, timeout);

            child.on('close', (code) => {
                clearTimeout(timeoutHandle);
                resolve([merged, code || 0]);
            });

            child.on('error', () => {
                clearTimeout(timeoutHandle);
                resolve([merged, -1]);
            });
        });
    }

    public static escapeProcessArgument(arg: string): string {
        if (!arg || arg.length === 0) return arg;
        
        // Check if already quoted
        const alreadyDoubleQuoted = arg.startsWith('"') && arg.endsWith('"');
        if (alreadyDoubleQuoted) return arg;

        // If no special characters, return as is
        const noSpacesOrSpecialChars = !arg.includes(' ') && !arg.includes('\\') && !arg.includes('"');
        if (noSpacesOrSpecialChars) return arg;

        // Escape backslashes and quotes
        const escaped = arg.replace(/\\/g, '\\\\').replace(/"/g, '\\"');
        
        // Add quotes if needed
        const needsDoubleQuotes = escaped.includes(' ') || escaped.includes('\\') || escaped.includes('"');
        return needsDoubleQuotes ? `"${escaped}"` : escaped;
    }
    
    public static escapeBashArgument(arg: string, forStdin: boolean = false): string {
        if (!arg || arg.length === 0) return arg;
        
        if (forStdin) {
            // For stdin injection, we need to escape single quotes differently
            return arg.replace(/'/g, "'\\''");
        } else {
            // For command line arguments, use single quotes
            const needsQuoting = arg.includes("'") || arg.includes(' ') || arg.includes('"') || 
                                arg.includes('\\') || arg.includes(';') || arg.includes('|') || 
                                arg.includes('<') || arg.includes('>') || arg.includes('&');
                                
            if (needsQuoting) {
                // Escape single quotes with '\'' pattern
                return `'${arg.replace(/'/g, "'\\''")}'`;
            }
            
            return arg;
        }
    }
    
    public static escapeCmdArgument(arg: string, forStdin: boolean = false): string {
        if (!arg || arg.length === 0) return arg;
        
        // CMD has different escaping depending on whether we're using it directly or through stdin
        if (forStdin) {
            // For stdin, we need to escape special characters with ^
            return arg
                .replace(/\^/g, '^^')
                .replace(/&/g, '^&')
                .replace(/\|/g, '^|')
                .replace(/</g, '^<')
                .replace(/>/g, '^>')
                .replace(/\(/g, '^(')
                .replace(/\)/g, '^)')
                .replace(/%/g, '^%');
        } else {
            // For command line, handle quotes differently
            const needsQuotes = arg.includes(' ') || arg.includes('&') || arg.includes('|') || 
                                arg.includes('%') || arg.includes('<') || arg.includes('>');
            
            if (needsQuotes) {
                // Double up internal quotes
                const escaped = arg.replace(/"/g, '""');
                return `"${escaped}"`;
            }
            
            return arg;
        }
    }
    
    public static escapePowerShellArgument(arg: string, forStdin: boolean = false): string {
        if (!arg || arg.length === 0) return arg;
        
        if (forStdin) {
            // For stdin injection, escape with backticks
            return arg
                .replace(/`/g, '``')
                .replace(/"/g, '`"')
                .replace(/\$/g, '`$')
                .replace(/&/g, '`&')
                .replace(/;/g, '`;')
                .replace(/\(/g, '`(')
                .replace(/\)/g, '`)')
                .replace(/\|/g, '`|');
        } else {
            // For command line, use double quotes with escaping
            const needsQuotes = arg.includes(' ') || arg.includes('&') || arg.includes(';') || 
                                arg.includes('(') || arg.includes(')') || arg.includes('<') ||
                                arg.includes('>') || arg.includes('|') || arg.includes('"');
            
            if (needsQuotes) {
                // Escape double quotes with double quotes (PowerShell convention)
                const escaped = arg.replace(/"/g, '""');
                return `"${escaped}"`;
            }
            
            return arg;
        }
    }
    
    public static splitCommand(commandLine: string): { fileName: string; arguments: string } {
        if (!commandLine || commandLine.length === 0) {
            return { fileName: '', arguments: '' };
        }
        
        commandLine = commandLine.trim();
        
        // Check if the command starts with a quoted path
        if (commandLine.startsWith('"')) {
            const closingQuoteIndex = commandLine.indexOf('"', 1);
            if (closingQuoteIndex > 1) {
                // Extract filename from quotes
                const fileName = commandLine.substring(1, closingQuoteIndex);
                
                // Get the rest as arguments, if any
                const args = closingQuoteIndex < commandLine.length - 1 
                    ? commandLine.substring(closingQuoteIndex + 1).trimStart() 
                    : '';

                return { fileName, arguments: args };
            }
        }
        
        // No quotes or invalid quotes, split on first space
        const spaceIndex = commandLine.indexOf(' ');
        
        if (spaceIndex > 0) {
            const fileName = commandLine.substring(0, spaceIndex);
            const args = commandLine.substring(spaceIndex + 1).trimStart();
            return { fileName, arguments: args };
        } else {
            // No space, the whole string is the filename
            return { fileName: commandLine, arguments: '' };
        }
    }

    public static splitArguments(args?: string): string[] {
        if (!args || args.length === 0) {
            return [];
        }

        const result: string[] = [];
        let currentArg = '';
        let inQuotes = false;
        let quoteChar = '';

        for (const c of args) {
            if ((c === '"' || c === "'") && !inQuotes) {
                // Opening quote
                inQuotes = true;
                quoteChar = c;
            } else if (c === quoteChar && inQuotes) {
                // Closing quote
                inQuotes = false;
                if (currentArg.length > 0) {
                    result.push(currentArg);
                    currentArg = '';
                }
            } else if (c === ' ' && !inQuotes) {
                // Space outside quotes
                if (currentArg.length > 0) {
                    result.push(currentArg);
                    currentArg = '';
                }
            } else {
                currentArg += c;
            }
        }

        // Add the last argument if any
        if (currentArg.length > 0) {
            result.push(currentArg);
        }

        return result;
    }

    public static findBashExe(): string {
        const gitBash = ProcessHelpers.findAndCacheGitBashExe();
        if (!gitBash || gitBash === 'bash.exe') {
            throw new Error('Could not find Git for Windows bash.exe in PATH!');
        }
        return gitBash;
    }

    public static findPwshExe(): string {
        const pwshExe = ProcessHelpers.findAndCachePwshExe();
        if (!pwshExe || pwshExe === 'pwsh.exe') {
            throw new Error('Could not find PowerShell Core pwsh.exe in PATH!');
        }
        return pwshExe;
    }

    private static findAndCacheGitBashExe(): string {
        const bashExe = 'bash.exe';
        if (ProcessHelpers._cliCache.has(bashExe)) {
            return ProcessHelpers._cliCache.get(bashExe)!;
        }

        const found = ProcessHelpers.findNoCacheGitBashExe();
        ProcessHelpers._cliCache.set(bashExe, found);

        return found;
    }

    private static findNoCacheGitBashExe(): string {
        const found = FileHelpers.findFilesInOsPath('bash.exe');
        return found.find(x => x.toLowerCase().includes('git')) || 'bash.exe';
    }

    private static findAndCachePwshExe(): string {
        const pwshExe = 'pwsh.exe';
        if (ProcessHelpers._cliCache.has(pwshExe)) {
            return ProcessHelpers._cliCache.get(pwshExe)!;
        }

        const found = ProcessHelpers.findNoCachePwshExe();
        ProcessHelpers._cliCache.set(pwshExe, found);

        return found;
    }

    private static findNoCachePwshExe(): string {
        const found = FileHelpers.findFilesInOsPath(OS.isWindows() ? 'pwsh.exe' : 'pwsh');
        const tryPowerShellExe = found.length === 0 && OS.isWindows();
        if (tryPowerShellExe) {
            const powershellFound = FileHelpers.findFilesInOsPath('powershell.exe');
            return powershellFound[0] || (OS.isWindows() ? 'powershell.exe' : 'pwsh');
        }

        return found[0] || (OS.isWindows() ? 'powershell.exe' : 'pwsh');
    }

    private static getShellProcessNameAndArgsFormat(shell: string): { shellProcessName: string; shellArgsFormat: string } {
        const { fileName: shellCommand, arguments: shellArgsFormat } = ProcessHelpers.splitCommand(shell);
        
        let shellProcessName: string;
        switch (shellCommand) {
            case 'cmd':
                shellProcessName = 'cmd.exe';
                break;
            case 'bash':
                shellProcessName = OS.isWindows() ? ProcessHelpers.findBashExe() : 'bash';
                break;
            case 'pwsh':
            case 'powershell':
                shellProcessName = ProcessHelpers.findPwshExe();
                break;
            default:
                shellProcessName = shellCommand;
                break;
        }

        let finalShellArgsFormat = shellArgsFormat;
        if (!finalShellArgsFormat || finalShellArgsFormat.length === 0) {
            finalShellArgsFormat = ProcessHelpers.getShellProcessArgsFormat(shellCommand);
        }

        const missingCurlyBracketZero = !finalShellArgsFormat.includes('{0}');
        finalShellArgsFormat = missingCurlyBracketZero ? finalShellArgsFormat + ' {0}' : finalShellArgsFormat;

        const missingCurlyBracketOne = !finalShellArgsFormat.includes('{1}');
        finalShellArgsFormat = missingCurlyBracketOne ? finalShellArgsFormat + ' {1}' : finalShellArgsFormat;

        return { shellProcessName, shellArgsFormat: finalShellArgsFormat };
    }

    private static getShellProcessArgsFormat(shellType: string): string {
        const argsFormat = ProcessHelpers._argsFormat.get(shellType.toLowerCase());
        return argsFormat || '';
    }

    private static getShellScriptFileExtension(shellType: string): string {
        const extension = ProcessHelpers._scriptExtension.get(shellType.toLowerCase());
        return extension || '';
    }

    private static wrapScriptContent(shellType: string, contents: string): string {
        switch (shellType) {
            case 'cmd':
                // Note, use @echo off instead of using the /Q command line switch.
                // When /Q is used, echo can't be turned on.
                contents = `@echo off\n${contents}`;
                break;
            case 'powershell':
            case 'pwsh':
                const prepend = "$ErrorActionPreference = 'stop'";
                const append = "if ((Test-Path -LiteralPath variable:\\LASTEXITCODE)) { exit $LASTEXITCODE }";
                contents = `${prepend}\n${contents}\n${append}`;
                break;
        }
        return contents;
    }

    // Common executable and script extensions on Windows
    private static readonly WindowsExecutableExtensions = ['.exe', '.cmd', '.bat', '.ps1'];

    /**
     * Finds an executable in the PATH with common Windows extensions.
     * @param baseName The base name of the executable without extension (e.g., "npm")
     * @returns The full path of the executable if found, or the original name if not found
     */
    public static findExecutableInPath(baseName: string): string {
        if (!baseName || baseName.length === 0) {
            return baseName;
        }

        // If we're not on Windows or the file already has an extension or exists, return it
        if (!OS.isWindows() || path.extname(baseName) || fs.existsSync(baseName)) {
            return baseName;
        }

        // Check for name in cache
        if (ProcessHelpers._cliCache.has(baseName)) {
            return ProcessHelpers._cliCache.get(baseName)!;
        }

        // Try to find with various extensions
        for (const ext of ProcessHelpers.WindowsExecutableExtensions) {
            // Skip if already has this extension
            if (baseName.endsWith(ext)) {
                continue;
            }

            const nameWithExt = baseName + ext;
            
            // First check local path
            if (fs.existsSync(nameWithExt)) {
                ProcessHelpers._cliCache.set(baseName, nameWithExt);
                return nameWithExt;
            }

            // Then check in PATH
            const foundInPath = FileHelpers.findFilesInOsPath(nameWithExt)[0];
            if (foundInPath) {
                ProcessHelpers._cliCache.set(baseName, foundInPath);
                return foundInPath;
            }
        }

        // Special handling for common npm and node tools in predictable locations
        if (['npm', 'npx', 'node'].includes(baseName.toLowerCase())) {
            const programFiles = process.env['ProgramFiles'] || 'C:\\Program Files';
            const nodejsPath = path.join(programFiles, 'nodejs');
            
            for (const ext of ProcessHelpers.WindowsExecutableExtensions) {
                const fullPath = path.join(nodejsPath, baseName + ext);
                if (fs.existsSync(fullPath)) {
                    ProcessHelpers._cliCache.set(baseName, fullPath);
                    return fullPath;
                }
            }
        }

        // If not found, cache the original name to avoid repeated searches
        ProcessHelpers._cliCache.set(baseName, baseName);
        return baseName;
    }

    private static readonly _argsFormat = new Map<string, string>([
        ['cmd', '/D /E:ON /V:OFF /S /C "CALL "{0}" {1}"'],
        ['pwsh', '-command "{0}" {1}'],
        ['powershell', '-Command "{0}" {1}'],
        ['bash', '--noprofile --norc -e -o pipefail {0} {1}'],
        ['sh', '-e {0} {1}'],
        ['python', '{0} {1}']
    ]);

    private static readonly _scriptExtension = new Map<string, string>([
        ['cmd', '.cmd'],
        ['pwsh', '.ps1'],
        ['powershell', '.ps1'],
        ['bash', '.sh'],
        ['sh', '.sh'],
        ['python', '.py']
    ]);
}