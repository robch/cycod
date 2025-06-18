using Newtonsoft.Json;

namespace CycodBench.Models;

/// <summary>
/// Represents performance metrics and statistics for a benchmark run.
/// </summary>
public class BenchmarkMetrics
{
    /// <summary>
    /// Gets or sets the success rate as a percentage.
    /// </summary>
    [JsonProperty("success_rate")]
    public double SuccessRate { get; set; }

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
    /// Gets or sets the average time taken per problem in milliseconds.
    /// </summary>
    [JsonProperty("average_time_per_problem_ms")]
    public double AverageTimePerProblemMs { get; set; }

    /// <summary>
    /// Gets or sets the average number of candidates needed to find a successful solution.
    /// </summary>
    [JsonProperty("average_candidates_per_success")]
    public double AverageCandidatesPerSuccess { get; set; }

    /// <summary>
    /// Gets or sets the average time spent in agent execution per problem in milliseconds.
    /// </summary>
    [JsonProperty("average_agent_time_ms")]
    public double AverageAgentTimeMs { get; set; }

    /// <summary>
    /// Gets or sets the average time spent in evaluation per problem in milliseconds.
    /// </summary>
    [JsonProperty("average_evaluation_time_ms")]
    public double AverageEvaluationTimeMs { get; set; }

    /// <summary>
    /// Gets or sets the average number of test failures per problem.
    /// </summary>
    [JsonProperty("average_test_failures")]
    public double AverageTestFailures { get; set; }

    /// <summary>
    /// Gets or sets the average number of test passes per problem.
    /// </summary>
    [JsonProperty("average_test_passes")]
    public double AverageTestPasses { get; set; }

    /// <summary>
    /// Gets or sets the histogram of solution attempts by category.
    /// </summary>
    [JsonProperty("solution_histogram")]
    public Dictionary<string, int> SolutionHistogram { get; set; } = new Dictionary<string, int>();
}