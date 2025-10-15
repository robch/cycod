# 5. Exception Handling and Error Returns

```csharp
// Return null for "not found" scenarios
public User FindUser(string username)
{
    // Like checking if a patient is registered at the hospital
    return _repository.GetByUsername(username);  // May be null
}

// Throw for invalid inputs & exceptional conditions
public void ProcessPayment(decimal amount)
{
    // Like refusing admission for invalid information
    if (amount <= 0) throw new ArgumentException("Amount must be positive", nameof(amount));
    
    // Like hitting the emergency button when a critical system fails
    if (paymentGateway.IsDown)
    {
        throw new PaymentException("Payment gateway unavailable");
    }
}

// Try pattern for operations expected to fail sometimes
public bool TryParseOrderId(string input, out int orderId)
{
    // Like attempting a non-critical procedure that might not work
    orderId = 0;
    if (string.IsNullOrEmpty(input)) return false;
    
    return int.TryParse(input, out orderId);
}

// Only catch exceptions you can handle
try 
{
    ProcessFile(fileName);
}
catch (FileNotFoundException ex)
{
    // Handle missing file specifically - like a specialized response team
    Logger.Warn($"File not found: {ex.FileName}");
}
catch (IOException ex)
{
    // Handle IO issues - another specialized response team
    Logger.Error($"IO error: {ex.Message}");
}
// Let other exceptions bubble up - like escalating to a higher-level emergency response

// Using finally for cleanup
public void ProcessPatientData(string patientFile)
{
    Database db = null;
    try
    {
        db = OpenDatabase();
        var data = ReadPatientFile(patientFile);
        UpdatePatientRecords(db, data);
    }
    catch (FileNotFoundException ex)
    {
        Logger.Error($"Patient file not found: {ex.Message}");
    }
    finally
    {
        // Always close the database - like restoring systems after an emergency
        db?.Close();
    }
}
```

### Core Principles

- Choose the right error handling approach based on context:
  - Return null/default for "not found" scenarios
  - Throw exceptions for invalid inputs and exceptional conditions
  - Use Try pattern for operations expected to fail sometimes
  - Return boolean for simple success/failure cases
- Catch only exceptions you can handle meaningfully
- Use specific exception types rather than generic Exception
- Include parameter names in exceptions for clarity
- Add descriptive error messages to aid troubleshooting
- Always clean up resources with finally blocks

### Why It Matters

Imagine a hospital's emergency response system - it must handle everything from routine check-ups to life-threatening emergencies with appropriate protocols for each situation. Similarly, your code needs different strategies for handling various error scenarios.

Well-designed exception handling:

1. **Improves Reliability**: Like hospital safety systems that prevent small issues from becoming critical failures
2. **Enhances Debugging**: Detailed error information helps diagnose problems, like accurate patient symptoms help doctors
3. **Preserves Resources**: Proper cleanup prevents resource leaks, like making sure all systems are restored after an emergency
4. **Guides Recovery**: Helps the application recover gracefully, like emergency protocols help a hospital return to normal operations

Poor exception handling can crash applications, corrupt data, leak resources, and make debugging nearly impossible.

### Common Mistakes

#### Using Exceptions for Normal Control Flow

```csharp
// BAD: Using exceptions for expected conditions
public int GetUserAge(string username)
{
    try
    {
        var user = _repository.GetByUsername(username);
        return user.Age;
    }
    catch (NullReferenceException)
    {
        return 0; // Default age if user not found
    }
}
```

**Why it's problematic**: This is like triggering the hospital's code blue emergency system to ask if a patient is available for a routine check-up. Exceptions should be for exceptional conditions, not normal program flow. They're expensive to create and process.

**Better approach**:

```csharp
// GOOD: Check for null instead of catching exceptions
public int GetUserAge(string username)
{
    var user = _repository.GetByUsername(username);
    return user?.Age ?? 0; // Null conditional and null coalescing operators
}
```

#### Catching Exception Base Class

```csharp
// BAD: Catching all exceptions
try
{
    ProcessOrder(order);
}
catch (Exception ex) // Too broad!
{
    Logger.Log(ex.Message);
    // Now what? How do we recover properly?
}
```

**Why it's problematic**: This is like having a single emergency response team for all hospital emergencies - from paper cuts to heart attacks to building fires. Without specialized handling, you can't respond appropriately to different error types.

**Better approach**:

```csharp
// GOOD: Catch specific exceptions you can handle
try
{
    ProcessOrder(order);
}
catch (InvalidOrderException ex)
{
    Logger.Warn($"Invalid order: {ex.Message}");
    NotifyUserAboutInvalidOrder(order, ex.ValidationErrors);
}
catch (PaymentDeclinedException ex)
{
    Logger.Error($"Payment declined: {ex.Message}");
    SuggestAlternativePaymentMethod(order);
}
catch (Exception ex) // Last resort with explicit re-throw
{
    Logger.Critical($"Unexpected error processing order: {ex.Message}");
    throw; // Re-throw to let higher-level handlers deal with it
}
```

#### Swallowing Exceptions

```csharp
// BAD: Exception disappears with no handling
try
{
    SendNotification(user, message);
}
catch (Exception)
{
    // Empty catch block - exception is ignored completely
}
```

**Why it's problematic**: This is like noticing a patient has stopped breathing, then ignoring it and marking them as "treated" without any intervention. It hides problems rather than addressing them.

**Better approach**:

```csharp
// GOOD: At minimum, log the exception
try
{
    SendNotification(user, message);
}
catch (NotificationException ex)
{
    Logger.Warn($"Failed to send notification: {ex.Message}");
    // Store for retry later or notify user through alternative channel
    _notificationRetryQueue.Enqueue(new NotificationRetry(user, message));
}
```

#### Not Cleaning Up Resources

```csharp
// BAD: Resource leak if exception occurs
public void ProcessDataFile(string path)
{
    var stream = new FileStream(path, FileMode.Open);
    // If an exception happens here, stream is never closed
    var data = ReadData(stream);
    ProcessData(data);
    stream.Close(); // May never reach this line
}
```

**Why it's problematic**: This is like evacuating a hospital wing for an emergency but forgetting to turn off the oxygen supply - creating new hazards. Unclosed resources can cause memory leaks and other issues.

**Better approach**:

```csharp
// GOOD: Using to ensure resource cleanup
public void ProcessDataFile(string path)
{
    using (var stream = new FileStream(path, FileMode.Open))
    {
        var data = ReadData(stream);
        ProcessData(data);
        // stream automatically closed when exiting the using block
    }
}

// Or in C# 8+:
public void ProcessDataFile(string path)
{
    using var stream = new FileStream(path, FileMode.Open);
    var data = ReadData(stream);
    ProcessData(data);
    // stream automatically closed at end of method
}
```

### Evolution Example

Let's see how error handling in a method might evolve:

**Initial Version - Poor error handling:**

```csharp
// Like a hospital with no emergency protocols
public User RegisterUser(string username, string email, string password)
{
    var user = new User
    {
        Username = username,
        Email = email,
        Password = password
    };
    
    _database.Save(user);
    _emailService.SendWelcomeEmail(email);
    return user;
}
```

**Intermediate Version - Basic exception handling:**

```csharp
// Like a hospital with basic emergency protocols but gaps
public User RegisterUser(string username, string email, string password)
{
    try
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email))
        {
            return null; // Returning null instead of throwing for invalid inputs
        }
        
        var user = new User
        {
            Username = username,
            Email = email,
            Password = password
        };
        
        _database.Save(user);
        
        try
        {
            _emailService.SendWelcomeEmail(email);
        }
        catch (Exception) 
        {
            // Swallowing email exceptions - not ideal
        }
        
        return user;
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error: " + ex.Message);
        return null; // Returning null for all errors - loses information
    }
}
```

**Final Version - Comprehensive error handling:**

```csharp
// Like a hospital with comprehensive emergency protocols
public User RegisterUser(string username, string email, string password)
{
    // Validation - like checking patient eligibility before admission
    if (string.IsNullOrEmpty(username))
        throw new ArgumentException("Username is required", nameof(username));
        
    if (string.IsNullOrEmpty(email))
        throw new ArgumentException("Email is required", nameof(email));
        
    if (string.IsNullOrEmpty(password))
        throw new ArgumentException("Password is required", nameof(password));
        
    // Check for existing user - return null for "not found" pattern
    if (_userRepository.UsernameExists(username))
        return null;
    
    try
    {
        // Create and save user
        var user = new User
        {
            Username = username,
            Email = email,
            // Store password securely
            PasswordHash = _passwordService.HashPassword(password)
        };
        
        _database.Save(user);
        
        try
        {
            // Non-critical operation gets its own try/catch
            _emailService.SendWelcomeEmail(email);
        }
        catch (EmailServiceException ex)
        {
            // Log and queue for retry, but don't fail registration
            _logger.Warn($"Welcome email failed: {ex.Message}");
            _emailRetryQueue.Enqueue(new EmailTask(email, EmailType.Welcome));
        }
        
        return user;
    }
    catch (DuplicateEmailException ex)
    {
        // Specific business rule violation
        _logger.Info($"Registration failed - duplicate email: {ex.Message}");
        throw new RegistrationException("Email already registered", ex);
    }
    catch (DatabaseException ex)
    {
        // Technical failure
        _logger.Error($"Database error during registration: {ex.Message}");
        throw new RegistrationException("Unable to complete registration", ex);
    }
}
```

### Deeper Understanding

#### The Exception Handling System

In C#, exception handling works like a hospital's emergency response system:

1. **Detection (throw)**: Like triggering an alarm when something goes wrong
2. **Response (catch)**: Like dispatching specialized teams based on the emergency type
3. **Escalation (re-throw)**: Like transferring a patient to a more specialized facility
4. **Cleanup (finally)**: Like restoring systems after an emergency, regardless of outcome

#### Exception Hierarchy

C# exceptions follow a hierarchical structure, like medical specialties:

- **Exception** - The base type (like "Medical Emergency")
  - **SystemException** - Framework-thrown exceptions (like "Known Medical Conditions")
    - **ArgumentException** - Invalid arguments (like "Patient Intake Errors")
      - **ArgumentNullException** - Null argument (like "Missing Patient Information")
    - **IOException** - I/O errors (like "Medical Equipment Failures")
    - **NullReferenceException** - Null object access (like "Attempted Procedure on Non-existent Patient")
  - **ApplicationException** - Application-specific exceptions (like "Hospital-Specific Protocols")

#### When to Use Different Error Handling Approaches

1. **Return null/default** - For "not found" scenarios
   - Like when a patient isn't in the hospital records
   - Example: `FindUserByName(username)` returns null when user doesn't exist

2. **Throw exceptions** - For unexpected or exceptional conditions
   - Like triggering a code blue when a patient has cardiac arrest
   - Example: `ProcessPayment(amount)` throws when payment fails

3. **Try pattern** - For operations that frequently fail in expected ways
   - Like attempting a procedure with uncertain outcome
   - Example: `bool success = TryParse(input, out result);`

4. **Boolean return** - For simple success/failure operations
   - Like checking if a patient is eligible for treatment
   - Example: `bool userExists = database.ContainsUser(username);`

#### Exception Performance Considerations

Exceptions are relatively expensive in performance terms - like calling a hospital emergency response team. They:

1. Capture stack information (expensive)
2. Unwind the call stack looking for handlers
3. Allocate memory for exception objects

For performance-critical code, consider alternatives like the Try pattern for expected "failures" that aren't truly exceptional.