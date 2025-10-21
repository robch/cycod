using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProcessExecution;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tests.ProcessExecution
{
    [TestClass]
    public class NamedProcessDispatcherTests
    {
        [TestMethod]
        public async Task GetOutputAsync_WithEmptyName_ReturnsError()
        {
            // Act
            var result = await NamedProcessDispatcher.GetOutputAsync("");

            // Assert
            Assert.AreEqual("Error: Process name cannot be empty", result);
        }

        [TestMethod]
        public async Task GetOutputAsync_WithNullName_ReturnsError()
        {
            // Act
            var result = await NamedProcessDispatcher.GetOutputAsync(null!);

            // Assert
            Assert.AreEqual("Error: Process name cannot be empty", result);
        }

        [TestMethod]
        public async Task GetOutputAsync_WithNonExistentProcess_ReturnsNotFoundError()
        {
            // Act
            var result = await NamedProcessDispatcher.GetOutputAsync("nonexistent-process");

            // Assert
            Assert.AreEqual("Error: No shell or background process found with name 'nonexistent-process'", result);
        }

        [TestMethod]
        public async Task SendInputAsync_WithEmptyName_ReturnsError()
        {
            // Act
            var result = await NamedProcessDispatcher.SendInputAsync("", "test input");

            // Assert
            Assert.AreEqual("Error: Process name cannot be empty", result);
        }

        [TestMethod]
        public async Task SendInputAsync_WithNonExistentProcess_ReturnsNotFoundError()
        {
            // Act
            var result = await NamedProcessDispatcher.SendInputAsync("nonexistent-process", "test input");

            // Assert
            Assert.AreEqual("Error: No shell or background process found with name 'nonexistent-process'", result);
        }

        [TestMethod]
        public void TerminateProcess_WithEmptyName_ReturnsError()
        {
            // Act
            var result = NamedProcessDispatcher.TerminateProcess("");

            // Assert
            Assert.AreEqual("Error: Process name cannot be empty", result);
        }

        [TestMethod]
        public void TerminateProcess_WithNonExistentProcess_ReturnsNotFoundError()
        {
            // Act
            var result = NamedProcessDispatcher.TerminateProcess("nonexistent-process");

            // Assert
            Assert.AreEqual("Error: No shell or background process found with name 'nonexistent-process'", result);
        }
    }
}