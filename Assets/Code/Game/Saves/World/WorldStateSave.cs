using Code.Game.Exploration.Domain;
using LocalSaveSystem;

namespace Code.Game.Saves.World
{
[SaveModel]
[SaveVersion(1)]
public struct WorldStateSave
{
    public int CurrentTimeUnits { get; set; }
    public string CurrentLocationId { get; set; }
    public string CurrentNodeId { get; set; }
    public int SuspicionLevel { get; set; }
    public FlagStateSave[] WorldFlags { get; set; }
    public LocationStateSave[] Locations { get; set; }
}

[SaveModel]
[SaveVersion(1)]
public struct LocationStateSave
{
    public string LocationId { get; set; }
    public NodeStateSave[] Nodes { get; set; }
    public EntityStateSave[] Entities { get; set; }
    public FlagStateSave[] LocalFlags { get; set; }
    public EventStateSave[] Events { get; set; }
}

[SaveModel]
[SaveVersion(1)]
public struct FlagStateSave
{
    public string FlagId { get; set; }
    public bool Value { get; set; }
}

[SaveModel]
[SaveVersion(1)]
public struct NodeStateSave
{
    public string NodeId { get; set; }
    public bool IsVisited { get; set; }
}

[SaveModel]
[SaveVersion(1)]
public struct EntityStateSave
{
    public string EntityId { get; set; }
    public KnowledgeState KnowledgeState { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsConsumed { get; set; }
}

[SaveModel]
[SaveVersion(1)]
public struct EventStateSave
{
    public string EventId { get; set; }
    public int LastTriggeredTimeUnits { get; set; }
    public int TriggerCount { get; set; }
}
}
