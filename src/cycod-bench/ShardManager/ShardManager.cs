using CycodBench.Configuration;
using CycodBench.Logging;
using CycodBench.Models;

namespace CycodBench.ShardManager;

/// <summary>
/// Implementation of the IShardManager interface for managing SWE-bench problem sharding.
/// </summary>
public class ShardManager : IShardManager
{
    private readonly IConfiguration _config;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ShardManager"/> class.
    /// </summary>
    /// <param name="config">The configuration.</param>
    /// <param name="logger">The logger.</param>
    public ShardManager(IConfiguration config, ILogger logger)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public int ShardCount => _config.ShardCount;

    /// <inheritdoc />
    public int ShardId => _config.ShardId;

    /// <inheritdoc />
    public IEnumerable<SwebenchProblem> GetProblemsForCurrentShard(IEnumerable<SwebenchProblem> allProblems)
    {
        return GetProblemsForShard(allProblems, ShardId);
    }

    /// <inheritdoc />
    public IEnumerable<SwebenchProblem> GetProblemsForShard(IEnumerable<SwebenchProblem> allProblems, int shardId)
    {
        if (allProblems == null)
        {
            throw new ArgumentNullException(nameof(allProblems));
        }

        if (shardId < 0 || (ShardCount > 0 && shardId >= ShardCount))
        {
            throw new ArgumentOutOfRangeException(nameof(shardId), $"Shard ID must be between 0 and {ShardCount - 1}");
        }

        // If shard count is 1 or 0, return all problems
        if (ShardCount <= 1)
        {
            return allProblems.ToList();
        }

        _logger.Info($"Filtering problems for shard {shardId} of {ShardCount}...");
        
        // Filter problems by their calculated shard ID
        var filteredProblems = allProblems.Where(p => CalculateShardId(p) == shardId).ToList();
        _logger.Info($"Assigned {filteredProblems.Count} problems to shard {shardId}");
        
        return filteredProblems;
    }

    /// <inheritdoc />
    public bool IsInCurrentShard(SwebenchProblem problem)
    {
        if (problem == null)
        {
            throw new ArgumentNullException(nameof(problem));
        }

        // If shard count is 1 or 0, all problems are in the current shard
        if (ShardCount <= 1)
        {
            return true;
        }

        return CalculateShardId(problem) == ShardId;
    }

    /// <inheritdoc />
    public async Task SaveShardStateAsync(IEnumerable<SwebenchProblem> problems, string outputPath)
    {
        if (problems == null)
        {
            throw new ArgumentNullException(nameof(problems));
        }

        if (string.IsNullOrEmpty(outputPath))
        {
            throw new ArgumentException("Output path cannot be null or empty", nameof(outputPath));
        }

        try
        {
            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Save each problem as a line in the JSONL file
            var problemsList = problems.ToList();
            _logger.Info($"Saving {problemsList.Count} problems to {outputPath}");
            
            using var writer = new StreamWriter(outputPath, false);
            foreach (var problem in problemsList)
            {
                await writer.WriteLineAsync(problem.ToJson());
            }
            
            _logger.Info($"Successfully saved shard state to {outputPath}");
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to save shard state: {ex.Message}");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SwebenchProblem>> LoadShardStateAsync(string inputPath)
    {
        if (string.IsNullOrEmpty(inputPath))
        {
            throw new ArgumentException("Input path cannot be null or empty", nameof(inputPath));
        }

        if (!File.Exists(inputPath))
        {
            _logger.Warning($"Shard state file not found at {inputPath}");
            return Enumerable.Empty<SwebenchProblem>();
        }

        try
        {
            _logger.Info($"Loading shard state from {inputPath}");
            var problems = new List<SwebenchProblem>();
            
            using var reader = new StreamReader(inputPath);
            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                
                try
                {
                    var problem = line.FromJson<SwebenchProblem>();
                    if (problem != null)
                    {
                        problems.Add(problem);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warning($"Failed to parse problem: {ex.Message}");
                }
            }
            
            _logger.Info($"Loaded {problems.Count} problems from shard state");
            return problems;
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to load shard state: {ex.Message}");
            throw;
        }
    }

    /// <inheritdoc />
    public int CalculateShardId(SwebenchProblem problem)
    {
        if (problem == null)
        {
            throw new ArgumentNullException(nameof(problem));
        }

        // If shard count is 1 or 0, assign to shard 0
        if (ShardCount <= 1)
        {
            return 0;
        }

        // Use a consistent hashing algorithm based on the problem ID to determine shard assignment
        // This ensures a deterministic distribution that doesn't change when shard count stays the same
        return Math.Abs(problem.Id.GetHashCode()) % ShardCount;
    }
}