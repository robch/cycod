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
    /// Gets or sets the head commit hash.
    /// </summary>
    [JsonProperty("head_commit")]
    public string HeadCommit { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the natural language problem statement.
    /// </summary>
    [JsonProperty("problem_statement")]
    public string ProblemStatement { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the issue URL.
    /// </summary>
    [JsonProperty("issue_url")]
    public string IssueUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the pull request URL.
    /// </summary>
    [JsonProperty("pr_url")]
    public string PullRequestUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets any additional context for the problem.
    /// </summary>
    [JsonProperty("additional_context")]
    public string AdditionalContext { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Docker image to use for this problem.
    /// </summary>
    [JsonProperty("docker_image")]
    public string DockerImage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the test command to run for evaluation.
    /// </summary>
    [JsonProperty("test_command")]
    public string TestCommand { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets any test files related to this problem.
    /// </summary>
    [JsonProperty("test_files")]
    public List<string> TestFiles { get; set; } = new List<string>();

    /// <summary>
    /// Gets the short ID of the problem (for display purposes).
    /// </summary>
    [JsonIgnore]
    public string ShortId => Id.Length > 8 ? Id[..8] : Id;

    /// <summary>
    /// Gets the repository owner.
    /// </summary>
    [JsonIgnore]
    public string Owner => Repository.Contains("/") ? Repository.Split('/')[0] : string.Empty;

    /// <summary>
    /// Gets the repository name without owner.
    /// </summary>
    [JsonIgnore]
    public string Repo => Repository.Contains("/") ? Repository.Split('/')[1] : Repository;
}