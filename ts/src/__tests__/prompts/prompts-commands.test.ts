import { CliTestHelper } from '../helpers/CliTestHelper';
import * as fs from 'fs-extra';
import * as path from 'path';
import * as os from 'os';

describe('Prompt Commands', () => {
  const promptsDir = path.join(os.homedir(), '.cycod', 'prompts');
  const testPromptPath = path.join(promptsDir, 'test-prompt.txt');
  const testPromptPath2 = path.join(promptsDir, 'test-prompt2.txt');

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
      expect(result.stdout).toMatch(/Created prompt.*test-prompt/);

      // Verify file was created
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
      expect(result.stdout).toMatch(/Created prompt.*test-prompt2/);

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
      expect(result.stderr).toMatch(/already exists/);
    });
  });

  describe('Prompt List', () => {
    it('should list all prompts', async () => {
      // Ensure we have at least one prompt
      await CliTestHelper.run('cycodjs prompt create test-prompt "Test prompt content"');

      const result = await CliTestHelper.run('cycodjs prompt list');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatch(/test-prompt/);
    });

    it('should show empty list when no prompts exist', async () => {
      // Clean up all prompts
      await cleanup();

      const result = await CliTestHelper.run('cycodjs prompt list');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatch(/No prompts found/);
    });
  });

  describe('Prompt Show', () => {
    it('should show existing prompt', async () => {
      // Create a prompt first
      await CliTestHelper.run('cycodjs prompt create test-prompt "You are a helpful assistant for testing."');

      const result = await CliTestHelper.run('cycodjs prompt show test-prompt');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatch(/test-prompt:/);
      expect(result.stdout).toMatch(/You are a helpful assistant for testing/);
    });

    it('should handle non-existent prompt', async () => {
      const result = await CliTestHelper.run('cycodjs prompt show non-existent');
      expect(result.exitCode).toBe(1);
      expect(result.stderr).toMatch(/Prompt.*non-existent.*not found/);
    });
  });

  describe('Prompt Delete', () => {
    it('should delete existing prompt', async () => {
      // Create a prompt first
      await CliTestHelper.run('cycodjs prompt create test-prompt "Prompt to be deleted"');
      expect(await fs.pathExists(testPromptPath)).toBe(true);

      const result = await CliTestHelper.run('cycodjs prompt delete test-prompt');
      expect(result.exitCode).toBe(0);
      expect(result.stdout).toMatch(/Deleted prompt.*test-prompt/);

      // Verify file was deleted
      expect(await fs.pathExists(testPromptPath)).toBe(false);
    });

    it('should handle deleting non-existent prompt', async () => {
      const result = await CliTestHelper.run('cycodjs prompt delete non-existent');
      expect(result.exitCode).toBe(1);
      expect(result.stderr).toMatch(/Prompt.*non-existent.*not found/);
    });
  });

  describe('Prompt Validation', () => {
    it('should handle invalid prompt name', async () => {
      const result = await CliTestHelper.run('cycodjs prompt create "invalid/name" "Test content"');
      expect(result.exitCode).toBe(1);
      expect(result.stderr).toMatch(/Invalid prompt name/);
    });

    it('should handle missing file for @file syntax', async () => {
      const result = await CliTestHelper.run('cycodjs prompt create test-prompt @/non/existent/file.txt');
      expect(result.exitCode).toBe(1);
      expect(result.stderr).toMatch(/File not found/);
    });
  });
});