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
            var problemStatementFile = Path.Combine(workspacePath, "problem_statement.txt");
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
                    "/tmp/cycod",
                    cancellationToken);

                // Copy agent settings to container
                await _dockerManager.CopyFileToContainerAsync(
                    containerId,
                    ".cycod/.",
                    $"/testbed/.cycod",
                    cancellationToken);

                // Copy problem statement file to container
                await _dockerManager.CopyFileToContainerAsync(
                    containerId,
                    problemStatementFile,
                    "/testbed/problem_statement.txt",
                    cancellationToken);
                
                // Set execute permissions
                await _dockerManager.ExecuteCommandInContainerAsync(
                    containerId,
                    "chmod +x /tmp/cycod",
                    timeoutMs: 5000,
                    cancellationToken);
                
                // Execute the agent in the container
                var result = await _dockerManager.ExecuteCommandInContainerAsync(
                    containerId,
                    $"/tmp/cycod --input /testbed/problem_statement.txt --folder /testbed",
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
            
            // Extract diff from the agent output
            candidateSolution.Diff = ExtractDiffFromOutput(agentOutput);
            
            // If we extracted a diff, save it to a separate file
            if (!string.IsNullOrEmpty(candidateSolution.Diff))
            {
                FileHelpers.WriteAllText(Path.Combine(workspacePath, "solution.diff"), candidateSolution.Diff);
                _logger.Debug($"Extracted diff of {candidateSolution.Diff.Length} characters");
            }
            else
            {
                _logger.Warning("No diff was extracted from agent output");
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
        if (string.IsNullOrEmpty(agentOutput))
        {
            return string.Empty;
        }
        
        try
        {
            // Look for diff markers in the output
            // This regex pattern looks for the typical diff format starting with "diff --git"
            var diffPattern = @"(?:^|\n)diff --git.*?(?=\n(?:diff --git|$))";
            var diffMatches = Regex.Matches(agentOutput, diffPattern, RegexOptions.Singleline);
            
            if (diffMatches.Count > 0)
            {
                // Collect all diff blocks
                var diffBuilder = new StringBuilder();
                foreach (Match match in diffMatches)
                {
                    diffBuilder.AppendLine(match.Value);
                }
                
                return diffBuilder.ToString().TrimEnd();
            }
            
            // If no standard diff format, look for blocks marked with triple backticks
            var codeBlockPattern = @"```diff\n(.*?)```";
            var codeBlockMatches = Regex.Matches(agentOutput, codeBlockPattern, RegexOptions.Singleline);
            
            if (codeBlockMatches.Count > 0)
            {
                // Collect all diff blocks from markdown code blocks
                var diffBuilder = new StringBuilder();
                foreach (Match match in codeBlockMatches)
                {
                    diffBuilder.AppendLine(match.Groups[1].Value);
                }
                
                return diffBuilder.ToString().TrimEnd();
            }
            
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Error extracting diff from agent output: {ex.Message}");
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