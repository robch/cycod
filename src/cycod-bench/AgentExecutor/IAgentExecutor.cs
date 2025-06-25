using System;
using CycodBench.Models;

namespace CycodBench.AgentExecutor;

/// <summary>
/// Defines methods for executing the cycod agent on SWE-bench problems.
/// </summary>
public interface IAgentExecutor
{
    /// <summary>
    /// Executes the cycod agent on a given problem and returns a candidate solution.
    /// </summary>
    /// <param name="problem">The SWE-bench problem to solve.</param>
    /// <param name="workspacePath">The path to the workspace directory.</param>
    /// <param name="candidateIndex">The index of this candidate solution.</param>
    /// <param name="containerId">The ID of the Docker container to use for execution.</param>
    /// <param name="agentPath">Optional path to the agent executable. If null, the default path from configuration will be used.</param>
    /// <param name="timeoutSeconds">Optional timeout in seconds. If null, the default timeout from configuration will be used.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A candidate solution containing the agent's output.</returns>
    Task<CandidateSolution> ExecuteAgentAsync(
        SwebenchProblem problem,
        string workspacePath, 
        int candidateIndex,
        string containerId,
        string? agentPath = null, 
        int? timeoutSeconds = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// [Deprecated] This method is kept for compatibility but no longer extracts diffs from agent output.
    /// Diffs are now generated using Git CLI.
    /// </summary>
    /// <param name="agentOutput">The raw output from the agent.</param>
    /// <returns>An empty string as this method is deprecated.</returns>
    [Obsolete("This method is deprecated. Diffs are now generated using Git CLI.")]
    string ExtractDiffFromOutput(string agentOutput);

    /// <summary>
    /// Gets the version of the cycod agent.
    /// </summary>
    /// <param name="agentPath">Optional path to the agent executable. If null, the default path from configuration will be used.</param>
    /// <returns>The agent version string.</returns>
    Task<string> GetAgentVersionAsync(string? agentPath = null);
}