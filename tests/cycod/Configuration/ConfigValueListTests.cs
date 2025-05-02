using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class ConfigValueListTests
{
    [TestMethod]
    public void AsList_HandlesVariousListRepresentations()
    {
        // Test with List<object>
        var objList = new List<object> { "item1", "item2" };
        var configValueObjList = new ConfigValue(objList);
        var resultObjList = configValueObjList.AsList();
        CollectionAssert.AreEqual(new List<string> { "item1", "item2" }, resultObjList);

        // Test with List<string>
        var strList = new List<string> { "item1", "item2" };
        var configValueStrList = new ConfigValue(strList);
        var resultStrList = configValueStrList.AsList();
        CollectionAssert.AreEqual(new List<string> { "item1", "item2" }, resultStrList);

        // Test with object[]
        var objArray = new object[] { "item1", "item2" };
        var configValueObjArray = new ConfigValue(objArray);
        var resultObjArray = configValueObjArray.AsList();
        CollectionAssert.AreEqual(new List<string> { "item1", "item2" }, resultObjArray);

        // Test with string[]
        var strArray = new string[] { "item1", "item2" };
        var configValueStrArray = new ConfigValue(strArray);
        var resultStrArray = configValueStrArray.AsList();
        CollectionAssert.AreEqual(new List<string> { "item1", "item2" }, resultStrArray);

        // Test with single string
        var singleString = "singleItem";
        var configValueSingleString = new ConfigValue(singleString);
        var resultSingleString = configValueSingleString.AsList();
        CollectionAssert.AreEqual(new List<string> { "singleItem" }, resultSingleString);

        // Test with empty array string representation
        var emptyArrayString = "[]";
        var configValueEmptyArray = new ConfigValue(emptyArrayString);
        var resultEmptyArray = configValueEmptyArray.AsList();
        Assert.AreEqual(0, resultEmptyArray.Count);

        // Test with null
        var configValueNull = new ConfigValue(null);
        var resultNull = configValueNull.AsList();
        Assert.AreEqual(0, resultNull.Count);
    }

    [TestMethod]
    public void TestEmptyListSerialization()
    {
        // Setup
        var testDir = Path.Combine(Path.GetTempPath(), "ConfigValueListTests_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);
        var testFile = Path.Combine(testDir, "test-config.yaml");

        try
        {
            var configStore = ConfigStore.Instance;

            // Test adding to a list and then removing all items
            configStore.AddToList("TestList", "Item1", testFile);
            configStore.AddToList("TestList", "Item2", testFile);

            // Verify both items are in the list
            var listAfterAdd = configStore.GetFromFileName("TestList", testFile).AsList();
            Assert.AreEqual(2, listAfterAdd.Count);

            // Remove all items
            configStore.RemoveFromList("TestList", "Item1", testFile);
            configStore.RemoveFromList("TestList", "Item2", testFile);

            // Verify the list is now empty
            var listAfterRemove = configStore.GetFromFileName("TestList", testFile).AsList();
            Assert.AreEqual(0, listAfterRemove.Count);

            // Add a new item to verify we can add to a previously emptied list
            configStore.AddToList("TestList", "Item3", testFile);
            var listAfterReAdd = configStore.GetFromFileName("TestList", testFile).AsList();
            Assert.AreEqual(1, listAfterReAdd.Count);
            CollectionAssert.Contains(listAfterReAdd, "Item3");

            // Print file content for debugging
            string fileContent = File.ReadAllText(testFile);
            Console.WriteLine($"Final YAML content:\n{fileContent}");
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(testDir))
            {
                Directory.Delete(testDir, true);
            }
        }
    }
}