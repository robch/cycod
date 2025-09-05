# CYCOD Custom Tools Specification

## Overview

Custom Tools provide a mechanism for CYCOD users to define, share, and execute shell commands as named tools with parameters. These tools can be used by LLMs through function calling, similar to MCPs but focused on shell command execution. The tools can also be used by other tools, or by humans via slash commands or on the CLI via `cycod tool run <tool-name>`.

### Purpose

Custom Tools allow users to:
1. Wrap frequently used shell commands with a consistent interface
2. Define parameters with descriptions and default values
3. Create multi-step tools
4. Share tools across multiple scopes (local, user, global)
5. Categorize tools for organization and security

## Tool Definition

### File Format and Location

Tool definitions are stored as YAML files in in the following locations:
- .cycod/tools/ folders
  * Local, User, Global scopes (see `cycod help find scope --expand`)
- any other folder if loaded via `--load-tool` or `--load-tools`

Tool files use the `.yaml` extension. The filename determines the tool's name if not explicitly specified in the YAML content.

### Schema

```yaml
# Basic information (for AI or Human)
name: tool-name                         # (optional if matches filename)
description: Tool description           # (optional if no more info is needed for the AI or Human)
parameters:                             # (optional section for parameters to tool)
  PARAM-NAME-GOES-HERE:
    description: Parameter description  # (optional if no more info is needed for the LLM)
    type: string                        # (optional; if not spepcified, default is string)
    required: true                      # (optional; if not specified, the is false/true if default provided or not)
    default: ''                         # (optional; if required: true, and no default, the default will be empty string)

  ANOTHER-PARAM-NAME-GOES-HERE:
    description: Another  description   # (optional if no more info is needed for the LLM)
    type: string                        # (optional; if not spepcified, default is string)
    required: false                     # (optional; if not specified, the is false/true if default provided or not)
    default: ''                         # (optional; if required: true, and no default, the default will be empty string)

# Selection criteria (which platform, what tags; used to allow and/or approve)
platforms: [windows, linux, macos]      # (optional, if not specified, defaults to all platforms)
tags: [tag1, tag2, read]                # (optional, if not specified, defaults to no tags, implying not read/write, but run security category)

# `uses` and `with` sections reference by name or define sub-resources (config, tools, mcps, ...)
uses:
  config: name0                         # (optional, single config name, found in scope folders)
  configs: [name1, ...]                 # (optional, array of config names, found in scope folders)
  profile: name0                        # (optional, single profile name, found in scope folders)
  profiles: [name1, ...]                # (optional, array of profile names, found in scope folders)
  mcp: name0                            # (optional, single MCP name, found in scope folders)
  mcps: [name1, ...]                    # (optional, array of MCP names, found in scope folders)
  tool: name0                           # (optional, single tool name, found in scope folders)
  tools: [name1, ...]                   # (optional, array of tool names, found in scope folders)

with:
  config: { ... }                       # (optional, inline config)
  mcps: [ { ... } ]                     # (optional, inline MCPs)
  tools: [ { ... } ]                    # (optional, inline tools)

# ---------------------------------------------------------------------------------------------
#  Single-step tool
# ---------------------------------------------------------------------------------------------

## OPTION 1: Execute a SINGLE process ---------------------------------------------------------

# (`run:` is required for OPTION 1, optional if using other options (2-5, or multi-step tool))
run: "..."                              # (optional, process name w/ optional arguments; if process name contains spaces, must be enclosed in double-quotes)
arguments: {}                           # (optional, process arguments; added to any args specified in `run:`)
input: "..."                            # (optional, data to pass via stdin)

# OPTION 2: Execute a SINGLE script (via alias as key) ----------------------------------------

# (one of `cmd`, `bash`, `pwsh`, `python` is required for OPTION 2)
cmd: "..."                              # (optional, inline CMD command, single or multi-line; alias for { script: ..., shell: cmd })
bash: "..."                             # (optional, inline bash script, single or multi-line; alias for { script: ..., shell: bash })
pwsh: "..."                             # (optional, inline PowerShell script, single or multi-line; alias for { script: ..., shell: pwsh })
python: "..."                           # (optional, inline Python script, single or multi-line; alias for { script: ..., shell: python })
arguments: {}                           # (optional, shell script arguments)
input: "..."                            # (optional, data to pass via stdin)

# OPTION 3: Execute a SINGLE script (via default shell, alias, or custom shell script template)

# (`script:` is required for OPTION 3)
script: "..."                           # (optional, inline script, single or multi-line; alias for { script: ..., shell: default })
shell: default|cmd|bash|pwsh|"..."      # (optional, alias for shell template; use {0} for process name, {1} for arguments)
arguments: {}                           # (optional, shell script arguments)
input: "..."                            # (optional, data to pass via stdin)

# OPTION 4: Execute a SINGLE tool (defined inline above (see `with:` above), or referenced by name (see `uses:` above))
mcp: "..."                              # (optional, if tool is in MCP)
tool: "..."                             # (optional, tool name to execute)
arguments: {}                           # (optional, tool arguments, by name)
input: "..."                            # (optional, data to pass via stdin)

# OPTION 5: Execute multiple tools in sequence ------------------------------------------------
steps:
  - name: "..."                         # (optional, step name; if not supplied defaults to step{x})

    # (one of OPTION 1 thru 4 is required for each step)
    run: "..."
    # ...or...
    cmd: "..."
    # ...or...
    bash: "..."
    # ...or...
    pwsh: "..."
    # ...or...
    python: "..."
    # ...or...
    script: "..."
    # ...or...
    tool: "..."

    arguments: {} # (optional, arguments)
    input: "..."  # (optional, data to pass via stdin for process or scripts)

  - name: "..."

    # (one of OPTION 1 thru 4 is required for each step)
    run: "..."
    # ...or...
    cmd: "..."
    # ...or...
    bash: "..."
    # ...or...
    pwsh: "..."
    # ...or...
    python: "..."
    # ...or...
    script: "..."
    # ...or...
    tool: "..."

    arguments: {} # (optional, arguments)
    input: "..."  # (optional, data to pass via stdin for process or scripts)

# Optional settings for each SINGLE/MULTI step, or the tool overall
timeout: 60000                     # (optional, timeout; specified in milliseconds)
working-directory: ~/path          # (optional, working directory)
environment:                       # (optional, environment variables for the tool)
  VARIABLE_NAME: value             # Variable name and value
  ANOTHER_VAR: "{PARAM}"           # Variable with parameter substitution

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

## Command Line Interface

### Tool Management Commands

```
cycod tool add NAME [options]
cycod tool get NAME [--scope]
cycod tool list [--scope]
cycod tool remove NAME [--scope]
cycod tool run NAME [parameters] [options]
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
- `--python "COMMAND"` - Python command to execute
- `--run "COMMAND"` - Direct command to execute
- `--script "SCRIPT"` - Script content (use with --shell)
- `--shell "SHELL"` - Shell to use with script
- `--arguments "ARGS"` - Arguments to pass to the command
- `--input "INPUT"` - Data to pass via stdin

- `--step NAME "COMMAND"` - Define a step (can be used multiple times for multi-step tools)

- `--parameter NAME "DESC"` [param-options] - Define a parameter (multiple allowed)
  - `type=string|number|boolean|array` - Parameter type
  - `required=true|false` - Whether parameter is required
  - `default=VALUE` - Default value if not provided

- `--platform windows|linux|macos` - Supported platform (multiple allowed)
- `--tag TAG` - Tag for categorization (multiple allowed)
- `--timeout MILLISECONDS` - Default timeout
- `--working-directory DIR` - Working directory
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

CONSIDER:  
* Add `--contains`? to filter tools by name or description or shell content? (search code for `--contains` for symmetry)

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

### Tool Run Command

```
cycod tool run NAME [parameters] [options]
```

Options:
- `--scope` - Scope to search for the tool (local, user, global, any)
- `--param NAME=VALUE` - Set parameter values (can be used multiple times)
- `--show-command` - Show the command that would be executed without running it
- `--dry-run` - Parse and validate parameters without executing the command
- `--timeout MILLISECONDS` - Override the tool's default timeout

Example:
```
cycod tool run weather-lookup --param LOCATION=London
cycod tool run github-commit-stats --param OWNER=microsoft --param REPO=vscode --scope user
cycod tool run search-code --param PATTERN="TODO" --param DIRECTORY="./src" --show-command
```
```

## Parameter Substitution

### Parameter References

Parameters are referenced in commands using curly braces:

```yaml
bash: command {PARAM1} {PARAM2}
```

Parameters can also be referenced in the `arguments` field:

```yaml
bash: command
arguments:
  arg1: "{PARAM1}"
  arg2: "{PARAM2}"
```

By default, all parameters are safely escaped to prevent command injection. If you need to insert a parameter without escaping (use with caution), use the RAW prefix:

```yaml
bash: command {PARAM1} {RAW:PARAM2}
```

This will insert PARAM2 without any escaping, which can be useful for certain advanced scenarios but should be used carefully as it may introduce security risks.

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
- `{step1.output}` - Combined standard output and standard error from the step
- `{step1.stdout}` - Standard output from the step
- `{step1.stderr}` - Standard error from the step
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

### Environment Variables

Tools can specify environment variables that will be set when the tool runs:

```yaml
environment:
  API_KEY: "{API_KEY}"         # From a parameter
  DEBUG: "true"                # Static value
  PATH: "${PATH}:/usr/local/bin"  # Extend existing environment variable
```

Environment variables can be set at the tool level (applies to all steps) or at the step level (applies only to that step).

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
   OR: cycod tool run TOOL_NAME [options]

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
    run             Execute a tool with parameters

SEE ALSO

  cycod help tool list
  cycod help tool get
  cycod help tool add
  cycod help tool remove
  cycod help tool run
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
    --python "COMMAND"              Define a Python command to execute
    --run "COMMAND"                 Define a direct command to execute
    --script "SCRIPT"               Define script content (use with --shell)
    --shell "SHELL"                 Shell to use with script
    --arguments "ARGS"              Arguments to pass to the command
    --input "INPUT"                 Data to pass via stdin

  MULTI-STEP TOOLS

    --step NAME "COMMAND"           Define a step (can be used multiple times)

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
    --env NAME=VALUE                Set environment variable (can be used multiple times)

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

### Tool Run Help

```
cycod help tool run
```

Output:
```
CYCOD TOOL RUN

  Execute a custom tool with specified parameters.

USAGE: cycod tool run TOOL_NAME [options]

OPTIONS

  PARAMETER OPTIONS

    --param NAME=VALUE              Set parameter value (can be used multiple times)

  EXECUTION OPTIONS

    --show-command                  Show the command that would be executed without running it
    --dry-run                       Parse and validate parameters without executing the command
    --timeout MILLISECONDS          Override the tool's default timeout

  SCOPE OPTIONS

    --global, -g                    Run a tool from global scope (all users)
    --user, -u                      Run a tool from user scope (current user)
    --local, -l                     Run a tool from local scope (current directory)
    --any, -a                       Run a tool from any scope (default)

EXAMPLES

  EXAMPLE 1: Run a weather lookup tool with a location parameter

    cycod tool run weather-lookup --param LOCATION=London

  EXAMPLE 2: Run a tool with multiple parameters

    cycod tool run github-commit-stats --param OWNER=microsoft --param REPO=vscode

  EXAMPLE 3: Show the command that would be executed without running it

    cycod tool run search-code --param PATTERN="TODO" --param DIRECTORY="./src" --show-command

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
    environment:
      GITHUB_TOKEN: "{AUTH_TOKEN}"  # Step-specific environment variable

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
  AUTH_TOKEN:
    type: string
    description: GitHub authentication token
    required: false

environment:
  DEBUG: "false"  # Tool-level environment variable applies to all steps

tags: [github, api, read]
timeout: 15000
```

### Python Tool Example

```yaml
name: analyze-data
description: Analyze data from a CSV file using Python

python: |
  import pandas as pd
  import sys
  
  # Read the CSV file
  data = pd.read_csv('{FILE_PATH}')
  
  # Perform analysis based on type
  if '{ANALYSIS_TYPE}' == 'summary':
      result = data.describe()
  elif '{ANALYSIS_TYPE}' == 'head':
      result = data.head({NUM_ROWS})
  elif '{ANALYSIS_TYPE}' == 'unique':
      result = data['{COLUMN}'].unique()
  else:
      print("Unknown analysis type")
      sys.exit(1)
  
  # Print the result
  print(result)

parameters:
  FILE_PATH:
    type: string
    description: Path to the CSV file
    required: true
  ANALYSIS_TYPE:
    type: string
    description: Type of analysis to perform (summary, head, unique)
    required: true
    default: "summary"
  NUM_ROWS:
    type: number
    description: Number of rows to display for head analysis
    required: false
    default: 5
  COLUMN:
    type: string
    description: Column name for unique value analysis
    required: false

tags: [data-analysis, python, read]
platforms: [windows, linux, macos]
timeout: 20000
```

### Tool with Complex Parameter Handling

```yaml
name: search-code
description: Search for patterns in code files

bash: grep -r {CASE_SENSITIVE} {WHOLE_WORD} "{PATTERN}" {DIRECTORY} --include="*.{FILE_TYPE}"
arguments:
  output-file: "{OUTPUT_FILE}"

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
    
  OUTPUT_FILE:
    type: string
    description: File to write results to (optional)
    required: false

tags: [search, code, read]
platforms: [windows, linux, macos]
```

## Need to research/decide
* Detail how calling MCP tools works in here
* Detail how calling custom tools works in here
* Detail how tools are "included for use" but not exposed from here directly
* Detail how mcps are "included for use" but not exposed here directly