# CI/CD Workflows for ChatX

This document provides an overview of the CI/CD workflows configured for the ChatX project.

## Available Workflows

### 1. CI Workflow

**File**: `.github/workflows/ci.yml`

**Triggers**:
- Pull requests to the `main` branch
- Direct pushes to the `main` branch
- Scheduled nightly builds (midnight UTC)
- Manual triggering via GitHub UI

**Purpose**:
- Validate that the codebase can be built successfully
- Run tests to ensure code quality
- Provide early feedback on pull requests
- Verify cross-platform builds (Windows, Linux, macOS)

**Steps**:
1. Check out code
2. Set up .NET 8.0
3. Restore dependencies
4. Build in Release configuration
5. Publish for multiple platforms (Windows, Linux, macOS)
6. Run tests and generate test reports
7. Upload test results as artifacts
8. Publish test results to GitHub UI
9. Upload build artifacts for inspection (including platform-specific builds)

### 2. Release Workflow

**File**: `.github/workflows/release.yml`

**Triggers**:
- Tags matching the pattern `X.Y.Z-YYYYMMDD` (e.g., `1.0.0-20250330`)
- Manual triggering via GitHub UI (with version parameter)

**Purpose**:
- Build and test the application
- Create platform-specific builds (Windows, Linux, macOS)
- Create and package a release version
- Generate a NuGet package with cross-platform support
- Optionally publish to NuGet.org

**Steps**:
1. Check out code
2. Set up .NET 8.0
3. Determine version from tag or input
4. Update version in project file
5. Build in Release configuration
6. Publish specific builds for Windows, Linux, and macOS
7. Run tests and generate test reports
8. Upload and publish test results
9. Create NuGet package (including all platform runtimes)
10. Generate package checksums
11. Upload NuGet package as an artifact
12. Publish to NuGet.org (if API key is configured)

## Cross-Platform Support

ChatX is built with cross-platform compatibility in mind and includes runtime components for:
- Windows (win-x64)
- Linux (linux-x64)
- macOS (osx-x64)

This means the NuGet package contains the necessary native dependencies for all supported platforms, ensuring a consistent experience regardless of the operating system.

## Usage

### Creating a Nightly Build

The nightly build runs automatically at midnight UTC. To manually trigger a build:

1. Go to the "Actions" tab in your GitHub repository
2. Select the "CI" workflow
3. Click "Run workflow"
4. Select the branch to build
5. Click "Run workflow"

### Viewing Test Results

Test results are captured and made visible in multiple ways:

1. **GitHub UI Test Summary**:
   - Each workflow run shows a test summary in the GitHub UI
   - Failed tests are highlighted with details about the failure
   - Access this by clicking on a workflow run and looking at the "Checks" tab

2. **Test Artifacts**:
   - Detailed test results are uploaded as artifacts
   - Download these for local analysis
   - Access by going to a workflow run and clicking on the "Artifacts" section

3. **Build Log**:
   - Complete test output is available in the build log
   - Useful for seeing console output from tests
   - Access by clicking on the "Test" step in any workflow run

If tests fail, the workflow will still complete, but will be marked as failed. Test results are always uploaded, even if tests fail, to help diagnose issues.

### Creating a Release

To create a new release:

1. Create and push a new tag following the format `X.Y.Z-YYYYMMDD`:
   ```
   git tag 1.0.0-20250330
   git push origin 1.0.0-20250330
   ```

2. The release workflow will automatically:
   - Build and test the application
   - Create platform-specific builds
   - Create a NuGet package with the specified version
   - Upload the package as an artifact
   - Publish to NuGet.org (if configured)

Alternatively, you can manually trigger a release:
1. Go to the "Actions" tab in your GitHub repository
2. Select the "Release" workflow
3. Click "Run workflow"
4. Enter the version number (e.g., "1.0.0-20250330")
5. Click "Run workflow"

### Publishing to NuGet

The release workflow can automatically publish to NuGet.org if properly configured. See [NuGet Publishing Guide](./nuget-publishing.md) for detailed instructions on:

- Setting up a NuGet feed
- Configuring GitHub secrets for NuGet API keys
- Enabling/disabling automatic publishing
- Manual publishing instructions

## Troubleshooting

### Build Failures

If a workflow fails:
1. Check the "Actions" tab in GitHub
2. Click on the failed workflow run
3. Examine the logs to identify the issue

Common issues include:
- Test failures
- Missing dependencies
- Version conflicts

### Analyzing Test Failures

When tests fail:

1. **View the Test Summary**:
   - The GitHub UI provides a summary of test results
   - Failed tests are highlighted with details of the failure
   - This is the fastest way to identify which tests failed

2. **Download Test Results**:
   - For detailed analysis, download the test artifacts
   - These contain complete test result files in TRX format
   - Can be opened in Visual Studio or other test result viewers

3. **Look for Patterns**:
   - Check if failures are consistent or intermittent
   - Look for environment-specific failures
   - Check if failures only occur in specific workflows

4. **Debugging Tips**:
   - Run the failing tests locally with verbose output
   - Use the same configuration as the CI environment
   - Consider adding more detailed logging to tests

### NuGet Publishing Issues

If NuGet publishing fails:
1. Verify your NuGet API key is correctly set in GitHub Secrets
2. Check that the package ID is not already taken on NuGet.org
3. Ensure package version is unique (you cannot republish a version)

For detailed help with NuGet publishing, see the [NuGet Publishing Guide](./nuget-publishing.md).