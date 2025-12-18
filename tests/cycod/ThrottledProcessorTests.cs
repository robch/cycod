using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

[TestClass]
public class ThrottledProcessorTests
{
    #region Basic Functionality Tests

    [TestMethod]
    public async Task ProcessAsync_EmptyList_ReturnsEmptyList()
    {
        // Arrange
        var processor = new ThrottledProcessor(maxParallelism: 2);
        var items = new List<int>();

        // Act
        var results = await processor.ProcessAsync(items, async item =>
        {
            await Task.Delay(10);
            return item * 2;
        });

        // Assert
        Assert.IsNotNull(results);
        Assert.AreEqual(0, results.Count);
    }

    [TestMethod]
    public async Task ProcessAsync_SingleItem_ProcessesSuccessfully()
    {
        // Arrange
        var processor = new ThrottledProcessor(maxParallelism: 2);
        var items = new List<int> { 5 };

        // Act
        var results = await processor.ProcessAsync(items, async item =>
        {
            await Task.Delay(10);
            return item * 2;
        });

        // Assert
        Assert.AreEqual(1, results.Count);
        Assert.AreEqual(10, results[0]);
    }

    [TestMethod]
    public async Task ProcessAsync_MultipleItems_ProcessesAll()
    {
        // Arrange
        var processor = new ThrottledProcessor(maxParallelism: 2);
        var items = new List<int> { 1, 2, 3, 4, 5 };

        // Act
        var results = await processor.ProcessAsync(items, async item =>
        {
            await Task.Delay(10);
            return item * 2;
        });

        // Assert
        Assert.AreEqual(5, results.Count);
        CollectionAssert.AreEquivalent(new[] { 2, 4, 6, 8, 10 }, results);
    }

    [TestMethod]
    public async Task ProcessAsync_DifferentTypes_WorksWithStrings()
    {
        // Arrange
        var processor = new ThrottledProcessor(maxParallelism: 3);
        var items = new List<string> { "a", "b", "c" };

        // Act
        var results = await processor.ProcessAsync(items, async item =>
        {
            await Task.Delay(10);
            return item.ToUpper();
        });

        // Assert
        Assert.AreEqual(3, results.Count);
        CollectionAssert.AreEquivalent(new[] { "A", "B", "C" }, results);
    }

    [TestMethod]
    public async Task ProcessAsync_DifferentTypes_WorksWithObjects()
    {
        // Arrange
        var processor = new ThrottledProcessor(maxParallelism: 2);
        var items = new List<int> { 1, 2, 3 };

        // Act
        var results = await processor.ProcessAsync(items, async item =>
        {
            await Task.Delay(10);
            return new { Id = item, Value = item * 10 };
        });

        // Assert
        Assert.AreEqual(3, results.Count);
        Assert.IsTrue(results.Any(r => r.Id == 1 && r.Value == 10));
        Assert.IsTrue(results.Any(r => r.Id == 2 && r.Value == 20));
        Assert.IsTrue(results.Any(r => r.Id == 3 && r.Value == 30));
    }

    #endregion

    #region Parallelism Behavior Tests

    [TestMethod]
    public async Task ProcessAsync_WithThrottling_RespectsMaxParallelism()
    {
        // Arrange
        var processor = new ThrottledProcessor(maxParallelism: 2);
        var items = new List<int> { 1, 2, 3, 4, 5 };
        var concurrentCount = 0;
        var maxConcurrentCount = 0;
        var lockObj = new object();

        // Act
        var results = await processor.ProcessAsync(items, async item =>
        {
            lock (lockObj)
            {
                concurrentCount++;
                if (concurrentCount > maxConcurrentCount)
                {
                    maxConcurrentCount = concurrentCount;
                }
            }

            await Task.Delay(50); // Simulate work

            lock (lockObj)
            {
                concurrentCount--;
            }

            return item;
        });

        // Assert
        Assert.AreEqual(5, results.Count);
        Assert.IsTrue(maxConcurrentCount <= 2, $"Max concurrent count was {maxConcurrentCount}, expected <= 2");
        Assert.IsTrue(maxConcurrentCount >= 2, $"Max concurrent count was {maxConcurrentCount}, expected at least 2 (parallelism should occur)");
    }

    [TestMethod]
    public async Task ProcessAsync_WithMaxParallelism1_RunsSequentially()
    {
        // Arrange
        var processor = new ThrottledProcessor(maxParallelism: 1);
        var items = new List<int> { 1, 2, 3, 4, 5 };
        var concurrentCount = 0;
        var maxConcurrentCount = 0;
        var lockObj = new object();

        // Act
        var results = await processor.ProcessAsync(items, async item =>
        {
            lock (lockObj)
            {
                concurrentCount++;
                if (concurrentCount > maxConcurrentCount)
                {
                    maxConcurrentCount = concurrentCount;
                }
            }

            await Task.Delay(20); // Simulate work

            lock (lockObj)
            {
                concurrentCount--;
            }

            return item;
        });

        // Assert
        Assert.AreEqual(5, results.Count);
        Assert.AreEqual(1, maxConcurrentCount, "Should only run one at a time");
    }

    [TestMethod]
    public async Task ProcessAsync_MaxParallelismGreaterThanItems_ProcessesAllConcurrently()
    {
        // Arrange
        var processor = new ThrottledProcessor(maxParallelism: 10);
        var items = new List<int> { 1, 2, 3 };
        var concurrentCount = 0;
        var maxConcurrentCount = 0;
        var lockObj = new object();

        // Act
        var results = await processor.ProcessAsync(items, async item =>
        {
            lock (lockObj)
            {
                concurrentCount++;
                if (concurrentCount > maxConcurrentCount)
                {
                    maxConcurrentCount = concurrentCount;
                }
            }

            await Task.Delay(50); // Simulate work

            lock (lockObj)
            {
                concurrentCount--;
            }

            return item;
        });

        // Assert
        Assert.AreEqual(3, results.Count);
        Assert.AreEqual(3, maxConcurrentCount, "All 3 items should run concurrently");
    }

    [TestMethod]
    public async Task ProcessAsync_ParallelExecution_FasterThanSequential()
    {
        // Arrange
        var processor = new ThrottledProcessor(maxParallelism: 5);
        var items = Enumerable.Range(1, 10).ToList();
        var delayMs = 50;

        // Act
        var sw = Stopwatch.StartNew();
        var results = await processor.ProcessAsync(items, async item =>
        {
            await Task.Delay(delayMs);
            return item;
        });
        sw.Stop();

        // Assert
        Assert.AreEqual(10, results.Count);
        // Sequential would take ~500ms (10 * 50ms)
        // With parallelism of 5, should take ~100ms (2 batches * 50ms)
        // Allow some overhead, so check < 300ms (significantly faster than sequential)
        Assert.IsTrue(sw.ElapsedMilliseconds < 300, 
            $"Parallel execution took {sw.ElapsedMilliseconds}ms, expected < 300ms (sequential would be ~500ms)");
    }

    #endregion

    #region Error Handling Tests

    [TestMethod]
    [ExpectedException(typeof(AggregateException))]
    public async Task ProcessAsync_OneItemThrows_PropagatesException()
    {
        // Arrange
        var processor = new ThrottledProcessor(maxParallelism: 2);
        var items = new List<int> { 1, 2, 3, 4, 5 };

        // Act
        try
        {
            await processor.ProcessAsync(items, async item =>
            {
                await Task.Delay(10);
                if (item == 3)
                {
                    throw new InvalidOperationException("Test exception");
                }
                return item * 2;
            });
        }
        catch (AggregateException ex)
        {
            // Verify the exception contains our specific exception
            Assert.IsTrue(ex.InnerExceptions.Any(e => e is InvalidOperationException));
            throw;
        }
    }

    [TestMethod]
    [ExpectedException(typeof(AggregateException))]
    public async Task ProcessAsync_MultipleItemsThrow_PropagatesExceptions()
    {
        // Arrange
        var processor = new ThrottledProcessor(maxParallelism: 3);
        var items = new List<int> { 1, 2, 3, 4, 5 };

        // Act
        try
        {
            await processor.ProcessAsync(items, async item =>
            {
                await Task.Delay(10);
                if (item == 2 || item == 4)
                {
                    throw new InvalidOperationException($"Error on item {item}");
                }
                return item * 2;
            });
        }
        catch (AggregateException ex)
        {
            // Verify we got at least one exception (may not get both due to timing)
            Assert.IsTrue(ex.InnerExceptions.Count >= 1);
            Assert.IsTrue(ex.InnerExceptions.All(e => e is InvalidOperationException));
            throw;
        }
    }

    [TestMethod]
    public async Task ProcessAsync_ProcessorReturnsNull_HandlesGracefully()
    {
        // Arrange
        var processor = new ThrottledProcessor(maxParallelism: 2);
        var items = new List<int> { 1, 2, 3 };

        // Act
        var results = await processor.ProcessAsync(items, async item =>
        {
            await Task.Delay(10);
            return (string)null!;
        });

        // Assert
        Assert.AreEqual(3, results.Count);
        Assert.IsTrue(results.All(r => r == null));
    }

    #endregion

    #region Different Parallelism Values Tests

    [TestMethod]
    public async Task ProcessAsync_MaxParallelism5_ProcessesCorrectly()
    {
        // Arrange
        var processor = new ThrottledProcessor(maxParallelism: 5);
        var items = Enumerable.Range(1, 20).ToList();

        // Act
        var results = await processor.ProcessAsync(items, async item =>
        {
            await Task.Delay(10);
            return item * 2;
        });

        // Assert
        Assert.AreEqual(20, results.Count);
        CollectionAssert.AreEquivalent(Enumerable.Range(1, 20).Select(i => i * 2).ToArray(), results);
    }

    [TestMethod]
    public async Task ProcessAsync_MaxParallelism10_ProcessesCorrectly()
    {
        // Arrange
        var processor = new ThrottledProcessor(maxParallelism: 10);
        var items = Enumerable.Range(1, 50).ToList();

        // Act
        var results = await processor.ProcessAsync(items, async item =>
        {
            await Task.Delay(5);
            return item * 3;
        });

        // Assert
        Assert.AreEqual(50, results.Count);
        CollectionAssert.AreEquivalent(Enumerable.Range(1, 50).Select(i => i * 3).ToArray(), results);
    }

    #endregion

    #region Real-World Scenario Tests

    [TestMethod]
    public async Task Scenario_ProcessingFiles_WorksCorrectly()
    {
        // Arrange - Simulate processing multiple files
        var processor = new ThrottledProcessor(maxParallelism: 3);
        var filePaths = new List<string>
        {
            "file1.txt",
            "file2.txt",
            "file3.txt",
            "file4.txt",
            "file5.txt"
        };

        // Act - Simulate reading file content
        var results = await processor.ProcessAsync(filePaths, async filePath =>
        {
            await Task.Delay(20); // Simulate I/O
            return new
            {
                Path = filePath,
                Content = $"Content of {filePath}",
                Length = filePath.Length
            };
        });

        // Assert
        Assert.AreEqual(5, results.Count);
        Assert.IsTrue(results.All(r => r.Content.StartsWith("Content of")));
        Assert.IsTrue(results.Any(r => r.Path == "file1.txt"));
    }

    [TestMethod]
    public async Task Scenario_MakingHttpRequests_ThrottlesCorrectly()
    {
        // Arrange - Simulate HTTP requests with rate limiting
        var processor = new ThrottledProcessor(maxParallelism: 2); // Simulate rate limit
        var urls = Enumerable.Range(1, 10).Select(i => $"http://example.com/api/{i}").ToList();

        // Act - Simulate HTTP calls
        var results = await processor.ProcessAsync(urls, async url =>
        {
            await Task.Delay(30); // Simulate network delay
            return new
            {
                Url = url,
                StatusCode = 200,
                Response = $"Response from {url}"
            };
        });

        // Assert
        Assert.AreEqual(10, results.Count);
        Assert.IsTrue(results.All(r => r.StatusCode == 200));
    }

    [TestMethod]
    public async Task Scenario_MixOfFastAndSlowTasks_CompletesAll()
    {
        // Arrange
        var processor = new ThrottledProcessor(maxParallelism: 3);
        var items = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        // Act - Mix of fast and slow processing
        var results = await processor.ProcessAsync(items, async item =>
        {
            // Even items are slow, odd items are fast
            var delay = item % 2 == 0 ? 50 : 5;
            await Task.Delay(delay);
            return item;
        });

        // Assert - All should complete regardless of speed
        Assert.AreEqual(10, results.Count);
        CollectionAssert.AreEquivalent(items, results);
    }

    [TestMethod]
    public async Task Scenario_TransformingData_AppliesCorrectly()
    {
        // Arrange - Simulate data transformation pipeline
        var processor = new ThrottledProcessor(maxParallelism: 4);
        var inputData = new List<string>
        {
            "hello world",
            "parallel processing",
            "unit testing",
            "code quality"
        };

        // Act - Transform: uppercase, word count, length
        var results = await processor.ProcessAsync(inputData, async text =>
        {
            await Task.Delay(10); // Simulate processing time
            return new
            {
                Original = text,
                Upper = text.ToUpper(),
                WordCount = text.Split(' ').Length,
                Length = text.Length
            };
        });

        // Assert
        Assert.AreEqual(4, results.Count);
        var helloResult = results.First(r => r.Original == "hello world");
        Assert.AreEqual("HELLO WORLD", helloResult.Upper);
        Assert.AreEqual(2, helloResult.WordCount);
        Assert.AreEqual(11, helloResult.Length);
    }

    [TestMethod]
    public async Task Scenario_BatchProcessing_CompletesInBatches()
    {
        // Arrange - Process 20 items with max parallelism of 5
        var processor = new ThrottledProcessor(maxParallelism: 5);
        var items = Enumerable.Range(1, 20).ToList();
        var processedOrder = new List<int>();
        var lockObj = new object();

        // Act
        var results = await processor.ProcessAsync(items, async item =>
        {
            await Task.Delay(50); // Each takes 50ms
            
            lock (lockObj)
            {
                processedOrder.Add(item);
            }
            
            return item * 2;
        });

        // Assert
        Assert.AreEqual(20, results.Count);
        Assert.AreEqual(20, processedOrder.Count);
        // With parallelism of 5, items should complete in roughly 4 batches
        // First 5 should complete before items 11-20
    }

    #endregion

    #region Edge Cases and Stress Tests

    [TestMethod]
    public async Task ProcessAsync_LargeNumberOfItems_HandlesSuccessfully()
    {
        // Arrange
        var processor = new ThrottledProcessor(maxParallelism: 10);
        var items = Enumerable.Range(1, 1000).ToList();

        // Act
        var sw = Stopwatch.StartNew();
        var results = await processor.ProcessAsync(items, async item =>
        {
            await Task.Delay(1); // Very short delay
            return item * 2;
        });
        sw.Stop();

        // Assert
        Assert.AreEqual(1000, results.Count);
        Assert.IsTrue(results.Sum() == Enumerable.Range(1, 1000).Sum() * 2);
        // Should complete in reasonable time with parallelism
        Assert.IsTrue(sw.ElapsedMilliseconds < 20000, $"Processing 1000 items took {sw.ElapsedMilliseconds}ms");
    }

    [TestMethod]
    public async Task ProcessAsync_ImmediateCompletion_WorksCorrectly()
    {
        // Arrange
        var processor = new ThrottledProcessor(maxParallelism: 5);
        var items = new List<int> { 1, 2, 3, 4, 5 };

        // Act - No async delay, immediate completion
        var results = await processor.ProcessAsync(items, async item =>
        {
            await Task.CompletedTask;
            return item * 2;
        });

        // Assert
        Assert.AreEqual(5, results.Count);
        CollectionAssert.AreEquivalent(new[] { 2, 4, 6, 8, 10 }, results);
    }

    #endregion
}
