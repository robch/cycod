using System.Reflection;

abstract public class ProgramInfo
{
    public static string Name { get => _getName(); }
    public static string ConfigDirName { get => _getConfigDirName(); }
    public static Assembly Assembly { get => _getAssembly(); }

    protected ProgramInfo(Func<string> name, Func<string> configDirName, Func<Assembly> assembly)
    {
        if (name == null || configDirName == null || assembly == null)
        {
            throw new ArgumentNullException("Name, ConfigDir, and Assembly cannot be null.");
        }

        Initialize(name, configDirName, assembly);
    }

    private static void Initialize(Func<string> name, Func<string> configDirName, Func<Assembly> assembly)
    {
        if (_getName != null || _getConfigDirName != null || _getAssembly != null)
        {
            throw new InvalidOperationException("ProgramInfo is already initialized.");
        }

        _getName = name;
        _getAssembly = assembly;
        _getConfigDirName = configDirName;
    }

    private static Func<string> _getName = null!;
    private static Func<Assembly> _getAssembly = null!;
    private static Func<string> _getConfigDirName = null!;
}
