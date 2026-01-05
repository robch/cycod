using Microsoft.CognitiveServices.Speech;
using System.Text;

namespace CyCoD.Helpers;

/// <summary>
/// Helper methods for Azure Cognitive Services Speech SDK integration.
/// </summary>
public static class SpeechHelpers
{
    /// <summary>
    /// Creates a SpeechConfig from configuration files (speech.key and speech.region).
    /// </summary>
    /// <returns>Configured SpeechConfig instance</returns>
    /// <exception cref="FileNotFoundException">If speech.key or speech.region files are not found</exception>
    public static SpeechConfig CreateSpeechConfig()
    {
        // Look for speech.key file in scope order: local, user, global
        var keyFile = ScopeFileHelpers.FindFileInAnyScope("speech.key", "", searchParents: false);
        if (keyFile == null)
        {
            // Try without subdirectory - look directly in config root
            keyFile = FindFileInConfigScopes("speech.key");
        }
        if (keyFile == null)
        {
            throw new FileNotFoundException("Could not find speech.key file in any configuration scope (local, user, or global).\n" +
                "Please create a speech.key file with your Azure Cognitive Services Speech key.\n" +
                "Run 'cycod config --help' for more information on configuration scopes.");
        }
        var key = File.ReadAllText(keyFile, Encoding.Default).Trim();
        
        // Look for speech.region file in scope order: local, user, global
        var regionFile = ScopeFileHelpers.FindFileInAnyScope("speech.region", "", searchParents: false);
        if (regionFile == null)
        {
            // Try without subdirectory - look directly in config root
            regionFile = FindFileInConfigScopes("speech.region");
        }
        if (regionFile == null)
        {
            throw new FileNotFoundException("Could not find speech.region file in any configuration scope (local, user, or global).\n" +
                "Please create a speech.region file with your Azure Cognitive Services Speech region (e.g., 'westus2').\n" +
                "Run 'cycod config --help' for more information on configuration scopes.");
        }
        var region = File.ReadAllText(regionFile, Encoding.Default).Trim();
        
        return SpeechConfig.FromSubscription(key, region);
    }
    
    /// <summary>
    /// Finds a file in configuration scopes (local, user, global) directly in the config directory root.
    /// </summary>
    private static string? FindFileInConfigScopes(string fileName)
    {
        foreach (var scope in new[] { ConfigFileScope.Local, ConfigFileScope.User, ConfigFileScope.Global })
        {
            var scopeDir = ConfigFileHelpers.GetScopeDirectoryPath(scope);
            if (scopeDir == null) continue;
            
            var filePath = Path.Combine(scopeDir, fileName);
            if (File.Exists(filePath))
            {
                return filePath;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Gets speech input from the user with real-time interim results.
    /// </summary>
    /// <returns>The final recognized text</returns>
    public static async Task<string> GetSpeechInputAsync()
    {
        Console.Write("\r");
        ConsoleHelpers.Write("\nUser: ", ConsoleColor.Yellow);
        Console.ForegroundColor = ConsoleColor.DarkGray;
        
        var text = "(listening)";
        Console.Write($"{text} ...");
        var lastTextDisplayed = text;
        
        var config = CreateSpeechConfig();
        var recognizer = new SpeechRecognizer(config);
        
        // Show interim results while recognizing
        recognizer.Recognizing += (s, e) =>
        {
            Console.Write("\r");
            ConsoleHelpers.Write("User: ", ConsoleColor.Yellow);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            
            text = e.Result.Text;
            Console.Write($"{text} ...");
            
            // Clear any leftover text from previous display
            if (text.Length < lastTextDisplayed.Length)
            {
                Console.Write(new string(' ', lastTextDisplayed.Length - text.Length));
            }
            lastTextDisplayed = text;
        };
        
        // Perform recognition
        var result = await recognizer.RecognizeOnceAsync();
        
        // Clear the interim result line
        Console.Write("\r");
        Console.Write(new string(' ', lastTextDisplayed.Length + 20));
        Console.Write("\r");
        ConsoleHelpers.Write("User: ", ConsoleColor.Yellow);
        
        return result.Text;
    }
}
