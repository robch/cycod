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
exports.IniConfigFile = exports.YamlConfigFile = exports.ConfigFile = void 0;
const yaml = __importStar(require("js-yaml"));
const fs = __importStar(require("fs-extra"));
const path = __importStar(require("path"));
/**
 * Abstract base class for configuration files.
 */
class ConfigFile {
    constructor(fileName, scope) {
        this._data = {};
        this._fileName = fileName;
        this._scope = scope;
    }
    get fileName() {
        return this._fileName;
    }
    get scope() {
        return this._scope;
    }
    /**
     * Gets a configuration value by key.
     */
    get(key) {
        return this.getNestedValue(this._data, key);
    }
    /**
     * Sets a configuration value by key.
     */
    set(key, value) {
        this.setNestedValue(this._data, key, value);
    }
    /**
     * Clears a configuration value by key.
     */
    clear(key) {
        this.deleteNestedValue(this._data, key);
    }
    /**
     * Gets all configuration values.
     */
    getAll() {
        return { ...this._data };
    }
    /**
     * Checks if the file exists.
     */
    async exists() {
        return fs.pathExists(this._fileName);
    }
    /**
     * Creates a ConfigFile instance from a file path.
     */
    static async fromFile(fileName, scope) {
        const ext = path.extname(fileName).toLowerCase();
        let configFile;
        if (ext === '.yml' || ext === '.yaml') {
            configFile = new YamlConfigFile(fileName, scope);
        }
        else if (ext === '.ini') {
            configFile = new IniConfigFile(fileName, scope);
        }
        else {
            // Default to YAML
            configFile = new YamlConfigFile(fileName, scope);
        }
        if (await configFile.exists()) {
            await configFile.load();
        }
        return configFile;
    }
    /**
     * Gets a nested value from an object using dot notation.
     */
    getNestedValue(obj, key) {
        return key.split('.').reduce((current, prop) => {
            return current && current[prop] !== undefined ? current[prop] : undefined;
        }, obj);
    }
    /**
     * Sets a nested value in an object using dot notation.
     */
    setNestedValue(obj, key, value) {
        const keys = key.split('.');
        const lastKey = keys.pop();
        const target = keys.reduce((current, prop) => {
            if (!current[prop] || typeof current[prop] !== 'object') {
                current[prop] = {};
            }
            return current[prop];
        }, obj);
        target[lastKey] = value;
    }
    /**
     * Deletes a nested value from an object using dot notation.
     */
    deleteNestedValue(obj, key) {
        const keys = key.split('.');
        const lastKey = keys.pop();
        const target = keys.reduce((current, prop) => {
            return current && current[prop] ? current[prop] : null;
        }, obj);
        if (target && target[lastKey] !== undefined) {
            delete target[lastKey];
        }
    }
}
exports.ConfigFile = ConfigFile;
/**
 * YAML configuration file implementation.
 */
class YamlConfigFile extends ConfigFile {
    async load() {
        try {
            const content = await fs.readFile(this._fileName, 'utf8');
            this._data = yaml.load(content) || {};
        }
        catch (error) {
            // If file doesn't exist or is invalid, start with empty data
            this._data = {};
        }
    }
    async save() {
        const content = yaml.dump(this._data, {
            indent: 2,
            lineWidth: -1,
            noRefs: true,
        });
        await fs.ensureDir(path.dirname(this._fileName));
        await fs.writeFile(this._fileName, content, 'utf8');
    }
}
exports.YamlConfigFile = YamlConfigFile;
/**
 * INI configuration file implementation (basic).
 */
class IniConfigFile extends ConfigFile {
    async load() {
        try {
            const content = await fs.readFile(this._fileName, 'utf8');
            this._data = this.parseIni(content);
        }
        catch (error) {
            this._data = {};
        }
    }
    async save() {
        const content = this.stringifyIni(this._data);
        await fs.ensureDir(path.dirname(this._fileName));
        await fs.writeFile(this._fileName, content, 'utf8');
    }
    parseIni(content) {
        const lines = content.split('\n');
        const result = {};
        let currentSection = '';
        for (const line of lines) {
            const trimmed = line.trim();
            if (!trimmed || trimmed.startsWith('#') || trimmed.startsWith(';')) {
                continue;
            }
            if (trimmed.startsWith('[') && trimmed.endsWith(']')) {
                currentSection = trimmed.slice(1, -1);
                if (!result[currentSection]) {
                    result[currentSection] = {};
                }
            }
            else {
                const equalIndex = trimmed.indexOf('=');
                if (equalIndex > 0) {
                    const key = trimmed.substring(0, equalIndex).trim();
                    const value = trimmed.substring(equalIndex + 1).trim();
                    if (currentSection) {
                        result[currentSection][key] = value;
                    }
                    else {
                        result[key] = value;
                    }
                }
            }
        }
        return result;
    }
    stringifyIni(data) {
        const lines = [];
        for (const [key, value] of Object.entries(data)) {
            if (typeof value === 'object' && value !== null) {
                lines.push(`[${key}]`);
                for (const [subKey, subValue] of Object.entries(value)) {
                    lines.push(`${subKey}=${subValue}`);
                }
                lines.push('');
            }
            else {
                lines.push(`${key}=${value}`);
            }
        }
        return lines.join('\n');
    }
}
exports.IniConfigFile = IniConfigFile;
//# sourceMappingURL=ConfigFile.js.map