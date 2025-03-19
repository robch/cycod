# Migrating to Dependency Injection

This document outlines the necessary changes to migrate the current static method-based function calling system to support dependency injection (DI) with instance methods.

## Current Implementation Analysis

The current function calling system is designed around static methods:

1. `FunctionFactory` uses reflection to discover and register static methods decorated with `HelperFunctionDescriptionAttribute`.
2. `MethodInfo.Invoke(null, args)` is used to call these static methods (the first parameter is null because the methods are static).
3. The system relies on static classes like `ShellCommandToolHelperFunctions`, `StrReplaceEditorHelperFunctions`, and `TimeAndDateHelperFunctions`.

## Required Changes for DI Support

### 1. FunctionFactory Modifications

```csharp
public class FunctionFactory
{
    // Add support for instance methods
    private readonly Dictionary<MethodInfo, (ChatTool Tool, object? Instance)> _functions = new();
    
    // New method to add instance methods from an object
    public void AddFunctions(object instance)
    {
        var type = instance.GetType();
        var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public);
        foreach (var method in methods)
        {
            AddFunction(method, instance);
        }
    }
    
    // Modified to store the instance along with the method
    public void AddFunction(MethodInfo method, object? instance = null)
    {
        var attributes = method.GetCustomAttributes(typeof(HelperFunctionDescriptionAttribute), false);
        if (attributes.Length > 0)
        {
            var funcDescriptionAttrib = attributes[0] as HelperFunctionDescriptionAttribute;
            var funcDescription = funcDescriptionAttrib!.Description;

            string json = GetMethodParametersJsonSchema(method);
            _functions.TryAdd(method, (ChatTool.CreateFunctionTool(method.Name, funcDescription, new BinaryData(json)), instance));
        }
    }
    
    // Modified to handle instances
    public bool TryCallFunction(string functionName, string functionArguments, out string? result)
    {
        result = null;
        if (!string.IsNullOrEmpty(functionName) && !string.IsNullOrEmpty(functionArguments))
        {
            var function = _functions.FirstOrDefault(x => x.Value.Tool.FunctionName == functionName);
            if (function.Key != null)
            {
                result = CallFunction(function.Key, function.Value.Tool, functionArguments, function.Value.Instance);
                return true;
            }
        }
        return false;
    }
    
    // Modified to support instance methods
    private static string? CallFunction(MethodInfo methodInfo, ChatTool chatTool, string argumentsAsJson, object? instance)
    {
        // Parse arguments as before
        var parsed = JsonDocument.Parse(argumentsAsJson).RootElement;
        var arguments = new List<object?>();
        
        // Same parameter parsing code as before
        // ...
        
        // Call the function with the instance
        var args = arguments.ToArray();
        var result = CallFunction(methodInfo, args, instance);
        return ConvertFunctionResultToString(result);
    }
    
    // Modified to support instance methods
    private static object? CallFunction(MethodInfo methodInfo, object?[] args, object? instance)
    {
        var t = methodInfo.ReturnType;
        return t == typeof(Task)
            ? CallVoidAsyncFunction(methodInfo, args, instance)
            : t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Task<>)
                ? CallAsyncFunction(methodInfo, args, instance)
                : t.Name != "Void"
                    ? CallSyncFunction(methodInfo, args, instance)
                    : CallVoidFunction(methodInfo, args, instance);
    }
    
    // Each of these call methods would need to be updated to pass the instance
    private static object? CallVoidAsyncFunction(MethodInfo methodInfo, object?[] args, object? instance)
    {
        var task = methodInfo.Invoke(instance, args) as Task;
        task!.Wait();
        return true;
    }
    
    // Same pattern for other call methods
    // ...
}
```

### 2. Service Registration and DI Container

```csharp
// Example DI setup with Microsoft.Extensions.DependencyInjection
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Register your helper classes
        services.AddTransient<ShellCommandToolHelpers>();
        services.AddTransient<StrReplaceEditorHelpers>();
        services.AddTransient<TimeAndDateHelpers>();
        
        // Configure FunctionFactory to use instances from DI
        services.AddSingleton<FunctionFactory>(serviceProvider => {
            var factory = new FunctionFactory();
            
            // Add instance methods from services
            factory.AddFunctions(serviceProvider.GetRequiredService<ShellCommandToolHelpers>());
            factory.AddFunctions(serviceProvider.GetRequiredService<StrReplaceEditorHelpers>());
            factory.AddFunctions(serviceProvider.GetRequiredService<TimeAndDateHelpers>());
            
            return factory;
        });
    }
}
```

### 3. Convert Static Classes to Instance Classes

```csharp
// Example conversion of a static helper class
public class ShellCommandToolHelpers
{
    private readonly ILogger _logger;
    private readonly IConfiguration _config;

    // Constructor for DI
    public ShellCommandToolHelpers(ILogger<ShellCommandToolHelpers> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    [HelperFunctionDescription("Run commands in a bash shell...")]
    public async Task<string> RunBashCommandAsync(
        [HelperFunctionParameterDescription("The bash command to run.")] string command,
        [HelperFunctionParameterDescription("Timeout in milliseconds for the command execution.")] int timeoutMs = 60000)
    {
        _logger.LogInformation($"Running bash command: {command}");
        // Implementation using injected dependencies
        // ...
    }
    
    // Other methods similarly converted
}
```

### 4. FunctionCallContext Updates

The `FunctionCallContext` class would need to be updated to work with the new FunctionFactory:

```csharp
public class FunctionCallContext
{
    // Constructor would remain similar, but the functionFactory would have the instance capabilities
    public FunctionCallContext(FunctionFactory functionFactory, IList<ChatMessage> messages)
    {
        _functionFactory = functionFactory;
        _messages = messages;
    }
    
    // Rest of the implementation would stay largely the same
}
```

## Singleton Patterns

For the persistent shell session patterns (BashShellSession, CmdShellSession, PowershellShellSession), you have a few options:

1. **Register as Singletons in DI**: Instead of using static Instance properties, register them as singletons in the DI container.

2. **Factory Pattern**: Use a factory that the DI container can provide to create/access the shell sessions.

3. **Hybrid Approach**: Keep the singleton pattern but make it injectable:

```csharp
public class BashShellSession
{
    private static BashShellSession? _instance;
    private static readonly object _lock = new object();
    
    // Private constructor to prevent direct instantiation
    private BashShellSession() { }
    
    // Allow DI container to access the instance
    public static BashShellSession Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new BashShellSession();
                }
            }
            return _instance;
        }
    }
    
    // Rest of the implementation
}
```

## Changes in Attribute Discovery

With instance methods, you'll need to ensure your attribute discovery logic accounts for the differences between static and instance methods:

```csharp
// In your function discovery code
var methods = type.GetMethods(BindingFlags.Public | 
                               (instance == null ? BindingFlags.Static : BindingFlags.Instance));
```

## Conclusion

Moving to DI requires substantial changes to the FunctionFactory implementation to support instance methods, but the core function calling mechanism can remain similar. The main changes involve:

1. Modifying FunctionFactory to store and use object instances
2. Converting static helper classes to instance classes with dependencies
3. Setting up a DI container to provide instances
4. Updating how methods are invoked to use the appropriate instance

These changes will make your code more testable, maintainable, and allow for proper separation of concerns.