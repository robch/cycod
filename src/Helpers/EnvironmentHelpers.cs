public class EnvironmentHelpers
{
    public static string? FindEnvVar(string variable)
    {
        var value = Environment.GetEnvironmentVariable(variable);
        if (!string.IsNullOrEmpty(value))
        {
            ConsoleHelpers.WriteDebugLine($"Found variable: {variable} with value: {value}");
            return value;
        }

        var currentDirectory = Directory.GetCurrentDirectory();
        while (!string.IsNullOrEmpty(currentDirectory))
        {
            var envFilePath = Path.Combine(currentDirectory, ".env");
            ConsoleHelpers.WriteDebugLine($"Checking for .env file at: {envFilePath}");
            if (File.Exists(envFilePath))
            {
                ConsoleHelpers.WriteDebugLine($"Reading .env file at: {envFilePath}");
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

            envFilePath = Path.Combine(currentDirectory, $".{Program.Name}", ".env");
            if (File.Exists(envFilePath))
            {
                ConsoleHelpers.WriteDebugLine($"Reading .env file at: {envFilePath}");
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