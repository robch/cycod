using System.Diagnostics;
using OpenAI.Chat;

class ChatCommand : Command
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

    public async Task<List<Task<int>>> ExecuteAsync(bool interactive)
    {
        // Transfer known settings to the command if not already set
        var maxOutputTokensSetting = ConfigStore.Instance.GetFromAnyScope(KnownSettings.AppMaxTokens);
        var useMaxOutputTokenSetting = !MaxOutputTokens.HasValue && maxOutputTokensSetting.AsInt() > 0;
        if (useMaxOutputTokenSetting) MaxOutputTokens = maxOutputTokensSetting.AsInt();

        // Ground the filenames (in case they're templatized)
        InputChatHistory = FileHelpers.GetFileNameFromTemplate(InputChatHistory ?? "chat-history.jsonl", InputChatHistory);
        OutputChatHistory = FileHelpers.GetFileNameFromTemplate(OutputChatHistory ?? "chat-history.jsonl", OutputChatHistory);
        OutputTrajectory = FileHelpers.GetFileNameFromTemplate(OutputTrajectory ?? "trajectory.jsonl", OutputTrajectory);

        // Ground the system prompt, and InputInstructions.
        SystemPrompt ??= EnvironmentHelpers.FindEnvVar("OPENAI_SYSTEM_PROMPT") ?? GetBuiltInSystemPrompt();
        SystemPrompt = ProcessTemplate(SystemPrompt);
        InputInstructions = InputInstructions
            .Select(x => UseTemplates ? ProcessTemplate(x) : x)
            .ToList();

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

        // Load the chat history from the file.
        var loadChatHistory = !string.IsNullOrEmpty(InputChatHistory);
        if (loadChatHistory) chat.LoadChatHistory(InputChatHistory!);

        // Check to make sure we're either in interactive mode, or have input instructions.
        if (!interactive && InputInstructions.Count == 0)
        {
            ConsoleHelpers.WriteWarning("\nNo input instructions provided. Exiting.");
            return new List<Task<int>>() { Task.FromResult(1) };
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

        return new List<Task<int>>() { Task.FromResult(0) };
    }

    private string GetBuiltInSystemPrompt()
    {
        if (FileHelpers.EmbeddedStreamExists("prompts.system.md"))
        {
            var text = FileHelpers.ReadEmbeddedStream("prompts.system.md")!;
            return ProcessTemplate(text);
        }

        return "You are a helpful AI assistant.";
    }

    private static bool TryHandleChatCommand(FunctionCallingChat chat, string userPrompt)
    {
        if (userPrompt == "/save")
        {
            ConsoleHelpers.Write("\nSaving chat-history.jsonl ...");
            chat.SaveChatHistory("chat-history.jsonl");
            ConsoleHelpers.WriteLine("Saved!\n");
            return true;
        }

        return false;
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
            chat.SaveChatHistory(fileName);

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
        var tokenTarget = TrimTokenTarget.HasValue
            ? TrimTokenTarget.Value
            : 160000;

        const int whenTrimmingToolContentTarget = 10;
        const string snippedIndicator = "...snip...";

        if (messages.IsTooBig(tokenTarget))
        {
            messages.ReduceToolCallContent(tokenTarget, whenTrimmingToolContentTarget, snippedIndicator);
        }

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
        var text = string.Join("", update.ContentUpdate
            .Where(x => x.Kind == ChatMessageContentPartKind.Text)
            .Select(x => x.Text)
            .ToList());
        DisplayAssistantResponse(text);
    }

    private void HandleFunctionCallCompleted(string name, string args, string? result)
    {
        DisplayFunctionResult(name, args, result);
    }

    private void DisplayUserPrompt()
    {
        ConsoleHelpers.Write("User: ", ConsoleColor.Green);
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

    private void DisplayFunctionResult(string name, string args, string? result)
    {
        if (_asssistantResponseNeedsLF)
        {
            var oneLineFeedOrTwo = ConsoleHelpers.IsQuiet() ? "\n\n" : "\n";
            ConsoleHelpers.Write(oneLineFeedOrTwo, overrideQuiet: true);
            _asssistantResponseNeedsLF = false;
        }
        
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

    public int? TrimTokenTarget { get; set; }
    public int? MaxOutputTokens { get; set; }

    public string? InputChatHistory;
    public string? OutputChatHistory;
    public string? OutputTrajectory;

    public List<string> InputInstructions = new();
    public bool UseTemplates = true;

    public Dictionary<string, string> Variables { get; set; } = new Dictionary<string, string>();

    private int _assistantResponseCharsSinceLabel = 0;
    private bool _asssistantResponseNeedsLF = false;
}