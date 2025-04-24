/// <summary>
/// Interface for classes that provide named values for template substitution
/// </summary>
public interface INamedValues
{
    /// <summary>
    /// Gets a value by name, or an empty string if not found
    /// </summary>
    /// <param name="name">The name of the value to retrieve</param>
    /// <returns>The value, or null if not found</returns>
    string? Get(string name);

    /// <summary>
    /// Sets a value by name
    /// </summary>
    /// <param name="name">The name of the value to set</param>
    /// <param name="value">The value to set</param>
    void Set(string name, string value);

    /// <summary>
    /// Checks if a value exists
    /// </summary>
    /// <param name="name">The name of the value to check</param>
    /// <returns>True if the value exists, false otherwise</returns>
    bool Contains(string name);
}