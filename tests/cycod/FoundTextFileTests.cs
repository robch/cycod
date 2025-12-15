using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[TestClass]
public class FoundTextFileTests
{
    #region Basic Construction Tests

    [TestMethod]
    public void Constructor_WithRequiredProperties_CreatesInstance()
    {
        // Arrange & Act
        var file = new FoundTextFile
        {
            Path = "test.txt",
            LoadContent = async () => await Task.FromResult("test content")
        };

        // Assert
        Assert.IsNotNull(file);
        Assert.AreEqual("test.txt", file.Path);
        Assert.IsNull(file.Content);
        Assert.IsNotNull(file.Metadata);
        Assert.AreEqual(0, file.Metadata.Count);
    }

    [TestMethod]
    public void Constructor_WithAllProperties_CreatesInstance()
    {
        // Arrange & Act
        var metadata = new Dictionary<string, object>
        {
            { "key1", "value1" },
            { "key2", 42 }
        };

        var file = new FoundTextFile
        {
            Path = "test.txt",
            Content = "initial content",
            LoadContent = async () => await Task.FromResult("loaded content"),
            Metadata = metadata
        };

        // Assert
        Assert.AreEqual("test.txt", file.Path);
        Assert.AreEqual("initial content", file.Content);
        Assert.AreEqual(2, file.Metadata.Count);
        Assert.AreEqual("value1", file.Metadata["key1"]);
        Assert.AreEqual(42, file.Metadata["key2"]);
    }

    [TestMethod]
    public void Constructor_WithEmptyPath_CreatesInstance()
    {
        // Arrange & Act
        var file = new FoundTextFile
        {
            Path = "",
            LoadContent = async () => await Task.FromResult("content")
        };

        // Assert
        Assert.AreEqual("", file.Path);
    }

    #endregion

    #region Lazy Loading Tests

    [TestMethod]
    public async Task LoadContent_WhenCalled_ReturnsExpectedContent()
    {
        // Arrange
        var expectedContent = "test content from lambda";
        var file = new FoundTextFile
        {
            Path = "test.txt",
            LoadContent = async () => await Task.FromResult(expectedContent)
        };

        // Act
        var content = await file.LoadContent();

        // Assert
        Assert.AreEqual(expectedContent, content);
    }

    [TestMethod]
    public async Task LoadContent_CalledMultipleTimes_ExecutesEachTime()
    {
        // Arrange
        var callCount = 0;
        var file = new FoundTextFile
        {
            Path = "test.txt",
            LoadContent = async () =>
            {
                callCount++;
                return await Task.FromResult($"call {callCount}");
            }
        };

        // Act
        var content1 = await file.LoadContent();
        var content2 = await file.LoadContent();
        var content3 = await file.LoadContent();

        // Assert
        Assert.AreEqual("call 1", content1);
        Assert.AreEqual("call 2", content2);
        Assert.AreEqual("call 3", content3);
        Assert.AreEqual(3, callCount);
    }

    [TestMethod]
    public async Task LoadContent_WithCaching_CanBeManuallyCached()
    {
        // Arrange
        var callCount = 0;
        var file = new FoundTextFile
        {
            Path = "test.txt",
            LoadContent = async () =>
            {
                callCount++;
                return await Task.FromResult("content");
            }
        };

        // Act - First call and cache
        file.Content = await file.LoadContent();
        
        // Act - Use cached value
        var content1 = file.Content;
        var content2 = file.Content;

        // Assert
        Assert.AreEqual(1, callCount); // Lambda only called once
        Assert.AreEqual("content", content1);
        Assert.AreEqual("content", content2);
    }

    [TestMethod]
    public async Task LoadContent_WithAsyncOperation_CompletesSuccessfully()
    {
        // Arrange
        var file = new FoundTextFile
        {
            Path = "test.txt",
            LoadContent = async () =>
            {
                await Task.Delay(10); // Simulate async work
                return "async content";
            }
        };

        // Act
        var content = await file.LoadContent();

        // Assert
        Assert.AreEqual("async content", content);
    }

    [TestMethod]
    public async Task LoadContent_ReturningNull_HandlesGracefully()
    {
        // Arrange
        var file = new FoundTextFile
        {
            Path = "test.txt",
            LoadContent = async () => await Task.FromResult<string>(null!)
        };

        // Act
        var content = await file.LoadContent();

        // Assert
        Assert.IsNull(content);
    }

    [TestMethod]
    public async Task LoadContent_ReturningEmptyString_HandlesGracefully()
    {
        // Arrange
        var file = new FoundTextFile
        {
            Path = "test.txt",
            LoadContent = async () => await Task.FromResult("")
        };

        // Act
        var content = await file.LoadContent();

        // Assert
        Assert.AreEqual("", content);
    }

    [TestMethod]
    public async Task LoadContent_WithLargeContent_HandlesSuccessfully()
    {
        // Arrange
        var largeContent = new string('x', 1_000_000); // 1MB of 'x'
        var file = new FoundTextFile
        {
            Path = "large.txt",
            LoadContent = async () => await Task.FromResult(largeContent)
        };

        // Act
        var content = await file.LoadContent();

        // Assert
        Assert.AreEqual(1_000_000, content.Length);
        Assert.AreEqual(largeContent, content);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public async Task LoadContent_ThrowsException_PropagatesException()
    {
        // Arrange
        var file = new FoundTextFile
        {
            Path = "error.txt",
            LoadContent = async () =>
            {
                await Task.CompletedTask;
                throw new InvalidOperationException("Simulated error");
            }
        };

        // Act
        await file.LoadContent();

        // Assert - ExpectedException attribute handles this
    }

    #endregion

    #region Metadata Tests

    [TestMethod]
    public void Metadata_AddSingleItem_Succeeds()
    {
        // Arrange
        var file = new FoundTextFile
        {
            Path = "test.txt",
            LoadContent = async () => await Task.FromResult("content")
        };

        // Act
        file.Metadata["key"] = "value";

        // Assert
        Assert.AreEqual(1, file.Metadata.Count);
        Assert.AreEqual("value", file.Metadata["key"]);
    }

    [TestMethod]
    public void Metadata_AddMultipleItems_Succeeds()
    {
        // Arrange
        var file = new FoundTextFile
        {
            Path = "test.txt",
            LoadContent = async () => await Task.FromResult("content")
        };

        // Act
        file.Metadata["string"] = "text";
        file.Metadata["int"] = 42;
        file.Metadata["bool"] = true;
        file.Metadata["object"] = new { Name = "Test" };

        // Assert
        Assert.AreEqual(4, file.Metadata.Count);
        Assert.AreEqual("text", file.Metadata["string"]);
        Assert.AreEqual(42, file.Metadata["int"]);
        Assert.AreEqual(true, file.Metadata["bool"]);
        Assert.IsNotNull(file.Metadata["object"]);
    }

    [TestMethod]
    public void Metadata_InitializedWithData_RetainsData()
    {
        // Arrange
        var metadata = new Dictionary<string, object>
        {
            { "repo", "test/repo" },
            { "sha", "abc123" },
            { "stars", 100 }
        };

        // Act
        var file = new FoundTextFile
        {
            Path = "test.txt",
            LoadContent = async () => await Task.FromResult("content"),
            Metadata = metadata
        };

        // Assert
        Assert.AreEqual(3, file.Metadata.Count);
        Assert.AreEqual("test/repo", file.Metadata["repo"]);
        Assert.AreEqual("abc123", file.Metadata["sha"]);
        Assert.AreEqual(100, file.Metadata["stars"]);
    }

    [TestMethod]
    public void Metadata_UpdateExistingKey_Succeeds()
    {
        // Arrange
        var file = new FoundTextFile
        {
            Path = "test.txt",
            LoadContent = async () => await Task.FromResult("content")
        };
        file.Metadata["version"] = 1;

        // Act
        file.Metadata["version"] = 2;

        // Assert
        Assert.AreEqual(1, file.Metadata.Count);
        Assert.AreEqual(2, file.Metadata["version"]);
    }

    [TestMethod]
    public void Metadata_RemoveKey_Succeeds()
    {
        // Arrange
        var file = new FoundTextFile
        {
            Path = "test.txt",
            LoadContent = async () => await Task.FromResult("content")
        };
        file.Metadata["key1"] = "value1";
        file.Metadata["key2"] = "value2";

        // Act
        file.Metadata.Remove("key1");

        // Assert
        Assert.AreEqual(1, file.Metadata.Count);
        Assert.IsFalse(file.Metadata.ContainsKey("key1"));
        Assert.IsTrue(file.Metadata.ContainsKey("key2"));
    }

    #endregion

    #region Real-World Scenario Tests

    [TestMethod]
    public async Task Scenario_FilesystemFile_LoadsCorrectly()
    {
        // Arrange - Simulate filesystem file
        var filePath = "src/Program.cs";
        var fileContent = "using System;\nclass Program { }";
        
        var file = new FoundTextFile
        {
            Path = filePath,
            LoadContent = async () => await Task.FromResult(fileContent) // Simulates File.ReadAllTextAsync
        };

        // Act
        var content = await file.LoadContent();

        // Assert
        Assert.AreEqual(filePath, file.Path);
        Assert.AreEqual(fileContent, content);
    }

    [TestMethod]
    public async Task Scenario_GitHubFile_LoadsWithMetadata()
    {
        // Arrange - Simulate GitHub file
        var file = new FoundTextFile
        {
            Path = "src/Program.cs",
            LoadContent = async () => await Task.FromResult("// GitHub content"),
            Metadata = new Dictionary<string, object>
            {
                { "Repository", "owner/repo" },
                { "Sha", "abc123def456" },
                { "Url", "https://github.com/owner/repo/blob/main/src/Program.cs" }
            }
        };

        // Act
        var content = await file.LoadContent();

        // Assert
        Assert.AreEqual("src/Program.cs", file.Path);
        Assert.AreEqual("// GitHub content", content);
        Assert.AreEqual("owner/repo", file.Metadata["Repository"]);
        Assert.AreEqual("abc123def456", file.Metadata["Sha"]);
    }

    [TestMethod]
    public async Task Scenario_LazyLoadingPattern_OnlyLoadsWhenNeeded()
    {
        // Arrange
        var loadCalled = false;
        var file = new FoundTextFile
        {
            Path = "lazy.txt",
            LoadContent = async () =>
            {
                loadCalled = true;
                return await Task.FromResult("lazy content");
            }
        };

        // Act - Create file but don't load
        Assert.IsFalse(loadCalled);
        
        // Act - Now load
        var content = await file.LoadContent();

        // Assert
        Assert.IsTrue(loadCalled);
        Assert.AreEqual("lazy content", content);
    }

    [TestMethod]
    public async Task Scenario_CachingPattern_ManualCacheManagement()
    {
        // Arrange
        var loadCount = 0;
        var file = new FoundTextFile
        {
            Path = "cached.txt",
            LoadContent = async () =>
            {
                loadCount++;
                return await Task.FromResult($"load #{loadCount}");
            }
        };

        // Act - Load and cache
        if (file.Content == null)
        {
            file.Content = await file.LoadContent();
        }
        var firstAccess = file.Content;

        // Subsequent accesses use cache
        var secondAccess = file.Content;
        var thirdAccess = file.Content;

        // Assert
        Assert.AreEqual(1, loadCount); // Only loaded once
        Assert.AreEqual("load #1", firstAccess);
        Assert.AreEqual("load #1", secondAccess);
        Assert.AreEqual("load #1", thirdAccess);
    }

    #endregion
}
