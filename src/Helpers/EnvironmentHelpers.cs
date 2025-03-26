public class EnvironmentHelpers
{
    public static string? FindEnvVar(string variable)
    {
        ConsoleHelpers.WriteDebugLine($"FindEnvVar: {variable}");

        var value = Environment.GetEnvironmentVariable(variable);
        var foundEnvVar = !string.IsNullOrEmpty(value);
        if (foundEnvVar)
        {
            ConsoleHelpers.WriteDebugLine($"Found {variable} value: {value}");
            return value;
        }

        var configStore = ConfigStore.Instance;
        var configValue = configStore.Get(variable);

        var foundInConfigStore = !configValue.IsNullOrEmpty();
        if (foundInConfigStore) return configValue.AsString();

        return null;
        // return FindEnvVarLegacy(variable);
    }

    /// <summary>
    /// Legacy implementation of FindEnvVar that directly accesses config files.
    /// </summary>
    /// <param name="variable">The environment variable name.</param>
    /// <returns>The value of the environment variable, or null if not found.</returns>
    private static string? FindEnvVarLegacy(string variable)
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        while (!string.IsNullOrEmpty(currentDirectory))
        {
            var envFilePath = Path.Combine(currentDirectory, $".{Program.Name}", "config");
            ConsoleHelpers.WriteDebugLine($"Checking for file at: {envFilePath}");
            if (File.Exists(envFilePath))
            {
                ConsoleHelpers.WriteDebugLine($"Reading file at: {envFilePath}");
                var lines = File.ReadAllLines(envFilePath);
                ConsoleHelpers.WriteDebugLine($"Looking for variable: {variable}");
                foreach (var line in lines)
                {
                    var parts = line.Split('=', 2);
                    if (parts.Length == 2 && parts[0].Trim() == variable)
                    {
                        ConsoleHelpers.WriteDebugLine($"Found variable: {variable} with value: {parts[1].Trim()}");
                        return parts[1].Trim();
                    }
                }
            }

            var parentDirectory = Directory.GetParent(currentDirectory);
            currentDirectory = parentDirectory?.FullName;
        }

        return null;
    }
}