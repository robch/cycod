using Newtonsoft.Json;

namespace CycodBench.Models;

/// <summary>
/// Represents the overall results of a benchmark run.
/// </summary>
public class BenchmarkResult
{
    /// <summary>
    /// Gets or sets the unique identifier for the benchmark run.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the timestamp when the benchmark was started.
    /// </summary>
    [JsonProperty("start_time")]
    public DateTimeOffset StartTime { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the benchmark was completed.
    /// </summary>
    [JsonProperty("end_time")]
    public DateTimeOffset EndTime { get; set; }

    /// <summary>
    /// Gets or sets the total elapsed time in milliseconds.
    /// </summary>
    [JsonProperty("total_elapsed_ms")]
    public long TotalElapsedMs { get; set; }
    
    /// <summary>
    /// Gets or sets the configuration used for this benchmark run.
    /// </summary>
    [JsonProperty("config")]
    public BenchmarkConfig Config { get; set; } = new BenchmarkConfig();

    /// <summary>
    /// Gets or sets the map of problem IDs to ensemble results.
    /// </summary>
    [JsonProperty("results")]
    public Dictionary<string, EnsembleResult> Results { get; set; } = new Dictionary<string, EnsembleResult>();

    /// <summary>
    /// Gets or sets the number of problems that were successfully solved.
    /// </summary>
    [JsonProperty("successful_count")]
    public int SuccessfulCount { get; set; }

    /// <summary>
    /// Gets or sets the total number of problems processed.
    /// </summary>
    [JsonProperty("total_count")]
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets the success rate as a percentage.
    /// </summary>
    [JsonProperty("success_rate")]
    public double SuccessRate => TotalCount > 0 ? (double)SuccessfulCount / TotalCount * 100 : 0;

    /// <summary>
    /// Gets or sets the agent version used for this benchmark run.
    /// </summary>
    [JsonProperty("agent_version")]
    public string AgentVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the benchmark runner version.
    /// </summary>
    [JsonProperty("runner_version")]
    public string RunnerVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional metadata for this benchmark result.
    /// </summary>
    [JsonProperty("metadata")]
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}