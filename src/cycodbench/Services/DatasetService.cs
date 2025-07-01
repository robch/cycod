using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CycodBench.Models;
using Parquet;
using Parquet.Data;
using Parquet.Schema;

namespace CycodBench.Services
{
    /// <summary>
    /// Implementation of the dataset service.
    /// </summary>
    public class DatasetService : IDatasetService
    {
        private readonly HttpClient _httpClient;
        
        private readonly Dictionary<string, string> _datasetRepositories = new()
        {
            { "verified", "princeton-nlp/SWE-bench_Verified" },
            { "full", "princeton-nlp/SWE-bench" },
            { "lite", "princeton-nlp/SWE-bench_Lite" }
        };
        
        public DatasetService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "CycodBench/1.0");
        }
        
        /// <summary>
        /// Get the Hugging Face repository name for the dataset type
        /// </summary>
        private string GetDatasetRepositoryName(string datasetType)
        {
            if (!_datasetRepositories.TryGetValue(datasetType, out var repoName))
            {
                throw new ArgumentException($"Unknown dataset type: {datasetType}", nameof(datasetType));
            }
            
            return repoName;
        }
        
        /// <summary>
        /// Get the path where the dataset will be stored
        /// </summary>
        private string GetDatasetPath(string datasetType, string? outputPath)
        {
            if (outputPath != null)
            {
                return outputPath;
            }
            
            string basePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "CycodBench",
                "datasets"
            );
            
            return Path.Combine(basePath, $"{datasetType}.json");
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
                    
                    // Handle the case where the list might be serialized as a JSON string
                    if (item is string jsonStr && !string.IsNullOrEmpty(jsonStr))
                    {
                        try
                        {
                            return JsonSerializer.Deserialize<List<string>>(jsonStr) ?? new List<string>();
                        }
                        catch
                        {
                            // If parsing fails, return empty list
                            return new List<string>();
                        }
                    }
                }
            }
            return new List<string>();
        }
        
        /// <summary>
        /// Read a Parquet file and convert it to a ProblemDataset
        /// </summary>
        private async Task<ProblemDataset> ReadParquetFileAsync(string parquetFilePath)
        {
            Console.WriteLine($"Reading Parquet file from {parquetFilePath}");
            var problems = new List<Problem>();
            
            using var fileStream = new FileStream(parquetFilePath, FileMode.Open, FileAccess.Read);
            using var parquetReader = await ParquetReader.CreateAsync(fileStream);
            
            // Get schema information
            var schema = parquetReader.Schema;
            var schemaFields = schema.GetDataFields();
            
            // Find the fields we need in the schema
            var idField = FindField(schemaFields, "instance_id");
            var repoField = FindField(schemaFields, "repo");
            var baseCommitField = FindField(schemaFields, "base_commit");
            var problemStatementField = FindField(schemaFields, "problem_statement");
            var patchField = FindField(schemaFields, "patch");
            var hintsTextField = FindField(schemaFields, "hints_text");
            var failToPassField = FindField(schemaFields, "FAIL_TO_PASS");
            var passToPassField = FindField(schemaFields, "PASS_TO_PASS");
            
            // Process each row group
            for (int i = 0; i < parquetReader.RowGroupCount; i++)
            {
                using var rowGroupReader = parquetReader.OpenRowGroupReader(i);
                var rowCount = rowGroupReader.RowCount;
                
                // Read individual columns
                var idColumn = idField != null ? await rowGroupReader.ReadColumnAsync(idField) : null;
                var repoColumn = repoField != null ? await rowGroupReader.ReadColumnAsync(repoField) : null;
                var baseCommitColumn = baseCommitField != null ? await rowGroupReader.ReadColumnAsync(baseCommitField) : null;
                var problemStatementColumn = problemStatementField != null ? await rowGroupReader.ReadColumnAsync(problemStatementField) : null;
                var patchColumn = patchField != null ? await rowGroupReader.ReadColumnAsync(patchField) : null;
                var hintsTextColumn = hintsTextField != null ? await rowGroupReader.ReadColumnAsync(hintsTextField) : null;
                var failToPassColumn = failToPassField != null ? await rowGroupReader.ReadColumnAsync(failToPassField) : null;
                var passToPassColumn = passToPassField != null ? await rowGroupReader.ReadColumnAsync(passToPassField) : null;
                
                // Build the problems list
                for (int row = 0; row < rowCount; row++)
                {
                    var problem = new Problem
                    {
                        Id = idColumn?.Data != null ? GetStringValue(idColumn, row) : string.Empty,
                        Repo = repoColumn?.Data != null ? GetStringValue(repoColumn, row) : string.Empty,
                        BaseCommit = baseCommitColumn?.Data != null ? GetStringValue(baseCommitColumn, row) : string.Empty,
                        ProblemStatement = problemStatementColumn?.Data != null ? GetStringValue(problemStatementColumn, row) : string.Empty,
                        Patch = patchColumn?.Data != null ? GetStringValue(patchColumn, row) : string.Empty,
                        HintsText = hintsTextColumn?.Data != null ? GetStringValue(hintsTextColumn, row) : string.Empty
                    };
                    
                    // Handle list fields
                    if (failToPassColumn?.Data != null)
                    {
                        problem.FailToPass = GetStringList(failToPassColumn, row);
                    }
                    
                    if (passToPassColumn?.Data != null)
                    {
                        problem.PassToPass = GetStringList(passToPassColumn, row);
                    }
                    
                    problems.Add(problem);
                }
            }
            
            Console.WriteLine($"Loaded {problems.Count} problems from dataset");
            return new ProblemDataset { Problems = problems };
        }
        
        /// <inheritdoc />
        public async Task<string> DownloadDatasetAsync(string datasetName, string? outputPath = null, bool force = false)
        {
            // Determine the output JSON file path
            string jsonFilePath = GetDatasetPath(datasetName, outputPath);
            
            // Check if the JSON file already exists
            if (File.Exists(jsonFilePath) && !force)
            {
                Console.WriteLine($"Dataset {datasetName} already exists at {jsonFilePath}. Use --force to redownload.");
                return jsonFilePath;
            }
            
            try
            {
                Console.WriteLine($"Downloading dataset {datasetName} from Hugging Face...");
                
                // Create temporary directory for the parquet file
                string tempDir = Path.Combine(Path.GetTempPath(), "CycodBench", "temp");
                Directory.CreateDirectory(tempDir);
                
                // Temporary parquet file path
                string parquetFilePath = Path.Combine(tempDir, $"{datasetName}.parquet");
                
                // Get the repository name
                string repoName = GetDatasetRepositoryName(datasetName);
                
                // Construct the URL for the dataset parquet file
                string url = $"https://huggingface.co/datasets/{repoName}/resolve/main/data/test-00000-of-00001.parquet?download=true";
                
                // Download the parquet file
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                // Save the parquet file to the temporary location
                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(parquetFilePath, FileMode.Create))
                {
                    await stream.CopyToAsync(fileStream);
                }
                
                Console.WriteLine($"Downloaded Parquet file to {parquetFilePath}");
                
                // Read the parquet file and convert to ProblemDataset
                var dataset = await ReadParquetFileAsync(parquetFilePath);
                
                // Create output directory if needed
                Directory.CreateDirectory(Path.GetDirectoryName(jsonFilePath) ?? string.Empty);
                
                // Save as JSON
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                
                string json = JsonSerializer.Serialize(dataset, options);
                await File.WriteAllTextAsync(jsonFilePath, json);
                
                Console.WriteLine($"Converted Parquet data to JSON and saved to {jsonFilePath}");
                
                // Clean up the temporary parquet file
                try
                {
                    File.Delete(parquetFilePath);
                    Console.WriteLine("Cleaned up temporary Parquet file");
                }
                catch (Exception ex)
                {
                    // Just log the error but don't fail the operation
                    Console.WriteLine($"Warning: Failed to delete temporary file: {ex.Message}");
                }
                
                return jsonFilePath;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Failed to download dataset {datasetName}: {ex.Message}", ex);
            }
            catch (IOException ex)
            {
                throw new Exception($"Failed to save dataset {datasetName}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Unexpected error downloading dataset {datasetName}: {ex.Message}", ex);
            }
        }
        
        /// <inheritdoc />
        public async Task<ProblemDataset> LoadDatasetAsync(string nameOrFilePath)
        {
            try
            {
                bool isStandardDataset = nameOrFilePath == "verified" || nameOrFilePath == "full" || nameOrFilePath == "lite";
                var filePath = isStandardDataset 
                    ? await DownloadDatasetAsync(nameOrFilePath, null, false) 
                    : nameOrFilePath;

                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"Dataset file not found: {filePath}");
                }
                
                var json = await File.ReadAllTextAsync(filePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                var dataset = JsonSerializer.Deserialize<ProblemDataset>(json, options);
                return dataset ?? new ProblemDataset();
            }
            catch (JsonException ex)
            {
                throw new Exception($"Failed to parse dataset file: {ex.Message}", ex);
            }
        }
        
        /// <inheritdoc />
        public async Task<string> SaveDatasetAsync(ProblemDataset dataset, string filePath)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                
                var json = JsonSerializer.Serialize(dataset, options);
                await File.WriteAllTextAsync(filePath, json);
                
                Console.WriteLine($"Saved dataset to {filePath}");
                return filePath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save dataset: {ex.Message}", ex);
            }
        }
        
        /// <inheritdoc />
        public Task<ProblemDataset> FilterProblemsAsync(
            ProblemDataset dataset, 
            string? problemId = null, 
            string? repository = null, 
            string? containsText = null, 
            int? maxProblems = null)
        {
            var filteredProblems = dataset.Problems.AsEnumerable();
            
            if (!string.IsNullOrEmpty(problemId))
            {
                filteredProblems = filteredProblems.Where(p => p.Id.Contains(problemId, StringComparison.OrdinalIgnoreCase));
            }
            
            if (!string.IsNullOrEmpty(repository))
            {
                filteredProblems = filteredProblems.Where(p => p.Repo.Contains(repository, StringComparison.OrdinalIgnoreCase));
            }
            
            if (!string.IsNullOrEmpty(containsText))
            {
                filteredProblems = filteredProblems.Where(p => 
                    p.Id.Contains(containsText, StringComparison.OrdinalIgnoreCase) ||
                    p.ProblemStatement.Contains(containsText, StringComparison.OrdinalIgnoreCase) ||
                    p.Patch.Contains(containsText, StringComparison.OrdinalIgnoreCase) ||
                    p.FailToPass.Any(t => t.Contains(containsText, StringComparison.OrdinalIgnoreCase)) ||
                    p.PassToPass.Any(t => t.Contains(containsText, StringComparison.OrdinalIgnoreCase)));
            }
            
            if (maxProblems.HasValue && maxProblems > 0)
            {
                filteredProblems = filteredProblems.Take(maxProblems.Value);
            }
            
            return Task.FromResult(new ProblemDataset
            {
                Problems = filteredProblems.ToList()
            });
        }
        
        /// <inheritdoc />
        public Task<ProblemDataset> MergeDatasetsAsync(params ProblemDataset[] datasets)
        {
            var mergedDataset = new ProblemDataset();
            
            foreach (var dataset in datasets)
            {
                mergedDataset.Problems.AddRange(dataset.Problems);
            }
            
            // Remove duplicates based on both ID and problem_idx
            mergedDataset.Problems = mergedDataset.Problems
                .GroupBy(p => new { p.Id, p.Idx })
                .Select(g => g.First())
                .ToList();
            
            return Task.FromResult(mergedDataset);
        }
        
        /// <inheritdoc />
        public Task<ProblemDataset[]> CreateShardsAsync(ProblemDataset dataset, int totalShards, int candidatesPerProblem = 1)
        {
            if (totalShards <= 0)
            {
                throw new ArgumentException("Total shards must be greater than 0", nameof(totalShards));
            }
            
            if (candidatesPerProblem <= 0)
            {
                throw new ArgumentException("Candidates per problem must be greater than 0", nameof(candidatesPerProblem));
            }
            
            var result = new ProblemDataset[totalShards];
            
            for (int i = 0; i < totalShards; i++)
            {
                result[i] = new ProblemDataset();
            }
            
            // Create a working copy of the dataset
            var expandedProblems = new List<Problem>();
            
            // If candidatesPerProblem > 1, expand each problem into multiple instances with different indices
            if (candidatesPerProblem > 1)
            {
                Console.WriteLine($"Expanding problems to create {candidatesPerProblem} candidates per problem...");
                foreach (var problem in dataset.Problems)
                {
                    for (int i = 0; i < candidatesPerProblem; i++)
                    {
                        var problemCopy = new Problem
                        {
                            Id = problem.Id,
                            Idx = i,
                            Repo = problem.Repo,
                            BaseCommit = problem.BaseCommit,
                            ProblemStatement = problem.ProblemStatement,
                            HintsText = problem.HintsText,
                            Patch = problem.Patch,
                            FailToPass = new List<string>(problem.FailToPass),
                            PassToPass = new List<string>(problem.PassToPass)
                        };
                        
                        expandedProblems.Add(problemCopy);
                    }
                }
                Console.WriteLine($"Expanded {dataset.Problems.Count} problems into {expandedProblems.Count} total problems with unique problem_idx values");
            }
            else
            {
                // Just use the original problems if candidatesPerProblem is 1
                expandedProblems.AddRange(dataset.Problems);
            }
            
            // Group problems by ID to ensure all instances of the same problem stay together
            var problemGroups = expandedProblems.GroupBy(p => p.Id).ToList();
            
            // Track the size of each shard for load balancing
            var shardSizes = new int[totalShards];
            
            // Assign each group to the shard with the least load
            foreach (var group in problemGroups)
            {
                // Find the shard with the smallest current size
                int targetShardIndex = Array.IndexOf(shardSizes, shardSizes.Min());
                
                // Add all problems from this group to the target shard
                foreach (var problem in group)
                {
                    result[targetShardIndex].Problems.Add(problem);
                }
                
                // Update the size of the target shard
                shardSizes[targetShardIndex] += group.Count();
            }
            
            return Task.FromResult(result);
        }
        
        /// <inheritdoc />
        public Task<ProblemDataset> GetShardAsync(ProblemDataset dataset, int shardIndex, int totalShards, int candidatesPerProblem = 1)
        {
            if (totalShards <= 0)
            {
                throw new ArgumentException("Total shards must be greater than 0", nameof(totalShards));
            }
            
            if (shardIndex < 0 || shardIndex >= totalShards)
            {
                throw new ArgumentException($"Shard index must be between 0 and {totalShards - 1}", nameof(shardIndex));
            }
            
            // Create all shards first with the specified number of candidates per problem
            var allShards = CreateShardsAsync(dataset, totalShards, candidatesPerProblem).Result;
            
            // Return the specified shard
            return Task.FromResult(allShards[shardIndex]);
        }
    }
}