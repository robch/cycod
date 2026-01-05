using System.Reflection;

abstract public class ProgramInfo
{
    public static string Name { get => _getName(); }
    public static string Description { get => _getDescription(); }
    public static string ConfigDirName { get => _getConfigDirName(); }
    public static Assembly Assembly { get => _getAssembly(); }
    public static string Exe { get => GetExecutablePath(); }

    public static string GetDisplayBannerText()
    {
        return $"{Name.ToUpper()} - {Description}, Version {VersionInfo.GetVersion()}";
    }

    private static string GetExecutablePath()
    {
        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        var location = assembly.Location;
        
        // On some platforms (like .NET Core single-file publish), Location might be empty
        // In that case, use the process name
        if (string.IsNullOrEmpty(location))
        {
            var processPath = Environment.ProcessPath;
            if (!string.IsNullOrEmpty(processPath))
            {
                return processPath;
            }
        }
        
        // If we have a .dll, convert to .exe for Windows
        if (location.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) && OS.IsWindows())
        {
            return location.Substring(0, location.Length - 4) + ".exe";
        }
        
        return location;
    }

    protected ProgramInfo(Func<string> name, Func<string> description, Func<string> configDirName, Func<Assembly> assembly)
    {
        if (name == null || description == null || configDirName == null || assembly == null)
        {
            throw new ArgumentNullException("Name, Description, ConfigDir, and Assembly cannot be null.");
        }

        Initialize(name, description, configDirName, assembly);
    }

    private static void Initialize(Func<string> name, Func<string> description, Func<string> configDirName, Func<Assembly> assembly)
    {
        if (_getName != null || _getDescription != null || _getConfigDirName != null || _getAssembly != null)
        {
            throw new InvalidOperationException("ProgramInfo is already initialized.");
        }

        _getName = name;
        _getDescription = description;
        _getAssembly = assembly;
        _getConfigDirName = configDirName;
    }

    private static Func<string> _getName = null!;
    private static Func<string> _getDescription = null!;
    private static Func<Assembly> _getAssembly = null!;
    private static Func<string> _getConfigDirName = null!;
}
