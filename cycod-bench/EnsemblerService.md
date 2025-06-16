# EnsemblerService.cs

This class is responsible for selecting the best solution from multiple candidate solutions using an ensemble approach.

## Responsibilities

- Process multiple candidate solutions for each problem
- Execute the `cycod` agent as an ensembler to select the best solution
- Construct appropriate ensembler prompts
- Parse and interpret ensembler responses
- Handle ensembler failures and timeouts
- Generate comprehensive ensemble reports

## Public Interface

```csharp
public interface IEnsemblerService
{
    Task<EnsembleResult> EnsembleSolutionsAsync(
        string inputPath,
        string outputPath,
        int maxParallelism = 8,
        CancellationToken cancellationToken = default);
    
    Task<ProblemEnsembleResult> EnsembleProblemSolutionsAsync(
        ProblemWithCandidates problem,
        CancellationToken cancellationToken = default);
    
    string BuildEnsemblerPrompt(
        ProblemWithCandidates problem);
    
    int ParseEnsembleSelection(
        string ensemblerOutput,
        int candidateCount);
    
    EnsembleMetrics CalculateMetrics(EnsembleResult result);
}
```

## Implementation

```csharp
public class EnsemblerService
{
    // Constructor
    public EnsemblerService(
        IProcessRunner processRunner,
        ILogger logger,
        EnsemblerConfiguration config);
    
    // Main ensemble method
    public async Task<EnsembleResult> EnsembleSolutionsAsync(
        string inputPath,
        string outputPath,
        int maxParallelism = 8,
        CancellationToken cancellationToken = default);
    
    // Process a single problem
    public async Task<ProblemEnsembleResult> EnsembleProblemSolutionsAsync(
        ProblemWithCandidates problem,
        CancellationToken cancellationToken = default);
    
    // Build the ensembler prompt
    public string BuildEnsemblerPrompt(
        ProblemWithCandidates problem);
    
    // Parse the ensemble selection
    public int ParseEnsembleSelection(
        string ensemblerOutput,
        int candidateCount);
    
    // Calculate ensemble success metrics
    public EnsembleMetrics CalculateMetrics(EnsembleResult result);
}
```

## Implementation Overview

The EnsemblerService class will:

1. **Load and process candidate solutions**:
   - Read the merged JSONL file containing all problem results
   - Group solutions by problem ID
   - Filter out invalid or incomplete solutions

2. **For each problem**:
   - Build an ensemble prompt with all candidate solutions
   - Execute the `cycod` agent as an ensembler
   - Parse the response to determine the selected solution
   - Create a problem-specific ensemble result

3. **Generate final ensemble results**:
   - Combine all problem ensemble results
   - Calculate overall success metrics
   - Save results to the specified output file

## Ensembler Prompt Construction

The EnsemblerService will build a prompt that:
- Includes the problem statement
- Presents each candidate solution (diff)
- Includes evaluation results for each candidate
- Asks the ensembler to analyze and select the best solution
- Provides clear criteria for selection

## Ensembler Execution

The service will execute the `cycod` agent with:
```
cycod --input "{ensembler_prompt_file}" --output "{ensembler_output_file}"
```

Where:
- `{ensembler_prompt_file}` is a file containing the ensemble prompt
- `{ensembler_output_file}` is a file where the ensembler will write its response

## Ensemble Result Analysis

The service will analyze the ensembler's response to:
- Extract the index of the selected solution
- Capture the ensembler's reasoning
- Validate that the selection is valid
- Handle cases where no clear selection is made

## Parallel Processing

The EnsemblerService will:
- Support parallel ensembling of multiple problems
- Control parallelism to avoid resource contention
- Track progress and provide status updates
- Handle failures in individual problem ensembling

## Error Handling

The EnsemblerService will:
- Handle timeouts in ensembler execution
- Implement fallback strategies when ensembling fails
- Provide detailed error information
- Support retries for transient failures

## Performance Considerations

The service will:
- Use batched processing to improve efficiency
- Cache intermediate results to avoid redundant computation
- Monitor resource usage during ensembling
- Optimize prompt construction for large problem sets

## Ensemble Metrics

The service will calculate metrics including:
- Overall success rate
- Comparison of ensemble success vs. best individual candidate
- Distribution of ensemble selections
- Consistency of ensemble decisions