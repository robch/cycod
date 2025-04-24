using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class ConfigurationSystemTests
{
    private string? _testDir;

    [TestInitialize]
    public void Setup()
    {
        // Create a temporary directory for test configuration files
        _testDir = Path.Combine(Path.GetTempPath(), "ChatXConfigTests_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDir);

        // Create a test environment
        Environment.SetEnvironmentVariable("TEST_ENV_VALUE", "env-value");
    }

    [TestCleanup]
    public void Cleanup()
    {
        // Remove test environment variables
        Environment.SetEnvironmentVariable("TEST_ENV_VALUE", null);

        // Clean up test directory
        if (Directory.Exists(_testDir))
        {
            Directory.Delete(_testDir, true);
        }
    }

    [TestMethod]
    public void KnownSettings_ToEnvironmentVariable_ReturnsCorrectEnvVar()
    {
        // Arrange
        var input = "GitHub.Token";
        var expected = "GITHUB_TOKEN";

        // Act
        var result = KnownSettings.ToEnvironmentVariable(input);

        // Assert
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void KnownSettings_ToCLIOption_ReturnsCorrectOption()
    {
        // Arrange
        var input = "GitHub.Token";
        var expected = "--github-token";

        // Act
        var result = KnownSettings.ToCLIOption(input);

        // Assert
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void KnownSettings_IsSecret_IdentifiesSecretSettings()
    {
        // Act & Assert
        Assert.IsTrue(KnownSettings.IsSecret("GitHub.Token"));
        Assert.IsTrue(KnownSettings.IsSecret("GITHUB_TOKEN"));
        Assert.IsTrue(KnownSettings.IsSecret("--github-token"));
        Assert.IsTrue(KnownSettings.IsSecret("OpenAI.ApiKey"));
        
        Assert.IsFalse(KnownSettings.IsSecret("OpenAI.ChatModelName"));
    }

    [TestMethod]
    public void ConfigValue_AsObfuscated_ObfuscatesSecretValues()
    {
        // Arrange
        var secretValue = new ConfigValue("secret123456", ConfigSource.CommandLine, true);
        var normalValue = new ConfigValue("normal123456", ConfigSource.CommandLine, false);

        // Act
        var obfuscatedSecret = secretValue.AsObfuscated();
        var obfuscatedNormal = normalValue.AsObfuscated();

        // Assert
        Assert.AreNotEqual("secret123456", obfuscatedSecret);
        Assert.IsTrue(obfuscatedSecret!.Contains("******"));
        Assert.AreEqual("normal123456", obfuscatedNormal);
    }

    [TestMethod]
    public void KnownSettingsCLIParser_TryParseCLIOption_ParsesCorrectly()
    {
        // Arrange
        var arg1 = "--github-token";
        // string arg2 = "mytoken";
        var arg3 = "--openai-api-key=secretkey";
        
        // Act
        var result1 = KnownSettingsCLIParser.TryParseCLIOption(arg1, out string? settingName1, out string? value1);
        var result2 = KnownSettingsCLIParser.TryParseCLIOption(arg3, out string? settingName2, out string? value2);
        
        // Assert
        Assert.IsTrue(result1);
        Assert.AreEqual("GitHub.Token", settingName1);
        Assert.IsNull(value1);
        
        Assert.IsTrue(result2);
        Assert.AreEqual("OpenAI.ApiKey", settingName2);
        Assert.AreEqual("secretkey", value2);
    }
}