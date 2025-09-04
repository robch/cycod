/**
 * Defines the source of a configuration value.
 */
export enum ConfigSource {
    /**
     * Command line argument (highest precedence).
     */
    CommandLine,

    /**
     * Environment variable.
     */
    EnvironmentVariable,

    /**
     * File explicitly loaded with --config.
     */
    ConfigFileName,

    /**
     * Local/project-specific configuration (in current directory or parent directories).
     */
    LocalConfig,

    /**
     * User-specific configuration (in user home directory).
     */
    UserConfig,

    /**
     * Global/system-wide configuration.
     */
    GlobalConfig,

    /**
     * Default value set by the application.
     */
    Default,

    /**
     * Setting not found in any source.
     */
    NotFound
}