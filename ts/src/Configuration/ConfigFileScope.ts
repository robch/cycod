/**
 * Defines the scope of configuration settings.
 */
export enum ConfigFileScope {
  /**
   * Global settings (system-wide).
   */
  Global = 'global',

  /**
   * User settings (user-specific).
   */
  User = 'user',

  /**
   * Local settings (project-specific).
   */
  Local = 'local',

  /**
   * Config file settings (from explicitly loaded configuration file paths).
   */
  FileName = 'filename',

  /**
   * All scopes (used for display/querying across scopes).
   */
  Any = 'any',
}