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
        var configValue = configStore.GetFromAnyScope(variable);

        var foundInConfigStore = !configValue.IsNotFoundNullOrEmpty();
        if (foundInConfigStore) return configValue.AsString();

        return null;
    }
}