using System.Reflection;

abstract public class ProgramInfo
{
    public static string Name { get => _getName(); }
    public static Assembly Assembly { get => _getAssembly(); }

    protected ProgramInfo(Func<string> name, Func<Assembly> assembly)
    {
        if (name == null || assembly == null)
        {
            throw new ArgumentNullException("Name and Assembly cannot be null.");
        }

        Initialize(name, assembly);
    }

    private static void Initialize(Func<string> name, Func<Assembly> assembly)
    {
        if (_getName != null || _getAssembly != null)
        {
            throw new InvalidOperationException("ProgramInfo is already initialized.");
        }

        _getName = name;
        _getAssembly = assembly;
    }

    private static Func<string> _getName = null!;
    private static Func<Assembly> _getAssembly = null!;
}
