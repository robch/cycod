public class PathHelpers
{
    public static string? Combine(string path1, string path2)
    {
        try
        {
            return Path.Combine(path1, path2);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    public static string? Combine(string path1, string path2, string path3)
    {
        try
        {
            return Path.Combine(path1, path2, path3);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    public static IEnumerable<string> Combine(string path1, IEnumerable<string> path2s)
    {
        var list = new List<string>();
        foreach (var path2 in path2s)
        {
            var combined = PathHelpers.Combine(path1, path2);
            if (combined == null) continue;

            list.Add(!string.IsNullOrEmpty(path2)
                ? combined
                : path1);
        }
        return list;
    }

    public static string NormalizePath(string outputDirectory)
    {
        var normalized = new DirectoryInfo(outputDirectory).FullName;
        var cwd = Directory.GetCurrentDirectory();
        return normalized.StartsWith(cwd) && normalized.Length > cwd.Length + 1
            ? normalized.Substring(cwd.Length + 1)
            : normalized;
    }
}
