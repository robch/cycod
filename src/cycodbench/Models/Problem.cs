using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CycodBench.Models
{
    /// <summary>
    /// Represents a software engineering problem from SWE-bench.
    /// </summary>
    public class Problem
    {
        /// <summary>
        /// Unique identifier for the problem, typically in the format `owner__repo-issue-id`.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Candidate index for the problem, used to differentiate multiple instances of the same problem.
        /// </summary>
        [JsonPropertyName("idx")]
        public int Idx { get; set; }

        /// <summary>
        /// Repository name and owner in the format "owner/repo".
        /// </summary>
        [JsonPropertyName("repo")]
        public string Repo { get; set; } = string.Empty;

        /// <summary>
        /// Base commit hash for the repository.
        /// </summary>
        [JsonPropertyName("base_commit")]
        public string BaseCommit { get; set; } = string.Empty;

        /// <summary>
        /// Problem statement text.
        /// </summary>
        [JsonPropertyName("problem_statement")]
        public string ProblemStatement { get; set; } = string.Empty;
        
        /// <summary>
        /// Hints text for the agent.
        /// </summary>
        [JsonPropertyName("hints_text")]
        public string HintsText { get; set; } = string.Empty;
        
        /// <summary>
        /// Patch that fixes the problem.
        /// </summary>
        [JsonPropertyName("patch")]
        public string Patch { get; set; } = string.Empty;
        
        /// <summary>
        /// Tests that should fail before the fix and pass after the fix.
        /// </summary>
        [JsonPropertyName("FAIL_TO_PASS")]
        public List<string> FailToPass { get; set; } = new List<string>();
        
        /// <summary>
        /// Tests that should pass before and after the fix.
        /// </summary>
        [JsonPropertyName("PASS_TO_PASS")]
        public List<string> PassToPass { get; set; } = new List<string>();
    }

    /// <summary>
    /// Represents a dataset of problems.
    /// </summary>
    public class ProblemDataset
    {
        /// <summary>
        /// List of problems in the dataset.
        /// </summary>
        [JsonPropertyName("problems")]
        public List<Problem> Problems { get; set; } = new List<Problem>();
    }
}