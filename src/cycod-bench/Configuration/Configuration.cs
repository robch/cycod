using Microsoft.Extensions.Configuration;
using CycodBench.Logging;

namespace CycodBench.Configuration;

/// <summary>
/// Implementation of the CycodBench configuration service.
/// </summary>
public class Configuration : IConfiguration
{
    private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
    private readonly string? _configFilePath;
    private readonly CycodBench.Logging.ILogger _logger;
    
    // Default configuration file name
    private const string DefaultConfigFileName = "cycodbench.json";
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Configuration"/> class with default parameters.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="args">Command line arguments.</param>
    public Configuration(CycodBench.Logging.ILogger logger, string[] args)
        : this(logger, args, GetDefaultConfigPath(), null)
    {
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Configuration"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="args">Command line arguments.</param>
    /// <param name="configFilePath">Path to the configuration file.</param>
    /// <param name="environmentPrefix">Environment variable prefix for configuration.</param>
    public Configuration(CycodBench.Logging.ILogger logger, string[] args, string? configFilePath, string? environmentPrefix)
    {
        _logger = logger;
        _configFilePath = configFilePath;
        
        var builder = new ConfigurationBuilder();
        
        // Add default values
        builder.AddInMemoryCollection(GetDefaultValues());
        
        // Add configuration from file if it exists
        if (!string.IsNullOrEmpty(configFilePath) && File.Exists(configFilePath))
        {
            _logger.Info("Loading configuration from file: {0}", configFilePath);
            try
            {
                builder.AddJsonFile(configFilePath, optional: true, reloadOnChange: true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error loading configuration file: {0}", configFilePath);
            }
        }
        else if (!string.IsNullOrEmpty(configFilePath))
        {
            _logger.Warning("Configuration file not found: {0}", configFilePath);
        }
        
        // Add environment variables using dictionary for now - .NET 9 environment variables extension might be different
        _logger.Debug("Adding environment variables with prefix: {0}", 
            !string.IsNullOrEmpty(environmentPrefix) ? environmentPrefix : "CYCODBENCH_");
            
        var prefix = !string.IsNullOrEmpty(environmentPrefix) ? environmentPrefix : "CYCODBENCH_";
        var envVars = Environment.GetEnvironmentVariables()
            .Cast<System.Collections.DictionaryEntry>()
            .Where(e => e.Key.ToString()?.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) == true)
            .ToDictionary(
                e => e.Key.ToString()!.Substring(prefix.Length).Replace("__", ":"),
                e => e.Value?.ToString()
            );
            
        builder.AddInMemoryCollection(envVars);
        
        // Add command line arguments
        if (args != null && args.Length > 0)
        {
            _logger.Debug("Adding command line arguments");
            
            // Simple command line parser to dictionary
            var commandLineArgs = new Dictionary<string, string?>();
            
            foreach (var arg in args)
            {
                if (arg.StartsWith("--"))
                {
                    var parts = arg.Substring(2).Split('=', 2);
                    if (parts.Length == 2)
                    {
                        // Handle both : and __ as section separators
                        var key = parts[0].Replace("__", ":");
                        commandLineArgs[key] = parts[1];
                    }
                    else
                    {
                        // Handle both : and __ as section separators
                        var key = parts[0].Replace("__", ":");
                        commandLineArgs[key] = "true";
                    }
                }
            }
            
            builder.AddInMemoryCollection(commandLineArgs);
        }
        
        _configuration = builder.Build();
        
        LogConfigurationSettings();
    }
    
    /// <inheritdoc/>
    public string? ConfigFilePath => _configFilePath;

    /// <inheritdoc/>
    public string? GetString(string key)
    {
        if (_runtimeValues.TryGetValue(key, out string? runtimeValue))
        {
            return runtimeValue;
        }

        return _configuration[key];
    }

    /// <inheritdoc/>
    public string GetString(string key, string defaultValue)
    {
        if (_runtimeValues.TryGetValue(key, out string? runtimeValue))
        {
            return runtimeValue ?? defaultValue;
        }

        return _configuration[key] ?? defaultValue;
    }

    /// <inheritdoc/>
    public int GetInt(string key, int defaultValue)
    {
        if (_runtimeValues.TryGetValue(key, out string? runtimeValue) && 
            int.TryParse(runtimeValue, out int result))
        {
            return result;
        }

        if (int.TryParse(_configuration[key], out int configResult))
        {
            return configResult;
        }
        
        return defaultValue;
    }

    /// <inheritdoc/>
    public bool GetBool(string key, bool defaultValue)
    {
        if (_runtimeValues.TryGetValue(key, out string? runtimeValue) && 
            bool.TryParse(runtimeValue, out bool result))
        {
            return result;
        }

        if (bool.TryParse(_configuration[key], out bool configResult))
        {
            return configResult;
        }
        
        return defaultValue;
    }

    /// <inheritdoc/>
    public double GetDouble(string key, double defaultValue)
    {
        if (_runtimeValues.TryGetValue(key, out string? runtimeValue) && 
            double.TryParse(runtimeValue, out double result))
        {
            return result;
        }

        if (double.TryParse(_configuration[key], out double configResult))
        {
            return configResult;
        }
        
        return defaultValue;
    }

    /// <inheritdoc/>
    public string[] GetStringArray(string key)
    {
        // Runtime values can't override arrays currently
        var section = _configuration.GetSection(key);
        if (section.Exists())
        {
            return section.Get<string[]>() ?? Array.Empty<string>();
        }
        
        return Array.Empty<string>();
    }

    /// <inheritdoc/>
    public IConfiguration GetSection(string key)
    {
        var section = _configuration.GetSection(key);
        return new ConfigurationSection(section, _logger);
    }

    /// <inheritdoc/>
    public IEnumerable<string> GetKeys()
    {
        return _configuration.AsEnumerable().Select(x => x.Key)
               .Concat(_runtimeValues.Keys)
               .Distinct();
    }

    /// <inheritdoc/>
    public bool Exists(string key)
    {
        return _runtimeValues.ContainsKey(key) || !string.IsNullOrEmpty(_configuration[key]);
    }
    
    // Helper method to log all configuration settings at debug level
    private void LogConfigurationSettings()
    {
        _logger.Debug("Configuration loaded with the following settings:");
        
        foreach (var kvp in _configuration.AsEnumerable()
                    .Where(x => !string.IsNullOrEmpty(x.Value))
                    .OrderBy(x => x.Key))
        {
            // Don't log sensitive values
            bool isSensitive = kvp.Key.Contains("password", StringComparison.OrdinalIgnoreCase) || 
                              kvp.Key.Contains("secret", StringComparison.OrdinalIgnoreCase) ||
                              kvp.Key.Contains("key", StringComparison.OrdinalIgnoreCase);
            
            string value = isSensitive ? "********" : kvp.Value ?? "(null)";
            _logger.Debug("  {0} = {1}", kvp.Key, value);
        }
    }
    
    // Get default values for configuration
    private static Dictionary<string, string?> GetDefaultValues()
    {
        return new Dictionary<string, string?>
        {
            // Docker configuration
            ["Docker:Image"] = "swebench/base:latest",
            ["Docker:WorkDir"] = "/workspace",
            ["Docker:Enabled"] = "true",
            
            // Sharding configuration
            ["ShardCount"] = "1",
            ["ShardId"] = "0", 
            
            // Execution configuration
            ["CandidateCount"] = "8",
            ["Parallelism"] = "4",
            
            // Dataset configuration
            ["Dataset:Path"] = "swebench_dataset",
            
            // Agent configuration
            ["Agent:ExecutablePath"] = "cycod",
            ["Agent:Timeout"] = "3600", // 1 hour in seconds
            
            // Workspace configuration
            ["Workspace:RootDirectory"] = Path.Combine(Directory.GetCurrentDirectory(), "workspaces"),
            
            // Evaluation configuration
            ["Evaluation:BuildTimeout"] = "600", // 10 minutes in seconds
            ["Evaluation:TestTimeout"] = "600", // 10 minutes in seconds
            ["Evaluation:DefaultBuildCommand"] = "make",
            ["Evaluation:DefaultTestCommand"] = "make test",
            
            // Ensemble configuration
            ["Ensemble:UseAgent"] = "true",
            
            // Logging configuration
            ["Logging:Level"] = "Information",
            ["Logging:FilePath"] = "logs/cycodbench.log"
        };
    }
    
    // Helper method to get the default configuration file path
    private static string GetDefaultConfigPath()
    {
        // First, check the current directory
        string currentDir = Directory.GetCurrentDirectory();
        string defaultPath = Path.Combine(currentDir, DefaultConfigFileName);
        
        if (File.Exists(defaultPath))
        {
            return defaultPath;
        }
        
        // Then check user profile directory
        string userProfileDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string userProfilePath = Path.Combine(userProfileDir, ".cycodbench", DefaultConfigFileName);
        
        if (File.Exists(userProfilePath))
        {
            return userProfilePath;
        }
        
        // Return the default path in the current directory (even if it doesn't exist)
        return defaultPath;
    }
    
    // Dictionary to hold runtime configuration values that override the loaded configuration
    private readonly Dictionary<string, string?> _runtimeValues = new();

    /// <inheritdoc/>
    public void SetValue(string key, string? value)
    {
        if (value == null)
        {
            _runtimeValues.Remove(key);
            _logger.Debug($"Removed runtime configuration value for key: {key}");
        }
        else
        {
            _runtimeValues[key] = value;
            _logger.Debug($"Set runtime configuration value for key: {key} = {value}");
        }
    }

    // Inner class to handle configuration sections
    private class ConfigurationSection : IConfiguration
    {
        private readonly Microsoft.Extensions.Configuration.IConfigurationSection _section;
        private readonly CycodBench.Logging.ILogger _logger;
        private readonly Dictionary<string, string?> _runtimeValues = new();
        
        public ConfigurationSection(Microsoft.Extensions.Configuration.IConfigurationSection section, CycodBench.Logging.ILogger logger)
        {
            _section = section;
            _logger = logger;
        }
        
        public string? ConfigFilePath => null;

        public string? GetString(string key)
        {
            if (_runtimeValues.TryGetValue(key, out string? runtimeValue))
            {
                return runtimeValue;
            }

            return _section[key];
        }

        public string GetString(string key, string defaultValue)
        {
            if (_runtimeValues.TryGetValue(key, out string? runtimeValue))
            {
                return runtimeValue ?? defaultValue;
            }

            return _section[key] ?? defaultValue;
        }

        public int GetInt(string key, int defaultValue)
        {
            if (_runtimeValues.TryGetValue(key, out string? runtimeValue) && 
                int.TryParse(runtimeValue, out int result))
            {
                return result;
            }

            if (int.TryParse(_section[key], out int sectionResult))
            {
                return sectionResult;
            }
            
            return defaultValue;
        }

        public bool GetBool(string key, bool defaultValue)
        {
            if (_runtimeValues.TryGetValue(key, out string? runtimeValue) && 
                bool.TryParse(runtimeValue, out bool result))
            {
                return result;
            }

            if (bool.TryParse(_section[key], out bool sectionResult))
            {
                return sectionResult;
            }
            
            return defaultValue;
        }

        public double GetDouble(string key, double defaultValue)
        {
            if (_runtimeValues.TryGetValue(key, out string? runtimeValue) && 
                double.TryParse(runtimeValue, out double result))
            {
                return result;
            }

            if (double.TryParse(_section[key], out double sectionResult))
            {
                return sectionResult;
            }
            
            return defaultValue;
        }

        public string[] GetStringArray(string key)
        {
            // Runtime values can't override arrays currently
            var section = _section.GetSection(key);
            if (section.Exists())
            {
                return section.Get<string[]>() ?? Array.Empty<string>();
            }
            
            return Array.Empty<string>();
        }

        public IConfiguration GetSection(string key)
        {
            var childSection = _section.GetSection(key);
            return new ConfigurationSection(childSection, _logger);
        }

        public IEnumerable<string> GetKeys()
        {
            return _section.GetChildren().Select(x => x.Key)
                   .Concat(_runtimeValues.Keys)
                   .Distinct();
        }

        public bool Exists(string key)
        {
            return _runtimeValues.ContainsKey(key) || _section.GetSection(key).Exists();
        }

        public void SetValue(string key, string? value)
        {
            if (value == null)
            {
                _runtimeValues.Remove(key);
                _logger.Debug($"Removed runtime configuration value for key: {key}");
            }
            else
            {
                _runtimeValues[key] = value;
                _logger.Debug($"Set runtime configuration value for key: {key} = {value}");
            }
        }
    }
}