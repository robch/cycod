using Newtonsoft.Json;

namespace CycodBench.Models;

/// <summary>
/// Represents a problem from the SWE-bench dataset.
/// </summary>
public class SwebenchProblem
{
    /// <summary>
    /// Gets or sets the unique identifier for the problem.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the repository name in the format owner/repo.
    /// </summary>
    [JsonProperty("repo")]
    public string Repository { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the base commit hash.
    /// </summary>
    [JsonProperty("base_commit")]
    public string BaseCommit { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the natural language problem statement.
    /// </summary>
    [JsonProperty("problem_statement")]
    public string ProblemStatement { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the test command to run for evaluation.
    /// </summary>
    [JsonProperty("test_command")]
    public string TestCommand { get; set; } = string.Empty;
}