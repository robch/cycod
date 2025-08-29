import { ConfigStore } from '../../Configuration/ConfigStore';
import { ConfigFileScope } from '../../Configuration/ConfigFileScope';
import { ConfigValueHelpers } from '../../Configuration/ConfigValue';
import { createTempDir, cleanupTempDir } from '../../test-setup';
import * as fs from 'fs-extra';
import * as path from 'path';

describe('ConfigStore', () => {
  let tempDir: string;
  let configStore: ConfigStore;
  let originalCwd: string;

  beforeEach(async () => {
    tempDir = await createTempDir();
    originalCwd = process.cwd();
    process.chdir(tempDir);
    
    // Create a new instance for each test
    // Note: In a real implementation, we'd need to reset the singleton
    configStore = ConfigStore.instance;
  });

  afterEach(async () => {
    process.chdir(originalCwd);
    await cleanupTempDir(tempDir);
  });

  describe('Configuration Scopes', () => {
    test('should handle local configuration', async () => {
      // Create a local config directory and file
      const localConfigDir = path.join(tempDir, '.cycod');
      const localConfigFile = path.join(localConfigDir, 'config.yaml');
      
      await fs.ensureDir(localConfigDir);
      await fs.writeFile(localConfigFile, 'provider: openai\n');

      // Get the value
      const value = await configStore.getFromScope('provider', ConfigFileScope.Local);
      
      expect(ConfigValueHelpers.hasValue(value)).toBe(true);
      expect(value?.value).toBe('openai');
      expect(value?.scope).toBe(ConfigFileScope.Local);
    });

    test('should set configuration in local scope', async () => {
      await configStore.set('test.key', 'test-value', ConfigFileScope.Local);

      const value = await configStore.getFromScope('test.key', ConfigFileScope.Local);
      
      expect(ConfigValueHelpers.hasValue(value)).toBe(true);
      expect(value?.value).toBe('test-value');
    });

    test('should handle list values', async () => {
      const listValue = ['item1', 'item2', 'item3'];
      await configStore.set('test.list', listValue, ConfigFileScope.Local);

      const value = await configStore.getFromScope('test.list', ConfigFileScope.Local);
      
      expect(ConfigValueHelpers.hasValue(value)).toBe(true);
      expect(Array.isArray(value?.value)).toBe(true);
      expect(value?.value).toEqual(listValue);
    });
  });

  describe('List Operations', () => {
    test('should add to list', async () => {
      // First, set an initial list
      await configStore.set('test.list', ['item1'], ConfigFileScope.Local);

      // Add to the list
      await configStore.addToList('test.list', 'item2', ConfigFileScope.Local);

      const value = await configStore.getFromScope('test.list', ConfigFileScope.Local);
      expect(value?.value).toEqual(['item1', 'item2']);
    });

    test('should remove from list', async () => {
      // Set an initial list
      await configStore.set('test.list', ['item1', 'item2', 'item3'], ConfigFileScope.Local);

      // Remove from the list
      await configStore.removeFromList('test.list', 'item2', ConfigFileScope.Local);

      const value = await configStore.getFromScope('test.list', ConfigFileScope.Local);
      expect(value?.value).toEqual(['item1', 'item3']);
    });

    test('should clear setting when removing last item from list', async () => {
      // Set a list with one item
      await configStore.set('test.list', ['item1'], ConfigFileScope.Local);

      // Remove the only item
      await configStore.removeFromList('test.list', 'item1', ConfigFileScope.Local);

      const value = await configStore.getFromScope('test.list', ConfigFileScope.Local);
      expect(ConfigValueHelpers.hasValue(value)).toBe(false);
    });
  });

  describe('Configuration Priority', () => {
    test('should respect configuration priority (command line > file > local)', async () => {
      // Set in local scope
      await configStore.set('test.priority', 'local-value', ConfigFileScope.Local);

      // Set command line value (highest priority)
      configStore.setCommandLineSetting('test.priority', 'commandline-value');

      const value = await configStore.getFromAnyScope('test.priority');
      expect(value?.value).toBe('commandline-value');
      expect(value?.source).toBe('Command line');
    });
  });

  describe('Clear Configuration', () => {
    test('should clear configuration setting', async () => {
      // Set a value
      await configStore.set('test.clear', 'value-to-clear', ConfigFileScope.Local);
      
      // Verify it exists
      let value = await configStore.getFromScope('test.clear', ConfigFileScope.Local);
      expect(ConfigValueHelpers.hasValue(value)).toBe(true);

      // Clear it
      await configStore.clear('test.clear', ConfigFileScope.Local);

      // Verify it's gone
      value = await configStore.getFromScope('test.clear', ConfigFileScope.Local);
      expect(ConfigValueHelpers.hasValue(value)).toBe(false);
    });
  });

  describe('List Values', () => {
    test('should list all values from scope', async () => {
      // Set multiple values
      await configStore.set('key1', 'value1', ConfigFileScope.Local);
      await configStore.set('key2', ['item1', 'item2'], ConfigFileScope.Local);
      await configStore.set('nested.key', 'nested-value', ConfigFileScope.Local);

      const values = await configStore.listValuesFromScope(ConfigFileScope.Local);

      expect(Object.keys(values)).toContain('key1');
      expect(Object.keys(values)).toContain('key2');
      expect(Object.keys(values)).toContain('nested.key');
      
      expect(values['key1'].value).toBe('value1');
      expect(values['key2'].value).toEqual(['item1', 'item2']);
      expect(values['nested.key'].value).toBe('nested-value');
    });
  });
});