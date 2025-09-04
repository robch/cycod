import { INamedValues } from './INamedValues';
import { ConfigStore } from '../Configuration/ConfigStore';
import { ConfigSource } from '../Configuration/ConfigSource';
import * as os from 'os';

/**
 * A simple implementation of INamedValues that stores values in a dictionary
 */
export class TemplateVariables implements INamedValues {
    private readonly _variables: Map<string, string>;

    /**
     * Creates a new TemplateVariables instance with an empty dictionary
     */
    public constructor();
    /**
     * Creates a new TemplateVariables instance with the provided dictionary
     * @param variables Initial variables to include
     */
    public constructor(variables: Map<string, string>);
    public constructor(variables?: Map<string, string>) {
        this._variables = new Map<string, string>();
        if (variables) {
            for (const [key, value] of variables) {
                this._variables.set(key.toLowerCase(), value);
            }
        }
    }
    
    /**
     * Gets a value by name, or an empty string if not found
     * @param name The name of the value to retrieve
     * @returns The value, or an empty string if not found
     */
    public Get(name: string): string | null {
        const lowerName = name.toLowerCase();
        
        // First check the variable dictionary
        if (this._variables.has(lowerName)) {
            return this._variables.get(lowerName)!;
        }
        
        // Then check configuration
        const configValue = ConfigStore.Instance.GetFromAnyScope(name);
        if (configValue.Source !== ConfigSource.NotFound) {
            return configValue.AsString();
        }
        
        // Then check environment variables
        const envValue = process.env[name];
        if (envValue != null) {
            return envValue;
        }
        
        // Finally check special variables
        const builtInValue = TemplateVariables.GetBuiltInVariable(name);
        if (builtInValue != null) {
            return builtInValue;
        }

        return null;
    }

    /**
     * Sets a value by name
     * @param name The name of the value to set
     * @param value The value to set
     */
    public Set(name: string, value: string): void {
        this._variables.set(name.toLowerCase(), value);
    }

    /**
     * Checks if a value exists
     * @param name The name of the value to check
     * @returns True if the value exists, false otherwise
     */
    public Contains(name: string): boolean {
        const lowerName = name.toLowerCase();
        return this._variables.has(lowerName) || 
               ConfigStore.Instance.GetFromAnyScope(name).Value != null ||
               process.env[name] != null ||
               TemplateVariables.GetBuiltInVariable(name) != null;
    }

    /**
     * Gets a built-in variable value by name
     * @param name The variable name
     * @returns The built-in variable value or null if not found
     */
    private static GetBuiltInVariable(name: string): string | null {
        const now = new Date();
        
        switch (name.toLowerCase()) {
            case "os":
                return `${os.type()} ${os.release()}`;
            case "osname":
                return os.platform();
            case "osversion":
                return os.release();
            case "date":
                return now.toISOString().split('T')[0]; // yyyy-MM-dd
            case "time":
                return now.toTimeString().split(' ')[0]; // HH:mm:ss
            case "datetime":
                return now.toISOString().replace('T', ' ').split('.')[0]; // yyyy-MM-dd HH:mm:ss
            case "year":
                return now.getFullYear().toString();
            case "month":
                return (now.getMonth() + 1).toString();
            case "day":
                return now.getDate().toString();
            case "hour":
                return now.getHours().toString();
            case "minute":
                return now.getMinutes().toString();
            case "second":
                return now.getSeconds().toString();
            case "random":
                return Math.floor(Math.random() * 1000).toString();
        }
        
        return null;
    }
}