using System;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace mdx.Tests
{
    [TestClass]
    public class CommandTests
    {
        [TestMethod]
        public void TestProgramExists()
        {
            // Verify the compiled program exists
            string exePath = GetMdxExePath();
            Assert.IsTrue(File.Exists(exePath), $"MDX executable not found at {exePath}");
        }

        [TestMethod]
        [Ignore("This test requires mdx to be in PATH")]
        public async Task TestSimpleEchoCommand()
        {
            // Skip this test if we're running in CI and SKIP_EXTERNAL_TOOL_TESTS is set
            if (Environment.GetEnvironmentVariable("SKIP_EXTERNAL_TOOL_TESTS") == "true")
            {
                return;
            }

            var result = await RunProcess("mdx", "run --script \"echo Hello World\"");
            StringAssert.Contains(result, "Hello World");
        }

        private string GetMdxExePath()
        {
            // This returns the path to the mdx executable
            string projectDir = AppDomain.CurrentDomain.BaseDirectory;
            
            // Check if we're running in a CI environment
            if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true")
            {
                // In CI, we expect mdx to be in PATH
                return "mdx";
            }
            
            // For local development, try to find the binary
            string configuration = "Debug";
#if RELEASE
            configuration = "Release";
#endif

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                string windowsPath = Path.Combine(projectDir, "..", "..", "..", "..", "src", "bin", configuration, "net8.0", "mdx.dll");
                if (File.Exists(windowsPath))
                    return windowsPath;
                
                // Try publish folder
                windowsPath = Path.Combine(projectDir, "..", "..", "..", "..", "src", "bin", configuration, "net8.0", "win-x64", "publish", "mdx.exe");
                if (File.Exists(windowsPath))
                    return windowsPath;
            }
            else
            {
                string linuxPath = Path.Combine(projectDir, "..", "..", "..", "..", "src", "bin", configuration, "net8.0", "linux-x64", "publish", "mdx");
                if (File.Exists(linuxPath))
                    return linuxPath;
            }
            
            // Fallback - maybe it's in the PATH
            return "mdx";
        }

        private async Task<string> RunProcess(string fileName, string arguments)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            process.Start();
            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new Exception($"Process exited with code {process.ExitCode}. Error: {error}");
            }

            return output;
        }
    }
}