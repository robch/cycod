using System.Diagnostics;
using CycodBench.AgentExecutor;
using CycodBench.Configuration;
using CycodBench.DockerManager;
using CycodBench.EvaluationService;
using CycodBench.Logging;
using CycodBench.Models;

namespace CycodBench.ProblemProcessor;

/// <summary>
/// Implementation of the IProblemProcessor interface for processing SWE-bench problems.
/// </summary>
public class ProblemProcessor : IProblemProcessor
{
    private readonly IAgentExecutor _agentExecutor;
    private readonly IDockerManager _dockerManager;
    private readonly IEvaluationService _evaluationService;
    private readonly IConfiguration _config;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProblemProcessor"/> class.
    /// </summary>
    /// <param name="agentExecutor">The agent executor.</param>
    /// <param name="dockerManager">The Docker manager.</param>
    /// <param name="evaluationService">The evaluation service.</param>
    /// <param name="config">The configuration.</param>
    /// <param name="logger">The logger.</param>
    public ProblemProcessor(
        IAgentExecutor agentExecutor,
        IDockerManager dockerManager,
        IEvaluationService evaluationService,
        IConfiguration config,
        ILogger logger)
    {
        _agentExecutor = agentExecutor ?? throw new ArgumentNullException(nameof(agentExecutor));
        _dockerManager = dockerManager ?? throw new ArgumentNullException(nameof(dockerManager));
        _evaluationService = evaluationService ?? throw new ArgumentNullException(nameof(evaluationService));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CandidateSolution>> ProcessProblemAsync(
        SwebenchProblem problem, 
        int candidateCount, 
        CancellationToken cancellationToken = default)
    {
        if (problem == null)
        {
            throw new ArgumentNullException(nameof(problem));
        }

        if (candidateCount <= 0)
        {
            throw new ArgumentException("Candidate count must be greater than 0", nameof(candidateCount));
        }

        var solutions = new List<CandidateSolution>();
        _logger.Info($"Processing problem {problem.Id} ({problem.Repository}) with {candidateCount} candidates");

        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Process each candidate solution
            for (int candidateIndex = 0; candidateIndex < candidateCount; candidateIndex++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.Warning("Processing cancelled");
                    break;
                }

                _logger.Info($"Processing candidate {candidateIndex + 1}/{candidateCount} for problem {problem.Id}");
                var candidateStopwatch = Stopwatch.StartNew();

                try
                {
                    // Set up workspace
                    var workspacePath = await SetupWorkspaceAsync(problem, candidateIndex, cancellationToken);
                    
                    // Execute agent
                    var solution = await ExecuteAgentAsync(problem, workspacePath, candidateIndex, cancellationToken);
                    
                    // Evaluate solution if it contains a diff
                    if (!string.IsNullOrWhiteSpace(solution.Diff))
                    {
                        solution = await EvaluateCandidateSolutionAsync(problem, solution, cancellationToken);
                    }
                    else
                    {
                        _logger.Warning($"Empty solution diff for candidate {candidateIndex}");
                        solution.EvaluationResult = new EvaluationResult
                        {
                            ErrorMessage = "Empty solution diff"
                        };
                    }
                    
                    solutions.Add(solution);
                    
                    // Cleanup
                    bool keepWorkspace = _config.KeepWorkspaces;
                    
                    await CleanupWorkspaceAsync(workspacePath, keepWorkspace);
                    
                    _logger.Info($"Candidate {candidateIndex + 1}/{candidateCount} completed in {candidateStopwatch.ElapsedMilliseconds}ms");
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to process candidate {candidateIndex + 1}: {ex.Message}");
                    
                    // Add a failed solution to maintain the candidate count
                    solutions.Add(new CandidateSolution
                    {
                        ProblemId = problem.Id,
                        CandidateIndex = candidateIndex,
                        AgentLogs = $"Error: {ex.Message}\n{ex.StackTrace}",
                        Timestamp = DateTimeOffset.UtcNow,
                        ExecutionTimeMs = candidateStopwatch.ElapsedMilliseconds
                    });
                }
            }

            _logger.Info($"Completed processing problem {problem.Id} in {stopwatch.ElapsedMilliseconds}ms");
            return solutions;
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to process problem {problem.Id}: {ex.Message}");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<string> SetupWorkspaceAsync(
        SwebenchProblem problem, 
        int candidateIndex, 
        CancellationToken cancellationToken = default)
    {
        if (problem == null)
        {
            throw new ArgumentNullException(nameof(problem));
        }

        if (candidateIndex < 0)
        {
            throw new ArgumentException("Candidate index must be non-negative", nameof(candidateIndex));
        }

        try
        {
            // Create workspace directory
            string workspaceRoot = _config.WorkspaceDirectory;
            if (string.IsNullOrEmpty(workspaceRoot))
            {
                workspaceRoot = Path.Combine(Path.GetTempPath(), "cycod-bench", "workspaces");
            }
            
            if (!Directory.Exists(workspaceRoot))
            {
                Directory.CreateDirectory(workspaceRoot);
            }

            var shortId = problem.Id.Length > 8 ? problem.Id[..8] : problem.Id;
            string workspaceId = $"{shortId}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}-{candidateIndex}";
            string workspacePath = Path.Combine(workspaceRoot, workspaceId);
            
            _logger.Debug($"Setting up workspace at {workspacePath}");
            
            if (Directory.Exists(workspacePath))
            {
                _logger.Debug($"Cleaning up existing workspace at {workspacePath}");
                Directory.Delete(workspacePath, true);
            }
            
            Directory.CreateDirectory(workspacePath);
            
            var dockerImage = _dockerManager.GetProblemImageName(problem.Id);
            _logger.Debug($"Pulling Docker image {dockerImage}");
            await _dockerManager.PullImageAsync(dockerImage);
            
            // Set up problem statement file
            string problemStatementPath = Path.Combine(workspacePath, "problem_statement.md");
            FileHelpers.WriteAllText(problemStatementPath, problem.ProblemStatement);
            
            return workspacePath;
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to set up workspace: {ex.Message}");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<CandidateSolution> ExecuteAgentAsync(
        SwebenchProblem problem, 
        string workspacePath, 
        int candidateIndex, 
        CancellationToken cancellationToken = default)
    {
        if (problem == null)
        {
            throw new ArgumentNullException(nameof(problem));
        }

        if (string.IsNullOrEmpty(workspacePath))
        {
            throw new ArgumentException("Workspace path cannot be null or empty", nameof(workspacePath));
        }

        if (candidateIndex < 0)
        {
            throw new ArgumentException("Candidate index must be non-negative", nameof(candidateIndex));
        }

        try
        {
            // Start container
            string containerId = await _dockerManager.StartContainerAsync(problem.Id, workspacePath);
            
            try
            {
                // Execute agent in the container
                _logger.Info($"Executing agent for problem {problem.Id}, candidate {candidateIndex}");
                
                var problemStatementPath = Path.Combine(workspacePath, "problem_statement.md");
                
                // Execute the agent
                var solution = await _agentExecutor.ExecuteAgentAsync(
                    problem, 
                    workspacePath, 
                    candidateIndex,
                    containerId,
                    _config.AgentPath,
                    _config.AgentTimeoutMs / 1000, // Convert to seconds
                    cancellationToken);
                
                _logger.Info($"Agent execution completed for problem {problem.Id}, candidate {candidateIndex}");
                
                return solution;
            }
            finally
            {
                // Always stop the container
                try
                {
                    await _dockerManager.StopContainerAsync(containerId);
                }
                catch (Exception ex)
                {
                    _logger.Warning($"Failed to stop container {containerId}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to execute agent: {ex.Message}");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<CandidateSolution> EvaluateCandidateSolutionAsync(
        SwebenchProblem problem, 
        CandidateSolution solution, 
        CancellationToken cancellationToken = default)
    {
        if (problem == null)
        {
            throw new ArgumentNullException(nameof(problem));
        }

        if (solution == null)
        {
            throw new ArgumentNullException(nameof(solution));
        }

        try
        {
            _logger.Info($"Evaluating solution for problem {problem.Id}, candidate {solution.CandidateIndex}");
            
            if (string.IsNullOrWhiteSpace(solution.Diff))
            {
                _logger.Warning("Solution has empty diff, skipping evaluation");
                solution.EvaluationResult = new EvaluationResult
                {
                    ErrorMessage = "Empty solution diff"
                };
                
                return solution;
            }

            // Start a container for evaluation
            string containerId = await _dockerManager.StartContainerAsync(problem.Id, solution.WorkspacePath);
            
            try
            {
                // Evaluate the solution
                var evaluationResult = await _evaluationService.EvaluateSolutionAsync(
                    problem,
                    solution,
                    containerId,
                    cancellationToken);
                
                solution.EvaluationResult = evaluationResult;
                
                _logger.Info($"Evaluation completed for problem {problem.Id}, candidate {solution.CandidateIndex}");
                
                return solution;
            }
            finally
            {
                // Always stop the container
                try
                {
                    await _dockerManager.StopContainerAsync(containerId);
                }
                catch (Exception ex)
                {
                    _logger.Warning($"Failed to stop evaluation container {containerId}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to evaluate solution: {ex.Message}");
            
            // Set a failed evaluation result
            solution.EvaluationResult = new EvaluationResult
            {
                ErrorMessage = $"Evaluation error: {ex.Message}"
            };
            
            return solution;
        }
    }

    /// <inheritdoc />
    public async Task CleanupWorkspaceAsync(string workspacePath, bool keepWorkspace = false)
    {
        if (string.IsNullOrEmpty(workspacePath))
        {
            throw new ArgumentException("Workspace path cannot be null or empty", nameof(workspacePath));
        }

        if (!Directory.Exists(workspacePath))
        {
            _logger.Debug($"Workspace directory not found at {workspacePath}, nothing to clean up");
            return;
        }

        if (keepWorkspace)
        {
            _logger.Debug($"Keeping workspace directory at {workspacePath}");
            return;
        }

        try
        {
            _logger.Debug($"Cleaning up workspace directory at {workspacePath}");
            Directory.Delete(workspacePath, true);
        }
        catch (Exception ex)
        {
            _logger.Warning($"Failed to clean up workspace: {ex.Message}");
        }
    }
}