using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CycodBench.Models;

namespace CycodBench.Services
{
    /// <summary>
    /// Implementation of the result service.
    /// </summary>
    public class ResultService : IResultService
    {
        /// <inheritdoc />
        public async Task<ResultCollection> LoadResultsAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"Results file not found: {filePath}");
                }
                
                var json = await File.ReadAllTextAsync(filePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                var results = JsonSerializer.Deserialize<ResultCollection>(json, options);
                return results ?? new ResultCollection();
            }
            catch (JsonException ex)
            {
                throw new Exception($"Failed to parse results file: {ex.Message}", ex);
            }
        }
        
        /// <inheritdoc />
        public async Task<string> SaveResultsAsync(ResultCollection results, string filePath)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                
                var json = JsonSerializer.Serialize(results, options);
                await File.WriteAllTextAsync(filePath, json);
                
                Console.WriteLine($"Saved {results.Results.Count} results to {filePath}");
                return filePath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save results: {ex.Message}", ex);
            }
        }
        
        /// <inheritdoc />
        public Task<ResultCollection> FilterResultsAsync(
            ResultCollection results, 
            string? problemId = null, 
            string? repository = null, 
            string? containsText = null, 
            string? status = null)
        {
            var filteredResults = results.Results.AsEnumerable();
            
            if (!string.IsNullOrEmpty(problemId))
            {
                filteredResults = filteredResults.Where(r => r.ProblemId.Contains(problemId, StringComparison.OrdinalIgnoreCase));
            }
            
            if (!string.IsNullOrEmpty(repository))
            {
                // Extract repository from problem ID (format is typically owner__repo-issue-id)
                filteredResults = filteredResults.Where(r => 
                {
                    var parts = r.ProblemId.Split('_');
                    if (parts.Length >= 2)
                    {
                        var repo = parts[0] + "/" + parts[1].Split('-')[0];
                        return repo.Contains(repository, StringComparison.OrdinalIgnoreCase);
                    }
                    return false;
                });
            }
            
            if (!string.IsNullOrEmpty(containsText))
            {
                filteredResults = filteredResults.Where(r => 
                    r.ProblemId.Contains(containsText, StringComparison.OrdinalIgnoreCase) ||
                    r.AgentPatch.Contains(containsText, StringComparison.OrdinalIgnoreCase) ||
                    r.AgentOutput.Contains(containsText, StringComparison.OrdinalIgnoreCase) ||
                    r.EvalOutput.Contains(containsText, StringComparison.OrdinalIgnoreCase));
            }
            
            if (!string.IsNullOrEmpty(status))
            {
                filteredResults = filteredResults.Where(r => r.EvalStatus.Equals(status, StringComparison.OrdinalIgnoreCase));
            }
            
            return Task.FromResult(new ResultCollection
            {
                Results = filteredResults.ToList()
            });
        }
        
        /// <inheritdoc />
        public Task<ResultCollection> MergeResultsAsync(params ResultCollection[] resultCollections)
        {
            var mergedResults = new ResultCollection();
            
            foreach (var collection in resultCollections)
            {
                mergedResults.Results.AddRange(collection.Results);
            }
            
            return Task.FromResult(mergedResults);
        }
        
        /// <inheritdoc />
        public Task<string> GenerateReportAsync(ResultCollection results, bool verbose = false)
        {
            var report = new StringBuilder();
            
            // Add report header
            report.AppendLine("# CycodBench Evaluation Report");
            report.AppendLine();
            report.AppendLine($"Generated on: {DateTime.Now}");
            report.AppendLine();
            
            // Add summary statistics
            int totalCount = results.Results.Count;
            int passedCount = results.Results.Count(r => r.EvalStatus.Equals("passed", StringComparison.OrdinalIgnoreCase));
            int failedCount = results.Results.Count(r => r.EvalStatus.Equals("failed", StringComparison.OrdinalIgnoreCase));
            int skippedCount = results.Results.Count(r => r.EvalStatus.Equals("skipped", StringComparison.OrdinalIgnoreCase));
            
            double passRate = totalCount > 0 ? (double)passedCount / totalCount * 100 : 0;
            
            report.AppendLine("## Summary");
            report.AppendLine();
            report.AppendLine($"- Total problems: {totalCount}");
            report.AppendLine($"- Passed: {passedCount} ({passRate:F1}%)");
            report.AppendLine($"- Failed: {failedCount}");
            report.AppendLine($"- Skipped: {skippedCount}");
            report.AppendLine();
            
            // Add results by repository
            report.AppendLine("## Results by Repository");
            report.AppendLine();
            
            var resultsByRepo = results.Results
                .GroupBy(r => {
                    var parts = r.ProblemId.Split('_');
                    if (parts.Length >= 2)
                    {
                        var repoParts = parts[1].Split('-');
                        if (repoParts.Length >= 1)
                        {
                            return $"{parts[0]}/{repoParts[0]}";
                        }
                    }
                    return "unknown";
                })
                .OrderBy(g => g.Key);
            
            report.AppendLine("| Repository | Total | Passed | Failed | Skipped | Pass Rate |");
            report.AppendLine("|------------|-------|--------|--------|---------|----------|");
            
            foreach (var repoGroup in resultsByRepo)
            {
                int repoTotal = repoGroup.Count();
                int repoPassed = repoGroup.Count(r => r.EvalStatus.Equals("passed", StringComparison.OrdinalIgnoreCase));
                int repoFailed = repoGroup.Count(r => r.EvalStatus.Equals("failed", StringComparison.OrdinalIgnoreCase));
                int repoSkipped = repoGroup.Count(r => r.EvalStatus.Equals("skipped", StringComparison.OrdinalIgnoreCase));
                double repoPassRate = repoTotal > 0 ? (double)repoPassed / repoTotal * 100 : 0;
                
                report.AppendLine($"| {repoGroup.Key} | {repoTotal} | {repoPassed} | {repoFailed} | {repoSkipped} | {repoPassRate:F1}% |");
            }
            
            report.AppendLine();
            
            // Add detailed results
            report.AppendLine("## Detailed Results");
            report.AppendLine();
            
            report.AppendLine("| Problem ID | Status | Details |");
            report.AppendLine("|------------|--------|---------|");
            
            foreach (var result in results.Results.OrderBy(r => r.ProblemId))
            {
                string details = verbose ? 
                    $"[Details](#problem-{result.ProblemId.Replace("__", "-").Replace("/", "-").Replace(".", "-").ToLowerInvariant()})" :
                    "";
                
                report.AppendLine($"| {result.ProblemId} | {result.EvalStatus} | {details} |");
            }
            
            // Add verbose details if requested
            if (verbose)
            {
                report.AppendLine();
                report.AppendLine("## Problem Details");
                report.AppendLine();
                
                foreach (var result in results.Results.OrderBy(r => r.ProblemId))
                {
                    string anchor = $"problem-{result.ProblemId.Replace("__", "-").Replace("/", "-").Replace(".", "-").ToLowerInvariant()}";
                    
                    report.AppendLine($"<a name=\"{anchor}\"></a>");
                    report.AppendLine($"### Problem: {result.ProblemId}");
                    report.AppendLine();
                    report.AppendLine($"**Status**: {result.EvalStatus}");
                    report.AppendLine();
                    
                    report.AppendLine("#### Agent Patch");
                    report.AppendLine("```diff");
                    report.AppendLine(result.AgentPatch);
                    report.AppendLine("```");
                    report.AppendLine();
                    
                    report.AppendLine("#### Evaluation Output");
                    report.AppendLine("```");
                    report.AppendLine(result.EvalOutput.Length > 2000 ? result.EvalOutput.Substring(0, 2000) + "..." : result.EvalOutput);
                    report.AppendLine("```");
                    report.AppendLine();
                }
            }
            
            return Task.FromResult(report.ToString());
        }
    }
}