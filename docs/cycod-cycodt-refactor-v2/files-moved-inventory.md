# File-by-File Move Inventory

This document catalogs where each file will be moved during the refactoring process.

## Help Files

### Current Location: src/assets/help

## Files Moving to CycoDev (cycod/assets/help/)

All non-test related help files will move to the CycoDev project's help directory.

| Original File | Destination |
|---------------|-------------|
| src/assets/help/-.txt | cycod/assets/help/-.txt |
| src/assets/help/alias delete.txt | cycod/assets/help/alias delete.txt |
| src/assets/help/alias get.txt | cycod/assets/help/alias get.txt |
| src/assets/help/alias list.txt | cycod/assets/help/alias list.txt |
| src/assets/help/alias.txt | cycod/assets/help/alias.txt |
| src/assets/help/aliases.txt | cycod/assets/help/aliases.txt |
| src/assets/help/chat history.txt | cycod/assets/help/chat history.txt |
| src/assets/help/config add.txt | cycod/assets/help/config add.txt |
| src/assets/help/config clear.txt | cycod/assets/help/config clear.txt |
| src/assets/help/config get.txt | cycod/assets/help/config get.txt |
| src/assets/help/config list.txt | cycod/assets/help/config list.txt |
| src/assets/help/config remove.txt | cycod/assets/help/config remove.txt |
| src/assets/help/config set.txt | cycod/assets/help/config set.txt |
| src/assets/help/config.txt | cycod/assets/help/config.txt |
| src/assets/help/configuration.txt | cycod/assets/help/configuration.txt |
| src/assets/help/examples.txt | cycod/assets/help/examples.txt |
| src/assets/help/github login.txt | cycod/assets/help/github login.txt |
| src/assets/help/github.txt | cycod/assets/help/github.txt |
| src/assets/help/help.txt | cycod/assets/help/help.txt |
| src/assets/help/mcp add.txt | cycod/assets/help/mcp add.txt |
| src/assets/help/mcp get.txt | cycod/assets/help/mcp get.txt |
| src/assets/help/mcp list.txt | cycod/assets/help/mcp list.txt |
| src/assets/help/mcp remove.txt | cycod/assets/help/mcp remove.txt |
| src/assets/help/mcp.txt | cycod/assets/help/mcp.txt |
| src/assets/help/options.txt | cycod/assets/help/options.txt |
| src/assets/help/prompt create.txt | cycod/assets/help/prompt create.txt |
| src/assets/help/prompt delete.txt | cycod/assets/help/prompt delete.txt |
| src/assets/help/prompt get.txt | cycod/assets/help/prompt get.txt |
| src/assets/help/prompt list.txt | cycod/assets/help/prompt list.txt |
| src/assets/help/prompt.txt | cycod/assets/help/prompt.txt |
| src/assets/help/prompts.txt | cycod/assets/help/prompts.txt |
| src/assets/help/provider.txt | cycod/assets/help/provider.txt |
| src/assets/help/slash commands.txt | cycod/assets/help/slash commands.txt |
| src/assets/help/usage.txt | cycod/assets/help/usage.txt |

## Files Moving to CycoDevTest (cycodt/assets/help/)

Test-related help files will be moved to the CycoDevTest project with the "test" prefix removed from the filenames.

| Original File | Destination |
|---------------|-------------|
| src/assets/help/test examples.txt | cycodt/assets/help/examples.txt |
| src/assets/help/test list.txt | cycodt/assets/help/list.txt |
| src/assets/help/test run.txt | cycodt/assets/help/run.txt |
| src/assets/help/test.txt | cycodt/assets/help/help.txt |

## Directory Content

The mcp directory and its contents should be moved to cycod/assets/help/mcp/ maintaining the same structure.
## Prompt Files

### Current Location: src/assets/prompts

All prompt files will be moved to the CycoDev project's prompts directory.

| Original File | Destination |
|---------------|-------------|
| *.yaml | cycod/assets/prompts/*.yaml |
| *.yml | cycod/assets/prompts/*.yml |
| *.md | cycod/assets/prompts/*.md |
| *.txt | cycod/assets/prompts/*.txt |

All files in the src/assets/prompts directory will be moved to cycod/assets/prompts/, maintaining the same structure and filenames. These prompt files are only used by the CycoDev (cycod) application, not by the CycoDevTest (cycodt) application.

## Helper Files

### Current Location: src/Helpers

Helper utility classes will be moved to the CycoDev.Common library, as they are used by both applications.

| Original File | Destination |
|---------------|-------------|
| src/Helpers/FileHelpers.cs | CycoDev.Common/Helpers/FileHelpers.cs |
| src/Helpers/DirectoryHelpers.cs | CycoDev.Common/Helpers/DirectoryHelpers.cs |
| src/Helpers/ConsoleHelpers.cs | CycoDev.Common/Helpers/ConsoleHelpers.cs |
| src/Helpers/ColorHelpers.cs | CycoDev.Common/Helpers/ColorHelpers.cs |
| src/Helpers/Colors.cs | CycoDev.Common/Helpers/Colors.cs |
| src/Helpers/StringHelpers.cs | CycoDev.Common/Helpers/StringHelpers.cs |
| src/Helpers/TryCatchHelpers.cs | CycoDev.Common/Helpers/TryCatchHelpers.cs |
| src/Helpers/ValueHelpers.cs | CycoDev.Common/Helpers/ValueHelpers.cs |
| src/Helpers/JsonHelpers.cs | CycoDev.Common/Helpers/JsonHelpers.cs |
| src/Helpers/AtFileHelpers.cs | CycoDev.Common/Helpers/AtFileHelpers.cs |
| src/Helpers/MarkdownHelpers.cs | CycoDev.Common/Helpers/MarkdownHelpers.cs |
| src/Helpers/HelpHelpers.cs | CycoDev.Common/Helpers/HelpHelpers.cs |
| src/Helpers/HMACHelper.cs | CycoDev.Common/Helpers/HMACHelper.cs |
| src/Helpers/ExceptionHelpers.cs | CycoDev.Common/Helpers/ExceptionHelpers.cs |
| src/Helpers/AliasDisplayHelpers.cs | CycoDev.Common/Helpers/AliasDisplayHelpers.cs |
| src/Helpers/AliasFileHelpers.cs | CycoDev.Common/Helpers/AliasFileHelpers.cs |
| src/Helpers/CommonDisplayHelpers.cs | CycoDev.Common/Helpers/CommonDisplayHelpers.cs |
| src/Helpers/EnvironmentHelpers.cs | CycoDev.Common/Helpers/EnvironmentHelpers.cs |
| src/Helpers/OS.cs | CycoDev.Common/Helpers/OS.cs |
| src/Helpers/EmbeddedFileHelpers.cs | CycoDev.Common/Helpers/EmbeddedFileHelpers.cs |

## FunctionCalling Files

### Current Location: src/FunctionCalling

All function calling files will be moved to the CycoDev project, maintaining their directory structure.

| Original File | Destination |
|---------------|-------------|
| src/FunctionCalling/FunctionCallDetector.cs | CycoDev/FunctionCalling/FunctionCallDetector.cs |
| src/FunctionCalling/FunctionFactory.cs | CycoDev/FunctionCalling/FunctionFactory.cs |
| src/FunctionCalling/McpFunctionFactory.cs | CycoDev/FunctionCalling/McpFunctionFactory.cs |

## Command Line Commands Files

### Current Location: src/CommandLineCommands

#### Files Moving to CycoDev (CycoDev/CommandLineCommands/)

All non-test related command files will be moved to the CycoDev project, preserving their directory structure:

**AliasCommands Directory:**
| Original File | Destination |
|---------------|-------------|
| src/CommandLineCommands/AliasCommands/AliasBaseCommand.cs | CycoDev/CommandLineCommands/AliasCommands/AliasBaseCommand.cs |
| src/CommandLineCommands/AliasCommands/AliasDeleteCommand.cs | CycoDev/CommandLineCommands/AliasCommands/AliasDeleteCommand.cs |
| src/CommandLineCommands/AliasCommands/AliasGetCommand.cs | CycoDev/CommandLineCommands/AliasCommands/AliasGetCommand.cs |
| src/CommandLineCommands/AliasCommands/AliasListCommand.cs | CycoDev/CommandLineCommands/AliasCommands/AliasListCommand.cs |

**ConfigCommands Directory:**
| Original File | Destination |
|---------------|-------------|
| src/CommandLineCommands/ConfigCommands/ConfigAddCommand.cs | CycoDev/CommandLineCommands/ConfigCommands/ConfigAddCommand.cs |
| src/CommandLineCommands/ConfigCommands/ConfigBaseCommand.cs | CycoDev/CommandLineCommands/ConfigCommands/ConfigBaseCommand.cs |
| src/CommandLineCommands/ConfigCommands/ConfigClearCommand.cs | CycoDev/CommandLineCommands/ConfigCommands/ConfigClearCommand.cs |
| src/CommandLineCommands/ConfigCommands/ConfigGetCommand.cs | CycoDev/CommandLineCommands/ConfigCommands/ConfigGetCommand.cs |
| src/CommandLineCommands/ConfigCommands/ConfigListCommand.cs | CycoDev/CommandLineCommands/ConfigCommands/ConfigListCommand.cs |
| src/CommandLineCommands/ConfigCommands/ConfigRemoveCommand.cs | CycoDev/CommandLineCommands/ConfigCommands/ConfigRemoveCommand.cs |
| src/CommandLineCommands/ConfigCommands/ConfigSetCommand.cs | CycoDev/CommandLineCommands/ConfigCommands/ConfigSetCommand.cs |

**McpCommands Directory:**
| Original File | Destination |
|---------------|-------------|
| src/CommandLineCommands/McpCommands/McpAddCommand.cs | CycoDev/CommandLineCommands/McpCommands/McpAddCommand.cs |
| src/CommandLineCommands/McpCommands/McpBaseCommand.cs | CycoDev/CommandLineCommands/McpCommands/McpBaseCommand.cs |
| src/CommandLineCommands/McpCommands/McpGetCommand.cs | CycoDev/CommandLineCommands/McpCommands/McpGetCommand.cs |
| src/CommandLineCommands/McpCommands/McpListCommand.cs | CycoDev/CommandLineCommands/McpCommands/McpListCommand.cs |
| src/CommandLineCommands/McpCommands/McpRemoveCommand.cs | CycoDev/CommandLineCommands/McpCommands/McpRemoveCommand.cs |

**PromptCommands Directory:**
| Original File | Destination |
|---------------|-------------|
| src/CommandLineCommands/PromptCommands/PromptBaseCommand.cs | CycoDev/CommandLineCommands/PromptCommands/PromptBaseCommand.cs |
| src/CommandLineCommands/PromptCommands/PromptCreateCommand.cs | CycoDev/CommandLineCommands/PromptCommands/PromptCreateCommand.cs |
| src/CommandLineCommands/PromptCommands/PromptDeleteCommand.cs | CycoDev/CommandLineCommands/PromptCommands/PromptDeleteCommand.cs |
| src/CommandLineCommands/PromptCommands/PromptGetCommand.cs | CycoDev/CommandLineCommands/PromptCommands/PromptGetCommand.cs |
| src/CommandLineCommands/PromptCommands/PromptListCommand.cs | CycoDev/CommandLineCommands/PromptCommands/PromptListCommand.cs |

**Root Commands:**
| Original File | Destination |
|---------------|-------------|
| src/CommandLineCommands/ChatCommand.cs | CycoDev/CommandLineCommands/ChatCommand.cs |
| src/CommandLineCommands/GitHubLoginCommand.cs | CycoDev/CommandLineCommands/GitHubLoginCommand.cs |
| src/CommandLineCommands/HelpCommand.cs | CycoDev/CommandLineCommands/HelpCommand.cs |
| src/CommandLineCommands/VersionCommand.cs | CycoDev/CommandLineCommands/VersionCommand.cs |
| src/CommandLineCommands/BaseCommand.cs | CycoDev/CommandLineCommands/BaseCommand.cs |
| src/CommandLineCommands/[any other non-test commands] | CycoDev/CommandLineCommands/[same filename] |

#### Files Moving to CycoDevTest (CycoDevTest/CommandLineCommands/)

Test-related command files will be moved to the CycoDevTest project. Note that while the command names will be simplified (no "test" prefix), the class names will remain the same for clarity.

**TestCommands Directory:**
| Original File | Destination |
|---------------|-------------|
| src/CommandLineCommands/TestCommands/TestListCommand.cs | CycoDevTest/CommandLineCommands/TestListCommand.cs |
| src/CommandLineCommands/TestCommands/TestRunCommand.cs | CycoDevTest/CommandLineCommands/TestRunCommand.cs |
| src/CommandLineCommands/TestCommands/TestBaseCommand.cs | CycoDevTest/CommandLineCommands/TestBaseCommand.cs |
| src/CommandLineCommands/TestCommands/[any other test-specific files] | CycoDevTest/CommandLineCommands/[same filename] |

**Root Test Commands:**
| Original File | Destination |
|---------------|-------------|
| src/CommandLineCommands/HelpCommand.cs | CycoDevTest/CommandLineCommands/HelpCommand.cs (specialized version) |

## McpHelpers Files

### Current Location: src/McpHelpers

All files in the McpHelpers directory will be moved to the CycoDev project as they are specific to the main application.

| Original File | Destination |
|---------------|-------------|
| src/McpHelpers/IMcpServerConfigItem.cs | CycoDev/McpHelpers/IMcpServerConfigItem.cs |
| src/McpHelpers/McpClientManager.cs | CycoDev/McpHelpers/McpClientManager.cs |
| src/McpHelpers/McpConfigFile.cs | CycoDev/McpHelpers/McpConfigFile.cs |
| src/McpHelpers/McpDisplayHelpers.cs | CycoDev/McpHelpers/McpDisplayHelpers.cs |
| src/McpHelpers/McpFileHelpers.cs | CycoDev/McpHelpers/McpFileHelpers.cs |
| src/McpHelpers/McpServerConfig.cs | CycoDev/McpHelpers/McpServerConfig.cs |
| src/McpHelpers/McpServerConfigItemConverter.cs | CycoDev/McpHelpers/McpServerConfigItemConverter.cs |
| src/McpHelpers/SseServerConfig.cs | CycoDev/McpHelpers/SseServerConfig.cs |
| src/McpHelpers/StdioServerConfig.cs | CycoDev/McpHelpers/StdioServerConfig.cs |

## ShellHelpers Files

### Current Location: src/ShellHelpers

All files in the ShellHelpers directory will be moved to the CycoDev project as they are specific to the main application.

| Original File | Destination |
|---------------|-------------|
| src/ShellHelpers/BashShellSession.cs | CycoDev/ShellHelpers/BashShellSession.cs |
| src/ShellHelpers/CmdShellSession.cs | CycoDev/ShellHelpers/CmdShellSession.cs |
| src/ShellHelpers/PowershellShellSession.cs | CycoDev/ShellHelpers/PowershellShellSession.cs |
| src/ShellHelpers/ShellSession.cs | CycoDev/ShellHelpers/ShellSession.cs |

## ChatClient Files

### Current Location: src/ChatClient

All files in the ChatClient directory will be moved to the CycoDev project as they are related to chat functionality.

| Original File | Destination |
|---------------|-------------|
| src/ChatClient/ChatClientFactory.cs | CycoDev/ChatClient/ChatClientFactory.cs |
| src/ChatClient/FunctionCallingChat.cs | CycoDev/ChatClient/FunctionCallingChat.cs |

## FunctionCallingTools Files

### Current Location: src/FunctionCallingTools

All files in the FunctionCallingTools directory will be moved to the CycoDev project as they are specific to the main application.

| Original File | Destination |
|---------------|-------------|
| src/FunctionCallingTools/DateAndTimeHelperFunctions.cs | CycoDev/FunctionCallingTools/DateAndTimeHelperFunctions.cs |
| src/FunctionCallingTools/CodeExplorationHelperFunctions.cs | CycoDev/FunctionCallingTools/CodeExplorationHelperFunctions.cs |
| src/FunctionCallingTools/MdxCliWrapper.cs | CycoDev/FunctionCallingTools/MdxCliWrapper.cs |
| src/FunctionCallingTools/ShellCommandToolHelperFunctions.cs | CycoDev/FunctionCallingTools/ShellCommandToolHelperFunctions.cs |
| src/FunctionCallingTools/StrReplaceEditorHelperFunctions.cs | CycoDev/FunctionCallingTools/StrReplaceEditorHelperFunctions.cs |
| src/FunctionCallingTools/ThinkingToolHelperFunction.cs | CycoDev/FunctionCallingTools/ThinkingToolHelperFunction.cs |

## SlashCommands Files

### Current Location: src/SlashCommands

All files in the SlashCommands directory will be moved to the CycoDev project as they are specific to the main application.

| Original File | Destination |
|---------------|-------------|
| src/SlashCommands/SlashMdxCommandHandler.cs | CycoDev/SlashCommands/SlashMdxCommandHandler.cs |
| src/SlashCommands/SlashPromptCommandHandler.cs | CycoDev/SlashCommands/SlashPromptCommandHandler.cs |

## Templates Files

### Current Location: src/Templates

All files in the Templates directory will be moved to the CycoDev.Common project as they are used by both applications.

| Original File | Destination |
|---------------|-------------|
| src/Templates/ExpressionCalculator.cs | CycoDev.Common/Templates/ExpressionCalculator.cs |
| src/Templates/INamedValues.cs | CycoDev.Common/Templates/INamedValues.cs |
| src/Templates/TemplateHelpers.cs | CycoDev.Common/Templates/TemplateHelpers.cs |
| src/Templates/TemplateVariables.cs | CycoDev.Common/Templates/TemplateVariables.cs |

## TestFramework Files

### Current Location: src/TestFramework

All files in the TestFramework directory will be moved to the CycoDevTest project as they are only needed by the test application.

| Original File | Destination |
|---------------|-------------|
| src/TestFramework/IYamlTestFrameworkHost.cs | CycoDevTest/TestFramework/IYamlTestFrameworkHost.cs |
| src/TestFramework/IYamlTestFrameworkLogger.cs | CycoDevTest/TestFramework/IYamlTestFrameworkLogger.cs |
| src/TestFramework/JunitXmlTestReporter.cs | CycoDevTest/TestFramework/JunitXmlTestReporter.cs |
| src/TestFramework/Logger.cs | CycoDevTest/TestFramework/Logger.cs |
| src/TestFramework/PathHelpers.cs | CycoDevTest/TestFramework/PathHelpers.cs |
| src/TestFramework/PropertyInterpolationHelpers.cs | CycoDevTest/TestFramework/PropertyInterpolationHelpers.cs |
| src/TestFramework/README.md | CycoDevTest/TestFramework/README.md |
| src/TestFramework/RunnableTestCase.cs | CycoDevTest/TestFramework/RunnableTestCase.cs |
| src/TestFramework/RunnableTestCaseItem.cs | CycoDevTest/TestFramework/RunnableTestCaseItem.cs |
| src/TestFramework/TestResultHelpers.cs | CycoDevTest/TestFramework/TestResultHelpers.cs |
| src/TestFramework/TestRun.cs | CycoDevTest/TestFramework/TestRun.cs |
| src/TestFramework/TimeSpanFormatter.cs | CycoDevTest/TestFramework/TimeSpanFormatter.cs |
| src/TestFramework/TrxXmlTestReporter.cs | CycoDevTest/TestFramework/TrxXmlTestReporter.cs |
| src/TestFramework/YamlEnvHelpers.cs | CycoDevTest/TestFramework/YamlEnvHelpers.cs |
| src/TestFramework/YamlHelpers.cs | CycoDevTest/TestFramework/YamlHelpers.cs |
| src/TestFramework/YamlNodeExtensions.cs | CycoDevTest/TestFramework/YamlNodeExtensions.cs |
| src/TestFramework/YamlTagHelpers.cs | CycoDevTest/TestFramework/YamlTagHelpers.cs |
| src/TestFramework/YamlTestCaseFilter.cs | CycoDevTest/TestFramework/YamlTestCaseFilter.cs |
| src/TestFramework/YamlTestCaseMatrixHelper.cs | CycoDevTest/TestFramework/YamlTestCaseMatrixHelper.cs |
| src/TestFramework/YamlTestCaseParseContext.cs | CycoDevTest/TestFramework/YamlTestCaseParseContext.cs |
| src/TestFramework/YamlTestCaseParser.cs | CycoDevTest/TestFramework/YamlTestCaseParser.cs |
| src/TestFramework/YamlTestCaseRunner.cs | CycoDevTest/TestFramework/YamlTestCaseRunner.cs |
| src/TestFramework/YamlTestConfigHelpers.cs | CycoDevTest/TestFramework/YamlTestConfigHelpers.cs |
| src/TestFramework/YamlTestFramework.cs | CycoDevTest/TestFramework/YamlTestFramework.cs |
| src/TestFramework/YamlTestFramework.csproj | CycoDevTest/TestFramework/YamlTestFramework.csproj |
| src/TestFramework/YamlTestFrameworkCommon.targets | CycoDevTest/TestFramework/YamlTestFrameworkCommon.targets |
| src/TestFramework/YamlTestFrameworkConsoleHost.cs | CycoDevTest/TestFramework/YamlTestFrameworkConsoleHost.cs |
| src/TestFramework/YamlTestFrameworkTestAdapterMessageLogger.cs | CycoDevTest/TestFramework/YamlTestFrameworkTestAdapterMessageLogger.cs |
| src/TestFramework/YamlTestProperties.cs | CycoDevTest/TestFramework/YamlTestProperties.cs |