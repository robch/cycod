using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChatX.Tests.Configuration
{
    [TestClass]
    public class ConfigStoreFileNameTests
    {
        private string? _testDir;
        private string? _testConfigFile;
        private ConfigStore? _configStore;

        [TestInitialize]
        public void Setup()
        {
            // Create a temporary directory for test configuration files
            _testDir = Path.Combine(Path.GetTempPath(), "ChatXConfigTests_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDir);
            
            // Create a specific test config file
            _testConfigFile = Path.Combine(_testDir, "test-config.yml");
            
            // Initialize the config store
            _configStore = ConfigStore.Instance;
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Clean up test directory
            if (Directory.Exists(_testDir))
            {
                Directory.Delete(_testDir, true);
            }
        }

        [TestMethod]
        public void SetAndGetValue_SpecificFile_Success()
        {
            // Act
            _configStore!.Set("OpenAI.ChatModelName", "gpt-4o", _testConfigFile!);
            var value = _configStore.GetFromFileName("OpenAI.ChatModelName", _testConfigFile!);

            // Assert
            Assert.AreEqual("gpt-4o", value.AsString());
            Assert.AreEqual(_testConfigFile, value.File?.FileName);
        }

        [TestMethod]
        public void FileValue_AndScopeValue_FileValueReturned()
        {
            // Arrange
            _configStore!.Set("OpenAI.ApiKey", "file-key", _testConfigFile!);
            _configStore.Set("OpenAI.ApiKey", "local-key", ConfigFileScope.Local);

            // Act - Get from specific sources
            var fileValue = _configStore.GetFromFileName("OpenAI.ApiKey", _testConfigFile!);
            var localValue = _configStore.GetFromScope("OpenAI.ApiKey", ConfigFileScope.Local);
            
            // Assert
            Assert.AreEqual("file-key", fileValue.AsString());
            Assert.AreEqual("local-key", localValue.AsString());
            Assert.AreEqual(_testConfigFile, fileValue.File?.FileName);
        }

        [TestMethod]
        public void AddToList_SpecificFile_Success()
        {
            // Act
            _configStore!.AddToList("AllowedTools", "Tool1", _testConfigFile!);
            _configStore.AddToList("AllowedTools", "Tool2", _testConfigFile!);
            
            var value = _configStore.GetFromFileName("AllowedTools", _testConfigFile!);
            var list = value.AsList();

            // Assert
            CollectionAssert.Contains(list, "Tool1");
            CollectionAssert.Contains(list, "Tool2");
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(_testConfigFile, value.File?.FileName);
        }
        
        [TestMethod]
        public void RemoveFromList_SpecificFile_Success()
        {
            // Arrange
            _configStore!.AddToList("AllowedTools", "Tool1", _testConfigFile!);
            _configStore.AddToList("AllowedTools", "Tool2", _testConfigFile!);
            
            // Act
            _configStore.RemoveFromList("AllowedTools", "Tool1", _testConfigFile!);
            var value = _configStore.GetFromFileName("AllowedTools", _testConfigFile!);
            var list = value.AsList();

            // Assert
            CollectionAssert.DoesNotContain(list, "Tool1");
            CollectionAssert.Contains(list, "Tool2");
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(_testConfigFile, value.File?.FileName);
        }
        
        [TestMethod]
        public void Clear_SpecificFile_Success()
        {
            // Arrange
            _configStore!.Set("OpenAI.ChatModelName", "gpt-4o", _testConfigFile!);
            
            // Act
            bool result = _configStore.Clear("OpenAI.ChatModelName", _testConfigFile!);
            var value = _configStore.GetFromFileName("OpenAI.ChatModelName", _testConfigFile!);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(value.IsNotFoundNullOrEmpty());
        }
    }
}