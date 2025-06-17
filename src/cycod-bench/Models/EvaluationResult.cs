using Newtonsoft.Json;

namespace CycodBench.Models;

/// <summary>
/// Represents the result of evaluating a candidate solution.
/// </summary>
public class EvaluationResult
{
    /// <summary>
    /// Gets or sets the unique identifier for the evaluation.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the ID of the candidate solution that was evaluated.
    /// </summary>
    [JsonProperty("solution_id")]
    public string SolutionId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the ID of the problem that was evaluated.
    /// </summary>
    [JsonProperty("problem_id")]
    public string ProblemId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the solution was successfully applied.
    /// </summary>
    [JsonProperty("applied")]
    public bool Applied { get; set; }

    /// <summary>
    /// Gets or sets whether the solution passed the tests.
    /// </summary>
    [JsonProperty("passed")]
    public bool Passed { get; set; }

    /// <summary>
    /// Gets or sets the build exit code.
    /// </summary>
    [JsonProperty("build_exit_code")]
    public int BuildExitCode { get; set; }

    /// <summary>
    /// Gets or sets the test exit code.
    /// </summary>
    [JsonProperty("test_exit_code")]
    public int TestExitCode { get; set; }
    
    /// <summary>
    /// Gets or sets the build output.
    /// </summary>
    [JsonProperty("build_output")]
    public string BuildOutput { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the test output.
    /// </summary>
    [JsonProperty("test_output")]
    public string TestOutput { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets any error messages.
    /// </summary>
    [JsonProperty("error_message")]
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the elapsed evaluation time in milliseconds.
    /// </summary>
    [JsonProperty("evaluation_time_ms")]
    public long EvaluationTimeMs { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the evaluation was completed.
    /// </summary>
    [JsonProperty("timestamp")]
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets additional metadata for this evaluation.
    /// </summary>
    [JsonProperty("metadata")]
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}