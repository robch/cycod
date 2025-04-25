# Implementation Steps

This document outlines the step-by-step process to implement the refactoring of "chatx" into "cycod" and "cycodt".

## Phase 1: Setup and Structure

### Step 1: Create Solution and Projects

1. Create new solution `CycoDevSolution.sln`
2. Create the three projects:
   - `CycoDev.Common` (Library)
   - `CycoDev` (Executable)
   - `CycoDevTest` (Executable)
3. Set up project references (both executables depend on the common library)
4. Configure output names in project files (.csproj)
   - `cycod.exe` for CycoDev
   - `cycodt.exe` for CycoDevTest

### Step 2: Create Initial Project Files

Create the basic project files (.csproj) for each project as outlined in [Project Structure](project-structure.md).

### Step 3: Create Test Project

1. Create the `CycoDev.Tests` project for unit tests
2. Configure project references to all other projects
3. Set up MSTest with necessary packages
4. Create folder structure:
   - CommonTests/ - For tests targeting the common library
   - CycoDevTests/ - For tests targeting the main application
   - TestHelpers/ - For test utilities and mocks
5. Migrate GlobalUsings.cs and any existing test files

## Phase 2: Move Shared Code to Common Library

### Step 1: Move Configuration System

1. Create the Configuration directory in CycoDev.Common
2. Move configuration-related files:
   - `ConfigFileScope.cs`
   - `ConfigFile.cs`
   - `ConfigFileHelpers.cs`
   - `ConfigStore.cs`
   - `ConfigValue.cs`
   - `ConfigSource.cs`
   - `ScopeFileHelpers.cs`
   - `IniConfigFile.cs`
   - `YamlConfigFile.cs`
   - `ConfigDisplayHelpers.cs`
   - `ProfileFileHelpers.cs`
   - `KnownSettingsCLIParser.cs`
3. Update configuration files to use a shared configuration directory:
   - In `ConfigFileHelpers.cs`:
     - Change `private const string CONFIG_DIR_NAME = $".{Program.Name}";` to use a fixed name
     - Use `private const string CONFIG_DIR_NAME = ".cycod";` to ensure both applications share configuration
   - In `ScopeFileHelpers.cs`:
     - Update occurrences of `var appDirName = $".{Program.Name}";` to use the fixed name
     - Change to `var appDirName = ".cycod";` in both `FindFileInAnyScope` and `FindDirectoryInAnyScope` methods


### Step 2: Move Command Line Infrastructure

1. Create the CommandLine directory in CycoDev.Common
2. Move command line-related files:
   - `Command.cs`
   - `CommandLineException.cs`
   - `KnownSettings.cs`
   - Base command line parsing components
3. Update environment variable prefixes in KnownSettings.cs:
   - Change "CHATX_PREFERRED_PROVIDER" to "CYCODEV_PREFERRED_PROVIDER"
   - Change "CHATX_AUTO_SAVE_CHAT_HISTORY" to "CYCODEV_AUTO_SAVE_CHAT_HISTORY"
   - Change "CHATX_AUTO_SAVE_TRAJECTORY" to "CYCODEV_AUTO_SAVE_TRAJECTORY"
   - Change "CHATX_CHAT_COMPLETION_TIMEOUT" to "CYCODEV_CHAT_COMPLETION_TIMEOUT"


### Step 3: Refactor CommandLineOptions.cs

1. Create a base `CommandLineOptionsBase` class in the common library:
   ```csharp
   public abstract class CommandLineOptionsBase
   {
       // Common properties
       public List<Command> Commands { get; } = new List<Command>();
       public List<string> UnknownArgs { get; } = new List<string>();
       
       // Abstract method for registering commands
       protected abstract void RegisterCommands(CommandRegistry registry);
       
       // Common parsing logic
       public bool Parse(string[] args, out CommandLineException exception);
       
       // Helper methods for parsing that are used by both applications
       protected bool TryParseCommonOptions(string arg, ref int i, string[] args);
   }
   ```

2. Create a `CommandRegistry` class in the common library:
   ```csharp
   public class CommandRegistry
   {
       private Dictionary<string, Func<Command>> _commandFactories = new();
       
       // Register a command with its factory method
       public void RegisterCommand(string name, Func<Command> factory);
       
       // Try to get a command factory by name (with prefix matching)
       public bool TryGetCommand(string name, out Func<Command> factory);
       
       // Get all registered command names
       public IEnumerable<string> GetRegisteredCommandNames();
   }
   ```

3. Create specialized CommandLineOptions classes for each application:

   a. For CycoDev:
   ```csharp
   // In CycoDev/CommandLineOptions.cs
   public class CommandLineOptions : CommandLineOptionsBase
   {
       protected override void RegisterCommands(CommandRegistry registry)
       {
           // Register all non-test commands
           registry.RegisterCommand("chat", () => new ChatCommand());
           registry.RegisterCommand("config list", () => new ConfigListCommand());
           // Register all other CycoDev-specific commands
       }
       
       // Override parsing method to handle CycoDev-specific options
       protected override bool TryParseCommonOptions(string arg, ref int i, string[] args)
       {
           // Handle CycoDev-specific options like --model
       }
   }
   ```

   b. For CycoDevTest:
   ```csharp
   // In CycoDevTest/CommandLineOptions.cs
   public class CommandLineOptions : CommandLineOptionsBase
   {
       protected override void RegisterCommands(CommandRegistry registry)
       {
           // Register test commands with simplified names but keeping original class names
           registry.RegisterCommand("list", () => new TestListCommand()); // Register with simple name but using TestListCommand class
           registry.RegisterCommand("run", () => new TestRunCommand()); // Register with simple name but using TestRunCommand class
           registry.RegisterCommand("help", () => new HelpCommand());
       }
       
       // Override parsing method to handle CycoDevTest-specific options
       protected override bool TryParseCommonOptions(string arg, ref int i, string[] args)
       {
           // Try base implementation first
           if (base.TryParseCommonOptions(arg, ref i, args))
               return true;
               
           // Handle CycoDevTest-specific options like --format
           if (arg == "--format")
           {
               // Process format option
               return true;
           }
           
           return false;
       }
   }
   ```

4. Create helper methods for command execution in the common library:
   ```csharp
   public static class CommandExecutionHelpers
   {
       // Maintain the same error handling approach as in the original implementation
       // Exceptions are typically caught at the top level in Program.cs
       public static async Task ExecuteCommands(IEnumerable<Command> commands, bool isInteractive);
   }
   ```

5. Update Program.cs files to use the new command line parsing architecture:
   - In CycoDev's Program.cs, use the CycoDev-specific CommandLineOptions class
   - In CycoDevTest's Program.cs, use the CycoDevTest-specific CommandLineOptions class


### Step 4: Note About Test Framework

The TestFramework will be handled separately in the CycoDevTest project implementation, not in the common library, as it's only needed by the test application.

### Step 5: Include FunctionCallingTools and Shell Helpers in CycoDev Project

1. Create the FunctionCallingTools directory in CycoDev (not in Common library)
2. Move function calling tool files to CycoDev project:
   - `CodeExplorationHelperFunctions.cs`
   - `DateAndTimeHelperFunctions.cs`
   - `MdxCliWrapper.cs`
   - `ShellCommandToolHelperFunctions.cs`
   - `StrReplaceEditorHelperFunctions.cs`
   - `ThinkingToolHelperFunction.cs`
   - Any other helper function classes
3. Create the ShellHelpers directory in CycoDev (not in Common library)
4. Move shell helper classes to CycoDev project:
   - `BashShellSession.cs`
   - Other shell session implementation files
   - The ShellSession base class

### Step 6: Move Helper Utilities

1. Create the Helpers directory in CycoDev.Common
2. Move shared helper classes:
   - `FileHelpers.cs`
   - `DirectoryHelpers.cs`
   - `ConsoleHelpers.cs`
   - `ColorHelpers.cs`
   - `Colors.cs`
   - `AliasDisplayHelpers.cs`
   - `AliasFileHelpers.cs`
   - `AtFileHelpers.cs`
   - `CommonDisplayHelpers.cs`
   - `EmbeddedFileHelpers.cs`
   - `EnvironmentHelpers.cs`
   - `HelpHelpers.cs`
   - `HMACHelper.cs`
   - `MarkdownHelpers.cs`
   - `OS.cs`
   - `StringHelpers.cs`
   - `TryCatchHelpers.cs`
   - `ValueHelpers.cs`
   - Other helper classes needed by both executables
3. Update references to Program.Name in helper classes:
   - In `AliasDisplayHelpers.cs`:
     - Change `CommonDisplayHelpers.DisplaySavedFiles(filesSaved, $"{Program.Name} [...] --{{name}}");` to use "cycod"
     - Replace with `CommonDisplayHelpers.DisplaySavedFiles(filesSaved, $"cycod [...] --{{name}}");`
   - In `HelpHelpers.cs`:
     - Add `appName` parameter to relevant methods
     - Modify `DisplayHelpTopic` method to support filtering test commands:
     ```csharp
     public static void DisplayHelpTopic(string topic, bool expandTopics = false, bool includeTestCommands = true, string appName = "cycod")
     {
         // Use appName parameter instead of Program.Name
         // Filter test topics based on includeTestCommands parameter
     }
     ```

### Step 7: Move Version Information

1. Move `VersionCommand.cs` to CycoDev.Common/CommandLine/Commands
2. Move `VersionInfo.cs` (or equivalent) to CycoDev.Common/Helpers
3. Update VersionInfo to provide application-specific versioning:
   ```csharp
   public static string GetVersion(bool isCycoDevTest = false)
   {
       return isCycoDevTest ? "CycoDevTest v1.0.0" : "CycoDev v1.0.0";
   }
   ```
### Step 8: Update HTTP Client Related Files

1. Identify which HTTP client related files should be moved to common vs. kept in CycoDev:
   - Move HTTP client components needed by both apps to CycoDev.Common/Helpers
   - Keep API-specific HTTP clients in CycoDev project (e.g., chat-specific clients)

### Step 9: Move Templates Directory

1. Create the Templates directory in CycoDev.Common
2. Move templates-related files:
   - `ExpressionCalculator.cs`
   - `INamedValues.cs`
   - `TemplateHelpers.cs`
   - `TemplateVariables.cs`
   - Any other template-related utility classes
3. Update namespace references as needed

## Phase 3: Implement CycoDev (Main Application)

### Step 1: Create Command Structure

1. Create CommandLineCommands directory
2. Copy all non-test commands from the original project with their complete directory structures:
   - **Full directories to copy:**
     - FunctionCalling directory with all its files and subdirectories
     - FunctionCallingTools directory with all helper functions
     - ShellHelpers directory with all shell session implementations
     - McpHelpers directory with all its files including `IMcpServerConfigItem.cs` and related files
     - SlashCommands directory with all handlers including `SlashMdxCommandHandler.cs` and `SlashPromptCommandHandler.cs`
   
   - **Specific files to copy:**
     - **Chat command and related files:**
       - `ChatCommand.cs`
       - `ChatClientFactory.cs`
       - `ChatHistoryFileHelpers.cs`
       - `ChatHistoryDefaults.cs`
       - `ChatMessageHelpers.cs`
       - `TrajectoryFile.cs`
       - `TrajectoryFormatter.cs`
     
     - **HTTP client-specific files:**
       - `CustomHeaderPolicy.cs`
       - `CustomJsonPropertyRemovalPolicy.cs`
       - `FixNullFunctionArgsPolicy.cs`
       - `LogTrafficEventPolicy.cs`
       - `TrafficEventPolicy.cs`
     
     - **Config commands:**
       - `ConfigGetCommand.cs`
       - `ConfigListCommand.cs`
       - `ConfigSetCommand.cs`
       - `ConfigDeleteCommand.cs`
     
     - **GitHub commands:**
       - `GitHubCopilotLoginCommand.cs`
       - `GitHubCopilotHelper.cs`
     
     - **Prompt commands and helpers:**
       - `PromptBaseCommand.cs`
       - `PromptCreateCommand.cs`
       - `PromptDeleteCommand.cs`
       - `PromptGetCommand.cs`
       - `PromptListCommand.cs`
       - `PromptDisplayHelpers.cs`
       - `PromptFileHelpers.cs`
     
     - **Alias commands:**
       - `AliasCreateCommand.cs`
       - `AliasDeleteCommand.cs`
       - `AliasListCommand.cs`
     
     - **Version command:**
       - `VersionCommand.cs`
3. Create a specialized `HelpCommand.cs` that:
   - Extends the base `Command` class
   - Implements the `Execute()` method to filter out test commands
   - Uses `HelpHelpers.DisplayHelpTopic(HelpTopic, false, includeTestCommands: false)`

### Step 2: Create Program.cs

1. Create a modified version of Program.cs that:
   - Uses the name "cycod" instead of "chatx"
   - Includes only non-test commands
   - Contains appropriate version and banner info

### Step 3: Create Command Line Parsing

1. Create specialized command line parser for CycoDev
2. Include logic to parse all commands except test commands

### Step 4: Copy and Update Assets

1. Create assets directory
2. Copy help files and other assets needed for the main application
   - Note: Help content for cycod should be completely independent from cycodt
   - Each CLI will have its own copy of the help files in its respective assets/help/ folder
3. Include common help files adapted specifically for cycod:
   - examples.txt
   - help.txt
   - options.txt
   - usage.txt
4. Update all help file content to replace "chatx" with "cycod":
   - Update command names in titles (e.g., "CHATX ALIAS DELETE" to "CYCOD ALIAS DELETE")
   - Update command syntax in usage sections
   - Update examples to use the new executable name
   - Preserve formatting and content structure during updates
5. Special handling for `usage.txt`:
   - Update to replace "CHATX" with "CYCOD" throughout
   - Remove test-related commands and examples
   - Update all command usage examples to use "cycod" instead of "chatx"
   - Update SEE ALSO section to remove test-related references

## Phase 4: Implement CycoDevTest (Test Application)

### Step 1: Move Test Framework

1. Create TestFramework directory in the CycoDevTest project
2. Move all test framework files from the original TestFramework directory, including:
   - **YamlTestFramework.cs** - Main test framework
   - **YamlTestCaseParser.cs** - Test case parsing
   - **YamlTestCaseRunner.cs** - Test execution
   - **YamlTestFrameworkConsoleHost.cs** - Console hosting
   - And other supporting files
3. Update YamlTestFramework.cs:
   ```csharp
   // Change:
   // public const string YamlDefaultTagsFileName = $"{Program.Name}-tests-default-tags.yaml";
   // to:
   public const string YamlDefaultTagsFileName = "cycod-tests-default-tags.yaml";
   // Note: Changed from "cycodev-tests-default-tags.yaml" to match the shortened configuration directory name
   ```
4. Update YamlTestCaseRunner.cs:
   ```csharp
   // Use a fixed string approach for CLI references, not a parameterized approach
   // In ExpectGptOutcome method (or any similar method), replace:
   // var startProcess = FindCacheCli("chatx");
   // with:
   // var startProcess = FindCacheCli("cycod");
   ```

### Step 2: Create Command Structure

1. Create CommandLineCommands directory
2. Modify test commands to work without the "test" prefix in command names but keeping their original class names:
   - Keep `TestListCommand.cs` but modify command name to "list" instead of "test list"
   - Keep `TestRunCommand.cs` but modify command name to "run" instead of "test run"
3. Update the existing TestBaseCommand class to work with the new command structure:
   ```csharp
   public abstract class TestBaseCommand : Command
   {
       // Keep class name as TestBaseCommand for clarity
       
       public TestBaseCommand()
       {
           Files = new List<string>();
           Tests = new List<string>();
           Contains = new List<string>();
           Remove = new List<string>();
       }
       
       // Methods updated to use "cycodt" instead of "chatx test"
   }
   ```
4. Create a specialized `HelpCommand.cs` that:
   - Extends the base `Command` class
   - Implements the `Execute()` method to show only test commands
   - Uses `HelpHelpers.DisplayHelpTopic(HelpTopic, false, includeTestCommands: true)`

### Step 2: Create Program.cs

1. Create a simplified Program.cs that:
   - Uses the name "cycodt" instead of "chatx"
   - Only processes test-related commands
   - Has appropriate version and banner info

### Step 3: Create Command Line Parsing

1. Create specialized command line parser for CycoDevTest
2. Implement parsing that treats "list" and "run" as primary commands

### Step 4: Create and Update Help Content

1. Create assets directory with help files
   - Note: Help content for cycodt should be completely independent from cycod
   - Each CLI will have its own copy of the help files in its respective assets/help/ folder
2. Include common help files adapted specifically for cycodt:
   - examples.txt
   - help.txt
   - options.txt
   - usage.txt
3. Rename test-specific help files to remove redundant "test" prefix and move to cycodt's assets/help/ folder:
   - "test examples.txt" → "examples.txt"
   - "test list.txt" → "list.txt"
   - "test run.txt" → "run.txt"
   - "test.txt" → "help.txt" or "overview.txt"
4. Update all test-related help files to:
   - Replace "chatx" with "cycodt" in all text
   - Remove "test" prefix from commands (e.g., change "chatx test list" to "cycodt list")
   - Update title headers (e.g., "CHATX TEST EXAMPLES" → "CYCODT EXAMPLES")
   - Update examples to reflect the new command structure
   - Ensure all command syntax sections show the simplified command format
   - Update "SEE ALSO" sections to reference the new command structure
5. Special handling for `usage.txt`:
   - Create a new simplified `usage.txt` focused solely on test functionality
   - Include examples that demonstrate the simplified command structure (e.g., "cycodt list" instead of "chatx test list")
   - Include appropriate SEE ALSO references to other test-related help topics

## Phase 5: Refine Shared Components

### Step 1: Comprehensive Global Search for String References

1. Create a tracking spreadsheet or document for "chatx" references:
   - Columns: File Path, Line Number, Context, Reference Type, Replacement Needed, Status
   - Use this to track progress and ensure all references are addressed

2. Perform multiple codebase-wide searches for "chatx" references using different tools and targeting various file types:
   ```bash
   # Search in C# files
   grep -r "chatx" --include="*.cs" . > chatx_references_cs.txt
   
   # Search in text and configuration files
   grep -r "chatx" --include="*.txt" --include="*.md" --include="*.json" --include="*.yaml" --include="*.yml" . > chatx_references_txt.txt
   
   # Search in project and solution files
   grep -r "chatx" --include="*.csproj" --include="*.sln" . > chatx_references_proj.txt
   
   # Search in resource files and other types
   grep -r "chatx" --include="*.resx" --include="*.config" --include="*.xml" . > chatx_references_other.txt
   
   # Search all files (to catch anything missed by the above)
   grep -r "chatx" --exclude-dir=".git" --exclude-dir="bin" --exclude-dir="obj" . > chatx_references_all.txt
   ```

3. On Windows systems, use PowerShell as an alternative search method:
   ```powershell
   # PowerShell recursive search (case insensitive)
   Get-ChildItem -Path . -Recurse -File | Select-String -Pattern "chatx" -CaseSensitive | Select Path,LineNumber,Line | Export-Csv -Path chatx_references.csv -NoTypeInformation
   ```

4. Systematically categorize each reference found in the tracking document:
   - **User Interface**: Messages directly shown to users
   - **Command-Line Arguments**: References in command parsing
   - **Help Text**: Text in help files
   - **File Paths**: Used for creating or reading files
   - **Configuration Keys**: Keys in configuration settings
   - **Environment Variables**: Names of environment variables
   - **Internal Identifiers**: Variable names, method names, comments
   - **API References**: References in API calls or URLs
   - **Tests**: References in test cases and assertions

5. Update hardcoded string references based on type and context:
   - **User Interface in CycoDev**: Change "chatx" to "cycod"
   - **User Interface in CycoDevTest**: Change "chatx" to "cycodt"
   - **Command-Line in CycoDev**: Change "chatx" to "cycod" 
   - **Command-Line in CycoDevTest**: Change "chatx" to "cycodt" (and remove "test" prefix where appropriate)
   - **Help Text in CycoDev**: Change "chatx" to "cycod"
   - **Help Text in CycoDevTest**: Change "chatx" to "cycodt" and remove "test" prefix
   - **File Paths**: Update to use ".cycod" where appropriate
   - **Configuration Keys**: Update keys in configuration stores
   - **Environment Variables**: Change "CHATX_" to "CYCODEV_"
   - **Internal Identifiers**: Update or parameterize as appropriate
   - **Comments**: Update to reflect new command names and structure

6. Pay special attention to files known to have critical hardcoded references:
   - **GitHubCopilotLoginCommand.cs**: Update success messages and command references
   - **All Command classes**: Check output messages mentioning "chatx"
   - **PromptDisplayHelpers.cs**: Check all display string references
   - **YamlTestCaseRunner.cs**: Check for hardcoded CLI name references
   - **TestBaseCommand.cs**: Ensure "chatx test" references are parameterized
   - **All error messages**: Check for app name in error text
   - **All prompt templates**: Check for "chatx" references in template files
   - **All log messages**: Check diagnostics output for app name references
   - **Config files**: Check for app name in config paths and keys

7. Create unit tests to verify string replacements haven't broken functionality:
   - Test that help commands display correctly with new app names
   - Test that file paths are correctly formed and accessible
   - Test that environment variables are correctly read

8. Use methods to get the executable name dynamically where appropriate:
   ```csharp
   // Instead of hardcoding the executable name, use:
   string exeName = Process.GetCurrentProcess().ProcessName;
   Console.WriteLine($"Run {exeName} help to see available commands");
   ```

9. After making all replacements, perform a verification search:
   ```bash
   # Verify no "chatx" references remain (except in specific allowed cases like comments about the refactoring)
   grep -r "chatx" --include="*.cs" --include="*.txt" --include="*.md" --exclude-dir=".git" --exclude-dir="bin" --exclude-dir="obj" .
   ```

10. Document any instances where "chatx" references are intentionally preserved (e.g., in comments explaining the refactoring, or in compatibility code).

### Step 1.5: Organize Test Resources

1. Create a structured test organization:
   - Create a top level `tests/` folder
   - Add sub-folders:
     - `common/` - For tests targeting shared components (most tests will go here)
     - `cycod/` - For tests specific to the main application (small number)
     - `cycodt/` - For tests specific to the test application (likely none currently)
   
2. For each test directory, create appropriate sub-project specific folders grouping tests together
   - Maintain a similar structure as the current test organization for consistency

3. Update test paths in test files to reflect the new structure
   - Update paths in test data references
   - Ensure test resources can be located correctly at runtime

### Step 2: Refine Command Line Infrastructure

1. Create extension points or abstract base classes in the common library
2. Ensure the command registration system works for both applications

### Step 3: Refine Help System

1. Update the help system to work with both applications
2. Create a mechanism to filter help content based on the application

### Step 4: Refine Command Base Classes

1. Ensure command base classes are appropriate for both applications
2. Create specialized base classes if needed

## Phase 6: Testing and Verification

### Step 1: Unit Testing

1. Update unit tests in the CycoDev.Tests project:
   - Adjust namespaces to match the new project structure
   - Update any references to reflect new class locations
   - Update environment variable prefixes from "CHATX_" to "CYCODEV_" in test files, specifically:
     - In ConfigStoreTests.cs: Update test cases that use environment variables prefixed with "CHATX_"
     - In TestFileHelpers.cs: Check for any environment variable references
     - In YamlTestFrameworkTests.cs: Update environment variable references
     - Search for all occurrences of "CHATX_" across test files to ensure complete coverage
   - Add tests for refactored components with changed functionality
   - Move GitHubLoginCommandTests.cs to CycoDev.Tests/CycoDevTests/ directory
   - Update namespace references to match the new location of GitHubCopilotHelper
   - Update ConfigStoreTests.cs to use the new environment variable prefixes for ConfigStore initialization tests
   - Preserve the subdirectory structure for tests (e.g., keep ExpressionCalculator tests in a subdirectory)
2. Migrate any tests from the original project that weren't moved during setup
3. Ensure all unit tests pass for all projects
4. Add test coverage for any new extension points or interfaces

### Step 2: Integration Testing

1. Test CycoDev functionality
   - Verify all non-test commands work as expected
   - Verify help system displays appropriate commands

2. Test CycoDevTest functionality
   - Verify test commands work without the "test" prefix
   - Verify help system displays only test-related content

### Step 3: Configuration Testing

1. Test that configuration is properly shared between applications
2. Verify configuration scoping works as expected

## Phase 7: Documentation and Finalization

### Step 1: Update Documentation

1. Update README.md and other documentation to reflect the new structure
2. Document the use of both applications
3. Include examples of using both tools

### Step 2: Review and Refine

1. Review the implementation for any edge cases or issues
2. Make any final refinements to the codebase

### Step 3: Finalize Build System

1. Configure the build system to build both executables
2. Ensure proper versioning for both applications

## Timeline

The implementation can be broken down into these estimated timeframes:

1. Phase 1 (Setup): 1-2 days
2. Phase 2 (Shared Code): 2-3 days
3. Phase 3 (CycoDev): 2-3 days
4. Phase 4 (CycoDevTest): 2 days
5. Phase 5 (Refinement): 2-3 days
6. Phase 6 (Testing): 2-3 days
7. Phase 7 (Documentation): 1-2 days

Total estimated time: 12-18 days