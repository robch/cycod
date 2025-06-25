using CycodBench.Models;

namespace CycodBench.EvaluationService;

/// <summary>
/// Defines methods for evaluating candidate solutions against SWEBench problems.
/// </summary>
public interface IEvaluationService
{
    /// <summary>
    /// Evaluates a candidate solution for a SWEBench problem.
    /// </summary>
    /// <param name="problem">The SWE-bench problem being solved.</param>
    /// <param name="candidateSolution">The candidate solution to evaluate.</param>
    /// <param name="containerId">The ID of the Docker container to use for evaluation (if applicable).</param>
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
    /// <param name="containerId">The ID of the Docker container to use for applying the diff (if applicable).</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>True if the diff was applied successfully, false otherwise.</returns>
    Task<bool> ApplyDiffAsync(
        string diff,
        string workspacePath,
        string containerId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Creates a predictions file in the format expected by SWEBench evaluator.
    /// </summary>
    /// <param name="problem">The SWEBench problem.</param>
    /// <param name="candidateSolution">The candidate solution.</param>
    /// <param name="outputDirectory">The directory where to save the predictions file.</param>
    /// <returns>The path to the created predictions file.</returns>
    Task<string> CreatePredictionsFileAsync(
        SwebenchProblem problem,
        CandidateSolution candidateSolution,
        string outputDirectory);
}