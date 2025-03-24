# Chatx Tests

This directory contains all tests for the Chatx project. The test structure mirrors the source code structure for easy navigation and maintenance.

## Test Structure

- Tests are organized to match the source code structure
- `src/Templates` → `tests/Templates`
- `src/Services` → `tests/Services`
- etc.

### Organization Rules

1. **Single test file per class**: If a component only needs one test file, place it directly in the corresponding folder
   - Example: `tests/Templates/SimpleClassTests.cs`

2. **Multiple test files per class**: For complex components that require multiple test files, create a subfolder with the component name
   - Example: `tests/Templates/ExpressionCalculator/ArithmeticTests.cs`, `tests/Templates/ExpressionCalculator/LogicalOperationTests.cs`, etc.

## Running Tests

To run all tests, use:

```bash
dotnet test
```

To run tests in a specific directory:

```bash
dotnet test --filter "FullyQualifiedName~Templates"
```

To run a specific test class:

```bash
dotnet test --filter "FullyQualifiedName~ExpressionCalculatorArithmeticTests"
```

## Benefits of This Structure

1. **Discoverability**: Tests are easy to find alongside their corresponding source code paths
2. **Maintainability**: Clear organization keeps the test suite manageable as it grows
3. **Scalability**: New tests can easily be added following the established pattern
4. **Focus**: Running tests for specific components is straightforward

## Adding New Tests

When adding tests for a new component, follow these steps:

1. Identify the path in `src/` where the component is located
2. Create a matching path in `tests/`
3. Create a test file with the naming convention `{ClassUnderTest}Tests.cs`
4. If needed, break complex tests into multiple files in a dedicated subfolder