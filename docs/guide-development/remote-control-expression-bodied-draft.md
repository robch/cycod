# 10. Expression-Bodied Members

```csharp
// Single-button operations (expression-bodied) for simple device functions
public string FullName => $"{FirstName} {LastName}";          // Name display button
public bool IsAdult => Age >= 18;                             // Age check button  
public int TotalPoints => WinPoints + BonusPoints;            // Score display button
public bool HasItems => Items?.Count > 0;                     // Inventory check button

// Single-button methods for simple device operations
public string GetGreeting() => $"Hello, {Name}!";             // Greeting display button
public decimal GetTax() => Price * TaxRate;                   // Tax calculation button
public bool IsValid() => !string.IsNullOrEmpty(Value);        // Validation check button
public User GetCurrentUser() => Session.CurrentUser;          // Current user button

// Simple display readouts (properties) - like reading gauges on a dashboard
public DateTime LastLogin => _user.LastLoginTime;
public string StatusDisplay => _connection.IsActive ? "Online" : "Offline";
public int RemainingCredits => MaxCredits - UsedCredits;

// BAD: Complex multi-step operations disguised as single buttons
public string GetFormattedAddress() => 
    $"{Street}, {City}, {State} {ZipCode}".Trim(',', ' ');  // Too complex for single button

public decimal CalculateShippingCost() => 
    Weight > 10 ? (Weight * 2.5m) + 5.0m : Weight * 1.5m;  // Too much logic for one button

// GOOD: Complex operations use traditional multi-step controls (method bodies)
public string GetFormattedAddress()
{
    var parts = new[] { Street, City, State, ZipCode }.Where(p => !string.IsNullOrEmpty(p));
    return string.Join(", ", parts);
}

public decimal CalculateShippingCost()
{
    var baseRate = Weight > 10 ? 2.5m : 1.5m;
    var baseCost = Weight * baseRate;
    var surcharge = Weight > 10 ? 5.0m : 0m;
    
    return baseCost + surcharge;
}

// Device interface design - choosing the right control type
public class MediaPlayer
{
    // Simple display readouts - like LED displays on the device
    public TimeSpan CurrentPosition => _player.Position;
    public TimeSpan TotalDuration => _currentTrack?.Duration ?? TimeSpan.Zero;
    public bool IsPlaying => _player.State == PlayState.Playing;
    public string CurrentTrack => _currentTrack?.Title ?? "No track loaded";
    
    // Single-button operations for common functions
    public void Play() => _player.Start();                    // Play button
    public void Pause() => _player.Pause();                   // Pause button
    public void Stop() => _player.Stop();                     // Stop button
    public void NextTrack() => LoadTrack(_currentIndex + 1);  // Next button
    
    // Complex operations require traditional controls (method bodies)
    public void LoadPlaylist(string playlistName)
    {
        // Multi-step operation - can't be a single button
        var playlist = _playlistService.GetPlaylist(playlistName);
        if (playlist == null)
        {
            ShowError($"Playlist '{playlistName}' not found");
            return;
        }
        
        _currentPlaylist = playlist;
        _currentIndex = 0;
        LoadTrack(0);
        UpdateDisplay();
    }
    
    // Device status calculations - single-button display functions
    public string GetTimeRemaining() => 
        $"{(TotalDuration - CurrentPosition).ToString(@"mm\:ss")}";
    
    public double GetProgressPercentage() => 
        TotalDuration.TotalSeconds > 0 ? (CurrentPosition.TotalSeconds / TotalDuration.TotalSeconds) * 100 : 0;
}

// Smart device controls - choosing appropriate interface complexity
public class SmartThermostat
{
    // Simple readouts like digital displays
    public int CurrentTemperature => _sensor.Temperature;
    public int TargetTemperature => _settings.Target;
    public bool IsHeating => _hvac.HeatingActive;
    public bool IsCooling => _hvac.CoolingActive;
    
    // Single-button adjustments
    public void IncreaseTarget() => SetTarget(_settings.Target + 1);
    public void DecreaseTarget() => SetTarget(_settings.Target - 1);
    public string GetModeDisplay() => _settings.Mode.ToString();
    
    // Complex operations use traditional multi-step interfaces
    public void SetSchedule(DayOfWeek day, List<TemperatureSchedule> schedule)
    {
        // Multi-step programming - requires traditional method body
        ValidateSchedule(schedule);
        
        _schedules[day] = schedule;
        _schedules[day].Sort((a, b) => a.Time.CompareTo(b.Time));
        
        SaveSchedulesToDevice();
        NotifyScheduleUpdated(day);
    }
}

// Information display design - appropriate complexity for readouts
public class SystemMonitor
{
    // Simple gauge-like displays
    public double CpuUsage => _monitor.GetCpuPercentage();
    public long MemoryUsed => _monitor.GetMemoryUsage();
    public int ActiveProcesses => _monitor.GetProcessCount();
    public bool IsHealthy => CpuUsage < 80 && MemoryUsed < MaxMemory * 0.9;
    
    // Single-calculation displays
    public string MemoryDisplay => $"{MemoryUsed / (1024 * 1024):F1} MB";
    public string StatusIndicator => IsHealthy ? "✓ Normal" : "⚠ Alert";
    
    // Complex monitoring requires traditional method implementations
    public SystemHealthReport GenerateHealthReport()
    {
        // Multi-step analysis - too complex for single-button operation
        var report = new SystemHealthReport();
        
        AnalyzeCpuTrends(report);
        AnalyzeMemoryPatterns(report);
        CheckDiskSpace(report);
        ValidateNetworkConnectivity(report);
        
        report.OverallScore = CalculateHealthScore(report);
        report.Recommendations = GenerateRecommendations(report);
        
        return report;
    }
}
```

### Core Principles

- Use single-button controls (expression-bodied members) for simple device operations and display readouts
- Reserve traditional multi-step controls (method bodies) for complex operations that require multiple actions
- Choose single-button interfaces for property getters that are simple calculations or direct readouts
- Use single-button methods for operations that are straightforward transformations or simple actions
- Prioritize readability - if a single button would be confusing, use traditional controls instead
- Design your device interface from the user's perspective - make common operations simple and complex operations clear

### Why It Matters

Think of expression-bodied members as designing the control interface for a device. Just as good device design puts common functions on single, clearly-labeled buttons while complex operations use traditional multi-step interfaces, good code design uses expression-bodied members for simple operations and traditional method bodies for complex logic.

A poorly designed device interface puts complex multi-step operations on single buttons (confusing) or puts simple operations behind complex menu systems (inefficient).

Well-designed device interfaces provide:

1. **Intuitive Single-Button Operations**: Common functions are immediately accessible
2. **Clear Display Readouts**: Status information is easily visible without complex procedures
3. **Appropriate Complexity Matching**: Simple operations get simple controls, complex operations get detailed interfaces
4. **Consistent Interface Patterns**: Similar operations use similar control types throughout the device
5. **User-Focused Design**: Interface complexity matches the operation complexity from the user's perspective

When expression-bodied members are used inappropriately, it's like a remote control with confusing buttons that try to do too much, or a device that requires complex procedures for simple operations.

### Common Mistakes

#### Complex Multi-Step Operations on Single Buttons

```csharp
// BAD: Trying to put complex operations on single buttons - confusing interface
public string ProcessUserData() => 
    ValidateInput(RawData) ? FormatOutput(TransformData(CleanData(RawData))) : "Invalid";

public decimal CalculateFinalPrice() => 
    (BasePrice + GetTaxes() + GetShipping() - GetDiscounts()) * GetExchangeRate();

public bool CanUserAccess() => 
    User?.IsActive == true && User.HasPermission("read") && !IsLocked && DateTime.Now < ExpiryDate;
```

**Why it's problematic**: This is like putting a complex sequence of operations behind a single button with no clear indication of what steps are happening. Users (developers reading the code) can't easily understand the multi-step process or debug issues.

**Better approach**:

```csharp
// GOOD: Complex operations use traditional multi-step controls
public string ProcessUserData()
{
    if (!ValidateInput(RawData))
        return "Invalid";
        
    var cleanedData = CleanData(RawData);
    var transformedData = TransformData(cleanedData);
    return FormatOutput(transformedData);
}

public decimal CalculateFinalPrice()
{
    var baseAmount = BasePrice;
    var taxes = GetTaxes();
    var shipping = GetShipping();
    var discounts = GetDiscounts();
    var exchangeRate = GetExchangeRate();
    
    return (baseAmount + taxes + shipping - discounts) * exchangeRate;
}

public bool CanUserAccess()
{
    var userIsActive = User?.IsActive == true;
    var userHasPermission = User?.HasPermission("read") == true;
    var systemIsUnlocked = !IsLocked;
    var notExpired = DateTime.Now < ExpiryDate;
    
    return userIsActive && userHasPermission && systemIsUnlocked && notExpired;
}
```

#### Simple Operations Hidden Behind Complex Interfaces

```csharp
// BAD: Simple operations using complex multi-step controls unnecessarily
public string GetUserName()
{
    // Unnecessary complexity for simple display readout
    var firstName = User.FirstName;
    var lastName = User.LastName;
    var fullName = $"{firstName} {lastName}";
    return fullName;
}

public bool IsDiscounted()
{
    // Over-engineering a simple check
    decimal originalPrice = Product.OriginalPrice;
    decimal currentPrice = Product.CurrentPrice;
    
    if (originalPrice > currentPrice)
    {
        return true;
    }
    else
    {
        return false;
    }
}
```

**Why it's problematic**: This is like requiring users to go through multiple menu screens just to see the current channel or volume level. Simple readouts should be immediately accessible.

**Better approach**:

```csharp
// GOOD: Simple operations get single-button interfaces
public string GetUserName() => $"{User.FirstName} {User.LastName}";

public bool IsDiscounted() => Product.CurrentPrice < Product.OriginalPrice;
```

#### Unclear Button Labels (Poor Expression Readability)

```csharp
// BAD: Button labels that don't clearly indicate what they do
public bool Check() => Data?.IsValid == true && Count > 0;
public string Format() => $"{A}-{B}-{C}".Replace(" ", "");
public decimal Calc() => X * Y + Z;
```

**Why it's problematic**: This is like having unlabeled buttons on a device - users don't know what each button does without testing them. The expressions are unclear about their purpose.

**Better approach**:

```csharp
// GOOD: Clear button labels that indicate their function
public bool IsDataValid() => Data?.IsValid == true && Count > 0;
public string FormatAsCode() => $"{A}-{B}-{C}".Replace(" ", "");
public decimal CalculateTotal() => X * Y + Z;
```

#### Inconsistent Interface Design

```csharp
// BAD: Mixing interface styles inconsistently
public class InventoryItem
{
    // Some simple operations use complex interfaces
    public bool IsAvailable()
    {
        return Quantity > 0;  // Could be expression-bodied
    }
    
    // Some complex operations use single buttons
    public string Description => 
        $"{Name} ({Category}) - ${Price:F2} - {(IsAvailable() ? "In Stock" : "Out of Stock")} - Updated {LastUpdate:yyyy-MM-dd}";
}
```

**Why it's problematic**: This is like a device where some simple functions require menu navigation while complex operations are on unlabeled single buttons. The interface design is inconsistent and confusing.

**Better approach**:

```csharp
// GOOD: Consistent interface design - simple operations get simple controls
public class InventoryItem
{
    // Simple check gets single-button interface
    public bool IsAvailable() => Quantity > 0;
    
    // Complex description formatting gets traditional interface
    public string GetDescription()
    {
        var availabilityStatus = IsAvailable() ? "In Stock" : "Out of Stock";
        var formattedPrice = Price.ToString("C");
        var formattedDate = LastUpdate.ToString("yyyy-MM-dd");
        
        return $"{Name} ({Category}) - {formattedPrice} - {availabilityStatus} - Updated {formattedDate}";
    }
    
    // Simple display readouts use expression-bodied syntax
    public string StatusIndicator => IsAvailable() ? "✓" : "✗";
    public string PriceDisplay => Price.ToString("C");
}
```

### Evolution Example

Let's see how expression-bodied member usage might evolve from poor interface design to excellent device controls:

**Initial Version - All operations use complex interfaces:**

```csharp
// Initial version - everything uses complex multi-step controls
public class Calculator
{
    public double GetSum()
    {
        double result = ValueA + ValueB;
        return result;
    }
    
    public bool GetIsPositive()
    {
        if (CurrentValue > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    
    public string GetDisplay()
    {
        string display = CurrentValue.ToString();
        return display;
    }
}
```

**Intermediate Version - Some single buttons but inconsistent:**

```csharp
// Better but inconsistent interface design
public class Calculator
{
    // Some simple operations get single-button controls
    public double Sum => ValueA + ValueB;
    public bool IsPositive => CurrentValue > 0;
    
    // But still using complex controls for simple operations
    public string GetDisplay()
    {
        return CurrentValue.ToString();
    }
    
    // And putting complex operations on single buttons
    public string ComplexDisplay => 
        $"Calculator: {CurrentValue:F2} (Sum: {Sum}, Positive: {(IsPositive ? "Yes" : "No")}, Last: {LastOperation})";
}
```

**Final Version - Excellent interface design with appropriate complexity:**

```csharp
// Excellent device interface design
public class Calculator
{
    // Simple display readouts use single-button interfaces
    public double Sum => ValueA + ValueB;
    public double Product => ValueA * ValueB;
    public bool IsPositive => CurrentValue > 0;
    public bool IsZero => Math.Abs(CurrentValue) < 0.0001;
    public string DisplayValue => CurrentValue.ToString("F2");
    
    // Single-button operations for simple functions
    public void Clear() => CurrentValue = 0;
    public void Negate() => CurrentValue = -CurrentValue;
    public double GetAbsolute() => Math.Abs(CurrentValue);
    
    // Complex operations use traditional multi-step interfaces
    public CalculationResult PerformComplexCalculation(string expression)
    {
        // Multi-step parsing and calculation - requires traditional method body
        try
        {
            var tokens = ParseExpression(expression);
            ValidateTokens(tokens);
            
            var result = EvaluateExpression(tokens);
            var history = CreateHistoryEntry(expression, result);
            
            SaveToHistory(history);
            CurrentValue = result;
            
            return new CalculationResult
            {
                Value = result,
                Expression = expression,
                Success = true
            };
        }
        catch (Exception ex)
        {
            return new CalculationResult
            {
                Error = ex.Message,
                Success = false
            };
        }
    }
    
    public string GenerateDetailedReport()
    {
        // Complex report generation requires traditional method
        var report = new StringBuilder();
        
        report.AppendLine($"Calculator State Report - {DateTime.Now:yyyy-MM-dd HH:mm}");
        report.AppendLine($"Current Value: {DisplayValue}");
        report.AppendLine($"Status: {(IsPositive ? "Positive" : IsZero ? "Zero" : "Negative")}");
        report.AppendLine();
        
        report.AppendLine("Recent Calculations:");
        foreach (var calc in GetRecentHistory(5))
        {
            report.AppendLine($"  {calc.Expression} = {calc.Result:F2}");
        }
        
        return report.ToString();
    }
    
    // Simple property calculations use expression-bodied syntax
    public string StatusIcon => IsPositive ? "+" : IsZero ? "0" : "-";
    public string ColorCode => IsPositive ? "Green" : IsZero ? "Gray" : "Red";
}
```

### Deeper Understanding

#### Device Interface Design Principles

Good expression-bodied member usage follows the same principles as good device interface design:

1. **Single-Button Operations**: Use for functions that are immediately understandable and execute in one step
2. **Display Readouts**: Use for properties that show current state or simple calculations
3. **Traditional Controls**: Use for operations that require multiple steps or complex logic
4. **Consistent Interface**: Similar operation complexity should use similar control types

#### When to Use Each Interface Type

**Single-Button Controls (Expression-Bodied)**:
```csharp
public bool IsActive => Status == Status.Active;        // Simple state check
public string FullName => $"{First} {Last}";            // Simple formatting
public int Total => Items.Sum(i => i.Value);           // Direct calculation
```

**Traditional Multi-Step Controls (Method Bodies)**:
```csharp
public ValidationResult Validate()  // Multi-step validation
{
    var errors = new List<string>();
    
    ValidateRequired(errors);
    ValidateFormat(errors);
    ValidateBusinessRules(errors);
    
    return new ValidationResult(errors);
}
```

#### Design Guidelines for Device Interfaces

1. **Clarity Over Brevity**: If an expression-bodied member is hard to understand, use a traditional method body
2. **Consistent Complexity**: Match the interface complexity to the operation complexity
3. **User Perspective**: Design from the caller's perspective - what makes sense to them?
4. **Maintenance Considerations**: Simple expressions are easier to debug than complex ones

#### Readability Test

Before using an expression-bodied member, ask:
- Is the operation immediately clear from reading it once?
- Would a junior developer understand this without explanation?
- If this were a button on a device, would users know what it does?

If the answer to any of these is "no," consider using a traditional method body with clear step-by-step logic.

Good expression-bodied member usage makes your code interface as intuitive and efficient as a well-designed device with clearly labeled buttons and easy-to-read displays.