using OpenAI.Chat;

public class ChatCommand : Command
{
    public ChatCommand()
    {
    }

    override public bool IsEmpty()
    {
        return false;
    }

    override public string GetCommandName()
    {
        return "";
    }

    public ChatCommand Clone()
    {
        var clone = new ChatCommand();
        
        // Copy all properties
        clone.SystemPrompt = this.SystemPrompt;
        clone.SystemPromptAdds = new List<string>(this.SystemPromptAdds);
        clone.UserPromptAdds = new List<string>(this.UserPromptAdds);
        clone.TrimTokenTarget = this.TrimTokenTarget;
        clone.MaxOutputTokens = this.MaxOutputTokens;
        clone.LoadMostRecentChatHistory = this.LoadMostRecentChatHistory;
        clone.InputChatHistory = this.InputChatHistory;
        clone.OutputChatHistory = this.OutputChatHistory;
        clone.OutputTrajectory = this.OutputTrajectory;
        clone.InputInstructions = new List<string>(this.InputInstructions);
        clone.UseTemplates = this.UseTemplates;
        
        // Deep copy variables dictionary
        clone.Variables = new Dictionary<string, string>(this.Variables);
        
        return clone;
    }

    public async Task<int> ExecuteAsync(bool interactive)
    {
        // Transfer known settings to the command if not already set
        var maxOutputTokensSetting = ConfigStore.Instance.GetFromAnyScope(KnownSettings.AppMaxTokens);
        var useMaxOutputTokenSetting = !MaxOutputTokens.HasValue && maxOutputTokensSetting.AsInt() > 0;
        if (useMaxOutputTokenSetting) MaxOutputTokens = maxOutputTokensSetting.AsInt();

        // Ground the filenames (in case they're templatized, or auto-save is enabled).
        InputChatHistory = GroundInputChatHistoryFileName();
        OutputChatHistory = GroundOutputChatHistoryFileName();
        OutputTrajectory = GroundOutputTrajectoryFileName();

        // Ground the system prompt, added user messages, and InputInstructions.
        SystemPrompt = GroundSystemPrompt();
        UserPromptAdds = GroundUserPromptAdds();
        InputInstructions = GroundInputInstructions();

        // Create the function factory and add functions.
        var factory = new FunctionFactory();
        factory.AddFunctions(new DateAndTimeHelperFunctions());
        factory.AddFunctions(new ShellCommandToolHelperFunctions());
        factory.AddFunctions(new StrReplaceEditorHelperFunctions());
        factory.AddFunctions(new ThinkingToolHelperFunction());
        factory.AddFunctions(new CodeExplorationHelperFunctions());

        // Create the chat completions object with the external ChatClient and system prompt.
        var chatClient = ChatClientFactory.CreateChatClient();
        var chat = new FunctionCallingChat(chatClient, SystemPrompt, factory, MaxOutputTokens);

        // Add the user prompt messages to the chat.
        chat.AddUserMessages(UserPromptAdds);

        // Load the chat history from the file.
        var loadChatHistory = !string.IsNullOrEmpty(InputChatHistory);
        if (loadChatHistory) chat.LoadChatHistory(InputChatHistory!, TrimTokenTarget ?? DefaultTrimTokenTarget);

        // Check to make sure we're either in interactive mode, or have input instructions.
        if (!interactive && InputInstructions.Count == 0)
        {
            ConsoleHelpers.WriteWarning("\nNo input instructions provided. Exiting.");
            return 1;
        }

        while (true)
        {
            DisplayUserPrompt();
            var userPrompt = interactive && !Console.IsInputRedirected
                ? InteractivelyReadLineOrSimulateInput(InputInstructions, "exit")
                : ReadLineOrSimulateInput(InputInstructions, "exit");
            if (string.IsNullOrWhiteSpace(userPrompt) || userPrompt == "exit") break;

            var handled = TryHandleChatCommand(chat, userPrompt);
            if (handled) continue;

            DisplayAssistantLabel();
            var response = await CompleteChatStreamingAsync(chat, userPrompt,
                (messages) => HandleUpdateMessages(messages),
                (update) => HandleStreamingChatCompletionUpdate(update),
                (name, args, result) => HandleFunctionCallCompleted(name, args, result));
            ConsoleHelpers.WriteLine("\n", overrideQuiet: true);
        }

        return 0;
    }

    private string GroundSystemPrompt()
    {
        SystemPrompt ??= GetBuiltInSystemPrompt();
        return ProcessTemplate(SystemPrompt + "\n\n" + GetSystemPromptAdds());
    }

    private List<string> GroundUserPromptAdds()
    {
        return UserPromptAdds
            .Select(x => UseTemplates ? ProcessTemplate(x) : x)
            .ToList();
    }

    private List<string> GroundInputInstructions()
    {
        return InputInstructions
            .Select(x => UseTemplates ? ProcessTemplate(x) : x)
            .ToList();
    }

    private string? GroundInputChatHistoryFileName()
    {
        var mostRecentChatHistoryFileName = LoadMostRecentChatHistory
            ? FindMostRecentChatHistoryFile()
            : null;

        var mostRecentChatHistoryFileExists = FileHelpers.FileExists(mostRecentChatHistoryFileName);
        if (mostRecentChatHistoryFileExists)
        {
            InputChatHistory = mostRecentChatHistoryFileName;
        }

        return FileHelpers.GetFileNameFromTemplate(InputChatHistory ?? "chat-history.jsonl", InputChatHistory);
    }

    private string? FindMostRecentChatHistoryFile()
    {
        var userScopeDir = ConfigFileHelpers.GetScopeDirectoryPath(ConfigFileScope.User);
        var userScopeHistoryDir = Path.Combine(userScopeDir!, "history");
        var userChatHistoryFiles = Directory.Exists(userScopeHistoryDir)
            ? Directory.GetFiles(userScopeHistoryDir, "chat-history-*.jsonl")
            : Array.Empty<string>();

        var localFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "chat-history-*.jsonl");

        var files = userChatHistoryFiles.ToList()
            .Concat(localFiles).ToList()
            .OrderByDescending(f => new FileInfo(f).LastWriteTime);
        var mostRecent = files
            .OrderByDescending(f => new FileInfo(f).LastWriteTime)
            .FirstOrDefault();

        ConsoleHelpers.WriteLine($"Loading: {mostRecent}\n", ConsoleColor.DarkGray);
        return mostRecent;
    }

    private string? GroundOutputChatHistoryFileName()
    {
        var userSpecified = !string.IsNullOrEmpty(OutputChatHistory);
        var shouldAutoSave = !userSpecified && ConfigStore.Instance.GetFromAnyScope(KnownSettings.AppAutoSaveChatHistory).AsBool(true);
        if (shouldAutoSave)
        {
            var historyDir = EnsureHistoryDirectory();
            OutputChatHistory = Path.Combine(historyDir, "chat-history-{time}.jsonl");
        }

        return FileHelpers.GetFileNameFromTemplate(OutputChatHistory ?? "chat-history.jsonl", OutputChatHistory);
    }

    private string? GroundOutputTrajectoryFileName()
    {
        var userSpecified = !string.IsNullOrEmpty(OutputTrajectory);
        var shouldAutoSave = !userSpecified && ConfigStore.Instance.GetFromAnyScope(KnownSettings.AppAutoSaveTrajectory).AsBool(true);
        if (shouldAutoSave)
        {
            var historyDir = EnsureHistoryDirectory();
            OutputTrajectory = Path.Combine(historyDir, "trajectory-{time}.jsonl");
        }

        return FileHelpers.GetFileNameFromTemplate(OutputTrajectory ?? "trajectory.jsonl", OutputTrajectory);
    }

    private string EnsureHistoryDirectory()
    {
        var userScopeDir = ConfigFileHelpers.GetScopeDirectoryPath(ConfigFileScope.User);
        var historyDir = Path.Combine(userScopeDir!, "history");
        return DirectoryHelpers.EnsureDirectoryExists(historyDir);
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

    private bool TryHandleChatCommand(FunctionCallingChat chat, string userPrompt)
    {
        if (userPrompt == "/save")
        {
            return SaveChatHistory(chat);
        }
        else if (userPrompt == "/clear")
        {
            return ClearChatHistory(chat);
        }
        else if (userPrompt == "/cost")
        {
            return ShowCost();
        }

        return false;
    }

    private bool ClearChatHistory(FunctionCallingChat chat)
    {
        chat.ClearChatHistory();
        _totalTokensIn = 0;
        _totalTokensOut = 0;
        ConsoleHelpers.WriteLine("Cleared chat history.\n", ConsoleColor.Yellow, overrideQuiet: true);
        return true;
    }

    private bool SaveChatHistory(FunctionCallingChat chat)
    {
        ConsoleHelpers.Write("Saving chat-history.jsonl ...", ConsoleColor.Yellow, overrideQuiet: true);
        chat.SaveChatHistoryToFile("chat-history.jsonl");
        ConsoleHelpers.WriteLine("Saved!\n", ConsoleColor.Yellow, overrideQuiet: true);
        return true;
    }

    private bool ShowCost()
    {
        ConsoleHelpers.WriteLine($"Tokens: {_totalTokensIn} in, {_totalTokensOut} out\n", ConsoleColor.Yellow, overrideQuiet: true);
        return true;
    }

    private async Task<string> CompleteChatStreamingAsync(
        FunctionCallingChat chat,
        string userPrompt,
        Action<IList<ChatMessage>>? messageCallback = null,
        Action<StreamingChatCompletionUpdate>? streamingCallback = null,
        Action<string, string, string?>? functionCallCallback = null)
    {
        messageCallback = TryCatchHelpers.NoThrowWrap(messageCallback);
        streamingCallback = TryCatchHelpers.NoThrowWrap(streamingCallback);
        functionCallCallback = TryCatchHelpers.NoThrowWrap(functionCallCallback);

        try
        {
            var response = await chat.CompleteChatStreamingAsync(userPrompt,
                (messages) => HandleUpdateMessages(messages),
                (update) => HandleStreamingChatCompletionUpdate(update),
                (name, args, result) => HandleFunctionCallCompleted(name, args, result));

            return response;
        }
        catch (Exception)
        {
            var fileName = FileHelpers.GetFileNameFromTemplate("exception-chat-history.jsonl", "{filebase}-{time}.{fileext}")!;
            chat.SaveChatHistoryToFile(fileName);

            ConsoleHelpers.Write("\n\n", overrideQuiet: true);
            ConsoleHelpers.WriteWarning($"SAVED: {fileName}");
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

        return Console.ReadLine() ?? defaultOnEndOfInput;
    }

    private void HandleUpdateMessages(IList<ChatMessage> messages)
    {
        messages.TryTrimToTarget(TrimTokenTarget ?? DefaultTrimTokenTarget);

        if (OutputChatHistory != null)
        {
            messages.SaveChatHistoryToFile(OutputChatHistory);
        }
        
        if (OutputTrajectory != null && messages.Count > 0)
        {
            messages.Last().AppendMessageToTrajectoryFile(OutputTrajectory);
        }
    }

    private void HandleStreamingChatCompletionUpdate(StreamingChatCompletionUpdate update)
    {
        var inTokens = update.Usage?.InputTokenCount ?? 0;
        var outTokens = update.Usage?.OutputTokenCount ?? 0;
        if (inTokens > 0 || outTokens > 0)
        {
            _totalTokensIn += inTokens;
            _totalTokensOut += outTokens;
            if (ConsoleHelpers.IsVerbose())
            {
                ConsoleHelpers.WriteLine($"\nTokens: {inTokens} in ({_totalTokensIn} total), {outTokens} out ({_totalTokensOut} total)", ConsoleColor.DarkYellow);
            }
        }

        var text = string.Join("", update.ContentUpdate
            .Where(x => x.Kind == ChatMessageContentPartKind.Text)
            .Select(x => x.Text)
            .ToList());
        DisplayAssistantResponse(text);
    }

    private void HandleFunctionCallCompleted(string name, string args, string? result)
    {
        DisplayFunctionCall(name, args, result);
    }

    private void DisplayUserPrompt()
    {
        ConsoleHelpers.Write("\rUser: ", ConsoleColor.Green);
        Console.ForegroundColor = ConsoleColor.White;
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

    private void DisplayFunctionCall(string name, string args, string? result)
    {
        EnsureLineFeeds();
        switch (name)
        {
            case "Think":
                DisplayThinkFunctionCall(args, result);
                break;

            default:
                DisplayGenericFunctionCall(name, args, result);
                break;
        }
    }
    
    private void DisplayThinkFunctionCall(string args, string? result)
    {
        var thought = JsonHelpers.GetJsonPropertyValue(args, "thought", args);
        var hasThought = !string.IsNullOrEmpty(thought);
        var hasResult = !string.IsNullOrEmpty(result);

        if (hasThought && !hasResult) ConsoleHelpers.WriteLine($"\n[THINKING]\n{thought}", ConsoleColor.DarkCyan);
        if (hasResult)
        {
            ConsoleHelpers.WriteLine($"\n{result}", ConsoleColor.DarkGray);
            DisplayAssistantLabel();
        }
    }
    
    private void DisplayGenericFunctionCall(string name, string args, string? result)
    {
        ConsoleHelpers.Write($"\rassistant-function: {name} {args} => ", ConsoleColor.DarkGray);
        
        if (result == null) ConsoleHelpers.Write("...", ConsoleColor.DarkGray);
        if (result != null)
        {
            ConsoleHelpers.WriteLine(result, ConsoleColor.DarkGray);
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

    public string? SystemPrompt { get; set; }
    public List<string> SystemPromptAdds { get; set; } = new List<string>();
    public List<string> UserPromptAdds { get; set; } = new List<string>();

    public int? TrimTokenTarget { get; set; }
    public int? MaxOutputTokens { get; set; }

    public bool LoadMostRecentChatHistory = false;
    public string? InputChatHistory;
    public string? OutputChatHistory;
    public string? OutputTrajectory;

    public List<string> InputInstructions = new();
    public bool UseTemplates = true;

    public Dictionary<string, string> Variables { get; set; } = new Dictionary<string, string>();
    public List<ForEachVariable> ForEachVariables { get; set; } = new List<ForEachVariable>();

    private int _assistantResponseCharsSinceLabel = 0;
    private bool _asssistantResponseNeedsLF = false;

    private int _totalTokensIn = 0;
    private int _totalTokensOut = 0;

    private const int DefaultTrimTokenTarget = 160000;
}