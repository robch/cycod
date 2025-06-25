using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CycodBench.Services;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Command to generate a report from evaluation results.
/// </summary>
public class ResultsReportCommand : ResultsCommand
{
    /// <summary>
    /// Path to results file
    /// </summary>
    public string? ResultsFilePath { get; set; }
    
    /// <summary>
    /// Show detailed information in the report
    /// </summary>
    public bool Verbose { get; set; } = false;

    public ResultsReportCommand()
    {
        // Set default output path
        OutputPath = "results.md";
    }

    public override string GetCommandName()
    {
        return "results report";
    }

    public override bool IsEmpty()
    {
        return string.IsNullOrEmpty(ResultsFilePath);
    }
    
    public override Command Validate()
    {
        if (string.IsNullOrEmpty(ResultsFilePath))
        {
            throw new CommandLineException("Results file path must be specified for 'results report'");
        }
        
        return this;
    }

    public override async Task<object> ExecuteAsync(bool interactive)
    {
        Console.WriteLine($"Generating report from results file: {ResultsFilePath}");
        
        try
        {
            // Get the result service
            var serviceProvider = ServiceConfiguration.GetServiceProvider();
            var resultService = serviceProvider.GetRequiredService<IResultService>();
            
            // Load results
            if (!File.Exists(ResultsFilePath))
            {
                throw new FileNotFoundException($"Results file not found: {ResultsFilePath}");
            }
            
            var results = await resultService.LoadResultsAsync(ResultsFilePath);
            Console.WriteLine($"Loaded {results.Results.Count} results from {ResultsFilePath}");
            
            // Generate report
            string reportContent = await resultService.GenerateReportAsync(results, Verbose);
            
            // Save report
            await File.WriteAllTextAsync(OutputPath!, reportContent);
            
            Console.WriteLine($"Report generated and saved to {OutputPath}");
            
            return OutputPath!;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error generating report: {ex.Message}");
            return false;
        }
    }
}