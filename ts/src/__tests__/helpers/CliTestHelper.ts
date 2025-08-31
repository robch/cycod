import { spawn } from 'child_process';
import * as path from 'path';

export interface CliResult {
  exitCode: number;
  stdout: string;
  stderr: string;
}

export class CliTestHelper {
  private static readonly CLI_PATH = path.join(__dirname, '../../../dist/bin/cycodjs.js');

  static async run(command: string): Promise<CliResult> {
    return new Promise((resolve) => {
      // Parse command into parts
      const parts = command.split(' ');
      const cliCommand = parts[0]; // Should be 'cycodjs' 
      const args = parts.slice(1); // Everything after 'cycodjs'

      // Replace 'cycod' with 'cycodjs' if needed
      const actualArgs = args.map(arg => arg === 'cycod' ? 'cycodjs' : arg);

      const child = spawn('node', [CliTestHelper.CLI_PATH, ...actualArgs], {
        stdio: ['pipe', 'pipe', 'pipe'],
        env: { ...process.env, NODE_ENV: 'test' },
        cwd: process.cwd() // Ensure consistent working directory
      });

      let stdout = '';
      let stderr = '';

      child.stdout?.on('data', (data) => {
        stdout += data.toString();
      });

      child.stderr?.on('data', (data) => {
        stderr += data.toString();
      });

      child.on('close', (code) => {
        resolve({
          exitCode: code || 0,
          stdout: stdout.trim(),
          stderr: stderr.trim()
        });
      });

      child.on('error', (error) => {
        resolve({
          exitCode: 1,
          stdout: '',
          stderr: error.message
        });
      });
    });
  }

  static async cleanup(key: string): Promise<void> {
    // Clean up test keys from all specific scopes (--any is not valid for clear command)
    const scopes = ['--local', '--user', '--global'];
    
    for (const scope of scopes) {
      try {
        await CliTestHelper.run(`cycodjs config clear ${key} ${scope}`);
        // Don't throw on errors, just continue cleanup
      } catch (error) {
        // Ignore cleanup errors
      }
    }
    
    // Small delay to ensure cleanup completes
    await new Promise(resolve => setTimeout(resolve, 10));
  }

  static async cleanupAllTestKeys(): Promise<void> {
    // List of all keys used across all test files
    const testKeys = [
      'TestKey',
      'TestList', 
      'NonexistentKey',
      'OpenAI.ApiKey',
      'Deep.Nested.Config.Key',
      'App.Setting',
      'App.Setting.Nested',
      'Scopes.Nested.Test',
      'Scopes.Local.Test',
      'Scopes.User.Test',
      'Scopes.Global.Test',
      'BoolTest',
      'TestInheritance'
    ];

    for (const key of testKeys) {
      await CliTestHelper.cleanup(key);
    }
  }
}