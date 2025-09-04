import * as fs from 'fs';
import * as yaml from 'js-yaml';
import { ConfigFile } from './ConfigFile';
import { ConfigFileScope } from './ConfigFileScope';

export class YamlConfigFile extends ConfigFile {
    public constructor(filePath: string, scope: ConfigFileScope) {
        super(filePath, scope);
    }

    protected ReadSettings(fileName: string): Map<string, any> {
        if (!fs.existsSync(fileName)) return new Map<string, any>();

        try {
            const yamlContent = fs.readFileSync(fileName, 'utf-8');
            const result = yaml.load(yamlContent) as any;
            
            if (result && typeof result === 'object') {
                return this.NormalizeToMap(result);
            }
            
            return new Map<string, any>();
        } catch (ex: any) {
            console.error(`Error reading YAML configuration file: ${ex.message}`);
            return new Map<string, any>();
        }
    }

    protected WriteSettings(fileName: string, settings?: Map<string, any> | null): void {
        try {
            const obj = this.MapToObject(this.Settings);
            const yamlContent = yaml.dump(obj, { 
                indent: 2,
                lineWidth: 120,
                noRefs: true
            });
            fs.writeFileSync(fileName, yamlContent);
        } catch (ex: any) {
            console.error(`Error writing YAML configuration file: ${ex.message}`);
        }
    }

    private NormalizeToMap(data: any): Map<string, any> {
        const result = new Map<string, any>();

        if (data && typeof data === 'object' && !Array.isArray(data)) {
            for (const [key, value] of Object.entries(data)) {
                if (value && typeof value === 'object' && !Array.isArray(value)) {
                    // Recursively normalize nested objects
                    result.set(key, this.NormalizeToMap(value));
                } else {
                    result.set(key, value);
                }
            }
        }

        return result;
    }

    private MapToObject(map: Map<string, any>): any {
        const result: any = {};
        
        for (const [key, value] of map) {
            if (value instanceof Map) {
                result[key] = this.MapToObject(value);
            } else {
                result[key] = value;
            }
        }
        
        return result;
    }
}