import * as fs from 'fs';
import * as path from 'path';
import * as os from 'os';
import { ConfigFileScope } from './ConfigFileScope';

/**
 * Provides methods for determining configuration file locations.
 */
export class ConfigFileHelpers {
    public static GetConfigFileName(scope: ConfigFileScope): string | null {
        const foundPath = ConfigFileHelpers.FindConfigFile(scope);
        if (foundPath != null) return foundPath;

        return ConfigFileHelpers.GetYamlConfigFileName(scope);
    }

    public static FindConfigFile(scope: ConfigFileScope, forceCreate: boolean = false): string | null {
        const yamlPath = ConfigFileHelpers.GetYamlConfigFileName(scope);
        if (yamlPath && fs.existsSync(yamlPath)) {
            console.debug(`Found YAML config file at: ${yamlPath}`);
            return yamlPath;
        }

        const iniPath = ConfigFileHelpers.GetIniConfigFileName(scope);
        if (iniPath && fs.existsSync(iniPath)) {
            console.debug(`Found INI config file at: ${iniPath}`);
            return iniPath;
        }

        if (forceCreate && yamlPath) {
            fs.writeFileSync(yamlPath, '');
            return yamlPath;
        }

        return null;
    }
    
    public static GetScopeDirectoryPath(scope: ConfigFileScope): string | null {
        switch (scope) {
            case ConfigFileScope.Global:
                return ConfigFileHelpers.GetGlobalScopeDirectory();
            case ConfigFileScope.User:
                return ConfigFileHelpers.GetUserScopeDirectory();
            case ConfigFileScope.Local:
                return ConfigFileHelpers.GetLocalScopeDirectory();
            default:
                return null;
        }
    }
    
    public static GetLocationDisplayName(scope: ConfigFileScope): string | null {
        const configPath = ConfigFileHelpers.FindConfigFile(scope);
        if (configPath != null) {
            return `${configPath} (${ConfigFileScope[scope].toLowerCase()})`;
        }
        
        const directory = ConfigFileHelpers.GetScopeDirectoryPath(scope);
        if (!directory) return null;

        const defaultPath = path.join(directory, ConfigFileHelpers.YAML_CONFIG_NAME);
        return `${defaultPath} (${ConfigFileScope[scope].toLowerCase()})`;
    }
    
    public static GetGlobalScopeDirectory(): string {
        const parent = ConfigFileHelpers.GetGlobalScopeParentDirectory();
        const configDir = path.join(parent, ConfigFileHelpers.CONFIG_DIR_NAME);
        console.debug(`Global config directory: ${configDir}`);
        return configDir;
    }

    public static GetUserScopeDirectory(): string {
        const parent = os.homedir();
        return path.join(parent, ConfigFileHelpers.CONFIG_DIR_NAME);
    }

    public static GetLocalScopeDirectory(): string {
        const existingYamlFile = ConfigFileHelpers.FindFileSearchParents(ConfigFileHelpers.CONFIG_DIR_NAME, ConfigFileHelpers.YAML_CONFIG_NAME);
        if (existingYamlFile != null) return path.dirname(existingYamlFile);

        const existingIniFile = ConfigFileHelpers.FindFileSearchParents(ConfigFileHelpers.CONFIG_DIR_NAME, ConfigFileHelpers.INI_CONFIG_NAME);
        if (existingIniFile != null) return path.dirname(existingIniFile);

        return path.join(process.cwd(), ConfigFileHelpers.CONFIG_DIR_NAME);
    }

    private static GetGlobalScopeParentDirectory(): string {
        const isLinuxOrMac = process.platform === 'linux' || process.platform === 'darwin';
        if (isLinuxOrMac) {
            const xdgDataHome = process.env.XDG_DATA_HOME;
            if (xdgDataHome) {
                console.debug(`On Linux/Mac... using XDG_DATA_HOME: ${xdgDataHome}`);
                return xdgDataHome;
            } else {
                const homeDir = os.homedir();
                const homeLocalShareDir = path.join(homeDir, '.local', 'share');
                console.debug(`On Linux/Mac... using HOME/.local/share: ${homeLocalShareDir}`);
                return homeLocalShareDir;
            }
        }

        // Windows - use ALLUSERSPROFILE or fallback
        const commonAppData = process.env.ALLUSERSPROFILE || 'C:\\ProgramData';
        console.debug(`On Windows... using CommonApplicationData: ${commonAppData}`);
        return commonAppData;
    }

    private static GetYamlConfigFileName(scope: ConfigFileScope): string | null {
        const dirPath = ConfigFileHelpers.GetScopeDirectoryPath(scope);
        return dirPath != null
            ? path.join(dirPath, ConfigFileHelpers.YAML_CONFIG_NAME)
            : null;
    }

    private static GetIniConfigFileName(scope: ConfigFileScope): string | null {
        const dirPath = ConfigFileHelpers.GetScopeDirectoryPath(scope);
        return dirPath != null
            ? path.join(dirPath, ConfigFileHelpers.INI_CONFIG_NAME)
            : null;
    }

    private static readonly CONFIG_DIR_NAME = '.cycod'; // ProgramInfo.ConfigDirName equivalent
    private static readonly YAML_CONFIG_NAME = 'config.yaml';
    private static readonly INI_CONFIG_NAME = 'config';

    private static FindFileSearchParents(dirName: string, fileName: string): string | null {
        let currentDir = process.cwd();
        
        while (currentDir !== path.dirname(currentDir)) {
            const configDir = path.join(currentDir, dirName);
            const configFile = path.join(configDir, fileName);
            
            if (fs.existsSync(configFile)) {
                return configFile;
            }
            
            currentDir = path.dirname(currentDir);
        }
        
        return null;
    }
}