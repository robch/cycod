using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShellExecution.Results;

namespace ProcessExecution
{
    /// <summary>
    /// Represents resource limits for processes and shells.
    /// </summary>
    public class ResourceLimits
    {
        /// <summary>
        /// Gets or sets the maximum memory usage in bytes.
        /// </summary>
        public long? MaxMemoryBytes { get; set; }

        /// <summary>
        /// Gets or sets the maximum CPU percentage (0-100).
        /// </summary>
        public int? MaxCpuPercent { get; set; }

        /// <summary>
        /// Gets or sets the maximum execution time.
        /// </summary>
        public TimeSpan? MaxExecutionTime { get; set; }

        /// <summary>
        /// Gets or sets the maximum output bytes.
        /// </summary>
        public long? MaxOutputBytes { get; set; }

        /// <summary>
        /// Gets a value indicating whether any limits are set.
        /// </summary>
        public bool HasLimits => MaxMemoryBytes.HasValue || MaxCpuPercent.HasValue || 
                                 MaxExecutionTime.HasValue || MaxOutputBytes.HasValue;
    }

    /// <summary>
    /// Represents resource usage information for a process or shell.
    /// </summary>
    public class ResourceUsage
    {
        /// <summary>
        /// Gets or sets the memory usage in bytes.
        /// </summary>
        public long MemoryBytes { get; set; }

        /// <summary>
        /// Gets or sets the CPU usage percentage (0-100).
        /// </summary>
        public double CpuPercent { get; set; }

        /// <summary>
        /// Gets or sets the execution time.
        /// </summary>
        public TimeSpan ExecutionTime { get; set; }

        /// <summary>
        /// Gets or sets the output size in bytes.
        /// </summary>
        public long OutputBytes { get; set; }

        /// <summary>
        /// Gets or sets the time when the measurement was taken.
        /// </summary>
        public DateTime MeasurementTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Event arguments for resource warning events.
    /// </summary>
    public class ResourceWarningEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the name of the resource.
        /// </summary>
        public string ResourceName { get; }

        /// <summary>
        /// Gets the type of resource.
        /// </summary>
        public string ResourceType { get; }

        /// <summary>
        /// Gets the exceeded limits description.
        /// </summary>
        public List<string> ExceededLimits { get; }

        /// <summary>
        /// Gets the current resource usage.
        /// </summary>
        public ResourceUsage CurrentUsage { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceWarningEventArgs"/> class.
        /// </summary>
        public ResourceWarningEventArgs(string resourceName, string resourceType, List<string> exceededLimits, ResourceUsage currentUsage)
        {
            ResourceName = resourceName;
            ResourceType = resourceType;
            ExceededLimits = exceededLimits;
            CurrentUsage = currentUsage;
        }
    }

    /// <summary>
    /// Event arguments for resource violation events.
    /// </summary>
    public class ResourceViolationEventArgs : ResourceWarningEventArgs
    {
        /// <summary>
        /// Gets the action that will be taken.
        /// </summary>
        public string Action { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceViolationEventArgs"/> class.
        /// </summary>
        public ResourceViolationEventArgs(
            string resourceName, 
            string resourceType, 
            List<string> exceededLimits, 
            ResourceUsage currentUsage, 
            string action)
            : base(resourceName, resourceType, exceededLimits, currentUsage)
        {
            Action = action;
        }
    }

    /// <summary>
    /// Tracks resource usage for managed processes and shells.
    /// </summary>
    public class ResourceMonitor
    {
        private class ManagedResource
        {
            public required Process Process { get; set; }
            public required string Name { get; set; }
            public required string Type { get; set; }
            public required ResourceLimits Limits { get; set; }
            public DateTime StartTime { get; set; } = DateTime.Now;
            public long OutputBytesTracked { get; set; }
            public bool WarningIssued { get; set; }
            public DateTime? GracePeriodEnd { get; set; }
        }

        private readonly ConcurrentDictionary<string, ManagedResource> _resources = new ConcurrentDictionary<string, ManagedResource>();
        private readonly Timer _monitoringTimer;
        private readonly TimeSpan _defaultMonitoringInterval = TimeSpan.FromSeconds(5);
        private readonly TimeSpan _defaultGracePeriod = TimeSpan.FromMinutes(1);
        private readonly object _syncLock = new object();

        /// <summary>
        /// Event raised when a resource is approaching its limits.
        /// </summary>
        public event EventHandler<ResourceWarningEventArgs> ResourceWarning;

        /// <summary>
        /// Event raised when a resource has exceeded its limits and action will be taken.
        /// </summary>
        public event EventHandler<ResourceViolationEventArgs> ResourceViolation;

        /// <summary>
        /// Gets or sets the system-wide maximum number of concurrent processes.
        /// </summary>
        public int MaxConcurrentProcesses { get; set; } = 10;

        /// <summary>
        /// Gets or sets the grace period after a warning before taking action.
        /// </summary>
        public TimeSpan GracePeriod { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceMonitor"/> class.
        /// </summary>
        public ResourceMonitor()
        {
            GracePeriod = _defaultGracePeriod;
            _monitoringTimer = new Timer(MonitorResources!, null, _defaultMonitoringInterval, _defaultMonitoringInterval);
            
            // Initialize events to prevent nullable warnings
            ResourceWarning += (s, e) => { };
            ResourceViolation += (s, e) => { };
        }

        /// <summary>
        /// Registers a process for monitoring.
        /// </summary>
        /// <param name="process">The process to monitor.</param>
        /// <param name="name">Name of the process.</param>
        /// <param name="type">Type of resource (e.g., "Process" or "Shell").</param>
        /// <param name="limits">Optional resource limits.</param>
        public void RegisterResource(Process process, string name, string type, ResourceLimits? limits = null)
        {
            if (process == null || string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Process and name are required");
            }

            var resource = new ManagedResource
            {
                Process = process,
                Name = name,
                Type = type ?? "Process",
                Limits = limits ?? new ResourceLimits(),
                StartTime = DateTime.Now
            };

            _resources[name] = resource;
        }

        /// <summary>
        /// Unregisters a resource from monitoring.
        /// </summary>
        /// <param name="name">Name of the resource to unregister.</param>
        /// <returns>True if the resource was unregistered, false otherwise.</returns>
        public bool UnregisterResource(string name)
        {
            return _resources.TryRemove(name, out _);
        }

        /// <summary>
        /// Gets the current resource usage for a named resource.
        /// </summary>
        /// <param name="name">Name of the resource.</param>
        /// <returns>Resource usage information, or null if the resource is not found.</returns>
        public ResourceUsage GetResourceUsage(string name)
        {
            if (!_resources.TryGetValue(name, out var resource) || resource.Process == null || resource.Process.HasExited)
            {
                return null!;
            }

            try
            {
                resource.Process.Refresh();

                return new ResourceUsage
                {
                    MemoryBytes = resource.Process.WorkingSet64,
                    CpuPercent = GetCpuUsage(resource.Process),
                    ExecutionTime = DateTime.Now - resource.StartTime,
                    OutputBytes = resource.OutputBytesTracked,
                    MeasurementTime = DateTime.Now
                };
            }
            catch
            {
                return null!;
            }
        }

        /// <summary>
        /// Sets resource limits for a named resource.
        /// </summary>
        /// <param name="name">Name of the resource.</param>
        /// <param name="limits">The limits to set.</param>
        /// <returns>True if limits were set, false if resource not found.</returns>
        public bool SetResourceLimits(string name, ResourceLimits limits)
        {
            if (!_resources.TryGetValue(name, out var resource))
            {
                return false;
            }

            resource.Limits = limits ?? new ResourceLimits();
            resource.WarningIssued = false;
            resource.GracePeriodEnd = null;
            
            return true;
        }

        /// <summary>
        /// Updates the tracked output size for a resource.
        /// </summary>
        /// <param name="name">Name of the resource.</param>
        /// <param name="outputBytes">Current output size in bytes.</param>
        public void UpdateOutputSize(string name, long outputBytes)
        {
            if (_resources.TryGetValue(name, out var resource))
            {
                resource.OutputBytesTracked = outputBytes;
            }
        }

        /// <summary>
        /// Checks if a resource is exceeding its limits.
        /// </summary>
        /// <param name="name">Name of the resource.</param>
        /// <param name="exceededLimits">List of descriptions for exceeded limits.</param>
        /// <returns>True if any limits are exceeded, false otherwise.</returns>
        public bool IsExceedingLimits(string name, out List<string> exceededLimits)
        {
            exceededLimits = new List<string>();
            
            if (!_resources.TryGetValue(name, out var resource) || !resource.Limits.HasLimits)
            {
                return false;
            }

            var usage = GetResourceUsage(name);
            if (usage == null)
            {
                return false;
            }

            if (resource.Limits.MaxMemoryBytes.HasValue && usage.MemoryBytes > resource.Limits.MaxMemoryBytes.Value)
            {
                exceededLimits.Add($"Memory: {FormatBytes(usage.MemoryBytes)} > {FormatBytes(resource.Limits.MaxMemoryBytes.Value)}");
            }

            if (resource.Limits.MaxCpuPercent.HasValue && usage.CpuPercent > resource.Limits.MaxCpuPercent.Value)
            {
                exceededLimits.Add($"CPU: {usage.CpuPercent:F1}% > {resource.Limits.MaxCpuPercent.Value}%");
            }

            if (resource.Limits.MaxExecutionTime.HasValue && usage.ExecutionTime > resource.Limits.MaxExecutionTime.Value)
            {
                exceededLimits.Add($"Time: {usage.ExecutionTime.TotalSeconds:F1}s > {resource.Limits.MaxExecutionTime.Value.TotalSeconds:F1}s");
            }

            if (resource.Limits.MaxOutputBytes.HasValue && usage.OutputBytes > resource.Limits.MaxOutputBytes.Value)
            {
                exceededLimits.Add($"Output: {FormatBytes(usage.OutputBytes)} > {FormatBytes(resource.Limits.MaxOutputBytes.Value)}");
            }

            return exceededLimits.Count > 0;
        }

        /// <summary>
        /// Gets the count of currently monitored resources.
        /// </summary>
        /// <returns>Number of resources being monitored.</returns>
        public int GetResourceCount()
        {
            return _resources.Count;
        }

        /// <summary>
        /// Gets all resource names.
        /// </summary>
        /// <returns>List of resource names.</returns>
        public List<string> GetAllResourceNames()
        {
            return _resources.Keys.ToList();
        }

        /// <summary>
        /// Cleans up resources that are no longer valid.
        /// </summary>
        public void CleanupResources()
        {
            foreach (var kvp in _resources.ToList())
            {
                try
                {
                    if (kvp.Value.Process == null || kvp.Value.Process.HasExited)
                    {
                        _resources.TryRemove(kvp.Key, out _);
                    }
                }
                catch
                {
                    // Process likely already disposed, remove it
                    _resources.TryRemove(kvp.Key, out _);
                }
            }
        }

        private void MonitorResources(object state)
        {
            try
            {
                // Clean up resources first
                CleanupResources();

                foreach (var kvp in _resources.ToList())
                {
                    string resourceName = kvp.Key;
                    ManagedResource resource = kvp.Value;
                    
                    if (resource.Process == null || resource.Process.HasExited)
                    {
                        continue;
                    }

                    // Check if limits are exceeded
                    if (IsExceedingLimits(resourceName, out var exceededLimits))
                    {
                        var usage = GetResourceUsage(resourceName);
                        
                        // If we haven't issued a warning yet, do so now
                        if (!resource.WarningIssued)
                        {
                            resource.WarningIssued = true;
                            resource.GracePeriodEnd = DateTime.Now + GracePeriod;
                            
                            ResourceWarning?.Invoke(this, new ResourceWarningEventArgs(
                                resourceName,
                                resource.Type,
                                exceededLimits,
                                usage
                            ));
                        }
                        // If grace period has expired, take action
                        else if (resource.GracePeriodEnd.HasValue && DateTime.Now > resource.GracePeriodEnd.Value)
                        {
                            const string action = "Terminating process";
                            
                            ResourceViolation?.Invoke(this, new ResourceViolationEventArgs(
                                resourceName,
                                resource.Type,
                                exceededLimits,
                                usage,
                                action
                            ));
                            
                            try
                            {
                                if (!resource.Process.HasExited)
                                {
                                    resource.Process.Kill();
                                }
                            }
                            catch
                            {
                                // Ignore errors during cleanup
                            }
                            finally
                            {
                                _resources.TryRemove(resourceName, out _);
                            }
                        }
                    }
                    else
                    {
                        // Reset warning status if no longer exceeding limits
                        resource.WarningIssued = false;
                        resource.GracePeriodEnd = null;
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleHelpers.WriteDebugLine($"Error in ResourceMonitor: {ex.Message}");
            }
        }

        private double GetCpuUsage(Process process)
        {
            // A simple approximation based on current process CPU time
            // In a real implementation, you would track usage over time for accuracy
            try
            {
                return process.TotalProcessorTime.TotalMilliseconds / 
                       (Environment.ProcessorCount * (DateTime.Now - process.StartTime).TotalMilliseconds) * 100.0;
            }
            catch
            {
                return 0;
            }
        }

        private static string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            double number = bytes;
            while (number >= 1024 && counter < suffixes.Length - 1)
            {
                number /= 1024;
                counter++;
            }
            return $"{number:0.##} {suffixes[counter]}";
        }
    }
}