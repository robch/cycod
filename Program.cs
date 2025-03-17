using OpenAI.Chat;
using System.Diagnostics;

public class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            SaveConsoleColor();
            await DoChat(ProcessDirectives(args));
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            RestoreConsoleColor();
        }
    }

    private static async Task DoChat(string[] args)
    {
        // Create the function factory and add functions.
        var factory = new FunctionFactory();
        factory.AddFunctions(typeof(TimeAndDateHelperFunctions));
        factory.AddFunctions(typeof(BashToolHelperFunctions));
        factory.AddFunctions(typeof(StrReplaceEditorHelperFunctions));

        // Create the chat completions object with the external ChatClient and system prompt.
        var chatClient = ChatClientFactory.CreateChatClientFromEnv();
        var systemPrompt = Environment.GetEnvironmentVariable("OPENAI_SYSTEM_PROMPT") ?? "You are a helpful AI assistant.";
        var chat = new FunctionCallingChat(chatClient, systemPrompt, factory);

        while (true)
        {
            DisplayUserPrompt();
            var userPrompt = Console.ReadLine();
            if (string.IsNullOrEmpty(userPrompt) || userPrompt == "exit") break;

            DisplayAssistantLabel();
            var response = await chat.GetChatCompletionsStreamingAsync(userPrompt,
                (messages) => HandleMessagesChanged(messages),
                (update) => HandleStreamingChatCompletionUpdate(update),
                (name, args, result) => HandleFunctionCallCompleted(name, args, result));
            Console.WriteLine("\n");
        }
    }

    private static void DisplayUserPrompt()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("User: ");
        Console.ForegroundColor = ConsoleColor.White;
    }

    private static void DisplayAssistantLabel()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("\nAssistant: ");
        Console.ForegroundColor = ConsoleColor.White;
    }

    private static void DisplayAssistantResponse(string text)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(text);
    }

    private static void DisplayFunctionResult(string name, string args, string? result)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write($"\rassistant-function: {name} {args} => ");
        
        if (result == null) Console.Write("...");
        if (result != null)
        {
            Console.WriteLine(result);
            DisplayAssistantLabel();
        }
    }

    private static void HandleMessagesChanged(IList<ChatMessage> messages)
    {
        messages.SaveChatHistoryToFile("chat-history.jsonl");
    }

    private static void HandleStreamingChatCompletionUpdate(StreamingChatCompletionUpdate update)
    {
        var text = string.Join("", update.ContentUpdate
            .Where(x => x.Kind == ChatMessageContentPartKind.Text)
            .Select(x => x.Text)
            .ToList());
        DisplayAssistantResponse(text);
    }

    private static void HandleFunctionCallCompleted(string name, string args, string? result)
    {
        DisplayFunctionResult(name, args, result);
    }

    private static string[] ProcessDirectives(string[] args)
    {
        return CheckWaitForDebugger(args);
    }

    private static string[] CheckWaitForDebugger(string[] args)
    {
        var debug = args.Length >= 2 && args[0] == "debug" && args[1] == "wait";
        if (debug)
        {
            args = args.Skip(2).ToArray();
            Console.Write("Waiting for debugger...");
            for (; !Debugger.IsAttached; Thread.Sleep(200))
            {
                Console.Write('.');
            }
            Console.WriteLine();
        }

        return args;
    }

    private static void SaveConsoleColor()
    {
        _originalForegroundColor = Console.ForegroundColor;
        _originalBackgroundColor = Console.BackgroundColor;
    }
    
    private static void RestoreConsoleColor()
    {
        Console.ForegroundColor = _originalForegroundColor;
        Console.BackgroundColor = _originalBackgroundColor;
    }

    private static ConsoleColor _originalForegroundColor;
    private static ConsoleColor _originalBackgroundColor;
}
