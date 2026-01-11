using CyCoD.Helpers;

/// <summary>
/// Handles the /speech slash command for voice input.
/// Implements async interface since speech recognition is asynchronous.
/// </summary>
public class SlashSpeechCommandHandler : IAsyncSlashCommandHandler
{
    /// <summary>
    /// Checks if this handler can process the /speech command.
    /// </summary>
    public bool CanHandle(string userPrompt)
    {
        if (string.IsNullOrWhiteSpace(userPrompt))
        {
            return false;
        }

        return userPrompt.Trim().Equals("/speech", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Handles the /speech command by getting speech input and returning it as text.
    /// </summary>
    public async Task<SlashCommandResult> HandleAsync(string userPrompt, FunctionCallingChat chat)
    {
        try
        {
            var recognizedText = await SpeechHelpers.GetSpeechInputAsync();
            
            if (string.IsNullOrWhiteSpace(recognizedText))
            {
                // No speech recognized - skip assistant response
                return SlashCommandResult.Success();
            }

            // Display the recognized text in yellow to show it was from speech
            ConsoleHelpers.WriteLine(recognizedText, ConsoleColor.White);
            
            // Return the recognized text to be sent to the assistant
            return SlashCommandResult.WithResponse(recognizedText);
        }
        catch (FileNotFoundException fnfe)
        {
            ConsoleHelpers.WriteErrorLine($"\nSpeech configuration error: {fnfe.Message}");
            return SlashCommandResult.Success();
        }
        catch (Exception e)
        {
            ConsoleHelpers.WriteErrorLine($"\nSpeech recognition error: {e.Message}");
            return SlashCommandResult.Success();
        }
    }
}
