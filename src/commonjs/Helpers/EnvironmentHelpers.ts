import * as fs from 'fs';
import * as path from 'path';
import { ConsoleHelpers } from './ConsoleHelpers';

/**
 * Helper class for working with environment variables and .env files.
 */
export class EnvironmentHelpers {
    /**
     * Finds an environment variable value from process.env, .env files, or config store.
     * @param variable The name of the environment variable to find
     * @param searchDotEnvFile Whether to search for .env files in the directory hierarchy
     * @returns The environment variable value, or null if not found
     */
    public static FindEnvVar(variable: string, searchDotEnvFile: boolean = false): string | null {
        ConsoleHelpers.WriteDebugLine(`FindEnvVar: ${variable}`);

        // First check process environment variables
        const value = process.env[variable];
        if (value && value.trim() !== '') {
            ConsoleHelpers.WriteDebugLine(`Found ${variable} value: ${value}`);
            return value;
        }

        // Search for .env files if requested
        if (searchDotEnvFile) {
            let currentDirectory: string | null = process.cwd();
            
            while (currentDirectory) {
                const envFilePath = path.join(currentDirectory, '.env');
                
                if (fs.existsSync(envFilePath)) {
                    try {
                        const content = fs.readFileSync(envFilePath, 'utf8');
                        const lines = content.split('\n');
                        
                        for (const line of lines) {
                            const trimmedLine = line.trim();
                            if (!trimmedLine || trimmedLine.startsWith('#')) continue;
                            
                            const parts = trimmedLine.split('=', 2);
                            if (parts.length === 2 && parts[0].trim() === variable) {
                                let envValue = parts[1].trim();
                                
                                // Remove quotes if present
                                if ((envValue.startsWith('"') && envValue.endsWith('"')) ||
                                    (envValue.startsWith("'") && envValue.endsWith("'"))) {
                                    envValue = envValue.slice(1, -1);
                                }
                                
                                return envValue;
                            }
                        }
                    } catch (error) {
                        // Continue searching if file can't be read
                        ConsoleHelpers.WriteDebugLine(`Could not read .env file: ${envFilePath}`);
                    }
                }

                // Move to parent directory
                const parentDirectory = path.dirname(currentDirectory);
                currentDirectory = parentDirectory === currentDirectory ? null : parentDirectory;
            }
        }

        // TODO: Add ConfigStore lookup when available
        // For now, this would need to be implemented based on the actual ConfigStore implementation
        // const configStore = ConfigStore.Instance;
        // const configValue = configStore.GetFromAnyScope(variable);
        // if (!configValue.IsNotFoundNullOrEmpty()) return configValue.AsString();

        return null;
    }
}