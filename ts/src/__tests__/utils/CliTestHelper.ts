import { spawn, ChildProcess } from 'child_process';
import * as path from 'path';

export interface CliResult {
  stdout: string;
  stderr: string;
  exitCode: number;
}

export class CliTestHelper {
  private static readonly CLI_PATH = path.join(__dirname, '../../../dist/bin/cycod.js');
  
  /**
   * Execute a CLI command and return the result
   */
  static async executeCommand(args: string[]): Promise<CliResult> {
    return new Promise((resolve, reject) => {
      const child: ChildProcess = spawn('node', [this.CLI_PATH, ...args], {
        stdio: 'pipe',
        env: { ...process.env }
      });

      let stdout = '';
      let stderr = '';

      if (child.stdout) {
        child.stdout.on('data', (data) => {
          stdout += data.toString();
        });
      }

      if (child.stderr) {
        child.stderr.on('data', (data) => {
          stderr += data.toString();
        });
      }

      child.on('close', (code) => {
        resolve({
          stdout: stdout.trim(),
          stderr: stderr.trim(),
          exitCode: code || 0
        });
      });

      child.on('error', (error) => {
        reject(error);
      });

      // Set a timeout to prevent hanging tests
      const timeout = setTimeout(() => {
        child.kill();
        reject(new Error('Command timeout'));
      }, 10000); // 10 second timeout

      child.on('close', () => {
        clearTimeout(timeout);
      });
    });
  }

  /**
   * Parse a command string into arguments array
   */
  static parseCommand(command: string): string[] {
    // Remove 'cycod' prefix if present
    const cleanCommand = command.replace(/^cycod\s+/, '');
    
    // Split by spaces but respect quoted strings
    const args: string[] = [];
    let current = '';
    let inQuotes = false;
    
    for (let i = 0; i < cleanCommand.length; i++) {
      const char = cleanCommand[i];
      
      if (char === '"' || char === "'") {
        inQuotes = !inQuotes;
      } else if (char === ' ' && !inQuotes) {
        if (current) {
          args.push(current);
          current = '';
        }
      } else {
        current += char;
      }
    }
    
    if (current) {
      args.push(current);
    }
    
    return args;
  }

  /**
   * Execute a command from a string (like in YAML tests)
   */
  static async run(command: string): Promise<CliResult> {
    const args = this.parseCommand(command);
    return this.executeCommand(args);
  }
}

export class RegexMatcher {
  /**
   * Check if text matches a regex pattern (similar to YAML expect-regex)
   */
  static matches(text: string, pattern: string): boolean {
    // Handle multi-line patterns from YAML
    const cleanPattern = pattern.trim();
    // Use dotAll flag to make . match newlines
    const regex = new RegExp(cleanPattern, 'ms');
    return regex.test(text);
  }

  /**
   * Jest custom matcher for regex expectations
   */
  static expectToMatchRegex(received: string, pattern: string): void {
    const matches = RegexMatcher.matches(received, pattern);
    if (!matches) {
      throw new Error(`Expected output to match regex pattern:\n${pattern}\n\nActual output:\n${received}`);
    }
  }
}

// Extend Jest matchers
declare global {
  namespace jest {
    interface Matchers<R> {
      toMatchYamlRegex(pattern: string): R;
    }
  }
}