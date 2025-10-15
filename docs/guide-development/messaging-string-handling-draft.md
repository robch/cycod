# 9. String Handling

```csharp
// Smart messaging (string interpolation) - automatically fills in contact names and info
var welcomeMessage = $"Hello, {user.Name}! You have {user.UnreadMessages} new messages.";
var appointmentReminder = $"Reminder: Your appointment with Dr. {doctor.LastName} is at {appointment.Time:h:mm tt} today.";
var orderConfirmation = $"Order #{order.Id} for ${order.Total:C} has been shipped to {customer.Address}.";

// Professional message formatting with proper display
var eventNotification = $"Event: {eventName} starts at {eventTime:yyyy-MM-dd HH:mm} in {location}";
var priceAlert = $"Stock {symbol} is now ${currentPrice:F2} ({changePercent:+0.0%;-0.0%}% change)";
var fileReport = $"Processed {fileCount:N0} files totaling {totalSize / (1024 * 1024):F1} MB";

// StringBuilder for composing long messages with many edits
var newsletter = new StringBuilder();
newsletter.AppendLine("Welcome to our monthly newsletter!");
newsletter.AppendLine();
newsletter.AppendLine($"This month's featured article: {featuredTitle}");
newsletter.AppendLine($"Written by: {author.Name}");
newsletter.AppendLine();
newsletter.AppendLine("Upcoming events:");
foreach (var eventItem in upcomingEvents)
{
    newsletter.AppendLine($"- {eventItem.Name} on {eventItem.Date:MMM dd}");
}
var finalNewsletter = newsletter.ToString();

// BAD: Manual message assembly (string concatenation) - like typing each piece separately
var badMessage = "Hello, " + user.Name + "! You have " + user.UnreadMessages.ToString() + " new messages.";
var badReport = "Order #" + order.Id.ToString() + " for $" + order.Total.ToString() + " has been shipped.";

// GOOD: Smart messaging automatically handles the formatting
var goodMessage = $"Hello, {user.Name}! You have {user.UnreadMessages} new messages.";
var goodReport = $"Order #{order.Id} for {order.Total:C} has been shipped.";

// Message templates for consistent communication
public string CreateWelcomeMessage(User newUser)
{
    // Smart message template with automatic personalization
    return $"Welcome to {AppName}, {newUser.FirstName}! Your account has been created successfully.";
}

public string FormatErrorMessage(string operation, Exception error)
{
    // Professional error message formatting
    return $"Error during {operation}: {error.Message} (Code: {error.GetType().Name})";
}

// Multi-line message composition using smart formatting
var emailBody = $@"
Dear {customer.Name},

Thank you for your order #{order.OrderNumber} placed on {order.OrderDate:MMMM dd, yyyy}.

Order Details:
- Items: {order.ItemCount}
- Subtotal: {order.Subtotal:C}
- Tax: {order.Tax:C}
- Total: {order.Total:C}

Your order will be shipped to:
{customer.ShippingAddress}

Estimated delivery: {order.EstimatedDelivery:dddd, MMMM dd}

Best regards,
The {CompanyName} Team";

// Message formatting for different audiences
public string FormatUserFriendlyDate(DateTime date)
{
    // Smart date formatting for message recipients
    var timespan = DateTime.Now - date;
    
    return timespan.TotalDays switch
    {
        < 1 => $"{timespan.Hours}h ago",
        < 7 => $"{timespan.Days}d ago", 
        < 30 => $"{timespan.Days / 7:F0}w ago",
        _ => date.ToString("MMM dd, yyyy")
    };
}

// Efficient message building for performance-critical scenarios
public string BuildLargeReport(List<DataPoint> dataPoints)
{
    // Use message builder for lots of text composition
    var reportBuilder = new StringBuilder(dataPoints.Count * 50); // Estimate capacity
    
    reportBuilder.AppendLine("Data Analysis Report");
    reportBuilder.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}");
    reportBuilder.AppendLine($"Total Records: {dataPoints.Count:N0}");
    reportBuilder.AppendLine();
    
    foreach (var point in dataPoints)
    {
        reportBuilder.AppendLine($"{point.Timestamp:HH:mm} - {point.Value:F2} ({point.Status})");
    }
    
    return reportBuilder.ToString();
}

// Message validation and safety
public string CreateSafeDisplayMessage(string userInput, string context)
{
    // Validate message content before displaying
    if (string.IsNullOrWhiteSpace(userInput))
        return $"No {context} provided";
    
    if (userInput.Length > MaxDisplayLength)
        return $"{userInput.Substring(0, MaxDisplayLength)}... (truncated)";
    
    return $"{context}: {userInput}";
}
```

### Core Principles

- Use smart messaging (string interpolation with `$"..."`) as your primary text composition method
- Avoid manual message assembly (string concatenation with `+`) when combining multiple pieces of information
- Use professional formatting templates for dates, numbers, and currency in messages
- Use message builders (StringBuilder) when composing long texts with many edits or performance requirements
- Design message templates that are clear and easy to read for the message recipient
- Apply appropriate formatting based on the message context and audience

### Why It Matters

Think of string handling as composing text messages and communications. Just as modern messaging apps automatically fill in contact names, format dates intelligently, and provide templates for common messages, good string handling makes text composition automatic, readable, and professional.

Poor string handling is like manually typing out each piece of a message separately, or sending messages with inconsistent formatting that confuse recipients.

Professional text composition provides:

1. **Automatic Information Filling**: Smart interpolation inserts values seamlessly into message templates
2. **Consistent Formatting**: Professional appearance with proper number, date, and currency formatting
3. **Readable Code**: Message templates show the final result clearly in the code
4. **Efficient Composition**: Appropriate tools for different message complexity levels
5. **Error Prevention**: Template-based approach reduces typos and formatting mistakes

When string handling is done poorly, it creates messages that look unprofessional, code that's hard to read, and text composition that's error-prone and inefficient.

### Common Mistakes

#### Manual Message Assembly for Complex Communications

```csharp
// BAD: Manually typing each piece of the message separately
var orderMessage = "Hello " + customer.FirstName + " " + customer.LastName + 
                  "! Your order #" + order.Id.ToString() + 
                  " for $" + order.Total.ToString() + 
                  " was placed on " + order.Date.Month.ToString() + "/" + 
                  order.Date.Day.ToString() + "/" + order.Date.Year.ToString() + 
                  " and will be delivered to " + customer.Address + ".";

var reportLine = "User: " + user.Name + " | Last Login: " + user.LastLogin.ToString() + 
                " | Status: " + user.Status + " | Messages: " + user.MessageCount.ToString();
```

**Why it's problematic**: This is like manually typing out each part of a text message instead of using auto-complete and smart formatting. It's error-prone, hard to read, and doesn't handle formatting properly (dates look like "3/15/2024" instead of "March 15, 2024").

**Better approach**:

```csharp
// GOOD: Smart messaging with automatic formatting
var orderMessage = $"Hello {customer.FirstName} {customer.LastName}! Your order #{order.Id} for {order.Total:C} was placed on {order.Date:MMMM dd, yyyy} and will be delivered to {customer.Address}.";

var reportLine = $"User: {user.Name} | Last Login: {user.LastLogin:g} | Status: {user.Status} | Messages: {user.MessageCount}";
```

#### No Professional Message Formatting

```csharp
// BAD: Unprofessional message formatting that confuses recipients
var priceAlert = $"Stock price changed to {price} that's {change} difference";  
var timeMessage = $"Meeting at {meetingTime}";  // Shows "3/15/2024 2:30:00 PM"
var reportSummary = $"Processed {count} items totaling {size} bytes";  // Shows "12847362 bytes"
```

**Why it's problematic**: This is like sending text messages with unclear formatting - recipients can't easily understand the information. Numbers, dates, and amounts need proper formatting for professional communication.

**Better approach**:

```csharp
// GOOD: Professional message formatting for clear communication
var priceAlert = $"Stock price changed to {price:C} (${change:+0.00;-0.00} difference)";
var timeMessage = $"Meeting at {meetingTime:dddd, MMMM dd 'at' h:mm tt}";  // "Monday, March 15 at 2:30 PM"
var reportSummary = $"Processed {count:N0} items totaling {size / (1024 * 1024):F1} MB";  // "1,284 items totaling 12.2 MB"
```

#### Using the Wrong Text Composition Tool

```csharp
// BAD: Using manual message assembly for long, complex content
public string GenerateNewsletterContent(List<Article> articles, List<Event> events)
{
    var newsletter = "Monthly Newsletter\n";
    newsletter += "==================\n\n";
    newsletter += "Featured Articles:\n";
    
    foreach (var article in articles)
    {
        newsletter += "- " + article.Title + " by " + article.Author + "\n";
        newsletter += "  Published: " + article.Date.ToString() + "\n";
        newsletter += "  " + article.Summary + "\n\n";
    }
    
    newsletter += "Upcoming Events:\n";
    foreach (var eventItem in events)
    {
        newsletter += "- " + eventItem.Name + " on " + eventItem.Date.ToString() + "\n";
    }
    
    return newsletter; // Creates many temporary strings - inefficient
}
```

**Why it's problematic**: This is like rewriting an entire text message from scratch every time you want to add one more word. Each concatenation creates a new string, making it inefficient for long content.

**Better approach**:

```csharp
// GOOD: Using message builder for long, complex content composition
public string GenerateNewsletterContent(List<Article> articles, List<Event> events)
{
    var newsletter = new StringBuilder();
    newsletter.AppendLine("Monthly Newsletter");
    newsletter.AppendLine("==================");
    newsletter.AppendLine();
    newsletter.AppendLine("Featured Articles:");
    
    foreach (var article in articles)
    {
        newsletter.AppendLine($"- {article.Title} by {article.Author}");
        newsletter.AppendLine($"  Published: {article.Date:MMMM dd, yyyy}");
        newsletter.AppendLine($"  {article.Summary}");
        newsletter.AppendLine();
    }
    
    newsletter.AppendLine("Upcoming Events:");
    foreach (var eventItem in events)
    {
        newsletter.AppendLine($"- {eventItem.Name} on {eventItem.Date:MMM dd}");
    }
    
    return newsletter.ToString(); // Efficient text building
}
```

#### Inconsistent Message Templates

```csharp
// BAD: Different formatting styles in the same application
public class NotificationService
{
    public string CreateLoginAlert(User user)
    {
        return "User " + user.Name + " logged in at " + DateTime.Now.ToString();
    }
    
    public string CreateLogoutAlert(User user) 
    {
        return $"Logout: {user.Name} @ {DateTime.Now}";
    }
    
    public string CreateErrorAlert(string error)
    {
        return string.Format("ERROR - {0} occurred at {1}", error, DateTime.Now);
    }
}
```

**Why it's problematic**: This is like having inconsistent text message styles in the same conversation - some messages are formal, others are abbreviated, and others use different date formats. It creates a confusing user experience.

**Better approach**:

```csharp
// GOOD: Consistent message templates throughout the application
public class NotificationService
{
    public string CreateLoginAlert(User user)
    {
        return $"User {user.Name} logged in at {DateTime.Now:yyyy-MM-dd HH:mm}";
    }
    
    public string CreateLogoutAlert(User user)
    {
        return $"User {user.Name} logged out at {DateTime.Now:yyyy-MM-dd HH:mm}";
    }
    
    public string CreateErrorAlert(string error)
    {
        return $"Error: {error} occurred at {DateTime.Now:yyyy-MM-dd HH:mm}";
    }
}
```

### Evolution Example

Let's see how string handling might evolve from poor text composition to professional messaging:

**Initial Version - Manual message assembly:**

```csharp
// Initial version - manually typing each piece of every message
public class MessageService
{
    public string CreateWelcomeMessage(User user)
    {
        // Manual assembly like typing each word separately
        string message = "Hello ";
        message = message + user.FirstName;
        message = message + " ";
        message = message + user.LastName;
        message = message + "! Welcome to ";
        message = message + ApplicationName;
        message = message + ". You joined on ";
        message = message + user.JoinDate.Month.ToString();
        message = message + "/";
        message = message + user.JoinDate.Day.ToString();
        message = message + "/"; 
        message = message + user.JoinDate.Year.ToString();
        message = message + ".";
        
        return message;
    }
}
```

**Intermediate Version - Some improvement but inconsistent:**

```csharp
// Better but still mixing different composition styles
public class MessageService
{
    public string CreateWelcomeMessage(User user)
    {
        // Mix of concatenation and interpolation - inconsistent style
        return "Hello " + user.FirstName + " " + user.LastName + 
               $"! Welcome to {ApplicationName}. You joined on " + user.JoinDate.ToString() + ".";
    }
    
    public string CreateOrderMessage(Order order)
    {
        // Using old-style formatting mixed with other approaches
        return string.Format("Order {0} for {1} total", order.Id, order.Total);
    }
}
```

**Final Version - Professional, consistent messaging:**

```csharp
// Excellent string handling with professional message composition
public class MessageService
{
    // Professional message templates with smart formatting
    public string CreateWelcomeMessage(User user)
    {
        return $"Hello {user.FirstName} {user.LastName}! Welcome to {ApplicationName}. You joined on {user.JoinDate:MMMM dd, yyyy}.";
    }
    
    public string CreateOrderConfirmation(Order order)
    {
        return $"Order #{order.Id} confirmed for {order.Total:C}. Estimated delivery: {order.EstimatedDelivery:dddd, MMMM dd}.";
    }
    
    public string CreateStatusUpdate(string operation, TimeSpan duration, int itemCount)
    {
        return $"{operation} completed in {duration.TotalSeconds:F1}s. Processed {itemCount:N0} items.";
    }
    
    // Professional multi-line message composition
    public string CreateDetailedReport(ReportData data)
    {
        return $@"
System Report - {data.Timestamp:yyyy-MM-dd HH:mm}
{'='.Repeat(40)}

Performance Metrics:
- CPU Usage: {data.CpuUsage:P1}
- Memory Usage: {data.MemoryUsage / (1024 * 1024):F1} MB
- Active Users: {data.ActiveUsers:N0}
- Response Time: {data.AverageResponseTime:F0}ms

Status: {data.SystemStatus}
Next Update: {data.NextUpdate:h:mm tt}
";
    }
    
    // Efficient message building for complex content
    public string CreateBulkNotification(List<NotificationItem> items)
    {
        var message = new StringBuilder(items.Count * 100); // Estimate capacity
        
        message.AppendLine($"Bulk Notification - {DateTime.Now:yyyy-MM-dd HH:mm}");
        message.AppendLine();
        
        foreach (var item in items)
        {
            message.AppendLine($"• {item.Title}");
            message.AppendLine($"  {item.Description}");
            message.AppendLine($"  Due: {item.DueDate:MMM dd 'at' h:mm tt}");
            message.AppendLine();
        }
        
        message.AppendLine($"Total items: {items.Count:N0}");
        
        return message.ToString();
    }
    
    // Safe message composition with validation
    public string CreateUserDisplayMessage(string userInput, string category)
    {
        if (string.IsNullOrWhiteSpace(userInput))
            return $"No {category.ToLower()} provided";
            
        var sanitizedInput = userInput.Length > 100 
            ? $"{userInput.Substring(0, 97)}..."
            : userInput;
            
        return $"{category}: {sanitizedInput}";
    }
}

// Extension method for string repetition (used in report formatting)
public static class StringExtensions
{
    public static string Repeat(this char character, int count)
    {
        return new string(character, count);
    }
}
```

### Deeper Understanding

#### Text Composition Tools and Their Uses

1. **Smart Messaging (String Interpolation)**:
   ```csharp
   var message = $"Hello {name}, you have {count} messages";
   ```
   Best for: Most text composition, readable templates, automatic formatting

2. **Message Builder (StringBuilder)**:
   ```csharp
   var builder = new StringBuilder();
   builder.AppendLine($"Line 1: {value1}");
   builder.AppendLine($"Line 2: {value2}");
   ```
   Best for: Long content, many additions, performance-critical scenarios

3. **Manual Assembly (String Concatenation)**:
   ```csharp
   var result = part1 + part2;  // Avoid for multiple parts
   ```
   Best for: Only when combining exactly two strings

#### Professional Message Formatting

**Currency Formatting**:
```csharp
$"Total: {amount:C}"           // $1,234.56
$"Change: {diff:+$0.00;-$0.00;$0.00}"  // +$1.23 or -$1.23
```

**Date and Time Formatting**:
```csharp
$"Meeting: {date:dddd, MMMM dd 'at' h:mm tt}"  // Monday, March 15 at 2:30 PM
$"Log: {timestamp:yyyy-MM-dd HH:mm:ss}"        // 2024-03-15 14:30:45
```

**Number Formatting**:
```csharp
$"Count: {number:N0}"          // 1,234 (no decimals)
$"Percentage: {ratio:P1}"      // 12.3%
$"File size: {bytes / 1024.0:F1} KB"  // 123.4 KB
```

#### Message Composition Best Practices

**Template Design**:
- Write message templates that clearly show the final result
- Use consistent formatting throughout the application
- Choose appropriate formatting for the message context

**Performance Considerations**:
- Use StringBuilder for composing long texts or many concatenations
- Estimate StringBuilder capacity when possible to improve performance
- Use string interpolation for most other scenarios

**Safety and Validation**:
- Validate user input before including it in messages
- Consider message length limits for different contexts
- Handle null or empty values gracefully

Good string handling makes your text composition as smooth and professional as sending well-formatted messages in a modern communication app.