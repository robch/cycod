# CYCOD Custom Tools Specification

## Table of Contents

1. [Overview](#overview)
   - [Purpose](#purpose)
2. [Tool Definition](#tool-definition)
   - [File Format and Location](#file-format-and-location)
   - [Schema](#schema)
   - [Single-Step vs Multi-Step Tools](#single-step-vs-multi-step-tools)
   - [Platform-Specific Commands](#platform-specific-commands)
   - [Parameter Definition](#parameter-definition)
   - [Tags and Platform Support](#tags-and-platform-support)
   - [Error Handling and Recovery](#error-handling-and-recovery)
   - [Parallel Execution](#parallel-execution)
   - [Output Streaming and Management](#output-streaming-and-management)
   - [LLM Function Calling Integration](#llm-function-calling-integration)
   - [Security Configuration](#security-configuration)
   - [Tool Composition](#tool-composition)
   - [Tool Dependencies](#tool-dependencies)
   - [Tool Aliasing](#tool-aliasing)
   - [Testing Configuration](#testing-configuration)
3. [Command Line Interface](#command-line-interface)
   - [Tool Management Commands](#tool-management-commands)
   - [Tool Add Command](#tool-add-command)
   - [Tool Get Command](#tool-get-command)
   - [Tool List Command](#tool-list-command)
   - [Tool Remove Command](#tool-remove-command)
   - [Tool Test Command](#tool-test-command)
   - [Tool Export Command](#tool-export-command)
   - [Tool Import Command](#tool-import-command)
4. [Parameter Substitution](#parameter-substitution)
   - [Parameter References](#parameter-references)
   - [Dynamic Parameter References](#dynamic-parameter-references)
   - [Parameter Transformations](#parameter-transformations)
   - [Step Output References](#step-output-references)
   - [Default Values](#default-values)
5. [Security Model](#security-model)
   - [Integration with Auto-Approve/Auto-Deny](#integration-with-auto-approveauto-deny)
   - [Security Tags](#security-tags)
   - [Execution Privileges](#execution-privileges)
   - [Required Permissions](#required-permissions)
6. [Best Practices](#best-practices)
   - [Tool Design Principles](#tool-design-principles)
   - [Parameter Naming Conventions](#parameter-naming-conventions)
   - [Error Handling Patterns](#error-handling-patterns)
   - [Consistency Guidelines](#consistency-guidelines)
   - [Performance Considerations](#performance-considerations)
   - [Cross-Platform Compatibility](#cross-platform-compatibility)
7. [Implementation Considerations](#implementation-considerations)
   - [Parameter Substitution and Escaping](#parameter-substitution-and-escaping)
   - [Security Enforcement](#security-enforcement)
   - [Cross-Platform Compatibility](#cross-platform-compatibility-1)
   - [Command Execution Context](#command-execution-context)
   - [Output Capturing and Streaming](#output-capturing-and-streaming)
   - [Parallel Execution](#parallel-execution-1)
   - [Resource Management and Cleanup](#resource-management-and-cleanup)
   - [Function Calling Integration](#function-calling-integration)
8. [Debugging and Troubleshooting](#debugging-and-troubleshooting)
   - [Debugging Tools](#debugging-tools)
   - [Execution Context](#execution-context)
   - [Common Issues and Solutions](#common-issues-and-solutions)
   - [Logging](#logging)
9. [Testing Framework](#testing-framework)
   - [Overview](#overview-1)
   - [Test Definition](#test-definition)
   - [Test Components](#test-components)
   - [Assertions](#assertions)
   - [Cleanup](#cleanup)
   - [Running Tests](#running-tests)
   - [Integration with CI/CD](#integration-with-cicd)
10. [Help Text](#help-text)
    - [Tool Help Command](#tool-help-command)
11. [Examples](#examples)
    - [Simple Single-Step Tool Example](#simple-single-step-tool-example)
    - [Multi-Step Tool with Output Capturing](#multi-step-tool-with-output-capturing)
    - [Tool with Conditional Steps and Error Handling](#tool-with-conditional-steps-and-error-handling)
    - [Tool with Complex Parameter Handling](#tool-with-complex-parameter-handling)
    - [Tool Alias Example](#tool-alias-example)
    - [Tool Using Another Tool](#tool-using-another-tool)
    - [Parallel Execution Example](#parallel-execution-example)
    - [Interactive Tool Example](#interactive-tool-example)

## Overview

Custom Tools provide a mechanism for CYCOD users to define, share, and execute shell commands as named tools with parameters. These tools can be used by LLMs through function calling, similar to MCPs but focused on shell command execution.

### Purpose

Custom Tools allow users to:
1. Wrap frequently used shell commands with a consistent interface
2. Define parameters with descriptions and default values
3. Create multi-step tools with conditional execution
4. Share tools across multiple scopes (local, user, global)
5. Categorize tools for organization and security
6. Integrate seamlessly with LLM function calling

## Tool Definition

### File Format and Location

Tool definitions are stored as YAML files in the following locations:
- Local scope: `./.cycod/tools/`
- User scope: `~/.cycod/tools/`
- Global scope: System-wide location (OS-dependent)

Tool files use the `.yaml` extension. The filename determines the tool's name if not explicitly specified in the YAML content.

### Schema

```yaml
# Basic information
name: tool-name                    # Optional if matches filename
description: Tool description      # Required
version: 1.0.0                     # Tool version
min-cycod-version: 1.0.0           # Minimum CYCOD version required

# Execution (one of the following)
bash: command {PARAM}              # For bash execution
cmd: command {PARAM}               # For Windows CMD execution
pwsh: command {PARAM}              # For PowerShell execution
run: command {PARAM}               # For direct command execution
script: |                          # For custom scripts
  command line 1
  command line 2
shell: shell-name                  # Shell to use with script

# OR for platform-specific commands
commands:
  default: find {DIRECTORY} -name "{PATTERN}"
  platforms:
    windows: powershell -Command "Get-ChildItem -Path '{DIRECTORY}' -Filter '{PATTERN}' -Recurse"
    unix: find {DIRECTORY} -name "{PATTERN}"

# OR for multi-step tools
steps:
  - name: step1                    # Step name (required)
    bash: command {PARAM}          # Command to execute
    continue-on-error: false       # Whether to continue if this step fails
    run-condition: "{step2.exit-code} == 0"  # Condition for when to run this step
    parallel: false                # Whether to run in parallel with previous step
    wait-for: [step2, step3]       # Steps to wait for before running
    
    # Use another tool
    use-tool: other-tool           # Name of another tool to use
    with:                          # Parameters to pass to the other tool
      PARAM1: value1
    
    # Error handling
    error-handling:
      retry:
        attempts: 3
        delay: 1000  # milliseconds
      fallback: alternative-command {PARAM}
    
    # Output management
    output:
      mode: buffer                 # buffer or stream
      buffer-limit: 10MB           # Maximum buffer size
      stream-callback: console     # Where to stream output
  
  - name: step2
    bash: command {PARAM}

# Parameters
parameters:
  PARAM1:
    type: string                   # Parameter type (string, number, boolean, array, object)
    description: Parameter description  # Required
    required: true                 # Whether parameter is required
    default: default value         # Default value if not provided
    
    # Validation rules
    validation:
      minimum: 1                   # Minimum value (for number type)
      maximum: 100                 # Maximum value (for number type)
      pattern: "^[a-z]+$"          # Regex pattern (for string type)
      
    # Transformation
    transform: "value.toLowerCase()"  # Transform function to apply
    format: "--param={value}"      # How parameter appears in command
    
    # Examples and help
    examples: ["example1", "example2"]  # Example values
    detailed-help: |
      Detailed help text for the parameter.
      This can span multiple lines.
    
    # Security
    security:
      escape-shell: true           # Escape shell metacharacters

# Environment variables
environment:
  variables:
    HTTP_PROXY: "{PROXY_URL}"
    DEBUG: "1"
  inherit: true                    # Inherit parent process environment

# Function calling integration for LLMs
function-calling:
  schema-generation:
    parameter-mapping:
      string: "string"
      number: "number"
      boolean: "boolean"
      array: "array"
    include-descriptions: true
    include-defaults: true
    example-generation: true

# Security configuration
security:
  execution-privilege: same-as-user  # Options: same-as-user, reduced, elevated (requires approval)
  isolation: process                 # Options: none, process, container
  required-permissions:
    - "filesystem:write:{DIRECTORY}"
    - "network:external:api.github.com"
  justification: "Required for cloning GitHub repositories"

# File paths handling
file-paths:
  normalize: true                  # Convert paths to platform-specific format
  working-directory: "{WORKSPACE}"
  temp-directory: "{TEMP}/cycod-tools/{TOOL_NAME}"
  cross-platform:
    windows-separator: "\\"
    unix-separator: "/"
    auto-convert: true

# Metadata for categorization and discovery
metadata:
  category: development
  subcategory: version-control
  tags: [git, repository, clone]
  search-keywords: [git, repo, download, checkout]

# Resource constraints
resources:
  timeout: 60000                   # Timeout in milliseconds
  max-memory: 512MB                # Maximum memory usage
  cleanup:
    - delete-temp-files: true
    - final-command: "rm -rf {TEMP_DIR}"

# Interactive configuration
interactive: false                 # Whether tool requires user interaction
interactive-options:
  timeout: 30000                   # How long to wait for user input
  default-response: "y"            # Default if no input provided

# For tool aliases
type: alias                        # Indicates this is an alias of another tool
base-tool: other-tool              # The tool this is an alias of
default-parameters:                # Default parameters for the alias
  PARAM1: value1
  PARAM2: value2

# Testing configuration
tests:
  - name: basic-test
    description: "Test basic functionality"
    parameters:
      PARAM1: "test value"
    expected:
      exit-code: 0
      output-contains: "Success"
      file-exists: "output.txt"
    cleanup:
      - "rm output.txt"

# Version history
changelog:
  - version: 1.0.0
    changes: "Initial release"
  - version: 1.1.0
    changes: "Added support for XYZ feature"

# Tool dependencies
dependencies:
  - tool: git-clone                # Required tool
    version: ">=1.0.0"             # Version constraint
    required: true                 # Whether dependency is required
    scope: any                     # Scope to search for tool

# Debugging configuration
debugging:
  verbose: false                   # Enable verbose output
  dry-run: false                   # Show commands without executing
  log-file: "~/.cycod/logs/{TOOL_NAME}.log"  # Log file location

# Optional settings
tags: [tag1, tag2, read]           # Categories and security tags
platforms: [windows, linux, macos] # Supported platforms
working-directory: ~/path          # Working directory
timeout: 60000                     # Timeout in milliseconds
ignore-errors: false               # Whether to continue if a step fails
input: "{INPUT_PARAM}"             # Data to pass via stdin
```

### Single-Step vs Multi-Step Tools

Tools can be defined in two ways:

1. **Single-Step Tools**:
   ```yaml
   name: simple-tool
   description: A simple tool
   bash: command {PARAM}
   parameters:
     PARAM:
       type: string
       description: A parameter
   ```

2. **Multi-Step Tools**:
   ```yaml
   name: multi-step-tool
   description: A tool with multiple steps
   steps:
     - name: step1
       bash: command1 {PARAM}
     - name: step2
       bash: command2
       run-condition: "{step1.exit-code} == 0"
   parameters:
     PARAM:
       type: string
       description: A parameter
   ```

### Platform-Specific Commands

For tools that need different implementations across platforms:

```yaml
name: find-files
description: Find files matching a pattern
commands:
  default: find {DIRECTORY} -name "{PATTERN}"
  platforms:
    windows: powershell -Command "Get-ChildItem -Path '{DIRECTORY}' -Filter '{PATTERN}' -Recurse"
    unix: find {DIRECTORY} -name "{PATTERN}"
parameters:
  DIRECTORY:
    type: string
    description: Directory to search in
  PATTERN:
    type: string
    description: File pattern to match
```

### Parameter Definition

Parameters define inputs that can be substituted into commands:

```yaml
parameters:
  NAME:
    type: string                # Parameter type
    description: Description    # Human-readable description
    required: true              # Whether parameter is required
    default: Default value      # Default value if not provided
    validation:                 # Validation rules
      minimum: 1                # For number type
      maximum: 100              # For number type
      pattern: "^[a-z]+$"       # Regex pattern for string type
      minLength: 3              # Minimum string length
      maxLength: 50             # Maximum string length
      enum: ["option1", "option2"]  # Allowed values
      # For array type
      minItems: 1               # Minimum number of items
      maxItems: 10              # Maximum number of items
      uniqueItems: true         # Whether items must be unique
      # For object type
      required: ["prop1", "prop2"]  # Required properties
      properties:               # Property validation
        prop1:
          type: string
          pattern: "^[a-z]+$"
    transform: "value.toLowerCase()"  # Transform function
    examples: ["example1", "example2"]  # Example values
```

Parameter types include:
- `string`: Text values
- `number`: Numeric values
- `boolean`: True/false values
- `array`: List of values
- `object`: Complex object with properties

### Tags and Platform Support

Tools can specify which platforms they work on and include tags for categorization:

```yaml
platforms: [windows, linux, macos]  # Only include platforms the tool works on
tags: [category1, category2, read]  # Tags for categorization and security
```

The security tags (`read`, `write`, `run`) are recommended but optional. If no security tag is present, the tool is considered high-risk and requires explicit approval.

### Error Handling and Recovery

Tools can specify how to handle errors:

```yaml
ignore-errors: false   # Whether to continue even if the tool fails
```

For multi-step tools, each step can have its own error handling:

```yaml
steps:
  - name: step1
    bash: command
    continue-on-error: true
    error-handling:
      retry:
        attempts: 3
        delay: 1000  # milliseconds
      fallback: alternative-command {PARAM}
```

### Parallel Execution

Steps can be executed in parallel for improved performance:

```yaml
steps:
  - name: step1
    bash: command1
    parallel: false  # Default is sequential
  
  - name: step2
    bash: command2
    parallel: true   # Execute in parallel with previous step
  
  - name: step3
    bash: command3
    wait-for: [step1, step2]  # Wait for specific steps to complete
```

### Output Streaming and Management

Control how command output is handled:

```yaml
steps:
  - name: generate-large-output
    bash: large-data-command
    output:
      mode: stream         # Options: buffer, stream
      buffer-limit: 10MB   # Maximum size when buffering
      stream-callback: console  # Where to stream (console, file, function)
```

### LLM Function Calling Integration

Custom tools are designed to be easily discoverable and usable by LLMs through function calling. The `function-calling` section of the tool definition controls how the tool is represented in JSON Schema format for LLM function calling.

When an LLM is presented with a function calling schema, it can understand the tool's purpose, required parameters, and expected behavior, enabling it to make accurate function calls based on user requests.

Configure how the tool is exposed to LLMs through function calling:

```yaml
function-calling:
  schema-generation:
    parameter-mapping:
      string: "string"      # Map tool parameter type to JSON Schema type
      number: "number"
      boolean: "boolean"
      array: "array"
      object: "object"
    include-descriptions: true   # Include parameter descriptions in schema
    include-defaults: true       # Include default values in schema
    example-generation: true     # Generate examples from parameter examples
    function-name: "{TOOL_NAME}" # Override function name (default: tool name)
    description-template: "Custom tool: {DESCRIPTION}" # Template for function description
```

#### Schema Generation

The `schema-generation` section controls how the tool definition is converted to a JSON Schema for function calling:

- `parameter-mapping`: Maps tool parameter types to JSON Schema types
- `include-descriptions`: Whether to include parameter descriptions in the schema
- `include-defaults`: Whether to include default values in the schema
- `example-generation`: Whether to generate examples based on parameter examples
- `function-name`: Optional override for the function name
- `description-template`: Template for generating the function description

#### Generated Schema Example

A tool defined as:

```yaml
name: weather-lookup
description: Get weather for a location
bash: curl -s 'wttr.in/{LOCATION}?format={FORMAT}'
parameters:
  LOCATION:
    type: string
    description: City or airport code
    required: true
    examples: ["New York", "SFO"]
  FORMAT:
    type: string
    description: Output format
    default: "3"
    examples: ["1", "3"]
function-calling:
  schema-generation:
    include-descriptions: true
    include-defaults: true
    example-generation: true
```

Would generate a function schema like:

```json
{
  "name": "weather-lookup",
  "description": "Get weather for a location",
  "parameters": {
    "type": "object",
    "properties": {
      "LOCATION": {
        "type": "string",
        "description": "City or airport code"
      },
      "FORMAT": {
        "type": "string",
        "description": "Output format",
        "default": "3"
      }
    },
    "required": ["LOCATION"]
  }
}
```

When the LLM encounters this schema, it can generate appropriate function calls based on user queries like "What's the weather in Seattle?" or "Show me the weather forecast for New York in detailed format."
    example-generation: true
```

### Security Configuration

Control execution privileges and security boundaries:

```yaml
security:
  execution-privilege: same-as-user  # Options: same-as-user, reduced, elevated
  isolation: process                 # Options: none, process, container
  required-permissions:
    - "filesystem:write:{DIRECTORY}"
    - "network:external:api.github.com"
  justification: "Required for cloning GitHub repositories"
  logging:
    enabled: true                   # Whether to log tool execution
    level: info                     # Log level (debug, info, warning, error)
    include-parameters: true        # Whether to include parameter values in logs
    sensitive-parameters: ["TOKEN"] # Parameters that should be masked in logs
  auditing:
    enabled: true                   # Whether to audit tool execution
    record-command: true            # Whether to record the executed command
    record-output: false            # Whether to record command output
    retention-period: 30d           # How long to retain audit records
```

### Tool Composition

Tools can use other tools as steps:

```yaml
steps:
  - name: clone-repo
    use-tool: git-clone
    with:
      URL: "{REPO_URL}"
      DIRECTORY: "{WORKSPACE}"
```

### Tool Dependencies

Define dependencies on other tools and specific versions:

```yaml
dependencies:
  - tool: git-clone
    version: ">=1.0.0"
    required: true
  - tool: npm-build
    version: "^2.0.0"
    required: false
  - tool: docker-run
    version: "1.x"
    scope: any
```

Dependencies can specify:
- `tool`: Name of the required tool
- `version`: Version constraint using semver notation
- `required`: Whether the dependency is required or optional
- `scope`: Scope to search for the tool (local, user, global, any)

When a tool is executed, its dependencies are checked and must be satisfied before execution can proceed.

### Tool Aliasing

Create simplified versions of tools with preset parameters:

```yaml
name: clone-my-repo
type: alias
base-tool: git-clone
default-parameters:
  URL: "https://github.com/my-org/my-repo.git"
  DIRECTORY: "./repos/{BRANCH}"
```

### Testing Configuration

Define tests to verify tool functionality:

```yaml
tests:
  - name: basic-test
    description: "Test basic functionality"
    parameters:
      URL: "https://example.com/repo.git"
      DIRECTORY: "test-dir"
    expected:
      exit-code: 0
      output-contains: "Cloning into 'test-dir'..."
      file-exists: "test-dir/.git/config"
    cleanup:
      - "rm -rf test-dir"
```

## Command Line Interface

### Tool Management Commands

```
cycod tool add NAME [options]
cycod tool get NAME [--scope]
cycod tool list [--scope]
cycod tool remove NAME [--scope]
cycod tool test NAME [--test TEST_NAME] [--scope]
cycod tool export NAME [--scope] [--output FILE]
cycod tool import FILE [--scope]
cycod tool debug NAME [--parameters...] [--scope]
cycod tool logs NAME [--limit N] [--scope]
```

### Tool Add Command

```
cycod tool add NAME --description "DESC" [options]
```

Options:
- `--description "DESC"` - Human-readable description of the tool (required)
- `--bash "COMMAND"` - Bash command to execute
- `--cmd "COMMAND"` - Windows CMD command to execute
- `--pwsh "COMMAND"` - PowerShell command to execute
- `--run "COMMAND"` - Direct command to execute
- `--script "SCRIPT"` - Script content (use with --shell)
- `--shell "SHELL"` - Shell to use with script

- `--step NAME "COMMAND"` - Define a step (can be used multiple times for multi-step tools)
  - `--continue-on-error` - Continue to next step even if this step fails
  - `--run-condition "CONDITION"` - Condition for when to run this step
  - `--parallel` - Run this step in parallel with previous steps
  - `--wait-for "STEP1,STEP2"` - Wait for specific steps to complete
  - `--retry-attempts N` - Number of retry attempts
  - `--retry-delay MS` - Delay between retries in milliseconds
  - `--fallback "COMMAND"` - Fallback command if step fails
  - `--output-mode MODE` - Output mode (buffer or stream)
  - `--buffer-limit SIZE` - Maximum buffer size
  - `--use-tool TOOL_NAME` - Use another tool for this step
  - `--with KEY=VALUE` - Parameters to pass to the used tool

- `--parameter NAME "DESC"` [param-options] - Define a parameter (multiple allowed)
  - `type=string|number|boolean|array|object` - Parameter type
  - `required=true|false` - Whether parameter is required
  - `default=VALUE` - Default value if not provided
  - `validation-min=VALUE` - Minimum value (for number type)
  - `validation-max=VALUE` - Maximum value (for number type)
  - `validation-pattern="PATTERN"` - Regex pattern (for string type)
  - `transform="TRANSFORM"` - Transform function to apply
  - `example="EXAMPLE"` - Example value (can be used multiple times)
  - `escape-shell=true|false` - Whether to escape shell metacharacters

- `--env NAME=VALUE` - Define an environment variable
- `--inherit-env true|false` - Whether to inherit parent environment

- `--security-privilege LEVEL` - Execution privilege level
- `--security-isolation MODE` - Isolation mode
- `--security-permission PERM` - Required permission (multiple allowed)
- `--security-justification "TEXT"` - Justification for permissions
- `--security-logging-enabled true|false` - Enable logging
- `--security-logging-level LEVEL` - Log level (debug, info, warning, error)
- `--security-logging-include-params true|false` - Include parameter values in logs
- `--security-logging-sensitive-param NAME` - Parameter to mask in logs (multiple allowed)
- `--security-auditing-enabled true|false` - Enable auditing
- `--security-auditing-record-command true|false` - Record executed command
- `--security-auditing-record-output true|false` - Record command output
- `--security-auditing-retention PERIOD` - Audit retention period

- `--function-param-mapping TYPE=JSON_TYPE` - Parameter type mapping for function calling
- `--function-include-descriptions true|false` - Include descriptions in function schema
- `--function-include-defaults true|false` - Include defaults in function schema
- `--function-example-generation true|false` - Generate examples in function schema

- `--file-paths-normalize true|false` - Whether to normalize file paths
- `--file-paths-working-dir "DIR"` - Working directory for file paths
- `--file-paths-temp-dir "DIR"` - Temporary directory for file paths
- `--file-paths-auto-convert true|false` - Auto-convert path separators

- `--category CATEGORY` - Category for tool
- `--subcategory SUBCATEGORY` - Subcategory for tool
- `--search-keyword KEYWORD` - Search keyword (multiple allowed)

- `--resource-timeout MS` - Timeout in milliseconds
- `--resource-max-memory SIZE` - Maximum memory usage
- `--resource-cleanup "COMMAND"` - Cleanup command (multiple allowed)

- `--dependency TOOL_NAME` - Required tool dependency (multiple allowed)
  - `version="VERSION_CONSTRAINT"` - Version constraint (e.g., ">=1.0.0")
  - `required=true|false` - Whether dependency is required
  - `scope=local|user|global|any` - Scope to search for dependency

- `--debugging-verbose true|false` - Enable verbose output
- `--debugging-dry-run true|false` - Show commands without executing
- `--debugging-log-file PATH` - Log file location

- `--interactive true|false` - Whether tool is interactive
- `--interactive-timeout MS` - Timeout for user input
- `--interactive-default "RESPONSE"` - Default response if no input provided

- `--version VERSION` - Tool version
- `--min-cycod-version VERSION` - Minimum CYCOD version required
- `--changelog-version VERSION --changelog-changes "TEXT"` - Add changelog entry

- `--test NAME "DESC"` - Add a test (multiple allowed)
  - `--test-param NAME=VALUE` - Test parameter
  - `--test-expect-exit-code CODE` - Expected exit code
  - `--test-expect-contains "TEXT"` - Expected output text
  - `--test-expect-file-exists "PATH"` - Expected file to exist
  - `--test-cleanup "COMMAND"` - Cleanup command for test

- `--platform PLATFORM` - Supported platform (windows|linux|macos, multiple allowed)
- `--tag TAG` - Tag for categorization (multiple allowed)
- `--timeout MILLISECONDS` - Default timeout
- `--working-directory DIR` - Working directory
- `--input "INPUT"` - Data to pass via stdin

- `--from-file PATH` - Load tool definition from file
- `--editor` - Open tool definition in default editor

- `--scope local|user|global` - Scope for the tool (default: local)

Example:
```
cycod tool add weather-lookup \
  --description "Get weather for a location" \
  --bash "curl -s 'wttr.in/{LOCATION}?format={FORMAT}'" \
  --parameter LOCATION "City or airport code" required=true \
  --parameter FORMAT "Output format" default=3 \
  --tag weather --tag api --tag read \
  --timeout 10000
```

Advanced example:
```
cycod tool add github-workflow \
  --description "Run a complete GitHub workflow" \
  --step clone "git clone {REPO_URL} {WORKSPACE}" \
  --step build "cd {WORKSPACE} && npm install && npm run build" \
  --step test "cd {WORKSPACE} && npm test" \
    --retry-attempts 3 --retry-delay 1000 \
    --run-condition "{build.exit-code} == 0" \
  --step cleanup "rm -rf {WORKSPACE}" \
    --continue-on-error true \
  --parameter REPO_URL "GitHub repository URL" required=true \
  --parameter WORKSPACE "Working directory" default="./workspace" \
  --resource-timeout 300000 \
  --security-permission "filesystem:write:{WORKSPACE}" \
  --tag github --tag build --tag write
```

### Tool Get Command

```
cycod tool get NAME [--scope]
```

Options:
- `--scope` - Scope to search in (local, user, global, any)
- `--format json|yaml` - Output format

Example:
```
cycod tool get weather-lookup
cycod tool get weather-lookup --scope user --format json
```

### Tool List Command

```
cycod tool list [--scope] [options]
```

Options:
- `--scope` - Scope to list from (local, user, global, any)
- `--tag TAG` - Filter by tag
- `--category CATEGORY` - Filter by category
- `--platform PLATFORM` - Filter by platform
- `--format table|json|yaml` - Output format

Example:
```
cycod tool list
cycod tool list --scope global --tag api --format json
```

### Tool Remove Command

```
cycod tool remove NAME [--scope]
```

Options:
- `--scope` - Scope to remove from (local, user, global, any)
- `--force` - Force removal without confirmation

Example:
```
cycod tool remove weather-lookup
cycod tool remove weather-lookup --scope user --force
```

### Tool Test Command

```
cycod tool test NAME [--test TEST_NAME] [--scope]
```

Options:
- `--test TEST_NAME` - Run specific test (default: all tests)
- `--scope` - Scope to search in (local, user, global, any)
- `--verbose` - Show detailed test output

Example:
```
cycod tool test github-repo-clone
cycod tool test github-repo-clone --test basic-test --verbose
```

### Tool Export Command

```
cycod tool export NAME [--scope] [--output FILE]
```

Options:
- `--scope` - Scope to search in (local, user, global, any)
- `--output FILE` - Output file (default: stdout)
- `--format json|yaml` - Output format (default: yaml)

Example:
```
cycod tool export weather-lookup --output weather-lookup.yaml
```

### Tool Import Command

```
cycod tool import FILE [--scope]
```

Options:
- `--scope` - Scope to import to (local, user, global)
- `--overwrite` - Overwrite existing tool if it exists

Example:
```
cycod tool import weather-lookup.yaml --scope user
```

## Parameter Substitution

### Parameter References

Parameters are referenced in commands using curly braces:

```yaml
bash: command {PARAM1} {PARAM2}
```

### Dynamic Parameter References

Parameters can reference other parameters in their default values:

```yaml
parameters:
  REPO:
    type: string
    description: Repository name
    required: true
  
  OUTPUT_DIR:
    type: string
    description: Output directory
    default: "./checkout/{REPO}"  # References another parameter
```

### Parameter Transformations

Parameter values can be transformed before they are substituted into commands. This allows for formatting, type conversion, validation, and other operations to be performed on parameter values.

```yaml
parameters:
  USERNAME:
    type: string
    description: Username
    transform: "value.toLowerCase()"  # Convert to lowercase
  CASE_SENSITIVE:
    type: boolean
    description: Whether to use case-sensitive search
    default: false
    transform: "value ? '' : '-i'"  # Empty string for true, -i for false
  COUNT:
    type: number
    description: Number of items
    default: 10
    transform: "Math.max(1, Math.min(100, Math.floor(value)))"  # Clamp between 1-100 and ensure integer
```

#### Transform Functions

Transform functions are JavaScript expressions that operate on the parameter value. The value is available as the variable `value` in the expression. The result of the expression becomes the substituted value.

Common transformation patterns:

1. **Type conversion**:
   ```yaml
   transform: "Number(value)"  # Convert string to number
   transform: "String(value)"  # Convert any value to string
   transform: "Boolean(value)"  # Convert to boolean
   ```

2. **String operations**:
   ```yaml
   transform: "value.toLowerCase()"  # Convert to lowercase
   transform: "value.toUpperCase()"  # Convert to uppercase
   transform: "value.trim()"         # Remove whitespace
   transform: "value.replace(/\s+/g, '-')"  # Replace spaces with hyphens
   ```

3. **Conditional transformations**:
   ```yaml
   transform: "value ? '-v' : ''"    # -v if true, empty if false
   transform: "value === 'auto' ? '' : '--format=' + value"  # Handle special values
   ```

4. **Numeric operations**:
   ```yaml
   transform: "Math.floor(value)"    # Round down to integer
   transform: "Math.max(0, value)"   # Ensure non-negative
   transform: "value * 1024"         # Convert KB to bytes
   ```

#### Chaining with Parameter Format

Transformations can be combined with parameter formatting:

```yaml
parameters:
  LIMIT:
    type: number
    description: Maximum number of results
    default: 10
    transform: "Math.max(1, Math.min(100, Math.floor(value)))"  # Ensure integer between 1-100
    format: "--limit={value}"  # Format as command line argument
```

In this example, the parameter value is first transformed to ensure it's an integer between 1 and 100, then formatted as a command line argument.

### Step Output References

In multi-step tools, the output of previous steps can be referenced:

```yaml
steps:
  - name: step1
    bash: command {PARAM}
  - name: step2
    bash: another-command {step1.output}
```

Additional properties available for steps:
- `{step1.output}` - Standard output from the step
- `{step1.error}` - Standard error from the step
- `{step1.exit-code}` - Exit code from the step

### Default Values

Parameters with default values don't need to be provided when calling the tool:

```yaml
parameters:
  FORMAT:
    type: string
    description: Output format
    default: "3"
```

## Security Model

### Integration with Auto-Approve/Auto-Deny

Custom tools integrate with CYCOD's existing auto-approve/auto-deny system:

```
cycod --auto-approve tool:read      # Auto-approve all tools tagged as 'read'
cycod --auto-approve tool:weather   # Auto-approve all tools tagged as 'weather'
```

### Security Tags

Tools should be tagged with one or more security tags to indicate their risk level:
- `read`: Tools that only read data, no modification or execution
- `write`: Tools that modify files or data
- `run`: Tools that execute other commands or scripts

If no security tag is provided, the tool is considered high-risk and requires explicit approval.

### Execution Privileges

Control the privileges used when executing commands:

```yaml
security:
  execution-privilege: same-as-user  # Options: same-as-user, reduced, elevated
```

Options:
- `same-as-user`: Execute with the same privileges as the user (default)
- `reduced`: Execute with reduced privileges (more secure)
- `elevated`: Execute with elevated privileges (requires approval)

### Required Permissions

Explicitly declare permissions required by the tool:

```yaml
security:
  required-permissions:
    - "filesystem:write:{DIRECTORY}"
    - "network:external:api.github.com"
  justification: "Required for cloning GitHub repositories"
```

This helps users understand the security implications before running the tool.

## Best Practices

### Tool Design Principles

1. **Single Responsibility**: Each tool should do one thing well.
2. **Clear Interface**: Parameters should be well-named with helpful descriptions.
3. **Resilience**: Handle errors gracefully with appropriate retry logic.
4. **Documentation**: Provide detailed help and examples.
5. **Security**: Minimize required permissions and use appropriate security tags.

### Parameter Naming Conventions

1. Use `UPPER_CASE` for parameter names.
2. Use descriptive names that indicate the purpose of the parameter.
3. Group related parameters with common prefixes.
4. Use consistent naming across similar tools.

### Error Handling Patterns

1. Always validate input parameters before executing commands.
2. Use appropriate retry logic for operations that might fail transiently.
3. Provide meaningful error messages.
4. Clean up temporary resources even when errors occur.

### Consistency Guidelines

1. **Naming Conventions**:
   - Use kebab-case for property names in YAML (e.g., `execution-privilege`, `file-paths`)
   - Use `UPPER_CASE` for parameter names
   - Use lowercase for platform identifiers in YAML (windows, linux, macos)
   - Use proper case (Windows, Linux, macOS) in descriptive text and comments

2. **Type Naming**:
   - Use standard types consistently: `string`, `number`, `boolean`, `array`, `object`
   - Use consistent validation property names: `minimum`, `maximum`, `pattern`, etc.

3. **Security Tags**:
   - Always include at least one security tag (`read`, `write`, `run`) on every tool
   - Use `read` for tools that only read data
   - Use `write` for tools that modify files or data
   - Use `run` for tools that execute other commands or scripts

4. **Documentation**:
   - Include a clear description for every tool and parameter
   - Provide examples for complex parameters
   - Include detailed help for parameters with multiple options
   - Document security implications and required permissions

5. **Step Naming**:
   - Use descriptive names for steps that indicate their purpose
   - Use consistent prefixes for related steps (e.g., `build-frontend`, `build-backend`)
   - Avoid special characters in step names

6. **Environment Variables**:
   - Use `UPPER_CASE` for environment variable names
   - Use prefixes to group related environment variables
   - Document the purpose of each environment variable

### Performance Considerations

1. **Minimize Command Execution**: Combine commands where possible to reduce process creation overhead.
2. **Use Parallel Execution**: Run independent tasks in parallel to reduce total execution time.
3. **Optimize Output Handling**: Use streaming for large outputs to avoid memory issues.
4. **Buffer Management**: Set appropriate buffer limits based on expected output size.
5. **Resource Constraints**: Set reasonable timeouts and memory limits to prevent runaway processes.
6. **Avoid Polling**: Use event-based mechanisms rather than polling where possible.
7. **Caching**: Consider caching results for expensive operations that are repeated frequently.
8. **Cleanup Efficiently**: Remove temporary files and resources as soon as they're no longer needed.

### Cross-Platform Compatibility

1. Use platform-specific commands when necessary.
2. Normalize file paths to handle differences between platforms.
3. Be mindful of line ending differences between platforms.
4. Test on all supported platforms before publishing tools.

## Implementation Considerations

### Parameter Substitution and Escaping

Parameters are substituted into commands using the following rules:
1. Parameter references are enclosed in curly braces: `{PARAM}`.
2. If `escape-shell` is true, shell metacharacters are escaped.
3. If a parameter has a format specified, the value is formatted accordingly.
4. If a parameter has a transform specified, the transformation is applied before substitution.

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

### Security Enforcement

Security is enforced through the following mechanisms:
1. Security tags determine auto-approval/denial.
2. Execution privileges control the level of access the tool has.
3. Required permissions are validated before the tool is run.
4. Parameter escaping prevents command injection.

### Cross-Platform Compatibility

Cross-platform compatibility is a key consideration for custom tools, especially when they need to work across different operating systems with different command syntaxes, path formats, and environment behaviors.

#### Path Normalization

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

#### Working Directory Resolution

The working directory should be resolved with cross-platform considerations:

```csharp
private string ResolveWorkingDirectory(ToolDefinition tool, StepDefinition step, Dictionary<string, object> parameters)
{
    // Get the working directory with parameter substitution
    string workingDir = step?.WorkingDirectory ?? tool.WorkingDirectory ?? Directory.GetCurrentDirectory();
    workingDir = SubstituteParameters(workingDir, parameters);
    
    // Handle special paths like ~ for home directory
    if (workingDir.StartsWith("~"))
    {
        string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        workingDir = Path.Combine(homePath, workingDir.Substring(1));
    }
    
    // Handle relative paths
    if (!Path.IsPathRooted(workingDir))
    {
        workingDir = Path.GetFullPath(workingDir);
    }
    
    // Normalize path separators if configured
    if (tool.FilePaths?.Normalize == true)
    {
        workingDir = NormalizePath(workingDir);
    }
    
    return workingDir;
}
```

#### Platform-Specific Commands

For tools with platform-specific commands, the implementation should select the appropriate command based on the current operating system:

```csharp
private string SelectPlatformCommand(PlatformCommands commands)
{
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && 
        !string.IsNullOrEmpty(commands.Windows))
    {
        return commands.Windows;
    }
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && 
             !string.IsNullOrEmpty(commands.Linux))
    {
        return commands.Linux;
    }
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && 
             !string.IsNullOrEmpty(commands.MacOS))
    {
        return commands.MacOS;
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

#### Shell Selection

The implementation should select the appropriate shell based on the platform:

```csharp
private (string shell, string args) SelectShell(string shellName)
{
    if (string.IsNullOrEmpty(shellName))
    {
        // Use default shell for platform
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return ("cmd.exe", "/c");
        }
        else
        {
            return ("/bin/sh", "-c");
        }
    }
    
    // Handle specific shell requests
    switch (shellName.ToLowerInvariant())
    {
        case "bash":
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // On Windows, look for Git Bash or WSL bash
                if (File.Exists("C:\\Program Files\\Git\\bin\\bash.exe"))
                {
                    return ("C:\\Program Files\\Git\\bin\\bash.exe", "-c");
                }
                else
                {
                    return ("wsl", "bash -c");
                }
            }
            else
            {
                return ("/bin/bash", "-c");
            }
            
        case "powershell":
        case "pwsh":
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return ("powershell.exe", "-Command");
            }
            else
            {
                return ("pwsh", "-Command");
            }
            
        case "cmd":
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return ("cmd.exe", "/c");
            }
            else
            {
                // Fall back to sh on non-Windows
                return ("/bin/sh", "-c");
            }
            
        default:
            // Assume it's a path to a shell
            return (shellName, "-c");
    }
}
```

#### Environment Variables

Environment variables should be handled with platform-specific considerations:

```csharp
private Dictionary<string, string> BuildEnvironmentVariables(ToolDefinition tool, Dictionary<string, object> parameters)
{
    var env = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    
    // Inherit from parent if specified
    if (tool.Environment?.Inherit == true)
    {
        foreach (var kvp in Environment.GetEnvironmentVariables())
        {
            if (kvp.Key is string key && kvp.Value is string value)
            {
                env[key] = value;
            }
        }
    }
    
    // Add tool-level environment variables
    if (tool.Environment?.Variables != null)
    {
        foreach (var kvp in tool.Environment.Variables)
        {
            env[kvp.Key] = SubstituteParameters(kvp.Value, parameters);
        }
    }
    
    // Add platform-specific environment variables
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        // Windows-specific env vars
        env["SYSTEMROOT"] = env.TryGetValue("SYSTEMROOT", out var sysRoot) ? 
            sysRoot : Environment.GetFolderPath(Environment.SpecialFolder.Windows);
    }
    else
    {
        // Unix-specific env vars
        env["HOME"] = env.TryGetValue("HOME", out var home) ? 
            home : Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    }
    
    // Add built-in variables
    env["CYCOD_TOOL_NAME"] = tool.Name;
    env["CYCOD_TOOL_VERSION"] = tool.Version ?? "1.0.0";
    env["CYCOD_WORKING_DIR"] = context.WorkingDirectory;
    env["CYCOD_OS"] = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? 
        "windows" : (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "linux" : "macos");
    
    return env;
}
```

### Command Execution Context

The command execution context defines the environment in which commands are executed:

```csharp
private ExecutionContext CreateExecutionContext(ToolDefinition tool, StepDefinition step, Dictionary<string, object> parameters)
{
    var context = new ExecutionContext
    {
        WorkingDirectory = ResolveWorkingDirectory(tool, step),
        Environment = BuildEnvironmentVariables(tool, parameters),
        Timeout = step?.Timeout ?? tool.Timeout ?? DefaultTimeout,
        StandardInput = ResolveStandardInput(tool, parameters),
        SecurityContext = BuildSecurityContext(tool)
    };
    
    return context;
}
```

#### Working Directory Resolution

Working directories are resolved with the following priority:
1. Step-specific working directory
2. Tool-level working directory
3. Current directory

```csharp
private string ResolveWorkingDirectory(ToolDefinition tool, StepDefinition step)
{
    string workingDir = step?.WorkingDirectory ?? tool.WorkingDirectory ?? Directory.GetCurrentDirectory();
    workingDir = SubstituteParameters(workingDir, parameters);
    
    if (tool.FilePaths?.Normalize == true)
    {
        workingDir = NormalizePath(workingDir);
    }
    
    return workingDir;
}
```

#### Environment Variables

Environment variables are built from:
1. Parent process environment (if `inherit: true`)
2. Tool-level environment variables
3. Step-specific environment variables

```csharp
private Dictionary<string, string> BuildEnvironmentVariables(ToolDefinition tool, Dictionary<string, object> parameters)
{
    var env = new Dictionary<string, string>();
    
    // Inherit from parent if specified
    if (tool.Environment?.Inherit == true)
    {
        foreach (var kvp in Environment.GetEnvironmentVariables())
        {
            if (kvp.Key is string key && kvp.Value is string value)
            {
                env[key] = value;
            }
        }
    }
    
    // Add tool-level environment variables
    if (tool.Environment?.Variables != null)
    {
        foreach (var kvp in tool.Environment.Variables)
        {
            env[kvp.Key] = SubstituteParameters(kvp.Value, parameters);
        }
    }
    
    // Add built-in variables
    env["CYCOD_TOOL_NAME"] = tool.Name;
    env["CYCOD_TOOL_VERSION"] = tool.Version ?? "1.0.0";
    env["CYCOD_WORKING_DIR"] = context.WorkingDirectory;
    
    return env;
}
```

#### Security Context

The security context controls execution privileges and isolation:

```csharp
private SecurityContext BuildSecurityContext(ToolDefinition tool)
{
    return new SecurityContext
    {
        ExecutionPrivilege = tool.Security?.ExecutionPrivilege ?? ExecutionPrivilege.SameAsUser,
        Isolation = tool.Security?.Isolation ?? Isolation.Process,
        RequiredPermissions = tool.Security?.RequiredPermissions ?? new List<string>()
    };
}
```

### Output Capturing and Streaming

For steps with `output.mode: buffer`, the implementation should:
1. Capture the standard output into a buffer
2. Respect the `buffer-limit` setting
3. Make the output available for reference by subsequent steps

For steps with `output.mode: stream`, the implementation should:
1. Stream the output to the specified destination
2. Not buffer more than necessary
3. Handle backpressure appropriately

### Parallel Execution

For tools with parallel steps, the implementation should:
1. Build a dependency graph based on `wait-for` relationships
2. Execute independent steps in parallel
3. Wait for dependencies to complete before executing dependent steps
4. Handle step failures and conditions appropriately

### Resource Management and Cleanup

The implementation should properly manage process lifecycle to avoid resource leaks:
1. Create processes with appropriate settings
2. Monitor execution time and enforce timeouts
3. Clean up resources when done
4. Handle process termination

The implementation should ensure cleanup actions are executed:
1. Run step-specific cleanup commands
2. Execute global cleanup actions
3. Clean up temporary files
4. Release system resources

### Function Calling Integration

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

## Debugging and Troubleshooting

### Debugging Tools

Custom tools can be debugged using the following techniques:

#### Debug Mode

```
cycod tool run NAME --debug [--parameters...]
```

In debug mode, CYCOD will:
1. Show each step being executed
2. Display parameter substitutions
3. Show environment variables
4. Log all shell commands before execution
5. Output detailed error information

#### Verbose Output

```
cycod tool run NAME --verbose [--parameters...]
```

Verbose mode provides detailed output but less than debug mode, focusing on execution progress.

#### Dry Run

```
cycod tool run NAME --dry-run [--parameters...]
```

Dry run mode shows what commands would be executed without actually running them, useful for validating complex tools.

### Execution Context

The execution context includes:

1. **Variable Scope**: Each step has access to:
   - Tool parameters
   - Environment variables
   - Output from previous steps
   - Special variables like `{TOOL_NAME}`, `{WORKSPACE}`, `{TEMP}`

2. **Working Directory**: The working directory can be set at the tool level or step level
   ```yaml
   working-directory: ~/projects    # Tool-level setting
   steps:
     - name: step1
       working-directory: ./src     # Step-level override
   ```

3. **Environment Isolation**: Environment variables can be:
   - Inherited from parent process (`environment.inherit: true`)
   - Defined explicitly in the tool (`environment.variables`)
   - Isolated per step

### Common Issues and Solutions

| Issue | Possible Causes | Solutions |
|-------|----------------|-----------|
| Command not found | Missing dependency, incorrect PATH | Install required dependency, set PATH environment variable |
| Permission denied | Insufficient privileges | Use appropriate security tags, adjust file permissions |
| Timeout errors | Long-running commands | Increase timeout setting, optimize command |
| Path issues | Cross-platform incompatibility | Use `file-paths.normalize: true`, use platform-specific commands |
| Parameter substitution fails | Incorrect parameter reference | Check parameter names, use parameter validation |

### Logging

Tool execution logs are stored in:
- Local scope: `./.cycod/logs/`
- User scope: `~/.cycod/logs/`

Access logs with:
```
cycod tool logs NAME [--limit N] [--scope]
```

## Testing Framework

### Overview

The Tool Testing Framework provides a structured approach for testing custom tools before deployment, ensuring they work as expected and handle various inputs correctly.

### Test Definition

Tests are defined in the tool YAML file under the `tests` section:

```yaml
tests:
  # Optional setup for all tests
  setup:
    - "mkdir -p test-data"
    - "echo 'test content' > test-data/input.txt"
  
  # Test cases
  - name: basic-test
    description: "Test basic functionality"
    parameters:
      URL: "https://example.com/repo.git"
      DIRECTORY: "test-dir"
    expected:
      exit-code: 0
      output-contains: "Cloning into 'test-dir'..."
      file-exists: "test-dir/.git/config"
    cleanup:
      - "rm -rf test-dir"
      
  # Optional global cleanup after all tests
  cleanup:
    - "rm -rf test-data"
```

### Test Components

Each test case includes:
- `name`: A unique identifier for the test
- `description`: Human-readable description of what the test verifies
- `parameters`: The parameters to pass to the tool
- `expected`: Expected results and assertions
- `cleanup`: Commands to run after the test completes

### Assertions

The testing framework supports various assertions:
- `exit-code`: Expected exit code from the tool
- `output-contains`: String or pattern that should be in the output
- `output-not-contains`: String or pattern that should not be in the output
- `output-matches`: Regular expression the output should match
- `file-exists`: File that should exist after execution
- `file-not-exists`: File that should not exist after execution
- `file-contains`: File that should contain specific content
- `error`: Whether an error is expected

### Cleanup

The `cleanup` section defines commands to run after the test completes, regardless of whether it passed or failed. This ensures the system is returned to a clean state.

### Running Tests

Tests can be run through the command line:

```
cycod tool test NAME [--test TEST_NAME] [--scope]
```

Options:
- `--test TEST_NAME`: Run a specific test (default: all tests)
- `--scope`: Scope to find the tool in (local, user, global, any)

### Integration with CI/CD

The testing framework integrates with CI/CD pipelines:

```yaml
# In CI configuration
tests:
  environment:
    CYCOD_TEST_MODE: true
  setup:
    - "mkdir -p test-data"
    - "echo 'test content' > test-data/sample.txt"
  cleanup:
    - "rm -rf test-data"
```

## Help Text

### Tool Help Command

```
cycod help tool
```

Output:
```
CYCOD TOOL COMMANDS

  These commands allow you to manage custom tools for CYCOD.

USAGE: cycod tool list [--scope]
   OR: cycod tool get TOOL_NAME [--scope]
   OR: cycod tool add TOOL_NAME [options]
   OR: cycod tool remove TOOL_NAME [--scope]
   OR: cycod tool test TOOL_NAME [--test TEST_NAME] [--scope]
   OR: cycod tool export TOOL_NAME [--scope] [--output FILE]
   OR: cycod tool import FILE [--scope]

OPTIONS

  SCOPE OPTIONS

    --global, -g    Manage tools from global scope (all users)
    --user, -u      Manage tools from user scope (current user)
    --local, -l     Manage tools from local scope (default for most commands)
    --any, -a       Manage tools from all scopes (default for 'list' and 'get' commands)

COMMANDS

    list            List all available tools
    get             Display the details of a specific tool
    add             Create a new tool
    remove          Delete a tool
    test            Test a tool
    export          Export a tool definition
    import          Import a tool definition
    debug           Debug a tool
    logs            View tool execution logs

SEE ALSO

  cycod help tool list
  cycod help tool get
  cycod help tool add
  cycod help tool remove
  cycod help tool test
  cycod help tool export
  cycod help tool import
  cycod help tool debug
  cycod help tool logs
  cycod help tools
```

## Examples

### Simple Single-Step Tool Example

```yaml
name: weather-lookup
description: Get weather for a location
version: 1.0.0
min-cycod-version: 1.0.0

bash: curl -s 'wttr.in/{LOCATION}?format={FORMAT}'

parameters:
  LOCATION:
    type: string
    description: City or airport code
    required: true
    examples: ["New York", "SFO", "London"]
  FORMAT:
    type: string
    description: Output format
    required: false
    default: "3"
    examples: ["1", "2", "3", "4"]
    detailed-help: |
      Format options:
      1 - Brief forecast
      2 - Compact forecast
      3 - Simple forecast (default)
      4 - Detailed forecast

timeout: 10000
tags: [weather, api, read]
platforms: [windows, linux, macos]

function-calling:
  schema-generation:
    include-descriptions: true
    include-defaults: true
    example-generation: true
```

### Multi-Step Tool with Output Capturing

```yaml
name: github-commit-stats
description: Get statistics for recent commits in a GitHub repository
version: 1.0.0

steps:
  - name: fetch-commits
    bash: curl -s "https://api.github.com/repos/{OWNER}/{REPO}/commits?per_page=10"
    output:
      mode: buffer
      buffer-limit: 5MB

  - name: count-authors
    bash: echo '{fetch-commits.output}' | jq 'group_by(.commit.author.name) | map({author: .[0].commit.author.name, count: length}) | sort_by(.count) | reverse'

  - name: format-output
    bash: echo '{count-authors.output}' | jq -r '.[] | "\(.author): \(.count) commits"'

parameters:
  OWNER:
    type: string
    description: Repository owner username
    required: true
  REPO:
    type: string
    description: Repository name
    required: true

security:
  execution-privilege: same-as-user
  required-permissions:
    - "network:external:api.github.com"
  justification: "Required for accessing GitHub API"

tags: [github, api, read]
timeout: 15000
```

### Tool with Conditional Steps and Error Handling

```yaml
name: github-repo-clone
description: Clone a GitHub repository with fallback methods
version: 1.0.0

steps:
  - name: try-https
    bash: git clone https://github.com/{OWNER}/{REPO}.git {OUTPUT_DIR}
    continue-on-error: true
    error-handling:
      retry:
        attempts: 3
        delay: 1000  # milliseconds

  - name: try-ssh
    bash: git clone git@github.com:{OWNER}/{REPO}.git {OUTPUT_DIR}
    run-condition: "{try-https.exit-code} != 0"
    continue-on-error: true

  - name: report-status
    bash: |
      if [ -d "{OUTPUT_DIR}/.git" ]; then
        echo "Successfully cloned {OWNER}/{REPO}"
      else
        echo "Failed to clone {OWNER}/{REPO} using both HTTPS and SSH"
        exit 1
      fi

parameters:
  OWNER:
    type: string
    description: Repository owner username
    required: true
  REPO:
    type: string
    description: Repository name
    required: true
  OUTPUT_DIR:
    type: string
    description: Output directory for the clone
    default: "{REPO}"

security:
  execution-privilege: same-as-user
  required-permissions:
    - "filesystem:write:{OUTPUT_DIR}"
    - "network:external:github.com"
  justification: "Required for cloning GitHub repositories"

tags: [github, git, clone, write]
platforms: [windows, linux, macos]

tests:
  - name: basic-test
    description: "Test basic cloning functionality"
    parameters:
      OWNER: "microsoft"
      REPO: "vscode"
      OUTPUT_DIR: "test-vscode"
    expected:
      exit-code: 0
      output-contains: "Successfully cloned"
      file-exists: "test-vscode/.git/config"
    cleanup:
      - "rm -rf test-vscode"
```

### Tool with Complex Parameter Handling

```yaml
name: search-code
description: Search for patterns in code files
version: 1.0.0

commands:
  default: grep -r {CASE_SENSITIVE} {WHOLE_WORD} "{PATTERN}" {DIRECTORY} --include="*.{FILE_TYPE}"
  platforms:
    windows: powershell -Command "Get-ChildItem -Path '{DIRECTORY}' -Filter '*.{FILE_TYPE}' -Recurse | Select-String -Pattern '{PATTERN}' {CASE_SENSITIVE} {WHOLE_WORD}"

parameters:
  PATTERN:
    type: string
    description: Pattern to search for
    required: true
    security:
      escape-shell: true

  DIRECTORY:
    type: string
    description: Directory to search in
    default: "."
    validation:
      pattern: "^[^<>:\"\\|?*]+$"  # Valid directory name

  FILE_TYPE:
    type: string
    description: File extension to search in
    default: "*"

  CASE_SENSITIVE:
    type: boolean
    description: Whether to use case-sensitive search
    default: false
    transform: "value ? '' : '-i'"  # Empty string for true, -i for false

  WHOLE_WORD:
    type: boolean
    description: Whether to search for whole words only
    default: false
    transform: "value ? '-w' : ''"  # -w for true, empty string for false

file-paths:
  normalize: true
  working-directory: "{WORKSPACE}"

metadata:
  category: development
  subcategory: code-search
  tags: [search, code, read]
  search-keywords: [find, grep, search, pattern]

tags: [search, code, read]
platforms: [windows, linux, macos]
```

### Tool Alias Example

```yaml
name: search-js
description: Search for patterns in JavaScript files
version: 1.0.0
type: alias
base-tool: search-code
default-parameters:
  FILE_TYPE: "js"
  DIRECTORY: "./src"

tags: [search, javascript, read]
```

### Tool Using Another Tool

```yaml
name: github-workflow
description: Run a complete GitHub workflow
version: 1.0.0

steps:
  - name: clone
    use-tool: github-repo-clone
    with:
      OWNER: "{OWNER}"
      REPO: "{REPO}"
      OUTPUT_DIR: "{WORKSPACE}"

  - name: install
    bash: cd {WORKSPACE} && npm install
    run-condition: "{clone.exit-code} == 0"

  - name: build
    bash: cd {WORKSPACE} && npm run build
    run-condition: "{install.exit-code} == 0"
    error-handling:
      retry:
        attempts: 2
        delay: 1000

  - name: test
    bash: cd {WORKSPACE} && npm test
    run-condition: "{build.exit-code} == 0"
    output:
      mode: stream
      stream-callback: console

  - name: cleanup
    bash: rm -rf {WORKSPACE}
    continue-on-error: true

parameters:
  OWNER:
    type: string
    description: Repository owner username
    required: true
  REPO:
    type: string
    description: Repository name
    required: true
  WORKSPACE:
    type: string
    description: Working directory
    default: "./workspace/{REPO}"

environment:
  variables:
    NODE_ENV: "development"
  inherit: true

resources:
  timeout: 300000  # 5 minutes
  max-memory: 1GB
  cleanup:
    - delete-temp-files: true
    - final-command: "rm -rf {WORKSPACE}"

security:
  execution-privilege: same-as-user
  required-permissions:
    - "filesystem:write:{WORKSPACE}"
    - "network:external:github.com"
    - "network:external:registry.npmjs.org"
  justification: "Required for GitHub workflow"

metadata:
  category: ci-cd
  subcategory: node
  tags: [github, npm, build, test]
  search-keywords: [workflow, pipeline, build, test]

tags: [github, npm, build, write, run]
platforms: [windows, linux, macos]

tests:
  - name: basic-test
    description: "Test basic workflow functionality"
    parameters:
      OWNER: "example"
      REPO: "sample-npm-project"
      WORKSPACE: "./test-workspace"
    expected:
      exit-code: 0
      output-contains: "All tests passed"
    cleanup:
      - "rm -rf ./test-workspace"

changelog:
  - version: 1.0.0
    changes: "Initial release"
```

### Parallel Execution Example

```yaml
name: parallel-build
description: Build multiple projects in parallel
version: 1.0.0

steps:
  - name: setup
    bash: mkdir -p {OUTPUT_DIR}

  - name: build-frontend
    bash: cd {FRONTEND_DIR} && npm run build && cp -r dist/* ../{OUTPUT_DIR}/
    parallel: true
    wait-for: [setup]

  - name: build-backend
    bash: cd {BACKEND_DIR} && mvn package && cp target/*.jar ../{OUTPUT_DIR}/
    parallel: true
    wait-for: [setup]

  - name: build-docs
    bash: cd {DOCS_DIR} && mkdocs build && cp -r site/* ../{OUTPUT_DIR}/docs/
    parallel: true
    wait-for: [setup]

  - name: report
    bash: |
      echo "Build completed:"
      ls -la {OUTPUT_DIR}
    wait-for: [build-frontend, build-backend, build-docs]

parameters:
  FRONTEND_DIR:
    type: string
    description: Frontend project directory
    default: "./frontend"
  BACKEND_DIR:
    type: string
    description: Backend project directory
    default: "./backend"
  DOCS_DIR:
    type: string
    description: Documentation directory
    default: "./docs"
  OUTPUT_DIR:
    type: string
    description: Output directory
    default: "./dist"

resources:
  timeout: 600000  # 10 minutes

tags: [build, parallel, write]
platforms: [linux, macos]
```

### Interactive Tool Example

```yaml
name: interactive-setup
description: Interactive project setup wizard
version: 1.0.0

steps:
  - name: prompt-project-name
    bash: |
      read -p "Enter project name: " PROJECT_NAME
      echo $PROJECT_NAME
    output:
      mode: buffer

  - name: prompt-language
    bash: |
      read -p "Select language (js/ts/python/go): " LANGUAGE
      echo $LANGUAGE
    output:
      mode: buffer

  - name: create-project
    bash: |
      PROJECT_NAME="{prompt-project-name.output}"
      LANGUAGE="{prompt-language.output}"
      
      mkdir -p $PROJECT_NAME
      
      case $LANGUAGE in
        js)
          echo "Creating JavaScript project..."
          cd $PROJECT_NAME && npm init -y
          ;;
        ts)
          echo "Creating TypeScript project..."
          cd $PROJECT_NAME && npm init -y && npm install typescript --save-dev
          ;;
        python)
          echo "Creating Python project..."
          cd $PROJECT_NAME && python -m venv venv && touch requirements.txt
          ;;
        go)
          echo "Creating Go project..."
          cd $PROJECT_NAME && go mod init $PROJECT_NAME
          ;;
        *)
          echo "Unsupported language: $LANGUAGE"
          exit 1
          ;;
      esac
      
      echo "Project created successfully!"

interactive: true
interactive-options:
  timeout: 60000  # 1 minute
  default-response: ""

tags: [setup, interactive, write]
platforms: [windows, linux, macos]
```