using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Manages configuration settings across different scopes.
/// </summary>
public class ConfigStore
{
    public static ConfigStore Instance => _instance;

    protected ConfigStore()
    {
        _loadedKnownConfigFiles = false;
        _commandLineSettings = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
    }

    public void LoadConfigFile(string fileName)
    {
        EnsureLoaded();

        Logger.Info($"Loading config file: {fileName}");
        var configFile = ConfigFile.FromFile(fileName, ConfigFileScope.FileName);
        _configFiles.Add(configFile);
    }

    public void LoadConfigFiles(IEnumerable<string> fileNames)
    {
        EnsureLoaded();

        foreach (var fileName in fileNames)
        {
            Logger.Info($"Loading config file: {fileName}");
            var configFile = ConfigFile.FromFile(fileName, ConfigFileScope.FileName);
            _configFiles.Add(configFile);
        }
    }

    public ConfigValue GetFromAnyScope(string key)
    {
        EnsureLoaded();

        var dotNotationKey = KnownSettings.ToDotNotation(key);
        bool isSecret = KnownSettings.IsSecret(dotNotationKey);

        // 1. First, check command line settings (highest priority)
        if (_commandLineSettings.TryGetValue(dotNotationKey, out var cmdLineValue))
        {
            ConsoleHelpers.WriteDebugLine($"ConfigStore.GetFromAnyScope; Found '{dotNotationKey}' in command line settings");
            return new ConfigValue(cmdLineValue, ConfigSource.CommandLine, isSecret);
        }

        // 2. Try environment variable 
        var envVarKey = KnownSettings.ToEnvironmentVariable(dotNotationKey);
        if (TryGetFromEnv(envVarKey, out var configValue))
        {
            ConsoleHelpers.WriteDebugLine($"ConfigStore.GetFromAnyScope; Found '{dotNotationKey}' in environment variable: {envVarKey}");
            return configValue!;
        }

        // 3. Then search the non-local, user, and global config files
        foreach (var configFile in _configFiles.Where(c => 
            c.Scope != ConfigFileScope.Local && 
            c.Scope != ConfigFileScope.User && 
            c.Scope != ConfigFileScope.Global))
        {
            var value = GetFromConfig(dotNotationKey, configFile);
            if (!value.IsNotFoundNullOrEmpty())
            {
                var source = ConfigSourceFromScope(configFile.Scope);
                ConsoleHelpers.WriteDebugLine($"ConfigStore.GetFromAnyScope; Found '{dotNotationKey}' in specified config file: {configFile.FileName}");
                return new ConfigValue(value.Value, source, isSecret) { File = configFile };
            }
        }


        // 3. Then search the config files in order of priority (Local, User, Global)
        // Check Local scope first
        var localConfigFile = _configFiles.FirstOrDefault(c => c.Scope == ConfigFileScope.Local);
        if (localConfigFile != null)
        {
            var value = GetFromConfig(dotNotationKey, localConfigFile);
            if (!value.IsNotFoundNullOrEmpty())
            {
                var source = ConfigSourceFromScope(localConfigFile.Scope);
                ConsoleHelpers.WriteDebugLine($"ConfigStore.GetFromAnyScope; Found '{dotNotationKey}' in local config file: {localConfigFile.FileName}");
                return new ConfigValue(value.Value, source, isSecret) { File = localConfigFile };
            }
        }
        
        // Check User scope next
        var userConfigFile = _configFiles.FirstOrDefault(c => c.Scope == ConfigFileScope.User);
        if (userConfigFile != null)
        {
            var value = GetFromConfig(dotNotationKey, userConfigFile);
            if (!value.IsNotFoundNullOrEmpty())
            {
                var source = ConfigSourceFromScope(userConfigFile.Scope);
                ConsoleHelpers.WriteDebugLine($"ConfigStore.GetFromAnyScope; Found '{dotNotationKey}' in user config file: {userConfigFile.FileName}");
                return new ConfigValue(value.Value, source, isSecret) { File = userConfigFile };
            }
        }
        
        // Check Global scope last - lowest priority
        var globalConfigFile = _configFiles.FirstOrDefault(c => c.Scope == ConfigFileScope.Global);
        if (globalConfigFile != null)
        {
            var value = GetFromConfig(dotNotationKey, globalConfigFile);
            if (!value.IsNotFoundNullOrEmpty())
            {
                var source = ConfigSourceFromScope(globalConfigFile.Scope);
                ConsoleHelpers.WriteDebugLine($"ConfigStore.GetFromAnyScope; Found '{dotNotationKey}' in global config file: {globalConfigFile.FileName}");
                return new ConfigValue(value.Value, source, isSecret) { File = globalConfigFile };
            }
        }
        
        ConsoleHelpers.WriteDebugLine($"ConfigStore.GetFromAnyScope; no value found for '{key}'");
        return new ConfigValue();
    }

    public ConfigValue GetFromFileName(string key, string fileName)
    {
        var configFile = _configFiles.FirstOrDefault(c => c.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
        configFile ??= ConfigFile.FromFile(fileName, ConfigFileScope.FileName);
        return GetFromConfig(key, configFile);
    }

    public ConfigValue GetFromScope(string key, ConfigFileScope scope)
    {
        if (scope == ConfigFileScope.Any) return GetFromAnyScope(key);

        var configFile = ConfigFileFromScope(scope);
        return GetFromConfig(key, configFile);
    }

    public ConfigValue GetFromConfig(string key, ConfigFile? configFile)
    {
        if (configFile != null)
        {
            EnsureLoaded();

            ConsoleHelpers.WriteDebugLine($"ConfigStore.GetFromConfig; checking {configFile.Scope} setting: {key}");

            var dotNotationKey = KnownSettings.ToDotNotation(key);
            bool isSecret = KnownSettings.IsSecret(dotNotationKey);
            var keyParts = dotNotationKey.Split('.');
            
            var value = GetNestedValue(configFile.Settings, keyParts);
            var foundInScope = value != null;

            if (foundInScope)
            {
                var configValue = new ConfigValue(value, ConfigSourceFromScope(configFile.Scope), isSecret) { File = configFile };
                var displayValue = configValue.IsSecret ? configValue.AsObfuscated() : configValue.Value?.ToString();
                ConsoleHelpers.WriteDebugLine($"ConfigStore.GetFromConfig; Found '{dotNotationKey}' in {configFile.FileName} setting: {displayValue}");
                return configValue;
            }
        }

        ConsoleHelpers.WriteDebugLine($"ConfigStore.GetFromConfig; no config file found for {key}");
        return new ConfigValue();
    }

    public bool Set(string key, object value, string fileName)
    {
        var configFile = _configFiles.FirstOrDefault(c => c.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
        configFile ??= ConfigFile.FromFile(fileName, ConfigFileScope.FileName);
        return Set(key, value, configFile, save: true);
    }

    public bool Set(string key, object value, ConfigFileScope scope = ConfigFileScope.Local, bool save = true)
    {
        var configFile = ConfigFileFromScope(scope, forceCreate: true)!;
        return Set(key, value, configFile, save);
    }

    public bool Set(string key, object value, ConfigFile configFile, bool save = true)
    {
        EnsureLoaded();

        var dotNotationKey = KnownSettings.ToDotNotation(key);
        var keyParts = dotNotationKey.Split('.');
        if (keyParts.Length == 0) return false;

        SetNestedValue(configFile.Settings, keyParts, value);
        if (save) 
        {
            configFile.Save();
            Logger.Info($"Config: Updated '{key}' in {configFile.FileName}");
        }

        return true;
    }

    public bool Clear(string key, string fileName, bool save = true)
    {
        var configFile = _configFiles.FirstOrDefault(c => c.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
        configFile ??= ConfigFile.FromFile(fileName, ConfigFileScope.FileName);
        return Clear(key, configFile, save);
    }

    public bool Clear(string key, ConfigFileScope scope = ConfigFileScope.Local, bool save = true)
    {
        var configFile = ConfigFileFromScope(scope, forceCreate: true)!;
        return Clear(key, configFile, save);
    }

    public bool Clear(string key, ConfigFile configFile, bool save = true)
    {
        EnsureLoaded();

        var dotNotationKey = KnownSettings.ToDotNotation(key);
        var keyParts = dotNotationKey.Split('.');
        if (keyParts.Length == 0) return false;
        
        if (keyParts.Length == 1)
        {
            if (configFile.Settings.ContainsKey(keyParts[0]))
            {
                configFile.Settings.Remove(keyParts[0]);
                if (save)
                {
                    configFile.Save();
                    Logger.Info($"Config: Cleared '{key}' in {configFile.FileName}");
                }

                return true;
            }
            return false;
        }

        // Navigate to the parent dictionary
        var parent = configFile.Settings;
        for (int i = 0; i < keyParts.Length - 1; i++)
        {
            if (!parent.ContainsKey(keyParts[i]) || !(parent[keyParts[i]] is Dictionary<string, object> nextLevel))
            {
                return false;
            }
            parent = nextLevel;
        }

        // Remove the key from the parent
        var lastKey = keyParts[^1];
        if (parent.ContainsKey(lastKey))
        {
            parent.Remove(lastKey);
            if (save)
            {
                configFile.Save();
                Logger.Info($"Config: Cleared nested key '{key}' in {configFile.FileName}");
            }

            return true;
        }

        return false;
    }

    public bool AddToList(string key, string value, string fileName, bool save = true)
    {
        var configFile = _configFiles.FirstOrDefault(c => c.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
        configFile ??= ConfigFile.FromFile(fileName, ConfigFileScope.FileName);
        return AddToList(key, value, configFile, save);
    }

    public bool AddToList(string key, string value, ConfigFileScope scope = ConfigFileScope.Local, bool save = true)
    {
        var configFile = ConfigFileFromScope(scope, forceCreate: true)!;
        return AddToList(key, value, configFile, save);
    }

    public bool AddToList(string key, string value, ConfigFile configFile, bool save = true)
    {
        EnsureLoaded();
        var configValue = GetFromConfig(key, configFile);

        List<string> list = configValue.AsList();
        if (!list.Contains(value))
        {
            ConsoleHelpers.WriteDebugLine($"ConfigStore.AddToList; adding '{value}' to '{key}' list");
            list.Add(value);
            Set(key, list, configFile, save);
            return true;
        }

        ConsoleHelpers.WriteDebugLine($"ConfigStore.AddToList; '{value}' already exists in '{key}' list");
        return false;
    }

    public bool RemoveFromList(string key, string value, string fileName, bool save = true)
    {
        var configFile = _configFiles.FirstOrDefault(c => c.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
        configFile ??= ConfigFile.FromFile(fileName, ConfigFileScope.FileName);
        return RemoveFromList(key, value, configFile, save);
    }

    public bool RemoveFromList(string key, string value, ConfigFileScope scope = ConfigFileScope.Local, bool save = true)
    {
        var configFile = ConfigFileFromScope(scope, forceCreate: true)!;
        return RemoveFromList(key, value, configFile, save);
    }

    public bool RemoveFromList(string key, string value, ConfigFile configFile, bool save = true)
    {
        EnsureLoaded();
        var configValue = GetFromConfig(key, configFile);

        List<string> list = configValue.AsList();
        if (list.Remove(value))
        {
            ConsoleHelpers.WriteDebugLine($"ConfigStore.RemoveFromList; removed '{value}' from '{key}' list");
            Set(key, list, configFile, save);
            return true;
        }
        
        ConsoleHelpers.WriteDebugLine($"ConfigStore.RemoveFromList; '{value}' not found in '{key}' list");
        return false;
    }

    public Dictionary<string, Dictionary<string, ConfigValue>> ListFileNameScopeValues()
    {
        EnsureLoaded();

        var result = new Dictionary<string, Dictionary<string, ConfigValue>>(StringComparer.OrdinalIgnoreCase);

        var configFiles = _configFiles.Where(c => c.Scope == ConfigFileScope.FileName);
        foreach (var configFile in configFiles)
        {
            var source = ConfigSourceFromScope(configFile.Scope);
            var fileName = Path.GetFileName(configFile.FileName);
            var settings = new Dictionary<string, ConfigValue>(StringComparer.OrdinalIgnoreCase);
            AddFlattenedValuesToResult(configFile.Settings, settings, source);
            result[fileName] = settings;
        }

        return result;
    }

    public Dictionary<string, ConfigValue> ListFromCommandLineSettings()
    {
        var result = new Dictionary<string, ConfigValue>(StringComparer.OrdinalIgnoreCase);
        foreach (var setting in _commandLineSettings)
        {
            var isSecret = KnownSettings.IsSecret(setting.Key);
            result[setting.Key] = new ConfigValue(setting.Value, ConfigSource.CommandLine, isSecret);
        }
        return result;
    }

    public Dictionary<string, ConfigValue> ListValuesFromKnownScope(ConfigFileScope scope)
    {
        var source = ConfigSourceFromScope(scope);
        var configFile = ConfigFileFromScope(scope);

        var result = new Dictionary<string, ConfigValue>(StringComparer.OrdinalIgnoreCase);
        AddFlattenedValuesToResult(configFile?.Settings, result, source);
        
        return result;
    }

    public Dictionary<string, ConfigValue> ListValuesFromFile(string fileName)
    {
        EnsureLoaded();

        var configFile = _configFiles.FirstOrDefault(c => c.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
        if (configFile == null)
        {
            throw new InvalidOperationException($"No config file found for {fileName}.");
        }

        return ListValuesFromFile(configFile);
    }

    public Dictionary<string, ConfigValue> ListValuesFromFile(ConfigFile configFile)
    {
        var result = new Dictionary<string, ConfigValue>(StringComparer.OrdinalIgnoreCase);
        var source = ConfigSourceFromScope(configFile.Scope);
        AddFlattenedValuesToResult(configFile.Settings, result, source);
        return result;
    }

    public bool SetFromCommandLine(string key, object value)
    {
        var dotNotationKey = KnownSettings.ToDotNotation(key);
        _commandLineSettings[dotNotationKey] = value;
        var displayValue = KnownSettings.IsSecret(dotNotationKey) ? "<secret>" : value?.ToString();
        Logger.Info($"Config: Set command line setting '{dotNotationKey}' to '{displayValue}'");
        return true;
    }
    
    private ConfigFile? ConfigFileFromScope(ConfigFileScope scope, bool forceCreate = false)
    {
        EnsureLoaded();

        var configFile = _configFiles.Where(c => c.Scope == scope).FirstOrDefault();
        if (configFile != null) return configFile;

        if (forceCreate)
        {
            var fileName = ConfigFileHelpers.FindConfigFile(scope, forceCreate: true)!;
            if (fileName != null)
            {
                configFile = ConfigFile.FromFile(fileName, scope);
                _configFiles.Add(configFile);
            }
        }

        return configFile;
    }

    private static ConfigSource ConfigSourceFromScope(ConfigFileScope scope)
    {
        return scope switch
        {
            ConfigFileScope.Local => ConfigSource.LocalConfig,
            ConfigFileScope.User => ConfigSource.UserConfig,
            ConfigFileScope.Global => ConfigSource.GlobalConfig,
            ConfigFileScope.FileName => ConfigSource.ConfigFileName,
            _ => ConfigSource.Default
        };
    }

    private static bool TryGetFromEnv(string key, out ConfigValue? configValue)
    {
        configValue = null;

        ConsoleHelpers.WriteDebugLine($"ConfigStore.TryGetFromEnv; checking environment variable: {key}");
        var value = Environment.GetEnvironmentVariable(key);
        var found = !string.IsNullOrEmpty(value);
        if (found)
        {
            ConsoleHelpers.WriteDebugLine($"ConfigStore.TryGetFromEnv; Found '{key}' environment variable: {value}");
            configValue = new ConfigValue(value, ConfigSource.EnvironmentVariable, isSecret: KnownSettings.IsSecret(key));
            return true;
        }

        return false;
    }

    private void AddFlattenedValuesToResult(Dictionary<string, object>? data, Dictionary<string, ConfigValue> result, ConfigSource source, string prefix = "")
    {
        if (data == null) return;

        foreach (var pair in data)
        {
            var key = string.IsNullOrEmpty(prefix) ? pair.Key : $"{prefix}.{pair.Key}";

            if (pair.Value is Dictionary<string, object> nestedDict)
            {
                // Recursively flatten nested dictionaries
                AddFlattenedValuesToResult(nestedDict, result, source, key);
            }
            else
            {
                // Check if this is a secret
                bool isSecret = KnownSettings.IsSecret(key);
                
                // Add or update the result
                result[key] = new ConfigValue(pair.Value, source, isSecret);
            }
        }
    }

    private object? GetNestedValue(Dictionary<string, object> data, string[] keyParts)
    {
        if (keyParts.Length == 0)
        {
            return null;
        }

        if (!data.ContainsKey(keyParts[0]))
        {
            return null;
        }

        if (keyParts.Length == 1)
        {
            return data[keyParts[0]];
        }

        if (data[keyParts[0]] is Dictionary<string, object> nestedDict)
        {
            return GetNestedValue(nestedDict, keyParts[1..]);
        }

        return null;
    }

    private void SetNestedValue(Dictionary<string, object> settings, string[] keyParts, object value)
    {
        if (keyParts.Length == 0)
        {
            return;
        }

        if (keyParts.Length == 1)
        {
            settings[keyParts[0]] = value;
            return;
        }

        if (!settings.ContainsKey(keyParts[0]) || !(settings[keyParts[0]] is Dictionary<string, object>))
        {
            settings[keyParts[0]] = new Dictionary<string, object>();
        }

        var nestedDict = (Dictionary<string, object>)settings[keyParts[0]];
        SetNestedValue(nestedDict, keyParts[1..], value);
    }

    private void EnsureLoaded()
    {
        if (!_loadedKnownConfigFiles)
        {
            LoadKnownConfigFiles();
        }
    }

    private void LoadKnownConfigFiles()
    {
        LoadConfigFileForScope(ConfigFileScope.Global);
        LoadConfigFileForScope(ConfigFileScope.User);
        LoadConfigFileForScope(ConfigFileScope.Local);
        _loadedKnownConfigFiles = true;
    }

    private void LoadConfigFileForScope(ConfigFileScope scope)
    {
        var configPath = ConfigFileHelpers.FindConfigFile(scope);
        if (configPath == null)
        {
            Logger.Info($"Config: No config file found for {scope} scope");
            return;
        }

        Logger.Info($"Config: Loading config file from {configPath} for {scope} scope");
        var configFile = ConfigFile.FromFile(configPath, scope);
        _configFiles.Add(configFile);
    }

    private bool _loadedKnownConfigFiles = false;
    private List<ConfigFile> _configFiles = new List<ConfigFile>();
    private readonly Dictionary<string, object> _commandLineSettings;

    private static readonly ConfigStore _instance = new ConfigStore();
}