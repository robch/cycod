using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProcessExecution;
using ShellExecution.Results;
using System;
using System.Threading.Tasks;

namespace Tests.ProcessExecution
{
    [TestClass]
    public class NamedShellProcessManagerTests
    {
        [TestMethod]
        public void CreateShell_WithValidParameters_ReturnsSuccess()
        {
            // Arrange
            var manager = NamedShellProcessManager.Instance;
            var shellName = "test-shell-" + Guid.NewGuid().ToString("N")[..8];

            try
            {
                // Act
                var result = manager.CreateShell(PersistentShellType.Bash, shellName, ".");

                // Assert
                Assert.IsTrue(result.Success);
                Assert.AreEqual(shellName, result.ResourceName);
                Assert.AreEqual("Shell", result.ResourceType);
            }
            finally
            {
                // Cleanup
                manager.TerminateShell(shellName);
            }
        }

        [TestMethod]
        public void CreateShell_WithDuplicateName_ReturnsError()
        {
            // Arrange
            var manager = NamedShellProcessManager.Instance;
            var shellName = "duplicate-shell-" + Guid.NewGuid().ToString("N")[..8];

            try
            {
                // Create first shell
                var firstResult = manager.CreateShell(PersistentShellType.Bash, shellName, ".");
                Assert.IsTrue(firstResult.Success);

                // Act - try to create second shell with same name
                var result = manager.CreateShell(PersistentShellType.Bash, shellName, ".");

                // Assert
                Assert.IsFalse(result.Success);
                Assert.IsTrue(result.ErrorMessage.Contains("already exists"));
            }
            finally
            {
                // Cleanup
                manager.TerminateShell(shellName);
            }
        }

        [TestMethod]
        public async Task ExecuteInShellAsync_WithNonExistentShell_ReturnsError()
        {
            // Arrange
            var manager = NamedShellProcessManager.Instance;

            // Act
            var result = await manager.ExecuteInShellAsync("nonexistent-shell", "echo test", 5000);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.ErrorMessage.Contains("not found"));
        }

        [TestMethod]
        public void TerminateShell_WithNonExistentShell_ReturnsError()
        {
            // Arrange
            var manager = NamedShellProcessManager.Instance;

            // Act
            var result = manager.TerminateShell("nonexistent-shell");

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.ErrorMessage.Contains("not found"));
        }

        [TestMethod]
        public void GetAllShellNames_ReturnsListOfShells()
        {
            // Arrange
            var manager = NamedShellProcessManager.Instance;

            // Act
            var result = manager.GetAllShellNames();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(System.Collections.Generic.List<string>));
        }

        [TestMethod]
        public void GetShellState_WithNonExistentShell_ReturnsTerminated()
        {
            // Arrange
            var manager = NamedShellProcessManager.Instance;

            // Act
            var result = manager.GetShellState("nonexistent-shell");

            // Assert
            Assert.AreEqual(ShellState.Terminated, result);
        }

        [TestMethod]
        public void IsShellBusy_WithNonExistentShell_ReturnsFalse()
        {
            // Arrange
            var manager = NamedShellProcessManager.Instance;

            // Act
            var result = manager.IsShellBusy("nonexistent-shell", out var commandInfo);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(commandInfo);
        }
    }
}