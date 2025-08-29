import { ConfigGetCommand } from '../../../CommandLineCommands/ConfigCommands/ConfigGetCommand';
import { ConfigFileScope } from '../../../Configuration/ConfigFileScope';
import { mockConsole } from '../../../test-setup';
import { mockConfigStore } from '../../__mocks__/Configuration/ConfigStore';

// Mock the ConfigStore
jest.mock('../../../Configuration/ConfigStore');

describe('ConfigGetCommand', () => {
  let command: ConfigGetCommand;
  let consoleMock: ReturnType<typeof mockConsole>;

  beforeEach(() => {
    consoleMock = mockConsole();
    command = new ConfigGetCommand();
    jest.clearAllMocks();
  });

  afterEach(() => {
    consoleMock.restore();
  });

  describe('Basic Functionality', () => {
    test('should get command name', () => {
      expect(command.getCommandName()).toBe('config get');
    });

    test('should be empty when key is not provided', () => {
      expect(command.isEmpty()).toBe(true);
      
      command.key = 'test.key';
      expect(command.isEmpty()).toBe(false);
      
      command.key = '';
      expect(command.isEmpty()).toBe(true);
    });
  });

  describe('Execute Command', () => {
    test('should get configuration value from any scope by default', async () => {
      const mockValue = {
        value: 'test-value',
        scope: ConfigFileScope.User,
        source: 'User',
        isSecret: false,
      };
      
      mockConfigStore.getFromScope.mockResolvedValue(mockValue);
      
      command.key = 'test.key';
      const exitCode = await command.executeAsync(false);
      
      expect(mockConfigStore.getFromScope).toHaveBeenCalledWith(
        'test.key',
        ConfigFileScope.Any
      );
      expect(exitCode).toBe(0);
      expect(consoleMock.logs).toContain('test.key=test-value (User)');
    });

    test('should get configuration value from specific scope', async () => {
      const mockValue = {
        value: 'local-value',
        scope: ConfigFileScope.Local,
        source: 'Local',
        isSecret: false,
      };
      
      mockConfigStore.getFromScope.mockResolvedValue(mockValue);
      
      command.key = 'test.key';
      command.scope = ConfigFileScope.Local;
      const exitCode = await command.executeAsync(false);
      
      expect(mockConfigStore.getFromScope).toHaveBeenCalledWith(
        'test.key',
        ConfigFileScope.Local
      );
      expect(exitCode).toBe(0);
      expect(consoleMock.logs).toContain('test.key=local-value (Local)');
    });

    test('should get configuration value from specific file', async () => {
      const mockValue = {
        value: 'file-value',
        scope: ConfigFileScope.FileName,
        source: '/path/to/config.yaml',
        isSecret: false,
      };
      
      mockConfigStore.getFromFileName.mockResolvedValue(mockValue);
      
      command.key = 'test.key';
      command.scope = ConfigFileScope.FileName;
      command.configFileName = '/path/to/config.yaml';
      const exitCode = await command.executeAsync(false);
      
      expect(mockConfigStore.getFromFileName).toHaveBeenCalledWith(
        'test.key',
        '/path/to/config.yaml'
      );
      expect(exitCode).toBe(0);
      expect(consoleMock.logs).toContain('test.key=file-value (/path/to/config.yaml)');
    });

    test('should handle missing configuration value', async () => {
      mockConfigStore.getFromScope.mockResolvedValue(undefined);
      
      command.key = 'missing.key';
      const exitCode = await command.executeAsync(false);
      
      expect(exitCode).toBe(0);
      expect(consoleMock.logs).toContain('missing.key= (not set)');
    });

    test('should handle secret values', async () => {
      const mockValue = {
        value: 'secret-token',
        scope: ConfigFileScope.User,
        source: 'User',
        isSecret: true,
      };
      
      mockConfigStore.getFromScope.mockResolvedValue(mockValue);
      
      command.key = 'openai.token';
      const exitCode = await command.executeAsync(false);
      
      expect(exitCode).toBe(0);
      expect(consoleMock.logs).toContain('openai.token=*** (User)');
    });

    test('should handle list values', async () => {
      const mockValue = {
        value: ['item1', 'item2', 'item3'],
        scope: ConfigFileScope.Local,
        source: 'Local',
        isSecret: false,
      };
      
      mockConfigStore.getFromScope.mockResolvedValue(mockValue);
      
      command.key = 'test.list';
      const exitCode = await command.executeAsync(false);
      
      expect(exitCode).toBe(0);
      expect(consoleMock.logs).toContain('test.list=[item1, item2, item3] (Local)');
    });

    test('should throw error when key is empty', async () => {
      command.key = '';
      
      await expect(command.executeAsync(false)).rejects.toThrow('Error: No key specified.');
    });

    test('should normalize known setting keys', async () => {
      const mockValue = {
        value: 'openai',
        scope: ConfigFileScope.User,
        source: 'User',
        isSecret: false,
      };
      
      mockConfigStore.getFromScope.mockResolvedValue(mockValue);
      
      // Use an alias that should be normalized
      command.key = 'ai_provider';
      await command.executeAsync(false);
      
      // Should be called with the canonical form
      expect(mockConfigStore.getFromScope).toHaveBeenCalledWith(
        'provider', // canonical form of ai_provider
        ConfigFileScope.Any
      );
    });
  });
});