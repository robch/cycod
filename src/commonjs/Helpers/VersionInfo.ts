import { ProgramInfo } from './ProgramInfo';

/**
 * Helper class to provide version information from the package.json.
 */
export class VersionInfo {
    /**
     * Gets the application version.
     * @returns The version string, optionally stripped of commit hash for release builds
     */
    public static GetVersion(): string {
        const version = VersionInfo.GetAssemblyVersion();
        return VersionInfo.IsReleaseBuild() ? VersionInfo.StripCommitHash(version) : version;
    }

    /**
     * Gets the assembly version from package.json or default.
     * @returns The version string
     */
    private static GetAssemblyVersion(): string {
        const assembly = ProgramInfo.Assembly;
        
        // Try to get version from package.json
        if (assembly && typeof assembly === 'object') {
            if (assembly.version) {
                return assembly.version;
            }
        }
        
        return VersionInfo.DefaultVersion;
    }

    /**
     * Determines if this is a release build based on NODE_ENV.
     * @returns True if this is a release build
     */
    private static IsReleaseBuild(): boolean {
        const nodeEnv = process.env.NODE_ENV;
        return nodeEnv === 'production' || nodeEnv === 'release';
    }

    /**
     * Strips the commit hash from a version string (removes everything after '+').
     * @param version The version string to process
     * @returns The version string without commit hash
     */
    private static StripCommitHash(version: string): string {
        const plusIndex = version.indexOf('+');
        if (plusIndex > 0) {
            return version.substring(0, plusIndex);
        }
        return version;
    }

    private static readonly DefaultVersion = "0.0.1";
}
