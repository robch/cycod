using CycodBench.Models;

namespace CycodBench.EvaluationService;

/// <summary>
/// Defines methods for evaluating candidate solutions against test cases.
/// </summary>
public interface IEvaluationService
{
    /// <summary>
    /// Evaluates a candidate solution against the test cases.
    /// </summary>
    /// <param name="problem">The SWE-bench problem being solved.</param>
    /// <param name="candidateSolution">The candidate solution to evaluate.</param>
    /// <param name="containerId">The ID of the Docker container to use for evaluation.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The evaluation result.</returns>
    Task<EvaluationResult> EvaluateSolutionAsync(
        SwebenchProblem problem,
        CandidateSolution candidateSolution,
        string containerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies a diff to the codebase.
    /// </summary>
    /// <param name="diff">The diff to apply.</param>
    /// <param name="workspacePath">The path to the workspace directory where the codebase is located.</param>
    /// <param name="containerId">The ID of the Docker container to use for applying the diff.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>True if the diff was applied successfully, false otherwise.</returns>
    Task<bool> ApplyDiffAsync(
        string diff,
        string workspacePath,
        string containerId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Runs the build command for a problem.
    /// </summary>
    /// <param name="problem">The SWE-bench problem.</param>
    /// <param name="workspacePath">The path to the workspace directory.</param>
    /// <param name="containerId">The ID of the Docker container to use for running the build command.</param>
    /// <param name="timeoutSeconds">Optional timeout in seconds. If null, the default timeout from configuration will be used.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A tuple containing the exit code and the output of the build command.</returns>
    Task<(int ExitCode, string Output)> RunBuildCommandAsync(
        SwebenchProblem problem,
        string workspacePath,
        string containerId,
        int? timeoutSeconds = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Runs the test command for a problem.
    /// </summary>
    /// <param name="problem">The SWE-bench problem.</param>
    /// <param name="workspacePath">The path to the workspace directory.</param>
    /// <param name="containerId">The ID of the Docker container to use for running the test command.</param>
    /// <param name="timeoutSeconds">Optional timeout in seconds. If null, the default timeout from configuration will be used.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A tuple containing the exit code and the output of the test command.</returns>
    Task<(int ExitCode, string Output)> RunTestCommandAsync(
        SwebenchProblem problem,
        string workspacePath,
        string containerId,
        int? timeoutSeconds = null,
        CancellationToken cancellationToken = default);
}