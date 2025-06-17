using System.Net.Http;
using CycodBench.Configuration;
using CycodBench.Logging;
using CycodBench.Models;
using Newtonsoft.Json;
using Parquet;
using Parquet.Data;
using Parquet.Schema;

namespace CycodBench.DatasetManager;

/// <summary>
/// Implementation of the SWE-bench dataset manager.
/// </summary>
public class DatasetManager : IDatasetManager
{
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly Dictionary<string, string> _datasetRepositories = new()
    {
        { "verified", "princeton-nlp/SWE-bench_Verified" },
        { "full", "princeton-nlp/SWE-bench" },
        { "lite", "princeton-nlp/SWE-bench_Lite" }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="DatasetManager"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="configuration">The configuration instance.</param>
    public DatasetManager(ILogger logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "CycodBench/1.0");
    }
    
    /// <inheritdoc/>
    public string GetDatasetRepositoryName(string datasetType)
    {
        if (!_datasetRepositories.TryGetValue(datasetType, out var repoName))
        {
            throw new ArgumentException($"Unknown dataset type: {datasetType}", nameof(datasetType));
        }
        
        return repoName;
    }
    
    /// <inheritdoc/>
    public string GetDatasetPath(string datasetType = "verified")
    {
        string basePath = _configuration.GetString("dataset_path", Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CycodBench",
            "datasets"
        ));
        
        return Path.Combine(basePath, datasetType);
    }
    
    /// <inheritdoc/>
    public bool IsDatasetCached(string datasetType = "verified")
    {
        string datasetPath = GetDatasetPath(datasetType);
        string datasetFile = Path.Combine(datasetPath, "dataset.parquet");
        return File.Exists(datasetFile);
    }

    /// <inheritdoc/>
    public async Task<bool> DownloadDatasetAsync(string datasetType = "verified", bool forceDownload = false)
    {
        string datasetPath = GetDatasetPath(datasetType);
        string datasetFile = Path.Combine(datasetPath, "dataset.parquet");
        
        if (IsDatasetCached(datasetType) && !forceDownload)
        {
            _logger.Info($"Dataset {datasetType} already exists at {datasetPath}");
            return true;
        }
        
        try
        {
            _logger.Info($"Downloading dataset {datasetType} from Hugging Face...");
            
            // Create the directory if it doesn't exist
            Directory.CreateDirectory(datasetPath);
            
            // Get the repository name
            string repoName = GetDatasetRepositoryName(datasetType);
            
            // Construct the URL for the dataset parquet file
            string url = $"https://huggingface.co/datasets/{repoName}/resolve/main/data/test-00000-of-00001.parquet?download=true";
            
            // Download the file
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            
            // Save the file
            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var fileStream = new FileStream(datasetFile, FileMode.Create))
            {
                await stream.CopyToAsync(fileStream);
            }
            
            _logger.Info($"Successfully downloaded dataset {datasetType} to {datasetPath}");
            return true;
        }
        catch (HttpRequestException ex)
        {
            _logger.Error(ex, $"Failed to download dataset {datasetType}: {ex.Message}");
            return false;
        }
        catch (IOException ex)
        {
            _logger.Error(ex, $"Failed to save dataset {datasetType}: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Unexpected error downloading dataset {datasetType}: {ex.Message}");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<List<SwebenchProblem>> GetProblemsAsync(string datasetType = "verified")
    {
        if (!IsDatasetCached(datasetType))
        {
            bool downloadSuccess = await DownloadDatasetAsync(datasetType);
            if (!downloadSuccess)
            {
                throw new InvalidOperationException($"Failed to download dataset {datasetType}");
            }
        }
        
        string datasetFile = Path.Combine(GetDatasetPath(datasetType), "dataset.parquet");
        var problems = new List<SwebenchProblem>();
        
        try
        {
            _logger.Info($"Reading Parquet file from {datasetFile}");
            
            using var fileStream = new FileStream(datasetFile, FileMode.Open, FileAccess.Read);
            using var parquetReader = await ParquetReader.CreateAsync(fileStream);
            
            // Get schema information
            var schema = parquetReader.Schema;
            
            // Find the fields we need in the schema
            var schemaFields = schema.GetDataFields();
            var idField = FindField(schemaFields, "id");
            var repoField = FindField(schemaFields, "repo");
            var baseCommitField = FindField(schemaFields, "base_commit");
            var headCommitField = FindField(schemaFields, "head_commit");
            var problemStatementField = FindField(schemaFields, "problem_statement");
            var issueUrlField = FindField(schemaFields, "issue_url");
            var prUrlField = FindField(schemaFields, "pr_url");
            var additionalContextField = FindField(schemaFields, "additional_context");
            var dockerImageField = FindField(schemaFields, "docker_image");
            var testCommandField = FindField(schemaFields, "test_command");
            var testFilesField = FindField(schemaFields, "test_files");
            
            // Process each row group
            for (int i = 0; i < parquetReader.RowGroupCount; i++)
            {
                using var rowGroupReader = parquetReader.OpenRowGroupReader(i);
                var rowCount = rowGroupReader.RowCount;
                
                // Read individual columns
                var idColumn = idField != null ? await rowGroupReader.ReadColumnAsync(idField) : null;
                var repoColumn = repoField != null ? await rowGroupReader.ReadColumnAsync(repoField) : null;
                var baseCommitColumn = baseCommitField != null ? await rowGroupReader.ReadColumnAsync(baseCommitField) : null;
                var headCommitColumn = headCommitField != null ? await rowGroupReader.ReadColumnAsync(headCommitField) : null;
                var problemStatementColumn = problemStatementField != null ? await rowGroupReader.ReadColumnAsync(problemStatementField) : null;
                var issueUrlColumn = issueUrlField != null ? await rowGroupReader.ReadColumnAsync(issueUrlField) : null;
                var prUrlColumn = prUrlField != null ? await rowGroupReader.ReadColumnAsync(prUrlField) : null;
                var additionalContextColumn = additionalContextField != null ? await rowGroupReader.ReadColumnAsync(additionalContextField) : null;
                var dockerImageColumn = dockerImageField != null ? await rowGroupReader.ReadColumnAsync(dockerImageField) : null;
                var testCommandColumn = testCommandField != null ? await rowGroupReader.ReadColumnAsync(testCommandField) : null;
                var testFilesColumn = testFilesField != null ? await rowGroupReader.ReadColumnAsync(testFilesField) : null;
                
                // Build the problems list
                for (int row = 0; row < rowCount; row++)
                {
                    var problem = new SwebenchProblem
                    {
                        Id = idColumn?.Data != null ? GetStringValue(idColumn, row) : string.Empty,
                        Repository = repoColumn?.Data != null ? GetStringValue(repoColumn, row) : string.Empty,
                        BaseCommit = baseCommitColumn?.Data != null ? GetStringValue(baseCommitColumn, row) : string.Empty,
                        HeadCommit = headCommitColumn?.Data != null ? GetStringValue(headCommitColumn, row) : string.Empty,
                        ProblemStatement = problemStatementColumn?.Data != null ? GetStringValue(problemStatementColumn, row) : string.Empty,
                        IssueUrl = issueUrlColumn?.Data != null ? GetStringValue(issueUrlColumn, row) : string.Empty,
                        PullRequestUrl = prUrlColumn?.Data != null ? GetStringValue(prUrlColumn, row) : string.Empty,
                        AdditionalContext = additionalContextColumn?.Data != null ? GetStringValue(additionalContextColumn, row) : string.Empty,
                        DockerImage = dockerImageColumn?.Data != null ? GetStringValue(dockerImageColumn, row) : string.Empty,
                        TestCommand = testCommandColumn?.Data != null ? GetStringValue(testCommandColumn, row) : string.Empty,
                        TestFiles = testFilesColumn?.Data != null ? GetStringList(testFilesColumn, row) : new List<string>()
                    };
                    
                    problems.Add(problem);
                }
            }
            
            _logger.Info($"Loaded {problems.Count} problems from {datasetType} dataset");
            return problems;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Failed to load problems from {datasetType} dataset: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// Helper method to find a field in the schema by name
    /// </summary>
    private DataField? FindField(DataField[] fields, string name)
    {
        return fields.FirstOrDefault(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// Helper method to get a string value from a data column
    /// </summary>
    private string GetStringValue(DataColumn column, int rowIndex)
    {
        if (column.Data is string[] stringArray && rowIndex < stringArray.Length)
        {
            return stringArray[rowIndex] ?? string.Empty;
        }
        return string.Empty;
    }
    
    /// <summary>
    /// Helper method to get a string list from a data column
    /// </summary>
    private List<string> GetStringList(DataColumn column, int rowIndex)
    {
        if (column.Data is Array arrayData && arrayData.GetType().GetElementType() != null)
        {
            if (rowIndex < arrayData.Length)
            {
                var item = arrayData.GetValue(rowIndex);
                if (item is IEnumerable<string> stringList)
                {
                    return stringList.ToList();
                }
            }
        }
        return new List<string>();
    }

    /// <inheritdoc/>
    public async Task<SwebenchProblem?> GetProblemByIdAsync(string problemId, string datasetType = "verified")
    {
        // Get all problems and find the one with the given ID
        var problems = await GetProblemsAsync(datasetType);
        return problems.FirstOrDefault(p => p.Id == problemId);
    }
}