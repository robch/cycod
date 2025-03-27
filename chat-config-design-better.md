# Configuration System Design Document

## Overview

This document outlines the design for a unified configuration system that integrates configuration from multiple sources with a clear precedence order. The system supports both well-known settings and user-defined variables, multiple configuration formats, and seamless integration with command-line arguments, environment variables, and template processing.

## Terminology

- **Hive**: A configuration storage location
- **Known Setting**: A predefined configuration setting with schema and multiple naming formats
- **Variable**: A user-defined configuration setting without schema
- **Secret**: A sensitive configuration value that should be obfuscated in display
- **Dot Notation**: The canonical format for setting names (e.g., `GitHub.Token`)
- **Environment Variable Format**: Format for environment variables (e.g., `GITHUB_TOKEN`)
- **CLI Option Format**: Format for command-line options (e.g., `--github-token`)

## Configuration Sources and Precedence

The configuration system reads from multiple sources in the following order (highest to lowest precedence):

1. **CLI Options**:
   - Known settings specified by their CLI flag (e.g., `--github-token`)
   - Variables set with `--var NAME=VALUE`

2. **Environment Variables**:
   - Only applicable for known settings
   - Using their environment variable format (e.g., `GITHUB_TOKEN`)

3. **Loadable Hives**: 
   - Explicitly loaded with `--load-config`
   - Order based on load sequence (last loaded has highest precedence)

4. **Local Hive**:
   - Project-specific settings
   - YAML takes precedence over INI

5. **User Hive**:
   - User-specific settings
   - YAML takes precedence over INI

6. **Global Hive**:
   - System-wide settings
   - YAML takes precedence over INI

## Core Components

### Existing Classes to Modify

#### `ConfigStore` (src\Configuration\ConfigStore.cs)

- Add field `_loadedHives` as `List<LoadableHive>`
- Add field `_commandLineSettings` as `Dictionary<string, object>`
- Add field `_secretSettings` as `HashSet<string>`
- Modify method `GetFromAnyScope` to include command-line and loadable hives
- Add method `AddLoadableHive(string path, int priority)`
- Add method `IsSecret(string key)`
- Add method `SetFromCommandLine(string key, object value)`
- Update methods `ListAll` and `ListScope` to handle all sources
- Modify method `Set` to respect precedence rules

#### `ConfigPathHelpers` (src\Configuration\ConfigPathHelpers.cs)

- Rename field `_pathToEnvVarMap` to `_knownSettingsMappings`
- Add field `_secretKeys` as `HashSet<string>`
- Add method `ToCLIOption(string dotNotation)`
- Add method `FromCLIOption(string cliOption)`
- Add method `IsKnownSetting(string key)`
- Add method `IsSecret(string key)`
- Enhance existing methods to better handle format conversion

#### `ConfigDisplayHelpers` (src\Configuration\ConfigDisplayHelpers.cs)

- Modify method `DisplayConfigValue` to handle secret obfuscation
- Add parameter `bool showSource` to display methods
- Add method `DisplaySettingSource` to show where a value came from
- Add method `DisplaySettingFormats` to show all formats for a setting

#### `ConfigFileHelpers` (src\Configuration\ConfigFileHelpers.cs)

- Add method `GetLoadableHiveFilePath(string path)`
- Add method `ValidateConfigFile(string path)`
- Update method `GetConfigFileName` to handle loadable hives

#### `ConfigValue` (src\Configuration\ConfigValue.cs)

- Add property `Source` as `ConfigSource`
- Add property `IsSecret` as `bool`
- Add method `AsObfuscated()` for secure display
- Add constructors that include source information

#### `ConfigFileScope` (src\Configuration\ConfigFileScope.cs)

- Add enum value `Loadable` to represent loadable hives
- Update related documentation

#### `CommandLineOptions` (src\CommandLine\CommandLineOptions.cs)

- Add fields for tracking configuration-related options
- Add method `ParseKnownSettingOption(string arg)`
- Update method `TryParseGlobalCommandLineOptions` to handle `--load-config`
- Enhance `TryParseChatCommandOptions` to use shared configuration

#### `Command` (src\CommandLine\Command.cs)

- Add method `AcceptsKnownSetting(string name)`
- Add property `AllowedSettings` as `HashSet<string>`
- Add method `InitializeConfiguration()` 

#### `TemplateVariables` (src\Templates\TemplateVariables.cs)

- Update method `Get` to check configuration from ConfigStore
- Add method `GetFromConfiguration(string name)`
- Add handling for secure template values

### New Classes to Create

#### `LoadableHive` (new file: src\Configuration\LoadableHive.cs)

- Properties:
  - `FilePath` as `string`
  - `Priority` as `int`
  - `ConfigData` as `Dictionary<string, object>`
- Methods:
  - `Constructor(string path, int priority)`
  - `Load()`
  - `GetValue(string key)`
  - `ContainsKey(string key)`

#### `KnownSettings` (new file: src\Configuration\KnownSettings.cs)

- Static class containing:
  - Constants for all known setting names
  - Method `IsKnown(string key)`
  - Method `IsSecret(string key)`
  - Method `GetAllKnownSettings()`
  - Categories for settings (Azure, OpenAI, GitHub, etc.)

#### `ConfigSource` (new file: src\Configuration\ConfigSource.cs)

- Enum with values:
  - `CommandLine`
  - `EnvironmentVariable`
  - `LoadableHive`
  - `LocalHive`
  - `UserHive`
  - `GlobalHive`
  - `Default`
  - `NotFound`

#### `KnownSettingsCLIParser` (new file: src\Configuration\KnownSettingsCLIParser.cs)

- Static class with methods:
  - `TryParseCLIOption(string arg, out string settingName, out string value)`
  - `GenerateCLIOption(string settingName, string value)`
  - `GetKnownSettingFromArg(string arg)`

#### `ConfigurationManager` (new file: src\Configuration\ConfigurationManager.cs)

- Singleton to coordinate configuration access:
  - Method `Initialize(CommandLineOptions options)`
  - Method `GetValue(string key, Command requestingCommand)`
  - Method `SetValue(string key, object value, ConfigSource source)`
  - Method `LoadConfigFile(string path)`
  - Method `IsAccessAllowed(string key, Command command)`

#### `SettingsAccessPolicy` (new file: src\Configuration\SettingsAccessPolicy.cs)

- Controls which settings a command can access:
  - Method `RegisterCommand(Type commandType, HashSet<string> allowedSettings)`
  - Method `IsAccessAllowed(string key, Command command)`
  - Method `GetAllowedSettings(Command command)`

## Integration Points

### Command-Line Integration

- New option `--load-config <path>` to load external configuration files
- Support for known settings in CLI format (e.g., `--github-token <value>`)
- Expansion of `@KEY` references to configuration values
- Commands declare which known settings they accept

### Template Integration

- Configuration values accessible in templates via `{key}` syntax
- Security for secrets in templates
- Consistent access pattern regardless of configuration source

### Environment Variable Integration

- Known settings accessible via environment variables
- Automatic format conversion between formats

## User Experience

### Configuration Display

- Enhanced display with source information
- Obfuscation of secret values
- Option to show all available formats for a setting
- Clear indication of conflicts between sources

### Help and Documentation

- Enhanced help text explaining the configuration system
- Documentation of known settings and their formats
- Examples of configuration usage

## Implementation Plan

1. **Core Components**:
   - Enhance ConfigStore and related classes
   - Add support for loadable hives
   - Implement source tracking and precedence

2. **CLI Integration**:
   - Update command-line parsing
   - Add known settings parsing
   - Support for configuration loading options

3. **Command Updates**:
   - Define allowed settings per command
   - Update commands to use unified configuration

4. **Display and UX**:
   - Enhance configuration display
   - Implement secret handling
   - Add verbose output options

5. **Documentation**:
   - Update help text
   - Add examples and tutorials
   - Document known settings

## Security Considerations

- Secret values must be obfuscated in display
- Access control for sensitive settings
- Careful handling of secrets in templates and command output
- Validation of configuration sources

## Backwards Compatibility

- Existing configuration files continue to work
- Commands maintain current behavior
- Environment variables still function as before
- Gradual transition to the new system

## Future Enhancements

- Configuration validation
- Schema-based configuration
- Dynamic configuration reloading
- Configuration export/import tools
- Interactive configuration setup