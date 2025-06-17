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
    /// Gets the file path of the configuration source, if applicable.
    /// </summary>
    string? ConfigFilePath { get; }
}