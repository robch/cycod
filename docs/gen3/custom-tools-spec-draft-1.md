# CYCOD Custom Tools Specification

## Overview

Custom Tools provide a mechanism for CYCOD users to define, share, and execute shell commands as named tools with parameters. These tools can be used by LLMs through function calling, similar to MCPs but focused on shell command execution.

### Purpose

Custom Tools allow users to:
1. Wrap frequently used shell commands with a consistent interface
2. Define parameters with descriptions and default values
3. Create multi-step tools with conditional execution
4. Share tools across multiple scopes (local, user, global)
5. Categorize tools for organization and security

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
min-cycod-version: "1.0.0"         # Minimum CYCOD version required

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
  default: command {PARAM}         # Default command for all platforms
  platforms:
    windows: command {PARAM}       # Windows-specific command
    linux: command {PARAM}         # Linux-specific command
    macos: command {PARAM}         # macOS-specific command

# OR for multi-step tools
steps:
  - name: step1                    # Step name (required)
    bash: command {PARAM}          # Command to execute
    # OR for platform-specific commands
    commands:
      default: command {PARAM}     # Default command for all platforms
      platforms:
        windows: command {PARAM}   # Windows-specific command
        linux: command {PARAM}     # Linux-specific command
        macos: command {PARAM}     # macOS-specific command
    continue-on-error: false       # Whether to continue if this step fails
    run-condition: "{step2.exit-code} == 0"  # Condition for when to run this step
    parallel: false                # Whether to run in parallel with previous steps
    wait-for: [step2, step3]       # Steps to wait for before running this step
    error-handling:                # Error handling configuration
      retry:
        attempts: 3                # Number of retry attempts
        delay: 1000                # Delay in milliseconds between retries
      fallback: alternative-command {PARAM} # Command to run if all retries fail
    output:
      mode: buffer                 # Output mode: buffer or stream
      buffer-limit: 10MB           # Maximum buffer size
      truncation: true             # Whether to truncate if exceeded
      stream-callback: console     # Where to stream (console, file, function)
  
  - name: step2
    bash: command {PARAM}
    # Or use another tool
    use-tool: another-tool
    with:
      PARAM1: value1
      PARAM2: value2

# Parameters
parameters:
  PARAM1:
    type: string                   # Parameter type (string, number, boolean, array, object)
    description: Parameter description  # Required
    required: true                 # Whether parameter is required
    default: default value         # Default value if not provided
    validation:                    # Validation rules
      minLength: 5                 # Minimum length (for strings)
      maxLength: 100               # Maximum length (for strings)
      pattern: "^[a-z]+$"          # Regex pattern (for strings)
      minimum: 1                   # Minimum value (for numbers)
      maximum: 100                 # Maximum value (for numbers)
      enum: [value1, value2]       # Allowed values
    transform: "lowercase"         # Transform function to apply
    format: "--param={value}"      # How parameter appears in command
    examples: ["example1", "example2"] # Example values
    detailed-help: |
      Detailed help text for the parameter.
      Can span multiple lines.
    security:
      escape-shell: true           # Whether to escape shell metacharacters

  PARAM2:
    type: number
    description: Another parameter
    required: false
    default: 5

# LLM Function Calling Integration
function-calling:
  schema-generation:
    parameter-mapping:
      string: "string"
      number: "number"
      boolean: "boolean"
      array: "array"
      object: "object"
    include-descriptions: true
    include-defaults: true
    example-generation: true

# Security Configuration
security:
  execution-privilege: same-as-user  # Options: same-as-user, reduced, elevated (requires approval)
  isolation: process                 # Options: none, process, container
  required-permissions:
    - "filesystem:write:{DIRECTORY}"
    - "network:external:api.github.com"
  justification: "Required for cloning GitHub repositories"

# Environment Variables
environment:
  variables:
    HTTP_PROXY: "{PROXY_URL}"
    DEBUG: "1"
  inherit: true                    # Inherit parent process environment

# File Path Handling
file-paths:
  normalize: true                  # Convert paths to platform-specific format
  working-directory: "{WORKSPACE}" # Working directory
  temp-directory: "{TEMP}/cycod-tools/{TOOL_NAME}" # Temporary directory
  cross-platform:
    windows-separator: "\\"        # Windows path separator
    unix-separator: "/"            # Unix path separator
    auto-convert: true             # Automatically convert paths

# Metadata for categorization and discovery
metadata:
  category: development
  subcategory: version-control
  tags: [git, repository, clone]
  search-keywords: [git, repo, download, checkout]

# Testing
tests:
  - name: basic-test
    description: "Test basic functionality"
    parameters:
      PARAM1: "test value"
      PARAM2: 42
    expected:
      exit-code: 0
      output-contains: "Success"
      file-exists: "output.txt"
      directory-exists: "output-dir"
    cleanup:
      - "rm -rf output.txt"
      - "rm -rf output-dir"

# Optional settings
timeout: 60000                     # Timeout in milliseconds
working-directory: ~/path          # Working directory
platforms: [windows, linux, macos] # Supported platforms
tags: [tag1, tag2, read]           # Categories and security tags
ignore-errors: false               # Whether to continue if a step fails
input: "{INPUT_PARAM}"             # Data to pass via stdin
interactive: false                 # Whether this tool is interactive
interactive-options:
  timeout: 30000                   # Timeout for user input
  default-response: "y"            # Default if no input provided

# Resource constraints
resources:
  timeout: 60000                   # Timeout in milliseconds
  max-memory: 512MB                # Maximum memory usage
  cleanup:
    - delete-temp-files: true
    - final-command: "docker rm {CONTAINER_ID}"

# Tool composition - for alias tools
type: alias                        # Indicates this is an alias tool
base-tool: git-clone               # The tool this is an alias of
default-parameters:                # Default parameters for the alias
  REPO: "my-repo"
  BRANCH: "main"

# Versioning
changelog:
  - version: 1.0.0
    changes: "Initial release"
  - version: 1.1.0
    changes: "Added support for feature X"

# LLM Function Calling Integration
function-calling:
  schema-generation:
    parameter-mapping:             # Maps YAML parameter types to JSON schema
      string: "string"
      number: "number"
      boolean: "boolean"
      array: "array"
      object: "object"
    include-descriptions: true     # Include parameter descriptions
    include-defaults: true         # Include default values
    example-generation: true       # Generate examples
    sanitize-schemas: true         # Ensure schemas are valid
```

### LLM Function Calling Integration

Custom Tools are designed to be easily accessible to LLMs through function calling. The tool definitions automatically generate function schemas that LLMs can use to discover and invoke tools.

The `function-calling` section configures how the tool is exposed to LLMs:

- `parameter-mapping`: Maps YAML parameter types to JSON schema types
- `include-descriptions`: Whether to include parameter descriptions in the schema
- `include-defaults`: Whether to include default values in the schema
- `example-generation`: Whether to generate examples for parameters
- `sanitize-schemas`: Ensures generated schemas are valid and secure

When a tool is registered, CYCOD automatically generates a JSON schema that conforms to LLM function calling specifications, allowing the tool to be discovered and invoked by LLMs.

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
       parallel: false
     - name: step2
       bash: command2
       run-condition: "{step1.exit-code} == 0"
       wait-for: [step1]
   parameters:
     PARAM:
       type: string
       description: A parameter
   ```

3. **Tool Aliases**:
   ```yaml
   name: repo-clone
   description: Clone my frequently used repository
   type: alias
   base-tool: git-clone
   default-parameters:
     REPO: "my-username/my-repo"
     BRANCH: "main"
     DIRECTORY: "./my-project"
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
      minLength: 5              # Minimum length (for strings)
      pattern: "^[a-z]+$"       # Regex pattern (for strings)
    examples: ["example1"]      # Example values
    transform: "lowercase"      # Transform function to apply
```

Parameter types include:
- `string`: Text values
- `number`: Numeric values
- `boolean`: True/false values
- `array`: List of values
- `object`: Complex structured data

### Tags and Platform Support

Tools can specify which platforms they work on and include tags for categorization:

```yaml
platforms: [windows, linux, macos]  # Only include platforms the tool works on
tags: [category1, category2, read]  # Tags for categorization and security
```

The security tags (`read`, `write`, `run`) are recommended but optional. If no security tag is present, the tool is considered high-risk and requires explicit approval.

For better organization and discovery, you can use the metadata section:

```yaml
metadata:
  category: development
  subcategory: version-control
  tags: [git, repository, clone]
  search-keywords: [git, repo, download, checkout]
```

### Error Handling

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
        attempts: 3               # Number of retry attempts
        delay: 1000               # Delay in milliseconds between retries
      fallback: alternative-command {PARAM}  # Command to run if all retries fail
```

### Output Management

For tools that produce large amounts of output or streaming data:

```yaml
steps:
  - name: generate-large-output
    bash: large-output-command
    output:
      mode: buffer                # Output mode: buffer or stream
      buffer-limit: 10MB          # Maximum buffer size
      truncation: true            # Whether to truncate if exceeded
      stream-callback: console    # Where to stream (console, file, function)
```

### Environment Variables

You can specify environment variables to set when running the tool:

```yaml
environment:
  variables:
    HTTP_PROXY: "{PROXY_URL}"
    DEBUG: "1"
  inherit: true                  # Inherit parent process environment
```

### Parallel Execution

For multi-step tools, steps can be executed in parallel:

```yaml
steps:
  - name: step1
    bash: command1
    parallel: false              # Run sequentially (default)
  
  - name: step2
    bash: command2
    parallel: true               # Run in parallel with previous step
  
  - name: step3
    bash: command3
    wait-for: [step1, step2]     # Wait for specific steps to complete
```

### Cross-Platform Path Handling

For tools that need to work across different operating systems:

```yaml
file-paths:
  normalize: true                # Convert paths to platform-specific format
  working-directory: "{WORKSPACE}"
  temp-directory: "{TEMP}/cycod-tools/{TOOL_NAME}"
  cross-platform:
    windows-separator: "\\"      # Windows path separator
    unix-separator: "/"          # Unix path separator
    auto-convert: true           # Automatically convert paths
```

### Tool Aliasing and Composition

CYCOD Custom Tools support two powerful mechanisms for reuse and abstraction: aliasing and composition.

#### Tool Aliasing

Tool aliases allow you to create simplified or specialized versions of existing tools:

```yaml
name: clone-my-repo
description: Clone my frequently used repository
type: alias
base-tool: github-repo-clone
default-parameters:
  OWNER: "my-username"
  REPO: "my-project"
  OUTPUT_DIR: "./projects/{REPO}"
```

An alias tool:
- References an existing tool with `base-tool`
- Provides default values for parameters with `default-parameters`
- Can still accept additional parameters when invoked
- Inherits all capabilities and behavior from the base tool

Aliases are useful for creating shortcuts for commonly used tools with specific parameter values.

#### Tool Composition

Tools can be composed by using one tool within another:

```yaml
name: setup-project
description: Clone a repository and install dependencies

steps:
  - name: clone-repo
    use-tool: github-repo-clone
    with:
      OWNER: "{OWNER}"
      REPO: "{REPO}"
      OUTPUT_DIR: "{WORKSPACE}"
  
  - name: install-deps
    bash: cd {WORKSPACE} && npm install
    run-condition: "{clone-repo.exit-code} == 0"
```

When composing tools:
- Use `use-tool` to specify the tool to invoke
- Use `with` to provide parameter values
- Reference outputs from the tool execution just like any other step
- Combine with conditions and parallel execution for complex workflows

Tool composition enables building complex workflows from smaller, reusable building blocks.

### Tool Testing

Tools can include tests to verify they work as expected:

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
cycod tool test NAME [--scope]
cycod tool validate NAME [--scope]
cycod tool edit NAME [--scope]
cycod tool export NAME [--scope] [--format]
cycod tool import PATH [--scope]
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
- `--from-file "PATH"` - Load tool definition from a YAML file
- `--editor` - Open definition in default editor

# Platform-specific commands
- `--command-default "COMMAND"` - Default command for all platforms
- `--command-windows "COMMAND"` - Windows-specific command
- `--command-linux "COMMAND"` - Linux-specific command
- `--command-macos "COMMAND"` - macOS-specific command

- `--step NAME "COMMAND"` - Define a step (can be used multiple times for multi-step tools)
  - `--continue-on-error` - Continue to next step even if this step fails
  - `--run-condition "CONDITION"` - Condition for when to run this step
  - `--parallel` - Run this step in parallel with previous steps
  - `--wait-for "STEP1,STEP2"` - Steps to wait for before running this step
  - `--retry-attempts N` - Number of retry attempts
  - `--retry-delay MS` - Delay in milliseconds between retries
  - `--fallback "COMMAND"` - Command to run if all retries fail
  - `--output-mode buffer|stream` - Output mode
  - `--output-buffer-limit SIZE` - Maximum buffer size
  
  # Platform-specific step commands
  - `--step-command-default NAME "COMMAND"` - Default command for all platforms
  - `--step-command-windows NAME "COMMAND"` - Windows-specific command
  - `--step-command-linux NAME "COMMAND"` - Linux-specific command
  - `--step-command-macos NAME "COMMAND"` - macOS-specific command

- `--parameter NAME "DESC"` [param-options] - Define a parameter (multiple allowed)
  - `type=string|number|boolean|array|object` - Parameter type
  - `required=true|false` - Whether parameter is required
  - `default=VALUE` - Default value if not provided
  - `min-length=N` - Minimum length (for strings)
  - `max-length=N` - Maximum length (for strings)
  - `pattern=REGEX` - Regex pattern (for strings)
  - `min=N` - Minimum value (for numbers)
  - `max=N` - Maximum value (for numbers)
  - `enum=VALUE1,VALUE2` - Allowed values
  - `transform=FUNCTION` - Transform function to apply
  - `format=FORMAT` - How parameter appears in command
  - `example=EXAMPLE` - Example value (can be used multiple times)
  - `escape-shell=true|false` - Whether to escape shell metacharacters

- `--platform windows|linux|macos` - Supported platform (multiple allowed)
- `--tag TAG` - Tag for categorization (multiple allowed)
- `--timeout MILLISECONDS` - Default timeout
- `--working-directory DIR` - Working directory
- `--input "INPUT"` - Data to pass via stdin
- `--scope local|user|global` - Scope for the tool (default: local)
- `--security-privilege same-as-user|reduced|elevated` - Execution privilege
- `--security-isolation none|process|container` - Isolation level
- `--security-permission PERMISSION` - Required permission (multiple allowed)
- `--env-var NAME=VALUE` - Environment variable (multiple allowed)
- `--inherit-env true|false` - Whether to inherit parent environment

- `--test NAME "DESC"` - Define a test (multiple allowed)
  - `--test-param NAME=VALUE` - Parameter for the test
  - `--expect-exit-code N` - Expected exit code
  - `--expect-output "TEXT"` - Expected output text
  - `--expect-file-exists "PATH"` - Expected file to exist
  - `--cleanup "COMMAND"` - Cleanup command to run after test

Example:
```
cycod tool add weather-lookup \
  --description "Get weather for a location" \
  --bash "curl -s 'wttr.in/{LOCATION}?format={FORMAT}'" \
  --parameter LOCATION "City or airport code" required=true example=London \
  --parameter FORMAT "Output format" default=3 \
  --tag weather --tag api --tag read \
  --timeout 10000
```

### Tool Test Command

```
cycod tool test NAME [--scope] [--test TEST_NAME]
```

Options:
- `--scope` - Scope to search in (local, user, global, any)
- `--test TEST_NAME` - Name of specific test to run (if omitted, runs all tests)
- `--verbose` - Show detailed output

Example:
```
cycod tool test weather-lookup
cycod tool test weather-lookup --test basic-test --verbose
```

### Tool Edit Command

```
cycod tool edit NAME [--scope]
```

Options:
- `--scope` - Scope to search in (local, user, global, any)
- `--editor EDITOR` - Editor to use (default: system default)

Example:
```
cycod tool edit weather-lookup
cycod tool edit weather-lookup --scope user --editor code
```

### Tool Validate Command

```
cycod tool validate NAME [--scope]
```

Options:
- `--scope` - Scope to search in (local, user, global, any)
- `--fix` - Fix validation issues if possible

Example:
```
cycod tool validate weather-lookup
cycod tool validate weather-lookup --fix
```

### Tool Export/Import Commands

```
cycod tool export NAME [--scope] [--format yaml|json]
cycod tool import PATH [--scope]
```

Options:
- `--scope` - Scope to export from/import to (local, user, global, any)
- `--format` - Export format (yaml or json, default: yaml)

Example:
```
cycod tool export weather-lookup --format json > weather-lookup.json
cycod tool import weather-lookup.yaml --scope user
```

### Tool Get Command

```
cycod tool get NAME [--scope] [--format]
```

Options:
- `--scope` - Scope to search in (local, user, global, any)
- `--format yaml|json` - Output format (default: yaml)
- `--detail` - Show detailed information

Example:
```
cycod tool get weather-lookup
cycod tool get weather-lookup --scope user --format json
cycod tool get weather-lookup --detail
```

### Tool List Command

```
cycod tool list [--scope] [--format] [--tag TAG] [--category CATEGORY]
```

Options:
- `--scope` - Scope to list from (local, user, global, any)
- `--format simple|table|json|yaml` - Output format (default: table)
- `--tag TAG` - Filter by tag
- `--category CATEGORY` - Filter by category
- `--search TEXT` - Search in name, description, and keywords

Example:
```
cycod tool list
cycod tool list --scope user --format json
cycod tool list --tag git --category development
cycod tool list --search "weather"
```

### Tool Remove Command

```
cycod tool remove NAME [--scope] [--force]
```

Options:
- `--scope` - Scope to remove from (local, user, global, any)
- `--force` - Skip confirmation prompt

Example:
```
cycod tool remove weather-lookup
cycod tool remove weather-lookup --scope user --force
```

## Parameter Substitution

### Parameter References

Parameters are referenced in commands using curly braces:

```yaml
bash: command {PARAM1} {PARAM2}
```

You can also use parameter transforms and formatting:

```yaml
bash: command --option={PARAM1:lowercase} --count={COUNT:format(0000)}
```

### Predefined Variables

In addition to defined parameters, you can use predefined variables:

- `{TOOL_NAME}` - The name of the current tool
- `{WORKSPACE}` - The current workspace directory
- `{TEMP}` - The system temporary directory
- `{HOME}` - The user's home directory
- `{OS}` - The current operating system (windows, linux, macos)
- `{DATE}` - The current date in ISO format (YYYY-MM-DD)
- `{TIME}` - The current time in ISO format (HH:MM:SS)
- `{TIMESTAMP}` - The current timestamp in ISO format (YYYY-MM-DDTHH:MM:SSZ)

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
- `{step1.duration}` - Execution duration in milliseconds
- `{step1.start-time}` - Timestamp when the step started
- `{step1.end-time}` - Timestamp when the step ended

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
    default: "./checkout/{REPO}"
```

### Default Values

Parameters with default values don't need to be provided when calling the tool:

```yaml
parameters:
  FORMAT:
    type: string
    description: Output format
    default: "3"
```

### Parameter Transforms

Parameters can be transformed before substitution:

```yaml
parameters:
  TEXT:
    type: string
    description: Text to transform
    transform: "lowercase"  # Predefined transform

  CUSTOM_TEXT:
    type: string
    description: Text with custom transform
    transform: "value.toUpperCase().replace(/\\s+/g, '-')"  # Custom transform using JavaScript
```

Available predefined transforms:
- `lowercase` - Convert to lowercase
- `uppercase` - Convert to uppercase
- `trim` - Remove leading and trailing whitespace
- `base64encode` - Encode as Base64
- `base64decode` - Decode from Base64
- `urlencode` - URL encode
- `urldecode` - URL decode
- `jsonescaped` - Escape for JSON inclusion
- `shellescaped` - Escape for shell command

### Parameter Formatting

Parameters can specify how they appear in commands:

```yaml
parameters:
  COUNT:
    type: number
    description: Count value
    default: 5
    format: "--count={value}"  # Will be substituted as --count=5

  DEBUG:
    type: boolean
    description: Enable debug mode
    default: false
    format: "{value ? '--debug' : ''}"  # Will be empty if false, --debug if true
```

### File-Based Definition Mode

While the command line interface is convenient for simple tools, more complex tools are easier to define in YAML files.

#### Creating Tools from YAML Files

You can define a tool in a YAML file and import it:

```
cycod tool add --from-file my-tool-definition.yaml
```

The YAML file should follow the schema outlined in this specification. For example:

```yaml
name: complex-workflow
description: A complex multi-step workflow

steps:
  - name: step1
    bash: command1 {PARAM1}
  - name: step2
    bash: command2 {PARAM2}
    run-condition: "{step1.exit-code} == 0"

parameters:
  PARAM1:
    type: string
    description: First parameter
    required: true
  PARAM2:
    type: string
    description: Second parameter
    default: "default value"
```

#### Interactive Editing

For iterative development, you can use the `--editor` option to open a template in your default text editor:

```
cycod tool add my-new-tool --editor
```

This will:
1. Create a template YAML file
2. Open it in your default editor
3. Import the tool after you save and close the editor

#### Exporting and Importing Tools

You can export existing tools to files:

```
cycod tool export my-tool > my-tool-definition.yaml
```

And import them into different scopes:

```
cycod tool import my-tool-definition.yaml --scope user
```

This facilitates sharing tools across projects and teams.

## LLM Function Calling Integration

Custom Tools are designed to be easily exposed to LLMs through function calling interfaces. This allows AI models to discover, understand, and invoke your tools.

### Function Schema Generation

When a custom tool is registered, CYCOD automatically generates a function calling schema for it:

```yaml
function-calling:
  schema-generation:
    parameter-mapping:
      string: "string"            # How to map YAML parameter types to LLM schema types
      number: "number"
      boolean: "boolean"
      array: "array"
      object: "object"
    include-descriptions: true    # Include parameter descriptions in schema
    include-defaults: true        # Include default values in schema
    example-generation: true      # Generate examples for parameters
```

### Discovery and Invocation

LLMs can discover available tools through function calling mechanisms:

1. When the LLM calls a function that matches a custom tool name, CYCOD will:
   - Map the parameters from the function call to the tool parameters
   - Execute the tool with the provided parameters
   - Return the result to the LLM

2. The LLM can use the tool's description and parameter descriptions to understand when and how to use the tool.

### Example

For a weather lookup tool defined as:

```yaml
name: weather-lookup
description: Get weather for a location
bash: curl -s 'wttr.in/{LOCATION}?format={FORMAT}'
parameters:
  LOCATION:
    type: string
    description: City or airport code
    required: true
    examples: ["London", "SFO"]
  FORMAT:
    type: string
    description: Output format
    default: "3"
```

The generated function calling schema would be:

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

## Best Practices

Creating effective custom tools requires careful consideration of design, usability, and security. This section provides guidelines and recommendations for creating high-quality custom tools.

### Tool Design Principles

1. **Focus on a Single Responsibility**
   - Each tool should do one thing and do it well
   - Break complex workflows into multiple tools that can be composed together
   - Example: Instead of a single "git-operations" tool, create separate tools for "git-clone", "git-commit", etc.

2. **Design for Reusability**
   - Parameterize everything that might vary between uses
   - Use sensible defaults for optional parameters
   - Example:
     ```yaml
     name: http-request
     description: Make an HTTP request to a URL
     bash: curl -X {METHOD} {HEADERS} "{URL}"
     parameters:
       URL:
         type: string
         description: URL to request
         required: true
       METHOD:
         type: string
         description: HTTP method
         default: "GET"
         validation:
           enum: ["GET", "POST", "PUT", "DELETE", "PATCH"]
       HEADERS:
         type: string
         description: HTTP headers
         default: ""
         examples: ["-H 'Content-Type: application/json'"]
     ```

3. **Make Tools Composable**
   - Design tools that can be used as building blocks in larger workflows
   - Ensure output formats are consistent and usable by other tools
   - Example: A "fetch-data" tool should output data in a format that can be consumed by a "process-data" tool

4. **Follow the Principle of Least Privilege**
   - Request only the permissions necessary for the tool to function
   - Use read-only access when write access isn't needed
   - Example:
     ```yaml
     security:
       execution-privilege: same-as-user
       required-permissions:
         - "network:external:api.example.com"  # Only request network access to specific domains
     ```

### Parameter Naming Conventions

1. **Use Consistent Naming Patterns**
   - Use uppercase for all parameters (e.g., `URL`, `FILE_PATH`)
   - Use snake_case or kebab-case consistently for multi-word parameters
   - Example: `OUTPUT_FORMAT` or `OUTPUT-FORMAT`, but not mixing styles

2. **Choose Descriptive Names**
   - Use names that clearly indicate the parameter's purpose
   - Avoid abbreviations unless they're universally understood
   - Good: `REPOSITORY_URL`, `OUTPUT_FORMAT`
   - Bad: `REPO`, `FMT`

3. **Group Related Parameters**
   - Use prefixes for related parameters
   - Example: `SOURCE_LANGUAGE`, `SOURCE_TEXT` for input parameters, and `TARGET_LANGUAGE`, `TARGET_TEXT` for output parameters

4. **Follow Platform Conventions**
   - For parameters corresponding to command-line options:
     - Use singular name for boolean flags (`VERBOSE` instead of `VERBOSITY`)
     - Use substantives for values (`COUNT` instead of `HOW_MANY`)

### Error Handling Patterns

1. **Implement Appropriate Retry Logic**
   - Use retries for operations that might fail transiently (network requests, file locks)
   - Use exponential backoff for retries
   - Example:
     ```yaml
     steps:
       - name: api-request
         bash: curl -s "{API_URL}"
         error-handling:
           retry:
             attempts: 3
             delay: 1000  # Start with 1 second, then 2, then 4 (exponential)
     ```

2. **Use Fallbacks for Critical Operations**
   - Provide alternative methods when primary methods fail
   - Example:
     ```yaml
     steps:
       - name: primary-method
         bash: primary-command
         continue-on-error: true
       - name: fallback-method
         bash: fallback-command
         run-condition: "{primary-method.exit-code} != 0"
     ```

3. **Provide Meaningful Error Messages**
   - Include context in error messages
   - Suggest possible fixes for common errors
   - Example:
     ```yaml
     steps:
       - name: validate
         bash: |
           if [ ! -f "{FILE_PATH}" ]; then
             echo "Error: File '{FILE_PATH}' not found. Please check the path and try again."
             exit 1
           fi
     ```

4. **Clean Up Resources**
   - Always clean up temporary files and resources
   - Use the resources.cleanup section for reliable cleanup
   - Example:
     ```yaml
     resources:
       cleanup:
         - delete-temp-files: true
         - final-command: "rm -rf {TEMP_DIR}"
     ```

### Cross-Platform Compatibility

1. **Use Platform-Specific Commands**
   - Provide different implementations for different platforms
   - Example:
     ```yaml
     commands:
       default: find {DIRECTORY} -name "{PATTERN}"
       platforms:
         windows: powershell -Command "Get-ChildItem -Path '{DIRECTORY}' -Filter '{PATTERN}' -Recurse"
         linux: find {DIRECTORY} -type f -name "{PATTERN}"
     ```

2. **Handle Path Differences**
   - Use the file-paths.normalize option
   - Avoid hardcoding path separators
   - Example:
     ```yaml
     file-paths:
       normalize: true
       cross-platform:
         auto-convert: true
     ```

3. **Use Platform-Agnostic Approaches When Possible**
   - Prefer languages and tools available on all platforms (Python, Node.js)
   - Use relative paths when possible
   - Example:
     ```yaml
     bash: python -c "import os, glob; print('\\n'.join(glob.glob('{PATTERN}')))"
     ```

4. **Test on All Supported Platforms**
   - Include tests for each supported platform
   - Verify behavior with platform-specific edge cases
   - Example:
     ```yaml
     tests:
       - name: test-windows
         parameters:
           PATH: "C:\\Windows\\Temp"
         platforms: [windows]
       - name: test-linux
         parameters:
           PATH: "/tmp"
         platforms: [linux]
     ```

### Security Considerations

1. **Escape User Inputs Properly**
   - Always use escape-shell for parameters that might contain special characters
   - Example:
     ```yaml
     parameters:
       QUERY:
         type: string
         description: Search query
         security:
           escape-shell: true
     ```

2. **Validate All Inputs**
   - Use validation rules to ensure inputs are safe and correct
   - Example:
     ```yaml
     parameters:
       FILE_TYPE:
         type: string
         description: File extension
         validation:
           pattern: "^[a-zA-Z0-9]+$"  # Only allow alphanumeric file extensions
     ```

3. **Avoid Exposing Sensitive Information**
   - Don't log sensitive data
   - Mask sensitive values in output
   - Example:
     ```yaml
     steps:
       - name: api-request
         bash: curl -s -H "Authorization: Bearer {API_KEY}" "{API_URL}"
         output:
           masked-values: ["{API_KEY}"]  # Replace API_KEY with asterisks in output
     ```

4. **Use Network Restrictions**
   - Limit network access to only required domains
   - Example:
     ```yaml
     security:
       required-permissions:
         - "network:external:api.github.com"  # Only allow access to GitHub API
     ```

### Performance Optimization

1. **Use Parallel Execution**
   - Run independent steps in parallel
   - Example:
     ```yaml
     steps:
       - name: fetch-data-1
         bash: curl -s "{API_URL_1}" > data1.json
         parallel: true
       - name: fetch-data-2
         bash: curl -s "{API_URL_2}" > data2.json
         parallel: true
       - name: process-data
         bash: jq -s '.[0] * .[1]' data1.json data2.json
         wait-for: [fetch-data-1, fetch-data-2]
     ```

2. **Implement Output Streaming**
   - Use streaming for large outputs
   - Example:
     ```yaml
     steps:
       - name: generate-large-output
         bash: generate-large-data
         output:
           mode: stream
           stream-callback: console
     ```

3. **Set Appropriate Timeouts**
   - Use timeouts that balance reliability with responsiveness
   - Example:
     ```yaml
     timeout: 30000  # 30 seconds for the entire tool
     steps:
       - name: quick-operation
         bash: quick-command
         timeout: 5000  # 5 seconds for this step
       - name: long-operation
         bash: long-command
         timeout: 25000  # 25 seconds for this step
     ```

4. **Optimize Resource Usage**
   - Limit memory usage for resource-intensive operations
   - Clean up temporary files as soon as they're no longer needed
   - Example:
     ```yaml
     resources:
       max-memory: 256MB
       cleanup:
         - delete-temp-files: true
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

### Privilege Control and Security Boundaries

You can define specific security boundaries for your tools:

```yaml
security:
  execution-privilege: same-as-user  # Options: same-as-user, reduced, elevated (requires approval)
  isolation: process                 # Options: none, process, container
  required-permissions:
    - "filesystem:write:{DIRECTORY}"
    - "network:external:api.github.com"
  justification: "Required for cloning GitHub repositories"
```

The available execution privilege levels are:
- `same-as-user`: Run with the same privileges as the user (default)
- `reduced`: Run with reduced privileges
- `elevated`: Run with elevated privileges (requires approval)

Isolation options:
- `none`: Run in the same process as CYCOD
- `process`: Run in a separate process (default)
- `container`: Run in a container for maximum isolation (if supported)

### Parameter Security

Parameters can specify security options:

```yaml
parameters:
  QUERY:
    type: string
    description: Search query
    security:
      escape-shell: true           # Automatically escape shell metacharacters
```

Setting `escape-shell: true` will automatically escape any shell metacharacters in the parameter value to prevent command injection attacks.

## Help Text

### Tool Help Command

```
cycod help tool
```

Output:
```
CYCOD TOOL COMMANDS

  These commands allow you to manage custom tools for CYCOD.

USAGE: cycod tool list [--scope] [options]
   OR: cycod tool get TOOL_NAME [--scope] [options]
   OR: cycod tool add TOOL_NAME [options]
   OR: cycod tool remove TOOL_NAME [--scope] [options]
   OR: cycod tool test TOOL_NAME [--scope] [options]
   OR: cycod tool validate TOOL_NAME [--scope] [options]
   OR: cycod tool edit TOOL_NAME [--scope] [options]
   OR: cycod tool export TOOL_NAME [--scope] [options]
   OR: cycod tool import PATH [--scope] [options]

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
    test            Test a tool with its defined tests
    validate        Validate a tool definition
    edit            Edit a tool definition
    export          Export a tool definition to a file
    import          Import a tool definition from a file

SEE ALSO

  cycod help tool list
  cycod help tool get
  cycod help tool add
  cycod help tool remove
  cycod help tool test
  cycod help tool validate
  cycod help tool edit
  cycod help tool export
  cycod help tool import
  cycod help tools
```

### Tool Add Help

```
cycod help tool add
```

Output:
```
CYCOD TOOL ADD

  Adds a new custom tool for CYCOD.
  
USAGE: cycod tool add TOOL_NAME --description "DESCRIPTION" [options]

OPTIONS

  TOOL DEFINITION

    --description "DESC"            Human-readable description of the tool (required)
    --from-file "PATH"              Load tool definition from a YAML file
    --editor                        Open definition in default editor
    
    --bash "COMMAND"                Define a Bash command to execute
    --cmd "COMMAND"                 Define a Windows CMD command to execute
    --pwsh "COMMAND"                Define a PowerShell command to execute
    --run "COMMAND"                 Define a direct command to execute
    --script "SCRIPT"               Define script content (use with --shell)
    --shell "SHELL"                 Shell to use with script

  MULTI-STEP TOOLS

    --step NAME "COMMAND"           Define a step (can be used multiple times)
      --continue-on-error           Continue to next step even if this step fails
      --run-condition "CONDITION"   Condition for when to run this step
      --parallel                    Run this step in parallel with previous steps
      --wait-for "STEP1,STEP2"      Steps to wait for before running this step
      --retry-attempts N            Number of retry attempts
      --retry-delay MS              Delay in milliseconds between retries
      --fallback "COMMAND"          Command to run if all retries fail
      --output-mode buffer|stream   Output mode
      --output-buffer-limit SIZE    Maximum buffer size

  PARAMETERS

    --parameter NAME "DESC" [options]  Define a parameter (can be used multiple times)
      type=string|number|boolean|array|object  Parameter type
      required=true|false               Whether parameter is required
      default=VALUE                     Default value if not provided
      min-length=N                      Minimum length (for strings)
      max-length=N                      Maximum length (for strings)
      pattern=REGEX                     Regex pattern (for strings)
      min=N                             Minimum value (for numbers)
      max=N                             Maximum value (for numbers)
      enum=VALUE1,VALUE2                Allowed values
      transform=FUNCTION                Transform function to apply
      format=FORMAT                     How parameter appears in command
      example=EXAMPLE                   Example value (can be used multiple times)
      escape-shell=true|false           Whether to escape shell metacharacters

  SECURITY AND ENVIRONMENT

    --security-privilege LEVEL      Execution privilege level (same-as-user|reduced|elevated)
    --security-isolation LEVEL      Isolation level (none|process|container)
    --security-permission PERM      Required permission (can be used multiple times)
    --env-var NAME=VALUE            Environment variable (can be used multiple times)
    --inherit-env true|false        Whether to inherit parent environment

  TESTING

    --test NAME "DESC"              Define a test (can be used multiple times)
      --test-param NAME=VALUE       Parameter for the test
      --expect-exit-code N          Expected exit code
      --expect-output "TEXT"        Expected output text
      --expect-file-exists "PATH"   Expected file to exist
      --cleanup "COMMAND"           Cleanup command to run after test

  ADDITIONAL OPTIONS

    --platform PLATFORM             Supported platform (windows|linux|macos)
    --tag TAG                       Tag for categorization
    --timeout MILLISECONDS          Default timeout
    --working-directory DIR         Working directory
    --input "INPUT"                 Data to pass via stdin

  SCOPE OPTIONS

    --global, -g                    Add tool to global scope (all users)
    --user, -u                      Add tool to user scope (current user)
    --local, -l                     Add tool to local scope (current directory, default)

EXAMPLES

  EXAMPLE 1: Create a simple weather lookup tool

    cycod tool add weather-lookup --description "Get weather for a location" \
      --bash "curl -s 'wttr.in/{LOCATION}?format={FORMAT}'" \
      --parameter LOCATION "City or airport code" required=true example=London \
      --parameter FORMAT "Output format" default=3 \
      --tag weather --tag api --tag read

  EXAMPLE 2: Create a multi-step tool that processes data

    cycod tool add process-data --description "Process data from a file" \
      --step fetch "curl -s '{URL}' > data.json" \
      --step process "jq '.items' data.json" \
      --step cleanup "rm data.json" \
      --parameter URL "URL to fetch data from" required=true \
      --tag api --tag read
      
  EXAMPLE 3: Create a tool from a YAML file

    cycod tool add --from-file my-tool-definition.yaml

  EXAMPLE 4: Create a tool with interactive editing

    cycod tool add my-new-tool --editor

SEE ALSO

  cycod help tool
  cycod help tools
```

### Tool Test Help

```
cycod help tool test
```

Output:
```
CYCOD TOOL TEST

  Test a tool with its defined tests.

USAGE: cycod tool test TOOL_NAME [--scope] [options]

OPTIONS

  SCOPE OPTIONS

    --global, -g    Test the tool from global scope (all users)
    --user, -u      Test the tool from user scope (current user)
    --local, -l     Test the tool from local scope (current directory)
    --any, -a       Test the tool from the first scope it's found in (default)

  TEST OPTIONS

    --test TEST_NAME    Name of specific test to run (if omitted, runs all tests)
    --verbose           Show detailed output
    --stop-on-failure   Stop testing after the first failure

EXAMPLES

  EXAMPLE 1: Run all tests for a tool

    cycod tool test weather-lookup

  EXAMPLE 2: Run a specific test with verbose output

    cycod tool test weather-lookup --test basic-test --verbose

SEE ALSO

  cycod help tool
  cycod help tools
```

### Tool Get Help

```
cycod help tool get
```

Output:
```
CYCOD TOOL GET

  Display the details of a specific tool.

USAGE: cycod tool get TOOL_NAME [--scope] [options]

OPTIONS

  SCOPE OPTIONS

    --global, -g    Look for the tool in global scope (all users)
    --user, -u      Look for the tool in user scope (current user)
    --local, -l     Look for the tool in local scope (current directory)
    --any, -a       Look for the tool in any scope (default)

  OUTPUT OPTIONS

    --format yaml|json    Output format (default: yaml)
    --detail              Show detailed information

EXAMPLES

  EXAMPLE 1: Get a tool from any scope

    cycod tool get weather-lookup

  EXAMPLE 2: Get a tool from user scope

    cycod tool get weather-lookup --user

  EXAMPLE 3: Get a tool in JSON format

    cycod tool get weather-lookup --format json

SEE ALSO

  cycod help tool
  cycod help tools
```

### Tool List Help

```
cycod help tool list
```

Output:
```
CYCOD TOOL LIST

  List all available tools across all scopes or in a specific scope.

USAGE: cycod tool list [--scope] [options]

OPTIONS

  SCOPE OPTIONS

    --global, -g    List only global tools (all users)
    --user, -u      List only user tools (current user)
    --local, -l     List only local tools (current directory)
    --any, -a       List tools from all scopes (default)

  FILTER OPTIONS

    --tag TAG             Filter by tag
    --category CATEGORY   Filter by category
    --search TEXT         Search in name, description, and keywords
    --platform PLATFORM   Filter by platform support

  OUTPUT OPTIONS

    --format simple|table|json|yaml    Output format (default: table)
    --sort name|category|scope         Sort order (default: name)

EXAMPLES

  EXAMPLE 1: List all tools from all scopes

    cycod tool list

  EXAMPLE 2: List only user tools

    cycod tool list --user

  EXAMPLE 3: List tools matching a tag

    cycod tool list --tag git

  EXAMPLE 4: List tools in JSON format

    cycod tool list --format json

SEE ALSO

  cycod help tool
  cycod help tools
```

### Tool Remove Help

```
cycod help tool remove
```

Output:
```
CYCOD TOOL REMOVE

  Delete a tool from a specific scope.

USAGE: cycod tool remove TOOL_NAME [--scope] [options]

OPTIONS

  SCOPE OPTIONS

    --global, -g    Delete the tool from global scope (all users)
    --user, -u      Delete the tool from user scope (current user)
    --local, -l     Delete the tool from local scope (current directory)
    --any, -a       Delete the tool from the first scope it's found in (default)

  ADDITIONAL OPTIONS

    --force         Skip confirmation prompt

EXAMPLES

  EXAMPLE 1: Delete a tool from any scope

    cycod tool remove weather-lookup

  EXAMPLE 2: Delete a tool from user scope

    cycod tool remove weather-lookup --user

  EXAMPLE 3: Delete a tool without confirmation

    cycod tool remove weather-lookup --force

SEE ALSO

  cycod help tool
  cycod help tools
```

### Tool Edit Help

```
cycod help tool edit
```

Output:
```
CYCOD TOOL EDIT

  Edit a tool definition using a text editor.

USAGE: cycod tool edit TOOL_NAME [--scope] [options]

OPTIONS

  SCOPE OPTIONS

    --global, -g    Edit the tool from global scope (all users)
    --user, -u      Edit the tool from user scope (current user)
    --local, -l     Edit the tool from local scope (current directory)
    --any, -a       Edit the tool from the first scope it's found in (default)

  EDITOR OPTIONS

    --editor EDITOR    Editor to use (default: system default)

EXAMPLES

  EXAMPLE 1: Edit a tool using the default editor

    cycod tool edit weather-lookup

  EXAMPLE 2: Edit a tool using a specific editor

    cycod tool edit weather-lookup --editor code

SEE ALSO

  cycod help tool
  cycod help tools
```

### Tool Export/Import Help

```
cycod help tool export
```

Output:
```
CYCOD TOOL EXPORT

  Export a tool definition to a file or standard output.

USAGE: cycod tool export TOOL_NAME [--scope] [options]

OPTIONS

  SCOPE OPTIONS

    --global, -g      Export the tool from global scope (all users)
    --user, -u        Export the tool from user scope (current user)
    --local, -l       Export the tool from local scope (current directory)
    --any, -a         Export the tool from the first scope it's found in (default)

  OUTPUT OPTIONS

    --format yaml|json    Export format (default: yaml)
    --output FILE         Output file (if omitted, prints to stdout)

EXAMPLES

  EXAMPLE 1: Export a tool to stdout

    cycod tool export weather-lookup

  EXAMPLE 2: Export a tool to a file in JSON format

    cycod tool export weather-lookup --format json --output weather-lookup.json

SEE ALSO

  cycod help tool
  cycod help tool import
  cycod help tools
```

```
cycod help tool import
```

Output:
```
CYCOD TOOL IMPORT

  Import a tool definition from a file.

USAGE: cycod tool import FILE [--scope] [options]

OPTIONS

  SCOPE OPTIONS

    --global, -g    Import the tool to global scope (all users)
    --user, -u      Import the tool to user scope (current user)
    --local, -l     Import the tool to local scope (current directory, default)

  IMPORT OPTIONS

    --override      Override existing tool if it already exists
    --validate      Validate the tool before importing

EXAMPLES

  EXAMPLE 1: Import a tool to local scope

    cycod tool import weather-lookup.yaml

  EXAMPLE 2: Import a tool to user scope, overriding any existing definition

    cycod tool import weather-lookup.yaml --user --override

SEE ALSO

  cycod help tool
  cycod help tool export
  cycod help tools
```

### Tool Validate Help

```
cycod help tool validate
```

Output:
```
CYCOD TOOL VALIDATE

  Validate a tool definition for correctness.

USAGE: cycod tool validate TOOL_NAME [--scope] [options]

OPTIONS

  SCOPE OPTIONS

    --global, -g    Validate the tool from global scope (all users)
    --user, -u      Validate the tool from user scope (current user)
    --local, -l     Validate the tool from local scope (current directory)
    --any, -a       Validate the tool from the first scope it's found in (default)

  VALIDATION OPTIONS

    --fix           Fix validation issues if possible
    --detailed      Show detailed validation results

EXAMPLES

  EXAMPLE 1: Validate a tool

    cycod tool validate weather-lookup

  EXAMPLE 2: Validate and automatically fix issues

    cycod tool validate weather-lookup --fix

SEE ALSO

  cycod help tool
  cycod help tools
```

## Examples

### Simple Single-Step Tool Example

```yaml
name: weather-lookup
description: Get weather for a location

bash: curl -s 'wttr.in/{LOCATION}?format={FORMAT}'

parameters:
  LOCATION:
    type: string
    description: City or airport code
    required: true
    examples: ["London", "SFO", "NYC"]
    validation:
      minLength: 2
  FORMAT:
    type: string
    description: Output format
    required: false
    default: "3"
    examples: ["1", "2", "3", "4"]

timeout: 10000
tags: [weather, api, read]
platforms: [windows, linux, macos]
```

### Multi-Step Tool with Output Capturing

```yaml
name: github-commit-stats
description: Get statistics for recent commits in a GitHub repository

steps:
  - name: fetch-commits
    bash: curl -s "https://api.github.com/repos/{OWNER}/{REPO}/commits?per_page=10"
    output:
      mode: buffer
      buffer-limit: 1MB

  - name: count-authors
    bash: echo '{fetch-commits.output}' | jq 'group_by(.commit.author.name) | map({author: .[0].commit.author.name, count: length}) | sort_by(.count) | reverse'
    error-handling:
      retry:
        attempts: 2
        delay: 1000

  - name: format-output
    bash: echo '{count-authors.output}' | jq -r '.[] | "\(.author): \(.count) commits"'
    run-condition: "{count-authors.exit-code} == 0"

parameters:
  OWNER:
    type: string
    description: Repository owner username
    required: true
    validation:
      pattern: "^[a-zA-Z0-9-]+$"
  REPO:
    type: string
    description: Repository name
    required: true

environment:
  variables:
    GITHUB_TOKEN: "{GITHUB_TOKEN}"
  inherit: true

security:
  execution-privilege: same-as-user
  isolation: process
  required-permissions:
    - "network:external:api.github.com"

tags: [github, api, read]
timeout: 15000

tests:
  - name: test-valid-repo
    parameters:
      OWNER: "microsoft"
      REPO: "vscode"
    expected:
      exit-code: 0
      output-contains: "commits"
```

### Tool with Conditional Steps and Parallel Execution

```yaml
name: github-repo-clone
description: Clone a GitHub repository with fallback methods

steps:
  - name: try-https
    bash: git clone https://github.com/{OWNER}/{REPO}.git {OUTPUT_DIR}
    continue-on-error: true

  - name: try-ssh
    bash: git clone git@github.com:{OWNER}/{REPO}.git {OUTPUT_DIR}
    run-condition: "{try-https.exit-code} != 0"
    continue-on-error: true

  - name: check-deps
    bash: |
      if ! command -v npm &> /dev/null; then
        echo "npm not found"
        exit 1
      fi
    parallel: true  # This runs in parallel with the cloning steps

  - name: install-deps
    bash: cd {OUTPUT_DIR} && npm install
    wait-for: [try-https, try-ssh, check-deps]
    run-condition: "(({try-https.exit-code} == 0) || ({try-ssh.exit-code} == 0)) && ({check-deps.exit-code} == 0)"

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

file-paths:
  normalize: true
  working-directory: "{WORKSPACE}"
  cross-platform:
    auto-convert: true

security:
  execution-privilege: same-as-user
  required-permissions:
    - "filesystem:write:{OUTPUT_DIR}"
    - "network:external:github.com"

tags: [github, git, clone, write]
platforms: [windows, linux, macos]
```

### Tool with Complex Parameter Handling and Validation

```yaml
name: search-code
description: Search for patterns in code files

bash: grep -r {CASE_SENSITIVE} {WHOLE_WORD} "{PATTERN}" {DIRECTORY} --include="*.{FILE_TYPE}"

parameters:
  PATTERN:
    type: string
    description: Pattern to search for
    required: true
    validation:
      minLength: 1
      maxLength: 100
    security:
      escape-shell: true
    examples: ["function", "TODO:", "FIXME:"]

  DIRECTORY:
    type: string
    description: Directory to search in
    default: "."
    validation:
      pattern: "^[^/\\\\]+.*$"  # Ensure it's a valid directory path

  FILE_TYPE:
    type: string
    description: File extension to search in
    default: "*"
    examples: ["js", "py", "cs", "java"]

  CASE_SENSITIVE:
    type: boolean
    description: Whether to use case-sensitive search
    default: false
    format: "{value ? '' : '-i'}"  # This will be converted to -i for false or empty string for true

  WHOLE_WORD:
    type: boolean
    description: Whether to search for whole words only
    default: false
    format: "{value ? '-w' : ''}"  # This will be converted to -w for true or empty string for false

tags: [search, code, read]
platforms: [windows, linux, macos]

tests:
  - name: test-basic-search
    parameters:
      PATTERN: "function"
      DIRECTORY: "./src"
      FILE_TYPE: "js"
    expected:
      exit-code: 0
```

### Cross-Platform Tool Example

```yaml
name: find-files
description: Find files by name pattern with cross-platform support

commands:
  default: find {DIRECTORY} -name "{PATTERN}"
  platforms:
    windows: powershell -Command "Get-ChildItem -Path '{DIRECTORY}' -Filter '{PATTERN}' -Recurse -File"
    linux: find {DIRECTORY} -type f -name "{PATTERN}"
    macos: find {DIRECTORY} -type f -name "{PATTERN}"

parameters:
  DIRECTORY:
    type: string
    description: Directory to search in
    default: "."
    validation:
      pattern: "^[^<>:\"|?*]+$"  # Validate for cross-platform compatibility
    examples: [".", "./src", "C:\\Projects"]

  PATTERN:
    type: string
    description: File pattern to match
    required: true
    examples: ["*.txt", "*.log", "*.{js,ts}"]
    validation:
      minLength: 1
    security:
      escape-shell: true

file-paths:
  normalize: true
  cross-platform:
    auto-convert: true

metadata:
  category: file-system
  subcategory: search
  tags: [files, search, find]
  search-keywords: [locate, search, find, pattern]

tags: [files, filesystem, search, read]
platforms: [windows, linux, macos]

tests:
  - name: test-find-text-files
    parameters:
      PATTERN: "*.txt"
      DIRECTORY: "./test-data"
    expected:
      exit-code: 0
  
  - name: test-platform-specific
    description: "Test that platform-specific command is used"
    parameters:
      PATTERN: "*.txt"
      DIRECTORY: "."
    expected:
      exit-code: 0
      output-contains-any: ["Get-ChildItem", "find"]  # Should contain either depending on platform
```

### Tool Alias Example

```yaml
name: clone-my-repo
description: Clone my frequently used repository
type: alias
base-tool: github-repo-clone
default-parameters:
  OWNER: "my-username"
  REPO: "my-project"
  OUTPUT_DIR: "./projects/{REPO}"
```

### Tool with LLM Function Calling Configuration

```yaml
name: translate-text
description: Translate text from one language to another

bash: curl -s -X POST "https://translation-api.example.com/translate" -d "text={TEXT}&source={SOURCE_LANG}&target={TARGET_LANG}"

parameters:
  TEXT:
    type: string
    description: Text to translate
    required: true
    examples: ["Hello world", "How are you?"]
  
  SOURCE_LANG:
    type: string
    description: Source language code
    default: "en"
    validation:
      enum: ["en", "es", "fr", "de", "it", "pt", "ru", "zh"]
  
  TARGET_LANG:
    type: string
    description: Target language code
    required: true
    validation:
      enum: ["en", "es", "fr", "de", "it", "pt", "ru", "zh"]

function-calling:
  schema-generation:
    include-descriptions: true
    include-defaults: true
    example-generation: true

tags: [translation, api, read]
timeout: 10000

tests:
  - name: test-english-to-spanish
    parameters:
      TEXT: "Hello world"
      SOURCE_LANG: "en"
      TARGET_LANG: "es"
    expected:
      exit-code: 0
      output-contains: "Hola mundo"
```

## Implementation Considerations

This section provides technical guidance for developers implementing the Custom Tools feature. It focuses on the internal mechanisms and considerations that aren't directly visible to end users but are essential for a robust implementation.

### Parameter Substitution and Escaping

#### Parameter Resolution Process

1. **Parse the command template**:
   - Identify parameter references using the `{PARAM}` syntax
   - Identify transforms and formats like `{PARAM:transform}` or `{PARAM:format(pattern)}`
   - Identify conditional expressions like `{condition ? value1 : value2}`

2. **Resolve parameter values**:
   - For each parameter reference, look up the actual value
   - Apply any transforms specified for the parameter
   - Apply any format specifications

3. **Handle special references**:
   - Step output references (`{step1.output}`)
   - Predefined variables (`{WORKSPACE}`, `{TEMP}`, etc.)
   - Environment variable references

4. **Security processing**:
   - Apply escaping rules based on the `security.escape-shell` setting
   - Apply masking for sensitive values in logs and output

#### Escaping Implementation

Shell command escaping should follow these rules:

```
# Bash escaping
function escapeForBash(value) {
    return value.replace(/(["\s'$`\\])/g, '\\$1');
}

# PowerShell escaping
function escapeForPowerShell(value) {
    return value.replace(/(["`$])/g, '`$1');
}

# CMD escaping
function escapeForCmd(value) {
    // Escape special characters for CMD
    let escaped = value.replace(/([&|<>^()])/g, '^$1');
    // Handle quotes specially
    if (escaped.includes('"')) {
        escaped = escaped.replace(/"/g, '""');
        escaped = `"${escaped}"`;
    } else if (escaped.includes(' ')) {
        escaped = `"${escaped}"`;
    }
    return escaped;
}
```

#### Parameter Type Handling

Different parameter types require different handling:

- **string**: Direct substitution with escaping
- **number**: Convert to string before substitution
- **boolean**: Convert to appropriate shell representation based on format
- **array**: Join with spaces or the specified delimiter
- **object**: Convert to JSON string unless format specifies otherwise

### Security Enforcement

#### Privilege Control Implementation

1. **same-as-user** (default):
   - Execute the command with the same user privileges
   - No special handling required

2. **reduced**:
   - On Unix-like systems, use mechanisms like setuid/setgid
   - On Windows, use job objects with restricted tokens
   - Example implementation:
     ```c#
     if (securityConfig.ExecutionPrivilege == "reduced") {
         if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
             // Create restricted token for Windows
             CreateRestrictedToken(out var restrictedToken);
             startInfo.UseShellExecute = false;
             startInfo.UserName = null;
             // Apply restricted token
             procSecurity.SetTokenInformation(TokenInformationClass.TokenIntegrityLevel, restrictedToken);
         } else {
             // On Unix, drop privileges
             startInfo.EnvironmentVariables["LD_PRELOAD"] = "/path/to/privdrop.so";
         }
     }
     ```

3. **elevated** (requires approval):
   - Use platform-specific elevation mechanisms
   - Require explicit user approval before execution
   - Log all elevated command executions

#### Isolation Implementation

1. **none**:
   - Execute in the same process
   - Minimal overhead but higher risk

2. **process** (default):
   - Execute in a separate process
   - Example implementation:
     ```c#
     var process = new Process
     {
         StartInfo = new ProcessStartInfo
         {
             FileName = shellPath,
             Arguments = shellArgs,
             RedirectStandardOutput = true,
             RedirectStandardError = true,
             UseShellExecute = false,
             CreateNoWindow = true
         }
     };
     ```

3. **container**:
   - Execute in a container (Docker, etc.)
   - Highest isolation but requires container runtime
   - Example implementation:
     ```c#
     var process = new Process
     {
         StartInfo = new ProcessStartInfo
         {
             FileName = "docker",
             Arguments = $"run --rm {containerConfig} {imageName} {containerCommand}",
             RedirectStandardOutput = true,
             RedirectStandardError = true,
             UseShellExecute = false
         }
     };
     ```

#### Permission Validation

Before executing a tool, validate that all required permissions are granted:

```c#
foreach (var permission in tool.Security.RequiredPermissions)
{
    if (!IsPermissionGranted(permission))
    {
        throw new SecurityException($"Permission not granted: {permission}");
    }
}
```

### Cross-Platform Compatibility

#### Platform Detection

Detect the current platform:

```c#
string GetCurrentPlatform()
{
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        return "windows";
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        return "linux";
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        return "macos";
    else
        return "unknown";
}
```

#### Command Selection

Select the appropriate command for the current platform:

```c#
string GetCommandForCurrentPlatform(CommandsConfig commands)
{
    string platform = GetCurrentPlatform();
    
    if (commands.Platforms.TryGetValue(platform, out var platformCommand))
        return platformCommand;
    
    return commands.Default;
}
```

#### Path Normalization

Implement path normalization:

```c#
string NormalizePath(string path, bool autoConvert)
{
    if (!autoConvert)
        return path;
    
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        return path.Replace("/", "\\");
    else
        return path.Replace("\\", "/");
}
```

### Output Capturing and Streaming

#### Buffered Output Capture

For buffered output:

```c#
var output = new StringBuilder();
var error = new StringBuilder();

process.OutputDataReceived += (sender, e) => {
    if (e.Data != null)
        output.AppendLine(e.Data);
};

process.ErrorDataReceived += (sender, e) => {
    if (e.Data != null)
        error.AppendLine(e.Data);
};

process.Start();
process.BeginOutputReadLine();
process.BeginErrorReadLine();
process.WaitForExit();

return new CommandResult {
    ExitCode = process.ExitCode,
    Output = output.ToString(),
    Error = error.ToString()
};
```

#### Streamed Output Implementation

For streamed output:

```c#
process.OutputDataReceived += (sender, e) => {
    if (e.Data != null)
        streamCallback(StreamType.Output, e.Data);
};

process.ErrorDataReceived += (sender, e) => {
    if (e.Data != null)
        streamCallback(StreamType.Error, e.Data);
};

process.Start();
process.BeginOutputReadLine();
process.BeginErrorReadLine();
process.WaitForExit();
```

#### Large Output Handling

When dealing with potentially large outputs:

```c#
if (output.Length > outputConfig.BufferLimit)
{
    if (outputConfig.Truncation)
    {
        // Truncate to buffer limit
        output = output.ToString(0, outputConfig.BufferLimit);
        logger.Warning($"Output truncated to {outputConfig.BufferLimit} bytes");
    }
    else
    {
        throw new OutputTooLargeException($"Output exceeds buffer limit of {outputConfig.BufferLimit} bytes");
    }
}
```

### Resource Management and Cleanup

#### Timeout Implementation

Implement timeouts for commands:

```c#
var cancellationTokenSource = new CancellationTokenSource(timeoutMs);
var task = process.WaitForExitAsync(cancellationTokenSource.Token);

try
{
    await task;
}
catch (OperationCanceledException)
{
    // Timeout occurred
    process.Kill(entireProcessTree: true);
    throw new CommandTimeoutException($"Command timed out after {timeoutMs}ms");
}
```

#### Memory Limits

Implement memory limits on Windows:

```c#
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    var jobObject = CreateJobObject(IntPtr.Zero, null);
    var info = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION
    {
        BasicLimitInformation = new JOBOBJECT_BASIC_LIMIT_INFORMATION
        {
            LimitFlags = JOB_OBJECT_LIMIT_PROCESS_MEMORY
        },
        ProcessMemoryLimit = resourceConfig.MaxMemory
    };
    
    SetInformationJobObject(jobObject, JobObjectInfoType.ExtendedLimitInformation, ref info);
    AssignProcessToJobObject(jobObject, process.Handle);
}
```

On Unix-like systems:

```c#
if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    // Use cgroups to limit memory
    var cgroup = $"/sys/fs/cgroup/memory/cycod/{processId}";
    Directory.CreateDirectory(cgroup);
    File.WriteAllText($"{cgroup}/memory.limit_in_bytes", resourceConfig.MaxMemory.ToString());
    File.WriteAllText($"{cgroup}/tasks", processId.ToString());
}
```

#### Reliable Cleanup

Ensure cleanup happens even after errors:

```c#
try
{
    // Execute command
}
finally
{
    // Always run cleanup commands
    foreach (var cleanup in resourceConfig.Cleanup)
    {
        try
        {
            ExecuteCleanupCommand(cleanup);
        }
        catch (Exception ex)
        {
            logger.Warning($"Cleanup command failed: {ex.Message}");
        }
    }
}
```

### Multi-Step Execution

#### Step Dependency Resolution

For multi-step tools with dependencies:

```c#
// Build the dependency graph
var graph = new Dictionary<string, List<string>>();
foreach (var step in tool.Steps)
{
    graph[step.Name] = step.WaitFor ?? new List<string>();
}

// Perform topological sort to determine execution order
var executionOrder = TopologicalSort(graph);

// Execute steps in order (or in parallel where possible)
foreach (var stepName in executionOrder)
{
    var step = tool.Steps.First(s => s.Name == stepName);
    
    if (step.Parallel && CanRunInParallel(step, runningSteps))
    {
        // Start step in parallel
        runningSteps.Add(ExecuteStepAsync(step));
    }
    else
    {
        // Wait for any dependencies to complete
        await Task.WhenAll(runningSteps.Where(s => step.WaitFor.Contains(s.Name)));
        
        // Execute step
        await ExecuteStepAsync(step);
    }
}
```

#### Condition Evaluation

Evaluate step conditions:

```c#
bool ShouldRunStep(StepConfig step, Dictionary<string, StepResult> previousResults)
{
    if (string.IsNullOrEmpty(step.RunCondition))
        return true;
    
    // Parse and evaluate the condition
    return EvaluateCondition(step.RunCondition, previousResults);
}

bool EvaluateCondition(string condition, Dictionary<string, StepResult> previousResults)
{
    // Replace references to previous step results
    var parsedCondition = ReplaceStepReferences(condition, previousResults);
    
    // Use a JavaScript engine or expression evaluator
    return jsEngine.Evaluate<bool>(parsedCondition);
}
```

## Tool Versioning

Custom Tools support versioning to manage changes and ensure compatibility over time.

### Version Properties

Tools can define version information:

```yaml
version: 1.0.0                    # Tool version (semantic versioning)
min-cycod-version: "1.2.0"        # Minimum CYCOD version required

changelog:
  - version: 1.0.0
    changes: "Initial release"
  - version: 1.1.0
    changes: "Added support for parallel execution"
```

### Version Compatibility

CYCOD enforces compatibility between tools and the runtime:

1. **Runtime Compatibility**: 
   - The `min-cycod-version` property specifies the minimum CYCOD version required
   - Tools with features not supported by the current CYCOD version will be rejected

2. **Tool Upgrades**:
   - When updating a tool, specify a new version number
   - Use the changelog to document changes between versions
   - Breaking changes should increment the major version number

### Backward Compatibility

When designing new versions of tools:

1. Maintain backward compatibility when possible
2. For breaking changes, consider creating a new tool or providing migration guidance
3. Use the `deprecated` property to mark obsolete tools or parameters

```yaml
deprecated: true                  # Mark a tool as deprecated
deprecated-message: "Use tool-v2 instead"  # Message for users

parameters:
  OLD_PARAM:
    type: string
    deprecated: true              # Mark a parameter as deprecated
    deprecated-alternative: "NEW_PARAM"  # Suggested alternative
```

## Environment Variables

Custom Tools can interact with environment variables in several ways:

### Defining Environment Variables

```yaml
environment:
  variables:
    HTTP_PROXY: "{PROXY_URL}"     # Set from parameter
    DEBUG: "1"                    # Set to fixed value
  inherit: true                   # Inherit parent process environment
```

### Accessing Environment Variables

Environment variables can be accessed in commands:

```yaml
# Bash
bash: echo $HOME

# Windows CMD
cmd: echo %USERPROFILE%

# PowerShell
pwsh: Write-Output $env:USERPROFILE
```

### Secrets and Sensitive Values

For sensitive values:

```yaml
environment:
  variables:
    API_KEY: "{API_KEY}"
  secrets:
    - API_KEY                     # Mark as sensitive
```

Sensitive values are:
- Masked in logs and output
- Not included in error messages
- Stored securely when used with scope

This helps prevent accidental exposure of sensitive information.