# ChatX YAML-Based Tests

This directory contains YAML-based tests for the ChatX CLI tool using the ai test framework.

## Test Files

The YAML test files cover different aspects of ChatX functionality:

| File | Description |
|------|-------------|
| `test_core_chat_basic.yaml` | Basic chat functionality, questions, inputs, and output formats |
| `test_config_commands.yaml` | Configuration commands (list, get, set, clear, add, remove) |
| `test_alias_commands.yaml` | Alias commands (create, list, get, delete, use) |
| `test_prompt_commands.yaml` | Prompt commands (create, list, get, delete) |
| `test_mcp_commands.yaml` | MCP server commands (add, list, get, remove) |
| `test_github_integration.yaml` | GitHub integration, primarily the login command |
| `test_system_prompt_templates.yaml` | System prompt customization, templates, and foreach loops |
| `test_advanced_features.yaml` | Advanced features like providers, directories, threads, etc. |
| `test_slash_commands.yaml` | Slash command functionality in interactive mode |
| `test_help_examples.yaml` | Tests based on examples from help documentation |
| `test_error_handling.yaml` | Error handling and edge cases |

## Running Tests

### Prerequisites

1. Install the `ai` CLI tool
2. Make sure `chatx` is installed and in your PATH

### Running All Tests

```bash
ai test tests/test_*.yaml
```

### Running Specific Test Files

```bash
ai test tests/test_core_chat_basic.yaml
```

### Running Tests with Specific Tags

```bash
ai test tests/test_*.yaml --filter-tag provider
```

### Excluding Tests with Specific Tags

```bash
ai test tests/test_*.yaml --exclude-tag parallel
```

## Test Structure

Each test file follows this general structure:

```yaml
# File description

- area: Feature Area Name
  tests:
  - name: Test Case Name
    command: chatx --some-option
    expect: |
      The output should include specific information
      The response should meet certain criteria

  - name: Multi-step Test Case
    steps:
    - name: Step 1 - Setup
      command: chatx --setup-command
      expect: The output should confirm setup was successful
    
    - name: Step 2 - Execute Test
      command: chatx --test-command
      expect: The output should meet test criteria
    
    - name: Step 3 - Cleanup
      bash: rm -f test-files
```

## Test Tags

Some tests use tags to categorize them:

- `provider`: Tests that depend on specific AI providers
- `parallel`: Tests that demonstrate parallel execution
- `interactive`: Tests that simulate interactive chat sessions

## Adding New Tests

When adding new tests:

1. Choose an appropriate existing file or create a new one
2. Follow the established YAML structure
3. Give tests descriptive names
4. Include expectations for each test case
5. Clean up any files or settings modified by your tests
6. Consider adding tags for special test requirements

## Testing Considerations

- Some tests may require a working connection to AI services (OpenAI, Azure OpenAI, GitHub Copilot)
- Tests that modify configuration are designed to be non-destructive
- Interactive features that require user input are simulated rather than fully tested
- Some tests use simulated responses or expect specific patterns rather than exact outputs