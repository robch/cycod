public class EnvironmentHelpers
{
    public static string? FindEnvVar(string variable, bool searchDotEnvFile = false)
    {
        ConsoleHelpers.WriteDebugLine($"FindEnvVar: {variable}");

        var value = Environment.GetEnvironmentVariable(variable);
        var foundEnvVar = !string.IsNullOrEmpty(value);
        if (foundEnvVar)
        {
            ConsoleHelpers.WriteDebugLine($"Found {variable} value: {value}");
            return value;
        }

        var currentDirectory = searchDotEnvFile
		    ? Directory.GetCurrentDirectory()
            : null;
        while (!string.IsNullOrEmpty(currentDirectory))
        {
            var envFilePath = Path.Combine(currentDirectory, ".env");
            if (File.Exists(envFilePath))
            {
                var lines = File.ReadAllLines(envFilePath);
                foreach (var line in lines)
                {
                    var parts = line.Split('=', 2);
                    if (parts.Length == 2 && parts[0].Trim() == variable)
                    {
                        return parts[1].Trim();
                    }
                }
            }

            var parentDirectory = Directory.GetParent(currentDirectory);
            currentDirectory = parentDirectory?.FullName;
        }

        var configStore = ConfigStore.Instance;
        var configValue = configStore.GetFromAnyScope(variable);

        var foundInConfigStore = !configValue.IsNotFoundNullOrEmpty();
        if (foundInConfigStore) return configValue.AsString();

        return null;
    }
}