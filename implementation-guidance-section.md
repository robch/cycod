# Implementation Guidance for Developers

## Overview

This section provides technical details and guidance for developers implementing the Custom Tools specification, covering key implementation considerations and best practices.

## Parameter Substitution and Escaping

### Parameter Replacement

Parameters in commands are enclosed in curly braces (`{PARAM_NAME}`) and should be replaced with the actual parameter values before execution. The implementation should:

1. Find all parameters in the command template
2. Replace each parameter with its value
3. Apply appropriate transformations and formats
4. Handle escape sequences properly

Example:
```csharp
// Example C# implementation
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

Example escaping function:
```csharp
// Example C# implementation for shell escaping
private string EscapeShellArgument(string arg)
{
    if (string.IsNullOrEmpty(arg)) return "\"\"";
    
    // Different escaping for different shells
    if (CurrentShell == Shell.Bash)
    {
        return $"'{arg.Replace("'", "'\\''")}'";
    }
    else if (CurrentShell == Shell.PowerShell)
    {
        return $"'{arg.Replace("'", "''")}'";
    }
    else // CMD
    {
        return $"\"{arg.Replace("\"", "\"\"")}\"";
    }
}
```

## Cross-Platform Compatibility

### Path Normalization

When `file-paths.normalize` is `true`, the implementation should convert paths to the correct format for the current platform:

```csharp
// Example C# implementation
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
// Example C# implementation
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

```csharp
// Example C# implementation
private async Task<string> CaptureOutputAsync(Process process, OutputOptions options)
{
    var output = new StringBuilder();
    var buffer = new char[4096];
    var reader = process.StandardOutput;
    
    while (!reader.EndOfStream)
    {
        var count = await reader.ReadAsync(buffer, 0, buffer.Length);
        var text = new string(buffer, 0, count);
        output.Append(text);
        
        // Check buffer limit
        if (options.BufferLimit > 0 && output.Length > options.BufferLimit)
        {
            output.Length = options.BufferLimit;
            break;
        }
    }
    
    return output.ToString();
}
```

### Streaming Output

For steps with `output.mode: stream`, the implementation should:

1. Stream the output to the specified destination
2. Not buffer more than necessary
3. Handle backpressure appropriately

```csharp
// Example C# implementation
private async Task StreamOutputAsync(Process process, OutputOptions options)
{
    var reader = process.StandardOutput;
    var buffer = new char[4096];
    
    while (!reader.EndOfStream)
    {
        var count = await reader.ReadAsync(buffer, 0, buffer.Length);
        var text = new string(buffer, 0, count);
        
        // Stream to the appropriate destination
        switch (options.StreamCallback)
        {
            case "console":
                Console.Write(text);
                break;
            case "file":
                await File.AppendAllTextAsync(options.StreamFile, text);
                break;
            case "function":
                await InvokeStreamCallbackAsync(text);
                break;
        }
    }
}
```

## Parallel Execution

### Step Execution Flow

For tools with parallel steps, the implementation should:

1. Build a dependency graph based on `wait-for` relationships
2. Execute independent steps in parallel
3. Wait for dependencies to complete before executing dependent steps
4. Handle step failures and conditions appropriately

```csharp
// Example C# implementation - simplified
private async Task ExecuteStepsAsync(List<ToolStep> steps, Dictionary<string, object> parameters)
{
    var results = new Dictionary<string, StepResult>();
    var pendingSteps = new HashSet<string>(steps.Select(s => s.Name));
    var runningTasks = new Dictionary<string, Task<StepResult>>();
    
    while (pendingSteps.Count > 0 || runningTasks.Count > 0)
    {
        // Start eligible steps
        foreach (var step in steps.Where(s => pendingSteps.Contains(s.Name)))
        {
            // Check if all dependencies are satisfied
            var waitFor = step.WaitFor ?? new List<string>();
            if (waitFor.All(dependency => results.ContainsKey(dependency)))
            {
                // Check run condition
                if (ShouldRunStep(step, results))
                {
                    pendingSteps.Remove(step.Name);
                    runningTasks[step.Name] = ExecuteStepAsync(step, parameters, results);
                }
                else
                {
                    // Skip this step
                    pendingSteps.Remove(step.Name);
                    results[step.Name] = new StepResult { ExitCode = 0, Output = "", Skipped = true };
                }
            }
        }
        
        // Wait for any task to complete
        if (runningTasks.Count > 0)
        {
            var completedTask = await Task.WhenAny(runningTasks.Values);
            var completedStep = runningTasks.First(kv => kv.Value == completedTask).Key;
            results[completedStep] = await completedTask;
            runningTasks.Remove(completedStep);
        }
    }
    
    return results;
}
```

## Resource Management and Cleanup

### Process Lifecycle

The implementation should properly manage process lifecycle to avoid resource leaks:

1. Create processes with appropriate settings
2. Monitor execution time and enforce timeouts
3. Clean up resources when done
4. Handle process termination

```csharp
// Example C# implementation
private async Task<ProcessResult> RunProcessAsync(ProcessStartInfo startInfo, int timeoutMs)
{
    using var process = new Process { StartInfo = startInfo };
    
    var cts = new CancellationTokenSource();
    if (timeoutMs > 0)
    {
        cts.CancelAfter(timeoutMs);
    }
    
    try
    {
        process.Start();
        
        var outputTask = CaptureOutputAsync(process, cts.Token);
        var errorTask = CaptureErrorAsync(process, cts.Token);
        
        var processExitTask = Task.Run(() => process.WaitForExit());
        
        await Task.WhenAll(processExitTask, outputTask, errorTask);
        
        return new ProcessResult
        {
            ExitCode = process.ExitCode,
            Output = await outputTask,
            Error = await errorTask
        };
    }
    catch (OperationCanceledException)
    {
        // Handle timeout
        process.Kill(entireProcessTree: true);
        return new ProcessResult
        {
            ExitCode = -1,
            Output = "Process timed out after " + timeoutMs + "ms",
            Error = "Process terminated due to timeout"
        };
    }
    finally
    {
        cts.Dispose();
    }
}
```

### Cleanup Actions

The implementation should ensure cleanup actions are executed:

1. Run step-specific cleanup commands
2. Execute global cleanup actions
3. Clean up temporary files
4. Release system resources

```csharp
// Example C# implementation
private async Task ExecuteCleanupActionsAsync(List<CleanupAction> actions)
{
    foreach (var action in actions)
    {
        try
        {
            if (action.DeleteTempFiles)
            {
                CleanupTempFiles();
            }
            
            if (!string.IsNullOrEmpty(action.FinalCommand))
            {
                await RunCommandAsync(action.FinalCommand);
            }
        }
        catch (Exception ex)
        {
            // Log but continue with other cleanup actions
            Console.Error.WriteLine($"Cleanup action failed: {ex.Message}");
        }
    }
}
```

## Function Calling Integration

### Schema Generation

The implementation should generate function schemas that match the tool definition:

```csharp
// Example C# implementation
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