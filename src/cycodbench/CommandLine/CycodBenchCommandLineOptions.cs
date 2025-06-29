using System;
using System.Collections.Generic;
using System.Linq;

public class CycodBenchCommandLineOptions : CommandLineOptions
{
    public static bool Parse(string[] args, out CommandLineOptions? commandLineOptions, out CommandLineException? ex)
    {
        commandLineOptions = new CycodBenchCommandLineOptions();
        return commandLineOptions.Parse(args, out ex);
    }

    override protected string PeekCommandName(string[] args, int i)
    {
        var name1 = GetInputOptionArgs(i, args, max: 1).FirstOrDefault();
        return name1 switch
        {
            "run" => "run",
            _ => base.PeekCommandName(args, i)
        };
    }

    override protected bool CheckPartialCommandNeedsHelp(string commandName)
    {
        return commandName switch
        {
            "problems" => true,
            "container" => true,
            "solutions" => true,
            "results" => true,
            _ => false
        };
    }

    override protected Command? NewCommandFromName(string commandName)
    {
        return commandName switch
        {
            // Problems commands
            "problems download" => new ProblemsDownloadCommand(),
            "problems list" => new ProblemsListCommand(),
            "problems merge" => new ProblemsMergeCommand(),
            "problems shard" => new ProblemsShardCommand(),
            "problems solve" => new ProblemsSolveCommand(),
            
            // Container commands
            "container init" => new ContainerInitCommand(),
            "container copy" => new ContainerCopyCommand(),
            "container exec" => new ContainerExecCommand(),
            "container list" => new ContainerListCommand(),
            "container stop" => new ContainerStopCommand(),
            
            // Solutions commands
            "solutions list" => new SolutionsListCommand(),
            "solutions merge" => new SolutionsMergeCommand(),
            "solutions pick" => new SolutionsPickCommand(),
            "solutions eval" => new SolutionsEvalCommand(),
            
            // Results commands
            "results list" => new ResultsListCommand(),
            "results merge" => new ResultsMergeCommand(),
            "results report" => new ResultsReportCommand(),
            
            // Run command
            "run" => new RunCommand(),
            
            _ => base.NewCommandFromName(commandName)
        };
    }

    override protected bool TryParseOtherCommandOptions(Command? command, string[] args, ref int i, string arg)
    {
        return TryParseProblemsCommandOptions(command as ProblemsCommand, args, ref i, arg) ||
               TryParseContainerCommandOptions(command as ContainerCommand, args, ref i, arg) ||
               TryParseSolutionsCommandOptions(command as SolutionsCommand, args, ref i, arg) ||
               TryParseResultsCommandOptions(command as ResultsCommand, args, ref i, arg) ||
               TryParseRunCommandOptions(command as RunCommand, args, ref i, arg) ||
               TryParseSharedCycodBenchCommandOptions(command as CycodBenchCommand, args, ref i, arg);
    }

    private bool TryParseProblemsCommandOptions(ProblemsCommand? command, string[] args, ref int i, string arg)
    {
        bool parsed = true;

        if (command == null)
        {
            parsed = false;
        }
        else if (arg == "--id" && command != null)
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var id = ValidateString(arg, max1Arg.FirstOrDefault(), "problem ID");
            command.ProblemId = id;
            i += max1Arg.Count();
        }
        else if (arg == "--repo" && command != null)
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var repo = ValidateString(arg, max1Arg.FirstOrDefault(), "repository");
            command.Repository = repo;
            i += max1Arg.Count();
        }
        else if (arg == "--contains" && command != null)
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var pattern = ValidateString(arg, max1Arg.FirstOrDefault(), "pattern");
            command.ContainsPattern = pattern;
            i += max1Arg.Count();
        }
        else if (arg == "--output" && command != null)
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var path = ValidateString(arg, max1Arg.FirstOrDefault(), "output path");
            command.OutputPath = path;
            i += max1Arg.Count();
        }
        else if (arg == "--max" && command != null)
        {
            var countStr = i + 1 < args.Count() ? args.ElementAt(++i) : null;
            command.MaxItems = ValidateInt(arg, countStr, "maximum count");
        }
        else if (command is ProblemsDownloadCommand downloadCmd)
        {
            if (arg == "--force")
            {
                downloadCmd.Force = true;
            }
            else
            {
                parsed = false;
            }
        }
        else if (command is ProblemsShardCommand shardCmd)
        {
            if (arg == "--shard")
            {
                var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
                var shardSpec = ValidateString(arg, max1Arg.FirstOrDefault(), "shard specification");
                
                var parts = shardSpec!.Split('/');
                if (parts.Length != 2 || !int.TryParse(parts[0], out int shardIndex) || !int.TryParse(parts[1], out int totalShards))
                {
                    throw new CommandLineException($"Invalid shard specification: {shardSpec}. Expected format: N/M");
                }
                
                shardCmd.ShardIndex = shardIndex;
                shardCmd.TotalShards = totalShards;
                i += max1Arg.Count();
            }
            else if (arg == "--shards")
            {
                var countStr = i + 1 < args.Count() ? args.ElementAt(++i) : null;
                shardCmd.TotalShards = ValidateInt(arg, countStr, "shards count");
            }
            else if (arg == "--candidates")
            {
                var countStr = i + 1 < args.Count() ? args.ElementAt(++i) : null;
                shardCmd.CandidatesPerProblem = ValidateInt(arg, countStr, "candidates count");
            }
            else
            {
                parsed = false;
            }
        }
        else if (command is ProblemsSolveCommand solveCmd)
        {
            if (arg == "--container")
            {
                var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
                var containerId = ValidateString(arg, max1Arg.FirstOrDefault(), "container ID");
                solveCmd.ContainerId = containerId;
                i += max1Arg.Count();
            }
            else if (arg == "--parallel")
            {
                var countStr = i + 1 < args.Count() ? args.ElementAt(++i) : null;
                solveCmd.ParallelCount = ValidateInt(arg, countStr, "parallel count");
            }
            else if (arg == "--docker")
            {
                solveCmd.ContainerProvider = "docker";
            }
            else if (arg == "--aca")
            {
                solveCmd.ContainerProvider = "aca";
            }
            else if (arg == "--aws")
            {
                solveCmd.ContainerProvider = "aws";
            }
            else if (arg == "--timeout")
            {
                var countStr = i + 1 < args.Count() ? args.ElementAt(++i) : null;
                solveCmd.Timeout = ValidateInt(arg, countStr, "timeout seconds");
            }
            else
            {
                parsed = false;
            }
        }
        else
        {
            parsed = false;
        }

        return parsed;
    }

    private bool TryParseContainerCommandOptions(ContainerCommand? command, string[] args, ref int i, string arg)
    {
        bool parsed = true;

        if (command == null)
        {
            parsed = false;
        }
        else if (arg == "--docker" && command != null)
        {
            command.ContainerProvider = "docker";
        }
        else if (arg == "--aca" && command != null)
        {
            command.ContainerProvider = "aca";
        }
        else if (arg == "--aws" && command != null)
        {
            command.ContainerProvider = "aws";
        }
        else if (command is ContainerInitCommand initCmd)
        {
            if (arg == "--name")
            {
                var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
                var name = ValidateString(arg, max1Arg.FirstOrDefault(), "container name");
                initCmd.Name = name;
                i += max1Arg.Count();
            }
            else if (arg == "--image")
            {
                var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
                var image = ValidateString(arg, max1Arg.FirstOrDefault(), "container image");
                initCmd.Image = image;
                i += max1Arg.Count();
            }
            else if (arg == "--memory")
            {
                var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
                var memory = ValidateString(arg, max1Arg.FirstOrDefault(), "memory limit");
                initCmd.Memory = memory;
                i += max1Arg.Count();
            }
            else if (arg == "--cpus")
            {
                var countStr = i + 1 < args.Count() ? args.ElementAt(++i) : null;
                initCmd.Cpus = ValidateInt(arg, countStr, "CPU count");
            }
            else if (arg == "--container")
            {
                var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
                var container = ValidateString(arg, max1Arg.FirstOrDefault(), "container ID");
                initCmd.ContainerId = container;
                i += max1Arg.Count();
            }
            else if (arg == "--workspace")
            {
                var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
                var workspace = ValidateString(arg, max1Arg.FirstOrDefault(), "workspace path");
                initCmd.WorkspacePath = workspace;
                i += max1Arg.Count();
            }
            else if (arg == "--setup-tools")
            {
                initCmd.SetupTools = true;
            }
            else if (arg == "--setup-agent")
            {
                initCmd.SetupAgent = true;
            }
            else
            {
                parsed = false;
            }
        }
        else if (command is ContainerExecCommand execCmd)
        {
            if (arg == "--timeout")
            {
                var countStr = i + 1 < args.Count() ? args.ElementAt(++i) : null;
                execCmd.Timeout = ValidateInt(arg, countStr, "timeout seconds");
            }
            else if (arg == "--workdir")
            {
                var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
                var workdir = ValidateString(arg, max1Arg.FirstOrDefault(), "working directory");
                execCmd.WorkingDirectory = workdir;
                i += max1Arg.Count();
            }
            else if (arg == "--output")
            {
                var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
                var output = ValidateString(arg, max1Arg.FirstOrDefault(), "output path");
                execCmd.OutputPath = output;
                i += max1Arg.Count();
            }
            else
            {
                parsed = false;
            }
        }
        else if (command is ContainerListCommand listCmd)
        {
            if (arg == "--output")
            {
                var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
                var output = ValidateString(arg, max1Arg.FirstOrDefault(), "output path");
                listCmd.OutputPath = output;
                i += max1Arg.Count();
            }
            else
            {
                parsed = false;
            }
        }
        else if (command is ContainerStopCommand stopCmd)
        {
            if (arg == "--keep")
            {
                stopCmd.Keep = true;
            }
            else if (arg == "--timeout")
            {
                var countStr = i + 1 < args.Count() ? args.ElementAt(++i) : null;
                stopCmd.Timeout = ValidateInt(arg, countStr, "timeout seconds");
            }
            else
            {
                parsed = false;
            }
        }
        else
        {
            parsed = false;
        }

        return parsed;
    }

    private bool TryParseSolutionsCommandOptions(SolutionsCommand? command, string[] args, ref int i, string arg)
    {
        bool parsed = true;

        if (command == null)
        {
            parsed = false;
        }
        else if (arg == "--id" && command != null)
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var id = ValidateString(arg, max1Arg.FirstOrDefault(), "problem ID");
            command.ProblemId = id;
            i += max1Arg.Count();
        }
        else if (arg == "--repo" && command != null)
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var repo = ValidateString(arg, max1Arg.FirstOrDefault(), "repository");
            command.Repository = repo;
            i += max1Arg.Count();
        }
        else if (arg == "--contains" && command != null)
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var pattern = ValidateString(arg, max1Arg.FirstOrDefault(), "pattern");
            command.ContainsPattern = pattern;
            i += max1Arg.Count();
        }
        else if (arg == "--output" && command != null)
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var path = ValidateString(arg, max1Arg.FirstOrDefault(), "output path");
            command.OutputPath = path;
            i += max1Arg.Count();
        }
        else if (arg == "--max" && command != null)
        {
            var countStr = i + 1 < args.Count() ? args.ElementAt(++i) : null;
            command.MaxItems = ValidateInt(arg, countStr, "maximum count");
        }
        else if (command is SolutionsEvalCommand evalCmd)
        {
            if (arg == "--docker")
            {
                evalCmd.ContainerProvider = "docker";
            }
            else if (arg == "--aca")
            {
                evalCmd.ContainerProvider = "aca";
            }
            else if (arg == "--aws")
            {
                evalCmd.ContainerProvider = "aws";
            }
            else if (arg == "--container")
            {
                var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
                var containerId = ValidateString(arg, max1Arg.FirstOrDefault(), "container ID");
                evalCmd.ContainerId = containerId;
                i += max1Arg.Count();
            }
            else if (arg == "--timeout")
            {
                var countStr = i + 1 < args.Count() ? args.ElementAt(++i) : null;
                evalCmd.Timeout = ValidateInt(arg, countStr, "timeout seconds");
            }
            else
            {
                parsed = false;
            }
        }
        else
        {
            parsed = false;
        }

        return parsed;
    }

    private bool TryParseResultsCommandOptions(ResultsCommand? command, string[] args, ref int i, string arg)
    {
        bool parsed = true;

        if (command == null)
        {
            parsed = false;
        }
        else if (arg == "--id" && command != null)
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var id = ValidateString(arg, max1Arg.FirstOrDefault(), "problem ID");
            command.ProblemId = id;
            i += max1Arg.Count();
        }
        else if (arg == "--repo" && command != null)
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var repo = ValidateString(arg, max1Arg.FirstOrDefault(), "repository");
            command.Repository = repo;
            i += max1Arg.Count();
        }
        else if (arg == "--contains" && command != null)
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var pattern = ValidateString(arg, max1Arg.FirstOrDefault(), "pattern");
            command.ContainsPattern = pattern;
            i += max1Arg.Count();
        }
        else if (arg == "--output" && command != null)
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var path = ValidateString(arg, max1Arg.FirstOrDefault(), "output path");
            command.OutputPath = path;
            i += max1Arg.Count();
        }
        else if (command is ResultsListCommand listCmd)
        {
            if (arg == "--status")
            {
                var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
                var status = ValidateString(arg, max1Arg.FirstOrDefault(), "status");
                listCmd.Status = status;
                i += max1Arg.Count();
            }
            else if (arg == "--max")
            {
                var countStr = i + 1 < args.Count() ? args.ElementAt(++i) : null;
                listCmd.MaxItems = ValidateInt(arg, countStr, "maximum count");
            }
            else
            {
                parsed = false;
            }
        }
        else
        {
            parsed = false;
        }

        return parsed;
    }

    private bool TryParseRunCommandOptions(RunCommand? command, string[] args, ref int i, string arg)
    {
        bool parsed = true;

        if (command == null)
        {
            parsed = false;
        }
        else if (arg == "--docker")
        {
            command.ContainerProvider = "docker";
        }
        else if (arg == "--aca")
        {
            command.ContainerProvider = "aca";
        }
        else if (arg == "--aws")
        {
            command.ContainerProvider = "aws";
        }
        else if (arg == "--container")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var containerId = ValidateString(arg, max1Arg.FirstOrDefault(), "container ID");
            command.ContainerId = containerId;
            i += max1Arg.Count();
        }
        else if (arg == "--candidates")
        {
            var countStr = i + 1 < args.Count() ? args.ElementAt(++i) : null;
            command.CandidatesPerProblem = ValidateInt(arg, countStr, "candidates count");
        }
        else if (arg == "--shard")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var shardSpec = ValidateString(arg, max1Arg.FirstOrDefault(), "shard specification");
            
            var parts = shardSpec!.Split('/');
            if (parts.Length != 2 || !int.TryParse(parts[0], out int shardIndex) || !int.TryParse(parts[1], out int totalShards))
            {
                throw new CommandLineException($"Invalid shard specification: {shardSpec}. Expected format: N/M");
            }
            
            command.ShardIndex = shardIndex;
            command.TotalShards = totalShards;
            i += max1Arg.Count();
        }
        else if (arg == "--parallel")
        {
            var countStr = i + 1 < args.Count() ? args.ElementAt(++i) : null;
            command.ParallelCount = ValidateInt(arg, countStr, "parallel count");
        }
        else if (arg == "--timeout")
        {
            var countStr = i + 1 < args.Count() ? args.ElementAt(++i) : null;
            command.Timeout = ValidateInt(arg, countStr, "timeout seconds");
        }
        else if (arg == "--output")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var output = ValidateString(arg, max1Arg.FirstOrDefault(), "output path");
            command.OutputPath = output;
            i += max1Arg.Count();
        }
        else
        {
            parsed = false;
        }

        return parsed;
    }

    private bool TryParseSharedCycodBenchCommandOptions(CycodBenchCommand? command, string[] args, ref int i, string arg)
    {
        bool parsed = true;

        if (command == null)
        {
            parsed = false;
        }
        else
        {
            parsed = false;
        }

        return parsed;
    }

    override protected bool TryParseOtherCommandArg(Command? command, string arg)
    {
        var parsedOption = false;

        if (command is ProblemsDownloadCommand downloadCmd)
        {
            if (string.IsNullOrEmpty(downloadCmd.DatasetName))
            {
                downloadCmd.DatasetName = arg;
                parsedOption = true;
            }
        }
        else if (command is ProblemsListCommand listCmd)
        {
            if (string.IsNullOrEmpty(listCmd.DatasetPath))
            {
                listCmd.DatasetPath = arg;
                parsedOption = true;
            }
        }
        else if (command is ProblemsMergeCommand mergeCmd)
        {
            mergeCmd.DatasetPaths.Add(arg);
            parsedOption = true;
        }
        else if (command is ProblemsShardCommand shardCmd)
        {
            if (string.IsNullOrEmpty(shardCmd.DatasetPath))
            {
                shardCmd.DatasetPath = arg;
                parsedOption = true;
            }
        }
        else if (command is ProblemsSolveCommand solveCmd)
        {
            var isStandardDataset = arg == "verified" || arg == "full" || arg == "lite";
            var isFilePath = FileHelpers.FileExists(arg);

            if (string.IsNullOrEmpty(solveCmd.DatasetPath) && (isStandardDataset || isFilePath))
            {
                solveCmd.DatasetPath = arg;
                parsedOption = true;
            }
            else if (string.IsNullOrEmpty(solveCmd.ProblemId))
            {
                solveCmd.ProblemId = arg;
                parsedOption = true;
            }
        }
        else if (command is ContainerInitCommand initCmd)
        {
            var isStandardDataset = arg == "verified" || arg == "full" || arg == "lite";
            var isFilePath = FileHelpers.FileExists(arg);

            if (string.IsNullOrEmpty(initCmd.DatasetPath) && (isStandardDataset || isFilePath))
            {
                initCmd.DatasetPath = arg;
                parsedOption = true;
            }
            else if (string.IsNullOrEmpty(initCmd.ProblemId))
            {
                initCmd.ProblemId = arg;
                parsedOption = true;
            }
        }
        else if (command is ContainerCopyCommand copyCmd)
        {
            if (copyCmd.Direction == null)
            {
                if (arg == "to" || arg == "from")
                {
                    copyCmd.Direction = arg;
                    parsedOption = true;
                }
            }
            else if (string.IsNullOrEmpty(copyCmd.ContainerId))
            {
                copyCmd.ContainerId = arg;
                parsedOption = true;
            }
            else if (string.IsNullOrEmpty(copyCmd.SourcePath))
            {
                copyCmd.SourcePath = arg;
                parsedOption = true;
            }
            else if (string.IsNullOrEmpty(copyCmd.DestinationPath))
            {
                copyCmd.DestinationPath = arg;
                parsedOption = true;
            }
        }
        else if (command is ContainerExecCommand execCmd)
        {
            if (string.IsNullOrEmpty(execCmd.ContainerId))
            {
                execCmd.ContainerId = arg;
                parsedOption = true;
            }
            else if (string.IsNullOrEmpty(execCmd.Command))
            {
                execCmd.Command = arg;
                parsedOption = true;
            }
        }
        else if (command is ContainerStopCommand stopCmd)
        {
            if (string.IsNullOrEmpty(stopCmd.ContainerId))
            {
                stopCmd.ContainerId = arg;
                parsedOption = true;
            }
        }
        else if (command is SolutionsListCommand sListCmd)
        {
            if (string.IsNullOrEmpty(sListCmd.SolutionsFilePath))
            {
                sListCmd.SolutionsFilePath = arg;
                parsedOption = true;
            }
        }
        else if (command is SolutionsMergeCommand sMergeCmd)
        {
            sMergeCmd.SolutionFilePaths.Add(arg);
            parsedOption = true;
        }
        else if (command is SolutionsPickCommand sPickCmd)
        {
            if (string.IsNullOrEmpty(sPickCmd.SolutionsFilePath))
            {
                sPickCmd.SolutionsFilePath = arg;
                parsedOption = true;
            }
        }
        else if (command is SolutionsEvalCommand sEvalCmd)
        {
            if (string.IsNullOrEmpty(sEvalCmd.SolutionsFilePath))
            {
                sEvalCmd.SolutionsFilePath = arg;
                parsedOption = true;
            }
            else if (string.IsNullOrEmpty(sEvalCmd.ProblemId))
            {
                sEvalCmd.ProblemId = arg;
                parsedOption = true;
            }
        }
        else if (command is ResultsListCommand rListCmd)
        {
            if (string.IsNullOrEmpty(rListCmd.ResultsFilePath))
            {
                rListCmd.ResultsFilePath = arg;
                parsedOption = true;
            }
        }
        else if (command is ResultsMergeCommand rMergeCmd)
        {
            rMergeCmd.ResultFilePaths.Add(arg);
            parsedOption = true;
        }
        else if (command is ResultsReportCommand rReportCmd)
        {
            if (string.IsNullOrEmpty(rReportCmd.ResultsFilePath))
            {
                rReportCmd.ResultsFilePath = arg;
                parsedOption = true;
            }
        }
        else if (command is RunCommand runCmd)
        {
            var isStandardDataset = arg == "verified" || arg == "full" || arg == "lite";
            var isFilePath = FileHelpers.FileExists(arg);

            if (string.IsNullOrEmpty(runCmd.DatasetPath) && (isStandardDataset || isFilePath))
            {
                runCmd.DatasetPath = arg;
                parsedOption = true;
            }
            else if (string.IsNullOrEmpty(runCmd.ProblemId))
            {
                runCmd.ProblemId = arg;
                parsedOption = true;
            }
        }

        return parsedOption;
    }
}