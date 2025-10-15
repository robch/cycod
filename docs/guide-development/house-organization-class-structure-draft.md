# 6. Class Structure

```csharp
// House organization with appropriate privacy levels for different areas
public class FamilyHome
{
    // Front entrance and living areas (public) - visitors welcome
    public string Address { get; set; }
    public string ContactPhone { get; set; }
    public bool IsHomeDuringBusinessHours { get; set; }
    
    // Living room and kitchen functions (public) - guests can use these areas
    public void WelcomeGuests(List<Guest> guests)
    {
        SetupLivingRoom(guests.Count);
        PrepareRefreshments();
    }
    
    public MealService PrepareMealForGuests(MealRequest request)
    {
        return Kitchen.PrepareMeal(request, GuestServingWare);
    }
    
    public void GiveHouseTour(Guest guest)
    {
        ShowLivingAreas(guest);
        ExplainHouseFeatures(guest);
    }
    
    // Family areas (protected) - family members and close friends can access
    protected string FamilyCalendar { get; set; }
    protected List<FamilyEvent> UpcomingEvents { get; set; }
    
    // Family room and study activities (protected) - family and close friends
    protected void PlanFamilyActivity(FamilyEvent activity)
    {
        FamilyCalendar.AddEvent(activity);
        NotifyFamilyMembers(activity);
    }
    
    protected void AccessFamilyRecords(RecordType recordType)
    {
        var records = FamilyDocuments.GetRecords(recordType);
        ReviewWithFamilyMembers(records);
    }
    
    // Private bedrooms and personal storage (private) - family members only
    private readonly BankAccount _familyFinances;
    private List<PersonalDocument> _privateDocuments;
    private SecuritySystem _homeSecuritySystem;
    
    // Personal and financial management (private) - internal family business only
    private void ManageFamilyFinances()
    {
        var monthlyBudget = CalculateMonthlyBudget();
        _familyFinances.AllocateFunds(monthlyBudget);
        UpdateFinancialRecords();
    }
    
    private void UpdateSecuritySettings()
    {
        _homeSecuritySystem.UpdateAccessCodes();
        _privateDocuments.Add(new SecurityUpdate(DateTime.Now));
    }
}

// Small apartment organization (simple class structure)
public class StudioApartment
{
    // Main living space (public) - visitors can see and use
    public string Address { get; set; }
    public int SquareFootage { get; set; }
    
    public void WelcomeVisitor(Guest guest)
    {
        OfferSeating(guest);
        MakeVisitorComfortable(guest);
    }
    
    // Personal storage area (private) - resident only
    private readonly List<PersonalItem> _personalBelongings;
    private SecurityDeposit _deposit;
}

// Large family house organization (complex class structure) 
public class LargeFamilyHouse
{
    // Public entrance and common areas - neighbors and visitors welcome
    public string Address { get; set; }
    public string HouseholdContactInfo { get; set; }
    public bool AcceptingVisitors { get; set; }
    
    // Community interaction functions (public)
    public void HostNeighborhoodEvent(CommunityEvent communityEvent)
    {
        SetupPublicAreas(communityEvent);
        WelcomeNeighbors(communityEvent.Attendees);
    }
    
    public void ReceivePackageDelivery(Package package)
    {
        LogPackageReceived(package);
        NotifyHouseholdMembers(package);
    }
    
    public HouseShowingResult GiveHouseShowingToProspectiveBuyers(List<ProspectiveBuyer> buyers)
    {
        var publicAreaTour = ShowPublicAreas(buyers);
        var houseFeaturesTour = DemonstrateHouseFeatures(buyers);
        
        return new HouseShowingResult(publicAreaTour, houseFeaturesTour);
    }
    
    // Extended family areas (protected) - family members and overnight guests
    protected List<FamilyMember> HouseholdMembers { get; set; }
    protected FamilySchedule SharedCalendar { get; set; }
    protected List<HouseholdRule> HouseRules { get; set; }
    
    // Family coordination functions (protected)
    protected void CoordinateHouseholdSchedule()
    {
        var conflictingEvents = FindScheduleConflicts(SharedCalendar);
        ResolveScheduleConflicts(conflictingEvents);
        NotifyHouseholdOfChanges();
    }
    
    protected void EnforceHouseRules(HouseholdRule rule, FamilyMember member)
    {
        var ruleViolation = CheckRuleCompliance(rule, member);
        if (ruleViolation.IsViolation)
        {
            AddressRuleViolation(member, ruleViolation);
        }
    }
    
    protected void PlanFamilyVacation(VacationRequest request)
    {
        var vacation = CreateVacationPlan(request, HouseholdMembers);
        UpdateSharedCalendar(vacation);
        ArrangeFamilyTravel(vacation);
    }
    
    // Private master bedroom and personal storage (private) - parents/owners only
    private readonly FinancialAccount _householdFinances;
    private readonly List<ImportantDocument> _legalDocuments;
    private readonly SecuritySystem _homeSecuritySystem;
    private readonly InsurancePolicy _homeInsurance;
    private Dictionary<string, PersonalItem> _privateStorage;
    
    // Personal household management (private) - sensitive family business
    private void ProcessMortgagePayment()
    {
        var monthlyPayment = CalculateMortgagePayment();
        _householdFinances.MakePayment(monthlyPayment);
        UpdateLoanRecords();
    }
    
    private void ReviewInsurancePolicies()
    {
        var currentCoverage = _homeInsurance.GetCurrentCoverage();
        var adequacyAssessment = AssessCoverageAdequacy(currentCoverage);
        
        if (adequacyAssessment.NeedsUpdate)
        {
            ContactInsuranceAgent(adequacyAssessment.Recommendations);
        }
    }
    
    private void ManageHouseholdSecurity()
    {
        _homeSecuritySystem.UpdateAccessCodes();
        ReviewSecurityEventLogs();
        TestEmergencyProcedures();
    }
    
    private void OrganizeImportantDocuments()
    {
        var expiredDocuments = _legalDocuments.Where(d => d.ExpirationDate < DateTime.Now);
        ProcessExpiredDocuments(expiredDocuments);
        
        var documentsNearingExpiration = _legalDocuments.Where(d => d.ExpirationDate < DateTime.Now.AddMonths(3));
        ScheduleDocumentRenewal(documentsNearingExpiration);
    }
}

// BAD: House with no privacy zones - everything mixed together in one room
public class ChaoticHouse
{
    // No room organization - everything accessible to everyone
    public BankAccount familyFinances;           // Financial information in living room!
    private string address;                      // Address hidden from guests
    public SecurityCode securitySystem;         // Security codes in public view!
    protected void PayBills() { }               // Bill paying in semi-private area?
    public List<PersonalDocument> documents;    // Personal documents in public area!
    private void WelcomeGuests() { }            // Guest services hidden away
}

// GOOD: Well-organized house with appropriate privacy levels
public class WellOrganizedHome
{
    // Public areas - guests and visitors welcome
    public string Address { get; set; }
    public string ContactInformation { get; set; }
    
    // Guest services in public areas
    public void WelcomeGuests(List<Guest> guests) { /* ... */ }
    public void OfferRefreshments() { /* ... */ }
    
    // Family areas - household members and close friends
    protected FamilySchedule SharedCalendar { get; set; }
    
    protected void PlanFamilyEvents() { /* ... */ }
    
    // Private areas - sensitive household information and management
    private readonly BankAccount _householdFinances;
    private readonly SecuritySystem _securitySystem;
    
    private void ManageHouseholdBudget() { /* ... */ }
    private void UpdateSecuritySettings() { /* ... */ }
}

// Inheritance hierarchy - different house types with appropriate organization
public abstract class House
{
    // Basic house features available to all house types
    public string Address { get; protected set; }
    public int Bedrooms { get; protected set; }
    public int Bathrooms { get; protected set; }
    
    // Common house functions
    public abstract void CalculatePropertyTax();
    
    protected void PerformRoutineMaintenance()
    {
        InspectPlumbing();
        CheckElectricalSystems();
        TestSmokeDetectors();
    }
    
    // Common house management
    private readonly UtilityAccounts _utilities;
    private readonly InsurancePolicy _homeInsurance;
}

public class SingleFamilyHome : House
{
    // Additional features specific to single-family homes
    public bool HasGarage { get; set; }
    public decimal LotSize { get; set; }
    
    // Single-family home functions
    public void MaintainYard()
    {
        MowLawn();
        TrimShrubs();
        WaterGarden();
    }
    
    public override void CalculatePropertyTax()
    {
        // Single-family home tax calculation
        var baseTax = AssessedValue * ResidentialTaxRate;
        var yardTax = LotSize * LandTaxRate;
        PropertyTax = baseTax + yardTax;
    }
    
    // Private single-family home management
    private void ManageYardMaintenanceSchedule()
    {
        var seasonalTasks = GetSeasonalYardTasks();
        ScheduleMaintenanceTasks(seasonalTasks);
    }
}

public class Condominium : House  
{
    // Condo-specific features
    public int FloorNumber { get; set; }
    public decimal MonthlyAssociationFee { get; set; }
    
    // Condo-specific functions
    public void PayAssociationFees()
    {
        var monthlyFee = CalculateMonthlyFees();
        CondoAssociation.ProcessPayment(monthlyFee);
    }
    
    public override void CalculatePropertyTax()
    {
        // Condominium tax calculation
        PropertyTax = AssessedValue * CondominiumTaxRate;
    }
    
    // Private condo management
    private void ParticipateInCondoAssociationMeeting()
    {
        var meetingAgenda = CondoAssociation.GetMeetingAgenda();
        ReviewAssociationIssues(meetingAgenda);
    }
}
```

### Core Principles

- Organize house areas by accessibility levels: public spaces for guests, family areas for household members, private areas for personal business
- Place guest services and visitor areas at the front of the house (public members first)
- Position family areas and shared spaces in the middle (protected members next)
- Keep personal bedrooms and storage in private areas (private fields at the bottom of each privacy zone)
- Maintain one family per house file for clear household organization
- Design house layout to flow logically from public to private areas

### Why It Matters

Think of class structure as organizing a house with appropriate privacy levels and accessibility zones. Just as a well-organized house has clear areas for different activities and appropriate privacy levels for family vs. guests, a well-structured class has clear organization that makes it easy to understand what's accessible to different types of users.

Poor class structure is like a house where the master bedroom is at the front entrance, where guests have to walk through private storage to reach the living room, and where personal financial documents are scattered throughout public areas.

Well-organized homes provide:

1. **Intuitive Navigation**: Visitors can easily find public areas and understand what's accessible to them
2. **Appropriate Privacy**: Personal and sensitive areas are properly protected from casual access
3. **Logical Flow**: House layout follows a natural progression from public to private spaces
4. **Easy Maintenance**: Organized storage and clear area purposes make housekeeping manageable
5. **Expandable Design**: New rooms and features can be added without disrupting existing organization

When class structure is poorly organized, developers can't easily understand what they can access, accidentally use private implementation details, and struggle to navigate the class interface.

### Common Mistakes

#### House with No Privacy Organization

```csharp
// BAD: Everything mixed together with no privacy zones
public class ChaoticHouse
{
    private string address;                    // Address hidden in private area
    public BankAccount personalFinances;      // Financial info in public living room!
    private void WelcomeGuests() { }          // Guest services hidden away
    public SecurityCode securitySystem;       // Security codes visible to visitors!
    protected string phoneNumber;            // Contact info in family-only area
    public void PayPrivateBills() { }        // Personal business in public area
    private bool hasGarage;                  // Basic house info hidden
    public List<PersonalDocument> documents; // Private documents in living room!
    
    // Visitors can't find basic information but can see sensitive financial data
}
```

**Why it's problematic**: This is like a house where visitors can see your bank statements in the living room but can't find the address or phone number, where guest services are hidden in private bedrooms, and where security codes are posted in public areas. The organization is backwards and exposes sensitive information while hiding basic services.

**Better approach**:

```csharp
// GOOD: Well-organized house with logical privacy zones
public class WellOrganizedHome
{
    // Public areas - visitors and guests welcome
    public string Address { get; set; }
    public string ContactPhone { get; set; }
    public bool IsAcceptingVisitors { get; set; }
    
    // Guest services in public areas
    public void WelcomeGuests(List<Guest> guests)
    {
        PreparePublicAreas(guests.Count);
        OfferRefreshments(guests);
    }
    
    public void ProvideHouseTour(Guest guest)
    {
        ShowPublicRooms(guest);
        ExplainHouseFeatures(guest);
    }
    
    // Family areas - household members and close family friends
    protected FamilySchedule SharedCalendar { get; set; }
    protected List<FamilyRule> HouseholdRules { get; set; }
    
    protected void PlanFamilyEvent(FamilyEvent familyEvent)
    {
        SharedCalendar.AddEvent(familyEvent);
        CoordinateWithFamily(familyEvent);
    }
    
    // Private areas - sensitive household management
    private readonly BankAccount _householdFinances;
    private readonly SecuritySystem _securitySystem;
    private readonly List<PersonalDocument> _privateDocuments;
    
    private void ManageHouseholdBudget()
    {
        var monthlyBudget = _householdFinances.CalculateMonthlyBudget();
        AllocateHouseholdFunds(monthlyBudget);
    }
    
    private void UpdateSecuritySystem()
    {
        _securitySystem.ChangeAccessCodes();
        TestSecuritySensors();
    }
}
```

#### Mixing Public and Private Areas Randomly

```csharp
// BAD: Random organization with privacy levels scattered throughout
public class RandomlyOrganizedHouse
{
    // Disorganized layout - public and private mixed randomly
    private string _guestWelcomeMessage;       // Guest service hidden in private area
    public void ManagePersonalFinances() { }  // Personal business in public area
    public string Address { get; set; }       // Public info mixed with private
    private readonly BankAccount _account;    // Appropriate private placement
    public SecurityCode _securityCode;        // Security info exposed publicly!
    protected void WelcomeGuests() { }        // Guest service in family-only area
    private bool _hasGarage;                  // Basic house info hidden
    public List<PersonalDocument> PrivateDocs; // Private docs in public area!
    
    // Organization is confusing and inconsistent
}
```

**Why it's problematic**: This is like a house where the guest bathroom is hidden in the master bedroom, where personal financial planning happens in the living room where visitors can observe, and where house security codes are posted in public areas. Visitors can't find basic services, but they can access sensitive personal information.

**Better approach**:

```csharp
// GOOD: Logical organization with consistent privacy zones
public class LogicallyOrganizedHouse
{
    // === PUBLIC AREAS: Guests and visitors welcome ===
    
    // Basic house information accessible to visitors
    public string Address { get; set; }
    public string ContactPhone { get; set; }
    public bool HasGuestParking { get; set; }
    
    // Guest services and visitor functions
    public void WelcomeGuests(List<Guest> guests)
    {
        PrepareGuestAreas(guests);
        ProvideVisitorInformation(guests);
    }
    
    public void OfferGuestAmenities(Guest guest)
    {
        ShowGuestBathroom(guest);
        ProvideWifiAccess(guest);
        OfferRefreshments(guest);
    }
    
    // === FAMILY AREAS: Household members and close friends ===
    
    // Family information and coordination
    protected FamilySchedule HouseholdCalendar { get; set; }
    protected List<FamilyMember> FamilyMembers { get; set; }
    
    // Family coordination and household management
    protected void CoordinateFamilySchedule()
    {
        ResolveFamilyScheduleConflicts();
        PlanFamilyActivities();
    }
    
    protected void ManageHouseholdResponsibilities()
    {
        AssignChores(FamilyMembers);
        TrackChoreCompletion();
    }
    
    // === PRIVATE AREAS: Sensitive household management ===
    
    // Personal and financial information (most private)
    private readonly BankAccount _householdFinances;
    private readonly InsurancePolicy _homeInsurance;
    private readonly SecuritySystem _securitySystem;
    private readonly List<PersonalDocument> _sensitiveDocuments;
    
    // Sensitive household operations
    private void ProcessMortgagePayment()
    {
        var payment = _householdFinances.CalculateMonthlyPayment();
        _householdFinances.ProcessPayment(payment);
    }
    
    private void ReviewInsuranceCoverage()
    {
        var currentCoverage = _homeInsurance.GetCoverageDetails();
        AssessInsuranceAdequacy(currentCoverage);
    }
    
    private void UpdateHouseholdSecurity()
    {
        _securitySystem.RotateAccessCodes();
        ReviewSecurityEventLogs();
        TestAlarmSystems();
    }
}
```

#### Multiple Families in One House File

```csharp
// BAD: Multiple households trying to share one house file
// File: FamilyHouses.cs
public class SmithFamily
{
    public void ManageSmithHousehold() { }
}

public class JohnsonFamily  
{
    public void ManageJohnsonHousehold() { }
}

public class BrownFamily
{
    public void ManageBrownHousehold() { }
}

// Families interfere with each other's household management
```

**Why it's problematic**: This is like trying to organize multiple different families in the same house file. Each family has different household rules, privacy needs, and management styles that get confused when mixed together.

**Better approach**:

```csharp
// GOOD: One family per house file for clear household organization
// File: SmithFamilyHome.cs
public class SmithFamilyHome
{
    // Smith family's house organization and management
    public void ManageSmithHousehold() { }
}

// File: JohnsonFamilyHome.cs  
public class JohnsonFamilyHome
{
    // Johnson family's house organization and management  
    public void ManageJohnsonHousehold() { }
}

// Each family has their own house file with their specific organization
```

### Evolution Example

Let's see how class structure might evolve from chaotic house organization to excellent household layout:

**Initial Version - No house organization:**

```csharp
// Initial version - everything scattered randomly throughout the house
public class House
{
    public void DoEverything() { }
    private string something;
    public BankAccount money;
    private void WelcomeVisitors() { }
    public SecurityInfo security;
    protected string address;
    public void PayBills() { }
    private bool hasStuff;
    public void ManageFamily() { }
    
    // No logical organization - visitors can't find anything
    // Sensitive information mixed with public information
}
```

**Intermediate Version - Some organization but inconsistent privacy:**

```csharp
// Better organization but still inconsistent privacy zones
public class House
{
    // Some public organization
    public string Address { get; set; }
    public void WelcomeGuests() { }
    
    // Mixed privacy levels
    public BankAccount Finances; // Should be private
    protected void ManageFinances() { } // Financial management in family area?
    
    // Some appropriate private organization
    private SecuritySystem _security;
    private void UpdateSecurity() { }
    
    // Still some confusion about what goes where
}
```

**Final Version - Excellent house organization with clear privacy zones:**

```csharp
// Excellent household organization with logical privacy progression
public class ModernFamilyHome
{
    // === PUBLIC ENTRANCE AND COMMON AREAS ===
    // Information and services accessible to visitors
    
    public string Address { get; set; }
    public string ContactPhone { get; set; }
    public bool IsAcceptingVisitors { get; set; }
    public bool HasGuestParking { get; set; }
    
    // Guest services and visitor functions
    public void WelcomeGuests(List<Guest> guests)
    {
        SetupGuestAreas(guests.Count);
        ProvideVisitorAmenities(guests);
    }
    
    public void GiveHouseTour(Guest guest)
    {
        ShowPublicRooms(guest);
        ExplainHouseFeatures(guest);
        ProvideLocalAreaInformation(guest);
    }
    
    public void ArrangeGuestAccommodation(Guest guest, AccommodationType accommodation)
    {
        var guestRoom = PrepareGuestRoom(accommodation);
        ProvideBeddingAndTowels(guestRoom);
        ExplainHouseRulesForGuests(guest);
    }
    
    // === FAMILY LIVING AREAS ===
    // Shared family spaces and household coordination
    
    protected FamilySchedule HouseholdCalendar { get; set; }
    protected List<FamilyMember> FamilyMembers { get; set; }
    protected List<HouseholdRule> HouseRules { get; set; }
    protected EmergencyContacts EmergencyInformation { get; set; }
    
    // Family coordination and household management functions
    protected void CoordinateFamilyActivities()
    {
        var familyEvents = PlanWeeklyFamilyEvents();
        UpdateHouseholdCalendar(familyEvents);
        NotifyFamilyOfScheduleChanges();
    }
    
    protected void ManageHouseholdChores()
    {
        var choreAssignments = CreateChoreSchedule(FamilyMembers);
        AssignWeeklyResponsibilities(choreAssignments);
        TrackChoreCompletion();
    }
    
    protected void HandleFamilyEmergency(EmergencyType emergency)
    {
        var emergencyPlan = GetEmergencyPlan(emergency);
        NotifyEmergencyContacts(emergency);
        ExecuteEmergencyProcedures(emergencyPlan);
    }
    
    // === PRIVATE MASTER BEDROOM AND PERSONAL STORAGE ===
    // Sensitive household management and personal business
    
    private readonly BankAccount _householdFinances;
    private readonly MortgageAccount _mortgageInformation;
    private readonly InsurancePolicy _homeInsurance;
    private readonly SecuritySystem _homeSecuritySystem;
    private readonly List<LegalDocument> _importantDocuments;
    private readonly Dictionary<string, PersonalItem> _privateStorage;
    
    // Personal household management functions
    private void ProcessMonthlyFinances()
    {
        var monthlyBudget = CalculateMonthlyBudget();
        _householdFinances.AllocateMonthlyFunds(monthlyBudget);
        ProcessMortgagePayment();
        ReviewInsurancePremiums();
        UpdateFinancialRecords();
    }
    
    private void ManageHouseholdSecurity()
    {
        _homeSecuritySystem.UpdateAccessCodes();
        ReviewSecurityEventHistory();
        TestAlarmSystemFunctionality();
        UpdateEmergencyContactInformation();
    }
    
    private void OrganizePersonalDocuments()
    {
        var documentsNeedingRenewal = _importantDocuments
            .Where(d => d.ExpirationDate < DateTime.Now.AddMonths(6))
            .ToList();
            
        ScheduleDocumentRenewal(documentsNeedingRenewal);
        ArchiveExpiredDocuments();
        UpdateDocumentInventory();
    }
    
    private void ManagePersonalStorage()
    {
        OrganizeSeasonalItems();
        DonateUnneededItems();
        InventoryPersonalBelongings();
    }
}
```

### Deeper Understanding

#### House Organization Principles

Good class structure follows the same principles as excellent home organization:

1. **Accessibility Zones**: Organize by who should have access to each area
2. **Logical Flow**: Arrange areas to flow naturally from public to private
3. **Functional Grouping**: Keep related functions in the same area of the house
4. **Privacy Protection**: Sensitive information and operations stay in private areas

#### Privacy Level Guidelines

**Public Areas (Public Members)**:
- Information and services that visitors and guests should be able to access
- Basic house information (address, contact info)
- Guest services and visitor functions
- Place these at the "front" of your class

**Family Areas (Protected Members)**:
- Information and functions for household members and extended family
- Family coordination and shared household management
- Information that's not sensitive but also not for general public access
- Place these in the "middle" of your class

**Private Areas (Private Members)**:
- Sensitive household management and personal business
- Financial information, security systems, personal documents
- Implementation details that should be hidden from outside access
- Place these at the "back" of your class, grouped by access level

#### House Layout Best Practices

**Within Each Privacy Zone**:
1. List house characteristics (properties) first
2. List house functions (methods) second  
3. List storage areas (fields) last

**Example Organization**:
```csharp
public class FamilyHome
{
    // === PUBLIC ZONE ===
    public string Address { get; set; }          // Public house info
    public bool HasGuestParking { get; set; }    // Public house info
    
    public void WelcomeGuests() { }              // Public guest services
    public void GiveHouseTour() { }              // Public guest services
    
    // === FAMILY ZONE ===  
    protected FamilySchedule Calendar { get; set; } // Family coordination info
    
    protected void PlanFamilyEvent() { }         // Family coordination functions
    
    // === PRIVATE ZONE ===
    private readonly BankAccount _finances;      // Private household storage
    private readonly SecuritySystem _security;   // Private household storage
    
    private void ManageBudget() { }              // Private household management
    private void UpdateSecurity() { }           // Private household management
}
```

This organization makes it easy for different types of users (visitors, family members, household managers) to quickly find the information and services they need while maintaining appropriate privacy and security.

Good class structure makes your code as organized and navigable as a well-designed home where everyone knows where things belong and can easily find what they need.