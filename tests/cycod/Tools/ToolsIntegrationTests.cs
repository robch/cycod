using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class ToolsIntegrationTests
{
    [TestInitialize]
    public void TestInitialize()
    {
        // Set environment variable to indicate we're running tests
        Environment.SetEnvironmentVariable("RUNNING_TESTS", "true");
    }

    [TestCleanup]
    public void TestCleanup()
    {
        // Clear environment variable
        Environment.SetEnvironmentVariable("RUNNING_TESTS", null);
    }

    [TestMethod]
    public async Task CanExecuteSimpleTool()
    {
        // Arrange
        var tool = new ToolDefinition
        {
            Name = "simple-test",
            Description = "A simple test tool",
            Tags = new List<string> { "read", "test" },
            Bash = "echo \"Hello, World!\"",
            Timeout = 5000
        };
        
        var parameters = new Dictionary<string, object?>();
        
        // Act
        var executor = new ToolExecutor(tool, parameters);
        var result = await executor.ExecuteAsync();
        
        // Assert
        Assert.IsNotNull(result);
        StringAssert.Contains(result.ToString(), "Hello, World!");
    }
    
    [TestMethod]
    public async Task CanExecuteToolWithParameters()
    {
        // Arrange
        var tool = new ToolDefinition
        {
            Name = "param-test",
            Description = "A tool with parameters",
            Tags = new List<string> { "read", "test" },
            Parameters = new Dictionary<string, ToolParameter>
            {
                ["message"] = new ToolParameter
                {
                    Type = "string",
                    Description = "Message to echo",
                    Default = "Default message"
                }
            },
            Bash = "echo \"{{message}}\""
        };
        
        var parameters = new Dictionary<string, object?>
        {
            ["message"] = "Custom message"
        };
        
        // Act
        var executor = new ToolExecutor(tool, parameters);
        var result = await executor.ExecuteAsync();
        
        // Assert
        Assert.IsNotNull(result);
        StringAssert.Contains(result.ToString(), "Custom message");
    }
    
    [TestMethod]
    public async Task CanExecuteMultiStepTool()
    {
        // Arrange
        var tool = new ToolDefinition
        {
            Name = "multistep-test",
            Description = "A multi-step tool",
            Tags = new List<string> { "read", "test" },
            Steps = new List<ToolStep>
            {
                new ToolStep
                {
                    Name = "step1",
                    Bash = "echo \"Step 1 output\""
                },
                new ToolStep
                {
                    Name = "step2",
                    Bash = "echo \"Step 2 with step1: {{step1.output}}\""
                }
            }
        };
        
        var parameters = new Dictionary<string, object?>();
        
        // Act
        var executor = new ToolExecutor(tool, parameters);
        var result = await executor.ExecuteAsync();
        
        // Assert
        Assert.IsNotNull(result);
        StringAssert.Contains(result.ToString(), "Step 1 output");
        StringAssert.Contains(result.ToString(), "Step 2 with step1");
    }
    
    [TestMethod]
    public void CanCheckToolSecurityLevel()
    {
        // Arrange
        var factory = new CustomToolFunctionFactory();
        
        var readTool = new ToolDefinition
        {
            Name = "read-tool",
            Tags = new List<string> { "read" },
            Bash = "echo \"Read-only tool\""
        };
        
        var writeTool = new ToolDefinition
        {
            Name = "write-tool",
            Tags = new List<string> { "write" },
            Bash = "echo \"Write tool\""
        };
        
        var runTool = new ToolDefinition
        {
            Name = "run-tool",
            Tags = new List<string> { "run" },
            Bash = "echo \"Run tool\""
        };
        
        // Act & Assert
        Assert.AreEqual(ToolSecurityLevel.Read, factory.GetToolSecurityLevel(readTool));
        Assert.AreEqual(ToolSecurityLevel.Write, factory.GetToolSecurityLevel(writeTool));
        Assert.AreEqual(ToolSecurityLevel.Run, factory.GetToolSecurityLevel(runTool));
    }
    
    [TestMethod]
    public void CanCheckAutoApproveAndDenyRules()
    {
        // Arrange
        var factory = new CustomToolFunctionFactory();
        
        var readTool = new ToolDefinition
        {
            Name = "read-tool",
            Tags = new List<string> { "read" },
            Bash = "echo \"Read-only tool\""
        };
        
        var writeTool = new ToolDefinition
        {
            Name = "write-tool",
            Tags = new List<string> { "write" },
            Bash = "echo \"Write tool\""
        };
        
        var runTool = new ToolDefinition
        {
            Name = "run-tool",
            Tags = new List<string> { "run" },
            Bash = "echo \"Run tool\""
        };
        
        factory.AddCustomTool(readTool);
        factory.AddCustomTool(writeTool);
        factory.AddCustomTool(runTool);
        
        var approveList = new List<string> { "read" };
        var denyList = new List<string> { "run" };
        
        // Act & Assert
        Assert.IsTrue(factory.ShouldAutoApproveTool("read-tool", approveList));
        Assert.IsFalse(factory.ShouldAutoApproveTool("run-tool", approveList));
        
        Assert.IsTrue(factory.ShouldAutoDenyTool("run-tool", denyList));
        Assert.IsFalse(factory.ShouldAutoDenyTool("read-tool", denyList));
    }
}