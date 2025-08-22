using System.IO;
using System.Runtime.InteropServices;

namespace CycoDev.CustomTools
{
    /// <summary>
    /// Provides utility methods for handling paths across different platforms.
    /// </summary>
    public static class PathUtils
    {
        /// <summary>
        /// Gets whether the current platform is Windows.
        /// </summary>
        public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        /// <summary>
        /// Gets whether the current platform is macOS.
        /// </summary>
        public static bool IsMacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        /// <summary>
        /// Gets whether the current platform is Linux.
        /// </summary>
        public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        /// <summary>
        /// Gets the platform-specific directory separator.
        /// </summary>
        public static char DirectorySeparator => Path.DirectorySeparatorChar;

        /// <summary>
        /// Gets the platform-specific path separator.
        /// </summary>
        public static char PathSeparator => Path.PathSeparator;

        /// <summary>
        /// Normalizes a path to the current platform's format.
        /// </summary>
        /// <param name="path">The path to normalize.</param>
        /// <returns>The normalized path.</returns>
        public static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            // Replace all separators with the platform-specific separator
            return path.Replace('\\', DirectorySeparator).Replace('/', DirectorySeparator);
        }

        /// <summary>
        /// Converts a path to Windows format.
        /// </summary>
        /// <param name="path">The path to convert.</param>
        /// <returns>The Windows-formatted path.</returns>
        public static string ToWindowsPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            return path.Replace('/', '\\');
        }

        /// <summary>
        /// Converts a path to Unix format.
        /// </summary>
        /// <param name="path">The path to convert.</param>
        /// <returns>The Unix-formatted path.</returns>
        public static string ToUnixPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            return path.Replace('\\', '/');
        }

        /// <summary>
        /// Resolves a path to its absolute form.
        /// </summary>
        /// <param name="path">The path to resolve.</param>
        /// <param name="workingDirectory">The working directory to resolve relative paths from.</param>
        /// <returns>The absolute path.</returns>
        public static string ResolvePath(string path, string workingDirectory)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            // If the path is already absolute, return it
            if (Path.IsPathRooted(path))
            {
                return path;
            }

            // Otherwise, resolve it relative to the working directory
            return Path.GetFullPath(Path.Combine(workingDirectory, path));
        }

        /// <summary>
        /// Replaces placeholder variables in a path with their values.
        /// </summary>
        /// <param name="path">The path containing placeholders.</param>
        /// <param name="variables">A dictionary of placeholder variables and their values.</param>
        /// <returns>The path with placeholders replaced.</returns>
        public static string ReplacePlaceholders(string path, Dictionary<string, string> variables)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            foreach (var variable in variables)
            {
                path = path.Replace($"{{{variable.Key}}}", variable.Value);
            }

            return path;
        }

        /// <summary>
        /// Gets the platform-specific temporary directory.
        /// </summary>
        /// <returns>The temporary directory path.</returns>
        public static string GetTempDirectory()
        {
            return Path.GetTempPath();
        }

        /// <summary>
        /// Gets a platform-specific temporary directory for a tool.
        /// </summary>
        /// <param name="toolName">The name of the tool.</param>
        /// <returns>The tool-specific temporary directory path.</returns>
        public static string GetToolTempDirectory(string toolName)
        {
            string tempDir = Path.Combine(GetTempDirectory(), "cycod-tools", toolName);
            Directory.CreateDirectory(tempDir);
            return tempDir;
        }

        /// <summary>
        /// Sanitizes a path to be used in a shell command.
        /// </summary>
        /// <param name="path">The path to sanitize.</param>
        /// <returns>The sanitized path.</returns>
        public static string SanitizePathForShell(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            // For Windows, wrap the path in quotes if it contains spaces
            if (IsWindows)
            {
                if (path.Contains(" ") && !path.StartsWith("\"") && !path.EndsWith("\""))
                {
                    return $"\"{path}\"";
                }
            }
            // For Unix-like systems, escape spaces
            else
            {
                if (path.Contains(" "))
                {
                    return path.Replace(" ", "\\ ");
                }
            }

            return path;
        }
    }
}