# DatasetManager.cs

This class is responsible for loading, managing, and providing access to the SWE-bench dataset.

## Responsibilities

- Download and cache the SWE-bench dataset from Hugging Face
- Load and parse the dataset into appropriate data structures
- Provide access to problems and metadata
- Handle dataset versioning and updates
- Support filtering and querying the dataset
- Support incremental dataset loading for large datasets

## Public Interface

```csharp
public interface IDatasetManager
{
    Task<bool> InitializeAsync(
        bool forceDownload = false,
        CancellationToken cancellationToken = default);
    
    List<SwebenchProblem> GetAllProblems();
    
    SwebenchProblem GetProblemById(string id);
    
    List<SwebenchProblem> GetProblems(
        Func<SwebenchProblem, bool> predicate,
        int? limit = null);
    
    DatasetInfo GetDatasetInfo();
    
    bool IsDatasetLoaded();
    
    Task<bool> UpdateDatasetAsync(
        CancellationToken cancellationToken = default);
}
```

## Implementation

```csharp
public class DatasetManager
{
    // Constructor
    public DatasetManager(ILogger logger, DatasetConfiguration config);
    
    // Initialize and load the dataset
    public async Task<bool> InitializeAsync(
        bool forceDownload = false,
        CancellationToken cancellationToken = default);
    
    // Get all problems in the dataset
    public List<SwebenchProblem> GetAllProblems();
    
    // Get a specific problem by ID
    public SwebenchProblem GetProblemById(string id);
    
    // Get problems matching specified criteria
    public List<SwebenchProblem> GetProblems(
        Func<SwebenchProblem, bool> predicate,
        int? limit = null);
    
    // Get dataset information
    public DatasetInfo GetDatasetInfo();
    
    // Check if dataset is loaded
    public bool IsDatasetLoaded();
    
    // Update dataset to latest version
    public async Task<bool> UpdateDatasetAsync(
        CancellationToken cancellationToken = default);
}
```

## Implementation Overview

The DatasetManager class will:

1. **Download and cache the dataset**:
   - Use HTTP client to download dataset files from Hugging Face
   - Store the dataset in a local cache directory
   - Support incremental updates to avoid full downloads
   - Handle authentication for private datasets if needed

2. **Parse and load the dataset**:
   - Parse JSON, CSV, or other formats into structured objects
   - Convert raw data into SwebenchProblem instances
   - Validate dataset integrity during loading
   - Handle large datasets efficiently

3. **Provide dataset access**:
   - In-memory access to problem data
   - Efficient lookup by problem ID
   - Filtering by repository, difficulty, or other attributes
   - Support for custom queries

4. **Manage dataset versions**:
   - Track dataset version information
   - Support updating to newer versions
   - Maintain compatibility with multiple versions

## Dataset Structure

The DatasetManager will load the SWE-bench Verified dataset with the following structure:
- Problem ID
- Repository name
- Problem statement
- Test files
- Docker image information
- Metadata

## Dataset Loading Process

The loading process will be:
1. Check if a cached version exists locally
2. If not, or if `forceDownload` is true, download from Hugging Face
3. Parse the dataset into memory-efficient structures
4. Index the dataset for efficient querying
5. Validate the loaded data for integrity

## Integration with Hugging Face

The DatasetManager will access the SWE-bench_Verified dataset directly from Hugging Face using the REST API:

### Direct HTTP Download

The process will involve:

1. First, downloading the dataset info to identify available files:
   ```
   https://huggingface.co/api/datasets/princeton-nlp/SWE-bench_Verified
   ```

2. Then, downloading the actual dataset files:
   ```
   https://huggingface.co/datasets/princeton-nlp/SWE-bench_Verified/resolve/main/{filename}
   ```

3. For Arrow/Parquet formatted files (common in Hugging Face):
   ```
   https://huggingface.co/datasets/princeton-nlp/SWE-bench_Verified/resolve/main/data/{split}/{file_idx}.arrow
   ```

This approach requires:
- HTTP client implementation using HttpClient
- Arrow/Parquet file parsing using a .NET library like Apache.Arrow or Parquet.Net
- Custom download and caching logic
- JSON deserialization of metadata

### Implementation Strategy

Since the SWE-bench dataset might be in Arrow or Parquet format (standard for Hugging Face), we'll:

1. Use NuGet packages for file format support:
   - Apache.Arrow for Arrow files
   - Parquet.Net for Parquet files
   - System.Text.Json for JSON metadata

2. Implement a download and caching system that:
   - Saves files with versioning information
   - Validates downloaded files
   - Supports resumable downloads for large files

## Configuration Options

The DatasetManager will support configuration for:
- Local cache directory
- Download timeout and retry settings
- Authentication credentials (if needed)
- Cache expiration policy
- Memory usage limits

## Error Handling

The DatasetManager will:
- Handle network errors during download
- Validate downloaded files for integrity
- Provide clear error messages for dataset issues
- Implement retry logic for transient failures
- Support fallback to cached versions on failure

## Performance Considerations

The implementation will:
- Use efficient data structures for in-memory representation
- Support lazy loading for large datasets
- Use indexing for fast lookups
- Minimize memory footprint
- Handle large datasets without out-of-memory errors

## Integration with Benchmark Runner

The DatasetManager will be:
- Initialized during application startup
- Used by BenchmarkRunner to fetch problems for processing
- Integrated with the sharding system to distribute problems