using System.Diagnostics;
using System.Text;
using CycodBench.Configuration;
using CycodBench.DockerManager;
using CycodBench.Logging;
using CycodBench.Models;

namespace CycodBench.EvaluationService;

/// <summary>
/// Service for evaluating candidate solutions against test cases.
/// </summary>
public class EvaluationService : IEvaluationService
{
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly IDockerManager _dockerManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="EvaluationService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="dockerManager">The Docker manager.</param>
    public EvaluationService(ILogger logger, IConfiguration configuration, IDockerManager dockerManager)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _dockerManager = dockerManager ?? throw new ArgumentNullException(nameof(dockerManager));
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
            
            // Run the build command
            _logger.Debug("Running build command...");
            (evaluationResult.BuildExitCode, evaluationResult.BuildOutput) = await RunBuildCommandAsync(
                problem,
                candidateSolution.WorkspacePath,
                containerId,
                _configuration.BuildTimeoutSeconds,
                cancellationToken);
            
            if (evaluationResult.BuildExitCode != 0)
            {
                _logger.Warning($"Build command failed with exit code {evaluationResult.BuildExitCode}");
                evaluationResult.ErrorMessage = $"Build command failed with exit code {evaluationResult.BuildExitCode}";
                evaluationResult.Passed = false;
            }
            else
            {
                _logger.Debug("Build completed successfully");
                
                // Run the test command
                _logger.Debug("Running test command...");
                (evaluationResult.TestExitCode, evaluationResult.TestOutput) = await RunTestCommandAsync(
                    problem,
                    candidateSolution.WorkspacePath,
                    containerId,
                    _configuration.TestTimeoutSeconds,
                    cancellationToken);
                
                // The solution passes if the test exit code is 0
                evaluationResult.Passed = evaluationResult.TestExitCode == 0;
                
                if (evaluationResult.Passed)
                {
                    _logger.Info("Solution passed the test!");
                }
                else
                {
                    _logger.Warning($"Test command failed with exit code {evaluationResult.TestExitCode}");
                    evaluationResult.ErrorMessage = $"Test command failed with exit code {evaluationResult.TestExitCode}";
                }
            }
            
            // Stop timer
            stopwatch.Stop();
            evaluationResult.EvaluationTimeMs = stopwatch.ElapsedMilliseconds;
            
            _logger.Debug($"Evaluation completed in {stopwatch.ElapsedMilliseconds}ms");
            
            // Save evaluation results to a file
            string evaluationResultsPath = Path.Combine(candidateSolution.WorkspacePath, "evaluation_results.json");
            FileHelpers.WriteAllText(evaluationResultsPath, Newtonsoft.Json.JsonConvert.SerializeObject(evaluationResult, Newtonsoft.Json.Formatting.Indented));
            
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
                    $"cd {workspacePath} && git apply --ignore-whitespace --reject {diffPath} || true",
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
    public async Task<(int ExitCode, string Output)> RunBuildCommandAsync(
        SwebenchProblem problem,
        string workspacePath,
        string containerId,
        int? timeoutSeconds = null,
        CancellationToken cancellationToken = default)
    {
        // Use default build command if not specified by the problem
        string buildCommand = _configuration.DefaultBuildCommand;
        int timeout = timeoutSeconds ?? _configuration.BuildTimeoutSeconds;
        
        _logger.Debug($"Running build command: {buildCommand}");
        
        if (_configuration.UseContainer)
        {
            // Execute in Docker container
            var result = await _dockerManager.ExecuteCommandInContainerAsync(
                containerId,
                $"cd {workspacePath} && {buildCommand}",
                timeoutMs: timeout * 1000,
                cancellationToken);
            
            return (result.ExitCode, result.Output);
        }
        else
        {
            // Execute locally
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{buildCommand}\"",
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
                return (1, "Failed to start build process");
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
            
            // Wait for process to complete with timeout
            bool completed = await Task.WhenAny(
                process.WaitForExitAsync(cancellationToken),
                Task.Delay(timeout * 1000, cancellationToken)
            ) == Task.CompletedTask;
            
            if (!completed)
            {
                process.Kill();
                return (1, $"Build command timed out after {timeout} seconds");
            }
            
            return (process.ExitCode, output.ToString());
        }
    }

    /// <inheritdoc />
    public async Task<(int ExitCode, string Output)> RunTestCommandAsync(
        SwebenchProblem problem,
        string workspacePath,
        string containerId,
        int? timeoutSeconds = null,
        CancellationToken cancellationToken = default)
    {
        // Use the test command specified by the problem, or fall back to default
        string testCommand = !string.IsNullOrEmpty(problem.TestCommand) 
            ? problem.TestCommand 
            : _configuration.DefaultTestCommand;
        
        int timeout = timeoutSeconds ?? _configuration.TestTimeoutSeconds;
        
        _logger.Debug($"Running test command: {testCommand}");
        
        if (_configuration.UseContainer)
        {
            // Execute in Docker container
            var result = await _dockerManager.ExecuteCommandInContainerAsync(
                containerId,
                $"cd {workspacePath} && {testCommand}",
                timeoutMs: timeout * 1000,
                cancellationToken);
            
            return (result.ExitCode, result.Output);
        }
        else
        {
            // Execute locally
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{testCommand}\"",
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
                return (1, "Failed to start test process");
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
            
            // Wait for process to complete with timeout
            bool completed = await Task.WhenAny(
                process.WaitForExitAsync(cancellationToken),
                Task.Delay(timeout * 1000, cancellationToken)
            ) == Task.CompletedTask;
            
            if (!completed)
            {
                process.Kill();
                return (1, $"Test command timed out after {timeout} seconds");
            }
            
            return (process.ExitCode, output.ToString());
        }
    }
}