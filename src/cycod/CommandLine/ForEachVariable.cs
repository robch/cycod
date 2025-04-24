using System;
using System.Collections.Generic;

/// <summary>
/// Represents a foreach variable used in command line expansion.
/// The variable has a name and a list of values that will be used
/// to create multiple ChatCommand instances during expansion.
/// </summary>
public class ForEachVariable
{
    /// <summary>
    /// The name of the variable, which will be used in templates as {name}
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// The list of values this variable can take
    /// </summary>
    public List<string> Values { get; }

    /// <summary>
    /// Creates a new ForEachVariable with the specified name and values
    /// </summary>
    /// <param name="name">The variable name</param>
    /// <param name="values">The list of values</param>
    public ForEachVariable(string name, List<string> values)
    {
        Name = name;
        Values = values;
    }
}