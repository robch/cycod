# Project Structure

## Current Structure
The current structure has a single project `chatx` that builds into one executable.

## New Structure
The new structure will consist of three projects:

### 1. CycoDev.Common (Library Project)
This shared library will contain code used by both executables.

```
CycoDev.Common/
├─ Configuration/        # Configuration system (shared through .cycod directory)
├─ CommandLine/          # Command line infrastructure
│  └─ Commands/          # Base command classes shared between applications
├─ Helpers/              # Shared utilities
├─ Templates/            # Template processing utilities
└─ CycoDev.Common.csproj
```

### 2. CycoDev (Executable Project)
This project will build the main `cycod` executable with all functionality except test commands.

```
CycoDev/
├─ CommandLineCommands/  # All non-test commands
│  ├─ Chat/              # Chat-related commands 
│  ├─ Config/            # Configuration commands
│  ├─ GitHub/            # GitHub-related commands
│  ├─ Prompt/            # Prompt-related commands
│  └─ Alias/             # Alias-related commands
├─ FunctionCalling/      # Function calling implementations
├─ FunctionCallingTools/ # Function calling helpers
├─ ShellHelpers/         # Shell session implementations
├─ SlashCommands/        # Slash command handlers
├─ McpHelpers/           # MCP functionality helpers
├─ Program.cs            # Main program (modified to exclude test commands)
├─ CommandLineOptions.cs # CycoDev-specific command line parsing
├─ assets/               # Application assets with app-specific help files
│  └─ help/              # Independent help files for cycod
└─ CycoDev.csproj
```

### 3. CycoDevTest (Executable Project)
This project will build the test-focused `cycodt` executable. Note that CycoDevTest launches CycoDev as a child process when executing tests.

```
CycoDevTest/
├─ TestFramework/        # Complete test framework moved from original project
│  ├─ YamlTestFramework.cs        # Core test framework
│  ├─ YamlTestCaseParser.cs       # Test case parsing
│  ├─ YamlTestCaseRunner.cs       # Test execution (uses fixed string "cycod" for CLI references)
│  ├─ YamlTestFrameworkConsoleHost.cs  # Console hosting
│  ├─ Reporters/                  # Test reporting components
│  └─ Commands/                   # Base test command classes
├─ CommandLineCommands/  # Test commands (modified to work without "test" prefix)
│  ├─ TestListCommand.cs # Keeps original class name but implements "list" command
│  └─ TestRunCommand.cs  # Keeps original class name but implements "run" command
├─ Program.cs            # Test-focused program
├─ CommandLineOptions.cs # CycoDevTest-specific command line parsing
├─ assets/               # Test-specific help files
│  └─ help/              # Independent help files for cycodt
└─ CycoDevTest.csproj
```

## Test Resources Organization

Test resources will be organized in a structured way:

```
tests/                # Top-level tests folder
├─ common/            # Most tests will migrate here
│  └─ ...             # Sub-project specific folders
├─ cycod/             # A small number of application-specific tests
│  └─ ...             # Sub-project specific folders
└─ cycodt/            # Currently no tests specific to this component
   └─ ...             # Reserved for future test organization
```

Nearly all tests currently in the `tests/` folder will migrate to the `tests/common/` folder.

## Solution Structure
The overall solution structure will look like this:

### 4. CycoDev.Tests (Test Project)
This project will contain the unit tests for the library and application code.

```
CycoDev.Tests/
├─ CommonTests/        # Tests for common library components
├─ CycoDevTests/       # Tests for main application components
├─ TestHelpers/        # Test utilities and helpers
├─ GlobalUsings.cs     # Global using directives for testing
└─ CycoDev.Tests.csproj
```

```
CycoDevSolution/
├─ CycoDev.Common/       # Shared library project
├─ CycoDev/              # Main application project
├─ CycoDevTest/          # Test application project
├─ CycoDev.Tests/        # Unit tests project
└─ CycoDevSolution.sln
```

## Configuration and Cross-Application Features

### Shared Configuration
- Both applications will share a configuration directory named `.cycod` (not `.cycodev`)
- Environment variables will use "CYCODEV_" prefix instead of "CHATX_"
- CycoDevTest can only read configuration settings but not modify them
- If application-specific settings are needed in the future, they would use dot notation prefixes (`cycod.<setting>` and `cycodt.<setting>`)

### Cross-Application References
- CycoDevTest will launch CycoDev as a child process when running tests
- CycoDev has no need to launch CycoDevTest as a child process
- Fixed string approach will be used for CLI references in CycoDevTest, not parameterization

### Help System Organization
- Each application will have independent help files in their respective `assets/help/` folders
- There will be no shared help content between applications
- Help files in each application will be specific to their own commands

## Project Dependencies

- **CycoDev.Common**: No dependencies on other projects
- **CycoDev**: Depends on CycoDev.Common
- **CycoDevTest**: Depends on CycoDev.Common
- **CycoDev.Tests**: Depends on CycoDev.Common, CycoDev, and CycoDevTest

## Project Files (.csproj)

### CycoDev.Common.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  
  <!-- Required packages -->
  <ItemGroup>
    <PackageReference Include="YamlDotNet" Version="16.3.0" />
    <PackageReference Include="System.Configuration.ConfigurationManager" Version="5.0.0" />
  </ItemGroup>
</Project>
```

### CycoDev.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <EnableDefaultCompileItems>true</EnableDefaultCompileItems>
    <OutputType>Exe</OutputType>
    <AssemblyName>cycod</AssemblyName>
    
    <!-- Cross-platform support -->
    <RuntimeIdentifiers>win-x64;linux-x64;osx-x64</RuntimeIdentifiers>
    
    <!-- Tool Configuration -->
    <PackAsTool>true</PackAsTool>
    <ToolCommandName>cycod</ToolCommandName>
  </PropertyGroup>
  
  <!-- Project references -->
  <ItemGroup>
    <ProjectReference Include="..\CycoDev.Common\CycoDev.Common.csproj" />
  </ItemGroup>
  
  <!-- External packages -->
  <ItemGroup>
    <PackageReference Include="Azure.Identity" Version="1.14.0-beta.2" />
    <PackageReference Include="Azure.AI.OpenAI" Version="2.2.0-beta.2" />
    <PackageReference Include="Microsoft.Extensions.AI" Version="9.3.0-preview.1.25161.3" />
    <PackageReference Include="Microsoft.Extensions.AI.Abstractions" Version="9.3.0-preview.1.25161.3" />
    <PackageReference Include="Microsoft.Extensions.AI.OpenAI" Version="9.3.0-preview.1.25161.3" />
    <PackageReference Include="ModelContextProtocol" Version="0.1.0-preview.6" />
  </ItemGroup>
  
  <!-- Embedded resources -->
  <ItemGroup>
    <EmbeddedResource Include="assets\help\**" />
    <EmbeddedResource Include="assets\prompts\**" />
  </ItemGroup>
</Project>
```

### CycoDevTest.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <EnableDefaultCompileItems>true</EnableDefaultCompileItems>
    <OutputType>Exe</OutputType>
    <AssemblyName>cycodt</AssemblyName>
    
    <!-- Cross-platform support -->
    <RuntimeIdentifiers>win-x64;linux-x64;osx-x64</RuntimeIdentifiers>
    
    <!-- Tool Configuration -->
    <PackAsTool>true</PackAsTool>
    <ToolCommandName>cycodt</ToolCommandName>
  </PropertyGroup>
  
  <!-- Project references -->
  <ItemGroup>
    <ProjectReference Include="..\CycoDev.Common\CycoDev.Common.csproj" />
  </ItemGroup>
  
  <!-- Test framework packages -->
  <ItemGroup>
    <PackageReference Include="YamlDotNet" Version="16.3.0" />
    <PackageReference Include="Microsoft.TestPlatform.ObjectModel" Version="17.0.0" />
  </ItemGroup>
  
  <!-- Embedded resources -->
  <ItemGroup>
    <EmbeddedResource Include="assets\help\**" />
  </ItemGroup>
</Project>
```

### CycoDev.Tests.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>
  
  <!-- Project references -->
  <ItemGroup>
    <ProjectReference Include="..\CycoDev.Common\CycoDev.Common.csproj" />
    <ProjectReference Include="..\CycoDev\CycoDev.csproj" />
    <ProjectReference Include="..\CycoDevTest\CycoDevTest.csproj" />
  </ItemGroup>
  
  <!-- Test packages -->
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.6.0" />
    <PackageReference Include="MSTest.TestAdapter" Version="3.0.0" />
    <PackageReference Include="MSTest.TestFramework" Version="3.0.0" />
    <PackageReference Include="coverlet.collector" Version="6.0.0" />
  </ItemGroup>
</Project>
```