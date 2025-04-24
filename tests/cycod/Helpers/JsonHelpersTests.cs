using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

[TestClass]
public class JsonHelpersTests
{
    #region GetJsonPropertyValue Tests

    [TestMethod]
    public void GetJsonPropertyValue_ValidJsonAndProperty_ReturnsValue()
    {
        // Arrange
        var json = "{\"name\":\"John\",\"age\":30}";
        
        // Act
        var result = JsonHelpers.GetJsonPropertyValue(json, "name");
        
        // Assert
        Assert.AreEqual("John", result);
    }

    [TestMethod]
    public void GetJsonPropertyValue_ValidJsonMissingProperty_ReturnsDefault()
    {
        // Arrange
        var json = "{\"name\":\"John\",\"age\":30}";
        
        // Act
        var result = JsonHelpers.GetJsonPropertyValue(json, "email", "default@example.com");
        
        // Assert
        Assert.AreEqual("default@example.com", result);
    }

    [TestMethod]
    public void GetJsonPropertyValue_EmptyJson_ReturnsDefault()
    {
        // Arrange
        var json = "";
        
        // Act
        var result = JsonHelpers.GetJsonPropertyValue(json, "name", "defaultName");
        
        // Assert
        Assert.AreEqual("defaultName", result);
    }

    [TestMethod]
    public void GetJsonPropertyValue_NullJson_ReturnsDefault()
    {
        // Arrange
        string? json = null;
        
        // Act
        var result = JsonHelpers.GetJsonPropertyValue(json!, "name", "defaultName");
        
        // Assert
        Assert.AreEqual("defaultName", result);
    }

    [TestMethod]
    public void GetJsonPropertyValue_InvalidJson_ReturnsDefault()
    {
        // Arrange
        var json = "{invalid json";
        
        // Act
        var result = JsonHelpers.GetJsonPropertyValue(json, "name", "defaultName");
        
        // Assert
        Assert.AreEqual("defaultName", result);
    }

    [TestMethod]
    public void GetJsonPropertyValue_JsonArray_ReturnsDefault()
    {
        // Arrange
        var json = "[1, 2, 3]";
        
        // Act
        var result = JsonHelpers.GetJsonPropertyValue(json, "name", "defaultName");
        
        // Assert
        Assert.AreEqual("defaultName", result);
    }

    [TestMethod]
    public void GetJsonPropertyValue_NonStringProperty_ReturnsDefault()
    {
        // Arrange
        var json = "{\"name\":\"John\",\"age\":30}";
        
        // Act
        var result = JsonHelpers.GetJsonPropertyValue(json, "age", "defaultAge");
        
        // Assert
        Assert.AreEqual("defaultAge", result);
    }

    [TestMethod]
    public void GetJsonPropertyValue_NestedProperty_ReturnsDefault()
    {
        // Arrange
        var json = "{\"name\":\"John\",\"address\":{\"city\":\"New York\"}}";
        
        // Act
        var result = JsonHelpers.GetJsonPropertyValue(json, "address.city", "defaultCity");
        
        // Assert
        // This implementation doesn't support nested properties
        Assert.AreEqual("defaultCity", result);
    }

    [TestMethod]
    public void GetJsonPropertyValue_NoDefaultProvided_ReturnsNull()
    {
        // Arrange
        var json = "{\"name\":\"John\"}";
        
        // Act
        var result = JsonHelpers.GetJsonPropertyValue(json, "age");
        
        // Assert
        Assert.IsNull(result);
    }

    #endregion
}