/// <summary>
/// Defines the scope of configuration settings.
/// </summary>
public enum ConfigFileScope
{
    /// <summary>
    /// Global settings (system-wide).
    /// </summary>
    Global,

    /// <summary>
    /// User settings (user-specific).
    /// </summary>
    User,

    /// <summary>
    /// Local settings (project-specific).
    /// </summary>
    Local,
    
    /// <summary>
    /// All scopes (used for display/querying across scopes).
    /// </summary>
    Any
}