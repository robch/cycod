import * as fs from 'fs-extra';
import * as path from 'path';
import * as yaml from 'js-yaml';
import * as os from 'os';
import { ConfigFileScope, ConfigValue } from '../types';

export class ConfigStore {
  private static readonly CONFIG_DIR_NAME = '.cycod';
  private static readonly CONFIG_FILE_NAME = 'config.yaml';

  private getConfigPath(scope: ConfigFileScope): string {
    switch (scope) {
      case ConfigFileScope.Local:
        return path.join(process.cwd(), ConfigStore.CONFIG_DIR_NAME, ConfigStore.CONFIG_FILE_NAME);
      case ConfigFileScope.User:
        return path.join(os.homedir(), ConfigStore.CONFIG_DIR_NAME, ConfigStore.CONFIG_FILE_NAME);
      case ConfigFileScope.Global:
        // For now, use a system-wide location. In production, this might be /etc/cycod or similar
        return path.join('/tmp', 'cycod-global', ConfigStore.CONFIG_FILE_NAME);
      default:
        throw new Error(`Invalid config scope: ${scope}`);
    }
  }

  private async ensureConfigDir(scope: ConfigFileScope): Promise<void> {
    const configPath = this.getConfigPath(scope);
    const configDir = path.dirname(configPath);
    await fs.ensureDir(configDir);
  }

  private async readConfigFile(scope: ConfigFileScope): Promise<any> {
    const configPath = this.getConfigPath(scope);
    
    try {
      if (await fs.pathExists(configPath)) {
        const content = await fs.readFile(configPath, 'utf8');
        return yaml.load(content) || {};
      }
      return {};
    } catch (error) {
      return {};
    }
  }

  private async writeConfigFile(scope: ConfigFileScope, data: any): Promise<void> {
    await this.ensureConfigDir(scope);
    const configPath = this.getConfigPath(scope);
    const yamlContent = yaml.dump(data, { 
      indent: 2,
      lineWidth: -1,
      noRefs: true
    });
    await fs.writeFile(configPath, yamlContent, 'utf8');
  }

  private setNestedValue(obj: any, keyPath: string, value: any): void {
    const keys = keyPath.split('.');
    let current = obj;

    for (let i = 0; i < keys.length - 1; i++) {
      const key = keys[i];
      if (!(key in current) || typeof current[key] !== 'object' || Array.isArray(current[key])) {
        current[key] = {};
      }
      current = current[key];
    }

    current[keys[keys.length - 1]] = value;
  }

  private getNestedValue(obj: any, keyPath: string): any {
    const keys = keyPath.split('.');
    let current = obj;

    for (const key of keys) {
      if (current == null || typeof current !== 'object' || !(key in current)) {
        return undefined;
      }
      current = current[key];
    }

    return current;
  }

  private deleteNestedValue(obj: any, keyPath: string): boolean {
    const keys = keyPath.split('.');
    let current = obj;
    const parents: any[] = [];

    // Navigate to the parent of the target key
    for (let i = 0; i < keys.length - 1; i++) {
      const key = keys[i];
      if (current == null || typeof current !== 'object' || !(key in current)) {
        return false;
      }
      parents.push({ obj: current, key });
      current = current[key];
    }

    const finalKey = keys[keys.length - 1];
    if (current == null || typeof current !== 'object' || !(finalKey in current)) {
      return false;
    }

    delete current[finalKey];

    // Clean up empty parent objects
    for (let i = parents.length - 1; i >= 0; i--) {
      const parent = parents[i];
      if (Object.keys(parent.obj[parent.key]).length === 0) {
        delete parent.obj[parent.key];
      } else {
        break;
      }
    }

    return true;
  }

  async getFromScope(key: string, scope: ConfigFileScope): Promise<ConfigValue | undefined> {
    // CRITICAL: Match C# behavior - redirect Any scope
    if (scope === ConfigFileScope.Any) {
      return this.getFromAnyScope(key);
    }

    const config = await this.readConfigFile(scope);
    const value = this.getNestedValue(config, key);

    if (value !== undefined) {
      return {
        value,
        scope,
        location: this.getConfigPath(scope)
      };
    }

    return undefined;
  }

  async getFromAnyScope(key: string): Promise<ConfigValue | undefined> {
    // Check in order: Local > User > Global
    const scopes = [ConfigFileScope.Local, ConfigFileScope.User, ConfigFileScope.Global];
    
    for (const scope of scopes) {
      const result = await this.getFromScope(key, scope);
      if (result) {
        return result;
      }
    }

    return undefined;
  }

  async setInScope(key: string, value: any, scope: ConfigFileScope): Promise<void> {
    if (scope === ConfigFileScope.Any) {
      throw new Error('Cannot set value with "any" scope. Use a specific scope.');
    }

    const config = await this.readConfigFile(scope);
    this.setNestedValue(config, key, value);
    await this.writeConfigFile(scope, config);
  }

  async clearFromScope(key: string, scope: ConfigFileScope): Promise<boolean> {
    if (scope === ConfigFileScope.Any) {
      // Clear from all scopes
      let cleared = false;
      const scopes = [ConfigFileScope.Local, ConfigFileScope.User, ConfigFileScope.Global];
      
      for (const s of scopes) {
        if (await this.clearFromScope(key, s)) {
          cleared = true;
        }
      }
      
      return cleared;
    }

    const config = await this.readConfigFile(scope);
    const wasDeleted = this.deleteNestedValue(config, key);
    
    if (wasDeleted) {
      await this.writeConfigFile(scope, config);
    }
    
    return wasDeleted;
  }

  async addToList(key: string, value: string, scope: ConfigFileScope): Promise<void> {
    if (scope === ConfigFileScope.Any) {
      throw new Error('Cannot add to list with "any" scope. Use a specific scope.');
    }

    const config = await this.readConfigFile(scope);
    let current = this.getNestedValue(config, key);

    if (current === undefined) {
      current = [];
    } else if (!Array.isArray(current)) {
      // Convert scalar to array
      current = [current];
    }

    if (!current.includes(value)) {
      current.push(value);
    }

    this.setNestedValue(config, key, current);
    await this.writeConfigFile(scope, config);
  }

  async removeFromList(key: string, value: string, scope: ConfigFileScope): Promise<void> {
    if (scope === ConfigFileScope.Any) {
      throw new Error('Cannot remove from list with "any" scope. Use a specific scope.');
    }

    const config = await this.readConfigFile(scope);
    const current = this.getNestedValue(config, key);

    if (Array.isArray(current)) {
      const index = current.indexOf(value);
      if (index > -1) {
        current.splice(index, 1);
        
        if (current.length === 0) {
          this.deleteNestedValue(config, key);
        } else {
          this.setNestedValue(config, key, current);
        }
        
        await this.writeConfigFile(scope, config);
      }
    }
  }

  async getAllConfigs(): Promise<Array<{ scope: ConfigFileScope; location: string; data: any }>> {
    const results = [];
    const scopes = [ConfigFileScope.Global, ConfigFileScope.User, ConfigFileScope.Local];

    for (const scope of scopes) {
      const data = await this.readConfigFile(scope);
      results.push({
        scope,
        location: this.getConfigPath(scope),
        data
      });
    }

    return results;
  }
}