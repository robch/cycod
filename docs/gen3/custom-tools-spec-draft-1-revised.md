# CYCOD Custom Tools Specification (Revised)

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

Configure how the tool is exposed to LLMs through function calling:

```yaml
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