using System.Reflection;

/// <summary>
/// Helper class to provide version information from the assembly.
/// </summary>
public static class VersionInfo
{
    public static string GetVersion()
    {
        var version = GetAssemblyVersion();
        return IsReleaseBuild() ? StripCommitHash(version) : version;
    }

    private static string GetAssemblyVersion()
    {
        return ProgramInfo.Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                    ?? ProgramInfo.Assembly.GetName().Version?.ToString()
                    ?? DefaultVersion;
    }

    private static bool IsReleaseBuild()
    {
        return ProgramInfo.Assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration == "Release";
    }

    private static string StripCommitHash(string version)
    {
        int plusIndex = version.IndexOf('+');
        if (plusIndex > 0)
        {
            return version.Substring(0, plusIndex);
        }
        return version;
    }

    private const string DefaultVersion = "0.0.1";
}
