using System;
using System.IO;

public class EnvironmentHelpers
{
    public static string FindEnvVar(string variable)
    {
        var value = Environment.GetEnvironmentVariable(variable);
        if (!string.IsNullOrEmpty(value))
        {
            return value;
        }

        var currentDirectory = Directory.GetCurrentDirectory();
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

        return null;
    }
}