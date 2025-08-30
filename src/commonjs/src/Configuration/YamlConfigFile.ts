import * as fs from 'fs';
import * as yaml from 'js-yaml';
import { ConfigFile } from './ConfigFile';
import { ConfigFileScope } from './ConfigFileScope';

export class YamlConfigFile extends ConfigFile {
    constructor(filePath: string, scope: ConfigFileScope) {
        super(filePath, scope);
    }

    protected readSettings(fileName: string): Record<string, any> {
        if (!fs.existsSync(fileName)) {
            return {};
        }

        try {
            const yamlContent = fs.readFileSync(fileName, 'utf-8');
            const result = yaml.load(yamlContent) as Record<string, any> || {};
            return this.normalizeNestedDictionaries(result);
        } catch (error) {
            console.error(`Error reading YAML configuration file: ${(error as Error).message}`);
            return {};
        }
    }

    protected writeSettings(fileName: string, settings?: Record<string, any>): void {
        try {
            const yamlContent = yaml.dump(this.settings, {
                indent: 2,
                lineWidth: -1,
                noRefs: true
            });
            fs.writeFileSync(fileName, yamlContent);
        } catch (error) {
            console.error(`Error writing YAML configuration file: ${(error as Error).message}`);
        }
    }

    private normalizeNestedDictionaries(data: Record<string, any>): Record<string, any> {
        const result: Record<string, any> = {};

        for (const [key, value] of Object.entries(data)) {
            if (typeof value === 'object' && value !== null && !Array.isArray(value)) {
                // Recursively normalize nested objects
                result[key] = this.normalizeNestedDictionaries(value);
            } else {
                result[key] = value ?? '';
            }
        }

        return result;
    }
}