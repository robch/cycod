# 12. Asynchronous Programming

```csharp
// Use async/await throughout the codebase
public async Task<OperationResult> ProcessDataAsync(DataRequest request)
{
    // Asynchronously get operation context
    var context = await LoadContextAsync();
    
    // Process data with proper exception handling
    try 
    {
        // Asynchronously process resources
        var resources = await ResourceHelper.GetResourcesAsync(request.Path);
        var results = new List<ResourceResult>();
        
        foreach (var resource in resources)
        {
            // Process each resource asynchronously
            var result = await ProcessResourceAsync(resource);
            results.Add(result);
        }
        
        return new OperationResult(results);
    }
    catch (ResourceNotFoundException ex)
    {
        // Specific exception handling
        Logger.LogError($"Resource not found: {ex.ResourceName}");
        return OperationResult.Error(ex.Message);
    }
    catch (Exception ex)
    {
        // General error handling
        Logger.LogError($"Operation failed: {ex.Message}");
        return OperationResult.Error(ex.Message);
    }
}

// GOOD: Return Task from async methods
public async Task<DocumentData> ReadDocumentAsync(string path)
{
    using var stream = new FileStream(path, FileMode.Open);
    using var reader = new StreamReader(stream);
    var content = await reader.ReadToEndAsync();
    return new DocumentData(path, content);
}

// BAD: Never use ConfigureAwait(false) in application code
public async Task<string> ReadDocumentContentBadAsync(string path)
{
    using var reader = new StreamReader(path);
    // DON'T DO THIS:
    return await reader.ReadToEndAsync().ConfigureAwait(false);
}

// GOOD: Proper ConfigureAwait usage
public async Task<string> ReadDocumentContentGoodAsync(string path)
{
    using var reader = new StreamReader(path);
    // Correct approach for application code:
    return await reader.ReadToEndAsync();
}

// BAD: Avoid async void except for event handlers
// This can cause unhandled exceptions that crash the application
private async void ProcessDocumentDangerous(string path)
{
    await Task.Delay(100); // Simulating work
    throw new Exception("This exception is unobservable!");
}

// GOOD: Use async Task instead
private async Task ProcessDocumentSafeAsync(string path)
{
    await Task.Delay(100); // Simulating work
    // Exception can be caught by caller
}

// GOOD: Event handler is an acceptable use of async void
private async void OnDocumentChanged(object sender, FileSystemEventArgs e)
{
    try
    {
        await ProcessDocumentChangeAsync(e.FullPath);
    }
    catch (Exception ex)
    {
        // Always handle exceptions in async void methods
        Logger.LogError($"Error processing document change: {ex.Message}");
    }
}

// GOOD: Proper async method naming
public async Task<List<DataRecord>> GetRecordsAsync(string collectionId)
{
    return await RecordRepository.LoadRecordsAsync(collectionId);
}
```

### Core Principles

- Use async/await consistently throughout the codebase
- Always suffix async methods with "Async"
- Return Task or Task<T> from async methods, never void (except for event handlers)
- Never use ConfigureAwait(false) in application code
- Always handle exceptions in async code, especially in async void methods
- Use proper resource cleanup with async methods
- Maintain consistent async patterns within related codebases

### Why It Matters

Asynchronous programming in C# allows operations like file I/O, network calls, and other potentially blocking tasks to execute without freezing the application. In the context of CLI applications:

1. **Responsiveness**: Even command-line applications benefit from not blocking while waiting for file operations or external resources
2. **Scalability**: Proper async patterns allow the application to handle more concurrent operations
3. **Consistent Error Handling**: Well-structured async code ensures exceptions are properly propagated and can be handled
4. **Composability**: Async methods can be easily composed together into larger operations
5. **Clean Cancellation**: Properly structured async code supports cancellation, allowing operations to be gracefully terminated

Using consistent async patterns makes the codebase more maintainable and prevents subtle bugs that can occur when mixing async and synchronous code.

### Common Mistakes

#### Using ConfigureAwait(false) in Application Code

```csharp
// BAD: Don't use ConfigureAwait(false) in application code
public async Task<string> ProcessDocumentAsync(string path)
{
    var content = await File.ReadAllTextAsync(path).ConfigureAwait(false);
    return content.ToUpper();
}
```

**Why it's problematic**: ConfigureAwait(false) is meant primarily for library code to prevent deadlocks when library code is called by synchronous code. In application code, it creates inconsistent execution context which complicates debugging and can lead to subtle bugs, especially with context-dependent operations like UI updates or scoped services.

#### Using Async Void Methods

```csharp
// BAD: Async void methods can't be awaited and exceptions can't be caught
public async void ProcessDocument(string path)
{
    var content = await File.ReadAllTextAsync(path);
    // If an exception occurs here, it can crash the application
}
```

**Why it's problematic**: Exceptions in async void methods can't be caught by the caller and will propagate directly to the SynchronizationContext, potentially crashing the application. Additionally, callers can't await async void methods, making it impossible to know when they complete.

#### Mixing Async and Sync Code

```csharp
// BAD: Blocking on async code with .Result or .Wait()
public string GetDocumentContent(string path)
{
    // This can cause deadlocks
    return ReadDocumentContentAsync(path).Result;
}
```

**Why it's problematic**: Blocking on async code with .Result or .Wait() can cause deadlocks, especially in applications with a SynchronizationContext (like WPF or ASP.NET). It also negates the benefits of asynchronous programming.

#### Not Handling Exceptions in Async Event Handlers

```csharp
// BAD: Unhandled exceptions in async void event handlers
private async void OnDocumentChanged(object sender, FileSystemEventArgs e)
{
    // If this throws, the exception is lost and may crash the application
    await ProcessDocumentChangeAsync(e.FullPath);
}
```

**Why it's problematic**: Since async void methods can't be awaited, exceptions that occur in them can't be caught by the caller. Always wrap the contents of async void event handlers in try/catch blocks.

### Evolution Example

Let's look at how a method might evolve from problematic to ideal implementation:

**Initial Version - Mixing sync and async, poor error handling:**

```csharp
// Initial version with multiple issues
public OperationResult ProcessData(DataRequest request)
{
    try
    {
        // Blocking call to async method - potential deadlock
        var context = LoadContextAsync().Result;
        
        var resources = Directory.GetFiles(request.Path);
        var results = new List<ResourceResult>();
        
        foreach (var resource in resources)
        {
            // More blocking calls
            var content = File.ReadAllText(resource);
            var result = ProcessContent(content).Result;
            results.Add(new ResourceResult(resource, result));
        }
        
        return new OperationResult(results);
    }
    catch (Exception ex)
    {
        // Generic error handling
        Console.WriteLine("Error: " + ex.Message);
        return null; // Returning null is problematic
    }
}
```

**Intermediate Version - Using async but with issues:**

```csharp
// Improved but still problematic
public async Task<OperationResult> ProcessDataAsync(DataRequest request)
{
    var context = await LoadContextAsync().ConfigureAwait(false); // Unnecessary ConfigureAwait
    
    try
    {
        var resources = Directory.GetFiles(request.Path); // Still synchronous
        var results = new List<ResourceResult>();
        
        // Not taking advantage of potential parallelism
        foreach (var resource in resources)
        {
            var content = await File.ReadAllTextAsync(resource);
            var result = await ProcessContentAsync(content);
            results.Add(new ResourceResult(resource, result));
        }
        
        return new OperationResult(results);
    }
    catch (Exception ex) // Too generic exception handling
    {
        Console.WriteLine("Error: " + ex.Message);
        return new OperationResult { Success = false }; // Better than null
    }
}
```

**Final Version - Properly async with good practices:**

```csharp
public async Task<OperationResult> ProcessDataAsync(DataRequest request)
{
    if (request == null) throw new ArgumentNullException(nameof(request));
    if (string.IsNullOrEmpty(request.Path)) throw new ArgumentException("Path is required", nameof(request));
    
    try
    {
        // Proper async all the way through
        var context = await LoadContextAsync();
        var resources = await ResourceHelper.GetResourcesAsync(request.Path);
        var results = new List<ResourceResult>();
        
        // Process resources with proper exception handling
        foreach (var resource in resources)
        {
            try
            {
                var content = await File.ReadAllTextAsync(resource);
                var result = await ProcessContentAsync(content);
                results.Add(new ResourceResult(resource, result));
            }
            catch (IOException ex)
            {
                // Specific handling for file errors
                Logger.LogError($"Error processing {resource}: {ex.Message}");
                results.Add(ResourceResult.Error(resource, ex.Message));
            }
        }
        
        return new OperationResult(results);
    }
    catch (DirectoryNotFoundException ex)
    {
        // Specific exception types
        Logger.LogError($"Directory not found: {ex.Message}");
        return OperationResult.Error(ex.Message);
    }
    catch (UnauthorizedAccessException ex)
    {
        Logger.LogError($"Access denied: {ex.Message}");
        return OperationResult.Error(ex.Message);
    }
    catch (Exception ex)
    {
        // General exception as last resort
        Logger.LogError($"Operation failed: {ex.Message}");
        return OperationResult.Error(ex.Message);
    }
}
```

### Deeper Understanding

#### The Task Model

The C# async/await pattern is built on the Task and Task<T> classes, which represent asynchronous operations. Understanding this model helps clarify many async programming concepts:

1. **Task Lifecycle**: A Task represents an operation that will complete in the future. It can be:
   - Running (not yet completed)
   - Completed successfully 
   - Completed with an error (faulted)
   - Canceled

2. **Task Completion**: When a Task completes, it allows code that was awaiting it to resume execution.

3. **Task Continuations**: The await keyword sets up a continuation that will run when the awaited Task completes.

#### Execution Context and SynchronizationContext

When you await a Task, the current execution context is captured and restored when the Task completes. This context includes:

1. **SynchronizationContext**: Controls where continuation code runs (UI thread in GUI apps, thread pool in console apps)
2. **ExecutionContext**: Carries information like security context, locale, and other ambient data

In CLI applications, there's typically no SynchronizationContext, which means continuations run on thread pool threads. This is why ConfigureAwait(false) is unnecessary in our code - it only makes a difference when there's a SynchronizationContext to capture.

#### Async Method Execution Flow

Understanding the execution flow of async methods helps clarify their behavior:

1. When an async method is called, it runs synchronously until it hits an await
2. At the await, if the awaited Task isn't complete, the method returns a Task to its caller
3. When the awaited Task completes, the method resumes from the await point
4. If an exception occurs in the awaited Task, it's propagated when awaiting

This explains why async void methods are problematic - since they don't return a Task, there's no way for exceptions to be observed by the caller.

#### Performance Considerations

Async methods have some overhead due to state machine creation and task scheduling. For very quick operations, this overhead might not be worth it. However, for I/O operations like file access, network calls, or database queries, the benefits of asynchronous execution far outweigh this overhead.

In CLI applications, use async for:
- File operations 
- Network requests
- External process execution
- Database access

But consider synchronous code for:
- Simple in-memory operations
- Very quick calculations
- Operations that must complete before continuing