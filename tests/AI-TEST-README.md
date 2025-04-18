# AI Test Framework for ChatX

This document describes how we use the `ai test` YAML-based framework to test the ChatX CLI tool.

## About AI Test Framework

The `ai test` framework is a YAML-based test runner designed for testing command-line tools and scripts. It allows you to:

1. Define test cases with expected outputs
2. Run commands and verify their results
3. Create multi-step test sequences
4. Compare outputs using regex patterns
5. Run tests in parallel
6. Organize tests into logical areas

## Key Features Used in ChatX Tests

### Command Testing

Test that ChatX commands produce expected outputs:

```yaml
- name: Ask a simple question
  command: chatx --question "What is 2+2?"
  expect: The output should clearly state that 2+2=4
```

### Multi-Step Tests

Create tests with setup, test, and cleanup steps:

```yaml
- name: Continue from saved chat history
  steps:
  - name: Initial chat to create history
    command: chatx --question "Tell me a joke" --output-chat-history temp-test-continue.jsonl
    expect: The output should include a joke
  
  - name: Continue chat from saved history
    command: chatx --input-chat-history temp-test-continue.jsonl --question "Tell me another"
    expect: The output should include a different joke
  
  - name: Clean up test files
    bash: rm -f temp-test-continue.jsonl
```

### Expected Output Patterns

Define expectations for test outputs:

```yaml
expect: |
  The output should include "test-local-mcp"
  The output should include "test-user-mcp"
  The output should indicate the scope of each MCP server
```

This helps the LLM evaluating the test results understand what to look for.

### Shell Commands

Run shell commands for setup, validation, or cleanup:

```yaml
bash: |
  if [ -f temp-test-trajectory.md ]; then
    echo "Trajectory file was created successfully"
    rm temp-test-trajectory.md
    exit 0
  else
    echo "Error: Trajectory file was not created"
    exit 1
  fi
```

### Test Organization

Group related tests into areas:

```yaml
- area: Config List Command
  tests:
  - name: List all configuration settings
    command: chatx config list
    # ...

  - name: List only local configuration settings
    command: chatx config list --local
    # ...
```

## Running the Tests

You can run all or specific ChatX tests using the following commands:

```bash
# Run all ChatX tests
ai test tests/test_*.yaml

# Run a specific test file
ai test tests/test_core_chat_basic.yaml

# Run tests with specific tags
ai test tests/test_*.yaml --filter-tag provider
```

## Test Coverage

Our test suite covers:

1. Core chat functionality
2. Configuration management
3. Alias management
4. Prompt management
5. MCP server management
6. GitHub integration
7. Template features
8. Advanced features
9. Error handling and edge cases

## Extending the Tests

When adding new features to ChatX:

1. Create or update relevant YAML test files
2. Test both happy paths and error conditions
3. Include examples from help documentation
4. Keep tests isolated and clean up after themselves
5. Use descriptive test names that explain what's being tested