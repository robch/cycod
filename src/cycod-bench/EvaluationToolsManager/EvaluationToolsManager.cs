using System.Diagnostics;
using System.Text;
using CycodBench.Configuration;
using CycodBench.DockerManager;
using CycodBench.Logging;
using CycodBench.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CycodBench.EvaluationToolsManager;

/// <summary>
/// Implementation for managing the SWE-bench evaluation tools.
/// </summary>
public class EvaluationToolsManager : IEvaluationToolsManager
{
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly IDockerManager _dockerManager;
    private static readonly SemaphoreSlim _evaluationSemaphore = new(1); // Limit to one evaluation at a time

    /// <summary>
    /// Initializes a new instance of the <see cref="EvaluationToolsManager"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <param name="dockerManager">The docker manager instance.</param>
    public EvaluationToolsManager(ILogger logger, IConfiguration configuration, IDockerManager dockerManager)
    {
        _logger = logger;
        _configuration = configuration;
        _dockerManager = dockerManager;
    }

    /// <inheritdoc/>
    public string GetToolsPath()
    {
        return _configuration.GetString("eval_tools_path", Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CycodBench",
            "eval_tools"
        ));
    }

    /// <inheritdoc/>
    public async Task<bool> AreToolsSetupAsync()
    {
        string toolsPath = GetToolsPath();
        string swebenchScript = Path.Combine(toolsPath, "venv", "Scripts", "swebench.exe");
        
        if (!File.Exists(swebenchScript))
        {
            // Check for Unix path
            swebenchScript = Path.Combine(toolsPath, "venv", "bin", "swebench");
            if (!File.Exists(swebenchScript))
            {
                return false;
            }
        }
        
        // Check if docker is available
        bool dockerAvailable = await _dockerManager.IsDockerAvailableAsync();
        if (!dockerAvailable)
        {
            _logger.Warning("Docker is not available. Some evaluation features may not work.");
        }
        
        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> SetupToolsAsync(bool forceSetup = false)
    {
        if (await AreToolsSetupAsync() && !forceSetup)
        {
            _logger.Info("Evaluation tools are already set up.");
            return true;
        }
        
        string toolsPath = GetToolsPath();
        
        try
        {
            _logger.Info($"Setting up evaluation tools at {toolsPath}...");
            Directory.CreateDirectory(toolsPath);
            
            // Check if Python is available
            if (!IsPythonAvailable())
            {
                _logger.Error("Python is not available. Please install Python 3.9 or newer.");
                return false;
            }
            
            // Create virtual environment
            await CreateVirtualEnvironmentAsync(toolsPath);
            
            // Install SWE-bench
            await InstallSwebenchAsync(toolsPath);
            
            _logger.Info("Evaluation tools setup completed successfully.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Failed to set up evaluation tools: {ex.Message}");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<string> CreatePredictionsFileAsync(SwebenchProblem problem, CandidateSolution candidate, string outputDirectory)
    {
        string predictionsFilePath = Path.Combine(outputDirectory, "predictions.json");
        
        // Create the predictions object
        var prediction = new
        {
            instance_id = problem.Id,
            commit_hash = problem.BaseCommit,
            repo_path = problem.Repository,
            diff = candidate.Diff
        };
        
        // Write the prediction to a file
        var predictions = new[] { prediction };
        string json = JsonConvert.SerializeObject(predictions, Formatting.Indented);
        Directory.CreateDirectory(outputDirectory);
        await File.WriteAllTextAsync(predictionsFilePath, json);
        
        _logger.Debug($"Created predictions file at {predictionsFilePath}");
        return predictionsFilePath;
    }

    /// <inheritdoc/>
    public async Task<EvaluationResult> EvaluateAsync(
        SwebenchProblem problem,
        CandidateSolution candidate,
        string outputDirectory,
        int numProcesses = 1)
    {
        // Make sure tools are set up
        if (!await AreToolsSetupAsync())
        {
            _logger.Warning("Evaluation tools are not set up. Attempting to set them up now.");
            bool setupSuccess = await SetupToolsAsync();
            if (!setupSuccess)
            {
                throw new InvalidOperationException("Failed to set up evaluation tools.");
            }
        }
        
        // Create the predictions file
        string predictionsFilePath = await CreatePredictionsFileAsync(problem, candidate, outputDirectory);
        string predictionFileName = Path.GetFileName(predictionsFilePath);
        
        // Get the Python executable path
        string pythonExecutable = GetPythonExecutablePath(GetToolsPath());
        
        // Prepare the evaluation command
        string runId = $"cycodbench-{DateTime.Now:yyyyMMdd-HHmmss}";
        string reportPath = Path.Combine(outputDirectory, $"cycodbench.{runId}.json");
        
        // Determine dataset type based on problem ID pattern (simplified for now)
        string datasetName = "princeton-nlp/SWE-bench_Verified";
        
        // Use the evaluation semaphore to prevent multiple evaluations from running at once
        await _evaluationSemaphore.WaitAsync();
        try
        {
            _logger.Info($"Starting evaluation for problem {problem.Id}");
            
            // Create report directory
            Directory.CreateDirectory(outputDirectory);
            
            // Build the command
            string arguments = $"-m swebench.harness.run_evaluation " +
                               $"--dataset_name {datasetName} " +
                               $"--predictions_path {predictionFileName} " +
                               $"--run_id {runId} " +
                               $"--max_workers {numProcesses} " +
                               $"--report_dir {outputDirectory} " +
                               $"--cache_level instance " +
                               $"--namespace swebench " +
                               $"--instance_image_tag latest";
            
            // Execute the evaluation
            (string output, int exitCode) = await RunProcessAsync(pythonExecutable, arguments, outputDirectory);
            
            // Check if the evaluation was successful
            if (exitCode != 0)
            {
                _logger.Error($"Evaluation failed with exit code {exitCode}: {output}");
                
                // Create a failed evaluation result
                return new EvaluationResult
                {
                    ProblemId = problem.Id,
                    SolutionId = candidate.Id,
                    Applied = false,
                    Passed = false,
                    ErrorMessage = $"Evaluation failed with exit code {exitCode}",
                    TestOutput = output
                };
            }
            
            // Parse the evaluation result
            string evalReportFile = Path.Combine(outputDirectory, $"cycodbench.{runId}.json");
            if (File.Exists(evalReportFile))
            {
                string evalReport = await File.ReadAllTextAsync(evalReportFile);
                return ParseEvaluationReport(evalReport, problem.Id, candidate.Id, output);
            }
            else
            {
                _logger.Warning($"Evaluation report file not found at {evalReportFile}");
                
                // Check log directory
                string logDir = Path.Combine(outputDirectory, "logs");
                if (Directory.Exists(logDir))
                {
                    var logFiles = Directory.GetFiles(logDir, "*.log", SearchOption.AllDirectories);
                    if (logFiles.Length > 0)
                    {
                        _logger.Debug($"Found {logFiles.Length} log files in {logDir}");
                        foreach (var logFile in logFiles)
                        {
                            _logger.Debug($"Log file: {logFile}");
                            if (logFile.Contains("run_instance"))
                            {
                                string logContent = await File.ReadAllTextAsync(logFile);
                                _logger.Debug($"Log content: {logContent}");
                            }
                        }
                    }
                }
                
                return new EvaluationResult
                {
                    ProblemId = problem.Id,
                    SolutionId = candidate.Id,
                    Applied = false,
                    Passed = false,
                    ErrorMessage = "Evaluation report file not found",
                    TestOutput = output
                };
            }
        }
        finally
        {
            _evaluationSemaphore.Release();
            
            // Stop any Docker containers created for this evaluation
            try
            {
                await _dockerManager.StopContainerAsync($"sweb.eval.{problem.Id}.swe_work");
            }
            catch (Exception ex)
            {
                _logger.Warning($"Failed to stop evaluation container: {ex.Message}");
            }
        }
    }

    private EvaluationResult ParseEvaluationReport(string reportJson, string problemId, string candidateId, string output)
    {
        try
        {
            var result = new EvaluationResult
            {
                ProblemId = problemId,
                SolutionId = candidateId,
                Applied = true,
                TestOutput = output
            };
            
            // Parse the JSON report
            JObject report = JObject.Parse(reportJson);
            
            // Extract the evaluation results
            if (report["instances"] is JArray instances && instances.Count > 0)
            {
                var instance = instances[0];
                
                // Check if the test passed
                bool? passed = instance["test_results"]?["passed"]?.Value<bool>();
                result.Passed = passed == true;
                
                // Get the test output
                result.TestOutput = instance["test_results"]?["stdout"]?.Value<string>() ?? string.Empty;
                
                // Check if there were compile errors
                bool? compileError = instance["test_results"]?["compile_error"]?.Value<bool>();
                result.BuildExitCode = compileError == true ? 1 : 0;
                
                // Get the error message if there was one
                result.ErrorMessage = instance["test_results"]?["stderr"]?.Value<string>() ?? string.Empty;
                
                // Get the execution time
                double? execTime = instance["test_results"]?["execution_time"]?.Value<double>();
                if (execTime.HasValue)
                {
                    result.EvaluationTimeMs = (long)(execTime.Value * 1000); // Convert to milliseconds
                }
            }
            else
            {
                _logger.Warning("Evaluation report does not contain any instances");
                result.Applied = false;
                result.ErrorMessage = "Evaluation report does not contain any instances";
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Failed to parse evaluation report: {ex.Message}");
            return new EvaluationResult
            {
                ProblemId = problemId,
                SolutionId = candidateId,
                Applied = false,
                Passed = false,
                ErrorMessage = $"Failed to parse evaluation report: {ex.Message}",
                TestOutput = output
            };
        }
    }

    private bool IsPythonAvailable()
    {
        try
        {
            var pythonProcess = Process.Start(new ProcessStartInfo
            {
                FileName = "python",
                Arguments = "--version",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });
            
            pythonProcess?.WaitForExit();
            return pythonProcess?.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    private async Task CreateVirtualEnvironmentAsync(string toolsPath)
    {
        _logger.Info("Creating virtual environment...");
        
        string venvPath = Path.Combine(toolsPath, "venv");
        if (Directory.Exists(venvPath))
        {
            _logger.Info("Virtual environment directory already exists. Removing it...");
            Directory.Delete(venvPath, true);
        }
        
        var processInfo = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = "-m venv venv",
            WorkingDirectory = toolsPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        
        var process = Process.Start(processInfo);
        if (process == null)
        {
            throw new Exception("Failed to start the virtual environment creation process");
        }
        
        await process.WaitForExitAsync();
        
        if (process.ExitCode != 0)
        {
            string error = await process.StandardError.ReadToEndAsync();
            throw new Exception($"Failed to create virtual environment: {error}");
        }
        
        _logger.Info("Virtual environment created successfully");
    }

    private async Task InstallSwebenchAsync(string toolsPath)
    {
        _logger.Info("Installing SWE-bench...");
        
        string pipExecutable = GetPipExecutablePath(toolsPath);
        
        var processInfo = new ProcessStartInfo
        {
            FileName = pipExecutable,
            Arguments = "install swebench",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        
        var process = Process.Start(processInfo);
        if (process == null)
        {
            throw new Exception("Failed to start the SWE-bench installation process");
        }
        
        string output = await process.StandardOutput.ReadToEndAsync();
        string error = await process.StandardError.ReadToEndAsync();
        
        await process.WaitForExitAsync();
        
        if (process.ExitCode != 0)
        {
            throw new Exception($"Failed to install SWE-bench: {error}");
        }
        
        _logger.Info("SWE-bench installed successfully");
        _logger.Debug($"Installation output: {output}");
    }

    private string GetPipExecutablePath(string toolsPath)
    {
        string venvPath = Path.Combine(toolsPath, "venv");
        
        // Check for Windows path
        string pipExecutable = Path.Combine(venvPath, "Scripts", "pip.exe");
        if (File.Exists(pipExecutable))
        {
            return pipExecutable;
        }
        
        // Check for Unix path
        pipExecutable = Path.Combine(venvPath, "bin", "pip");
        if (File.Exists(pipExecutable))
        {
            return pipExecutable;
        }
        
        throw new FileNotFoundException("pip executable not found in the virtual environment");
    }

    private string GetPythonExecutablePath(string toolsPath)
    {
        string venvPath = Path.Combine(toolsPath, "venv");
        
        // Check for Windows path
        string pythonExecutable = Path.Combine(venvPath, "Scripts", "python.exe");
        if (File.Exists(pythonExecutable))
        {
            return pythonExecutable;
        }
        
        // Check for Unix path
        pythonExecutable = Path.Combine(venvPath, "bin", "python");
        if (File.Exists(pythonExecutable))
        {
            return pythonExecutable;
        }
        
        throw new FileNotFoundException("Python executable not found in the virtual environment");
    }
    
    private async Task<(string output, int exitCode)> RunProcessAsync(string executable, string arguments, string workingDirectory)
    {
        var processInfo = new ProcessStartInfo
        {
            FileName = executable,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        
        _logger.Debug($"Running command: {executable} {arguments}");
        
        var process = Process.Start(processInfo);
        if (process == null)
        {
            throw new Exception("Failed to start process");
        }
        
        var outputBuilder = new StringBuilder();
        var outputHandler = new DataReceivedEventHandler((sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                _logger.Debug($"[Process Output] {e.Data}");
                outputBuilder.AppendLine(e.Data);
            }
        });
        
        var errorHandler = new DataReceivedEventHandler((sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                _logger.Warning($"[Process Error] {e.Data}");
                outputBuilder.AppendLine($"ERROR: {e.Data}");
            }
        });
        
        process.OutputDataReceived += outputHandler;
        process.ErrorDataReceived += errorHandler;
        
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        
        int timeout = _configuration.GetInt("evaluation_timeout_ms", 5 * 60 * 1000); // 5 minutes default
        
        // Wait for the process to exit or timeout
        bool completed = await Task.Run(() => process.WaitForExit(timeout));
        
        if (!completed)
        {
            _logger.Warning($"Process timed out after {timeout}ms");
            try
            {
                process.Kill(true);
            }
            catch (Exception ex)
            {
                _logger.Warning($"Failed to kill process: {ex.Message}");
            }
            
            outputBuilder.AppendLine($"Process timed out after {timeout}ms");
            return (outputBuilder.ToString(), -1);
        }
        
        // Clean up the event handlers
        process.OutputDataReceived -= outputHandler;
        process.ErrorDataReceived -= errorHandler;
        
        return (outputBuilder.ToString(), process.ExitCode);
    }
}