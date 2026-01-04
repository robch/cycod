using System.Runtime.InteropServices;

/// <summary>
/// Handles the /screenshot slash command for user-initiated screenshots.
/// Implements async interface since file I/O operations are involved.
/// </summary>
public class SlashScreenshotCommandHandler : IAsyncSlashCommandHandler
{
    public SlashScreenshotCommandHandler(ChatCommand chatCommand)
    {
        _chatCommand = chatCommand;
    }

    /// <summary>
    /// Checks if this handler can process the /screenshot command.
    /// </summary>
    public bool CanHandle(string userPrompt)
    {
        if (string.IsNullOrWhiteSpace(userPrompt))
        {
            return false;
        }

        return userPrompt.Trim().Equals("/screenshot", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Handles the /screenshot command by capturing a screenshot and adding it to the conversation.
    /// </summary>
    public async Task<SlashCommandResult> HandleAsync(string userPrompt, FunctionCallingChat chat)
    {
        // Display function call to user
        ConsoleHelpers.DisplayUserFunctionCall("/screenshot", null);

        // Check platform support
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            var errorMessage = ScreenshotHelper.GetPlatformErrorMessage();
            ConsoleHelpers.DisplayUserFunctionCall("/screenshot", errorMessage);
            chat.Conversation.AddUserMessage(errorMessage);
            return SlashCommandResult.Success();
        }

        try
        {
            // Capture screenshot (await to make async, even though the method is synchronous)
            var filePath = await Task.Run(() => ScreenshotHelper.TakeScreenshot());
            
            if (filePath == null)
            {
                var errorMessage = "Failed to capture screenshot. Please check that the display is accessible.";
                ConsoleHelpers.DisplayUserFunctionCall("/screenshot", errorMessage);
                chat.Conversation.AddUserMessage(errorMessage);
                return SlashCommandResult.Success();
            }

            // Add the screenshot to the image patterns so it gets included in the next message
            _chatCommand.ImagePatterns.Add(filePath);
            
            var successMessage = $"Screenshot captured and will be included in next message: {filePath}";
            ConsoleHelpers.DisplayUserFunctionCall("/screenshot", successMessage);
            
            return SlashCommandResult.Success();
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error capturing screenshot: {ex.Message}";
            ConsoleHelpers.DisplayUserFunctionCall("/screenshot", errorMessage);
            chat.Conversation.AddUserMessage(errorMessage);
            return SlashCommandResult.Success();
        }
    }

    private readonly ChatCommand _chatCommand;
}
