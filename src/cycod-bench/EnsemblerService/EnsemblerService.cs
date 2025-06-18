using CycodBench.AgentExecutor;
using CycodBench.Configuration;
using CycodBench.Helpers;
using CycodBench.Logging;
using CycodBench.Models;
using System.Diagnostics;
using System.Text;

namespace CycodBench.EnsemblerService;

/// <summary>
/// Service for ensembling multiple candidate solutions to select the best one.
/// </summary>
public class EnsemblerService : IEnsemblerService
{
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly IAgentExecutor _agentExecutor;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnsemblerService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="agentExecutor">The agent executor.</param>
    public EnsemblerService(ILogger logger, IConfiguration configuration, IAgentExecutor agentExecutor)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _agentExecutor = agentExecutor ?? throw new ArgumentNullException(nameof(agentExecutor));
    }

    /// <inheritdoc />
    public async Task<EnsembleResult> EnsembleSolutionsAsync(
        SwebenchProblem problem,
        IList<CandidateSolution> candidateSolutions,
        CancellationToken cancellationToken = default)
    {
        _logger.Info($"Ensembling {candidateSolutions.Count} solutions for problem {problem.Id}");
        
        // Create the ensemble result to track the outcome
        var ensembleResult = new EnsembleResult
        {
            ProblemId = problem.Id,
            CandidateSolutionIds = candidateSolutions.Select(s => s.Id).ToList(),
            Timestamp = DateTimeOffset.UtcNow
        };
        
        try
        {
            // If no solutions provided, return empty result
            if (candidateSolutions.Count == 0)
            {
                _logger.Warning("No candidate solutions provided for ensembling");
                ensembleResult.SelectionReason = "No candidate solutions provided";
                return ensembleResult;
            }
            
            // If only one solution, select it directly
            if (candidateSolutions.Count == 1)
            {
                var solution = candidateSolutions[0];
                ensembleResult.SelectedSolutionId = solution.Id;
                ensembleResult.SelectedSolution = solution;
                ensembleResult.SelectionReason = "Only one candidate solution available";
                
                _logger.Info($"Selected the only available solution (index {solution.CandidateIndex})");
                return ensembleResult;
            }
            
            // Find any passing solutions
            var passingSolutions = candidateSolutions
                .Where(s => s.EvaluationResult?.Passed == true)
                .ToList();
            
            // If we have passing solutions, prioritize them
            if (passingSolutions.Count > 0)
            {
                _logger.Info($"Found {passingSolutions.Count} passing solutions");
                
                // If we have multiple passing solutions, select the one with the simplest diff
                if (passingSolutions.Count > 1)
                {
                    // Use the solution with the smallest diff size as a simple heuristic for simplicity
                    var selectedSolution = passingSolutions
                        .OrderBy(s => DiffComplexityScore(s.Diff))
                        .ThenBy(s => s.ExecutionTimeMs) // As a tiebreaker, prefer faster solutions
                        .First();
                    
                    ensembleResult.SelectedSolutionId = selectedSolution.Id;
                    ensembleResult.SelectedSolution = selectedSolution;
                    ensembleResult.SelectionReason = 
                        $"Selected the simplest passing solution (index {selectedSolution.CandidateIndex}) " +
                        $"with complexity score {DiffComplexityScore(selectedSolution.Diff)}";
                    
                    _logger.Info($"Selected the simplest passing solution (index {selectedSolution.CandidateIndex})");
                    return ensembleResult;
                }
                else
                {
                    // Only one passing solution
                    var selectedSolution = passingSolutions[0];
                    ensembleResult.SelectedSolutionId = selectedSolution.Id;
                    ensembleResult.SelectedSolution = selectedSolution;
                    ensembleResult.SelectionReason = $"Selected the only passing solution (index {selectedSolution.CandidateIndex})";
                    
                    _logger.Info($"Selected the only passing solution (index {selectedSolution.CandidateIndex})");
                    return ensembleResult;
                }
            }
            
            // No passing solutions, check for solutions that at least build correctly
            var buildingSuccessSolutions = candidateSolutions
                .Where(s => s.EvaluationResult?.BuildExitCode == 0)
                .ToList();
            
            if (buildingSuccessSolutions.Count > 0)
            {
                _logger.Info($"No passing solutions, but found {buildingSuccessSolutions.Count} solutions that build correctly");
                
                // Use the solution with the smallest diff size as a simple heuristic for simplicity
                var selectedSolution = buildingSuccessSolutions
                    .OrderBy(s => DiffComplexityScore(s.Diff))
                    .ThenBy(s => s.ExecutionTimeMs) // As a tiebreaker, prefer faster solutions
                    .First();
                
                ensembleResult.SelectedSolutionId = selectedSolution.Id;
                ensembleResult.SelectedSolution = selectedSolution;
                ensembleResult.SelectionReason = 
                    $"Selected the simplest building solution (index {selectedSolution.CandidateIndex}) " +
                    $"with complexity score {DiffComplexityScore(selectedSolution.Diff)}";
                
                _logger.Info($"Selected the simplest building solution (index {selectedSolution.CandidateIndex})");
                return ensembleResult;
            }
            
            // If ensembling using cycod is enabled, use the agent to select the best solution
            if (_configuration.UseAgentEnsembling)
            {
                _logger.Info("Using cycod agent to select best solution");
                var agentSelectedSolution = await SelectBestSolutionUsingAgentAsync(
                    problem,
                    candidateSolutions,
                    cancellationToken);
                
                if (agentSelectedSolution != null)
                {
                    ensembleResult.SelectedSolutionId = agentSelectedSolution.Id;
                    ensembleResult.SelectedSolution = agentSelectedSolution;
                    ensembleResult.SelectionReason = 
                        $"Selected by cycod agent (index {agentSelectedSolution.CandidateIndex})";
                    
                    _logger.Info($"Agent selected solution with index {agentSelectedSolution.CandidateIndex}");
                    return ensembleResult;
                }
            }
            
            // Fall back to selecting the solution with the most complete diff
            // (i.e., the one that appears to have done the most work)
            var fallbackSolution = candidateSolutions
                .OrderByDescending(s => s.Diff?.Length ?? 0)
                .ThenBy(s => s.ExecutionTimeMs) // As a tiebreaker, prefer faster solutions
                .First();
            
            ensembleResult.SelectedSolutionId = fallbackSolution.Id;
            ensembleResult.SelectedSolution = fallbackSolution;
            ensembleResult.SelectionReason = 
                $"Fallback selection: chose the most comprehensive solution (index {fallbackSolution.CandidateIndex}) " +
                $"with diff size {fallbackSolution.Diff?.Length ?? 0} characters";
            
            _logger.Info($"Fallback to most comprehensive solution (index {fallbackSolution.CandidateIndex})");
            return ensembleResult;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Error during ensembling: {ex.Message}");
            
            // Fall back to the first solution if an error occurs
            if (candidateSolutions.Count > 0)
            {
                var fallbackSolution = candidateSolutions[0];
                ensembleResult.SelectedSolutionId = fallbackSolution.Id;
                ensembleResult.SelectedSolution = fallbackSolution;
                ensembleResult.SelectionReason = $"Error occurred during ensembling, defaulted to first solution (index {fallbackSolution.CandidateIndex})";
            }
            else
            {
                ensembleResult.SelectionReason = "Error occurred during ensembling, no solutions available";
            }
            
            return ensembleResult;
        }
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, EnsembleResult>> EnsembleMultipleProblemsAsync(
        Dictionary<string, (SwebenchProblem Problem, IList<CandidateSolution> Solutions)> problemSolutions,
        CancellationToken cancellationToken = default)
    {
        _logger.Info($"Ensembling solutions for {problemSolutions.Count} problems");
        
        var results = new Dictionary<string, EnsembleResult>();
        
        foreach (var (problemId, (problem, solutions)) in problemSolutions)
        {
            _logger.Info($"Processing problem {problem.Id} with {solutions.Count} solutions");
            
            try
            {
                var ensembleResult = await EnsembleSolutionsAsync(problem, solutions, cancellationToken);
                results[problemId] = ensembleResult;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error ensembling solutions for problem {problem.Id}: {ex.Message}");
                
                // Create an error result
                var errorResult = new EnsembleResult
                {
                    ProblemId = problemId,
                    CandidateSolutionIds = solutions.Select(s => s.Id).ToList(),
                    SelectionReason = $"Error: {ex.Message}"
                };
                
                results[problemId] = errorResult;
            }
        }
        
        return results;
    }
    
    /// <summary>
    /// Calculates a complexity score for a diff based on its size and content.
    /// Lower scores indicate simpler diffs.
    /// </summary>
    /// <param name="diff">The diff to score.</param>
    /// <returns>A complexity score where lower is simpler.</returns>
    private int DiffComplexityScore(string? diff)
    {
        if (string.IsNullOrEmpty(diff))
        {
            return int.MaxValue; // Empty diffs are not useful
        }
        
        // Simple scoring based on:
        // 1. Overall size
        // 2. Number of files modified
        // 3. Number of chunks
        // 4. Number of added/removed lines
        
        var lines = diff.Split('\n');
        int filesModified = 0;
        int chunks = 0;
        int addedLines = 0;
        int removedLines = 0;
        
        foreach (var line in lines)
        {
            if (line.StartsWith("--- a/") || line.StartsWith("+++ b/"))
            {
                filesModified++;
            }
            else if (line.StartsWith("@@"))
            {
                chunks++;
            }
            else if (line.StartsWith("+") && !line.StartsWith("+++"))
            {
                addedLines++;
            }
            else if (line.StartsWith("-") && !line.StartsWith("---"))
            {
                removedLines++;
            }
        }
        
        // Normalize filesModified to be half of total (we count each file twice: --- and +++)
        filesModified = filesModified / 2;
        
        // The complexity score is a weighted sum
        return 
            diff.Length +              // Overall size
            (filesModified * 100) +     // Each file modified adds significant complexity
            (chunks * 20) +            // Each chunk adds moderate complexity
            (addedLines * 5) +         // Added lines add some complexity
            (removedLines * 3);        // Removed lines add less complexity
    }
    
    /// <summary>
    /// Uses the cycod agent to select the best solution from multiple candidates.
    /// </summary>
    /// <param name="problem">The SWE-bench problem.</param>
    /// <param name="candidateSolutions">The list of candidate solutions.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The best solution selected by the agent, or null if selection fails.</returns>
    private async Task<CandidateSolution?> SelectBestSolutionUsingAgentAsync(
        SwebenchProblem problem,
        IList<CandidateSolution> candidateSolutions,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Create a temporary workspace for the ensemble operation
            string workspaceDir = Path.Combine(
                _configuration.WorkspaceRoot,
                $"ensemble_{problem.Id}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");
            
            Directory.CreateDirectory(workspaceDir);
            _logger.Debug($"Created ensemble workspace: {workspaceDir}");
            
            // Create a prompt for the agent
            var promptBuilder = new StringBuilder();
            promptBuilder.AppendLine("# Ensemble Task");
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("## Problem Statement");
            promptBuilder.AppendLine();
            promptBuilder.AppendLine(problem.ProblemStatement);
            promptBuilder.AppendLine();
            
            promptBuilder.AppendLine("## Candidate Solutions");
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("You must select the best solution from the following candidates. " +
                                     "Consider factors like correctness, simplicity, and maintainability.");
            promptBuilder.AppendLine();
            
            // Add details about each candidate solution
            for (int i = 0; i < candidateSolutions.Count; i++)
            {
                var solution = candidateSolutions[i];
                
                promptBuilder.AppendLine($"### Candidate {i + 1}");
                promptBuilder.AppendLine();
                
                // Add evaluation results if available
                if (solution.EvaluationResult != null)
                {
                    promptBuilder.AppendLine($"Build Status: {(solution.EvaluationResult.BuildExitCode == 0 ? "Success" : "Failed")}");
                    promptBuilder.AppendLine($"Test Status: {(solution.EvaluationResult.Passed ? "Passed" : "Failed")}");
                    
                    if (!string.IsNullOrEmpty(solution.EvaluationResult.ErrorMessage))
                    {
                        promptBuilder.AppendLine($"Error: {solution.EvaluationResult.ErrorMessage}");
                    }
                }
                else
                {
                    promptBuilder.AppendLine("Evaluation Result: Not available");
                }
                
                // Add the diff
                promptBuilder.AppendLine();
                promptBuilder.AppendLine("```diff");
                promptBuilder.AppendLine(solution.Diff.Trim());
                promptBuilder.AppendLine("```");
                promptBuilder.AppendLine();
            }
            
            promptBuilder.AppendLine("## Task");
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("Analyze each solution and select the best one. " +
                                     "Return your answer as: \"Best Solution: [number]\" followed by a brief explanation.");
            
            // Save the prompt to a file
            string promptPath = Path.Combine(workspaceDir, "ensemble_prompt.md");
            FileHelpers.WriteAllText(promptPath, promptBuilder.ToString());
            
            // Execute the agent
            _logger.Info("Executing cycod agent to select best solution");
            
            var agentPath = _configuration.AgentPath;
            if (string.IsNullOrEmpty(agentPath))
            {
                _logger.Error("Agent path not specified in configuration");
                return null;
            }
            
            var processStartInfo = new ProcessStartInfo
            {
                FileName = agentPath,
                Arguments = $"--input \"{promptPath}\" --singleAgent",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            // Execute the process
            var processResult = await CycodBench.Helpers.ProcessHelpers.ExecuteProcessAsync(
                processStartInfo,
                timeoutMs: _configuration.AgentTimeoutSeconds * 1000,
                cancellationToken: cancellationToken);
            
            string output = processResult.StdOut;
            
            // Parse the agent's response to find the selected solution
            var match = System.Text.RegularExpressions.Regex.Match(output, @"Best Solution:\s*(\d+)");
            if (match.Success)
            {
                int selectedIndex = int.Parse(match.Groups[1].Value);
                
                // Adjust for 1-based indexing in the prompt
                selectedIndex--;
                
                if (selectedIndex >= 0 && selectedIndex < candidateSolutions.Count)
                {
                    _logger.Info($"Agent selected solution {selectedIndex + 1}");
                    return candidateSolutions[selectedIndex];
                }
                else
                {
                    _logger.Warning($"Agent selected invalid solution index: {selectedIndex + 1}");
                }
            }
            else
            {
                _logger.Warning("Agent did not provide a clear solution selection");
                _logger.Debug($"Agent output: {output}");
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Error using agent for ensemble selection: {ex.Message}");
            return null;
        }
    }
}