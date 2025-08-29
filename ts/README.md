# CycoD TypeScript Implementation

A Node.js/TypeScript implementation of the CYCODEV CLI, specifically focusing on the `cycod config` functionality.

## Features Implemented

- ✅ Complete configuration management system with local/user/global scopes
- ✅ YAML configuration file support
- ✅ Known settings validation and normalization
- ✅ Secret value masking
- ✅ List value operations (add/remove)
- ✅ All config commands: `list`, `get`, `set`, `clear`, `add`, `remove`
- ✅ Comprehensive test suite
- ✅ TypeScript with strict type checking

## Installation

```bash
npm install
npm run build
```

## Usage

### Configuration Commands

```bash
# List all configuration settings
./dist/bin/cycod.js config list

# Get a specific setting
./dist/bin/cycod.js config get provider

# Set a configuration value
./dist/bin/cycod.js config set provider openai

# Set a list value
./dist/bin/cycod.js config set "test.list" "[item1, item2, item3]"

# Add to a list
./dist/bin/cycod.js config add "test.list" "item4"

# Remove from a list
./dist/bin/cycod.js config remove "test.list" "item2"

# Clear a setting
./dist/bin/cycod.js config clear provider
```

### Scope Options

```bash
# Set in specific scope
./dist/bin/cycod.js config set --scope user provider openai
./dist/bin/cycod.js config set --scope global provider openai

# Get from specific scope
./dist/bin/cycod.js config get --scope local provider
```

## Architecture

### Core Components

1. **ConfigStore** - Singleton configuration manager with scope precedence
2. **ConfigFile** - Abstract base class with YAML/INI implementations  
3. **ConfigFileScope** - Enum defining configuration scopes (Global/User/Local/FileName/Any)
4. **KnownSettings** - Registry of recognized settings with validation and normalization
5. **ConfigValue** - Type-safe configuration value container with source tracking

### Configuration Scopes

Settings are resolved in this priority order:
1. Command line arguments (highest priority)
2. Filename scope (--config-file)
3. Local scope (./.cycod/config.yaml)
4. User scope (~/.cycod/config.yaml)
5. Global scope (/etc/cycod/config.yaml on Unix, %ALLUSERSPROFILE%\\cycod on Windows)

### Known Settings

The system recognizes these configuration categories:
- **Provider settings**: `provider`
- **GitHub Copilot**: `github.copilot.token`, `github.copilot.endpoint`
- **OpenAI**: `openai.token`, `openai.endpoint`, `openai.model`
- **Azure OpenAI**: `azure.openai.token`, `azure.openai.endpoint`, `azure.openai.deployment`
- **Chat History**: `chat.history.directory`, `chat.history.max.files`
- **System**: `system.debug`, `system.quiet`, `system.prompt.default`
- **Timeouts**: `timeout.request`, `timeout.response`

Aliases are supported (e.g., `openai_token` → `openai.token`, `debug` → `system.debug`).

## Development

```bash
# Run tests
npm test

# Run tests in watch mode
npm run test:watch

# Run with coverage
npm run test:coverage

# Lint code
npm run lint

# Format code
npm run format
```

## File Structure

```
src/
├── Configuration/
│   ├── ConfigFileScope.ts      # Scope enumeration
│   ├── ConfigValue.ts          # Value container with metadata
│   ├── ConfigFile.ts          # File I/O abstraction
│   ├── ConfigStore.ts         # Main configuration manager
│   └── KnownSettings.ts       # Settings registry
├── CommandLineCommands/ConfigCommands/
│   ├── ConfigBaseCommand.ts    # Base command class
│   ├── ConfigListCommand.ts    # List settings
│   ├── ConfigGetCommand.ts     # Get setting
│   ├── ConfigSetCommand.ts     # Set setting
│   ├── ConfigClearCommand.ts   # Clear setting
│   ├── ConfigAddCommand.ts     # Add to list
│   └── ConfigRemoveCommand.ts  # Remove from list
├── Helpers/
│   ├── PathHelpers.ts         # File path utilities
│   └── ConsoleHelpers.ts      # Output formatting
├── bin/
│   └── cycod.ts              # CLI entry point
└── __tests__/                # Test suites
```

## Next Steps

To complete the full CYCODEV CLI implementation:

1. **Chat functionality** - Implement interactive AI chat with provider integration
2. **System prompts** - Add prompt management commands
3. **Aliases** - Implement command aliases
4. **History management** - Add chat history commands
5. **MCP support** - Model Context Protocol integration
6. **Web operations** - Search and fetch functionality
7. **Shell commands** - Interactive slash commands

The configuration system provides a solid foundation that other components can build upon.