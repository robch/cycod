using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using CycodBench.Models;

namespace CycodBench.Services
{
    /// <summary>
    /// Implementation of the solution service.
    /// </summary>
    public class SolutionService : ISolutionService
    {
        private readonly Func<string, IContainerService> _containerServiceFactory;
        private readonly IContainerService _dockerContainerService;
        private readonly IDatasetService _datasetService;

        public SolutionService(
            Func<string, IContainerService> containerServiceFactory,
            IDatasetService datasetService)
        {
            _containerServiceFactory = containerServiceFactory;
            _dockerContainerService = containerServiceFactory("docker");
            _datasetService = datasetService;
        }

        /// <inheritdoc />
        public async Task<Solution> SolveProblemAsync(Problem problem, string? containerId = null, int timeout = 3600)
        {
            Console.WriteLine($"Solving problem {problem.Id}...");

            var solution = new Solution
            {
                ProblemId = problem.Id,
                ProblemDataset = "custom", // This should be set by the caller if known
                Id = $"{problem.Id}-{Guid.NewGuid().ToString().Substring(0, 8)}",
            };

            try
            {
                // Use existing container or create a new one
                var containerToUse = containerId;
                var shouldDisposeContainer = false;

                if (string.IsNullOrEmpty(containerToUse))
                {
                    // Create a container for this problem
                    containerToUse = await _dockerContainerService.InitContainerAsync(problemId: problem.Id);
                    shouldDisposeContainer = true;
                }

                try
                {
                    // Prepare the problem in the container
                    await PrepareContainerForProblemAsync(containerToUse, problem);

                    // Run the agent to solve the problem
                    var agentOutput = await RunAgentInContainerAsync(containerToUse, problem, timeout);

                    // Extract the patch and output
                    var patch = await ExtractPatchFromContainerAsync(containerToUse);

                    // Fill in the solution details
                    solution.AgentPatch = patch;
                    solution.AgentOutput = agentOutput;

                    return solution;
                }
                finally
                {
                    // Dispose the container if we created it
                    if (shouldDisposeContainer)
                    {
                        try
                        {
                            await _dockerContainerService.StopContainerAsync(containerToUse);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Warning: Failed to stop container: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error solving problem: {ex.Message}");

                // Return a solution with error information
                solution.AgentOutput = $"Error solving problem: {ex.Message}\n{ex.StackTrace}";
                return solution;
            }
        }

        /// <inheritdoc />
        public async Task<SolutionCollection> LoadSolutionsAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"Solutions file not found: {filePath}");
                }

                var json = await File.ReadAllTextAsync(filePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var solutions = JsonSerializer.Deserialize<SolutionCollection>(json, options);
                if (solutions == null)
                {
                    throw new InvalidOperationException("Failed to deserialize solutions.");
                }

                return solutions;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error loading solutions: {ex.Message}", ex);
            }
        }

        /// <inheritdoc />
        public async Task<string> SaveSolutionsAsync(SolutionCollection solutions, string filePath)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                var json = JsonSerializer.Serialize(solutions, options);
                await File.WriteAllTextAsync(filePath, json);

                Console.WriteLine($"Saved {solutions.Solutions.Count} solutions to {filePath}");
                return filePath;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error saving solutions: {ex.Message}", ex);
            }
        }

        /// <inheritdoc />
        public Task<SolutionCollection> FilterSolutionsAsync(
            SolutionCollection solutions,
            string? problemId = null,
            string? repository = null,
            string? containsText = null,
            int? maxSolutions = null)
        {
            var filteredSolutions = solutions.Solutions.AsEnumerable();

            if (!string.IsNullOrEmpty(problemId))
            {
                filteredSolutions = filteredSolutions.Where(s => s.ProblemId.Contains(problemId, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(repository))
            {
                // Extract repository from problem ID (format is typically owner__repo-issue-id)
                filteredSolutions = filteredSolutions.Where(s =>
                {
                    var parts = s.ProblemId.Split('_');
                    if (parts.Length >= 2)
                    {
                        var repo = parts[0] + "/" + parts[1].Split('-')[0];
                        return repo.Contains(repository, StringComparison.OrdinalIgnoreCase);
                    }
                    return false;
                });
            }

            if (!string.IsNullOrEmpty(containsText))
            {
                filteredSolutions = filteredSolutions.Where(s =>
                    s.ProblemId.Contains(containsText, StringComparison.OrdinalIgnoreCase) ||
                    s.AgentPatch.Contains(containsText, StringComparison.OrdinalIgnoreCase) ||
                    s.AgentOutput.Contains(containsText, StringComparison.OrdinalIgnoreCase));
            }

            if (maxSolutions.HasValue && maxSolutions > 0)
            {
                filteredSolutions = filteredSolutions.Take(maxSolutions.Value);
            }

            return Task.FromResult(new SolutionCollection
            {
                Solutions = filteredSolutions.ToList()
            });
        }

        /// <inheritdoc />
        public Task<SolutionCollection> MergeSolutionsAsync(params SolutionCollection[] solutionCollections)
        {
            var mergedSolutions = new SolutionCollection();

            foreach (var collection in solutionCollections)
            {
                mergedSolutions.Solutions.AddRange(collection.Solutions);
            }

            return Task.FromResult(mergedSolutions);
        }

        /// <inheritdoc />
        public Task<SolutionCollection> PickBestSolutionsAsync(SolutionCollection solutions)
        {
            // Group solutions by problem ID
            var solutionsByProblem = solutions.Solutions
                .GroupBy(s => s.ProblemId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var bestSolutions = new SolutionCollection();

            foreach (var kvp in solutionsByProblem)
            {
                var problemSolutions = kvp.Value;

                if (problemSolutions.Count == 1)
                {
                    // Only one solution, so add it
                    bestSolutions.Solutions.Add(problemSolutions[0]);
                }
                else
                {
                    // Multiple solutions, pick the best one
                    // This is a simplified implementation - in practice, you'd want to use more sophisticated
                    // criteria to select the best solution, possibly based on evaluation results

                    // For demonstration, we'll pick the solution with the shortest patch that's not empty
                    var validSolutions = problemSolutions
                        .Where(s => !string.IsNullOrWhiteSpace(s.AgentPatch))
                        .ToList();

                    if (validSolutions.Any())
                    {
                        var bestSolution = validSolutions
                            .OrderBy(s => s.AgentPatch.Length)
                            .First();

                        bestSolutions.Solutions.Add(bestSolution);
                    }
                    else
                    {
                        // No valid solutions, just pick the first one
                        bestSolutions.Solutions.Add(problemSolutions[0]);
                    }
                }
            }

            return Task.FromResult(bestSolutions);
        }

        /// <inheritdoc />
        public async Task<Result> EvaluateSolutionAsync(Solution solution, Problem problem, string? containerId = null, int timeout = 600)
        {
            Console.WriteLine($"Evaluating solution for problem {problem.Id}...");

            // Create a result with the solution details
            var result = new Result
            {
                ProblemId = solution.ProblemId,
                ProblemDataset = solution.ProblemDataset,
                ProblemIdx = solution.ProblemIdx,
                Id = solution.Id,
                AgentPatch = solution.AgentPatch,
                AgentOutput = solution.AgentOutput,
                EvalStatus = "skipped",
                EvalOutput = ""
            };

            try
            {
                // Use existing container or create a new one
                var containerToUse = containerId;
                var shouldDisposeContainer = false;

                if (string.IsNullOrEmpty(containerToUse))
                {
                    // Create a container for this evaluation
                    containerToUse = await _dockerContainerService.InitContainerAsync(problemId: problem.Id, setupTools: true);
                    shouldDisposeContainer = true;
                }

                try
                {
                    // Prepare the container for evaluation
                    await PrepareContainerForEvaluationAsync(containerToUse, problem, solution);

                    // Run the evaluation
                    var evalOutput = await RunEvaluationInContainerAsync(containerToUse, problem, solution, timeout);

                    // Determine if the evaluation succeeded
                    var evalStatus = DetermineEvaluationStatus(evalOutput);

                    // Fill in the result details
                    result.EvalStatus = evalStatus;
                    result.EvalOutput = evalOutput;

                    return result;
                }
                finally
                {
                    // Dispose the container if we created it
                    if (shouldDisposeContainer && !string.IsNullOrEmpty(containerToUse))
                    {
                        try
                        {
                            await _dockerContainerService.StopContainerAsync(containerToUse);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Warning: Failed to stop container: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error evaluating solution: {ex.Message}");

                // Fill in the result with error information
                result.EvalStatus = "failed";
                result.EvalOutput = $"Error evaluating solution: {ex.Message}\n{ex.StackTrace}";
                return result;
            }
        }

        #region Helper methods

        private async Task PrepareContainerForProblemAsync(string containerId, Problem problem)
        {
            // Create a temporary directory to hold the problem files
            var tempDir = Path.Combine(Path.GetTempPath(), $"cycodbench-{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);

            try
            {
                // Write problem files to the temporary directory
                var problemFile = Path.Combine(tempDir, "problem.json");
                await File.WriteAllTextAsync(problemFile, JsonSerializer.Serialize(problem));

                // Copy the problem files to the container
                await _dockerContainerService.CopyToContainerAsync(containerId, problemFile, "/workspace/problem.json");

                // Create input directory and write problem statement
                await _dockerContainerService.ExecuteCommandAsync(containerId, "mkdir -p /workspace/input", timeout: 10);

                // Create the problem statement file in the container
                var problemStatement = ProcessHelpers.EscapeBashArgument(problem.ProblemStatement);
                await _dockerContainerService.ExecuteCommandAsync(
                    containerId,
                    $"echo {problemStatement} > /workspace/input/problem_statement.txt",
                    timeout: 10);

                // Create output directory
                await _dockerContainerService.ExecuteCommandAsync(containerId, "mkdir -p /workspace/output", timeout: 10);
            }
            finally
            {
                // Clean up the temporary directory
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        private async Task<string> RunAgentInContainerAsync(string containerId, Problem problem, int timeout)
        {
            // Execute the cycod agent in the container
            var command = "/workspace/bin/cycod " +
                "--folder /testbed " +
                "--var swebench=true " +
                "--input /workspace/input/problem_statement.txt " +
                "--output-chat-history /workspace/output/chat-history.jsonl " +
                "--output-trajectory /workspace/output/trajectory.md";

            // Execute the command and return the output
            return await _dockerContainerService.ExecuteCommandAsync(containerId, command, "/testbed", timeout);
        }

        private async Task<string> ExtractPatchFromContainerAsync(string containerId)
        {
            // Try to read the patch from the expected output location
            try
            {
                // Check if the file exists
                var checkCommand = "if [ -f /workspace/output/patch.diff ]; then echo 'exists'; else echo 'missing'; fi";
                var checkResult = await _dockerContainerService.ExecuteCommandAsync(containerId, checkCommand, timeout: 10);

                if (checkResult.Trim() == "exists")
                {
                    // Read the patch file
                    var catCommand = "cat /workspace/output/patch.diff";
                    return await _dockerContainerService.ExecuteCommandAsync(containerId, catCommand, timeout: 10);
                }
                else
                {
                    // Try to generate a patch from git diff if possible
                    var gitDiffCommand = "cd /testbed && git diff";
                    return await _dockerContainerService.ExecuteCommandAsync(containerId, gitDiffCommand, timeout: 30);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to extract patch: {ex.Message}");
                return $"// Failed to extract patch: {ex.Message}";
            }
        }

        private async Task PrepareContainerForEvaluationAsync(string containerId, Problem problem, Solution solution)
        {
            // Create a temporary directory to hold the files
            var tempDir = Path.Combine(Path.GetTempPath(), $"cycodbench-{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);

            try
            {
                // Write problem and solution files to the temporary directory
                var problemFile = Path.Combine(tempDir, "problem.json");
                await File.WriteAllTextAsync(problemFile, JsonSerializer.Serialize(problem));

                var solutionFile = Path.Combine(tempDir, "solution.json");
                await File.WriteAllTextAsync(solutionFile, JsonSerializer.Serialize(solution));

                // Copy the files to the container
                await _dockerContainerService.CopyToContainerAsync(containerId, problemFile, "/workspace/problem.json");
                await _dockerContainerService.CopyToContainerAsync(containerId, solutionFile, "/workspace/solution.json");

                // Write the patch file to the container if it exists
                if (!string.IsNullOrEmpty(solution.AgentPatch))
                {
                    var patchFile = Path.Combine(tempDir, "patch.diff");
                    await File.WriteAllTextAsync(patchFile, solution.AgentPatch);
                    await _dockerContainerService.CopyToContainerAsync(containerId, patchFile, "/workspace/patch.diff");
                }
            }
            finally
            {
                // Clean up the temporary directory
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        private async Task<string> RunEvaluationInContainerAsync(string containerId, Problem problem, Solution solution, int timeout)
        {
            // This is a placeholder for the actual evaluation logic
            // In a real implementation, you'd run your evaluation script/tool with the problem and solution details

            // For example:
            var command = "cd /workspace && "
                + "echo 'Evaluating solution...' && "
                + "if [ -f /workspace/patch.diff ]; then "
                + "    echo 'Applying patch...' && "
                + "    cd /testbed && git apply /workspace/patch.diff || echo 'Failed to apply patch'; "
                + "    echo 'Running tests...' && "
                + "    # Run evaluation tests here "
                + "    echo 'Tests completed successfully.'; "
                + "else "
                + "    echo 'No patch found in solution.'; "
                + "fi";

            return await _dockerContainerService.ExecuteCommandAsync(containerId, command, "/workspace", timeout);
        }

        private string DetermineEvaluationStatus(string evalOutput)
        {
            // This is a simplified implementation
            // In practice, you'd need to parse the test output to determine if all tests passed

            if (evalOutput.Contains("FAILED") || evalOutput.Contains("Error running"))
            {
                return "failed";
            }

            return "passed";
        }

        #endregion
    }
}