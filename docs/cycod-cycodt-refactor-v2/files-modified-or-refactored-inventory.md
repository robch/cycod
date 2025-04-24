# Modified and Duplicated File Inventory

This document identifies files that need significant modification or duplication with application-specific changes as part of the refactoring project. These are distinct from files that are simply moved or brand new files.

## Core Files Requiring Major Refactoring

### Program.cs
- **Original**: src/Program.cs
- **New Versions**: 
  - src/cycod/Program.cs
  - src/cycodt/Program.cs
- **Modifications**:
  - Both versions need updated name/version display
  - cycod version retains all command dispatch logic except test commands
  - cycodt version uses simplified command structure focused on test commands
  - Command dispatching logic needs to be adapted for each application
  - All "chatx" references need to be replaced with appropriate executable name
  - Error handling strategy remains the same but adapted to each application

### CommandLineOptions.cs
- **Original**: src/CommandLineOptions.cs
- **New Versions**: 
  - src/common/CommandLine/CommandLineOptionsBase.cs (abstract base class)
  - src/cycod/CommandLineOptions.cs (derived implementation)
  - src/cycodt/CommandLineOptions.cs (derived implementation)
- **Modifications**:
  - Extract common parsing logic to the base class with extension points
  - Implement specialized command registration in derived classes
  - cycod handles all non-test commands with original naming
  - cycodt handles test commands with simplified naming (remove "test" prefix)
  - Each version needs different option handling logic

## Help Files That Need Application-Specific Versions

For each of these files, two versions need to be created - one for each application. Both need "chatx" references replaced with the appropriate application name.

### help/options.txt
- **src/cycod Version**: 
  - Include all options except test-specific ones
  - Update "chatx" references to "cycod"
- **src/cycodt Version**:
  - Focus only on test-specific options 
  - Simplify commands by removing "test" prefix
  - Update "chatx" references to "cycodt"

### help/usage.txt
- **src/cycod Version**:
  - Update "chatx" references to "cycod"
  - Remove test-related examples and commands
- **src/cycodt Version**:
  - Create simplified version with only test commands
  - Rename test commands to remove "test" prefix
  - Update welcome message to indicate test-specific focus

### help/help.txt
- **src/cycod Version**:
  - Update "chatx" references to "cycod"
  - Remove test topic references
- **src/cycodt Version**:
  - Create focused version on test functionality
  - Simplify command references by removing "test" prefix

### help/examples.txt
- **src/cycod Version**:
  - Update "chatx" references to "cycod"
  - Remove test-related examples
- **src/cycodt Version**:
  - Focus solely on test examples
  - Simplify command names by removing "test" prefix

### help/-.txt
- **src/cycod Version**:
  - Update application name references to "cycod"
- **src/cycodt Version**:
  - Update application name references to "cycodt"

## Test Commands Requiring Modification

### TestListCommand.cs
- **Original**: src/CommandLineCommands/TestCommands/TestListCommand.cs
- **New**: src/cycodt/CommandLineCommands/TestListCommand.cs
- **Modifications**:
  - Keep class name as TestListCommand but change command name from "test list" to just "list"
  - Update help references and examples
  - Change command execution logic to use "cycodt" instead of "chatx test"
  - Update constructor to use "cycodt" as command prefix

### TestRunCommand.cs
- **Original**: src/CommandLineCommands/TestCommands/TestRunCommand.cs
- **New**: src/cycodt/CommandLineCommands/TestRunCommand.cs
- **Modifications**:
  - Keep class name as TestRunCommand but change command name from "test run" to just "run"
  - Update help references and examples
  - Change command execution logic to use "cycodt" instead of "chatx test"
  - Update constructor to use "cycodt" as command prefix

### TestBaseCommand.cs
- **Original**: src/CommandLineCommands/TestCommands/TestBaseCommand.cs
- **New**: src/cycodt/CommandLineCommands/TestBaseCommand.cs
- **Modifications**:
  - Update to use "cycodt" instead of "chatx test" in all methods
  - Update command name handling to remove "test" prefix
  - Retain class name for clarity

## Other Files Requiring Significant Modification

### HelpCommand.cs
- **Original**: src/CommandLineCommands/HelpCommand.cs
- **New Versions**:
  - src/cycod/CommandLineCommands/HelpCommand.cs
  - src/cycodt/CommandLineCommands/HelpCommand.cs
- **Modifications**:
  - cycod version: Filter out test topics using includeTestCommands parameter
  - cycodt version: Show only test topics using includeTestCommands parameter
  - Update to use appropriate application name
  - Each version contains application-specific display logic

### VersionCommand.cs
- **Original**: src/CommandLineCommands/VersionCommand.cs
- **New**: src/common/CommandLine/Commands/VersionCommand.cs
- **Modifications**:
  - Add ability to identify which application is requesting version info
  - Update version display to show appropriate app name and version

### HelpHelpers.cs
- **Original**: src/Helpers/HelpHelpers.cs
- **New**: src/common/Helpers/HelpHelpers.cs
- **Modifications**:
  - Add appName parameter to DisplayHelpTopic method:
    ```csharp
    public static void DisplayHelpTopic(string topic, bool expandTopics = false, 
                                       bool includeTestCommands = true, string appName = "cycod")
    ```
  - Add logic to filter help topics based on includeTestCommands parameter
  - Update to use appName parameter instead of Program.Name

### ConfigFileHelpers.cs
- **Original**: src/Configuration/ConfigFileHelpers.cs
- **New**: src/common/Configuration/ConfigFileHelpers.cs
- **Modifications**:
  - Change `private const string CONFIG_DIR_NAME = $".{Program.Name}";` 
  - To: `private const string CONFIG_DIR_NAME = ".cycod";`

### ScopeFileHelpers.cs
- **Original**: src/Configuration/ScopeFileHelpers.cs
- **New**: src/common/Configuration/ScopeFileHelpers.cs
- **Modifications**:
  - Update occurrences of `var appDirName = $".{Program.Name}";`
  - To: `var appDirName = ".cycod";` in both `FindFileInAnyScope` and `FindDirectoryInAnyScope` methods

### KnownSettings.cs
- **Original**: src/Configuration/KnownSettings.cs
- **New**: src/common/Configuration/KnownSettings.cs
- **Modifications**:
  - Update environment variable prefixes from "CHATX_" to "CYCODEV_":
    - "CHATX_PREFERRED_PROVIDER" → "CYCODEV_PREFERRED_PROVIDER"
    - "CHATX_AUTO_SAVE_CHAT_HISTORY" → "CYCODEV_AUTO_SAVE_CHAT_HISTORY"
    - "CHATX_AUTO_SAVE_TRAJECTORY" → "CYCODEV_AUTO_SAVE_TRAJECTORY"
    - "CHATX_CHAT_COMPLETION_TIMEOUT" → "CYCODEV_CHAT_COMPLETION_TIMEOUT"
    - And all other environment variables with "CHATX_" prefix

### YamlTestFramework.cs
- **Original**: src/TestFramework/YamlTestFramework.cs
- **New**: src/cycodt/TestFramework/YamlTestFramework.cs
- **Modifications**:
  - Change `public const string YamlDefaultTagsFileName = $"{Program.Name}-tests-default-tags.yaml";`
  - To: `public const string YamlDefaultTagsFileName = "cycod-tests-default-tags.yaml";`

### YamlTestCaseRunner.cs
- **Original**: src/TestFramework/YamlTestCaseRunner.cs
- **New**: src/cycodt/TestFramework/YamlTestCaseRunner.cs
- **Modifications**:
  - Replace hardcoded "chatx" reference with "cycod":
    ```csharp
    // In ExpectGptOutcome method, replace:
    // var startProcess = FindCacheCli("chatx");
    // with:
    // var startProcess = FindCacheCli("cycod");
    ```

### AliasDisplayHelpers.cs
- **Original**: src/Helpers/AliasDisplayHelpers.cs
- **New**: src/common/Helpers/AliasDisplayHelpers.cs
- **Modifications**:
  - Change `CommonDisplayHelpers.DisplaySavedFiles(filesSaved, $"{Program.Name} [...] --{{name}}");`
  - To: `CommonDisplayHelpers.DisplaySavedFiles(filesSaved, $"cycod [...] --{{name}}");`

## Unit Tests Requiring Modification

### ConfigStoreTests.cs
- **Original**: src/tests/ConfigStoreTests.cs
- **New**: tests/common/ConfigStoreTests.cs
- **Modifications**:
  - Update environment variable references from "CHATX_" to "CYCODEV_" in test cases
  - Update test initialization that sets/clears environment variables

### Other test files with "CHATX_" environment variable references
- All test files that use environment variables with "CHATX_" prefix need to be updated
- Particularly test files that create test configurations or mock environment variables