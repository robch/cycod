# Implementation Guidance

## Overview

This section provides technical details for developers implementing the Custom Tools specification, covering key implementation considerations and best practices.

## Parameter Substitution and Escaping

### Parameter Replacement

Parameters in commands are enclosed in curly braces (`{PARAM_NAME}`) and should be replaced with the actual parameter values before execution. The implementation should:

1. Find all parameters in the command template
2. Replace each parameter with its value
3. Apply appropriate transformations and formats
4. Handle escape sequences properly

Example implementation:
```csharp
private string SubstituteParameters(string command, Dictionary<string, object> parameters)
{
    foreach (var param in parameters)
    {
        var placeholder = $"{{{param.Key}}}";
        var value = param.Value?.ToString() ?? string.Empty;
        
        // Apply transformations if defined
        if (parameterDefinitions.TryGetValue(param.Key, out var definition) && 
            !string.IsNullOrEmpty(definition.Transform))
        {
            value = ApplyTransformation(value, definition.Transform);
        }
        
        // Format the parameter if specified
        if (definition?.Format != null)
        {
            value = definition.Format.Replace("{value}", value);
        }
        
        // Escape shell metacharacters if required
        if (definition?.Security?.EscapeShell == true)
        {
            value = EscapeShellArgument(value);
        }
        
        command = command.Replace(placeholder, value);
    }
    
    return command;
}
```

### Security Considerations

To prevent command injection, implementations should:

1. Validate all parameter values against their defined validation rules
2. Escape shell metacharacters when `escape-shell: true` is specified
3. Use proper argument passing mechanisms rather than direct string interpolation

## Cross-Platform Compatibility

### Path Normalization

When `file-paths.normalize` is `true`, the implementation should convert paths to the correct format for the current platform:

```csharp
private string NormalizePath(string path)
{
    if (string.IsNullOrEmpty(path)) return path;
    
    // Replace separators based on platform
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        return path.Replace('/', '\\');
    }
    else
    {
        return path.Replace('\\', '/');
    }
}
```

### Platform-Specific Command Selection

For tools with platform-specific commands, the implementation should select the appropriate command based on the current operating system:

```csharp
private string SelectPlatformCommand(PlatformCommands commands)
{
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && 
        !string.IsNullOrEmpty(commands.Windows))
    {
        return commands.Windows;
    }
    else if ((RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || 
              RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) && 
             !string.IsNullOrEmpty(commands.Unix))
    {
        return commands.Unix;
    }
    else
    {
        return commands.Default;
    }
}
```

## Output Capturing and Streaming

### Buffered Output

For steps with `output.mode: buffer`, the implementation should:

1. Capture the standard output into a buffer
2. Respect the `buffer-limit` setting
3. Make the output available for reference by subsequent steps

### Streaming Output

For steps with `output.mode: stream`, the implementation should:

1. Stream the output to the specified destination
2. Not buffer more than necessary
3. Handle backpressure appropriately

## Parallel Execution

### Step Execution Flow

For tools with parallel steps, the implementation should:

1. Build a dependency graph based on `wait-for` relationships
2. Execute independent steps in parallel
3. Wait for dependencies to complete before executing dependent steps
4. Handle step failures and conditions appropriately

## Resource Management and Cleanup

### Process Lifecycle

The implementation should properly manage process lifecycle to avoid resource leaks:

1. Create processes with appropriate settings
2. Monitor execution time and enforce timeouts
3. Clean up resources when done
4. Handle process termination

### Cleanup Actions

The implementation should ensure cleanup actions are executed:

1. Run step-specific cleanup commands
2. Execute global cleanup actions
3. Clean up temporary files
4. Release system resources

## Function Calling Integration

### Schema Generation

The implementation should generate function schemas that match the tool definition:

```csharp
private FunctionSchema GenerateFunctionSchema(CustomToolDefinition tool)
{
    var schema = new FunctionSchema
    {
        Name = tool.Name,
        Description = tool.Description,
        Parameters = new ParametersObject { Properties = new Dictionary<string, ParameterDefinition>() }
    };
    
    foreach (var param in tool.Parameters)
    {
        schema.Parameters.Properties[param.Key] = new ParameterDefinition
        {
            Type = MapParameterType(param.Value.Type),
            Description = param.Value.Description,
            Required = param.Value.Required
        };
        
        if (param.Value.Default != null)
        {
            schema.Parameters.Properties[param.Key].Default = param.Value.Default;
        }
    }
    
    return schema;
}

private string MapParameterType(string toolType)
{
    return toolType switch
    {
        "string" => "string",
        "number" => "number",
        "boolean" => "boolean",
        "array" => "array",
        "object" => "object",
        _ => "string"  // Default to string for unknown types
    };
}
```