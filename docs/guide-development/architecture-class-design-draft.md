# 22. Class Design and Relationships

```csharp
// Inheritance for "is-a" relationships - architectural families
public abstract class Building
{
    // Common architectural features all buildings share
    public string Address { get; set; }
    public int SquareFootage { get; set; }
    public DateTime ConstructionDate { get; set; }
    
    // Abstract method - each building type implements differently
    public abstract decimal CalculatePropertyTax();
    
    // Common building functionality
    public virtual void PerformMaintenance()
    {
        InspectFoundation();
        CheckRoof();
        TestElectrical();
    }
}

// Residential building family - inherits core building characteristics
public class ResidentialBuilding : Building
{
    public int NumberOfBedrooms { get; set; }
    public bool HasGarage { get; set; }
    
    public override decimal CalculatePropertyTax()
    {
        // Residential tax calculation
        return SquareFootage * 0.012m;
    }
}

// Commercial building family - different characteristics but still a building
public class CommercialBuilding : Building
{
    public int NumberOfUnits { get; set; }
    public bool HasParkingGarage { get; set; }
    
    public override decimal CalculatePropertyTax()
    {
        // Commercial tax calculation
        return SquareFootage * 0.018m;
    }
}

// Composition for "has-a" relationships - buildings contain components
public class House : ResidentialBuilding
{
    // House HAS-A kitchen, garage, heating system (composition)
    private readonly Kitchen _kitchen;
    private readonly Garage _garage;
    private readonly HeatingSystem _heatingSystem;
    private readonly ElectricalSystem _electricalSystem;
    
    public House(Kitchen kitchen, Garage garage, HeatingSystem heating, ElectricalSystem electrical)
    {
        _kitchen = kitchen ?? throw new ArgumentNullException(nameof(kitchen));
        _garage = garage;  // Garage is optional
        _heatingSystem = heating ?? throw new ArgumentNullException(nameof(heating));
        _electricalSystem = electrical ?? throw new ArgumentNullException(nameof(electrical));
    }
    
    // House delegates kitchen operations to its kitchen component
    public void PrepareMeal()
    {
        _kitchen.StartOven();
        _kitchen.TurnOnLights();
    }
    
    // House coordinates its various systems
    public void SetTemperature(int degrees)
    {
        _heatingSystem.SetTargetTemperature(degrees);
    }
    
    // House can access garage features if garage exists
    public bool CanParkCar()
    {
        return _garage?.HasSpace() ?? false; // No garage means no parking
    }
}

// Building codes (interfaces) define standards all buildings must meet
public interface IFireSafetyCompliant
{
    bool HasSprinklerSystem { get; }
    bool HasFireExtinguishers { get; }
    void ActivateFireSuppression();
}

public interface IAccessibilityCompliant
{
    bool HasWheelchairAccess { get; }
    bool HasElevator { get; }
    void EnsureAccessibilityStandards();
}

// Buildings implement the codes they need to comply with
public class Office : CommercialBuilding, IFireSafetyCompliant, IAccessibilityCompliant
{
    // Fire safety features required by code
    public bool HasSprinklerSystem { get; private set; } = true;
    public bool HasFireExtinguishers { get; private set; } = true;
    
    // Accessibility features required by code
    public bool HasWheelchairAccess { get; private set; } = true;
    public bool HasElevator { get; private set; } = true;
    
    public void ActivateFireSuppression()
    {
        // Implement fire safety protocol
        TriggerSprinklers();
        SoundFireAlarm();
    }
    
    public void EnsureAccessibilityStandards()
    {
        // Verify accessibility compliance
        CheckRampGrades();
        TestElevatorOperation();
    }
}

// Programming to interfaces (building codes) rather than concrete types
public class BuildingInspector
{
    // Inspector works with any building that meets fire safety codes
    public InspectionResult InspectFireSafety(IFireSafetyCompliant building)
    {
        var hasSprinklers = building.HasSprinklerSystem;
        var hasExtinguishers = building.HasFireExtinguishers;
        
        if (hasSprinklers && hasExtinguishers)
        {
            building.ActivateFireSuppression(); // Test the system
            return InspectionResult.Passed();
        }
        
        return InspectionResult.Failed("Fire safety requirements not met");
    }
    
    // Can inspect any building type that implements the interface
    public void PerformAccessibilityInspection(IAccessibilityCompliant building)
    {
        building.EnsureAccessibilityStandards();
    }
}

// Polymorphism - different building types can be treated uniformly when they share interfaces
public class CityPlanner
{
    public void EvaluateFireSafety(List<IFireSafetyCompliant> buildings)
    {
        var inspector = new BuildingInspector();
        
        // Can work with any building type that implements fire safety
        foreach (var building in buildings)
        {
            var result = inspector.InspectFireSafety(building);
            ProcessInspectionResult(result);
        }
    }
}

// Encapsulation - private building systems vs. public interfaces
public class SmartHome : House
{
    // Private building systems (implementation details)
    private readonly SecuritySystem _securitySystem;
    private readonly HomeAutomation _automation;
    
    // Public interface for residents
    public bool IsSecurityArmed => _securitySystem.IsActive;
    public int CurrentTemperature => _heatingSystem.CurrentTemperature;
    
    // Controlled access to building systems
    public void ArmSecurity(string accessCode)
    {
        if (ValidateAccessCode(accessCode))
        {
            _securitySystem.Activate();
        }
    }
    
    // Private building operations
    private bool ValidateAccessCode(string code)
    {
        return _securitySystem.ValidateCode(code);
    }
    
    // Public building services
    public void SetHomeToVacationMode()
    {
        _automation.SetLightingSchedule(VacationLightingPattern);
        _heatingSystem.SetEconomyMode();
        _securitySystem.SetHighSensitivity();
    }
}
```

### Core Principles

- Use inheritance for "is-a" relationships where one building type extends another architectural family
- Use composition for "has-a" relationships where buildings contain components and systems
- Prefer composition over inheritance for flexibility - it's easier to swap building components than redesign architectural lineage
- Keep inheritance hierarchies shallow to avoid complex architectural genealogies
- Program to building codes (interfaces) rather than specific building types for maximum flexibility
- Encapsulate building systems - control access to internal mechanisms while providing clean public interfaces
- Design buildings that can be extended and modified without rebuilding from scratch

### Why It Matters

Think of class design as architectural planning for a city development project. Just as good architectural planning creates buildings that are functional, maintainable, and adaptable to changing needs, good class design creates code that is robust, extensible, and easy to work with.

Poor class design is like a city with inconsistent building codes, no zoning laws, and buildings that can't be renovated or expanded without demolishing neighboring structures.

Well-designed architectural systems provide:

1. **Clear Building Codes**: Interfaces define standards that all building types can follow
2. **Flexible Construction**: Composition allows you to swap out building components without redesigning the entire structure
3. **Maintainable Infrastructure**: Encapsulation protects building systems while providing clean access points
4. **Scalable Development**: Good design patterns allow the city to grow without chaos
5. **Code Reusability**: Standard architectural elements can be used across multiple building projects

When class relationships are poorly designed, it creates "architectural debt" - tangled dependencies that make maintenance expensive and expansion risky.

### Common Mistakes

#### Over-Engineering Architectural Hierarchies

```csharp
// BAD: Overly complex architectural inheritance that's hard to maintain
public abstract class Structure
{
    public abstract void Exist();
}

public abstract class Building : Structure
{
    public abstract void ProvidesShelter();
}

public abstract class ResidentialBuilding : Building
{
    public abstract void HousesFamily();
}

public abstract class SingleFamilyHome : ResidentialBuilding
{
    public abstract void AccommodatesSingleFamily();
}

public class TwoStoryHouse : SingleFamilyHome
{
    public override void Exist() { /* ... */ }
    public override void ProvidesShelter() { /* ... */ }
    public override void HousesFamily() { /* ... */ }
    public override void AccommodatesSingleFamily() { /* ... */ }
    
    // Finally, actual functionality buried under layers of abstraction
    public void OpenFrontDoor() { /* ... */ }
}
```

**Why it's problematic**: This is like having a zoning code that requires 6 different architectural review boards for building a simple house. The inheritance hierarchy is so deep and abstract that it obscures the actual functionality and makes changes risky because they affect multiple levels.

**Better approach**:

```csharp
// GOOD: Simple, focused architectural hierarchy
public abstract class Building
{
    public string Address { get; set; }
    public abstract decimal CalculatePropertyTax();
}

public class House : Building
{
    public int Bedrooms { get; set; }
    public bool HasGarage { get; set; }
    
    public override decimal CalculatePropertyTax()
    {
        return SquareFootage * ResidentialTaxRate;
    }
    
    // Actual functionality is directly accessible
    public void OpenFrontDoor() { /* ... */ }
}
```

#### Misusing Inheritance for Component Relationships

```csharp
// BAD: Using inheritance when composition is more appropriate
public class Kitchen
{
    public void CookFood() { /* ... */ }
}

public class Garage
{
    public void ParkCar() { /* ... */ }
}

// This creates impossible architectural relationships
public class House : Kitchen  // House IS-A Kitchen? That makes no sense!
{
    // Now House has all Kitchen methods directly, which is confusing
    public void LiveIn() { /* ... */ }
}

// Even worse - multiple inheritance isn't supported, so we can't do:
// public class House : Kitchen, Garage  // Can't inherit from multiple classes
```

**Why it's problematic**: This is like saying "a house IS a kitchen" rather than "a house HAS a kitchen." It creates nonsensical architectural relationships and makes the house responsible for cooking operations directly, rather than delegating to its kitchen component.

**Better approach**:

```csharp
// GOOD: Composition - House HAS-A Kitchen, HAS-A Garage
public class House : Building
{
    private readonly Kitchen _kitchen;
    private readonly Garage _garage;
    
    public House(Kitchen kitchen, Garage garage)
    {
        _kitchen = kitchen;
        _garage = garage;
    }
    
    // House delegates cooking operations to its kitchen
    public void PrepareMeal()
    {
        _kitchen.CookFood();
    }
    
    // House delegates parking operations to its garage
    public void ParkCar()
    {
        _garage.ParkCar();
    }
}
```

#### Exposing Internal Building Systems

```csharp
// BAD: All building systems are exposed publicly
public class SmartHome
{
    public SecuritySystem SecuritySystem;      // Direct access to security
    public HeatingSystem HeatingSystem;       // Direct access to heating
    public ElectricalSystem ElectricalSystem; // Direct access to electrical
    
    public void TurnOnLights()
    {
        // Anyone can bypass proper protocols and directly manipulate systems
        ElectricalSystem.ActivateCircuit("living-room-lights");
    }
}

// This allows dangerous direct manipulation:
var home = new SmartHome();
home.SecuritySystem.Deactivate(); // No access control!
home.ElectricalSystem.OverrideCircuitBreaker(); // Dangerous!
```

**Why it's problematic**: This is like a building where all the electrical panels, security controls, and mechanical systems are accessible to anyone without any safety protocols. It bypasses building codes and safety systems, creating security risks and potential for damage.

**Better approach**:

```csharp
// GOOD: Controlled access to building systems through proper interfaces
public class SmartHome
{
    // Private building systems - protected by encapsulation
    private readonly SecuritySystem _securitySystem;
    private readonly HeatingSystem _heatingSystem;
    private readonly ElectricalSystem _electricalSystem;
    
    // Controlled public interface for building operations
    public bool IsSecurityArmed => _securitySystem.IsActive;
    
    public void ArmSecurity(string authorizationCode)
    {
        if (ValidateAuthorization(authorizationCode))
        {
            _securitySystem.Activate();
        }
    }
    
    public void SetTemperature(int degrees)
    {
        if (degrees >= MinTemp && degrees <= MaxTemp)
        {
            _heatingSystem.SetTargetTemperature(degrees);
        }
    }
    
    // Safe, controlled lighting operation
    public void TurnOnLights(string room)
    {
        _electricalSystem.SafelyActivateLights(room);
    }
}
```

#### Not Programming to Building Codes

```csharp
// BAD: Inspector tied to specific building types
public class BuildingInspector
{
    public InspectionResult InspectOffice(Office office)
    {
        // Tightly coupled to Office class
        var sprinklers = office.HasSprinklerSystem;
        return sprinklers ? InspectionResult.Pass() : InspectionResult.Fail();
    }
    
    public InspectionResult InspectWarehouse(Warehouse warehouse)
    {
        // Duplicate logic for different building type
        var sprinklers = warehouse.HasSprinklerSystem;
        return sprinklers ? InspectionResult.Pass() : InspectionResult.Fail();
    }
    
    // Need separate method for each building type - doesn't scale!
}
```

**Why it's problematic**: This is like having building inspectors who can only inspect one specific type of building. You'd need a different inspector for every building design, even when they're checking the same building codes.

**Better approach**:

```csharp
// GOOD: Inspector works with building codes (interfaces)
public class BuildingInspector
{
    public InspectionResult InspectFireSafety(IFireSafetyCompliant building)
    {
        // Works with any building that implements fire safety standards
        var sprinklers = building.HasSprinklerSystem;
        var extinguishers = building.HasFireExtinguishers;
        
        return (sprinklers && extinguishers) 
            ? InspectionResult.Pass() 
            : InspectionResult.Fail();
    }
    
    // Single method works with all building types that implement the interface
}
```

### Evolution Example

Let's see how class design might evolve from poor architectural planning to excellent structural design:

**Initial Version - No architectural planning:**

```csharp
// Initial version - everything mixed together with no organization
public class House
{
    // No clear architectural structure
    public string address;
    public int bedrooms;
    public string kitchenType;
    public bool hasGarage;
    public string securityCode;
    public int temperatureSetting;
    
    public void DoEverything()
    {
        // House tries to do everything itself
        CookFood();
        ParkCar();
        ArmSecurity();
        HeatBuilding();
    }
    
    // All operations mixed together
    public void CookFood() { /* cooking logic */ }
    public void ParkCar() { /* parking logic */ }
    public void ArmSecurity() { /* security logic */ }
    public void HeatBuilding() { /* heating logic */ }
}
```

**Intermediate Version - Some organization but still problematic:**

```csharp
// Better organization but still architectural issues
public class Building
{
    public string Address { get; set; }
    public int SquareFootage { get; set; }
}

public class House : Building
{
    // Some separation but still mixed responsibilities
    public Kitchen Kitchen;        // Public field - no encapsulation
    public Garage Garage;          // Public field - no encapsulation
    public SecuritySystem Security; // Public field - no encapsulation
    
    public void PrepareMeal()
    {
        Kitchen.CookFood(); // Direct access to kitchen
    }
    
    public void SecureHouse()
    {
        Security.Activate(); // No access control
    }
}
```

**Final Version - Excellent architectural design:**

```csharp
// Excellent architectural design with clear separation and proper relationships
public abstract class Building
{
    public string Address { get; protected set; }
    public int SquareFootage { get; protected set; }
    public DateTime ConstructionDate { get; protected set; }
    
    public abstract decimal CalculatePropertyTax();
    
    protected Building(string address, int squareFootage)
    {
        Address = address ?? throw new ArgumentNullException(nameof(address));
        SquareFootage = squareFootage > 0 ? squareFootage : throw new ArgumentException("Square footage must be positive");
        ConstructionDate = DateTime.Now;
    }
}

public interface ISecurable
{
    bool IsSecure { get; }
    void Arm(string authorizationCode);
    void Disarm(string authorizationCode);
}

public interface IClimateControlled
{
    int CurrentTemperature { get; }
    void SetTemperature(int targetTemperature);
}

public class House : Building, ISecurable, IClimateControlled
{
    // Private building systems - proper encapsulation
    private readonly Kitchen _kitchen;
    private readonly Garage _garage;
    private readonly SecuritySystem _securitySystem;
    private readonly ClimateSystem _climateSystem;
    
    // Controlled public interface
    public bool IsSecure => _securitySystem.IsArmed;
    public int CurrentTemperature => _climateSystem.CurrentTemperature;
    public bool HasGarage => _garage != null;
    
    public House(string address, int squareFootage, Kitchen kitchen, ClimateSystem climate) 
        : base(address, squareFootage)
    {
        _kitchen = kitchen ?? throw new ArgumentNullException(nameof(kitchen));
        _climateSystem = climate ?? throw new ArgumentNullException(nameof(climate));
        _securitySystem = new SecuritySystem();
        _garage = null; // Optional component
    }
    
    // Proper delegation to building components
    public void PrepareMeal(MealType mealType)
    {
        _kitchen.PrepareMeal(mealType);
    }
    
    // Controlled access with validation
    public void Arm(string authorizationCode)
    {
        if (_securitySystem.ValidateCode(authorizationCode))
        {
            _securitySystem.Arm();
        }
    }
    
    public void Disarm(string authorizationCode)
    {
        if (_securitySystem.ValidateCode(authorizationCode))
        {
            _securitySystem.Disarm();
        }
    }
    
    public void SetTemperature(int targetTemperature)
    {
        _climateSystem.SetTargetTemperature(targetTemperature);
    }
    
    public override decimal CalculatePropertyTax()
    {
        var baseTax = SquareFootage * 0.012m;
        var garageBonus = HasGarage ? 1000m : 0m;
        return baseTax + garageBonus;
    }
}
```

### Deeper Understanding

#### Architectural Planning Principles

Good class design follows the same principles as good architectural planning:

1. **Zoning Laws (Single Responsibility)**: Each building type has a clear, focused purpose
2. **Building Codes (Interfaces)**: Standard requirements that multiple building types can implement
3. **Construction Methods**: Composition (assembling components) vs. inheritance (architectural lineage)
4. **Access Control**: Public spaces vs. private building systems

#### Inheritance vs. Composition in Architecture

**Inheritance (IS-A relationships)**:
```csharp
public class Office : CommercialBuilding  // Office IS-A CommercialBuilding
public class House : ResidentialBuilding // House IS-A ResidentialBuilding
```
Like architectural families: Victorian houses inherit characteristics from the Victorian architectural style.

**Composition (HAS-A relationships)**:
```csharp
public class House
{
    private Kitchen _kitchen;    // House HAS-A Kitchen
    private Garage _garage;      // House HAS-A Garage
}
```
Like building components: Houses contain kitchens, garages, and other functional spaces.

#### When to Use Each Approach

**Use Inheritance when**:
- You have a genuine "is-a" relationship
- Subclasses share substantial common functionality
- The hierarchy is shallow (2-3 levels maximum)

**Use Composition when**:
- You have a "has-a" or "uses-a" relationship
- You need flexibility to swap components
- You want to combine behaviors from multiple sources

#### Interface Design as Building Codes

Interfaces are like building codes that define standards without specifying implementation:

```csharp
public interface IFireSafetyCompliant
{
    bool HasSprinklerSystem { get; }
    void ActivateFireSuppression();
}
```

Any building type can implement fire safety compliance:
- Offices implement it with commercial-grade systems
- Houses implement it with residential systems
- Warehouses implement it with industrial systems

#### Encapsulation as Building Security

Encapsulation protects building systems while providing controlled access:

```csharp
public class SmartHome
{
    private SecuritySystem _security;  // Private - internal building system
    
    public bool IsSecure => _security.IsArmed;  // Public - safe status reading
    
    public void Arm(string code)  // Public - controlled system access
    {
        if (ValidateCode(code))
            _security.Activate();
    }
}
```

This is like having building systems behind locked panels with authorized access points, rather than exposing all the wiring and controls to everyone who enters the building.