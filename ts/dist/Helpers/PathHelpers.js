"use strict";
var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    var desc = Object.getOwnPropertyDescriptor(m, k);
    if (!desc || ("get" in desc ? !m.__esModule : desc.writable || desc.configurable)) {
      desc = { enumerable: true, get: function() { return m[k]; } };
    }
    Object.defineProperty(o, k2, desc);
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || (function () {
    var ownKeys = function(o) {
        ownKeys = Object.getOwnPropertyNames || function (o) {
            var ar = [];
            for (var k in o) if (Object.prototype.hasOwnProperty.call(o, k)) ar[ar.length] = k;
            return ar;
        };
        return ownKeys(o);
    };
    return function (mod) {
        if (mod && mod.__esModule) return mod;
        var result = {};
        if (mod != null) for (var k = ownKeys(mod), i = 0; i < k.length; i++) if (k[i] !== "default") __createBinding(result, mod, k[i]);
        __setModuleDefault(result, mod);
        return result;
    };
})();
Object.defineProperty(exports, "__esModule", { value: true });
exports.PathHelpers = void 0;
const path = __importStar(require("path"));
const os = __importStar(require("os"));
const fs = __importStar(require("fs-extra"));
/**
 * Helper functions for working with file paths.
 */
class PathHelpers {
    /**
     * Gets the user's home directory.
     */
    static getHomeDirectory() {
        return os.homedir();
    }
    /**
     * Gets the global configuration directory.
     */
    static getGlobalConfigDirectory() {
        if (process.platform === 'win32') {
            const commonAppData = process.env.ALLUSERSPROFILE || 'C:\\ProgramData';
            return path.join(commonAppData, '.cycod');
        }
        else {
            // On Linux/Mac, use XDG_DATA_HOME or ~/.local/share
            const xdgDataHome = process.env.XDG_DATA_HOME;
            if (xdgDataHome) {
                return path.join(xdgDataHome, '.cycod');
            }
            else {
                const homeDir = this.getHomeDirectory();
                return path.join(homeDir, '.local', 'share', '.cycod');
            }
        }
    }
    /**
     * Gets the user configuration directory.
     */
    static getUserConfigDirectory() {
        const home = this.getHomeDirectory();
        return path.join(home, '.cycod');
    }
    /**
     * Gets the local configuration directory (current working directory).
     */
    static getLocalConfigDirectory() {
        return path.join(process.cwd(), '.cycod');
    }
    /**
     * Gets the configuration file path for a given scope.
     */
    static getConfigFilePath(scope) {
        let configDir;
        switch (scope) {
            case 'global':
                configDir = this.getGlobalConfigDirectory();
                break;
            case 'user':
                configDir = this.getUserConfigDirectory();
                break;
            case 'local':
                configDir = this.getLocalConfigDirectory();
                break;
            default:
                throw new Error(`Invalid scope: ${scope}`);
        }
        return path.join(configDir, 'config.yaml');
    }
    /**
     * Ensures that a directory exists.
     */
    static async ensureDirectory(dirPath) {
        await fs.ensureDir(dirPath);
    }
    /**
     * Expands a path with tilde (~) to the full home directory path.
     */
    static expandPath(filePath) {
        if (filePath.startsWith('~/')) {
            return path.join(this.getHomeDirectory(), filePath.slice(2));
        }
        return filePath;
    }
    /**
     * Normalizes a file path.
     */
    static normalizePath(filePath) {
        return path.normalize(this.expandPath(filePath));
    }
    /**
     * Checks if a path is absolute.
     */
    static isAbsolute(filePath) {
        return path.isAbsolute(filePath);
    }
    /**
     * Resolves a path relative to the current working directory.
     */
    static resolve(filePath) {
        return path.resolve(this.expandPath(filePath));
    }
}
exports.PathHelpers = PathHelpers;
//# sourceMappingURL=PathHelpers.js.map