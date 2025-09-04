import { ConfigFileScope } from '../Configuration/ConfigFileScope';
import { ScopeFileHelpers } from '../Configuration/ScopeFileHelpers';
import { ConsoleHelpers } from './ConsoleHelpers';
import { ConfigStore } from '../Configuration/ConfigStore';

export class CommandLineException extends Error {
    constructor(message: string) {
        super(message);
        this.name = 'CommandLineException';
    }
}

export class ProfileFileHelpers {
    /**
     * Loads a profile by name into the configuration store.
     * @param profileName The name of the profile to load
     * @throws CommandLineException Thrown when profile name is empty or profile not found
     */
    public static loadProfile(profileName: string): void {
        if (!profileName || profileName.length === 0) {
            throw new CommandLineException('Profile name cannot be empty.');
        }
        
        const profilePath = ProfileFileHelpers.findProfileFile(profileName);
        if (profilePath === null) {
            throw new CommandLineException(`Profile '${profileName}' not found in any scope or parent directories.`);
        }
        
        ConsoleHelpers.writeDebugLine(`Loading profile from ${profilePath}`);
        ConfigStore.instance.loadConfigFile(profilePath);
    }
    
    /**
     * Finds a profile file by name across all scopes and optionally parent directories.
     * @param profileName The name of the profile to find
     * @returns The path to the profile file if found, null otherwise
     */
    public static findProfileFile(profileName: string): string | null {
        const yamlFileName = `${profileName}.yaml`;
        const yamlProfilePath = ScopeFileHelpers.findFileInAnyScope(yamlFileName, 'profiles', true);
        if (yamlProfilePath !== null) {
            return yamlProfilePath;
        }

        const bareFileNameNoExt = `${profileName}`;
        return ScopeFileHelpers.findFileInAnyScope(bareFileNameNoExt, 'profiles', true);
    }
    
    /**
     * Finds the profiles directory in the specified scope.
     * @param scope The scope to search in
     * @param create Whether to create the directory if it doesn't exist
     * @returns The path to the profiles directory, or null if not found
     */
    public static findProfilesDirectoryInScope(scope: ConfigFileScope, create: boolean = false): string | null {
        return create
            ? ScopeFileHelpers.ensureDirectoryInScope('profiles', scope)
            : ScopeFileHelpers.findDirectoryInScope('profiles', scope);
    }
}