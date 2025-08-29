import { YamlConfigFile } from '../../Configuration/ConfigFile';
import { ConfigFileScope } from '../../Configuration/ConfigFileScope';
import { createTempDir, cleanupTempDir } from '../../test-setup';
import * as path from 'path';

describe('YamlConfigFile', () => {
  let tempDir: string;
  let configFile: YamlConfigFile;
  let testFilePath: string;

  beforeEach(async () => {
    tempDir = await createTempDir();
    testFilePath = path.join(tempDir, 'test-config.yaml');
    configFile = new YamlConfigFile(testFilePath, ConfigFileScope.Local);
  });

  afterEach(async () => {
    await cleanupTempDir(tempDir);
  });

  test('should create yaml config file instance', () => {
    expect(configFile.fileName).toBe(testFilePath);
    expect(configFile.scope).toBe(ConfigFileScope.Local);
  });

  test('should set and get simple values', () => {
    configFile.set('test.key', 'test-value');
    expect(configFile.get('test.key')).toBe('test-value');
  });

  test('should set and get nested values', () => {
    configFile.set('level1.level2.key', 'nested-value');
    expect(configFile.get('level1.level2.key')).toBe('nested-value');
  });

  test('should set and get list values', () => {
    const listValue = ['item1', 'item2', 'item3'];
    configFile.set('test.list', listValue);
    expect(configFile.get('test.list')).toEqual(listValue);
  });

  test('should save and load from file', async () => {
    configFile.set('saved.key', 'saved-value');
    configFile.set('saved.list', ['a', 'b', 'c']);
    
    await configFile.save();
    
    // Create new instance and load
    const newConfigFile = new YamlConfigFile(testFilePath, ConfigFileScope.Local);
    await newConfigFile.load();
    
    expect(newConfigFile.get('saved.key')).toBe('saved-value');
    expect(newConfigFile.get('saved.list')).toEqual(['a', 'b', 'c']);
  });

  test('should clear values', () => {
    configFile.set('to.clear', 'value');
    expect(configFile.get('to.clear')).toBe('value');
    
    configFile.clear('to.clear');
    expect(configFile.get('to.clear')).toBeUndefined();
  });

  test('should return all values', () => {
    configFile.set('key1', 'value1');
    configFile.set('key2', 'value2');
    configFile.set('nested.key', 'nested-value');
    
    const allValues = configFile.getAll();
    expect(allValues.key1).toBe('value1');
    expect(allValues.key2).toBe('value2');
    expect(allValues.nested.key).toBe('nested-value');
  });

  test('should check if file exists', async () => {
    expect(await configFile.exists()).toBe(false);
    
    await configFile.save();
    expect(await configFile.exists()).toBe(true);
  });
});