# CycodBench: SWE-bench Benchmark Runner Punch List

This document outlines the implementation plan for the CycodBench benchmark runner, which will evaluate the `cycod` agent's performance on the SWE-bench dataset.

## Project Structure

The project will be structured as follows:
- Location: `src/cycod-bench` (main project)
- Tests: `tests/cycod-bench` (test project)
- Will create self-contained binaries similar to cycod, cycodt, and cycodmd

## Implementation Phases

### Phase 0: Project Setup and Infrastructure

1. **Create Solution Structure**
   - Create CycodBench project in `src/cycod-bench`
   - Create test project in `tests/cycod-bench`
   - Update cycod.sln to include the new projects
   - Set up project references and NuGet packages
   - Configure code analyzers consistent with the rest of chatx

2. **Setup Build and Packaging**
   - Integrate with the existing BuildCommon.targets
   - Set up RuntimeIdentifiers for cross-platform support
   - Configure NuGet packaging properties
   - Add selfie/ output directory setup for built binaries

### Phase 1: Foundation Layer

3. **Implement Models**
   - Create core data models (SwebenchProblem, CandidateSolution, etc.)
   - Implement JSON serialization/deserialization
   - Write unit tests for model serialization
   - **Quality Gate**: Model validation and serialization tests

4. **Implement Logger**
   - Implement ILogger interface
   - Reuse or adapt existing logging components from cycod if possible
   - Create logging extensions
   - Write unit tests for Logger
   - **Quality Gate**: Log output validation in various scenarios

5. **Implement Configuration**
   - Implement IConfiguration interface
   - Reuse configuration patterns from cycod
   - Create configuration loading/validation
   - Write unit tests for Configuration
   - **Quality Gate**: Configuration validation tests

### Phase 2: Infrastructure Layer

6. **Implement DockerManager**
   - Implement IDockerManager interface
   - Use ProcessHelpers from cycod.common for process execution
   - Write unit tests with mock Docker CLI responses
   - Write integration tests with real Docker if available
   - **Quality Gate**: Container lifecycle tests

7. **Implement DatasetManager**
   - Implement IDatasetManager interface
   - Create HTTP client for Hugging Face dataset access
   - Create dataset caching logic
   - Write unit tests with mock HTTP responses
   - Write integration tests with sample dataset
   - **Quality Gate**: Dataset loading and caching tests

8. **Implement EvaluationToolsManager**
   - Implement IEvaluationToolsManager interface
   - Create tools installation logic
   - Write unit tests with mock execution
   - Write integration tests with minimal tools
   - **Quality Gate**: Tools setup and execution tests

### Phase 3: Service Layer

9. **Implement AgentExecutor**
   - Implement IAgentExecutor interface
   - Create process execution logic for cycod
   - Implement diff extraction
   - Write unit tests with mock agent responses
   - Write integration tests with actual cycod
   - **Quality Gate**: Agent execution and output parsing tests

10. **Implement EvaluationService**
    - Implement IEvaluationService interface
    - Create evaluation logic
    - Write unit tests with mock evaluation
    - Write integration tests with simple evaluations
    - **Quality Gate**: Evaluation results parsing tests

11. **Implement EnsemblerService**
    - Implement IEnsemblerService interface
    - Create ensembling logic using cycod
    - Write unit tests with mock ensembling
    - Write integration tests with sample solutions
    - **Quality Gate**: Solution selection tests

### Phase 4: Orchestration Layer

12. **Implement ShardManager**
    - Implement IShardManager interface
    - Create sharding algorithms
    - Write unit tests for sharding logic
    - **Quality Gate**: Problem distribution tests

13. **Implement ProblemProcessor**
    - Implement IProblemProcessor interface
    - Create candidate solution handling
    - Write unit tests with mock dependencies
    - Write integration tests with simple problems
    - **Quality Gate**: Full problem processing tests

14. **Implement ResultManager**
    - Implement IResultManager interface
    - Create result storage/retrieval
    - Write unit tests for result handling
    - **Quality Gate**: Result file integrity tests

### Phase 5: Application Layer

15. **Implement BenchmarkRunner**
    - Implement IBenchmarkRunner interface
    - Create orchestration logic
    - Write unit tests with mock components
    - Write integration tests with minimal problems
    - **Quality Gate**: End-to-end workflow tests

16. **Implement Program and CLI**
    - Implement command-line interface using System.CommandLine
    - Setup dependency injection with Microsoft.Extensions.DependencyInjection
    - Create help documentation
    - Write integration tests for main commands
    - **Quality Gate**: Command execution tests

### Phase 6: Testing and Documentation

17. **End-to-End Testing**
    - Create full workflow tests
    - Test with realistic SWE-bench problems
    - Performance testing
    - **Quality Gate**: Full benchmark execution

18. **Documentation**
    - Create user documentation
    - Update developer documentation
    - Create usage examples
    - **Quality Gate**: Documentation review

## Key Implementation Details

### Process Execution

- Use ProcessHelpers from common library for executing cycod
- Command will be: `cycod --input "{problem_statement_file}" --folder {workspace_path}`
- Handle process timeouts and output capture

### Docker Integration

- Use Docker CLI commands via Process.Start
- Implement container lifecycle management
- Support volume mounting for workspace directories

### Result File Format

- Use JSONL for individual problem results (one problem per line)
- Use JSON for ensemble results and metrics
- Support streaming processing for efficient file handling

## Testing Strategy

### Unit Tests

- Use xUnit for test framework (consistent with cycod)
- Use Moq for mocking dependencies
- Test both success paths and failure modes
- Target >80% code coverage

### Integration Tests

- Test components working together
- Use Docker test containers for Docker-dependent tests
- Create filesystem test helpers for filesystem operations

### End-to-End Tests

- Test the full workflow with simplified benchmark problems
- Create mock SWE-bench problems for testing

## Quality Assurance Checklist for Each Phase

### Design Review
- Interface properly defined with XML documentation
- Dependency patterns follow best practices
- Error handling strategy consistent
- Logging appropriately implemented

### Implementation Checklist
- All methods implemented
- Error cases handled
- Resource cleanup in using statements or try/finally
- Performance considerations addressed

### Test Coverage
- Unit tests for public methods
- Edge cases covered
- Error paths tested
- Timeouts and cancellation tested

### Code Review
- Code follows style guidelines
- No hardcoded values (use configuration)
- Proper exception handling
- No resource leaks
- Thread-safety where required