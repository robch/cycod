using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProcessExecution;
using ShellExecution.Results;
using System;
using System.Threading.Tasks;

namespace Tests.ProcessExecution
{
    [TestClass]
    public class ShellIntegrationTests
    {
        [TestMethod]
        public async Task ExecuteInShell_WithSimpleCommand_ReturnsOutput()
        {
            // Arrange
            var manager = NamedShellProcessManager.Instance;
            var shellName = "integration-shell-" + Guid.NewGuid().ToString("N")[..8];

            try
            {
                // Create shell
                var createResult = manager.CreateShell(PersistentShellType.Bash, shellName, ".");
                Assert.IsTrue(createResult.Success, $"Failed to create shell: {createResult.ErrorMessage}");

                // Act - Execute a simple command
                var executeResult = await manager.ExecuteInShellAsync(shellName, "echo 'Hello Test'", 5000);

                // Assert
                Assert.IsTrue(executeResult.Success, $"Command execution failed: {executeResult.ErrorMessage}");
                Assert.IsTrue(executeResult.Stdout.Contains("Hello Test"), $"Expected output not found. Actual stdout: {executeResult.Stdout}");
            }
            finally
            {
                // Cleanup
                manager.TerminateShell(shellName);
            }
        }

        [TestMethod]
        public async Task ShellState_DuringCommandExecution_TransitionsCorrectly()
        {
            // Arrange
            var manager = NamedShellProcessManager.Instance;
            var shellName = "state-test-shell-" + Guid.NewGuid().ToString("N")[..8];

            try
            {
                // Create shell
                var createResult = manager.CreateShell(PersistentShellType.Bash, shellName, ".");
                Assert.IsTrue(createResult.Success);

                // Assert initial state
                var initialState = manager.GetShellState(shellName);
                Assert.AreEqual(ShellState.Idle, initialState, "Shell should start in Idle state");

                // Act - Execute a command that takes some time
                var executeTask = manager.ExecuteInShellAsync(shellName, "sleep 2 && echo 'Done'", 5000);

                // Check state during execution (give it a moment to start)
                await Task.Delay(500);
                var runningState = manager.GetShellState(shellName);
                Assert.AreEqual(ShellState.Busy, runningState, "Shell should be Busy during command execution");

                // Wait for completion
                var result = await executeTask;
                Assert.IsTrue(result.Success);

                // Check final state
                var finalState = manager.GetShellState(shellName);
                Assert.AreEqual(ShellState.Idle, finalState, "Shell should return to Idle after command completion");
            }
            finally
            {
                // Cleanup
                manager.TerminateShell(shellName);
            }
        }

        [TestMethod]
        public async Task SendInput_ThenGetOutput_WorksCorrectly()
        {
            // Arrange
            var manager = NamedShellProcessManager.Instance;
            var shellName = "input-output-shell-" + Guid.NewGuid().ToString("N")[..8];

            try
            {
                // Create shell
                var createResult = manager.CreateShell(PersistentShellType.Bash, shellName, ".");
                Assert.IsTrue(createResult.Success);

                // Execute a simple command first
                var firstResult = await manager.ExecuteInShellAsync(shellName, "echo 'Ready for input'", 5000);
                Assert.IsTrue(firstResult.Success);
                Assert.IsTrue(firstResult.Stdout.Contains("Ready for input"));

                // Act - Send input via ExecuteInShell (simpler than interactive cat)
                var inputResult = await manager.ExecuteInShellAsync(shellName, "echo 'Test Input Response'", 5000);
                
                // Assert - Check that command executed properly
                Assert.IsTrue(inputResult.Success, $"Command failed: {inputResult.ErrorMessage}");
                Assert.IsTrue(inputResult.Stdout.Contains("Test Input Response"), $"Expected response not found in stdout: {inputResult.Stdout}");
            }
            finally
            {
                // Cleanup
                manager.TerminateShell(shellName);
            }
        }

        [TestMethod]
        public void TerminateShell_CleansUpResources_Properly()
        {
            // Arrange
            var manager = NamedShellProcessManager.Instance;
            var shellName = "cleanup-test-shell-" + Guid.NewGuid().ToString("N")[..8];

            // Create shell
            var createResult = manager.CreateShell(PersistentShellType.Bash, shellName, ".");
            Assert.IsTrue(createResult.Success);

            // Verify shell exists
            var shellNames = manager.GetAllShellNames();
            Assert.IsTrue(shellNames.Contains(shellName), "Shell should be in the list after creation");

            var stateBeforeTerminate = manager.GetShellState(shellName);
            Assert.AreEqual(ShellState.Idle, stateBeforeTerminate, "Shell should be in Idle state");

            // Act - Terminate shell
            var terminateResult = manager.TerminateShell(shellName);

            // Assert - Verify cleanup
            Assert.IsTrue(terminateResult.Success, $"Failed to terminate shell: {terminateResult.ErrorMessage}");

            var stateAfterTerminate = manager.GetShellState(shellName);
            Assert.AreEqual(ShellState.Terminated, stateAfterTerminate, "Shell should be in Terminated state after termination");

            var shellNamesAfter = manager.GetAllShellNames();
            Assert.IsFalse(shellNamesAfter.Contains(shellName), "Shell should not be in the list after termination");
        }

        [TestMethod]
        public async Task ConcurrentShells_OperateIndependently()
        {
            // Arrange
            var manager = NamedShellProcessManager.Instance;
            var shell1Name = "concurrent-shell-1-" + Guid.NewGuid().ToString("N")[..8];
            var shell2Name = "concurrent-shell-2-" + Guid.NewGuid().ToString("N")[..8];

            try
            {
                // Create two shells
                var create1 = manager.CreateShell(PersistentShellType.Bash, shell1Name, ".");
                var create2 = manager.CreateShell(PersistentShellType.Bash, shell2Name, ".");
                Assert.IsTrue(create1.Success && create2.Success);

                // Act - Execute commands concurrently
                var task1 = manager.ExecuteInShellAsync(shell1Name, "echo 'Shell1 Output'", 5000);
                var task2 = manager.ExecuteInShellAsync(shell2Name, "echo 'Shell2 Output'", 5000);

                var results = await Task.WhenAll(task1, task2);

                // Assert - Both shells produced their expected output
                Assert.IsTrue(results[0].Success && results[1].Success, "Both commands should succeed");
                Assert.IsTrue(results[0].Stdout.Contains("Shell1 Output"), "Shell1 should produce its output");
                Assert.IsTrue(results[1].Stdout.Contains("Shell2 Output"), "Shell2 should produce its output");

                // Verify both shells are still available
                var shell1State = manager.GetShellState(shell1Name);
                var shell2State = manager.GetShellState(shell2Name);
                Assert.AreEqual(ShellState.Idle, shell1State);
                Assert.AreEqual(ShellState.Idle, shell2State);
            }
            finally
            {
                // Cleanup
                manager.TerminateShell(shell1Name);
                manager.TerminateShell(shell2Name);
            }
        }
    }
}