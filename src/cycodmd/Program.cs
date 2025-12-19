using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static async Task<int> Main(string[] args)
    {
        CycoDmdProgramInfo _programInfo = new ();

        LoggingInitializer.InitializeMemoryLogger();
        LoggingInitializer.LogStartupDetails(args);
        Logger.Info($"Starting {ProgramInfo.Name}, version {VersionInfo.GetVersion()}");

        var playwrightCommand = args.Length >= 1 && args[0] == "playwright";
        if (playwrightCommand) return PlaywrightHelpers.RunCli(args.Skip(1).ToArray());

        if (!CycoDmdCommandLineOptions.Parse(args, out var commandLineOptions, out var ex))
        {
            DisplayBanner();
            if (ex != null)
            {
                Logger.Error($"Command line error: {ex.Message}");
                DisplayException(ex);
                HelpHelpers.DisplayUsage(ex.GetHelpTopic());
                return 2;
            }
            else
            {
                Logger.Warning("Displaying help due to command line parsing issue");
                HelpHelpers.DisplayUsage(commandLineOptions!.HelpTopic);
                return 1;
            }
        }

        var debug = ConsoleHelpers.IsDebug() || commandLineOptions!.Debug;
        var verbose = ConsoleHelpers.IsVerbose() || commandLineOptions!.Verbose;
        var quiet = ConsoleHelpers.IsQuiet() || commandLineOptions!.Quiet;
        ConsoleHelpers.Configure(debug, verbose, quiet);
        
        LoggingInitializer.InitializeLogging(commandLineOptions?.LogFile, debug);

        BingApiWebSearchHelpers.ConfigureEndpoint(
            EnvironmentHelpers.FindEnvVar("BING_SEARCH_V7_ENDPOINT", searchDotEnvFile: true),
            EnvironmentHelpers.FindEnvVar("BING_SEARCH_V7_KEY", searchDotEnvFile: true));
        GoogleApiWebSearchHelpers.ConfigureEndpoint(
            EnvironmentHelpers.FindEnvVar("GOOGLE_SEARCH_ENDPOINT", searchDotEnvFile: true),
            EnvironmentHelpers.FindEnvVar("GOOGLE_SEARCH_KEY", searchDotEnvFile: true),
            EnvironmentHelpers.FindEnvVar("GOOGLE_SEARCH_ENGINE_ID", searchDotEnvFile: true));
        OpenAIChatCompletionsClass.Configure(
            EnvironmentHelpers.FindEnvVar("AZURE_OPENAI_ENDPOINT", searchDotEnvFile: true),
            EnvironmentHelpers.FindEnvVar("AZURE_OPENAI_API_KEY", searchDotEnvFile: true),
            EnvironmentHelpers.FindEnvVar("AZURE_OPENAI_CHAT_DEPLOYMENT", searchDotEnvFile: true),
            EnvironmentHelpers.FindEnvVar("AZURE_OPENAI_SYSTEM_PROMPT", searchDotEnvFile: true));

        var helpCommand = commandLineOptions!.Commands.OfType<HelpCommand>().FirstOrDefault();
        if (helpCommand != null)
        {
            DisplayBanner();
            HelpHelpers.DisplayHelpTopic(commandLineOptions.HelpTopic, commandLineOptions.ExpandHelpTopics);
            return 0;
        }

        var shouldSaveAlias = !string.IsNullOrEmpty(commandLineOptions.SaveAliasName);
        if (shouldSaveAlias)
        {
            var filesSaved = AliasFileHelpers.SaveAlias(
                commandLineOptions.SaveAliasName!,
                commandLineOptions.AllOptions,
                commandLineOptions.SaveAliasScope ?? ConfigFileScope.Local);

            DisplayBanner();
            AliasDisplayHelpers.DisplaySavedAliasFiles(filesSaved);

            return 0;
        }

        var shouldSetWorkingDir = !string.IsNullOrEmpty(commandLineOptions.WorkingDirectory);
        if (shouldSetWorkingDir)
        {
            DirectoryHelpers.EnsureDirectoryExists(commandLineOptions.WorkingDirectory!);
            Directory.SetCurrentDirectory(commandLineOptions.WorkingDirectory!);
        }

        var threadCountMax = commandLineOptions.ThreadCount;
        var parallelism = threadCountMax > 0 ? threadCountMax : Environment.ProcessorCount;
        var processor = new ThrottledProcessor(parallelism);

        var allTasks = new List<Task<string>>();

        foreach (var command in commandLineOptions.Commands)
        {
            var cycoDmdCommand = command as CycoDmdCommand;
            bool delayOutputToApplyInstructions = cycoDmdCommand?.InstructionsList.Any() ?? false;

            var tasksThisCommand = command switch
            {
                FindFilesCommand findFilesCommand => await HandleFindFileCommand(commandLineOptions, findFilesCommand, processor, delayOutputToApplyInstructions),
                WebSearchCommand webSearchCommand => await HandleWebSearchCommandAsync(commandLineOptions, webSearchCommand, processor, delayOutputToApplyInstructions),
                WebGetCommand webGetCommand => HandleWebGetCommand(commandLineOptions, webGetCommand, processor, delayOutputToApplyInstructions),
                RunCommand runCommand => HandleRunCommand(commandLineOptions, runCommand, processor, delayOutputToApplyInstructions),
                VersionCommand versionCommand => HandleVersionCommand(commandLineOptions, versionCommand, processor, delayOutputToApplyInstructions),
                _ => new List<Task<string>>()
            };

            allTasks.AddRange(tasksThisCommand);

            var shouldSaveOutput = cycoDmdCommand != null && !string.IsNullOrEmpty(cycoDmdCommand.SaveOutput);
            if (shouldSaveOutput || delayOutputToApplyInstructions)
            {
                await Task.WhenAll(tasksThisCommand.ToArray());
                var commandOutput = string.Join("\n", tasksThisCommand.Select(t => t.Result));

                if (delayOutputToApplyInstructions)
                {
                    commandOutput = AiInstructionProcessor.ApplyAllInstructions(cycoDmdCommand!.InstructionsList, commandOutput, cycoDmdCommand.UseBuiltInFunctions, cycoDmdCommand.SaveChatHistory);
                    ConsoleHelpers.WriteLine(commandOutput);
                }

                if (shouldSaveOutput)
                {
                    var saveFileName = FileHelpers.GetFileNameFromTemplate("output.md", cycoDmdCommand!.SaveOutput)!;
                    FileHelpers.WriteAllText(saveFileName, commandOutput);
                }
            }
        }

        await Task.WhenAll(allTasks.ToArray());
        ConsoleHelpers.DisplayStatusErase();

        return 0;
    }

    private static void DisplayBanner()
    {
        var programNameUppercase = Program.Name.ToUpper();
        
		// TODO: Do we really need this?
        // Make sure ProgramInfo is initialized before accessing VersionInfo
        // (This method may be called before _programInfo is initialized in Main)
        if (ProgramInfo.Assembly == null)
        {
            new CycoDmdProgramInfo();
        }
        
        ConsoleHelpers.WriteLine(
            $"{programNameUppercase} - The AI-Powered Markdown Generator CLI, Version {VersionInfo.GetVersion()}\n" +
            "Copyright(c) 2025, Rob Chambers. All rights reserved.\n");
    }

    private static void DisplayException(CommandLineException ex)
    {
        var printMessage = !string.IsNullOrEmpty(ex.Message);
        if (printMessage) ConsoleHelpers.WriteLine($"  {ex.Message}\n\n");
    }

    private static async Task<List<Task<string>>> HandleFindFileCommand(CommandLineOptions commandLineOptions, FindFilesCommand findFilesCommand, ThrottledProcessor processor, bool delayOutputToApplyInstructions)
    {
        // Log the search operation being initiated
        Logger.Info($"Finding files matching glob pattern(s): '{string.Join("', '", findFilesCommand.Globs)}'");
        
        // Special logging for content patterns, which is what we're particularly interested in
        if (findFilesCommand.IncludeFileContainsPatternList.Any())
        {
            Logger.Info($"Finding files containing regex pattern(s):");
            foreach (var pattern in findFilesCommand.IncludeFileContainsPatternList)
            {
                Logger.Info($"  Content pattern: '{pattern}'");
            }
        }
        
        var files = FileHelpers.FindMatchingFiles(
            findFilesCommand.Globs,
            findFilesCommand.ExcludeGlobs,
            findFilesCommand.ExcludeFileNamePatternList,
            findFilesCommand.IncludeFileContainsPatternList,
            findFilesCommand.ExcludeFileContainsPatternList,
            findFilesCommand.ModifiedAfter,
            findFilesCommand.ModifiedBefore,
            findFilesCommand.CreatedAfter,
            findFilesCommand.CreatedBefore,
            findFilesCommand.AccessedAfter,
            findFilesCommand.AccessedBefore,
            findFilesCommand.AnyTimeAfter,
            findFilesCommand.AnyTimeBefore)
            .ToList();

        // If FilesOnly mode, return just the file paths
        if (findFilesCommand.FilesOnly)
        {
            var fileListOutput = string.Join(Environment.NewLine, files);
            var task = Task.FromResult(fileListOutput);
            
            if (!delayOutputToApplyInstructions)
            {
                ConsoleHelpers.WriteLineIfNotEmpty(fileListOutput);
            }
            
            return new List<Task<string>> { task };
        }
        
        // If replacement mode (both --contains and --replace-with), handle diff/replacement
        if (!string.IsNullOrEmpty(findFilesCommand.ReplaceWithText) && 
            findFilesCommand.IncludeLineContainsPatternList.Count > 0)
        {
            return await HandleReplacementMode(
                findFilesCommand, files, processor, delayOutputToApplyInstructions);
        }

        // Decide whether to use throttling based on whether AI instructions are present
        var needsThrottling = findFilesCommand.FileInstructionsList.Any();
        
        // Derive actual highlight matches value from tri-state:
        // - If explicitly set (true/false), use that value
        // - If null (not specified), auto-enable when we have line numbers AND context lines
        var actualHighlightMatches = findFilesCommand.HighlightMatches ?? 
            (findFilesCommand.IncludeLineNumbers && 
             (findFilesCommand.IncludeLineCountBefore > 0 || findFilesCommand.IncludeLineCountAfter > 0));
        
        // Define the file processor function (used in both throttled and unthrottled paths)
        Func<string, Task<string>> getCheckSaveFileContent = async file =>
        {
            var onlyOneFile = files.Count == 1 && commandLineOptions.Commands.Count == 1;
            var skipMarkdownWrapping = onlyOneFile && FileConverters.CanConvert(file);
            var wrapInMarkdown = !skipMarkdownWrapping;

            return await GetCheckSaveFileContentAsync(
                file,
                wrapInMarkdown,
                findFilesCommand.IncludeLineContainsPatternList,
                findFilesCommand.IncludeLineCountBefore,
                findFilesCommand.IncludeLineCountAfter,
                findFilesCommand.IncludeLineNumbers,
                findFilesCommand.RemoveAllLineContainsPatternList,
                actualHighlightMatches,
                findFilesCommand.FileInstructionsList,
                findFilesCommand.UseBuiltInFunctions,
                findFilesCommand.SaveChatHistory,
                findFilesCommand.SaveFileOutput);
        };

        // Choose processing path: throttled if AI instructions present, unthrottled otherwise
        var tasks = needsThrottling
            ? await processor.StartTasksAsync(files, getCheckSaveFileContent)
            : files.Select(getCheckSaveFileContent).ToList();

        // Handle output based on whether we're delaying for final instructions
        if (!delayOutputToApplyInstructions)
        {
            var outputTasks = tasks.Select(task => task.ContinueWith(t =>
            {
                ConsoleHelpers.WriteLineIfNotEmpty(t.Result);
                return t.Result;
            })).ToList();
            
            return outputTasks;
        }

        return tasks;
    }

    private static async Task<List<Task<string>>> HandleWebSearchCommandAsync(CommandLineOptions commandLineOptions, WebSearchCommand command, ThrottledProcessor processor, bool delayOutputToApplyInstructions)
    {
        var provider = command.SearchProvider;
        var query = string.Join(" ", command.Terms);
        var maxResults = command.MaxResults;
        var excludeURLContainsPatternList = command.ExcludeURLContainsPatternList;
        var getContent = command.GetContent;
        var stripHtml = command.StripHtml;
        var saveToFolder = command.SaveFolder;
        var browserType = command.Browser;
        var interactive = command.Interactive;
        var useBuiltInFunctions = command.UseBuiltInFunctions;
        var saveChatHistory = command.SaveChatHistory;

        var savePageOutput = command.SavePageOutput;
        var pageInstructionsList = command.PageInstructionsList
            .Select(x => Tuple.Create(
                x.Item1
                    .Replace("{searchTerms}", query)
                    .Replace("{query}", query)
                    .Replace("{terms}", query)
                    .Replace("{q}", query),
                x.Item2.ToLowerInvariant()))
            .ToList();

        var searchSectionHeader = $"## Web Search for '{query}' using {provider}";

        var urls = await WebSearchHelpers.GetWebSearchResultUrlsAsync(provider, query, maxResults, excludeURLContainsPatternList, browserType, interactive);
        var searchSection = urls.Count == 0
            ? $"{searchSectionHeader}\n\nNo results found\n"
            : $"{searchSectionHeader}\n\n" + string.Join("\n", urls) + "\n";

        if (!delayOutputToApplyInstructions) ConsoleHelpers.WriteLine(searchSection);

        if (urls.Count == 0 || !getContent)
        {
            return new List<Task<string>>() { Task.FromResult(searchSection) };
        }

        var tasks = new List<Task<string>>();
        tasks.Add(Task.FromResult(searchSection));

        foreach (var url in urls)
        {
            var getCheckSaveTask = GetCheckSaveWebPageContentAsync(url, stripHtml, saveToFolder, browserType, interactive, pageInstructionsList, useBuiltInFunctions, saveChatHistory, savePageOutput);
            var taskToAdd = delayOutputToApplyInstructions
                ? getCheckSaveTask
                : getCheckSaveTask.ContinueWith(t =>
                {
                    ConsoleHelpers.WriteLineIfNotEmpty(t.Result);
                    return t.Result;
                });

            tasks.Add(taskToAdd);
        }

        return tasks;
    }

    private static List<Task<string>> HandleWebGetCommand(CommandLineOptions commandLineOptions, WebGetCommand command, ThrottledProcessor processor, bool delayOutputToApplyInstructions)
    {
        var urls = command.Urls;
        var stripHtml = command.StripHtml;
        var saveToFolder = command.SaveFolder;
        var browserType = command.Browser;
        var interactive = command.Interactive;
        var pageInstructionsList = command.PageInstructionsList;
        var useBuiltInFunctions = command.UseBuiltInFunctions;
        var saveChatHistory = command.SaveChatHistory;
        var savePageOutput = command.SavePageOutput;

        var badUrls = command.Urls.Where(l => !l.StartsWith("http")).ToList();
        if (badUrls.Any())
        {
            var message = (badUrls.Count == 1)
                ? $"Invalid URL: {badUrls[0]}"
                : "Invalid URLs:\n" + string.Join(Environment.NewLine, badUrls.Select(url => "  " + url));
            return new List<Task<string>>() { Task.FromResult(message) };
        }

        var tasks = new List<Task<string>>();
        foreach (var url in urls)
        {
            var getCheckSaveTask = GetCheckSaveWebPageContentAsync(url, stripHtml, saveToFolder, browserType, interactive, pageInstructionsList, useBuiltInFunctions, saveChatHistory, savePageOutput);
            var taskToAdd = delayOutputToApplyInstructions
                ? getCheckSaveTask
                : getCheckSaveTask.ContinueWith(t =>
                {
                    ConsoleHelpers.WriteLineIfNotEmpty(t.Result);
                    return t.Result;
                });

            tasks.Add(taskToAdd);
        }

        return tasks;
    }

    private static List<Task<string>> HandleRunCommand(CommandLineOptions commandLineOptions, RunCommand command, ThrottledProcessor processor, bool delayOutputToApplyInstructions)
    {
        var tasks = new List<Task<string>>();
        var getCheckSaveTask = GetCheckSaveRunCommandContentAsync(command);
        
        var taskToAdd = delayOutputToApplyInstructions
            ? getCheckSaveTask
            : getCheckSaveTask.ContinueWith(t =>
            {
                ConsoleHelpers.WriteLineIfNotEmpty(t.Result);
                return t.Result;
            });

        tasks.Add(taskToAdd);
        return tasks;
    }

    private static List<Task<string>> HandleVersionCommand(CommandLineOptions commandLineOptions, VersionCommand command, ThrottledProcessor processor, bool delayOutputToApplyInstructions)
    {
		// TODO: Do we really need this?
        // Make sure ProgramInfo is initialized before accessing VersionInfo
        if (ProgramInfo.Assembly == null)
        {
            new CycoDmdProgramInfo();
        }
        
        var version = command.ExecuteAsync(false).Result.ToString()!;
        var tasks = new List<Task<string>>();
        tasks.Add(Task.FromResult(version));
        return tasks;
    }

    private static async Task<string> GetCheckSaveRunCommandContentAsync(RunCommand command)
    {
        try
        {
            ConsoleHelpers.DisplayStatus($"Executing: {command.ScriptToRun} ...");
            var finalContent = await GetFinalRunCommandContentAsync(command);

            if (!string.IsNullOrEmpty(command.SaveOutput))
            {
                var saveFileName = FileHelpers.GetFileNameFromTemplate("command-output.md", command.SaveOutput)!;
                FileHelpers.WriteAllText(saveFileName, finalContent);
                ConsoleHelpers.DisplayStatus($"Saving to: {saveFileName} ... Done!");
            }

            return finalContent;
        }
        finally
        {
            ConsoleHelpers.DisplayStatusErase();
        }
    }

    private static async Task<string> GetFinalRunCommandContentAsync(RunCommand command)
    {
        var formatted = await GetFormattedRunCommandContentAsync(command);

        // var afterInstructions = command.InstructionsList.Any()
        //     ? AiInstructionProcessor.ApplyAllInstructions(command.InstructionsList, formatted, command.UseBuiltInFunctions, command.SaveChatHistory)
        //     : formatted;

        // return afterInstructions;

        return formatted;
    }

    private static async Task<string> GetFormattedRunCommandContentAsync(RunCommand command)
    {
        try
        {
            var script = command.ScriptToRun;
            var shell = command.Type switch
            {
                RunCommand.ScriptType.Cmd => "cmd",
                RunCommand.ScriptType.Bash => "bash",
                RunCommand.ScriptType.PowerShell => "pwsh",
                _ => OS.IsWindows() ? "cmd" : "bash",
            };

            var result = await ProcessHelpers.RunShellScriptAsync(shell, script);
            var output = result.MergedOutput;
            var exitCode = result.ExitCode;

            var isMultiLine = script.Contains("\n");
            var header = isMultiLine ? "## Run\n\n" : $"## `{script}`\n\n";

            var backticks = new string('`', MarkdownHelpers.GetCodeBlockBacktickCharCountRequired(output));
            var scriptPart = isMultiLine ? $"Run:\n{backticks}\n{script.TrimEnd()}\n{backticks}\n\n" : string.Empty;
            var outputPart = $"Output:\n{backticks}\n{output.TrimEnd()}\n{backticks}\n\n";
            var exitCodePart = exitCode != 0 ? $"Exit code: {exitCode}\n\n" : string.Empty;

            var sb = new StringBuilder();
            sb.Append(header);
            sb.Append(scriptPart);
            sb.Append(outputPart);
            sb.Append(exitCodePart);

            return sb.ToString().TrimEnd();
        }
        catch (Exception ex)
        {
            return $"## Error executing command: {command.ScriptToRun}\n\n{ex.Message}\n{ex.StackTrace}";
        }
    }

    private static Task<string> GetCheckSaveFileContentAsync(string fileName, bool wrapInMarkdown, List<Regex> includeLineContainsPatternList, int includeLineCountBefore, int includeLineCountAfter, bool includeLineNumbers, List<Regex> removeAllLineContainsPatternList, bool highlightMatches, List<Tuple<string, string>> fileInstructionsList, bool useBuiltInFunctions, string? saveChatHistory, string? saveFileOutput)
    {
        return Task.Run(() => 
            GetCheckSaveFileContent(
                fileName,
                wrapInMarkdown,
                includeLineContainsPatternList,
                includeLineCountBefore,
                includeLineCountAfter,
                includeLineNumbers,
                removeAllLineContainsPatternList,
                highlightMatches,
                fileInstructionsList,
                useBuiltInFunctions,
                saveChatHistory,
                saveFileOutput));
    }

    private static string GetCheckSaveFileContent(string fileName, bool wrapInMarkdown, List<Regex> includeLineContainsPatternList, int includeLineCountBefore, int includeLineCountAfter, bool includeLineNumbers, List<Regex> removeAllLineContainsPatternList, bool highlightMatches, List<Tuple<string, string>> fileInstructionsList, bool useBuiltInFunctions, string? saveChatHistory, string? saveFileOutput)
    {
        try
        {
            ConsoleHelpers.DisplayStatus($"Processing: {fileName} ...");
            
            if (includeLineContainsPatternList.Count > 0)
            {
                Logger.Info($"Using {includeLineContainsPatternList.Count} include regex patterns on '{fileName}':");
                foreach (var pattern in includeLineContainsPatternList)
                {
                    Logger.Info($"  Include pattern: '{pattern}'");
                }
            }
            
            if (removeAllLineContainsPatternList.Count > 0)
            {
                Logger.Info($"Using {removeAllLineContainsPatternList.Count} exclude regex patterns on '{fileName}':");
                foreach (var pattern in removeAllLineContainsPatternList)
                {
                    Logger.Info($"  Exclude pattern: '{pattern}'");
                }
            }

            var finalContent = GetFinalFileContent(
                fileName,
                wrapInMarkdown,
                includeLineContainsPatternList,
                includeLineCountBefore,
                includeLineCountAfter,
                includeLineNumbers,
                removeAllLineContainsPatternList,
                highlightMatches,
                fileInstructionsList,
                useBuiltInFunctions,
                saveChatHistory);

            if (!string.IsNullOrEmpty(saveFileOutput))
            {
                var saveFileName = FileHelpers.GetFileNameFromTemplate(fileName, saveFileOutput)!;
                FileHelpers.WriteAllText(saveFileName, finalContent);
                ConsoleHelpers.DisplayStatus($"Saving to: {saveFileName} ... Done!");
            }

            return finalContent;
        }
        finally
        {
            ConsoleHelpers.DisplayStatusErase();
        }
    }

    private static string GetFinalFileContent(string fileName, bool wrapInMarkdown, List<Regex> includeLineContainsPatternList, int includeLineCountBefore, int includeLineCountAfter, bool includeLineNumbers, List<Regex> removeAllLineContainsPatternList, bool highlightMatches, List<Tuple<string, string>> fileInstructionsList, bool useBuiltInFunctions, string? saveChatHistory)
    {
        var formatted = GetFormattedFileContent(
            fileName,
            wrapInMarkdown,
            includeLineContainsPatternList,
            includeLineCountBefore,
            includeLineCountAfter,
            includeLineNumbers,
            removeAllLineContainsPatternList,
            highlightMatches);

        var instructionsForThisFile = fileInstructionsList
            .Where(x => FileNameMatchesInstructionsCriteria(fileName, x.Item2))
            .Select(x => x.Item1)
            .ToList();

        var afterInstructions = instructionsForThisFile.Any()
            ? AiInstructionProcessor.ApplyAllInstructions(instructionsForThisFile, formatted, useBuiltInFunctions, saveChatHistory)
            : formatted;

        return afterInstructions;
    }

    private static bool FileNameMatchesInstructionsCriteria(string fileName, string fileNameCriteria)
    {
        return string.IsNullOrEmpty(fileNameCriteria) ||
            fileName.EndsWith($".{fileNameCriteria}") ||
            fileName == fileNameCriteria;
    }

    private static string GetFormattedFileContent(string fileName, bool wrapInMarkdown, List<Regex> includeLineContainsPatternList, int includeLineCountBefore, int includeLineCountAfter, bool includeLineNumbers, List<Regex> removeAllLineContainsPatternList, bool highlightMatches)
    {
        try
        {
            var content = FileConverterHelpers.ReadAllText(fileName, out var isMarkdown, out var isStdin, out var isBinary);
            if (content == null) return string.Empty;

            var backticks = isMarkdown || isStdin
                ? new string('`', MarkdownHelpers.GetCodeBlockBacktickCharCountRequired(content))
                : "```";

            var filterContent = includeLineContainsPatternList.Any() || removeAllLineContainsPatternList.Any();
            if (filterContent)
            {
                content = LineHelpers.FilterAndExpandContext(
                    content,
                    includeLineContainsPatternList,
                    includeLineCountBefore,
                    includeLineCountAfter,
                    includeLineNumbers,
                    removeAllLineContainsPatternList,
                    backticks,
                    highlightMatches);

                // If no content matches the filter criteria, skip this file entirely
                var shouldSkipFile = content == null;
                if (shouldSkipFile) return string.Empty;
                
                wrapInMarkdown = true;
            }
            else if (includeLineNumbers)
            {
                content = LineHelpers.AddLineNumbers(content);
                wrapInMarkdown = true;
            }

            if (wrapInMarkdown)
            {
                if (fileName != "-")
                {
                    var fileInfo = new FileInfo(fileName);
                    var modified = FileHelpers.GetFriendlyLastModified(fileInfo);
                    var size = FileHelpers.GetFriendlySize(fileInfo);
                    var lang = FileHelpers.GetMarkdownLanguage(fileInfo.Extension);

                    content = ConsoleHelpers.IsVerbose()
                        ? $"## {fileName}\n\nModified: {modified}\nSize: {size}\n\n{backticks}{lang}\n{content}\n{backticks}\n"
                        : $"## {fileName}\n\n{backticks}{lang}\n{content}\n{backticks}\n";
                }
                else
                {
                    content = $"## (stdin)\n\n{backticks}\n{content}\n{backticks}\n";
                }
            }

            return content!;
        }
        catch (Exception ex)
        {
            return $"## {fileName} - Error reading file: {ex.Message}\n\n{ex.StackTrace}";
        }
    }

    private static async Task<string> GetCheckSaveWebPageContentAsync(string url, bool stripHtml, string? saveToFolder, BrowserType browserType, bool interactive, List<Tuple<string, string>> pageInstructionsList, bool useBuiltInFunctions, string? saveChatHistory, string? savePageOutput)
    {
        try
        {
            ConsoleHelpers.DisplayStatus($"Processing: {url} ...");
            var finalContent = await GetFinalWebPageContentAsync(url, stripHtml, saveToFolder, browserType, interactive, pageInstructionsList, useBuiltInFunctions, saveChatHistory);

            if (!string.IsNullOrEmpty(savePageOutput))
            {
                var fileName = FileHelpers.GenerateUniqueFileNameFromUrl(url, saveToFolder ?? "web-pages");
                var saveFileName = FileHelpers.GetFileNameFromTemplate(fileName, savePageOutput)!;
                FileHelpers.WriteAllText(saveFileName, finalContent);
                ConsoleHelpers.DisplayStatus($"Saving to: {saveFileName} ... Done!");
            }

            return finalContent;
        }
        finally
        {
            ConsoleHelpers.DisplayStatusErase();
        }
    }

    private static async Task<string> GetFinalWebPageContentAsync(string url, bool stripHtml, string? saveToFolder, BrowserType browserType, bool interactive, List<Tuple<string, string>> pageInstructionsList, bool useBuiltInFunctions, string? saveChatHistory)
    {
        var formatted = await GetFormattedWebPageContentAsync(url, stripHtml, saveToFolder, browserType, interactive);

        var instructionsForThisPage = pageInstructionsList
            .Where(x => WebPageMatchesInstructionsCriteria(url, x.Item2))
            .Select(x => x.Item1)
            .ToList();

        var afterInstructions = instructionsForThisPage.Any()
            ? AiInstructionProcessor.ApplyAllInstructions(instructionsForThisPage, formatted, useBuiltInFunctions, saveChatHistory)
            : formatted;

        return afterInstructions;
    }

    private static bool WebPageMatchesInstructionsCriteria(string url, string webPageCriteria)
    {
        return string.IsNullOrEmpty(webPageCriteria) ||
            url.Contains($".{webPageCriteria}") ||
            url == webPageCriteria;
    }

    private static async Task<string> GetFormattedWebPageContentAsync(string url, bool stripHtml, string? saveToFolder, BrowserType browserType, bool interactive)
    {
        try
        {
            var (content, title) = await PlaywrightHelpers.GetPageAndTitle(url, stripHtml, saveToFolder, browserType, interactive);

            var sb = new StringBuilder();
            sb.AppendLine($"## {title}\n");
            sb.AppendLine($"url: {url}\n");
            sb.AppendLine("```");
            sb.AppendLine(content);
            sb.AppendLine("```\n");

            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"## Error fetching {url}\n\n{ex.Message}\n{ex.StackTrace}";
        }
    }
    
    /// <summary>
    /// Handles replacement mode - shows diff preview or executes replacements
    /// </summary>
    private static async Task<List<Task<string>>> HandleReplacementMode(
        FindFilesCommand findFilesCommand, 
        List<string> files, 
        ThrottledProcessor processor, 
        bool delayOutputToApplyInstructions)
    {
        var searchPatterns = findFilesCommand.IncludeLineContainsPatternList;
        var replacementText = findFilesCommand.ReplaceWithText!;
        var executeMode = findFilesCommand.ExecuteMode;
        
        var results = await processor.ProcessAsync(files, async file =>
            await ProcessFileForReplacement(file, searchPatterns, replacementText, executeMode));
        
        var allResults = string.Join(Environment.NewLine, results.Where(r => !string.IsNullOrEmpty(r)));
        
        if (!delayOutputToApplyInstructions)
        {
            ConsoleHelpers.WriteLineIfNotEmpty(allResults);
        }
        
        return new List<Task<string>> { Task.FromResult(allResults) };
    }
    
    /// <summary>
    /// Processes a single file for replacement (preview or execute)
    /// </summary>
    private static async Task<string> ProcessFileForReplacement(
        string filePath, 
        List<Regex> searchPatterns, 
        string replacementText, 
        bool executeMode)
    {
        if (!File.Exists(filePath))
            return string.Empty;
            
        var content = await File.ReadAllTextAsync(filePath);
        var lines = content.Split('\n');
        
        var matchingLines = new List<(int lineNumber, string originalLine, string newLine)>();
        
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var hasMatch = searchPatterns.Any(pattern => pattern.IsMatch(line));
            
            if (hasMatch)
            {
                var newLine = line;
                foreach (var pattern in searchPatterns)
                {
                    newLine = pattern.Replace(newLine, replacementText);
                }
                
                if (newLine != line)
                {
                    matchingLines.Add((i + 1, line, newLine));
                }
            }
        }
        
        if (matchingLines.Count == 0)
            return string.Empty;
            
        // If execute mode, actually perform the replacement
        if (executeMode)
        {
            var newContent = content;
            foreach (var pattern in searchPatterns)
            {
                newContent = pattern.Replace(newContent, replacementText);
            }
            
            await File.WriteAllTextAsync(filePath, newContent);
        }
        
        // Return diff format
        return FormatReplacementDiff(filePath, matchingLines, executeMode);
    }
    
    /// <summary>
    /// Formats replacement results as git-style diff with context
    /// </summary>
    private static string FormatReplacementDiff(
        string filePath, 
        List<(int lineNumber, string originalLine, string newLine)> matchingLines,
        bool wasExecuted)
    {
        if (matchingLines.Count == 0)
            return string.Empty;
            
        var sb = new StringBuilder();
        sb.AppendLine($"## {filePath}");
        sb.AppendLine();
        
        // Read file to get context lines
        var allLines = File.ReadAllLines(filePath);
        const int contextLines = 3;
        
        // Find the range we need to show (with context)
        var minLine = matchingLines.Min(m => m.lineNumber) - 1; // Convert to 0-indexed
        var maxLine = matchingLines.Max(m => m.lineNumber) - 1;
        
        var startLine = Math.Max(0, minLine - contextLines);
        var endLine = Math.Min(allLines.Length - 1, maxLine + contextLines);
        
        var changedLineNumbers = new HashSet<int>(matchingLines.Select(m => m.lineNumber - 1));
        
        // Show context and removed lines
        for (int i = startLine; i <= endLine; i++)
        {
            if (changedLineNumbers.Contains(i))
            {
                // This is a line that will be changed - show as removed
                var match = matchingLines.First(m => m.lineNumber - 1 == i);
                sb.AppendLine($"- {i + 1}: {match.originalLine.TrimEnd()}");
            }
            else
            {
                // Context line
                sb.AppendLine($"  {i + 1}: {allLines[i].TrimEnd()}");
            }
        }
        
        sb.AppendLine();
        
        // Show added lines (replacement)
        for (int i = startLine; i <= endLine; i++)
        {
            if (changedLineNumbers.Contains(i))
            {
                // This is a line that was/will be changed - show as added
                var match = matchingLines.First(m => m.lineNumber - 1 == i);
                sb.AppendLine($"+ {i + 1}: {match.newLine.TrimEnd()}");
            }
            else
            {
                // Context line
                sb.AppendLine($"  {i + 1}: {allLines[i].TrimEnd()}");
            }
        }
        
        sb.AppendLine();
        if (wasExecuted)
        {
            sb.AppendLine($"Replaced {matchingLines.Count} occurrence(s) in this file.");
        }
        else
        {
            sb.AppendLine($"Will replace {matchingLines.Count} occurrence(s) in this file.");
        }
        
        return sb.ToString();
    }

    public const string Name = "cycodmd";
}
