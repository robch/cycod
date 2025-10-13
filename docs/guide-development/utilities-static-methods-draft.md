# 13. Static Methods and Classes

```csharp
// Public utility companies (static classes) provide shared services to everyone
public static class StringUtilities
{
    // Utility services available to all without needing personal equipment
    public static string RemoveSpecialCharacters(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return Regex.Replace(input, @"[^\w\s]", "");
    }
    
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrEmpty(email)) return false;
        return email.Contains("@") && email.Contains(".");
    }
    
    public static string Truncate(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text)) return text;
        return text.Length <= maxLength ? text : text.Substring(0, maxLength) + "...";
    }
}

public static class MathUtilities
{
    // Mathematical utility services available to all
    public static decimal CalculatePercentage(decimal value, decimal total)
    {
        return total == 0 ? 0 : (value / total) * 100;
    }
    
    public static double ConvertCelsiusToFahrenheit(double celsius)
    {
        return (celsius * 9.0 / 5.0) + 32.0;
    }
    
    public static bool IsEven(int number)
    {
        return number % 2 == 0;
    }
}

public static class FileUtilities
{
    // File handling utility services for common operations
    public static string GetSafeFileName(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return "unnamed";
        
        var invalidChars = Path.GetInvalidFileNameChars();
        return invalidChars.Aggregate(fileName, (current, c) => current.Replace(c, '_'));
    }
    
    public static long GetFileSizeInMB(string filePath)
    {
        var fileInfo = new FileInfo(filePath);
        return fileInfo.Length / (1024 * 1024);
    }
    
    public static bool IsImageFile(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLower();
        return new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" }.Contains(extension);
    }
}

// Individual households (instance classes) with their own private systems
public class CustomerAccount
{
    // Private household systems and data
    private readonly string _accountNumber;
    private decimal _currentBalance;
    private readonly List<Transaction> _transactionHistory;
    
    public CustomerAccount(string accountNumber)
    {
        _accountNumber = accountNumber;
        _currentBalance = 0;
        _transactionHistory = new List<Transaction>();
    }
    
    // Household services that depend on this specific account's data
    public void MakePayment(decimal amount)
    {
        _currentBalance -= amount;
        _transactionHistory.Add(new Transaction("Payment", amount, DateTime.Now));
    }
    
    public decimal GetCurrentBalance()
    {
        return _currentBalance;
    }
    
    // Using public utility services within the household
    public string GetFormattedAccountNumber()
    {
        // Call public utility service - no household equipment needed
        return StringUtilities.Truncate(_accountNumber, 10);
    }
    
    public string GetSafeFileName()
    {
        // Use shared file utility service
        return FileUtilities.GetSafeFileName($"account_{_accountNumber}_statement");
    }
    
    // Household utility function that could be shared (static method in instance class)
    public static bool IsValidAccountNumber(string accountNumber)
    {
        // This validation logic doesn't depend on any specific household
        // Could be used by any household or even the utility company
        return !string.IsNullOrEmpty(accountNumber) && 
               accountNumber.Length == 10 && 
               accountNumber.All(char.IsDigit);
    }
}

// BAD: Creating personal equipment for simple utility services
public class BadStringProcessor
{
    // This should be a public utility, not personal equipment
    private string _someInternalState; // No instance state needed for string utilities
    
    public string RemoveSpaces(string input)
    {
        // This doesn't use any instance state - should be a utility service
        return input?.Replace(" ", "") ?? "";
    }
    
    public bool IsEmpty(string input)
    {
        // Pure utility function disguised as personal equipment
        return string.IsNullOrEmpty(input);
    }
}

// GOOD: Public utility service for shared string operations
public static class StringUtilities
{
    // Utility services that everyone can use without personal equipment
    public static string RemoveSpaces(string input)
    {
        return input?.Replace(" ", "") ?? "";
    }
    
    public static bool IsEmpty(string input)
    {
        return string.IsNullOrEmpty(input);
    }
}

// Using public utilities vs. personal equipment appropriately
public class DocumentProcessor
{
    // Personal equipment - document processor has its own state and configuration
    private readonly DocumentSettings _settings;
    private readonly List<ProcessedDocument> _processedDocuments;
    
    public DocumentProcessor(DocumentSettings settings)
    {
        _settings = settings;
        _processedDocuments = new List<ProcessedDocument>();
    }
    
    // Personal service that depends on this processor's configuration
    public ProcessedDocument ProcessDocument(string content)
    {
        // Use personal equipment settings
        var processed = ApplyProcessingRules(content, _settings);
        _processedDocuments.Add(processed);
        return processed;
    }
    
    // Using public utility services within personal operations
    public ProcessedDocument ProcessDocumentSafely(string content, string fileName)
    {
        // Use public utility service for file name safety
        var safeFileName = FileUtilities.GetSafeFileName(fileName);
        
        // Use public utility service for text cleaning
        var cleanContent = StringUtilities.RemoveSpecialCharacters(content);
        
        // Use personal equipment for document processing
        return ProcessDocument(cleanContent);
    }
    
    // Household utility that could be shared (static method in instance class)
    public static bool IsValidDocumentFormat(string fileExtension)
    {
        // This validation doesn't depend on any specific processor instance
        var validExtensions = new[] { ".txt", ".doc", ".docx", ".pdf" };
        return validExtensions.Contains(fileExtension.ToLower());
    }
}

// Public calculator utility (static class) for mathematical services
public static class CalculationUtilities
{
    // Shared calculation services available to all
    public static decimal CalculateCompoundInterest(decimal principal, decimal rate, int periods)
    {
        return principal * (decimal)Math.Pow((double)(1 + rate), periods);
    }
    
    public static double CalculateDistance(Point point1, Point point2)
    {
        var deltaX = point2.X - point1.X;
        var deltaY = point2.Y - point1.Y;
        return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
    }
    
    public static TimeSpan CalculateWorkingHours(DateTime startTime, DateTime endTime)
    {
        return endTime - startTime;
    }
}
```

### Core Principles

- Create public utility companies (static classes) for services that everyone can use without personal equipment or individual account state
- Name utility companies clearly to indicate their service area (StringUtilities, FileUtilities, MathUtilities)
- Use shared utility services (static methods) when the operation doesn't depend on any specific household's (instance's) private data
- Keep personal equipment (instance classes) for operations that require individual configuration, state, or account information
- Call public utility services from within household operations when appropriate
- Consider making household utilities into public services if they could benefit everyone

### Why It Matters

Think of static methods and classes as public utility services - electricity, water, gas, and internet services that are shared infrastructure available to all households without requiring personal ownership of power plants or water treatment facilities.

Just as it would be wasteful for every household to build their own power plant for basic electricity needs, it's wasteful to create instance classes when static utility services would serve the same purpose more efficiently.

Public utility systems provide:

1. **Shared Infrastructure**: Services available to all without personal investment in equipment
2. **Efficient Resource Use**: One power plant serves many households instead of each building their own
3. **Standardized Services**: Consistent, reliable service quality across all users
4. **Easy Access**: Simple connection to utility services without complex personal equipment
5. **Cost Effectiveness**: Shared costs are lower than individual ownership for common services

When static classes are used inappropriately, it's like forcing everyone to build personal power plants for simple electricity needs, or like making public utilities into private household equipment that can't be shared.

### Common Mistakes

#### Building Personal Power Plants for Simple Utility Needs

```csharp
// BAD: Creating personal equipment when public utilities would work better
public class PersonalStringProcessor
{
    private string _lastProcessedString; // Unnecessary personal state for utility functions
    
    public string TrimWhitespace(string input)
    {
        // This doesn't need personal equipment - should be a public utility
        _lastProcessedString = input; // Storing state that's never used
        return input?.Trim() ?? "";
    }
    
    public bool IsEmailValid(string email)
    {
        // Pure utility function disguised as personal equipment
        return !string.IsNullOrEmpty(email) && email.Contains("@");
    }
}

// Usage requires creating personal equipment for simple utility needs
var processor = new PersonalStringProcessor(); // Unnecessary personal equipment
var result = processor.TrimWhitespace(userInput); // Should be utility service
```

**Why it's problematic**: This is like every household building their own personal power plant just to charge their phones. The string processing functions don't need any personal state, so creating instance objects is wasteful and confusing.

**Better approach**:

```csharp
// GOOD: Public utility service for shared string operations
public static class StringUtilities
{
    public static string TrimWhitespace(string input)
    {
        return input?.Trim() ?? "";
    }
    
    public static bool IsEmailValid(string email)
    {
        return !string.IsNullOrEmpty(email) && email.Contains("@");
    }
}

// Usage connects directly to public utility
var result = StringUtilities.TrimWhitespace(userInput); // Direct utility connection
```

#### Making Public Utilities Into Personal Equipment

```csharp
// BAD: Making utility services require personal equipment when they should be public
public class MathCalculator
{
    // These should be public utility services, not personal equipment
    public static double CalculateCircleArea(double radius)
    {
        return Math.PI * radius * radius; // Pure calculation - should be utility service
    }
    
    public static int GetMaxValue(int a, int b)
    {
        return Math.Max(a, b); // Basic comparison - should be utility service
    }
}
```

**Why it's problematic**: This is like requiring everyone to buy personal mathematical equipment when these could be shared public calculation services. The class can't be instantiated but looks like personal equipment rather than a public utility.

**Better approach**:

```csharp
// GOOD: Explicit public utility service
public static class MathUtilities
{
    public static double CalculateCircleArea(double radius)
    {
        return Math.PI * radius * radius;
    }
    
    public static int GetMaxValue(int a, int b)
    {
        return Math.Max(a, b);
    }
}
```

#### Using Public Utilities for Personal Equipment Operations

```csharp
// BAD: Making instance operations into public utilities when they need personal state
public static class CustomerService
{
    // This should be personal equipment, not public utility
    public static void ProcessCustomerOrder(Customer customer, Order order)
    {
        // Where does the customer database connection come from?
        // Where are customer preferences stored?
        // This needs personal equipment (instance state) to work properly
    }
    
    public static decimal CalculateCustomerDiscount(Customer customer)
    {
        // Needs access to customer database, pricing rules, etc.
        // Should be instance method with proper household equipment
    }
}
```

**Why it's problematic**: This is like trying to make personal household services into public utilities. Customer processing requires access to customer databases, personal preferences, and account-specific information that should be part of personal equipment (instance classes).

**Better approach**:

```csharp
// GOOD: Personal equipment for operations that require household state
public class CustomerService
{
    private readonly ICustomerRepository _customerDatabase;  // Personal household equipment
    private readonly IPricingEngine _pricingEngine;          // Personal pricing equipment
    
    public CustomerService(ICustomerRepository customerDb, IPricingEngine pricing)
    {
        _customerDatabase = customerDb;
        _pricingEngine = pricing;
    }
    
    // Personal services that use household equipment
    public void ProcessCustomerOrder(Customer customer, Order order)
    {
        // Uses personal equipment (database connections, configuration)
        var customerAccount = _customerDatabase.GetAccount(customer.Id);
        var pricingRules = _pricingEngine.GetRulesForCustomer(customer);
        
        // Process using personal equipment
    }
    
    // Utility functions that could be public services (static methods in instance class)
    public static bool IsValidCustomerId(string customerId)
    {
        // This validation doesn't need any personal equipment
        return !string.IsNullOrEmpty(customerId) && customerId.Length == 10;
    }
}
```

#### Poor Utility Company Naming

```csharp
// BAD: Unclear utility company names that don't indicate their service area
public static class Helpers   // What kind of help? What services?
{
    public static string CleanText(string input) { /* ... */ }
    public static bool ValidateEmail(string email) { /* ... */ }
    public static decimal CalculateTax(decimal amount) { /* ... */ }
}

public static class Utils     // Vague utility company name
{
    public static DateTime ParseDate(string dateString) { /* ... */ }
    public static string FormatCurrency(decimal amount) { /* ... */ }
}
```

**Why it's problematic**: This is like having utility companies named "Service Company" or "Utilities Inc." without indicating whether they provide electricity, water, or gas. Users don't know what services are available or which utility company to call.

**Better approach**:

```csharp
// GOOD: Clear utility company names that indicate service areas
public static class StringUtilities
{
    public static string RemoveSpecialCharacters(string input) { /* ... */ }
    public static bool IsValidEmail(string email) { /* ... */ }
    public static string FormatForDisplay(string text) { /* ... */ }
}

public static class DateTimeUtilities
{
    public static DateTime ParseFlexibleFormat(string dateString) { /* ... */ }
    public static string FormatUserFriendly(DateTime date) { /* ... */ }
    public static bool IsBusinessDay(DateTime date) { /* ... */ }
}

public static class CurrencyUtilities
{
    public static string FormatAsCurrency(decimal amount) { /* ... */ }
    public static decimal CalculateTax(decimal amount, decimal rate) { /* ... */ }
    public static decimal ConvertCurrency(decimal amount, string fromCurrency, string toCurrency) { /* ... */ }
}
```

### Evolution Example

Let's see how static class usage might evolve from inefficient personal equipment to well-organized public utilities:

**Initial Version - Everyone builds personal equipment for basic services:**

```csharp
// Initial version - every household building personal power plants
public class CustomerProcessor
{
    public string CleanText(string input)
    {
        // Personal text cleaning equipment in every household
        return input?.Trim().Replace("  ", " ") ?? "";
    }
    
    public bool IsEmailValid(string email)
    {
        // Personal email validation equipment in every household
        return !string.IsNullOrEmpty(email) && email.Contains("@");
    }
    
    public decimal CalculatePercentage(decimal value, decimal total)
    {
        // Personal calculator in every household
        return total == 0 ? 0 : (value / total) * 100;
    }
}

public class OrderProcessor
{
    public string CleanText(string input)
    {
        // Duplicate personal equipment - same text cleaning as above
        return input?.Trim().Replace("  ", " ") ?? "";
    }
    
    public decimal CalculatePercentage(decimal value, decimal total)
    {
        // Duplicate personal calculator equipment
        return total == 0 ? 0 : (value / total) * 100;
    }
}
```

**Intermediate Version - Some utilities but inconsistent service distribution:**

```csharp
// Better but still mixing personal equipment with utility services
public static class Utilities // Vague utility company name
{
    public static string CleanText(string input)
    {
        return input?.Trim().Replace("  ", " ") ?? "";
    }
    
    public static bool IsEmailValid(string email)
    {
        return !string.IsNullOrEmpty(email) && email.Contains("@");
    }
}

public class CustomerProcessor
{
    private readonly ICustomerDatabase _database; // Appropriate personal equipment
    
    public void ProcessCustomer(Customer customer)
    {
        // Mix of personal equipment and public utilities
        var cleanName = Utilities.CleanText(customer.Name); // Good - using utility
        var isEmailValid = IsEmailValid(customer.Email);    // Bad - personal method for utility function
        
        // Personal equipment usage
        _database.SaveCustomer(customer);
    }
    
    // Should be public utility service
    private bool IsEmailValid(string email)
    {
        return !string.IsNullOrEmpty(email) && email.Contains("@");
    }
}
```

**Final Version - Well-organized public utilities and appropriate personal equipment:**

```csharp
// Excellent utility service organization
public static class StringUtilities
{
    public static string RemoveExtraWhitespace(string input)
    {
        return input?.Trim().Replace("  ", " ") ?? "";
    }
    
    public static bool IsValidEmail(string email)
    {
        return !string.IsNullOrEmpty(email) && email.Contains("@") && email.Contains(".");
    }
    
    public static string FormatDisplayName(string firstName, string lastName)
    {
        return $"{firstName?.Trim()} {lastName?.Trim()}".Trim();
    }
}

public static class ValidationUtilities
{
    public static bool IsValidPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber)) return false;
        var digitsOnly = new string(phoneNumber.Where(char.IsDigit).ToArray());
        return digitsOnly.Length == 10;
    }
    
    public static bool IsValidPostalCode(string postalCode)
    {
        return !string.IsNullOrEmpty(postalCode) && 
               (postalCode.Length == 5 || postalCode.Length == 9);
    }
}

public class CustomerService
{
    // Personal household equipment for customer-specific operations
    private readonly ICustomerRepository _customerRepository;
    private readonly INotificationService _notificationService;
    private readonly CustomerServiceSettings _settings;
    
    public CustomerService(ICustomerRepository repository, INotificationService notifications, CustomerServiceSettings settings)
    {
        _customerRepository = repository;
        _notificationService = notifications;
        _settings = settings;
    }
    
    // Personal service using both household equipment and public utilities
    public async Task<CustomerRegistrationResult> RegisterCustomerAsync(CustomerRegistration registration)
    {
        // Use public utility services for common operations
        var nameIsValid = !StringUtilities.IsEmpty(registration.FirstName) && 
                         !StringUtilities.IsEmpty(registration.LastName);
        var emailIsValid = StringUtilities.IsValidEmail(registration.Email);
        var phoneIsValid = ValidationUtilities.IsValidPhoneNumber(registration.PhoneNumber);
        
        if (!nameIsValid || !emailIsValid || !phoneIsValid)
        {
            return CustomerRegistrationResult.ValidationFailed("Customer information is invalid");
        }
        
        // Use personal household equipment for customer-specific operations
        var existingCustomer = await _customerRepository.FindByEmailAsync(registration.Email);
        if (existingCustomer != null)
        {
            return CustomerRegistrationResult.AlreadyExists("Customer already registered");
        }
        
        var newCustomer = new Customer
        {
            FirstName = StringUtilities.RemoveExtraWhitespace(registration.FirstName),
            LastName = StringUtilities.RemoveExtraWhitespace(registration.LastName),
            Email = registration.Email.ToLower(),
            PhoneNumber = registration.PhoneNumber,
            RegistrationDate = DateTime.Now
        };
        
        await _customerRepository.SaveAsync(newCustomer);
        await _notificationService.SendWelcomeEmailAsync(newCustomer);
        
        return CustomerRegistrationResult.Success(newCustomer.Id);
    }
    
    // Utility function that could be shared (static method in instance class)
    public static bool IsBusinessHoursRegistration(DateTime registrationTime)
    {
        // This logic doesn't depend on any specific service instance
        var hour = registrationTime.Hour;
        return hour >= 9 && hour <= 17 && registrationTime.DayOfWeek != DayOfWeek.Sunday;
    }
}
```

### Deeper Understanding

#### Public Utility vs. Personal Equipment Design

**Use Public Utilities (Static Classes) When**:
- The operation doesn't require any personal state or configuration
- The functionality is broadly useful across many different contexts
- The operation is a pure function (same inputs always produce same outputs)
- No instance-specific customization is needed

**Use Personal Equipment (Instance Classes) When**:
- The operation requires access to personal state, configuration, or resources
- The functionality needs to be customized per instance
- The operation maintains or modifies instance-specific data
- The class coordinates multiple related operations using shared state

#### Utility Service Organization

**Well-Organized Utility Companies**:
```csharp
public static class StringUtilities    // Clear service area
public static class FileUtilities      // Specific utility type
public static class ValidationUtilities // Focused service category
```

**Poorly Organized Utility Services**:
```csharp
public static class Helpers     // Vague - what kind of help?
public static class Utils       // Generic - what utilities?
public static class CommonCode  // Unclear - what common functionality?
```

#### Hybrid Approach: Personal Equipment with Utility Access

Many well-designed classes combine personal equipment with public utility services:

```csharp
public class OrderProcessor
{
    // Personal equipment
    private readonly IOrderRepository _repository;
    
    // Personal service
    public async Task<Order> ProcessAsync(OrderData data)
    {
        // Use public utilities for common operations
        var isValidEmail = StringUtilities.IsValidEmail(data.CustomerEmail);
        var formattedPhone = StringUtilities.FormatPhoneNumber(data.Phone);
        
        // Use personal equipment for order-specific operations
        var order = await _repository.CreateOrderAsync(data);
        return order;
    }
    
    // Utility function that could be shared
    public static bool IsValidOrderAmount(decimal amount)
    {
        return amount > 0 && amount <= MaxOrderValue;
    }
}
```

This hybrid approach provides the best of both worlds - efficient shared utilities for common operations and appropriate personal equipment for instance-specific functionality.

Good static class design makes your codebase as efficient and well-organized as a modern city with excellent public utilities and appropriate personal equipment in each household.