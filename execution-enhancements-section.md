## Execution Enhancements

### Parallel Execution

Multi-step tools can execute steps in parallel for better performance:

```yaml
steps:
  - name: step1
    bash: command1 {PARAM}
    parallel: false                    # Default is sequential

  - name: step2
    bash: command2 {PARAM}
    parallel: true                     # Execute in parallel with previous step

  - name: step3
    bash: command3
    wait-for: [step1, step2]           # Wait for specific steps to complete
```

### Output Streaming and Management

Tools can control how command output is handled:

```yaml
steps:
  - name: generate-large-output
    bash: large-data-command
    output:
      mode: stream                     # Options: buffer, stream
      buffer-limit: 10MB               # Maximum size when buffering
      stream-callback: console         # Where to stream (console, file, function)
      truncation: true                 # Whether to truncate if limit exceeded
```

### Advanced Error Recovery

Tools can define sophisticated error handling strategies:

```yaml
steps:
  - name: risky-step
    bash: command {PARAM}
    error-handling:
      retry:
        attempts: 3                    # Number of retry attempts
        delay: 1000                    # Milliseconds between retries
        backoff: exponential           # Delay strategy (none, linear, exponential)
      fallback: alternative-command {PARAM}  # Command to run if all retries fail
      on-error-output: "Error occurred: {error}"  # Custom error message
```

### Cross-Platform Path Handling

Tools can specify how to handle file paths across different platforms:

```yaml
file-paths:
  normalize: true                      # Convert paths to platform-specific format
  working-directory: "{WORKSPACE}"     # Base directory for relative paths
  temp-directory: "{TEMP}/cycod-tools/{TOOL_NAME}"  # Directory for temporary files
  cross-platform:
    windows-separator: "\\"            # Windows path separator
    unix-separator: "/"                # Unix path separator
    auto-convert: true                 # Automatically convert between platforms
```

### Platform-Specific Commands

Tools can provide different commands for different platforms:

```yaml
commands:
  default: find {DIRECTORY} -name "{PATTERN}"  # Default command
  platforms:
    windows: powershell -Command "Get-ChildItem -Path '{DIRECTORY}' -Filter '{PATTERN}' -Recurse"
    unix: find {DIRECTORY} -name "{PATTERN}"   # Unix-specific command
```