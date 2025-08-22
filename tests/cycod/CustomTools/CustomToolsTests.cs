using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CycoDev.CustomTools;
using CycoDev.CustomTools.Models;
using Xunit;
// Use only XUnit assertions
using Assert = Xunit.Assert;

namespace CycoDev.Tests.CustomTools
{
    public class CustomToolsTests
    {
        [Fact]
        public void TestCustomToolDefinitionValidation()
        {
            // Create a valid tool
            var tool = new CustomToolDefinition
            {
                Name = "test-tool",
                Description = "A test tool",
                BashCommand = "echo 'Hello, {NAME}!'",
                Parameters = new Dictionary<string, CustomToolParameter>
                {
                    ["NAME"] = new CustomToolParameter
                    {
                        Type = "string",
                        Description = "Name to greet",
                        Required = true
                    }
                },
                Tags = new List<string> { "test", "read" }
            };

            // Validate should pass
            Assert.True(tool.Validate(out string? errorMessage));
            Assert.Null(errorMessage);

            // Create an invalid tool (missing required fields)
            var invalidTool = new CustomToolDefinition
            {
                Name = "invalid-tool"
            };

            // Validate should fail
            Assert.False(invalidTool.Validate(out errorMessage));
            Assert.NotNull(errorMessage);
        }

        [Fact]
        public void TestCustomToolParameterValidation()
        {
            // Create a parameter
            var parameter = new CustomToolParameter
            {
                Type = "string",
                Description = "Test parameter",
                Required = true
            };

            // Validate string parameter
            Assert.True(parameter.Validate("test", out string? errorMessage));
            Assert.Null(errorMessage);

            // Create a number parameter
            var numberParam = new CustomToolParameter
            {
                Type = "number",
                Description = "Number parameter",
                Required = true
            };

            // Validate number parameter
            Assert.True(numberParam.Validate(42, out errorMessage));
            Assert.Null(errorMessage);
            Assert.False(numberParam.Validate("not a number", out errorMessage));
            Assert.NotNull(errorMessage);

            // Create a boolean parameter
            var boolParam = new CustomToolParameter
            {
                Type = "boolean",
                Description = "Boolean parameter",
                Required = true
            };

            // Validate boolean parameter
            Assert.True(boolParam.Validate(true, out errorMessage));
            Assert.Null(errorMessage);
            Assert.False(boolParam.Validate("not a boolean", out errorMessage));
            Assert.NotNull(errorMessage);
        }

        [Fact]
        public async Task TestCustomToolFactory()
        {
            var factory = new CustomToolFactory();
            
            // Add a tool programmatically
            var tool = new CustomToolDefinition
            {
                Name = "factory-test",
                Description = "A test tool",
                BashCommand = "echo 'Hello, World!'",
                Tags = new List<string> { "test", "read" }
            };

            factory.AddTool(tool);
            
            // Get the tool
            var retrievedTool = factory.GetTool("factory-test");
            Assert.NotNull(retrievedTool);
            Assert.Equal("factory-test", retrievedTool.Name);
            Assert.Equal("A test tool", retrievedTool.Description);
        }

        [Fact]
        public async Task TestCustomToolExecutor()
        {
            var executor = new CustomToolExecutor();
            
            // Create a simple tool
            var tool = new CustomToolDefinition
            {
                Name = "echo-test",
                Description = "A test tool",
                BashCommand = "echo 'Hello, {NAME}!'",
                Parameters = new Dictionary<string, CustomToolParameter>
                {
                    ["NAME"] = new CustomToolParameter
                    {
                        Type = "string",
                        Description = "Name to greet",
                        Required = true
                    }
                }
            };

            // Execute the tool
            var result = await executor.ExecuteToolAsync(tool, new Dictionary<string, object?>
            {
                ["NAME"] = "World"
            });
            
            // Check the result
            Assert.Equal(0, result.ExitCode);
            Assert.Contains("Hello, World!", result.Output);
        }

        [Fact]
        public async Task TestMultiStepTool()
        {
            var executor = new CustomToolExecutor();
            
            // Create a multi-step tool
            var tool = new CustomToolDefinition
            {
                Name = "multi-step-test",
                Description = "A multi-step test tool",
                Steps = new List<CustomToolStep>
                {
                    new CustomToolStep
                    {
                        Name = "step1",
                        BashCommand = "echo 'Step 1: {NAME}'"
                    },
                    new CustomToolStep
                    {
                        Name = "step2",
                        BashCommand = "echo 'Step 2: {step1.output}'"
                    }
                },
                Parameters = new Dictionary<string, CustomToolParameter>
                {
                    ["NAME"] = new CustomToolParameter
                    {
                        Type = "string",
                        Description = "Name to use",
                        Required = true
                    }
                }
            };

            // Execute the tool
            var result = await executor.ExecuteToolAsync(tool, new Dictionary<string, object?>
            {
                ["NAME"] = "Test"
            });
            
            // Check the result
            Assert.Equal(0, result.ExitCode);
            Assert.Contains("Step 1: Test", result.Output);
            Assert.Contains("Step 2:", result.Output);
        }
    }
}