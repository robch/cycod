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
        throw new FileNotFoundException(@"netcoredbg not found.

Set NETCOREDBG_PATH to the full executable path or install netcoredbg.

Windows suggestions:
  %USERPROFILE%\netcoredbg\netcoredbg.exe
  Chocolatey (if installed): choco install netcoredbg
  Scoop (if installed): scoop install netcoredbg

macOS/Linux suggestions:
  ~/.local/bin/netcoredbg
  /usr/local/bin/netcoredbg (ensure executable permission)

Downloads: https://github.com/Samsung/netcoredbg/releases
");
    }

    static string? Locate()
    {
        var exeName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "netcoredbg.exe" : "netcoredbg";

        // Environment override
        var envPath = Environment.GetEnvironmentVariable("NETCOREDBG_PATH");
        if (!string.IsNullOrWhiteSpace(envPath) && File.Exists(envPath)) return envPath;

        IEnumerable<string> candidates;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var user = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var pfx86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            var choc = Environment.GetEnvironmentVariable("ChocolateyInstall");

            candidates = new[]
            {
                Path.Combine(user, "netcoredbg", exeName),
                Path.Combine(user, ".local", "netcoredbg", exeName),
                Path.Combine(user, ".local", "bin", exeName),
                Path.Combine(user, "Tools", "netcoredbg", exeName),

                Path.Combine(pf, "Samsung", "netcoredbg", exeName),
                Path.Combine(pfx86, "Samsung", "netcoredbg", exeName),

                Path.Combine(pf, "netcoredbg", exeName),
                Path.Combine(pfx86, "netcoredbg", exeName),

                choc != null ? Path.Combine(choc, "bin", exeName) : null,
                Path.Combine(user, "scoop", "apps", "netcoredbg", "current", exeName),
                Path.Combine(pf, "dotnet", "tools", exeName)
            }.Where(p => !string.IsNullOrEmpty(p));
        }
        else
        {
            var user = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            candidates = new[]
            {
                Path.Combine(user, ".local", "netcoredbg", exeName),
                Path.Combine(user, ".local", "bin", exeName),
                Path.Combine("/usr","local","netcoredbg", exeName),
                Path.Combine("/usr","local","bin", exeName),
                Path.Combine("/usr","bin", exeName),
                Path.Combine("/opt","homebrew","bin", exeName)
            };
        }

        foreach (var c in candidates)
            if (File.Exists(c)) return c;

        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (pathEnv != null)
        {
            foreach (var dir in pathEnv.Split(Path.PathSeparator))
            {
                var full = Path.Combine(dir, exeName);
                if (File.Exists(full)) return full;
            }
        }
        return null;
    }
}
