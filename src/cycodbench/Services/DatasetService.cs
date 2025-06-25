using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CycodBench.Models;

namespace CycodBench.Services
{
    /// <summary>
    /// Implementation of the dataset service.
    /// </summary>
    public class DatasetService : IDatasetService
    {
        private readonly HttpClient _httpClient;
        
        private const string DATASET_BASE_URL = "https://storage.googleapis.com/swebench/datasets/";
        
        public DatasetService()
        {
            _httpClient = new HttpClient();
        }
        
        /// <inheritdoc />
        public async Task<string> DownloadDatasetAsync(string datasetName, string? outputPath = null, bool force = false)
        {
            // Determine file path
            var filePath = outputPath ?? $"{datasetName}.json";
            
            // Check if file already exists
            if (File.Exists(filePath) && !force)
            {
                Console.WriteLine($"Dataset file {filePath} already exists. Use --force to redownload.");
                return filePath;
            }
            
            // Ensure the dataset name is valid
            if (datasetName != "verified" && datasetName != "full" && datasetName != "lite")
            {
                throw new ArgumentException($"Invalid dataset name: {datasetName}. Must be 'verified', 'full', or 'lite'.");
            }
            
            // Download the dataset
            string url = $"{DATASET_BASE_URL}{datasetName}.json";
            Console.WriteLine($"Downloading dataset from {url}...");
            
            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsByteArrayAsync();
                await File.WriteAllBytesAsync(filePath, content);
                
                Console.WriteLine($"Downloaded dataset to {filePath}");
                return filePath;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Failed to download dataset: {ex.Message}", ex);
            }
        }
        
        /// <inheritdoc />
        public async Task<ProblemDataset> LoadDatasetAsync(string filePath)
        {
            try
            {
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
            
            // Remove duplicates
            mergedDataset.Problems = mergedDataset.Problems
                .GroupBy(p => p.Id)
                .Select(g => g.First())
                .ToList();
            
            return Task.FromResult(mergedDataset);
        }
        
        /// <inheritdoc />
        public Task<ProblemDataset[]> CreateShardsAsync(ProblemDataset dataset, int totalShards)
        {
            if (totalShards <= 0)
            {
                throw new ArgumentException("Total shards must be greater than 0", nameof(totalShards));
            }
            
            var result = new ProblemDataset[totalShards];
            
            for (int i = 0; i < totalShards; i++)
            {
                result[i] = new ProblemDataset();
            }
            
            var problems = dataset.Problems;
            for (int i = 0; i < problems.Count; i++)
            {
                var shardIndex = i % totalShards;
                result[shardIndex].Problems.Add(problems[i]);
            }
            
            return Task.FromResult(result);
        }
        
        /// <inheritdoc />
        public Task<ProblemDataset> GetShardAsync(ProblemDataset dataset, int shardIndex, int totalShards)
        {
            if (totalShards <= 0)
            {
                throw new ArgumentException("Total shards must be greater than 0", nameof(totalShards));
            }
            
            if (shardIndex < 0 || shardIndex >= totalShards)
            {
                throw new ArgumentException($"Shard index must be between 0 and {totalShards - 1}", nameof(shardIndex));
            }
            
            var result = new ProblemDataset();
            var problems = dataset.Problems;
            
            for (int i = 0; i < problems.Count; i++)
            {
                if (i % totalShards == shardIndex)
                {
                    result.Problems.Add(problems[i]);
                }
            }
            
            return Task.FromResult(result);
        }
    }
}