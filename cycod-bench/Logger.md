# Logger.cs

This class provides a comprehensive logging system for the CycodBench benchmark system.

## Responsibilities

- Provide consistent logging across all components
- Support multiple log targets (console, file, structured logging)
- Implement different log levels (Debug, Info, Warning, Error)
- Format log messages appropriately for different contexts
- Handle structured logging data
- Support correlation across different operations
- Configure logging based on application settings

## Public Interface

```csharp
public interface ILogger
{
    void Log(
        LogLevel level,
        string message,
        Exception exception = null,
        IDictionary<string, object> properties = null);
    
    void Debug(string message, IDictionary<string, object> properties = null);
    void Info(string message, IDictionary<string, object> properties = null);
    void Warning(string message, Exception exception = null, IDictionary<string, object> properties = null);
    void Error(string message, Exception exception = null, IDictionary<string, object> properties = null);
    void Critical(string message, Exception exception = null, IDictionary<string, object> properties = null);
    
    IDisposable BeginScope(string scopeName, IDictionary<string, object> properties = null);
    
    void Configure(LoggerConfiguration config);
    
    ILogger CreateLogger(string componentName);
    
    void Flush();
}
```

## Implementation

```csharp
public class Logger : ILogger
{
    // Constructor
    public Logger(LoggerConfiguration config);
    
    // Core logging methods
    public void Log(
        LogLevel level,
        string message,
        Exception exception = null,
        IDictionary<string, object> properties = null);
    
    public void Debug(string message, IDictionary<string, object> properties = null);
    public void Info(string message, IDictionary<string, object> properties = null);
    public void Warning(string message, Exception exception = null, IDictionary<string, object> properties = null);
    public void Error(string message, Exception exception = null, IDictionary<string, object> properties = null);
    public void Critical(string message, Exception exception = null, IDictionary<string, object> properties = null);
    
    // Logging contexts
    public IDisposable BeginScope(string scopeName, IDictionary<string, object> properties = null);
    
    // Configure logging
    public void Configure(LoggerConfiguration config);
    
    // Create a specialized logger for a component
    public ILogger CreateLogger(string componentName);
    
    // Flush logs to ensure they're written
    public void Flush();
}
```

## Implementation Overview

The Logger class will:

1. **Support multiple log targets**:
   - Console output with color coding
   - File logging with rotation support
   - Structured logging for machine processing
   - Optional integration with external logging systems

2. **Implement logging levels**:
   - Debug: Detailed information for debugging
   - Info: General operational messages
   - Warning: Potential issues that don't prevent operation
   - Error: Errors that prevent specific operations
   - Critical: Critical failures that require immediate attention

3. **Support structured logging**:
   - Log additional properties with messages
   - Support for serializing complex objects
   - Consistent formatting for structured data
   - Easy filtering and searching of logs

4. **Implement correlation and context**:
   - Track operations across multiple components
   - Maintain context through async operations
   - Support for nested operation contexts
   - Include timing information for operations

## Log Format

The logger will support multiple output formats:

### Console Format
```
[2023-08-15 14:30:45.123] [INFO] [DockerManager] Container c123abc started for problem-123
```

### File Format
```
2023-08-15T14:30:45.123Z|INFO|DockerManager|Container c123abc started for problem-123|{"containerId":"c123abc","problemId":"problem-123"}
```

### Structured Format (JSON)
```json
{
  "timestamp": "2023-08-15T14:30:45.123Z",
  "level": "INFO",
  "component": "DockerManager",
  "message": "Container c123abc started for problem-123",
  "properties": {
    "containerId": "c123abc",
    "problemId": "problem-123"
  }
}
```

## Context and Correlation

The Logger will support operation contexts:

```csharp
using (logger.BeginScope("ProcessProblem", new Dictionary<string, object> { ["problemId"] = "problem-123" }))
{
    // All logs within this scope will include the problemId property
    logger.Info("Starting problem processing");
    
    // Nested scope
    using (logger.BeginScope("DockerSetup"))
    {
        logger.Info("Creating container");
    }
}
```

## Configuration

The Logger will support configuration of:
- Minimum log level for each target
- Output formats
- File paths and rotation policies
- Console colors and formatting
- Property inclusion/exclusion

## Performance Considerations

The Logger will:
- Use asynchronous I/O for file logging
- Implement log level filtering early to avoid unnecessary string formatting
- Buffer logs to minimize I/O overhead
- Support batching for high-volume logging
- Use efficient serialization for structured logging

## Integration with .NET Logging

The Logger will:
- Implement standard .NET logging interfaces
- Support dependency injection patterns
- Allow integration with existing logging providers
- Follow .NET logging best practices

## Special Features

The Logger can support:
- Log redaction for sensitive information
- Log compaction for high-volume environments
- Log forwarding to centralized systems
- Real-time log filtering and analysis
- Performance metric collection tied to logging