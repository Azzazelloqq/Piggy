using System;
using System.Collections.Generic;
using Code.Config.Pages.Exploration;
using Code.Game.Exploration.Domain;
using Code.Game.Saves.World;

namespace Code.Game.Exploration.Runtime
{
public static class WorldStateMapper
{
    public static WorldRuntimeState ToRuntime(
        WorldStateSave save,
        WorldConfigPage worldConfig,
        IReadOnlyDictionary<string, LocationConfig> locationLookup)
    {
        if (worldConfig == null)
        {
            throw new ArgumentNullException(nameof(worldConfig));
        }

        if (locationLookup == null)
        {
            throw new ArgumentNullException(nameof(locationLookup));
        }

        if (locationLookup.Count == 0)
        {
            throw new InvalidOperationException("Exploration locations are not configured.");
        }

        var currentLocationId = ResolveCurrentLocationId(save, worldConfig, locationLookup);
        var currentNodeId = ResolveCurrentNodeId(save, worldConfig, locationLookup, currentLocationId);
        var currentTimeUnits = ResolveCurrentTimeUnits(save, worldConfig);

        var worldState = new WorldRuntimeState(currentLocationId, currentNodeId, currentTimeUnits)
            {
                SuspicionLevel = save.SuspicionLevel
            };
        ApplyFlags(save.WorldFlags, worldState.Flags);

        foreach (var pair in locationLookup)
        {
            var locationConfig = pair.Value;
            var locationState = BuildLocationState(locationConfig, save);
            worldState.Locations[locationState.LocationId] = locationState;
        }

        if (!worldState.Locations.TryGetValue(currentLocationId, out var currentLocationState))
        {
            throw new InvalidOperationException($"Runtime state for location '{currentLocationId}' is missing.");
        }

        if (!currentLocationState.Nodes.ContainsKey(currentNodeId))
        {
            throw new InvalidOperationException(
                $"Runtime state for location '{currentLocationId}' does not contain node '{currentNodeId}'.");
        }

        return worldState;
    }

    public static WorldStateSave ToSave(WorldRuntimeState runtime)
    {
        if (runtime == null)
        {
            throw new ArgumentNullException(nameof(runtime));
        }

        var locations = new List<LocationStateSave>();
        foreach (var locationPair in runtime.Locations)
        {
            var location = locationPair.Value;
            var locationSave = new LocationStateSave
            {
                LocationId = location.LocationId,
                Nodes = BuildNodeSaves(location.Nodes),
                Entities = BuildEntitySaves(location.Entities),
                LocalFlags = BuildFlagSaves(location.LocalFlags),
                Events = BuildEventSaves(location.Events)
            };
            locations.Add(locationSave);
        }

        return new WorldStateSave
        {
            CurrentTimeUnits = runtime.CurrentTimeUnits,
            CurrentLocationId = runtime.CurrentLocationId,
            CurrentNodeId = runtime.CurrentNodeId,
            SuspicionLevel = runtime.SuspicionLevel,
            WorldFlags = BuildFlagSaves(runtime.Flags),
            Locations = locations.ToArray()
        };
    }

    private static string ResolveCurrentLocationId(
        WorldStateSave save,
        WorldConfigPage worldConfig,
        IReadOnlyDictionary<string, LocationConfig> locationLookup)
    {
        if (!string.IsNullOrWhiteSpace(save.CurrentLocationId))
        {
            EnsureLocationExists(save.CurrentLocationId, locationLookup);
            return save.CurrentLocationId;
        }

        if (!string.IsNullOrWhiteSpace(worldConfig.DefaultStartLocationId))
        {
            EnsureLocationExists(worldConfig.DefaultStartLocationId, locationLookup);
            return worldConfig.DefaultStartLocationId;
        }

        throw new InvalidOperationException("WorldConfigPage.DefaultStartLocationId must be configured.");
    }

    private static string ResolveCurrentNodeId(
        WorldStateSave save,
        WorldConfigPage worldConfig,
        IReadOnlyDictionary<string, LocationConfig> locationLookup,
        string locationId)
    {
        EnsureLocationExists(locationId, locationLookup);
        var location = locationLookup[locationId];

        if (!string.IsNullOrWhiteSpace(save.CurrentNodeId))
        {
            EnsureLocationHasNode(location, save.CurrentNodeId);
            return save.CurrentNodeId;
        }

        if (!string.IsNullOrWhiteSpace(worldConfig.DefaultStartNodeId))
        {
            EnsureLocationHasNode(location, worldConfig.DefaultStartNodeId);
            return worldConfig.DefaultStartNodeId;
        }

        if (!string.IsNullOrWhiteSpace(location.DefaultNodeId))
        {
            EnsureLocationHasNode(location, location.DefaultNodeId);
            return location.DefaultNodeId;
        }

        throw new InvalidOperationException($"Location '{locationId}' does not define a default node id.");
    }

    private static int ResolveCurrentTimeUnits(WorldStateSave save, WorldConfigPage worldConfig)
    {
        if (save.CurrentTimeUnits > 0)
        {
            return save.CurrentTimeUnits;
        }

        return worldConfig.StartTimeUnits;
    }

    private static void EnsureLocationExists(string locationId, IReadOnlyDictionary<string, LocationConfig> locationLookup)
    {
        if (string.IsNullOrWhiteSpace(locationId))
        {
            throw new InvalidOperationException("Location id must not be empty.");
        }

        if (!locationLookup.ContainsKey(locationId))
        {
            throw new InvalidOperationException($"Location '{locationId}' is missing from the exploration config.");
        }
    }

    private static void EnsureLocationHasNode(LocationConfig location, string nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
        {
            throw new InvalidOperationException($"Location '{location.Id}' requires a non-empty node id.");
        }

        var nodes = location.Nodes;
        if (nodes == null || nodes.Length == 0)
        {
            throw new InvalidOperationException($"Location '{location.Id}' does not contain any nodes.");
        }

        for (var i = 0; i < nodes.Length; i++)
        {
            if (string.Equals(nodes[i].Id, nodeId, StringComparison.Ordinal))
            {
                return;
            }
        }

        throw new InvalidOperationException($"Location '{location.Id}' does not contain node '{nodeId}'.");
    }

    private static LocationRuntimeState BuildLocationState(LocationConfig config, WorldStateSave save)
    {
        var locationState = new LocationRuntimeState(config.Id);
        var locationSave = FindLocationSave(save.Locations, config.Id);

        ApplyFlags(locationSave.LocalFlags, locationState.LocalFlags);
        ApplyNodes(config.Nodes, locationSave.Nodes, locationState.Nodes);
        ApplyEntities(config.Entities, locationSave.Entities, locationState.Entities);
        ApplyEvents(config.Events, locationSave.Events, locationState.Events);

        if (config.Entities != null)
        {
            for (var i = 0; i < config.Entities.Length; i++)
            {
                var entity = config.Entities[i];
                if (entity.Events == null)
                {
                    continue;
                }

                ApplyEvents(entity.Events, locationSave.Events, locationState.Events);
            }
        }

        return locationState;
    }

    private static void ApplyFlags(FlagStateSave[] saves, Dictionary<string, bool> flags)
    {
        if (saves == null)
        {
            return;
        }

        foreach (var save in saves)
        {
            if (string.IsNullOrWhiteSpace(save.FlagId))
            {
                continue;
            }

            flags[save.FlagId] = save.Value;
        }
    }

    private static void ApplyNodes(
        NodeConfig[] nodes,
        NodeStateSave[] saves,
        Dictionary<string, NodeRuntimeState> runtimeNodes)
    {
        if (nodes == null)
        {
            return;
        }

        var saveLookup = BuildNodeLookup(saves);
        for (var i = 0; i < nodes.Length; i++)
        {
            var node = nodes[i];
            if (string.IsNullOrWhiteSpace(node.Id))
            {
                continue;
            }

            var runtime = new NodeRuntimeState(node.Id);
            if (saveLookup.TryGetValue(node.Id, out var nodeSave))
            {
                runtime.IsVisited = nodeSave.IsVisited;
            }

            runtimeNodes[node.Id] = runtime;
        }
    }

    private static void ApplyEntities(
        EntityConfig[] entities,
        EntityStateSave[] saves,
        Dictionary<string, EntityRuntimeState> runtimeEntities)
    {
        if (entities == null)
        {
            return;
        }

        var saveLookup = BuildEntityLookup(saves);
        foreach (var entity in entities)
        {
            if (string.IsNullOrWhiteSpace(entity.Id))
            {
                continue;
            }

            var knowledgeState = entity.Discovery.InitialState;
            var runtime = new EntityRuntimeState(entity.Id, knowledgeState);
            if (saveLookup.TryGetValue(entity.Id, out var entitySave))
            {
                runtime.KnowledgeState = entitySave.KnowledgeState;
                runtime.IsAvailable = entitySave.IsAvailable;
                runtime.IsConsumed = entitySave.IsConsumed;
            }

            runtimeEntities[entity.Id] = runtime;
        }
    }

    private static void ApplyEvents(
        EventConfig[] events,
        EventStateSave[] saves,
        Dictionary<string, EventRuntimeState> runtimeEvents)
    {
        if (events == null)
        {
            return;
        }

        var saveLookup = BuildEventLookup(saves);
        foreach (var eventConfig in events)
        {
            if (string.IsNullOrWhiteSpace(eventConfig.Id))
            {
                continue;
            }

            var runtime = new EventRuntimeState(eventConfig.Id);
            if (saveLookup.TryGetValue(eventConfig.Id, out var eventSave))
            {
                runtime.LastTriggeredTimeUnits = eventSave.LastTriggeredTimeUnits;
                runtime.TriggerCount = eventSave.TriggerCount;
            }

            runtimeEvents[eventConfig.Id] = runtime;
        }
    }

    private static LocationStateSave FindLocationSave(LocationStateSave[] saves, string locationId)
    {
        if (saves == null || string.IsNullOrWhiteSpace(locationId))
        {
            return default;
        }

        foreach (var save in saves)
        {
            if (string.Equals(save.LocationId, locationId, StringComparison.Ordinal))
            {
                return save;
            }
        }

        return default;
    }

    private static Dictionary<string, NodeStateSave> BuildNodeLookup(NodeStateSave[] saves)
    {
        var lookup = new Dictionary<string, NodeStateSave>(StringComparer.Ordinal);
        if (saves == null)
        {
            return lookup;
        }

        foreach (var save in saves)
        {
            if (string.IsNullOrWhiteSpace(save.NodeId))
            {
                continue;
            }

            lookup[save.NodeId] = save;
        }

        return lookup;
    }

    private static Dictionary<string, EntityStateSave> BuildEntityLookup(EntityStateSave[] saves)
    {
        var lookup = new Dictionary<string, EntityStateSave>(StringComparer.Ordinal);
        if (saves == null)
        {
            return lookup;
        }

        foreach (var save in saves)
        {
            if (string.IsNullOrWhiteSpace(save.EntityId))
            {
                continue;
            }

            lookup[save.EntityId] = save;
        }

        return lookup;
    }

    private static Dictionary<string, EventStateSave> BuildEventLookup(EventStateSave[] saves)
    {
        var lookup = new Dictionary<string, EventStateSave>(StringComparer.Ordinal);
        if (saves == null)
        {
            return lookup;
        }

        foreach (var save in saves)
        {
            if (string.IsNullOrWhiteSpace(save.EventId))
            {
                continue;
            }

            lookup[save.EventId] = save;
        }

        return lookup;
    }

    private static FlagStateSave[] BuildFlagSaves(Dictionary<string, bool> flags)
    {
        if (flags == null || flags.Count == 0)
        {
            return Array.Empty<FlagStateSave>();
        }

        var result = new FlagStateSave[flags.Count];
        var index = 0;
        foreach (var pair in flags)
        {
            result[index++] = new FlagStateSave
            {
                FlagId = pair.Key,
                Value = pair.Value
            };
        }

        return result;
    }

    private static NodeStateSave[] BuildNodeSaves(Dictionary<string, NodeRuntimeState> nodes)
    {
        if (nodes == null || nodes.Count == 0)
        {
            return Array.Empty<NodeStateSave>();
        }

        var result = new NodeStateSave[nodes.Count];
        var index = 0;
        foreach (var pair in nodes)
        {
            var node = pair.Value;
            result[index++] = new NodeStateSave
            {
                NodeId = node.NodeId,
                IsVisited = node.IsVisited
            };
        }

        return result;
    }

    private static EntityStateSave[] BuildEntitySaves(Dictionary<string, EntityRuntimeState> entities)
    {
        if (entities == null || entities.Count == 0)
        {
            return Array.Empty<EntityStateSave>();
        }

        var result = new EntityStateSave[entities.Count];
        var index = 0;
        foreach (var pair in entities)
        {
            var entity = pair.Value;
            result[index++] = new EntityStateSave
            {
                EntityId = entity.EntityId,
                KnowledgeState = entity.KnowledgeState,
                IsAvailable = entity.IsAvailable,
                IsConsumed = entity.IsConsumed
            };
        }

        return result;
    }

    private static EventStateSave[] BuildEventSaves(Dictionary<string, EventRuntimeState> events)
    {
        if (events == null || events.Count == 0)
        {
            return Array.Empty<EventStateSave>();
        }

        var result = new EventStateSave[events.Count];
        var index = 0;
        foreach (var pair in events)
        {
            var runtimeEvent = pair.Value;
            result[index++] = new EventStateSave
            {
                EventId = runtimeEvent.EventId,
                LastTriggeredTimeUnits = runtimeEvent.LastTriggeredTimeUnits,
                TriggerCount = runtimeEvent.TriggerCount
            };
        }

        return result;
    }
}
}