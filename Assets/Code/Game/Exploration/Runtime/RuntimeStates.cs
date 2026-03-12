using System;
using System.Collections.Generic;
using Code.Game.Exploration.Domain;

namespace Code.Game.Exploration.Runtime
{
public sealed class WorldRuntimeState
{
    public WorldRuntimeState(string currentLocationId, string currentNodeId, int currentTimeUnits)
    {
        CurrentLocationId = currentLocationId ?? string.Empty;
        CurrentNodeId = currentNodeId ?? string.Empty;
        CurrentTimeUnits = currentTimeUnits;
        Flags = new Dictionary<string, bool>(StringComparer.Ordinal);
        Locations = new Dictionary<string, LocationRuntimeState>(StringComparer.Ordinal);
    }

    public string CurrentLocationId { get; set; }
    public string CurrentNodeId { get; set; }
    public int CurrentTimeUnits { get; set; }
    public int SuspicionLevel { get; set; }

    public Dictionary<string, bool> Flags { get; }
    public Dictionary<string, LocationRuntimeState> Locations { get; }
}

public sealed class LocationRuntimeState
{
    public LocationRuntimeState(string locationId)
    {
        LocationId = locationId ?? string.Empty;
        Nodes = new Dictionary<string, NodeRuntimeState>(StringComparer.Ordinal);
        Entities = new Dictionary<string, EntityRuntimeState>(StringComparer.Ordinal);
        LocalFlags = new Dictionary<string, bool>(StringComparer.Ordinal);
        Events = new Dictionary<string, EventRuntimeState>(StringComparer.Ordinal);
    }

    public string LocationId { get; }
    public Dictionary<string, NodeRuntimeState> Nodes { get; }
    public Dictionary<string, EntityRuntimeState> Entities { get; }
    public Dictionary<string, bool> LocalFlags { get; }
    public Dictionary<string, EventRuntimeState> Events { get; }
}

public sealed class NodeRuntimeState
{
    public NodeRuntimeState(string nodeId)
    {
        NodeId = nodeId ?? string.Empty;
    }

    public string NodeId { get; }
    public bool IsVisited { get; set; }
}

public sealed class EntityRuntimeState
{
    public EntityRuntimeState(string entityId, KnowledgeState knowledgeState)
    {
        EntityId = entityId ?? string.Empty;
        KnowledgeState = knowledgeState;
    }

    public string EntityId { get; }
    public KnowledgeState KnowledgeState { get; set; }
    public bool IsAvailable { get; set; } = true;
    public bool IsConsumed { get; set; }
}

public sealed class EventRuntimeState
{
    public EventRuntimeState(string eventId)
    {
        EventId = eventId ?? string.Empty;
    }

    public string EventId { get; }
    public int LastTriggeredTimeUnits { get; set; } = -1;
    public int TriggerCount { get; set; }
}
}