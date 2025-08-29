#!/usr/bin/env node
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const commander_1 = require("commander");
const ConfigListCommand_1 = require("../CommandLineCommands/ConfigCommands/ConfigListCommand");
const ConfigGetCommand_1 = require("../CommandLineCommands/ConfigCommands/ConfigGetCommand");
const ConfigSetCommand_1 = require("../CommandLineCommands/ConfigCommands/ConfigSetCommand");
const ConfigClearCommand_1 = require("../CommandLineCommands/ConfigCommands/ConfigClearCommand");
const ConfigAddCommand_1 = require("../CommandLineCommands/ConfigCommands/ConfigAddCommand");
const ConfigRemoveCommand_1 = require("../CommandLineCommands/ConfigCommands/ConfigRemoveCommand");
const ConfigFileScope_1 = require("../Configuration/ConfigFileScope");
const ConsoleHelpers_1 = require("../Helpers/ConsoleHelpers");
const program = new commander_1.Command();
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
    if (opts.quiet)
        ConsoleHelpers_1.ConsoleHelpers.setQuiet(true);
    if (opts.debug)
        ConsoleHelpers_1.ConsoleHelpers.setDebug(true);
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
        const command = new ConfigListCommand_1.ConfigListCommand();
        command.scope = parseScopeFromFlags(options);
        command.configFileName = options.configFile;
        const exitCode = await command.executeAsync(false);
        process.exit(exitCode);
    }
    catch (error) {
        ConsoleHelpers_1.ConsoleHelpers.writeErrorLine(`Error: ${error instanceof Error ? error.message : error}`);
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
        const command = new ConfigGetCommand_1.ConfigGetCommand();
        command.key = key;
        command.scope = parseScopeFromFlags(options);
        command.configFileName = options.configFile;
        const exitCode = await command.executeAsync(false);
        process.exit(exitCode);
    }
    catch (error) {
        ConsoleHelpers_1.ConsoleHelpers.writeErrorLine(`Error: ${error instanceof Error ? error.message : error}`);
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
        const command = new ConfigSetCommand_1.ConfigSetCommand();
        command.key = key;
        command.value = value;
        command.scope = parseScopeFromFlagsForSet(options);
        command.configFileName = options.configFile;
        const exitCode = await command.executeAsync(false);
        process.exit(exitCode);
    }
    catch (error) {
        ConsoleHelpers_1.ConsoleHelpers.writeErrorLine(`Error: ${error instanceof Error ? error.message : error}`);
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
        const command = new ConfigClearCommand_1.ConfigClearCommand();
        command.key = key;
        command.scope = parseScopeFromFlagsForSet(options);
        command.configFileName = options.configFile;
        const exitCode = await command.executeAsync(false);
        process.exit(exitCode);
    }
    catch (error) {
        ConsoleHelpers_1.ConsoleHelpers.writeErrorLine(`Error: ${error instanceof Error ? error.message : error}`);
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
        const command = new ConfigAddCommand_1.ConfigAddCommand();
        command.key = key;
        command.value = value;
        command.scope = parseScopeFromFlagsForSet(options);
        const exitCode = await command.executeAsync(false);
        process.exit(exitCode);
    }
    catch (error) {
        ConsoleHelpers_1.ConsoleHelpers.writeErrorLine(`Error: ${error instanceof Error ? error.message : error}`);
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
        const command = new ConfigRemoveCommand_1.ConfigRemoveCommand();
        command.key = key;
        command.value = value;
        command.scope = parseScopeFromFlagsForSet(options);
        const exitCode = await command.executeAsync(false);
        process.exit(exitCode);
    }
    catch (error) {
        ConsoleHelpers_1.ConsoleHelpers.writeErrorLine(`Error: ${error instanceof Error ? error.message : error}`);
        process.exit(1);
    }
});
// Helper function to parse scope from individual flags
function parseScopeFromFlags(options) {
    if (options.global)
        return ConfigFileScope_1.ConfigFileScope.Global;
    if (options.user)
        return ConfigFileScope_1.ConfigFileScope.User;
    if (options.local)
        return ConfigFileScope_1.ConfigFileScope.Local;
    if (options.any)
        return ConfigFileScope_1.ConfigFileScope.Any;
    // Default to Any for config get (matches C# behavior)
    return ConfigFileScope_1.ConfigFileScope.Any;
}
// Helper function to parse scope from individual flags for set operations
function parseScopeFromFlagsForSet(options) {
    if (options.global)
        return ConfigFileScope_1.ConfigFileScope.Global;
    if (options.user)
        return ConfigFileScope_1.ConfigFileScope.User;
    if (options.local)
        return ConfigFileScope_1.ConfigFileScope.Local;
    // Default to Local for config set/clear/add/remove (matches C# behavior)
    return ConfigFileScope_1.ConfigFileScope.Local;
}
// Parse command line arguments
program.parse();
//# sourceMappingURL=cycod.js.map