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

    public async Task<List<Task<int>>> ExecuteAsync()
    {
        // Ground the filenames (in case they're templatized)
        InputChatHistory = FileHelpers.GetFileNameFromTemplate(InputChatHistory ?? "chat-history.jsonl", InputChatHistory);
        OutputChatHistory = FileHelpers.GetFileNameFromTemplate(OutputChatHistory ?? "chat-history.jsonl", OutputChatHistory);

        // Create the function factory and add functions.
        var factory = new FunctionFactory();
        factory.AddFunctions(typeof(TimeAndDateHelperFunctions));
        factory.AddFunctions(typeof(BashToolHelperFunctions));
        factory.AddFunctions(typeof(StrReplaceEditorHelperFunctions));

        // Create the chat completions object with the external ChatClient and system prompt.
        var chatClient = ChatClientFactory.CreateChatClientFromEnv();
        var systemPrompt = EnvironmentHelpers.FindEnvVar("OPENAI_SYSTEM_PROMPT") ?? "You are a helpful AI assistant.";
        var chat = new FunctionCallingChat(chatClient, systemPrompt, factory);

        // Load the chat history from the file.
        var loadChatHistory = !string.IsNullOrEmpty(InputChatHistory);
        if (loadChatHistory) chat.LoadChatHistory(InputChatHistory!);

        while (true)
        {
            DisplayUserPrompt();
            var userPrompt = ReadLineOrSimulateInput(InputInstructions, "exit");
            if (string.IsNullOrEmpty(userPrompt) || userPrompt == "exit") break;

            DisplayAssistantLabel();
            var response = await chat.GetChatCompletionsStreamingAsync(userPrompt,
                (messages) => HandleUpdateMessages(messages),
                (update) => HandleStreamingChatCompletionUpdate(update),
                (name, args, result) => HandleFunctionCallCompleted(name, args, result));
            ConsoleHelpers.WriteLine("\n");
        }

        return new List<Task<int>>() { Task.FromResult(0) };
    }

    private static string? ReadLineOrSimulateInput(List<string> inputInstructions, string? defaultOnEndOfRedirectedInput = null)
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
            line ??= defaultOnEndOfRedirectedInput;
            if (line != null) ConsoleHelpers.WriteLine(line);
            return line;
        }

        return Console.ReadLine();
    }

    private void HandleUpdateMessages(IList<ChatMessage> messages)
    {
        if (OutputChatHistory != null)
        {
            messages.SaveChatHistoryToFile(OutputChatHistory);
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

    private static void DisplayUserPrompt()
    {
        ConsoleHelpers.Write("User: ", ConsoleColor.Green);
        Console.ForegroundColor = ConsoleColor.White;
    }

    private static void DisplayAssistantLabel()
    {
        ConsoleHelpers.Write("\nAssistant: ", ConsoleColor.Green);
    }

    private static void DisplayAssistantResponse(string text)
    {
        ConsoleHelpers.Write(text, ConsoleColor.White);
    }

    private static void DisplayFunctionResult(string name, string args, string? result)
    {
        ConsoleHelpers.Write($"\rassistant-function: {name} {args} => ", ConsoleColor.DarkGray);
        
        if (result == null) ConsoleHelpers.Write("...", ConsoleColor.DarkGray);
        if (result != null)
        {
            ConsoleHelpers.WriteLine(result, ConsoleColor.DarkGray);
            DisplayAssistantLabel();
        }
    }

    public string? InputChatHistory;
    public string? OutputChatHistory;

    public List<string> InputInstructions = new();
}
