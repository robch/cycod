/**
 * Defines the scope of configuration settings.
 */
export enum ConfigFileScope {
    /**
     * Global settings (system-wide).
     */
    Global,

    /**
     * User settings (user-specific).
     */
    User,

    /**
     * Local settings (project-specific).
     */
    Local,
    
    /**
     * Config file settings (from explicitly loaded configuration file paths).
     */
    FileName,
    
    /**
     * All scopes (used for display/querying across scopes).
     */
    Any
}