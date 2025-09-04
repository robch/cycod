/**
 * Abstract class for managing program information and metadata.
 * Provides access to program name, description, config directory name, and assembly details.
 */
export abstract class ProgramInfo {
    /**
     * Gets the program name.
     */
    public static get Name(): string {
        return ProgramInfo._getName();
    }

    /**
     * Gets the program description.
     */
    public static get Description(): string {
        return ProgramInfo._getDescription();
    }

    /**
     * Gets the configuration directory name.
     */
    public static get ConfigDirName(): string {
        return ProgramInfo._getConfigDirName();
    }

    /**
     * Gets the assembly information (package.json equivalent in Node.js).
     */
    public static get Assembly(): any {
        return ProgramInfo._getAssembly();
    }

    /**
     * Protected constructor to initialize program information.
     * @param name Function that returns the program name
     * @param description Function that returns the program description
     * @param configDirName Function that returns the config directory name
     * @param assembly Function that returns assembly/package information
     */
    protected constructor(
        name: () => string,
        description: () => string,
        configDirName: () => string,
        assembly: () => any
    ) {
        if (!name || !description || !configDirName || !assembly) {
            throw new Error("Name, Description, ConfigDir, and Assembly cannot be null.");
        }

        ProgramInfo.Initialize(name, description, configDirName, assembly);
    }

    /**
     * Initializes the static program information.
     * @param name Function that returns the program name
     * @param description Function that returns the program description
     * @param configDirName Function that returns the config directory name
     * @param assembly Function that returns assembly/package information
     */
    private static Initialize(
        name: () => string,
        description: () => string,
        configDirName: () => string,
        assembly: () => any
    ): void {
        if (ProgramInfo._getName || ProgramInfo._getDescription || ProgramInfo._getConfigDirName || ProgramInfo._getAssembly) {
            throw new Error("ProgramInfo is already initialized.");
        }

        ProgramInfo._getName = name;
        ProgramInfo._getDescription = description;
        ProgramInfo._getAssembly = assembly;
        ProgramInfo._getConfigDirName = configDirName;
    }

    private static _getName: () => string;
    private static _getDescription: () => string;
    private static _getAssembly: () => any;
    private static _getConfigDirName: () => string;
}
