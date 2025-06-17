using Newtonsoft.Json;

namespace CycodBench.Models;

/// <summary>
/// Represents the configuration for the benchmark runner.
/// </summary>
public class BenchmarkConfig
{
    /// <summary>
    /// Gets or sets the path to the SWE-bench dataset.
    /// </summary>
    [JsonProperty("dataset_path")]
    public string DatasetPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path to the evaluation tools.
    /// </summary>
    [JsonProperty("eval_tools_path")]
    public string EvalToolsPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path to the cycod agent executable.
    /// </summary>
    [JsonProperty("agent_path")]
    public string AgentPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path to the workspace directory.
    /// </summary>
    [JsonProperty("workspace_path")]
    public string WorkspacePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path to the results directory.
    /// </summary>
    [JsonProperty("results_path")]
    public string ResultsPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the total number of shards to split the dataset into.
    /// </summary>
    [JsonProperty("shard_count")]
    public int ShardCount { get; set; } = 1;

    /// <summary>
    /// Gets or sets the ID of the current shard (0 to ShardCount-1).
    /// </summary>
    [JsonProperty("shard_id")]
    public int ShardId { get; set; } = 0;

    /// <summary>
    /// Gets or sets the number of candidate solutions to generate per problem.
    /// </summary>
    [JsonProperty("candidates_per_problem")]
    public int CandidatesPerProblem { get; set; } = 1;

    /// <summary>
    /// Gets or sets the level of parallelism for processing problems.
    /// </summary>
    [JsonProperty("parallelism")]
    public int Parallelism { get; set; } = 1;

    /// <summary>
    /// Gets or sets the timeout for agent execution in milliseconds.
    /// </summary>
    [JsonProperty("agent_timeout_ms")]
    public int AgentTimeoutMs { get; set; } = 30 * 60 * 1000; // 30 minutes default

    /// <summary>
    /// Gets or sets the timeout for evaluation in milliseconds.
    /// </summary>
    [JsonProperty("evaluation_timeout_ms")]
    public int EvaluationTimeoutMs { get; set; } = 5 * 60 * 1000; // 5 minutes default

    /// <summary>
    /// Gets or sets the Docker container memory limit.
    /// </summary>
    [JsonProperty("container_memory_limit")]
    public string ContainerMemoryLimit { get; set; } = "8g";

    /// <summary>
    /// Gets or sets the Docker container CPU limit.
    /// </summary>
    [JsonProperty("container_cpu_limit")]
    public decimal ContainerCpuLimit { get; set; } = 4;

    /// <summary>
    /// Gets or sets whether to keep workspaces after completion.
    /// </summary>
    [JsonProperty("keep_workspaces")]
    public bool KeepWorkspaces { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to run in debug mode.
    /// </summary>
    [JsonProperty("debug_mode")]
    public bool DebugMode { get; set; } = false;

    /// <summary>
    /// Gets or sets the agent arguments.
    /// </summary>
    [JsonProperty("agent_args")]
    public Dictionary<string, string> AgentArgs { get; set; } = new Dictionary<string, string>();
}