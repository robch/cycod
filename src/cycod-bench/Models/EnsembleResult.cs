using Newtonsoft.Json;

namespace CycodBench.Models;

/// <summary>
/// Represents the result of ensembling multiple candidate solutions.
/// </summary>
public class EnsembleResult
{
    /// <summary>
    /// Gets or sets the unique identifier for the ensemble result.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the ID of the problem that was ensembled.
    /// </summary>
    [JsonProperty("problem_id")]
    public string ProblemId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the IDs of all candidate solutions that were considered.
    /// </summary>
    [JsonProperty("candidate_solution_ids")]
    public List<string> CandidateSolutionIds { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the ID of the selected best candidate solution.
    /// </summary>
    [JsonProperty("selected_solution_id")]
    public string SelectedSolutionId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the reason for selecting this candidate.
    /// </summary>
    [JsonProperty("selection_reason")]
    public string SelectionReason { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the ensemble was completed.
    /// </summary>
    [JsonProperty("timestamp")]
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets additional metadata for this ensemble result.
    /// </summary>
    [JsonProperty("metadata")]
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    
    /// <summary>
    /// Gets or sets the best candidate solution selected by the ensemble.
    /// </summary>
    [JsonIgnore]
    public CandidateSolution? SelectedSolution { get; set; }
}