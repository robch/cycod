# C# Coding Style Guide: Streamlined

This guide demonstrates C# best practices through clear examples, focusing on letting code speak for itself while providing essential context.

These guidelines offer best practices for common scenarios, but context matters. Use your judgment when deciding whether to strictly follow a guideline or adapt to your specific situation. Follow existing patterns in the file you're modifying ('When in Rome'), but when establishing new patterns or significantly refactoring, apply these guidelines to incrementally improve the codebase.

## 1. Variables and Types

```csharp
// Use var for local variables
var customer = GetCustomerById(123);
var isValid = Validate(customer);
var orders = customer.Orders.Where(o => o.IsActive).ToList();

// Private fields use underscore prefix
private readonly IUserService _userService;
private int _retryCount;
private string _connectionString;

// Constants use PascalCase
public const int MaxRetryAttempts = 3;
public const string DefaultRegion = "US-West";

// Use descriptive names that explain purpose
var isEligibleForDiscount = customer.Status == CustomerStatus.Premium && order.Total > 1000;
var hasShippingAddress = !string.IsNullOrEmpty(order.ShippingAddress);
```

### Principles:
- Use var consistently and choose descriptive names that reflect purpose
- Use underscore prefix and camelCase for private fields (`_fieldName`)
- Use PascalCase for constants
- Use descriptive variable names that indicate purpose
- Prefix boolean variables with Is, Has, or Can

### Notes:
- Only use explicit types when var would make the type unclear
- Choose variable names that explain "why" a value exists, not just what it contains

## 2. Method and Property Declarations

```csharp
// Methods start with verbs, use PascalCase
public User GetUserById(int id) { return _repository.Find(id); }
public void ProcessPayment(Payment payment) { _processor.Process(payment); }

// Boolean members use Is/Has/Can prefix
public bool IsActive { get; set; }
public bool HasPermission(string permission) { return _permissions.Contains(permission); }
public bool CanUserEditDocument(User user, Document doc) { return user.Id == doc.OwnerId; }

// Auto-properties for simple cases
public string Name { get; set; }
public DateTime CreatedAt { get; set; }

// Backing fields only when custom logic needed
private string _email;
public string Email 
{
    get => _email;
    set 
    {
        ValidateEmailFormat(value);
        _email = value;
    }
}

// Keep methods short and focused
public decimal CalculateDiscount(Order order) 
{
    if (order == null) return 0;
    if (!order.Items.Any()) return 0;
    
    var subtotal = order.Items.Sum(i => i.Price);
    var discountRate = DetermineDiscountRate(order);
    
    return Math.Round(subtotal * discountRate, 2);
}
```

### Principles:
- Use PascalCase for all public members
- Prefix methods with verbs (Get, Set, Update)
- Prefix boolean members with "Is", "Has", or "Can"
- Use auto-properties for simple cases without custom logic
- Keep methods short (<20 lines) and focused on a single responsibility
- Break complex methods into smaller helper methods
- Design APIs with the caller's perspective in mind. Consider how the method will be used and what patterns will make it most intuitive for other developers

### Notes:
- Use backing fields only when you need custom logic in property accessors
- For complex operations, consider breaking the main method into smaller helper methods

## 3. Control Flow

```csharp
// Early returns with semantic variables reduce nesting
public ValidationResult Validate(User user)
{
    var userIsNull = user == null;
    if (userIsNull) return ValidationResult.Invalid("User cannot be null");
    
    var emailMissing = string.IsNullOrEmpty(user.Email);
    if (emailMissing) return ValidationResult.Invalid("Email required");
    
    var nameMissing = string.IsNullOrEmpty(user.Name);
    if (nameMissing) return ValidationResult.Invalid("Name required");
    
    return ValidationResult.Success();
}

// Ternary for simple conditions
var displayName = user.Name ?? "Guest";
var statusLabel = user.IsActive ? "Active" : "Inactive";

// If/else for complex conditions
if (user.IsAuthenticated && 
    user.HasPermission("edit") && 
    !document.IsLocked) 
{
    document.AllowEditing();
}

// Single-line if for very simple cases - direct conditions only for the simplest checks
if (id <= 0) return null;
if (items.Length > maxAllowed) throw new ArgumentException("Too many items");

// Meaningful variables for conditions
var isEligibleForDiscount = user.IsVip && order.Total > 1000;
var hasRequiredDocuments = passport != null && visa != null;
if (isEligibleForDiscount && hasRequiredDocuments) 
{
    ApplySpecialDiscount();
}
```

### Principles:
- Use early returns to reduce nesting and improve readability
- Use ternary operators for simple conditions, if/else for complex ones
- Write single-line if statements for simple guard clauses
- Create descriptive variables for complex conditions
- Prefer positive conditions over negative ones

### Notes:
- Multi-line conditions should be indented for readability
- For complex boolean logic, extract conditions into well-named methods

## 4. Collections

```csharp
// Collection initializers for simple cases
var colors = new List<string> { "Red", "Green", "Blue" };
var ages = new Dictionary<string, int> 
{
    ["John"] = 30,
    ["Alice"] = 25
};

// Empty collections
var emptyList = new List<string>();
var emptyDict = new Dictionary<string, int>();

// Copy collections
var original = new List<string> { "one", "two" };
var copy = new List<string>(original);

// HashSet for unique values
var uniqueIds = new HashSet<int> { 1, 2, 3, 4, 5 };

// Return read-only collections from public APIs
public IReadOnlyList<string> GetAvailableColors()
{
    return new List<string> { "Red", "Green", "Blue" }.AsReadOnly();
}
```

### Principles:
- Choose the right collection type based on your usage pattern
- Use collection initializers for concise initialization
- Return interface types (`IEnumerable<T>`, `IReadOnlyList<T>`) from public methods
- Return empty collections rather than null when no items exist

### Notes:
- For large collections, consider specifying initial capacity to improve performance
- Be mindful of performance implications when using LINQ operations on large collections

## 5. Exception Handling and Error Returns

```csharp
// Return null for "not found" scenarios
public User FindUser(string username)
{
    return _repository.GetByUsername(username);  // May be null
}

// Throw for invalid inputs & exceptional conditions
public void ProcessPayment(decimal amount)
{
    if (amount <= 0) throw new ArgumentException("Amount must be positive", nameof(amount));
    
    if (paymentGateway.IsDown)
    {
        throw new PaymentException("Payment gateway unavailable");
    }
}

// Try pattern for operations expected to fail sometimes
public bool TryParseOrderId(string input, out int orderId)
{
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
    // Handle missing file specifically
    Logger.Warn($"File not found: {ex.FileName}");
}
catch (IOException ex)
{
    // Handle IO issues
    Logger.Error($"IO error: {ex.Message}");
}
// Let other exceptions bubble up
```

### Principles:
- Choose the right error handling approach based on context:
  - Return null/default for "not found" scenarios
  - Throw exceptions for invalid inputs and exceptional conditions
  - Use Try pattern (bool return + out parameter) for operations expected to fail
  - Return boolean for simple success/failure cases
  - Consider the caller's perspective when choosing an approach
- Catch only exceptions you can meaningfully handle
- Use specific exception types rather than generic Exception

### Notes:
- Include the parameter name in ArgumentException using nameof()
- Add descriptive error messages that help with troubleshooting

## 6. Class Structure

```csharp
// Organize by access level, then by type
public class Customer
{
    // Public properties
    public int Id { get; set; }
    public string Name { get; set; }
    
    // Public methods
    public void UpdateProfile(ProfileData data) { /* ... */ }
    public bool CanPlaceOrder() { /* ... */ }
    
    // Protected properties
    protected DateTime LastUpdated { get; set; }
    
    // Protected methods
    protected void OnProfileUpdated() { /* ... */ }
    
    // Private fields (at the bottom)
    private readonly ICustomerRepository _repository;
    private List<Order> _cachedOrders;
}
```

### Principles:
- Organize members by access level: public, then protected, then private
- Within each access level, group by type: properties, methods, fields
- Keep fields at the bottom of each access level group
- Use one class per file in most cases

### Notes:
- Small, focused classes are easier to test and maintain
- Consider partial classes for separating generated code from hand-written code

## 7. Comments and Documentation

```csharp
// XML documentation for public APIs
/// <summary>
/// Processes a payment for an order.
/// </summary>
/// <param name="orderId">The order identifier to process payment for</param>
/// <param name="paymentMethod">The payment method to use</param>
/// <returns>Transaction receipt with confirmation details</returns>
/// <exception cref="PaymentDeclinedException">Thrown when payment is declined</exception>
public Receipt ProcessPayment(int orderId, PaymentMethod paymentMethod)
{
    var order = _orderRepository.GetById(orderId);
    var paymentProcessor = _paymentFactory.CreateProcessor(paymentMethod);
    
    // Self-documenting code with minimal comments
    var orderTotal = CalculateOrderTotal(order);
    var wasAuthorized = paymentProcessor.Authorize(orderTotal);
    
    if (!wasAuthorized)
    {
        throw new PaymentDeclinedException(paymentProcessor.DeclineReason);
    }
    
    var receipt = paymentProcessor.Capture(orderTotal);
    _orderRepository.MarkAsPaid(orderId, receipt.TransactionId);
    
    return receipt;
}

// Comments only for complex logic that isn't obvious
public decimal CalculateShipping(Order order)
{
    var baseShipping = order.Weight * _shippingRatePerKg;
    
    // Apply progressive discount for heavier packages
    // (Complex business rule that needs explanation)
    if (order.Weight > 10)
    {
        var discountTiers = Math.Floor((order.Weight - 10) / 5);
        var discountMultiplier = Math.Min(discountTiers * 0.05, 0.5);
        baseShipping *= (1 - discountMultiplier);
    }
    
    return baseShipping;
}
```

### Principles:
- Use complete XML documentation that describes purpose, parameters, return values, and exceptions, focusing on information that isn't obvious from the method signature
- Write comments that explain WHY, not WHAT the code does
- Let code be self-documenting through descriptive naming
- Add comments only for complex logic that isn't obvious from the code itself
- Document error handling behavior with XML comments, especially `<exception>` tags for thrown exceptions

### Notes:
- Comments that merely repeat what the code already says should be avoided
- XML documentation can be used to generate API documentation

## 8. LINQ

```csharp
// Single line for simple queries
var activeUsers = users.Where(u => u.IsActive).ToList();

// Multi-line for complex queries
var topCustomers = customers
    .Where(c => c.IsActive)
    .OrderByDescending(c => c.TotalSpent)
    .Take(10)
    .Select(c => new CustomerSummary(c))
    .ToList();

// Extract intermediate variables for complex queries
var activeAccounts = accounts.Where(a => a.IsActive);
var highValueAccounts = activeAccounts.Where(a => a.Balance > 100000);
var riskyAccounts = highValueAccounts.Where(a => a.HasRecentSuspiciousActivity);
```

### Principles:
- Use single-line format for simple LINQ queries
- Use multi-line format for complex queries, with dot at start of each line
- Prefer method syntax (Where, Select) over query syntax
- Extract intermediate variables for complex queries or when reusing results

### Notes:
- Breaking complex queries into steps with meaningful variable names improves readability
- Consider performance implications when working with large datasets

## 9. String Handling

```csharp
// Use string interpolation
var greeting = $"Hello, {user.Name}!";
var logMessage = $"User {userId} logged in at {loginTime:yyyy-MM-dd HH:mm}";

// Avoid string concatenation for multiple values
// BAD:
var message = "Hello, " + user.FirstName + " " + user.LastName + "!";

// GOOD:
var message = $"Hello, {user.FirstName} {user.LastName}!";
```

### Principles:
- Use string interpolation (`$"..."`) as the primary string formatting approach
- Avoid string concatenation with + operator when multiple values are involved
- Use appropriate formatting specifiers for dates, numbers, and currency

### Notes:
- String interpolation is more readable and less error-prone than concatenation
- For performance-critical code with many string operations, use StringBuilder

## 10. Expression-Bodied Members

```csharp
// Use for simple property getters
public string FullName => $"{FirstName} {LastName}";
public bool IsAdult => Age >= 18;

// Use for simple methods
public string GetGreeting() => $"Hello, {Name}!";
public decimal GetTotal() => Items.Sum(i => i.Price);

// Avoid for complex logic
// BAD:
public string GetFormattedAddress() => 
    $"{Street}, {City}, {State} {ZipCode}".Trim(',', ' ');

// GOOD:
public string GetFormattedAddress()
{
    return $"{Street}, {City}, {State} {ZipCode}".Trim(',', ' ');
}
```

### Principles:
- Use expression-bodied members for simple property getters and methods
- Prefer traditional block bodies for more complex logic
- Prioritize readability over brevity

### Notes:
- Expression-bodied members can make simple code more concise and readable
- If you find yourself adding line breaks inside an expression-bodied member, it's probably too complex for this syntax

## 11. Null Handling

```csharp
// Nullable annotations make intent clear
public User? FindUser(string username)
{
    return _repository.GetByUsername(username);
}

// Null-conditional for safe navigation
var city = address?.City ?? "Unknown";
var zipCode = order?.ShippingAddress?.ZipCode;

// Null-coalescing for defaults
var displayName = user.Name ?? "Guest";
var sortOrder = request.SortOrder ?? DefaultSortOrder;

// Null-coalescing assignment for lazy initialization
private List<Order> _cachedOrders;
public List<Order> CachedOrders 
{
    get 
    {
        _cachedOrders ??= LoadOrdersFromDatabase();
        return _cachedOrders;
    }
}

// Explicit checks for important validation
public void ProcessOrder(Order order)
{
    if (order == null) throw new ArgumentNullException(nameof(order));
    if (order.Customer == null) throw new ArgumentException("Order must have a customer");
    
    // Process order...
}
```

### Principles:
- Use nullable annotations to make nullability intent clear
- Use null-conditional operator (`?.`) for safe navigation through object chains
- Use null-coalescing operator (`??`) for providing default values
- Use null-coalescing assignment (`??=`) for lazy initialization
- Use explicit null checks for important validation
- Use explicit if/else null checks when logic is complex or when clarity is more important than conciseness

### Notes:
- Return empty collections instead of null to simplify code for callers
- Use pattern matching (`is null`) for more readable null checks in complex conditions

## 12. Asynchronous Programming

```csharp
// Use async/await throughout
public async Task<User> GetUserAsync(int id)
{
    var user = await _repository.GetByIdAsync(id);
    var settings = await _settingsService.GetUserSettingsAsync(id);
    
    user.Settings = settings;
    return user;
}

// Never use ConfigureAwait(false)
// BAD:
var data = await GetDataAsync().ConfigureAwait(false);

// GOOD:
var data = await GetDataAsync();
```

### Principles:
- Use async/await consistently throughout your codebase
- Return Task or Task<T> from async methods, not void (except for event handlers)
- Name async methods with the "Async" suffix
- Never use ConfigureAwait(false) in application code

### Notes:
- Avoid async void methods except for event handlers, as exceptions in async void methods can crash the application
- In application code, ConfigureAwait(false) introduces inconsistent execution context which complicates debugging and can lead to subtle bugs. It's primarily useful in library code
- Async methods propagate exceptions when awaited, so handle them appropriately
- Consider adding cancellation support for long-running operations

## 13. Static Methods and Classes

```csharp
// Use static class for utility classes with only static methods
public static class StringHelpers
{
    public static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }
    
    public static string Slugify(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        
        var slug = value.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("&", "and");
            
        return slug;
    }
}

// BAD: Non-static helper class
public class FileHelpers
{
    public static string ReadAllText(string path)
    {
        return File.ReadAllText(path);
    }
}

// GOOD: Static helper class
public static class FileHelpers
{
    public static string ReadAllText(string path)
    {
        return File.ReadAllText(path);
    }
}
```

### Principles:
- Use static classes for utility/helper methods with no instance state
- Make utility classes explicitly static
- Name utility classes with clear descriptive names, often ending in "Helpers" or "Utils"
- Use static methods within instance classes when they don't rely on instance state

### Notes:
- Static classes cannot be instantiated or inherited
- Static methods can improve performance slightly as they avoid virtual method dispatch

## 14. Parameters

```csharp
// Use nullable reference types for optional parameters
public User CreateUser(string name, string email, string? phoneNumber = null)
{
    var user = new User
    {
        Name = name,
        Email = email
    };
    
    if (phoneNumber != null)
    {
        user.PhoneNumber = phoneNumber;
    }
    
    return user;
}

// Use optional parameters with defaults
public PaginatedResult<Product> GetProducts(
    int page = 1,
    int pageSize = 20,
    string sortBy = "name",
    bool ascending = true)
{
    // ...
}

// Use descriptive names for boolean parameters
// BAD:
SubmitOrder(order, true);

// GOOD:
SubmitOrder(order, sendConfirmationEmail: true);
```

### Principles:
- Use nullable reference types (`string?`) to indicate optional parameters
- Place required parameters before optional parameters
- Use descriptive parameter names that indicate purpose
- Use named arguments when calling methods with boolean parameters

### Notes:
- Set sensible defaults for optional parameters that work for most cases
- Boolean parameters should default to false for safer behavior
- For AI-callable functions (methods with Description attributes in FunctionCallingTools), prefer direct, concise parameter names over verbose ones. For example, `force` is better than `shouldForceKill` as it's clearer for AI consumption and follows common CLI conventions

## 15. Code Organization

```csharp
// Static classes for utilities/helpers at edges
public static class DateHelpers 
{
    public static DateTime StartOfWeek(DateTime date) => 
        date.AddDays(-(int)date.DayOfWeek);
}

// Instance classes for core business logic
public class OrderProcessor 
{
    private readonly IOrderRepository _repository;
    
    public OrderProcessor(IOrderRepository repository)
    {
        _repository = repository;
    }
    
    public void ProcessOrder(Order order) { /* ... */ }
}

// Partial class for generated code separation
// File: Customer.cs
public partial class Customer
{
    // Main implementation here
}

// File: Customer.Generated.cs
public partial class Customer
{
    // Generated properties and methods here
}
```

### Principles:
- Organize code by feature rather than by type
- Keep files focused on a single responsibility
- Use partial classes to separate generated code from hand-written code
- Place static utility classes at application edges
- Place instance classes for core business logic in the middle

### Notes:
- 'Edges' refers to where your code interfaces with external systems, frameworks, or APIs, while 'middle' refers to your core business logic and domain models. The 'top' represents the API surface or the 'head' of the application that users or other systems interact with directly
- A well-organized codebase makes it easier to locate specific functionality
- Use consistent organization patterns across the codebase

## 16. Method Returns

```csharp
// Use early returns with semantic variables to reduce nesting
public ValidationResult ValidateRegistration(RegistrationRequest request)
{
    var requestIsNull = request == null;
    if (requestIsNull) return ValidationResult.Invalid("Request is required");
    
    var emailMissing = string.IsNullOrEmpty(request.Email);
    if (emailMissing) return ValidationResult.Invalid("Email is required");
    
    var passwordMissing = string.IsNullOrEmpty(request.Password);
    if (passwordMissing) return ValidationResult.Invalid("Password is required");
    
    var passwordTooShort = request.Password.Length < 8;
    if (passwordTooShort) return ValidationResult.Invalid("Password too short");
    
    var emailTaken = _userRepository.EmailExists(request.Email);
    if (emailTaken) return ValidationResult.Invalid("Email already registered");
    
    return ValidationResult.Success();
}

// Use ternary for returns - single line for very simple cases
public string GetDisplayName(User user)
{
    return !string.IsNullOrEmpty(user.FirstName) ? user.FirstName : user.Email;
}

// Multi-line format for more complex ternary returns
public string GetFormattedAddress(Address address)
{
    return address != null 
        ? $"{address.Street}, {address.City}, {address.State} {address.ZipCode}"
        : "No address provided";
}
```

### Principles:
- Use early returns to reduce nesting and improve readability
- Use ternary operators for simple conditional return values
- Return empty collections instead of null for collection results
- Use expression-bodied methods for very simple returns

### Notes:
- Early returns make code more readable by reducing indentation levels
- Consistent return types make your APIs more predictable

## 17. Parameter Handling

```csharp
// Use nullable annotations for optional parameters
public void SendNotification(User user, string message, NotificationPriority? priority = null)
{
    var actualPriority = priority ?? NotificationPriority.Normal;
    // ...
}

// Use descriptive names for boolean parameters
// BAD:
SubmitOrder(order, true);

// GOOD:
SubmitOrder(order, sendConfirmationEmail: true);
```

### Principles:
- Validate parameters at the beginning of methods
- Use nullable annotations for parameters that can be null
- Use descriptive names for boolean parameters
- Use named arguments when calling methods with boolean parameters

### Notes:
- Fail fast by validating parameters early in the method
- Consider creating parameter objects for methods with many parameters

## 18. Method Chaining

```csharp
// Format multi-line method chains with the dot at the beginning of each new line
var result = collection
    .Where(x => x.IsActive)
    .Select(x => x.Name)
    .OrderBy(x => x)
    .ToList();

// For builder patterns
var process = new ProcessBuilder()
    .WithFileName("cmd.exe")
    .WithArguments("/c echo Hello")
    .WithTimeout(1000)
    .Build();
```

### Principles:
- Format multi-line method chains with the dot at the beginning of each line
- Place each method call on a separate line for readability
- Use proper indentation for chained method calls
- Use builder pattern with method chaining for complex object construction

### Notes:
- Method chaining can improve readability when used appropriately
- For very long chains, consider extracting intermediate variables

## 19. Resource Cleanup

```csharp
// Simple cases: using declarations (C# 8.0+)
public string ReadFileContent(string path)
{
    using var reader = new StreamReader(path);
    return reader.ReadToEnd();
}

// Complex cases with multiple steps: try/finally
public void ProcessLargeFile(string path)
{
    var stream = new FileStream(path, FileMode.Open);
    try 
    {
        // Multiple processing steps that could fail independently
        // ...
    }
    finally
    {
        stream.Dispose();
    }
}
```

### Principles:
- Use `using` declarations for simple resource cleanup
- Use try/finally blocks for complex cleanup scenarios
- Implement IDisposable for classes that own disposable resources
- Dispose of resources as soon as you're done with them

### Notes:
- Using declarations (C# 8.0+) are more concise than using statements
- Always dispose of resources that implement IDisposable

## 20. Field Initialization

```csharp
// Initialize simple fields at declaration
private int _retryCount = 3;
private readonly List<string> _errorMessages = new List<string>();

// Complex initialization in constructors
public class UserService
{
    private readonly IUserRepository _repository;
    private readonly IValidator<User> _validator;
    private readonly UserSettings _settings;
    
    public UserService(
        IUserRepository repository,
        IValidator<User> validator,
        IOptions<UserSettings> options)
    {
        _repository = repository;
        _validator = validator;
        _settings = options.Value;
    }
}
```

### Principles:
- Initialize simple fields at declaration point
- Perform complex initialization in constructors
- Use readonly for fields that shouldn't change after initialization
- Initialize collections to empty collections rather than null

### Notes:
- Field initialization happens before constructor code runs
- Use null-coalescing assignment (`??=`) for lazy initialization of fields

## 21. Logging Conventions

```csharp
// Include context values, not class/method names
// BAD:
Logger.Info("UserService.CreateUser: User created");

// GOOD:
Logger.Info($"User created: {user.Id} ({user.Email})");

// Include relevant values for debugging
Logger.Debug($"Processing order {order.Id} with {order.Items.Count} items, total: {order.Total:C}");
```

### Principles:
- Include context values that help understand the scenario
- Focus log messages on "what happened" rather than implementation details
- Use appropriate log levels for different types of information
- Don't log sensitive information (passwords, PII, etc.)

### Notes:
- Log at application boundaries (API controllers, background jobs, etc.)
- Use structured logging when available to preserve data semantics

## 22. Class Design and Relationships

```csharp
// Inheritance for "is-a" relationships
public abstract class Document
{
    public string Id { get; set; }
    public string Title { get; set; }
    public abstract string GetDocumentType();
}

public class Invoice : Document
{
    public decimal Amount { get; set; }
    public override string GetDocumentType() => "Invoice";
}

// Composition for "has-a" relationships
public class Order
{
    private readonly IPaymentProcessor _paymentProcessor;
    private readonly IShippingCalculator _shippingCalculator;
    
    public Order(IPaymentProcessor paymentProcessor, IShippingCalculator shippingCalculator)
    {
        _paymentProcessor = paymentProcessor;
        _shippingCalculator = shippingCalculator;
    }
    
    public void Process()
    {
        _paymentProcessor.ProcessPayment(this);
    }
    
    public decimal CalculateShipping()
    {
        return _shippingCalculator.Calculate(this);
    }
}
```

### Principles:
- Use inheritance for "is-a" relationships
- Use composition for "has-a" or "uses-a" relationships
- Prefer composition over inheritance for flexibility
- Keep inheritance hierarchies shallow
- Program to interfaces rather than concrete implementations

### Notes:
- Inheritance creates tight coupling between classes
- Composition makes code more flexible and testable

## 23. Condition Checking Style

```csharp
// Store conditions in descriptive variables 
public bool CanUserEditDocument(User user, Document document)
{
    var isDocumentOwner = document.OwnerId == user.Id;
    var hasEditPermission = user.Permissions.Contains("edit");
    var isAdminUser = user.Role == UserRole.Admin;
    var isDocumentLocked = document.Status == DocumentStatus.Locked;
    
    return (isDocumentOwner || hasEditPermission || isAdminUser) && !isDocumentLocked;
}

// Use semantic variables for clean single-line returns
public string ValidateUserFile(string filePath)
{
    var fileNotFound = !File.Exists(filePath);
    if (fileNotFound) return "File not found";
    
    var hasNoContent = new FileInfo(filePath).Length == 0;
    if (hasNoContent) return "File is empty";
    
    return "File is valid";
}

// Early returns for guard clauses
public void SendNotification(User user, Notification notification)
{
    if (user == null) throw new ArgumentNullException(nameof(user));
    if (notification == null) throw new ArgumentNullException(nameof(notification));
    
    var notifier = _notifierFactory.CreateNotifier(user.PreferredChannel);
    notifier.Send(notification);
    _notificationLog.RecordSent(user.Id, notification.Id);
}
```

### Principles:
- Store condition results in descriptive variables
- Use semantic variables to keep if-statements short and enable single-line returns
- Use early returns with clear variable names for validation
- Prefer positive conditions over negative ones
- Extract complex conditions into well-named methods

### Notes:
- Descriptive variable names make complex boolean logic easier to understand
- Semantic variables create more compact, readable code
- Guard clauses reduce nesting and improve readability

## 24. Builder Patterns and Fluent Interfaces

```csharp
// Return this from builder methods
public class EmailBuilder
{
    private readonly Email _email = new Email();
    
    public EmailBuilder WithSubject(string subject)
    {
        _email.Subject = subject;
        return this;
    }
    
    public EmailBuilder WithBody(string body)
    {
        _email.Body = body;
        return this;
    }
    
    public EmailBuilder WithRecipient(string recipient)
    {
        _email.Recipients.Add(recipient);
        return this;
    }
    
    public Email Build()
    {
        return _email;
    }
}

// Usage
var email = new EmailBuilder()
    .WithSubject("Hello")
    .WithBody("This is a test")
    .WithRecipient("user@example.com")
    .Build();
```

### Principles:
- Return `this` from builder methods to enable method chaining
- Name builder methods with "With" prefix
- Format each method call on a separate line for readability
- End with a Build() or similar method to create the final object

### Notes:
- Builder pattern is useful for objects with many optional parameters
- Fluent interfaces can make code more readable and self-documenting

## 25. Using Directives

```csharp
// Group System namespaces first, then others, alphabetized within groups
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// No namespaces in the codebase - use top-level statements
```

### Principles:
- Group using directives by type (System namespaces first, then others)
- Alphabetize within each group
- Keep a blank line between different import groups

### Notes:
- Well-organized using directives improve code readability
- Consider using global using directives (C# 10+) for commonly used imports

## 26. Default Values and Constants

```csharp
// Use explicit defaults
var name = userName ?? "Anonymous";
var count = requestedCount > 0 ? requestedCount : 10;

// Use named constants for magic numbers
private const int MaxRetryAttempts = 3;
private const double StandardDiscountRate = 0.1;
private const string ApiEndpoint = "https://api.example.com/v2";

// Boolean parameters default to false (safer)
public void ProcessOrder(Order order, bool sendConfirmation = false)
{
    // ...
}
```

### Principles:
- Use named constants for magic numbers and repeated values
- Use explicit defaults instead of relying on default values
- Choose safer defaults for boolean parameters (usually false)
- Use sensible defaults that work for most cases

### Notes:
- Constants improve code readability and maintainability
- Constants are compiled into the code that uses them

## 27. Extension Methods

```csharp
// Use only when providing significant readability benefits
public static class StringExtensions
{
    public static bool IsValidEmail(this string email)
    {
        return !string.IsNullOrEmpty(email) && email.Contains("@") && email.Contains(".");
    }
    
    public static string Truncate(this string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }
}

// Usage
if (user.Email.IsValidEmail())
{
    // Process valid email
}
```

### Principles:
- Use extension methods only when they provide significant readability benefits
- Keep extension methods in a dedicated static class with naming pattern `[Type]Extensions`
- Use extension methods for fluent APIs and operations that conceptually belong to the type
- Make methods discoverable with logical naming

### Notes:
- Extension methods are syntactic sugar; they're still static methods under the hood
- Consider static helper methods instead when the operation doesn't conceptually belong to the type

## 28. Attributes

```csharp
// Class attributes on separate lines
[Serializable]
[ApiController]
public class ProductController
{
    // Property attributes on same line as property
    [Required] public string Name { get; set; }
    
    // Method with parameter attributes
    public IActionResult Get([FromQuery] int id)
    {
        // ...
    }
}
```

### Principles:
- Place class and method attributes on separate lines before the declaration
- Place property attributes on the same line as the property for simple cases
- Use one attribute per line for multiple attributes
- Place parameter attributes immediately before the parameter

### Notes:
- Attributes provide metadata about code elements
- Common attributes include Serializable, Required, and ApiController

## 29. Generics

```csharp
// Use constraints when needed
public class Repository<T> where T : class, IEntity, new()
{
    public T GetById(int id)
    {
        // ...
    }
    
    public void Save(T entity)
    {
        // ...
    }
}

// Use descriptive names for complex cases
public interface IConverter<TSource, TDestination>
{
    TDestination Convert(TSource source);
}

// Single letter parameters for simple cases
public class Cache<T>
{
    private readonly Dictionary<string, T> _items = new Dictionary<string, T>();
    
    public void Add(string key, T value)
    {
        _items[key] = value;
    }
    
    public T Get(string key)
    {
        return _items.TryGetValue(key, out var value) ? value : default;
    }
}
```

### Principles:
- Use constraints to enforce requirements on type parameters
- Use descriptive names (TSource, TDestination) for complex generic relationships
- Use single-letter type parameters (T, K, V) for simple cases
- Apply meaningful constraints to improve type safety

### Notes:
- Generic type constraints help catch errors at compile time
- Common constraints include `where T : class`, `where T : struct`, and `where T : new()`

## 30. Project Organization

```csharp
// Group files by feature/functionality
// Example project structure:
//
// /Customers
//   CustomerController.cs
//   CustomerService.cs
//   CustomerRepository.cs
//   CustomerValidator.cs
//   Models/
//     Customer.cs
//     CustomerViewModel.cs
//
// /Orders
//   OrderController.cs
//   OrderService.cs
//   OrderRepository.cs
//   Models/
//     Order.cs
//     OrderViewModel.cs
```

### Principles:
- Group files by feature/functionality rather than by type
- Keep related files together in the same directory
- Use descriptive directory names that reflect contained functionality
- Follow a consistent organization pattern across the codebase
- Organize code at all levels: group files into meaningful subdirectories within projects, and keep project dependencies clean and well-defined. Directory structure should reflect functional organization rather than arbitrary categories

### Notes:
- Feature-based organization makes it easier to find related code
- Vertical slices (by feature) are more maintainable than horizontal slices (by type)