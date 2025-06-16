# ResultManager.cs

This class is responsible for managing the storage, retrieval, and manipulation of benchmark results.

## Responsibilities

- Store individual problem results in a structured format
- Merge results from multiple shards
- Load and save result files
- Generate reports and statistics
- Handle result file versioning
- Validate result data integrity
- Format results for different use cases (analysis, visualization)

## Public Interface

```csharp
public interface IResultManager
{
    Task SaveShardResultsAsync(
        List<ProblemResult> results,
        string filePath,
        CancellationToken cancellationToken = default);
    
    Task<List<ProblemResult>> LoadShardResultsAsync(
        string filePath,
        CancellationToken cancellationToken = default);
    
    Task<int> MergeShardFilesAsync(
        IEnumerable<string> inputPaths,
        string outputPath,
        bool skipDuplicates = true,
        CancellationToken cancellationToken = default);
    
    Task SaveEnsembleResultsAsync(
        EnsembleResult results,
        string filePath,
        CancellationToken cancellationToken = default);
    
    Task<EnsembleResult> LoadEnsembleResultsAsync(
        string filePath,
        CancellationToken cancellationToken = default);
    
    ResultStatistics CalculateStatistics(List<ProblemResult> results);
    
    string GenerateReport(
        List<ProblemResult> results,
        ReportFormat format = ReportFormat.Text);
}
```

## Implementation

```csharp
public class ResultManager
{
    // Constructor
    public ResultManager(ILogger logger, ResultConfiguration config);
    
    // Save problem results to a shard file
    public async Task SaveShardResultsAsync(
        List<ProblemResult> results,
        string filePath,
        CancellationToken cancellationToken = default);
    
    // Load problem results from a shard file
    public async Task<List<ProblemResult>> LoadShardResultsAsync(
        string filePath,
        CancellationToken cancellationToken = default);
    
    // Merge multiple shard files into a single file
    public async Task<int> MergeShardFilesAsync(
        IEnumerable<string> inputPaths,
        string outputPath,
        bool skipDuplicates = true,
        CancellationToken cancellationToken = default);
    
    // Save ensemble results
    public async Task SaveEnsembleResultsAsync(
        EnsembleResult results,
        string filePath,
        CancellationToken cancellationToken = default);
    
    // Load ensemble results
    public async Task<EnsembleResult> LoadEnsembleResultsAsync(
        string filePath,
        CancellationToken cancellationToken = default);
    
    // Generate statistics and metrics
    public ResultStatistics CalculateStatistics(List<ProblemResult> results);
    
    // Generate a human-readable report
    public string GenerateReport(
        List<ProblemResult> results,
        ReportFormat format = ReportFormat.Text);
}
```

## Implementation Overview

The ResultManager class will:

1. **Manage result file formats**:
   - Define and maintain structured formats for storing results
   - Support JSONL format for shard results (one problem per line)
   - Support JSON format for ensemble results
   - Ensure backward compatibility with format changes

2. **Save and load results**:
   - Implement efficient file I/O for large result files
   - Use streaming JSON processing for better memory efficiency
   - Support compression for large result files
   - Validate data during save/load operations

3. **Merge shard results**:
   - Combine results from multiple shards
   - Handle duplicate problems appropriately
   - Preserve all candidate solutions
   - Maintain metadata across merges

4. **Calculate statistics and metrics**:
   - Success rate across all problems
   - Distribution of results by repository, problem type, etc.
   - Performance metrics (execution time, resource usage)
   - Comparison to baseline or previous runs

5. **Generate reports**:
   - Create human-readable reports in different formats
   - Support for text, Markdown, HTML, and CSV formats
   - Include summary statistics and detailed problem results
   - Generate visualizations (charts, tables) where appropriate

## Result File Formats

### Shard Result Format (JSONL)
Each line represents a single problem with:
- Problem metadata (id, repository, description)
- Candidate solutions with their diffs
- Evaluation results for each candidate
- Execution metrics and timing information

### Ensemble Result Format (JSON)
A structured JSON document with:
- Overall benchmark statistics
- Problem-specific results with selected solutions
- Ensemble decision information
- Aggregated metrics

## Data Validation

The ResultManager will validate:
- Required fields are present
- Data types are correct
- Relationships between entities are maintained
- File integrity (checksums if appropriate)

## Performance Considerations

The ResultManager will:
- Use streaming for large file operations
- Implement efficient serialization/deserialization
- Support parallel processing for file operations where appropriate
- Use appropriate memory management for large result sets

## Advanced Features

The ResultManager can support:
- Result comparison between different runs
- Differential analysis to identify improvements/regressions
- Exporting results to external analysis tools
- Visualization data preparation