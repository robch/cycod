using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChatX.Tests.Configuration
{
    [TestClass]
    public class ConfigValueTests
    {
        [TestMethod]
        public void IsNotFound_NotFoundSource_ReturnsTrue()
        {
            // Arrange
            var configValue = new ConfigValue();
            
            // Act & Assert
            Assert.IsTrue(configValue.IsNotFound());
            Assert.AreEqual(ConfigSource.NotFound, configValue.Source);
        }
        
        [TestMethod]
        public void IsNotFound_WithValue_ReturnsFalse()
        {
            // Arrange
            var configValue = new ConfigValue("test-value");
            
            // Act & Assert
            Assert.IsFalse(configValue.IsNotFound());
        }
        
        [TestMethod]
        public void IsNotFoundNullOrEmpty_NotFoundSource_ReturnsTrue()
        {
            // Arrange
            var configValue = new ConfigValue();
            
            // Act & Assert
            Assert.IsTrue(configValue.IsNotFoundNullOrEmpty());
        }
        
        [TestMethod]
        public void IsNotFoundNullOrEmpty_NullValue_ReturnsTrue()
        {
            // Arrange
            var configValue = new ConfigValue(null, ConfigSource.Default);
            
            // Act & Assert
            Assert.IsTrue(configValue.IsNotFoundNullOrEmpty());
        }
        
        [TestMethod]
        public void IsNotFoundNullOrEmpty_EmptyString_ReturnsTrue()
        {
            // Arrange
            var configValue = new ConfigValue("", ConfigSource.Default);
            
            // Act & Assert
            Assert.IsTrue(configValue.IsNotFoundNullOrEmpty());
        }
        
        [TestMethod]
        public void IsNotFoundNullOrEmpty_EmptyList_ReturnsTrue()
        {
            // Arrange
            var configValue = new ConfigValue(new List<string>(), ConfigSource.Default);
            
            // Act & Assert
            Assert.IsTrue(configValue.IsNotFoundNullOrEmpty());
        }
        
        [TestMethod]
        public void IsNotFoundNullOrEmpty_NonEmptyValue_ReturnsFalse()
        {
            // Arrange
            var configValue = new ConfigValue("test-value", ConfigSource.Default);
            
            // Act & Assert
            Assert.IsFalse(configValue.IsNotFoundNullOrEmpty());
        }
        
        [TestMethod]
        public void IsNotFoundNullOrEmpty_NonEmptyList_ReturnsFalse()
        {
            // Arrange
            var configValue = new ConfigValue(new List<string> { "item1" }, ConfigSource.Default);
            
            // Act & Assert
            Assert.IsFalse(configValue.IsNotFoundNullOrEmpty());
        }
        
        // We cannot directly test File property as it's internal setter only
        // This is covered by the ConfigStore tests that check File property indirectly
    }
}