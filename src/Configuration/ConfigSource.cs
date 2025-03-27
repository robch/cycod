/// <summary>
/// Defines the source of a configuration value.
/// </summary>
public enum ConfigSource
{
    /// <summary>
    /// Command line argument (highest precedence).
    /// </summary>
    CommandLine,

    /// <summary>
    /// Environment variable.
    /// </summary>
    EnvironmentVariable,

    /// <summary>
    /// File explicitly loaded with --config.
    /// </summary>
    ConfigFileName,

    /// <summary>
    /// Local/project-specific configuration (in current directory or parent directories).
    /// </summary>
    LocalConfig,

    /// <summary>
    /// User-specific configuration (in user home directory).
    /// </summary>
    UserConfig,

    /// <summary>
    /// Global/system-wide configuration.
    /// </summary>
    GlobalConfig,

    /// <summary>
    /// Default value set by the application.
    /// </summary>
    Default,

    /// <summary>
    /// Setting not found in any source.
    /// </summary>
    NotFound
}