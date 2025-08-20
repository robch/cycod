using System;

/// <summary>
/// Service that tracks an interaction with a chat service.
/// This is used for grouping requests to a logical interaction with the UI.
/// It is just used for telemetry collection and request tracking.
/// </summary>
public class InteractionService
{
    private string _interactionId = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Starts a new interaction by generating a new interaction ID.
    /// Call this at the beginning of a conversation or when the user submits a new query.
    /// </summary>
    public void StartInteraction() => _interactionId = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Gets the current interaction ID.
    /// </summary>
    public string InteractionId => _interactionId;
}