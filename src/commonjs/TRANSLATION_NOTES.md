# C# to TypeScript Translation Notes

This document describes the translation of the cycod common C# library to TypeScript, noting any issues and differences.

## Project Structure

The TypeScript project maintains the same folder structure as the original C# project:

- **CommandLine/** - Command-line parsing and command classes
- **CommandLineCommands/** - Built-in commands (Help, Version)
- **Configuration/** - Configuration management (files, values, scoping)
- **Helpers/** - Utility classes (File, Console, Process, Path, String helpers)
- **ProcessExecution/** - Process execution and management
- **ShellHelpers/** - Shell session management
- **Templates/** - Template processing and variable substitution

## Key Dependencies Added

The TypeScript version uses these Node.js libraries to replace C# functionality:

- **js-yaml** - For YAML configuration file parsing (replaces YamlDotNet)
- **ini** - For INI configuration file parsing
- **chalk** - For console color support (optional)
- **commander** - For command-line parsing enhancement (optional)
- **glob** - For file pattern matching

## Translation Details

### CommandLine Classes

| C# Class | TypeScript Class | Notes |
|----------|------------------|-------|
| `Command` | `Command` | ✅ Abstract base class, async methods use Promise |
| `CommandLineException` | `CommandLineException` | ✅ Extends Error class |
| `CommandLineOptions` | `CommandLineOptions` | ⚠️ Simplified parsing logic, alias expansion not fully implemented |
| `CommandWithVariables` | `CommandWithVariables` | ✅ Uses Map instead of Dictionary |
| `ForEachVariable` | `ForEachVariable` | ✅ Direct translation |
| `ForEachVarHelpers` | `ForEachVarHelpers` | ✅ Logic preserved with array methods |

### Configuration Classes

| C# Class | TypeScript Class | Notes |
|----------|------------------|-------|
| `ConfigSource` | `ConfigSource` | ✅ Enum converted to string enum |
| `ConfigFileScope` | `ConfigFileScope` | ✅ Enum converted to string enum |
| `ConfigValue` | `ConfigValue` | ✅ Type conversions adapted for JavaScript |
| `ConfigFile` | `ConfigFile` | ✅ Factory pattern preserved |
| `IniConfigFile` | `IniConfigFile` | ✅ Uses 'ini' library |
| `YamlConfigFile` | `YamlConfigFile` | ✅ Uses 'js-yaml' library |
| `ConfigStore` | `ConfigStore` | ⚠️ Simplified implementation, missing some advanced features |

### Helper Classes

| C# Class | TypeScript Class | Notes |
|----------|------------------|-------|
| `FileHelpers` | `FileHelpers` | ✅ Uses Node.js fs and glob modules |
| `ConsoleHelpers` | `ConsoleHelpers` | ⚠️ Color support simplified, stdin handling different |
| `ProcessHelpers` | `ProcessHelpers` | ✅ Uses Node.js child_process |
| `PathHelpers` | `PathHelpers` | ✅ Uses Node.js path module |
| `StringHelpers` | `StringHelpers` | ✅ String manipulation methods |

### Process Execution

| C# Class | TypeScript Class | Notes |
|----------|------------------|-------|
| `RunnableProcess` | `RunnableProcess` | ✅ Uses Node.js child_process |
| `RunnableProcessBuilder` | `RunnableProcessBuilder` | ✅ Builder pattern preserved |
| `RunnableProcessResult` | `RunnableProcessResult` | ✅ All enums and properties translated |

### Other Classes

| C# Class | TypeScript Class | Notes |
|----------|------------------|-------|
| `ProgramRunner` | `ProgramRunner` | ⚠️ Simplified error handling and console management |
| `ShellSession` | `ShellSession` | ⚠️ Basic implementation, persistent shell features simplified |
| `TemplateHelpers` | `TemplateHelpers` | ⚠️ Basic template processing, expression evaluation simplified |

## Known Issues and Limitations

### ⚠️ Partial Implementations

1. **CommandLineOptions**: 
   - Alias expansion logic is stubbed
   - Some advanced parsing features missing
   - Known settings integration incomplete

2. **ConfigStore**:
   - Environment variable handling simplified  
   - Config file scope resolution basic
   - Some advanced configuration features missing

3. **ConsoleHelpers**:
   - Color support not fully implemented
   - Status display simplified (no cursor manipulation)
   - Stdin reading is synchronous placeholder

4. **ProgramRunner**:
   - Exception handling simplified
   - Console color save/restore not implemented
   - Help and version display logic stubbed

5. **ShellSession**:
   - Persistent shell functionality basic
   - Process management simplified
   - Command execution timeout handling basic

6. **TemplateHelpers**:
   - Expression calculator not implemented
   - Conditional logic very basic
   - Advanced template features missing

### 🛠️ TODO Items

1. Implement full alias file expansion in CommandLineOptions
2. Add comprehensive known settings support to ConfigStore
3. Implement proper console color support in ConsoleHelpers
4. Add expression calculator for template processing
5. Enhance shell session persistence and command queuing
6. Add comprehensive error handling and logging
7. Implement proper stdin/stdout handling
8. Add unit tests for all translated classes

## Usage Example

```typescript
import { 
  ProgramRunner, 
  CommandLineOptions, 
  ConfigStore,
  FileHelpers,
  ProcessHelpers 
} from './src';

class MyCLI extends ProgramRunner {
  protected parseCommandLine(args: string[]) {
    const options = new CommandLineOptions();
    return options.parse(args);
  }
}

// Use configuration
const config = ConfigStore.instance;
config.loadConfigFile('./config.yaml');
const value = config.getFromAnyScope('myapp.setting');

// Use file helpers
const files = FileHelpers.findFiles('.', '*.ts');

// Use process helpers
const result = await ProcessHelpers.runProcess('ls -la');
```

## Building and Running

```bash
# Install dependencies
npm install

# Build TypeScript
npm run build

# Run in development mode
npm run dev
```

The compiled JavaScript will be output to the `dist/` directory with corresponding `.d.ts` type definition files for TypeScript consumers.