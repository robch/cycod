/// <summary>
/// Handles the /title slash command for viewing and setting conversation titles.
/// Supports subcommands: view, set, lock, unlock
/// </summary>
public class SlashTitleCommandHandler : SlashCommandBase
{
    public override string CommandName => "title";
    
    public SlashTitleCommandHandler()
    {
        // Register subcommands
        _subcommands["view"] = HandleView;
        _subcommands["set"] = HandleSet;
        _subcommands["lock"] = HandleLock;
        _subcommands["unlock"] = HandleUnlock;
    }
    
    /// <summary>
    /// Handles the default /title command (no subcommand) - shows title and help.
    /// </summary>
    protected override SlashCommandResult HandleDefault(FunctionCallingChat chat)
    {
        ShowTitleAndHelp(chat.Metadata);
        return SlashCommandResult.Handled;
    }
    
    /// <summary>
    /// Handles /title view - shows current title and status only.
    /// </summary>
    private SlashCommandResult HandleView(string[] args, FunctionCallingChat chat)
    {
        ShowCurrentTitle(chat.Metadata);
        return SlashCommandResult.Handled;
    }
    
    /// <summary>
    /// Handles /title set <text> - sets title and locks it.
    /// </summary>
    private SlashCommandResult HandleSet(string[] args, FunctionCallingChat chat)
    {
        if (args.Length == 0)
        {
            ConsoleHelpers.WriteLine("Error: /title set requires a title. Usage: /title set <text>\n", ConsoleColor.Red);
            return SlashCommandResult.Handled;
        }
        
        var title = string.Join(" ", args);
        SetUserTitle(chat, title);
        return SlashCommandResult.NeedsSave; // 🚀 Request immediate save
    }
    
    /// <summary>
    /// Handles /title lock - locks current title from AI changes.
    /// </summary>
    private SlashCommandResult HandleLock(string[] args, FunctionCallingChat chat)
    {
        var metadata = chat.Metadata ?? ConversationMetadataHelpers.CreateDefault();
        
        if (metadata.TitleLocked)
        {
            ConsoleHelpers.WriteLine("Title is already locked.\n", ConsoleColor.Yellow);
            return SlashCommandResult.Handled;
        }
        else
        {
            metadata.TitleLocked = true;
            chat.UpdateMetadata(metadata);
            ConsoleHelpers.WriteLine("Title locked from AI changes.\n", ConsoleColor.Yellow);
            return SlashCommandResult.NeedsSave; // 🚀 Request immediate save
        }
    }
    
    /// <summary>
    /// Handles /title unlock - unlocks title to allow AI regeneration.
    /// </summary>
    private SlashCommandResult HandleUnlock(string[] args, FunctionCallingChat chat)
    {
        var metadata = chat.Metadata ?? ConversationMetadataHelpers.CreateDefault();
        
        if (!metadata.TitleLocked)
        {
            ConsoleHelpers.WriteLine("Title is already unlocked.\n", ConsoleColor.Yellow);
            return SlashCommandResult.Handled;
        }
        else
        {
            metadata.TitleLocked = false;
            chat.UpdateMetadata(metadata);
            ConsoleHelpers.WriteLine("Title unlocked - AI can now regenerate the title.\n", ConsoleColor.Yellow);
            return SlashCommandResult.NeedsSave; // 🚀 Request immediate save
        }
    }
    
    /// <summary>
    /// Shows current title and available subcommands (default /title behavior).
    /// </summary>
    private void ShowTitleAndHelp(ConversationMetadata? metadata)
    {
        ShowCurrentTitle(metadata);
        ShowHelp(metadata);
    }
    
    /// <summary>
    /// Shows available title subcommands.
    /// </summary>
    protected override void ShowHelp(ConversationMetadata? metadata)
    {
        ConsoleHelpers.WriteLine("Available commands:", ConsoleColor.Yellow);
        ConsoleHelpers.WriteLine("  /title view         Show current title and lock status", ConsoleColor.White);
        ConsoleHelpers.WriteLine("  /title set <text>   Set title and lock from AI changes", ConsoleColor.White);
        ConsoleHelpers.WriteLine("  /title lock         Lock current title from AI changes", ConsoleColor.White);
        ConsoleHelpers.WriteLine("  /title unlock       Unlock title to allow AI regeneration\n", ConsoleColor.White);
    }
    
    /// <summary>
    /// Displays the current title and its lock status.
    /// </summary>
    private void ShowCurrentTitle(ConversationMetadata? metadata)
    {
        var title = metadata?.Title ?? "No title set";
        var status = metadata?.TitleLocked == true ? "locked" : "unlocked";
        
        ConsoleHelpers.WriteLine($"Current title: \"{title}\" ({status})", ConsoleColor.Yellow);
        ConsoleHelpers.WriteLine();
    }
    
    /// <summary>
    /// Sets a user-provided title and marks it as locked.
    /// </summary>
    private void SetUserTitle(FunctionCallingChat chat, string title)
    {
        // Initialize metadata if not present
        var metadata = chat.Metadata ?? ConversationMetadataHelpers.CreateDefault();
        
        // Set and lock the title
        ConversationMetadataHelpers.SetUserTitle(metadata, title);
        
        // Update the chat's metadata
        chat.UpdateMetadata(metadata);
        
        ConsoleHelpers.WriteLine($"Title updated to: \"{title}\" (locked from AI changes)\n", ConsoleColor.Yellow);
    }
}