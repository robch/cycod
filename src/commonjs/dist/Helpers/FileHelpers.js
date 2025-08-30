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
exports.FileHelpers = void 0;
const fs = __importStar(require("fs"));
const path = __importStar(require("path"));
const glob_1 = require("glob");
const os = __importStar(require("os"));
class FileHelpers {
    static findFiles(pathPattern, pattern) {
        if (pattern) {
            const combined = path.join(pathPattern, pattern);
            return this.findFiles(combined);
        }
        try {
            // Handle glob patterns
            return (0, glob_1.sync)(pathPattern, { cwd: process.cwd() });
        }
        catch (error) {
            console.error(`Error finding files: ${error.message}`);
            return [];
        }
    }
    static fileExists(filePath) {
        try {
            return fs.existsSync(filePath) && fs.statSync(filePath).isFile();
        }
        catch {
            return false;
        }
    }
    static directoryExists(dirPath) {
        try {
            return fs.existsSync(dirPath) && fs.statSync(dirPath).isDirectory();
        }
        catch {
            return false;
        }
    }
    static readAllText(filePath) {
        return fs.readFileSync(filePath, 'utf-8');
    }
    static writeAllText(filePath, content) {
        const dir = path.dirname(filePath);
        if (!fs.existsSync(dir)) {
            fs.mkdirSync(dir, { recursive: true });
        }
        fs.writeFileSync(filePath, content, 'utf-8');
    }
    static writeTextToTempFile(content, extension) {
        const tempDir = os.tmpdir();
        const fileName = `temp_${Date.now()}_${Math.random().toString(36).substring(2, 15)}${extension || '.tmp'}`;
        const tempFilePath = path.join(tempDir, fileName);
        this.writeAllText(tempFilePath, content);
        return tempFilePath;
    }
    static deleteFile(filePath) {
        try {
            fs.unlinkSync(filePath);
            return true;
        }
        catch {
            return false;
        }
    }
    static copyFile(source, destination) {
        const destDir = path.dirname(destination);
        if (!fs.existsSync(destDir)) {
            fs.mkdirSync(destDir, { recursive: true });
        }
        fs.copyFileSync(source, destination);
    }
}
exports.FileHelpers = FileHelpers;
//# sourceMappingURL=FileHelpers.js.map