# 5. Exception Handling and Error Returns

```csharp
// Return null for "not found" scenarios
public Patient FindPatient(string patientId)
{
    // Check if patient is registered in the hospital system
    return _patientRepository.GetByPatientId(patientId);  // May be null
}

// Throw for invalid inputs & exceptional conditions
public void AdmitPatient(Patient patient, Ward ward)
{
    // Verify patient is eligible for this ward (like pediatrics)
    if (patient.Age > ward.MaxAge)
        throw new ArgumentException("Patient too old for this ward", nameof(patient));
    
    // Critical system failure - like oxygen system failure
    if (!ward.HasAvailableBeds)
    {
        throw new WardCapacityException("No available beds in ward");
    }
}

// Try pattern for operations expected to fail sometimes
public bool TryGetMedicalHistory(string patientId, out MedicalHistory history)
{
    // Some patients might not have history records - this is normal
    history = null;
    
    if (string.IsNullOrEmpty(patientId))
        return false;
    
    var patient = _patientRepository.GetByPatientId(patientId);
    if (patient == null)
        return false;
        
    history = _medicalRecords.FindHistory(patientId);
    return history != null;
}

// Only catch exceptions you can handle
try 
{
    PerformSurgery(patient, surgeryType);
}
catch (InstrumentFailureException ex)
{
    // Specific handling for equipment failure - like calling for replacement tools
    Logger.Warn($"Instrument failure: {ex.InstrumentName}");
    RequestBackupInstruments(ex.InstrumentName);
}
catch (PowerOutageException ex)
{
    // Handle power issues - like activating backup generators
    Logger.Error($"Power issue during surgery: {ex.Message}");
    ActivateBackupPower();
}
// Let other medical emergencies bubble up to the chief surgeon

// Using finally for cleanup
public void PerformLabTest(Patient patient)
{
    LabEquipment equipment = null;
    try
    {
        equipment = ReserveLabEquipment();
        var sample = CollectSample(patient);
        AnalyzeSample(equipment, sample);
    }
    catch (SampleContaminationException ex)
    {
        Logger.Error($"Sample contaminated: {ex.Message}");
    }
    finally
    {
        // Always sanitize equipment - like cleanup protocol after any procedure
        equipment?.Sanitize();
        equipment?.Release();
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
3. **Preserves Resources**: Proper cleanup prevents resource leaks, like making sure all equipment is sterilized after procedures
4. **Guides Recovery**: Helps the application recover gracefully, like emergency protocols help a hospital return to normal operations

Poor exception handling can crash applications, corrupt data, leak resources, and make debugging nearly impossible.

### Common Mistakes

#### Using Exceptions for Normal Control Flow

```csharp
// BAD: Using exceptions for expected conditions
public int GetPatientAge(string patientId)
{
    try
    {
        var patient = _patientRepository.GetByPatientId(patientId);
        return patient.Age;
    }
    catch (NullReferenceException)
    {
        return 0; // Default age if patient not found
    }
}
```

**Why it's problematic**: This is like triggering the hospital's code blue emergency system to ask if a patient is available for a routine check-up. Exceptions should be for exceptional conditions, not normal program flow. They're expensive to create and process.

**Better approach**:

```csharp
// GOOD: Check for null instead of catching exceptions
public int GetPatientAge(string patientId)
{
    var patient = _patientRepository.GetByPatientId(patientId);
    return patient?.Age ?? 0; // Null conditional and null coalescing operators
}
```

#### Catching Exception Base Class

```csharp
// BAD: Catching all exceptions
try
{
    PerformSurgery(patient, surgeryType);
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
    PerformSurgery(patient, surgeryType);
}
catch (PatientComplicationException ex)
{
    Logger.Warn($"Patient complication: {ex.Message}");
    NotifySpecialist(ex.ComplicationType, patient);
}
catch (AnesthesiaFailureException ex)
{
    Logger.Error($"Anesthesia issue: {ex.Message}");
    CallAnesthesiologist(ex.Details);
}
catch (Exception ex) // Last resort with explicit re-throw
{
    Logger.Critical($"Unexpected emergency during surgery: {ex.Message}");
    throw; // Re-throw to let chief surgeon or hospital administrator handle it
}
```

#### Swallowing Exceptions

```csharp
// BAD: Exception disappears with no handling
try
{
    NotifyPatientFamily(patient, surgeryOutcome);
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
    NotifyPatientFamily(patient, surgeryOutcome);
}
catch (NotificationException ex)
{
    Logger.Warn($"Failed to notify family: {ex.Message}");
    // Store for retry later or use alternative contact method
    _notificationRetryQueue.Enqueue(new FamilyNotification(patient, surgeryOutcome));
}
```

#### Not Cleaning Up Resources

```csharp
// BAD: Resource leak if exception occurs
public void PerformXRay(Patient patient)
{
    var xrayMachine = XRayDepartment.ReserveMachine();
    // If an exception happens here, machine is never released
    var xrayImage = xrayMachine.CaptureImage(patient);
    AnalyzeXRayImage(xrayImage);
    xrayMachine.Release(); // May never reach this line
}
```

**Why it's problematic**: This is like reserving an operating room for surgery but forgetting to release it if there's an emergency, preventing other patients from using it. Unreleased resources can cause shortages and system failures.

**Better approach**:

```csharp
// GOOD: Using to ensure resource cleanup
public void PerformXRay(Patient patient)
{
    using (var xrayMachine = XRayDepartment.ReserveMachine())
    {
        var xrayImage = xrayMachine.CaptureImage(patient);
        AnalyzeXRayImage(xrayImage);
        // xrayMachine automatically released when exiting the using block
    }
}

// Or in C# 8+:
public void PerformXRay(Patient patient)
{
    using var xrayMachine = XRayDepartment.ReserveMachine();
    var xrayImage = xrayMachine.CaptureImage(patient);
    AnalyzeXRayImage(xrayImage);
    // xrayMachine automatically released at end of method
}
```

### Evolution Example

Let's see how error handling in a method might evolve:

**Initial Version - Poor error handling:**

```csharp
// Like a hospital with no emergency protocols
public Patient AdmitPatientToEmergency(string name, string condition)
{
    var patient = new Patient
    {
        Name = name,
        ReportedCondition = condition
    };
    
    _hospitalDatabase.RegisterPatient(patient);
    _nursingStation.AssignBed(patient);
    return patient;
}
```

**Intermediate Version - Basic exception handling:**

```csharp
// Like a hospital with basic emergency protocols but gaps
public Patient AdmitPatientToEmergency(string name, string condition)
{
    try
    {
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(condition))
        {
            return null; // Returning null instead of throwing for invalid inputs
        }
        
        var patient = new Patient
        {
            Name = name,
            ReportedCondition = condition
        };
        
        _hospitalDatabase.RegisterPatient(patient);
        
        try
        {
            _nursingStation.AssignBed(patient);
        }
        catch (Exception) 
        {
            // Swallowing bed assignment exceptions - not ideal
        }
        
        return patient;
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
public Patient AdmitPatientToEmergency(string name, string condition)
{
    // Validation - like checking patient information at intake
    if (string.IsNullOrEmpty(name))
        throw new ArgumentException("Patient name is required", nameof(name));
        
    if (string.IsNullOrEmpty(condition))
        throw new ArgumentException("Medical condition is required", nameof(condition));
        
    // Check for existing patient - return existing record if found
    var existingPatient = _hospitalDatabase.FindPatient(name);
    if (existingPatient != null)
        return existingPatient;
    
    try
    {
        // Create and register patient
        var patient = new Patient
        {
            Name = name,
            ReportedCondition = condition,
            AdmissionTime = DateTime.Now,
            TriageStatus = DeterminePriority(condition)
        };
        
        _hospitalDatabase.RegisterPatient(patient);
        
        try
        {
            // Non-critical operation gets its own try/catch
            _nursingStation.AssignBed(patient);
        }
        catch (NoBedAvailableException ex)
        {
            // Log and handle bed shortage, but don't fail admission
            _logger.Warn($"No beds available: {ex.Message}");
            _bedRequestQueue.Enqueue(new BedRequest(patient, patient.TriageStatus));
            patient.Status = PatientStatus.WaitingForBed;
        }
        
        return patient;
    }
    catch (DuplicatePatientIdException ex)
    {
        // Specific business rule violation - patient ID already exists
        _logger.Info($"Admission failed - duplicate ID: {ex.Message}");
        throw new PatientAdmissionException("Patient ID already exists in system", ex);
    }
    catch (DatabaseException ex)
    {
        // Technical failure - database error
        _logger.Error($"Database error during patient admission: {ex.Message}");
        throw new PatientAdmissionException("Unable to complete patient registration", ex);
    }
}
```

### Deeper Understanding

#### The Exception Handling System

In C#, exception handling works like a hospital's emergency response system:

1. **Detection (throw)**: Like triggering a code blue when a patient has cardiac arrest
2. **Response (catch)**: Like dispatching specialized teams based on the emergency type
3. **Escalation (re-throw)**: Like transferring a patient to a specialized cardiac unit
4. **Cleanup (finally)**: Like sanitizing an operating room after surgery, regardless of outcome

#### Exception Hierarchy

C# exceptions follow a hierarchical structure, like medical specialties:

- **Exception** - The base type (like "Medical Emergency")
  - **SystemException** - Framework-thrown exceptions (like "Common Medical Conditions")
    - **ArgumentException** - Invalid arguments (like "Patient Intake Errors")
      - **ArgumentNullException** - Null argument (like "Missing Critical Patient Information")
    - **IOException** - I/O errors (like "Medical Equipment Failures")
    - **NullReferenceException** - Null object access (like "Attempting Treatment on Non-existent Patient")
  - **ApplicationException** - Application-specific exceptions (like "Hospital-Specific Protocols")

#### When to Use Different Error Handling Approaches

1. **Return null/default** - For "not found" scenarios
   - Like when a patient isn't in the hospital records
   - Example: `FindPatient(patientId)` returns null when patient doesn't exist

2. **Throw exceptions** - For unexpected or exceptional conditions
   - Like triggering a code blue when a patient has cardiac arrest
   - Example: `PerformSurgery(patient)` throws when vital signs crash

3. **Try pattern** - For operations that frequently fail in expected ways
   - Like attempting to retrieve archived medical records that might be missing
   - Example: `bool found = TryGetMedicalHistory(patientId, out history);`

4. **Boolean return** - For simple success/failure operations
   - Like checking if a patient is eligible for a specific treatment
   - Example: `bool isEligible = ScreenForSurgery(patient, procedureType);`

#### Exception Performance Considerations

Exceptions are relatively expensive in performance terms - like calling a hospital-wide emergency response team. They:

1. Capture stack information (expensive)
2. Unwind the call stack looking for handlers
3. Allocate memory for exception objects

For performance-critical code, consider alternatives like the Try pattern for expected "failures" that aren't truly exceptional. This is like having standard protocols for common situations rather than triggering emergency responses.