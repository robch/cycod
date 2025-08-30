import * as path from 'path';
import * as os from 'os';
import * as fs from 'fs-extra';

/**
 * Helper functions for working with file paths.
 */
export class PathHelpers {
  /**
   * Gets the user's home directory.
   */
  static getHomeDirectory(): string {
    return os.homedir();
  }

  /**
   * Gets the global configuration directory.
   */
  static getGlobalConfigDirectory(): string {
    if (process.platform === 'win32') {
      const commonAppData = process.env.ALLUSERSPROFILE || 'C:\\ProgramData';
      return path.join(commonAppData, '.cycod');
    } else {
      // On Linux/Mac, use XDG_DATA_HOME or ~/.local/share
      const xdgDataHome = process.env.XDG_DATA_HOME;
      if (xdgDataHome) {
        return path.join(xdgDataHome, '.cycod');
      } else {
        const homeDir = this.getHomeDirectory();
        return path.join(homeDir, '.local', 'share', '.cycod');
      }
    }
  }

  /**
   * Gets the user configuration directory.
   */
  static getUserConfigDirectory(): string {
    const home = this.getHomeDirectory();
    return path.join(home, '.cycod');
  }

  /**
   * Gets the local configuration directory (current working directory).
   */
  static getLocalConfigDirectory(): string {
    return path.join(process.cwd(), '.cycod');
  }

  /**
   * Gets the configuration file path for a given scope.
   */
  static getConfigFilePath(scope: 'global' | 'user' | 'local'): string {
    let configDir: string;

    switch (scope) {
      case 'global':
        configDir = this.getGlobalConfigDirectory();
        break;
      case 'user':
        configDir = this.getUserConfigDirectory();
        break;
      case 'local':
        configDir = this.getLocalConfigDirectory();
        break;
      default:
        throw new Error(`Invalid scope: ${scope}`);
    }

    return path.join(configDir, 'config.yaml');
  }

  /**
   * Ensures that a directory exists.
   */
  static async ensureDirectory(dirPath: string): Promise<void> {
    await fs.ensureDir(dirPath);
  }

  /**
   * Expands a path with tilde (~) to the full home directory path.
   */
  static expandPath(filePath: string): string {
    if (filePath.startsWith('~/')) {
      return path.join(this.getHomeDirectory(), filePath.slice(2));
    }
    return filePath;
  }

  /**
   * Normalizes a file path.
   */
  static normalizePath(filePath: string): string {
    return path.normalize(this.expandPath(filePath));
  }

  /**
   * Checks if a path is absolute.
   */
  static isAbsolute(filePath: string): boolean {
    return path.isAbsolute(filePath);
  }

  /**
   * Resolves a path relative to the current working directory.
   */
  static resolve(filePath: string): string {
    return path.resolve(this.expandPath(filePath));
  }

  /**
   * Gets the directory path for a given scope.
   */
  static getScopeDirectoryPath(scope: any): string | null {
    // Handle string enum values
    const scopeStr = scope?.toString?.()?.toLowerCase() || String(scope).toLowerCase();
    
    switch (scopeStr) {
      case 'global':
        return this.getGlobalConfigDirectory();
      case 'user':
        return this.getUserConfigDirectory();
      case 'local':
        return this.getLocalConfigDirectory();
      default:
        return null;
    }
  }
}