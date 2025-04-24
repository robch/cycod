# File-by-File Move Inventory

This document catalogs where each file will be moved during the refactoring process.

## Help Files

### Current Location: src/assets/help

## Files Moving to src/cycod/assets/help/

All non-test related help files will move to the cycod project's help directory.

| Original File | Destination |
|---------------|-------------|
| src/assets/help/-.txt | src/cycod/assets/help/-.txt |
| src/assets/help/alias delete.txt | src/cycod/assets/help/alias delete.txt |
| src/assets/help/alias get.txt | src/cycod/assets/help/alias get.txt |
| src/assets/help/alias list.txt | src/cycod/assets/help/alias list.txt |
| src/assets/help/alias.txt | src/cycod/assets/help/alias.txt |
| src/assets/help/aliases.txt | src/cycod/assets/help/aliases.txt |
| src/assets/help/chat history.txt | src/cycod/assets/help/chat history.txt |
| src/assets/help/config add.txt | src/cycod/assets/help/config add.txt |
| src/assets/help/config clear.txt | src/cycod/assets/help/config clear.txt |
| src/assets/help/config get.txt | src/cycod/assets/help/config get.txt |
| src/assets/help/config list.txt | src/cycod/assets/help/config list.txt |
| src/assets/help/config remove.txt | src/cycod/assets/help/config remove.txt |
| src/assets/help/config set.txt | src/cycod/assets/help/config set.txt |
| src/assets/help/config.txt | src/cycod/assets/help/config.txt |
| src/assets/help/configuration.txt | src/cycod/assets/help/configuration.txt |
| src/assets/help/examples.txt | src/cycod/assets/help/examples.txt |
| src/assets/help/github login.txt | src/cycod/assets/help/github login.txt |
| src/assets/help/github.txt | src/cycod/assets/help/github.txt |
| src/assets/help/help.txt | src/cycod/assets/help/help.txt |
| src/assets/help/mcp add.txt | src/cycod/assets/help/mcp add.txt |
| src/assets/help/mcp get.txt | src/cycod/assets/help/mcp get.txt |
| src/assets/help/mcp list.txt | src/cycod/assets/help/mcp list.txt |
| src/assets/help/mcp remove.txt | src/cycod/assets/help/mcp remove.txt |
| src/assets/help/mcp.txt | src/cycod/assets/help/mcp.txt |
| src/assets/help/options.txt | src/cycod/assets/help/options.txt |
| src/assets/help/prompt create.txt | src/cycod/assets/help/prompt create.txt |
| src/assets/help/prompt delete.txt | src/cycod/assets/help/prompt delete.txt |
| src/assets/help/prompt get.txt | src/cycod/assets/help/prompt get.txt |
| src/assets/help/prompt list.txt | src/cycod/assets/help/prompt list.txt |
| src/assets/help/prompt.txt | src/cycod/assets/help/prompt.txt |
| src/assets/help/prompts.txt | src/cycod/assets/help/prompts.txt |
| src/assets/help/provider.txt | src/cycod/assets/help/provider.txt |
| src/assets/help/slash commands.txt | src/cycod/assets/help/slash commands.txt |
| src/assets/help/usage.txt | src/cycod/assets/help/usage.txt |

## Files Moving to src/cycodt/assets/help/

Test-related help files will be moved to the cycodt project with the "test" prefix removed from the filenames.

| Original File | Destination |
|---------------|-------------|
| src/assets/help/test examples.txt | src/cycodt/assets/help/examples.txt |
| src/assets/help/test list.txt | src/cycodt/assets/help/list.txt |
| src/assets/help/test run.txt | src/cycodt/assets/help/run.txt |
| src/assets/help/test.txt | src/cycodt/assets/help/help.txt |

## Directory Content

The mcp directory and its contents should be moved to src/cycod/assets/help/mcp/ maintaining the same structure.
## Prompt Files

### Current Location: src/assets/prompts

All prompt files will be moved to the cycod project's prompts directory.

| Original File | Destination |
|---------------|-------------|
| *.yaml | src/cycod/assets/prompts/*.yaml |
| *.yml | src/cycod/assets/prompts/*.yml |
| *.md | src/cycod/assets/prompts/*.md |
| *.txt | src/cycod/assets/prompts/*.txt |

All files in the src/assets/prompts directory will be moved to src/cycod/assets/prompts/, maintaining the same structure and filenames. These prompt files are only used by the cycod application, not by the cycodt application.

## Helper Files

### Current Location: src/Helpers

Helper utility classes will be moved to the common library, as they are used by both applications.

| Original File | Destination |
|---------------|-------------|
| src/Helpers/FileHelpers.cs | src/common/Helpers/FileHelpers.cs |
| src/Helpers/DirectoryHelpers.cs | src/common/Helpers/DirectoryHelpers.cs |
| src/Helpers/ConsoleHelpers.cs | src/common/Helpers/ConsoleHelpers.cs |
| src/Helpers/ColorHelpers.cs | src/common/Helpers/ColorHelpers.cs |
| src/Helpers/Colors.cs | src/common/Helpers/Colors.cs |
| src/Helpers/StringHelpers.cs | src/common/Helpers/StringHelpers.cs |
| src/Helpers/TryCatchHelpers.cs | src/common/Helpers/TryCatchHelpers.cs |
| src/Helpers/ValueHelpers.cs | src/common/Helpers/ValueHelpers.cs |
| src/Helpers/JsonHelpers.cs | src/common/Helpers/JsonHelpers.cs |
| src/Helpers/AtFileHelpers.cs | src/common/Helpers/AtFileHelpers.cs |
| src/Helpers/MarkdownHelpers.cs | src/common/Helpers/MarkdownHelpers.cs |
| src/Helpers/HelpHelpers.cs | src/common/Helpers/HelpHelpers.cs |
| src/Helpers/HMACHelper.cs | src/common/Helpers/HMACHelper.cs |
| src/Helpers/ExceptionHelpers.cs | src/common/Helpers/ExceptionHelpers.cs |
| src/Helpers/AliasDisplayHelpers.cs | src/common/Helpers/AliasDisplayHelpers.cs |
| src/Helpers/AliasFileHelpers.cs | src/common/Helpers/AliasFileHelpers.cs |
| src/Helpers/CommonDisplayHelpers.cs | src/common/Helpers/CommonDisplayHelpers.cs |
| src/Helpers/EnvironmentHelpers.cs | src/common/Helpers/EnvironmentHelpers.cs |
| src/Helpers/OS.cs | src/common/Helpers/OS.cs |
| src/Helpers/EmbeddedFileHelpers.cs | src/common/Helpers/EmbeddedFileHelpers.cs |

## FunctionCalling Files

### Current Location: src/FunctionCalling

All function calling files will be moved to the cycod project, maintaining their directory structure.

| Original File | Destination |
|---------------|-------------|
| src/FunctionCalling/FunctionCallDetector.cs | src/cycod/FunctionCalling/FunctionCallDetector.cs |
| src/FunctionCalling/FunctionFactory.cs | src/cycod/FunctionCalling/FunctionFactory.cs |
| src/FunctionCalling/McpFunctionFactory.cs | src/cycod/FunctionCalling/McpFunctionFactory.cs |

## Command Line Commands Files

### Current Location: src/CommandLineCommands

#### Files Moving to src/cycod/CommandLineCommands/

All non-test related command files will be moved to the cycod project, preserving their directory structure:

**AliasCommands Directory:**
| Original File | Destination |
|---------------|-------------|
| src/CommandLineCommands/AliasCommands/AliasBaseCommand.cs | src/cycod/CommandLineCommands/AliasCommands/AliasBaseCommand.cs |
| src/CommandLineCommands/AliasCommands/AliasDeleteCommand.cs | src/cycod/CommandLineCommands/AliasCommands/AliasDeleteCommand.cs |
| src/CommandLineCommands/AliasCommands/AliasGetCommand.cs | src/cycod/CommandLineCommands/AliasCommands/AliasGetCommand.cs |
| src/CommandLineCommands/AliasCommands/AliasListCommand.cs | src/cycod/CommandLineCommands/AliasCommands/AliasListCommand.cs |

**ConfigCommands Directory:**
| Original File | Destination |
|---------------|-------------|
| src/CommandLineCommands/ConfigCommands/ConfigAddCommand.cs | src/cycod/CommandLineCommands/ConfigCommands/ConfigAddCommand.cs |
| src/CommandLineCommands/ConfigCommands/ConfigBaseCommand.cs | src/cycod/CommandLineCommands/ConfigCommands/ConfigBaseCommand.cs |
| src/CommandLineCommands/ConfigCommands/ConfigClearCommand.cs | src/cycod/CommandLineCommands/ConfigCommands/ConfigClearCommand.cs |
| src/CommandLineCommands/ConfigCommands/ConfigGetCommand.cs | src/cycod/CommandLineCommands/ConfigCommands/ConfigGetCommand.cs |
| src/CommandLineCommands/ConfigCommands/ConfigListCommand.cs | src/cycod/CommandLineCommands/ConfigCommands/ConfigListCommand.cs |
| src/CommandLineCommands/ConfigCommands/ConfigRemoveCommand.cs | src/cycod/CommandLineCommands/ConfigCommands/ConfigRemoveCommand.cs |
| src/CommandLineCommands/ConfigCommands/ConfigSetCommand.cs | src/cycod/CommandLineCommands/ConfigCommands/ConfigSetCommand.cs |

**McpCommands Directory:**
| Original File | Destination |
|---------------|-------------|
| src/CommandLineCommands/McpCommands/McpAddCommand.cs | src/cycod/CommandLineCommands/McpCommands/McpAddCommand.cs |
| src/CommandLineCommands/McpCommands/McpBaseCommand.cs | src/cycod/CommandLineCommands/McpCommands/McpBaseCommand.cs |
| src/CommandLineCommands/McpCommands/McpGetCommand.cs | src/cycod/CommandLineCommands/McpCommands/McpGetCommand.cs |
| src/CommandLineCommands/McpCommands/McpListCommand.cs | src/cycod/CommandLineCommands/McpCommands/McpListCommand.cs |
| src/CommandLineCommands/McpCommands/McpRemoveCommand.cs | src/cycod/CommandLineCommands/McpCommands/McpRemoveCommand.cs |

**PromptCommands Directory:**
| Original File | Destination |
|---------------|-------------|
| src/CommandLineCommands/PromptCommands/PromptBaseCommand.cs | src/cycod/CommandLineCommands/PromptCommands/PromptBaseCommand.cs |
| src/CommandLineCommands/PromptCommands/PromptCreateCommand.cs | src/cycod/CommandLineCommands/PromptCommands/PromptCreateCommand.cs |
| src/CommandLineCommands/PromptCommands/PromptDeleteCommand.cs | src/cycod/CommandLineCommands/PromptCommands/PromptDeleteCommand.cs |
| src/CommandLineCommands/PromptCommands/PromptGetCommand.cs | src/cycod/CommandLineCommands/PromptCommands/PromptGetCommand.cs |
| src/CommandLineCommands/PromptCommands/PromptListCommand.cs | src/cycod/CommandLineCommands/PromptCommands/PromptListCommand.cs |

**Root Commands:**
| Original File | Destination |
|---------------|-------------|
| src/CommandLineCommands/ChatCommand.cs | src/cycod/CommandLineCommands/ChatCommand.cs |
| src/CommandLineCommands/GitHubLoginCommand.cs | src/cycod/CommandLineCommands/GitHubLoginCommand.cs |
| src/CommandLineCommands/HelpCommand.cs | src/cycod/CommandLineCommands/HelpCommand.cs |
| src/CommandLineCommands/VersionCommand.cs | src/cycod/CommandLineCommands/VersionCommand.cs |
| src/CommandLineCommands/BaseCommand.cs | src/cycod/CommandLineCommands/BaseCommand.cs |
| src/CommandLineCommands/[any other non-test commands] | src/cycod/CommandLineCommands/[same filename] |

#### Files Moving to src/cycodt/CommandLineCommands/

Test-related command files will be moved to the cycodt project. Note that while the command names will be simplified (no "test" prefix), the class names will remain the same for clarity.

**TestCommands Directory:**
| Original File | Destination |
|---------------|-------------|
| src/CommandLineCommands/TestCommands/TestListCommand.cs | src/cycodt/CommandLineCommands/TestListCommand.cs |
| src/CommandLineCommands/TestCommands/TestRunCommand.cs | src/cycodt/CommandLineCommands/TestRunCommand.cs |
| src/CommandLineCommands/TestCommands/TestBaseCommand.cs | src/cycodt/CommandLineCommands/TestBaseCommand.cs |
| src/CommandLineCommands/TestCommands/[any other test-specific files] | src/cycodt/CommandLineCommands/[same filename] |

**Root Test Commands:**
| Original File | Destination |
|---------------|-------------|
| src/CommandLineCommands/HelpCommand.cs | src/cycodt/CommandLineCommands/HelpCommand.cs (specialized version) |

## McpHelpers Files

### Current Location: src/McpHelpers

All files in the McpHelpers directory will be moved to the cycod project as they are specific to the main application.

| Original File | Destination |
|---------------|-------------|
| src/McpHelpers/IMcpServerConfigItem.cs | src/cycod/McpHelpers/IMcpServerConfigItem.cs |
| src/McpHelpers/McpClientManager.cs | src/cycod/McpHelpers/McpClientManager.cs |
| src/McpHelpers/McpConfigFile.cs | src/cycod/McpHelpers/McpConfigFile.cs |
| src/McpHelpers/McpDisplayHelpers.cs | src/cycod/McpHelpers/McpDisplayHelpers.cs |
| src/McpHelpers/McpFileHelpers.cs | src/cycod/McpHelpers/McpFileHelpers.cs |
| src/McpHelpers/McpServerConfig.cs | src/cycod/McpHelpers/McpServerConfig.cs |
| src/McpHelpers/McpServerConfigItemConverter.cs | src/cycod/McpHelpers/McpServerConfigItemConverter.cs |
| src/McpHelpers/SseServerConfig.cs | src/cycod/McpHelpers/SseServerConfig.cs |
| src/McpHelpers/StdioServerConfig.cs | src/cycod/McpHelpers/StdioServerConfig.cs |

## ShellHelpers Files

### Current Location: src/ShellHelpers

All files in the ShellHelpers directory will be moved to the cycod project as they are specific to the main application.

| Original File | Destination |
|---------------|-------------|
| src/ShellHelpers/BashShellSession.cs | src/cycod/ShellHelpers/BashShellSession.cs |
| src/ShellHelpers/CmdShellSession.cs | src/cycod/ShellHelpers/CmdShellSession.cs |
| src/ShellHelpers/PowershellShellSession.cs | src/cycod/ShellHelpers/PowershellShellSession.cs |
| src/ShellHelpers/ShellSession.cs | src/cycod/ShellHelpers/ShellSession.cs |

## ChatClient Files

### Current Location: src/ChatClient

All files in the ChatClient directory will be moved to the cycod project as they are related to chat functionality.

| Original File | Destination |
|---------------|-------------|
| src/ChatClient/ChatClientFactory.cs | src/cycod/ChatClient/ChatClientFactory.cs |
| src/ChatClient/FunctionCallingChat.cs | src/cycod/ChatClient/FunctionCallingChat.cs |

## FunctionCallingTools Files

### Current Location: src/FunctionCallingTools

All files in the FunctionCallingTools directory will be moved to the cycod project as they are specific to the main application.

| Original File | Destination |
|---------------|-------------|
| src/FunctionCallingTools/DateAndTimeHelperFunctions.cs | src/cycod/FunctionCallingTools/DateAndTimeHelperFunctions.cs |
| src/FunctionCallingTools/CodeExplorationHelperFunctions.cs | src/cycod/FunctionCallingTools/CodeExplorationHelperFunctions.cs |
| src/FunctionCallingTools/MdxCliWrapper.cs | src/cycod/FunctionCallingTools/MdxCliWrapper.cs |
| src/FunctionCallingTools/ShellCommandToolHelperFunctions.cs | src/cycod/FunctionCallingTools/ShellCommandToolHelperFunctions.cs |
| src/FunctionCallingTools/StrReplaceEditorHelperFunctions.cs | src/cycod/FunctionCallingTools/StrReplaceEditorHelperFunctions.cs |
| src/FunctionCallingTools/ThinkingToolHelperFunction.cs | src/cycod/FunctionCallingTools/ThinkingToolHelperFunction.cs |

## SlashCommands Files

### Current Location: src/SlashCommands

All files in the SlashCommands directory will be moved to the cycod project as they are specific to the main application.

| Original File | Destination |
|---------------|-------------|
| src/SlashCommands/SlashMdxCommandHandler.cs | src/cycod/SlashCommands/SlashMdxCommandHandler.cs |
| src/SlashCommands/SlashPromptCommandHandler.cs | src/cycod/SlashCommands/SlashPromptCommandHandler.cs |

## Templates Files

### Current Location: src/Templates

All files in the Templates directory will be moved to the common project as they are used by both applications.

| Original File | Destination |
|---------------|-------------|
| src/Templates/ExpressionCalculator.cs | src/common/Templates/ExpressionCalculator.cs |
| src/Templates/INamedValues.cs | src/common/Templates/INamedValues.cs |
| src/Templates/TemplateHelpers.cs | src/common/Templates/TemplateHelpers.cs |
| src/Templates/TemplateVariables.cs | src/common/Templates/TemplateVariables.cs |

## TestFramework Files

### Current Location: src/TestFramework

All files in the TestFramework directory will be moved to the cycodt project as they are only needed by the test application.

| Original File | Destination |
|---------------|-------------|
| src/TestFramework/IYamlTestFrameworkHost.cs | src/cycodt/TestFramework/IYamlTestFrameworkHost.cs |
| src/TestFramework/IYamlTestFrameworkLogger.cs | src/cycodt/TestFramework/IYamlTestFrameworkLogger.cs |
| src/TestFramework/JunitXmlTestReporter.cs | src/cycodt/TestFramework/JunitXmlTestReporter.cs |
| src/TestFramework/Logger.cs | src/cycodt/TestFramework/Logger.cs |
| src/TestFramework/PathHelpers.cs | src/cycodt/TestFramework/PathHelpers.cs |
| src/TestFramework/PropertyInterpolationHelpers.cs | src/cycodt/TestFramework/PropertyInterpolationHelpers.cs |
| src/TestFramework/README.md | src/cycodt/TestFramework/README.md |
| src/TestFramework/RunnableTestCase.cs | src/cycodt/TestFramework/RunnableTestCase.cs |
| src/TestFramework/RunnableTestCaseItem.cs | src/cycodt/TestFramework/RunnableTestCaseItem.cs |
| src/TestFramework/TestResultHelpers.cs | src/cycodt/TestFramework/TestResultHelpers.cs |
| src/TestFramework/TestRun.cs | src/cycodt/TestFramework/TestRun.cs |
| src/TestFramework/TimeSpanFormatter.cs | src/cycodt/TestFramework/TimeSpanFormatter.cs |
| src/TestFramework/TrxXmlTestReporter.cs | src/cycodt/TestFramework/TrxXmlTestReporter.cs |
| src/TestFramework/YamlEnvHelpers.cs | src/cycodt/TestFramework/YamlEnvHelpers.cs |
| src/TestFramework/YamlHelpers.cs | src/cycodt/TestFramework/YamlHelpers.cs |
| src/TestFramework/YamlNodeExtensions.cs | src/cycodt/TestFramework/YamlNodeExtensions.cs |
| src/TestFramework/YamlTagHelpers.cs | src/cycodt/TestFramework/YamlTagHelpers.cs |
| src/TestFramework/YamlTestCaseFilter.cs | src/cycodt/TestFramework/YamlTestCaseFilter.cs |
| src/TestFramework/YamlTestCaseMatrixHelper.cs | src/cycodt/TestFramework/YamlTestCaseMatrixHelper.cs |
| src/TestFramework/YamlTestCaseParseContext.cs | src/cycodt/TestFramework/YamlTestCaseParseContext.cs |
| src/TestFramework/YamlTestCaseParser.cs | src/cycodt/TestFramework/YamlTestCaseParser.cs |
| src/TestFramework/YamlTestCaseRunner.cs | src/cycodt/TestFramework/YamlTestCaseRunner.cs |
| src/TestFramework/YamlTestConfigHelpers.cs | src/cycodt/TestFramework/YamlTestConfigHelpers.cs |
| src/TestFramework/YamlTestFramework.cs | src/cycodt/TestFramework/YamlTestFramework.cs |
| src/TestFramework/YamlTestFramework.csproj | src/cycodt/TestFramework/YamlTestFramework.csproj |
| src/TestFramework/YamlTestFrameworkCommon.targets | src/cycodt/TestFramework/YamlTestFrameworkCommon.targets |
| src/TestFramework/YamlTestFrameworkConsoleHost.cs | src/cycodt/TestFramework/YamlTestFrameworkConsoleHost.cs |
| src/TestFramework/YamlTestFrameworkTestAdapterMessageLogger.cs | src/cycodt/TestFramework/YamlTestFrameworkTestAdapterMessageLogger.cs |
| src/TestFramework/YamlTestProperties.cs | src/cycodt/TestFramework/YamlTestProperties.cs |