using CycodBench.Models;

namespace CycodBench.EvaluationToolsManager;

/// <summary>
/// Interface for managing the SWE-bench evaluation tools.
/// </summary>
public interface IEvaluationToolsManager
{
    /// <summary>
    /// Sets up the evaluation tools.
    /// </summary>
    /// <param name="forceSetup">Whether to force setup even if the tools already exist.</param>
    Task<bool> SetupToolsAsync(bool forceSetup = false);

    /// <summary>
    /// Evaluates a candidate solution against the test cases.
    /// </summary>
    /// <param name="problem">The problem to evaluate.</param>
    /// <param name="candidate">The candidate solution to evaluate.</param>
    /// <param name="outputDirectory">The directory where evaluation output should be saved.</param>
    /// <param name="numProcesses">The number of parallel processes to use for evaluation.</param>
    /// <returns>The evaluation result.</returns>
    Task<EvaluationResult> EvaluateAsync(
        SwebenchProblem problem, 
        CandidateSolution candidate, 
        string outputDirectory, 
        int numProcesses = 1);

    /// <summary>
    /// Checks if the evaluation tools are correctly set up.
    /// </summary>
    /// <returns>True if the tools are correctly set up, otherwise false.</returns>
    Task<bool> AreToolsSetupAsync();
    
    /// <summary>
    /// Gets the path to the evaluation tools.
    /// </summary>
    /// <returns>The path to the evaluation tools.</returns>
    string GetToolsPath();
    
    /// <summary>
    /// Creates a predictions file for evaluation.
    /// </summary>
    /// <param name="problem">The problem to evaluate.</param>
    /// <param name="candidate">The candidate solution.</param>
    /// <param name="outputDirectory">The directory where the predictions file should be saved.</param>
    /// <returns>The path to the predictions file.</returns>
    Task<string> CreatePredictionsFileAsync(SwebenchProblem problem, CandidateSolution candidate, string outputDirectory);
}