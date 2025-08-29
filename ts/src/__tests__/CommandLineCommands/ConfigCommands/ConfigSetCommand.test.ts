import { ConfigSetCommand } from '../../../CommandLineCommands/ConfigCommands/ConfigSetCommand';
import { ConfigStore } from '../../../Configuration/ConfigStore';
import { ConfigFileScope } from '../../../Configuration/ConfigFileScope';
import { mockConsole } from '../../../test-setup';

// Mock the ConfigStore
jest.mock('../../../Configuration/ConfigStore');

describe('ConfigSetCommand', () => {
  let command: ConfigSetCommand;
  let mockConfigStore: jest.Mocked<ConfigStore>;
  let consoleMock: ReturnType<typeof mockConsole>;

  beforeEach(() => {
    consoleMock = mockConsole();
    command = new ConfigSetCommand();
    mockConfigStore = ConfigStore.instance as jest.Mocked<ConfigStore>;
  });

  afterEach(() => {
    consoleMock.restore();
    jest.clearAllMocks();
  });

  describe('Basic Functionality', () => {
    test('should get command name', () => {
      expect(command.getCommandName()).toBe('config set');
    });

    test('should be empty when key or value is not provided', () => {
      expect(command.isEmpty()).toBe(true);
      
      command.key = 'test.key';
      expect(command.isEmpty()).toBe(true); // Still missing value
      
      command.value = 'test-value';
      expect(command.isEmpty()).toBe(false);
      
      command.key = '';
      expect(command.isEmpty()).toBe(true);
    });
  });

  describe('Execute Command', () => {
    test('should set simple configuration value', async () => {
      const mockValue = {
        value: 'test-value',
        scope: ConfigFileScope.Local,
        source: 'Local',
        isSecret: false,
      };
      
      mockConfigStore.set.mockResolvedValue();
      mockConfigStore.getFromScope.mockResolvedValue(mockValue);
      
      command.key = 'test.key';
      command.value = 'test-value';
      const exitCode = await command.executeAsync(false);
      
      expect(mockConfigStore.set).toHaveBeenCalledWith(
        'test.key',
        'test-value',
        ConfigFileScope.Local,
        true
      );
      expect(exitCode).toBe(0);
      expect(consoleMock.logs).toContain('test.key=test-value (Local)');
    });

    test('should set configuration value in specific scope', async () => {
      const mockValue = {
        value: 'user-value',
        scope: ConfigFileScope.User,
        source: 'User',
        isSecret: false,
      };
      
      mockConfigStore.set.mockResolvedValue();
      mockConfigStore.getFromScope.mockResolvedValue(mockValue);
      
      command.key = 'test.key';
      command.value = 'user-value';
      command.scope = ConfigFileScope.User;
      const exitCode = await command.executeAsync(false);
      
      expect(mockConfigStore.set).toHaveBeenCalledWith(
        'test.key',
        'user-value',
        ConfigFileScope.User,
        true
      );
      expect(exitCode).toBe(0);
      expect(consoleMock.logs).toContain('test.key=user-value (User)');
    });

    test('should set configuration value in specific file', async () => {
      const mockValue = {
        value: 'file-value',
        scope: ConfigFileScope.FileName,
        source: '/path/to/config.yaml',
        isSecret: false,
      };
      
      mockConfigStore.setInFile.mockResolvedValue();
      mockConfigStore.getFromFileName.mockResolvedValue(mockValue);
      
      command.key = 'test.key';
      command.value = 'file-value';
      command.scope = ConfigFileScope.FileName;
      command.configFileName = '/path/to/config.yaml';
      const exitCode = await command.executeAsync(false);
      
      expect(mockConfigStore.setInFile).toHaveBeenCalledWith(
        'test.key',
        'file-value',
        '/path/to/config.yaml'
      );
      expect(exitCode).toBe(0);
      expect(consoleMock.logs).toContain('test.key=file-value (/path/to/config.yaml)');
    });

    test('should handle list values in bracket notation', async () => {
      mockConfigStore.set.mockResolvedValue();
      
      command.key = 'test.list';
      command.value = '[item1, item2, item3]';
      const exitCode = await command.executeAsync(false);
      
      expect(mockConfigStore.set).toHaveBeenCalledWith(
        'test.list',
        ['item1', 'item2', 'item3'],
        ConfigFileScope.Local,
        true
      );
      expect(exitCode).toBe(0);
      expect(consoleMock.logs).toContain('test.list=[item1, item2, item3]');
    });

    test('should handle empty list values', async () => {
      mockConfigStore.set.mockResolvedValue();
      
      command.key = 'test.list';
      command.value = '[]';
      const exitCode = await command.executeAsync(false);
      
      expect(mockConfigStore.set).toHaveBeenCalledWith(
        'test.list',
        [],
        ConfigFileScope.Local,
        true
      );
      expect(exitCode).toBe(0);
      expect(consoleMock.logs).toContain('test.list=[]');
    });

    test('should handle list values with spaces', async () => {
      mockConfigStore.set.mockResolvedValue();
      
      command.key = 'test.list';
      command.value = '[ item1 , item2 , item3 ]';
      const exitCode = await command.executeAsync(false);
      
      expect(mockConfigStore.set).toHaveBeenCalledWith(
        'test.list',
        ['item1', 'item2', 'item3'],
        ConfigFileScope.Local,
        true
      );
      expect(exitCode).toBe(0);
      expect(consoleMock.logs).toContain('test.list=[item1, item2, item3]');
    });

    test('should throw error when key is empty', async () => {
      command.key = '';
      command.value = 'test-value';
      
      await expect(command.executeAsync(false)).rejects.toThrow('Error: No key specified.');
    });

    test('should throw error when value is undefined', async () => {
      command.key = 'test.key';
      command.value = undefined;
      
      await expect(command.executeAsync(false)).rejects.toThrow('Error: No value specified.');
    });

    test('should normalize known setting keys', async () => {
      const mockValue = {
        value: 'openai',
        scope: ConfigFileScope.Local,
        source: 'Local',
        isSecret: false,
      };
      
      mockConfigStore.set.mockResolvedValue();
      mockConfigStore.getFromScope.mockResolvedValue(mockValue);
      
      // Use an alias that should be normalized
      command.key = 'ai_provider';
      command.value = 'openai';
      await command.executeAsync(false);
      
      // Should be called with the canonical form
      expect(mockConfigStore.set).toHaveBeenCalledWith(
        'provider', // canonical form of ai_provider
        'openai',
        ConfigFileScope.Local,
        true
      );
    });

    test('should warn about unknown settings', async () => {
      const mockValue = {
        value: 'unknown-value',
        scope: ConfigFileScope.Local,
        source: 'Local',
        isSecret: false,
      };
      
      mockConfigStore.set.mockResolvedValue();
      mockConfigStore.getFromScope.mockResolvedValue(mockValue);
      
      command.key = 'unknown.setting';
      command.value = 'unknown-value';
      await command.executeAsync(false);
      
      // Should warn about unknown setting
      expect(consoleMock.errors.some(log => log.includes('Warning: Unknown setting'))).toBe(true);
      
      // Should still set the value with original key
      expect(mockConfigStore.set).toHaveBeenCalledWith(
        'unknown.setting',
        'unknown-value',
        ConfigFileScope.Local,
        true
      );
    });
  });
});