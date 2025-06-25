using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CycodBench.Services;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Command to download a SWE-bench problem dataset.
/// </summary>
public class ProblemsDownloadCommand : ProblemsCommand
{
    /// <summary>
    /// Dataset name to download: verified, full, or lite
    /// </summary>
    public string? DatasetName { get; set; } = "verified";
    
    /// <summary>
    /// Force redownload even if dataset exists
    /// </summary>
    public bool Force { get; set; } = false;

    public override string GetCommandName()
    {
        return "problems download";
    }

    public override async Task<object> ExecuteAsync(bool interactive)
    {
        Console.WriteLine($"Downloading {DatasetName} dataset...");

        try
        {
            // Get the dataset service
            var serviceProvider = ServiceConfiguration.GetServiceProvider();
            var datasetService = serviceProvider.GetRequiredService<IDatasetService>();

            // Download the dataset
            var filePath = await datasetService.DownloadDatasetAsync(DatasetName!, OutputPath, Force);

            Console.WriteLine($"Successfully downloaded dataset to {filePath}");
            return filePath;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error downloading dataset: {ex.Message}");
            return false;
        }
    }

    public override bool IsEmpty()
    {
        return string.IsNullOrEmpty(DatasetName);
    }
    
    public override Command Validate()
    {
        if (string.IsNullOrEmpty(DatasetName))
        {
            DatasetName = "verified";
        }
        
        return this;
    }
}