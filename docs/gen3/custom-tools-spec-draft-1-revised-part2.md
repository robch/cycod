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

Parameter values can be transformed before use:

```yaml
parameters:
  USERNAME:
    type: string
    description: Username
    transform: "value.toLowerCase()"  # Convert to lowercase
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

### Security Enforcement

Security is enforced through the following mechanisms:
1. Security tags determine auto-approval/denial.
2. Execution privileges control the level of access the tool has.
3. Required permissions are validated before the tool is run.
4. Parameter escaping prevents command injection.

### Output Capturing and Streaming

Output can be captured or streamed:
1. Buffered output is stored in memory and available as `{step.output}`.
2. Streamed output is sent to the specified destination (console, file, callback).
3. Large outputs can be truncated to prevent memory issues.

### Resource Management and Cleanup

Resources are managed through:
1. Timeout limits for command execution.
2. Memory limits to prevent excessive memory usage.
3. Cleanup commands that run after the tool completes or fails.
4. Automatic deletion of temporary files.

## Testing Framework

The testing framework allows you to verify tool functionality:

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

Test execution:
1. Set up the test environment.
2. Run the tool with the specified parameters.
3. Verify the results against expectations.
4. Run cleanup commands.

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

SEE ALSO

  cycod help tool list
  cycod help tool get
  cycod help tool add
  cycod help tool remove
  cycod help tool test
  cycod help tool export
  cycod help tool import
  cycod help tools
```