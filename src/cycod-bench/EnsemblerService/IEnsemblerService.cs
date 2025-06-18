using CycodBench.Models;

namespace CycodBench.EnsemblerService;

/// <summary>
/// Defines methods for ensembling multiple candidate solutions to select the best one.
/// </summary>
public interface IEnsemblerService
{
    /// <summary>
    /// Selects the best solution from multiple candidates for a single problem.
    /// </summary>
    /// <param name="problem">The SWE-bench problem.</param>
    /// <param name="candidateSolutions">The list of candidate solutions to ensemble.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The ensemble result containing the selected best solution.</returns>
    Task<EnsembleResult> EnsembleSolutionsAsync(
        SwebenchProblem problem,
        IList<CandidateSolution> candidateSolutions,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Ensembles multiple solution sets for different problems.
    /// </summary>
    /// <param name="problemSolutions">Dictionary mapping problem IDs to lists of candidate solutions.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A dictionary mapping problem IDs to ensemble results.</returns>
    Task<Dictionary<string, EnsembleResult>> EnsembleMultipleProblemsAsync(
        Dictionary<string, (SwebenchProblem Problem, IList<CandidateSolution> Solutions)> problemSolutions,
        CancellationToken cancellationToken = default);
}