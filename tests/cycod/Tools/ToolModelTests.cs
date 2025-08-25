using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

[TestClass]
public class ToolModelTests
{
    [TestMethod]
    public void ToolDefinition_BasicPropertiesSetCorrectly()
    {
        // Arrange & Act
        var tool = new ToolDefinition
        {
            Name = "test-tool",
            Description = "A test tool",
            Bash = "echo 'Hello'",
            Timeout = 5000,
            WorkingDirectory = "/tmp",
            Tags = new List<string> { "read", "test" },
            Parameters = new Dictionary<string, ToolParameter>
            {
                ["param1"] = new ToolParameter { Description = "Parameter 1", Type = "string", Default = "default" }
            }
        };

        // Assert
        Assert.AreEqual("test-tool", tool.Name);
        Assert.AreEqual("A test tool", tool.Description);
        Assert.AreEqual("echo 'Hello'", tool.Bash);
        Assert.AreEqual(5000, tool.Timeout);
        Assert.AreEqual("/tmp", tool.WorkingDirectory);
        CollectionAssert.AreEqual(new[] { "read", "test" }, tool.Tags.ToArray());
        Assert.AreEqual(1, tool.Parameters.Count);
        Assert.AreEqual("Parameter 1", tool.Parameters["param1"].Description);
    }

    [TestMethod]
    public void ToolDefinition_GetEffectiveCommandType_ReturnsBashWhenSet()
    {
        // Arrange
        var tool = new ToolDefinition
        {
            Bash = "echo 'Hello'"
        };

        // Act
        var commandType = tool.GetEffectiveCommandType();

        // Assert
        Assert.AreEqual("bash", commandType);
    }

    [TestMethod]
    public void ToolDefinition_GetEffectiveCommandType_ReturnsCmdWhenSet()
    {
        // Arrange
        var tool = new ToolDefinition
        {
            Cmd = "echo Hello"
        };

        // Act
        var commandType = tool.GetEffectiveCommandType();

        // Assert
        Assert.AreEqual("cmd", commandType);
    }

    [TestMethod]
    public void ToolDefinition_GetEffectiveCommandType_ReturnsPwshWhenSet()
    {
        // Arrange
        var tool = new ToolDefinition
        {
            Pwsh = "Write-Host 'Hello'"
        };

        // Act
        var commandType = tool.GetEffectiveCommandType();

        // Assert
        Assert.AreEqual("pwsh", commandType);
    }

    [TestMethod]
    public void ToolDefinition_GetEffectiveCommandType_ReturnsPythonWhenSet()
    {
        // Arrange
        var tool = new ToolDefinition
        {
            Python = "print('Hello')"
        };

        // Act
        var commandType = tool.GetEffectiveCommandType();

        // Assert
        Assert.AreEqual("python", commandType);
    }

    [TestMethod]
    public void ToolDefinition_GetEffectiveCommandType_ReturnsRunWhenSet()
    {
        // Arrange
        var tool = new ToolDefinition
        {
            Run = "some-command"
        };

        // Act
        var commandType = tool.GetEffectiveCommandType();

        // Assert
        Assert.AreEqual("run", commandType);
    }

    [TestMethod]
    public void ToolDefinition_GetEffectiveCommandType_ReturnsScriptWhenScriptAndShellSet()
    {
        // Arrange
        var tool = new ToolDefinition
        {
            Script = "echo 'Hello'",
            Shell = "bash"
        };

        // Act
        var commandType = tool.GetEffectiveCommandType();

        // Assert
        Assert.AreEqual("script", commandType);
    }

    [TestMethod]
    public void ToolDefinition_GetEffectiveCommandContent_ReturnsCorrectContentForCommandType()
    {
        // Arrange - Create a tool with just one command type at a time
        var tool = new ToolDefinition
        {
            Run = "run-command"
        };

        // Act & Assert - Test the run command
        Assert.AreEqual("run-command", tool.GetEffectiveCommandContent());
        
        // Create a tool with just bash command
        tool = new ToolDefinition
        {
            Bash = "echo 'Bash'"
        };
        Assert.AreEqual("echo 'Bash'", tool.GetEffectiveCommandContent());
        
        // Create a tool with just cmd command
        tool = new ToolDefinition
        {
            Cmd = "echo Cmd"
        };
        Assert.AreEqual("echo Cmd", tool.GetEffectiveCommandContent());
        
        // Create a tool with just pwsh command
        tool = new ToolDefinition
        {
            Pwsh = "Write-Host 'Pwsh'"
        };
        Assert.AreEqual("Write-Host 'Pwsh'", tool.GetEffectiveCommandContent());
        
        // Create a tool with just python command
        tool = new ToolDefinition
        {
            Python = "print('Python')"
        };
        Assert.AreEqual("print('Python')", tool.GetEffectiveCommandContent());
        
        // Create a tool with just script command
        tool = new ToolDefinition
        {
            Script = "echo 'Script'",
            Shell = "custom-shell"
        };
        Assert.AreEqual("echo 'Script'", tool.GetEffectiveCommandContent());
    }

    [TestMethod]
    public void ToolStep_BasicPropertiesSetCorrectly()
    {
        // Arrange & Act
        var step = new ToolStep
        {
            Name = "step1",
            Bash = "echo 'Step 1'",
            Timeout = 3000,
            WorkingDirectory = "/home/user",
            Environment = new Dictionary<string, string>
            {
                ["ENV_VAR"] = "value"
            }
        };

        // Assert
        Assert.AreEqual("step1", step.Name);
        Assert.AreEqual("echo 'Step 1'", step.Bash);
        Assert.AreEqual(3000, step.Timeout);
        Assert.AreEqual("/home/user", step.WorkingDirectory);
        Assert.AreEqual(1, step.Environment.Count);
        Assert.AreEqual("value", step.Environment["ENV_VAR"]);
    }

    [TestMethod]
    public void ToolStep_GetEffectiveCommandType_ReturnsCommandTypeFromStep()
    {
        // Arrange
        var step = new ToolStep
        {
            Bash = "echo 'Step'"
        };

        // Act
        var commandType = step.GetEffectiveCommandType();

        // Assert
        Assert.AreEqual("bash", commandType);
    }

    [TestMethod]
    public void ToolStep_GetEffectiveCommandContent_ReturnsContentFromStep()
    {
        // Arrange
        var step = new ToolStep
        {
            Bash = "echo 'Step'"
        };

        // Act
        var content = step.GetEffectiveCommandContent();

        // Assert
        Assert.AreEqual("echo 'Step'", content);
    }

    [TestMethod]
    public void ToolParameter_BasicPropertiesSetCorrectly()
    {
        // Arrange & Act
        var parameter = new ToolParameter
        {
            Description = "A parameter",
            Type = "string",
            Default = "default value",
            Required = true
        };

        // Assert
        Assert.AreEqual("A parameter", parameter.Description);
        Assert.AreEqual("string", parameter.Type);
        Assert.AreEqual("default value", parameter.Default);
        Assert.IsTrue(parameter.Required);
    }

    [TestMethod]
    public void ToolDefinition_SerializesToYaml()
    {
        // Arrange
        var tool = new ToolDefinition
        {
            Name = "yaml-test",
            Description = "YAML serialization test",
            Bash = "echo 'Hello from YAML'",
            Tags = new List<string> { "read", "test" },
            Parameters = new Dictionary<string, ToolParameter>
            {
                ["param1"] = new ToolParameter 
                { 
                    Description = "Parameter 1", 
                    Type = "string", 
                    Default = "default" 
                }
            }
        };

        // Act
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var yaml = serializer.Serialize(tool);

        // Assert
        StringAssert.Contains(yaml, "name: yaml-test");
        StringAssert.Contains(yaml, "description: YAML serialization test");
        StringAssert.Contains(yaml, "bash: echo 'Hello from YAML'");
        StringAssert.Contains(yaml, "- read");
        StringAssert.Contains(yaml, "- test");
        StringAssert.Contains(yaml, "param1:");
        StringAssert.Contains(yaml, "description: Parameter 1");
        StringAssert.Contains(yaml, "type: string");
        StringAssert.Contains(yaml, "default: default");
    }

    [TestMethod]
    public void ToolDefinition_DeserializesFromYaml()
    {
        // Arrange
        var yaml = @"
name: yaml-test
description: YAML serialization test
bash: echo 'Hello from YAML'
tags:
  - read
  - test
parameters:
  param1:
    description: Parameter 1
    type: string
    default: default
";

        // Act
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var tool = deserializer.Deserialize<ToolDefinition>(yaml);

        // Assert
        Assert.AreEqual("yaml-test", tool.Name);
        Assert.AreEqual("YAML serialization test", tool.Description);
        Assert.AreEqual("echo 'Hello from YAML'", tool.Bash);
        CollectionAssert.AreEqual(new[] { "read", "test" }, tool.Tags.ToArray());
        Assert.AreEqual(1, tool.Parameters.Count);
        Assert.AreEqual("Parameter 1", tool.Parameters["param1"].Description);
        Assert.AreEqual("string", tool.Parameters["param1"].Type);
        Assert.AreEqual("default", tool.Parameters["param1"].Default);
    }
}