public class NoNamedValues : INamedValues
{
    /// <summary>
    /// Gets a value by name, always returns null
    /// </summary>
    /// <param name="name">The name of the value to retrieve</param>
    /// <returns>Always null</returns>
    public string? Get(string name)
    {
        return null;
    }

    /// <summary>
    /// Sets a value by name, does nothing
    /// </summary>
    /// <param name="name">The name of the value to set</param>
    /// <param name="value">The value to set</param>
    public void Set(string name, string value)
    {
        // No operation
    }

    /// <summary>
    /// Checks if a value exists, always returns false
    /// </summary>
    /// <param name="name">The name of the value to check</param>
    /// <returns>Always false</returns>
    public bool Contains(string name)
    {
        return false;
    }
}
