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

# Execution (one of the following)
bash: command {PARAM}              # For bash execution
cmd: command {PARAM}               # For Windows CMD execution
pwsh: command {PARAM}              # For PowerShell execution
run: command {PARAM}               # For direct command execution
script: |                          # For custom scripts
  command line 1
  command line 2
shell: shell-name                  # Shell to use with script

# OR for multi-step tools
steps:
  - name: step1                    # Step name (required)
    bash: command {PARAM}          # Command to execute
    continue-on-error: false       # Whether to continue if this step fails
    run-condition: "{step2.exit-code} == 0"  # Condition for when to run this step
  
  - name: step2
    bash: command {PARAM}

# Parameters
parameters:
  PARAM1:
    type: string                   # Parameter type (string, number, boolean, array)
    description: Parameter description  # Required
    required: true                 # Whether parameter is required
    default: default value         # Default value if not provided

  PARAM2:
    type: number
    description: Another parameter
    required: false
    default: 5

# Optional settings
timeout: 60000                     # Timeout in milliseconds
working-directory: ~/path          # Working directory
platforms: [windows, linux, macos] # Supported platforms
tags: [tag1, tag2, read]           # Categories and security tags
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

### Parameter Definition

Parameters define inputs that can be substituted into commands:

```yaml
parameters:
  NAME:
    type: string                # Parameter type
    description: Description    # Human-readable description
    required: true              # Whether parameter is required
    default: Default value      # Default value if not provided
```

Parameter types include:
- `string`: Text values
- `number`: Numeric values
- `boolean`: True/false values
- `array`: List of values

### Tags and Platform Support

Tools can specify which platforms they work on and include tags for categorization:

```yaml
platforms: [windows, linux, macos]  # Only include platforms the tool works on
tags: [category1, category2, read]  # Tags for categorization and security
```

The security tags (`read`, `write`, `run`) are recommended but optional. If no security tag is present, the tool is considered high-risk and requires explicit approval.

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
```

## Command Line Interface

### Tool Management Commands

```
cycod tool add NAME [options]
cycod tool get NAME [--scope]
cycod tool list [--scope]
cycod tool remove NAME [--scope]
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

- `--parameter NAME "DESC"` [param-options] - Define a parameter (multiple allowed)
  - `type=string|number|boolean|array` - Parameter type
  - `required=true|false` - Whether parameter is required
  - `default=VALUE` - Default value if not provided

- `--platform windows|linux|macos` - Supported platform (multiple allowed)
- `--tag TAG` - Tag for categorization (multiple allowed)
- `--timeout MILLISECONDS` - Default timeout
- `--working-directory DIR` - Working directory
- `--input "INPUT"` - Data to pass via stdin
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

### Tool Get Command

```
cycod tool get NAME [--scope]
```

Options:
- `--scope` - Scope to search in (local, user, global, any)

Example:
```
cycod tool get weather-lookup
cycod tool get weather-lookup --scope user
```

### Tool List Command

```
cycod tool list [--scope]
```

Options:
- `--scope` - Scope to list from (local, user, global, any)

Example:
```
cycod tool list
cycod tool list --scope global
```

### Tool Remove Command

```
cycod tool remove NAME [--scope]
```

Options:
- `--scope` - Scope to remove from (local, user, global, any)

Example:
```
cycod tool remove weather-lookup
cycod tool remove weather-lookup --scope user
```

## Parameter Substitution

### Parameter References

Parameters are referenced in commands using curly braces:

```yaml
bash: command {PARAM1} {PARAM2}
```

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

SEE ALSO

  cycod help tool list
  cycod help tool get
  cycod help tool add
  cycod help tool remove
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

  PARAMETERS

    --parameter NAME "DESC" [options]  Define a parameter (can be used multiple times)
      type=string|number|boolean|array  Parameter type
      required=true|false               Whether parameter is required
      default=VALUE                     Default value if not provided

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
      --parameter LOCATION "City or airport code" required=true \
      --parameter FORMAT "Output format" default=3 \
      --tag weather --tag api --tag read

  EXAMPLE 2: Create a multi-step tool that processes data

    cycod tool add process-data --description "Process data from a file" \
      --step fetch "curl -s '{URL}' > data.json" \
      --step process "jq '.items' data.json" \
      --step cleanup "rm data.json" \
      --parameter URL "URL to fetch data from" required=true \
      --tag api --tag read

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

USAGE: cycod tool get TOOL_NAME [--scope]

OPTIONS

  SCOPE OPTIONS

    --global, -g    Look for the tool in global scope (all users)
    --user, -u      Look for the tool in user scope (current user)
    --local, -l     Look for the tool in local scope (current directory)
    --any, -a       Look for the tool in any scope (default)

EXAMPLES

  EXAMPLE 1: Get a tool from any scope

    cycod tool get weather-lookup

  EXAMPLE 2: Get a tool from user scope

    cycod tool get weather-lookup --user

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

USAGE: cycod tool list [--scope]

OPTIONS

  SCOPE OPTIONS

    --global, -g    List only global tools (all users)
    --user, -u      List only user tools (current user)
    --local, -l     List only local tools (current directory)
    --any, -a       List tools from all scopes (default)

EXAMPLES

  EXAMPLE 1: List all tools from all scopes

    cycod tool list

  EXAMPLE 2: List only user tools

    cycod tool list --user

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

USAGE: cycod tool remove TOOL_NAME [--scope]

OPTIONS

  SCOPE OPTIONS

    --global, -g    Delete the tool from global scope (all users)
    --user, -u      Delete the tool from user scope (current user)
    --local, -l     Delete the tool from local scope (current directory)
    --any, -a       Delete the tool from the first scope it's found in (default)

EXAMPLES

  EXAMPLE 1: Delete a tool from any scope

    cycod tool remove weather-lookup

  EXAMPLE 2: Delete a tool from user scope

    cycod tool remove weather-lookup --user

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
  FORMAT:
    type: string
    description: Output format
    required: false
    default: "3"

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

tags: [github, api, read]
timeout: 15000
```

### Tool with Conditional Steps

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

tags: [github, git, clone, write]
platforms: [windows, linux, macos]
```

### Tool with Complex Parameter Handling

```yaml
name: search-code
description: Search for patterns in code files

bash: grep -r {CASE_SENSITIVE} {WHOLE_WORD} "{PATTERN}" {DIRECTORY} --include="*.{FILE_TYPE}"

parameters:
  PATTERN:
    type: string
    description: Pattern to search for
    required: true

  DIRECTORY:
    type: string
    description: Directory to search in
    default: "."

  FILE_TYPE:
    type: string
    description: File extension to search in
    default: "*"

  CASE_SENSITIVE:
    type: boolean
    description: Whether to use case-sensitive search
    default: false
    # This will be converted to -i for false or empty string for true

  WHOLE_WORD:
    type: boolean
    description: Whether to search for whole words only
    default: false
    # This will be converted to -w for true or empty string for false

tags: [search, code, read]
platforms: [windows, linux, macos]
```