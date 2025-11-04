using System.Runtime.InteropServices;

namespace Cycod.Debugging.Dap;

public static class NetcoredbgLocator
{
    static string? _cachedPath;

    public static string FindNetcoredbg()
    {
        if (_cachedPath != null) return _cachedPath;
        var path = Locate();
        if (path != null)
        {
            _cachedPath = path;
            return _cachedPath;
        }
        throw new FileNotFoundException(@"netcoredbg not found. Install from https://github.com/Samsung/netcoredbg/releases and place in ~/.local/bin or /usr/local/bin");
    }

    static string? Locate()
    {
        var exe = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "netcoredbg.exe" : "netcoredbg";
        string[] candidates =
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "netcoredbg", exe),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "bin", exe),
            Path.Combine("/usr","local","netcoredbg", exe),
            Path.Combine("/usr","local","bin", exe),
            Path.Combine("/usr","bin", exe)
        };
        foreach (var c in candidates) if (File.Exists(c)) return c;
        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (pathEnv != null)
        {
            foreach (var dir in pathEnv.Split(Path.PathSeparator))
            {
                var full = Path.Combine(dir, exe);
                if (File.Exists(full)) return full;
            }
        }
        return null;
    }
}
