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
        _isLoaded = false;
        _scopeToSettings = new Dictionary<ConfigFileScope, Dictionary<string, object>>
        {
            { ConfigFileScope.Global, new Dictionary<string, object>() },
            { ConfigFileScope.User, new Dictionary<string, object>() },
            { ConfigFileScope.Local, new Dictionary<string, object>() }
        };
    }

    public void LoadConfig()
    {
        foreach (ConfigFileScope scope in Enum.GetValues(typeof(ConfigFileScope)))
        {
            LoadConfigFilesForScope(scope);
        }
        _isLoaded = true;
    }
 
    public void SaveConfig(ConfigFileScope scope)
    {
        var fileName = ConfigFileHelpers.GetConfigFileName(scope);
        DirectoryHelpers.EnsureDirectoryForFileExists(fileName!);

        var configFile = ConfigFile.FromFile(fileName);
        configFile.Write(_scopeToSettings[scope]);
    }

    public ConfigValue GetFromAnyScope(string key)
    {
        EnsureLoaded();

        // First, try environment variable (highest priority)
        if (TryGetFromEnv(key, out var configValue)) return configValue!;

        // Second, try environment again, but with normalized key
        var normalizedKey = ConfigPathHelpers.Normalize(key);
        var envVarKey = ConfigPathHelpers.ToEnvVar(normalizedKey);
        if (TryGetFromEnv(envVarKey, out configValue)) return configValue!;

        // Then search in order: Project -> User -> Global
        foreach (ConfigFileScope scope in new[] { ConfigFileScope.Local, ConfigFileScope.User, ConfigFileScope.Global })
        {
            var value = GetFromScope(normalizedKey, scope);
            if (!value.IsNullOrEmpty())
            {
                return value;
            }
        }

        ConsoleHelpers.WriteDebugLine($"ConfigStore.Get; no value found for '{key}'");
        return new ConfigValue();
    }

    public ConfigValue GetFromScope(string key, ConfigFileScope scope)
    {
        EnsureLoaded();

        ConsoleHelpers.WriteDebugLine($"ConfigStore.GetFromScope; checking {scope} variable: {key}");
        var normalizedKey = ConfigPathHelpers.Normalize(key);
        var keyParts = normalizedKey.Split('.');
        
        Dictionary<string, object> scopeData = _scopeToSettings[scope];
        var value = GetNestedValue(scopeData, keyParts);

        var foundInScope = value != null;
        if (foundInScope)
        {
            ConsoleHelpers.WriteDebugLine($"ConfigStore.GetFromScope; Found '{normalizedKey}' in {scope} variable: {value}");
            return new ConfigValue(value);
        }

        return new ConfigValue();
    }

    public bool Set(string key, object value, ConfigFileScope scope = ConfigFileScope.Local, bool save = true)
    {
        EnsureLoaded();
        var normalizedKey = ConfigPathHelpers.Normalize(key);
        var keyParts = normalizedKey.Split('.');
        
        var scopeData = _scopeToSettings[scope];
        SetNestedValue(scopeData, keyParts, value);
        
        if (save)
        {
            SaveConfig(scope);
        }
        
        return true;
    }

    public bool Clear(string key, ConfigFileScope scope = ConfigFileScope.Local, bool save = true)
    {
        EnsureLoaded();
        var normalizedKey = ConfigPathHelpers.Normalize(key);
        var keyParts = normalizedKey.Split('.');
        
        if (keyParts.Length == 0)
        {
            return false;
        }

        var scopeData = _scopeToSettings[scope];
        
        if (keyParts.Length == 1)
        {
            if (scopeData.ContainsKey(keyParts[0]))
            {
                scopeData.Remove(keyParts[0]);
                if (save)
                {
                    SaveConfig(scope);
                }
                return true;
            }
            return false;
        }

        // Navigate to the parent dictionary
        var parent = scopeData;
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
                SaveConfig(scope);
            }
            return true;
        }

        return false;
    }

    public bool AddToList(string key, string value, ConfigFileScope scope = ConfigFileScope.Local, bool save = true)
    {
        EnsureLoaded();
        var configValue = GetFromScope(key, scope);
        List<string> list = configValue.AsList();
        
        if (!list.Contains(value))
        {
            list.Add(value);
            Set(key, list, scope, save);
            return true;
        }
        
        return false;
    }

    public bool RemoveFromList(string key, string value, ConfigFileScope scope = ConfigFileScope.Local, bool save = true)
    {
        EnsureLoaded();
        var configValue = GetFromScope(key, scope);
        List<string> list = configValue.AsList();
        
        if (list.Remove(value))
        {
            Set(key, list, scope, save);
            return true;
        }
        
        return false;
    }

    public Dictionary<string, ConfigValue> ListAll()
    {
        EnsureLoaded();
        var result = new Dictionary<string, ConfigValue>();

        // Start with global scope
        AddFlattenedValuesToResult(_scopeToSettings[ConfigFileScope.Global], result, ConfigFileScope.Global);
        
        // Override with user scope
        AddFlattenedValuesToResult(_scopeToSettings[ConfigFileScope.User], result, ConfigFileScope.User);
        
        // Finally override with project scope
        AddFlattenedValuesToResult(_scopeToSettings[ConfigFileScope.Local], result, ConfigFileScope.Local);

        // Add environment variables if they override any values
        foreach (var key in result.Keys.ToList()) // Use ToList to avoid collection modified during iteration
        {
            var envVarKey = ConfigPathHelpers.ToEnvVar(key);
            var envValue = Environment.GetEnvironmentVariable(envVarKey);
            if (!string.IsNullOrEmpty(envValue))
            {
                result[key] = new ConfigValue(envValue);
            }
        }

        return result;
    }

    public Dictionary<string, ConfigValue> ListScope(ConfigFileScope scope)
    {
        EnsureLoaded();
        var result = new Dictionary<string, ConfigValue>();
        AddFlattenedValuesToResult(_scopeToSettings[scope], result, scope);
        return result;
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
            configValue = new ConfigValue(value);
            return true;
        }

        return false;
    }

    private void AddFlattenedValuesToResult(Dictionary<string, object> data, Dictionary<string, ConfigValue> result, ConfigFileScope scope, string prefix = "")
    {
        foreach (var pair in data)
        {
            var key = string.IsNullOrEmpty(prefix) ? pair.Key : $"{prefix}.{pair.Key}";

            if (pair.Value is Dictionary<string, object> nestedDict)
            {
                // Recursively flatten nested dictionaries
                AddFlattenedValuesToResult(nestedDict, result, scope, key);
            }
            else
            {
                // Add or update the result
                result[key] = new ConfigValue(pair.Value);
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

    private void SetNestedValue(Dictionary<string, object> data, string[] keyParts, object value)
    {
        if (keyParts.Length == 0)
        {
            return;
        }

        if (keyParts.Length == 1)
        {
            data[keyParts[0]] = value;
            return;
        }

        if (!data.ContainsKey(keyParts[0]) || !(data[keyParts[0]] is Dictionary<string, object>))
        {
            data[keyParts[0]] = new Dictionary<string, object>();
        }

        var nestedDict = (Dictionary<string, object>)data[keyParts[0]];
        SetNestedValue(nestedDict, keyParts[1..], value);
    }

    private void EnsureLoaded()
    {
        if (!_isLoaded)
        {
            LoadConfig();
        }
    }

    private void LoadConfigFilesForScope(ConfigFileScope scope)
    {
        var configPath = ConfigFileHelpers.FindConfigFile(scope);
        if (configPath == null)
        {
            ConsoleHelpers.WriteDebugLine($"ConfigStore.LoadConfig; no config file found for {scope} scope");
            _scopeToSettings[scope] = new Dictionary<string, object>();
            return;
        }

        ConsoleHelpers.WriteDebugLine($"ConfigStore.LoadConfig; loading config file from {configPath}");
        var configFile = ConfigFile.FromFile(configPath);
        _scopeToSettings[scope] = configFile.Read();
    }

    private bool _isLoaded;
    private readonly Dictionary<ConfigFileScope, Dictionary<string, object>> _scopeToSettings;

    private static readonly ConfigStore _instance = new ConfigStore();
}