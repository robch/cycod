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
exports.IniConfigFile = void 0;
const fs = __importStar(require("fs"));
// @ts-ignore
const ini = __importStar(require("ini"));
const ConfigFile_1 = require("./ConfigFile");
class IniConfigFile extends ConfigFile_1.ConfigFile {
    constructor(filePath, scope) {
        super(filePath, scope);
    }
    readSettings(fileName) {
        const result = {};
        if (!fs.existsSync(fileName)) {
            return result;
        }
        try {
            const content = fs.readFileSync(fileName, 'utf-8');
            const parsed = ini.parse(content);
            // Convert flat INI structure to nested object
            for (const [key, value] of Object.entries(parsed)) {
                this.setNestedValue(result, key.split('.'), value);
            }
            return result;
        }
        catch (error) {
            console.error(`Error reading INI configuration file: ${error.message}`);
            return result;
        }
    }
    writeSettings(fileName, settings) {
        try {
            const flatData = this.flattenDictionary(this.settings);
            const iniContent = ini.stringify(flatData);
            fs.writeFileSync(fileName, iniContent);
        }
        catch (error) {
            console.error(`Error writing INI configuration file: ${error.message}`);
        }
    }
    setNestedValue(data, keyParts, value) {
        if (keyParts.length === 1) {
            data[keyParts[0]] = value;
            return;
        }
        if (!data[keyParts[0]]) {
            data[keyParts[0]] = {};
        }
        if (typeof data[keyParts[0]] === 'object') {
            this.setNestedValue(data[keyParts[0]], keyParts.slice(1), value);
        }
        else {
            // Replace non-object value with an object
            const newObj = {};
            data[keyParts[0]] = newObj;
            this.setNestedValue(newObj, keyParts.slice(1), value);
        }
    }
    flattenDictionary(data, prefix = '') {
        const result = {};
        for (const [key, value] of Object.entries(data)) {
            const flatKey = prefix ? `${prefix}.${key}` : key;
            if (typeof value === 'object' && value !== null && !Array.isArray(value)) {
                // Recursively flatten nested objects
                Object.assign(result, this.flattenDictionary(value, flatKey));
            }
            else if (Array.isArray(value)) {
                // Handle array values by joining with comma
                result[flatKey] = value.join(',');
            }
            else {
                result[flatKey] = value;
            }
        }
        return result;
    }
}
exports.IniConfigFile = IniConfigFile;
//# sourceMappingURL=IniConfigFile.js.map