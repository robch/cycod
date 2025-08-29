// Mock implementation of ConfigStore
export const mockConfigStore = {
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

export class ConfigStore {
  private static _instance: ConfigStore;

  static get instance(): ConfigStore {
    if (!ConfigStore._instance) {
      ConfigStore._instance = new ConfigStore();
    }
    return ConfigStore._instance;
  }

  getFromScope = mockConfigStore.getFromScope;
  getFromFileName = mockConfigStore.getFromFileName;
  getFromAnyScope = mockConfigStore.getFromAnyScope;
  set = mockConfigStore.set;
  setInFile = mockConfigStore.setInFile;
  clear = mockConfigStore.clear;
  addToList = mockConfigStore.addToList;
  removeFromList = mockConfigStore.removeFromList;
  listValuesFromScope = mockConfigStore.listValuesFromScope;
  listFromCommandLineSettings = mockConfigStore.listFromCommandLineSettings;
  setCommandLineSetting = mockConfigStore.setCommandLineSetting;
  loadConfigFile = mockConfigStore.loadConfigFile;
  loadConfigFiles = mockConfigStore.loadConfigFiles;
}