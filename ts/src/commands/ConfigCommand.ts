import { Command } from 'commander';
import { ConfigStore } from '../config/ConfigStore';
import { ConfigFileScope } from '../types';
import { ConsoleHelpers } from '../helpers/ConsoleHelpers';

export class ConfigCommand {
  private static parseScopeFromFlags(options: any): ConfigFileScope {
    // Handle Commander.js flag variations (including capitalized short flags)
    if (options.global || options.g || options.G) return ConfigFileScope.Global;
    if (options.user || options.u || options.U) return ConfigFileScope.User;
    if (options.local || options.l || options.L) return ConfigFileScope.Local;
    if (options.any || options.a || options.A) return ConfigFileScope.Any;
    
    // Default to local scope if no flags specified
    return ConfigFileScope.Local;
  }

  static createCommand(): Command {
    const configCmd = new Command('config');
    const configStore = new ConfigStore();
    
    configCmd
      .command('list')
      .description('Show all configuration settings')
      .option('--local', 'Show local configuration only')
      .option('--user', 'Show user configuration only')
      .option('--global', 'Show global configuration only')
      .action(async (options) => {
        try {
          const scope = ConfigCommand.parseScopeFromFlags(options);
          
          if (scope === ConfigFileScope.Any || (!options.local && !options.user && !options.global)) {
            // Show all scopes
            const allConfigs = await configStore.getAllConfigs();
            for (const config of allConfigs) {
              ConsoleHelpers.displayLocationHeader(config.location, config.scope);
              ConsoleHelpers.displayYamlContent(config.data);
            }
          } else {
            // Show specific scope
            const allConfigs = await configStore.getAllConfigs();
            const config = allConfigs.find(c => c.scope === scope);
            if (config) {
              ConsoleHelpers.displayLocationHeader(config.location, config.scope);
              ConsoleHelpers.displayYamlContent(config.data);
            }
          }
        } catch (error) {
          console.error('Error listing configuration:', error);
          process.exit(1);
        }
      });

    configCmd
      .command('get <key>')
      .description('Get a configuration value')
      .option('--local', 'Get from local configuration')
      .option('--user', 'Get from user configuration')  
      .option('--global', 'Get from global configuration')
      .option('--any', 'Get from any scope (follows precedence)')
      .action(async (key: string, options) => {
        try {
          const scope = ConfigCommand.parseScopeFromFlags(options);
          const result = await configStore.getFromScope(key, scope);
          
          if (result) {
            ConsoleHelpers.displayLocationHeader(result.location, result.scope);
            ConsoleHelpers.displayConfigValue(key, result.value, true);
          } else {
            ConsoleHelpers.displayNotFoundMessage(key);
          }
        } catch (error) {
          console.error('Error getting configuration:', error);
          process.exit(1);
        }
      });

    configCmd
      .command('set <key> <value>')
      .description('Set a configuration value')
      .option('--local', 'Set in local configuration')
      .option('--user', 'Set in user configuration')
      .option('--global', 'Set in global configuration')
      .action(async (key: string, value: string, options) => {
        try {
          const scope = ConfigCommand.parseScopeFromFlags(options);
          await configStore.setInScope(key, value, scope);
          
          // Show the result
          const result = await configStore.getFromScope(key, scope);
          if (result) {
            ConsoleHelpers.displayLocationHeader(result.location, result.scope);
            ConsoleHelpers.displayConfigValue(key, result.value, true);
          }
        } catch (error) {
          console.error('Error setting configuration:', error);
          process.exit(1);
        }
      });

    configCmd
      .command('clear <key>')
      .description('Clear a configuration value')
      .option('--local', 'Clear from local configuration')
      .option('--user', 'Clear from user configuration')
      .option('--global', 'Clear from global configuration')
      .action(async (key: string, options) => {
        try {
          const scope = ConfigCommand.parseScopeFromFlags(options);
          const wasCleared = await configStore.clearFromScope(key, scope);
          
          if (wasCleared) {
            ConsoleHelpers.displayClearedMessage(key);
          } else {
            ConsoleHelpers.displayNotFoundMessage(key);
          }
        } catch (error) {
          console.error('Error clearing configuration:', error);
          process.exit(1);
        }
      });

    configCmd
      .command('add <key> <value>')
      .description('Add value to a list configuration')
      .option('--local', 'Add to local configuration')
      .option('--user', 'Add to user configuration')
      .option('--global', 'Add to global configuration')
      .action(async (key: string, value: string, options) => {
        try {
          const scope = ConfigCommand.parseScopeFromFlags(options);
          await configStore.addToList(key, value, scope);
          
          // Show the result
          const result = await configStore.getFromScope(key, scope);
          if (result) {
            ConsoleHelpers.displayConfigValue(key, result.value, false);
          }
        } catch (error) {
          console.error('Error adding to configuration list:', error);
          process.exit(1);
        }
      });

    configCmd
      .command('remove <key> <value>')
      .description('Remove value from a list configuration')
      .option('--local', 'Remove from local configuration')
      .option('--user', 'Remove from user configuration') 
      .option('--global', 'Remove from global configuration')
      .action(async (key: string, value: string, options) => {
        try {
          const scope = ConfigCommand.parseScopeFromFlags(options);
          await configStore.removeFromList(key, value, scope);
          
          // Show the result
          const result = await configStore.getFromScope(key, scope);
          if (result) {
            ConsoleHelpers.displayConfigValue(key, result.value, false);
          } else {
            // List was cleared completely
            ConsoleHelpers.displayClearedMessage(key);
          }
        } catch (error) {
          console.error('Error removing from configuration list:', error);
          process.exit(1);
        }
      });

    return configCmd;
  }
}