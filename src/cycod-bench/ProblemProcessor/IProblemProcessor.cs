using CycodBench.Models;

namespace CycodBench.ProblemProcessor;

/// <summary>
/// Interface for processing SWE-bench problems.
/// </summary>
public interface IProblemProcessor
{
    /// <summary>
    /// Processes a single SWE-bench problem, generating and evaluating candidate solutions.
    /// </summary>
    /// <param name="problem">The problem to process.</param>
    /// <param name="candidateCount">The number of candidate solutions to generate.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A list of candidate solutions with evaluation results.</returns>
    Task<IEnumerable<CandidateSolution>> ProcessProblemAsync(SwebenchProblem problem, int candidateCount, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets up a workspace for processing a problem.
    /// </summary>
    /// <param name="problem">The problem to set up the workspace for.</param>
    /// <param name="candidateIndex">The index of the candidate solution being processed.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The path to the workspace directory.</returns>
    Task<string> SetupWorkspaceAsync(SwebenchProblem problem, int candidateIndex, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the agent to generate a solution for a problem.
    /// </summary>
    /// <param name="problem">The problem to generate a solution for.</param>
    /// <param name="workspacePath">The path to the workspace directory.</param>
    /// <param name="candidateIndex">The index of the candidate solution being processed.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The candidate solution.</returns>
    Task<CandidateSolution> ExecuteAgentAsync(SwebenchProblem problem, string workspacePath, int candidateIndex, CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluates a candidate solution.
    /// </summary>
    /// <param name="problem">The problem the solution is for.</param>
    /// <param name="solution">The candidate solution to evaluate.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The candidate solution with evaluation results.</returns>
    Task<CandidateSolution> EvaluateCandidateSolutionAsync(SwebenchProblem problem, CandidateSolution solution, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cleans up resources after processing a problem.
    /// </summary>
    /// <param name="workspacePath">The path to the workspace directory to clean up.</param>
    /// <param name="keepWorkspace">Whether to keep the workspace directory.</param>
    /// <returns>A task representing the asynchronous cleanup operation.</returns>
    Task CleanupWorkspaceAsync(string workspacePath, bool keepWorkspace = false);
}