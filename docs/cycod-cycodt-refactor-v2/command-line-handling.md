# Command Line Handling

This document outlines how command-line handling will be refactored to support both applications.

## Current Command Line Structure

Currently, commands are structured as:

- `chatx <command> [options]`
- For test commands: `chatx test list [options]` and `chatx test run [options]`

The command parsing is handled in `CommandLineOptions.cs` which has a complex parsing logic that:

1. Identifies the command type (e.g., "test list", "config get")
2. Creates the appropriate command object
3. Parses command-specific options

## New Command Line Structure

### For cycod

The structure will remain similar to the current one:

- `cycod <command> [options]`

But test commands will be removed from this executable.

### For cycodt

The structure will be simplified to eliminate the "test" prefix:

- `cycodt list [options]` (instead of `chatx test list [options]`)
- `cycodt run [options]` (instead of `chatx test run [options]`)

Note that while the command names are simplified, the class names will remain as `TestListCommand` and `TestRunCommand` for clarity and consistency.

## Detailed Command Line Options Refactoring

The `CommandLineOptions.cs` class is a critical component that requires careful refactoring to support both applications while maintaining a clean architecture.

### Step 1: Create Base Class in Common Library

Create a base `CommandLineOptionsBase` class in the common library that contains shared functionality:

```csharp
// In src/common/CommandLine/CommandLineOptionsBase.cs
public abstract class CommandLineOptionsBase
{
    // Common properties for all command line options
    public List<Command> Commands { get; } = new List<Command>();
    public List<string> UnknownArgs { get; } = new List<string>();
    
    // Abstract method for registering commands
    protected abstract void RegisterCommands(CommandRegistry registry);
    
    // Common parsing logic
    public bool Parse(string[] args, out CommandLineException exception)
    {
        exception = null;
        
        try
        {
            // Create registry and register commands
            var registry = new CommandRegistry();
            RegisterCommands(registry);
            
            // Common parsing logic using the registry
            // This is the core of the parsing algorithm that both apps will share
            
            return true;
        }
        catch (CommandLineException ex)
        {
            exception = ex;
            return false;
        }
    }
    
    // Other helper methods for parsing that are used by both applications
    protected bool TryParseCommonOptions(string arg, ref int i, string[] args) 
    {
        // Process common options like --verbose, --quiet, etc.
        // Return true if the argument was handled
        return false; // Default implementation for options shared by both apps
    }
}
```

### Step 2: Create Application-Specific Classes

Create specialized classes for each application:

```csharp
// In src/cycod/CommandLineOptions.cs
public class CommandLineOptions : CommandLineOptionsBase
{
    // cycod-specific properties
    
    protected override void RegisterCommands(CommandRegistry registry)
    {
        // Register all non-test commands
        registry.RegisterCommand("chat", () => new ChatCommand());
        registry.RegisterCommand("config list", () => new ConfigListCommand());
        registry.RegisterCommand("config get", () => new ConfigGetCommand());
        // Register all other non-test commands
    }
    
    // Override common option parsing to handle cycod-specific options
    protected override bool TryParseCommonOptions(string arg, ref int i, string[] args)
    {
        // Try base implementation first
        if (base.TryParseCommonOptions(arg, ref i, args))
            return true;
            
        // Handle cycod-specific options
        if (arg == "--model")
        {
            // Process model option
            return true;
        }
        
        return false;
    }
}

// In src/cycodt/CommandLineOptions.cs
public class CommandLineOptions : CommandLineOptionsBase
{
    // cycodt-specific properties
    
    protected override void RegisterCommands(CommandRegistry registry)
    {
        // Register test commands with simplified names but keeping original class names
        registry.RegisterCommand("list", () => new TestListCommand()); // Register with simple name but using TestListCommand class
        registry.RegisterCommand("run", () => new TestRunCommand()); // Register with simple name but using TestRunCommand class
        registry.RegisterCommand("help", () => new HelpCommand());
        // Any other test-specific commands
    }
    
    // Override common option parsing to handle cycodt-specific options
    protected override bool TryParseCommonOptions(string arg, ref int i, string[] args)
    {
        // Try base implementation first
        if (base.TryParseCommonOptions(arg, ref i, args))
            return true;
            
        // Handle cycodt-specific options
        if (arg == "--format")
        {
            // Process format option
            return true;
        }
        
        return false;
    }
}
```

### Step 3: Command Registry Implementation

Create a robust command registry in the common library:

```csharp
// In src/common/CommandLine/CommandRegistry.cs
public class CommandRegistry
{
    private Dictionary<string, Func<Command>> _commandFactories = new();
    
    public void RegisterCommand(string name, Func<Command> factory)
    {
        _commandFactories[name.ToLowerInvariant()] = factory;
    }
    
    public bool TryGetCommand(string name, out Func<Command> factory)
    {
        // Try exact match first
        if (_commandFactories.TryGetValue(name.ToLowerInvariant(), out factory))
            return true;
            
        // Try prefix matching for multi-word commands
        foreach (var entry in _commandFactories)
        {
            if (entry.Key.StartsWith(name.ToLowerInvariant() + " "))
            {
                factory = entry.Value;
                return true;
            }
        }
        
        factory = null;
        return false;
    }
    
    public IEnumerable<string> GetRegisteredCommandNames()
    {
        return _commandFactories.Keys.ToList();
    }
}
```

## Command Implementation Changes

### TestListCommand and TestRunCommand

These commands will need to be modified to work without the "test" prefix in their exposed command names, while keeping their class names as is for clarity:

1. Create new command classes in the cycodt project:

```csharp
// In src/cycodt/CommandLineCommands/TestListCommand.cs
// Note: Class name remains as TestListCommand for clarity, only the command name is simplified
public class TestListCommand : TestBaseCommand
{
    public TestListCommand() : base("cycodt")
    {
        // Initialize with cycodt as the command prefix
    }
    
    public override string GetCommandName() => "list"; // Command name simplified (no "test" prefix)
    
    // Other methods remain the same
    public override void Execute()
    {
        // Implementation that uses _commandPrefix instead of hardcoded "chatx test"
    }
}

// In src/cycodt/CommandLineCommands/TestRunCommand.cs
// Note: Class name remains as TestRunCommand for clarity, only the command name is simplified
public class TestRunCommand : TestBaseCommand
{
    public TestRunCommand() : base("cycodt")
    {
        // Initialize with cycodt as the command prefix
    }
    
    public override string GetCommandName() => "run"; // Command name simplified (no "test" prefix)
    
    // Other methods remain the same
    public override void Execute()
    {
        // Implementation that uses _commandPrefix instead of hardcoded "chatx test"
    }
}
```

### Help Command

Create specialized help commands for each application:

```csharp
// In src/cycod/CommandLineCommands/HelpCommand.cs
public class HelpCommand : Command
{
    public override void Execute()
    {
        HelpHelpers.DisplayHelpTopic(HelpTopic, false, includeTestCommands: false, appName: "cycod");
    }
    
    public override string GetCommandName() => "help";
}

// In src/cycodt/CommandLineCommands/HelpCommand.cs
public class HelpCommand : Command
{
    public override void Execute()
    {
        HelpHelpers.DisplayHelpTopic(HelpTopic, false, includeTestCommands: true, appName: "cycodt");
    }
    
    public override string GetCommandName() => "help";
}
```

## Program.cs Changes

### cycod Program.cs

```csharp
public static class Program
{
    // Update the application name
    public static string Name => "cycod";
    
    public static async Task<int> Main(string[] args)
    {
        Console.WriteLine($"cycod v1.0.0");
        
        var commandLineOptions = new CommandLineOptions();
        if (!commandLineOptions.Parse(args, out var exception))
        {
            Console.Error.WriteLine($"Error: {exception.Message}");
            return 1;
        }
        
        var commands = ForEachVarHelpers.ExpandForEachVars(commandLineOptions.Commands).ToList();
        foreach (var command in commands)
        {
            throttler.Wait();
            
            var startedTask = command switch
            {
                ChatCommand chatCommand => chatCommand.ExecuteAsync(isTruelyInteractive),
                VersionCommand versionCommand => versionCommand.ExecuteAsync(isTruelyInteractive),
                GitHubLoginCommand loginCommand => loginCommand.ExecuteAsync(isTruelyInteractive),
                ConfigListCommand configListCommand => configListCommand.Execute(isTruelyInteractive),
                // Other non-test commands...
                _ => throw new NotImplementedException($"Command type {command.GetType()} not implemented.")
            };
            
            // Wait for completion or handle errors
        }
        
        return 0;
    }
}
```

### cycodt Program.cs

```csharp
public static class Program
{
    // Set the application name
    public static string Name => "cycodt";
    
    public static async Task<int> Main(string[] args)
    {
        Console.WriteLine($"cycodt v1.0.0");
        
        var commandLineOptions = new CommandLineOptions();
        if (!commandLineOptions.Parse(args, out var exception))
        {
            Console.Error.WriteLine($"Error: {exception.Message}");
            return 1;
        }
        
        var commands = ForEachVarHelpers.ExpandForEachVars(commandLineOptions.Commands).ToList();
        foreach (var command in commands)
        {
            throttler.Wait();
            
            var startedTask = command switch
            {
                TestListCommand listCommand => listCommand.Execute(isTruelyInteractive),
                TestRunCommand runCommand => runCommand.Execute(isTruelyInteractive),
                HelpCommand helpCommand => helpCommand.Execute(isTruelyInteractive),
                VersionCommand versionCommand => versionCommand.ExecuteAsync(isTruelyInteractive),
                _ => throw new NotImplementedException($"Command type {command.GetType()} not implemented.")
            };
            
            // Wait for completion or handle errors
        }
        
        return 0;
    }
    
    // No need for special configuration anymore
    static Program()
    {
        // We'll use a fixed string approach for CLI references
        // The test framework will always launch "cycod" as a child process
        // No parameterized approach is needed
    }
}
```

## Handling Multiple Commands

Both applications need to handle multiple commands with a proper throttling mechanism:

```csharp
// Move this to a common helper class
// Note: Error handling will remain the same as in the original implementation
// Exceptions are typically caught at the top level in Program.cs and displayed to the user
public static class CommandExecutionHelpers
{
    public static async Task ExecuteCommands(IEnumerable<Command> commands, bool isInteractive)
    {
        var throttler = new CommandThrottler();
        
        foreach (var command in commands)
        {
            throttler.Wait();
            
            try
            {
                await ExecuteCommandAsync(command, isInteractive);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error executing command: {ex.Message}");
            }
        }
    }
    
    private static async Task ExecuteCommandAsync(Command command, bool isInteractive)
    {
        // Dispatch based on command type
        // This will be implemented differently in each application
    }
}
```

## Conclusion

This refactoring approach:

1. Maintains a clean separation of concerns
2. Reuses shared parsing logic
3. Allows for application-specific command handling
4. Supports the different command structures needed by each application
5. Creates a maintainable and extensible architecture
6. Maintains class naming clarity (keeping TestListCommand/TestRunCommand classes) while simplifying command names
7. Preserves the existing error handling approach where exceptions are caught at top level