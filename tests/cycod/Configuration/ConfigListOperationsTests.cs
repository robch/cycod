using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class ConfigListOperationsTests
{
    private string? _testDir;
    private ConfigStore? _configStore;
    private string? _testConfigFile;

    [TestInitialize]
    public void Setup()
    {
        // Create a temporary directory for test configuration files
        _testDir = Path.Combine(Path.GetTempPath(), "CycoDConfigListTests_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDir);
        _testConfigFile = Path.Combine(_testDir, "test-config.yaml");

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
    public void AddAndRemoveFromList_SingleItem_Success()
    {
        // Act - Add an item
        _configStore!.AddToList("TestList", "Item1", _testConfigFile!);
        
        // Verify item was added
        var configValue = _configStore.GetFromFileName("TestList", _testConfigFile!);
        var list = configValue.AsList();
        Assert.AreEqual(1, list.Count);
        CollectionAssert.Contains(list, "Item1");
        
        // Act - Remove the item
        bool removed = _configStore.RemoveFromList("TestList", "Item1", _testConfigFile!);
        
        // Verify item was removed
        Assert.IsTrue(removed);
        configValue = _configStore.GetFromFileName("TestList", _testConfigFile!);
        list = configValue.AsList();
        Assert.AreEqual(0, list.Count);
    }

    [TestMethod]
    public void AddAndRemoveFromList_MultipleItems_Success()
    {
        // Act - Add multiple items
        _configStore!.AddToList("TestList", "Item1", _testConfigFile!);
        _configStore.AddToList("TestList", "Item2", _testConfigFile!);
        
        // Verify items were added
        var configValue = _configStore.GetFromFileName("TestList", _testConfigFile!);
        var list = configValue.AsList();
        Assert.AreEqual(2, list.Count);
        CollectionAssert.Contains(list, "Item1");
        CollectionAssert.Contains(list, "Item2");
        
        // Act - Remove one item
        bool removed = _configStore.RemoveFromList("TestList", "Item1", _testConfigFile!);
        
        // Verify item was removed but other remains
        Assert.IsTrue(removed);
        configValue = _configStore.GetFromFileName("TestList", _testConfigFile!);
        list = configValue.AsList();
        Assert.AreEqual(1, list.Count);
        CollectionAssert.DoesNotContain(list, "Item1");
        CollectionAssert.Contains(list, "Item2");
    }

    [TestMethod]
    public void AddToList_DuplicateItem_NotAdded()
    {
        // Act - Add the same item twice
        bool firstAdd = _configStore!.AddToList("TestList", "Item1", _testConfigFile!);
        bool secondAdd = _configStore.AddToList("TestList", "Item1", _testConfigFile!);
        
        // Verify item was added once
        Assert.IsTrue(firstAdd);
        Assert.IsFalse(secondAdd); // Should return false for duplicate
        
        var configValue = _configStore.GetFromFileName("TestList", _testConfigFile!);
        var list = configValue.AsList();
        Assert.AreEqual(1, list.Count);
        CollectionAssert.Contains(list, "Item1");
    }

    [TestMethod]
    public void RemoveFromList_NonExistentItem_ReturnsFalse()
    {
        // Add an item
        _configStore!.AddToList("TestList", "Item1", _testConfigFile!);
        
        // Act - Try to remove a non-existent item
        bool removed = _configStore.RemoveFromList("TestList", "Item2", _testConfigFile!);
        
        // Verify operation failed and list is unchanged
        Assert.IsFalse(removed);
        var configValue = _configStore.GetFromFileName("TestList", _testConfigFile!);
        var list = configValue.AsList();
        Assert.AreEqual(1, list.Count);
        CollectionAssert.Contains(list, "Item1");
    }

    // This test specifically targets the reported issue
    [TestMethod]
    public void AddRemoveMultiple_ReproduceReportedIssue()
    {
        // Act - Add items sequentially
        _configStore!.AddToList("TestList", "Item1", _testConfigFile!);
        _configStore.AddToList("TestList", "Item2", _testConfigFile!);
        
        // Verify before remove
        var configValue = _configStore.GetFromFileName("TestList", _testConfigFile!);
        Console.WriteLine($"List after adding: {string.Join(", ", configValue.AsList())}");
        
        // Dump the actual file content for debugging
        string fileContent = File.ReadAllText(_testConfigFile!);
        Console.WriteLine($"File content before remove:\n{fileContent}");
        
        // Try to remove items
        bool removed1 = _configStore.RemoveFromList("TestList", "Item1", _testConfigFile!);
        
        // Dump the file content after first remove
        fileContent = File.ReadAllText(_testConfigFile!);
        Console.WriteLine($"File content after first remove:\n{fileContent}");
        
        bool removed2 = _configStore.RemoveFromList("TestList", "Item2", _testConfigFile!);
        
        // Dump the file content after second remove
        fileContent = File.ReadAllText(_testConfigFile!);
        Console.WriteLine($"File content after second remove:\n{fileContent}");
        
        // Verify results
        Assert.IsTrue(removed1, "Should successfully remove Item1");
        Assert.IsTrue(removed2, "Should successfully remove Item2");
        
        configValue = _configStore.GetFromFileName("TestList", _testConfigFile!);
        var list = configValue.AsList();
        Assert.AreEqual(0, list.Count, "List should be empty after removing all items");
    }
}