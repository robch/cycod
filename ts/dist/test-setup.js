"use strict";
// Test setup file for Jest
// Add any global test setup here
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
exports.mockConsole = exports.cleanupTempDir = exports.createTempDir = void 0;
const fs = __importStar(require("fs-extra"));
const path = __importStar(require("path"));
const os = __importStar(require("os"));
// Setup temporary directories for tests
const createTempDir = async () => {
    const tempDir = await fs.mkdtemp(path.join(os.tmpdir(), 'cycod-test-'));
    return tempDir;
};
exports.createTempDir = createTempDir;
const cleanupTempDir = async (tempDir) => {
    await fs.remove(tempDir);
};
exports.cleanupTempDir = cleanupTempDir;
// Mock console methods for testing
const mockConsole = () => {
    const originalLog = console.log;
    const originalError = console.error;
    const logs = [];
    const errors = [];
    console.log = jest.fn((...args) => {
        logs.push(args.join(' '));
    });
    console.error = jest.fn((...args) => {
        errors.push(args.join(' '));
    });
    return {
        logs,
        errors,
        restore: () => {
            console.log = originalLog;
            console.error = originalError;
        },
    };
};
exports.mockConsole = mockConsole;
//# sourceMappingURL=test-setup.js.map