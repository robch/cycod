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
exports.YamlConfigFile = void 0;
const fs = __importStar(require("fs"));
const yaml = __importStar(require("js-yaml"));
const ConfigFile_1 = require("./ConfigFile");
class YamlConfigFile extends ConfigFile_1.ConfigFile {
    constructor(filePath, scope) {
        super(filePath, scope);
    }
    readSettings(fileName) {
        if (!fs.existsSync(fileName)) {
            return {};
        }
        try {
            const yamlContent = fs.readFileSync(fileName, 'utf-8');
            const result = yaml.load(yamlContent) || {};
            return this.normalizeNestedDictionaries(result);
        }
        catch (error) {
            console.error(`Error reading YAML configuration file: ${error.message}`);
            return {};
        }
    }
    writeSettings(fileName, settings) {
        try {
            const yamlContent = yaml.dump(this.settings, {
                indent: 2,
                lineWidth: -1,
                noRefs: true
            });
            fs.writeFileSync(fileName, yamlContent);
        }
        catch (error) {
            console.error(`Error writing YAML configuration file: ${error.message}`);
        }
    }
    normalizeNestedDictionaries(data) {
        const result = {};
        for (const [key, value] of Object.entries(data)) {
            if (typeof value === 'object' && value !== null && !Array.isArray(value)) {
                // Recursively normalize nested objects
                result[key] = this.normalizeNestedDictionaries(value);
            }
            else {
                result[key] = value ?? '';
            }
        }
        return result;
    }
}
exports.YamlConfigFile = YamlConfigFile;
//# sourceMappingURL=YamlConfigFile.js.map