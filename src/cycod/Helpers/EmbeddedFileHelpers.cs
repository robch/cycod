public class EmbeddedFileHelpers
{
    public static IEnumerable<string> GetEmbeddedStreamFileNames()
    {
        var assembly = ProgramInfo.Assembly;
        return assembly.GetManifestResourceNames();
    }

    public static bool EmbeddedStreamExists(string fileName)
    {
        var assembly = ProgramInfo.Assembly;
        var resourceName = assembly.GetManifestResourceNames()
            .Where(name => name.EndsWith(fileName, StringComparison.OrdinalIgnoreCase))
            .OrderBy(name => name.Length)
            .FirstOrDefault();

        var found = resourceName != null;
        if (found) return true;

        var allResourceNames = string.Join("\n  ", assembly.GetManifestResourceNames());
        ConsoleHelpers.WriteDebugLine($"DEBUG: Embedded resources ({assembly.GetManifestResourceNames().Count()}):\n\n  {allResourceNames}\n");

        return false;
    }

    public static string? ReadEmbeddedStream(string fileName)
    {
        var assembly = ProgramInfo.Assembly;
        var resourceName = assembly.GetManifestResourceNames()
            .Where(name => name.EndsWith(fileName, StringComparison.OrdinalIgnoreCase))
            .OrderBy(name => name.Length)
            .FirstOrDefault();

        if (resourceName == null) return null;

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null) return null;

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
