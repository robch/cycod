using System.Diagnostics;
using System.Text;
using CycodBench.Configuration;
using CycodBench.DockerManager;
using CycodBench.EvaluationToolsManager;
using CycodBench.Logging;
using CycodBench.Models;
using Newtonsoft.Json;

namespace CycodBench.EvaluationService;

/// <summary>
/// Service for evaluating candidate solutions using SWEBench evaluation tools.
/// </summary>
public class EvaluationService : IEvaluationService
{
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly IDockerManager _dockerManager;
    private readonly IEvaluationToolsManager _evaluationToolsManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="EvaluationService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="dockerManager">The Docker manager.</param>
    /// <param name="evaluationToolsManager">The evaluation tools manager.</param>
    public EvaluationService(
        ILogger logger, 
        IConfiguration configuration, 
        IDockerManager dockerManager,
        IEvaluationToolsManager evaluationToolsManager)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _dockerManager = dockerManager ?? throw new ArgumentNullException(nameof(dockerManager));
        _evaluationToolsManager = evaluationToolsManager ?? throw new ArgumentNullException(nameof(evaluationToolsManager));
    }

    /// <inheritdoc />
    public async Task<EvaluationResult> EvaluateSolutionAsync(
        SwebenchProblem problem,
        CandidateSolution candidateSolution,
        string containerId,
        CancellationToken cancellationToken = default)
    {
        _logger.Info($"Evaluating solution for problem {problem.Id} (candidate {candidateSolution.CandidateIndex})");
        
        // Create evaluation result to track results
        var evaluationResult = new EvaluationResult
        {
            ProblemId = problem.Id,
            SolutionId = candidateSolution.Id,
            Timestamp = DateTimeOffset.UtcNow
        };
        
        try
        {
            // Start timer to measure evaluation time
            var stopwatch = Stopwatch.StartNew();
            
            // Apply the diff to the codebase
            _logger.Debug("Applying diff to codebase...");
            evaluationResult.Applied = await ApplyDiffAsync(candidateSolution.Diff, candidateSolution.WorkspacePath, containerId, cancellationToken);
            
            if (!evaluationResult.Applied)
            {
                _logger.Warning("Failed to apply diff to codebase");
                evaluationResult.ErrorMessage = "Failed to apply diff to codebase";
                return evaluationResult;
            }
            
            _logger.Debug("Diff applied successfully");

            // Make sure SWEBench evaluation tools are set up
            if (!await _evaluationToolsManager.AreToolsSetupAsync())
            {
                _logger.Info("Setting up SWEBench evaluation tools...");
                bool setupSuccess = await _evaluationToolsManager.SetupToolsAsync();
                if (!setupSuccess)
                {
                    _logger.Error("Failed to set up SWEBench evaluation tools");
                    evaluationResult.ErrorMessage = "Failed to set up SWEBench evaluation tools";
                    evaluationResult.Passed = false;
                    return evaluationResult;
                }
            }
            
            // Create predictions file for SWEBench evaluator
            string outputDirectory = Path.Combine(candidateSolution.WorkspacePath, "evaluation");
            string predictionsFile = await CreatePredictionsFileAsync(problem, candidateSolution, outputDirectory);
            
            // Run SWEBench evaluation
            _logger.Debug("Running SWEBench evaluation...");
            var swebenchResult = await _evaluationToolsManager.EvaluateAsync(problem, candidateSolution, outputDirectory);
            
            // Update evaluation result from SWEBench evaluation
            evaluationResult.Passed = swebenchResult.Passed;
            evaluationResult.BuildExitCode = swebenchResult.BuildExitCode;
            evaluationResult.TestExitCode = 0; // Not used with SWEBench evaluation
            evaluationResult.BuildOutput = swebenchResult.BuildOutput;
            evaluationResult.TestOutput = swebenchResult.TestOutput;
            evaluationResult.ErrorMessage = swebenchResult.ErrorMessage;
            
            // Stop timer
            stopwatch.Stop();
            evaluationResult.EvaluationTimeMs = stopwatch.ElapsedMilliseconds;
            
            _logger.Debug($"Evaluation completed in {stopwatch.ElapsedMilliseconds}ms");
            
            // Save evaluation results to a file
            string evaluationResultsPath = Path.Combine(candidateSolution.WorkspacePath, "evaluation_results.json");
            FileHelpers.WriteAllText(evaluationResultsPath, JsonConvert.SerializeObject(evaluationResult, Formatting.Indented));
            
            return evaluationResult;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Error evaluating solution: {ex.Message}");
            
            // Return the evaluation result with error information
            evaluationResult.ErrorMessage = $"Error evaluating solution: {ex.Message}";
            evaluationResult.Passed = false;
            return evaluationResult;
        }
    }

    /// <inheritdoc />
    public async Task<bool> ApplyDiffAsync(
        string diff,
        string workspacePath,
        string containerId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(diff))
        {
            _logger.Warning("Empty diff provided, nothing to apply");
            return false;
        }
        
        try
        {
            // Write diff to a temporary file
            string diffPath = Path.Combine(workspacePath, "solution.diff");
            FileHelpers.WriteAllText(diffPath, diff);
            
            // Apply the diff using git apply
            _logger.Debug($"Applying diff from file: {diffPath}");
            
            if (_configuration.UseContainer)
            {
                // Execute in Docker container
                var result = await _dockerManager.ExecuteCommandInContainerAsync(
                    containerId,
                    $"bash -c \"cd {workspacePath} && git apply --ignore-whitespace --reject {diffPath} || true\"",
                    timeoutMs: 60000,
                    cancellationToken);
                
                // Check for fail indicators in the output
                bool success = !result.Output.Contains("patch failed:")
                               && !result.Output.Contains("error: patch failed:");
                
                if (!success)
                {
                    _logger.Warning($"Diff application had issues: {result.Output}");
                }
                
                return success;
            }
            else
            {
                // Execute locally
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"apply --ignore-whitespace --reject {diffPath}",
                    WorkingDirectory = workspacePath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                
                // Execute the process
                var process = Process.Start(processStartInfo);
                if (process == null)
                {
                    return false;
                }
                
                // Read output
                var output = new StringBuilder();
                string? line;
                while ((line = await process.StandardOutput.ReadLineAsync()) != null)
                {
                    output.AppendLine(line);
                }
                
                while ((line = await process.StandardError.ReadLineAsync()) != null)
                {
                    output.AppendLine(line);
                }
                
                await process.WaitForExitAsync(cancellationToken);
                
                // Check for fail indicators in the output
                string outputStr = output.ToString();
                bool success = !outputStr.Contains("patch failed:")
                               && !outputStr.Contains("error: patch failed:");
                
                if (!success)
                {
                    _logger.Warning($"Diff application had issues: {outputStr}");
                }
                
                return success;
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Error applying diff: {ex.Message}");
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<string> CreatePredictionsFileAsync(
        SwebenchProblem problem,
        CandidateSolution candidateSolution,
        string outputDirectory)
    {
        // Create the output directory if it doesn't exist
        Directory.CreateDirectory(outputDirectory);
        
        string predictionsFilePath = Path.Combine(outputDirectory, "predictions.json");
        
        // Create the predictions object in SWEBench format
        var prediction = new
        {
            instance_id = problem.Id,
            commit_hash = problem.BaseCommit,
            repo_path = problem.Repository,
            diff = candidateSolution.Diff
        };
        
        // Write the prediction to a file
        var predictions = new[] { prediction };
        string json = JsonConvert.SerializeObject(predictions, Formatting.Indented);
        FileHelpers.WriteAllText(predictionsFilePath, json);
        
        _logger.Debug($"Created predictions file at {predictionsFilePath}");
        return predictionsFilePath;
    }
}