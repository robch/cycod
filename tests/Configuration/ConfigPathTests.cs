using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChatX.Tests.Configuration
{
    [TestClass]
    public class ConfigPathTests
    {
        [TestMethod]
        public void ToEnvVar_WithDotNotation_ReturnsCorrectEnvVar()
        {
            // Arrange
            string input = "Azure.OpenAI.ApiKey";
            string expected = "AZURE_OPENAI_API_KEY";

            // Act
            string result = ConfigPathHelpers.ToEnvVar(input);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void FromEnvVar_WithEnvVarFormat_ReturnsDotNotation()
        {
            // Arrange
            string input = "AZURE_OPENAI_API_KEY";
            string expected = "Azure.OpenAI.ApiKey";

            // Act
            string result = ConfigPathHelpers.FromEnvVar(input);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Normalize_WithMixedCase_ReturnsNormalizedPath()
        {
            // Arrange
            string input = "azure.openAI.apiKey";
            string expected = "Azure.OpenAI.ApiKey";

            // Act
            string result = ConfigPathHelpers.Normalize(input);

            // Assert
            Assert.AreEqual(expected, result);
        }
    }
}