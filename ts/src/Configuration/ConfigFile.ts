import * as yaml from 'js-yaml';
import * as fs from 'fs-extra';
import * as path from 'path';
import { ConfigFileScope } from './ConfigFileScope';
import { ConfigSetting } from './ConfigValue';

/**
 * Abstract base class for configuration files.
 */
export abstract class ConfigFile {
  protected _fileName: string;
  protected _scope: ConfigFileScope;
  protected _data: Record<string, any> = {};

  constructor(fileName: string, scope: ConfigFileScope) {
    this._fileName = fileName;
    this._scope = scope;
  }

  get fileName(): string {
    return this._fileName;
  }

  get scope(): ConfigFileScope {
    return this._scope;
  }

  /**
   * Loads the configuration file from disk.
   */
  abstract load(): Promise<void>;

  /**
   * Saves the configuration file to disk.
   */
  abstract save(): Promise<void>;

  /**
   * Gets a configuration value by key.
   */
  get(key: string): any {
    return this.getNestedValue(this._data, key);
  }

  /**
   * Sets a configuration value by key.
   */
  set(key: string, value: ConfigSetting): void {
    this.setNestedValue(this._data, key, value);
  }

  /**
   * Clears a configuration value by key.
   */
  clear(key: string): void {
    this.deleteNestedValue(this._data, key);
  }

  /**
   * Gets all configuration values.
   */
  getAll(): Record<string, any> {
    return { ...this._data };
  }

  /**
   * Checks if the file exists.
   */
  async exists(): Promise<boolean> {
    return fs.pathExists(this._fileName);
  }

  /**
   * Creates a ConfigFile instance from a file path.
   */
  static async fromFile(fileName: string, scope: ConfigFileScope): Promise<ConfigFile> {
    const ext = path.extname(fileName).toLowerCase();
    let configFile: ConfigFile;

    if (ext === '.yml' || ext === '.yaml') {
      configFile = new YamlConfigFile(fileName, scope);
    } else if (ext === '.ini') {
      configFile = new IniConfigFile(fileName, scope);
    } else {
      // Default to YAML
      configFile = new YamlConfigFile(fileName, scope);
    }

    if (await configFile.exists()) {
      await configFile.load();
    }

    return configFile;
  }

  /**
   * Gets a nested value from an object using dot notation.
   */
  protected getNestedValue(obj: any, key: string): any {
    return key.split('.').reduce((current, prop) => {
      return current && current[prop] !== undefined ? current[prop] : undefined;
    }, obj);
  }

  /**
   * Sets a nested value in an object using dot notation.
   */
  protected setNestedValue(obj: any, key: string, value: any): void {
    const keys = key.split('.');
    const lastKey = keys.pop()!;
    const target = keys.reduce((current, prop) => {
      if (!current[prop] || typeof current[prop] !== 'object') {
        current[prop] = {};
      }
      return current[prop];
    }, obj);
    target[lastKey] = value;
  }

  /**
   * Deletes a nested value from an object using dot notation.
   */
  protected deleteNestedValue(obj: any, key: string): void {
    const keys = key.split('.');
    const lastKey = keys.pop()!;
    const target = keys.reduce((current, prop) => {
      return current && current[prop] ? current[prop] : null;
    }, obj);
    
    if (target && target[lastKey] !== undefined) {
      delete target[lastKey];
    }
  }
}

/**
 * YAML configuration file implementation.
 */
export class YamlConfigFile extends ConfigFile {
  async load(): Promise<void> {
    try {
      const content = await fs.readFile(this._fileName, 'utf8');
      this._data = yaml.load(content) as Record<string, any> || {};
    } catch (error) {
      // If file doesn't exist or is invalid, start with empty data
      this._data = {};
    }
  }

  async save(): Promise<void> {
    const content = yaml.dump(this._data, {
      indent: 2,
      lineWidth: -1,
      noRefs: true,
    });
    
    await fs.ensureDir(path.dirname(this._fileName));
    await fs.writeFile(this._fileName, content, 'utf8');
  }
}

/**
 * INI configuration file implementation (basic).
 */
export class IniConfigFile extends ConfigFile {
  async load(): Promise<void> {
    try {
      const content = await fs.readFile(this._fileName, 'utf8');
      this._data = this.parseIni(content);
    } catch (error) {
      this._data = {};
    }
  }

  async save(): Promise<void> {
    const content = this.stringifyIni(this._data);
    await fs.ensureDir(path.dirname(this._fileName));
    await fs.writeFile(this._fileName, content, 'utf8');
  }

  private parseIni(content: string): Record<string, any> {
    const lines = content.split('\n');
    const result: Record<string, any> = {};
    let currentSection = '';

    for (const line of lines) {
      const trimmed = line.trim();
      if (!trimmed || trimmed.startsWith('#') || trimmed.startsWith(';')) {
        continue;
      }

      if (trimmed.startsWith('[') && trimmed.endsWith(']')) {
        currentSection = trimmed.slice(1, -1);
        if (!result[currentSection]) {
          result[currentSection] = {};
        }
      } else {
        const equalIndex = trimmed.indexOf('=');
        if (equalIndex > 0) {
          const key = trimmed.substring(0, equalIndex).trim();
          const value = trimmed.substring(equalIndex + 1).trim();
          
          if (currentSection) {
            result[currentSection][key] = value;
          } else {
            result[key] = value;
          }
        }
      }
    }

    return result;
  }

  private stringifyIni(data: Record<string, any>): string {
    const lines: string[] = [];

    for (const [key, value] of Object.entries(data)) {
      if (typeof value === 'object' && value !== null) {
        lines.push(`[${key}]`);
        for (const [subKey, subValue] of Object.entries(value)) {
          lines.push(`${subKey}=${subValue}`);
        }
        lines.push('');
      } else {
        lines.push(`${key}=${value}`);
      }
    }

    return lines.join('\n');
  }
}