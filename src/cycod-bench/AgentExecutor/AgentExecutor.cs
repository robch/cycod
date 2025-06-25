using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using CycodBench.Configuration;
using CycodBench.DockerManager;
using CycodBench.Helpers;
using CycodBench.Logging;
using CycodBench.Models;

namespace CycodBench.AgentExecutor;

/// <summary>
/// Executes the cycod agent on SWE-bench problems and extracts results.
/// </summary>
public class AgentExecutor : IAgentExecutor
{
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly IDockerManager _dockerManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="AgentExecutor"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="dockerManager">The Docker manager.</param>
    public AgentExecutor(ILogger logger, IConfiguration configuration, IDockerManager dockerManager)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _dockerManager = dockerManager ?? throw new ArgumentNullException(nameof(dockerManager));
    }

    /// <inheritdoc />
    public async Task<CandidateSolution> ExecuteAgentAsync(
        SwebenchProblem problem,
        string workspacePath,
        int candidateIndex,
        string containerId,
        string? agentPath = null,
        int? timeoutSeconds = null,
        CancellationToken cancellationToken = default)
    {
        _logger.Info($"Executing agent for problem {problem.Id} (candidate {candidateIndex})");
        
        // Create a candidate solution to track results
        var candidateSolution = new CandidateSolution
        {
            ProblemId = problem.Id,
            CandidateIndex = candidateIndex,
            WorkspacePath = workspacePath,
            Timestamp = DateTimeOffset.UtcNow
        };
        
        try
        {
            // Resolve agent path and timeout
            agentPath = agentPath ?? _configuration.AgentPath;
            timeoutSeconds = timeoutSeconds ?? _configuration.AgentTimeoutSeconds;
            
            _logger.Debug($"Using agent at: {agentPath}");
            _logger.Debug($"Timeout set to: {timeoutSeconds} seconds");
            
            if (string.IsNullOrEmpty(agentPath))
            {
                throw new InvalidOperationException("Agent path not specified. Please set it in configuration or provide it as a parameter.");
            }
            
            // Prepare problem statement file
            var problemStatementFile = Path.Combine(workspacePath, "input", "problem_statement.txt");
            FileHelpers.WriteAllText(problemStatementFile, problem.ProblemStatement);
            _logger.Debug($"Problem statement ({problem.ProblemStatement.Length} chars) written to: {problemStatementFile}");
            
            // Set up execution command
            string outputPath = Path.Combine(workspacePath, "agent_output.txt");
            string logsPath = Path.Combine(workspacePath, "agent_logs.txt");
            
            // Start timer to measure execution time
            var stopwatch = Stopwatch.StartNew();
            
            // Execute the agent either directly or in a Docker container
            string agentOutput;
            if (_configuration.UseContainer)
            {
                _logger.Debug("Executing agent in Docker container");
                
                // Copy agent to container
                await _dockerManager.CopyFileToContainerAsync(
                    containerId,
                    agentPath,
                    "/workspace/bin/cycod",
                    cancellationToken);

                // Copy agent settings to container
                await _dockerManager.CopyFileToContainerAsync(
                    containerId,
                    ".cycod/.",
                    $"/testbed/.cycod",
                    cancellationToken);

                // Set execute permissions
                await _dockerManager.ExecuteCommandInContainerAsync(
                    containerId,
                    "chmod +x /workspace/bin/cycod",
                    timeoutMs: 5000,
                    cancellationToken);
                
                // Execute the agent in the container
                var result = await _dockerManager.ExecuteCommandInContainerAsync(
                    containerId,
                    "/workspace/bin/cycod " +
                        "--folder /testbed " +
                        "--add-system-prompt \"Ensure your work is on current commit. Don't consider using newer commits, nor look at them\" " +
                        "--input /workspace/input/problem_statement.txt " +
                        "--output-chat-history /workspace/output/chat-history.jsonl " +
                        "--output-trajectory /workspace/output/trajectory.md",
                    timeoutMs: timeoutSeconds.Value * 1000,
                    cancellationToken);
                
                agentOutput = result.Output;
                
                // Save output to file in the workspace
                FileHelpers.WriteAllText(outputPath, agentOutput);
            }
            else
            {
                _logger.Debug("Executing agent locally");
                
                // Create process start info
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = agentPath,
                    Arguments = $"--input \"{problemStatementFile}\" --folder \"{workspacePath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                
                // Execute the process
                var processResult = await CycodBench.Helpers.ProcessHelpers.ExecuteProcessAsync(
                    processStartInfo,
                    timeoutMs: timeoutSeconds.Value * 1000,
                    cancellationToken: cancellationToken);
                
                // Combine stdout and stderr
                var outputBuilder = new StringBuilder();
                outputBuilder.AppendLine(processResult.StdOut);
                
                if (!string.IsNullOrEmpty(processResult.StdErr))
                {
                    outputBuilder.AppendLine("STDERR:");
                    outputBuilder.AppendLine(processResult.StdErr);
                }
                
                agentOutput = outputBuilder.ToString();
                
                // Save output to file in the workspace
                FileHelpers.WriteAllText(outputPath, agentOutput);
            }
            
            // Stop timer
            stopwatch.Stop();
            candidateSolution.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
            
            _logger.Debug($"Agent execution completed in {stopwatch.ElapsedMilliseconds}ms");
            
            // Generate diff using Git CLI based on the base commit
            candidateSolution.Diff = await GenerateDiffFromGitAsync(workspacePath, problem.BaseCommit, containerId);
            
            // Save the generated diff to a file
            if (!string.IsNullOrEmpty(candidateSolution.Diff))
            {
                FileHelpers.WriteAllText(Path.Combine(workspacePath, "solution.diff"), candidateSolution.Diff);
                _logger.Debug($"Generated diff of {candidateSolution.Diff.Length} characters");
            }
            else
            {
                _logger.Warning("No diff was generated from Git");
            }
            
            // Save logs
            candidateSolution.AgentLogs = agentOutput;
            FileHelpers.WriteAllText(logsPath, agentOutput);
            
            // Get agent version
            candidateSolution.AgentVersion = await GetAgentVersionAsync(agentPath);
            
            return candidateSolution;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Error executing agent: {ex.Message}");
            
            // Return the candidate solution with error information
            candidateSolution.AgentLogs = $"ERROR: {ex.Message}\n{ex.StackTrace}";
            return candidateSolution;
        }
    }

    /// <inheritdoc />
    public string ExtractDiffFromOutput(string agentOutput)
    {
        // This method is kept for backward compatibility
        // It's no longer used to extract diffs from agent output
        // as we now generate diffs using Git CLI
        _logger.Debug("ExtractDiffFromOutput is deprecated. Using Git to generate diffs instead.");
        return string.Empty;
    }
    
    /// <summary>
    /// Generates a diff using Git CLI by comparing the current state with the specified base commit.
    /// </summary>
    /// <param name="workspacePath">The path to the workspace directory (testbed).</param>
    /// <param name="baseCommit">The base commit hash to diff against.</param>
    /// <param name="containerId">The Docker container ID where the agent is executed, or null if running locally.</param>
    /// <returns>The generated diff as a string.</returns>
    private async Task<string> GenerateDiffFromGitAsync(string workspacePath, string baseCommit, string? containerId)
    {
        if (string.IsNullOrEmpty(baseCommit))
        {
            _logger.Warning("Base commit is empty, cannot generate diff");
            return string.Empty;
        }
        
        try
        {
            _logger.Debug($"Generating diff against base commit: {baseCommit}");
            
            string diff;
            
            if (!string.IsNullOrEmpty(containerId))
            {
                _logger.Debug($"Running Git commands inside container: {containerId}");
                
                // Generate diff between the current state and base commit inside the container
                var diffResult = await _dockerManager.ExecuteCommandInContainerAsync(
                    containerId, 
                    $"bash -c \"cd /testbed && git diff {baseCommit}\"", 
                    timeoutMs: 30000);
                    
                if (!string.IsNullOrEmpty(diffResult.Error))
                {
                    _logger.Warning($"Git diff warning/error in container: {diffResult.Error}");
                }
                
                diff = diffResult.Output;
            }
            else
            {
                _logger.Debug("Running Git commands locally");
                
                // Generate diff between the current state and base commit locally
                var diffResult = await CycodBench.Helpers.ProcessHelpers.ExecuteProcessAsync(
                    new ProcessStartInfo
                    {
                        FileName = "git",
                        Arguments = $"diff {baseCommit}",
                        WorkingDirectory = workspacePath,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    },
                    timeoutMs: 30000);
                    
                if (!string.IsNullOrEmpty(diffResult.StdErr))
                {
                    _logger.Warning($"Git diff warning/error locally: {diffResult.StdErr}");
                }
                
                diff = diffResult.StdOut;
            }
            
            if (string.IsNullOrWhiteSpace(diff))
            {
                _logger.Warning("Git diff produced no output");
                return string.Empty;
            }
            
            _logger.Debug($"Successfully generated diff of {diff.Length} characters");
            return diff;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Error generating diff using Git: {ex.Message}");
            return string.Empty;
        }
    }

    /// <inheritdoc />
    public async Task<string> GetAgentVersionAsync(string? agentPath = null)
    {
        try
        {
            // Resolve agent path
            agentPath = agentPath ?? _configuration.AgentPath;
            
            if (string.IsNullOrEmpty(agentPath))
            {
                return "unknown";
            }
            
            // Create process start info
            var processStartInfo = new ProcessStartInfo
            {
                FileName = agentPath,
                Arguments = "--version",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            // Execute the process
            var processResult = await CycodBench.Helpers.ProcessHelpers.ExecuteProcessAsync(
                processStartInfo,
                timeoutMs: 5000);
            
            // Extract version from output
            string output = processResult.StdOut.Trim();
            
            // If the output contains a version number, return it
            var versionMatch = Regex.Match(output, @"(\d+\.\d+\.\d+)");
            if (versionMatch.Success)
            {
                return versionMatch.Groups[1].Value;
            }
            
            // Otherwise, return the whole output if it's not empty
            return !string.IsNullOrEmpty(output) ? output : "unknown";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Error getting agent version: {ex.Message}");
            return "unknown";
        }
    }
}