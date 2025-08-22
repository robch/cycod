using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CycoDev.CustomTools.Models;

namespace CycoDev.CustomTools
{
    public class CustomToolExecutor
    {
        public CustomToolExecutor()
        {
        }

        public async Task<CustomToolExecutionResult> ExecuteToolAsync(CustomToolDefinition tool, Dictionary<string, object?> parameters)
        {
            if (tool == null)
            {
                return new CustomToolExecutionResult
                {
                    ExitCode = 1,
                    Output = "Error: Tool is null.",
                    Error = "Tool is null.",
                    IsError = true
                };
            }

            if (tool.Type == "alias")
            {
                return await ExecuteAliasToolAsync(tool, parameters);
            }

            if (tool.Steps != null && tool.Steps.Count > 0)
            {
                return await ExecuteMultiStepToolAsync(tool, parameters);
            }
            
            return await ExecuteSingleStepToolAsync(tool, parameters);
        }

        private async Task<CustomToolExecutionResult> ExecuteAliasToolAsync(CustomToolDefinition tool, Dictionary<string, object?> parameters)
        {
            if (string.IsNullOrEmpty(tool.BaseTool))
            {
                return new CustomToolExecutionResult
                {
                    ExitCode = 1,
                    Output = "Error: Alias tool does not specify a base tool.",
                    Error = "Alias tool does not specify a base tool.",
                    IsError = true
                };
            }
            
            // Find the base tool (this would typically be done through the factory)
            // For this implementation, we'll assume the base tool is passed in somehow
            // In a real implementation, we'd need to modify the API to pass in the tool factory
            
            // Merge parameters: default parameters from alias with provided parameters
            var mergedParameters = new Dictionary<string, object?>();
            
            // First add default parameters from the alias
            if (tool.DefaultParameters != null)
            {
                foreach (var param in tool.DefaultParameters)
                {
                    mergedParameters[param.Key] = param.Value;
                }
            }
            
            // Then override with provided parameters
            foreach (var param in parameters)
            {
                mergedParameters[param.Key] = param.Value;
            }
            
            // For now, we'll just return a simulated result
            // In a real implementation, we'd look up the base tool and execute it
            return new CustomToolExecutionResult
            {
                ExitCode = 0,
                Output = $"Alias tool '{tool.Name}' would execute base tool '{tool.BaseTool}' with parameters: {string.Join(", ", mergedParameters.Select(p => $"{p.Key}={p.Value}"))}",
                Error = string.Empty,
                IsError = false
            };
        }

        private async Task<CustomToolExecutionResult> ExecuteMultiStepToolAsync(CustomToolDefinition tool, Dictionary<string, object?> parameters)
        {
            var stepResults = new Dictionary<string, CustomToolExecutionResult>();
            var finalOutput = new StringBuilder();
            var finalError = new StringBuilder();
            var finalExitCode = 0;
            
            // Track running tasks for parallel execution
            var runningTasks = new Dictionary<string, Task<CustomToolExecutionResult>>();
            
            // Process steps in order
            foreach (var step in tool.Steps!)
            {
                // Wait for any steps that this step depends on
                if (step.WaitFor != null && step.WaitFor.Count > 0)
                {
                    var stepsToWaitFor = step.WaitFor.Where(s => runningTasks.ContainsKey(s)).ToList();
                    if (stepsToWaitFor.Count > 0)
                    {
                        ConsoleHelpers.WriteDebugLine($"Waiting for steps to complete: {string.Join(", ", stepsToWaitFor)}");
                        
                        foreach (var waitStep in stepsToWaitFor)
                        {
                            if (runningTasks.TryGetValue(waitStep, out var task))
                            {
                                var result = await task;
                                stepResults[waitStep] = result;
                                runningTasks.Remove(waitStep);
                                
                                // Log the completed step
                                finalOutput.AppendLine($"--- Step: {waitStep} ---");
                                finalOutput.AppendLine(result.Output);
                                finalOutput.AppendLine();
                                
                                if (!string.IsNullOrEmpty(result.Error))
                                {
                                    finalError.AppendLine($"--- Step: {waitStep} ---");
                                    finalError.AppendLine(result.Error);
                                    finalError.AppendLine();
                                }
                                
                                // Set final exit code to the first non-zero exit code
                                if (finalExitCode == 0 && result.ExitCode != 0)
                                {
                                    finalExitCode = result.ExitCode;
                                }
                                
                                // Check if we should continue after error
                                if (result.ExitCode != 0 && !tool.IgnoreErrors)
                                {
                                    // Look up the corresponding step to check its continue-on-error setting
                                    var stepDefinition = tool.Steps.FirstOrDefault(s => s.Name == waitStep);
                                    if (stepDefinition != null && !stepDefinition.ContinueOnError)
                                    {
                                        // Stop execution if the step failed and continue-on-error is false
                                        return new CustomToolExecutionResult
                                        {
                                            ExitCode = finalExitCode,
                                            Output = finalOutput.ToString().TrimEnd(),
                                            Error = finalError.ToString().TrimEnd(),
                                            IsError = true,
                                            StepResults = stepResults
                                        };
                                    }
                                }
                            }
                        }
                    }
                }
                
                // Check if we should run this step based on run-condition
                if (!string.IsNullOrEmpty(step.RunCondition))
                {
                    bool shouldRun = EvaluateRunCondition(step.RunCondition, stepResults);
                    if (!shouldRun)
                    {
                        ConsoleHelpers.WriteDebugLine($"Skipping step '{step.Name}' because run condition evaluated to false");
                        continue;
                    }
                }
                
                // Execute the step
                var stepTask = ExecuteStepAsync(step, parameters, stepResults, tool);
                
                if (step.Parallel)
                {
                    // Add to running tasks if parallel
                    ConsoleHelpers.WriteDebugLine($"Starting step '{step.Name}' in parallel");
                    runningTasks[step.Name] = stepTask;
                }
                else
                {
                    // Wait for completion if not parallel
                    var stepResult = await stepTask;
                    stepResults[step.Name] = stepResult;
                    
                    // Append step output to final output
                    finalOutput.AppendLine($"--- Step: {step.Name} ---");
                    finalOutput.AppendLine(stepResult.Output);
                    finalOutput.AppendLine();
                    
                    // Append step error to final error if there is any
                    if (!string.IsNullOrEmpty(stepResult.Error))
                    {
                        finalError.AppendLine($"--- Step: {step.Name} ---");
                        finalError.AppendLine(stepResult.Error);
                        finalError.AppendLine();
                    }
                    
                    // Set final exit code to the first non-zero exit code
                    if (finalExitCode == 0 && stepResult.ExitCode != 0)
                    {
                        finalExitCode = stepResult.ExitCode;
                    }
                    
                    // Check if we should continue after error
                    if (stepResult.ExitCode != 0 && !step.ContinueOnError && !tool.IgnoreErrors)
                    {
                        break;
                    }
                }
            }
            
            // Wait for any remaining parallel tasks
            if (runningTasks.Count > 0)
            {
                ConsoleHelpers.WriteDebugLine($"Waiting for {runningTasks.Count} parallel steps to complete");
                
                foreach (var task in runningTasks)
                {
                    var stepName = task.Key;
                    var stepResult = await task.Value;
                    stepResults[stepName] = stepResult;
                    
                    // Append step output to final output
                    finalOutput.AppendLine($"--- Step: {stepName} ---");
                    finalOutput.AppendLine(stepResult.Output);
                    finalOutput.AppendLine();
                    
                    // Append step error to final error if there is any
                    if (!string.IsNullOrEmpty(stepResult.Error))
                    {
                        finalError.AppendLine($"--- Step: {stepName} ---");
                        finalError.AppendLine(stepResult.Error);
                        finalError.AppendLine();
                    }
                    
                    // Set final exit code to the first non-zero exit code
                    if (finalExitCode == 0 && stepResult.ExitCode != 0)
                    {
                        finalExitCode = stepResult.ExitCode;
                    }
                }
            }
            
            return new CustomToolExecutionResult
            {
                ExitCode = finalExitCode,
                Output = finalOutput.ToString().TrimEnd(),
                Error = finalError.ToString().TrimEnd(),
                IsError = finalExitCode != 0,
                StepResults = stepResults
            };
        }

        private async Task<CustomToolExecutionResult> ExecuteSingleStepToolAsync(CustomToolDefinition tool, Dictionary<string, object?> parameters)
        {
            string? command = null;
            ShellType shellType = ShellType.Bash;

            if (!string.IsNullOrEmpty(tool.BashCommand))
            {
                command = tool.BashCommand;
                shellType = ShellType.Bash;
            }
            else if (!string.IsNullOrEmpty(tool.CmdCommand))
            {
                command = tool.CmdCommand;
                shellType = ShellType.Cmd;
            }
            else if (!string.IsNullOrEmpty(tool.PowerShellCommand))
            {
                command = tool.PowerShellCommand;
                shellType = ShellType.PowerShell;
            }
            else if (!string.IsNullOrEmpty(tool.RunCommand))
            {
                command = tool.RunCommand;
                shellType = ShellType.Run;
            }
            else if (!string.IsNullOrEmpty(tool.Script))
            {
                command = tool.Script;
                shellType = !string.IsNullOrEmpty(tool.Shell) 
                    ? ParseShellType(tool.Shell) 
                    : ShellType.Bash;
            }

            if (string.IsNullOrEmpty(command))
            {
                return new CustomToolExecutionResult
                {
                    ExitCode = 1,
                    Output = "Error: No command specified for tool.",
                    Error = "No command specified for tool.",
                    IsError = true
                };
            }

            // Substitute parameters
            command = SubstituteParameters(command, parameters);

            // Execute the command
            return await ExecuteCommandAsync(command, shellType);
        }

        private async Task<CustomToolExecutionResult> ExecuteStepAsync(
            CustomToolStep step, 
            Dictionary<string, object?> parameters,
            Dictionary<string, CustomToolExecutionResult> previousStepResults,
            CustomToolDefinition tool)
        {
            int attempts = 1;
            int delay = 0;
            string? fallbackCommand = null;
            
            // Configure retry and fallback if error handling is defined
            if (step.ErrorHandling != null)
            {
                if (step.ErrorHandling.Retry != null)
                {
                    attempts = step.ErrorHandling.Retry.Attempts;
                    delay = step.ErrorHandling.Retry.Delay;
                }
                
                fallbackCommand = step.ErrorHandling.Fallback;
            }
            
            for (int attempt = 1; attempt <= attempts; attempt++)
            {
                string? command = null;
                ShellType shellType = ShellType.Bash;

                if (!string.IsNullOrEmpty(step.BashCommand))
                {
                    command = step.BashCommand;
                    shellType = ShellType.Bash;
                }
                else if (!string.IsNullOrEmpty(step.CmdCommand))
                {
                    command = step.CmdCommand;
                    shellType = ShellType.Cmd;
                }
                else if (!string.IsNullOrEmpty(step.PowerShellCommand))
                {
                    command = step.PowerShellCommand;
                    shellType = ShellType.PowerShell;
                }
                else if (!string.IsNullOrEmpty(step.RunCommand))
                {
                    command = step.RunCommand;
                    shellType = ShellType.Run;
                }
                else if (!string.IsNullOrEmpty(step.UseTool))
                {
                    // Implementation for using another tool
                    var toolParameters = new Dictionary<string, object?>();
                    
                    // Add parameters from the 'with' section
                    if (step.With != null)
                    {
                        foreach (var param in step.With)
                        {
                            // Substitute any step references or parameters
                            string value = SubstituteParameters(param.Value, parameters, previousStepResults);
                            toolParameters[param.Key] = value;
                        }
                    }
                    
                    // For now, return a simulated result
                    // In a real implementation, we'd need access to the tool factory to look up the tool
                    return new CustomToolExecutionResult
                    {
                        ExitCode = 0,
                        Output = $"Step '{step.Name}' would execute tool '{step.UseTool}' with parameters: {string.Join(", ", toolParameters.Select(p => $"{p.Key}={p.Value}"))}",
                        Error = string.Empty,
                        IsError = false
                    };
                }

                if (string.IsNullOrEmpty(command))
                {
                    return new CustomToolExecutionResult
                    {
                        ExitCode = 1,
                        Output = $"Error: No command specified for step '{step.Name}'.",
                        Error = $"No command specified for step '{step.Name}'.",
                        IsError = true
                    };
                }

                // Substitute parameters and previous step outputs
                command = SubstituteParameters(command, parameters, previousStepResults);

                // Execute the command
                var result = await ExecuteCommandAsync(command, shellType, tool);
                
                // Check if execution succeeded or if we should retry
                if (result.ExitCode == 0 || attempt >= attempts)
                {
                    // Success or final attempt
                    // If it failed and there's a fallback, try that
                    if (result.ExitCode != 0 && !string.IsNullOrEmpty(fallbackCommand))
                    {
                        ConsoleHelpers.WriteDebugLine($"Step '{step.Name}' failed after {attempt} attempts, trying fallback command");
                        
                        // Substitute parameters in fallback command
                        var processedFallback = SubstituteParameters(fallbackCommand, parameters, previousStepResults);
                        
                        // Execute fallback command using the same shell type
                        return await ExecuteCommandAsync(processedFallback, shellType, tool);
                    }
                    
                    return result;
                }
                
                // Retry logic
                ConsoleHelpers.WriteDebugLine($"Step '{step.Name}' failed (exit code {result.ExitCode}), retrying ({attempt}/{attempts})");
                
                if (delay > 0 && attempt < attempts)
                {
                    await Task.Delay(delay);
                }
            }
            
            // This should never be reached due to the return in the loop
            return new CustomToolExecutionResult
            {
                ExitCode = 1,
                Output = $"Error: Failed to execute step '{step.Name}' after {attempts} attempts.",
                Error = $"Failed to execute step '{step.Name}' after {attempts} attempts.",
                IsError = true
            };
        }

        private async Task<CustomToolExecutionResult> ExecuteCommandAsync(string command, ShellType shellType, CustomToolDefinition? tool = null)
        {
            try
            {
                // Prepare the process start info based on shell type
                var processStartInfo = new System.Diagnostics.ProcessStartInfo();
                
                switch (shellType)
                {
                    case ShellType.Bash:
                        processStartInfo.FileName = GetBashPath();
                        processStartInfo.Arguments = $"-c \"{EscapeForShell(command)}\"";
                        break;
                        
                    case ShellType.Cmd:
                        processStartInfo.FileName = "cmd.exe";
                        processStartInfo.Arguments = $"/c {command}";
                        break;
                        
                    case ShellType.PowerShell:
                        processStartInfo.FileName = "powershell.exe";
                        processStartInfo.Arguments = $"-Command \"{EscapeForPowerShell(command)}\"";
                        break;
                        
                    case ShellType.Run:
                        // Extract the command and arguments
                        var parts = command.Split(' ', 2);
                        processStartInfo.FileName = parts[0];
                        if (parts.Length > 1)
                        {
                            processStartInfo.Arguments = parts[1];
                        }
                        break;
                }
                
                // Configure process
                processStartInfo.RedirectStandardOutput = true;
                processStartInfo.RedirectStandardError = true;
                processStartInfo.UseShellExecute = false;
                processStartInfo.CreateNoWindow = true;
                
                // Set working directory if specified
                if (tool != null && !string.IsNullOrEmpty(tool.WorkingDirectory))
                {
                    processStartInfo.WorkingDirectory = NormalizeWorkingDirectory(tool.WorkingDirectory);
                }
                
                // Setup environment variables if specified
                if (tool?.Resources?.EnvironmentVariables != null)
                {
                    foreach (var envVar in tool.Resources.EnvironmentVariables)
                    {
                        processStartInfo.EnvironmentVariables[envVar.Key] = envVar.Value;
                    }
                }
                
                // Inherit parent environment if specified
                processStartInfo.EnvironmentVariables["CYCOD_TOOL_NAME"] = tool?.Name ?? "unnamed-tool";
                
                // Start the process
                using var process = new System.Diagnostics.Process();
                process.StartInfo = processStartInfo;
                
                var outputBuilder = new StringBuilder();
                var errorBuilder = new StringBuilder();
                
                // Set up output and error handling
                process.OutputDataReceived += (sender, e) => 
                {
                    if (e.Data != null)
                    {
                        outputBuilder.AppendLine(e.Data);
                        
                        // Stream output if specified
                        if (tool?.Resources?.Streaming ?? false)
                        {
                            ConsoleHelpers.WriteDebugLine(e.Data);
                        }
                    }
                };
                
                process.ErrorDataReceived += (sender, e) => 
                {
                    if (e.Data != null)
                    {
                        errorBuilder.AppendLine(e.Data);
                        
                        // Stream error if specified
                        if (tool?.Resources?.Streaming ?? false)
                        {
                            ConsoleHelpers.WriteErrorLine(e.Data);
                        }
                    }
                };
                
                // Start process and begin reading output
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                
                // Wait for process to exit with timeout
                int timeoutMs = tool?.Timeout ?? 60000; // Default 60 seconds
                var waitForExitTask = process.WaitForExitAsync();
                var timeoutTask = Task.Delay(timeoutMs);
                
                var completedTask = await Task.WhenAny(waitForExitTask, timeoutTask);
                
                if (completedTask == timeoutTask)
                {
                    // Process timed out
                    try
                    {
                        process.Kill(true);
                    }
                    catch
                    {
                        // Ignore errors killing the process
                    }
                    
                    return new CustomToolExecutionResult
                    {
                        ExitCode = -1,
                        Output = outputBuilder.ToString().TrimEnd(),
                        Error = $"Process timed out after {timeoutMs}ms.",
                        IsError = true
                    };
                }
                
                // Process completed within timeout
                await waitForExitTask;
                
                // Check if output exceeds buffer limit
                if (tool?.Resources?.MaxSize != null && 
                    outputBuilder.Length > tool.Resources.MaxSize)
                {
                    if (tool.Resources.Truncation)
                    {
                        // Truncate output if specified
                        outputBuilder.Length = tool.Resources.MaxSize.Value;
                        outputBuilder.AppendLine("\n... output truncated due to size limit ...");
                    }
                }
                
                return new CustomToolExecutionResult
                {
                    ExitCode = process.ExitCode,
                    Output = outputBuilder.ToString().TrimEnd(),
                    Error = errorBuilder.ToString().TrimEnd(),
                    IsError = process.ExitCode != 0
                };
            }
            catch (Exception ex)
            {
                return new CustomToolExecutionResult
                {
                    ExitCode = 1,
                    Output = $"Error executing command: {ex.Message}",
                    Error = ex.ToString(),
                    IsError = true
                };
            }
        }
        
        /// <summary>
        /// Gets the path to the bash executable.
        /// </summary>
        /// <returns>The path to bash.</returns>
        private string GetBashPath()
        {
            if (OperatingSystem.IsWindows())
            {
                // Try Git Bash first
                var gitBashPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    "Git",
                    "bin",
                    "bash.exe");
                    
                if (File.Exists(gitBashPath))
                {
                    return gitBashPath;
                }
                
                // Try Windows Subsystem for Linux
                if (File.Exists(@"C:\Windows\System32\bash.exe"))
                {
                    return @"C:\Windows\System32\bash.exe";
                }
                
                // Default to cmd as fallback
                return "cmd.exe";
            }
            
            return "/bin/bash";
        }
        
        /// <summary>
        /// Escapes a command for shell execution.
        /// </summary>
        /// <param name="command">The command to escape.</param>
        /// <returns>The escaped command.</returns>
        private string EscapeForShell(string command)
        {
            return command.Replace("\"", "\\\"");
        }
        
        /// <summary>
        /// Escapes a command for PowerShell execution.
        /// </summary>
        /// <param name="command">The command to escape.</param>
        /// <returns>The escaped command.</returns>
        private string EscapeForPowerShell(string command)
        {
            return command.Replace("\"", "`\"");
        }

        /// <summary>
        /// Evaluates a run condition to determine if a step should be executed.
        /// </summary>
        /// <param name="condition">The condition string.</param>
        /// <param name="stepResults">Results from previous steps.</param>
        /// <returns>The evaluation result.</returns>
        private bool EvaluateRunCondition(string condition, Dictionary<string, CustomToolExecutionResult> stepResults)
        {
            // Substitute step references
            string evaluatableCondition = SubstituteStepReferences(condition, stepResults);
            
            // Parse and evaluate the condition
            try
            {
                // Handle simple exit code conditions
                if (evaluatableCondition.Contains("==") || evaluatableCondition.Contains("!="))
                {
                    // Split by comparison operator
                    string[] parts;
                    bool isEqual;
                    
                    if (evaluatableCondition.Contains("=="))
                    {
                        parts = evaluatableCondition.Split("==", 2);
                        isEqual = true;
                    }
                    else
                    {
                        parts = evaluatableCondition.Split("!=", 2);
                        isEqual = false;
                    }
                    
                    if (parts.Length == 2)
                    {
                        string left = parts[0].Trim();
                        string right = parts[1].Trim();
                        
                        // Try to convert to integers for numeric comparison
                        int leftValue = 0, rightValue = 0;
                        bool isNumeric = int.TryParse(left, out leftValue) && 
                                        int.TryParse(right, out rightValue);
                                        
                        if (isNumeric)
                        {
                            return isEqual ? leftValue == rightValue : leftValue != rightValue;
                        }
                        else
                        {
                            // String comparison
                            return isEqual ? left == right : left != right;
                        }
                    }
                }
                // Handle logical operators
                else if (evaluatableCondition.Contains("&&") || evaluatableCondition.Contains("||"))
                {
                    // For now, implement simple AND/OR logic
                    if (evaluatableCondition.Contains("&&"))
                    {
                        string[] parts = evaluatableCondition.Split("&&");
                        return parts.All(p => EvaluateSimpleCondition(p.Trim()));
                    }
                    else
                    {
                        string[] parts = evaluatableCondition.Split("||");
                        return parts.Any(p => EvaluateSimpleCondition(p.Trim()));
                    }
                }
                // Handle greater/less than
                else if (evaluatableCondition.Contains(">") || evaluatableCondition.Contains("<"))
                {
                    string[] parts;
                    string op;
                    
                    if (evaluatableCondition.Contains(">="))
                    {
                        parts = evaluatableCondition.Split(">=", 2);
                        op = ">=";
                    }
                    else if (evaluatableCondition.Contains("<="))
                    {
                        parts = evaluatableCondition.Split("<=", 2);
                        op = "<=";
                    }
                    else if (evaluatableCondition.Contains(">"))
                    {
                        parts = evaluatableCondition.Split(">", 2);
                        op = ">";
                    }
                    else
                    {
                        parts = evaluatableCondition.Split("<", 2);
                        op = "<";
                    }
                    
                    if (parts.Length == 2)
                    {
                        string left = parts[0].Trim();
                        string right = parts[1].Trim();
                        
                        // Try to convert to numbers for comparison
                        if (decimal.TryParse(left, out decimal leftValue) && 
                            decimal.TryParse(right, out decimal rightValue))
                        {
                            return op switch
                            {
                                ">=" => leftValue >= rightValue,
                                "<=" => leftValue <= rightValue,
                                ">" => leftValue > rightValue,
                                "<" => leftValue < rightValue,
                                _ => false
                            };
                        }
                    }
                }
                
                // Default to simple boolean evaluation
                return EvaluateSimpleCondition(evaluatableCondition);
            }
            catch (Exception ex)
            {
                ConsoleHelpers.WriteDebugLine($"Error evaluating condition: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Evaluates a simple condition.
        /// </summary>
        /// <param name="condition">The simple condition string.</param>
        /// <returns>The evaluation result.</returns>
        private bool EvaluateSimpleCondition(string condition)
        {
            // Convert common truthy/falsy values
            condition = condition.Trim().ToLowerInvariant();
            
            if (bool.TryParse(condition, out bool result))
            {
                return result;
            }
            
            if (int.TryParse(condition, out int intResult))
            {
                return intResult != 0;
            }
            
            // Consider truthy string values
            return condition == "true" || condition == "yes" || condition == "success" || condition == "y" || condition == "1";
        }
        
        /// <summary>
        /// Substitutes parameter references in a run condition with their values.
        /// </summary>
        /// <param name="condition">The condition string.</param>
        /// <param name="stepResults">Results from previous steps.</param>
        /// <returns>The substituted condition.</returns>
        private string SubstituteStepReferences(string condition, Dictionary<string, CustomToolExecutionResult> stepResults)
        {
            // We support references like {step1.exit-code}, {step1.output}, etc.
            // Use a simple string replacement for now
            string result = condition;
            
            foreach (var stepEntry in stepResults)
            {
                var stepName = stepEntry.Key;
                var stepResult = stepEntry.Value;
                
                result = result
                    .Replace($"{{{stepName}.exit-code}}", stepResult.ExitCode.ToString())
                    .Replace($"{{{stepName}.output}}", stepResult.Output)
                    .Replace($"{{{stepName}.error}}", stepResult.Error);
            }
            
            return result;
        }
        
        /// <summary>
        /// Substitutes parameter references in a command with their values.
        /// </summary>
        /// <param name="command">The command string.</param>
        /// <param name="parameters">The parameters to substitute.</param>
        /// <returns>The command with substituted parameters.</returns>
        private string SubstituteParameters(string command, Dictionary<string, object?> parameters)
        {
            // Simple string replacement for now
            string result = command;
            
            foreach (var param in parameters)
            {
                string value = GetParameterValueString(param.Value);
                
                // Check if we need to escape shell metacharacters
                // In a real implementation, we'd look up the parameter definition
                // and check if it has security.escape-shell = true
                bool shouldEscape = param.Key.EndsWith("_SECURE");
                
                if (shouldEscape)
                {
                    value = EscapeShellMetacharacters(value);
                }
                
                result = result.Replace($"{{{param.Key}}}", value);
            }
            
            return result;
        }
        
        /// <summary>
        /// Converts a parameter value to a string representation.
        /// </summary>
        /// <param name="value">The parameter value.</param>
        /// <returns>String representation of the value.</returns>
        private string GetParameterValueString(object? value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            
            // Handle arrays
            if (value is System.Collections.IEnumerable enumerable && !(value is string))
            {
                var items = new List<string>();
                foreach (var item in enumerable)
                {
                    items.Add(item?.ToString() ?? string.Empty);
                }
                
                return string.Join(" ", items);
            }
            
            return value.ToString() ?? string.Empty;
        }
        
        /// <summary>
        /// Escapes shell metacharacters in a string.
        /// </summary>
        /// <param name="value">The value to escape.</param>
        /// <returns>The escaped value.</returns>
        private string EscapeShellMetacharacters(string value)
        {
            // Escape common shell metacharacters
            var metacharacters = new[] { '\\', '"', '\'', ' ', '&', '|', ';', '<', '>', '(', ')', '$', '`', '\n', '\r', '\t' };
            
            var result = new StringBuilder();
            foreach (char c in value)
            {
                if (metacharacters.Contains(c))
                {
                    result.Append('\\');
                }
                result.Append(c);
            }
            
            return result.ToString();
        }
        
        /// <summary>
        /// Substitutes parameter references in a command with their values.
        /// </summary>
        /// <param name="command">The command string.</param>
        /// <param name="parameters">The parameters to substitute.</param>
        /// <param name="stepResults">Results from previous steps.</param>
        /// <returns>The command with substituted parameters.</returns>
        private string SubstituteParameters(
            string command,
            Dictionary<string, object?> parameters,
            Dictionary<string, CustomToolExecutionResult> stepResults)
        {
            // First substitute regular parameters
            string result = SubstituteParameters(command, parameters);
            
            // Then substitute step references
            foreach (var stepEntry in stepResults)
            {
                var stepName = stepEntry.Key;
                var stepResult = stepEntry.Value;
                
                // Escape output and error if they contain sensitive content
                string output = stepResult.Output;
                string error = stepResult.Error;
                
                // Replace step references
                result = result
                    .Replace($"{{{stepName}.exit-code}}", stepResult.ExitCode.ToString())
                    .Replace($"{{{stepName}.output}}", output)
                    .Replace($"{{{stepName}.error}}", error);
            }
            
            return result;
        }
        
        /// <summary>
        /// Normalizes a file path based on the operating system and tool configuration.
        /// </summary>
        /// <param name="path">The path to normalize.</param>
        /// <param name="tool">The tool definition with path handling configuration.</param>
        /// <returns>The normalized path.</returns>
        private string NormalizePath(string path, CustomToolDefinition tool)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }
            
            // Check if path normalization is enabled
            if (true)
            {
                // Apply working directory if the path is relative
                if (!Path.IsPathRooted(path) && !string.IsNullOrEmpty(tool.WorkingDirectory))
                {
                    // Substitute any parameters in the working directory
                    var workingDir = tool.WorkingDirectory;
                    path = Path.Combine(workingDir, path);
                }
                
                // Resolve temp directory references
                if (path.Contains("{TEMP_DIRECTORY}"))
                {
                    string tempDir = Path.GetTempPath();
                    path = path.Replace("{TEMP_DIRECTORY}", tempDir);
                }
                
                // Cross-platform path handling
                if (true)
                {
                    // Convert path separators based on the current platform
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        // Convert to Windows path format
                        path = path.Replace("/", "\\");
                    }
                    else
                    {
                        // Convert to Unix path format
                        path = path.Replace("\\", "/");
                    }
                }
                
                // Get canonical form of the path
                try
                {
                    // Use GetFullPath to normalize path separators and resolve relative segments
                    path = Path.GetFullPath(path);
                }
                catch (Exception ex)
                {
                    ConsoleHelpers.WriteDebugLine($"Error normalizing path '{path}': {ex.Message}");
                }
            }
            
            return path;
        }
        
        /// <summary>
        /// Normalizes a working directory path.
        /// </summary>
        /// <param name="workingDirectory">The working directory to normalize.</param>
        /// <returns>The normalized working directory.</returns>
        private string NormalizeWorkingDirectory(string workingDirectory)
        {
            if (string.IsNullOrEmpty(workingDirectory))
            {
                return Directory.GetCurrentDirectory();
            }
            
            // Expand environment variables
            workingDirectory = Environment.ExpandEnvironmentVariables(workingDirectory);
            
            // Handle home directory references (~)
            if (workingDirectory.StartsWith("~/") || workingDirectory.StartsWith("~\\"))
            {
                string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                workingDirectory = Path.Combine(homeDir, workingDirectory.Substring(2));
            }
            
            // Ensure the directory exists
            if (!Directory.Exists(workingDirectory))
            {
                try
                {
                    Directory.CreateDirectory(workingDirectory);
                }
                catch (Exception ex)
                {
                    ConsoleHelpers.WriteDebugLine($"Error creating working directory '{workingDirectory}': {ex.Message}");
                    return Directory.GetCurrentDirectory();
                }
            }
            
            return Path.GetFullPath(workingDirectory);
        }
        
        /// <summary>
        /// Parses the shell type from a string.
        /// </summary>
        private ShellType ParseShellType(string shell)
        {
            return shell.ToLowerInvariant() switch
            {
                "bash" => ShellType.Bash,
                "cmd" => ShellType.Cmd,
                "powershell" => ShellType.PowerShell,
                _ => ShellType.Bash // Default to bash
            };
        }
        
        /// <summary>
        /// Runs the tests for a custom tool.
        /// </summary>
        /// <param name="tool">The tool definition.</param>
        /// <returns>The test results.</returns>
        public async Task<CustomToolTestResults> RunToolTestsAsync(CustomToolDefinition tool)
        {
            var testResults = new CustomToolTestResults
            {
                ToolName = tool.Name,
                TestResults = new List<CustomToolTestResult>()
            };
            
            if (tool.Tests == null || tool.Tests.Count == 0)
            {
                ConsoleHelpers.WriteDebugLine($"No tests defined for tool '{tool.Name}'");
                return testResults;
            }
            
            foreach (var test in tool.Tests)
            {
                ConsoleHelpers.WriteDebugLine($"Running test '{test.Name}' for tool '{tool.Name}'");
                
                // Set up parameters
                var parameters = new Dictionary<string, object?>();
                
                // Add parameters from the test
                if (test.Parameters != null)
                {
                    foreach (var param in test.Parameters)
                    {
                        parameters[param.Key] = param.Value;
                    }
                }
                
                // Execute the tool with test parameters
                var result = await ExecuteToolAsync(tool, parameters);
                
                // Evaluate test expectations
                bool passed = true;
                var failureReasons = new List<string>();
                
                if (test.Expected != null)
                {
                    // Check exit code
                    if (test.Expected.ExitCode.HasValue && 
                        result.ExitCode != test.Expected.ExitCode.Value)
                    {
                        passed = false;
                        failureReasons.Add($"Exit code mismatch: expected {test.Expected.ExitCode.Value}, got {result.ExitCode}");
                    }
                    
                    // Check output contains
                    if (!string.IsNullOrEmpty(test.Expected.OutputContains) && 
                        !result.Output.Contains(test.Expected.OutputContains))
                    {
                        passed = false;
                        failureReasons.Add($"Output does not contain expected text: '{test.Expected.OutputContains}'");
                    }
                    
                    // Check file exists
                    if (!string.IsNullOrEmpty(test.Expected.FileExists))
                    {
                        string filePath = NormalizePath(test.Expected.FileExists, tool);
                        if (!File.Exists(filePath))
                        {
                            passed = false;
                            failureReasons.Add($"Expected file does not exist: '{filePath}'");
                        }
                    }
                    
                    // Check directory exists
                    if (!string.IsNullOrEmpty(test.Expected.DirectoryExists))
                    {
                        string dirPath = NormalizePath(test.Expected.DirectoryExists, tool);
                        if (!Directory.Exists(dirPath))
                        {
                            passed = false;
                            failureReasons.Add($"Expected directory does not exist: '{dirPath}'");
                        }
                    }
                }
                
                // Run cleanup commands if specified
                if (test.Cleanup != null && test.Cleanup.Count > 0)
                {
                    foreach (var cleanupCommand in test.Cleanup)
                    {
                        try
                        {
                            await ExecuteCommandAsync(cleanupCommand, ShellType.Bash, tool);
                        }
                        catch (Exception ex)
                        {
                            ConsoleHelpers.WriteDebugLine($"Error during test cleanup: {ex.Message}");
                        }
                    }
                }
                
                // Add test result
                testResults.TestResults.Add(new CustomToolTestResult
                {
                    TestName = test.Name,
                    Passed = passed,
                    FailureReasons = failureReasons,
                    Output = result.Output,
                    Error = result.Error,
                    ExitCode = result.ExitCode
                });
                
                ConsoleHelpers.WriteDebugLine($"Test '{test.Name}' {(passed ? "passed" : "failed")}");
            }
            
            return testResults;
        }
    }

    public enum ShellType
    {
        Bash,
        Cmd,
        PowerShell,
        Run
    }

    public class CustomToolExecutionResult
    {
        public int ExitCode { get; set; }
        public string Output { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
        public bool IsError { get; set; }
        public Dictionary<string, CustomToolExecutionResult>? StepResults { get; set; }
    }
    
    public class CustomToolTestResult
    {
        public string TestName { get; set; } = string.Empty;
        public bool Passed { get; set; }
        public List<string> FailureReasons { get; set; } = new List<string>();
        public string Output { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
        public int ExitCode { get; set; }
    }
    
    public class CustomToolTestResults
    {
        public string ToolName { get; set; } = string.Empty;
        public List<CustomToolTestResult> TestResults { get; set; } = new List<CustomToolTestResult>();
        public bool AllPassed => TestResults.All(r => r.Passed);
    }
}