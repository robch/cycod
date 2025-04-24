# Common Components

This document outlines the components that will be moved to the shared common library (src/common).

## Configuration System

The configuration system is used by both applications and should be moved to the common library.

### Configuration Components to Move
- **ConfigFileScope.cs** - Defines scope levels (Global, User, Local, etc.)
- **ConfigFile.cs** - Base configuration file handling
- **ConfigFileHelpers.cs** - Helper methods for config files
- **ConfigStore.cs** - Main configuration storage
- **ConfigValue.cs** - Configuration value representation
- **ConfigSource.cs** - Defines configuration value source precedence
- **ScopeFileHelpers.cs** - Helpers for scoped files
- **IniConfigFile.cs** - INI file format support
- **YamlConfigFile.cs** - YAML file format support
- **ConfigDisplayHelpers.cs** - Helpers for displaying configuration settings
- **ProfileFileHelpers.cs** - Helpers for loading and managing profiles
- **KnownSettings.cs** - Defines known configuration settings and format conversions
- **KnownSettingsCLIParser.cs** - Parses command line options related to known settings

These components are essential for both applications as they handle:
- Reading/writing configuration files
- Managing configuration scopes (Global, User, Local)
- Retrieving configuration values

### Configuration System Updates Required
- In **ConfigFileHelpers.cs**:
  - Change `private const string CONFIG_DIR_NAME = $".{Program.Name}";` to use a fixed name
  - Update to `private const string CONFIG_DIR_NAME = ".cycod";`
- In **ScopeFileHelpers.cs**:
  - Update occurrences of `var appDirName = $".{Program.Name}";` to use the fixed name
  - Change to `var appDirName = ".cycod";` in both `FindFileInAnyScope` and `FindDirectoryInAnyScope` methods
- For application-specific settings (if needed in the future):
  - Settings specific to cycod would use the prefix `cycod.` in dot notation
  - Settings specific to cycodt would use the prefix `cycodt.` in dot notation
  - Currently, there are no known settings that are unique to either application
  - Note that cycodt will only read configuration settings and has no ability to modify them
- In **KnownSettings.cs**:
  - Update environment variable prefixes:
    - "CHATX_PREFERRED_PROVIDER" → "CYCODEV_PREFERRED_PROVIDER"
    - "CHATX_AUTO_SAVE_CHAT_HISTORY" → "CYCODEV_AUTO_SAVE_CHAT_HISTORY"
    - "CHATX_AUTO_SAVE_TRAJECTORY" → "CYCODEV_AUTO_SAVE_TRAJECTORY"
    - "CHATX_CHAT_COMPLETION_TIMEOUT" → "CYCODEV_CHAT_COMPLETION_TIMEOUT"
    - Update all other environment variables with "CHATX_" prefix

## Command Line Infrastructure

The command line infrastructure provides the foundation for parsing and handling commands.

### Command Line Components to Move
- **Command.cs** - Base class for all commands
- **CommandLineException.cs** - Exception handling for command line
- **ForEachVarHelpers.cs** - Variable handling helpers
- **ForEachVariable.cs** - Variable representation
- **BaseCommand.cs** - Common base command functionality
- **VersionCommand.cs** - Version command implementation
- **VersionInfo.cs** - Version information handling

### Command Line Parsing Refactoring
The **CommandLineOptions.cs** file requires significant refactoring:

1. Create a base **CommandLineOptionsBase.cs** class in the common library with:
   - Core argument parsing logic
   - Common option handling
   - Extension points for application-specific command registration

2. Create application-specific derived classes:
   - **src/cycod/CommandLineOptions.cs** - Registers all non-test commands
   - **src/cycodt/CommandLineOptions.cs** - Registers test commands with simplified names

3. The base class should include:
   - Methods for basic command detection (`TryGetCommand`)
   - Option parsing helpers
   - Error handling infrastructure
   
4. In both derived classes, implement:
   - `RegisterCommands()` method to register application-specific commands
   - Command name mapping appropriate for each application

5. For error handling strategy:
   - Both applications will use the same error handling strategy as currently implemented
   - Exceptions are typically caught at the top level in Program.cs and displayed to the user
   - No changes to the fundamental error handling approach are needed in the refactoring

## Note on Test Framework

The test framework components should be placed solely in the cycodt project (src/cycodt) and NOT in the common library, as they are only needed by the test application.

### Test Framework Architecture
- The entire **TestFramework/** directory will be moved to src/cycodt project, including:
  - **YamlTestFramework.cs** - Main test framework
  - **YamlTestCaseParser.cs** - Test case parsing
  - **YamlTestCaseRunner.cs** - Test execution
  - **YamlTestFrameworkConsoleHost.cs** - Console hosting
  - **Reporters/** - Test reporting
  - And other supporting files

### Test Framework Updates Required
1. In **YamlTestFramework.cs**:
   - Change `public const string YamlDefaultTagsFileName = $"{Program.Name}-tests-default-tags.yaml";`
   - Update to `public const string YamlDefaultTagsFileName = "cycod-tests-default-tags.yaml";`

2. In **YamlTestCaseRunner.cs**:
   - Replace hardcoded "chatx" reference with "cycod"
   ```csharp
   // In ExpectGptOutcome method, replace:
   // var startProcess = FindCacheCli("chatx");
   // with:
   // var startProcess = FindCacheCli("cycod");
   ```
   
   > Note: Use a fixed string approach rather than a parameterized approach as the test framework will always invoke cycod.

3. Create a new implementation of **TestBaseCommand.cs** in src/cycodt that uses simplified commands:
   ```csharp
   public abstract class TestBaseCommand : Command
   {
      public TestBaseCommand()
      {
         Files = new List<string>();
         Tests = new List<string>();
         Contains = new List<string>();
         Remove = new List<string>();
      }
      
      // Use "cycodt" instead of "chatx test" in all methods while preserving class names
   }
   ```
   
   > Note: Class names should remain as `TestListCommand` and `TestRunCommand`, but the command names should be changed to `list` and `run` respectively. The test commands should be renamed to remove the "test" prefix, but the class names should remain as is for clarity.

## Helper Utilities

Various helper classes are used throughout both applications and should be shared.

### Helper Components to Move
- **FileHelpers.cs** - File operations
- **DirectoryHelpers.cs** - Directory operations
- **ConsoleHelpers.cs** - Console interaction
- **ColorHelpers.cs** - Console color management
- **Colors.cs** - Console color pair storage
- **StringHelpers.cs** - String manipulation
- **TryCatchHelpers.cs** - Exception handling utilities
- **ValueHelpers.cs** - String interpolation and value replacement utilities
- **JsonHelpers.cs** - JSON handling
- **AtFileHelpers.cs** - @ file expansion utilities
- **MarkdownHelpers.cs** - Markdown processing
- **HelpHelpers.cs** - Base help functionality
- **HMACHelper.cs** - Security authentication (HMAC) utilities
- **ExceptionHelpers.cs** - Exception handling
- **AliasDisplayHelpers.cs** - Alias display utilities
- **AliasFileHelpers.cs** - Alias file management
- **CommonDisplayHelpers.cs** - Common display formatting
- **EnvironmentHelpers.cs** - Environment variable handling and configuration integration
- **OS.cs** - Platform detection utilities (Windows, Mac, Linux, Android, Codespaces)
- **EmbeddedFileHelpers.cs** - Helpers for accessing embedded resources

### Helper Utilities Updates Required
- In **AliasDisplayHelpers.cs**:
  - Change `CommonDisplayHelpers.DisplaySavedFiles(filesSaved, $"{Program.Name} [...] --{{name}}");`
  - Update to `CommonDisplayHelpers.DisplaySavedFiles(filesSaved, $"cycod [...] --{{name}}");`
- In **HelpHelpers.cs**:
  - Add `appName` parameter to relevant methods
  - Modify `DisplayHelpTopic` method:
  ```csharp
  public static void DisplayHelpTopic(string topic, bool expandTopics = false, bool includeTestCommands = true, string appName = "cycod")
  {
      // Use appName parameter instead of Program.Name
      // Filter test topics based on includeTestCommands parameter
  }
  ```
- Update **VersionInfo.cs** to provide application-specific versioning:
  ```csharp
  public static string GetVersion(bool isCycodtApp = false)
  {
      return isCycodtApp ? "cycodt v1.0.0" : "cycod v1.0.0";
  }
  ```

## Cycod-Specific Components

The following components will be kept in the src/cycod project as they are only needed by the main application:

### Function Calling Tools
- **DateAndTimeHelperFunctions.cs** - Date and time utility functions
- **CodeExplorationHelperFunctions.cs** - Code exploration functions
- **MdxCliWrapper.cs** - Utility for executing MDX CLI commands and processing their output
- **ShellCommandToolHelperFunctions.cs** - Utilities for executing commands in shells
- **StrReplaceEditorHelperFunctions.cs** - File manipulation and editing utilities
- **ThinkingToolHelperFunction.cs** - Thinking tool functionality

### Shell Helpers
- **BashShellSession.cs** - Bash shell implementation
- **ShellSession.cs** - Base class for shell sessions
- **CmdShellSession.cs** - Windows CMD shell implementation
- **PowerShellSession.cs** - PowerShell implementation
- Other shell session implementations

## Templates

The Templates directory contains utility classes for parsing and evaluating expressions.

### Template Components to Move
- **ExpressionCalculator.cs** - Mathematical expression parser and evaluator with support for variables, constants, and functions
- **INamedValues.cs** - Interface for classes that provide named values for template substitution
- **TemplateHelpers.cs** - Utility for processing templates with conditions and variable interpolation
- **TemplateVariables.cs** - Implementation of INamedValues that stores template variables and integrates with configuration system

## HTTP Client Components

While most HTTP client functionality is application-specific, some base components may be useful in both applications.

### HTTP Components to Move
- **HttpClientHelpers.cs** - If present, basic HTTP client utilities
- Base policy interfaces and implementations that might be used by both apps

### HTTP Components to Keep in src/cycod
- **CustomHeaderPolicy.cs**
- **CustomJsonPropertyRemovalPolicy.cs**
- **FixNullFunctionArgsPolicy.cs**
- **LogTrafficEventPolicy.cs**
- **TrafficEventPolicy.cs**

## Extension Points

To ensure flexibility, we'll need to design extension points in the common library that allow each application to customize behavior while sharing core functionality.

### Key Extension Points
1. **Command Registration** - Allow applications to register their specific commands
2. **Help System** - Allow applications to provide custom help content
3. **Command Line Parsing** - Support application-specific command line parsing

### Extension Point Implementations

#### Command Registration
```csharp
// In common library
public interface ICommandRegistration
{
    void RegisterCommands(CommandRegistry registry);
}

public class CommandRegistry
{
    private Dictionary<string, Func<Command>> _commandFactories = new();
    
    public void RegisterCommand(string name, Func<Command> factory)
    {
        _commandFactories[name] = factory;
    }
    
    public bool TryGetCommand(string name, out Func<Command> factory)
    {
        return _commandFactories.TryGetValue(name, out factory);
    }
}
```

#### Help System
```csharp
// In common library
public interface IHelpSystem
{
    void RegisterHelpTopics(HelpRegistry registry);
    bool ShouldShowTopic(string topicName);
}

public class HelpRegistry
{
    private Dictionary<string, string> _helpTopics = new();
    
    public void RegisterHelpTopic(string name, string content)
    {
        _helpTopics[name] = content;
    }
    
    public bool TryGetHelpTopic(string name, out string content)
    {
        return _helpTopics.TryGetValue(name, out content);
    }
}
```

## Code Organization Principles

When moving code to the src/common library:

1. Minimize dependencies between components
2. Use interfaces to define contracts between components
3. Keep application-specific logic out of the common library
4. Ensure proper namespace organization
5. Consider backward compatibility for future updates
6. Search for and update all hardcoded "chatx" references
7. Ensure environment variable prefixes are consistently updated