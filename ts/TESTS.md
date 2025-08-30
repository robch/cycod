# CYCODEV CLI Tests

This document describes the comprehensive Jest test suite for the CYCODEV CLI TypeScript implementation, which validates complete functional parity with the C# version.

## Test Overview

The test suite recreates the original YAML-based tests in Jest format, providing identical test scenarios and expectations to ensure the TypeScript CLI behaves exactly like the C# version.

### Test Coverage

- **25 total tests** covering all configuration command functionality
- **100% compatibility validation** with C# implementation
- **Real CLI execution testing** using actual command invocation
- **Cross-scope inheritance testing** for configuration precedence

## Test Structure

```
src/__tests__/
├── config/
│   ├── config-commands.test.ts    # Core config operations (16 tests)
│   ├── config-scopes.test.ts      # Scope inheritance (3 tests) 
│   ├── config-nested-keys.test.ts # Nested key handling (6 tests)
│   └── helpers/
│       └── CliTestHelper.ts       # CLI execution utilities
└── test-setup.ts                  # Jest configuration
```

## Test Categories

### 1. **Config Commands Test** (`config-commands.test.ts`)
Tests all core configuration operations:

- **Config List**: Show configurations from all scopes or specific scopes
- **Config Set/Get**: Store and retrieve configuration values
- **Config Clear**: Remove configuration settings
- **Config Add/Remove**: Manage list-type configuration values
- **Key Normalization**: Test proper casing (e.g., `openai.apikey` → `OpenAI.ApiKey`)

**Key Test Scenarios:**
```bash
cycod config list --local
cycod config set TestKey TestValue --local
cycod config get TestKey --local
cycod config clear TestKey --local
cycod config add TestList item1 --local
```

### 2. **Config Scopes Test** (`config-scopes.test.ts`)
Tests configuration inheritance and scope precedence:

- **Scope Inheritance**: Local > User > Global precedence
- **Value Overrides**: Higher priority scopes override lower ones
- **Cross-Scope Operations**: Getting values with `--any` flag
- **Scope Isolation**: Values set in one scope don't affect others

**Key Test Scenarios:**
```bash
# Test inheritance: set in global, override in user, get with --any
cycod config set BoolTest true --global
cycod config set BoolTest false --user  
cycod config get BoolTest --any  # Should return user value (false)
```

### 3. **Config Nested Keys Test** (`config-nested-keys.test.ts`)
Tests handling of hierarchical configuration keys:

- **Dot Notation**: `App.Setting.NestedValue`
- **Deep Nesting**: `Deep.Nested.Config.Key`
- **Cross-Scope Nesting**: Same nested key in different scopes
- **Override Behavior**: Nested key precedence rules

**Key Test Scenarios:**
```bash
cycod config set App.Setting.Nested NestedValue --local
cycod config get Deep.Nested.Config.Key --user
```

## Test Infrastructure

### CLI Test Helper
The `CliTestHelper` class provides utilities for executing CLI commands and validating output:

```typescript
// Execute CLI command and capture output
const result = await CliTestHelper.run('cycod config get TestKey --local');

// Validate exit code and output format
expect(result.exitCode).toBe(0);
expect(result.stdout).toMatchYamlRegex('LOCATION: .*\\.cycod.*\\(local\\).*TestKey: TestValue');
```

### YAML-Style Regex Matching
Custom Jest matcher `toMatchYamlRegex()` handles multiline patterns matching the original YAML test expectations:

```typescript
expect(result.stdout).toMatchYamlRegex(
  `LOCATION: .*[\\\\/]\\.cycod[\\\\/]config(\\.yaml){0,1} \\(local\\)` +
  `.*TestKey: TestValue`
);
```

## Running Tests

### Quick Commands

```bash
# Run all config tests
npm test -- src/__tests__/config/

# Run with detailed output
npm test -- src/__tests__/config/ --verbose

# Run specific test file
npm test -- src/__tests__/config/config-commands.test.ts

# Run in watch mode (auto-rerun on changes)
npm run test:watch -- src/__tests__/config/
```

### Individual Test Files

```bash
# Core configuration commands
npm test -- src/__tests__/config/config-commands.test.ts

# Scope inheritance behavior
npm test -- src/__tests__/config/config-scopes.test.ts

# Nested key handling
npm test -- src/__tests__/config/config-nested-keys.test.ts
```

### Development Commands

```bash
# Run with coverage report
npm run test:coverage -- src/__tests__/config/

# Run only failed tests from last run
npm test -- --onlyFailures

# Run tests matching a pattern
npm test -- --testNamePattern="Config Set"

# Debug with detailed error output
npm test -- src/__tests__/config/ --verbose --no-cache
```

## Test Output Examples

### Successful Test Run
```
PASS src/__tests__/config/config-commands.test.ts
  Config Commands
    Config List
      ✓ Config List (all scopes) (36 ms)
      ✓ Config List (local scope only) (35 ms)
    Config Set and Get
      ✓ Config Set (local scope) (36 ms)
      ✓ Config Get (local scope) (34 ms)

Test Suites: 3 passed, 3 total
Tests:       25 passed, 25 total
```

### Failed Test Example
```
● Config Commands › Config Get › Config Get (local scope)

Expected output to match regex pattern:
LOCATION: .*[\/]\.cycod[\/]config(\.yaml){0,1} \(local\).*TestKey: TestValue

Actual output:
TestKey: (not found or empty)
```

## Test Validation

The tests validate exact C# compatibility by checking:

1. **CLI Argument Parsing**: `--user`, `--local`, `--global`, `--any` flags
2. **Output Format**: Location headers, indented values, error messages
3. **Secret Masking**: API keys show first 2 characters (e.g., "12")  
4. **YAML Display**: Lists use dash format (`- item1`, `- item2`)
5. **Scope Precedence**: Local > User > Global inheritance
6. **Key Normalization**: `openai.apikey` → `OpenAI.ApiKey`

## Test Isolation

Tests use proper setup/cleanup to avoid interference:

- **beforeAll**: Clean up test keys before test suite
- **afterAll**: Clean up test keys after test suite  
- **Sequential Tests**: Tests within a suite build on each other
- **Scope Clearing**: Remove values from all scopes (`--global`, `--user`, `--local`)

## Debugging Tests

### Common Issues

1. **Build Required**: Tests automatically build TypeScript, but you can run `npm run build` manually
2. **Config Conflicts**: Tests clean up automatically, but manual cleanup: `cycod config clear TestKey --local`
3. **Timeout Issues**: Long-running tests have 10-second timeout per command

### Debug Commands

```bash
# Check what's in config files
cycod config list --local
cycod config list --user  
cycod config list --global

# Manually test CLI commands
node ./dist/bin/cycod.js config get TestKey --local

# Clean up test data manually  
cycod config clear TestKey --local
cycod config clear TestList --local
```

## Continuous Integration

The test suite is designed for CI/CD pipelines:

- **Fast Execution**: ~6 seconds total runtime
- **No External Dependencies**: Uses local config files
- **Deterministic**: Proper cleanup ensures consistent results
- **Comprehensive**: 100% functional coverage of config operations

For CI integration, run:
```bash
npm run build && npm test -- src/__tests__/config/
```

This validates that the TypeScript CLI implementation maintains complete compatibility with the C# version across all configuration management functionality.