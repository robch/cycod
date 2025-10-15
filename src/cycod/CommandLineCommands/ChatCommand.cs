using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using System.Diagnostics;
using System.Text;

public class ChatCommand : CommandWithVariables
{
    public ChatCommand()
    {
    }

    public override bool IsEmpty()
    {
        return false;
    }

    public override string GetCommandName()
    {
        return "";
    }

    public override ChatCommand Clone()
    {
        var clone = new ChatCommand();
        
        // Copy all properties
        clone.SystemPrompt = this.SystemPrompt;
        clone.SystemPromptAdds = new List<string>(this.SystemPromptAdds);
        clone.UserPromptAdds = new List<string>(this.UserPromptAdds);
        clone.MaxPromptTokenTarget = this.MaxPromptTokenTarget;
        clone.MaxToolTokenTarget = this.MaxToolTokenTarget;
        clone.MaxOutputTokens = this.MaxOutputTokens;
        clone.MaxChatTokenTarget = this.MaxChatTokenTarget;
        clone.LoadMostRecentChatHistory = this.LoadMostRecentChatHistory;
        clone.InputChatHistory = this.InputChatHistory;
        clone.OutputChatHistory = this.OutputChatHistory;
        clone.OutputTrajectory = this.OutputTrajectory;
        clone.AutoSaveOutputChatHistory = this.AutoSaveOutputChatHistory;
        clone.AutoSaveOutputTrajectory = this.AutoSaveOutputTrajectory;
        clone.InputInstructions = new List<string>(this.InputInstructions);
        clone.UseTemplates = this.UseTemplates;
        
        // Deep copy variables dictionary
        clone.Variables = new Dictionary<string, string>(this.Variables);
        
        // Deep copy UseMcps and ImagePatterns
        clone.UseMcps = new List<string>(this.UseMcps);
        clone.WithStdioMcps = new Dictionary<string, StdioMcpServerConfig>(this.WithStdioMcps);
        clone.ImagePatterns = new List<string>(this.ImagePatterns);
        
        return clone;
    }

    public override async Task<object> ExecuteAsync(bool interactive)
    {
        // Setup the named values
        _namedValues = new TemplateVariables(Variables);
        AddAgentsFileContentToTemplateVariables();
        
        // Initialize slash command handlers
        _cycoDmdCommandHandler = new SlashCycoDmdCommandHandler(this);
        _titleCommandHandler = new SlashTitleCommandHandler();

        // Transfer known settings to the command
        var maxOutputTokens = ConfigStore.Instance.GetFromAnyScope(KnownSettings.AppMaxOutputTokens).AsInt(defaultValue: 0);
        if (maxOutputTokens > 0) MaxOutputTokens = maxOutputTokens;

        MaxPromptTokenTarget = ConfigStore.Instance.GetFromAnyScope(KnownSettings.AppMaxPromptTokens).AsInt(DefaultMaxPromptTokenTarget);
        MaxToolTokenTarget = ConfigStore.Instance.GetFromAnyScope(KnownSettings.AppMaxToolTokens).AsInt(DefaultMaxToolTokenTarget);
        MaxChatTokenTarget = ConfigStore.Instance.GetFromAnyScope(KnownSettings.AppMaxChatTokens).AsInt(DefaultMaxChatTokenTarget);

        // Ground the filenames (in case they're templatized, or auto-save is enabled).
        InputChatHistory = ChatHistoryFileHelpers.GroundInputChatHistoryFileName(InputChatHistory, LoadMostRecentChatHistory)?.ReplaceValues(_namedValues);
        AutoSaveOutputChatHistory = ChatHistoryFileHelpers.GroundAutoSaveChatHistoryFileName()?.ReplaceValues(_namedValues);
        AutoSaveOutputTrajectory = ChatHistoryFileHelpers.GroundAutoSaveTrajectoryFileName()?.ReplaceValues(_namedValues);
        OutputChatHistory = OutputChatHistory != null ? FileHelpers.GetFileNameFromTemplate(OutputChatHistory, OutputChatHistory)?.ReplaceValues(_namedValues) : null;
        OutputTrajectory = OutputTrajectory != null ? FileHelpers.GetFileNameFromTemplate(OutputTrajectory, OutputTrajectory)?.ReplaceValues(_namedValues) : null;
        
        // Set the conversation file path for title operations (after it's been grounded)
        _titleCommandHandler.SetFilePath(AutoSaveOutputChatHistory);
        
        // Initialize trajectory files
        _trajectoryFile = new TrajectoryFile(OutputTrajectory);
        _autoSaveTrajectoryFile = new TrajectoryFile(AutoSaveOutputTrajectory);

        // Ground the system prompt, added user messages, and InputInstructions.
        SystemPrompt = GroundSystemPrompt();
        UserPromptAdds = GroundUserPromptAdds();
        InputInstructions = GroundInputInstructions();

        // Create the function factory and add functions.
        var factory = new McpFunctionFactory();
        factory.AddFunctions(new DateAndTimeHelperFunctions());
        factory.AddFunctions(new ShellCommandToolHelperFunctions());
        factory.AddFunctions(new BackgroundProcessHelperFunctions());
        factory.AddFunctions(new StrReplaceEditorHelperFunctions());
        factory.AddFunctions(new ThinkingToolHelperFunction());
        factory.AddFunctions(new CodeExplorationHelperFunctions());
        factory.AddFunctions(new ImageHelperFunctions(this));
        
        // Add MCP functions if any are configured
        await AddMcpFunctions(factory);

        // Create the chat completions object with the external ChatClient and system prompt.
        var chatClient = ChatClientFactory.CreateChatClient(out var options);
        var chat = new FunctionCallingChat(chatClient, SystemPrompt, factory, options, MaxOutputTokens);
        _currentChat = chat;

        // Initialize metadata for new conversations
        if (chat.Metadata == null)
        {
            chat.UpdateMetadata(ConversationMetadataHelpers.CreateDefault());
        }

        try
        {
            // Add the user prompt messages to the chat.
            chat.AddUserMessages(
                UserPromptAdds,
                maxPromptTokenTarget: MaxPromptTokenTarget,
                maxChatTokenTarget: MaxChatTokenTarget);

            // Load the chat history from the file.
            var loadChatHistory = !string.IsNullOrEmpty(InputChatHistory);
            if (loadChatHistory)
            {
                chat.LoadChatHistory(InputChatHistory!,
                    maxPromptTokenTarget: MaxPromptTokenTarget,
                    maxToolTokenTarget: MaxToolTokenTarget,
                    maxChatTokenTarget: MaxChatTokenTarget,
                    useOpenAIFormat: ChatHistoryDefaults.UseOpenAIFormat);
                
                // Update console title with loaded conversation title
                ConsoleTitleHelper.UpdateWindowTitle(chat.Metadata);
            }

            // Check to make sure we're either in interactive mode, or have input instructions.
            if (!interactive && InputInstructions.Count == 0)
            {
                ConsoleHelpers.WriteWarning("\nNo input instructions provided. Exiting.");
                return 1;
            }

            while (true)
            {
                DisplayUserPrompt();
                var userPrompt = interactive
                    ? InteractivelyReadLineOrSimulateInput(InputInstructions, "exit")
                    : ReadLineOrSimulateInput(InputInstructions, "exit");
                if (string.IsNullOrWhiteSpace(userPrompt) || userPrompt == "exit")
                {
                    // Show any pending notifications before exiting
                    CheckAndShowPendingNotifications(chat);
                    break;
                }

                var (skipAssistant, replaceUserPrompt) = await TryHandleChatCommandAsync(chat, userPrompt);
                if (skipAssistant) continue; // Some chat commands don't require a response from the assistant.

                var shouldReplaceUserPrompt = !string.IsNullOrEmpty(replaceUserPrompt);
                if (shouldReplaceUserPrompt) DisplayPromptReplacement(userPrompt, replaceUserPrompt);

                var giveAssistant = shouldReplaceUserPrompt ? replaceUserPrompt! : userPrompt;

                CheckAndShowPendingNotifications(chat);
                DisplayAssistantLabel();
                
                var imageFiles = ImagePatterns.Any() ? ImageResolver.ResolveImagePatterns(ImagePatterns) : new List<string>();
                ImagePatterns.Clear();
                
                var response = await CompleteChatStreamingAsync(chat, giveAssistant, imageFiles,
                    (messages) => HandleUpdateMessages(messages),
                    (update) => HandleStreamingChatCompletionUpdate(update),
                    (name, args) => HandleFunctionCallApproval(factory, name, args!),
                    (name, args, result) => HandleFunctionCallCompleted(name, args, result));
                ConsoleHelpers.WriteLine("\n", overrideQuiet: true);
            }

            return 0;
        }
        finally
        {
            // Clean up resources
            await chat.DisposeAsync();
            
            // If we have an MCP function factory, dispose its resources too
            if (factory is McpFunctionFactory mcpFactory)
            {
                await mcpFactory.DisposeAsync();
            }
        }
    }

    private string GroundSystemPrompt()
    {
        SystemPrompt ??= GetBuiltInSystemPrompt();
        SystemPrompt = GroundPromptName(SystemPrompt);
        SystemPrompt = GroundSlashPrompt(SystemPrompt);

        var processed = ProcessTemplate(SystemPrompt + "\n\n" + GetSystemPromptAdds());
        return _namedValues != null ? processed.ReplaceValues(_namedValues) : processed;
    }
    
    /// <summary>
    /// Adds AGENTS.md file content to template variables
    /// </summary>
    private void AddAgentsFileContentToTemplateVariables()
    {
        if (_namedValues == null)
            return;
            
        var agentsFile = AgentsFileHelpers.FindAgentsFile();
        if (agentsFile != null)
        {
            var agentsContent = FileHelpers.ReadAllText(agentsFile);
            if (!string.IsNullOrEmpty(agentsContent))
            {
                // Store the AGENTS.md content as a template variable
                _namedValues.Set("agents.md", agentsContent);
                _namedValues.Set("agents.file", Path.GetFileName(agentsFile));
                _namedValues.Set("agents.path", agentsFile);

                ConsoleHelpers.WriteDebugLine($"Added AGENTS.md content from {agentsFile} as template variable");
            }
        }
    }

    private List<string> GroundUserPromptAdds()
    {
        return UserPromptAdds
            .Select(x => GroundSlashPrompt(x))
            .Select(x => UseTemplates ? ProcessTemplate(x) : x)
            .Select(x => _namedValues != null ? x.ReplaceValues(_namedValues) : x)
            .ToList();
    }

    private string GroundPromptName(string promptOrName)
    {
        var check = $"/{promptOrName}";
        var isPromptCommand = _promptCommandHandler.IsCommand(check);
        return isPromptCommand
            ? _promptCommandHandler.HandleCommand(check) ?? promptOrName
            : promptOrName;
    }

    private string GroundSlashPrompt(string promptOrSlashPromptCommand)
    {
        var isPromptCommand = _promptCommandHandler.IsCommand(promptOrSlashPromptCommand);
        return isPromptCommand
            ? _promptCommandHandler.HandleCommand(promptOrSlashPromptCommand) ?? promptOrSlashPromptCommand
            : promptOrSlashPromptCommand;
    }
    
    private List<string> GroundInputInstructions()
    {
        return InputInstructions
            .Select(x => UseTemplates ? ProcessTemplate(x) : x)
            .Select(x => _namedValues != null ? x.ReplaceValues(_namedValues) : x)
            .ToList();
    }

    private string GetBuiltInSystemPrompt()
    {
        if (EmbeddedFileHelpers.EmbeddedStreamExists("prompts.system.md"))
        {
            var text = EmbeddedFileHelpers.ReadEmbeddedStream("prompts.system.md")!;
            return ProcessTemplate(text);
        }

        return "You are a helpful AI assistant.";
    }

    private string GetSystemPromptAdds()
    {
        var processedAdds = SystemPromptAdds
            .Select(x => UseTemplates ? ProcessTemplate(x) : x)
            .ToList();
        var joined = string.Join("\n\n", processedAdds);
        return joined.Trim(new char[] { '\n', '\r', ' ' });
    }

    private async Task<(bool skipAssistant, string? giveAssistant)> TryHandleChatCommandAsync(FunctionCallingChat chat, string userPrompt)
    {
        bool skipAssistant = false;
        string? giveAssistant = null;

        if (_titleCommandHandler?.TryHandle(userPrompt, chat, out var titleResult) == true)
        {
            skipAssistant = true;
            
            // Handle immediate save if requested
            if (titleResult == SlashCommandResult.NeedsSave)
            {
                TrySaveChatHistoryToFileWithMetadata(AutoSaveOutputChatHistory);
                if (OutputChatHistory != AutoSaveOutputChatHistory)
                {
                    TrySaveChatHistoryToFileWithMetadata(OutputChatHistory);
                }
                ConsoleHelpers.WriteDebugLine("Title command triggered immediate save");
            }
        }
        else if (userPrompt.StartsWith("/save"))
        {
            skipAssistant = HandleSaveChatHistoryCommand(chat, userPrompt.Substring("/save".Length).Trim());
        }
        else if (userPrompt == "/clear")
        {
            skipAssistant = HandleClearChatHistoryCommand(chat);
        }
        else if (userPrompt == "/cost")
        {
            skipAssistant = HandleShowCostCommand();
        }
        else if (userPrompt == "/help")
        {
            skipAssistant = HandleHelpCommand();
        }
        else if (_cycoDmdCommandHandler?.IsCommand(userPrompt) == true)
        {
            skipAssistant = await HandleCycoDmdCommand(chat, userPrompt);
        }
        else if (_promptCommandHandler.IsCommand(userPrompt))
        {
            var handled = HandlePromptCommand(chat, userPrompt, out giveAssistant);
            if (!handled) giveAssistant = null;
        }

        return (skipAssistant, giveAssistant);
    }

    private void DisplayPromptReplacement(string userPrompt, string? replaceUserPrompt)
    {
        ConsoleHelpers.WriteLine($"\rUser: {userPrompt} => {replaceUserPrompt}", ConsoleColor.DarkGray, overrideQuiet: true);
    }

    private bool HandlePromptCommand(FunctionCallingChat chat, string userPrompt, out string? giveAssistant)
    {
        giveAssistant = _promptCommandHandler.HandleCommand(userPrompt);
        return !string.IsNullOrEmpty(giveAssistant);
    }

    private async Task<bool> HandleCycoDmdCommand(FunctionCallingChat chat, string userPrompt)
    {
        var userFunctionName = _cycoDmdCommandHandler?.GetCommandName(userPrompt) ?? "";
        DisplayUserFunctionCall(userFunctionName, null);

        var result = await _cycoDmdCommandHandler?.HandleCommand(userPrompt)!;
        if (result != null) chat.AddUserMessage(result);

        DisplayUserFunctionCall(userFunctionName, result ?? string.Empty);
        return true;
    }

    private bool HandleClearChatHistoryCommand(FunctionCallingChat chat)
    {
        chat.ClearChatHistory();
        _totalTokensIn = 0;
        _totalTokensOut = 0;
        ConsoleHelpers.WriteLine("Cleared chat history.\n", ConsoleColor.Yellow, overrideQuiet: true);
        return true;
    }

    private bool HandleSaveChatHistoryCommand(FunctionCallingChat chat, string? fileName = null)
    {
        var useDefaultFileName = string.IsNullOrEmpty(fileName);
        if (useDefaultFileName) fileName = "chat-history.jsonl";

        ConsoleHelpers.Write($"Saving {fileName} ...", ConsoleColor.Yellow, overrideQuiet: true);
        chat.SaveChatHistoryToFile(fileName!, useOpenAIFormat: ChatHistoryDefaults.UseOpenAIFormat);
        ConsoleHelpers.WriteLine("Saved!\n", ConsoleColor.Yellow, overrideQuiet: true);

        return true;
    }

    private bool HandleShowCostCommand()
    {
        ConsoleHelpers.WriteLine($"Tokens: {_totalTokensIn} in, {_totalTokensOut} out\n", ConsoleColor.Yellow, overrideQuiet: true);
        return true;
    }

    private bool HandleHelpCommand()
    {
        var helpBuilder = new StringBuilder();
        
        // Built-in chat commands
        helpBuilder.AppendLine("BUILT-IN");
        helpBuilder.AppendLine();
        helpBuilder.AppendLine("  /save     Save chat history to file");
        helpBuilder.AppendLine("  /clear    Clear chat history");
        helpBuilder.AppendLine("  /cost     Show token usage statistics");
        helpBuilder.AppendLine("  /help     Show this help message");
        helpBuilder.AppendLine();
        
        // CYCODMD integration commands
        helpBuilder.AppendLine("EXTERNAL");
        helpBuilder.AppendLine();
        helpBuilder.AppendLine("  /files    List files matching pattern");
        helpBuilder.AppendLine("  /file     Get contents of a file");
        helpBuilder.AppendLine("  /find     Find content in files");
        helpBuilder.AppendLine();
        helpBuilder.AppendLine("  /search   Search the web");
        helpBuilder.AppendLine("  /get      Get content from URL");
        helpBuilder.AppendLine();
        helpBuilder.AppendLine("  /run      Run a command");
        helpBuilder.AppendLine("  /image    Add image file(s) to conversation");
        helpBuilder.AppendLine();

        // User-defined prompts
        helpBuilder.AppendLine("PROMPTS");
        helpBuilder.AppendLine();
        
        // Get all scopes for prompt files
        bool foundPrompts = false;
        foreach (var scope in new[] { ConfigFileScope.Local, ConfigFileScope.User, ConfigFileScope.Global })
        {
            var promptDir = PromptFileHelpers.FindPromptDirectoryInScope(scope);
            if (promptDir == null || !Directory.Exists(promptDir)) continue;
            
            var promptFiles = Directory.GetFiles(promptDir, "*.prompt")
                .OrderBy(file => Path.GetFileNameWithoutExtension(file))
                .ToList();
                
            if (promptFiles.Count == 0) continue;
            
            foundPrompts = true;
            
            // Format location header consistently
            var locationDisplay = CommonDisplayHelpers.FormatLocationHeader(promptDir, scope);
            helpBuilder.AppendLine($"  LOCATION: {locationDisplay}");
            helpBuilder.AppendLine();

            var indent = new string(' ', 4);
            
            foreach (var promptFile in promptFiles)
            {
                var promptName = Path.GetFileNameWithoutExtension(promptFile);
                helpBuilder.AppendLine($"{indent}/{promptName}");
                helpBuilder.AppendLine();

                // Get prompt content and truncate it for preview
                var promptText = PromptFileHelpers.GetPromptText(promptName, scope)!;
                var truncatedContent = CommonDisplayHelpers.TruncateContent(
                    promptText, 
                    CommonDisplayHelpers.DefaultMaxContentLines, 
                    CommonDisplayHelpers.DefaultMaxContentWidth, 
                    8);  // deeper indentation for content
                
                helpBuilder.AppendLine(truncatedContent);
                helpBuilder.AppendLine();
            }
            
            helpBuilder.AppendLine();
        }
        
        if (!foundPrompts)
        {
            helpBuilder.AppendLine("  No custom prompts found.");
            helpBuilder.AppendLine();
        }
        
        var indented = "\n  " + helpBuilder.ToString().Replace("\n", "\n  ").TrimEnd() + "\n";
        ConsoleHelpers.WriteLine(indented, overrideQuiet: true);
        return true;
    }

    private async Task<string> CompleteChatStreamingAsync(
        FunctionCallingChat chat,
        string userPrompt,
        IEnumerable<string> imageFiles,
        Action<IList<ChatMessage>>? messageCallback = null,
        Action<ChatResponseUpdate>? streamingCallback = null,
        Func<string, string?, bool>? approveFunctionCall = null,
        Action<string, string, object?>? functionCallCallback = null)
    {
        messageCallback = TryCatchHelpers.NoThrowWrap(messageCallback);
        streamingCallback = TryCatchHelpers.NoThrowWrap(streamingCallback);
        functionCallCallback = TryCatchHelpers.NoThrowWrap(functionCallCallback);

        try
        {
            var response = await chat.CompleteChatStreamingAsync(userPrompt, imageFiles,
                (messages) => messageCallback?.Invoke(messages),
                (update) => streamingCallback?.Invoke(update),
                (name, args) => approveFunctionCall?.Invoke(name, args) ?? true,
                (name, args, result) => functionCallCallback?.Invoke(name, args, result));

            return response;
        }
        catch (Exception ex)
        {
            ConsoleHelpers.LogException(ex, "Exception occurred during chat completion", showToUser: false);
            SaveExceptionHistory(chat);
            throw;
        }
    }

    private string? ReadLineOrSimulateInput(List<string> inputInstructions, string? defaultOnEndOfInput = null)
    {
        while (inputInstructions?.Count > 0)
        {
            var input = inputInstructions[0];
            inputInstructions.RemoveAt(0);

            var empty = string.IsNullOrEmpty(input);
            if (!empty)
            {
                ConsoleHelpers.WriteLine(input);
                return input;
            }
        }

        if (Console.IsInputRedirected)
        {
            var line = Console.ReadLine();
            line ??= defaultOnEndOfInput;
            if (line != null) ConsoleHelpers.WriteLine(line);
            return line;
        }

        return defaultOnEndOfInput;
    }

    private string? InteractivelyReadLineOrSimulateInput(List<string> inputInstructions, string? defaultOnEndOfInput = null)
    {
        var input = ReadLineOrSimulateInput(inputInstructions, null);
        if (input != null) return input;

        string? line = Console.ReadLine();
        if (line == null) return defaultOnEndOfInput;

        var isMultiLine = MultilineInputHelper.StartsWithBackticks(line);
        return isMultiLine ? InteractivelyReadMultiLineInput(line) : line;
    }

    private string? InteractivelyReadMultiLineInput(string firstLine)    
    {
        ConsoleHelpers.WriteLine("Entering multiline mode. Enter a matching number of backticks on a line by itself to end.", ConsoleColor.DarkGray);
        return MultilineInputHelper.ReadMultilineInput(firstLine);
    }

    private void HandleUpdateMessages(IList<ChatMessage> messages)
    {
        messages.TryTrimToTarget(MaxPromptTokenTarget, MaxToolTokenTarget, MaxChatTokenTarget);

        // Auto-save with metadata support
        TrySaveChatHistoryToFileWithMetadata(AutoSaveOutputChatHistory);
        if (OutputChatHistory != AutoSaveOutputChatHistory)
        {
            TrySaveChatHistoryToFileWithMetadata(OutputChatHistory);
        }
        
        // Generate title after first meaningful exchange (unless disabled by environment variable)
        var envDisabled = Environment.GetEnvironmentVariable("CYCOD_DISABLE_TITLE_GENERATION") == "true";
        var shouldGenerate = ShouldGenerateTitle(messages);
        
        ConsoleHelpers.WriteDebugLine($"Title generation check: attempted={_titleGenerationAttempted}, shouldGenerate={shouldGenerate}, envDisabled={envDisabled}, messageCount={messages.Count}");
        
        if (!_titleGenerationAttempted && shouldGenerate && !envDisabled)
        {
            _titleGenerationAttempted = true;
            ConsoleHelpers.WriteDebugLine($"🎯 Triggering title generation for file: {AutoSaveOutputChatHistory}");
            _ = Task.Run(async () => await TryGenerateAndSaveTitle(AutoSaveOutputChatHistory));
        }
        
        var lastMessage = messages.LastOrDefault();
        _autoSaveTrajectoryFile.AppendMessage(lastMessage);
        _trajectoryFile.AppendMessage(lastMessage);
    }

    private void TrySaveChatHistoryToFile(IList<ChatMessage> messages, string? filePath)
    {
        if (filePath == null) return;
        
        try
        {
            messages.SaveChatHistoryToFile(filePath, useOpenAIFormat: ChatHistoryDefaults.UseOpenAIFormat);
        }
        catch (Exception ex)
        {
            ConsoleHelpers.LogException(ex, $"Warning: Failed to save chat history to '{filePath}'", showToUser: true);
        }
    }

    /// <summary>
    /// Saves chat history to file with metadata support.
    /// </summary>
    private void TrySaveChatHistoryToFileWithMetadata(string? filePath)
    {
        if (filePath == null || _currentChat == null) return;
        
        try
        {
            _currentChat.SaveChatHistoryToFile(filePath, useOpenAIFormat: ChatHistoryDefaults.UseOpenAIFormat);
        }
        catch (Exception ex)
        {
            ConsoleHelpers.LogException(ex, $"Warning: Failed to save chat history with metadata to '{filePath}'", showToUser: true);
        }
    }

    /// <summary>
    /// Determines if a title should be generated for this conversation.
    /// </summary>
    private bool ShouldGenerateTitle(IList<ChatMessage> messages)
    {
        var hasUserAssistantExchange = messages.Any(m => m.Role == ChatRole.Assistant);
        var needsTitle = ConversationMetadataHelpers.ShouldGenerateTitle(_currentChat?.Metadata);

        return hasUserAssistantExchange && needsTitle;
    }

    /// <summary>
    /// Attempts to generate and save a title for the conversation using cycodmd.
    /// </summary>
    private async Task TryGenerateAndSaveTitle(string? filePath)
    {
        if (filePath == null || _currentChat?.Metadata == null) 
        {
            ConsoleHelpers.WriteDebugLine($"Skipping title generation: filePath={filePath}, metadata={_currentChat?.Metadata != null}");
            return;
        }
        
        ConsoleHelpers.WriteDebugLine($"Starting title generation for: {filePath}");
        
        try
        {
            var generatedTitle = await GenerateTitleAsync(filePath);
            if (!string.IsNullOrEmpty(generatedTitle))
            {
                ConsoleHelpers.WriteDebugLine($"Generated title: '{generatedTitle}', setting to metadata");
                ConversationMetadataHelpers.SetGeneratedTitle(_currentChat.Metadata, generatedTitle);
                
                // Save again with updated title
                ConsoleHelpers.WriteDebugLine($"Saving conversation with updated title to: {filePath}");
                _currentChat.SaveChatHistoryToFile(filePath, useOpenAIFormat: ChatHistoryDefaults.UseOpenAIFormat);
                
                // Update console title with new auto-generated title
                ConsoleTitleHelper.UpdateWindowTitle(_currentChat.Metadata);
                
                // Set pending notification for next assistant response
                _currentChat.SetPendingNotification(NotificationType.Title, generatedTitle);
                
                ConsoleHelpers.WriteDebugLine($"✅ Successfully generated and saved title: '{generatedTitle}'");
            }
            else
            {
                ConsoleHelpers.WriteDebugLine("❌ Title generation returned empty result");
            }
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteDebugLine($"❌ Failed to generate title: {ex.Message}");
        }
    }

    /// <summary>
    /// Generates a conversation title using cycodmd with environment variables to prevent infinite loops and auto-saving.
    /// </summary>
    private async Task<string?> GenerateTitleAsync(string conversationFilePath)
    {
        // Save original environment variable values
        var originalTitleGeneration = Environment.GetEnvironmentVariable("CYCOD_DISABLE_TITLE_GENERATION");
        var originalAutoSaveChat = Environment.GetEnvironmentVariable("CYCOD_AUTO_SAVE_CHAT_HISTORY");
        var originalAutoSaveTrajectory = Environment.GetEnvironmentVariable("CYCOD_AUTO_SAVE_TRAJECTORY");
        var originalAutoSaveLog = Environment.GetEnvironmentVariable("CYCOD_AUTO_SAVE_LOG");
        
        // Set environment variables for child process
        Environment.SetEnvironmentVariable("CYCOD_DISABLE_TITLE_GENERATION", "true");
        Environment.SetEnvironmentVariable("CYCOD_AUTO_SAVE_CHAT_HISTORY", "false");
        Environment.SetEnvironmentVariable("CYCOD_AUTO_SAVE_TRAJECTORY", "false");
        Environment.SetEnvironmentVariable("CYCOD_AUTO_SAVE_LOG", "false");
        
        string? tempFilePath = null;
        
        try 
        {
            // Check if file exists before trying to process it
            if (!File.Exists(conversationFilePath))
            {
                ConsoleHelpers.WriteDebugLine($"❌ File does not exist for title generation: {conversationFilePath}");
                return null;
            }
            
            // Create filtered content with only user and assistant messages
            var filteredContent = CreateFilteredConversationContent(conversationFilePath);
            if (string.IsNullOrEmpty(filteredContent))
            {
                ConsoleHelpers.WriteDebugLine($"❌ No meaningful conversation content found for title generation");
                return null;
            }
            
            // Write filtered content to temp file
            tempFilePath = Path.GetTempFileName();
            await File.WriteAllTextAsync(tempFilePath, filteredContent);
            
            ConsoleHelpers.WriteDebugLine($"Created filtered temp file: {tempFilePath}");
            ConsoleHelpers.WriteDebugLine($"Filtered content preview: {filteredContent.Substring(0, Math.Min(200, filteredContent.Length))}...");

            // Convert temp file path to Unix-style for bash
            var bashPath = tempFilePath.Replace("\\", "/");
            
            // Since we're using BashShellSession, always use bash commands
            var titleAgentSystemPrompt = "Generate a concise title for this conversation (3-5 words). No markdown formatting allowed. Only return the title text.";
            var command = $"cat \"{bashPath}\" | cycodmd - --instructions \"{titleAgentSystemPrompt}\"";

            ConsoleHelpers.WriteDebugLine($"Title generation command: {command}");
            
            var result = await BashShellSession.Instance.ExecuteCommandAsync(command, timeoutMs: 30000);
            
            ConsoleHelpers.WriteDebugLine($"Command exit code: {result.ExitCode}");
            ConsoleHelpers.WriteDebugLine($"Command output: {result.MergedOutput}");
            ConsoleHelpers.WriteDebugLine($"Command timeout: {result.IsTimeout}");
            
            if (result.ExitCode != 0 || result.IsTimeout)
            {
                ConsoleHelpers.WriteDebugLine($"Title generation command failed with exit code {result.ExitCode}");
                return null;
            }
            
            var title = result.MergedOutput?.Trim();
            ConsoleHelpers.WriteDebugLine($"Raw title result: '{title}'");
            
            return string.IsNullOrEmpty(title) ? null : SanitizeTitle(title);
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteDebugLine($"Title generation failed: {ex.Message}");
            return null;
        }
        finally
        {
            // Clean up temp file
            if (tempFilePath != null && File.Exists(tempFilePath))
            {
                try
                {
                    File.Delete(tempFilePath);
                    ConsoleHelpers.WriteDebugLine($"Cleaned up temp file: {tempFilePath}");
                }
                catch (Exception ex)
                {
                    ConsoleHelpers.WriteDebugLine($"Warning: Failed to delete temp file {tempFilePath}: {ex.Message}");
                }
            }
            
            // Restore original environment variables
            Environment.SetEnvironmentVariable("CYCOD_DISABLE_TITLE_GENERATION", originalTitleGeneration);
            Environment.SetEnvironmentVariable("CYCOD_AUTO_SAVE_CHAT_HISTORY", originalAutoSaveChat);
            Environment.SetEnvironmentVariable("CYCOD_AUTO_SAVE_TRAJECTORY", originalAutoSaveTrajectory);
            Environment.SetEnvironmentVariable("CYCOD_AUTO_SAVE_LOG", originalAutoSaveLog);
            
            ConsoleHelpers.WriteDebugLine("Restored original environment variables");
        }
    }

    /// <summary>
    /// Creates filtered conversation content containing only user and assistant messages.
    /// </summary>
    private string CreateFilteredConversationContent(string conversationFilePath)
    {
        try
        {
            // Load and parse the conversation
            var (metadata, messages) = AIExtensionsChatHelpers.ReadChatHistoryFromFileWithMetadata(conversationFilePath, useOpenAIFormat: ChatHistoryDefaults.UseOpenAIFormat);
            
            // Filter to only user and assistant messages
            var filteredMessages = messages
                .Where(m => m.Role == ChatRole.User || m.Role == ChatRole.Assistant)
                .ToList();
            
            ConsoleHelpers.WriteDebugLine($"Filtered {messages.Count} total messages down to {filteredMessages.Count} user/assistant messages");
            
            if (filteredMessages.Count == 0)
            {
                return string.Empty;
            }
            
            // Convert to simple text format for title generation
            var content = new System.Text.StringBuilder();
            content.AppendLine("Conversation between User and Assistant:");
            content.AppendLine();
            
            foreach (var message in filteredMessages)
            {
                var roleLabel = message.Role == ChatRole.User ? "User" : "Assistant";
                content.AppendLine($"{roleLabel}: {message.Text}");
                content.AppendLine();
            }
            
            return content.ToString();
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteDebugLine($"Failed to create filtered conversation content: {ex.Message}");
            return string.Empty;
        }
    }

    /// <summary>
    /// Checks for and displays any pending notifications.
    /// </summary>
    private void CheckAndShowPendingNotifications(FunctionCallingChat chat)
    {
        if (chat.HasPendingNotifications())
        {
            ConsoleHelpers.WriteLine();
            var notifications = chat.GetAndClearPendingNotifications();
            foreach (var notification in notifications)
            {
                var message = $"[{char.ToUpper(notification.Type[0])}{notification.Type[1..]} updated to: \"{notification.Content}\"]";
                ConsoleHelpers.WriteLine(message, ConsoleColor.DarkGray);
            }
        }
    }

    /// <summary>
    /// Cleans and formats a generated title.
    /// </summary>
    private string SanitizeTitle(string title)
    {
        // Remove quotes and extra whitespace
        var sanitized = title.Trim('"', '\'', ' ', '\n', '\r', '\t');
        
        // Remove any error messages or shell output that got mixed in
        var lines = sanitized.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        
        // Find the actual title (skip error lines, bash prompts, etc.)
        foreach (var line in lines)
        {
            var cleanLine = line.Trim();
            // Skip obvious error/command lines
            if (cleanLine.StartsWith("/usr/bin/bash:") ||
                cleanLine.StartsWith("$") ||
                cleanLine.StartsWith("bash:") ||
                cleanLine.Contains("command not found") ||
                string.IsNullOrWhiteSpace(cleanLine))
            {
                continue;
            }
            
            // This looks like actual content
            sanitized = cleanLine;
            break;
        }
        
        // Final cleanup
        sanitized = sanitized.Trim('"', '\'', ' ', '\n', '\r', '\t');
        
        // Limit length for display
        if (sanitized.Length > 80)
        {
            sanitized = sanitized.Substring(0, 77) + "...";
        }
        
        ConsoleHelpers.WriteDebugLine($"Sanitized title: '{title}' → '{sanitized}'");
        
        return sanitized;
    }

    private void HandleStreamingChatCompletionUpdate(ChatResponseUpdate update)
    {
        var usageUpdate = update.Contents
            .Where(x => x is UsageContent)
            .Cast<UsageContent>()
            .FirstOrDefault();
        var inTokens = usageUpdate?.Details.InputTokenCount ?? 0;
        var outTokens = usageUpdate?.Details.OutputTokenCount ?? 0;
        if (inTokens > 0 || outTokens > 0)
        {
            _totalTokensIn += inTokens;
            _totalTokensOut += outTokens;
            if (ConsoleHelpers.IsVerbose())
            {
                ConsoleHelpers.WriteLine($"\nTokens: {inTokens} in ({_totalTokensIn} total), {outTokens} out ({_totalTokensOut} total)", ConsoleColor.DarkYellow);
            }
        }

        var text = string.Join("", update.Contents
            .Where(x => x is TextContent)
            .Cast<TextContent>()
            .Select(x => x.Text)
            .ToList());
        DisplayAssistantResponse(text);
    }

    private bool HandleFunctionCallApproval(McpFunctionFactory factory, string name, string args)
    {
        var autoApprove = ShouldAutoApprove(factory, name);
        if (autoApprove) return true;

        while (true)
        {
            var approvePrompt = " Approve? (Y/n or ?) ";
            var erasePrompt = new string(' ', approvePrompt.Length);
            EnsureLineFeeds();
            DisplayGenericAssistantFunctionCall(name, args, null);
            ConsoleHelpers.Write(approvePrompt, ConsoleColor.Yellow);

            ConsoleKeyInfo? key = ShouldDenyFunctionCall(factory, name) ? null : ConsoleHelpers.ReadKey(true);
            DisplayGenericAssistantFunctionCall(name, args, null);
            ConsoleHelpers.Write(erasePrompt, ColorHelpers.MapColor(ConsoleColor.DarkBlue));
            DisplayGenericAssistantFunctionCall(name, args, null);

            if (key?.KeyChar == 'Y' || key?.Key == ConsoleKey.Enter)
            {
                ConsoleHelpers.WriteLine($"\b\b\b\b Approved (session)", ConsoleColor.Yellow);
                _approvedFunctionCallNames.Add(name);
                return true;
            }
            else if (key == null || key?.KeyChar == 'N')
            {
                _deniedFunctionCallNames.Add(name);
                ConsoleHelpers.WriteLine($"\b\b\b\b Declined (session)", ConsoleColor.Red);
                return false;
            }
            else if (key?.KeyChar == 'y')
            {
                ConsoleHelpers.WriteLine($"\b\b\b\b Approved (once)", ConsoleColor.Yellow);
                return true;
            }
            else if (key?.KeyChar == 'n')
            {
                ConsoleHelpers.WriteLine($"\b\b\b\b Declined (once)", ConsoleColor.Red);
                return false;
            }
            else if (key?.KeyChar == '?')
            {
                ConsoleHelpers.WriteLine($"\b\b\b\b Help\n", ConsoleColor.Yellow);
                ConsoleHelpers.WriteLine("  Enter: Approve this function call for this session");
                ConsoleHelpers.WriteLine("  Y: Approve this function call for this session");
                ConsoleHelpers.WriteLine("  y: Approve this function call for this one time");
                ConsoleHelpers.WriteLine("  N: Decline this function call for this session");
                ConsoleHelpers.WriteLine("  n: Decline this function call for this one time");
                ConsoleHelpers.WriteLine("  ?: Show this help message\n");
                ConsoleHelpers.Write("  See ");
                ConsoleHelpers.Write("cycod help function calls", ConsoleColor.Yellow);
                ConsoleHelpers.WriteLine(" for more information.\n");
            }
            else
            {
                ConsoleHelpers.WriteLine($"\b\b\b\b Invalid input", ConsoleColor.Red);
            }
        }
    }

    private void HandleFunctionCallCompleted(string name, string args, object? result)
    {
        DisplayAssistantFunctionCall(name, args, result);
    }

    private void DisplayUserPrompt()
    {
        ConsoleHelpers.Write("\rUser: ", ConsoleColor.Green);
        ConsoleHelpers.SetForegroundColor(ConsoleColor.White);
    }

    private void DisplayUserFunctionCall(string userFunctionName, string? result)
    {
        ConsoleHelpers.Write($"\ruser-function: {userFunctionName} => ", ConsoleColor.DarkGray);

        if (result == null) ConsoleHelpers.Write("...", ConsoleColor.DarkGray);
        if (result != null)
        {
            ConsoleHelpers.WriteLine(result, ConsoleColor.DarkGray);
            DisplayUserPrompt();
        }
    }

    private void DisplayAssistantLabel()
    {
        ConsoleHelpers.Write("\nAssistant: ", ConsoleColor.Green);
        _assistantResponseCharsSinceLabel = 0;
    }

    private void DisplayAssistantResponse(string text)
    {
        if (_assistantResponseCharsSinceLabel == 0 && text.StartsWith("\n"))
        {
            text = text.TrimStart(new char[] { '\n', '\r', ' ' });
        }

        ConsoleHelpers.Write(text, ConsoleColor.White, overrideQuiet: true);

        _assistantResponseCharsSinceLabel += text.Length;
        _asssistantResponseNeedsLF = !text.EndsWith("\n");
    }

    private void EnsureLineFeeds()
    {
        if (_asssistantResponseNeedsLF)
        {
            var oneLineFeedOrTwo = ConsoleHelpers.IsQuiet() ? "\n\n" : "\n";
            ConsoleHelpers.Write(oneLineFeedOrTwo, overrideQuiet: true);
            _asssistantResponseNeedsLF = false;
        }
    }

    private void DisplayAssistantFunctionCall(string name, string args, object? result)
    {
        EnsureLineFeeds();
        switch (name)
        {
            case "Think":
                DisplayAssistantThinkFunctionCall(args, result);
                break;

            default:
                DisplayGenericAssistantFunctionCall(name, args, result);
                break;
        }
    }

    private void DisplayAssistantThinkFunctionCall(string args, object? result)
    {
        var thought = JsonHelpers.GetJsonPropertyValue(args, "thought", args);
        var hasThought = !string.IsNullOrEmpty(thought);
        var resultText = result is TextContent textContent ? textContent.Text : result?.ToString();
        var hasResult = !string.IsNullOrEmpty(resultText);

        if (hasThought && !hasResult) ConsoleHelpers.WriteLine($"\n[THINKING]\n{thought}", ConsoleColor.DarkCyan);
        if (hasResult)
        {
            ConsoleHelpers.WriteLine($"\n{resultText}", ConsoleColor.DarkGray);
            DisplayAssistantLabel();
        }
    }

    private void DisplayGenericAssistantFunctionCall(string name, string args, object? result)
    {
        ConsoleHelpers.Write($"\rassistant-function: {name} {args} => ", ConsoleColor.DarkGray);
        
        if (result == null) ConsoleHelpers.Write("...", ConsoleColor.DarkGray);
        if (result != null)
        {
            var text = result as String ?? "non-string result";
            ConsoleHelpers.WriteLine(text, ConsoleColor.DarkGray);
            DisplayAssistantLabel();
        }
    }

    private string ProcessTemplate(string template)
    {
        if (string.IsNullOrEmpty(template))
        {
            return template;
        }

        var variables = new TemplateVariables(Variables);
        return TemplateHelpers.ProcessTemplate(template, variables);
    }

    private static void SaveExceptionHistory(FunctionCallingChat chat)
    {
        ConsoleHelpers.Write("\n\n", overrideQuiet: true);
        SaveExceptionChatHistory(chat);
        ConsoleHelpers.Write("\n", overrideQuiet: true);
        SaveExceptionTrajectory(chat);
    }

    private static void SaveExceptionChatHistory(FunctionCallingChat chat)
    {
        try
        {
            var fileName = FileHelpers.GetFileNameFromTemplate("exception-chat-history.jsonl", "{filebase}-{time}.{fileext}")!;
            var saveToFolderOnAccessDenied = ScopeFileHelpers.EnsureDirectoryInScope("exceptions", ConfigFileScope.User);
            chat.SaveChatHistoryToFile(fileName, useOpenAIFormat: ChatHistoryDefaults.UseOpenAIFormat, saveToFolderOnAccessDenied);
            ConsoleHelpers.WriteWarning($"SAVED: {fileName}");
        }
        catch (Exception ex)
        {
            ConsoleHelpers.LogException(ex, "Failed to save exception chat history");
            ConsoleHelpers.WriteWarning("Failed to save exception chat history.");
        }
    }

    private static void SaveExceptionTrajectory(FunctionCallingChat chat)
    {
        try
        {
            var fileName = FileHelpers.GetFileNameFromTemplate("exception-trajectory.md", "{filebase}-{time}.{fileext}")!;
            var saveToFolderOnAccessDenied = ScopeFileHelpers.EnsureDirectoryInScope("exceptions", ConfigFileScope.User);
            chat.SaveTrajectoryToFile(fileName, useOpenAIFormat: ChatHistoryDefaults.UseOpenAIFormat, saveToFolderOnAccessDenied);
            ConsoleHelpers.WriteWarning($"SAVED: {fileName}");
        }
        catch (Exception ex)
        {
            ConsoleHelpers.LogException(ex, "Failed to save exception trajectory", showToUser: false);
            ConsoleHelpers.WriteWarning("Failed to save exception trajectory.");
        }
    }

    private async Task AddMcpFunctions(McpFunctionFactory factory)
    {
        Logger.Info("MCP: Initializing MCP functions for chat");
        
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var clients = await CreateMcpClientsFromConfig();
        await CreateWithMcpClients(clients);
        
        Logger.Info($"MCP: Created {clients.Count} MCP clients in {sw.ElapsedMilliseconds}ms");

        // Add all tools from all clients to the function factory
        foreach (var clientEntry in clients)
        {
            var serverName = clientEntry.Key;
            var client = clientEntry.Value;

            try
            {
                Logger.Info($"MCP: Adding tools from server '{serverName}'");
                await factory.AddMcpClientToolsAsync(client, serverName);
            }
            catch (Exception ex)
            {
                ConsoleHelpers.LogException(ex, $"Error adding tools from MCP server '{serverName}'");
            }
        }
        
        sw.Stop();
        Logger.Info($"MCP: Finished initializing MCP functions in {sw.ElapsedMilliseconds}ms");
    }

    private async Task<Dictionary<string, IMcpClient>> CreateMcpClientsFromConfig()
    {
        var noMcps = UseMcps.Count == 0;
        if (noMcps)
        {
            ConsoleHelpers.WriteDebugLine("MCP functions are disabled.");
            Logger.Info("MCP: Functions are disabled (no MCPs specified)");
            return new();
        }

        Logger.Info($"MCP: Looking for MCP servers matching criteria: {string.Join(", ", UseMcps)}");
        
        var allServers = McpFileHelpers.ListMcpServers(ConfigFileScope.Any);
        Logger.Verbose($"MCP: Found {allServers.Count} total MCP servers in configuration");
        
        var servers = allServers
            .Where(kvp => ShouldUseMcpFromConfig(kvp.Key, kvp.Value))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            
        Logger.Info($"MCP: Selected {servers.Count} MCP servers matching criteria");
        
        if (servers.Count > 0)
        {
            var serverList = string.Join(", ", servers.Keys);
            Logger.Info($"MCP: Creating clients for servers: {serverList}");
        }

        var clients = await McpClientManager.CreateClientsAsync(servers);
        if (clients.Count == 0)
        {
            var criteria = string.Join(", ", UseMcps);
            ConsoleHelpers.WriteDebugLine($"Searched {UseMcps.Count} MCPs; found no MCPs matching criteria: {criteria}");
            Logger.Warning($"MCP: No functioning MCP servers found matching criteria: {criteria}");
            return new();
        }

        return clients;
    }

    private async Task CreateWithMcpClients(Dictionary<string, IMcpClient> clients)
    {
        var servers = WithStdioMcps;

        var noMcps = servers.Count == 0;
        if (noMcps)
        {
            ConsoleHelpers.WriteDebugLine("MCP functions are disabled.");
            Logger.Info("MCP: No ad-hoc MCP servers specified");
            return;
        }

        Logger.Info($"MCP: Creating {servers.Count} ad-hoc MCP server(s)");
        
        var start = DateTime.Now;
        ConsoleHelpers.Write($"Loading {servers.Count} ad-hoc MCP server(s) ...", ConsoleColor.DarkGray);

        var loaded = 0;
        var failed = 0;
        
        foreach (var serverName in servers.Keys)
        {
            var stdioConfig = servers[serverName];
            try
            {
                Logger.Info($"MCP: Creating ad-hoc MCP client '{serverName}' with command: {stdioConfig.Command}");
                
                var sw = System.Diagnostics.Stopwatch.StartNew();
                var client = await McpClientFactory.CreateAsync(new StdioClientTransport(new()
                {
                    Name = serverName,
                    Command = stdioConfig.Command,
                    Arguments = stdioConfig.Args,
                    EnvironmentVariables = stdioConfig.Env,
                }));
                sw.Stop();

                ConsoleHelpers.WriteDebugLine($"Created MCP client for '{serverName}' with command: {stdioConfig.Command}");
                Logger.Info($"MCP: Successfully created ad-hoc MCP client '{serverName}' in {sw.ElapsedMilliseconds}ms");
                
                clients[serverName] = client;
                loaded++;
            }
            catch (Exception ex)
            {
                failed++;
                ConsoleHelpers.LogException(ex, $"Failed to create MCP client for '{serverName}'");
            }
        }

        var duration = TimeSpanFormatter.FormatMsOrSeconds(DateTime.Now - start);
        var statusMsg = $"Loaded {loaded} ad-hoc MCP server(s) ({duration})";
        ConsoleHelpers.WriteLine($"\r{statusMsg}", ConsoleColor.DarkGray);
        
        if (failed > 0)
        {
            Logger.Warning($"MCP: {statusMsg}, {failed} server(s) failed to load");
        }
        else
        {
            Logger.Info($"MCP: {statusMsg}");
        }
    }

    private bool ShouldUseMcpFromConfig(string name, IMcpServerConfigItem item)
    {
        var shouldUse = UseMcps.Contains(name) || UseMcps.Contains("*");
        
        if (shouldUse)
        {
            Logger.Verbose($"MCP: Selected server '{name}' of type {item.Type} (matched criteria)");
        }
        
        return shouldUse;
    }

    private bool ShouldAutoApprove(McpFunctionFactory factory, string name)
    {
        var needToAddAutoApproveToolDefaults = _approvedFunctionCallNames.Count == 0;
        if (needToAddAutoApproveToolDefaults) AddAutoApproveToolDefaults();

        var approvedByName = _approvedFunctionCallNames.Contains(name);
        if (approvedByName) return true;

        var approveAll = _approvedFunctionCallNames.Contains("*");
        if (approveAll) return true;

        var approveAllRunFunctions = _approvedFunctionCallNames.Contains("run");
        var approveAllWriteFunctions = approveAllRunFunctions || _approvedFunctionCallNames.Contains("write");
        var approveAllReadFunctions = approveAllWriteFunctions || _approvedFunctionCallNames.Contains("read");

        var isReadonly = factory.IsReadOnlyFunction(name);
        var approved = isReadonly switch
        {
            true => approveAllReadFunctions,
            false => approveAllWriteFunctions,
            null => approveAllRunFunctions
        };

        return approved;
    }

    private void AddAutoApproveToolDefaults()
    {
        _approvedFunctionCallNames.Add("Think");

        var items = ConfigStore.Instance.GetFromAnyScope(KnownSettings.AppAutoApprove).AsList();
        foreach (var item in items)
        {
            _approvedFunctionCallNames.Add(item);
        }
    }

    private bool ShouldDenyFunctionCall(McpFunctionFactory factory, string name)
    {
        var needToAddAutoDenyToolDefaults = _deniedFunctionCallNames.Count == 0;
        if (needToAddAutoDenyToolDefaults) AddAutoDenyToolDefaults();

        var deniedByName = _deniedFunctionCallNames.Contains(name);
        if (deniedByName) return true;

        var denyAll = _deniedFunctionCallNames.Contains("*");
        if (denyAll) return true;

        var denyAllRunFunctions = _deniedFunctionCallNames.Contains("run");
        var denyAllWriteFunctions = _deniedFunctionCallNames.Contains("write");
        var denyAllReadFunctions = _deniedFunctionCallNames.Contains("read");

        var isReadonly = factory.IsReadOnlyFunction(name);
        var denied = isReadonly switch
        {
            true => denyAllReadFunctions,
            false => denyAllWriteFunctions,
            null => denyAllRunFunctions
        };

        return denied;
    }

    private void AddAutoDenyToolDefaults()
    {
        var items = ConfigStore.Instance.GetFromAnyScope(KnownSettings.AppAutoDeny).AsList();
        foreach (var item in items)
        {
            _deniedFunctionCallNames.Add(item);
        }
    }

    public string? SystemPrompt { get; set; }
    public List<string> SystemPromptAdds { get; set; } = new List<string>();
    public List<string> UserPromptAdds { get; set; } = new List<string>();

    public int MaxPromptTokenTarget { get; set; }
    public int MaxToolTokenTarget { get; set; }
    public int? MaxOutputTokens { get; set; }
    public int MaxChatTokenTarget { get; set; }

    public bool LoadMostRecentChatHistory = false;
    public string? InputChatHistory;
    public string? OutputChatHistory;
    public string? OutputTrajectory;
    
    public string? AutoSaveOutputChatHistory;
    public string? AutoSaveOutputTrajectory;

    public List<string> InputInstructions = new();
    public bool UseTemplates = true;

    public List<string> UseMcps = new();
    public Dictionary<string, StdioMcpServerConfig> WithStdioMcps = new();
    
    public List<string> ImagePatterns = new();

    private int _assistantResponseCharsSinceLabel = 0;
    private bool _asssistantResponseNeedsLF = false;

    private INamedValues? _namedValues;
    private TrajectoryFile _trajectoryFile = null!;
    private TrajectoryFile _autoSaveTrajectoryFile = null!;
    private SlashCycoDmdCommandHandler? _cycoDmdCommandHandler;
    private SlashPromptCommandHandler _promptCommandHandler = new();
    private SlashTitleCommandHandler? _titleCommandHandler;
    private FunctionCallingChat? _currentChat;
    private bool _titleGenerationAttempted = false;

    private long _totalTokensIn = 0;
    private long _totalTokensOut = 0;

    private const int DefaultMaxPromptTokenTarget = 50000;
    private const int DefaultMaxToolTokenTarget = 50000;
    private const int DefaultMaxChatTokenTarget = 160000;

    private HashSet<string> _approvedFunctionCallNames = new HashSet<string>();
    private HashSet<string> _deniedFunctionCallNames = new HashSet<string>();
}
