namespace CycodBench.Configuration;

/// <summary>
/// Interface for accessing configuration settings for the benchmark runner.
/// </summary>
public interface IConfiguration
{
    /// <summary>
    /// Gets a configuration value as a string.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <returns>The configuration value, or null if not found.</returns>
    string? GetString(string key);
    
    /// <summary>
    /// Gets a configuration value as a string, with a default value if not found.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <param name="defaultValue">The default value to use if the key is not found.</param>
    /// <returns>The configuration value, or the default value if not found.</returns>
    string GetString(string key, string defaultValue);
    
    /// <summary>
    /// Gets a configuration value as an integer.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <param name="defaultValue">The default value to use if the key is not found or cannot be converted.</param>
    /// <returns>The configuration value as an integer, or the default value if not found or invalid.</returns>
    int GetInt(string key, int defaultValue);
    
    /// <summary>
    /// Gets a configuration value as a boolean.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <param name="defaultValue">The default value to use if the key is not found or cannot be converted.</param>
    /// <returns>The configuration value as a boolean, or the default value if not found or invalid.</returns>
    bool GetBool(string key, bool defaultValue);
    
    /// <summary>
    /// Gets a configuration value as a double.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <param name="defaultValue">The default value to use if the key is not found or cannot be converted.</param>
    /// <returns>The configuration value as a double, or the default value if not found or invalid.</returns>
    double GetDouble(string key, double defaultValue);
    
    /// <summary>
    /// Gets a configuration value as a string array.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <returns>The configuration value as a string array, or an empty array if not found.</returns>
    string[] GetStringArray(string key);
    
    /// <summary>
    /// Gets a configuration section as a sub-configuration.
    /// </summary>
    /// <param name="key">The section key.</param>
    /// <returns>A configuration object for the specified section.</returns>
    IConfiguration GetSection(string key);
    
    /// <summary>
    /// Gets all keys in the current configuration section.
    /// </summary>
    /// <returns>An IEnumerable of all keys.</returns>
    IEnumerable<string> GetKeys();
    
    /// <summary>
    /// Checks if a configuration key exists.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <returns>true if the key exists, false otherwise.</returns>
    bool Exists(string key);
    
    /// <summary>
    /// Sets a configuration value for the current session.
    /// Note: This does not persist the value to the configuration file.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <param name="value">The value to set.</param>
    void SetValue(string key, string? value);
    
    /// <summary>
    /// Gets the file path of the configuration source, if applicable.
    /// </summary>
    string? ConfigFilePath { get; }
    
    /// <summary>
    /// Gets the path to the cycod agent executable.
    /// </summary>
    string AgentPath => GetString("Agent:ExecutablePath", "cycod");
    
    /// <summary>
    /// Gets the timeout in seconds for agent execution.
    /// </summary>
    int AgentTimeoutSeconds => GetInt("Agent:Timeout", 3600);
    
    /// <summary>
    /// Gets the timeout in seconds for build execution.
    /// </summary>
    int BuildTimeoutSeconds => GetInt("Evaluation:BuildTimeout", 600);
    
    /// <summary>
    /// Gets the timeout in seconds for test execution.
    /// </summary>
    int TestTimeoutSeconds => GetInt("Evaluation:TestTimeout", 600);
    
    /// <summary>
    /// Gets whether to execute in Docker containers.
    /// </summary>
    bool UseContainer => GetBool("Docker:Enabled", true);
    
    /// <summary>
    /// Gets whether to use the agent for ensembling.
    /// </summary>
    bool UseAgentEnsembling => GetBool("Ensemble:UseAgent", true);
    
    /// <summary>
    /// Gets the root directory for workspaces.
    /// </summary>
    string WorkspaceRoot => GetString("Workspace:RootDirectory", Path.Combine(Directory.GetCurrentDirectory(), "workspaces"));
    
    /// <summary>
    /// Gets the default build command.
    /// </summary>
    string DefaultBuildCommand => GetString("Evaluation:DefaultBuildCommand", "make");
    
    /// <summary>
    /// Gets the default test command.
    /// </summary>
    string DefaultTestCommand => GetString("Evaluation:DefaultTestCommand", "make test");
    
    /// <summary>
    /// Gets the number of shards for distributed processing.
    /// </summary>
    int ShardCount => GetInt("ShardCount", 1);
    
    /// <summary>
    /// Gets the current shard ID (zero-based).
    /// </summary>
    int ShardId => GetInt("ShardId", 0);
    
    /// <summary>
    /// Gets the number of candidate solutions to generate per problem.
    /// </summary>
    int CandidateCount => GetInt("CandidateCount", 8);
    
    /// <summary>
    /// Gets the level of parallelism for processing problems.
    /// </summary>
    int Parallelism => GetInt("Parallelism", 4);
    
    /// <summary>
    /// Gets the default Docker image to use if not specified by the problem.
    /// </summary>
    string DefaultDockerImage => GetString("Docker:Image", "swebench/base:latest");
    
    /// <summary>
    /// Gets the directory where workspaces will be created.
    /// </summary>
    string WorkspaceDirectory => GetString("Workspace:RootDirectory", Path.Combine(Path.GetTempPath(), "cycod-bench", "workspaces"));
    
    /// <summary>
    /// Gets whether to keep workspace directories after processing.
    /// </summary>
    bool KeepWorkspaces => GetBool("Workspace:KeepWorkspaces", true);
    
    /// <summary>
    /// Gets the directory where results will be stored.
    /// </summary>
    string ResultsDirectory => GetString("Results:Directory", Path.Combine(Directory.GetCurrentDirectory(), "results"));
    
    /// <summary>
    /// Gets the timeout in milliseconds for agent execution.
    /// </summary>
    int AgentTimeoutMs => GetInt("Agent:Timeout", 3600) * 1000;
    
    /// <summary>
    /// Gets the timeout in milliseconds for evaluation.
    /// </summary>
    int EvaluationTimeoutMs => GetInt("Evaluation:TestTimeout", 600) * 1000;
}