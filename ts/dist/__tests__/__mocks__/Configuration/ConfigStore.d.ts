export declare const mockConfigStore: {
    getFromScope: jest.Mock<any, any, any>;
    getFromFileName: jest.Mock<any, any, any>;
    getFromAnyScope: jest.Mock<any, any, any>;
    set: jest.Mock<any, any, any>;
    setInFile: jest.Mock<any, any, any>;
    clear: jest.Mock<any, any, any>;
    addToList: jest.Mock<any, any, any>;
    removeFromList: jest.Mock<any, any, any>;
    listValuesFromScope: jest.Mock<any, any, any>;
    listFromCommandLineSettings: jest.Mock<any, any, any>;
    setCommandLineSetting: jest.Mock<any, any, any>;
    loadConfigFile: jest.Mock<any, any, any>;
    loadConfigFiles: jest.Mock<any, any, any>;
};
export declare class ConfigStore {
    private static _instance;
    static get instance(): ConfigStore;
    getFromScope: jest.Mock<any, any, any>;
    getFromFileName: jest.Mock<any, any, any>;
    getFromAnyScope: jest.Mock<any, any, any>;
    set: jest.Mock<any, any, any>;
    setInFile: jest.Mock<any, any, any>;
    clear: jest.Mock<any, any, any>;
    addToList: jest.Mock<any, any, any>;
    removeFromList: jest.Mock<any, any, any>;
    listValuesFromScope: jest.Mock<any, any, any>;
    listFromCommandLineSettings: jest.Mock<any, any, any>;
    setCommandLineSetting: jest.Mock<any, any, any>;
    loadConfigFile: jest.Mock<any, any, any>;
    loadConfigFiles: jest.Mock<any, any, any>;
}
//# sourceMappingURL=ConfigStore.d.ts.map