#!/usr/bin/env node

import { Command } from 'commander';
import { ConfigListCommand } from '../CommandLineCommands/ConfigCommands/ConfigListCommand';
import { ConfigGetCommand } from '../CommandLineCommands/ConfigCommands/ConfigGetCommand';
import { ConfigSetCommand } from '../CommandLineCommands/ConfigCommands/ConfigSetCommand';
import { ConfigClearCommand } from '../CommandLineCommands/ConfigCommands/ConfigClearCommand';
import { ConfigAddCommand } from '../CommandLineCommands/ConfigCommands/ConfigAddCommand';
import { ConfigRemoveCommand } from '../CommandLineCommands/ConfigCommands/ConfigRemoveCommand';
import { ConfigFileScope } from '../Configuration/ConfigFileScope';
import { ConsoleHelpers } from '../Helpers/ConsoleHelpers';

const program = new Command();

program
  .name('cycod')
  .description('CYCODEV CLI - AI-powered command-line interface')
  .version('1.0.0');

// Global options
program
  .option('--quiet', 'suppress verbose output')
  .option('--debug', 'enable debug output')
  .hook('preAction', (thisCommand) => {
    const opts = thisCommand.opts();
    if (opts.quiet) ConsoleHelpers.setQuiet(true);
    if (opts.debug) ConsoleHelpers.setDebug(true);
  });

// Config command group
const configCommand = program
  .command('config')
  .description('manage configuration settings');

// Config list command
configCommand
  .command('list')
  .description('list configuration settings')
  .option('--global, -g', 'use global scope (all users)')
  .option('--user, -u', 'use user scope (current user)')
  .option('--local, -l', 'use local scope')
  .option('--any, -a', 'list from any scope (default)')
  .option('--config-file <file>', 'specific configuration file')
  .action(async (options) => {
    try {
      const command = new ConfigListCommand();
      const scope = parseScopeFromFlags(options);
      command.scope = scope;
      command.configFileName = options.configFile;
      
      const exitCode = await command.executeAsync(false);
      process.exit(exitCode);
    } catch (error) {
      ConsoleHelpers.writeErrorLine(`Error: ${error instanceof Error ? error.message : error}`);
      process.exit(1);
    }
  });

// Config get command
configCommand
  .command('get <key>')
  .description('get a configuration setting')
  .option('--global, -g', 'use global scope (all users)')
  .option('--user, -u', 'use user scope (current user)')
  .option('--local, -l', 'use local scope (default)')
  .option('--any, -a', 'get from any scope (searches local, then user, then global)')
  .option('--config-file <file>', 'specific configuration file')
  .action(async (key, options) => {
    try {
      const command = new ConfigGetCommand();
      command.key = key;
      command.scope = parseScopeFromFlags(options);
      command.configFileName = options.configFile;
      
      const exitCode = await command.executeAsync(false);
      process.exit(exitCode);
    } catch (error) {
      ConsoleHelpers.writeErrorLine(`Error: ${error instanceof Error ? error.message : error}`);
      process.exit(1);
    }
  });

// Config set command
configCommand
  .command('set <key> <value>')
  .description('set a configuration setting')
  .option('--global, -g', 'use global scope (all users)')
  .option('--user, -u', 'use user scope (current user)')
  .option('--local, -l', 'use local scope (default)')
  .option('--config-file <file>', 'specific configuration file')
  .action(async (key, value, options) => {
    try {
      const command = new ConfigSetCommand();
      command.key = key;
      command.value = value;
      command.scope = parseScopeFromFlagsForSet(options);
      command.configFileName = options.configFile;
      
      const exitCode = await command.executeAsync(false);
      process.exit(exitCode);
    } catch (error) {
      ConsoleHelpers.writeErrorLine(`Error: ${error instanceof Error ? error.message : error}`);
      process.exit(1);
    }
  });

// Config clear command
configCommand
  .command('clear <key>')
  .description('clear a configuration setting')
  .option('--global, -g', 'use global scope (all users)')
  .option('--user, -u', 'use user scope (current user)')
  .option('--local, -l', 'use local scope (default)')
  .option('--config-file <file>', 'specific configuration file')
  .action(async (key, options) => {
    try {
      const command = new ConfigClearCommand();
      command.key = key;
      command.scope = parseScopeFromFlagsForSet(options);
      command.configFileName = options.configFile;
      
      const exitCode = await command.executeAsync(false);
      process.exit(exitCode);
    } catch (error) {
      ConsoleHelpers.writeErrorLine(`Error: ${error instanceof Error ? error.message : error}`);
      process.exit(1);
    }
  });

// Config add command
configCommand
  .command('add <key> <value>')
  .description('add a value to a list configuration setting')
  .option('--global, -g', 'use global scope (all users)')
  .option('--user, -u', 'use user scope (current user)')
  .option('--local, -l', 'use local scope (default)')
  .action(async (key, value, options) => {
    try {
      const command = new ConfigAddCommand();
      command.key = key;
      command.value = value;
      command.scope = parseScopeFromFlagsForSet(options);
      
      const exitCode = await command.executeAsync(false);
      process.exit(exitCode);
    } catch (error) {
      ConsoleHelpers.writeErrorLine(`Error: ${error instanceof Error ? error.message : error}`);
      process.exit(1);
    }
  });

// Config remove command
configCommand
  .command('remove <key> <value>')
  .description('remove a value from a list configuration setting')
  .option('--global, -g', 'use global scope (all users)')
  .option('--user, -u', 'use user scope (current user)')
  .option('--local, -l', 'use local scope (default)')
  .action(async (key, value, options) => {
    try {
      const command = new ConfigRemoveCommand();
      command.key = key;
      command.value = value;
      command.scope = parseScopeFromFlagsForSet(options);
      
      const exitCode = await command.executeAsync(false);
      process.exit(exitCode);
    } catch (error) {
      ConsoleHelpers.writeErrorLine(`Error: ${error instanceof Error ? error.message : error}`);
      process.exit(1);
    }
  });


// Helper function to parse scope from individual flags
function parseScopeFromFlags(options: any): ConfigFileScope {
  if (options.global || options.g || options.G) return ConfigFileScope.Global;
  if (options.user || options.u || options.U) return ConfigFileScope.User;
  if (options.local || options.l || options.L) return ConfigFileScope.Local;
  if (options.any || options.a || options.A) return ConfigFileScope.Any;
  
  // Default to Any for config get (matches C# behavior)
  return ConfigFileScope.Any;
}

// Helper function to parse scope from individual flags for set operations
function parseScopeFromFlagsForSet(options: any): ConfigFileScope {
  if (options.global || options.g || options.G) return ConfigFileScope.Global;
  if (options.user || options.u || options.U) return ConfigFileScope.User;
  if (options.local || options.l || options.L) return ConfigFileScope.Local;
  
  // Default to Local for config set/clear/add/remove (matches C# behavior)
  return ConfigFileScope.Local;
}

// Parse command line arguments
program.parse();