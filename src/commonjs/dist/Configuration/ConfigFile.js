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
exports.ConfigFile = void 0;
const path = __importStar(require("path"));
const ConfigFileScope_1 = require("./ConfigFileScope");
class ConfigFile {
    static fromFile(filePath, scope) {
        const ext = path.extname(filePath).toLowerCase();
        switch (ext) {
            case '.yaml':
            case '.yml':
                // Dynamically import to avoid circular dependency
                const { YamlConfigFile } = require('./YamlConfigFile');
                return new YamlConfigFile(filePath, scope);
            default:
                // Dynamically import to avoid circular dependency
                const { IniConfigFile } = require('./IniConfigFile');
                return new IniConfigFile(filePath, scope);
        }
    }
    get fileName() {
        return this._fileName;
    }
    get scope() {
        return this._scope;
    }
    get settings() {
        this.ensureLoaded();
        return this._settings;
    }
    set settings(value) {
        this._settings = value;
    }
    constructor(fileName, scope = ConfigFileScope_1.ConfigFileScope.FileName) {
        this._fileName = fileName;
        this._scope = scope;
    }
    save() {
        this.ensureLoaded();
        this.writeSettings(this._fileName);
    }
    ensureLoaded() {
        if (this._settings === undefined) {
            this._settings = this.readSettings(this._fileName);
        }
    }
}
exports.ConfigFile = ConfigFile;
//# sourceMappingURL=ConfigFile.js.map