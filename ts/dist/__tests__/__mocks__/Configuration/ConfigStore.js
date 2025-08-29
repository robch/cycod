"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.ConfigStore = exports.mockConfigStore = void 0;
// Mock implementation of ConfigStore
exports.mockConfigStore = {
    getFromScope: jest.fn(),
    getFromFileName: jest.fn(),
    getFromAnyScope: jest.fn(),
    set: jest.fn(),
    setInFile: jest.fn(),
    clear: jest.fn(),
    addToList: jest.fn(),
    removeFromList: jest.fn(),
    listValuesFromScope: jest.fn(),
    listFromCommandLineSettings: jest.fn(),
    setCommandLineSetting: jest.fn(),
    loadConfigFile: jest.fn(),
    loadConfigFiles: jest.fn(),
};
class ConfigStore {
    constructor() {
        this.getFromScope = exports.mockConfigStore.getFromScope;
        this.getFromFileName = exports.mockConfigStore.getFromFileName;
        this.getFromAnyScope = exports.mockConfigStore.getFromAnyScope;
        this.set = exports.mockConfigStore.set;
        this.setInFile = exports.mockConfigStore.setInFile;
        this.clear = exports.mockConfigStore.clear;
        this.addToList = exports.mockConfigStore.addToList;
        this.removeFromList = exports.mockConfigStore.removeFromList;
        this.listValuesFromScope = exports.mockConfigStore.listValuesFromScope;
        this.listFromCommandLineSettings = exports.mockConfigStore.listFromCommandLineSettings;
        this.setCommandLineSetting = exports.mockConfigStore.setCommandLineSetting;
        this.loadConfigFile = exports.mockConfigStore.loadConfigFile;
        this.loadConfigFiles = exports.mockConfigStore.loadConfigFiles;
    }
    static get instance() {
        if (!ConfigStore._instance) {
            ConfigStore._instance = new ConfigStore();
        }
        return ConfigStore._instance;
    }
}
exports.ConfigStore = ConfigStore;
//# sourceMappingURL=ConfigStore.js.map