using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChatX.Tests.Configuration
{
    [TestClass]
    public class ConfigStoreTests
    {
        private string? _testDir;
        private ConfigStore? _configStore;

        [TestInitialize]
        public void Setup()
        {
            // Create a temporary directory for test configuration files
            _testDir = Path.Combine(Path.GetTempPath(), "ChatXConfigTests_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDir);

            // Create a test environment
            Environment.SetEnvironmentVariable("CHATX_TEST_DIR", _testDir);

            // Initialize the config store
            _configStore = ConfigStore.Instance;
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Remove test environment variables
            Environment.SetEnvironmentVariable("CHATX_TEST_DIR", null);
            Environment.SetEnvironmentVariable("AZURE_OPENAI_API_KEY", null);

            // Clean up test directory
            if (Directory.Exists(_testDir))
            {
                Directory.Delete(_testDir, true);
            }
        }

        [TestMethod]
        public void SetAndGetValue_ProjectScope_Success()
        {
            // Act
            _configStore!.Set("OpenAI.ChatModelName", "gpt-4o", ConfigFileScope.Local);
            var value = _configStore.GetFromAnyScope("OpenAI.ChatModelName");

            // Assert
            Assert.AreEqual("gpt-4o", value.AsString());
        }

        [TestMethod]
        public void SetAndGetValue_DifferentScopes_ReturnsHighestPriorityValue()
        {
            // Arrange
            _configStore!.Set("OpenAI.ApiKey", "global-key", ConfigFileScope.Global);
            _configStore.Set("OpenAI.ApiKey", "user-key", ConfigFileScope.User);
            _configStore.Set("OpenAI.ApiKey", "project-key", ConfigFileScope.Local);

            // Act - Get from specific scopes
            var globalValue = _configStore.GetFromScope("OpenAI.ApiKey", ConfigFileScope.Global);
            var userValue = _configStore.GetFromScope("OpenAI.ApiKey", ConfigFileScope.User);
            var projectValue = _configStore.GetFromScope("OpenAI.ApiKey", ConfigFileScope.Local);
            
            // Act - Get automatically (should return highest priority)
            var highestPriorityValue = _configStore.GetFromAnyScope("OpenAI.ApiKey");

            // Assert
            Assert.AreEqual("global-key", globalValue.AsString());
            Assert.AreEqual("user-key", userValue.AsString());
            Assert.AreEqual("project-key", projectValue.AsString());
            Assert.AreEqual("project-key", highestPriorityValue.AsString());
        }

        [TestMethod]
        public void GetValue_EnvironmentVarOverride_ReturnsEnvVar()
        {
            // Arrange
            _configStore!.Set("Azure.OpenAI.ApiKey", "config-value", ConfigFileScope.User);
            
            // Make sure the environment variable is set
            Environment.SetEnvironmentVariable("AZURE_OPENAI_API_KEY", "env-value");
            Console.WriteLine($"Environment variable AZURE_OPENAI_API_KEY value: {Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")}");
            
            // Test direct environment variable conversion
            string normalizedKey = ConfigPathHelpers.Normalize("Azure.OpenAI.ApiKey");
            string envVarKey = ConfigPathHelpers.ToEnvVar(normalizedKey);
            Console.WriteLine($"Normalized key: {normalizedKey}");
            Console.WriteLine($"Environment variable key: {envVarKey}");
            
            // Act
            var value = _configStore.GetFromAnyScope("Azure.OpenAI.ApiKey");
            Console.WriteLine($"Value from ConfigStore.Get(): {value.AsString()}");

            // Assert
            Assert.AreEqual("env-value", value.AsString());
        }
        
        [TestMethod]
        public void AddToList_Success()
        {
            // Act
            _configStore!.AddToList("AllowedTools", "Tool1", ConfigFileScope.User);
            _configStore.AddToList("AllowedTools", "Tool2", ConfigFileScope.User);
            
            var value = _configStore.GetFromAnyScope("AllowedTools");
            var list = value.AsList();

            // Assert
            CollectionAssert.Contains(list, "Tool1");
            CollectionAssert.Contains(list, "Tool2");
            Assert.AreEqual(2, list.Count);
        }
        
        [TestMethod]
        public void RemoveFromList_Success()
        {
            // Arrange
            _configStore!.AddToList("AllowedTools", "Tool1", ConfigFileScope.User);
            _configStore.AddToList("AllowedTools", "Tool2", ConfigFileScope.User);
            
            // Act
            _configStore.RemoveFromList("AllowedTools", "Tool1", ConfigFileScope.User);
            var value = _configStore.GetFromAnyScope("AllowedTools");
            var list = value.AsList();

            // Assert
            CollectionAssert.DoesNotContain(list, "Tool1");
            CollectionAssert.Contains(list, "Tool2");
            Assert.AreEqual(1, list.Count);
        }
        
        [TestMethod]
        public void Clear_Success()
        {
            // Arrange
            _configStore!.Set("OpenAI.ChatModelName", "gpt-4o", ConfigFileScope.User);
            
            // Act
            bool result = _configStore.Clear("OpenAI.ChatModelName", ConfigFileScope.User);
            var value = _configStore.GetFromScope("OpenAI.ChatModelName", ConfigFileScope.User);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(value.IsNullOrEmpty());
        }
    }
}