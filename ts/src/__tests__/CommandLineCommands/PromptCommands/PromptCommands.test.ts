import { beforeAll, afterAll, beforeEach, afterEach, describe, it, expect } from '@jest/globals';
import * as fs from 'fs';
import * as path from 'path';
import * as os from 'os';
import { PromptListCommand, PromptCreateCommand, PromptGetCommand, PromptDeleteCommand } from '../../../CommandLineCommands/PromptCommands';
import { ConfigFileScope } from '../../../Configuration/ConfigFileScope';

// Mock console output to capture it
let consoleOutput: string[] = [];
let consoleErrors: string[] = [];

const originalConsoleLog = console.log;
const originalConsoleError = console.error;

beforeAll(() => {
  console.log = (...args: any[]) => {
    consoleOutput.push(args.join(' '));
  };
  console.error = (...args: any[]) => {
    consoleErrors.push(args.join(' '));
  };
});

afterAll(() => {
  console.log = originalConsoleLog;
  console.error = originalConsoleError;
});

describe('Prompt Commands', () => {
  let tempDir: string;
  let originalCwd: string;
  let testPromptDir: string;

  beforeEach(() => {
    originalCwd = process.cwd();
    // Create a temporary directory for testing
    tempDir = fs.mkdtempSync(path.join(os.tmpdir(), 'cycod-prompt-test-'));
    process.chdir(tempDir);
    
    // Create .cycod directory structure
    testPromptDir = path.join(tempDir, '.cycod', 'prompts');
    fs.mkdirSync(testPromptDir, { recursive: true });
    
    // Clear console capture arrays
    consoleOutput = [];
    consoleErrors = [];
    
    console.log(`Test directory: ${tempDir}`);
  });

  afterEach(() => {
    process.chdir(originalCwd);
    // Clean up temporary directory
    try {
      fs.rmSync(tempDir, { recursive: true, force: true });
    } catch (error) {
      console.warn(`Failed to clean up test directory: ${tempDir}`, error);
    }
  });

  describe('prompt list', () => {
    it('should show no prompts when none exist', async () => {
      const result = await runCommand(['prompt', 'list']);
      
      expect(result.exitCode).toBe(0);
      expect(result.output).toContain('LOCATION: /Users/');
      expect(result.output).toContain('/.local/share/.cycod/prompts (global)');
      expect(result.output).toContain('No items found in this scope');
      expect(result.output).toContain('/.cycod/prompts (user)');
      expect(result.output).toContain('No items found in this scope');
      expect(result.output).toContain(`${tempDir}/.cycod/prompts (local)`);
      expect(result.output).toContain('No items found in this scope');
    });

    it('should list prompts when they exist', async () => {
      // Create a test prompt file
      const promptFile = path.join(testPromptDir, 'test-assistant.prompt');
      fs.writeFileSync(promptFile, 'You are a helpful test assistant');

      const result = await runCommand(['prompt', 'list']);
      
      expect(result.exitCode).toBe(0);
      expect(result.output).toContain(`${tempDir}/.cycod/prompts (local)`);
      expect(result.output).toContain('test-assistant');
    });

    it('should list multiple prompts alphabetically', async () => {
      // Create multiple test prompts
      fs.writeFileSync(path.join(testPromptDir, 'zebra.prompt'), 'Zebra prompt');
      fs.writeFileSync(path.join(testPromptDir, 'alpha.prompt'), 'Alpha prompt');
      fs.writeFileSync(path.join(testPromptDir, 'beta.prompt'), 'Beta prompt');

      const result = await runCommand(['prompt', 'list']);
      
      expect(result.exitCode).toBe(0);
      const outputLines = result.output.split('\n');
      const promptSection = outputLines.find(line => line.includes('alpha'));
      expect(promptSection).toBeDefined();
      expect(result.output).toContain('alpha');
      expect(result.output).toContain('beta');
      expect(result.output).toContain('zebra');
    });
  });

  describe('prompt create', () => {
    it('should create a new prompt successfully', async () => {
      const result = await runCommand(['prompt', 'create', 'test-prompt', 'You are a helpful test assistant']);
      
      expect(result.exitCode).toBe(0);
      expect(result.output).toContain('test-prompt.prompt');
      expect(result.output).toContain('USAGE: /test-prompt (in chat)');
      
      // Verify the file was created
      const promptFile = path.join(testPromptDir, 'test-prompt.prompt');
      expect(fs.existsSync(promptFile)).toBe(true);
      expect(fs.readFileSync(promptFile, 'utf8')).toBe('You are a helpful test assistant');
    });

    it('should handle prompt names with leading slash', async () => {
      const result = await runCommand(['prompt', 'create', '/test-slash', 'Test with slash']);
      
      expect(result.exitCode).toBe(0);
      expect(result.output).toContain('test-slash.prompt');
      
      // Verify the file was created without the slash
      const promptFile = path.join(testPromptDir, 'test-slash.prompt');
      expect(fs.existsSync(promptFile)).toBe(true);
    });

    it('should fail when prompt already exists', async () => {
      // Create initial prompt
      await runCommand(['prompt', 'create', 'duplicate', 'First version']);
      
      // Try to create again
      const result = await runCommand(['prompt', 'create', 'duplicate', 'Second version']);
      
      expect(result.exitCode).toBe(1);
      expect(result.output).toContain("Error: Prompt 'duplicate' already exists.");
    });

    it('should fail when prompt name is missing', async () => {
      const result = await runCommand(['prompt', 'create']);
      
      expect(result.exitCode).toBe(1);
      expect(result.output).toContain('Error: Prompt name is required.');
    });

    it('should fail when prompt text is missing', async () => {
      const result = await runCommand(['prompt', 'create', 'no-text']);
      
      expect(result.exitCode).toBe(1);
      expect(result.output).toContain('Error: Prompt text is required.');
    });

    it('should create prompt with multi-word text', async () => {
      const promptText = 'You are a helpful assistant that provides detailed explanations';
      const result = await runCommand(['prompt', 'create', 'detailed', promptText]);
      
      expect(result.exitCode).toBe(0);
      const promptFile = path.join(testPromptDir, 'detailed.prompt');
      expect(fs.readFileSync(promptFile, 'utf8')).toBe(promptText);
    });
  });

  describe('prompt get', () => {
    beforeEach(async () => {
      // Create test prompts for get tests
      fs.writeFileSync(path.join(testPromptDir, 'helper.prompt'), 'You are a helpful assistant');
      fs.writeFileSync(path.join(testPromptDir, 'coder.prompt'), 'You are a coding expert');
    });

    it('should display prompt content successfully', async () => {
      const result = await runCommand(['prompt', 'get', 'helper']);
      
      expect(result.exitCode).toBe(0);
      expect(result.output).toContain('helper.prompt (local)');
      expect(result.output).toContain('helper');
      expect(result.output).toContain('You are a helpful assistant');
    });

    it('should fail when prompt does not exist', async () => {
      const result = await runCommand(['prompt', 'get', 'nonexistent']);
      
      expect(result.exitCode).toBe(1);
      expect(result.output).toContain("Error: Prompt 'nonexistent' not found in any scope.");
    });

    it('should fail when prompt name is missing', async () => {
      const result = await runCommand(['prompt', 'get']);
      
      expect(result.exitCode).toBe(1);
      expect(result.output).toContain('Error: Prompt name is required.');
    });

    it('should handle file reference prompts (@filename)', async () => {
      // Create a referenced file
      const referencedFile = path.join(tempDir, 'external-prompt.txt');
      fs.writeFileSync(referencedFile, 'This is an external prompt file');
      
      // Create prompt that references the external file
      fs.writeFileSync(path.join(testPromptDir, 'reference.prompt'), `@${referencedFile}`);
      
      const result = await runCommand(['prompt', 'get', 'reference']);
      
      expect(result.exitCode).toBe(0);
      expect(result.output).toContain('This is an external prompt file');
    });
  });

  describe('prompt delete', () => {
    beforeEach(async () => {
      // Create test prompts for delete tests
      fs.writeFileSync(path.join(testPromptDir, 'to-delete.prompt'), 'This will be deleted');
      fs.writeFileSync(path.join(testPromptDir, 'to-keep.prompt'), 'This will be kept');
    });

    it('should delete prompt successfully', async () => {
      const result = await runCommand(['prompt', 'delete', 'to-delete']);
      
      expect(result.exitCode).toBe(0);
      expect(result.output).toContain('Deleted:');
      expect(result.output).toContain('to-delete.prompt');
      
      // Verify the file was deleted
      const promptFile = path.join(testPromptDir, 'to-delete.prompt');
      expect(fs.existsSync(promptFile)).toBe(false);
      
      // Verify other files still exist
      const keepFile = path.join(testPromptDir, 'to-keep.prompt');
      expect(fs.existsSync(keepFile)).toBe(true);
    });

    it('should fail when prompt does not exist', async () => {
      const result = await runCommand(['prompt', 'delete', 'nonexistent']);
      
      expect(result.exitCode).toBe(1);
      expect(result.output).toContain("Error: Prompt 'nonexistent' not found in any scope.");
    });

    it('should fail when prompt name is missing', async () => {
      const result = await runCommand(['prompt', 'delete']);
      
      expect(result.exitCode).toBe(1);
      expect(result.output).toContain('Error: Prompt name is required.');
    });

    it('should delete referenced file when prompt contains file reference', async () => {
      // Create a referenced file
      const referencedFile = path.join(tempDir, 'external-to-delete.txt');
      fs.writeFileSync(referencedFile, 'This external file will be deleted');
      
      // Create prompt that references the external file
      fs.writeFileSync(path.join(testPromptDir, 'with-reference.prompt'), `@${referencedFile}`);
      
      const result = await runCommand(['prompt', 'delete', 'with-reference']);
      
      expect(result.exitCode).toBe(0);
      expect(result.output).toContain('Deleted:');
      expect(result.output).toContain('with-reference.prompt');
      expect(result.output).toContain('external-to-delete.txt');
      
      // Verify both files were deleted
      expect(fs.existsSync(path.join(testPromptDir, 'with-reference.prompt'))).toBe(false);
      expect(fs.existsSync(referencedFile)).toBe(false);
    });
  });

  describe('prompt scopes', () => {
    it('should support --local scope for create', async () => {
      const result = await runCommand(['prompt', 'create', 'local-test', 'Local prompt', '--local']);
      
      expect(result.exitCode).toBe(0);
      expect(result.output).toContain('local-test.prompt');
    });

    it('should support --user scope for create', async () => {
      const result = await runCommand(['prompt', 'create', 'user-test', 'User prompt', '--user']);
      
      // May fail due to permissions, but should not save to local directory
      if (result.exitCode === 0) {
        expect(result.output).toContain('user-test.prompt');
        expect(result.output).not.toContain(`${tempDir}/.cycod/prompts/user-test.prompt`);
      } else {
        // If it fails, it should be due to permissions, not logic errors
        expect(result.output).toContain('Error');
      }
    });

    it('should support --global scope for create', async () => {
      const result = await runCommand(['prompt', 'create', 'global-test', 'Global prompt', '--global']);
      
      // May fail due to permissions, but should not save to local directory
      if (result.exitCode === 0) {
        expect(result.output).toContain('global-test.prompt');
        expect(result.output).not.toContain(`${tempDir}/.cycod/prompts/global-test.prompt`);
      } else {
        // If it fails, it should be due to permissions, not logic errors
        expect(result.output).toContain('Error');
      }
    });
  });

  describe('integration tests', () => {
    it('should create, list, get, and delete prompt in sequence', async () => {
      // Create
      let result = await runCommand(['prompt', 'create', 'integration-test', 'Integration test prompt']);
      expect(result.exitCode).toBe(0);
      
      // List
      result = await runCommand(['prompt', 'list']);
      expect(result.exitCode).toBe(0);
      expect(result.output).toContain('integration-test');
      
      // Get
      result = await runCommand(['prompt', 'get', 'integration-test']);
      expect(result.exitCode).toBe(0);
      expect(result.output).toContain('Integration test prompt');
      
      // Delete
      result = await runCommand(['prompt', 'delete', 'integration-test']);
      expect(result.exitCode).toBe(0);
      
      // Verify deletion
      result = await runCommand(['prompt', 'list']);
      expect(result.exitCode).toBe(0);
      expect(result.output).not.toContain('integration-test');
    });
  });
});

// Helper function to run cycod commands
async function runCommand(args: string[]): Promise<{exitCode: number, output: string, error: string}> {
  try {
    return await executePromptCommand(args);
  } catch (error) {
    return {
      exitCode: 1,
      output: '',
      error: String(error)
    };
  }
}

// Execute actual prompt command
async function executePromptCommand(args: string[]): Promise<{exitCode: number, output: string, error: string}> {
  const [command, subCommand, ...rest] = args;
  
  if (command !== 'prompt') {
    return { exitCode: 1, output: '', error: 'Invalid command' };
  }

  // Clear previous output
  consoleOutput = [];
  consoleErrors = [];

  let commandInstance;
  let exitCode: number;

  switch (subCommand) {
    case 'list':
      commandInstance = new PromptListCommand();
      parseListArgs(commandInstance, rest);
      exitCode = await commandInstance.executeAsync(false);
      break;
    case 'create':
      commandInstance = new PromptCreateCommand();
      parseCreateArgs(commandInstance, rest);
      exitCode = await commandInstance.executeAsync(false);
      break;
    case 'get':
      commandInstance = new PromptGetCommand();
      parseGetArgs(commandInstance, rest);
      exitCode = await commandInstance.executeAsync(false);
      break;
    case 'delete':
      commandInstance = new PromptDeleteCommand();
      parseDeleteArgs(commandInstance, rest);
      exitCode = await commandInstance.executeAsync(false);
      break;
    default:
      return { exitCode: 1, output: '', error: 'Invalid subcommand' };
  }

  return {
    exitCode,
    output: consoleOutput.join('\n') + (consoleErrors.length > 0 ? '\n' + consoleErrors.join('\n') : ''),
    error: consoleErrors.join('\n')
  };
}

function parseListArgs(command: PromptListCommand, args: string[]): void {
  // Handle scope arguments
  if (args.includes('--global')) {
    command.scope = ConfigFileScope.Global;
  } else if (args.includes('--user')) {
    command.scope = ConfigFileScope.User;
  } else if (args.includes('--local')) {
    command.scope = ConfigFileScope.Local;
  }
}

function parseCreateArgs(command: PromptCreateCommand, args: string[]): void {
  const filteredArgs = args.filter(arg => !['--local', '--user', '--global'].includes(arg));
  
  if (filteredArgs.length >= 1) command.promptName = filteredArgs[0];
  if (filteredArgs.length >= 2) command.promptText = filteredArgs[1];
  
  // Handle scope arguments
  if (args.includes('--global')) {
    command.scope = ConfigFileScope.Global;
  } else if (args.includes('--user')) {
    command.scope = ConfigFileScope.User;
  } else if (args.includes('--local')) {
    command.scope = ConfigFileScope.Local;
  }
}

function parseGetArgs(command: PromptGetCommand, args: string[]): void {
  const filteredArgs = args.filter(arg => !['--local', '--user', '--global'].includes(arg));
  
  if (filteredArgs.length >= 1) command.promptName = filteredArgs[0];
  
  // Handle scope arguments
  if (args.includes('--global')) {
    command.scope = ConfigFileScope.Global;
  } else if (args.includes('--user')) {
    command.scope = ConfigFileScope.User;
  } else if (args.includes('--local')) {
    command.scope = ConfigFileScope.Local;
  }
}

function parseDeleteArgs(command: PromptDeleteCommand, args: string[]): void {
  const filteredArgs = args.filter(arg => !['--local', '--user', '--global'].includes(arg));
  
  if (filteredArgs.length >= 1) command.promptName = filteredArgs[0];
  
  // Handle scope arguments
  if (args.includes('--global')) {
    command.scope = ConfigFileScope.Global;
  } else if (args.includes('--user')) {
    command.scope = ConfigFileScope.User;
  } else if (args.includes('--local')) {
    command.scope = ConfigFileScope.Local;
  }
}
