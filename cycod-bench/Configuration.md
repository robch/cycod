# Configuration.cs

This class is responsible for managing the configuration settings for the CycodBench benchmark system.

## Responsibilities

- Load and validate configuration from various sources
- Provide strongly-typed access to configuration settings
- Handle default values and overrides
- Support multiple configuration profiles
- Validate configuration against requirements
- Provide clear error messages for misconfiguration

## Public Interface

```csharp
public interface IConfiguration
{
    Task LoadConfigurationAsync(
        string configFilePath = null,
        IDictionary<string, string> commandLineOverrides = null,
        CancellationToken cancellationToken = default);
    
    T GetSection<T>(string sectionName) where T : new();
    
    T GetValue<T>(string key, T defaultValue = default);
    
    (bool IsValid, IEnumerable<string> ValidationErrors) Validate();
    
    Task SaveConfigurationAsync(
        string configFilePath,
        CancellationToken cancellationToken = default);
}
```

## Implementation

```csharp
public class Configuration
{
    // Constructor
    public Configuration(ILogger logger);
    
    // Load configuration
    public async Task LoadConfigurationAsync(
        string configFilePath = null,
        IDictionary<string, string> commandLineOverrides = null,
        CancellationToken cancellationToken = default);
    
    // Get typed configuration section
    public T GetSection<T>(string sectionName) where T : new();
    
    // Get a specific configuration value
    public T GetValue<T>(string key, T defaultValue = default);
    
    // Check if configuration is valid
    public (bool IsValid, IEnumerable<string> ValidationErrors) Validate();
    
    // Save configuration
    public Task SaveConfigurationAsync(
        string configFilePath,
        CancellationToken cancellationToken = default);
}
```

## Implementation Overview

The Configuration class will:

1. **Support hierarchical configuration sources**:
   - File-based configuration (JSON, YAML)
   - Environment variables
   - Command-line arguments
   - In-memory defaults and overrides

2. **Handle configuration loading**:
   - Load from specified configuration file
   - Apply environment variable overrides
   - Apply command-line overrides
   - Resolve configuration inheritance

3. **Implement configuration validation**:
   - Validate required fields are present
   - Type checking and conversion
   - Range and constraint validation
   - Check for conflicting settings
   - Validate that Docker and other dependencies are properly configured

4. **Support configuration sections**:
   - Benchmark settings
   - Docker settings
   - Agent settings
   - Evaluation settings
   - Ensembler settings
   - Result settings
   - Logging settings

## Configuration Schema

The configuration file will follow a structured format:

```json
{
  "benchmark": {
    "workspaceBasePath": "./workspace",
    "maxParallelism": 8,
    "defaultCandidateCount": 8,
    "timeoutMinutes": 60
  },
  "docker": {
    "baseImageRegistry": "swebench.azurecr.io",
    "useContainerPool": false,
    "maxContainers": 10,
    "resourceLimits": {
      "cpuLimit": "1",
      "memoryLimit": "4g"
    }
  },
  "agent": {
    "executablePath": "./cycod",
    "timeoutMinutes": 30
  },
  "evaluation": {
    "timeoutMinutes": 15,
    "retryCount": 3
  },
  "ensembler": {
    "timeoutMinutes": 5,
    "maxBatchSize": 5
  },
  "results": {
    "outputBasePath": "./results",
    "saveIntermediateResults": true,
    "compressResults": false
  },
  "logging": {
    "logLevel": "Information",
    "logFilePath": "./logs/cycod-bench.log",
    "consoleLogLevel": "Information"
  }
}
```

## Environment Variable Support

The class will support environment variables with the format:
```
CYCOD_BENCH_SECTION__KEY=value
```

For example:
```
CYCOD_BENCH_BENCHMARK__WORKSPACEBASEPATH=/tmp/workspace
CYCOD_BENCH_DOCKER__USECONTAINERPOOL=true
```

## Command-Line Override Support

Command-line arguments will override both file and environment configurations:
```
--benchmark:workspaceBasePath=/custom/path
--docker:useContainerPool=true
```

## Validation and Error Handling

The Configuration class will:
- Perform validation when loading configuration
- Provide clear error messages for invalid settings
- Support validation rules for individual settings
- Validate compatibility between related settings
- Check that required external dependencies exist

## Profile Support

The class will support multiple configuration profiles:
- Development
- Production
- CI/CD
- Custom profiles

The active profile can be specified via:
```
--profile Production
```

## Default Configuration

The class will include sensible defaults for all settings, making it possible to run with minimal configuration.

## Advanced Features

The Configuration class can support:
- Dynamic reloading of configuration
- Configuration change notifications
- Configuration history tracking
- Export and import of configuration
- Sensitive value masking for logging and display