using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace CycodBench.Services
{
    /// <summary>
    /// Configures services for dependency injection.
    /// </summary>
    public static class ServiceConfiguration
    {
        /// <summary>
        /// Configure services for the application.
        /// </summary>
        /// <returns>The service provider</returns>
        public static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            
            // Register core services
            services.AddSingleton<IDatasetService, DatasetService>();
            
            // Register container services with factory pattern
            services.AddSingleton<DockerContainerService>();
            services.AddSingleton<Func<string, IContainerService>>(serviceProvider => provider =>
            {
                return provider.ToLower() switch
                {
                    "docker" => serviceProvider.GetRequiredService<DockerContainerService>(),
                    "aca" => throw new NotImplementedException("Azure Container Apps provider not implemented"),
                    "aws" => throw new NotImplementedException("AWS provider not implemented"),
                    _ => throw new ArgumentOutOfRangeException(provider)
                };
            });
            
            // Register solution and result services
            services.AddSingleton<ISolutionService, SolutionService>();
            services.AddSingleton<IResultService, ResultService>();
            
            // Register benchmark service
            services.AddSingleton<IBenchmarkService, BenchmarkService>();
            
            return services.BuildServiceProvider();
        }
        
        /// <summary>
        /// Gets a service provider with all required services.
        /// </summary>
        /// <returns>The service provider</returns>
        public static IServiceProvider GetServiceProvider()
        {
            return ConfigureServices();
        }
    }
}