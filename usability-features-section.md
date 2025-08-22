## Additional Usability Features

### File-Based Definition

Tools can be defined and managed using YAML files:

```bash
# Create a tool from a YAML file
cycod tool add --from-file tool-definition.yaml

# Open the default editor to create or edit a tool
cycod tool add tool-name --editor

# Export an existing tool to a YAML file
cycod tool export tool-name --output tool-definition.yaml
```

### Testing Framework

Tools can include built-in tests to verify functionality:

```yaml
tests:
  - name: basic-test
    description: "Test basic functionality"
    parameters:                           # Test parameters
      URL: "https://example.com/repo.git"
      DIRECTORY: "test-dir"
    expected:                             # Expected results
      exit-code: 0                        # Expected exit code
      output-contains: "Cloning into 'test-dir'..."  # Text that should be in output
      output-matches: "^Clone.*successful$"  # Regex pattern for output
      file-exists: "test-dir/.git/config"  # File that should exist after running
    cleanup:                              # Commands to run after test
      - "rm -rf test-dir"                 # Clean up after test
```

### Resource Management

Tools can specify resource constraints and cleanup actions:

```yaml
resources:
  timeout: 60000                          # Overall timeout in milliseconds
  max-memory: 512MB                       # Maximum memory usage
  max-cpu: 50%                            # CPU usage limit
  cleanup:                                # Cleanup actions
    delete-temp-files: true               # Whether to delete temp files
    final-command: "docker rm {CONTAINER_ID}"  # Command to run for cleanup
```

### Interactive Mode

Tools can support interactive input during execution:

```yaml
interactive: true                         # Tool may prompt for user input
interactive-options:
  timeout: 30000                          # How long to wait for user input
  default-response: "y"                   # Default if no input provided
  prompts:                                # Defined prompts
    - text: "Do you want to continue? [Y/n]"  # Prompt text
      variable: CONTINUE                  # Where to store the response
      validation: "^[YyNn]$"              # Validate the response
```