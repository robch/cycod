
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

[TestClass]
public class TemplateVariablesTests
{
    private TemplateVariables? _variables;

    [TestInitialize]
    public void Setup()
    {
        _variables = new TemplateVariables();
    }

    #region Constructor Tests

    [TestMethod]
    public void Constructor_EmptyDictionary_CreatesInstance()
    {
        // Arrange & Act
        var variables = new TemplateVariables();
        
        // Assert
        Assert.IsNotNull(variables);
    }

    [TestMethod]
    public void Constructor_WithDictionary_CreatesInstanceWithValues()
    {
        // Arrange
        var initialValues = new Dictionary<string, string>
        {
            { "key1", "value1" },
            { "key2", "value2" }
        };
        
        // Act
        var variables = new TemplateVariables(initialValues);
        
        // Assert
        Assert.AreEqual("value1", variables.Get("key1"));
        Assert.AreEqual("value2", variables.Get("key2"));
    }

    [TestMethod]
    public void Constructor_WithDictionary_IsCaseInsensitive()
    {
        // Arrange
        var initialValues = new Dictionary<string, string>
        {
            { "Key", "value" }
        };
        
        // Act
        var variables = new TemplateVariables(initialValues);
        
        // Assert
        Assert.AreEqual("value", variables.Get("key"));
        Assert.AreEqual("value", variables.Get("KEY"));
    }

    #endregion

    #region Get Tests

    [TestMethod]
    public void Get_ExistingVariable_ReturnsValue()
    {
        // Arrange
        _variables!.Set("testKey", "testValue");
        
        // Act
        var result = _variables!.Get("testKey");
        
        // Assert
        Assert.AreEqual("testValue", result);
    }

    [TestMethod]
    public void Get_CaseInsensitiveKey_ReturnsValue()
    {
        // Arrange
        _variables!.Set("TestKey", "testValue");
        
        // Act
        var result = _variables!.Get("testkey");
        
        // Assert
        Assert.AreEqual("testValue", result);
    }

    [TestMethod]
    public void Get_NonExistentKey_ReturnsNull()
    {
        // Arrange & Act
        var result = _variables!.Get("nonExistentKey");
        
        // Assert
        Assert.IsNull(result);
    }

    #endregion

    #region Set Tests

    [TestMethod]
    public void Set_NewKey_AddsKeyValuePair()
    {
        // Arrange & Act
        _variables!.Set("newKey", "newValue");
        
        // Assert
        Assert.AreEqual("newValue", _variables!.Get("newKey"));
    }

    [TestMethod]
    public void Set_ExistingKey_UpdatesValue()
    {
        // Arrange
        _variables!.Set("existingKey", "oldValue");
        
        // Act
        _variables!.Set("existingKey", "newValue");
        
        // Assert
        Assert.AreEqual("newValue", _variables!.Get("existingKey"));
    }

    [TestMethod]
    public void Set_CaseInsensitive_UpdatesCorrectly()
    {
        // Arrange
        _variables!.Set("Key", "value1");
        
        // Act
        _variables!.Set("KEY", "value2");
        
        // Assert
        Assert.AreEqual("value2", _variables!.Get("key"));
    }

    #endregion

    #region Contains Tests

    [TestMethod]
    public void Contains_ExistingKey_ReturnsTrue()
    {
        // Arrange
        _variables!.Set("key", "value");
        
        // Act & Assert
        Assert.IsTrue(_variables!.Contains("key"));
    }

    [TestMethod]
    public void Contains_NonExistentKey_ReturnsFalse()
    {
        // Arrange & Act & Assert
        Assert.IsFalse(_variables!.Contains("THIS_VARIABLE_SHOULD_NOT_EXIST_ANYWHERE"));
    }

    [TestMethod]
    public void Contains_CaseInsensitive_ReturnsTrue()
    {
        // Arrange
        _variables!.Set("myKey", "value");
        
        // Act & Assert
        Assert.IsTrue(_variables!.Contains("MYKEY"));
    }

    [TestMethod]
    public void Contains_SpecialVariables_ReturnsTrue()
    {
        // Act & Assert
        Assert.IsTrue(_variables!.Contains("os"));
        Assert.IsTrue(_variables!.Contains("date"));
        Assert.IsTrue(_variables!.Contains("year"));
    }

    #endregion
}