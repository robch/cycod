# Contributing to ChatX

Thank you for your interest in contributing to ChatX! This document provides guidelines and instructions for contributing to this project.

## Code of Conduct

Please be respectful and considerate of others when contributing to this project. We aim to foster an inclusive and welcoming community.

## Getting Started

1. **Fork the repository** on GitHub
2. **Clone your fork** locally:
   ```
   git clone https://github.com/yourusername/chatx.git
   cd chatx
   ```
3. **Set up the development environment**:
   - Install .NET 8.0 SDK or later
   - Build the project with `dotnet build`

## Development Workflow

### Branch Strategy

- **main**: Production-ready code
- **develop**: Integration branch for features
- Feature branches: Create from develop with format `feature/your-feature-name`
- Bug fix branches: Create with format `bugfix/issue-description`

### Making Changes

1. Create a new branch for your changes:
   ```
   git checkout -b feature/your-feature-name
   ```

2. Make your changes and ensure they follow the project's coding style

3. Write or update tests as necessary

4. Build and test your changes:
   ```
   dotnet build
   dotnet test
   ```

5. Commit your changes with clear, descriptive commit messages:
   ```
   git commit -m "Add feature: description of the feature"
   ```

### Pull Requests

1. Push your changes to your fork:
   ```
   git push origin feature/your-feature-name
   ```

2. Open a pull request against the `develop` branch

3. Describe your changes in detail:
   - What does this PR do?
   - What issue does it address?
   - Any relevant screenshots or output examples

4. Wait for review and address any feedback

## Adding New Functions

If you're adding new helper functions for the AI to call:

1. Create a new class in the `FunctionCallingTools` directory or add to existing classes

2. Use the `HelperFunctionDescription` and `HelperFunctionParameterDescription` attributes to document your function:
   ```csharp
   [HelperFunctionDescription("Description of what this function does")]
   public static string YourFunction(
       [HelperFunctionParameterDescription("Description of this parameter")] string param1)
   {
       // Implementation
   }
   ```

3. Register your function in the `ChatCommand.ExecuteAsync()` method

## Coding Guidelines

- Follow C# naming conventions and style
- Add XML documentation to public APIs
- Keep functions focused on a single responsibility
- Write meaningful variable and function names
- Use async/await patterns for I/O operations

## Testing

- Add unit tests for new functionality
- Ensure existing tests pass with your changes

## Documentation

- Update README.md if you add new features
- Document new command-line options
- Keep code comments up to date

## Submitting Issues

When submitting issues, please:

1. Check if the issue already exists
2. Use a clear and descriptive title
3. Include steps to reproduce the problem
4. Describe expected and actual behavior
5. Include environment details (OS, .NET version)
6. Add screenshots if applicable

## License

By contributing to this project, you agree that your contributions will be licensed under the same license as the project.