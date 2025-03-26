using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Manages configuration settings across different scopes.
/// </summary>
public class ConfigStore
{
    private readonly Dictionary<ConfigScope, Dictionary<string, object>> _configData;
    private bool _isLoaded;

    public static ConfigStore Instance => _instance;

    private static readonly ConfigStore _instance = new ConfigStore();

    /// <summary>
    /// Initializes a new instance of the ConfigStore class.
    /// </summary>
    protected ConfigStore()
    {
        _configData = new Dictionary<ConfigScope, Dictionary<string, object>>
        {
            { ConfigScope.Global, new Dictionary<string, object>() },
            { ConfigScope.User, new Dictionary<string, object>() },
            { ConfigScope.Project, new Dictionary<string, object>() }
        };
        _isLoaded = false;
    }

    /// <summary>
    /// Loads all configuration from all scopes.
    /// </summary>
    public void LoadAll()
    {
        foreach (ConfigScope scope in Enum.GetValues(typeof(ConfigScope)))
        {
            LoadConfig(scope);
        }
        _isLoaded = true;
    }

    /// <summary>
    /// Loads configuration from the specified scope.
    /// </summary>
    /// <param name="scope">The configuration scope to load.</param>
    public void LoadConfig(ConfigScope scope)
    {
        string? configPath = ConfigLocation.GetExistingConfigPath(scope);
        if (configPath == null)
        {
            ConsoleHelpers.WriteDebugLine($"ConfigStore.LoadConfig; no config file found for {scope} scope");
            _configData[scope] = new Dictionary<string, object>();
            return;
        }

        ConsoleHelpers.WriteDebugLine($"ConfigStore.LoadConfig; loading config file from {configPath}");
        var configFile = ConfigFile.Create(configPath);
        _configData[scope] = configFile.Read();
    }

    /// <summary>
    /// Saves configuration to the specified scope.
    /// </summary>
    /// <param name="scope">The configuration scope to save.</param>
    /// <param name="useYaml">Whether to use YAML format (true) or INI format (false).</param>
    public void SaveConfig(ConfigScope scope, bool useYaml = true)
    {
        ConfigLocation.EnsureConfigDirectoryExists(scope);
        
        string configPath = useYaml 
            ? ConfigLocation.GetYamlConfigPath(scope) 
            : ConfigLocation.GetIniConfigPath(scope);
            
        var configFile = ConfigFile.Create(configPath);
        configFile.Write(_configData[scope]);
    }

    /// <summary>
    /// Gets a configuration value from the highest priority scope that defines it.
    /// </summary>
    /// <param name="key">The configuration key (dot notation).</param>
    /// <returns>The configuration value.</returns>
    public ConfigValue Get(string key)
    {
        EnsureLoaded();

        // First, try environment variable (highest priority)
        ConsoleHelpers.WriteDebugLine($"ConfigStore.Get; checking environment variable: {key}");
        var envValue = Environment.GetEnvironmentVariable(key);
        var envValueInEnvironment = !string.IsNullOrEmpty(envValue);
        if (envValueInEnvironment)
        {
            ConsoleHelpers.WriteDebugLine($"Found '{key}' environment variable: {envValue}");
            return new ConfigValue(envValue);
        }

        // Second, try environment again, but with normalized key
        var normalizedKey = ConfigPathHelpers.Normalize(key);
        var envVarKey = ConfigPathHelpers.ToEnvVar(normalizedKey);

        ConsoleHelpers.WriteDebugLine($"ConfigStore.Get; checking environment variable: {envVarKey}");
        envValue = Environment.GetEnvironmentVariable(envVarKey);
        envValueInEnvironment = !string.IsNullOrEmpty(envValue);
        if (envValueInEnvironment)
        {
            ConsoleHelpers.WriteDebugLine($"Found '{envVarKey}' in environment variable: {envValue}");
            return new ConfigValue(envValue);
        }

        // Then search in order: Project -> User -> Global
        foreach (ConfigScope scope in new[] { ConfigScope.Project, ConfigScope.User, ConfigScope.Global })
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

    /// <summary>
    /// Gets a configuration value from a specific scope.
    /// </summary>
    /// <param name="key">The configuration key (dot notation).</param>
    /// <param name="scope">The configuration scope.</param>
    /// <returns>The configuration value.</returns>
    public ConfigValue GetFromScope(string key, ConfigScope scope)
    {
        EnsureLoaded();

        ConsoleHelpers.WriteDebugLine($"ConfigStore.GetFromScope; checking {scope} variable: {key}");
        string normalizedKey = ConfigPathHelpers.Normalize(key);
        string[] keyParts = normalizedKey.Split('.');
        
        Dictionary<string, object> scopeData = _configData[scope];
        object? value = GetNestedValue(scopeData, keyParts);

        var foundInScope = value != null;
        if (foundInScope)
        {
            ConsoleHelpers.WriteDebugLine($"ConfigStore.GetFromScope; Found '{normalizedKey}' in {scope} variable: {value}");
            return new ConfigValue(value);
        }

        return new ConfigValue();
    }

    /// <summary>
    /// Sets a configuration value in the specified scope.
    /// </summary>
    /// <param name="key">The configuration key (dot notation).</param>
    /// <param name="value">The value to set.</param>
    /// <param name="scope">The configuration scope.</param>
    /// <param name="save">Whether to save the configuration file immediately.</param>
    /// <returns>True if the value was set, false otherwise.</returns>
    public bool Set(string key, object value, ConfigScope scope = ConfigScope.Project, bool save = true)
    {
        EnsureLoaded();
        string normalizedKey = ConfigPathHelpers.Normalize(key);
        string[] keyParts = normalizedKey.Split('.');
        
        Dictionary<string, object> scopeData = _configData[scope];
        SetNestedValue(scopeData, keyParts, value);
        
        if (save)
        {
            SaveConfig(scope);
        }
        
        return true;
    }

    /// <summary>
    /// Clears a configuration value in the specified scope.
    /// </summary>
    /// <param name="key">The configuration key (dot notation).</param>
    /// <param name="scope">The configuration scope.</param>
    /// <param name="save">Whether to save the configuration file immediately.</param>
    /// <returns>True if the value was cleared, false otherwise.</returns>
    public bool Clear(string key, ConfigScope scope = ConfigScope.Project, bool save = true)
    {
        EnsureLoaded();
        string normalizedKey = ConfigPathHelpers.Normalize(key);
        string[] keyParts = normalizedKey.Split('.');
        
        if (keyParts.Length == 0)
        {
            return false;
        }

        Dictionary<string, object> scopeData = _configData[scope];
        
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
        Dictionary<string, object>? parent = scopeData;
        for (int i = 0; i < keyParts.Length - 1; i++)
        {
            if (!parent.ContainsKey(keyParts[i]) || !(parent[keyParts[i]] is Dictionary<string, object> nextLevel))
            {
                return false;
            }
            parent = nextLevel;
        }

        // Remove the key from the parent
        string lastKey = keyParts[^1];
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

    /// <summary>
    /// Adds a value to a list configuration in the specified scope.
    /// </summary>
    /// <param name="key">The configuration key (dot notation).</param>
    /// <param name="value">The value to add to the list.</param>
    /// <param name="scope">The configuration scope.</param>
    /// <param name="save">Whether to save the configuration file immediately.</param>
    /// <returns>True if the value was added, false otherwise.</returns>
    public bool AddToList(string key, string value, ConfigScope scope = ConfigScope.Project, bool save = true)
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

    /// <summary>
    /// Removes a value from a list configuration in the specified scope.
    /// </summary>
    /// <param name="key">The configuration key (dot notation).</param>
    /// <param name="value">The value to remove from the list.</param>
    /// <param name="scope">The configuration scope.</param>
    /// <param name="save">Whether to save the configuration file immediately.</param>
    /// <returns>True if the value was removed, false otherwise.</returns>
    public bool RemoveFromList(string key, string value, ConfigScope scope = ConfigScope.Project, bool save = true)
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

    /// <summary>
    /// Lists all configuration keys and their values in a flat dictionary.
    /// Includes all scopes with the highest priority scope's values taking precedence.
    /// </summary>
    /// <returns>A dictionary of all configuration keys and values.</returns>
    public Dictionary<string, ConfigValue> ListAll()
    {
        EnsureLoaded();
        var result = new Dictionary<string, ConfigValue>();

        // Start with global scope
        AddFlattenedValuesToResult(_configData[ConfigScope.Global], result, ConfigScope.Global);
        
        // Override with user scope
        AddFlattenedValuesToResult(_configData[ConfigScope.User], result, ConfigScope.User);
        
        // Finally override with project scope
        AddFlattenedValuesToResult(_configData[ConfigScope.Project], result, ConfigScope.Project);

        // Add environment variables if they override any values
        foreach (var key in result.Keys.ToList()) // Use ToList to avoid collection modified during iteration
        {
            string envVarKey = ConfigPathHelpers.ToEnvVar(key);
            string? envValue = Environment.GetEnvironmentVariable(envVarKey);
            if (!string.IsNullOrEmpty(envValue))
            {
                result[key] = new ConfigValue(envValue);
            }
        }

        return result;
    }

    /// <summary>
    /// Lists all configuration keys and their values in a specific scope.
    /// </summary>
    /// <param name="scope">The configuration scope.</param>
    /// <returns>A dictionary of configuration keys and values in the specified scope.</returns>
    public Dictionary<string, ConfigValue> ListScope(ConfigScope scope)
    {
        EnsureLoaded();
        var result = new Dictionary<string, ConfigValue>();
        AddFlattenedValuesToResult(_configData[scope], result, scope);
        return result;
    }

    // Helper methods

    /// <summary>
    /// Adds flattened values from a nested dictionary to a result dictionary.
    /// </summary>
    /// <param name="data">The source nested dictionary.</param>
    /// <param name="result">The target flat dictionary.</param>
    /// <param name="scope">The configuration scope.</param>
    /// <param name="prefix">The key prefix for the flattened keys.</param>
    private void AddFlattenedValuesToResult(Dictionary<string, object> data, Dictionary<string, ConfigValue> result, ConfigScope scope, string prefix = "")
    {
        foreach (var pair in data)
        {
            string key = string.IsNullOrEmpty(prefix) ? pair.Key : $"{prefix}.{pair.Key}";

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

    /// <summary>
    /// Gets a nested value from a dictionary using a key path.
    /// </summary>
    /// <param name="data">The source dictionary.</param>
    /// <param name="keyParts">The key parts representing the path to the value.</param>
    /// <returns>The nested value, or null if not found.</returns>
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

    /// <summary>
    /// Sets a nested value in a dictionary using a key path.
    /// </summary>
    /// <param name="data">The target dictionary.</param>
    /// <param name="keyParts">The key parts representing the path to the value.</param>
    /// <param name="value">The value to set.</param>
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

    /// <summary>
    /// Ensures the configuration is loaded.
    /// </summary>
    private void EnsureLoaded()
    {
        if (!_isLoaded)
        {
            LoadAll();
        }
    }
}