# 2. Method and Property Declarations

## Core Principles

### Method Naming: Start with Verbs, Use PascalCase
Methods should clearly communicate what action they perform. Like a restaurant menu that lists "Prepare Caesar Salad" rather than just "Caesar Salad," method names should start with action verbs.

```csharp
// GOOD: Clear action verbs in PascalCase
public User GetUserById(int id) { return _repository.Find(id); }
public void ProcessPayment(Payment payment) { _processor.Process(payment); }
public bool ValidateEmailFormat(string email) { return email.Contains("@"); }

// BAD: Missing verbs or unclear actions
public User UserById(int id) { return _repository.Find(id); }      // Missing verb
public void Payment(Payment payment) { _processor.Process(payment); }  // Unclear action
public bool EmailFormat(string email) { return email.Contains("@"); }   // Missing action
```

### Boolean Members: Use Is/Has/Can Prefixes
Boolean properties and methods should clearly indicate they return yes/no answers, like asking "Is the restaurant open?" rather than "Restaurant status?"

```csharp
// GOOD: Clear yes/no questions with appropriate prefixes
public bool IsActive { get; set; }
public bool HasPermission(string permission) { return _permissions.Contains(permission); }
public bool CanUserEditDocument(User user, Document doc) { return user.Id == doc.OwnerId; }

// BAD: Unclear boolean purpose
public bool Active { get; set; }                    // Unclear if this is a status or action
public bool Permission(string permission) { ... }   // Doesn't indicate yes/no nature
public bool UserEditDocument(User user, Document doc) { ... }  // Confusing purpose
```

### Property Design: Auto-Properties vs. Backing Fields
Use auto-properties for simple data storage, but implement backing fields when you need custom logic - like how a restaurant displays basic information simply but has complex processes for handling reservations.

```csharp
// GOOD: Auto-properties for simple cases
public string Name { get; set; }
public DateTime CreatedAt { get; set; }
public int MaxCapacity { get; set; }

// GOOD: Backing fields when custom logic is needed
private string _email;
public string Email 
{
    get => _email;
    set 
    {
        ValidateEmailFormat(value);  // Custom validation logic
        _email = value;
    }
}

// BAD: Unnecessary backing fields for simple properties
private string _name;
public string Name 
{
    get => _name;
    set => _name = value;  // No custom logic - just use auto-property
}
```

### Method Focus: Short and Single Responsibility
Keep methods focused on one specific task, like a chef who specializes in preparing one dish well rather than trying to cook everything at once.

```csharp
// GOOD: Focused method with single responsibility
public decimal CalculateDiscount(Order order) 
{
    if (order == null) return 0;
    if (!order.Items.Any()) return 0;
    
    var subtotal = order.Items.Sum(i => i.Price);
    var discountRate = DetermineDiscountRate(order);
    
    return Math.Round(subtotal * discountRate, 2);
}

// BAD: Method trying to do too many things
public string ProcessCompleteOrder(Order order)
{
    // Validating, calculating, updating database, sending emails - too many responsibilities
    if (order == null) throw new ArgumentNullException();
    var total = order.Items.Sum(i => i.Price);
    var discount = CalculateDiscount(order);
    total -= discount;
    _database.SaveOrder(order);
    _emailService.SendConfirmation(order.Customer.Email);
    _inventoryService.UpdateStock(order.Items);
    return $"Order processed: {total:C}";
}
```

### API Design: Caller's Perspective
Design your methods thinking about how they'll be used, not how they're implemented internally. Like a restaurant menu designed for diners, not kitchen staff.

```csharp
// GOOD: Method designed from caller's perspective
public OrderResult PlaceOrder(Customer customer, List<MenuItem> items)
{
    // Clear what the caller gets and what they need to provide
    var order = new Order(customer, items, DateTime.Now);
    ProcessOrder(order);
    return new OrderResult(order.Id, order.EstimatedTime);
}

// BAD: Method designed from implementer's perspective
public void InternalOrderProcessingWorkflow(object[] parameters)
{
    // Unclear what caller should pass or what they'll receive
    var customer = (Customer)parameters[0];
    var items = (List<MenuItem>)parameters[1];
    // ...
}
```

## Examples

```csharp
// Method naming follows verb-first pattern for clear actions
public Order CreateOrder(Customer customer, List<Product> items)
{
    return new Order(customer, items, DateTime.Now);
}

public void UpdateInventory(Product product, int quantity)
{
    product.StockLevel += quantity;
    _database.SaveChanges();
}

public bool ValidateCustomer(Customer customer)
{
    return customer != null && !string.IsNullOrEmpty(customer.Email);
}

// Boolean properties clearly indicate yes/no information
public bool IsProcessing { get; private set; }
public bool HasValidLicense { get; set; }
public bool CanProcessRefunds => _permissions.Contains("refund");

// Auto-properties for simple data without custom logic
public string ProductName { get; set; }
public decimal Price { get; set; }
public DateTime LastUpdated { get; set; }

// Backing fields only when custom logic is required
private int _stockLevel;
public int StockLevel
{
    get => _stockLevel;
    set
    {
        if (value < 0)
            throw new ArgumentException("Stock level cannot be negative");
        
        _stockLevel = value;
        NotifyLowStockIfNeeded();
    }
}

// Methods designed from caller's perspective
public SearchResult FindProducts(string searchTerm, int maxResults = 10)
{
    // Clear parameters and return type from user's viewpoint
    var products = _repository.Search(searchTerm).Take(maxResults);
    return new SearchResult(products, searchTerm);
}

public PaymentResult ProcessPayment(Order order, PaymentMethod method)
{
    // Focused responsibility: just payment processing
    var result = _paymentGateway.Charge(order.Total, method);
    return new PaymentResult(result.TransactionId, result.Success);
}

// Short, focused helper methods that do one thing well
private decimal CalculateShippingCost(Order order)
{
    var baseRate = 5.99m;
    var weightRate = order.TotalWeight * 0.50m;
    return baseRate + weightRate;
}

private bool IsEligibleForFreeShipping(Order order)
{
    return order.Total > 50.00m || order.Customer.IsPremiumMember;
}
```

### Why It Matters

Well-designed methods and properties make your code as easy to use as a well-organized restaurant menu. Just as restaurant guests should immediately understand what dishes are available and what they'll receive, other developers should immediately understand what your methods do and how to use them.

Consider the difference between these two restaurant experiences:

**Confusing Restaurant**: Menu items listed as "Kitchen Process A," "Food Handler B," and "Thing C." Staff can't explain what dishes contain or how long they take to prepare.

**Well-Designed Restaurant**: Menu clearly lists "Grilled Salmon with Lemon" and "Caesar Salad with Chicken." Staff can explain ingredients, preparation time, and dietary information.

Your code should aim for the well-designed restaurant experience:

1. **Clear Communication**: Method names immediately convey what action will be performed
2. **Predictable Results**: Return types and parameter requirements are obvious from the method signature
3. **Focused Services**: Each method has a clear, single responsibility
4. **User-Friendly Design**: APIs are designed for the convenience of callers, not implementers

### Common Mistakes

#### Unclear Method Naming

```csharp
// BAD: Missing verbs or unclear actions
public string Handle(Order order) { ... }           // Handle how?
public bool Check(User user) { ... }               // Check what?
public object Process(List<Item> items) { ... }    // Process into what?
```

Like a restaurant menu listing "Food Item A" and "Kitchen Thing B" - callers can't understand what service they're requesting.

**Better approach**:
```csharp
// GOOD: Clear verb-first naming
public string FormatOrderSummary(Order order) { ... }
public bool ValidateUserCredentials(User user) { ... }
public ShippingQuote CalculateShippingCosts(List<Item> items) { ... }
```

#### Overloaded Methods (Doing Too Much)

```csharp
// BAD: Method trying to handle everything
public string HandleUserAction(User user, string action, object data)
{
    if (action == "login") return ProcessLogin(user, (LoginData)data);
    if (action == "logout") return ProcessLogout(user);
    if (action == "update") return ProcessUpdate(user, (UpdateData)data);
    if (action == "delete") return ProcessDelete(user);
    // ... many more responsibilities
}
```

Like a restaurant where one server takes orders, cooks food, processes payments, and cleans tables - too many responsibilities create confusion and errors.

**Better approach**:
```csharp
// GOOD: Focused methods with single responsibilities
public LoginResult ProcessUserLogin(User user, LoginCredentials credentials) { ... }
public void ProcessUserLogout(User user) { ... }
public UpdateResult UpdateUserProfile(User user, ProfileData data) { ... }
public void DeleteUserAccount(User user) { ... }
```

#### Inappropriate Property Implementation

```csharp
// BAD: Complex logic in auto-property (impossible)
public string Email { get; set; } // But we need validation!

// BAD: Unnecessary backing field for simple property
private string _name;
public string Name 
{
    get => _name;
    set => _name = value;  // No custom logic needed
}

// BAD: Missing validation where it's needed
public int Age { get; set; }  // Should validate age is reasonable
```

**Better approach**:
```csharp
// GOOD: Auto-property for simple cases
public string Name { get; set; }

// GOOD: Backing field when custom logic is needed
private string _email;
public string Email
{
    get => _email;
    set
    {
        if (string.IsNullOrEmpty(value) || !value.Contains("@"))
            throw new ArgumentException("Invalid email format");
        _email = value;
    }
}

// GOOD: Validation for properties that need it
private int _age;
public int Age
{
    get => _age;
    set
    {
        if (value < 0 || value > 150)
            throw new ArgumentException("Age must be between 0 and 150");
        _age = value;
    }
}
```

### Evolution Example

Let's see how method and property design evolves from confusing to clear:

**Initial Version - Confusing design:**
```csharp
public class Order
{
    public string data;
    public bool flag;
    
    public object Do(object input) { return Process(input); }
    public string Handle(string type, object info) { ... }
}
```

**Intermediate Version - Better but inconsistent:**
```csharp
public class Order
{
    public string OrderData { get; set; }      // Better naming
    public bool IsProcessed { get; set; }      // Clear boolean
    
    public OrderResult ProcessOrder() { ... }   // Clear action
    public string GetOrderSummary() { ... }     // Good verb-first naming
    public void HandlePayment(Payment payment) { ... }  // Still generic "handle"
}
```

**Final Version - Clear, focused design:**
```csharp
public class Order
{
    // Auto-properties for simple data
    public string OrderId { get; set; }
    public DateTime CreatedDate { get; set; }
    public List<OrderItem> Items { get; set; }
    
    // Clear boolean properties
    public bool IsCompleted { get; private set; }
    public bool HasShippingAddress => !string.IsNullOrEmpty(ShippingAddress);
    public bool CanBeModified => !IsCompleted && !IsProcessing;
    
    // Backing field when validation is needed
    private string _customerEmail;
    public string CustomerEmail
    {
        get => _customerEmail;
        set
        {
            if (string.IsNullOrEmpty(value) || !value.Contains("@"))
                throw new ArgumentException("Valid email required");
            _customerEmail = value;
        }
    }
    
    // Focused methods with clear responsibilities
    public OrderResult ProcessPayment(PaymentMethod method)
    {
        if (IsCompleted) throw new InvalidOperationException("Order already completed");
        
        var result = _paymentProcessor.ProcessPayment(CalculateTotal(), method);
        if (result.Success)
        {
            IsCompleted = true;
        }
        return new OrderResult(result.TransactionId, result.Success);
    }
    
    public decimal CalculateTotal()
    {
        var subtotal = Items.Sum(item => item.Price * item.Quantity);
        var tax = subtotal * 0.08m;
        var shipping = CalculateShipping();
        return subtotal + tax + shipping;
    }
    
    public bool ValidateForProcessing()
    {
        return Items.Any() && 
               !string.IsNullOrEmpty(CustomerEmail) && 
               !string.IsNullOrEmpty(ShippingAddress);
    }
    
    // Private helper methods for focused responsibilities
    private decimal CalculateShipping()
    {
        if (CalculateTotal() > 50) return 0;
        return 5.99m;
    }
}
```

### Deeper Understanding

#### The Restaurant Menu Principle

Think of your class's public methods as a restaurant menu - they should be designed for the customer (caller), not the kitchen staff (implementer). Each menu item should:

1. **Clearly describe what you'll receive** (return type and method name)
2. **List required ingredients** (parameters)
3. **Be focused on one dish** (single responsibility)
4. **Use familiar language** (conventional naming patterns)

#### When to Use Auto-Properties vs. Backing Fields

**Use auto-properties when:**
- You're storing simple data without validation
- No custom logic is needed on get or set
- The property represents straightforward information

**Use backing fields when:**
- You need to validate values before storing them
- You need to trigger other actions when the property changes
- You need to transform data on get or set
- You need custom access control (like private set logic)

#### Method Length and Responsibility

The guideline "keep methods short (<20 lines)" isn't about arbitrary line counting - it's about focused responsibility. A method that does one thing well naturally tends to be short. If a method is getting long, it's often because it's trying to do too many things.

Good indicators of focused methods:
- The method name accurately describes everything the method does
- You can explain the method's purpose in one sentence
- The method has a single reason to change
- Testing the method requires only one set of test cases for the main flow