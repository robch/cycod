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
class PathHelpers {
    static combine(basePath, ...paths) {
        try {
            return path.join(basePath, ...paths);
        }
        catch {
            return null;
        }
    }
    static getAbsolutePath(relativePath, basePath) {
        if (path.isAbsolute(relativePath)) {
            return relativePath;
        }
        const base = basePath || process.cwd();
        return path.resolve(base, relativePath);
    }
    static getRelativePath(from, to) {
        return path.relative(from, to);
    }
    static getFileName(filePath) {
        return path.basename(filePath);
    }
    static getFileNameWithoutExtension(filePath) {
        const fileName = path.basename(filePath);
        const ext = path.extname(fileName);
        return fileName.slice(0, -ext.length);
    }
    static getDirectoryName(filePath) {
        return path.dirname(filePath);
    }
    static getExtension(filePath) {
        return path.extname(filePath);
    }
    static changeExtension(filePath, newExtension) {
        const dir = path.dirname(filePath);
        const name = path.basename(filePath, path.extname(filePath));
        const ext = newExtension.startsWith('.') ? newExtension : `.${newExtension}`;
        return path.join(dir, name + ext);
    }
    static getTempPath() {
        return os.tmpdir();
    }
    static getHomePath() {
        return os.homedir();
    }
    static normalizePath(filePath) {
        return path.normalize(filePath);
    }
    static isPathRooted(filePath) {
        return path.isAbsolute(filePath);
    }
}
exports.PathHelpers = PathHelpers;
//# sourceMappingURL=PathHelpers.js.map