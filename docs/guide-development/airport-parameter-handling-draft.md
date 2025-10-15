# 17. Parameter Handling

```csharp
// Airport security screening (parameter validation) at the checkpoint entrance
public FlightReservation BookFlight(string passportNumber, DateTime departureDate, string destination)
{
    // Security checkpoint - validate all travel documents at the entrance
    if (string.IsNullOrEmpty(passportNumber))
        throw new ArgumentException("Passport number is required for international travel", nameof(passportNumber));
        
    if (departureDate < DateTime.Today)
        throw new ArgumentException("Cannot book flights for past dates", nameof(departureDate));
        
    if (string.IsNullOrEmpty(destination))
        throw new ArgumentException("Destination is required for flight booking", nameof(destination));
    
    // All documents verified - proceed to booking
    return ProcessFlightBooking(passportNumber, departureDate, destination);
}

// Optional travel documents (nullable parameters) for different trip types
public BoardingPass CheckInForFlight(string confirmationCode, TravelDocument? visa = null, SpecialAssistance? assistance = null)
{
    // Required travel document validation
    if (string.IsNullOrEmpty(confirmationCode))
        throw new ArgumentException("Confirmation code required for check-in", nameof(confirmationCode));
    
    // Optional documents - some passengers need them, others don't
    var requiresVisa = IsInternationalDestination(confirmationCode);
    if (requiresVisa && visa == null)
        throw new ArgumentException("Visa required for international travel", nameof(visa));
    
    var boardingPass = GenerateBoardingPass(confirmationCode);
    
    // Handle optional special assistance if provided
    if (assistance != null)
    {
        ArrangeSpecialAssistance(boardingPass, assistance);
    }
    
    return boardingPass;
}

// Clear travel preference questions (descriptive boolean parameter names)
public SeatAssignment SelectSeat(Flight flight, bool windowSeat = false, bool extraLegroom = false, bool nearRestroom = false)
{
    var availableSeats = GetAvailableSeats(flight);
    
    // Filter seats based on passenger preferences
    if (windowSeat)
        availableSeats = availableSeats.Where(s => s.IsWindowSeat).ToList();
        
    if (extraLegroom)
        availableSeats = availableSeats.Where(s => s.HasExtraLegroom).ToList();
        
    if (nearRestroom)
        availableSeats = availableSeats.Where(s => s.IsNearRestroom).ToList();
    
    return availableSeats.FirstOrDefault() ?? SeatAssignment.None;
}

// Using named check-in options for clear travel preferences
public void BookFlightWithPreferences()
{
    // Clear preference selections using named arguments
    var reservation = BookFlightWithOptions(
        destination: "Paris",
        departureDate: DateTime.Today.AddDays(30),
        windowSeat: true,           // Clear preference
        mealPreference: "vegetarian",
        extraLegroom: false,        // Clear preference
        priorityBoarding: true      // Clear preference
    );
}

// Travel document verification with proper error messages
public DocumentVerificationResult VerifyTravelDocuments(TravelDocument passport, TravelDocument? visa, TravelDocument? vaccinationCard)
{
    // Checkpoint validation with specific error reporting
    var passportValidation = ValidatePassport(passport);
    if (!passportValidation.IsValid)
    {
        return DocumentVerificationResult.Failed($"Passport verification failed: {passportValidation.ErrorMessage}");
    }
    
    // Optional document validation - only check if passenger provided them
    if (visa != null)
    {
        var visaValidation = ValidateVisa(visa);
        if (!visaValidation.IsValid)
        {
            return DocumentVerificationResult.Failed($"Visa verification failed: {visaValidation.ErrorMessage}");
        }
    }
    
    if (vaccinationCard != null)
    {
        var vaccinationValidation = ValidateVaccinationCard(vaccinationCard);
        if (!vaccinationValidation.IsValid)
        {
            return DocumentVerificationResult.Failed($"Vaccination verification failed: {vaccinationValidation.ErrorMessage}");
        }
    }
    
    return DocumentVerificationResult.Success("All travel documents verified");
}

// BAD: Unclear check-in questions that confuse passengers
public FlightReservation BookFlight(string code, DateTime date, bool flag1, bool flag2, bool option)
{
    // Passengers don't understand what these yes/no questions mean
    return ProcessBooking(code, date, flag1, flag2, option);
}

// GOOD: Clear check-in questions with obvious meanings
public FlightReservation BookFlight(string confirmationCode, DateTime departureDate, 
    bool requestWindowSeat = false, bool needsWheelchairAssistance = false, bool wantsPriorityBoarding = false)
{
    // Passengers clearly understand each travel preference question
    return ProcessBooking(confirmationCode, departureDate, requestWindowSeat, needsWheelchairAssistance, wantsPriorityBoarding);
}

// Airport check-in kiosk with clear options
public class AirportCheckInKiosk
{
    /// <summary>
    /// Processes passenger check-in with travel preferences and document verification.
    /// </summary>
    /// <param name="confirmationCode">Required flight confirmation code from booking</param>
    /// <param name="travelDocument">Required passport or government ID for identification</param>
    /// <param name="seatPreference">Optional seat location preference (window, aisle, middle)</param>
    /// <param name="requiresAssistance">Whether passenger needs special assistance during travel</param>
    /// <param name="hasBags">Whether passenger is checking bags (affects check-in process)</param>
    /// <returns>Boarding pass with seat assignment and travel details</returns>
    public CheckInResult ProcessCheckIn(
        string confirmationCode, 
        TravelDocument travelDocument, 
        SeatPreference? seatPreference = null,
        bool requiresAssistance = false,
        bool hasBags = false)
    {
        // Security checkpoint validation at entrance
        var confirmationIsValid = !string.IsNullOrEmpty(confirmationCode);
        if (!confirmationIsValid)
        {
            return CheckInResult.Failed("Confirmation code is required for check-in");
        }
        
        var documentIsValid = travelDocument != null && ValidateDocument(travelDocument);
        if (!documentIsValid)
        {
            return CheckInResult.Failed("Valid travel document required for check-in");
        }
        
        try
        {
            // Process check-in with validated information
            var flight = GetFlight(confirmationCode);
            var seat = AssignSeat(flight, seatPreference);
            
            var boardingPass = new BoardingPass
            {
                Flight = flight,
                Seat = seat,
                PassengerName = travelDocument.Name,
                RequiresAssistance = requiresAssistance,
                HasCheckedBags = hasBags
            };
            
            if (hasBags)
            {
                ProcessBaggageCheck(boardingPass);
            }
            
            if (requiresAssistance)
            {
                ArrangeSpecialAssistance(boardingPass);
            }
            
            return CheckInResult.Success(boardingPass);
        }
        catch (FlightNotFoundException ex)
        {
            return CheckInResult.Failed($"Flight not found: {ex.Message}");
        }
        catch (SeatUnavailableException ex)
        {
            return CheckInResult.Failed($"Seat assignment failed: {ex.Message}");
        }
    }
}

// Using check-in kiosk with clear travel preference selections
public void CheckInWithPreferences()
{
    var checkInResult = _kiosk.ProcessCheckIn(
        confirmationCode: "ABC123",
        travelDocument: myPassport,
        seatPreference: SeatPreference.Window,     // Named preference selection
        requiresAssistance: false,                 // Clear yes/no question
        hasBags: true                             // Clear yes/no question
    );
}

// Travel group coordination - handling multiple passengers with different requirements
public GroupCheckInResult CheckInTravelGroup(List<GroupMember> groupMembers)
{
    var checkInResults = new List<CheckInResult>();
    
    foreach (var member in groupMembers)
    {
        // Validate each group member's travel documents
        var memberValidation = ValidateGroupMemberDocuments(member);
        if (!memberValidation.IsValid)
        {
            return GroupCheckInResult.Failed($"Group member {member.Name} has invalid documents: {memberValidation.ErrorMessage}");
        }
        
        // Process individual check-in with member-specific preferences
        var individualCheckIn = ProcessCheckIn(
            confirmationCode: member.ConfirmationCode,
            travelDocument: member.TravelDocument,
            seatPreference: member.PreferredSeat,
            requiresAssistance: member.NeedsAssistance,
            hasBags: member.HasCheckedBags
        );
        
        checkInResults.Add(individualCheckIn);
    }
    
    return new GroupCheckInResult(checkInResults);
}

// Flight manifest processing with comprehensive passenger validation
public FlightManifest PrepareFlightManifest(string flightNumber, List<PassengerRecord> passengers)
{
    // Flight security requires comprehensive passenger validation
    if (string.IsNullOrEmpty(flightNumber))
        throw new ArgumentException("Flight number required for manifest preparation", nameof(flightNumber));
        
    if (passengers == null)
        throw new ArgumentNullException(nameof(passengers), "Passenger list required for flight manifest");
        
    if (!passengers.Any())
        throw new ArgumentException("At least one passenger required for flight manifest", nameof(passengers));
    
    // Validate each passenger record at security checkpoint
    var validationErrors = new List<string>();
    
    foreach (var passenger in passengers)
    {
        var passengerValidation = ValidatePassengerRecord(passenger);
        if (!passengerValidation.IsValid)
        {
            validationErrors.Add($"Passenger {passenger.Name}: {passengerValidation.ErrorMessage}");
        }
    }
    
    if (validationErrors.Any())
    {
        throw new ArgumentException($"Passenger validation failed: {string.Join("; ", validationErrors)}");
    }
    
    // All security checks passed - prepare flight manifest
    return new FlightManifest
    {
        FlightNumber = flightNumber,
        Passengers = passengers,
        TotalPassengers = passengers.Count,
        ManifestPreparedAt = DateTime.Now
    };
}
```

### Core Principles

- Verify all travel documents (validate parameters) at the security checkpoint before allowing airport access
- Mark optional travel documents clearly (use nullable annotations) so passengers know what they might need
- Ask clear yes/no questions (descriptive boolean parameter names) that passengers can easily understand
- Use specific check-in options (named arguments) when selecting travel preferences to avoid confusion
- Provide helpful error messages when travel documents are invalid or missing
- Validate passenger information early in the check-in process to prevent problems at the gate

### Why It Matters

Think of parameter handling as an airport security and check-in system. Just as airports must verify passenger information, validate travel documents, and clearly communicate requirements before allowing access to flights, your methods must validate input parameters and clearly communicate their requirements before processing begins.

Poor parameter handling is like an airport with unclear check-in procedures, confusing questions about travel preferences, and security checkpoints that don't validate documents properly. This creates confusion, delays, and potential security issues.

Professional airport check-in systems provide:

1. **Clear Document Requirements**: Passengers know exactly what information and documents they need
2. **Early Validation**: Problems are caught at check-in, not at the gate when it's too late
3. **Helpful Error Messages**: Clear explanations when travel documents are invalid or missing
4. **Obvious Preference Options**: Yes/no questions and selections are clearly labeled
5. **Consistent Process**: Similar travel scenarios use similar check-in procedures

When parameter handling is poor, it's like an airport where passengers don't know what documents they need, where error messages are confusing, and where check-in procedures are inconsistent across different flights.

### Common Mistakes

#### Poor Security Checkpoint Procedures (Inadequate Parameter Validation)

```csharp
// BAD: Allowing passengers through security without proper document verification
public FlightBooking ProcessFlightBooking(string destination, DateTime date, PassengerInfo passenger)
{
    // No security checkpoint - passengers proceed without validation
    var booking = new FlightBooking(destination, date, passenger);
    return SaveBooking(booking);
    // Problems discovered later at the gate when it's too late to fix
}

public void AssignSeat(Passenger passenger, string seatNumber)
{
    // No verification of passenger or seat information
    passenger.AssignedSeat = seatNumber;
    // What if passenger is null? What if seat number is invalid?
}
```

**Why it's problematic**: This is like an airport that lets anyone through security without checking IDs or tickets. Problems are only discovered later when passengers try to board, creating delays and confusion at the gate.

**Better approach**:

```csharp
// GOOD: Thorough security checkpoint validation before processing
public FlightBooking ProcessFlightBooking(string destination, DateTime date, PassengerInfo passenger)
{
    // Security checkpoint validates all travel documents
    if (string.IsNullOrEmpty(destination))
        throw new ArgumentException("Destination is required for flight booking", nameof(destination));
        
    if (date < DateTime.Today)
        throw new ArgumentException("Cannot book flights for past dates", nameof(date));
        
    if (passenger == null)
        throw new ArgumentNullException(nameof(passenger), "Passenger information is required");
        
    if (string.IsNullOrEmpty(passenger.Name))
        throw new ArgumentException("Passenger name is required", nameof(passenger));
    
    // All documents verified - proceed to booking
    var booking = new FlightBooking(destination, date, passenger);
    return SaveBooking(booking);
}

public void AssignSeat(Passenger passenger, string seatNumber)
{
    if (passenger == null)
        throw new ArgumentNullException(nameof(passenger), "Passenger required for seat assignment");
        
    if (string.IsNullOrEmpty(seatNumber))
        throw new ArgumentException("Seat number required for assignment", nameof(seatNumber));
        
    if (!IsValidSeatNumber(seatNumber))
        throw new ArgumentException($"Invalid seat number: {seatNumber}", nameof(seatNumber));
    
    passenger.AssignedSeat = seatNumber;
}
```

#### Confusing Check-in Questions (Unclear Boolean Parameters)

```csharp
// BAD: Unclear yes/no questions that confuse passengers
public BoardingPass CheckInPassenger(string code, bool flag1, bool flag2, bool option)
{
    // Passengers don't understand what these questions mean
    return ProcessCheckIn(code, flag1, flag2, option);
}

// Usage is completely unclear
var boardingPass = CheckInPassenger("ABC123", true, false, true); // What do these mean?
```

**Why it's problematic**: This is like airport check-in kiosks with yes/no buttons labeled "Option 1," "Flag 2," and "Choice 3." Passengers have no idea what they're agreeing to or selecting.

**Better approach**:

```csharp
// GOOD: Clear check-in questions that passengers can easily understand
public BoardingPass CheckInPassenger(string confirmationCode, 
    bool requestWindowSeat = false, 
    bool needsWheelchairAssistance = false, 
    bool wantsPriorityBoarding = false)
{
    return ProcessCheckIn(confirmationCode, requestWindowSeat, needsWheelchairAssistance, wantsPriorityBoarding);
}

// Usage with named check-in options is crystal clear
var boardingPass = CheckInPassenger(
    confirmationCode: "ABC123",
    requestWindowSeat: true,        // Clear preference
    needsWheelchairAssistance: false, // Clear service question
    wantsPriorityBoarding: true     // Clear upgrade option
);
```

#### No Travel Document Specifications (Missing Nullable Annotations)

```csharp
// BAD: Not indicating which travel documents are optional vs required
public CheckInResult ProcessInternationalCheckIn(string passport, string visa, string vaccinationCard)
{
    // Are visa and vaccination card always required? Sometimes required? Never required?
    // Passengers don't know what documents to bring
    
    if (string.IsNullOrEmpty(visa))
    {
        // This suggests visa is required, but parameter doesn't indicate that
        return CheckInResult.Failed("Visa required");
    }
    
    // Similar confusion for vaccination card
}
```

**Why it's problematic**: This is like an airline that doesn't clearly communicate which travel documents are required vs. optional for different destinations. Passengers don't know what to prepare for their trip.

**Better approach**:

```csharp
// GOOD: Clear travel document requirements with optional document indicators
public CheckInResult ProcessInternationalCheckIn(string passport, string? visa, string? vaccinationCard)
{
    // Required document validation
    if (string.IsNullOrEmpty(passport))
        throw new ArgumentException("Passport required for international travel", nameof(passport));
    
    // Optional document validation - only validate if passenger provided them
    var destinationRequiresVisa = CheckVisaRequirement(passport);
    if (destinationRequiresVisa && string.IsNullOrEmpty(visa))
    {
        return CheckInResult.Failed("Visa required for this destination");
    }
    
    var destinationRequiresVaccination = CheckVaccinationRequirement(passport);
    if (destinationRequiresVaccination && string.IsNullOrEmpty(vaccinationCard))
    {
        return CheckInResult.Failed("Vaccination documentation required for this destination");
    }
    
    return CheckInResult.Success("All required documents verified");
}
```

#### Poor Security Checkpoint Error Messages

```csharp
// BAD: Unhelpful security messages that don't guide passengers
public void ValidateFlightBooking(FlightBookingRequest request)
{
    if (request.DepartureDate < DateTime.Today)
        throw new Exception("Bad date"); // Unhelpful message
        
    if (string.IsNullOrEmpty(request.PassengerName))
        throw new Exception("Error"); // No guidance for passenger
        
    if (request.PassengerAge < 0)
        throw new Exception("Invalid"); // Doesn't explain the problem
}
```

**Why it's problematic**: This is like airport security giving passengers error messages like "Problem with documents" or "Invalid information" without explaining what specifically is wrong or how to fix it.

**Better approach**:

```csharp
// GOOD: Helpful security checkpoint messages that guide passengers
public void ValidateFlightBooking(FlightBookingRequest request)
{
    if (request.DepartureDate < DateTime.Today)
        throw new ArgumentException($"Cannot book flights for past dates. Please select a date from {DateTime.Today:yyyy-MM-dd} onward.", nameof(request.DepartureDate));
        
    if (string.IsNullOrEmpty(request.PassengerName))
        throw new ArgumentException("Passenger name is required for flight booking. Please provide the full name as it appears on your travel document.", nameof(request.PassengerName));
        
    if (request.PassengerAge < 0)
        throw new ArgumentException($"Invalid passenger age: {request.PassengerAge}. Age must be 0 or greater.", nameof(request.PassengerAge));
}
```

### Evolution Example

Let's see how parameter handling might evolve from chaotic airport procedures to professional check-in systems:

**Initial Version - No security checkpoints:**

```csharp
// Initial version - passengers proceed without any document verification
public class FlightService
{
    public object BookFlight(object data)
    {
        // No security checkpoint - anyone can proceed
        var flight = CreateFlight(data);
        return flight;
    }
    
    public void ProcessPassenger(object passenger, object options)
    {
        // No document verification or preference clarification
        DoSomething(passenger, options);
    }
}
```

**Intermediate Version - Basic checkpoints but poor procedures:**

```csharp
// Better security but poor checkpoint procedures
public class FlightService
{
    public FlightBooking BookFlight(string destination, DateTime date, object passenger)
    {
        // Some validation but poor error reporting
        if (destination == null || date == DateTime.MinValue)
            throw new Exception("Invalid"); // Unhelpful error message
        
        return CreateBooking(destination, date, passenger);
    }
    
    public void CheckInPassenger(string code, bool option1, bool option2)
    {
        // Boolean questions without clear meaning
        ProcessCheckIn(code, option1, option2);
    }
}
```

**Final Version - Professional airport security and check-in systems:**

```csharp
// Excellent parameter handling with professional airport procedures
public class FlightService
{
    /// <summary>
    /// Books a flight reservation with comprehensive passenger validation.
    /// </summary>
    /// <param name="destination">Flight destination city or airport code</param>
    /// <param name="departureDate">Requested departure date (must be today or future)</param>
    /// <param name="passenger">Passenger information for booking and security verification</param>
    /// <param name="travelPreferences">Optional travel preferences for seat and service selection</param>
    /// <returns>Flight booking confirmation with reservation details</returns>
    /// <exception cref="ArgumentException">Thrown when booking parameters are invalid</exception>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are missing</exception>
    public FlightBooking BookFlight(
        string destination, 
        DateTime departureDate, 
        PassengerInfo passenger,
        TravelPreferences? travelPreferences = null)
    {
        // Comprehensive security checkpoint validation
        if (string.IsNullOrEmpty(destination))
            throw new ArgumentException("Destination is required for flight booking", nameof(destination));
            
        if (departureDate < DateTime.Today)
            throw new ArgumentException($"Cannot book flights for past dates. Please select {DateTime.Today:yyyy-MM-dd} or later.", nameof(departureDate));
            
        if (passenger == null)
            throw new ArgumentNullException(nameof(passenger), "Passenger information is required for flight booking");
            
        if (string.IsNullOrEmpty(passenger.Name))
            throw new ArgumentException("Passenger name is required as it appears on travel documents", nameof(passenger));
        
        // Security validation passed - proceed with booking
        var booking = CreateFlightBooking(destination, departureDate, passenger);
        
        // Apply optional travel preferences if provided
        if (travelPreferences != null)
        {
            ApplyTravelPreferences(booking, travelPreferences);
        }
        
        return booking;
    }
    
    /// <summary>
    /// Processes passenger check-in with travel document verification and preference selection.
    /// </summary>
    /// <param name="confirmationCode">Flight confirmation code from original booking</param>
    /// <param name="travelDocument">Required government-issued travel identification</param>
    /// <param name="requestWindowSeat">Whether passenger prefers window seat assignment</param>
    /// <param name="needsWheelchairAssistance">Whether passenger requires mobility assistance</param>
    /// <param name="hasCheckedBags">Whether passenger is checking bags (affects boarding process)</param>
    /// <returns>Check-in result with boarding pass or error details</returns>
    public CheckInResult CheckInPassenger(
        string confirmationCode,
        TravelDocument travelDocument,
        bool requestWindowSeat = false,
        bool needsWheelchairAssistance = false, 
        bool hasCheckedBags = false)
    {
        // Security checkpoint validation with specific error guidance
        var confirmationValidation = ValidateConfirmationCode(confirmationCode);
        if (!confirmationValidation.IsValid)
        {
            return CheckInResult.Failed($"Confirmation code verification failed: {confirmationValidation.ErrorMessage}");
        }
        
        var documentValidation = ValidateTravelDocument(travelDocument);
        if (!documentValidation.IsValid)
        {
            return CheckInResult.Failed($"Travel document verification failed: {documentValidation.ErrorMessage}");
        }
        
        try
        {
            // Process check-in with validated information and clear preferences
            var flight = GetFlight(confirmationCode);
            var seatAssignment = SelectSeat(flight, requestWindowSeat);
            
            var boardingPass = new BoardingPass
            {
                Flight = flight,
                Seat = seatAssignment,
                PassengerName = travelDocument.Name,
                BoardingGroup = DetermineBoardingGroup(flight, needsWheelchairAssistance),
                RequiresAssistance = needsWheelchairAssistance
            };
            
            // Handle optional services based on passenger preferences
            if (hasCheckedBags)
            {
                var baggageResult = ProcessCheckedBaggage(boardingPass);
                boardingPass.BaggageClaimTickets = baggageResult.ClaimTickets;
            }
            
            if (needsWheelchairAssistance)
            {
                ArrangeWheelchairAssistance(boardingPass);
            }
            
            return CheckInResult.Success(boardingPass);
        }
        catch (FlightNotFoundException ex)
        {
            return CheckInResult.Failed($"Flight not found for confirmation code {confirmationCode}: {ex.Message}");
        }
        catch (SeatAssignmentException ex)
        {
            return CheckInResult.Failed($"Seat assignment failed: {ex.Message}");
        }
    }
    
    // Airport utility functions for document validation
    private static bool IsValidPassportNumber(string passportNumber)
    {
        // Utility function for document format validation
        return !string.IsNullOrEmpty(passportNumber) && 
               passportNumber.Length >= 6 && 
               passportNumber.All(char.IsLetterOrDigit);
    }
    
    private static bool IsValidFlightDate(DateTime flightDate)
    {
        // Utility function for date validation
        var maxBookingAdvance = TimeSpan.FromDays(365);
        return flightDate >= DateTime.Today && 
               flightDate <= DateTime.Today.Add(maxBookingAdvance);
    }
}

// Group check-in coordination with clear passenger requirements
public class GroupCheckInService
{
    /// <summary>
    /// Processes check-in for a travel group with coordinated seat assignments.
    /// </summary>
    /// <param name="groupLeaderConfirmation">Confirmation code for the group leader's booking</param>
    /// <param name="groupMembers">List of all group members with their individual travel documents</param>
    /// <param name="requestAdjacentSeats">Whether group prefers to sit together</param>
    /// <param name="hasGroupLuggage">Whether group is checking bags together</param>
    /// <returns>Group check-in result with individual boarding passes for each member</returns>
    public GroupCheckInResult CheckInTravelGroup(
        string groupLeaderConfirmation,
        List<GroupPassenger> groupMembers,
        bool requestAdjacentSeats = false,
        bool hasGroupLuggage = false)
    {
        // Group security checkpoint - validate group leader and all members
        if (string.IsNullOrEmpty(groupLeaderConfirmation))
            throw new ArgumentException("Group leader confirmation code required", nameof(groupLeaderConfirmation));
            
        if (groupMembers == null || !groupMembers.Any())
            throw new ArgumentException("At least one group member required for group check-in", nameof(groupMembers));
        
        // Validate each group member's travel documents
        var documentValidationResults = new List<DocumentValidationResult>();
        
        foreach (var member in groupMembers)
        {
            var validation = ValidateGroupMemberDocuments(member);
            documentValidationResults.Add(validation);
            
            if (!validation.IsValid)
            {
                return GroupCheckInResult.Failed(
                    $"Group member {member.PassengerName} failed document verification: {validation.ErrorMessage}");
            }
        }
        
        // All group security checks passed - process coordinated check-in
        try
        {
            var groupBooking = GetGroupBooking(groupLeaderConfirmation);
            var seatAssignments = AssignGroupSeats(groupBooking, groupMembers, requestAdjacentSeats);
            
            var boardingPasses = new List<BoardingPass>();
            
            for (int i = 0; i < groupMembers.Count; i++)
            {
                var member = groupMembers[i];
                var seat = seatAssignments[i];
                
                var boardingPass = new BoardingPass
                {
                    Flight = groupBooking.Flight,
                    Seat = seat,
                    PassengerName = member.PassengerName,
                    IsGroupMember = true,
                    GroupLeader = groupLeaderConfirmation
                };
                
                boardingPasses.Add(boardingPass);
            }
            
            // Handle group luggage if applicable
            if (hasGroupLuggage)
            {
                var luggageResult = ProcessGroupLuggage(groupBooking, groupMembers);
                foreach (var boardingPass in boardingPasses)
                {
                    boardingPass.GroupLuggageTickets = luggageResult.ClaimTickets;
                }
            }
            
            return GroupCheckInResult.Success(boardingPasses);
        }
        catch (GroupBookingException ex)
        {
            return GroupCheckInResult.Failed($"Group booking processing failed: {ex.Message}");
        }
    }
}
```

### Deeper Understanding

#### Airport Security Design Principles

Good parameter handling follows the same principles as professional airport security and check-in procedures:

1. **Early Validation**: Check all documents at the security checkpoint, not at the gate
2. **Clear Requirements**: Passengers know exactly what documents and information they need
3. **Helpful Guidance**: Error messages explain what's wrong and how to fix it
4. **Consistent Procedures**: Similar travel scenarios use similar validation and check-in processes

#### Travel Document Classification

**Required Documents (Non-Nullable Parameters)**:
```csharp
public void BookFlight(string passport, DateTime departureDate)
// All passengers must provide these for any flight
```

**Optional Documents (Nullable Parameters)**:
```csharp
public void BookInternationalFlight(string passport, string? visa, string? vaccinationCard)
// Some destinations require these, others don't
```

**Travel Preferences (Boolean Parameters with Defaults)**:
```csharp
public void SelectServices(bool windowSeat = false, bool extraLegroom = false)
// Passengers can choose these options, but they default to standard service
```

#### Security Checkpoint Best Practices

**Validation Order**:
1. Check for null parameters first
2. Validate parameter format and content
3. Verify business rules and constraints
4. Provide specific error messages with guidance

**Error Message Quality**:
- Include parameter name using `nameof()`
- Explain what's wrong and what's expected
- Provide examples of valid values when helpful
- Guide passengers toward successful completion

**Performance Considerations**:
- Validate cheap operations first (null checks)
- Validate expensive operations last (database lookups)
- Fail fast to avoid unnecessary processing

Good parameter handling makes your methods as reliable and user-friendly as a professional airport with clear security procedures, helpful staff, and efficient check-in systems that get passengers to their flights safely and on time.