import { CliTestHelper } from '../helpers/CliTestHelper';
import * as fs from 'fs-extra';
import * as path from 'path';
import * as os from 'os';

describe('Prompt Commands', () => {
  const localPromptsDir = path.join(process.cwd(), '.cycod', 'prompts');
  const userPromptsDir = path.join(os.homedir(), '.cycod', 'prompts');
  const globalPromptsDir = path.join('/tmp', 'cycod-global', 'prompts');
  
  const testPromptPath = path.join(localPromptsDir, 'test-prompt.prompt');
  const testPromptPath2 = path.join(localPromptsDir, 'test-prompt2.prompt');

  beforeAll(async () => {
    // Clean up any existing test prompts
    await cleanup();
  });

  afterAll(async () => {
    // Clean up test prompts after all tests
    await cleanup();
  });

  async function cleanup() {
    try {
      if (await fs.pathExists(testPromptPath)) {
        await fs.remove(testPromptPath);
      }
      if (await fs.pathExists(testPromptPath2)) {
        await fs.remove(testPromptPath2);
      }
    } catch (error) {
      // Ignore cleanup errors
    }
  }

  describe('Prompt Create', () => {
    it('should create prompt with text', async () => {
      const result = await CliTestHelper.run('cycodjs prompt create test-prompt "You are a helpful assistant."');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toBe('/test-prompt (in chat)');

      // Verify file was created with .prompt extension
      expect(await fs.pathExists(testPromptPath)).toBe(true);
      const content = await fs.readFile(testPromptPath, 'utf8');
      expect(content.trim()).toBe('You are a helpful assistant.');
    });

    it('should create prompt from file', async () => {
      // Create a temporary file with prompt content
      const tempFile = path.join(os.tmpdir(), 'temp-prompt.txt');
      await fs.writeFile(tempFile, 'You are a coding assistant.');

      const result = await CliTestHelper.run(`cycodjs prompt create test-prompt2 @${tempFile}`);
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toBe('/test-prompt2 (in chat)');

      // Verify file was created with correct content
      expect(await fs.pathExists(testPromptPath2)).toBe(true);
      const content = await fs.readFile(testPromptPath2, 'utf8');
      expect(content.trim()).toBe('You are a coding assistant.');

      // Clean up temp file
      await fs.remove(tempFile);
    });

    it('should handle creating duplicate prompt', async () => {
      // First create a prompt
      await CliTestHelper.run('cycodjs prompt create test-prompt "Original prompt"');
      
      // Try to create same prompt again
      const result = await CliTestHelper.run('cycodjs prompt create test-prompt "New prompt"');
      expect(result.exitCode).toBe(1);
      expect(result.stderr).toMatch(/Error: Prompt 'test-prompt' already exists/);
    });
  });

  describe('Prompt List', () => {
    it('should list all prompts with location headers', async () => {
      // Ensure we have at least one prompt
      await CliTestHelper.run('cycodjs prompt create test-prompt "Test prompt content"');

      const result = await CliTestHelper.run('cycodjs prompt list');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatch(/LOCATION: .*\/tmp\/cycod-global\/prompts \(global\)/);
      expect(result.stdout).toMatch(/LOCATION: .*\.cycod\/prompts \(user\)/);
      expect(result.stdout).toMatch(/LOCATION: .*\.cycod\/prompts \(local\)/);
      expect(result.stdout).toMatch(/  \/test-prompt/);
    });

    it('should show empty scopes when no prompts exist', async () => {
      // Clean up all prompts
      await cleanup();

      const result = await CliTestHelper.run('cycodjs prompt list');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatch(/LOCATION: .*\/tmp\/cycod-global\/prompts \(global\)/);
      expect(result.stdout).toMatch(/LOCATION: .*\.cycod\/prompts \(user\)/);
      expect(result.stdout).toMatch(/LOCATION: .*\.cycod\/prompts \(local\)/);
    });
  });

  describe('Prompt Show', () => {
    it('should show existing prompt', async () => {
      // Create a prompt first
      await CliTestHelper.run('cycodjs prompt create test-prompt "You are a helpful assistant for testing."');

      const result = await CliTestHelper.run('cycodjs prompt show test-prompt');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatch(/LOCATION: .*\.cycod\/prompts\/test-prompt\.prompt \(local\)/);
      expect(result.stdout).toMatch(/test-prompt:/);
      expect(result.stdout).toMatch(/You are a helpful assistant for testing/);
    });

    it('should handle non-existent prompt', async () => {
      const result = await CliTestHelper.run('cycodjs prompt show non-existent');
      expect(result.exitCode).toBe(1);
      expect(result.stderr).toMatch(/Error showing prompt: Prompt 'non-existent' not found/);
    });
  });

  describe('Prompt Delete', () => {
    it('should delete existing prompt', async () => {
      // Create a prompt first
      await CliTestHelper.run('cycodjs prompt create test-prompt "Prompt to be deleted"');
      expect(await fs.pathExists(testPromptPath)).toBe(true);

      const result = await CliTestHelper.run('cycodjs prompt delete test-prompt');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatch(/\/test-prompt \(deleted\)/);

      // Verify file was deleted
      expect(await fs.pathExists(testPromptPath)).toBe(false);
    });

    it('should handle deleting non-existent prompt', async () => {
      const result = await CliTestHelper.run('cycodjs prompt delete non-existent');
      expect(result.exitCode).toBe(1);
      expect(result.stderr).toMatch(/Error deleting prompt: Prompt 'non-existent' not found/);
    });
  });

  describe('Prompt Validation', () => {
    it('should handle invalid prompt name', async () => {
      const result = await CliTestHelper.run('cycodjs prompt create "invalid/name" "Test content"');
      expect(result.exitCode).toBe(1);
      expect(result.stderr).toMatch(/Invalid prompt name: "invalid\/name". Prompt names cannot contain path separators or special characters/);
    });

    it('should handle missing file for @file syntax', async () => {
      const result = await CliTestHelper.run('cycodjs prompt create test-prompt @/non/existent/file.txt');
      expect(result.exitCode).toBe(1);
      expect(result.stderr).toMatch(/File not found/);
    });
  });
});