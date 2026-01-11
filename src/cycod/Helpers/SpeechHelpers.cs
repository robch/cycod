using Microsoft.CognitiveServices.Speech;
using System.Text;

namespace CyCoD.Helpers;

/// <summary>
/// Helper methods for Azure Cognitive Services Speech SDK integration.
/// </summary>
public static class SpeechHelpers
{
    /// <summary>
    /// Creates a SpeechConfig from configuration settings (Speech.Key and Speech.Region).
    /// </summary>
    /// <returns>Configured SpeechConfig instance</returns>
    /// <exception cref="InvalidOperationException">If speech key or region are not configured</exception>
    public static SpeechConfig CreateSpeechConfig()
    {
        // Get speech key from configuration system
        var keyConfig = ConfigStore.Instance.GetFromAnyScope(KnownSettings.SpeechKey);
        if (keyConfig.IsNotFoundNullOrEmpty())
        {
            throw new InvalidOperationException("Speech key not configured. " +
                "Set it using: 'cycod config set Speech.Key <your-key>' or environment variable SPEECH_KEY.\n" +
                "Run 'cycod config --help' for more information on configuration options.");
        }
        var key = keyConfig.AsString()!.Trim();
        
        // Get speech region from configuration system
        var regionConfig = ConfigStore.Instance.GetFromAnyScope(KnownSettings.SpeechRegion);
        if (regionConfig.IsNotFoundNullOrEmpty())
        {
            throw new InvalidOperationException("Speech region not configured. " +
                "Set it using: 'cycod config set Speech.Region <region>' or environment variable SPEECH_REGION (e.g., 'westus2').\n" +
                "Run 'cycod config --help' for more information on configuration options.");
        }
        var region = regionConfig.AsString()!.Trim();
        
        return SpeechConfig.FromSubscription(key, region);
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
